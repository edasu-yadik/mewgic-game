#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part15EWorldsProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Part15E";
        static Part15EWorldsProbe active;
        readonly Dictionary<string, string> player = new Dictionary<string, string>();
        readonly List<string> errors = new List<string>();
        readonly List<string> milestones = new List<string>();
        int checks, peakStones;

        public static void Begin()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            new GameObject("PART15E isolated QA").AddComponent<Part15EWorldsProbe>();
            EditorApplication.isPlaying = true;
        }

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            Directory.CreateDirectory(Output);
            foreach (string suffix in new[] { "", ".backup", ".corrupt" })
            {
                string key = GameplayProgress.StorageKey + suffix;
                player[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
            }

            Profile(599);
            Application.logMessageReceived += Log;
        }

        void Profile(int completed)
        {
            var data = Part12BProbe.Data(completed);
            data.presentation.manualLocale = "tr";
            data.worldMap.lastCelebratedIsland = WorldMapRules.Island(completed + 1);
            GameplayProgress.UseTestProfile(data);
        }

        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || type == LogType.Warning)
                errors.Add(message);
        }

        void Check(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label);
            checks++;
        }

        IEnumerator Start()
        {
            var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { 1600 });
            view.Show();
            view.maximized = true;
            view.Focus();

            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(Run());
            while (stack.Count > 0)
            {
                object next = null;
                bool more = false;
                try { more = stack.Peek().MoveNext(); if (more) next = stack.Peek().Current; }
                catch (Exception error) { failure = error.ToString(); break; }
                if (!more) { stack.Pop(); continue; }
                if (next is IEnumerator iterator && !(next is CustomYieldInstruction)) { stack.Push(iterator); continue; }
                yield return next;
            }
            bool intact = player.All(p => p.Value == null ? !PlayerPrefs.HasKey(p.Key) : PlayerPrefs.GetString(p.Key) == p.Value);
            File.WriteAllText(Path.Combine(Output, "UNITY_TESTS.json"), Newtonsoft.Json.JsonConvert.SerializeObject(
                new { success = failure == null && errors.Count == 0 && intact, checks, peakStones, failure, playerSaveIntact = intact, errors, milestones },
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        MapWorld Map => FindFirstObjectByType<MapWorld>();

        IEnumerator Ready()
        {
            float timeout = Time.unscaledTime + 25;
            while (Time.unscaledTime < timeout && (Map == null || !Map.Mounted || !L.Ready))
                yield return null;
            Check(Map != null && Map.Mounted && L.Ready, "Gerçek LevelMap hazır");
        }

        IEnumerator Run()
        {
            yield return Ready();
            var names = new HashSet<string>();
            for (int region = 0; region < 12; region++)
            {
                Map.OpenChapter(region, 2);
                var scroll = GameObject.Find("JourneyScroll").GetComponent<ScrollRect>();
                var world = Map.Progression.Region(region);
                Check(world.firstLevel == region * 50 + 1 && world.levelCount == 50, "Ada kimliği " + region);
                if (region == 1)
                    Check(GameObject.Find("CrystalFallsTerraces").GetComponentsInChildren<RawImage>().Length == 5, "Kristal çizimleri korundu");
                else
                {
                    var textures = GameObject.Find("ThemedJourneyTerraces").GetComponentsInChildren<ThemedTerraceImage>();
                    Check(textures.Length == 4 && textures.All(i => i.texture == Map.LoadedJourneyAtlas), "Tek atlas, dört çizim " + region);
                    Check(names.Add(Map.LoadedJourneyAtlas.name) && textures.Select(i => i.uvRect).Distinct().Count() == 4,
                        "Benzersiz ada ve dört farklı panel " + region);
                }
                Check(GameObject.Find("StoneRoute") == null, "Yol resim üzerinde " + region);
                var seen = new HashSet<int>();
                for (int step = 0; step <= 20; step++)
                {
                    scroll.verticalNormalizedPosition = 1 - step / 20f;
                    yield return null;
                    yield return null;
                    var stones = Map.Points.Where(p => p.kind == MapPointKind.LevelSlot && p.gameObject.activeInHierarchy).ToArray();
                    peakStones = Mathf.Max(peakStones, stones.Length);
                    Check(stones.Length < 20, "Düğüm havuzu sınırlı " + region);
                    foreach (var point in stones)
                    {
                        var badge = point.GetComponent<MapNodeBadge>();
                        int id = Map.Progression.LevelId(region, point.slotIndex);
                        seen.Add(id);
                        Check(point.regionIndex == region && badge.Status == Map.Progression.Status(id) && badge.Stars == Map.Progression.Stars(id),
                            "Gerçek bölüm durumu " + id);
                    }
                }
                Check(seen.Count == 50, "50 bölüm kaydırılabilir " + region + " / " + string.Join(",", Enumerable.Range(world.firstLevel, 50).Where(id => !seen.Contains(id))));
                scroll.verticalNormalizedPosition = .51f;
                yield return null;
                var stone = Map.Points.Where(p => p.kind == MapPointKind.LevelSlot && p.gameObject.activeInHierarchy)
                    .OrderBy(p => Mathf.Abs(p.slotIndex - 24)).First();
                var data = new PointerEventData(EventSystem.current)
                    { position = RectTransformUtility.WorldToScreenPoint(null, stone.transform.position) };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, hits);
                Check(hits.Count > 0 && hits[0].gameObject == stone.gameObject, "Gerçek raycast " + region);
                yield return Shot("Ada_" + (region + 1).ToString("D2"));
                GameObject.Find("ChapterClose").GetComponent<Button>().onClick.Invoke();
                Check(Map.LoadedJourneyAtlas == null, "Ada atlası bırakıldı " + region);
            }

            foreach (int height in new[] { 1950, 2000 })
            {
                Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { height });
                yield return new WaitForSecondsRealtime(.55f);
                Map.OpenChapter(4, 2);
                yield return null;
                Check(Screen.width == 900 && Screen.height == height, "Uzun portre " + height);
                foreach (var point in Map.Points.Where(p => p.kind == MapPointKind.LevelSlot && p.gameObject.activeInHierarchy))
                {
                    var rect = (RectTransform)point.transform;
                    Check(Mathf.Min(rect.rect.width, rect.rect.height) * Map.Canvas.scaleFactor >= 47.9f,
                        "Portrede 48 piksel hedef");
                    var label = point.transform.Find("LevelNumber").GetComponent<TMPro.TMP_Text>();
                    label.ForceMeshUpdate();
                    Check(!label.isTextOverflowing, "Uzun portrede numara taşmıyor");
                }
                GameObject.Find("ChapterClose").GetComponent<Button>().onClick.Invoke();
            }
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { 1600 });
            yield return new WaitForSecondsRealtime(.55f);

            foreach (int boundary in Enumerable.Range(1, 12).Select(index => index * 50))
            {
                Profile(boundary - 1);
                Check(GameplayProgress.Current.Claim(boundary, 3, true, out bool first) && first, "Kazanış " + boundary);
                if (boundary == 50) GameplayProgress.Current.ClearMapJourney();
                SceneManager.LoadScene("LevelMap");
                yield return null;
                yield return Ready();
                Check(Map.JourneyTarget == boundary + 1, "Görsel geçiş başladı " + boundary);
                float timeout = Time.unscaledTime + 5;
                while (Map.JourneyRunning && GameObject.Find("JourneyIslandWelcome") == null && Time.unscaledTime < timeout)
                    yield return null;
                Check(GameObject.Find("JourneyIslandWelcome") != null, "Yeni adanın adı görünür " + boundary);
                if (boundary == 100) { yield return new WaitForSecondsRealtime(.38f); yield return Shot("Ada_Acilisi_101"); }
                while (Map.JourneyRunning && Time.unscaledTime < timeout)
                    yield return null;
                Check(Map.Progression.CurrentId == boundary + 1 && !Map.JourneyRunning && GameObject.Find("JourneyIslandWelcome") == null,
                    "Geçiş tamamlandı " + boundary);
                Check(GameplayProgress.Current.WorldMap.lastCelebratedIsland == boundary / 50, "Kutlama bir kez kayıtlı " + boundary);
                SceneManager.LoadScene("LevelMap");
                yield return null;
                yield return Ready();
                Check(!Map.JourneyRunning && GameObject.Find("JourneyIslandWelcome") == null, "Aynı geçiş yeniden oynamadı " + boundary);
                milestones.Add(boundary + "→" + (boundary + 1));
                if (boundary == 600)
                {
                    var preparation = GameplayLevelCatalog.PrepareAsync(601);
                    float generatorTimeout = Time.unscaledTime + 35;
                    while (!preparation.IsCompleted && Time.unscaledTime < generatorTimeout)
                        yield return null;
                    Check(preparation.Status == System.Threading.Tasks.TaskStatus.RanToCompletion &&
                        preparation.Result.Generated.level == 601, "601 gerçek prosedürel bölüm üretildi");
                }
            }
            Check(L.Missing.Count == 0, "Eksik yerelleştirme anahtarı yok");
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Check(Screen.width == 900 && Screen.height == 1600, "Gerçek portre Game View");
            var render = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            var flipped = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            var before = RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(render);
                Graphics.Blit(render, flipped, new Vector2(1, -1), new Vector2(0, 1));
                RenderTexture.active = flipped;
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(Output, name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = before;
                RenderTexture.ReleaseTemporary(render);
                RenderTexture.ReleaseTemporary(flipped);
                Destroy(texture);
            }
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
        }
    }
}
#endif
