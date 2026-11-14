using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    // Connects the painted terrace exit to the next entrance in the scrolling track.
    public sealed class MapRoadJoinGraphic : RawImage
    {
        Color startTint = Color.white, endTint = Color.white;
        static readonly float[] Widths = { -1, -.87f, -.76f, .76f, .87f, 1 };

        internal void SetTints(Color start, Color end)
        {
            if (startTint == start && endTint == end) return;
            startTint = start;
            endTint = end;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (texture == null) return;
            const int steps = 32;
            Rect box = rectTransform.rect;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 route = ContinuousMapRoute.Join(t);
                float y = box.yMax - route.y * box.height;
                // Sample a paved diagonal from the painted lower landing. Keep the UV basis
                // transverse to that diagonal so the stone pattern keeps its proportions.
                float sourceX = Mathf.Lerp(.218f, .452f, t);
                float sourceY = Mathf.Lerp(.839f, .813f, t);
                float width = Mathf.Lerp(.119f, .104f, t);
                Color tint = Color.Lerp(startTint, endTint, t);
                float seamFeather = Mathf.SmoothStep(0, 1, Mathf.Min(t, 1 - t) * 24);
                for (int j = 0; j < Widths.Length; j++)
                {
                    Color vertex = tint * (j is 2 or 3 ? new Color(.98f,.95f,.91f) : new Color(.67f,.55f,.62f));
                    vertex.a = j is 0 or 5 ? 0 : seamFeather;
                    mesh.AddVert(new Vector2(box.xMin + (route.x + Widths[j] * width) * box.width, y), vertex,
                        new Vector2(sourceX + Widths[j] * .025f, 1 - sourceY + Widths[j] * .030f));
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
