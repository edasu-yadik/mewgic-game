#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Daily;
using Mewgic.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class GameplayPolishProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/GameplayPolish";
        const string OnboardingOutput = "docs/ProductionReports/PawOnboardingReplay";
        [SerializeField] bool onboardingOnly;
        [SerializeField] string outputOverride;
        string OutputFolder => !string.IsNullOrEmpty(outputOverride) ? outputOverride : onboardingOnly ? OnboardingOutput : Output;
        static GameplayPolishProbe active;
        readonly List<string> checks = new List<string>(), errors = new List<string>();
        DateTimeOffset now = new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.FromHours(2));
        ProgressData disk;
        PrototypeApp App => FindFirstObjectByType<PrototypeApp>();
        PawGuide Guide => FindFirstObjectByType<PawGuide>();
        Puzzle Model => (Puzzle)typeof(PrototypeApp).GetField("puzzle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(App);
        ProgressLedger Ledger => GameplayProgress.Current;
        void Check(bool okay, string message) { if (!okay) throw new Exception(message); checks.Add(message); }
        public static void Begin(bool onboardingOnly = false, string output = null)
        {
            if (EditorApplication.isPlaying) throw new Exception("Önce Play Mode durdurulmalı");
            var player = new Dictionary<string, string>();
            foreach (string suffix in new[] { "", ".pending", ".backup", ".previous", ".corrupt" })
            { string key = GameplayProgress.StorageKey + suffix; player[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null; }
            SessionState.SetString("GameplayPolish.PlayerBefore", Newtonsoft.Json.JsonConvert.SerializeObject(player));
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            var probe = new GameObject("Gameplay Polish - isolated QA").AddComponent<GameplayPolishProbe>();
            probe.onboardingOnly = onboardingOnly;
            probe.outputOverride = output;
            Size(1600); EditorApplication.isPlaying = true;
        }
        static void Size(int height)
        {
            var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")); view.Show(); view.maximized = true; view.Focus();
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { height });
        }
        IEnumerator Resize(int height)
        {
            Size(height);
            float until = Time.unscaledTime + 4;
            while ((Screen.width != 900 || Screen.height != height) && Time.unscaledTime < until) yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            Check(Screen.width == 900 && Screen.height == height, $"Game View 900 × {height}");
        }
        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this; DontDestroyOnLoad(gameObject); Application.runInBackground = true;
            Directory.CreateDirectory(OutputFolder); Application.logMessageReceived += Log;
            Bind(0);
        }
        void Bind(int completed, ProgressData data = null)
        {
            if (data == null)
            {
                data = new ProgressData(); data.presentation.manualLocale = "tr";
                for (int id = 1; id <= completed; id++) data.levels.Add(new LevelRecord { level = id, bestStars = 3, rewardClaimed = true });
                data.daily.lastGiftUtc = data.daily.highestUtc = data.daily.taskCycleUtc = now.UtcTicks;
                data.daily.day = data.daily.highestDay = data.daily.lastGiftDay = DailyRules.Day(now);
            }
            disk = data.Copy();
            Attach(new ProgressLedger(disk, value => disk = value.Copy(), () => now));
        }
        void Attach(ProgressLedger ledger)
        {
            GameplayProgress.UseTestProfile();
            typeof(GameplayProgress).GetField("current", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, ledger);
            GameplayLevelCatalog.ResetSelection();
        }
        void Log(string text, string stack, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) errors.Add(text); }
        IEnumerator Start()
        {
            var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")); view.Show(); view.maximized = true; view.Focus(); Size(1600);
            string failure = null; var stack = new Stack<IEnumerator>(); stack.Push(Run());
            while (stack.Count > 0)
            {
                bool more = false; object item = null;
                try { more = stack.Peek().MoveNext(); if (more) item = stack.Peek().Current; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) { stack.Pop(); continue; }
                if (item is IEnumerator child && !(item is CustomYieldInstruction)) stack.Push(child); else yield return item;
            }
            var original = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(SessionState.GetString("GameplayPolish.PlayerBefore", "{}"));
            bool playerIntact = original.All(pair => pair.Value == null ? !PlayerPrefs.HasKey(pair.Key) : PlayerPrefs.GetString(pair.Key) == pair.Value);
            if (!playerIntact) errors.Add("Oyuncu kayıt anahtarları değişti"); else checks.Add("Oyuncunun ana/pending/backup/previous kayıtları byte düzeyinde korundu");
            File.WriteAllText(OutputFolder + "/UNITY_TESTS.json", Newtonsoft.Json.JsonConvert.SerializeObject(new { success = failure == null && errors.Count == 0, count = checks.Count, checks, errors, failure, editor = Application.unityVersion }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }
        IEnumerator Ready(string scene)
        {
            float until = Time.unscaledTime + 30;
            bool ReadyNow() => SceneManager.GetActiveScene().name == scene && Mewgic.Localization.L.Ready && (scene switch
            {
                "Gameplay" => Guide != null,
                "LevelMap" => FindFirstObjectByType<Mewgic.Map.MapWorld>()?.Mounted == true,
                _ => FindFirstObjectByType<DailyActivitiesPanel>() != null
            });
            while (!ReadyNow() && Time.unscaledTime < until) yield return null;
            yield return null; Canvas.ForceUpdateCanvases();
            Check(ReadyNow(), scene + " hazır");
        }
        IEnumerator LoadLevel(int id, int completed)
        {
            Bind(completed);
            yield return EnterLevel(id);
        }
        IEnumerator EnterLevel(int id)
        {
            var task = GameplayLevelCatalog.PrepareAsync(id);
            while (!task.IsCompleted) yield return null;
            Check(!task.IsFaulted && GameplayLevelCatalog.Select(id, Ledger), id + " bölümü hazırlandı");
            SceneManager.LoadScene("Gameplay"); yield return null; yield return Ready("Gameplay");
        }
        static void Tap(string name) => GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        static List<RaycastResult> Hits(RectTransform target)
        {
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center)) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits); return hits;
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var render = RenderTexture.GetTemporary(Screen.width, Screen.height); var flip = RenderTexture.GetTemporary(Screen.width, Screen.height);
            var old = RenderTexture.active; var image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(render); Graphics.Blit(render, flip, new Vector2(1, -1), Vector2.up);
            RenderTexture.active = flip; image.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); image.Apply();
            File.WriteAllBytes(OutputFolder + "/" + name + ".png", image.EncodeToPNG()); RenderTexture.active = old;
            RenderTexture.ReleaseTemporary(render); RenderTexture.ReleaseTemporary(flip); Destroy(image);
        }
        IEnumerator WaitForPaw()
        {
            float until = Time.unscaledTime + 8;
            while (!Guide.OnboardingVisible && Time.unscaledTime < until) yield return null;
        }

        bool IsSolvableMove()
        {
            var board = new Puzzle(Model.Export());
            var move = Guide.Recommended;
            return board.Pour(move.Source, move.Target) > 0 && PuzzleSolver.Solve(board.Export()).Status == SearchStatus.Solved;
        }

        IEnumerator ReopenCompletedProfile()
        {
            Bind(4);
            var profile = Ledger.Snapshot;
            profile.completedTutorials.Add(PawGuide.BasicId);
            for (int id = 1; id <= 4; id++) profile.completedTutorials.Add($"paw.level.{id}.v1");

            string key = "Mewgic.Tests.PawOnboardingReplay." + Guid.NewGuid().ToString("N");
            try
            {
                PlayerPrefs.SetString(key, JsonUtility.ToJson(profile));
                PlayerPrefs.Save();
                GameplayProgress.RestorePlayerProfile();
                Attach(GameplayProgress.Open(key, () => now));

                for (int id = 1; id <= 4; id++)
                {
                    yield return EnterLevel(id);
                    string beforeHint = JsonUtility.ToJson(Ledger.Snapshot);
                    Check(!Guide.OnboardingVisible && Ledger.Completed(id) && Ledger.BestStars(id) == 3 &&
                        Ledger.TutorialCompleted($"paw.level.{id}.v1"), id + " yeniden açılan kayıtta tamamlanma ve eski öğretici işareti var");
                    yield return WaitForPaw();
                    Check(Guide.OnboardingVisible && IsSolvableMove() && Model.Moves == 0 &&
                        JsonUtility.ToJson(Ledger.Snapshot) == beforeHint, id + " tamamlanmış bölümde pati tekrar açılıyor ve kayıt değişmiyor");
                }

                App.UseBooster(BoosterKind.Hint);
                int selected = App.GuideSelectedBottle;
                Check(selected >= 0 && !Guide.OnboardingVisible, "İpucu güçlendiricisi seçim yapıp önceki patiyi kapatıyor");
                yield return WaitForPaw();
                Check(Guide.OnboardingVisible && Guide.State == PawGuide.Phase.Target && Guide.Recommended.Source == selected &&
                    IsSolvableMove(), "Güçlendiriciyle seçilen şişe için pati yalnız geçerli hedefi gösteriyor");
            }
            finally
            {
                foreach (string suffix in new[] { "", ".pending", ".backup", ".previous", ".corrupt" }) PlayerPrefs.DeleteKey(key + suffix);
                PlayerPrefs.Save();
            }
        }

        IEnumerator Run()
        {
            yield return Ready("Gameplay");
            for (int id = 1; id <= 4; id++)
            {
                if (id > 1) yield return LoadLevel(id, id - 1);
                float idleStarted = Time.unscaledTime;
                yield return WaitForPaw();
                if (id == 1) Check(Time.unscaledTime - idleStarted is >= 2.4f and <= 3.6f, "Pati yaklaşık üç saniye bekliyor");
                Check(Guide.OnboardingVisible && !Guide.DemoRunning && !Guide.BlocksGameplayActions && Model.Moves == 0,
                    id + " bölümde bekleyen oyuncuya kontrolü kilitlemeyen pati gösteriliyor");
                yield return new WaitForSecondsRealtime(.85f);
                if (id == 1) yield return Shot("01-Paw-Source");
                var move = Guide.Recommended; var copy = new Puzzle(Model.Export());
                Check(copy.Pour(move.Source, move.Target) > 0, id + " için önerilen ilk aktarım geçerli");
                var source = GameObject.Find("Bottle " + (move.Source + 1));
                Check(Hits((RectTransform)source.transform).First().gameObject == source, id + " pati şişe raycast alanını engellemiyor");
                Tap(source.name);
                Check(!Guide.OnboardingVisible && Model.Moves == 0, id + " oyuncu kaynağa dokununca pati çekiliyor");
                if (id == 4)
                {
                    yield return new WaitForSecondsRealtime(3.35f);
                    Check(Guide.OnboardingVisible && Guide.State == PawGuide.Phase.Target && Guide.Recommended.Source == move.Source,
                        "Seçili kaynakla beklenince yalnız doğru hedef gösteriliyor");
                    yield return Shot("02-Paw-Target");
                }
                string beforeMove = JsonUtility.ToJson(Ledger.Snapshot);
                Tap("Bottle " + (move.Target + 1)); yield return new WaitForSecondsRealtime(1.15f);
                Check(Model.Moves == 1 && !Guide.OnboardingVisible && JsonUtility.ToJson(Ledger.Snapshot) == beforeMove,
                    id + " başarılı hamlede pati çekiliyor ve kayıt yazılmıyor");
                yield return WaitForPaw();
                var oldMove = new Puzzle(Model.Export());
                Check(Guide.OnboardingVisible && IsSolvableMove() &&
                    (oldMove.Pour(move.Source, move.Target) > 0 || Guide.Recommended.Source != move.Source || Guide.Recommended.Target != move.Target),
                    id + " tekrar beklemede güncel ve çözülebilir hamle gösteriliyor");
                if (id == 1)
                {
                    yield return EnterLevel(id);
                    Check(!Guide.OnboardingVisible && Model.Moves == 0, "Aynı oturumda tekrar giriş patinin beklemesini sıfırlıyor");
                    yield return WaitForPaw();
                    Check(Guide.OnboardingVisible && IsSolvableMove(), "Aynı oturumda tekrar girişte pati yeniden gösteriliyor");
                }
            }
            yield return ReopenCompletedProfile();
            yield return LoadLevel(1, 0);
            var firstSolution = PuzzleSolver.Solve(Model.Export());
            Check(firstSolution.Status == SearchStatus.Solved, "İlk bölümün gerçek şişe dizilimi çözülebilir");
            foreach (var step in firstSolution.Moves)
            {
                Tap("Bottle " + (step.Source + 1));
                Tap("Bottle " + (step.Target + 1));
                yield return new WaitForSecondsRealtime(1.1f);
            }
            int wonStars = Ledger.BestStars(1);
            Check(Model.Won && Ledger.Completed(1) && wonStars > 0 && GameObject.Find("ResultHome") != null,
                "1. bölüm gerçek tıklama ve döküşlerle kazanıldı");
            Tap("ResultHome"); yield return Ready("LevelMap");
            var map = FindFirstObjectByType<Mewgic.Map.MapWorld>();
            map.SkipJourney();
            map.OpenChapter(0, 0);
            map.Points.First(point => point.kind == Mewgic.Map.MapPointKind.LevelSlot && point.regionIndex == 0 &&
                point.slotIndex == 0 && point.gameObject.activeInHierarchy).GetComponent<Button>().onClick.Invoke();
            yield return Ready("Gameplay");
            yield return WaitForPaw();
            Check(Guide.OnboardingVisible && IsSolvableMove() && Ledger.BestStars(1) == wonStars,
                "Gerçek kazanıştan sonra haritadan aynı bölümü açınca pati ve yıldızlar korunur");
            yield return LoadLevel(5, 4); yield return new WaitForSecondsRealtime(3.4f);
            Check(!Guide.OnboardingVisible && !Guide.DemoRunning && Model.Moves == 0, "5. bölümde yoğun öğretici yok");
            if (onboardingOnly) yield break;
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                yield return Resize(height);
                FindFirstObjectByType<GameplayPresentation>().Layout(); Canvas.ForceUpdateCanvases();
                var cat = GameObject.Find("GameplayWizardCat").GetComponent<RectTransform>(); var safe = GameObject.Find("Safe area").GetComponent<RectTransform>();
                float unit = Mathf.Min(safe.rect.width / 900, safe.rect.height / 1600);
                float previous = Mathf.Min(safe.rect.width * .18f / cat.rect.width, 300 * unit / cat.rect.height);
                float growth = cat.localScale.x / previous - 1;
                Check(growth is >= .10f and <= .16f, $"{height} maskot büyümesi {growth:P1} (hedef yaklaşık %13)");
                var corners = new Vector3[4]; cat.GetWorldCorners(corners);
                Check(corners.All(point => safe.rect.Contains(safe.InverseTransformPoint(point))), height + " büyütülmüş maskot Safe Area içinde");
                var catRect = Rect.MinMaxRect(safe.InverseTransformPoint(corners[0]).x, safe.InverseTransformPoint(corners[0]).y,
                    safe.InverseTransformPoint(corners[2]).x, safe.InverseTransformPoint(corners[2]).y);
                foreach (var bottle in FindObjectsByType<GameplayBottleView>(FindObjectsSortMode.None))
                {
                    bottle.GetComponent<RectTransform>().GetWorldCorners(corners);
                    var bounds = Rect.MinMaxRect(safe.InverseTransformPoint(corners[0]).x, safe.InverseTransformPoint(corners[0]).y,
                        safe.InverseTransformPoint(corners[2]).x, safe.InverseTransformPoint(corners[2]).y);
                    Check(!catRect.Overlaps(bounds), height + " maskot şişeyi kapatmıyor: " + bottle.name);
                }
                yield return Shot("03-Gameplay-" + height);
            }
            Size(1600); yield return null; yield return null;
            // İki bağımsız şişede aynı karede tamamlama; başlangıçtaki dolu şişe ve tekrar Present ayrı doğrulanır.
            var bottles = FindObjectsByType<GameplayBottleView>(FindObjectsSortMode.None).OrderBy(view => view.name).Take(3).ToArray();
            foreach (var bottle in bottles) bottle.Present(new[] { 0, 0, 0 }, false);
            bottles[0].Present(new[] { 0, 0, 0, 0 }, false); bottles[1].Present(new[] { 1, 1, 1, 1 }, false);
            int plays = bottles[0].Completion.PlayCount;
            bottles[0].Present(new[] { 0, 0, 0, 0 }, false);
            Check(bottles[0].Completion.Playing && bottles[1].Completion.Playing && bottles[0].Completion.PlayCount == plays,
                "İki şişe bağımsız kutluyor; aynı tamamlanmış verinin tekrarı efekti yeniden başlatmıyor");
            yield return new WaitForSecondsRealtime(.24f); yield return Shot("04-Bottle-Completion");
            bottles[0].SetPaused(true); yield return new WaitForSecondsRealtime(.2f); Check(bottles[0].Completion.Playing, "Pause kutlamayı donduruyor"); bottles[0].SetPaused(false);
            yield return new WaitForSecondsRealtime(1.2f);
            Check(!bottles[0].Completion.Playing && !bottles[1].Completion.Playing, "Kutlama sonlanıyor ve sonsuz dönmüyor");
            bottles[0].Present(new[] { 0, 0 }, false); bottles[0].Present(new[] { 0, 0, 0, 0 }, false);
            Check(bottles[0].Completion.PlayCount == plays + 1, "Undo sonrası gerçekten yeniden tamamlanınca tekrar kutlanabiliyor");
            Ledger.SetReducedMotion(true); bottles[0].Present(new[] { 0, 0 }, false); bottles[0].Present(new[] { 0, 0, 0, 0 }, false);
            yield return new WaitForSecondsRealtime(.45f);
            Check(!bottles[0].Completion.Playing, "Az hareket tercihinde kısa kutlama sonlanıyor"); Ledger.SetReducedMotion(false);
            var seeded = new GameObject("QA-Initially-Complete", typeof(RectTransform), typeof(GameplayBottleView));
            seeded.transform.SetParent(GameObject.Find("Bottle board").transform, false);
            seeded.SetActive(false);
            var seededView = seeded.GetComponent<GameplayBottleView>();
            seededView.Initialize(Resources.Load<Sprite>("Gameplay/Gameplay_GlassShell_v001"), new[] { Color.cyan });
            seededView.Present(new[] { 0, 0, 0, 0 }, false);
            Check(seededView.Completion.PlayCount == 0, "Sahne başında zaten dolu şişe kutlamayı tetiklemiyor"); Destroy(seeded);
            var fixture = new Puzzle(new[] { new[] { 0, 0, 0 }, new[] { 1, 1, 1 }, new[] { 0 }, new[] { 1 },
                new[] { 2, 3, 2, 3 }, new[] { 3, 2, 3, 2 }, new int[0], new int[0] });
            typeof(PrototypeApp).GetField("puzzle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(App, fixture);
            typeof(PrototypeApp).GetField("selected", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(App, -1);
            typeof(PrototypeApp).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(App, new object[] { false });
            yield return null; yield return null;
            Tap("Bottle 3"); Tap("Bottle 1"); yield return new WaitForSecondsRealtime(.95f);
            var poured = GameObject.Find("Bottle 1").GetComponent<GameplayBottleView>();
            Check(Model.Moves == 1 && Model.Bottle(0).Count == 4 && poured.Completion.PlayCount == 1 && poured.Completion.Playing,
                "Gerçek şişe tıklaması ve döküş bitişi tek renk kutlamasını tetikliyor");
            yield return Shot("07-Real-Pour-Completion");

            Bind(4); disk.daily.progress = new[] { 2, 2, 3, 0 }; Bind(4, disk);
            SceneManager.LoadScene("Home"); yield return null; yield return Ready("Home");
            var daily = FindFirstObjectByType<DailyActivitiesPanel>();
            var icon = GameObject.Find("QuestIcon").GetComponent<RectTransform>(); var mission = GameObject.Find("MissionsButton");
            var hits = Hits(icon); Check(hits.Count > 0 && hits[0].gameObject == mission, "Görev ikonunun gerçek üst raycast hedefi MissionsButton");
            ExecuteEvents.Execute(mission, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            Check(daily.IsOpen && !daily.GiftVisible, "Doğrudan ikona tıklama görev panelini açıyor");
            string before = GameObject.Find("DailyReset").GetComponent<TMP_Text>().text;
            string homeBefore = mission.transform.Find("HomeQuestCountdown").GetComponent<TMP_Text>().text;
            now = now.AddSeconds(1); yield return new WaitForSecondsRealtime(1.15f);
            Check(GameObject.Find("DailyReset").GetComponent<TMP_Text>().text != before, "Popup sayacı saniye saniye canlı güncelleniyor");
            Check(mission.transform.Find("HomeQuestCountdown").GetComponent<TMP_Text>().text != homeBefore,
                "Ana sayfa sayacı da aynı anda güncelleniyor");
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Size(height); yield return null; yield return null; daily.Layout(); Canvas.ForceUpdateCanvases();
                yield return Shot("05-Quests-" + height);
                Check(!GameObject.Find("DailyReset").GetComponent<TMP_Text>().isTextOverflowing, height + " sayaç okunur ve taşmıyor");
            }
            int hintsBefore = Ledger.Remaining(BoosterKind.Hint);
            Tap("DailyAction_1"); Tap("DailyAction_1");
            Check(Ledger.DailySnapshot.claimed[1] && Ledger.Remaining(BoosterKind.Hint) == hintsBefore + 1 &&
                !GameObject.Find("DailyAction_1").GetComponent<Button>().interactable,
                "Görev panelinde iki ödül tıklaması yalnız bir ödül verir");
            yield return Shot("08-Daily-Claimed");
            daily.Close(); Size(1600); yield return null; yield return null; yield return new WaitForSecondsRealtime(.25f); yield return Shot("06-Home-Quest-Icon");
            hits = Hits(mission.transform.Find("Label").GetComponent<RectTransform>());
            Check(hits.Count > 0 && hits[0].gameObject == mission, "Görev yazısı ve ikon aynı doğal tıklama alanında: " + string.Join(",", hits.Select(hit => hit.gameObject.name)));
            var saved = JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(disk));
            var waitBeforeReload = Ledger.DailyTaskWait;
            Bind(4, saved); SceneManager.LoadScene("Home"); yield return null; yield return Ready("Home");
            Check(Ledger.DailyTaskWait == waitBeforeReload && Ledger.DailySnapshot.claimed[1] &&
                Ledger.Remaining(BoosterKind.Hint) == hintsBefore + 1,
                "Ana sayfa yeniden açılınca 24 saatlik sayaç ve ödül kaydı korunur");
            now = now.AddHours(25); Bind(4, saved); SceneManager.LoadScene("Home"); yield return null; yield return Ready("Home");
            Check(Ledger.DailySnapshot.progress.All(x => x == 0) && Ledger.DailyTaskWait.TotalHours > 22,
                "Oyun kapalıyken geçen 25 saatten sonra görevler yenileniyor ve sayaç doğru kalıyor");
            PrototypeApp.RequestTutorialReplay(); yield return null; yield return Ready("Gameplay");
            Check(Guide.DemoRunning && Guide.BlocksGameplayActions, "Ayarlar için açıkça istenen eski demo ayrı akışta çalışmaya devam ediyor");
            Guide.Close();
            Check(Mewgic.Localization.L.Missing.Count == 0, "Yerelleştirme anahtarı eksiği yok");
        }
        void OnDestroy()
        {
            if (active != this) return; active = null; Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile(); GameplayLevelCatalog.ResetSelection();
        }
    }
}
#endif
