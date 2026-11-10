using Mewgic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    // One mesh per bottle, rebuilt only while its completion is playing.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BottleCompletionGraphic : MaskableGraphic
    {
        [SerializeField, Range(.6f, 1.5f)] float duration = 1.05f;
        const float Turn = Mathf.PI * 2;
        static readonly Color Gold = new Color(1f, .80f, .13f);
        static readonly Color WhiteGold = new Color(1f, .97f, .63f);

        float age;
        bool paused, reduced;
        Color tint = Color.white;
        public bool Playing { get; private set; }
        public int PlayCount { get; private set; }

        public void Play()
        {
            var effect = GameplayProgress.Current.Equipped(ShopCategory.Effects);
            tint = effect == ShopItem.ClassicEffect ? Color.white : ShopCosmetics.EffectTint(effect);
            reduced = GameFeel.Reduced;
            age = 0;
            PlayCount++;
            Playing = true;
            enabled = true;
            SetVerticesDirty();
        }

        public void Stop() { Playing = false; enabled = false; }
        public void SetPaused(bool value) => paused = value;

        void Update()
        {
            if (!Playing || paused) return;
            age += Time.unscaledDeltaTime;
            if (age >= (reduced ? .35f : duration)) { Stop(); return; }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (!Playing) return;

            var bounds = rectTransform.rect;
            float t = Mathf.Clamp01(age / (reduced ? .35f : duration));
            float fade = Ease(t, 0, .10f) * (1 - Ease(t, .57f, 1));
            float seal = fade * (1 - Ease(t, .28f, .65f));
            DrawSeal(mesh, bounds, seal);
            if (reduced)
            {
                Spark(mesh, Point(bounds, .11f, .21f), bounds.width * .055f, WhiteGold, fade, true);
                Spark(mesh, Point(bounds, .91f, .63f), bounds.width * .047f, WhiteGold, fade, true);
                return;
            }

            DrawSpiral(mesh, bounds, t, fade);
            DrawPlume(mesh, bounds, t);
        }

        void DrawSeal(VertexHelper mesh, Rect bounds, float alpha)
        {
            if (alpha <= 0) return;
            Vector2 center = Point(bounds, .5f, .087f);
            Vector2 radius = new Vector2(bounds.width * .465f, bounds.height * .040f);
            Vector2 previous = center + Vector2.right * radius.x;
            for (int i = 1; i <= 30; i++)
            {
                float angle = i * Turn / 30;
                Vector2 next = center + Vector2.Scale(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), radius);
                Ribbon(mesh, previous, next, bounds.width * .018f, Gold, alpha * .54f);
                previous = next;
            }
        }

        void DrawSpiral(VertexHelper mesh, Rect bounds, float t, float fade)
        {
            float height = Mathf.Lerp(.16f, 1, Ease(t, .02f, .39f));
            const int segments = 54;
            Vector2 previous = SpiralPoint(bounds, 0, t, out _);
            for (int i = 1; i <= segments; i++)
            {
                float u = height * i / segments;
                Vector2 next = SpiralPoint(bounds, u, t, out float front);
                float alpha = fade * Mathf.Lerp(.14f, .76f, front);
                Ribbon(mesh, previous, next, bounds.width * .036f, Gold, alpha * .40f);
                Ribbon(mesh, previous, next, bounds.width * .011f, WhiteGold, alpha);
                previous = next;
            }

            Color accent = Color.Lerp(Gold, tint, .26f);
            const int particles = 108;
            for (int i = 0; i < particles; i++)
            {
                float u = (i + .4f) / particles;
                if (u > height) break;
                Vector2 position = SpiralPoint(bounds, u, t, out float front);
                float jitter = Mathf.Repeat((i + 1) * .618034f, 1) - .5f;
                position.x += bounds.width * jitter * .23f;
                position.y += bounds.height * (Mathf.Repeat(i * .381966f, 1) * .045f + t * .05f);
                float flicker = .72f + .28f * Mathf.Sin(i * 2.4f + t * 31);
                float size = bounds.width * (i % 17 == 0 ? .052f : Mathf.Lerp(.013f, .029f, flicker));
                Spark(mesh, position, size, i % 5 == 0 ? accent : WhiteGold,
                    fade * Mathf.Lerp(.65f, 1, front) * flicker, i % 17 == 0);
            }
        }

        static Vector2 SpiralPoint(Rect bounds, float u, float t, out float front)
        {
            float angle = (u * 2.82f - t * 1.15f + .10f) * Turn;
            front = .5f + .5f * Mathf.Sin(angle);
            float radius = Mathf.Lerp(.475f, .397f, Ease(u, .65f, 1));
            return Point(bounds, .5f + Mathf.Cos(angle) * radius, .106f + u * .713f + (front - .5f) * .055f);
        }

        static void DrawPlume(VertexHelper mesh, Rect bounds, float t)
        {
            float rise = Ease(t, .13f, .50f);
            float alpha = Ease(t, .13f, .26f) * (1 - Ease(t, .59f, .91f));
            if (alpha <= 0) return;

            float start = Mathf.Max(0, rise - .76f);
            PlumeRibbon(mesh, bounds, start, rise, Gold, alpha * .61f, 1.65f);
            PlumeRibbon(mesh, bounds, start, rise, WhiteGold, alpha, 1);
            Spark(mesh, PlumePoint(bounds, rise), bounds.width * .073f, WhiteGold, alpha, false);
            for (int i = 0; i < 7; i++)
            {
                float u = Mathf.Clamp01(rise - i * .095f);
                Vector2 point = PlumePoint(bounds, u);
                point.x += bounds.width * (i % 2 == 0 ? .081f : -.067f);
                point.y += bounds.height * (i * .008f);
                Spark(mesh, point, bounds.width * (i % 3 == 0 ? .024f : .013f), Gold,
                    alpha * (1 - i * .09f), i == 2);
            }
        }

        static void PlumeRibbon(VertexHelper mesh, Rect bounds, float start, float end,
            Color tint, float alpha, float widthScale)
        {
            int first = mesh.currentVertCount;
            for (int i = 0; i <= 18; i++)
            {
                float t = i / 18f;
                float u = Mathf.Lerp(start, end, t);
                Vector2 center = PlumePoint(bounds, u);
                Vector2 direction = PlumePoint(bounds, u + .01f) - PlumePoint(bounds, u - .01f);
                float width = bounds.width * Mathf.Lerp(.028f, .063f, t) * widthScale;
                Vector2 side = Vector2.Perpendicular(direction.normalized) * width;
                tint.a = 0;
                mesh.AddVert(center - side, tint, Vector2.zero);
                tint.a = alpha * Mathf.SmoothStep(.12f, 1, t);
                mesh.AddVert(center, tint, Vector2.zero);
                tint.a = 0;
                mesh.AddVert(center + side, tint, Vector2.zero);
                if (i == 0) continue;
                int vertex = first + i * 3;
                mesh.AddTriangle(vertex - 3, vertex, vertex + 1);
                mesh.AddTriangle(vertex - 3, vertex + 1, vertex - 2);
                mesh.AddTriangle(vertex - 2, vertex + 1, vertex + 2);
                mesh.AddTriangle(vertex - 2, vertex + 2, vertex - 1);
            }
        }

        static Vector2 PlumePoint(Rect bounds, float u) =>
            Point(bounds, .50f - Mathf.Sin(u * Turn) * .13f + u * .085f, .953f + u * .244f);

        static Vector2 Point(Rect bounds, float x, float y) =>
            new Vector2(bounds.xMin + bounds.width * x, bounds.yMin + bounds.height * y);

        static float Ease(float value, float start, float end) =>
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(start, end, value));

        static void Ribbon(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint, float alpha)
        {
            Vector2 side = Vector2.Perpendicular((end - start).normalized) * width;
            int vertex = mesh.currentVertCount;
            tint.a = 0;
            mesh.AddVert(start - side, tint, Vector2.zero);
            mesh.AddVert(end - side, tint, Vector2.zero);
            mesh.AddVert(end + side, tint, Vector2.zero);
            mesh.AddVert(start + side, tint, Vector2.zero);
            tint.a = alpha;
            mesh.AddVert(start, tint, Vector2.zero);
            mesh.AddVert(end, tint, Vector2.zero);
            mesh.AddTriangle(vertex, vertex + 1, vertex + 5);
            mesh.AddTriangle(vertex, vertex + 5, vertex + 4);
            mesh.AddTriangle(vertex + 4, vertex + 5, vertex + 2);
            mesh.AddTriangle(vertex + 4, vertex + 2, vertex + 3);
        }

        static void Spark(VertexHelper mesh, Vector2 center, float size, Color tint, float alpha, bool star)
        {
            if (alpha <= 0) return;
            int first = mesh.currentVertCount;
            tint.a = alpha;
            mesh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Turn / 8;
                float radius = star && i % 2 != 0 ? size * .21f : size;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                tint.a = alpha * .87f;
                mesh.AddVert(center + offset * .34f, tint, Vector2.zero);
                tint.a = 0;
                mesh.AddVert(center + offset, tint, Vector2.zero);
            }
            for (int i = 0; i < 8; i++)
            {
                int a = first + 1 + i * 2;
                int b = first + 1 + (i + 1) % 8 * 2;
                mesh.AddTriangle(first, a, b);
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
            }
        }
    }
}
