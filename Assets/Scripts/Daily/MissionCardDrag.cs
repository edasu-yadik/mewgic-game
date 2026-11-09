using UnityEngine;
using UnityEngine.EventSystems;

namespace Mewgic.Daily
{
    internal sealed class MissionCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        RectTransform card, track;
        float offset, destination, velocity, grab;
        bool dragging, moving, rightDock;
        int pointer;

        internal void Bind(RectTransform card, RectTransform track)
        {
            this.card = card;
            this.track = track;
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (dragging || data.button != PointerEventData.InputButton.Left) return;
            dragging = moving = true;
            pointer = data.pointerId;
            grab = TrackX(data)-offset;
            data.eligibleForClick = false;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!dragging || pointer != data.pointerId) return;
            destination = Mathf.Clamp(TrackX(data)-grab,0,TrackWidth);
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (!dragging || pointer != data.pointerId) return;
            dragging = false;
            rightDock = destination >= TrackWidth*.5f;
            destination = rightDock ? TrackWidth : 0;
        }

        void OnDisable()
        {
            dragging = moving = false;
            velocity = 0;
            if (card != null) Place(Mathf.Clamp(destination,0,TrackWidth));
        }

        float TrackWidth => Mathf.Max(0,track.rect.width*.964f-card.rect.width);
        float TrackX(PointerEventData data)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(track,data.position,data.pressEventCamera,out var point);
            return point.x;
        }

        void LateUpdate()
        {
            if (!moving)
            {
                if (rightDock && offset != TrackWidth) Place(TrackWidth);
                return;
            }
            if (!dragging) destination = rightDock ? TrackWidth : 0;
            destination = Mathf.Clamp(destination,0,TrackWidth);
            Place(GameFeel.Reduced ? destination : Mathf.SmoothDamp(offset,destination,ref velocity,
                dragging ? .045f : .14f, Mathf.Infinity, Time.unscaledDeltaTime));
            if (!dragging && Mathf.Abs(offset-destination) < .2f)
            {
                Place(destination);
                moving = false;
            }
        }

        void Place(float value)
        {
            offset = value;
            card.anchoredPosition = new Vector2(value,0);
        }
    }
}
