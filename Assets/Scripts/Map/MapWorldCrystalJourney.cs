using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const int CrystalRegion = 1;
        const int LevelsPerTerrace = 10;
        const float TerraceFeather = .024f;
        [SerializeField] Texture2D[] crystalTerraces;
        readonly RawImage[] crystalArt = new RawImage[5];
        readonly Graphic[] crystalMotes = new Graphic[6];
        RectTransform crystalBackdrop;
        JourneySoftDisc crystalCatShadow;
        Graphic journeyRoute;
        Material crystalWater;

        // The anchors follow the painted walkable surfaces in each illustration.
        static readonly float[,] CrystalLanes =
        {
            { .50f, .63f, .53f, .27f, .20f, .38f, .64f, .65f, .47f, .43f },
            { .48f, .69f, .47f, .22f, .33f, .31f, .64f, .68f, .48f, .48f },
            { .48f, .66f, .55f, .20f, .23f, .34f, .71f, .71f, .48f, .49f },
            { .54f, .74f, .53f, .26f, .32f, .34f, .63f, .62f, .46f, .49f },
            { .48f, .73f, .55f, .275f, .24f, .52f, .55f, .70f, .49f, .455f }
        };

        bool HasCrystalArt => crystalBackdrop != null && journeyRegion == CrystalRegion;
        float JourneyLane(int slot) => HasCrystalArt
            ? CrystalLanes[slot / LevelsPerTerrace, slot % LevelsPerTerrace]
            : JourneyRouteGraphic.Lane(slot);

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
                crystalWater.SetFloat("_TerraceFeather", TerraceFeather);
            }

            crystalBackdrop = Rect(journeyContent, "CrystalFallsTerraces", Vector2.zero, Vector2.one);
            for (int i = 0; i < crystalArt.Length; i++)
            {
                var terrace = Rect(crystalBackdrop, "CrystalTerrace_" + (i + 1), Vector2.up, Vector2.one);
                var art = terrace.gameObject.AddComponent<RawImage>();
                art.texture = crystalTerraces[i];
                art.material = crystalWater;
                art.raycastTarget = false;
                crystalArt[i] = art;
            }

            var shadow = Rect(journeyContent, "WizardCatContactShadow", Vector2.up, Vector2.up);
            crystalCatShadow = shadow.gameObject.AddComponent<JourneySoftDisc>();
            crystalCatShadow.color = new Color(.12f, .045f, .19f, .74f);
            crystalCatShadow.raycastTarget = false;

            var motes = Rect(journeyScroll.viewport, "CrystalFallsMotes", Vector2.zero, Vector2.one);
            for (int i = 0; i < crystalMotes.Length; i++)
            {
                var spark = Rect(motes, "CrystalMote_" + i, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
                spark.sizeDelta = Vector2.one * (5 + i % 3 * 2);
                var graphic = spark.gameObject.AddComponent<Characters.WizardSparkGraphic>();
                graphic.raycastTarget = false;
                graphic.color = i % 2 == 0 ? new Color(1f, .77f, .39f) : new Color(.53f, .96f, 1f);
                crystalMotes[i] = graphic;
            }
        }

        void LayoutCrystalJourney(float width)
        {
            if (crystalBackdrop == null)
                return;

            bool crystal = HasCrystalArt;
            crystalBackdrop.gameObject.SetActive(crystal);
            crystalCatShadow.gameObject.SetActive(crystal && journeyMascot != null);
            crystalMotes[0].transform.parent.gameObject.SetActive(crystal && animateEnvironment);
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

            float catHeight = width * .172f;
            var catRect = journeyCat.rectTransform;
            catRect.pivot = new Vector2(.5f, .035f);
            catRect.sizeDelta = journeyMascot == null ? Vector2.zero :
                new Vector2(catHeight * journeyMascot.width / journeyMascot.height, catHeight);
            catRect.anchoredPosition = new Vector2(width * .827f, -terraceHeight * 4.86f);
            journeyCat.color = new Color(1f, .91f, .96f);
            var shadow = crystalCatShadow.rectTransform;
            shadow.anchoredPosition = catRect.anchoredPosition + new Vector2(0, width * .004f);
            shadow.sizeDelta = new Vector2(width * .142f, width * .043f);
        }

        void AnimateCrystalJourney()
        {
            if (!HasCrystalArt || !animateEnvironment)
                return;

            float time = EnvironmentTime;
            if (crystalWater != null)
                crystalWater.SetFloat("_EnvironmentTime", time);
            var size = journeyScroll.viewport.rect.size;
            for (int i = 0; i < crystalMotes.Length; i++)
            {
                var mote = crystalMotes[i];
                float phase = Mathf.Repeat(time * (.045f + i * .005f) + i * .153f, 1);
                float x = Mathf.Sin(i * 9.14f) * .38f + Mathf.Sin(time * .4f + i) * .045f;
                mote.rectTransform.anchoredPosition = new Vector2(x * size.x, (phase - .5f) * size.y);
                mote.canvasRenderer.SetAlpha(Mathf.Sin(phase * Mathf.PI) * .52f);
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
