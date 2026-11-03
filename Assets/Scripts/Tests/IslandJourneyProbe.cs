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
    public sealed class IslandJourneyProbe : MonoBehaviour
    {
        [SerializeField] string output = "docs/ProductionReports/IslandJourney/";
        readonly Dictionary<string, string> player = new Dictionary<string, string>();
        readonly List<string> errors = new List<string>();
        int checks, peakStones;
        MapWorld map;

        public static void Begin(string reportDirectory = "docs/ProductionReports/IslandJourney/")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            new GameObject("Island Journey QA").AddComponent<IslandJourneyProbe>().output = reportDirectory;
            EditorApplication.isPlaying = true;
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            foreach (string suffix in new[] { "", ".backup", ".corrupt" })
            {
                string key = GameplayProgress.StorageKey + suffix;
                player[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
            }

            string fixture = "Mewgic.Tests.IslandJourney." + Guid.NewGuid().ToString("N");
            var data = Part12BProbe.Data(99);
            data.presentation.manualLocale = "tr";
            GameplayProgress.UseTestProfile();
            PlayerPrefs.SetString(fixture, JsonUtility.ToJson(data));
            typeof(GameplayProgress).GetField("current", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, GameplayProgress.Open(fixture));
            GameplayLevelCatalog.TestArchive = new FileLevelArchive(Path.Combine(Path.GetTempPath(), fixture));
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived += Log;
            Directory.CreateDirectory(output);
        }

        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception || type == LogType.Warning)
                errors.Add(type + " " + message);
        }

        void Check(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException(description);
            checks++;
        }

        IEnumerator Start()
        {
            var run = Run();
            string failure = null;
            while (true)
            {
                bool more;
                try { more = run.MoveNext(); }
                catch (Exception exception) { failure = exception.ToString(); break; }
                if (!more)
                    break;
                yield return run.Current;
            }
            bool intact = player.All(entry => entry.Value == null ? !PlayerPrefs.HasKey(entry.Key) :
                PlayerPrefs.GetString(entry.Key) == entry.Value);
            File.WriteAllText(Path.Combine(output, "UNITY_TESTS.json"), Newtonsoft.Json.JsonConvert.SerializeObject(
                new { success = failure == null && errors.Count == 0 && intact, checks, peakStones, failure, playerSaveIntact = intact, errors },
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        static void GameView(int height)
        {
            Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null, new object[] { height });
            var window = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            window.Show();
            window.maximized = true;
            window.Focus();
        }

        MapPoint[] Stones() => map.Points.Where(p => p.kind == MapPointKind.LevelSlot && p.gameObject.activeInHierarchy).ToArray();
        MapPoint Stone(int slot) => Stones().Single(p => p.slotIndex == slot);
        static void Tap(MapPoint point)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(point.gameObject, data, ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Run()
        {
            GameView(1600);
            float limit = Time.unscaledTime + 25;
            while (Time.unscaledTime < limit && (!L.Ready || (map = FindFirstObjectByType<MapWorld>()) == null || !map.Mounted))
                yield return null;
            Check(map != null && map.Mounted && L.Ready, "LevelMap yüklenmeli");
            map.FocusRegion(1, false);
            var island = map.Points.Single(p => p.kind == MapPointKind.LevelGroup && p.regionIndex == 1 && p.gameObject.activeInHierarchy);
            Tap(island);
            yield return null;
            var scroll = GameObject.Find("JourneyScroll").GetComponent<ScrollRect>();
            var terraces = GameObject.Find("CrystalFallsTerraces").GetComponentsInChildren<RawImage>();
            Check(terraces.Length == 5 && terraces.Select(a => a.texture).Distinct().Count() == 5, "Beş ayrı Kristal Şelaleler zemini");
            Check(GameObject.Find("StoneRoute") == null && GameObject.Find("WizardCatContactShadow") != null, "Boyanmış rota ve kedi temas gölgesi");
            Check(Stone(49).transform.Find("StoneLight").gameObject.activeInHierarchy, "100. bölüm kilometre taşı ışığı");
            var water = terraces[4].material;
            float waterBefore = water.GetFloat("_EnvironmentTime");
            var marker = Stone(49).transform.Find("CurrentMarker");
            float markerBefore = marker.localPosition.y;
            yield return new WaitForSecondsRealtime(.26f);
            Check(water.IsKeywordEnabled("MEWGIC_CRYSTAL_FLOW") && water.GetFloat("_EnvironmentTime") > waterBefore, "Şelale materyali akıyor");
            Check(Mathf.Abs(marker.localPosition.y - markerBefore) > .01f, "Aktif bölüm işareti hareket ediyor");
            Check(Stone(49).GetComponent<MapNodeBadge>().Status == MapLevelStatus.Current, "100 güncel olmalı");
            Check(Stone(48).GetComponent<MapNodeBadge>().Stars == map.Progression.Stars(99), "99 yıldız kaydı");
            var seen = new HashSet<int>();
            for (int region = 0; region <= 12; region++)
            {
                map.OpenChapter(region, 0);
                seen.Clear();
                for (int sample = 0; sample <= 25; sample++)
                {
                    scroll.verticalNormalizedPosition = 1 - sample / 25f;
                    yield return null;
                    var stones = Stones();
                    peakStones = Mathf.Max(peakStones, stones.Length);
                    Check(stones.Length < 20, "Görünür düğüm havuzu sınırlı");
                    foreach (var point in stones)
                    {
                        seen.Add(point.slotIndex);
                        var id = map.Progression.LevelId(region, point.slotIndex);
                        var text = point.transform.Find("LevelNumber").GetComponent<TMP_Text>();
                        text.ForceMeshUpdate();
                        Check(point.regionIndex == region && text.text == id.ToString() && !text.isTextOverflowing, "Kimlik ve taşma " + id);
                        Check(point.GetComponent<MapNodeBadge>().Status == map.Progression.Status(id), "Durum " + id);
                    }
                }
                Check(seen.Count == 50, "Bütün bölüm taşlarına erişim " + region);
            }

            map.OpenChapter(1, 4);
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                GameView(height);
                yield return new WaitForSecondsRealtime(.65f);
                Check(Screen.width == 900 && Screen.height == height, "Gerçek Game View boyutu " + height + " / " + Screen.width + "x" + Screen.height);
                AssertHit(Stone(49));
                if (height == 1600)
                    RenderPortrait("GameView", 900, height, false);
            }
            RenderPortrait("SafeArea_900x2000", 900, 2000, true);
            RenderPortrait("Compact_360x640", 360, 640, false);

            var pointer = new PointerEventData(EventSystem.current) { scrollDelta = Vector2.up * 7 };
            float before = scroll.content.anchoredPosition.y;
            ExecuteEvents.Execute(scroll.gameObject, pointer, ExecuteEvents.scrollHandler);
            yield return null;
            Check(scroll.content.anchoredPosition.y < before, "Kaydırma girdisi rotayı hareket ettirmeli");

            var drag = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(Screen.width * .5f, Screen.height * .55f)
            };
            before = scroll.content.anchoredPosition.y;
            ExecuteEvents.Execute(scroll.gameObject, drag, ExecuteEvents.initializePotentialDrag);
            ExecuteEvents.Execute(scroll.gameObject, drag, ExecuteEvents.beginDragHandler);
            drag.position -= Vector2.up * 180;
            ExecuteEvents.Execute(scroll.gameObject, drag, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(scroll.gameObject, drag, ExecuteEvents.endDragHandler);
            yield return null;
            Check(scroll.content.anchoredPosition.y < before, "Dikey sürükleme rotayı hareket ettirmeli");

            map.OpenChapter(2, 0);
            yield return null;
            var locked = Stone(0);
            AssertHit(locked);
            Tap(locked);
            Check(map.InformationVisible && SceneManager.GetActiveScene().name == "LevelMap", "Kilitli 101 oyun açmamalı");
            GameObject.Find("MapInfoClose").GetComponent<Button>().onClick.Invoke();

            map.OpenChapter(1, 4);
            yield return null;
            var current = Stone(49);
            var move = new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Up };
            current.GetComponent<Button>().Select();
            ExecuteEvents.Execute(current.gameObject, move, ExecuteEvents.moveHandler);
            Check(EventSystem.current.currentSelectedGameObject == Stone(48).gameObject, "Klavye önceki bölüm odağı");
            AssertHit(current);
            Tap(current);
            limit = Time.unscaledTime + 35;
            while (Time.unscaledTime < limit && SceneManager.GetActiveScene().name != "Gameplay")
                yield return null;
            Check(SceneManager.GetActiveScene().name == "Gameplay" && GameplayLevelCatalog.SelectedId == 100, "100 gerçek Gameplay geçişi");
            yield return null;
            yield return null;
            FindFirstObjectByType<PawGuide>()?.Close();
            foreach (var moveStep in GameplayLevelCatalog.Current().Generated.solution)
            {
                GameObject.Find("Bottle " + (moveStep.source + 1)).GetComponent<Button>().onClick.Invoke();
                GameObject.Find("Bottle " + (moveStep.target + 1)).GetComponent<Button>().onClick.Invoke();
                yield return new WaitForSecondsRealtime(1.05f);
            }
            Check(GameObject.Find("ResultHome") != null && GameObject.Find("Home") == null, "Sonuçta tek Harita düğmesi");
            Check(GameObject.Find("VictoryBackdrop") != null, "Sonuç arka planı karartılır ve tıklamalar kesilir");
            GameObject.Find("Replay").GetComponent<Button>().onClick.Invoke();
            Check(GameObject.Find("Home") != null && GameObject.Find("ResultHome") == null, "Tekrar oynatmada oyun kontrolleri geri gelir");
        }

        void RenderPortrait(string name, int width, int height, bool inset)
        {
            var canvas = map.Canvas;
            var camera = Camera.main;
            var safe = map.SafeArea;
            Vector2 min = safe.anchorMin, max = safe.anchorMax;
            var originalMode = canvas.renderMode;
            var originalCamera = canvas.worldCamera;
            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;
            float originalDistance = canvas.planeDistance, originalScale = canvas.scaleFactor;
            var target = new RenderTexture(width, height, 24);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                // Mirror the map CanvasScaler's width/height match for this offscreen target.
                var scaler = canvas.GetComponent<CanvasScaler>();
                var reference = scaler.referenceResolution;
                canvas.scaleFactor = Mathf.Pow(width / reference.x, 1 - scaler.matchWidthOrHeight) *
                    Mathf.Pow(height / reference.y, scaler.matchWidthOrHeight);
                if (inset)
                {
                    safe.anchorMin = new Vector2(.035f, .045f);
                    safe.anchorMax = new Vector2(.965f, .94f);
                }
                Canvas.ForceUpdateCanvases();
                map.Layout();
                Canvas.ForceUpdateCanvases();
                var viewport = GameObject.Find("JourneyScroll").GetComponent<ScrollRect>().viewport;
                Check(viewport.rect.width <= safe.rect.width && viewport.rect.height < safe.rect.height, "Rota Safe Area içinde " + name);
                var current = (RectTransform)Stone(49).transform;
                var local = viewport.InverseTransformPoint(current.position);
                Check(viewport.rect.Contains((Vector2)local), "Güncel taş yeniden boyutlandırma sonrası görünür " + name);
                foreach (var point in Stones())
                {
                    var rect = (RectTransform)point.transform;
                    Check(Mathf.Min(rect.rect.width, rect.rect.height) * canvas.scaleFactor >= 47.9f, "Kompakt dokunma alanı " + name);
                    var label = point.transform.Find("LevelNumber").GetComponent<TMP_Text>();
                    label.ForceMeshUpdate();
                    Check(!label.isTextOverflowing, "Portre bölüm numarası " + name);
                }
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = originalTarget;
                canvas.renderMode = originalMode;
                canvas.worldCamera = originalCamera;
                canvas.planeDistance = originalDistance;
                canvas.scaleFactor = originalScale;
                safe.anchorMin = min;
                safe.anchorMax = max;
                RenderTexture.active = originalActive;
                DestroyImmediate(image);
                target.Release();
                DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
                map.Layout();
            }
        }

        void AssertHit(MapPoint point)
        {
            var rect = (RectTransform)point.transform;
            Check(Mathf.Min(rect.rect.width, rect.rect.height) * map.Canvas.scaleFactor >= 47.9f, "Dokunma alanı en az 48 piksel");
            var data = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.position) };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            Check(hits.Count > 0 && hits[0].gameObject == point.gameObject, "Gerçek raycast " + point.slotIndex);
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            GameplayLevelCatalog.TestArchive = null;
        }
    }
}
#endif
