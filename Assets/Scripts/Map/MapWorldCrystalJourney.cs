using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const int CrystalRegion = 1;
        const int LevelsPerTerrace = 10;
        const float TerraceFeather = .036f;
        [SerializeField] Texture2D[] crystalTerraces;
        readonly CrystalTerraceImage[] crystalArt = new CrystalTerraceImage[5];
        readonly Graphic[] crystalMotes = new Graphic[6];
        RectTransform crystalBackdrop;
        JourneySoftDisc crystalCatShadow;
        Graphic journeyRoute;
        Material crystalWater;
        bool crystalLite;

        // The anchors follow the painted walkable surfaces in each illustration.
        static readonly float[,] CrystalLanes =
        {
            { .50f, .63f, .53f, .27f, .20f, .38f, .64f, .65f, .47f, .43f },
            { .48f, .69f, .47f, .22f, .33f, .31f, .64f, .68f, .48f, .48f },
            { .48f, .66f, .55f, .20f, .23f, .34f, .71f, .71f, .48f, .49f },
            { .54f, .74f, .53f, .26f, .32f, .34f, .63f, .62f, .46f, .49f },
            { .48f, .73f, .55f, .275f, .24f, .49f, .55f, .68f, .49f, .455f }
        };

        // Painted road center at the upper and lower edge of each terrace.
        static readonly float[] TerraceEntrance = { .50f, .49f, .50f, .53f, .50f };
        static readonly float[] TerraceExit = { .43f, .49f, .50f, .51f, .49f };

        bool HasCrystalArt => crystalBackdrop != null && journeyRegion == CrystalRegion;
        float JourneyLane(int slot) => HasCrystalArt
            ? CrystalLanes[slot / LevelsPerTerrace, slot % LevelsPerTerrace]
            : HasThemedArt ? ThemedLane(slot) : JourneyRouteGraphic.Lane(slot);

        void BuildCrystalJourney()
        {
            if (crystalTerraces == null || crystalTerraces.Length != crystalArt.Length)
                return;
            foreach (var texture in crystalTerraces)
                if (texture == null)
                    return;

            if (waterLightShader != null)
            {
                crystalWater = new Material(waterLightShader);
                crystalWater.EnableKeyword("UNITY_UI_CLIP_RECT");
                crystalWater.EnableKeyword("MEWGIC_CRYSTAL_FLOW");
                crystalLite = Application.isMobilePlatform || SystemInfo.graphicsMemorySize is > 0 and < 768;
                if (crystalLite)
                    crystalWater.EnableKeyword("MEWGIC_CRYSTAL_FLOW_LITE");
                crystalWater.SetFloat("_TerraceFeather", TerraceFeather);
            }

            crystalBackdrop = Rect(journeyContent, "CrystalFallsTerraces", Vector2.zero, Vector2.one);
            for (int i = 0; i < crystalArt.Length; i++)
            {
                var terrace = Rect(crystalBackdrop, "CrystalTerrace_" + (i + 1), Vector2.up, Vector2.one);
                var art = terrace.gameObject.AddComponent<CrystalTerraceImage>();
                art.Entrance = TerraceEntrance[i];
                art.Exit = TerraceExit[i];
                art.texture = crystalTerraces[i];
                art.material = crystalWater;
                art.raycastTarget = false;
                crystalArt[i] = art;
            }

            var shadow = Rect(journeyContent, "WizardCatContactShadow", Vector2.up, Vector2.up);
            crystalCatShadow = shadow.gameObject.AddComponent<JourneySoftDisc>();
            crystalCatShadow.color = new Color(.12f, .045f, .19f, .74f);
            crystalCatShadow.raycastTarget = false;

            var motes = Rect(journeyContent, "CrystalFallsMotes", Vector2.zero, Vector2.one);
            for (int i = 0; i < crystalMotes.Length; i++)
            {
                var spark = Rect(motes, "CrystalMote_" + i, Vector2.up, Vector2.up);
                spark.sizeDelta = Vector2.one * (5 + i % 3 * 2);
                var graphic = spark.gameObject.AddComponent<Characters.WizardSparkGraphic>();
                graphic.raycastTarget = false;
                graphic.color = i % 2 == 0 ? new Color(1f, .77f, .39f) : new Color(.53f, .96f, 1f);
                crystalMotes[i] = graphic;
            }
        }

        void LayoutCrystalJourney()
        {
            if (crystalBackdrop == null)
                return;

            bool crystal = HasCrystalArt;
            crystalBackdrop.gameObject.SetActive(crystal);
            crystalCatShadow.gameObject.SetActive(journeyMascot != null);
            crystalMotes[0].transform.parent.gameObject.SetActive(crystal && animateEnvironment && !reducedJourneyMotion);
            if (!crystal)
                return;

            float terraceHeight = rowHeight * LevelsPerTerrace;
            for (int i = 0; i < crystalArt.Length; i++)
            {
                var rect = crystalArt[i].rectTransform;
                float overlap = i < crystalArt.Length - 1 ? terraceHeight * (TerraceFeather + .003f) : 0;
                rect.sizeDelta = new Vector2(0, terraceHeight + overlap + .5f);
                rect.anchoredPosition = new Vector2(0, -(i + .5f) * terraceHeight - overlap * .5f);
            }

        }

        void AnimateCrystalJourney()
        {
            if (!HasCrystalArt || !animateEnvironment)
                return;

            float time = EnvironmentTime;
            if (crystalWater != null)
                crystalWater.SetFloat("_EnvironmentTime", reducedJourneyMotion ? time * .25f : time);
            if (reducedJourneyMotion) return;
            float width = journeyScroll.viewport.rect.width;
            float top = Mathf.Max(0, journeyContent.anchoredPosition.y);
            int first = Mathf.FloorToInt(top / rowHeight);
            int budget = crystalLite ? 3 : crystalMotes.Length;
            for (int i = 0; i < crystalMotes.Length; i++)
            {
                var mote = crystalMotes[i];
                int slot = first + i;
                if (i >= budget || slot >= JourneyLevelCount - 1)
                {
                    mote.canvasRenderer.SetAlpha(0);
                    continue;
                }

                // Only the spaces between nearby stones receive directional light.
                float phase = Mathf.Repeat(time * .42f + slot * .27f, 1);
                float progress = Mathf.Lerp(.39f, .61f, phase);
                float x = Mathf.Lerp(JourneyLane(slot), JourneyLane(slot + 1), progress);
                mote.rectTransform.anchoredPosition = new Vector2(x * width, -(slot + .5f + progress) * rowHeight);
                mote.canvasRenderer.SetAlpha(Mathf.Sin(phase * Mathf.PI) * .86f);
            }
        }
    }

    // Align the road mouths only at the edges, leaving the central artwork and stone anchors intact.
    public sealed class CrystalTerraceImage : RawImage
    {
        static readonly float[] Rows = { 0, .04f, .12f, .88f, .96f, 1 };
        public float Entrance = .5f;
        public float Exit = .5f;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            foreach (float row in Rows)
            {
                float offset = row <= .04f ? Exit - .5f : row >= .96f ? Entrance - .5f : 0;
                float y = Mathf.Lerp(bounds.yMin, bounds.yMax, row);
                mesh.AddVert(new Vector2(bounds.xMin, y), color, new Vector2(offset, row));
                mesh.AddVert(new Vector2(bounds.xMax, y), color, new Vector2(1 + offset, row));
            }
            for (int i = 0; i < (Rows.Length - 1) * 2; i += 2)
            {
                mesh.AddTriangle(i, i + 2, i + 1);
                mesh.AddTriangle(i + 1, i + 2, i + 3);
            }
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class JourneySoftDisc : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            mesh.AddVert(Vector2.zero, color, Vector2.zero);
            var radius = rectTransform.rect.size * .5f;
            var edge = new Color(color.r, color.g, color.b, 0);
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI / 12;
                mesh.AddVert(new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y), edge, Vector2.zero);
                mesh.AddTriangle(0, i + 1, (i + 1) % 24 + 1);
            }
        }
    }
}
