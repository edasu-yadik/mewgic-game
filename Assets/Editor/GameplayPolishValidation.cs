using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic;
using Mewgic.Core;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

public static class GameplayPolishValidation
{
    const string Output = "docs/ProductionReports/GameplayPolish";
    [MenuItem("Mewgic/Gameplay Polish/Run Isolated Play Mode QA")]
    public static void Run() => GameplayPolishProbe.Begin();

    public static void UpdateCountdownTranslations()
    {
        var row = Part12CLocalization.Source().Single(item => item.key == "daily.reset");
        var collection = LocalizationEditorSettings.GetStringTableCollection("Mewgic");
        foreach (var code in Mewgic.Localization.L.Codes)
        {
            var table = (StringTable)collection.GetTable(code);
            table.GetEntry(row.key).Value = row.values[code];
            EditorUtility.SetDirty(table);
        }
        AssetDatabase.SaveAssets();
    }

    public static void ImportQuestIcon()
    {
        const string path = "Assets/Resources/Daily/QuestIcon_v001.png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.isReadable = false;
        importer.maxTextureSize = 512; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
        var android = importer.GetPlatformTextureSettings("Android"); android.name = "Android"; android.overridden = true;
        android.maxTextureSize = 512; android.format = TextureImporterFormat.ASTC_4x4; importer.SetPlatformTextureSettings(android);
        importer.SaveAndReimport();
    }

    public static void LegacyModels()
    {
        var field = typeof(DailyActivitiesTests).GetField("checks", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var checks = (List<string>)field.GetValue(null);
        checks.Clear();
        DailyActivitiesTests.Models();
        File.WriteAllText(Output + "/DAILY_REGRESSION.json", Newtonsoft.Json.JsonConvert.SerializeObject(
            new { success = true, count = checks.Count, checks }, Newtonsoft.Json.Formatting.Indented));
    }

    [MenuItem("Mewgic/Gameplay Polish/Validate Daily Save Rules")]
    public static void Models()
    {
        Directory.CreateDirectory(Output);
        var checks = new List<string>();
        void Check(bool okay, string message) { if (!okay) throw new Exception(message); checks.Add(message); }
        var now = new DateTimeOffset(2026, 11, 6, 23, 59, 0, TimeSpan.FromHours(2));
        ProgressData disk = null;
        var ledger = new ProgressLedger(new ProgressData(), value => disk = value.Copy(), () => now);
        Check(ledger.ObserveDaily() && ledger.DailyTaskWait == TimeSpan.FromHours(24), "İlk gözlemde 24 saat başlıyor");
        ledger.Claim(1, 3, true, out _, "polish-1", 2);
        var progress = ledger.DailySnapshot.progress;
        now = now.AddMinutes(2);
        Check(ledger.ObserveDaily() && ledger.DailySnapshot.progress.SequenceEqual(progress), "Gece yarısı görevleri sıfırlamıyor");
        Check(ledger.DailyTaskWait == TimeSpan.FromHours(24) - TimeSpan.FromMinutes(2), "Canlı bekleme UTC süre farkından hesaplanıyor");
        ledger = new ProgressLedger(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(disk)), value => disk = value.Copy(), () => now);
        Check(ledger.DailySnapshot.progress.SequenceEqual(progress) && ledger.DailyTaskWait.TotalMinutes == 1438, "Kayıt yeniden yüklenince sayaç ve ilerleme korunuyor");
        now = now.AddHours(24).AddMinutes(-2).AddSeconds(-1);
        Check(ledger.ObserveDaily() && ledger.DailySnapshot.progress.SequenceEqual(progress) && ledger.DailyTaskWait.TotalSeconds == 1, "24 saatten bir saniye önce yenileme yok");
        now = now.AddSeconds(1);
        Check(ledger.ObserveDaily() && ledger.DailySnapshot.progress.All(x => x == 0) && ledger.DailySnapshot.claimed.All(x => !x) && ledger.DailySnapshot.wins.Count == 0,
            "24 saatte ilerleme, ödül işaretleri ve makbuzlar birlikte yenileniyor");
        Check(ledger.DailyTaskWait == TimeSpan.FromHours(24) && ledger.Completed(1), "Yeni döngü oyun ilerlemesini değiştirmiyor");
        now = now.AddDays(5).AddHours(2);
        Check(ledger.ObserveDaily() && ledger.DailyTaskWait == TimeSpan.FromHours(22), "Uzun çevrimdışı sürede tek güncel döngüye geçiliyor");
        var stamp = ledger.DailySnapshot.taskCycleUtc;
        now = now.AddHours(-3);
        Check(ledger.DailyClockBlocked && !ledger.ObserveDaily() && ledger.DailySnapshot.taskCycleUtc == stamp, "Saat geri alınması yenileme veya ödül üretmiyor");
        now = now.AddHours(3);
        var legacy = ledger.Snapshot;
        legacy.daily.taskCycleUtc = 0;
        legacy.daily.progress = new[] { 4, 2, 6, 1 };
        legacy.daily.claimed = new[] { true, false, true, false };
        ledger = new ProgressLedger(legacy, value => disk = value.Copy(), () => now);
        Check(ledger.ObserveDaily() && ledger.DailySnapshot.progress.SequenceEqual(legacy.daily.progress) && ledger.DailySnapshot.claimed.SequenceEqual(legacy.daily.claimed),
            "Eski kayıttaki kazanılmış ilerleme ve alınmış ödüller korunuyor");
        int hints = ledger.Remaining(BoosterKind.Hint);
        Check(ledger.ClaimDailyTask(DailyTaskKind.Wins) && !ledger.ClaimDailyTask(DailyTaskKind.Wins) && ledger.Remaining(BoosterKind.Hint) == hints + 1,
            "Görev ödülü bir kez veriliyor");
        bool fail = false;
        var atomic = new ProgressLedger(ledger.Snapshot, _ => { if (fail) throw new IOException("QA"); }, () => now);
        fail = true; now = now.AddHours(24);
        Check(!atomic.ObserveDaily() && atomic.DailySnapshot.taskCycleUtc == ledger.DailySnapshot.taskCycleUtc && atomic.DailySnapshot.claimed[1],
            "Yenileme save hatası eski döngüyü atomik koruyor");
        string key = "Mewgic.Tests.GameplayPolish." + Guid.NewGuid().ToString("N");
        string player = PlayerPrefs.GetString(GameplayProgress.StorageKey, "");
        try
        {
            var actual = GameplayProgress.Open(key, () => now);
            Check(actual.ObserveDaily(), "Gerçek PlayerPrefs/journal döngüyü kaydediyor");
            var expected = actual.DailySnapshot.taskCycleUtc;
            now = now.AddHours(7);
            actual = GameplayProgress.Open(key, () => now);
            Check(actual.DailySnapshot.taskCycleUtc == expected && actual.DailyTaskWait == TimeSpan.FromHours(17), "Gerçek kapat-aç 17 saat kalanı koruyor");
            Check(PlayerPrefs.GetString(GameplayProgress.StorageKey, "") == player, "Oyuncunun ana kaydı değişmedi");
        }
        finally
        {
            foreach (string suffix in new[] { "", ".pending", ".backup", ".previous", ".corrupt" }) PlayerPrefs.DeleteKey(key + suffix);
            PlayerPrefs.Save();
        }
        GameplayBoosterTests.ModelTests(); checks.Add("Mevcut puzzle/booster model regresyonu geçti");
        File.WriteAllText(Output + "/MODEL_TESTS.json", Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, count = checks.Count, checks }, Newtonsoft.Json.Formatting.Indented));
    }
}
