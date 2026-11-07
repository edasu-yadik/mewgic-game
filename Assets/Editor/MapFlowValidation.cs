using System;
using System.Collections.Generic;
using System.IO;
using Mewgic;
using Mewgic.Core;
using UnityEditor;
using UnityEngine;

public static class MapFlowValidation
{
    [MenuItem("Mewgic/Map Flow/Run Isolated Play Mode QA")]
    public static void Run() => MapFlowProbe.Begin();

    [MenuItem("Mewgic/Map Flow/Run Journey Foundation QA")]
    public static void RunFoundation() => MapFlowProbe.Begin("docs/ProductionReports/MapJourneyFoundation");

    [MenuItem("Mewgic/Map Flow/Validate Save Model")]
    public static void Models()
    {
        var checks = new List<string>();
        void Check(bool okay, string message) { if (!okay) throw new Exception(message); checks.Add(message); }
        foreach (int completed in new[] { 0, 49, 90, 98, 99, 599, 649 })
        {
            ProgressData disk = MapFlowProbe.Data(completed);
            var ledger = new ProgressLedger(disk, value => disk = value.Copy());
            int id = completed + 1;
            Check(!ledger.MapJourney.Pending && ledger.MapJourney.displayedLevel == id, id + " eski kayıttan sessiz geçiş");
            Check(!ledger.Claim(id, 0, false, out _) && !ledger.MapJourney.Pending, id + " başarısız bölüm bilet oluşturmaz");
            Check(ledger.Claim(id, 2, true, out _) && ledger.MapJourney.Pending, id + " ilk kazanışla atomik bilet");
            var fromDisk = new ProgressLedger(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(disk)), value => disk = value.Copy());
            Check(fromDisk.PeekMapJourney(out int from, out int to) && from == id && to == id + 1, id + " JSON/disk round-trip");
            Check(fromDisk.PeekMapJourney(out _, out _) && fromDisk.MapJourney.Pending, id + " okumak bileti tüketmez");
            Check(fromDisk.Claim(id, 3, true, out _) && fromDisk.MapJourney.Pending, id + " replay bekleyen bileti korur");
            Check(!fromDisk.AcknowledgeMapJourney(id) && fromDisk.MapJourney.Pending, id + " yanlış ack hedefi reddedilir");
            Check(fromDisk.AcknowledgeMapJourney(to) && !fromDisk.MapJourney.Pending && fromDisk.MapJourney.displayedLevel == to, id + " varış atomik ack");
            var reopened = new ProgressLedger(disk, _ => { });
            Check(!reopened.MapJourney.Pending && reopened.MapJourney.lastShownLevel == to, id + " ack yeniden yüklemede tekrar etmez");
            Check(reopened.AcknowledgeMapJourney(to), id + " idempotent ack");
        }
        bool fail = true;
        var failed = new ProgressLedger(MapFlowProbe.Data(90), _ => { if (fail) throw new IOException("QA"); });
        Check(!failed.Claim(91, 3, true, out _) && !failed.MapJourney.Pending && !failed.Completed(91), "Kazanış save hatası ödül/bileti birlikte geri alır");
        fail = false; failed.Claim(91, 3, true, out _); fail = true;
        Check(!failed.AcknowledgeMapJourney(92) && failed.MapJourney.Pending && failed.MapJourney.displayedLevel == 91,
            "Ack save hatası pending ve eski maskot konumunu korur");
        var consecutive = new ProgressLedger(MapFlowProbe.Data(98), _ => { });
        consecutive.Claim(99, 2, true, out _); consecutive.Claim(100, 2, true, out _);
        Check(consecutive.MapJourney.pendingFrom == 99 && consecutive.MapJourney.pendingTo == 101 && consecutive.CurrentCompletedLevel == 100,
            "Next zinciri dünya sınırında birleşir; completed ve unlocked ayrıdır");
        var copy = consecutive.MapJourney; copy.pendingTo = 999;
        Check(consecutive.MapJourney.pendingTo == 101, "Dışarı verilen yolculuk verisi kayıt içeriğini değiştiremez");
        string testKey = "Mewgic.Tests.MapFlow." + Guid.NewGuid().ToString("N");
        string playerBefore = PlayerPrefs.GetString(GameplayProgress.StorageKey, "");
        try
        {
            PlayerPrefs.SetString(testKey, JsonUtility.ToJson(MapFlowProbe.Data(98)));
            PlayerPrefs.Save();
            var actual = GameplayProgress.Open(testKey);
            Check(actual.Claim(99, 3, true, out _), "Gerçek save journal kazanışı kabul ediyor");
            actual = GameplayProgress.Open(testKey);
            Check(actual.MapJourney.Pending && actual.MapJourney.pendingTo == 100, "Gerçek PlayerPrefs kapat-aç bekleyen animasyonu koruyor");
            Check(actual.AcknowledgeMapJourney(100), "Gerçek save journal varışı kaydediyor");
            actual = GameplayProgress.Open(testKey);
            Check(!actual.MapJourney.Pending && actual.MapJourney.lastShownLevel == 100, "Gerçek save journal ikinci girişte tekrar etmiyor");
            Check(PlayerPrefs.GetString(GameplayProgress.StorageKey, "") == playerBefore, "Gerçek oyuncu kayıt anahtarı değişmedi");
        }
        finally
        {
            foreach (string suffix in new[] { "", ".pending", ".backup", ".previous", ".corrupt" }) PlayerPrefs.DeleteKey(testKey + suffix);
            PlayerPrefs.Save();
        }
        GameplayBoosterTests.ModelTests();
        checks.Add("Mevcut puzzle/booster model regresyonu geçti");
        Directory.CreateDirectory("docs/ProductionReports/MapFlow");
        File.WriteAllText("docs/ProductionReports/MapFlow/MODEL_TESTS.json", Newtonsoft.Json.JsonConvert.SerializeObject(
            new { success = true, count = checks.Count, checks }, Newtonsoft.Json.Formatting.Indented));
    }
}
