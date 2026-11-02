using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class FrostSnowGraphic : MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper vh)
  {
   vh.Clear(); const int segments = 8;
   Vector2 center = rectTransform.rect.center; float radius = rectTransform.rect.width * .5f;
   vh.AddVert(center, color, Vector2.zero); Color edge = color; edge.a = 0;
   for (int i = 0; i < segments; i++)
   {
    float a = i * Mathf.PI * 2 / segments;
    vh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, edge, Vector2.zero);
   }
   for (int i = 0; i < segments; i++) vh.AddTriangle(0, i + 1, (i + 1) % segments + 1);
  }
 }
}
