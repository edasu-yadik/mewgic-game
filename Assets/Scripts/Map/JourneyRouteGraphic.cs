using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneyRouteGraphic : MaskableGraphic
    {
        bool foundation, terraceBridges;
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

        internal void UseTheme(MapJourneyTheme theme, Texture2D commonPaving, bool useTerraceBridges)
        {
            foundation = true;
            terraceBridges = useTerraceBridges;
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
            float half = bounds.width * (terraceBridges ? .046f : .080f);
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
                if (terraceBridges)
                {
                    var landing = new Vector2(bounds.width * .051f, row * .28f);
                    Disc(mesh, center + Vector2.down * depth, landing, shadow, false);
                    Disc(mesh, center, landing, wall, false);
                    Disc(mesh, center, landing * .94f, stoneTint, true);
                    if (slot % 2 == 1 && slot % 10 != 9)
                        for (int step = 1; step < 4; step++)
                        {
                            Vector2 stone = Point(slot + step * .25f);
                            var radius = new Vector2(bounds.width * .023f, row * .075f);
                            Disc(mesh, stone + Vector2.down * 4, radius, shadow, false);
                            Disc(mesh, stone, radius, stoneTint, true);
                        }
                }
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
                // Two landings share a short bridge. Stepping stones connect the next pair.
                if (terraceBridges && Mathf.FloorToInt(a) % 2 != 0)
                    continue;
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
        bool crystalGold;

        public void SetCrystalGold(bool value)
        {
            if (crystalGold == value) return;
            crystalGold = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var outline = JourneyStoneGraphic.Outline;
            var size = rectTransform.rect.size;
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 edge = Vector2.Scale(outline[i], size);
                mesh.AddVert(edge * .85f, new Color32(148, 67, 240, 0), Vector2.zero);
                mesh.AddVert(edge * .98f, crystalGold ? new Color32(255, 242, 158, 255) : new Color32(255, 216, 117, 245), Vector2.zero);
                mesh.AddVert(edge * (crystalGold ? 1.10f : 1.035f),
                    crystalGold ? new Color32(255, 168, 49, 210) : new Color32(190, 98, 247, 168), Vector2.zero);
                mesh.AddVert(edge * (crystalGold ? 1.36f : 1.115f),
                    crystalGold ? new Color32(255, 190, 85, 0) : new Color32(175, 76, 242, 0), Vector2.zero);
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

            if (crystalGold)
            {
                CrestDiamond(mesh, new Vector2(0, size.y * .57f), new Vector2(11, 18));
                CrestDiamond(mesh, new Vector2(-16, size.y * .51f), new Vector2(6, 9));
                CrestDiamond(mesh, new Vector2(16, size.y * .51f), new Vector2(6, 9));
            }
        }

        static void CrestDiamond(VertexHelper mesh, Vector2 center, Vector2 radius)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(center, new Color32(255, 246, 185, 255), Vector2.zero);
            mesh.AddVert(center + Vector2.up * radius.y, new Color32(255, 227, 140, 255), Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius.x, new Color32(255, 174, 51, 240), Vector2.zero);
            mesh.AddVert(center - Vector2.up * radius.y, new Color32(210, 107, 32, 240), Vector2.zero);
            mesh.AddVert(center - Vector2.right * radius.x, new Color32(255, 196, 76, 240), Vector2.zero);
            for (int i = 0; i < 4; i++)
                mesh.AddTriangle(index, index + 1 + i, index + 1 + (i + 1) % 4);
        }
    }
}
