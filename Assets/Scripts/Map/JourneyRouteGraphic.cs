using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyRouteGraphic : MaskableGraphic
    {
        bool foundation;
        Color paving, wall, gold;
        Texture2D pavingTexture;
        public override Texture mainTexture => pavingTexture != null ? pavingTexture : base.mainTexture;
        internal static float Lane(int slot) => Lane((float)slot);
        static float Lane(float slot) => .50f + .245f * Mathf.Sin(slot * .82f - 1.05f);
        internal static float FoundationLane(float slot) => .5f + .195f * Mathf.Sin(slot * .63f - .75f);
        internal static float NodeLane(int slot)
        {
            float lane = FoundationLane(slot);
            return lane + (lane < .5f ? -.175f : .175f);
        }

        internal void UseTheme(MapJourneyTheme theme, Texture2D commonPaving)
        {
            foundation = true;
            pavingTexture = theme != null && theme.pavingTexture != null ? theme.pavingTexture : commonPaving;
            SetMaterialDirty();
            paving = theme != null ? theme.paving : new Color32(178, 138, 160, 255);
            wall = theme != null ? theme.stoneWall : new Color32(55, 40, 84, 255);
            gold = theme != null ? theme.gold : new Color32(236, 183, 116, 255);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = rectTransform.rect;
            if (bounds.width <= 0 || bounds.height <= 0) return;
            if (foundation)
            {
                DrawFoundation(mesh, bounds);
                return;
            }
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

        void DrawFoundation(VertexHelper mesh, Rect bounds)
        {
            float row = bounds.height / 50;
            float half = bounds.width * .080f;
            float depth = bounds.width * .019f;
            Vector2 Point(float slot) => new Vector2(bounds.xMin + FoundationLane(slot) * bounds.width, bounds.yMax - (slot + .5f) * row);
            Vector2 Edge(float slot) => Vector2.Perpendicular((Point(slot + .015f) - Point(slot - .015f)).normalized) * half;
            var shadow = new Color32(27, 17, 50, 230);
            var stoneTint = Color.Lerp(Color.white, paving, .34f);
            for (int slot = 0; slot < 50; slot++)
            {
                var center = Point(slot);
                var bank = new Vector2(bounds.xMin + NodeLane(slot) * bounds.width, center.y);
                var pad = new Vector2(bounds.width * .083f, row * .34f);
                Strip(mesh, center + Vector2.down * depth, bank + Vector2.down * depth, Vector2.up * row * .145f, shadow);
                Strip(mesh, center, bank, Vector2.up * row * .155f, wall);
                Strip(mesh, center + Vector2.up * 2, bank + Vector2.up * 2, Vector2.up * row * .127f, paving);
                Disc(mesh, bank + Vector2.down * depth, pad, shadow, false);
                Disc(mesh, bank + Vector2.down * (depth * .58f), pad, wall, false);
                Disc(mesh, bank, pad, gold, false);
                Disc(mesh, bank, pad * .95f, stoneTint, true);
            }

            float distance = 0;
            float tileLength = half * 2.6f;
            for (int step = 0; step < 400; step++)
            {
                float a = step / 8f - .5f, b = a + .125f;
                Vector2 start = Point(a), end = Point(b), left = Edge(a), right = Edge(b);
                var drop = Vector2.down * depth;
                Quad(mesh, start - left + drop, end - right + drop, end + right + drop, start + left + drop, shadow);
                Quad(mesh, start - left, end - right, end - right + drop, start - left + drop, wall);
                Quad(mesh, start + left, end + right, end + right + drop, start + left + drop, wall);
                Quad(mesh, start - left, end - right, end + right, start + left, Color.Lerp(wall, paving, .60f));
                float v0 = distance / tileLength;
                distance += Vector2.Distance(start, end);
                float v1 = distance / tileLength;
                Surface(mesh, start - left * .95f, end - right * .95f, end + right * .95f,
                    start + left * .95f, stoneTint, v0, v1);
                Quad(mesh, start - left, end - right, end - right * .96f, start - left * .96f, gold);
                Quad(mesh, start + left, end + right, end + right * .96f, start + left * .96f, gold);
                if (step % 8 == 2 || step % 8 == 6)
                {
                    Vector2 edge = start + (step % 8 == 2 ? left : -left);
                    float size = bounds.width * .011f;
                    Quad(mesh, edge + Vector2.left * size, edge + Vector2.down * size, edge + Vector2.right * size, edge + Vector2.up * size, wall);
                    Quad(mesh, edge + Vector2.left * size * .7f, edge + Vector2.down * size * .55f,
                        edge + Vector2.right * size * .7f, edge + Vector2.up * size * .94f, gold);
                }
            }
        }

        static readonly Vector2 Solid = new Vector2(-2, 0);

        static void Disc(VertexHelper mesh, Vector2 center, Vector2 radius, Color tint, bool textured)
        {
            const int edges = 20;
            int start = mesh.currentVertCount;
            mesh.AddVert(center, tint, textured ? new Vector2(.5f, .5f) : Solid);
            for (int i = 0; i < edges; i++)
            {
                float angle = i * Mathf.PI * 2 / edges;
                var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                mesh.AddVert(center + Vector2.Scale(radius, point), tint,
                    textured ? new Vector2(.5f, .5f) + point * .46f : Solid);
                mesh.AddTriangle(start, start + i + 1, start + (i + 1) % edges + 1);
            }
        }

        static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color) =>
            AddQuad(mesh, a, b, c, d, color, Solid, Solid, Solid, Solid);

        static void Surface(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, float v0, float v1) =>
            AddQuad(mesh, a, b, c, d, color, new Vector2(.04f, v0), new Vector2(.04f, v1), new Vector2(.96f, v1), new Vector2(.96f, v0));

        static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color,
            Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, color, uvA);
            mesh.AddVert(b, color, uvB);
            mesh.AddVert(c, color, uvC);
            mesh.AddVert(d, color, uvD);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }

        static void Strip(VertexHelper mesh, Vector2 start, Vector2 end, Vector2 normal, Color32 color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(start - normal, color, Solid);
            mesh.AddVert(end - normal, color, Solid);
            mesh.AddVert(end + normal, color, Solid);
            mesh.AddVert(start + normal, color, Solid);
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
        internal const float ArrivalDuration = 1.8f;
        static readonly Color Amber = new(1f, .50f, .026f);
        static readonly Color Gold = new(1f, .76f, .095f);
        static readonly Color Highlight = new(1f, .98f, .69f);
        static readonly float[] RimOffsets = { 0, 2, 4, 9, 20, 36 };
        static readonly float[] RimAlphas = { .38f, 1, .9f, .67f, .20f, 0 };
        bool crystalGold;
        float elapsed = ArrivalDuration;

        internal float ArrivalTime => elapsed;
        internal float ArrivalStrength => Rise(.20f, .74f) * (1 - Rise(.93f, ArrivalDuration));

        internal void SetArrivalTime(float seconds)
        {
            seconds = Mathf.Clamp(seconds, 0, ArrivalDuration);
            if (elapsed == seconds) return;
            elapsed = seconds;
            SetVerticesDirty();
        }

        public void SetCrystalGold(bool value)
        {
            if (crystalGold == value) return;
            crystalGold = value;
            SetVerticesDirty();
        }

        float Rise(float start, float end) =>
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(start, end, elapsed));

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 size = rectTransform.rect.size;
            float energy = crystalGold ? ArrivalStrength : 0;
            float unit = Mathf.Min(size.x / 164, size.y / 111);
            float ground = crystalGold ? Rise(.54f, .86f) * (1 - Rise(.98f, ArrivalDuration)) : 0;

            if (crystalGold)
            {
                SoftLight(mesh, new Vector2(0, size.y * .11f), size * new Vector2(.83f, 1.12f),
                    Amber, .25f + energy * .53f);
                if (energy > .005f) Rays(mesh, size, energy);
                Vector2 floor = new(0, -size.y * .47f);
                SoftLight(mesh, floor, size * new Vector2(.90f, .27f), Amber, .30f + ground * .68f);
                SoftLight(mesh, floor, size * new Vector2(.79f, .14f), Gold, .34f + ground * .65f);
                SoftLight(mesh, floor - Vector2.up * unit * 2, size * new Vector2(.42f, .031f),
                    Highlight, .36f + ground * .63f);
                if (energy > .005f)
                {
                    SoftLight(mesh, floor - Vector2.up * unit * 3, size * new Vector2(.17f, .045f), Highlight, ground);
                    Flourish(mesh, size, -1, energy);
                    Flourish(mesh, size, 1, energy);
                    Sparks(mesh, size, energy);
                }
            }

            Rim(mesh, size, unit, energy);

            if (crystalGold)
            {
                Diamond(mesh, new Vector2(-size.x * .108f, size.y * .60f), new Vector2(5, 8) * unit, .38f, .64f);
                Diamond(mesh, new Vector2(0, size.y * .67f), new Vector2(9, 14) * unit, .51f, .77f);
                Diamond(mesh, new Vector2(size.x * .108f, size.y * .60f), new Vector2(5, 8) * unit, .64f, .90f);
            }
        }

        void Rim(VertexHelper mesh, Vector2 size, float unit, float energy)
        {
            var outline = JourneyStoneGraphic.Outline;
            Vector2 stoneSize = size - new Vector2(18, 16);
            int root = mesh.currentVertCount;
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 point = outline[i];
                Vector2 edge = point * stoneSize;
                edge.y -= Mathf.InverseLerp(-.20f, -.43f, point.y) * 7;
                Vector2 outside = (point - Vector2.down * .15f).normalized;
                float height = Mathf.InverseLerp(-.49f, .49f, point.y);
                float sweep = Rise(.045f + height * .29f, .19f + height * .36f);
                float power = crystalGold ? .08f + sweep * (.69f + .23f * energy) : .60f;
                for (int band = 0; band < RimOffsets.Length; band++)
                {
                    Color tint = band == 1 ? Highlight : band <= 3 ? Gold : Amber;
                    float offset = RimOffsets[band] * unit * (crystalGold ? 1 + energy * .20f : .48f);
                    mesh.AddVert(edge + outside * offset, Alpha(tint, power * RimAlphas[band]), Vector2.zero);
                }
            }

            for (int i = 0; i < outline.Length; i++)
            {
                int row = root + i * RimOffsets.Length;
                int next = root + (i + 1) % outline.Length * RimOffsets.Length;
                JoinRows(mesh, row, next, RimOffsets.Length);
            }
        }

        static Color Alpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        static void JoinRows(VertexHelper mesh, int first, int second, int width)
        {
            for (int i = 0; i < width - 1; i++)
            {
                mesh.AddTriangle(first + i, first + i + 1, second + i);
                mesh.AddTriangle(second + i, first + i + 1, second + i + 1);
            }
        }

        static void SoftLight(VertexHelper mesh, Vector2 center, Vector2 radius, Color color, float alpha)
        {
            const int segments = 32;
            int root = mesh.currentVertCount;
            for (int ring = 0; ring < 4; ring++)
            {
                float t = ring / 3f;
                float falloff = (1 - t) * (1 - t);
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2 / segments;
                    Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    mesh.AddVert(center + point * t, Alpha(color, alpha * falloff), Vector2.zero);
                }
                if (ring > 0) JoinRows(mesh, root + (ring - 1) * (segments + 1), root + ring * (segments + 1), segments + 1);
            }
        }

        static void Rays(VertexHelper mesh, Vector2 size, float energy)
        {
            var center = Vector2.down * size.y * .12f;
            for (int ray = 0; ray < 20; ray++)
            {
                float angle = .15f + ray * .148f + Mathf.Sin(ray * 8.13f) * .033f;
                var forward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var across = new Vector2(-forward.y, forward.x);
                float reach = .78f + ((ray * 7) % 11) * .066f;
                float width = .017f + ((ray * 3) % 7) * .009f;
                int root = mesh.currentVertCount;
                for (int row = 0; row < 5; row++)
                {
                    float t = row / 4f;
                    Vector2 axis = center + forward * size * Mathf.Lerp(.34f, reach, t);
                    Vector2 span = across * size * width * Mathf.Lerp(.50f, 1.42f, t);
                    float intensity = Mathf.Sin(Mathf.PI * Mathf.Lerp(.44f, 1, t)) * (1 - t);
                    Color tint = Color.Lerp(Gold, Amber, t);
                    float opacity = energy * intensity * (ray % 3 == 0 ? .81f : .43f);
                    mesh.AddVert(axis - span, Alpha(tint, 0), Vector2.zero);
                    mesh.AddVert(axis, Alpha(tint, opacity), Vector2.zero);
                    mesh.AddVert(axis + span, Alpha(tint, 0), Vector2.zero);
                    if (row > 0) JoinRows(mesh, root + row * 3 - 3, root + row * 3, 3);
                }
            }
        }

        static void Flourish(VertexHelper mesh, Vector2 size, float side, float energy)
        {
            int root = mesh.currentVertCount;
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                float x = .46f + .33f * Mathf.Sin(Mathf.PI * t);
                float y = Mathf.Lerp(-.42f, .24f, t);
                var point = new Vector2(side * x * size.x, y * size.y);
                float opacity = energy * Mathf.Sin(Mathf.PI * t) * .88f;
                mesh.AddVert(point - Vector2.up * 2.5f, Alpha(Amber, 0), Vector2.zero);
                mesh.AddVert(point, Alpha(Gold, opacity), Vector2.zero);
                mesh.AddVert(point + Vector2.up * 2.5f, Alpha(Amber, 0), Vector2.zero);
                if (i > 0) JoinRows(mesh, root + i * 3 - 3, root + i * 3, 3);
            }
        }

        static void Sparks(VertexHelper mesh, Vector2 size, float energy)
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = i * 2.399963f;
                float radius = .56f + (i % 5) * .058f;
                var position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .74f) * size * radius;
                float width = (i % 4 == 0 ? .013f : .006f) * size.x;
                Spark(mesh, position, new Vector2(width, width * 1.22f), energy);
            }
        }

        static void Spark(VertexHelper mesh, Vector2 center, Vector2 radius, float light)
        {
            int root = mesh.currentVertCount;
            mesh.AddVert(center, Alpha(Highlight, light), Vector2.zero);
            mesh.AddVert(center + Vector2.up * radius.y, Alpha(Gold, light * .24f), Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius.x, Alpha(Gold, light * .24f), Vector2.zero);
            mesh.AddVert(center - Vector2.up * radius.y, Alpha(Gold, light * .24f), Vector2.zero);
            mesh.AddVert(center - Vector2.right * radius.x, Alpha(Gold, light * .24f), Vector2.zero);
            for (int i = 0; i < 4; i++)
                mesh.AddTriangle(root, root + 1 + i, root + 1 + (i + 1) % 4);
        }

        void Diamond(VertexHelper mesh, Vector2 center, Vector2 radius, float start, float peak)
        {
            float shine = Rise(start, peak) * (1 - Rise(1.0f, ArrivalDuration));
            float brightness = .68f * Rise(start, peak) + .32f * shine;
            if (brightness < .005f) return;
            SoftLight(mesh, center, radius * 2.2f, Gold, .20f + shine * .46f);
            int root = mesh.currentVertCount;
            mesh.AddVert(center, Alpha(Highlight, brightness), Vector2.zero);
            mesh.AddVert(center + Vector2.up * radius.y, Alpha(Highlight, brightness), Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius.x, Alpha(Gold, brightness), Vector2.zero);
            mesh.AddVert(center - Vector2.up * radius.y, Alpha(new Color(1, .64f, .055f), brightness), Vector2.zero);
            mesh.AddVert(center - Vector2.right * radius.x, Alpha(new Color(1, .86f, .33f), brightness), Vector2.zero);
            for (int i = 0; i < 4; i++)
                mesh.AddTriangle(root, root + 1 + i, root + 1 + (i + 1) % 4);
            Spark(mesh, center, radius * .53f, brightness * (.47f + shine * .53f));
        }
    }
}
