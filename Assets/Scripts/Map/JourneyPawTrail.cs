using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    // One pooled UI mesh: no textures, particles, per-step objects or per-frame allocation.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyPawTrail : MaskableGraphic
    {
        internal const int Capacity = 28;
        readonly Vector2[] positions = new Vector2[Capacity], directions = new Vector2[Capacity];
        readonly float[] sizes = new float[Capacity], alphas = new float[Capacity], lights = new float[Capacity];
        int count;
        internal int VisibleSteps => count;
        public void Clear() { count = 0; SetVerticesDirty(); }
        public void Set(int index, Vector2 position, Vector2 direction, float size, float alpha, float light)
        {
            positions[index] = position;
            directions[index] = direction.sqrMagnitude > .0001f ? direction.normalized : Vector2.down;
            sizes[index] = size;
            alphas[index] = alpha;
            lights[index] = light;
        }
        public void Render(int steps) { count = Mathf.Clamp(steps, 0, positions.Length); SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            for (int i = 0; i < count; i++)
            {
                if (alphas[i] < .005f) continue;
                float alpha = alphas[i];
                var glow = new Color(1, .56f, .08f, alpha * (.30f + lights[i] * .44f));
                Oval(mesh, i, Vector2.zero, new Vector2(.92f, 1f), glow, true);
                Pad(mesh, i, new Vector2(0, -.18f), new Vector2(.37f, .32f), alpha, mainPad: true);
                Pad(mesh, i, new Vector2(-.39f, .16f), new Vector2(.145f, .17f), alpha);
                Pad(mesh, i, new Vector2(-.15f, .39f), new Vector2(.134f, .17f), alpha);
                Pad(mesh, i, new Vector2(.15f, .39f), new Vector2(.134f, .17f), alpha);
                Pad(mesh, i, new Vector2(.39f, .16f), new Vector2(.145f, .17f), alpha);
                if (lights[i] > .78f)
                    Oval(mesh, i, new Vector2(.64f, .43f), new Vector2(.11f, .13f),
                        new Color(1, .90f, .49f, alpha * lights[i]), true);
            }
        }

        void Pad(VertexHelper mesh, int paw, Vector2 center, Vector2 radius, float alpha, bool mainPad = false)
        {
            var edge = new Color(1, .68f, .15f, alpha);
            Oval(mesh, paw, center, radius, edge, mainPad: mainPad,
                fill: new Color(1, .93f, Mathf.Lerp(.40f, .69f, lights[paw]), alpha));
        }

        void Oval(VertexHelper mesh, int paw, Vector2 center, Vector2 radius, Color tint,
            bool soft = false, bool mainPad = false, Color? fill = null)
        {
            int first = mesh.currentVertCount;
            Vector2 forward = directions[paw], right = new Vector2(forward.y, -forward.x);
            var bounds = rectTransform.rect;
            Vector2 position = new Vector2(bounds.xMin, bounds.yMax) + positions[paw] +
                (right * center.x + forward * center.y) * sizes[paw];
            mesh.AddVert(position, fill ?? tint, Vector2.zero);
            for (int n = 0; n <= 16; n++)
            {
                float angle = n * Mathf.PI / 8;
                float y = Mathf.Sin(angle);
                float profile = mainPad ? 1 - .25f * y : 1;
                var edge = position + (right * Mathf.Cos(angle) * radius.x * profile + forward * y * radius.y) * sizes[paw];
                mesh.AddVert(edge, soft ? new Color(tint.r, tint.g, tint.b, 0) : tint, Vector2.zero);
                if (n > 0) mesh.AddTriangle(first, first + n, first + n + 1);
            }
        }
    }
}
