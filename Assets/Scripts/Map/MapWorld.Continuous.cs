using System;
using System.Collections.Generic;
using System.Linq;
using Mewgic.Core;
using Mewgic.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Map
{
    public sealed partial class MapWorld
    {
        const int RoadLevels = 10, RoadWindow = 9, RoadPool = 5;
        const float PlateTop = .181f, PlateBottom = .890f;
        const int LastRoadTile = (WorldMapRules.MaxLevel - 1) / RoadLevels;

        [Serializable] sealed class RoadTheme
        {
            public int firstLevel;
            public string title, roadTint, backdropTint;
        }
        [Serializable] sealed class RoadThemeConfig
        {
            public float blendLevels;
            public RoadTheme[] themes;
        }
        sealed class RoadTile
        {
            public int index = -1;
            public RectTransform root, stones;
            public MapPlateGraphic art;
            public MapRoadJoinGraphic join;
            public readonly MapPoint[] nodes = new MapPoint[RoadLevels];
            public MapPoint chest;
        }

        readonly RoadTile[] roadTiles = new RoadTile[RoadPool];
        RoadThemeConfig roadThemes;
        Color[] roadTints, backdropTints;
        RawImage fixedBackdrop;
        MapPlateGraphic fixedDetails;
        RectTransform roadTrack, roadJoins, roadStones;
        TMPro.TMP_Text roadRange;
        float roadHeight, roadWidth, cameraVelocity;
        int roadBase, roadWindowSize, visibleTile = -1, paintedRevision = -1, arrivalLevel;
        int boundFirst = -1, boundLast = -1;
        bool rebasing, following, awaitingJourney;
        const int TrailSamples = 128;
        const float StepInterval = .16f, FirstStepDelay = .06f;
        readonly float[] trailDistances = new float[TrailSamples + 1];
        readonly float[] stepPhases = new float[JourneyPawTrail.Capacity];
        float journeyElapsed, arrivalElapsed;
        int stepCount, trailRoadBase;
        int walkFrom, walkTo;
        const float WalkStartFraction = .16f;
        float focusGoal;
        Texture2D roadPlate;
        int MaxRoadTile => Mathf.Min(LastRoadTile, (progression.CurrentId - 1) / RoadLevels + 1);

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

        void MountContinuous()
        {
            if (Mounted) return;
            var crystal = Resources.Load<MapJourneyTheme>("Map/JourneyThemes/crystal");
            if (crystal == null || crystal.background == null || crystal.referenceTerrace == null)
                throw new InvalidOperationException("Tek harita için çevre veya yol kaynağı eksik.");
            var asset = Resources.Load<TextAsset>("Map/ContinuousThemes");
            roadThemes = asset == null ? null : JsonUtility.FromJson<RoadThemeConfig>(asset.text);
            if (roadThemes?.themes == null || roadThemes.themes.Length == 0 || roadThemes.themes[0].firstLevel != 1 ||
                roadThemes.blendLevels <= 0 || roadThemes.themes.Zip(roadThemes.themes.Skip(1),
                    (a,b) => a.firstLevel < b.firstLevel).Any(valid => !valid))
                throw new InvalidOperationException("Harita tema aralıkları geçersiz.");
            roadTints = roadThemes.themes.Select(theme => ParseRoadTint(theme.roadTint)).ToArray();
            backdropTints = roadThemes.themes.Select(theme => ParseRoadTint(theme.backdropTint)).ToArray();
            roadPlate = crystal.referenceTerrace;
            font = MewgicTypographySettings.Current.body;
            panel = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");

            var root = new GameObject("LevelMapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            Canvas = root.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(900, 1600);
            scaler.matchWidthOrHeight = .5f;

            fixedBackdrop = Rect(root.transform, "FixedMapEnvironment", Vector2.zero, Vector2.one).gameObject.AddComponent<RawImage>();
            fixedBackdrop.texture = crystal.background;
            fixedBackdrop.raycastTarget = false;
            fixedDetails = AddPlate(root.transform, "FixedCastleAndRiverbanks", true);
            SafeArea = Rect(root.transform, "MapSafeArea", Vector2.zero, Vector2.one);
            ApplySafeArea();

            Viewport = Rect(SafeArea, "MapViewport", new Vector2(0, 1 - PlateBottom), new Vector2(1, 1 - PlateTop));
            Viewport.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(0, 45);
            Viewport.gameObject.AddComponent<Image>().color = Color.clear;
            Scroll = Viewport.gameObject.AddComponent<MapScrollRect>();
            Scroll.horizontal = false;
            Scroll.movementType = ScrollRect.MovementType.Clamped;
            Scroll.scrollSensitivity = 85;
            Scroll.decelerationRate = .14f;
            Scroll.viewport = Viewport;
            Scroll.UserInput = () => { following = false; cameraVelocity = 0; };
            Content = Rect(Viewport, "ContinuousRoadContent", Vector2.up, Vector2.one);
            Content.pivot = new Vector2(.5f, 1);
            Scroll.content = Content;
            roadTrack = Rect(Content, "RoadTiles", Vector2.zero, Vector2.one);
            WorldRoot = roadTrack;
            roadJoins = Rect(Content, "BridgeConnections", Vector2.zero, Vector2.one);
            roadStones = Rect(Content, "LevelStones", Vector2.zero, Vector2.one);
            for (int i = 0; i < roadTiles.Length; i++) roadTiles[i] = CreateRoadTile(i);

            pawTrail = Rect(Content, "GoldenPawTrail", Vector2.zero, Vector2.one).gameObject.AddComponent<JourneyPawTrail>();
            pawTrail.raycastTarget = false;
            pawTrail.transform.SetSiblingIndex(roadStones.GetSiblingIndex());
            var brand = Panel(SafeArea, "MapTitle", new Vector2(.344f,.919f), new Vector2(.656f,.982f));
            Label(brand.transform, "Brand", "MEWGIC", 33, new Vector2(.06f,.39f), new Vector2(.94f,.98f));
            Label(brand.transform, "Title", L.V("common.map"), 23, new Vector2(.06f,.06f), new Vector2(.94f,.46f));
            var title = Panel(SafeArea, "RoadRange", new Vector2(.252f,.849f), new Vector2(.748f,.905f));
            roadRange = Label(title.transform, "Label", "", 27, new Vector2(.04f,.04f), new Vector2(.96f,.96f));
            Control(SafeArea, "MapHome", L.V("common.home"), new Vector2(.042f,.016f), new Vector2(.395f,.070f), () =>
            {
                GameplayLevelCatalog.ResetSelection();
                SceneNavigation.LoadOnce("Home");
            });
            Control(SafeArea, "FocusCurrent", L.V("map.focus"), new Vector2(.415f,.016f), new Vector2(.756f,.070f), FocusCurrent);
            var motionButton = Control(SafeArea, "JourneyMotion", "", new Vector2(.778f,.017f), new Vector2(.989f,.066f), () =>
            {
                if (ledger == null || !ledger.SetReducedMotion(!reducedJourneyMotion)) return;
                reducedJourneyMotion = ledger.ReducedMotion;
                UpdateMotionLabel();
                if (journeyRunning && reducedJourneyMotion) FinishRoadWalk();
                InvalidateRoad();
            });
            motionLabel = motionButton.GetComponentInChildren<TMPro.TMP_Text>();
            motionLabel.fontSizeMax = 22;

            BuildInformation();
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("MapEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            BindProgress();
            if (progression == null) return;
            reducedJourneyMotion = ledger.ReducedMotion;
            UpdateMotionLabel();
            Mounted = true;
            Canvas.ForceUpdateCanvases();
            int returnedFrom = resultLevel;
            resultLevel = 0;
            bool pending = ledger.PeekMapJourney(out walkFrom, out walkTo) &&
                walkTo == progression.CurrentId && (returnedFrom == 0 || returnedFrom == walkFrom || returnedFrom == walkTo - 1);
            awaitingJourney = pending;
            int focus = pending ? walkFrom : returnedFrom > 0 ? Mathf.Min(returnedFrom, progression.CurrentId) : progression.CurrentId;
            LayoutRoad(focus);
        }

        static Color ParseRoadTint(string value) => ColorUtility.TryParseHtmlString(value, out var tint)
            ? tint : throw new ArgumentException("Harita tema rengi geçersiz: " + value);

        MapPlateGraphic AddPlate(Transform parent, string name, bool fixedLayer)
        {
            var art = Rect(parent, name, Vector2.zero, Vector2.one).gameObject.AddComponent<MapPlateGraphic>();
            art.texture = roadPlate;
            art.FixedLayer = fixedLayer;
            art.raycastTarget = false;
            return art;
        }

        RoadTile CreateRoadTile(int index)
        {
            var tile = new RoadTile { root = Rect(roadTrack, "RoadTile_" + index, Vector2.up, Vector2.one) };
            tile.root.pivot = new Vector2(.5f, 1);
            tile.art = AddPlate(tile.root, "PaintedBridge", false);
            tile.art.rectTransform.offsetMin = Vector2.down * 14;
            tile.art.rectTransform.offsetMax = Vector2.up * 14;
            var join = Rect(roadJoins, "StoneConnection_" + index, Vector2.up, Vector2.one);
            join.pivot = new Vector2(.5f, 1);
            tile.join = join.gameObject.AddComponent<MapRoadJoinGraphic>();
            tile.join.texture = roadPlate;
            tile.join.raycastTarget = false;
            tile.join.enabled = false;
            tile.stones = Rect(roadStones, "StoneGroup_" + index, Vector2.up, Vector2.one);
            tile.stones.pivot = new Vector2(.5f, 1);
            for (int i = 0; i < RoadLevels; i++)
                tile.nodes[i] = Point(tile.stones, "LevelStone", MapPointKind.LevelSlot, node, 125);
            tile.chest = Point(tile.stones, "MapChest", MapPointKind.Chest, chest, 70);
            tile.root.gameObject.SetActive(false);
            tile.stones.gameObject.SetActive(false);
            return tile;
        }

        int TileOf(int level) => (Mathf.Clamp(level, 1, WorldMapRules.MaxLevel) - 1) / RoadLevels;

        void LayoutRoad(int level)
        {
            lastSafeSize = SafeArea.rect.size;
            roadWidth = Viewport.rect.width;
            roadHeight = Viewport.rect.height;
            if (roadWidth <= 0 || roadHeight <= 0) return;
            float crop = Mathf.Min(1, fixedBackdrop.texture.width * fixedBackdrop.rectTransform.rect.height /
                (fixedBackdrop.texture.height * fixedBackdrop.rectTransform.rect.width));
            fixedBackdrop.uvRect = new Rect(0, (1 - crop) * .5f, 1, crop);
            RoadJump(level);
            if (journeyRunning || arrivalLevel != 0 && stepCount > 0)
            {
                PreparePawSteps(preserveCount: true);
                if (!journeyRunning) PaintPawSteps(stepCount, journeyElapsed + Mathf.Min(1.75f, arrivalElapsed));
            }
        }

        void RebaseRoad(int tile, float progress)
        {
            rebasing = true;
            roadWindowSize = Mathf.Min(RoadWindow, MaxRoadTile + 1);
            roadBase = Mathf.Clamp(tile - RoadWindow / 2, 0, MaxRoadTile + 1 - roadWindowSize);
            Content.sizeDelta = new Vector2(0, roadWindowSize * roadHeight);
            WorldHeight = Content.sizeDelta.y;
            float offset = (tile - roadBase + progress) * roadHeight;
            Content.anchoredPosition = new Vector2(0, Mathf.Clamp(offset, 0, WorldHeight - roadHeight));
            rebasing = false;
            foreach (var part in roadTiles) part.index = -1;
            InvalidateRoad();
            BindRoad();
        }

        void RoadJump(int level)
        {
            if (!Mounted || progression == null || roadHeight <= 0) return;
            following = false;
            Scroll.StopMovement();
            RebaseRoad(Mathf.Min(MaxRoadTile, TileOf(level)), 0);
        }

        void InvalidateRoad() => paintedRevision = -1;

        void BindRoad()
        {
            if (rebasing || roadHeight <= 0) return;
            float position = Mathf.Clamp(Content.anchoredPosition.y / roadHeight, 0, roadWindowSize - 1);
            int lower = Mathf.FloorToInt(position);
            int first = roadBase + Mathf.Max(0, lower - 1);
            int last = Mathf.Min(MaxRoadTile, roadBase + lower + 2);
            int previous = visibleTile;
            visibleTile = roadBase + Mathf.RoundToInt(position);
            ActiveRegion = WorldMapRules.Island(visibleTile * RoadLevels + 1);
            SelectedChapter = visibleTile;
            if (boundFirst != first || boundLast != last || paintedRevision != ledger.Revision)
            {
                foreach (var tile in roadTiles)
                    if (tile.index < first || tile.index > last)
                    {
                        tile.index = -1;
                        tile.root.gameObject.SetActive(false);
                        tile.stones.gameObject.SetActive(false);
                        tile.join.enabled = false;
                    }
                for (int index = first; index <= last; index++)
                {
                    var tile = Array.Find(roadTiles, part => part.index == index) ??
                        Array.Find(roadTiles, part => part.index < 0);
                    if (tile == null) break;
                    if (tile.index != index || paintedRevision != ledger.Revision)
                        PaintRoadTile(tile, index);
                }
                paintedRevision = ledger.Revision;
                boundFirst = first;
                boundLast = last;
            }
            // A connection is visible as the camera passes between full terrace compositions.
            float offCenter = Mathf.Abs(position - Mathf.Round(position));
            float connectionAlpha = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.025f, .16f, offCenter));
            foreach (var tile in roadTiles)
                if (tile.join.enabled) tile.join.canvasRenderer.SetAlpha(connectionAlpha);
            double centerLevel = roadBase * (double)RoadLevels + position * RoadLevels + 5.5;
            BlendRoadTheme(centerLevel, out var tint, out _, out _);
            if (fixedBackdrop.color != tint)
            {
                fixedBackdrop.color = tint;
                fixedDetails.color = tint;
            }
            if (previous != visibleTile || roadRange.text.Length == 0)
            {
                int start = visibleTile * RoadLevels + 1;
                int end = Mathf.Min(start + RoadLevels - 1, WorldMapRules.MaxLevel);
                int headerTheme = ThemeFor(start);
                roadRange.text = $"{roadThemes.themes[headerTheme].title}\n{start:N0}–{end:N0}";
            }
        }

        void PaintRoadTile(RoadTile tile, int index)
        {
            tile.index = index;
            tile.root.sizeDelta = new Vector2(0, roadHeight);
            tile.root.anchoredPosition = Vector2.down * ((index - roadBase) * roadHeight);
            int first = index * RoadLevels + 1;
            BlendRoadTheme(first - .5, out _, out var roadStart, out _);
            BlendRoadTheme(first + 9.5, out _, out var roadEnd, out _);
            tile.art.SetRoadTints(roadStart, roadEnd);
            tile.join.enabled = index < MaxRoadTile;
            var join = tile.join.rectTransform;
            join.sizeDelta = new Vector2(0, roadHeight);
            join.anchoredPosition = tile.root.anchoredPosition;
            tile.join.SetTint(roadEnd);
            tile.root.gameObject.SetActive(true);
            tile.stones.sizeDelta = tile.root.sizeDelta;
            tile.stones.anchoredPosition = tile.root.anchoredPosition;
            tile.stones.gameObject.SetActive(true);
            for (int i = 0; i < RoadLevels; i++)
            {
                int id = first + i;
                var p = tile.nodes[i];
                p.name = "MapLevel_" + id;
                p.regionIndex = WorldMapRules.Island(id);
                p.slotIndex = (id - 1) % WorldMapRules.LevelsPerIsland;
                var rect = (RectTransform)p.transform;
                Vector2 foot = ReferenceWalk[i][0];
                rect.anchoredPosition = new Vector2(foot.x * roadWidth, -Band(foot.y - (i == 9 ? .034f : .024f)) * roadHeight);
                rect.sizeDelta = new Vector2(roadWidth * (i == 9 ? .18f : .145f), roadHeight * (i == 9 ? .093f : .078f));
                p.GetComponent<MapNodeBadge>().PresentJourney(id, progression.Status(id), progression.Stars(id),
                    !reducedJourneyMotion, i == 9 && id == progression.CurrentId, true,
                    id != walkTo || !journeyRunning && !awaitingJourney);
            }
            int last = first + RoadLevels - 1;
            bool milestone = last % WorldMapRules.LevelsPerIsland == 0;
            int region = WorldMapRules.Island(last);
            tile.chest.gameObject.SetActive(milestone && (progression.ChestReady(region) || progression.ChestClaimed(region)));
            if (tile.chest.gameObject.activeSelf)
            {
                tile.chest.regionIndex = region;
                ((RectTransform)tile.chest.transform).anchoredPosition = new Vector2(roadWidth * .86f, -roadHeight * .86f);
                tile.chest.GetComponent<MapNodeBadge>().PresentChest(progression.ChestReady(region), progression.ChestClaimed(region));
            }
        }

        int ThemeFor(double level)
        {
            int index = roadThemes.themes.Length - 1;
            while (index > 0 && level < roadThemes.themes[index].firstLevel) index--;
            return index;
        }

        void BlendRoadTheme(double level, out Color backdrop, out Color road, out int index)
        {
            index = ThemeFor(level);
            int next = Math.Min(roadThemes.themes.Length - 1, index + 1);
            float half = roadThemes.blendLevels * .5f;
            if (index > 0 && level < roadThemes.themes[index].firstLevel + half) { next = index; index--; }
            float t = index == next ? 0 : Mathf.SmoothStep(0, 1,
                Mathf.InverseLerp(-half, half, (float)(level - roadThemes.themes[next].firstLevel)));
            backdrop = Color.Lerp(backdropTints[index], backdropTints[next], t);
            road = Color.Lerp(roadTints[index], roadTints[next], t);
        }

        internal static float Band(float y) => (y - PlateTop) / (PlateBottom - PlateTop);

        Vector2 RoadPosition(int id, float phase)
        {
            int slot = (id - 1) % RoadLevels;
            var route = ReferenceWalk[slot];
            float f = Mathf.Clamp01(phase) * (route.Length - 1);
            int i = Mathf.Min(Mathf.FloorToInt(f), route.Length - 2);
            var a = route[i];
            var b = route[i + 1];
            float t = f - i;
            var before = route[Mathf.Max(0, i - 1)];
            var after = route[Mathf.Min(route.Length - 1, i + 2)];
            var chord = b - a;
            var p = Vector2.Lerp(a, b, t) + (((b - before) * .5f - chord) * (1 - t) -
                ((after - a) * .5f - chord) * t) * (t * (1 - t));
            return new Vector2(p.x * roadWidth, -(TileOf(id) - roadBase + Band(p.y)) * roadHeight);
        }

        Vector2 WalkPosition(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (walkFrom == walkTo) return RoadPosition(walkTo, 0);
            if (TileOf(walkFrom) != TileOf(walkTo))
            {
                // Follow the painted bridge to the join, ending at the next terrace's first stone.
                if (progress < .65f) return RoadPosition(walkFrom, Mathf.Lerp(WalkStartFraction, 1, progress / .65f));
                var exit = RoadPosition(walkFrom, 1);
                var entry = RoadPosition(walkTo, 0);
                float t = (progress - .65f) / .35f;
                return new Vector2(MapRoadJoinGraphic.Center(t) * roadWidth, Mathf.Lerp(exit.y, entry.y, t));
            }
            float f = Mathf.Lerp(WalkStartFraction, walkTo - walkFrom, progress);
            int whole = Mathf.FloorToInt(f);
            return RoadPosition(walkFrom + whole, f - whole);
        }

        void PreparePawSteps(bool preserveCount = false)
        {
            // Measure the already painted route, including bridge joins, in canvas units.
            Vector2 previous = WalkPosition(0);
            for (int i = 1; i <= TrailSamples; i++)
            {
                Vector2 point = WalkPosition((float)i / TrailSamples);
                trailDistances[i] = trailDistances[i - 1] + Vector2.Distance(previous, point);
                previous = point;
            }
            float length = trailDistances[TrailSamples];
            if (TileOf(walkFrom) == TileOf(walkTo))
                length -= Mathf.Min(length * .22f, roadWidth * .080f);
            if (!preserveCount)
                stepCount = Mathf.Clamp(Mathf.RoundToInt(length / (roadWidth * .052f)) + 1, 2, stepPhases.Length);

            int sample = 1;
            for (int i = 0; i < stepCount; i++)
            {
                float distance = length * i / (stepCount - 1);
                while (sample < TrailSamples && trailDistances[sample] < distance) sample++;
                stepPhases[i] = (sample - 1 + Mathf.InverseLerp(trailDistances[sample - 1],
                    trailDistances[sample], distance)) / TrailSamples;
            }
        }

        void PaintPawSteps(int shown, float elapsed)
        {
            int first = Mathf.Max(0, shown - 12);
            int count = 0;
            for (int i = first; i < shown; i++)
            {
                float phase = stepPhases[i];
                Vector2 point = WalkPosition(phase);
                Vector2 tangent = WalkPosition(phase + .006f) - WalkPosition(phase - .006f);
                float age = elapsed - (FirstStepDelay + i * StepInterval);
                float settle = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .95f, age));
                float remainder = arrivalLevel != 0 && i >= stepCount - 5 ? .56f : 0;
                float light = Mathf.Max(remainder, 1 - settle);
                float alpha = Mathf.Lerp(remainder, 1, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.56f, 1.75f, age)));
                float depth = Mathf.Repeat(-point.y / roadHeight, 1);
                float size = roadWidth * Mathf.Lerp(.029f, .041f, depth);
                pawTrail.Set(count++, point, tangent, size, alpha, light);
            }
            pawTrail.Render(count);
        }

        void BeginRoadWalk()
        {
            awaitingJourney = false;
            if (reducedJourneyMotion) { FinishRoadWalk(); return; }
            // A backlog of many wins moves through the last two stones of the same continuous road.
            if (walkTo - walkFrom > 3 || TileOf(walkTo) != TileOf(walkFrom) && walkTo - walkFrom != 1)
            {
                walkFrom = walkTo - 1;
                RoadJump(walkFrom);
            }
            arrivalLevel = 0;
            PreparePawSteps();
            InvalidateRoad();
            journeyElapsed = 0;
            journeyRunning = true;
            Scroll.StopMovement();
            Scroll.enabled = false;
        }

        void FinishRoadWalk()
        {
            if (walkTo <= 0 || (!journeyRunning && !awaitingJourney && !ledger.PeekMapJourney(out _, out _))) return;
            awaitingJourney = false;
            journeyRunning = false;
            Scroll.enabled = true;
            RoadJump(walkTo);
            arrivalLevel = walkTo;
            arrivalElapsed = 0;
            if (!ledger.AcknowledgeMapJourney(walkTo)) Message(L.V("error.save"));
            if (reducedJourneyMotion || journeyElapsed < FirstStepDelay + (stepCount - 1) * StepInterval)
            {
                stepCount = 0;
                pawTrail.Clear();
            }
        }

        void TickRoad()
        {
            if (!Mounted || progression == null) return;
            if (screenWidth != Screen.width || screenHeight != Screen.height || nativeSafe != Screen.safeArea) ApplySafeArea();
            if (lastSafeSize != SafeArea.rect.size)
                LayoutRoad(journeyRunning ? walkFrom : visibleTile >= 0 ? visibleTile * RoadLevels + 1 : progression.CurrentId);
            if (awaitingJourney && L.Ready) BeginRoadWalk();
            float dt = Mathf.Min(Time.unscaledDeltaTime, .07f);
            if (journeyRunning)
            {
                journeyElapsed += dt;
                int shown = Mathf.Clamp(Mathf.FloorToInt((journeyElapsed - FirstStepDelay) / StepInterval) + 1, 0, stepCount);
                PaintPawSteps(shown, journeyElapsed);
                float lastStep = FirstStepDelay + (stepCount - 1) * StepInterval;
                float moveTravel = shown == 0 ? 0 : Mathf.Lerp(stepPhases[shown - 1], stepPhases[Mathf.Min(shown, stepCount - 1)],
                    Mathf.Repeat(journeyElapsed - FirstStepDelay, StepInterval) / StepInterval);
                if (TileOf(walkTo) != TileOf(walkFrom))
                {
                    float offset = (TileOf(walkFrom) - roadBase + moveTravel) * roadHeight;
                    Content.anchoredPosition = Vector2.up * Mathf.Clamp(offset, 0, WorldHeight - roadHeight);
                }
                if (journeyElapsed >= lastStep + .12f) FinishRoadWalk();
            }
            else
            {
                if (following)
                {
                    float value = Mathf.SmoothDamp(Content.anchoredPosition.y, focusGoal, ref cameraVelocity, .24f, Mathf.Infinity, dt);
                    Content.anchoredPosition = Vector2.up * value;
                    if (Mathf.Abs(value - focusGoal) < 1) { Content.anchoredPosition = Vector2.up * focusGoal; following = false; }
                }
                else if (!Scroll.Dragging)
                {
                    float offset = Content.anchoredPosition.y / roadHeight;
                    int lower = Mathf.Clamp(Mathf.FloorToInt(offset), 0, roadWindowSize - 1);
                    bool nearEdge = lower < 2 && roadBase > 0 ||
                        lower >= roadWindowSize - 3 && roadBase + roadWindowSize - 1 < MaxRoadTile;
                    if (nearEdge)
                    {
                        Vector2 speed = Scroll.velocity;
                        RebaseRoad(roadBase + lower, Mathf.Clamp01(offset - lower));
                        Scroll.velocity = speed;
                    }
                }
            }
            BindRoad();
            if (arrivalLevel != 0 && !journeyRunning)
            {
                float previous = arrivalElapsed;
                arrivalElapsed += dt;
                if (stepCount > 0 && (arrivalElapsed < 1.75f || previous < 1.75f || trailRoadBase != roadBase))
                {
                    PaintPawSteps(stepCount, journeyElapsed + Mathf.Min(1.75f, arrivalElapsed));
                    trailRoadBase = roadBase;
                }
                if (previous < .95f)
                {
                    var tile = Array.Find(roadTiles, part => part.index == TileOf(arrivalLevel));
                    float pulse = reducedJourneyMotion ? 0 : 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(arrivalElapsed / .95f));
                    tile?.nodes[(arrivalLevel - 1) % RoadLevels].GetComponent<MapNodeBadge>().SetArrivalLight(pulse);
                }
            }
        }

        void FocusRoadCurrent()
        {
            if (progression == null) return;
            if (journeyRunning || awaitingJourney) { FinishRoadWalk(); return; }
            int tile = TileOf(progression.CurrentId);
            if (reducedJourneyMotion || tile < roadBase || tile >= roadBase + roadWindowSize)
                RoadJump(progression.CurrentId);
            else
            {
                Scroll.StopMovement();
                focusGoal = (tile - roadBase) * roadHeight;
                cameraVelocity = 0;
                following = true;
            }
        }
    }
}
