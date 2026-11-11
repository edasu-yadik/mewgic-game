using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    // Connects the painted terrace exit to the next entrance in the scrolling track.
    public sealed class MapRoadJoinGraphic : RawImage
    {
        Color tint = Color.white;
        static readonly float[] Widths = { -.109f, -.092f, -.076f, .076f, .092f, .109f };

        internal static float Center(float t) => Mathf.Lerp(.206f, .26f, t) - .034f * Mathf.Sin(t * Mathf.PI);

        internal void SetTint(Color value)
        {
            if (tint == value) return;
            tint = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (texture == null) return;
            const int steps = 18;
            Rect box = rectTransform.rect;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float center = Center(t);
                float y = box.yMax - Mathf.Lerp(MapWorld.Band(.857f), 1 + MapWorld.Band(.239f), t) * box.height;
                float sourceX = Mathf.Lerp(.235f, .477f, t);
                float sourceY = Mathf.Lerp(.837f, .808f, t);
                float opacity = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t * 5)) *
                    Mathf.SmoothStep(0, 1, Mathf.Clamp01((1 - t) * 5));
                for (int j = 0; j < Widths.Length; j++)
                {
                    Color vertex = tint * (j is 2 or 3 ? new Color(.98f,.95f,.91f) : new Color(.79f,.68f,.65f));
                    vertex.a = j is 0 or 5 ? 0 : opacity;
                    mesh.AddVert(new Vector2(box.xMin + (center + Widths[j]) * box.width, y), vertex,
                        new Vector2(sourceX, 1 - sourceY + Widths[j] * .18f));
                }
                if (i == 0) continue;
                for (int j = 0; j < Widths.Length - 1; j++)
                {
                    int a = (i - 1) * Widths.Length + j, b = a + Widths.Length;
                    mesh.AddTriangle(a, a + 1, b + 1);
                    mesh.AddTriangle(a, b + 1, b);
                }
            }
        }
    }
}
