#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Map;
using Mewgic.Localization;
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
    public sealed class Part15DJourneyProbe : MonoBehaviour
    {
        [SerializeField] string output = "docs/ProductionReports/Part15D";
        readonly List<string> checks = new List<string>();
        readonly List<string> issues = new List<string>();
        readonly Dictionary<string, string> player = new Dictionary<string, string>();
        static Part15DJourneyProbe active;
        string profile;
        MapWorld Map => FindFirstObjectByType<MapWorld>();
        RectTransform Cat => (RectTransform)GameObject.Find("WizardCatGuide").transform;
        MapScrollRect Scroll => GameObject.Find("JourneyScroll").GetComponent<MapScrollRect>();

        public static void Begin(string folder = "docs/ProductionReports/Part15D")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            new GameObject("PART15D isolated QA").AddComponent<Part15DJourneyProbe>().output = folder;
            EditorApplication.isPlaying = true;
        }

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            Directory.CreateDirectory(output);
            Application.runInBackground = true;
            foreach (string suffix in new[] { "", ".backup", ".corrupt" })
            {
                string key = GameplayProgress.StorageKey + suffix;
                player[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
            }
            profile = "Mewgic.Tests.Part15D." + Guid.NewGuid().ToString("N");
            GameplayLevelCatalog.TestArchive = new FileLevelArchive(Path.Combine(Path.GetTempPath(), profile));
            Bind(90);
            Application.logMessageReceived += Log;
        }

        void Bind(int completed = -1)
        {
            if (completed >= 0)
            {
                var data = Part12BProbe.Data(completed);
                data.presentation.manualLocale = "tr";
                PlayerPrefs.SetString(profile, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
            }
            GameplayProgress.UseTestProfile();
            typeof(GameplayProgress).GetField("current", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, GameplayProgress.Open(profile));
            GameplayLevelCatalog.ResetSelection();
        }

        void Log(string message, string stack, LogType kind)
        {
            if (kind == LogType.Error || kind == LogType.Warning || kind == LogType.Exception || kind == LogType.Assert)
                issues.Add(message);
        }

        void Check(bool okay, string message)
        {
            if (!okay) throw new InvalidOperationException(message);
            checks.Add(message);
        }

        IEnumerator Start()
        {
            var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { 1600 });
            view.Show();
            view.maximized = true;
            view.Focus();

            string failure = null;
            var work = new Stack<IEnumerator>();
            work.Push(Run());
            while (work.Count > 0)
            {
                object next = null;
                bool more = false;
                try { more = work.Peek().MoveNext(); if (more) next = work.Peek().Current; }
                catch (Exception error) { failure = error.ToString(); break; }
                if (!more) { work.Pop(); continue; }
                if (next is IEnumerator child && !(next is CustomYieldInstruction)) { work.Push(child); continue; }
                yield return next;
            }
            bool intact = player.All(p => p.Value == null ? !PlayerPrefs.HasKey(p.Key) : PlayerPrefs.GetString(p.Key) == p.Value);
            File.WriteAllText(Path.Combine(output, "UNITY_TESTS.json"), Newtonsoft.Json.JsonConvert.SerializeObject(
                new { success = failure == null && issues.Count == 0 && intact, failure, checks, issues, playerSaveIntact = intact },
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        IEnumerator Ready(string scene, Func<bool> extra)
        {
            float timeout = Time.unscaledTime + 40;
            while (Time.unscaledTime < timeout &&
                (SceneManager.GetActiveScene().name != scene || !extra()))
                yield return null;
            Check(SceneManager.GetActiveScene().name == scene && extra(), scene + " hazır");
        }

        IEnumerator Load(int completed)
        {
            Bind(completed);
            SceneManager.LoadScene("LevelMap");
            yield return null;
            yield return Ready("LevelMap", () => Map != null && Map.Mounted && L.Ready);
        }

        IEnumerator Win(int id)
        {
            Map.EnterLevel(id);
            yield return Ready("Gameplay", () => GameplayLevelCatalog.SelectedId == id && GameObject.Find("Bottle 1") != null);
            FindFirstObjectByType<PawGuide>()?.Close();
            foreach (var step in GameplayLevelCatalog.Current().Generated.solution)
            {
                Tap("Bottle " + (step.source + 1));
                Tap("Bottle " + (step.target + 1));
                yield return new WaitForSecondsRealtime(1.05f);
            }
            Check(GameplayProgress.Current.Completed(id) && GameObject.Find("ResultHome") != null, id + " gerçek kazanış ve kayıt");
            var ledger = GameplayProgress.Current;
            var pending = (int)typeof(ProgressLedger).GetField("journeyTo", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ledger);
            checks.Add("Kazanış sonrası bekleyen hedef " + pending);
            Tap("ResultHome");
            yield return Ready("LevelMap", () => Map != null && Map.Mounted && L.Ready);
        }

        static void Tap(string name) => GameObject.Find(name).GetComponent<Button>().onClick.Invoke();

        MapPoint Stone(int slot) => Map.Points.Single(p =>
            p.kind == MapPointKind.LevelSlot && p.slotIndex == slot && p.gameObject.activeInHierarchy);

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var render = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            var flipped = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            var before = RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(render);
                Graphics.Blit(render, flipped, new Vector2(1, -1), new Vector2(0, 1));
                RenderTexture.active = flipped;
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = before;
                RenderTexture.ReleaseTemporary(render);
                RenderTexture.ReleaseTemporary(flipped);
                Destroy(texture);
            }
        }

        IEnumerator Run()
        {
            yield return Ready("LevelMap", () => Map != null && Map.Mounted && L.Ready);
            Map.OpenChapter(1, 4);
            var toggle = GameObject.Find("JourneyMotion").GetComponent<Button>();
            if (toggle.GetComponentInChildren<TMPro.TMP_Text>().text.Contains("Az"))
                toggle.onClick.Invoke();
            Check(!Map.JourneyRunning && Cat.gameObject.activeInHierarchy, "Doğrudan harita, kedi mevcut taşın yanında");
            yield return Win(91);
            Check(Map.JourneyTarget == 92 && Stone(41).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Locked,
                "91→92 yolculuğu ve geçici kilit (" + Map.JourneyTarget + "/" + Map.Progression.CurrentId + "/" + (GameObject.Find("JourneyScroll") != null) + ")");
            Vector2 catStart = Cat.anchoredPosition;
            float cameraStart = Scroll.content.anchoredPosition.y;
            yield return new WaitForSecondsRealtime(.55f);
            Check(Vector2.Distance(catStart, Cat.anchoredPosition) > 8 && Scroll.content.anchoredPosition.y > cameraStart,
                "Kedi hareket ediyor ve kamera yumuşak takip ediyor");
            yield return Shot("Walk_91_92");
            yield return new WaitForSecondsRealtime(1.5f);
            Check(!Map.JourneyRunning && Stone(41).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current, "92 açılışı tamamlandı");
            var data = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, Stone(41).transform.position) };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            Check(hits.Count > 0 && hits[0].gameObject == Stone(41).gameObject, "92 gerçek dokunma hedefi");

            yield return Win(91);
            Check(!Map.JourneyRunning && Map.Progression.CurrentId == 92, "91 tekrarında yeni yolculuk yok");

            yield return Load(98);
            yield return Win(99);
            Check(Map.JourneyTarget == 100 && Stone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Locked, "99→100 geçici kilit");
            while (Map.JourneyRunning && Stone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Locked) yield return null;
            yield return new WaitForSecondsRealtime(.17f);
            Check(Map.JourneyRunning && Stone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current, "100 hareket bitmeden görünür olarak açılır");
            yield return Shot("Unlock_100");
            Scroll.OnScroll(new PointerEventData(EventSystem.current) { scrollDelta = Vector2.up * 5 });
            Check(!Map.JourneyRunning && Map.Progression.CanEnter(100), "Manuel kaydırma kamerayı ve sunumu bırakır");

            yield return Win(100);
            Check(Map.JourneyTarget == 101, "100→101 gerçek ada geçişi başlar");
            while (Map.JourneyRunning && !Map.Points.Any(p => p.kind == MapPointKind.LevelSlot && p.regionIndex == 2 && p.gameObject.activeInHierarchy)) yield return null;
            yield return new WaitForSecondsRealtime(.22f);
            Check(Stone(0).regionIndex == 2 && Map.JourneyRunning, "Kısa ada geçişi hedef adayı bağlar");
            yield return Shot("Island_101");

            long coins = GameplayProgress.Current.Presentation.coins;
            Bind();
            SceneManager.LoadScene("LevelMap");
            yield return null;
            yield return Ready("LevelMap", () => Map != null && Map.Mounted && L.Ready);
            Check(!Map.JourneyRunning && Map.Progression.CurrentId == 101 && Map.ActiveRegion == 2 && GameplayProgress.Current.Presentation.coins == coins,
                "Animasyon sırasında sahne kapanışı ve disk profili yeniden açma ilerlemeyi/ödülü korur");

            foreach (int boundary in new[] { 50, 600 })
            {
                yield return Load(boundary - 1);
                Check(GameplayProgress.Current.Claim(boundary, 2, true, out bool first) && first, boundary + " sınır kaydı");
                SceneManager.LoadScene("LevelMap");
            yield return null;
                yield return Ready("LevelMap", () => Map != null && Map.Mounted && L.Ready);
                Check(Map.JourneyTarget == boundary + 1, boundary + " sınır sunumu");
                yield return new WaitForSecondsRealtime(2);
                Check(Map.Progression.CurrentId == boundary + 1 && Map.PackStart == (boundary == 600 ? 12 : 0) &&
                    Stone(0).regionIndex == boundary / 50, boundary + " hedef ada ve paket");
            }

            yield return Load(90);
            Map.OpenChapter(1, 4);
            var motion = GameObject.Find("JourneyMotion").GetComponentInChildren<TMPro.TMP_Text>();
            Check(!string.IsNullOrEmpty(motion.text), "Hareket tercihi yerelleştirilmiş ve görünür");
            if (!motion.text.Contains("Az")) Tap("JourneyMotion");
            Check(GameplayProgress.Current.Claim(91, 2, true, out bool granted) && granted, "Az hareket için ilk kazanış");
            typeof(MapWorld).GetMethod("TryStartJourney", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Map, null);
            Check(Map.JourneyTarget == 92 && Stone(41).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current &&
                GameObject.Find("JourneyUnlockLights") == null, "Az hareket doğrudan hedefi gösterir");
            yield return new WaitForSecondsRealtime(.42f);
            Check(!Map.JourneyRunning && L.Missing.Count == 0, "Az hareket kısa sürede tamamlanır ve yerelleştirme eksiksiz");
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            GameplayLevelCatalog.TestArchive = null;
        }
    }
}
#endif
