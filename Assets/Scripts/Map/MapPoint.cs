using System;using UnityEngine;using UnityEngine.EventSystems;using UnityEngine.UI;
namespace Mewgic.Map
{
    public enum MapPointKind {LevelSlot,Chest,LevelGroup}
    public sealed class MapPoint : MonoBehaviour
    {public MapPointKind kind;public int regionIndex;public int slotIndex;}
    public sealed class MapScrollRect : ScrollRect
    {
        public Action UserInput;
        public override void OnBeginDrag(PointerEventData e){UserInput?.Invoke();base.OnBeginDrag(e);}
        public override void OnScroll(PointerEventData e){UserInput?.Invoke();base.OnScroll(e);}
    }
}
