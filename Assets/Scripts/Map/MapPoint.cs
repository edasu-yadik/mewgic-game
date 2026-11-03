using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public enum MapPointKind { LevelSlot, Chest, LevelGroup }

    public sealed class MapPoint : MonoBehaviour
    {
        public MapPointKind kind;
        public int regionIndex;
        public int slotIndex;
    }

    public sealed class MapScrollRect : ScrollRect
    {
        public Action UserInput;
        internal int GestureVersion { get; private set; }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                GestureVersion++;
                UserInput?.Invoke();
            }
            base.OnBeginDrag(eventData);
        }

        public override void OnScroll(PointerEventData eventData)
        {
            GestureVersion++;
            UserInput?.Invoke();
            base.OnScroll(eventData);
        }
    }

    // A virtualized button may represent a different level when a finger is released.
    public sealed class MapPointButton : Button
    {
        internal MapPoint Point;
        internal MapScrollRect Scroller;
        bool armed;
        int pointer, region, slot, gesture;
        Vector2 pressPosition, contentPosition;

        public override void OnPointerDown(PointerEventData eventData)
        {
            armed = eventData.button == PointerEventData.InputButton.Left && IsInteractable();
            if (armed)
            {
                pointer = eventData.pointerId;
                region = Point.regionIndex;
                slot = Point.slotIndex;
                gesture = Scroller.GestureVersion;
                pressPosition = eventData.position;
                contentPosition = Scroller.content.anchoredPosition;
            }
            base.OnPointerDown(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            bool valid = armed && !eventData.dragging && eventData.pointerId == pointer &&
                region == Point.regionIndex && slot == Point.slotIndex && gesture == Scroller.GestureVersion &&
                (eventData.position - pressPosition).sqrMagnitude <= 100 &&
                (Scroller.content.anchoredPosition - contentPosition).sqrMagnitude <= 4;
            armed = false;
            if (valid) base.OnPointerClick(eventData);
        }

        protected override void OnDisable()
        {
            armed = false;
            base.OnDisable();
        }
    }
}
