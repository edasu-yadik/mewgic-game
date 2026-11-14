using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    // Both passes sample the approved painting. The fixed pass omits the painted road,
    // and the travelling pass retains its bridge silhouettes, lamps and immediate rocks.
    public sealed class MapPlateGraphic : RawImage
    {
        internal bool FixedLayer;
        internal bool VillageLayer;
        int chunk;
        Color endTint = Color.white;

        internal void SetChunk(int value)
        {
            if (chunk == value) return;
            chunk = value;
            SetVerticesDirty();
        }

        internal void SetRoadTints(Color start, Color end)
        {
            color = start;
            if (endTint == end) return;
            endTint = end;
            SetVerticesDirty();
        }
        static readonly Vector4[] Bridge =
        {
            new(.181f,.10f,.39f,0), new(.197f,.065f,.51f,.55f), new(.223f,.03f,.76f,1),
            new(.252f,.035f,.90f,1), new(.288f,.08f,.935f,1), new(.325f,.07f,.915f,1),
            new(.375f,.055f,.88f,1), new(.425f,.07f,.835f,1), new(.470f,.075f,.86f,1),
            new(.505f,.09f,.935f,1), new(.555f,.08f,.947f,1), new(.600f,.06f,.91f,1),
            new(.651f,.065f,.88f,1), new(.700f,.07f,.88f,1), new(.751f,.08f,.92f,1),
            new(.797f,.075f,.90f,1), new(.837f,.04f,.75f,1), new(.863f,.045f,.66f,1),
            new(.882f,.07f,.57f,.75f), new(.890f,.10f,.40f,0)
        };
        static readonly Vector4[] LeftBank =
        {
            new(0,0,1,1), new(.126f,0,1,1), new(.176f,0,1,1),
            new(.209f,0,.10f,1), new(.26f,0,.083f,1), new(.46f,0,.115f,1),
            new(.64f,0,.091f,1), new(.79f,0,.115f,1), new(.861f,0,.10f,1),
            new(.908f,0,.99f,1), new(1,0,1,1)
        };
        static readonly Vector4[] RightBank =
        {
            new(.159f,.84f,1,0), new(.192f,.81f,1,1), new(.28f,.924f,1,1),
            new(.40f,.865f,1,1), new(.48f,.857f,1,1), new(.65f,.925f,1,1),
            new(.80f,.92f,1,1), new(.906f,.89f,1,0)
        };

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (texture == null) return;
            if (VillageLayer) DrawVillageVista(mesh);
            else if (FixedLayer)
            {
                Strip(mesh, LeftBank, 0, 1, .055f);
                Strip(mesh, RightBank, 0, 1, .045f);
            }
            else Strip(mesh, Bridge, Bridge[0].x, Bridge[^1].x, .045f);
        }

        void Strip(VertexHelper mesh, Vector4[] rows, float first, float last, float feather)
        {
            var box = rectTransform.rect;
            int start = mesh.currentVertCount;
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                float width = Mathf.Min(feather, (row.z - row.y) * .34f);
                float y = Mathf.Lerp(box.yMax, box.yMin, (row.x - first) / (last - first));
                for (int j = 0; j < 4; j++)
                {
                    float x = j == 0 ? row.y : j == 1 ? row.y + width : j == 2 ? row.z - width : row.z;
                    Color tint = FixedLayer ? color : Color.Lerp(color, endTint, (row.x - first) / (last - first));
                    bool edge = (j == 0 && row.y > 0) || (j == 3 && row.z < 1);
                    tint.a *= edge ? 0 : row.w;
                    float shift = FixedLayer ? 0 : ContinuousMapRoute.Warp(chunk, row.x);
                    mesh.AddVert(new Vector2(Mathf.Lerp(box.xMin, box.xMax, x + shift), y), tint, new Vector2(x, 1 - row.x));
                }
                if (i == 0) continue;
                for (int j = 0; j < 3; j++)
                {
                    int a = start + (i - 1) * 4 + j, b = a + 4;
                    mesh.AddTriangle(a, a + 1, b + 1);
                    mesh.AddTriangle(a, b + 1, b);
                }
            }
        }

        void DrawVillageVista(VertexHelper mesh)
        {
            // A soft-edged window into the existing village painting, behind the road.
            var box = rectTransform.rect;
            const int subdivisions = 8;
            for (int y = 0; y <= subdivisions; y++)
            for (int x = 0; x <= subdivisions; x++)
            {
                float u = x / (float)subdivisions, v = y / (float)subdivisions;
                float opacity = Mathf.SmoothStep(0, 1, Mathf.Min(u, 1-u, v, 1-v) * 6);
                var position = new Vector2(Mathf.Lerp(box.xMin, box.xMax, Mathf.Lerp(.08f, .96f, u)),
                    Mathf.Lerp(box.yMin, box.yMax, Mathf.Lerp(.65f, .98f, v)));
                mesh.AddVert(position, new Color(1, 1, 1, opacity), new Vector2(u, Mathf.Lerp(.37f, 1, v)));
                if (x == 0 || y == 0) continue;
                int last = mesh.currentVertCount - 1;
                int previousRow = last - subdivisions - 1;
                mesh.AddTriangle(previousRow - 1, last - 1, last);
                mesh.AddTriangle(previousRow - 1, last, previousRow);
            }
        }
    }
}
