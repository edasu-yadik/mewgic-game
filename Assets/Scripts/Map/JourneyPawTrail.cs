using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    // One pooled UI mesh: no textures, particles, per-step objects or per-frame allocation.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyPawTrail : MaskableGraphic
    {
        readonly Vector2[] positions = new Vector2[12], directions = new Vector2[12];
        readonly float[] sizes = new float[12], alphas = new float[12];
        int count;
        public void Clear() { count = 0; SetVerticesDirty(); }
        public void Set(int index, Vector2 position, Vector2 direction, float size, float alpha)
        {
            positions[index] = position;
            directions[index] = direction.sqrMagnitude > .0001f ? direction.normalized : Vector2.down;
            sizes[index] = size;
            alphas[index] = alpha;
        }
        public void Render(int steps) { count = Mathf.Clamp(steps, 0, positions.Length); SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            for (int i = 0; i < count; i++)
            {
                if (alphas[i] < .005f) continue;
                var gold = new Color(1, .72f, .20f, alphas[i] * .24f);
                Oval(mesh, i, new Vector2(0, -.04f), new Vector2(.70f, .84f), gold, true);
                gold = new Color(1, .91f, .58f, alphas[i]);
                Oval(mesh, i, new Vector2(0, -.18f), new Vector2(.31f, .28f), gold);
                Oval(mesh, i, new Vector2(-.37f, .15f), new Vector2(.13f, .18f), gold);
                Oval(mesh, i, new Vector2(-.14f, .39f), new Vector2(.14f, .19f), gold);
                Oval(mesh, i, new Vector2(.14f, .39f), new Vector2(.14f, .19f), gold);
                Oval(mesh, i, new Vector2(.37f, .15f), new Vector2(.13f, .18f), gold);
            }
        }
        void Oval(VertexHelper mesh, int paw, Vector2 center, Vector2 radius, Color tint, bool soft = false)
        {
            int first = mesh.currentVertCount;
            Vector2 forward = directions[paw], right = new Vector2(forward.y, -forward.x);
            var bounds = rectTransform.rect;
            Vector2 position = new Vector2(bounds.xMin, bounds.yMax) + positions[paw] +
                (right * center.x + forward * center.y) * sizes[paw];
            mesh.AddVert(position, tint, Vector2.zero);
            for (int n = 0; n <= 16; n++)
            {
                float angle = n * Mathf.PI / 8;
                var edge = position + (right * Mathf.Cos(angle) * radius.x + forward * Mathf.Sin(angle) * radius.y) * sizes[paw];
                mesh.AddVert(edge, soft ? new Color(tint.r, tint.g, tint.b, 0) : tint, Vector2.zero);
                if (n > 0) mesh.AddTriangle(first, first + n, first + n + 1);
            }
        }
    }
}
