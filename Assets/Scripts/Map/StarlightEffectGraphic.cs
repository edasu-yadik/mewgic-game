using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class StarlightEffectGraphic : MaskableGraphic
 {
  public bool halo;
  protected override void OnPopulateMesh(VertexHelper vh)
  {
   vh.Clear(); int count = halo ? 32 : 8;
   Vector2 center = rectTransform.rect.center, radius = rectTransform.rect.size * .5f;
   vh.AddVert(center, color, Vector2.zero); Color edge = color; edge.a = 0;
   for (int i = 0; i < count; i++)
   {
    float a = i * Mathf.PI * 2 / count, r = halo || i % 2 == 0 ? 1 : .22f;
    vh.AddVert(center + new Vector2(Mathf.Cos(a) * radius.x, Mathf.Sin(a) * radius.y) * r, edge, Vector2.zero);
   }
   for (int i = 0; i < count; i++) vh.AddTriangle(0, i + 1, (i + 1) % count + 1);
  }
 }
}
