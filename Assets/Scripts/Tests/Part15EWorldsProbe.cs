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
        [SerializeField] string output = "docs/ProductionReports/Part15E";
        [SerializeField] bool polish;
        static Part15EWorldsProbe active;
        readonly Dictionary<string, string> player = new Dictionary<string, string>();
        readonly List<string> errors = new List<string>();
        readonly List<string> milestones = new List<string>();
        int checks, peakStones;

        public static void Begin(string folder = "docs/ProductionReports/Part15E", bool runPolish = false)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            var probe = new GameObject("Map Worlds isolated QA").AddComponent<Part15EWorldsProbe>();
            probe.output = folder;
            probe.polish = runPolish;
            EditorApplication.isPlaying = true;
        }

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            Directory.CreateDirectory(output);
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
            yield return null;
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { 1600 });
            yield return new WaitForSecondsRealtime(.55f);

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
            File.WriteAllText(Path.Combine(output, "UNITY_TESTS.json"), Newtonsoft.Json.JsonConvert.SerializeObject(
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
                if (polish)
                {
                    Map.FocusRegion(region, false);
                    float artDeadline = Time.unscaledTime + 8;
                    while (Time.unscaledTime < artDeadline && FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None)
                        .Any(art => art.BoundKey != null && !art.Loaded))
                        yield return null;
                    Check(Map.ActiveRegion == region, "Dünya hiyerarşisi odağı " + region);
                    Check(Map.Points.Any(p => p.kind == MapPointKind.LevelGroup && p.regionIndex == region &&
                        p.gameObject.activeInHierarchy), "Dünyada açık ada kapsülü " + region);
                    foreach (var art in FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None))
                        foreach (Transform child in art.transform)
                            if (child.name.EndsWith("_LayeredArt", StringComparison.Ordinal))
                                Check(child.GetComponentsInChildren<Graphic>(true).All(g => !g.raycastTarget),
                                    "Ada dekoru raycast almaz");
                }
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
            if (polish) yield return Polish();
            Check(L.Missing.Count == 0, "Eksik yerelleştirme anahtarı yok");
        }

        IEnumerator Polish()
        {
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { height });
                yield return new WaitForSecondsRealtime(.55f);
                Check(Screen.width == 900 && Screen.height == height, "Polish Game View oranı " + height);
                var safe = Map.SafeArea;
                Vector2 originalMin = safe.anchorMin, originalMax = safe.anchorMax;
                safe.anchorMin = new Vector2(.025f, .045f);
                safe.anchorMax = new Vector2(.975f, .955f);
                Canvas.ForceUpdateCanvases();
                Map.Layout();

                foreach (int level in new[] { 1, 99, 100, 101, 599, 600, 601, 1000 })
                {
                    int region = WorldMapRules.Island(level);
                    int slot = (level - 1) % 50;
                    Map.OpenChapter(region, slot / 10);
                    var scroll = GameObject.Find("JourneyScroll").GetComponent<MapScrollRect>();
                    float row = scroll.content.rect.height / 50;
                    float hidden = scroll.content.rect.height - scroll.viewport.rect.height;
                    scroll.verticalNormalizedPosition = 1 - Mathf.Clamp01(((slot + .5f) * row - scroll.viewport.rect.height * .5f) / hidden);
                    yield return null;
                    yield return null;
                    var stone = LevelStone(slot);
                    var label = stone.transform.Find("LevelNumber").GetComponent<TMPro.TMP_Text>();
                    label.ForceMeshUpdate();
                    var rect = (RectTransform)stone.transform;
                    var screen = RectTransformUtility.WorldToScreenPoint(null, stone.transform.position);
                    Check(label.text == level.ToString() && !label.isTextOverflowing &&
                        label.textBounds.size.x <= label.rectTransform.rect.width + 1, "Taşma yok " + level + " / " + height);
                    Check(Mathf.Min(rect.rect.width, rect.rect.height) * Map.Canvas.scaleFactor >= 47.9f &&
                        RectTransformUtility.RectangleContainsScreenPoint(safe, screen), "Güvenli dokunma alanı " + level + " / " + height);
                    Check(Raycast(screen) == stone.gameObject, "Gerçek taş raycast " + level + " / " + height);
                    if (level == 1000)
                    {
                        Check(stone.GetComponent<MapNodeBadge>().Status == MapLevelStatus.Locked, "1000 kilitli ve okunaklı");
                        yield return Shot("Portre_" + height, height);
                    }
                }
                safe.anchorMin = originalMin;
                safe.anchorMax = originalMax;
                Canvas.ForceUpdateCanvases();
                Map.Layout();
                GameObject.Find("ChapterClose").GetComponent<Button>().onClick.Invoke();
            }

            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { 1600 });
            Profile(99);
            SceneManager.LoadScene("LevelMap");
            yield return null;
            yield return Ready();
            yield return new WaitForSecondsRealtime(.55f);
            yield return Shot("Dunya");
            Profile(999);
            SceneManager.LoadScene("LevelMap");
            yield return null;
            yield return Ready();

            GameObject.Find("FocusCurrent").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Check(Map.ActiveRegion == 19 && LevelStone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current,
                "Dünyadan Bulunduğum Bölüm gerçek 1000");
            var journey = GameObject.Find("JourneyScroll").GetComponent<MapScrollRect>();
            var stone1000 = LevelStone(49);
            var pointer = new PointerEventData(EventSystem.current)
            {
                pointerId = 7,
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, stone1000.transform.position)
            };

            float before = journey.content.anchoredPosition.y;
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            journey.OnInitializePotentialDrag(pointer);
            journey.OnBeginDrag(pointer);
            pointer.position -= Vector2.up * 170;
            journey.OnDrag(pointer);
            journey.OnEndDrag(pointer);
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Check(journey.content.anchoredPosition.y < before && !Map.InformationVisible, "Sürükleyip bırakma taş seçmedi");
            yield return null;
            GameObject.Find("JourneyFocus").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.1f);
            Check(LevelStone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current, "Ada içi yumuşak güncel odak");
            var viewport = journey.viewport;
            var decoration = RectTransformUtility.WorldToScreenPoint(null,
                viewport.TransformPoint(new Vector3(viewport.rect.xMin + 6, 0, 0)));
            var decorationHit = Raycast(decoration);
            Check(decorationHit != null && ExecuteEvents.GetEventHandler<IPointerClickHandler>(decorationHit) == null,
                "Dekor ve geçiş yüzeyi tıklanamaz");

            stone1000 = LevelStone(49);
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, stone1000.transform.position);
            Check(Raycast(pointer.position) == stone1000.gameObject, "1000 temiz dokunma raycast");
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(stone1000.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            float timeout = Time.unscaledTime + 40;
            while ((SceneManager.GetActiveScene().name != "Gameplay" || GameObject.Find("Bottle 1") == null) && Time.unscaledTime < timeout)
                yield return null;
            Check(SceneManager.GetActiveScene().name == "Gameplay" && GameplayLevelCatalog.SelectedId == 1000,
                "Temiz dokunma gerçek Gameplay 1000 açtı");
            FindFirstObjectByType<Mewgic.UI.PawGuide>()?.Close();
            yield return Shot("Oynanis_1000");
            SceneManager.LoadScene("Home");
            yield return null;
            timeout = Time.unscaledTime + 20;
            while (GameObject.Find("Play") == null && Time.unscaledTime < timeout) yield return null;
            Check(GameObject.Find("Play") != null, "Ana ekran hazır");
            yield return new WaitForSecondsRealtime(.5f);
            var gift = GameObject.Find("ArrivalOpen");
            if (gift != null)
            {
                gift.GetComponent<Button>().onClick.Invoke();
                yield return new WaitForSecondsRealtime(1.4f);
                GameObject.Find("ArrivalContinue")?.GetComponent<Button>().onClick.Invoke();
            }
            yield return new WaitForSecondsRealtime(.5f);
            yield return Shot("Ana_Menu");
        }

        MapPoint LevelStone(int slot) => Map.Points.Single(p =>
            p.kind == MapPointKind.LevelSlot && p.slotIndex == slot && p.gameObject.activeInHierarchy);

        static GameObject Raycast(Vector2 position)
        {
            var data = new PointerEventData(EventSystem.current) { position = position };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            return hits.Count > 0 ? hits[0].gameObject : null;
        }

        IEnumerator Shot(string name, int height = 1600)
        {
            yield return new WaitForEndOfFrame();
            Check(Screen.width == 900 && Screen.height == height, "Gerçek portre Game View " + name + " / " + Screen.width + "x" + Screen.height);
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
