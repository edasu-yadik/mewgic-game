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
        float rowHeight, journeyViewportHeight;

        void BuildChapter()
        {
            var overlay = Rect(SafeArea, "ChapterSelection", Vector2.zero, Vector2.one);
            chapter = overlay.gameObject;
            var dim = overlay.gameObject.AddComponent<Image>();
            dim.color = new Color32(9, 19, 36, 255);
            var clouds = Art(overlay, "JourneySky", sky, Vector2.zero, Vector2.one);
            clouds.color = new Color(.57f, .70f, .82f);

            var scroll = Rect(overlay, "JourneyScroll", new Vector2(0, .105f), new Vector2(1, .87f));
            journeyScroll = scroll.gameObject.AddComponent<ScrollRect>();
            journeyScroll.horizontal = false;
            journeyScroll.movementType = ScrollRect.MovementType.Clamped;
            journeyScroll.scrollSensitivity = 70;
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
            route.gameObject.AddComponent<JourneyRouteGraphic>().raycastTarget = false;

            var cat = Rect(journeyContent, "WizardCatGuide", Vector2.up, Vector2.up);
            journeyCat = cat.gameObject.AddComponent<RawImage>();
            journeyCat.texture = journeyMascot;
            journeyCat.raycastTarget = false;
            journeyCat.enabled = journeyMascot != null;

            var heading = Art(overlay, "JourneyHeading", null, new Vector2(0, .87f), Vector2.one);
            heading.color = new Color32(12, 26, 44, 255);
            chapterTitle = Label(heading.transform, "ChapterTitle", "", 36, new Vector2(.04f, .34f), new Vector2(.96f, .95f));
            Label(heading.transform, "ChapterInstruction", L.V("map.choose"), 24,
                new Vector2(.05f, .03f), new Vector2(.95f, .35f));

            var footer = Art(overlay, "JourneyFooter", null, Vector2.zero, new Vector2(1, .105f));
            footer.color = heading.color;
            journeyBack = Control(footer.transform, "ChapterClose", L.V("map.return"),
                new Vector2(.07f, .12f), new Vector2(.60f, .88f), CloseJourney);
            journeyFocus = Control(footer.transform, "JourneyFocus", L.V("map.focus"),
                new Vector2(.63f, .12f), new Vector2(.93f, .88f), () => JumpToJourneySlot(CurrentJourneySlot()));
            journeyScroll.onValueChanged.AddListener(_ => BindJourneyWindow());
            chapter.SetActive(false);
        }

        public void OpenChapter(int region, int group)
        {
            if (navigating || progression == null || region < 0 || region > (int.MaxValue - 50) / 50 || group < 0 || group > 4)
                return;

            journeyRegion = region;
            SelectedChapter = region * 5 + group;
            int first = progression.LevelId(region, 0);
            chapterTitle.text = L.V("map.chapter", L.Island(region), first, progression.LevelId(region, 49));
            Scroll.StopMovement();
            jumping = false;
            pendingFocus = -1;
            ledger.SaveMapFocus(first);

            chapter.SetActive(true);
            chapter.transform.SetAsLastSibling();
            UnityEngine.Canvas.ForceUpdateCanvases();
            ResizeJourney(false);
            int current = CurrentJourneySlot();
            JumpToJourneySlot(current / 10 == group ? current : group * 10);
            journeyBack.Select();
        }

        void CloseJourney()
        {
            chapter.SetActive(false);
            var island = views.Find(view => view.region == journeyRegion);
            if (island != null)
                island.groups[0].GetComponent<Button>().Select();
        }

        int CurrentJourneySlot() => Mathf.Clamp(progression.CurrentId - progression.LevelId(journeyRegion, 0), 0, 49);

        void JumpToJourneySlot(int slot)
        {
            journeyScroll.StopMovement();
            float viewportHeight = journeyScroll.viewport.rect.height;
            journeyContent.anchoredPosition = Vector2.up * Mathf.Clamp((slot + .5f) * rowHeight - viewportHeight * .5f,
                0, Mathf.Max(0, journeyContent.rect.height - viewportHeight));
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
            rowHeight = Mathf.Max(width * .193f, 56 / Canvas.scaleFactor);
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

            for (int i = 0; i < journeyScenery.Count; i++)
            {
                var art = journeyScenery[i];
                art.rectTransform.sizeDelta = new Vector2(width * .13f, rowHeight * 5.8f);
                art.rectTransform.anchoredPosition = new Vector2((i % 2 == 0 ? -.075f : .075f) * width, -(i * 5 + 2.65f) * rowHeight);
            }

            var catSize = Mathf.Min(width * .235f, rowHeight * 1.25f);
            journeyCat.rectTransform.sizeDelta = journeyMascot == null ? Vector2.zero :
                new Vector2(catSize * journeyMascot.width / journeyMascot.height, catSize);
            int catSlot = CurrentJourneySlot();
            float catX = JourneyRouteGraphic.Lane(catSlot) > .5f ? .24f : .76f;
            journeyCat.rectTransform.anchoredPosition = new Vector2(width * catX, -(catSlot + .52f) * rowHeight);
            firstBound = -1;
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
                    rect.anchoredPosition = new Vector2(width * JourneyRouteGraphic.Lane(slot), -(slot + .5f) * rowHeight);
                    rect.sizeDelta = new Vector2(Mathf.Max(156, 48 / Canvas.scaleFactor), Mathf.Max(110, 48 / Canvas.scaleFactor));
                    int id = progression.LevelId(journeyRegion, slot);
                    point.GetComponent<MapNodeBadge>().PresentJourney(id, progression.Status(id), progression.Stars(id), animateEnvironment);
                }

                point.gameObject.SetActive(true);
            }

            firstBound = first;
            lastBound = last;
            int firstVisible = Mathf.Clamp(Mathf.CeilToInt(top / rowHeight - .5f), first, last);
            SetJourneyNavigation(journeyBack, chapterNodes[firstVisible % chapterNodes.Count].GetComponent<Button>(), journeyFocus);
            SetJourneyNavigation(journeyFocus, chapterNodes[firstVisible % chapterNodes.Count].GetComponent<Button>(), journeyBack);
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