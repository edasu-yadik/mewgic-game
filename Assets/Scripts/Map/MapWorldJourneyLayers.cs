using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        [SerializeField] bool useLegacyJourneyArt;
        [SerializeField] Texture2D journeyCatPoses, journeyPaving, journeyNearCrystals;
        RawImage journeyBackplate;
        Image journeyHaze, journeyForeground;
        JourneySoftDisc journeyCatShadow;
        Material journeyPlateMaterial, journeyCatMaterial, journeyStoneMaterial;
        readonly RawImage[] crystalEdges = new RawImage[2];
        RectTransform journeyEdge;
        readonly Graphic[] journeyEmbers = new Graphic[8];
        MapJourneyTheme journeyTheme;

        void BuildJourneyBackground(RectTransform viewport)
        {
            var root = Rect(viewport, "JourneyFixedBackground", Vector2.zero, Vector2.one);
            var fallback = Art(root, "JourneyCommonSky", sky, Vector2.zero, Vector2.one);
            fallback.color = new Color(.65f, .63f, .84f);
            journeyBackplate = Rect(root, "JourneyBackplate", Vector2.zero, Vector2.one).gameObject.AddComponent<RawImage>();
            journeyBackplate.raycastTarget = false;
            if (waterLightShader != null)
            {
                journeyPlateMaterial = new Material(waterLightShader);
                journeyPlateMaterial.EnableKeyword("MEWGIC_JOURNEY_PLATE");
                journeyPlateMaterial.EnableKeyword("UNITY_UI_CLIP_RECT");
                journeyBackplate.material = journeyPlateMaterial;
            }
            journeyHaze = Art(root, "JourneyMiddleMist", fog, Vector2.zero, Vector2.one);
            journeyHaze.color = new Color(.67f, .67f, .94f, .13f);
        }

        void BuildJourneyForeground(RectTransform viewport)
        {
            var root = Rect(viewport, "JourneyForeground", Vector2.zero, Vector2.one);
            journeyEdge = Rect(root, "JourneyNearCrystals", Vector2.zero, new Vector2(1, .23f));
            for (int i = 0; i < crystalEdges.Length; i++)
            {
                bool left = i == 0;
                var edge = Rect(journeyEdge, "CrystalCluster_" + i,
                    new Vector2(left ? -.055f : .895f, 0), new Vector2(left ? .105f : 1.055f, left ? .59f : .51f));
                var graphic = edge.gameObject.AddComponent<RawImage>();
                graphic.texture = journeyNearCrystals;
                graphic.raycastTarget = false;
                if (!left) graphic.uvRect = new Rect(1, 0, -1, 1);
                crystalEdges[i] = graphic;
            }
            if (waterLightShader != null && journeyPaving != null)
            {
                journeyStoneMaterial = new Material(waterLightShader);
                journeyStoneMaterial.EnableKeyword("MEWGIC_JOURNEY_STONE");
                journeyStoneMaterial.EnableKeyword("UNITY_UI_CLIP_RECT");
                journeyRoute.material = journeyStoneMaterial;
            }
            journeyForeground = Art(root, "JourneyForegroundArt", null, Vector2.zero, Vector2.one);
            journeyForeground.preserveAspect = true;
            for (int i = 0; i < journeyEmbers.Length; i++)
            {
                float x = i % 2 == 0 ? .09f + i * .006f : .91f - i * .006f;
                var anchor = new Vector2(x, .15f + (i * 31 % 73) * .01f);
                var rect = Rect(root, "JourneyEmber_" + i, anchor, anchor);
                rect.sizeDelta = Vector2.one * (i % 3 == 0 ? 13 : 8);
                var ember = rect.gameObject.AddComponent<Characters.WizardSparkGraphic>();
                ember.raycastTarget = false;
                ember.color = new Color(1, .78f, .48f, .65f);
                journeyEmbers[i] = ember;
            }
        }

        void BindJourneyTheme(int region)
        {
            if (useLegacyJourneyArt) return;
            journeyTheme = Resources.Load<MapJourneyTheme>("Map/JourneyThemes/" + progression.Region(region).key);
            journeyBackplate.texture = journeyTheme != null ? journeyTheme.background : null;
            journeyBackplate.enabled = journeyBackplate.texture != null;
            journeyForeground.sprite = journeyTheme != null ? journeyTheme.foreground : null;
            journeyForeground.enabled = journeyForeground.sprite != null;
            if (journeyPlateMaterial != null)
                journeyPlateMaterial.SetFloat("_JourneyWater", journeyTheme != null && journeyTheme.flowingWater ? 1 : 0);
            journeyEdge.gameObject.SetActive(journeyNearCrystals != null && !journeyForeground.enabled);
            ((JourneyRouteGraphic)journeyRoute).UseTheme(journeyTheme, journeyPaving);
            UpdateJourneyDepth();
        }

        void UpdateJourneyDepth()
        {
            if (useLegacyJourneyArt || journeyBackplate == null) return;
            bool motion = animateEnvironment && !reducedJourneyMotion;
            float offset = motion ? 2 * (1 - journeyScroll.verticalNormalizedPosition) - 1 : 0;
            // A bounded offset keeps the composition anchored even across all 50 levels.
            var back = journeyBackplate.rectTransform;
            back.sizeDelta = new Vector2(0, 20);
            back.anchoredPosition = Vector2.up * (offset * 7);
            journeyHaze.rectTransform.sizeDelta = new Vector2(0, 40);
            journeyHaze.rectTransform.anchoredPosition = Vector2.up * (offset * -13);
            journeyForeground.rectTransform.anchoredPosition = Vector2.up * (offset * -18);
            journeyEdge.anchoredPosition = Vector2.up * (offset * -18 - 18);
            if (journeyPlateMaterial != null)
                journeyPlateMaterial.SetFloat("_EnvironmentTime", EnvironmentTime);
            for (int i = 0; i < journeyEmbers.Length; i++)
            {
                float phase = EnvironmentTime * .65f + i * 1.65f;
                var ember = journeyEmbers[i];
                ember.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(phase) * 7, offset * -18 + Mathf.Cos(phase) * 12);
                ember.canvasRenderer.SetAlpha(motion ? .36f + Mathf.Sin(phase * 1.8f) * .24f : .20f);
            }
        }

        void BuildJourneyCatShadow()
        {
            var rect = Rect(journeyContent, "JourneyCatContact", Vector2.up, Vector2.up);
            journeyCatShadow = rect.gameObject.AddComponent<JourneySoftDisc>();
            journeyCatShadow.raycastTarget = false;
            journeyCatShadow.color = new Color(.12f, .06f, .19f, .83f);
        }

        void SetJourneyCatMaterial()
        {
            if (journeyCatPoses == null || waterLightShader == null) return;
            journeyCatMaterial = new Material(waterLightShader);
            journeyCatMaterial.EnableKeyword("MEWGIC_JOURNEY_CAT");
            journeyCatMaterial.EnableKeyword("UNITY_UI_CLIP_RECT");
            journeyCat.material = journeyCatMaterial;
            foreach (var edge in crystalEdges)
                if (edge != null) edge.material = journeyCatMaterial;
        }
    }

}
