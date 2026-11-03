using Mewgic.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const string ReducedMotionKey = "Mewgic.Map.ReducedMotion";
        const float JourneyDuration = 1.8f;
        const float ArrivalTime = 1.17f;
        [SerializeField, Range(0, 8)] int journeyEffectBudget = 8;
        readonly Graphic[] journeyLights = new Graphic[8];
        RectTransform journeyLightRoot;
        Image journeyVeil;
        CanvasGroup journeyWelcome;
        TMP_Text journeyWelcomeTitle;
        int newlyOpenedRegion = -1;
        TMP_Text motionLabel;
        Button motionButton;
        JourneyCatImage catImage;
        bool reducedJourneyMotion, journeyRunning, journeyArrived, journeyCrossed;
        int fromLevel, toLevel, fromRegion, toRegion, fromSlot, toSlot, activeJourneyLights;
        float journeyClock, catPoseClock;

        public bool JourneyRunning => journeyRunning;
        public int JourneyTarget => journeyRunning ? toLevel : 0;

        void BuildJourneyMotion(RectTransform heading)
        {
            reducedJourneyMotion = PlayerPrefs.GetInt(ReducedMotionKey, 0) == 1;
            motionButton = Control(heading, "JourneyMotion", "", new Vector2(.72f, .05f), new Vector2(.97f, .33f), ToggleJourneyMotion);
            motionLabel = motionButton.GetComponentInChildren<TMP_Text>();
            motionLabel.fontSizeMax = 22;
            UpdateMotionLabel();
            activeJourneyLights = Mathf.Clamp(journeyEffectBudget, 0, journeyLights.Length);
            if (SystemInfo.systemMemorySize > 0 && SystemInfo.systemMemorySize < 3072)
                activeJourneyLights = Mathf.Min(activeJourneyLights, 3);

            journeyLightRoot = Rect(journeyContent, "JourneyUnlockLights", Vector2.zero, Vector2.one);
            for (int i = 0; i < activeJourneyLights; i++)
            {
                var r = Rect(journeyLightRoot, "JourneySpark_" + i, Vector2.up, Vector2.up);
                r.sizeDelta = Vector2.one * (i % 2 == 0 ? 15 : 10);
                var spark = r.gameObject.AddComponent<Characters.WizardSparkGraphic>();
                spark.color = i % 2 == 0 ? new Color(1, .81f, .4f) : new Color(.85f, .51f, 1);
                spark.raycastTarget = false;
                journeyLights[i] = spark;
            }
            journeyLightRoot.gameObject.SetActive(false);
            journeyVeil = Art(journeyScroll.viewport, "JourneyIslandVeil", null, Vector2.zero, Vector2.one);
            var welcome = Panel(chapter.transform, "JourneyIslandWelcome", new Vector2(.10f, .718f), new Vector2(.90f, .838f));
            welcome.raycastTarget = false;
            journeyWelcome = welcome.gameObject.AddComponent<CanvasGroup>();
            journeyWelcome.blocksRaycasts = false;
            journeyWelcome.interactable = false;
            journeyWelcomeTitle = Label(welcome.transform, "IslandName", "", 39, new Vector2(.08f, .16f), new Vector2(.92f, .85f));
            journeyWelcome.gameObject.SetActive(false);
            journeyVeil.gameObject.SetActive(false);
        }

        void UpdateMotionLabel() => motionLabel.text = L.V(reducedJourneyMotion ? "map.motion_reduced" : "map.motion_full");

        void ToggleJourneyMotion()
        {
            reducedJourneyMotion = !reducedJourneyMotion;
            if (!GameplayProgress.IsTestProfile)
            {
                PlayerPrefs.SetInt(ReducedMotionKey, reducedJourneyMotion ? 1 : 0);
                PlayerPrefs.Save();
            }
            UpdateMotionLabel();
            if (journeyRunning)
                FinishJourney(true);
            firstBound = -1;
            BindJourneyWindow();
            if (crystalMotes[0] != null)
                crystalMotes[0].transform.parent.gameObject.SetActive(HasCrystalArt && animateEnvironment && !reducedJourneyMotion);
        }

        void TryStartJourney()
        {
            if (progression == null)
                return;
            if (!ledger.TakeMapJourney(out fromLevel, out toLevel))
            {
                // A player may reach the first level via Next or reopen after a completed save.
                if (newlyOpenedRegion <= 0 || progression.CurrentRegion != newlyOpenedRegion ||
                    progression.CurrentId != WorldMapRules.FirstLevel(newlyOpenedRegion))
                    return;
                toLevel = progression.CurrentId;
                fromLevel = toLevel - 1;
            }
            if (progression.CurrentId != toLevel || !progression.CanEnter(toLevel))
                return;

            fromRegion = WorldMapRules.Island(fromLevel);
            toRegion = WorldMapRules.Island(toLevel);
            fromSlot = (fromLevel - 1) % JourneyLevelCount;
            toSlot = (toLevel - 1) % JourneyLevelCount;
            journeyClock = 0;
            catPoseClock = 0;
            journeyRunning = true;
            journeyArrived = false;
            bool simple = reducedJourneyMotion || !animateEnvironment;
            journeyCrossed = simple || fromRegion == toRegion;
            int region = simple ? toRegion : fromRegion;
            int slot = simple ? toSlot : fromSlot;
            FocusRegion(toRegion, false);
            ShowChapter(region, slot / 10, slot, false);
            if (simple)
            {
                RevealJourney();
                PlaceRestingCat();
            }
            else
                PoseCat(CatRest(fromSlot), 1, 0, 0);
        }

        bool JourneyLockPending(int level) => journeyRunning && !journeyArrived && level == toLevel;

        // A scene change or manual scroll can discard the presentation. The save is already committed.
        void FinishJourney(bool focus)
        {
            if (!journeyRunning)
                return;
            journeyRunning = false;
            journeyVeil.gameObject.SetActive(false);
            journeyLightRoot.gameObject.SetActive(false);
            journeyWelcome.gameObject.SetActive(false);
            if (focus && journeyRegion != toRegion)
                ShowChapter(toRegion, toSlot / 10, toSlot, false);
            if (focus)
                JumpToJourneySlot(toSlot);
            firstBound = -1;
            BindJourneyWindow();
            PlaceRestingCat();
        }

        void RevealJourney()
        {
            journeyArrived = true;
            firstBound = -1;
            BindJourneyWindow();
        }

        void AnimateJourney()
        {
            if (!journeyRunning)
                return;

            float dt = Mathf.Min(Time.unscaledDeltaTime, .05f);
            journeyClock += dt;
            if (reducedJourneyMotion || !animateEnvironment)
            {
                if (journeyClock >= .28f)
                    FinishJourney(true);
                return;
            }

            bool crossing = fromRegion != toRegion;
            if (crossing)
            {
                float veil = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.40f, .69f, journeyClock)) *
                    (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.73f, 1.04f, journeyClock)));
                journeyVeil.gameObject.SetActive(veil > .001f);
                journeyVeil.color = new Color(.16f, .075f, .30f, veil);
                if (!journeyCrossed && journeyClock >= .70f)
                {
                    journeyCrossed = true;
                    ShowChapter(toRegion, toSlot / 10, toSlot, false);
                    if (toRegion == newlyOpenedRegion)
                    {
                        journeyWelcomeTitle.text = L.Island(toRegion);
                        journeyWelcome.gameObject.SetActive(true);
                    }
                }
            }

            if (!journeyArrived && journeyClock >= ArrivalTime)
                RevealJourney();

            float walk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.14f, ArrivalTime, journeyClock));
            float position = Mathf.Lerp(fromSlot, toSlot, walk);
            if (crossing)
                position = journeyCrossed
                    ? toSlot - .43f * (1 - Mathf.InverseLerp(.70f, ArrivalTime, journeyClock))
                    : fromSlot + .43f * Mathf.InverseLerp(.14f, .70f, journeyClock);

            Vector2 feet = RoutePosition(position);
            if (walk < .25f && !crossing)
                feet = Vector2.Lerp(CatRest(fromSlot), feet, walk * 4);
            if (journeyArrived)
                feet = Vector2.Lerp(feet, CatRest(toSlot), Mathf.SmoothStep(0, 1, Mathf.InverseLerp(ArrivalTime, 1.47f, journeyClock)));

            float hop = journeyArrived ? Mathf.Sin(Mathf.Clamp01((journeyClock - 1.32f) / .36f) * Mathf.PI) * 18 : 0;
            float gait = walk < 1 && journeyClock > .14f ? Mathf.Sin(journeyClock * 35) : 0;
            float size = .92f + Mathf.Clamp01((position + .5f) / JourneyLevelCount) * .11f;
            PoseCat(feet, size, Mathf.Abs(gait) * 6 + hop, gait);
            catPoseClock -= dt;
            if (catPoseClock <= 0)
            {
                catPoseClock = 1f / 24;
                if (InJourneyView(feet.y)) catImage.SetPose(gait);
            }

            float desired = ScrollForSlot(position);
            if (journeyWelcome.gameObject.activeSelf)
            {
                float appear = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.83f, 1.12f, journeyClock));
                journeyWelcome.alpha = appear * (1 - Mathf.InverseLerp(1.55f, JourneyDuration, journeyClock));
                journeyWelcome.transform.localScale = Vector3.one * Mathf.Lerp(.94f, 1, appear);
                desired = Mathf.Max(desired, rowHeight * .70f *
                    Mathf.Sin(Mathf.InverseLerp(.72f, JourneyDuration, journeyClock) * Mathf.PI));
            }
            var content = journeyContent.anchoredPosition;
            content.y = Mathf.Lerp(content.y, desired, 1 - Mathf.Exp(-dt * 8));
            journeyContent.anchoredPosition = content;

            float arrival = Mathf.InverseLerp(ArrivalTime, JourneyDuration, journeyClock);
            foreach (var point in chapterNodes)
            {
                if (!point.gameObject.activeSelf)
                    continue;
                bool source = point.regionIndex == fromRegion && point.slotIndex == fromSlot;
                bool target = point.regionIndex == toRegion && point.slotIndex == toSlot;
                if (source || target)
                    point.GetComponent<MapNodeBadge>().SetJourneyAccent(source
                        ? Mathf.Sin(Mathf.Clamp01(journeyClock / .77f) * Mathf.PI) * .72f
                        : journeyArrived ? Mathf.Sin(arrival * Mathf.PI) : 0, target);
            }
            DrawJourneyLights(position, crossing, arrival);

            if (journeyClock >= JourneyDuration)
                FinishJourney(true);
        }

        float ScrollForSlot(float slot) => Mathf.Clamp((slot + .5f) * rowHeight - journeyScroll.viewport.rect.height * .5f,
            0, Mathf.Max(0, journeyContent.rect.height - journeyScroll.viewport.rect.height));

        bool InJourneyView(float y)
        {
            float top = journeyContent.anchoredPosition.y;
            return -y >= top - rowHeight && -y <= top + journeyScroll.viewport.rect.height + rowHeight;
        }

        void DrawJourneyLights(float position, bool crossing, float arrival)
        {
            if (activeJourneyLights == 0)
                return;
            journeyLightRoot.gameObject.SetActive(true);
            for (int i = 0; i < activeJourneyLights; i++)
            {
                var light = journeyLights[i];
                if (journeyArrived)
                {
                    float angle = i * (Mathf.PI * 2 / activeJourneyLights) + arrival * 3.8f;
                    Vector2 center = RoutePosition(toSlot);
                    float radius = journeyScroll.viewport.rect.width * (toLevel == 100 ? .103f : .085f);
                    light.rectTransform.anchoredPosition = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .68f) * radius;
                    light.canvasRenderer.SetAlpha(InJourneyView(center.y) ? Mathf.Sin(arrival * Mathf.PI) * .84f : 0);
                }
                else
                {
                    float t = (i + 1f) / (activeJourneyLights + 1);
                    float slot = crossing ? position + (t - .5f) * .47f : Mathf.Lerp(fromSlot, toSlot, t);
                    light.rectTransform.anchoredPosition = RoutePosition(slot);
                    float pulse = Mathf.Clamp01(1 - Mathf.Abs(journeyClock - (.13f + t * .8f)) / .38f);
                    light.canvasRenderer.SetAlpha(InJourneyView(light.rectTransform.anchoredPosition.y) ? pulse * .91f : 0);
                }
            }
        }

        Vector2 RoutePosition(float slot)
        {
            int a = Mathf.Clamp(Mathf.FloorToInt(slot), 0, JourneyLevelCount - 2);
            float t = Mathf.Clamp01(slot - a);
            float start = JourneyLane(a), end = JourneyLane(a + 1);
            float mid = (start + end) * .5f;
            if (HasThemedArt)
                return new Vector2(ThemedLane(slot) * journeyScroll.viewport.rect.width, -(slot + .5f) * rowHeight);
            if (HasCrystalArt)
            {
                if (a >= 40)
                    mid = CrystalFinalBends[a - 40];
                else if ((a + 1) % 10 == 0)
                    mid = .5f;
            }
            float x = Mathf.Lerp(start, end, t) + 4 * t * (1 - t) * (mid - (start + end) * .5f);
            return new Vector2(x * journeyScroll.viewport.rect.width, -(slot + .5f) * rowHeight);
        }

        static readonly float[] CrystalFinalBends = { .58f, .72f, .42f, .22f, .40f, .43f, .69f, .62f, .43f };

        Vector2 CatRest(int slot)
        {
            var pos = RoutePosition(slot + .31f);
            float width = journeyScroll.viewport.rect.width;
            if (HasCrystalArt && slot == JourneyLevelCount - 1)
                return new Vector2(width * .572f, -rowHeight * 49.22f);
            pos.x += width * (JourneyLane(slot) < .52f ? .092f : -.092f);
            return pos;
        }

        void PlaceRestingCat()
        {
            if (journeyCat == null || journeyMascot == null)
                return;
            int slot = CurrentJourneySlot();
            PoseCat(CatRest(slot), 1, 0, 0);
            catImage.SetPose(0);
        }

        void PoseCat(Vector2 feet, float size, float lift, float gait)
        {
            if (journeyMascot == null)
                return;
            float width = journeyScroll.viewport.rect.width;
            float height = width * .124f * size;
            var r = journeyCat.rectTransform;
            r.pivot = new Vector2(.5f, .12f);
            r.sizeDelta = new Vector2(height * journeyMascot.width / journeyMascot.height, height);
            r.anchoredPosition = feet + Vector2.up * lift;
            r.localScale = new Vector3(1 + Mathf.Abs(gait) * .025f, 1 - Mathf.Abs(gait) * .023f, 1);
            r.localRotation = Quaternion.Euler(0, 0, gait * 2.6f);
            if (crystalCatShadow != null)
            {
                crystalCatShadow.rectTransform.anchoredPosition = feet;
                crystalCatShadow.rectTransform.sizeDelta = new Vector2(width * .115f, width * .034f) * size;
            }
        }
    }

    // A small grid deforms the original texture, keeping the approved character and its markings.
    public sealed class JourneyCatImage : RawImage
    {
        const int Columns = 10, Rows = 10;
        float gait;

        public void SetPose(float step)
        {
            if (Mathf.Abs(gait - step) < .01f)
                return;
            gait = step;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            for (int y = 0; y <= Rows; y++)
                for (int x = 0; x <= Columns; x++)
                {
                    float u = x / (float)Columns, v = y / (float)Rows;
                    float tail = Mathf.Clamp01(1 - Mathf.Abs(u - .19f) / .20f) *
                        Mathf.Clamp01(1 - Mathf.Abs(v - .24f) / .18f);
                    float paws = Mathf.Clamp01(1 - Mathf.Abs(v - .16f) / .17f) *
                        Mathf.Clamp01(1 - Mathf.Abs(u - .58f) / .32f);
                    var p = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, u), Mathf.Lerp(rect.yMin, rect.yMax, v));
                    p += new Vector2(tail * gait * 9, tail * gait * 6 + paws * Mathf.Max(0, u < .58f ? gait : -gait) * 8);
                    mesh.AddVert(p, color, new Vector2(u, v));
                    if (x == 0 || y == 0)
                        continue;
                    int i = mesh.currentVertCount - 1;
                    mesh.AddTriangle(i - Columns - 2, i - 1, i);
                    mesh.AddTriangle(i - Columns - 2, i, i - Columns - 1);
                }
        }
    }
}
