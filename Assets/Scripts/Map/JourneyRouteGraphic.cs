using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyRouteGraphic : MaskableGraphic
    {
        internal static float Lane(int slot) => Lane((float)slot);
        static float Lane(float slot) => .50f + .245f * Mathf.Sin(slot * .82f - 1.05f);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = rectTransform.rect;
            float row = bounds.height / 50;
            float width = Mathf.Min(bounds.width * .044f, row * .26f);
            for (int step = 0; step < 400; step++)
            {
                float a = step / 8f - .5f;
                float b = a + .125f;
                Vector2 start = new Vector2(bounds.xMin + Lane(a) * bounds.width, bounds.yMax - (a + .5f) * row);
                Vector2 end = new Vector2(bounds.xMin + Lane(b) * bounds.width, bounds.yMax - (b + .5f) * row);
                Vector2 normal = Vector2.Perpendicular((end - start).normalized);
                bool bridge = step % 40 >= 30 || step % 40 < 4;

                Strip(mesh, start, end, normal * (width + 6), new Color32(26, 25, 34, 230));
                Strip(mesh, start, end, normal * width,
                    bridge ? new Color32(102, 101, 113, 255) : new Color32(114, 80, 56, 255));
                Color32 stone = bridge
                    ? new Color32((byte)(158 + step % 3 * 9), 150, 149, 255)
                    : new Color32((byte)(193 + step % 3 * 10), 156, 105, 255);
                Strip(mesh, Vector2.Lerp(start, end, .10f), Vector2.Lerp(start, end, .90f), normal * (width - 5), stone);
                if (bridge)
                {
                    var edge = normal * (width + 2);
                    var rail = new Color32(207, 188, 162, 255);
                    Strip(mesh, start + edge, end + edge, normal * 4, rail);
                    Strip(mesh, start - edge, end - edge, normal * 4, rail);
                }
            }
        }

        static void Strip(VertexHelper mesh, Vector2 start, Vector2 end, Vector2 normal, Color32 color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(start - normal, color, Vector2.zero);
            mesh.AddVert(end - normal, color, Vector2.zero);
            mesh.AddVert(end + normal, color, Vector2.zero);
            mesh.AddVert(start + normal, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyStoneGraphic : MaskableGraphic
    {
        bool sculpted, milestone;
        public void Configure(bool crystal, bool landmark)
        {
            if (sculpted == crystal && milestone == landmark) return;
            sculpted = crystal;
            milestone = landmark;
            SetVerticesDirty();
        }

        internal static readonly Vector2[] Outline = Dome();
        static Vector2[] Dome()
        {
            var points = new Vector2[27];
            points[0] = new Vector2(-.49f, -.31f);
            points[1] = new Vector2(-.37f, -.43f);
            points[2] = new Vector2(.37f, -.43f);
            points[3] = new Vector2(.49f, -.31f);
            for (int i = 0; i < 23; i++)
            {
                float angle = i * Mathf.PI / 22;
                points[i + 4] = new Vector2(Mathf.Cos(angle) * .49f, -.20f + Mathf.Sin(angle) * .69f);
            }
            return points;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sculpted)
            {
                var face = milestone ? new Color32(115, 36, 155, 255) : new Color32(88, 65, 98, 255);
                Facet(mesh, new Vector2(1.05f, 1.03f), new Vector2(0, -10), new Color32(18, 10, 26, 195));
                Facet(mesh, Vector2.one, new Vector2(0, -6), new Color32(96, 60, 32, 255));
                Facet(mesh, Vector2.one, Vector2.zero, color);
                Facet(mesh, new Vector2(.90f, .85f), new Vector2(0, 2), face);
                Facet(mesh, new Vector2(.83f, .78f), new Vector2(0, 4), milestone ? new Color32(94, 24, 137, 255) : new Color32(69, 49, 78, 255));
                return;
            }

            Facet(mesh, Vector2.one, new Vector2(0, -9), new Color32(13, 18, 29, 255));
            Facet(mesh, Vector2.one, Vector2.zero, color);
            Facet(mesh, new Vector2(.91f, .87f), new Vector2(0, 2), new Color32(56, 57, 67, 255));
            Facet(mesh, new Vector2(.80f, .76f), new Vector2(0, 5), new Color32(27, 33, 46, 255));
        }

        void Facet(VertexHelper mesh, Vector2 scale, Vector2 offset, Color32 tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(offset, sculpted ? Color32.Lerp(tint, new Color32(255, 232, 192, tint.a), .12f) : tint, Vector2.zero);
            var size = Vector2.Scale(rectTransform.rect.size, scale);
            for (int i = 0; i < Outline.Length; i++)
            {
                Color32 edge = sculpted
                    ? Color32.Lerp(new Color32(39, 19, 59, tint.a), tint, Mathf.Clamp01(Outline[i].y + .59f))
                    : tint;
                mesh.AddVert(Vector2.Scale(Outline[i], size) + offset, edge, Vector2.zero);
            }
            for (int i = 0; i < Outline.Length; i++)
                mesh.AddTriangle(index, index + i + 1, index + (i + 1) % Outline.Length + 1);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyStoneRing : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var outline = JourneyStoneGraphic.Outline;
            var size = rectTransform.rect.size;
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 edge = Vector2.Scale(outline[i], size);
                mesh.AddVert(edge * .85f, new Color32(148, 67, 240, 0), Vector2.zero);
                mesh.AddVert(edge * .98f, new Color32(255, 216, 117, 245), Vector2.zero);
                mesh.AddVert(edge * 1.035f, new Color32(190, 98, 247, 168), Vector2.zero);
                mesh.AddVert(edge * 1.115f, new Color32(175, 76, 242, 0), Vector2.zero);
            }

            for (int i = 0; i < outline.Length; i++)
            {
                int start = i * 4;
                int next = (i + 1) % outline.Length * 4;
                for (int band = 0; band < 3; band++)
                {
                    mesh.AddTriangle(start + band, start + band + 1, next + band);
                    mesh.AddTriangle(next + band, start + band + 1, next + band + 1);
                }
            }
        }
    }
}
