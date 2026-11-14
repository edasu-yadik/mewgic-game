using System;
using System.Collections.Generic;
using System.Linq;
using Mewgic.Core;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Map
{
    public sealed partial class MapWorld : MonoBehaviour
    {
        public Sprite node, chest;
        // Kept as source art references for existing editor probes. The route uses the theme assets.
        public Sprite sky, fog, homeIcon;
        public Sprite[] terrain;
        public Shader waterLightShader;
        public bool animateEnvironment = true;

        public Texture2D LoadedJourneyAtlas => roadPlate;
        public Canvas Canvas { get; private set; }
        public RectTransform SafeArea { get; private set; }
        public RectTransform Viewport { get; private set; }
        public RectTransform Content { get; private set; }
        public RectTransform WorldRoot { get; private set; }
        public MapScrollRect Scroll { get; private set; }
        public float WorldHeight { get; private set; }
        public float EnvironmentTime { get; private set; }
        public int ActiveRegion { get; private set; }
        public bool Mounted { get; private set; }
        public bool ProgressionConnected => progression != null;
        public bool InformationVisible => information != null && information.activeSelf;
        public IReadOnlyList<MapPoint> Points => points;
        public static readonly string[] RegionNames = { "Enchanted Village", "Crystal Falls", "Moon Garden", "Sky Castle" };
        public int PackStart => roadBase / 5;
        public int PooledIslands => roadTiles.Length;
        public int ActivePointCount => points.Count(point => point.gameObject.activeInHierarchy);
        public int SelectedChapter { get; private set; } = -1;
        public bool JourneyRunning => journeyRunning;
        public int JourneyTarget => journeyRunning ? walkTo : 0;

        readonly List<MapPoint> points = new();
        Sprite panel;
        TMP_FontAsset font;
        Rect nativeSafe;
        int screenWidth, screenHeight;
        Vector2 lastSafeSize;
        GameObject information;
        TMP_Text informationText, motionLabel;
        Button infoClose;
        JourneyPawTrail pawTrail;
        bool journeyRunning, reducedJourneyMotion;

        static int resultLevel;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetMapRequest() => resultLevel = 0;
        internal static bool RequestResultFocus(int level) { resultLevel = level; return true; }

        void Start() => Mount();
        public void Mount() => MountContinuous();
        void LateUpdate()
        {
            if (animateEnvironment && !reducedJourneyMotion) EnvironmentTime += Time.unscaledDeltaTime;
            TickRoad();
        }

        static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        static Image Art(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max)
        {
            var image = Rect(parent, name, min, max).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        TMP_Text Label(Transform parent, string name, string text, int size, Vector2 min, Vector2 max)
        {
            var label = Rect(parent, name, min, max).gameObject.AddComponent<Localization.LocalizedTMP>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.enableAutoSizing = true;
            label.fontSizeMin = 14;
            label.fontSizeMax = size;
            label.color = new Color32(255, 248, 228, 255);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            MewgicTypographySettings.Prepare(label);
            return label;
        }

        Image Panel(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var image = Art(parent, name, panel, min, max);
            image.type = Image.Type.Sliced;
            return image;
        }

        Button Control(Transform parent, string name, string title, Vector2 min, Vector2 max, Action action)
        {
            var image = Panel(parent, name, min, max);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<PremiumButton>();
            button.SetRole(ButtonRole.Artwork);
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());
            Label(image.transform, "Label", title, 24, new Vector2(.06f,.08f), new Vector2(.94f,.92f));
            return button;
        }

        MapPoint Point(Transform parent, string name, MapPointKind kind, Sprite sprite, float size)
        {
            var rect = Rect(parent, name, Vector2.up, Vector2.up);
            rect.sizeDelta = Vector2.one * size;
            var hit = rect.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var image = Art(rect, "Art", sprite, Vector2.one * .5f, Vector2.one * .5f);
            image.rectTransform.sizeDelta = Vector2.one * size;
            image.preserveAspect = true;
            var button = rect.gameObject.AddComponent<MapPointButton>();
            var point = rect.gameObject.AddComponent<MapPoint>();
            point.kind = kind;
            button.Point = point;
            button.Scroller = Scroll;
            button.targetGraphic = kind == MapPointKind.Chest ? image : hit;
            points.Add(point);
            point.gameObject.AddComponent<MapNodeBadge>().Build(kind == MapPointKind.Chest,
                Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001"), font);
            button.onClick.AddListener(() => ShowInformation(point));
            return point;
        }

        void BuildInformation()
        {
            var overlay = Rect(SafeArea, "MapInformation", Vector2.zero, Vector2.one);
            information = overlay.gameObject;
            var dim = information.AddComponent<Image>();
            dim.color = new Color(.02f,.02f,.07f,.78f);
            var box = Panel(overlay, "InformationPanel", new Vector2(.08f,.32f), new Vector2(.92f,.68f));
            box.raycastTarget = true;
            Label(box.transform, "Title", L.V("common.map"), 35, new Vector2(.07f,.76f), new Vector2(.93f,.94f));
            informationText = Label(box.transform, "Message", "", 27, new Vector2(.08f,.45f), new Vector2(.92f,.75f));
            claimButton = Control(box.transform, "ClaimMapChest", L.V("common.claim"), new Vector2(.15f,.28f), new Vector2(.85f,.44f), ClaimSelectedChest);
            infoClose = Control(box.transform, "MapInfoClose", L.V("common.ok"), new Vector2(.15f,.07f), new Vector2(.85f,.25f),
                () => information.SetActive(false));
            information.SetActive(false);
        }

        void ApplySafeArea()
        {
            nativeSafe = Screen.safeArea;
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            if (screenWidth <= 0 || screenHeight <= 0) return;
            SafeArea.anchorMin = new Vector2(nativeSafe.xMin / screenWidth, nativeSafe.yMin / screenHeight);
            SafeArea.anchorMax = new Vector2(nativeSafe.xMax / screenWidth, nativeSafe.yMax / screenHeight);
            SafeArea.offsetMin = SafeArea.offsetMax = Vector2.zero;
        }

        void UpdateMotionLabel() => motionLabel.text = L.V(reducedJourneyMotion ? "map.motion_reduced" : "map.motion_full");
        public void FocusCurrent() => FocusRoadCurrent();
        public void FocusLevel(int level) => RoadJump(level);
        public void SkipJourney() { if (journeyRunning || awaitingJourney) FinishRoadWalk(); }
        public void CancelJump() { following = false; cameraVelocity = 0; }
        public void Layout() { if (Mounted) LayoutRoad(visibleTile >= 0 ? visibleTile * RoadLevels + 1 : progression.CurrentId); }

        // Older QA helpers use region terminology. These focus the same continuous route.
        public void FocusRegion(int index, bool smooth = true)
        {
            if (index >= 0 && index <= WorldMapRules.LastIsland) FocusLevel(WorldMapRules.FirstLevel(index));
        }
        public void OpenChapter(int region, int group)
        {
            if (region >= 0 && region <= WorldMapRules.LastIsland && group >= 0 && group < 5)
                FocusLevel(WorldMapRules.FirstLevel(region) + group * RoadLevels);
        }
        public float RegionScrollPosition(int index) => WorldHeight <= roadHeight || index < 0 || index > WorldMapRules.LastIsland
            ? 0 : Mathf.Clamp01(1 - (TileOf(WorldMapRules.FirstLevel(index)) - roadBase) * roadHeight / (WorldHeight - roadHeight));
    }
}
