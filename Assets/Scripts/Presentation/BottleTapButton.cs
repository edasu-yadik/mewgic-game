using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mewgic
{
    public sealed class BottleTapButton : Button
    {
        internal Func<bool> AcceptTap;
        int pointer;
        Vector2 origin;
        bool armed;

        public override void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && IsInteractable())
            {
                armed = true;
                pointer = data.pointerId;
                origin = data.position;
            }
            base.OnPointerDown(data);
        }

        public override void OnPointerClick(PointerEventData data)
        {
            if (!armed || pointer != data.pointerId)
                return;
            armed = false;
            if (!data.dragging && (data.position - origin).sqrMagnitude <= 144 && IsInteractable() &&
                (AcceptTap?.Invoke() ?? false))
                base.OnPointerClick(data);
        }

        public override void OnPointerUp(PointerEventData data)
        {
            base.OnPointerUp(data);
            if (pointer == data.pointerId && (!data.eligibleForClick ||
                data.pointerCurrentRaycast.gameObject == null ||
                !data.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform)))
                armed = false;
        }

        protected override void OnDisable()
        {
            armed = false;
            base.OnDisable();
        }
    }
}
