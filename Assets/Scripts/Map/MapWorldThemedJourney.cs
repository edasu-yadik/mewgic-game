using Mewgic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed class ThemedTerraceImage : RawImage
    {
        static readonly float[] Rows = { 0, .05f, .16f, .84f, .945f, 1 };
        static readonly float[] Columns = { 0, .15f, .85f, 1 };
        public float Entrance = .5f;
        public float Exit = .5f;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = GetPixelAdjustedRect();
            var uv = uvRect;
            float insetX = .75f / (texture != null ? texture.width : 1024);
            float insetY = .75f / (texture != null ? texture.height : 1536);
            for (int i = 0; i < Rows.Length; i++)
            {
                float y = Rows[i];
                float shift = y <= .05f ? Exit - .5f : y >= .945f ? Entrance - .5f : 0;
                Color tint = color;
                if (y == 1) tint.a = 0;
                foreach (float x in Columns)
                {
                    float sample = x == 0 || x == 1 ? x : Mathf.Clamp01(x + shift);
                    mesh.AddVert(new Vector2(Mathf.Lerp(bounds.xMin, bounds.xMax, x), Mathf.Lerp(bounds.yMin, bounds.yMax, y)),
                        tint, new Vector2(Mathf.Lerp(uv.xMin + insetX, uv.xMax - insetX, sample),
                            Mathf.Lerp(uv.yMin + insetY, uv.yMax - insetY, y)));
                }

                if (i == 0)
                    continue;
                for (int j = 0; j < Columns.Length - 1; j++)
                {
                    int a = (i - 1) * Columns.Length + j;
                    int b = a + Columns.Length;
                    mesh.AddTriangle(a, b, a + 1);
                    mesh.AddTriangle(a + 1, b, b + 1);
                }
            }
        }
    }
}
