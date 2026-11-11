using Mewgic.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        [SerializeField, Range(1.5f, 3f)] float journeyDuration = 2.2f;
        [SerializeField, Range(.5f, .8f)] float arrivalFraction = .65f;
        float JourneyDuration => crystalJourneyPoses ? Mathf.Max(2.85f, journeyDuration) : Mathf.Clamp(journeyDuration, 1.5f, 3f);
        float ArrivalTime => JourneyDuration * Mathf.Clamp(arrivalFraction, .5f, .8f);
        float CrossingTime => ArrivalTime * .49f;
        float WalkStart => crystalJourneyPoses && fromRegion == CrystalRegion ? .48f : ArrivalTime * .10f;

        [SerializeField, Range(0, 8)] int journeyEffectBudget = 8;
        readonly Graphic[] journeyLights = new Graphic[8];
        RectTransform journeyLightRoot;
        JourneyPawTrail pawTrail;
        CanvasGroup journeySkip;
        bool journeySaveFailed;
        bool journeyStartupPending = true;
        Image journeyVeil;
        CanvasGroup journeyWelcome;
        TMP_Text journeyWelcomeTitle;
        int newlyOpenedRegion = -1;
        TMP_Text motionLabel;
        Button motionButton;
        JourneyCatImage catImage;
        bool reducedJourneyMotion, journeyRunning, journeyArrived, journeyCrossed, crystalJourneyPoses;
        int fromLevel, toLevel, fromRegion, toRegion, fromSlot, toSlot, activeJourneyLights;
        float journeyClock;
        int catFacing = 1;

        public bool JourneyRunning => journeyRunning;
        public int JourneyTarget => journeyRunning ? toLevel : 0;

        void BuildJourneyMotion(RectTransform heading)
        {
            reducedJourneyMotion = GameplayProgress.Current.ReducedMotion;
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
            pawTrail = Rect(journeyContent, "GoldenPawTrail", Vector2.zero, Vector2.one).gameObject.AddComponent<JourneyPawTrail>();
            pawTrail.raycastTarget = false;
            pawTrail.transform.SetSiblingIndex(journeyCat.transform.GetSiblingIndex());
            pawTrail.gameObject.SetActive(false);
            var skip = Art(chapter.transform, "TapToSkipJourney", null, new Vector2(0, .105f), new Vector2(1, .87f));
            skip.color = Color.clear;
            skip.raycastTarget = true;
            var skipButton = skip.gameObject.AddComponent<Button>();
            skipButton.transition = Selectable.Transition.None;
            skipButton.onClick.AddListener(SkipJourney);
            journeySkip = skip.gameObject.AddComponent<CanvasGroup>();
            journeySkip.gameObject.SetActive(false);
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
            if (!ledger.SetReducedMotion(!reducedJourneyMotion)) return;
            reducedJourneyMotion = GameplayProgress.Current.ReducedMotion;
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
            // A direct LevelMap launch also initializes localization asynchronously.
            if (!L.Ready) return;
            journeyStartupPending = false;
            if (progression == null)
                return;
            if (!ledger.PeekMapJourney(out fromLevel, out toLevel))
                return;
            if (progression.CurrentId != toLevel || !progression.CanEnter(toLevel))
                return;

            fromRegion = WorldMapRules.Island(fromLevel);
            toRegion = WorldMapRules.Island(toLevel);
            crystalJourneyPoses = crystalCatRestAtlas != null && (fromRegion == CrystalRegion || toRegion == CrystalRegion);
            fromSlot = (fromLevel - 1) % JourneyLevelCount;
            toSlot = (toLevel - 1) % JourneyLevelCount;
            journeySaveFailed = false;
            journeyClock = 0;
            catFacing = 1;
            journeyRunning = true;
            CancelJourneyCameraOnly();
            journeyScroll.StopMovement();
            journeyScroll.enabled = false;
            journeySkip.gameObject.SetActive(true);
            journeySkip.transform.SetAsLastSibling();
            journeyArrived = false;
            bool simple = reducedJourneyMotion || !animateEnvironment;
            journeyCrossed = simple || !JourneyCuts;
            int region = simple ? toRegion : fromRegion;
            int slot = simple ? toSlot : fromSlot;
            FocusRegion(toRegion, false);
            ShowChapter(region, slot / 10, slot, false);
            if (simple)
            {
                RevealJourney();
                PoseCat(CatRest(toSlot), RestPose);
            }
            else
                PoseCat(CatRest(fromSlot), RestPose);
            pawTrail.gameObject.SetActive(!simple);
            pawTrail.Clear();
        }

        bool JourneyCuts => fromRegion != toRegion || Mathf.Abs(toSlot - fromSlot) > 3 ||
            ((fromRegion == VillageRegion && !useLegacyJourneyArt || fromRegion == CrystalRegion && crystalReference != null) &&
                fromSlot / LevelsPerTerrace != toSlot / LevelsPerTerrace);

        bool JourneyLockPending(int level) => journeyRunning && !journeyArrived && level > fromLevel && level <= toLevel;

        public void SkipJourney() => FinishJourney(true);

        // Acknowledgement follows arrival/explicit skip. Destruction leaves the durable ticket pending.
        void FinishJourney(bool focus)
        {
            if (!journeyRunning)
                return;
            journeySaveFailed = !ledger.AcknowledgeMapJourney(toLevel);
            journeyRunning = false;
            journeyScroll.enabled = true;
            journeyScroll.StopMovement();
            journeySkip.gameObject.SetActive(false);
            pawTrail.gameObject.SetActive(false);
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
            if (journeySaveFailed) Message(L.V("error.save"));
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
            {
                AnimateRestingCat();
                return;
            }

            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            journeyClock += dt;
            if (reducedJourneyMotion || !animateEnvironment)
            {
                if (journeyClock >= .28f)
                    FinishJourney(true);
                return;
            }

            bool crossing = fromRegion != toRegion;
            bool cut = JourneyCuts;
            if (cut)
            {
                float veil = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(ArrivalTime * .28f, ArrivalTime * .48f, journeyClock)) *
                    (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(ArrivalTime * .51f, ArrivalTime * .73f, journeyClock)));
                journeyVeil.gameObject.SetActive(veil > .001f);
                journeyVeil.color = new Color(.16f, .075f, .30f, veil);
                if (!journeyCrossed && journeyClock >= CrossingTime)
                {
                    journeyCrossed = true;
                    ShowChapter(toRegion, toSlot / 10, toSlot, false);
                    if (crossing && toRegion == newlyOpenedRegion)
                    {
                        journeyWelcomeTitle.text = L.Island(toRegion);
                        journeyWelcome.gameObject.SetActive(true);
                    }
                }
            }

            if (!journeyArrived && journeyClock >= ArrivalTime)
                RevealJourney();

            float start = RestSlot(fromSlot);
            float end = RestSlot(toSlot);
            float walk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(WalkStart, ArrivalTime, journeyClock));
            if (cut)
            {
                if (journeyCrossed)
                {
                    start = Mathf.Max(-.45f, end - .85f);
                    if (HasJourneyPages) start = Mathf.Max(toSlot / 10 * 10, start);
                    walk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(CrossingTime, ArrivalTime, journeyClock));
                }
                else
                {
                    end = Mathf.Min(49.45f, start + .60f);
                    walk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(WalkStart, CrossingTime, journeyClock));
                }
            }

            float position = Mathf.Lerp(start, end, walk);
            if (!journeyArrived && !cut && HasCrystalComposition && toSlot == 49 && position >= 49)
                RevealJourney();
            Vector2 feet = RoutePosition(position);
            Vector2 tangent = RoutePosition(position + .02f) - RoutePosition(position - .02f);
            if (Mathf.Abs(tangent.x) > .25f) catFacing = tangent.x > 0 ? 1 : -1;
            float arrival = Mathf.InverseLerp(ArrivalTime, JourneyDuration, journeyClock);
            int steps = JourneyStepCount(start, end);
            PoseCat(feet, CurrentCatPose(walk, arrival, steps, position - start));

            float desired = ScrollForSlot(position);
            if (journeyWelcome.gameObject.activeSelf)
            {
                float appear = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(ArrivalTime * .58f, ArrivalTime * .78f, journeyClock));
                journeyWelcome.alpha = appear * (1 - Mathf.InverseLerp(ArrivalTime, JourneyDuration, journeyClock));
                journeyWelcome.transform.localScale = Vector3.one * Mathf.Lerp(.94f, 1, appear);
            }
            var content = journeyContent.anchoredPosition;
            content.y = Mathf.Lerp(content.y, desired, 1 - Mathf.Exp(-dt * 8));
            journeyContent.anchoredPosition = content;

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
            UpdatePawTrail(start, end, walk, arrival, steps);

            if (journeyClock >= JourneyDuration)
                FinishJourney(true);
        }

        void CancelJourneyCameraOnly() { journeyFocusSlot = -1; focusVelocity = 0; }

        JourneyCatPose RestPose => HasCrystalRest ? JourneyCatPose.Curled : JourneyCatPose.Sitting;

        JourneyCatPose CurrentCatPose(float progress, float arrival, int steps, float travelled)
        {
            if (journeyClock < WalkStart)
            {
                if (!HasCrystalRest) return RestPose;
                float wake = journeyClock / WalkStart;
                if (wake < .18f) return JourneyCatPose.Curled;
                if (wake < .40f) return JourneyCatPose.Blink;
                return wake < .69f ? JourneyCatPose.Stretch : JourneyCatPose.Standing;
            }
            if (journeyClock < ArrivalTime)
            {
                int step = HasCrystalRest ? Mathf.FloorToInt(progress * (steps + 1)) : Mathf.FloorToInt(Mathf.Abs(travelled) * 7);
                return step % 2 == 0 ? JourneyCatPose.StepA : JourneyCatPose.StepB;
            }
            if (!HasCrystalRest || arrival >= .53f) return RestPose;
            return arrival < .19f ? JourneyCatPose.Standing : JourneyCatPose.Stretch;
        }

        void AnimateRestingCat()
        {
            if (reducedJourneyMotion || !animateEnvironment || journeyCat == null || !journeyCat.enabled) return;
            int displayed = ledger.MapJourney.displayedLevel;
            if (WorldMapRules.Island(displayed) != journeyRegion) return;
            int slot = (displayed - 1) % JourneyLevelCount;
            Vector2 feet = CatRest(slot);
            if (!InJourneyView(feet.y)) return;
            float breath = Mathf.Sin(Time.unscaledTime * 2.1f);
            float blink = Mathf.Repeat(EnvironmentTime + .9f, 5.2f);
            PoseCat(feet, HasCrystalRest && blink < .14f ? JourneyCatPose.Blink : RestPose);
            journeyCat.rectTransform.localScale = new Vector3(1 - breath * .006f, 1 + breath * .009f, 1);
        }

        int JourneyStepCount(float start, float end)
        {
            int budget = activeJourneyLights == 0 ? 4 : activeJourneyLights + 4;
            return Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(end - start) * 7), 3, budget);
        }

        void UpdatePawTrail(float start, float end, float progress, float arrival, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1);
                Vector2 point = RoutePosition(Mathf.Lerp(start, end, t));
                Vector2 tangent = RoutePosition(Mathf.Lerp(start, end, Mathf.Min(1, t + .015f))) - point;
                Vector2 side = Vector2.Perpendicular(tangent.normalized);
                float alpha = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(t, t + .07f, progress)) * (1 - arrival);
                pawTrail.Set(i, point + side * (i % 2 == 0 ? 7 : -7), tangent,
                    journeyScroll.viewport.rect.width * (HasCrystalComposition ? .026f : .021f), alpha);
            }
            pawTrail.Render(count);
        }

        float ScrollForSlot(float slot) => HasJourneyPages
            ? Mathf.Clamp(Mathf.Floor((slot + .01f) / 10), 0, 4) * rowHeight * 10
            : Mathf.Clamp((slot + .5f) * rowHeight - journeyScroll.viewport.rect.height * .5f,
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
                    Vector2 center = JourneyNodePosition(toSlot);
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
            if (HasCrystalComposition) return CrystalRoutePosition(slot);
            if (HasVillageTerraces) return VillagePosition(slot,false);
            if (!useLegacyJourneyArt)
                return new Vector2(JourneyRouteGraphic.FoundationLane(slot) * journeyScroll.viewport.rect.width, -(slot + .5f) * rowHeight);
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

        // The first node sits against the upper scroll limit. Leave room for the cat's hat.
        static readonly float[] CrystalRestOffsets = { .42f, .32f, .43f, .22f, .37f, .38f, .34f, .22f, .41f, .85f };

        float RestSlot(int slot)
        {
            if (HasCrystalComposition && slot >= 40) return slot + CrystalRestOffsets[slot - 40];
            return !useLegacyJourneyArt && slot == 0 ? .70f : slot + .22f;
        }
        Vector2 CatRest(int slot) => RoutePosition(RestSlot(slot));

        void PlaceRestingCat()
        {
            if (journeyCat == null || journeyCat.texture == null) return;
            int displayed = ledger.MapJourney.displayedLevel;
            journeyCat.enabled = WorldMapRules.Island(displayed) == journeyRegion;
            var shadow = journeyCatShadow != null ? journeyCatShadow : crystalCatShadow;
            if (shadow != null) shadow.gameObject.SetActive(journeyCat.enabled);
            if (!journeyCat.enabled) return;
            PoseCat(CatRest((displayed - 1) % JourneyLevelCount), RestPose);
        }

        void PoseCat(Vector2 feet, JourneyCatPose pose)
        {
            if (journeyCat.texture == null) return;
            journeyCat.enabled = true;
            float width = journeyScroll.viewport.rect.width;
            float height = Mathf.Min(width * .205f, rowHeight * 1.15f);
            if (HasCrystalComposition && feet.y < -40 * rowHeight)
                height = Mathf.Lerp(width * .16f, width * .276f, Mathf.InverseLerp(.25f, .85f, (-feet.y / rowHeight - 40) / 10));
            float aspect = catImage.ShowPose(journeyMascot, journeyCatPoses, crystalCatRestAtlas, pose);
            if (pose is JourneyCatPose.Curled or JourneyCatPose.Blink) height *= .71f;
            if (pose == JourneyCatPose.Stretch) height *= .91f;
            var rect = journeyCat.rectTransform;
            rect.pivot = new Vector2(.5f, journeyCatPoses != null ? .012f : .12f);
            rect.sizeDelta = new Vector2(height * aspect, height);
            rect.anchoredPosition = feet;
            bool directed = pose is JourneyCatPose.StepA or JourneyCatPose.StepB or JourneyCatPose.Standing or JourneyCatPose.Stretch;
            rect.localScale = new Vector3(directed ? catFacing : 1, 1, 1);
            var shadow = journeyCatShadow != null ? journeyCatShadow : crystalCatShadow;
            if (shadow == null) return;
            shadow.gameObject.SetActive(true);
            shadow.rectTransform.anchoredPosition = feet;
            shadow.rectTransform.sizeDelta = new Vector2(width * .116f, width * .036f);
        }
    }

    internal enum JourneyCatPose { Sitting, StepA, StepB, Curled, Blink, Stretch, Standing }

    public sealed class JourneyCatImage : RawImage
    {
        // Three padded UV crops share a single atlas and the same paw baseline.
        static readonly Rect[] Poses =
        {
            new Rect(32f / 1536, 244f / 1024, 424f / 1536, 548f / 1024),
            new Rect(513f / 1536, 244f / 1024, 484f / 1536, 548f / 1024),
            new Rect(1025f / 1536, 244f / 1024, 484f / 1536, 548f / 1024)
        };
        static readonly Rect[] RestPoses =
        {
            AtlasCrop(100, 130, 468, 407), AtlasCrop(683, 130, 468, 407),
            AtlasCrop(46, 691, 544, 511), AtlasCrop(650, 635, 532, 567)
        };

        static Rect AtlasCrop(float x, float y, float width, float height)
        {
            const float size = 1254;
            return new Rect(x / size, 1 - (y + height) / size, width / size, height / size);
        }

        internal static Rect SittingUV => Poses[0];
        Material defaultMaterial;
        bool usingCostume;
        public ShopItem WornCostume { get; private set; }
        public int PoseIndex { get; private set; }
        internal JourneyCatPose Pose => (JourneyCatPose)PoseIndex;

        internal float ShowPose(Texture2D fallback, Texture2D walkAtlas, Texture2D restAtlas, JourneyCatPose pose)
        {
            WornCostume = GameplayProgress.Current.Equipped(ShopCategory.Cats);
            var dressed = Characters.CostumeArt.Get(WornCostume, (Characters.CostumePose)((int)pose + 1));
            if (dressed != null)
            {
                if (!usingCostume) { defaultMaterial = material; material = null; usingCostume = true; }
                PoseIndex = (int)pose;
                texture = dressed.texture;
                uvRect = Characters.CostumeArt.UV(dressed);
                return dressed.rect.width / dressed.rect.height;
            }
            if (usingCostume) { material = defaultMaterial; usingCostume = false; }
            bool rest = pose >= JourneyCatPose.Curled && restAtlas != null;
            PoseIndex = rest || walkAtlas != null ? (int)pose : 0;
            Texture desired = rest ? restAtlas : walkAtlas != null ? walkAtlas : fallback;
            if (texture != desired) texture = desired;
            var crop = rest ? RestPoses[PoseIndex - (int)JourneyCatPose.Curled] :
                walkAtlas != null ? Poses[Mathf.Min(PoseIndex, 2)] : new Rect(0, 0, 1, 1);
            if (uvRect != crop) uvRect = crop;
            return texture.width * crop.width / (texture.height * crop.height);
        }
    }
}
