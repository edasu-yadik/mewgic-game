using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
    // Küçük yumuşak altın ışık; tam ekran parçacık dokusu veya ParticleSystem gerektirmez.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoodsFireflyGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 c = rectTransform.rect.center;
            float radius = rectTransform.rect.width * .5f;
            vh.AddVert(c, color, Vector2.zero);
            Color clear = color; clear.a = 0;
            const int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                vh.AddVert(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, clear, Vector2.zero);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(0, i + 1, (i + 1) % segments + 1);
        }
    }
}
