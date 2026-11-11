#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using Mewgic.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed partial class LevelProgressionProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/LevelProgression/";
        [SerializeField] bool frontier, boosterIntegration;
        [SerializeField] string regressionOutput;
        string Evidence => !string.IsNullOrEmpty(regressionOutput) ? regressionOutput :
            frontier ? "docs/ProductionReports/InfiniteLevels/" : Output;
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo LedgerField = typeof(GameplayProgress).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle", BindingFlags.Instance | BindingFlags.NonPublic);
        static LevelProgressionProbe active;
        readonly List<object> checks = new(), boards = new();
        readonly List<string> errors = new(); readonly List<object> transitions = new();
        Dictionary<string, string> original;
        string key, archiveRoot;
        bool passed = true, previousRunInBackground;
        ProgressLedger Ledger => GameplayProgress.Current;
        GameplayHud Hud => FindFirstObjectByType<GameplayHud>();
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run Level Progression")]
        public static void Begin() => Begin(false);

        [MenuItem("Mewgic/QA/Run Infinite Levels and Boosters")]
        public static void BeginFrontier() => Begin(true);

        public static void BeginFrontier(string folder) => Begin(true, folder);

        public static void BeginRegression(string folder) => Begin(false, folder);

        [MenuItem("Mewgic/QA/Run Gameplay Booster Integration")]
        public static void BeginBoosterIntegration() => Begin(false, "docs/ProductionReports/BoosterIntegration/", true);

        public static void BeginBoosterIntegration(string folder) => Begin(false, folder, true);

        static void Begin(bool frontier, string regressionOutput = null, bool boosterIntegration = false, int journeyPhase = 0)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Açık sahnede kaydedilmemiş değişiklik var.");
            EditorSceneManager.OpenScene(journeyPhase == 0 ? "Assets/Scenes/Gameplay.unity" : "Assets/Scenes/Boot.unity");
            var probe = new GameObject("Isolated level progression QA").AddComponent<LevelProgressionProbe>();
            probe.frontier = frontier;
            probe.boosterIntegration = boosterIntegration;
            probe.regressionOutput = regressionOutput;
            probe.journeyPhase = journeyPhase;
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor")?.GetMethod("Size")?.Invoke(null, new object[] { 1600 });
            EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.isPlaying = true;
        }

        static Dictionary<string, string> PlayerSave() => Suffixes.ToDictionary(
            suffix => suffix, suffix => PlayerPrefs.HasKey(GameplayProgress.StorageKey + suffix)
                ? PlayerPrefs.GetString(GameplayProgress.StorageKey + suffix) : null);

        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}
            active=this;
            DontDestroyOnLoad(gameObject);
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            original = PlayerSave();
            archiveRoot = Path.GetFullPath("Library/LevelProgressionQA/" + Guid.NewGuid().ToString("N"));
            Application.logMessageReceived += Log;
            if (journeyPhase == 2)
            {
                key = SessionState.GetString(JourneyState + "Key", "");
                archiveRoot = SessionState.GetString(JourneyState + "Archive", "");
                Reopen();
            }
            else Seed(0);
        }

        void Log(string message, string stack, LogType kind)
        {
            if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) errors.Add(message);
        }

        void Seed(int completed)
        {
            ClearJournal();
            key = "Mewgic.Tests.LevelProgression." + Guid.NewGuid().ToString("N");
            var data = MapFlowProbe.Data(completed);
            data.presentation.currentLevel = completed + 1;
            data.presentation.reduceMotion = journeyPhase != 1;
            data.presentation.motionPreferenceSet = true;
            PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            Reopen();
        }

        void Reopen()
        {
            GameplayProgress.RestorePlayerProfile();
            GameplayProgress.UseTestProfile();
            LedgerField.SetValue(null, GameplayProgress.Open(key));
            GameplayLevelCatalog.TestArchive = new FileLevelArchive(archiveRoot);
            GameplayLevelCatalog.ResetSelection();
            GameplayLevelCatalog.ResumeFromSave();
        }

        void ClearJournal()
        {
            if (key == null) return;
            foreach (string suffix in Suffixes) PlayerPrefs.DeleteKey(key + suffix);
            PlayerPrefs.Save();
        }

        void Check(bool ok, string message, bool required = true)
        {
            checks.Add(new { ok, message });
            passed &= ok;
            if (!ok && required) throw new InvalidOperationException(message);
        }

        IEnumerator Start()
        {
            // Mount the initial level before changing the isolated profile for frontier tests.
            if (frontier || boosterIntegration) yield return null;
            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(journeyPhase == 1 ? RunJourney() : journeyPhase == 2 ? RunJourneyRestart() :
                boosterIntegration ? RunBoosterIntegration() : frontier ? RunFrontier() : Run());
            while (stack.Count > 0)
            {
                bool more = false; object item = null;
                try { more = stack.Peek().MoveNext(); if (more) item = stack.Peek().Current; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) { stack.Pop(); continue; }
                if (item is IEnumerator nested && item is not CustomYieldInstruction) stack.Push(nested);
                else yield return item;
            }
            var after = PlayerSave();
            bool intact = original.All(pair => after[pair.Key] == pair.Value);
            keepJourneySave &= passed && failure == null && intact && errors.Count == 0;
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence + "PLAY_MODE.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = passed && failure == null && intact && errors.Count == 0,
                editor = Application.unityVersion, checks, boards, transitions, errors, failure, playerSaveIntact = intact
            }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        void OnDestroy()
        {
            if(active!=this)return;
            active=null;
            Application.runInBackground = previousRunInBackground;
            Application.logMessageReceived -= Log;
            if (!keepJourneySave)
            {
                ClearJournal();
                if (journeyPhase != 0)
                    foreach (string suffix in new[] { "Key", "Archive", "Snapshot" })
                        SessionState.EraseString(JourneyState + suffix);
            }
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.TestArchive = null;
            GameplayLevelCatalog.ResetSelection();
        }

        IEnumerator Until(Func<bool> ready, string message)
        {
            float deadline = Time.realtimeSinceStartup + (frontier ? 40 : 15);
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Check(ready(), message);
        }

        IEnumerator Ready(int id)
        {
            yield return Until(() => L.Ready && Hud != null && Hud.State.Level == id &&
                SceneManager.GetActiveScene().name == "Gameplay", $"Gameplay ve HUD: {id}");
            FindFirstObjectByType<PawGuide>()?.Close();
            Check(GameplayLevelCatalog.SelectedId == id && Board.Moves == 0 && !Board.Won, $"Bölüm {id} temiz başladı");
        }

        void Click(string name, bool raycast = false)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>();
            Check(button != null && button.IsInteractable(), $"{name} etkileşimli");
            if (!raycast) { button.onClick.Invoke(); return; }
            var hit = RaycastButton(button, out var pointer, out var hitName);
            Check(hit == button.gameObject, $"UI raycast {name}: {hitName}");
            if (button is BottleTapButton)
            {
                pointer.eligibleForClick = true;
                ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerUpHandler);
            }
            ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerClickHandler);
        }

        static GameObject RaycastButton(Button button, out PointerEventData pointer, out string hitName)
        {
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            hitName = hits.Count == 0 ? "boş" : hits[0].gameObject.name;
            if (hits.Count == 0) return null;
            pointer.pointerCurrentRaycast = hits[0];
            return ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
        }

        IEnumerator ClickWhenReady(string name)
        {
            yield return Until(() =>
            {
                var button = GameObject.Find(name)?.GetComponent<Button>();
                return button != null && button.IsInteractable() && EventSystem.current != null &&
                    RaycastButton(button, out _, out _) == button.gameObject;
            }, $"{name}: dokunma alanı hazır");
            Click(name, true);
        }

        IEnumerator Win(int id, int expectedCoins)
        {
            var solution = PuzzleSolver.Solve(Board.Export());
            Check(solution.Status == SearchStatus.Solved && solution.Moves.Count > 0, $"{id} çözülebilir");
            if (frontier && expectedCoins > 0)
                expectedCoins = new CompletionReward(0, 3, true,
                    (bool)typeof(PrototypeApp).GetField("firstAttempt", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(FindFirstObjectByType<PrototypeApp>()), false).Coins;
            long coins = Ledger.Presentation.coins; int hints = Ledger.Remaining(BoosterKind.Hint);
            foreach (var move in solution.Moves)
            {
                int count = Board.Moves;
                Click($"Bottle {move.Source + 1}");
                yield return null;
                Click($"Bottle {move.Target + 1}");
                yield return Until(() => Board.Moves == count + 1 && (Board.Won || FindFirstObjectByType<PrototypeApp>().GuideCanInteract), $"{id}: döküş {count + 1}");
            }
            yield return Until(() => GameObject.Find("NextLevel") != null, $"{id}: yıldızlı sonuç ekranı");
            yield return null;
            Check(Board.Won && Hud.State.AwardedStars == 3 && Ledger.BestStars(id) == 3, $"{id}: üç yıldız hesaplandı ve kaydedildi");
            Check(Ledger.Presentation.coins - coins == expectedCoins && Ledger.LastCoinReward == expectedCoins &&
                Ledger.Remaining(BoosterKind.Hint) - hints == (expectedCoins > 0 ? EconomyConfig.Current.firstClearHints : 0),
                $"{id}: ödül {expectedCoins} coin (bakiye farkı {Ledger.Presentation.coins - coins}, kayıt {Ledger.LastCoinReward}, ipucu farkı {Ledger.Remaining(BoosterKind.Hint) - hints})");
        }

        IEnumerator Chain(int start, int end)
        {
            string previous = null;
            for (int id = start; id <= end; id++)
            {
                yield return Ready(id);
                var layout = Board.Export();
                string signature = LevelSignatures.Canonical(layout);
                var solution = PuzzleSolver.Solve(layout);
                Check(signature != previous && solution.Status == SearchStatus.Solved, $"{id}: farklı ve çözülebilir tahta");
                var level = GameplayLevelCatalog.Load(id);
                if (level.Generated != null)
                {
                    var pool = Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(Resources.Load<TextAsset>("Levels/GeneratedFallbacksV2").text);
                    var rebuilt = LevelGenerator.Resolve(WorldMapRules.Request(id), null, pool).level;
                    Check(rebuilt != null && LevelGenerator.Exact(layout, rebuilt.Export()), $"{id}: arşivsiz yeniden üretim birebir aynı");
                }
                boards.Add(new { id, signature, moves = solution.Moves.Count, difficulty = level.Generated?.difficulty.measured.ToString() ?? "Hazır" });
                previous = signature;
                if (id == end) break;
                yield return Win(id, 50);
                Check(Ledger.CurrentCompletedLevel == id && Ledger.CurrentUnlockedLevel == id + 1, $"{id}: tamamlanan ve açık bölüm ayrıldı");
                if (id == 1)
                {
                    var reopened = GameplayProgress.Open(key);
                    Check(reopened.Presentation.currentLevel == 2, $"Sonraki tıklanmadan kazanış diskte: devam={reopened.Presentation.currentLevel}", false);
                }
                Click("NextLevel", true);
                ObserveTransition(id);
                // A second click before the scene activates must not skip a level.
                GameObject.Find("NextLevel")?.GetComponent<Button>().onClick.Invoke();
                yield return null;
                ObserveTransition(id);
            }
        }

        void ObserveTransition(int from)
        {
            var app = FindFirstObjectByType<PrototypeApp>();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            transitions.Add(new
            {
                from, scene = SceneManager.GetActiveScene().name, hud = Hud?.State.Level, GameplayLevelCatalog.SelectedId,
                Ledger.LastError, saved = Ledger.Presentation.currentLevel, canLoad = Application.CanStreamedLevelBeLoaded("Gameplay"),
                frame = Time.frameCount, L.Ready,
                lastNavigationFrame = typeof(SceneNavigation).GetField("lastFrame", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null),
                navigating = app != null ? typeof(PrototypeApp).GetField("navigating", flags).GetValue(app) : null,
                status = ((TMPro.TMP_Text)typeof(PrototypeApp).GetField("status", flags).GetValue(app))?.text
            });
        }

        IEnumerator Open(int id)
        {
            var task = GameplayLevelCatalog.PrepareAsync(id);
            yield return Until(() => task.IsCompleted, $"{id}: hazırlık tamamlandı");
            Check(task.IsCompletedSuccessfully, $"{id}: doğrulanmış düzen hazır");
            Check(GameplayLevelCatalog.Select(id, Ledger), $"{id}: katalog seçimi");
            SceneManager.LoadScene("Gameplay");
            yield return null;
        }

        sealed class FailingArchive : ILevelArchive
        {
            readonly ILevelArchive archive;
            public volatile bool Fail = true;
            public FailingArchive(ILevelArchive archive) => this.archive = archive;
            public bool TryRead(int id, out GeneratedLevel level)
            {
                if (id == 3 && Fail) throw new IOException("İzole hazırlık hatası");
                return archive.TryRead(id, out level);
            }
            public void Write(GeneratedLevel level) => archive.Write(level);
        }

        IEnumerator RetryPreparation()
        {
            Seed(1);
            var archive = new FailingArchive(GameplayLevelCatalog.TestArchive);
            GameplayLevelCatalog.TestArchive = archive;
            yield return Open(2);
            yield return Ready(2);
            yield return Win(2, 50);
            Click("NextLevel", true);
            yield return Until(() => GameObject.Find("RewardBreakdown")?.GetComponent<TMPro.TMP_Text>().text ==
                L.Resolve(L.V("error.prepare")), "Hazırlık hatası sonuç ekranında görünür");
            Check(Hud.State.Level == 2 && Board.Won && Ledger.CurrentUnlockedLevel == 3 &&
                GameplayLevelCatalog.SelectedId == 2, "Hazırlık hatası oynanan bölümü ve kazanış kaydını korudu");
            archive.Fail = false;
            Click("NextLevel", true);
            yield return Ready(3);
            Check(Ledger.Presentation.coins == 50 && Ledger.BestStars(2) == 3, "Hazırlığı tekrar denemek ikinci ödül vermedi");
        }

        IEnumerator HomeReady()
        {
            yield return Until(() => SceneManager.GetActiveScene().name == "Home" && GameObject.Find("Play") != null &&
                Mewgic.BootLoadingController.Active == null, "Ana menü hazır");
            yield return null;
        }

        void SeedFrontier(int completed, bool persistent = false)
        {
            ClearJournal();
            key = null;
            var data = MapFlowProbe.Data(completed);
            foreach (var record in data.levels) record.bestStars = 3;
            data.presentation.currentLevel = completed + 1;
            data.presentation.reduceMotion = true;
            data.presentation.motionPreferenceSet = true;
            data.presentation.coins = 500;
            if (persistent)
            {
                key = "Mewgic.Tests.InfiniteLevels." + Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
                Reopen();
                return;
            }
            GameplayProgress.UseTestProfile(data);
            GameplayLevelCatalog.TestArchive = new FileLevelArchive(archiveRoot);
            GameplayLevelCatalog.ResetSelection();
            GameplayLevelCatalog.ResumeFromSave();
        }

        void CheckJournal()
        {
            key = "Mewgic.Tests.InfiniteLevels." + Guid.NewGuid().ToString("N");
            var legacy = MapFlowProbe.Data(1800);
            legacy.colorSwap = 1;
            legacy.presentation.coins = Enum.GetValues(typeof(BoosterKind)).Cast<BoosterKind>().Sum(ProgressLedger.BoosterPrice) + 100;
            var directory = Path.Combine(archiveRoot, "Progress");
            string json = JsonUtility.ToJson(legacy);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            var writer = GameplayProgress.Open(key, directory: directory);
            Check(writer.CurrentUnlockedLevel == 1801 && writer.Claim(1801, 3, true, out bool granted) && granted,
                "Eski PlayerPrefs profili okundu ve yeni kazanış işlendi");
            var stale = GameplayProgress.Open(key, directory: directory);
            foreach (BoosterKind kind in Enum.GetValues(typeof(BoosterKind)))
            {
                var item = ShopCatalog.Product(kind);
                int price = ProgressLedger.BoosterPrice(kind);
                int stock = writer.Remaining(kind);
                long before = writer.Presentation.coins, request = writer.NextShopPurchase;
                Check(writer.Purchase(item, price, request) == ShopPurchaseResult.Purchased &&
                    writer.Purchase(item, price, request) == ShopPurchaseResult.Duplicate &&
                    writer.Remaining(kind) == stock + 1 && writer.Presentation.coins == before - price,
                    $"{kind}: satın alma, stok, altın ve aynı istekle çift işlem engeli");
            }
            Check(writer.TrySpend(BoosterKind.BottleHammer) && writer.Remaining(BoosterKind.BottleHammer) == 0,
                "Çekiç stok düşümü dosya günlüğüne yazıldı");
            var file = Directory.GetFiles(directory, "*.json").Single(path =>
                !Path.GetFileNameWithoutExtension(path).Contains("."));
            Check(!stale.SetCurrentLevel(1) && PlayerPrefs.GetString(key) == json,
                "Eski açık yazıcı yeni kaydı ezmedi; eski PlayerPrefs kopyası duruyor");
            File.WriteAllText(file, "corrupt");
            var reopened = GameplayProgress.Open(key, directory: directory);
            Check(reopened.CurrentUnlockedLevel == 1802 && reopened.HighestCompletedLevel == 1801 &&
                reopened.Presentation.coins == writer.Presentation.coins && reopened.Remaining(BoosterKind.ColorSwap) == 2 &&
                reopened.Remaining(BoosterKind.BottleHammer) == 0 &&
                reopened.BestStars(1801) == 3, "Bozuk ana dosyadan yedekle devam, altın, stok ve yıldız kurtarıldı");
        }

        void CheckExistingPowers()
        {
            var board = new Puzzle(new[] { new[] { 0, 0, 0, 1 }, new[] { 1, 1, 1, 0 }, Array.Empty<int>(), Array.Empty<int>() });
            var saved = new ProgressData { undo = 1, hint = 1, extra = 1, shuffle = 1, colorSwap = 1 };
            var ledger = new ProgressLedger(saved, _ => { });
            var actions = new BoosterActions(board, ledger, board.Count);
            Check(!actions.Undo() && ledger.Remaining(BoosterKind.Undo) == 1, "Geri Al: döküş yokken stok korunuyor");
            Check(actions.Hint(out var move) && ledger.Remaining(BoosterKind.Hint) == 0 &&
                board.Pour(move.Source, move.Target) > 0 && !actions.Hint(out _), "Akıllı İpucu: geçerli çözüm hamlesi ve stok sınırı");
            Check(actions.Undo() && board.Moves == 0 && ledger.Remaining(BoosterKind.Undo) == 0, "Geri Al: gerçek döküş geri alındı");
            int length = PuzzleSolver.Solve(board.Export()).Moves.Count;
            Check(actions.Shuffle() && !actions.Shuffle() && PuzzleSolver.Solve(board.Export()).Moves.Count == length,
                "Karıştır: tüm şişeler taşındı ve çözüm uzunluğu korundu");
            Check(actions.Extra() && board.Count == 5 && !actions.Extra() &&
                PuzzleSolver.Solve(board.Export()).Status == SearchStatus.Solved, "Ek Şişe: tek boş şişe ve çözülebilir tahta");
            Check(!actions.ColorSwap(0, out _) && ledger.Remaining(BoosterKind.ColorSwap) == 1, "Renk Dönüşümü: erken bölümde kapalı");
        }

        static ProgressLedger PowerFixture(Action<ProgressData> persist = null) =>
            new ProgressLedger(new ProgressData { extra = 2, shuffle = 2, hint = 2, colorSwap = 2, bottleHammer = 2 },
                persist ?? (_ => { }));

        static int[] Totals(int[][] board) => board.SelectMany(bottle => bottle).OrderBy(color => color).ToArray();

        static int FindColorSwap(Puzzle board, int level)
        {
            var data = board.Export();
            for (int source = 0; source < data.Length; source++)
            {
                var copy = new Puzzle(data);
                if (new BoosterActions(copy, PowerFixture(), copy.Count, level).ColorSwap(source, out _)) return source;
            }
            return -1;
        }

        IEnumerator PowerShot(string name)
        {
            Directory.CreateDirectory(Evidence);
            ScreenCapture.CaptureScreenshot(Evidence + name + ".png");
            yield return new WaitForSecondsRealtime(.4f);
        }

        IEnumerator RunBoosterIntegration()
        {
            int level = 0, hammerSource = -1, swapSource = -1;
            int[][] layout = null;
            for (int id = 10001; id <= 10025 && level == 0; id++)
            {
                var task = GameplayLevelCatalog.PrepareAsync(id);
                yield return Until(() => task.IsCompletedSuccessfully, $"{id} prosedürel düzeni hazır");
                var bottles = task.Result.Bottles;
                for (int source = 0; source < bottles.Length && level == 0; source++)
                {
                    var board = new Puzzle(bottles);
                    var actions = new BoosterActions(board, PowerFixture(), board.Count, id);
                    if (!actions.Extra() || !actions.Shuffle() || !actions.Hammer(source)) continue;
                    int swap = FindColorSwap(board, id);
                    if (swap < 0) continue;
                    level = id; hammerSource = source; swapSource = swap; layout = bottles;
                }
            }
            Check(level > 0, "Aynı yüksek bölümde beş güç için geçerli sıra bulundu");
            CheckExistingPowers();
            CheckJournal();
            var independent = new Puzzle(layout);
            int independentHammer = Enumerable.Range(0, independent.Count).Where(source =>
            {
                var copy = new Puzzle(layout);
                return new BoosterActions(copy, PowerFixture(), copy.Count, level).Hammer(source);
            }).DefaultIfEmpty(-1).First();
            Check(independentHammer >= 0, "Çekiç, diğer güçler kullanılmadan da çalıştı");
            var failedLedger = PowerFixture(_ => throw new IOException("izole kayıt hatası"));
            Check(!new BoosterActions(independent, failedLedger, independent.Count, level).Hammer(independentHammer) &&
                failedLedger.Remaining(BoosterKind.BottleHammer) == 2 && LevelGenerator.Exact(layout, independent.Export()),
                "Çekiç kayıt hatasında tahta ve stok korunuyor");
            Check(FindColorSwap(independent, level) >= 0, "Renk Değiştir diğer güçler olmadan da çözücü onayı aldı");

            // Keep the actual board/archive isolated; the older large-save journal test covers persistence separately.
            SeedFrontier(level - 1);
            Check(Ledger.SetResources(10000, 0, 5) && Ledger.SetReducedMotion(false), "İzole test bakiyesi ve animasyonlar hazır");
            yield return Open(level);
            yield return Ready(level);
            Check(LevelGenerator.Exact(layout, Board.Export()), "Gerçek Gameplay aynı prosedürel düzeni açtı");
            int count = Board.Count;
            var totals = Totals(Board.Export());

            Click("BottleHammer", true);
            yield return null;
            Check(GameObject.Find("ShopPage_Powers") != null, "Sıfır Çekiç stoğunda Oyun Güçleri açıldı");
            yield return ClickWhenReady("BuyBottleHammer");
            yield return null;
            var receipt = Ledger.NextShopPurchase;
            Click("ShopConfirm", true);
            yield return null;
            Check(Ledger.Remaining(BoosterKind.BottleHammer) == 1 &&
                Ledger.Presentation.coins == 10000 - EconomyConfig.Current.bottleHammerPrice &&
                Ledger.Purchase(ShopItem.BottleHammer, EconomyConfig.Current.bottleHammerPrice, receipt) == ShopPurchaseResult.Duplicate,
                "Çekiç bir kez satın alındı ve aynı makbuz reddedildi");
            yield return PowerShot("HammerShop");
            Click("ShopClose", true);
            yield return null;
            Check(LevelGenerator.Exact(layout, Board.Export()), "Mağaza dönüşünde tahta ve hamleler korundu");

            Click("ColorSwap", true);
            yield return null;
            yield return ClickWhenReady("BuyColorSwap");
            yield return null;
            Click("ShopConfirm", true);
            yield return null;
            Click("ShopClose", true);
            yield return null;

            Click("BottleHammer", true);
            Check(Ledger.Remaining(BoosterKind.BottleHammer) == 1 && Board.Count == count, "Şişe seçilmeden Çekiç stok harcamadı");
            yield return null;
            Click("ExtraBottle", true);
            Check(Board.Count == count + 1 && Board.Bottle(count).Count == 0 &&
                Ledger.Remaining(BoosterKind.ExtraBottle) == 0, "Ek Şişe gerçek tahtada tek boş şişe ekledi");
            yield return PowerShot("ExtraBottle");

            var beforeShuffle = Board.Export();
            Click("Shuffle", true);
            Check(!LevelGenerator.Exact(beforeShuffle, Board.Export()) && Totals(Board.Export()).SequenceEqual(totals) &&
                Ledger.Remaining(BoosterKind.Shuffle) == 0 && PuzzleSolver.Solve(Board.Export()).Status == SearchStatus.Solved,
                "Karıştır yerleri değiştirdi, renkler ve çözüm korundu");
            yield return PowerShot("Shuffle");
            FindFirstObjectByType<PawGuide>()?.Close();
            yield return new WaitForSecondsRealtime(.25f);

            Click($"Bottle {hammerSource + 1}", true);
            yield return null;
            Click("BottleHammer", true);
            Check(Board.BottleHammerUsed && Board.Count == count && Totals(Board.Export()).SequenceEqual(totals) &&
                Ledger.Remaining(BoosterKind.BottleHammer) == 0 && Board.Moves == 0 && !Board.Won &&
                GameObject.Find("HammerBreak") != null,
                $"Çekiç şişeyi kaldırdı, hacim ve hamleler korundu; animasyon başladı ({Board.BottleHammerUsed}, {Board.Count}/{count}, stok {Ledger.Remaining(BoosterKind.BottleHammer)})");
            yield return new WaitForSecondsRealtime(.08f);
            yield return PowerShot("BottleHammer");

            Click($"Bottle {swapSource + 1}", true);
            yield return null;
            Click("ColorSwap", true);
            Check(Board.ColorSwapUsed && Totals(Board.Export()).SequenceEqual(totals) &&
                Ledger.Remaining(BoosterKind.ColorSwap) == 0 && !Board.Won,
                "Renk Değiştir Çekiç sonrası da çözücü onayıyla çalıştı");
            yield return PowerShot("ColorSwap");

            int hints = Ledger.Remaining(BoosterKind.Hint);
            Click("Hint", true);
            Check(Ledger.Remaining(BoosterKind.Hint) == hints - 1 &&
                FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length == 2,
                "İpucu gerçek çözüm yolundaki iki şişeyi işaretledi");
            yield return PowerShot("Hint");

            var solution = PuzzleSolver.Solve(Board.Export());
            Check(solution.Status == SearchStatus.Solved && solution.Moves.Count > 0 &&
                Board.Export().All(row => row.Length <= Puzzle.Capacity), "Beş güçten sonraki tahta çözülebilir ve kapasite geçerli");
            var snapshot = Ledger.Snapshot;
            var restored = new ProgressLedger(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(snapshot)), _ => {});
            Check(restored.Remaining(BoosterKind.BottleHammer) == 0 && restored.Remaining(BoosterKind.ColorSwap) == 0 &&
                restored.Presentation.coins == Ledger.Presentation.coins, "Yeni ve eski stoklar kayıt biçiminde korunuyor");
            Check(Ledger.LastCoinReward == 0 && Ledger.CurrentUnlockedLevel == level,
                "Güçler tek başına zafer ödülü veya bölüm ilerlemesi vermedi");
            FindFirstObjectByType<PawGuide>()?.Close();
            yield return null;
            var app = FindFirstObjectByType<PrototypeApp>();
            int selected = (int)typeof(PrototypeApp).GetField("selected", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);
            Click($"Bottle {selected + 1}");
            yield return null;
            yield return Win(level, new CompletionReward(0, 3, true, true, true).Coins);
            Check(Ledger.CurrentUnlockedLevel == level + 1, "Gerçek döküşlerle kazanış normal ilerlemeyi açtı");
            yield return new WaitForSecondsRealtime(4f);
            Click("Replay", true);
            yield return null;
            Check(LevelGenerator.Exact(layout, Board.Export()) && !Board.BottleHammerUsed && !Board.ExtraBottleUsed,
                "Tekrar Oyna özgün tahtayı ve deneme sınırlarını sıfırladı");
            yield return Win(level, 0);

            SeedFrontier(10004);
            yield return Open(10005);
            yield return Ready(10005);
            Check(Board.Count == 6, "10005 dört renk ve altı başlangıç şişesiyle açıldı");
            yield return null;
            Click("ExtraBottle", true);
            Check(Board.Count == 7 && PuzzleSolver.Solve(Board.Export()).Status == SearchStatus.Solved,
                "10005 yedi şişe ile çözülebilir");
            yield return PowerShot("ExtraBottleSeven");
            FullGamePreview.Size(2000);
            yield return Until(() => Screen.height == 2000, "Yedi şişe için 9:20 Game View");
            yield return null;
            foreach (var label in GameObject.Find("GameplayBoosters").GetComponentsInChildren<TMPro.TMP_Text>())
            {
                label.ForceMeshUpdate();
                Check(!label.isTextOverflowing && label.firstOverflowCharacterIndex == -1, "9:20 güç etiketi sığıyor: " + label.text);
            }
            for (int i = 1; i <= 7; i++)
            {
                var rect = (RectTransform)GameObject.Find($"Bottle {i}").transform;
                var position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                Check(position.x > 0 && position.x < Screen.width && position.y > 0 && position.y < Screen.height,
                    $"9:20 şişe {i} görünüm içinde");
            }
            yield return PowerShot("ExtraBottleSevenTall");
            FullGamePreview.Size(1600);
        }

        IEnumerator RunFrontier()
        {
            Directory.CreateDirectory(Evidence);
            CheckJournal();
            CheckExistingPowers();
            foreach (int level in new[] { 999, 1000, 9999, 99999, 999999 })
            {
                SeedFrontier(level - 1);
                yield return Open(level);
                yield return Chain(level, level + 1);
            }

            ScreenCapture.CaptureScreenshot(Evidence + "Level1000000.png");
            yield return new WaitForSecondsRealtime(.5f);
            SceneManager.LoadScene("LevelMap");
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true, "Milyonuncu bölümün gerçek LevelMap sahnesi hazır");
            var map = FindFirstObjectByType<MapWorld>();
            map.FocusCurrent();
            yield return null;
            Check(map.ActiveRegion == WorldMapRules.Island(1000000) && map.PooledIslands == 5 && map.ActivePointCount <= 40,
                $"1.000.000: doğru dünya, {map.PooledIslands} havuz adası, {map.ActivePointCount} etkin nokta");
            ScreenCapture.CaptureScreenshot(Evidence + "Map1000000.png");
            yield return new WaitForSecondsRealtime(.5f);

            SeedFrontier(100);
            yield return Open(99);
            yield return Ready(99);
            yield return Win(99, 0);
            Check(Ledger.CurrentUnlockedLevel == 101 && Ledger.HighestCompletedLevel == 100 && Ledger.Presentation.currentLevel == 101,
                "Eski bölüm tekrarı en yüksek kayıtları ve devam imlecini düşürmedi");
            SeedFrontier(120, persistent: true);
            int swapPrice = ProgressLedger.BoosterPrice(BoosterKind.ColorSwap);
            Check(Ledger.SetResources(swapPrice + 440, 0, 5), "Mağaza test bakiyesi hazır");
            SceneManager.LoadScene("Home");
            yield return HomeReady();
            if (GameObject.Find("ArrivalDismiss") != null) Click("ArrivalDismiss");
            Click("NavInventory");
            yield return null;
            Click("ShopTab_Powers");
            yield return null;
            var scroll = GameObject.Find("MagicShop").GetComponentInChildren<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 0;
            yield return null;
            ScreenCapture.CaptureScreenshot(Evidence + "BoosterShop.png");
            yield return new WaitForSecondsRealtime(.5f);
            Click("BuyColorSwap");
            yield return null;
            Click("ShopConfirm");
            Check(Ledger.Remaining(BoosterKind.ColorSwap) == 1 && Ledger.Presentation.coins == 440,
                $"Mağaza arayüzü: Renk Dönüşümü {swapPrice} altına bir stok verdi");

            int source = -1, swapLevel = 0;
            for (int id = 101; id <= 120 && source < 0; id++)
            {
                var task = GameplayLevelCatalog.PrepareAsync(id);
                yield return Until(() => task.IsCompletedSuccessfully, $"Dönüşüm denemesi için {id} hazır");
                var layout = task.Result.Bottles;
                for (int i = 0; i < layout.Length; i++)
                {
                    var clone = new Puzzle(layout);
                    var temporary = new ProgressLedger(new ProgressData { colorSwap = 1 }, _ => { });
                    if (!new BoosterActions(clone, temporary, clone.Count, id).ColorSwap(i, out _)) continue;
                    source = i;
                    swapLevel = id;
                    break;
                }
            }
            Check(source >= 0, $"{swapLevel}. bölümde doğrulanmış dönüşüm hedefi bulundu");
            yield return Open(swapLevel);
            yield return Ready(swapLevel);
            int before = PuzzleSolver.Solve(Board.Export()).Moves.Count;
            Click("ColorSwap");
            Check(Ledger.Remaining(BoosterKind.ColorSwap) == 1, "Şişe seçmeden kullanma stoğu korudu");
            Click($"Bottle {source + 1}");
            yield return null;
            Click("ColorSwap");
            var solved = PuzzleSolver.Solve(Board.Export());
            Check(Board.ColorSwapUsed && !Board.Won && solved.Status == SearchStatus.Solved &&
                solved.Moves.Count < before && Ledger.Remaining(BoosterKind.ColorSwap) == 0,
                $"Gerçek Gameplay: dönüşüm {before} → {solved.Moves.Count} hamle, bir stok");
            ScreenCapture.CaptureScreenshot(Evidence + "ColorSwap.png");
            yield return new WaitForSecondsRealtime(.5f);
            var reopened = GameplayProgress.Open(key);
            Check(reopened.Remaining(BoosterKind.ColorSwap) == 0 && reopened.Presentation.coins == 440 &&
                reopened.CurrentUnlockedLevel == 121, "Güç kullanımı sonrası yeniden açılan kayıtta stok, bakiye ve ilerleme doğru");
        }

        IEnumerator Run()
        {
            yield return Chain(1, 5);
            Seed(8); yield return Open(9); yield return Chain(9, 11);
            Seed(98); yield return Open(99); yield return Chain(99, 101);
            var latest = Board.Export();
            long coins = Ledger.Presentation.coins;
            yield return Open(99);
            yield return Ready(99);
            var replay = Board.Export();
            yield return Win(99, 0);
            Click("Replay", true);
            Check(Board.Moves == 0 && !Board.Won && LevelGenerator.Exact(replay, Board.Export()), "Tekrar Oyna 99'u aynı düzenle sıfırladı");
            yield return Win(99, 0);
            Click("NextLevel", true);
            yield return Ready(100);
            yield return Win(100, 0);
            Directory.CreateDirectory(Evidence);
            ScreenCapture.CaptureScreenshot(Evidence + "Result100.png");
            yield return new WaitForSecondsRealtime(.5f);
            Click("ResultHome", true);
            yield return Until(() => SceneManager.GetActiveScene().name == "LevelMap" &&
                FindFirstObjectByType<MapWorld>()?.Mounted == true, "Haritaya Dön gerçek LevelMap sahnesini açtı");
            Check(Ledger.CurrentUnlockedLevel == 101 && Ledger.CurrentCompletedLevel == 100 &&
                Ledger.Presentation.coins == coins && Ledger.BestStars(100) == 3, "Eski bölüm tekrarı ödül, yıldız ve açılan seviyeyi bozmadı");
            Reopen();
            Check(Ledger.Presentation.currentLevel == 101 && GameplayLevelCatalog.SelectedId == 101 &&
                Ledger.Presentation.coins == coins && Ledger.BestStars(99) == 3 && Ledger.BestStars(100) == 3,
                "Kayıt yeniden açıldı: devam=101, önceki yıldızlar ve coin korundu");
            Check(LevelGenerator.Exact(latest, GameplayLevelCatalog.Load(101).Bottles), "101 dosya arşivi yeniden açıldığında düzen birebir aynı");

            // Reproduce an existing player's stale resume cursor without changing their completion records.
            Check(Ledger.SetCurrentLevel(1), "Eski devam imleci izole kayda yazıldı");
            Reopen();
            SceneManager.LoadScene("Boot");
            yield return null;
            yield return HomeReady();
            Click("Play");
            yield return Ready(101);
            Check(LevelGenerator.Exact(latest, Board.Export()), "Boot ve ana menü eski imleci aştı; 101 aynı düzenle açıldı");
            ScreenCapture.CaptureScreenshot(Evidence + "Level101.png");
            yield return new WaitForSecondsRealtime(.5f);

            SceneManager.LoadScene("Home");
            yield return null;
            yield return HomeReady();
            Click("MissionsButton");
            Click("DailyAction_3");
            yield return Ready(101);
            Check(LevelGenerator.Exact(latest, Board.Export()), "Günlük görev Oyna doğru bölümü açtı");

            SceneManager.LoadScene("Home");
            yield return null;
            yield return HomeReady();
            FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>().Open(1);
            Click("WeeklyGo");
            yield return Ready(101);
            Check(LevelGenerator.Exact(latest, Board.Export()), "Ödül merkezi Oyna doğru bölümü açtı");
            yield return RetryPreparation();
        }
    }
}
#endif
