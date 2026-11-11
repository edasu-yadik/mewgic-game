using System.Collections.Generic;
using Mewgic.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const int JourneyLevelCount = 50;
        [SerializeField] Texture2D journeyMascot;
        readonly List<Image> journeyScenery = new List<Image>();
        ScrollRect journeyScroll;
        RectTransform journeyContent;
        RawImage journeyCat;
        Button journeyBack, journeyFocus, detailPrevious, detailNext;
        const int TerraceCount = JourneyLevelCount / LevelsPerTerrace;
        const int VillageRegion = 0;
        bool HasVillageTerraces => !useLegacyJourneyArt && journeyRegion == VillageRegion;
        bool HasJourneyPages => HasCrystalComposition || HasVillageTerraces;
        RectTransform journeyHeading, journeyFooter;
        GameObject journeyInstruction, crystalBrand, villageScenery;
        int journeyRegion, firstBound = -1, lastBound = -1;
        float rowHeight, journeyViewportHeight, focusVelocity;
        int journeyFocusSlot = -1;

        void BuildChapter()
        {
            var overlay = Rect(SafeArea, "ChapterSelection", Vector2.zero, Vector2.one);
            chapter = overlay.gameObject;
            var dim = overlay.gameObject.AddComponent<Image>();
            dim.color = new Color32(9, 19, 36, 255);
            var clouds = Art(overlay, "JourneySky", sky, Vector2.zero, Vector2.one);
            clouds.color = new Color(.57f, .70f, .82f);

            var scroll = Rect(overlay, "JourneyScroll", new Vector2(0, .105f), new Vector2(1, .87f));
            var mapScroll = scroll.gameObject.AddComponent<MapScrollRect>();
            mapScroll.UserInput = CancelJourneyFocus;
            journeyScroll = mapScroll;
            journeyScroll.horizontal = false;
            journeyScroll.movementType = ScrollRect.MovementType.Clamped;
            journeyScroll.scrollSensitivity = 56;
            journeyScroll.decelerationRate = .10f;
            scroll.gameObject.AddComponent<Image>().color = Color.clear;
            scroll.gameObject.AddComponent<RectMask2D>();
            journeyScroll.viewport = scroll;
            if (!useLegacyJourneyArt)
            {
                BuildJourneyBackground(scroll);
                BuildVillageScenery(scroll);
            }
            journeyContent = Rect(scroll, "JourneyContent", Vector2.up, Vector2.one);
            journeyContent.pivot = new Vector2(.5f, 1);
            journeyScroll.content = journeyContent;

            for (int i = 0; useLegacyJourneyArt && i < 10; i++)
            {
                var art = Art(journeyContent, "IslandLandscape_" + i, terrain[1], Vector2.up, Vector2.one);
                art.preserveAspect = true;
                art.material = waterLight;
                journeyScenery.Add(art);
            }

            var route = Rect(journeyContent, "StoneRoute", Vector2.zero, Vector2.one);
            journeyRoute = route.gameObject.AddComponent<JourneyRouteGraphic>();
            journeyRoute.raycastTarget = false;
            BuildCrystalJourney();
            if (useLegacyJourneyArt)
                BuildThemedJourney();
            else
            {
                BuildJourneyCatShadow();
                BuildJourneyForeground(scroll);
            }

            var cat = Rect(journeyContent, "WizardCatGuide", Vector2.up, Vector2.up);
            catImage = cat.gameObject.AddComponent<JourneyCatImage>();
            journeyCat = catImage;
            journeyCat.texture = journeyCatPoses != null ? journeyCatPoses : journeyMascot;
            journeyCat.raycastTarget = false;
            journeyCat.enabled = journeyCat.texture != null;
            SetJourneyCatMaterial();

            var heading = Panel(overlay, "JourneyHeading", new Vector2(0, .87f), Vector2.one);
            heading.color = new Color32(238, 215, 255, 255);
            journeyHeading = heading.rectTransform;
            var brand = Panel(overlay, "CrystalBrand", new Vector2(.339f, .917f), new Vector2(.659f, .987f));
            brand.color = new Color32(224, 203, 241, 245);
            crystalBrand = brand.gameObject;
            Label(brand.transform, "ChapterTitle", "MEWGIC", 33, new Vector2(.08f, .43f), new Vector2(.92f, .98f));
            Label(brand.transform, "WorldTitle", L.V("home.nav.map"), 22,
                new Vector2(.09f, .06f), new Vector2(.91f, .45f));
            crystalBrand.SetActive(false);
            Art(heading.transform, "GoldEdge", null, Vector2.zero, new Vector2(1, .013f)).color = new Color32(159, 115, 58, 255);
            chapterTitle = Label(heading.transform, "ChapterTitle", "", 36, new Vector2(.04f, .34f), new Vector2(.96f, .95f));
            journeyInstruction = Label(heading.transform, "ChapterInstruction", L.V("map.choose"), 24,
                new Vector2(.05f, .03f), new Vector2(.70f, .35f)).gameObject;
            detailPrevious = Control(overlay, "DetailPrevious", "‹", new Vector2(.16f, .851f), new Vector2(.238f, .905f), () => ChangeTerrace(-1));
            detailNext = Control(overlay, "DetailNext", "›", new Vector2(.762f, .851f), new Vector2(.84f, .905f), () => ChangeTerrace(1));
            BuildJourneyMotion(heading.rectTransform);

            var footer = Panel(overlay, "JourneyFooter", Vector2.zero, new Vector2(1, .105f));
            footer.color = heading.color;
            journeyFooter = footer.rectTransform;
            journeyBack = Control(footer.transform, "ChapterClose", L.V("map.return"),
                new Vector2(.07f, .12f), new Vector2(.60f, .88f), CloseJourney);
            journeyFocus = Control(footer.transform, "JourneyFocus", L.V("map.focus"),
                new Vector2(.63f, .12f), new Vector2(.93f, .88f), FocusJourneyCurrent);
            journeyScroll.onValueChanged.AddListener(_ => BindJourneyWindow());
            chapter.SetActive(false);
        }

        void BuildVillageScenery(RectTransform viewport)
        {
            var scenery = Rect(viewport, "VillageChapterScenery", Vector2.zero, Vector2.one);
            villageScenery = scenery.gameObject;
            var board = Panel(scenery, "VillageChapterBoard", new Vector2(.065f,.034f), new Vector2(.935f,.515f));
            board.color = new Color32(232,214,248,255);
            var island = Art(scenery, "VillagePaintedIsland", terrain[VillageRegion],
                new Vector2(-.035f,.415f), new Vector2(1.035f,1.025f));
            island.preserveAspect = true;
            villageScenery.SetActive(false);
        }

        Vector2 VillagePosition(float slot, bool marker)
        {
            int terrace = Mathf.Clamp(Mathf.FloorToInt(slot / LevelsPerTerrace),0,TerraceCount-1);
            float within = Mathf.Clamp(slot-terrace*LevelsPerTerrace,0,9.65f);
            int node = Mathf.Min(9,Mathf.FloorToInt(within));
            float row = node / 2;
            if (node % 2 != 0) row += within-node;
            float x = marker ? (node%2 == 0 ? .285f : .715f) : .5f + Mathf.Sin(within*Mathf.PI)*.055f;
            if (marker && node%2 != 0) row = node / 2;
            return new Vector2(x*journeyScroll.viewport.rect.width,
                -(terrace*LevelsPerTerrace+(5.35f+row*.90f))*rowHeight);
        }

        public void OpenChapter(int region, int group)
        {
            if (!journeyRunning && ledger != null && ledger.PeekMapJourney(out int start, out int target) &&
                (JourneyTerraceContains(start, region, group) || JourneyTerraceContains(target, region, group)))
            {
                TryStartJourney();
                if (journeyRunning) return;
            }
            FinishJourney(false);
            ShowChapter(region, group, -1, true);
        }

        static bool JourneyTerraceContains(int level, int region, int group) =>
            WorldMapRules.Island(level) == region && (level - 1) % JourneyLevelCount / LevelsPerTerrace == group;

        void ShowChapter(int region, int group, int focusSlot, bool saveFocus)
        {
            if (navigating || progression == null || region < 0 || region > WorldMapRules.LastIsland || group < 0 || group > 4)
                return;

            journeyFocusSlot = -1;
            journeySettleAt = 0;
            journeyRegion = region;
            if (useLegacyJourneyArt) LoadThemedJourney(region);
            SelectedChapter = region * 5 + group;
            int first = progression.LevelId(region, 0);
            chapterTitle.text = L.V("map.chapter", L.Island(region), first, progression.LevelId(region, 49));
            Scroll.StopMovement();
            jumping = false;
            pendingFocus = -1;
            if (saveFocus) ledger.SaveMapFocus(progression.LevelId(region, group * LevelsPerTerrace));

            chapter.SetActive(true);
            chapter.transform.SetAsLastSibling();
            LayoutJourneyFrame();
            UnityEngine.Canvas.ForceUpdateCanvases();
            ResizeJourney(false);
            BindJourneyTheme(region);
            int current = CurrentJourneySlot();
            JumpToJourneySlot(focusSlot >= 0 ? focusSlot : current / 10 == group ? current : group * 10);
            journeyBack.Select();
        }

        void CloseJourney()
        {
            FinishJourney(false);
            chapter.SetActive(false);
            ReleaseThemedJourney();
            FocusRegion(journeyRegion, false);
            pendingFocus = -1;
            var island = views.Find(view => view.region == journeyRegion);
            if (island != null)
                island.groups[0].GetComponent<Button>().Select();
        }

        void ChangeTerrace(int direction)
        {
            int group = SelectedChapter - journeyRegion * TerraceCount + direction;
            if (journeyRunning || group < 0 || group >= TerraceCount)
                return;

            CancelJourneyFocus();
            journeySettleAt = 0;
            JumpToJourneySlot(group * LevelsPerTerrace);
            ledger.SaveMapFocus(progression.LevelId(journeyRegion, group * LevelsPerTerrace));
        }

        void CancelJourneyFocus()
        {
            journeyFocusSlot = -1;
            FinishJourney(false);
            if (HasJourneyPages) journeySettleAt = Time.unscaledTime + .24f;
        }

        void FocusJourneyCurrent()
        {
            CancelJourneyFocus();
            journeySettleAt = 0;
            if (journeyRegion != progression.CurrentRegion)
            {
                FocusCurrent();
                return;
            }

            int slot = CurrentJourneySlot();
            if (reducedJourneyMotion || !animateEnvironment)
            {
                JumpToJourneySlot(slot);
                return;
            }
            journeyScroll.StopMovement();
            focusVelocity = 0;
            journeyFocusSlot = slot;
        }

        void AnimateJourneyFocus()
        {
            SettleJourneyPage();
            if (journeyFocusSlot < 0)
                return;
            var position = journeyContent.anchoredPosition;
            float target = ScrollForSlot(journeyFocusSlot);
            position.y = Mathf.SmoothDamp(position.y, target, ref focusVelocity, .19f, 5400, Time.unscaledDeltaTime);
            journeyContent.anchoredPosition = position;
            if (Mathf.Abs(position.y - target) < .5f)
            {
                JumpToJourneySlot(journeyFocusSlot);
                journeyFocusSlot = -1;
            }
        }

        int CurrentJourneySlot() => Mathf.Clamp(progression.CurrentId - progression.LevelId(journeyRegion, 0), 0, 49);

        void JumpToJourneySlot(int slot)
        {
            journeyScroll.StopMovement();
            journeyContent.anchoredPosition = Vector2.up * ScrollForSlot(slot);
            firstBound = -1;
            BindJourneyWindow();
        }

        void ResizeJourney(bool keepPosition = true)
        {
            if (!chapter.activeSelf)
                return;

            float oldHeight = rowHeight;
            float width = journeyScroll.viewport.rect.width;
            float previousViewportHeight = journeyViewportHeight;
            journeyViewportHeight = journeyScroll.viewport.rect.height;
            // Keep a ten-level terrace within the viewport on short phones.
            float preferredRow = HasJourneyPages ? journeyViewportHeight / LevelsPerTerrace : !useLegacyJourneyArt ? width * .17f : HasCrystalArt ? Mathf.Min(width * .15f, journeyViewportHeight / 10f) :
                width * (HasThemedArt ? .12f : .193f);
            rowHeight = HasJourneyPages ? preferredRow : Mathf.Max(preferredRow, 56 / Canvas.scaleFactor);
            journeyContent.sizeDelta = new Vector2(0, rowHeight * JourneyLevelCount);
            if (keepPosition && oldHeight > 0)
            {
                float oldTop = journeyContent.anchoredPosition.y;
                float hidden = Mathf.Max(0, journeyContent.rect.height - journeyViewportHeight);
                float centerRow = (oldTop + previousViewportHeight * .5f) / oldHeight;
                float newTop = centerRow * rowHeight - journeyViewportHeight * .5f;
                if (oldTop <= .5f)
                    newTop = 0;
                else if (oldTop >= oldHeight * JourneyLevelCount - previousViewportHeight - .5f)
                    newTop = hidden;
                journeyContent.anchoredPosition = Vector2.up * Mathf.Clamp(newTop, 0, hidden);
            }

            journeyRoute.gameObject.SetActive(!HasPaintedJourney && !HasVillageTerraces);
            for (int i = 0; i < journeyScenery.Count; i++)
            {
                var art = journeyScenery[i];
                art.gameObject.SetActive(!HasPaintedJourney);
                art.rectTransform.sizeDelta = new Vector2(width * .13f, rowHeight * 5.8f);
                art.rectTransform.anchoredPosition = new Vector2((i % 2 == 0 ? -.075f : .075f) * width, -(i * 5 + 2.65f) * rowHeight);
            }

            journeyCat.color = HasCrystalArt ? new Color(1f, .91f, .96f) : Color.white;
            firstBound = -1;
            LayoutCrystalJourney();
            if (useLegacyJourneyArt) LayoutThemedJourney();
            if (!journeyRunning)
                PlaceRestingCat();
            if (keepPosition)
                BindJourneyWindow();
        }

        void BindJourneyWindow()
        {
            if (!chapter.activeSelf || rowHeight <= 0)
                return;

            float top = Mathf.Max(0, journeyContent.anchoredPosition.y);
            int visibleGroup = Mathf.Clamp(Mathf.FloorToInt((top + journeyScroll.viewport.rect.height * .5f) / rowHeight) / 10, 0, 4);
            SelectedChapter = journeyRegion * TerraceCount + visibleGroup;
            detailPrevious.interactable = visibleGroup > 0;
            detailNext.interactable = visibleGroup < TerraceCount - 1;
            int groupFirst = progression.LevelId(journeyRegion, visibleGroup * LevelsPerTerrace);
            chapterTitle.text = L.V("map.chapter", L.Island(journeyRegion), groupFirst, groupFirst + 9);
            int first = Mathf.Clamp(Mathf.FloorToInt(top / rowHeight) - 1, 0, 49);
            int last = Mathf.Clamp(Mathf.CeilToInt((top + journeyScroll.viewport.rect.height) / rowHeight), first, 49);
            if (first == firstBound && last == lastBound)
                return;

            int capacity = Mathf.Min(JourneyLevelCount, Mathf.CeilToInt(journeyScroll.viewport.rect.height / rowHeight) + 3);
            if (chapterNodes.Count < capacity)
            {
                for (int i = chapterNodes.Count; i < capacity; i++)
                {
                    var point = Point(journeyContent, 0, 0, MapPointKind.LevelSlot, Vector2.up, node, 100);
                    var input = point.gameObject.AddComponent<JourneyLevelInput>();
                    input.Move = direction => SelectJourneySlot(point.slotIndex + direction);
                    SetJourneyNavigation(point.GetComponent<Button>(), null, null);
                    chapterNodes.Add(point);
                }
                if (useLegacyJourneyArt) journeyCat.transform.SetAsLastSibling();
                firstBound = -1;
            }

            float width = journeyScroll.viewport.rect.width;
            for (int i = 0; i < chapterNodes.Count; i++)
            {
                int slot = first + (i - first % chapterNodes.Count + chapterNodes.Count) % chapterNodes.Count;
                var point = chapterNodes[i];
                if (slot > last)
                {
                    point.gameObject.SetActive(false);
                    continue;
                }

                if (firstBound < 0 || !point.gameObject.activeSelf || point.slotIndex != slot || point.regionIndex != journeyRegion)
                {
                    // A recycled control must not retain keyboard focus on a different level.
                    if (EventSystem.current?.currentSelectedGameObject == point.gameObject)
                        journeyBack.Select();
                    point.regionIndex = journeyRegion;
                    point.slotIndex = slot;
                    var rect = (RectTransform)point.transform;
                    rect.anchoredPosition = JourneyNodePosition(slot);
                    bool milestone = slot == JourneyLevelCount - 1;
                    float stoneWidth = !useLegacyJourneyArt ? width * (milestone ? .16f : .135f) : HasPaintedJourney ? width * (milestone ? .205f : .145f) : 156;
                    float stoneHeight = !useLegacyJourneyArt ? rowHeight * .57f : HasPaintedJourney ? rowHeight * (milestone ? .88f : .72f) : 110;
                    if (HasVillageTerraces)
                    {
                        stoneWidth = width * .205f;
                        stoneHeight = rowHeight * .71f;
                    }
                    if (HasCrystalComposition)
                    {
                        stoneWidth = width * (milestone ? .177f : .146f);
                        stoneHeight = rowHeight * (milestone ? .64f : .57f);
                    }
                    rect.sizeDelta = new Vector2(Mathf.Max(stoneWidth, 48 / Canvas.scaleFactor),
                        Mathf.Max(stoneHeight, 48 / Canvas.scaleFactor));
                    int id = progression.LevelId(journeyRegion, slot);
                    var status = JourneyLockPending(id) ? MapLevelStatus.Locked : progression.Status(id);
                    point.GetComponent<MapNodeBadge>().PresentJourney(id, status, progression.Stars(id),
                        animateEnvironment && !reducedJourneyMotion, milestone, HasCrystalComposition);
                }

                point.gameObject.SetActive(true);
            }

            firstBound = first;
            lastBound = last;
            int firstVisible = Mathf.Clamp(Mathf.CeilToInt(top / rowHeight - .5f), first, last);
            SetJourneyNavigation(journeyBack, chapterNodes[firstVisible % chapterNodes.Count].GetComponent<Button>(), journeyFocus);
            SetJourneyNavigation(journeyFocus, chapterNodes[firstVisible % chapterNodes.Count].GetComponent<Button>(), journeyBack);
            var focusNavigation = journeyFocus.navigation;
            focusNavigation.selectOnRight = motionButton;
            journeyFocus.navigation = focusNavigation;
            motionButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = journeyFocus, selectOnLeft = journeyBack };
        }

        void SelectJourneySlot(int slot)
        {
            if (slot < 0 || slot >= JourneyLevelCount)
            {
                journeyBack.Select();
                return;
            }

            float top = journeyContent.anchoredPosition.y;
            float viewportHeight = journeyScroll.viewport.rect.height;
            bool outside = HasJourneyPages
                ? slot < firstBound || slot > lastBound ||
                    -JourneyNodePosition(slot).y < top + rowHeight * .4f ||
                    -JourneyNodePosition(slot).y > top + viewportHeight - rowHeight * .4f
                : slot * rowHeight < top || (slot + 1) * rowHeight > top + viewportHeight;
            if (outside)
                JumpToJourneySlot(slot);
            chapterNodes[slot % chapterNodes.Count].GetComponent<Button>().Select();
        }

        static void SetJourneyNavigation(Button button, Selectable up, Selectable neighbor)
        {
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnLeft = neighbor,
                selectOnRight = neighbor
            };
        }
    }
    public sealed class JourneyLevelInput : MonoBehaviour, IMoveHandler
    {
        public System.Action<int> Move;

        public void OnMove(AxisEventData data)
        {
            if (data.moveDir == MoveDirection.Up)
                Move?.Invoke(-1);
            else if (data.moveDir == MoveDirection.Down)
                Move?.Invoke(1);
        }
    }
}
