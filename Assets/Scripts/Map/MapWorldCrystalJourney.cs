using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed class CrystalTerraceImage : RawImage
    {
        static readonly float[] Rows = { 0, .04f, .12f, .88f, .96f, 1 };
        public float Entrance = .5f;
        public float Exit = .5f;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            foreach (float row in Rows)
            {
                float offset = row <= .04f ? Exit - .5f : row >= .96f ? Entrance - .5f : 0;
                float y = Mathf.Lerp(bounds.yMin, bounds.yMax, row);
                mesh.AddVert(new Vector2(bounds.xMin, y), color, new Vector2(offset, row));
                mesh.AddVert(new Vector2(bounds.xMax, y), color, new Vector2(1 + offset, row));
            }
            for (int i = 0; i < (Rows.Length - 1) * 2; i += 2)
            {
                mesh.AddTriangle(i, i + 2, i + 1);
                mesh.AddTriangle(i + 1, i + 2, i + 3);
            }
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneySoftDisc : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            mesh.AddVert(Vector2.zero, color, Vector2.zero);
            var radius = rectTransform.rect.size * .5f;
            var edge = new Color(color.r, color.g, color.b, 0);
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI / 12;
                mesh.AddVert(new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y), edge, Vector2.zero);
                mesh.AddTriangle(0, i + 1, (i + 1) % 24 + 1);
            }
        }
    }
}
