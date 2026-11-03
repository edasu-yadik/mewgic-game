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
        Button journeyBack, journeyFocus;
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
            journeyContent = Rect(scroll, "JourneyContent", Vector2.up, Vector2.one);
            journeyContent.pivot = new Vector2(.5f, 1);
            journeyScroll.content = journeyContent;

            for (int i = 0; i < 10; i++)
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
            BuildThemedJourney();

            var cat = Rect(journeyContent, "WizardCatGuide", Vector2.up, Vector2.up);
            catImage = cat.gameObject.AddComponent<JourneyCatImage>();
            journeyCat = catImage;
            journeyCat.texture = journeyMascot;
            journeyCat.raycastTarget = false;
            journeyCat.enabled = journeyMascot != null;

            var heading = Art(overlay, "JourneyHeading", null, new Vector2(0, .87f), Vector2.one);
            heading.color = new Color32(23, 20, 40, 255);
            Art(heading.transform, "GoldEdge", null, Vector2.zero, new Vector2(1, .013f)).color = new Color32(159, 115, 58, 255);
            chapterTitle = Label(heading.transform, "ChapterTitle", "", 36, new Vector2(.04f, .34f), new Vector2(.96f, .95f));
            Label(heading.transform, "ChapterInstruction", L.V("map.choose"), 24,
                new Vector2(.05f, .03f), new Vector2(.70f, .35f));
            BuildJourneyMotion(heading.rectTransform);

            var footer = Art(overlay, "JourneyFooter", null, Vector2.zero, new Vector2(1, .105f));
            footer.color = heading.color;
            journeyBack = Control(footer.transform, "ChapterClose", L.V("map.return"),
                new Vector2(.07f, .12f), new Vector2(.60f, .88f), CloseJourney);
            journeyFocus = Control(footer.transform, "JourneyFocus", L.V("map.focus"),
                new Vector2(.63f, .12f), new Vector2(.93f, .88f), FocusJourneyCurrent);
            journeyScroll.onValueChanged.AddListener(_ => BindJourneyWindow());
            chapter.SetActive(false);
        }

        public void OpenChapter(int region, int group)
        {
            FinishJourney(false);
            ShowChapter(region, group, -1, true);
        }

        void ShowChapter(int region, int group, int focusSlot, bool saveFocus)
        {
            if (navigating || progression == null || region < 0 || region > (int.MaxValue - 50) / 50 || group < 0 || group > 4)
                return;

            journeyFocusSlot = -1;
            journeyRegion = region;
            LoadThemedJourney(region);
            SelectedChapter = region * 5 + group;
            int first = progression.LevelId(region, 0);
            chapterTitle.text = L.V("map.chapter", L.Island(region), first, progression.LevelId(region, 49));
            Scroll.StopMovement();
            jumping = false;
            pendingFocus = -1;
            if (saveFocus) ledger.SaveMapFocus(first);

            chapter.SetActive(true);
            chapter.transform.SetAsLastSibling();
            UnityEngine.Canvas.ForceUpdateCanvases();
            ResizeJourney(false);
            int current = CurrentJourneySlot();
            JumpToJourneySlot(focusSlot >= 0 ? focusSlot : current / 10 == group ? current : group * 10);
            journeyBack.Select();
        }

        void CloseJourney()
        {
            FinishJourney(false);
            chapter.SetActive(false);
            ReleaseThemedJourney();
            var island = views.Find(view => view.region == journeyRegion);
            if (island != null)
                island.groups[0].GetComponent<Button>().Select();
        }

        void CancelJourneyFocus()
        {
            journeyFocusSlot = -1;
            FinishJourney(false);
        }

        void FocusJourneyCurrent()
        {
            CancelJourneyFocus();
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
            rowHeight = Mathf.Max(width * (HasCrystalArt ? .15f : HasThemedArt ? .12f : .193f), 56 / Canvas.scaleFactor);
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

            journeyRoute.gameObject.SetActive(!HasPaintedJourney);
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
            LayoutThemedJourney();
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
                journeyCat.transform.SetAsLastSibling();
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
                    rect.anchoredPosition = new Vector2(width * JourneyLane(slot), -(slot + .5f) * rowHeight);
                    bool milestone = slot == JourneyLevelCount - 1;
                    float stoneWidth = HasPaintedJourney ? width * (milestone ? .205f : .145f) : 156;
                    float stoneHeight = HasPaintedJourney ? rowHeight * (milestone ? .88f : .72f) : 110;
                    rect.sizeDelta = new Vector2(Mathf.Max(stoneWidth, 48 / Canvas.scaleFactor),
                        Mathf.Max(stoneHeight, 48 / Canvas.scaleFactor));
                    int id = progression.LevelId(journeyRegion, slot);
                    var status = JourneyLockPending(id) ? MapLevelStatus.Locked : progression.Status(id);
                    point.GetComponent<MapNodeBadge>().PresentJourney(id, status, progression.Stars(id),
                        animateEnvironment && !reducedJourneyMotion, milestone);
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
            if (slot * rowHeight < top || (slot + 1) * rowHeight > top + viewportHeight)
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
