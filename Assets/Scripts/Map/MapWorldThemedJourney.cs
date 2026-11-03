using Mewgic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const float ThemedOverlap = .058f;
        static readonly int[] TerraceStarts = { 0, 12, 25, 37, 50 };
        readonly ThemedTerraceImage[] themedTerraces = new ThemedTerraceImage[4];
        RectTransform themedBackdrop;
        Texture2D themedAtlas;
        int themedRegion = -1, themedSource;

        bool HasThemedArt => themedAtlas != null && themedRegion == journeyRegion;
        bool HasPaintedJourney => HasCrystalArt || HasThemedArt;
        public Texture2D LoadedJourneyAtlas => themedAtlas;

        void BuildThemedJourney()
        {
            themedBackdrop = Rect(journeyContent, "ThemedJourneyTerraces", Vector2.zero, Vector2.one);
            for (int i = 0; i < themedTerraces.Length; i++)
            {
                var rect = Rect(themedBackdrop, "ThemedTerrace_" + (i + 1), Vector2.up, Vector2.one);
                var image = rect.gameObject.AddComponent<ThemedTerraceImage>();
                image.raycastTarget = false;

                themedTerraces[i] = image;
            }
            themedBackdrop.gameObject.SetActive(false);
        }

        void LoadThemedJourney(int region)
        {
            if (themedRegion == region)
                return;
            ReleaseThemedJourney();
            themedRegion = region;
            if (region == CrystalRegion || region < 0)
                return;

            themedSource = region;
            if (region >= WorldMapRules.IslandsPerPack)
            {
                themedSource = region % (WorldMapRules.IslandsPerPack - 1);
                if (themedSource >= CrystalRegion) themedSource++;
            }
            themedAtlas = Resources.Load<Texture2D>("Map/Journeys/Island_" + (themedSource + 1).ToString("D2"));
            if (themedAtlas == null)
                return;
            for (int i = 0; i < themedTerraces.Length; i++)
            {
                var terrace = themedTerraces[i];
                terrace.texture = themedAtlas;
                float crop = TopCrop(themedSource, i);
                terrace.uvRect = new Rect(i % 2 * .5f, i < 2 ? .5f : 0, .5f, .5f * (1 - crop));
                terrace.Entrance = ThemedPath.Lane(themedSource, i, crop);
                terrace.Exit = ThemedPath.Lane(themedSource, i, 1);
            }
        }

        void ReleaseThemedJourney()
        {
            if (themedAtlas != null)
            {
                foreach (var terrace in themedTerraces)
                    if (terrace != null) terrace.texture = null;
                Resources.UnloadAsset(themedAtlas);
                themedAtlas = null;
            }
            themedRegion = -1;
        }

        void LayoutThemedJourney()
        {
            themedBackdrop.gameObject.SetActive(HasThemedArt);
            if (!HasThemedArt)
                return;
            for (int i = 0; i < themedTerraces.Length; i++)
            {
                float top = TerraceStarts[i] * rowHeight;
                float height = (TerraceStarts[i + 1] - TerraceStarts[i]) * rowHeight;
                float overlap = i < themedTerraces.Length - 1 ? height * ThemedOverlap : 0;
                var rect = themedTerraces[i].rectTransform;
                rect.sizeDelta = new Vector2(0, height + overlap + .5f);
                rect.anchoredPosition = Vector2.down * (top + (height + overlap) * .5f);
            }
        }

        static float TopCrop(int region, int panel) => region == 11 ? .125f : region == 7 && panel == 0 ? .09f : 0;

        float ThemedLane(float slot)
        {
            float position = Mathf.Clamp(slot + .5f, 0, JourneyLevelCount);
            int panel = 0;
            while (panel < themedTerraces.Length - 1 && position >= TerraceStarts[panel + 1])
                panel++;

            float height = TerraceStarts[panel + 1] - TerraceStarts[panel];
            float overlap = panel < themedTerraces.Length - 1 ? ThemedOverlap : 0;
            float t = (position - TerraceStarts[panel]) / (height * (1 + overlap));
            float crop = TopCrop(themedSource, panel);
            float raw = ThemedPath.Lane(themedSource, panel, Mathf.Lerp(crop, 1, t));
            // Match the edge warp of the art while leaving the center of the panel untouched.
            float startOffset = ThemedPath.Lane(themedSource, panel, crop) - .5f;
            float endOffset = ThemedPath.Lane(themedSource, panel, 1) - .5f;
            return raw - startOffset * (1 - Mathf.InverseLerp(.055f, .16f, t)) -
                endOffset * Mathf.InverseLerp(.84f, .95f, t);
        }
    }

    // Four independent painted panels share one texture. The UV inset avoids adjacent atlas pixels.
    [RequireComponent(typeof(CanvasRenderer))]
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
