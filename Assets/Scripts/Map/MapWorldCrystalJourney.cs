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
        Material crystalWater, crystalReferenceWater;
        Texture2D crystalReference, crystalCatRestAtlas;
        RectTransform crystalAtmosphere;
        float crystalSettleAt;
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
        bool HasCrystalComposition => HasCrystalArt && crystalReference != null;
        bool HasCrystalRest => HasCrystalComposition && crystalCatRestAtlas != null;

        // Coordinates in the reference plate, with the origin at the upper left.
        // The first point of each segment is at the foot of its numbered stone.
        static readonly Vector2[][] ReferenceWalk =
        {
            new[] { new Vector2(.26f,.239f), new Vector2(.43f,.242f), new Vector2(.61f,.263f) },
            new[] { new Vector2(.61f,.263f), new Vector2(.73f,.287f), new Vector2(.73f,.319f), new Vector2(.62f,.351f), new Vector2(.35f,.365f) },
            new[] { new Vector2(.35f,.365f), new Vector2(.47f,.354f), new Vector2(.64f,.350f) },
            new[] { new Vector2(.64f,.350f), new Vector2(.40f,.357f), new Vector2(.26f,.380f), new Vector2(.234f,.413f), new Vector2(.257f,.445f) },
            new[] { new Vector2(.257f,.445f), new Vector2(.44f,.466f), new Vector2(.60f,.485f), new Vector2(.71f,.507f) },
            new[] { new Vector2(.71f,.507f), new Vector2(.78f,.534f), new Vector2(.72f,.558f), new Vector2(.54f,.581f), new Vector2(.25f,.609f) },
            new[] { new Vector2(.25f,.609f), new Vector2(.38f,.596f), new Vector2(.55f,.584f), new Vector2(.67f,.573f) },
            new[] { new Vector2(.67f,.573f), new Vector2(.49f,.584f), new Vector2(.26f,.605f), new Vector2(.238f,.642f), new Vector2(.34f,.687f) },
            new[] { new Vector2(.34f,.687f), new Vector2(.48f,.706f), new Vector2(.62f,.723f), new Vector2(.705f,.758f) },
            new[] { new Vector2(.705f,.758f), new Vector2(.62f,.788f), new Vector2(.47f,.810f), new Vector2(.31f,.838f), new Vector2(.206f,.857f) },
        };

        Vector2 JourneyNodePosition(int slot)
        {
            float width = journeyScroll.viewport.rect.width;
            if (!HasCrystalComposition)
                return new Vector2(width * (useLegacyJourneyArt ? JourneyLane(slot) : JourneyRouteGraphic.NodeLane(slot)),
                    -(slot + .5f) * rowHeight);
            if (slot < 40)
                return new Vector2(width * JourneyLane(slot), -CrystalDepth(slot));
            Vector2 foot = ReferenceWalk[slot - 40][0];
            return ReferencePosition(new Vector2(foot.x, foot.y - (slot == 49 ? .034f : .024f)));
        }

        const float EarlierTerraceInset = .205f;
        const float EarlierTerraceHeight = .715f;

        float CrystalDepth(float slot) => (Mathf.Floor(slot / 10) * 10 + EarlierTerraceInset * 10 +
            (Mathf.Repeat(slot, 10) + .5f) * EarlierTerraceHeight) * rowHeight;

        Vector2 ReferencePosition(Vector2 point) =>
            new Vector2(point.x * journeyScroll.viewport.rect.width, -(40 + 10 * point.y) * rowHeight);

        Vector2 CrystalRoutePosition(float slot)
        {
            slot = Mathf.Clamp(slot, 0, 49.85f);
            int index = Mathf.FloorToInt(slot);
            float progress = slot - index;
            if (index < 40)
            {
                float x = Mathf.Lerp(JourneyLane(index), JourneyLane(Mathf.Min(index + 1, index / 10 * 10 + 9)), progress);
                return new Vector2(x * journeyScroll.viewport.rect.width, -CrystalDepth(slot));
            }

            var points = ReferenceWalk[index - 40];
            if (index == 49) progress /= .85f;
            float sample = progress * (points.Length - 1);
            int start = Mathf.Min((int)sample, points.Length - 2);
            Vector2 a = points[start], b = points[start + 1];
            Vector2 next = points[Mathf.Min(start + 2, points.Length - 1)];
            Vector2 previous = points[Mathf.Max(start - 1, 0)];
            float t = sample - start;
            Vector2 chord = b - a;
            Vector2 entry = (b - previous) * .5f - chord;
            Vector2 exit = (next - a) * .5f - chord;
            Vector2 cornerBlend = (entry * (1 - t) - exit * t) * (t * (1 - t));
            return ReferencePosition(Vector2.Lerp(a, b, t) + cornerBlend);
        }

        void LayoutCrystalFrame()
        {
            bool crystal = HasCrystalComposition;
            SetJourneyFrame(journeyScroll.viewport, crystal ? Vector2.zero : new Vector2(0, .105f), crystal ? Vector2.one : new Vector2(1, .87f));
            SetJourneyFrame(journeyHeading, crystal ? new Vector2(.247f, .851f) : new Vector2(0, .87f),
                crystal ? new Vector2(.753f, .905f) : Vector2.one);
            SetJourneyFrame(chapterTitle.rectTransform, crystal ? new Vector2(.05f, .14f) : new Vector2(.04f, .34f),
                crystal ? new Vector2(.95f, .89f) : new Vector2(.96f, .95f));
            motionButton.transform.SetParent(crystal ? journeyFooter : journeyHeading, false);
            SetJourneyFrame(motionButton.transform as RectTransform, crystal ? new Vector2(1.035f, .22f) : new Vector2(.72f, .05f),
                crystal ? new Vector2(1.315f, .79f) : new Vector2(.97f, .33f));
            SetJourneyFrame(journeyFooter, crystal ? new Vector2(.033f, .009f) : Vector2.zero,
                crystal ? new Vector2(.754f, .078f) : new Vector2(1, .105f));
            journeyFooter.GetComponent<Image>().enabled = !crystal;
            SetJourneyFrame(journeyBack.transform as RectTransform, crystal ? new Vector2(.01f, .05f) : new Vector2(.07f, .12f),
                crystal ? new Vector2(.51f, .95f) : new Vector2(.60f, .88f));
            SetJourneyFrame(journeyFocus.transform as RectTransform, crystal ? new Vector2(.53f, .05f) : new Vector2(.63f, .12f),
                crystal ? new Vector2(.99f, .95f) : new Vector2(.93f, .88f));
            SetJourneyFrame(journeySkip.transform as RectTransform, journeyScroll.viewport.anchorMin, journeyScroll.viewport.anchorMax);
            journeyInstruction.SetActive(!crystal);
            crystalBrand.SetActive(crystal);
            if (crystalAtmosphere != null) crystalAtmosphere.gameObject.SetActive(crystal);
        }

        static void SetJourneyFrame(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        void SettleCrystalPage()
        {
            if (!HasCrystalComposition || crystalSettleAt == 0 || journeyRunning ||
                Time.unscaledTime < crystalSettleAt || ((MapScrollRect)journeyScroll).Dragging ||
                Mathf.Abs(journeyScroll.velocity.y) > 180) return;

            crystalSettleAt = 0;
            int page = Mathf.Clamp(Mathf.RoundToInt(journeyContent.anchoredPosition.y / (10 * rowHeight)), 0, 4);
            journeyScroll.StopMovement();
            if (reducedJourneyMotion || !animateEnvironment) JumpToJourneySlot(page * 10);
            else
            {
                focusVelocity = 0;
                journeyFocusSlot = page * 10;
            }
        }
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

            if (!useLegacyJourneyArt)
            {
                var theme = Resources.Load<MapJourneyTheme>("Map/JourneyThemes/crystal");
                crystalReference = theme != null ? theme.referenceTerrace : null;
                if (crystalReference == null) return;
                crystalCatRestAtlas = theme.mascotRestAtlas;
                BuildCrystalAtmosphere();
            }

            if (waterLightShader != null)
            {
                crystalWater = new Material(waterLightShader);
                crystalWater.EnableKeyword("UNITY_UI_CLIP_RECT");
                crystalWater.EnableKeyword("MEWGIC_CRYSTAL_FLOW");
                crystalLite = Application.isMobilePlatform || SystemInfo.graphicsMemorySize is > 0 and < 768;
                if (crystalLite)
                    crystalWater.EnableKeyword("MEWGIC_CRYSTAL_FLOW_LITE");
                crystalWater.SetFloat("_TerraceFeather", TerraceFeather);
                if (crystalReference != null)
                {
                    crystalReferenceWater = new Material(waterLightShader);
                    crystalReferenceWater.EnableKeyword("UNITY_UI_CLIP_RECT");
                    crystalReferenceWater.EnableKeyword("MEWGIC_CRYSTAL_REFERENCE");
                }
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
                if (i == crystalArt.Length - 1 && crystalReference != null)
                {
                    art.Entrance = art.Exit = .5f;
                    art.texture = crystalReference;
                    art.material = crystalReferenceWater;
                }
                art.raycastTarget = false;
                crystalArt[i] = art;
            }

            if (useLegacyJourneyArt)
            {
                var shadow = Rect(journeyContent, "WizardCatContactShadow", Vector2.up, Vector2.up);
                crystalCatShadow = shadow.gameObject.AddComponent<JourneySoftDisc>();
                crystalCatShadow.color = new Color(.12f, .045f, .19f, .74f);
                crystalCatShadow.raycastTarget = false;
            }

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

        void BuildCrystalAtmosphere()
        {
            crystalAtmosphere = Rect(journeyScroll.viewport, "CrystalAnchoredAtmosphere", Vector2.zero, Vector2.one);
            crystalAtmosphere.gameObject.SetActive(false);
            AddCrystalHaze("LeftMist", new Vector2(.00f, .37f), new Vector2(.30f, .30f), new Color(.44f, .70f, 1, .17f));
            AddCrystalHaze("RightMist", new Vector2(1, .57f), new Vector2(.29f, .30f), new Color(.65f, .46f, 1, .14f));
            AddCrystalHaze("NearMist", new Vector2(.86f, .063f), new Vector2(.58f, .21f), new Color(.33f, .76f, 1, .12f));
        }

        void AddCrystalHaze(string name, Vector2 center, Vector2 size, Color color)
        {
            var rect = Rect(crystalAtmosphere, name, center - size * .5f, center + size * .5f);
            var mist = rect.gameObject.AddComponent<JourneySoftDisc>();
            mist.color = color;
            mist.raycastTarget = false;
        }

        void LayoutCrystalJourney()
        {
            if (crystalBackdrop == null)
                return;

            bool crystal = HasCrystalArt;
            crystalBackdrop.gameObject.SetActive(crystal);
            if (crystalCatShadow != null) crystalCatShadow.gameObject.SetActive(journeyMascot != null);
            crystalMotes[0].transform.parent.gameObject.SetActive(crystal && animateEnvironment && !reducedJourneyMotion);
            if (!crystal)
                return;

            float terraceHeight = rowHeight * LevelsPerTerrace;
            for (int i = 0; i < crystalArt.Length; i++)
            {
                var rect = crystalArt[i].rectTransform;
                bool reference = HasCrystalComposition && i == crystalArt.Length - 1;
                float inset = HasCrystalComposition && !reference ? terraceHeight * EarlierTerraceInset : 0;
                float height = inset > 0 ? terraceHeight * EarlierTerraceHeight : terraceHeight;
                float overlap = !HasCrystalComposition && i < crystalArt.Length - 1 ? terraceHeight * (TerraceFeather + .003f) : 0;
                rect.sizeDelta = new Vector2(0, height + overlap + .5f);
                rect.anchoredPosition = new Vector2(0, -i * terraceHeight - inset - (height + overlap) * .5f);
            }

        }

        float nextCrystalMotion;
        void AnimateCrystalJourney()
        {
            if (!HasCrystalArt || !animateEnvironment)
                return;

            float time = EnvironmentTime;
            if (crystalReferenceWater != null) crystalReferenceWater.SetFloat("_EnvironmentTime", time);
            if (crystalWater != null)
                crystalWater.SetFloat("_EnvironmentTime", reducedJourneyMotion ? time * .25f : time);
            if (reducedJourneyMotion || time < nextCrystalMotion) return;
            nextCrystalMotion=time+1f/30f;
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
                mote.rectTransform.anchoredPosition = HasCrystalComposition
                    ? CrystalRoutePosition(slot + progress)
                    : new Vector2(Mathf.Lerp(JourneyLane(slot), JourneyLane(slot + 1), progress) * width,
                        -(slot + .5f + progress) * rowHeight);
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
