using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Characters
{
    // A small reusable mesh, not a new raster asset or a character redraw.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WizardSparkGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var c = r.center;
            var bright = (Color32)color;
            var edge = new Color(color.r, color.g, color.b, 0);
            vh.AddVert(c, bright, Vector2.zero);
            for (int i=0;i<8;i++)
            {
                float angle = Mathf.PI*.25f*i;
                float radius = i%2==0 ? 1f : .16f;
                vh.AddVert(c + new Vector2(Mathf.Cos(angle)*r.width*.5f,Mathf.Sin(angle)*r.height*.5f)*radius, edge, Vector2.zero);
            }
            for(int i=0;i<8;i++) vh.AddTriangle(0,i+1,(i+1)%8+1);
        }
    }
}
