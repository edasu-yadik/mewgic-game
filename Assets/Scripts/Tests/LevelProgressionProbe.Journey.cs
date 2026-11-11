#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Mewgic.Characters;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using Mewgic.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    public sealed partial class LevelProgressionProbe
    {
        const string JourneyState = "Mewgic.ShopEconomyGameplayQA.";
        [SerializeField] int journeyPhase;
        bool keepJourneySave;

        [MenuItem("Mewgic/QA/Run Shop Economy Gameplay Journey")]
        public static void BeginJourney() => BeginJourney("Library/ShopEconomyGameplayQA/Journey/");

        public static void BeginJourney(string folder) => Begin(false, folder, journeyPhase: 1);

        [MenuItem("Mewgic/QA/Resume Shop Economy Gameplay Journey")]
        public static void BeginJourneyRestart() => BeginJourneyRestart("Library/ShopEconomyGameplayQA/Restart/");

        public static void BeginJourneyRestart(string folder)
        {
            if (string.IsNullOrEmpty(SessionState.GetString(JourneyState + "Key", "")))
                throw new InvalidOperationException("İlk QA oturumu tamamlanmalı.");
            Begin(false, folder, journeyPhase: 2);
        }

        IEnumerator JourneyHome()
        {
            yield return HomeReady();
            yield return Until(() => L.Ready && GameObject.Find("NavInventory") != null, "Ana Menü arayüzü hazır");
            if (GameObject.Find("ArrivalDismiss") != null)
            {
                yield return null;
                yield return ClickWhenReady("ArrivalDismiss");
                yield return null;
            }
        }

        IEnumerator WaitForJourneyLevel(int level) => Until(() =>
            SceneManager.GetActiveScene().name == "Gameplay" && Hud?.State.Level == level &&
            FindFirstObjectByType<PrototypeApp>()?.GuideCanInteract == true && !Board.Won,
            $"Gerçek oyun ekranı açıldı, bölüm {level}");

        IEnumerator JourneyWin(int level)
        {
            long before = Ledger.Presentation.coins;
            var solution = PuzzleSolver.Solve(Board.Export());
            Check(solution.Status == SearchStatus.Solved && solution.Moves.Count > 0, $"{level}: çözücü geçerli bir rota buldu");
            foreach (var move in solution.Moves)
            {
                int previous = Board.Moves;
                yield return ClickWhenReady("Bottle " + (move.Source + 1));
                yield return null;
                yield return ClickWhenReady("Bottle " + (move.Target + 1));
                yield return Until(() => Board.Moves == previous + 1 && (Board.Won ||
                    FindFirstObjectByType<PrototypeApp>().GuideCanInteract), $"{level}: gerçek döküş {previous + 1}");
            }
            yield return Until(() =>
            {
                var next = GameObject.Find("NextLevel")?.GetComponent<Button>();
                return next != null && next.IsInteractable() && next.GetComponentInParent<CanvasGroup>()?.blocksRaycasts == true;
            }, $"{level}: sonuç ekranı ve etkileşim katmanı hazır");
            yield return null;
            Check(Ledger.Completed(level) && Ledger.CurrentUnlockedLevel == level + 1 &&
                Ledger.Presentation.coins == before + Ledger.LastCoinReward && Ledger.LastCoinReward > 0,
                $"{level}: yıldız, altın ve sonraki bölüm aynı kazanışla kaydedildi");
        }

        IEnumerator JourneyBackHome()
        {
            yield return ClickWhenReady("ResultHome");
            yield return Until(() => SceneManager.GetActiveScene().name == "LevelMap" &&
                FindFirstObjectByType<MapWorld>()?.Mounted == true, "Sonuçtan gerçek harita açıldı");
            yield return null;
            if (GameObject.Find("ChapterClose") != null)
                yield return ClickWhenReady("ChapterClose");
            yield return ClickWhenReady("MapHome");
            yield return JourneyHome();
        }

        IEnumerator JourneyShot(string name)
        {
            Directory.CreateDirectory(Evidence);
            string path = Evidence + name + ".png";
            ScreenCapture.CaptureScreenshot(path);
            yield return Until(() => File.Exists(path) && new FileInfo(path).Length > 0,
                $"{name}: gerçek Game View görüntüsü kaydedildi");
        }

        IEnumerator RunJourney()
        {
            Directory.CreateDirectory(Evidence);
            yield return JourneyHome();
            Check(Ledger.Presentation.coins == 0 && Ledger.Presentation.gems == 0 &&
                Ledger.CurrentUnlockedLevel == 1 && Ledger.HighestCompletedLevel == 0, "Sıfır bakiyeli yeni oyuncu");
            yield return ClickWhenReady("Play");
            for (int level = 1; level <= 4; level++)
            {
                yield return WaitForJourneyLevel(level);
                var guide = FindFirstObjectByType<PawGuide>();
                yield return Until(() => guide.OnboardingVisible, $"{level}: bekleyince pati öğreticisi göründü");
                var recommended = guide.Recommended;
                var afterHint = new Puzzle(Board.Export());
                Check(afterHint.Pour(recommended.Source, recommended.Target) > 0 &&
                    PuzzleSolver.Solve(afterHint.Export()).Status == SearchStatus.Solved && Board.Moves == 0,
                    $"{level}: öğretici geçerli hamleyi gösterdi ve oyuncu tahtasına dokunmadı");
                yield return new WaitForSecondsRealtime(.55f);
                yield return JourneyShot("Tutorial" + level);
                yield return JourneyWin(level);
                if (level == 1)
                {
                    var meter = GameObject.Find("RewardCoinBalance").GetComponent<TMP_Text>();
                    long award = Ledger.LastCoinReward;
                    yield return Until(() => GameObject.Find("FlyingCoin0") != null, "Ödül simgesi bar yönünde hareket etti");
                    yield return Until(() => long.TryParse(meter.text, out long count) && count > 0 && count < award,
                        "Ödül barı kayıtlı ödüle kademeli ilerledi");
                    yield return JourneyShot("CoinFlight");
                    yield return Until(() => meter.text == HomeTopBar.FormatAmount(Ledger.Presentation.coins),
                        "Ödül barı kayıtlı altınla eşleşti");
                    Check(Ledger.SetReducedMotion(true), "Kalan senaryolar için azaltılmış hareket etkin");
                }
                if (level < 4) { yield return ClickWhenReady("NextLevel"); yield return null; }
            }
            yield return JourneyBackHome();

            var config = EconomyConfig.Current;
            const ShopItem costume = ShopItem.ExplorerCat;
            int costumePrice = config.Price(costume);
            Check(Ledger.Presentation.gems == 0 &&
                Ledger.SetResources(Math.Max(Ledger.Presentation.coins, config.extraPrice), costumePrice - config.dailyStarsGems, 5),
                "Uzun birikim yerine izole test bakiyesi, elmas günlük görevin iki elmas eksiğinde");
            long beforeGold = Ledger.Presentation.coins;
            int stock = Ledger.Remaining(BoosterKind.ExtraBottle);
            yield return ClickWhenReady("NavInventory");
            yield return null;
            yield return ClickWhenReady("ShopTab_Powers");
            yield return null;
            yield return ClickWhenReady("BuyExtraBottle");
            yield return null;
            yield return ClickWhenReady("ShopConfirm");
            Check(Ledger.Presentation.coins == beforeGold - config.extraPrice &&
                Ledger.Presentation.gems == costumePrice - config.dailyStarsGems &&
                Ledger.Remaining(BoosterKind.ExtraBottle) == stock + config.boosterQuantity, "Ek Şişe altınla alındı");
            yield return ClickWhenReady("ShopClose");
            yield return null;
            yield return ClickWhenReady("Play");
            yield return WaitForJourneyLevel(5);
            int bottleCount = Board.Count;
            int boughtStock = Ledger.Remaining(BoosterKind.ExtraBottle);
            yield return ClickWhenReady("ExtraBottle");
            yield return Until(() => Board.Count == bottleCount + 1, "Satın alınan Ek Şişe gerçek bulmacaya eklendi");
            Check(Board.Export().Count(bottle => bottle.Length == 0) >= 1 &&
                Ledger.Remaining(BoosterKind.ExtraBottle) == boughtStock - 1 &&
                PuzzleSolver.Solve(Board.Export()).Status == SearchStatus.Solved, "Stok bir düştü ve bulmaca çözülebilir kaldı");
            yield return JourneyShot("PurchasedBooster");
            yield return JourneyWin(5);
            yield return JourneyBackHome();

            yield return ClickWhenReady("MissionsButton");
            yield return null;
            long beforeTaskGold = Ledger.Presentation.coins;
            yield return ClickWhenReady("DailyAction_2");
            Check(Ledger.DailySnapshot.claimed[(int)DailyTaskKind.Stars] && Ledger.Presentation.gems == costumePrice &&
                Ledger.Presentation.coins == beforeTaskGold + config.taskStarsGold &&
                !Ledger.ClaimDailyTask(DailyTaskKind.Stars), "Gerçek günlük görev altın ve elmas verdi, tekrar talep reddedildi");
            yield return JourneyShot("DailyClaim");
            yield return ClickWhenReady("DailyClose");
            yield return null;
            yield return ClickWhenReady("NavInventory");
            yield return null;
            yield return ClickWhenReady("ShopTab_Costumes");
            yield return null;
            beforeGold = Ledger.Presentation.coins;
            yield return ClickWhenReady("ShopAction_ExplorerCat");
            yield return null;
            yield return ClickWhenReady("ShopConfirm");
            Check(Ledger.Owns(costume) && Ledger.Presentation.gems == 0 && Ledger.Presentation.coins == beforeGold,
                "Kaşif Kedi yalnız elmasla alındı");
            yield return ClickWhenReady("ShopAction_ExplorerCat");
            Check(Ledger.Equipped(ShopCategory.Cats) == costume, "Kaşif Kedi düğme ile kuşanıldı");
            yield return ClickWhenReady("ShopClose");
            yield return null;
            yield return ClickWhenReady("NavMap");
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true, "Kostüm ile dünya haritası açıldı");
            var map = FindFirstObjectByType<MapWorld>();
            yield return Until(() => map.Canvas.GetComponentsInChildren<RawImage>().Any(image => image.name == "WorldCostumeMascot" &&
                image.enabled && image.texture == CostumeArt.Get(costume, CostumePose.Sitting).texture), "Dünya haritasında gerçek Kaşif Kedi dokusu");
            yield return JourneyShot("CostumeMap");
            yield return ClickWhenReady("MapHome");
            yield return JourneyHome();
            yield return ClickWhenReady("Play");
            yield return WaitForJourneyLevel(6);
            Check(GameObject.Find("GameplayWizardCat").GetComponent<WizardCatPresentation>().WornCostume == costume,
                "Oyun maskotu aynı kuşanılmış kostümü gösterdi");
            yield return JourneyShot("CostumeGameplay");

            SessionState.SetString(JourneyState + "Snapshot", JourneyProgress(Ledger));
            SessionState.SetString(JourneyState + "Archive", archiveRoot);
            SessionState.SetString(JourneyState + "Key", key);
            keepJourneySave = true;
        }

        static string JourneyProgress(ProgressLedger ledger)
        {
            var data = ledger.Snapshot;
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                ledger.CurrentUnlockedLevel, ledger.HighestCompletedLevel, data.presentation.currentLevel,
                data.presentation.coins, data.presentation.gems, data.presentation.energy, data.undo, data.hint,
                data.extra, data.shuffle, data.colorSwap, data.bottleHammer, data.levels, data.shop, data.completedTutorials,
                data.daily.progress, data.daily.claimed, data.walletTransactions, data.storeDeliveries
            });
        }

        IEnumerator RunJourneyRestart()
        {
            yield return JourneyHome();
            var expected = SessionState.GetString(JourneyState + "Snapshot", "");
            Check(expected.Length > 0 && JourneyProgress(Ledger) == expected,
                "Ayrı Play Mode oturumu: bakiye, stok, sahiplik, kuşanma, görev, işlem ve bölüm kayıtları birebir");
            Check(!Ledger.ClaimDailyTask(DailyTaskKind.Stars) && Ledger.Equipped(ShopCategory.Cats) == ShopItem.ExplorerCat,
                "Yeniden açılışta günlük ödül tekrarlanmadı ve kostüm kuşanılmış kaldı");
            yield return ClickWhenReady("Play");
            yield return WaitForJourneyLevel(6);
            Check(!FindFirstObjectByType<PawGuide>().OnboardingVisible &&
                GameObject.Find("GameplayWizardCat").GetComponent<WizardCatPresentation>().WornCostume == ShopItem.ExplorerCat,
                "Yeniden açılan oyunda doğru bölüm ve kostüm");
            yield return JourneyShot("RestartGameplay");
        }
    }
}
#endif
