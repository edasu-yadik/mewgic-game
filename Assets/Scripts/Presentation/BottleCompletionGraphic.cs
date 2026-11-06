using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    // Her şişede yeniden kullanılan tek UI mesh'i; sıvının orta alanını kapatmaz.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BottleCompletionGraphic : MaskableGraphic
    {
        [SerializeField, Range(.6f, 1.5f)] float duration = 1.05f;
        float age;
        bool paused;
        public bool Playing { get; private set; }
        public int PlayCount { get; private set; }
        public void Play() { age = 0; PlayCount++; Playing = true; enabled = true; SetVerticesDirty(); }
        public void Stop() { Playing = false; enabled = false; }
        public void SetPaused(bool value) => paused = value;
        void Update()
        {
            if (!Playing || paused) return;
            age += Time.unscaledDeltaTime;
            if (age >= (GameFeel.Reduced ? .35f : duration)) { Stop(); return; }
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (!Playing) return;
            float t = Mathf.Clamp01(age / (GameFeel.Reduced ? .35f : duration));
            float alpha = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .12f)) * (1 - Mathf.SmoothStep(.45f, 1, t));
            var r = rectTransform.rect;
            Vector2 center = new Vector2(r.center.x, r.yMin + r.height * .43f);
            Vector2 radius = new Vector2(r.width * .58f, r.height * .43f);
            float expand = GameFeel.Reduced ? 1 : 1 + t * .16f;
            Ring(mesh, center, radius * (expand + .06f), r.width * .12f, new Color(1, .66f, .13f, alpha * .26f));
            Ring(mesh, center, radius * expand, r.width * .045f, new Color(1, .73f, .23f, alpha * .60f));
            Ring(mesh, center, radius * expand, r.width * .018f, new Color(1, .95f, .68f, alpha));
            var baseCenter = new Vector2(r.center.x, r.yMin + r.height * .08f);
            Ring(mesh, baseCenter, new Vector2(r.width * .62f, r.height * .055f) * expand,
                r.width * .018f, new Color(1, .77f, .26f, alpha * .9f));
            int count = GameFeel.Reduced ? 4 : 12;
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2 / count + (GameFeel.Reduced ? 0 : t * .48f);
                Vector2 p = center + Vector2.Scale(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), radius) * (1.05f + t * .14f);
                float size = r.width * (i % 3 == 0 ? .15f : .075f) * Mathf.Sin(Mathf.Clamp01(t * 1.6f + i % 3 * .1f) * Mathf.PI);
                Star(mesh, p, size, new Color(1, .90f, .57f, alpha));
            }
        }
        static void Ring(VertexHelper mesh, Vector2 center, Vector2 radius, float width, Color color)
        {
            int first = mesh.currentVertCount;
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI / 32;
                var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                mesh.AddVert(center + Vector2.Scale(v, radius), color, Vector2.zero);
                mesh.AddVert(center + Vector2.Scale(v, radius - Vector2.one * width), color, Vector2.zero);
                if (i == 0) continue;
                int n = first + i * 2;
                mesh.AddTriangle(n - 2, n, n - 1); mesh.AddTriangle(n - 1, n, n + 1);
            }
        }
        static void Star(VertexHelper mesh, Vector2 center, float size, Color color)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i <= 8; i++)
            {
                float a = i * Mathf.PI / 4;
                mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * size * (i % 2 == 0 ? 1 : .24f), color, Vector2.zero);
                if (i > 0) mesh.AddTriangle(first, first + i, first + i + 1);
            }
        }
    }
}
