#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class MapFlowProbe : MonoBehaviour
    {
        [SerializeField] string output = "docs/ProductionReports/MapFlow";
        [SerializeField] bool recordMotion;
        static readonly string[] SaveSuffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        readonly List<string> checks = new List<string>(), errors = new List<string>();
        static MapFlowProbe active;
        ProgressData disk;
        string journalKey, motionFolder;
        bool usingJournal;
        MapWorld Map => FindFirstObjectByType<MapWorld>();
        ProgressLedger Ledger => GameplayProgress.Current;
        static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        void Check(bool condition, string message) { Require(condition, message); checks.Add(message); }
        public static ProgressData Data(int completed)
        {
            var data = new ProgressData();
            data.presentation.manualLocale = "tr";
            data.worldMap.migrated = true;
            data.worldMap.lastCelebratedIsland = (completed / 50);
            for (int i = 1; i <= completed; i++) data.levels.Add(new LevelRecord { level = i, bestStars = i % 3 + 1, rewardClaimed = true });
            return data;
        }
        static Dictionary<string, string> PlayerSave() => SaveSuffixes.ToDictionary(
            suffix => GameplayProgress.StorageKey + suffix,
            suffix => PlayerPrefs.HasKey(GameplayProgress.StorageKey + suffix) ? PlayerPrefs.GetString(GameplayProgress.StorageKey + suffix) : null);

        public static void Begin(string folder = "docs/ProductionReports/MapFlow", bool captureMotion = false)
        {
            Require(!EditorApplication.isPlaying, "Önce Play Mode durdurulmalı");
            SessionState.SetString("MapFlow.PlayerBefore", Newtonsoft.Json.JsonConvert.SerializeObject(PlayerSave()));
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            var probe = new GameObject("Map Flow QA - isolated").AddComponent<MapFlowProbe>();
            probe.output = folder;
            probe.recordMotion = captureMotion;
            typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            Part12BEditorSize(1600);
            EditorApplication.isPlaying = true;
        }
        static void Part12BEditorSize(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { height });
        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            Directory.CreateDirectory(output);
            if (recordMotion)
            {
                motionFolder = "Library/ComprehensiveAuditFrames/" + Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(motionFolder);
            }
            Application.logMessageReceived += Log;
            Bind(Data(90));
            Ledger.Claim(91, 3, true, out _);
            Reopen();
        }
        void Bind(ProgressData data)
        {
            usingJournal = false;
            disk = data.Copy();
            Reopen();
        }
        void UseJournal()
        {
            journalKey = "Mewgic.Tests.MapFlow.Runtime." + Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(journalKey, JsonUtility.ToJson(disk));
            PlayerPrefs.Save();
            usingJournal = true;
            Reopen();
        }

        void Reopen()
        {
            GameplayProgress.UseTestProfile();
            var ledger = usingJournal ? GameplayProgress.Open(journalKey) :
                new ProgressLedger(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(disk)), value => disk = value.Copy());
            typeof(GameplayProgress).GetField("current", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, ledger);
            GameplayLevelCatalog.ResetSelection();
        }
        void Log(string message, string stack, LogType kind)
        {
            if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert) errors.Add(message);
        }
        IEnumerator Start()
        {
            var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            view.Show(); view.maximized = true; view.Focus();
            Part12BEditorSize(1600);
            yield return null; yield return null;

            string failure = null;
            var stack = new Stack<IEnumerator>(); stack.Push(Run());
            while (stack.Count > 0)
            {
                bool more = false; object item = null;
                try { more = stack.Peek().MoveNext(); if (more) item = stack.Peek().Current; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) { stack.Pop(); continue; }
                if (item is IEnumerator child && !(item is CustomYieldInstruction)) stack.Push(child);
                else yield return item;
            }
            if (SessionState.GetString("MapFlow.PlayerBefore", "") != Newtonsoft.Json.JsonConvert.SerializeObject(PlayerSave()))
                errors.Add("Oyuncu kayıt anahtarları değişti");
            else
                checks.Add("Oyuncu ve kurtarma kayıtları byte düzeyinde korundu");
            File.WriteAllText(output + "/UNITY_TESTS.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            { success = failure == null && errors.Count == 0, count = checks.Count, checks, errors, failure, editor = Application.unityVersion, motionFolder }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }
        IEnumerator Ready()
        {
            float until = Time.realtimeSinceStartup + 25;
            while ((Map == null || !Map.Mounted || !Mewgic.Localization.L.Ready) && Time.realtimeSinceStartup < until) yield return null;
            yield return null;
            Check(Map != null && Map.Mounted, "LevelMap hazır");
        }
        IEnumerator Reload()
        {
            SceneManager.LoadScene("LevelMap"); yield return null; yield return Ready();
        }
        MapNodeBadge Node(int id) => Map.Points.First(p => p.kind == MapPointKind.LevelSlot &&
            p.gameObject.activeInHierarchy && p.regionIndex * 50 + p.slotIndex + 1 == id).GetComponent<MapNodeBadge>();
        IEnumerator Shot(string name, int width = 0)
        {
            yield return new WaitForEndOfFrame();
            var render = RenderTexture.GetTemporary(Screen.width, Screen.height);
            int imageWidth = width > 0 ? width : Screen.width;
            int imageHeight = Mathf.RoundToInt(imageWidth * Screen.height / (float)Screen.width);
            var flipped = RenderTexture.GetTemporary(imageWidth, imageHeight);
            var previous = RenderTexture.active;
            var texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(render);
            Graphics.Blit(render, flipped, new Vector2(1, -1), Vector2.up);
            RenderTexture.active = flipped;
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); texture.Apply();
            File.WriteAllBytes((width > 0 ? motionFolder : output) + "/" + name + ".png", texture.EncodeToPNG());
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(render); RenderTexture.ReleaseTemporary(flipped); Destroy(texture);
        }
        static IEnumerator Until(Func<bool> condition, float timeout = 4)
        {
            float limit = Time.unscaledTime + timeout;
            while (!condition() && Time.unscaledTime < limit) yield return null;
        }
        IEnumerator Run()
        {
            yield return Ready();
            Check(Map.JourneyTarget == 92 && Ledger.MapJourney.Pending, "Kayıt yeniden açılınca 91→92 bekliyor ve başlıyor");
            Check(Node(92).Status == MapLevelStatus.Locked, "Hedef varışa kadar görsel kilitli");
            var cat = GameObject.Find("WizardCatGuide").GetComponent<RectTransform>();
            var catView = cat.GetComponent<JourneyCatImage>();
            var contact = GameObject.Find("JourneyCatContact").GetComponent<RectTransform>();
            var backplate = GameObject.Find("CrystalTerrace_5").GetComponent<RawImage>();
            var water = backplate.material;
            float waterClock = water.GetFloat("_EnvironmentTime");
            Check(backplate.texture.name == "Crystal_Reference_91_100" &&
                water.IsKeywordEnabled("MEWGIC_CRYSTAL_REFERENCE") && !ShaderUtil.ShaderHasError(water.shader),
                "Kristal referans görseli ve katmanlı şelale shader'ı etkin");
            Vector2 before = cat.anchoredPosition;
            yield return Until(() => catView.Pose == JourneyCatPose.Blink);
            Check(catView.Pose == JourneyCatPose.Blink && catView.texture.name == "CrystalCat_RestWake" &&
                Vector2.Distance(before, cat.anchoredPosition) < .01f, "Kedi yerinde gözünü açıp kulağını kaldırıyor");
            yield return Shot("09-Wake");
            yield return Until(() => catView.Pose == JourneyCatPose.Standing);
            Check(catView.Pose == JourneyCatPose.Standing && Vector2.Distance(before, cat.anchoredPosition) < .01f,
                "Mutlu ayağa kalkma pozu ilk adımdan önce aynı zeminde");
            yield return Shot("10-Standing");
            var paw = GameObject.Find("GoldenPawTrail").GetComponent<JourneyPawTrail>();
            yield return Until(() => paw.canvasRenderer.GetMesh() != null && paw.canvasRenderer.GetMesh().vertexCount > 0);
            Check(Vector2.Distance(before, cat.anchoredPosition) > 5, "Maskot rota boyunca ilerliyor");
            Check(catView.PoseIndex > 0 && catView.texture.name == "MapCat_Poses" &&
                Vector2.Distance(cat.anchoredPosition, contact.anchoredPosition) < .01f, "Asasız yürüyüş pozu ve temas gölgesi zemine bağlı");
            Check(water.GetFloat("_EnvironmentTime") > waterClock, "Şelale saati kaydırmadan bağımsız ilerliyor");
            Check(paw.canvasRenderer.GetMesh() != null && paw.canvasRenderer.GetMesh().vertexCount > 0, "Altın pati mesh'i kademeli oluşuyor");
            var viewport = GameObject.Find("JourneyScroll").GetComponent<RectTransform>();
            Check(paw.canvasRenderer.GetMesh().vertices.Any(vertex => RectTransformUtility.RectangleContainsScreenPoint(viewport,
                RectTransformUtility.WorldToScreenPoint(null, paw.transform.TransformPoint(vertex)))), "Pati izleri görünür viewport içinde ve rota ile aynı koordinat uzayında");
            yield return Shot("01-Walk");
            yield return Until(() => Node(92).Status == MapLevelStatus.Current);
            Check(Node(92).Status == MapLevelStatus.Current && Map.JourneyRunning, "Varışta kilit açılıp altın burst başlıyor");
            yield return new WaitForSecondsRealtime(.2f);
            yield return Shot("02-Unlock");
            yield return Until(() => !Map.JourneyRunning);
            Check(!Map.JourneyRunning && !Ledger.MapJourney.Pending && Ledger.MapJourney.displayedLevel == 92 && Ledger.MapJourney.lastShownLevel == 92,
                "Tamamlanınca gösterilen konum ve already-shown atomik kaydediliyor");
            Check(catView.Pose == JourneyCatPose.Curled && Vector2.Distance(cat.anchoredPosition, contact.anchoredPosition) < .01f,
                "Maskot yerde kıvrılmış dinlenme pozuna dönüyor");
            var scroll = viewport.GetComponent<ScrollRect>();
            Check(backplate.transform.IsChildOf(scroll.content) && GameObject.Find("StoneRoute") == null &&
                Node(91).transform.position.x < Node(92).transform.position.x &&
                Node(93).transform.position.x < Node(94).transform.position.x &&
                Node(97).transform.position.x < Node(98).transform.position.x, "Kristal taşları, çizilmiş köprüler ve maskot aynı sayfada");
            var routeBefore = scroll.content.anchoredPosition;
            var atmosphere = GameObject.Find("CrystalAnchoredAtmosphere").transform;
            var atmosphereBefore = atmosphere.position;
            scroll.verticalNormalizedPosition = .38f;
            yield return null; yield return null;
            Check(Vector2.Distance(scroll.content.anchoredPosition, routeBefore) > 500 &&
                GameObject.Find("CrystalBrand") != null && atmosphere.position == atmosphereBefore &&
                !atmosphere.IsChildOf(scroll.content), "Kristal rotası kayarken sabit dış sis ve başlık yerinde kalır");
            ((MapScrollRect)scroll).UserInput.Invoke();
            yield return new WaitForSecondsRealtime(1f);
            Check(Mathf.Abs(scroll.content.anchoredPosition.y / viewport.rect.height - 2) < .01f,
                "Kaydırma bırakılınca en yakın teras kadraja yerleşir");
            var headingBounds = new Vector3[4];
            GameObject.Find("JourneyHeading").GetComponent<RectTransform>().GetWorldCorners(headingBounds);
            Check(Node(71).transform.position.y < headingBounds[0].y &&
                Node(80).transform.position.y > GameObject.Find("ChapterClose").transform.position.y,
                "Önceki terasın ilk ve son taşı sabit kontrollerin arasında");
            yield return Shot("06-Scroll-Settled");
            Reopen(); yield return Reload();
            Check(!Map.JourneyRunning && Node(92).Status == MapLevelStatus.Current, "İkinci giriş: running=" + Map.JourneyRunning + " status=" + Node(92).Status + " current=" + Map.Progression.CurrentId + " pending=" + Ledger.MapJourney.Pending);
            Ledger.Claim(91, 3, true, out _); yield return Reload();
            Check(!Map.JourneyRunning && Ledger.CurrentUnlockedLevel == 92, "Eski bölüm tekrarı animasyon üretmiyor");

            Bind(Data(98)); UseJournal(); Ledger.Claim(99, 3, true, out _); yield return Reload();
            yield return new WaitForSecondsRealtime(.65f);
            typeof(MapWorld).GetMethod("OnApplicationPause", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Map, new object[] { true });
            Check(Ledger.MapJourney.Pending && Ledger.MapJourney.lastShownLevel == 99, "Arka plana geçiş animasyonu gösterildi olarak tüketmiyor");
            // Destroy the scene before arrival: durable acknowledgement must not be written.
            Reopen(); yield return Reload();
            Check(Map.JourneyTarget == 100 && Ledger.MapJourney.Pending, "Yarıda kesilen sunum gerçek PlayerPrefs günlüğünden yeniden bekliyor");
            Canvas.ForceUpdateCanvases();
            yield return null;
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            { position = RectTransformUtility.WorldToScreenPoint(null, Node(100).transform.position) };
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && hits[0].gameObject.name == "TapToSkipJourney", "Animasyonda gerçek raycast atlama yüzeyine ulaşıyor: " + string.Join(",", hits.Select(hit => hit.gameObject.name)));
            GameObject.Find("TapToSkipJourney").GetComponent<Button>().onClick.Invoke();
            Check(!Map.JourneyRunning && Node(100).Status == MapLevelStatus.Current && Ledger.MapJourney.lastShownLevel == 100,
                "Ekrana dokunma atlar, hedefe odaklar ve bir kez kaydeder");
            var endCat = GameObject.Find("WizardCatGuide").GetComponent<JourneyCatImage>();
            Check(endCat.Pose == JourneyCatPose.Curled && endCat.transform.position.x < Node(100).transform.position.x,
                "100 açıldığında asasız maskot alt sol kaya üzerinde kıvrılır");
            yield return Shot("03-Crystal-100");
            yield return new WaitForSecondsRealtime(.4f);
            yield return Shot("08-Water-Later");
            var restRect = endCat.rectTransform;
            Vector2 restFeet = restRect.anchoredPosition;
            float initialBreath = restRect.localScale.y;
            yield return Until(() => Mathf.Abs(restRect.localScale.y - initialBreath) > .002f, 1);
            Check(Mathf.Abs(restRect.localScale.y - initialBreath) > .002f && restRect.anchoredPosition == restFeet,
                "Dinlenme nefesi hareket ediyor, pati teması sabit");
            yield return Until(() => endCat.Pose == JourneyCatPose.Blink, 6);
            Check(endCat.Pose == JourneyCatPose.Blink && !Map.JourneyRunning && restRect.anchoredPosition == restFeet,
                "Dinlenen kedi yerinde kısa göz ve kulak hareketi yapar");
            yield return Shot("11-Idle-Blink");
            Reopen(); yield return Reload();
            Check(!Map.JourneyRunning && Node(100).Status == MapLevelStatus.Current && Ledger.MapJourney.lastShownLevel == 100,
                "Gerçek günlükten yeniden giriş gösterilmiş 100 animasyonunu tekrarlamaz");
            Bind(Data(98)); Ledger.Claim(99, 3, true, out _); yield return Reload();
            yield return new WaitForSecondsRealtime(1.1f);
            Check(GameObject.Find("GoldenPawTrail").GetComponent<JourneyPawTrail>().canvasRenderer.GetMesh().vertexCount > 0,
                "99 → 100 alt kıvrımında pati izleri çiziliyor");
            yield return Shot("07-Walk-99-100");
            yield return Until(() => !Map.JourneyRunning);
            Check(Node(100).Status == MapLevelStatus.Current && Ledger.MapJourney.lastShownLevel == 100,
                "99 → 100 yürüyüşü tamamlandığında hedef yalnızca bir kez kaydedilir");
            if (recordMotion)
            {
                Bind(Data(98)); Ledger.Claim(99, 3, true, out _); yield return Reload();
                for (int frame = 0; frame < 25; frame++)
                {
                    yield return Shot("Crystal-" + frame.ToString("D3"), 360);
                    yield return new WaitForSecondsRealtime(.12f);
                }
                Check(!Map.JourneyRunning, "99 → 100 canlı Game View animasyon kareleri alındı");
            }

            Bind(Data(89)); Ledger.Claim(90, 3, true, out _); yield return Reload();
            Check(Map.JourneyTarget == 91, "90 → 91 önceki terastan başlar");
            yield return Until(() => !Map.JourneyRunning);
            var crystalScroll = GameObject.Find("JourneyScroll").GetComponent<ScrollRect>();
            Check(Node(91).Status == MapLevelStatus.Current && !Map.JourneyRunning &&
                Mathf.Abs(crystalScroll.content.anchoredPosition.y / crystalScroll.viewport.rect.height - 4) < .01f,
                "90 → 91 sonunda referans terası tam kadraja gelir");

            foreach (int boundary in Enumerable.Range(1, 12).Select(index => index * 50))
            {
                Bind(Data(boundary - 1)); Ledger.Claim(boundary, 2, true, out _); Reopen(); yield return Reload();
                Check(Map.JourneyTarget == boundary + 1, boundary + " dünya sınırı animasyonu");
                yield return Until(() => !Map.JourneyRunning);
                Check(!Map.JourneyRunning && Node(boundary + 1).Status == MapLevelStatus.Current && Ledger.MapJourney.displayedLevel == boundary + 1,
                    boundary + " sınırında doğru ada, kedi ve açılış");
                var catBounds = new Vector3[4];
                GameObject.Find("WizardCatGuide").GetComponent<RectTransform>().GetWorldCorners(catBounds);
                var targetViewport = GameObject.Find("JourneyScroll").GetComponent<RectTransform>();
                Check(targetViewport.InverseTransformPoint(catBounds[1]).y <= targetViewport.rect.yMax + 1,
                    (boundary + 1) + " ilk bölümünde kedinin şapkası görünür");
                if (boundary == 100 || boundary == 600)
                {
                    Check(GameObject.Find("CrystalBrand") == null && GameObject.Find("StoneRoute") != null &&
                        GameObject.Find("JourneyNearCrystals") != null, "Kristal dışındaki dünya ortak katman ve kontrol düzenini koruyor");
                    yield return Shot("05-Island-" + (boundary + 1));
                }
            }
            Bind(Data(98)); Ledger.Claim(99, 2, true, out _); Ledger.Claim(100, 3, true, out _); Reopen(); yield return Reload();
            Check(Map.JourneyTarget == 101 && Ledger.MapJourney.pendingFrom == 99, "Ardışık Next kazanışları tek kısa yolculukta birleşiyor");
            Map.SkipJourney();
            Bind(Data(98)); Ledger.SetReducedMotion(true); Ledger.Claim(99, 2, true, out _); yield return Reload();
            yield return new WaitForSecondsRealtime(.45f);
            Check(!Map.JourneyRunning && !Ledger.MapJourney.Pending && Ledger.MapJourney.displayedLevel == 100, "Az hareket modu kısa açılışla doğru hedefi kaydediyor");

            Bind(Data(99)); yield return Reload();
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Part12BEditorSize(height); yield return null; yield return null;
                Map.Layout(); yield return Shot("04-Portrait-" + height);
                Check(Map.ActivePointCount <= 60, height + " oranında düğüm havuzu sınırlı");
            }
            Bind(Data(50)); yield return Reload();
            Map.EnterLevel(51);
            float limit = Time.unscaledTime + 40;
            while ((SceneManager.GetActiveScene().name != "Gameplay" || GameObject.Find("Bottle 1") == null) && Time.unscaledTime < limit) yield return null;
            Check(SceneManager.GetActiveScene().name == "Gameplay" && GameplayLevelCatalog.SelectedId == 51, "Gerçek map node → Gameplay 51");
            FindFirstObjectByType<Mewgic.UI.PawGuide>()?.Close();
            foreach (var step in GameplayLevelCatalog.Current().Generated.solution)
            {
                GameObject.Find("Bottle " + (step.source + 1)).GetComponent<Button>().onClick.Invoke();
                GameObject.Find("Bottle " + (step.target + 1)).GetComponent<Button>().onClick.Invoke();
                yield return new WaitForSecondsRealtime(1.1f);
            }
            Check(Ledger.Completed(51) && Ledger.MapJourney.pendingTo == 52 && GameObject.Find("ResultHome") != null, "Gerçek kazanış ödülü ve bekleyen açılış birlikte oluştu");
            GameObject.Find("NextLevel").GetComponent<Button>().onClick.Invoke();
            limit = Time.unscaledTime + 40;
            while ((GameplayLevelCatalog.SelectedId != 52 || GameObject.Find("ResultHome") != null) && Time.unscaledTime < limit) yield return null;
            Check(GameplayLevelCatalog.SelectedId == 52 && Ledger.MapJourney.Pending, "Sonraki bölüm butonu bekleyen yolculuğu silmedi");
            yield return new WaitForSecondsRealtime(.3f);
            var app = FindFirstObjectByType<PrototypeApp>();
            app.PauseGame(); app.ExitPaused();
            yield return null; yield return Ready();
            Check(Map.JourneyTarget == 52, "Gameplay çıkışı → map bekleyen yolculuğu bir kez başlattı");
            Map.SkipJourney();
            Check(Mewgic.Localization.L.Missing.Count == 0, "Yerelleştirme eksiği yok");
        }
        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.logMessageReceived -= Log;
            if (journalKey != null)
            {
                foreach (var suffix in SaveSuffixes) PlayerPrefs.DeleteKey(journalKey + suffix);
                PlayerPrefs.Save();
            }
            GameplayProgress.RestorePlayerProfile(); GameplayLevelCatalog.ResetSelection();
        }
    }
}
#endif
