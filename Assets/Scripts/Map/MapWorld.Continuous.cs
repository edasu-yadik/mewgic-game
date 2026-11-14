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
        const int RoadLevels = ContinuousMapRoute.LevelsPerChunk;
        [SerializeField, Range(1, 3)] int preloadChunks = 1;
        int RoadWindow => 7 + preloadChunks * 2;
        const float PlateTop = ContinuousMapRoute.PlateTop, PlateBottom = ContinuousMapRoute.PlateBottom;
        const int LastRoadTile = (WorldMapRules.MaxLevel - 1) / RoadLevels;

        [Serializable] sealed class RoadTheme
        {
            public int firstLevel, transitionStart, transitionEnd;
            public string title, roadTint, backdropTint;
            public float villageVista;
        }
        [Serializable] sealed class RoadThemeConfig
        {
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

        RoadTile[] roadTiles = Array.Empty<RoadTile>();
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
        float focusGoal, focusFadeTime;
        CanvasGroup focusCurtain;
        bool focusReposition, focusMoved;
        internal bool ReturningToCurrent => following || focusReposition;
        int focusTile;
        Texture2D villageBackdrop;
        MapPlateGraphic villageScenery;
        Material roadWater;
        Texture2D roadPlate;
        int MaxRoadTile => Mathf.Min(LastRoadTile, (progression.CurrentId - 1) / RoadLevels + 1);

        void MountContinuous()
        {
            if (Mounted) return;
            var crystal = Resources.Load<MapJourneyTheme>("Map/JourneyThemes/crystal");
            if (crystal == null || crystal.background == null || crystal.referenceTerrace == null)
                throw new InvalidOperationException("Tek harita için çevre veya yol kaynağı eksik.");
            var asset = Resources.Load<TextAsset>("Map/ContinuousThemes");
            roadThemes = asset == null ? null : JsonUtility.FromJson<RoadThemeConfig>(asset.text);
            if (roadThemes?.themes == null || roadThemes.themes.Length == 0 || roadThemes.themes[0].firstLevel != 1 ||
                roadThemes.themes.Zip(roadThemes.themes.Skip(1),
                    (a,b) => a.firstLevel < b.firstLevel && b.transitionStart >= a.firstLevel &&
                        b.transitionStart >= a.transitionEnd && b.transitionStart < b.firstLevel &&
                        b.transitionEnd >= b.firstLevel).Any(valid => !valid))
                throw new InvalidOperationException("Harita tema aralıkları geçersiz.");
            roadTints = roadThemes.themes.Select(theme => ParseRoadTint(theme.roadTint)).ToArray();
            backdropTints = roadThemes.themes.Select(theme => ParseRoadTint(theme.backdropTint)).ToArray();
            roadPlate = crystal.referenceTerrace;
            villageBackdrop = crystal.villageVista;
            if (waterLightShader != null && crystal.flowingWater)
                roadWater = new Material(waterLightShader);
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
            if (roadWater != null) fixedBackdrop.material = roadWater;
            fixedBackdrop.raycastTarget = false;
            fixedDetails = AddPlate(root.transform, "FixedCastleAndRiverbanks", true);
            villageScenery = AddPlate(root.transform, "FixedVillageVista", true);
            villageScenery.texture = villageBackdrop;
            villageScenery.VillageLayer = true;
            villageScenery.enabled = false;
            SafeArea = Rect(root.transform, "MapSafeArea", Vector2.zero, Vector2.one);
            ApplySafeArea();

            Viewport = Rect(SafeArea, "MapViewport", new Vector2(0, 1 - PlateBottom), new Vector2(1, 1 - PlateTop));
            Viewport.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(0, 45);
            Viewport.gameObject.AddComponent<Image>().color = Color.clear;
            Scroll = Viewport.gameObject.AddComponent<MapScrollRect>();
            Scroll.horizontal = false;
            Scroll.movementType = ScrollRect.MovementType.Clamped;
            Scroll.scrollSensitivity = 85;
            Scroll.decelerationRate = .095f;
            Scroll.viewport = Viewport;
            Scroll.UserInput = () =>
            {
                following = false;
                cameraVelocity = 0;
                focusReposition = false;
                focusCurtain.alpha = 0;
            };
            Content = Rect(Viewport, "ContinuousRoadContent", Vector2.up, Vector2.one);
            Content.pivot = new Vector2(.5f, 1);
            Scroll.content = Content;
            roadTrack = Rect(Content, "RoadTiles", Vector2.zero, Vector2.one);
            WorldRoot = roadTrack;
            roadJoins = Rect(Content, "BridgeConnections", Vector2.zero, Vector2.one);
            roadJoins.SetSiblingIndex(roadTrack.GetSiblingIndex());
            roadStones = Rect(Content, "LevelStones", Vector2.zero, Vector2.one);
            roadTiles = new RoadTile[4 + 2 * preloadChunks];
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

            var curtain = Rect(root.transform, "DistantFocusVeil", Vector2.zero, Vector2.one);
            var veil = curtain.gameObject.AddComponent<Image>();
            veil.color = new Color(.074f, .044f, .168f, 1);
            veil.raycastTarget = false;
            focusCurtain = curtain.gameObject.AddComponent<CanvasGroup>();
            focusCurtain.alpha = 0;
            focusCurtain.blocksRaycasts = false;
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
            if (roadWater != null) art.material = roadWater;
            art.FixedLayer = fixedLayer;
            art.raycastTarget = false;
            return art;
        }

        RoadTile CreateRoadTile(int index)
        {
            var tile = new RoadTile { root = Rect(roadTrack, "RoadTile_" + index, Vector2.up, Vector2.one) };
            tile.root.pivot = new Vector2(.5f, 1);
            tile.art = AddPlate(tile.root, "PaintedBridge", false);
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
            float oldHeight = roadHeight;
            float oldPosition = oldHeight > 0 ? Content.anchoredPosition.y / oldHeight : 0;
            roadWidth = Viewport.rect.width;
            roadHeight = Viewport.rect.height;
            if (roadWidth <= 0 || roadHeight <= 0) return;
            float crop = Mathf.Min(1, fixedBackdrop.texture.width * fixedBackdrop.rectTransform.rect.height /
                (fixedBackdrop.texture.height * fixedBackdrop.rectTransform.rect.width));
            fixedBackdrop.uvRect = new Rect(0, (1 - crop) * .5f, 1, crop);
            InvalidateRoad();
            if (oldHeight > 0 && !journeyRunning)
                RebaseRoad(roadBase + Mathf.FloorToInt(oldPosition), Mathf.Repeat(oldPosition, 1));
            else RoadJump(level);
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
            Scroll.Rebase(Vector2.up * Mathf.Clamp(offset, 0, WorldHeight - roadHeight));
            foreach (var part in roadTiles)
                if (part.index >= 0) PositionRoadTile(part);
            rebasing = false;
            boundFirst = boundLast = -1;
            BindRoad();
        }

        void RoadJump(int level)
        {
            if (!Mounted || progression == null || roadHeight <= 0) return;
            following = false;
            focusReposition = false;
            focusCurtain.alpha = 0;
            Scroll.StopMovement();
            RebaseRoad(Mathf.Min(MaxRoadTile, TileOf(level)), 0);
        }

        void InvalidateRoad() => paintedRevision = -1;

        void BindRoad()
        {
            if (rebasing || roadHeight <= 0) return;
            float position = Mathf.Clamp(Content.anchoredPosition.y / roadHeight, 0, roadWindowSize - 1);
            int lower = Mathf.FloorToInt(position);
            // The previous chunk can still own a connector inside this viewport.
            int first = Mathf.Max(0, roadBase + lower - 1 - preloadChunks);
            int last = Mathf.Min(MaxRoadTile, roadBase + lower + 1 + preloadChunks);
            int revision = ledger.Revision;
            int previous = visibleTile;
            visibleTile = roadBase + Mathf.RoundToInt(position);
            ActiveRegion = WorldMapRules.Island(visibleTile * RoadLevels + 1);
            SelectedChapter = visibleTile;
            if (boundFirst != first || boundLast != last || paintedRevision != revision)
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
                    if (tile.index != index || paintedRevision != revision)
                        PaintRoadTile(tile, index);
                }
                paintedRevision = revision;
                boundFirst = first;
                boundLast = last;
            }
            double centerLevel = roadBase * (double)RoadLevels + position * RoadLevels + 5.5;
            BlendRoadTheme(centerLevel, out var tint, out _, out _, out float vista);
            fixedBackdrop.color = tint;
            fixedDetails.canvasRenderer.SetAlpha(1 - vista * .88f);
            villageScenery.enabled = villageBackdrop != null && vista > .001f;
            if (villageScenery.enabled) villageScenery.canvasRenderer.SetAlpha(vista);
            if (previous != visibleTile || roadRange.text.Length == 0)
            {
                int start = visibleTile * RoadLevels + 1;
                int end = Mathf.Min(start + RoadLevels - 1, WorldMapRules.MaxLevel);
                int headerTheme = ThemeFor(start);
                roadRange.text = $"{roadThemes.themes[headerTheme].title}\n{start:N0}–{end:N0}";
            }
        }

        void PositionRoadTile(RoadTile tile)
        {
            Vector2 size = Vector2.up * roadHeight;
            Vector2 position = Vector2.down * ((tile.index - roadBase) * roadHeight);
            tile.root.sizeDelta = tile.stones.sizeDelta = tile.join.rectTransform.sizeDelta = size;
            tile.root.anchoredPosition = tile.stones.anchoredPosition = tile.join.rectTransform.anchoredPosition = position;
        }

        void PaintRoadTile(RoadTile tile, int index)
        {
            tile.index = index;
            PositionRoadTile(tile);
            int first = index * RoadLevels + 1;
            BlendRoadTheme(first - .5, out _, out var roadStart, out _, out _);
            BlendRoadTheme(first + 9.5, out _, out var roadEnd, out _, out _);
            BlendRoadTheme(first + RoadLevels - .5, out _, out var nextRoad, out _, out _);
            tile.art.SetChunk(index);
            tile.art.SetRoadTints(roadStart, roadEnd);
            tile.join.enabled = index < MaxRoadTile;
            tile.join.SetTints(roadEnd, nextRoad);
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
                Vector2 foot = ContinuousMapRoute.Node(i);
                foot.x += ContinuousMapRoute.Warp(index, foot.y);
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

        void BlendRoadTheme(double level, out Color backdrop, out Color road, out int index, out float vista)
        {
            index = ThemeFor(level);
            int next = index;
            if (index > 0 && level < roadThemes.themes[index].transitionEnd) index--;
            else if (index + 1 < roadThemes.themes.Length && level >= roadThemes.themes[index + 1].transitionStart)
                next++;
            var entering = roadThemes.themes[next];
            float t = index == next ? 0 : Mathf.SmoothStep(0, 1,
                Mathf.Clamp01((float)((level - entering.transitionStart) /
                    (entering.transitionEnd - (double)entering.transitionStart))));
            backdrop = Color.Lerp(backdropTints[index], backdropTints[next], t);
            road = Color.Lerp(roadTints[index], roadTints[next], t);
            if (level > roadThemes.themes[^1].firstLevel + 50)
            {
                float variation = .5f + .5f * Mathf.Sin((float)(level % 480 / 480 * (2 * Math.PI)));
                road *= Color.Lerp(Color.white, new Color(.92f, .94f, 1), variation);
            }
            vista = Mathf.Lerp(roadThemes.themes[index].villageVista, entering.villageVista, t);
        }

        internal static float Band(float y) => ContinuousMapRoute.Band(y);

        Vector2 RoadPosition(int id, float phase)
        {
            int chunk = TileOf(id);
            var point = ContinuousMapRoute.Road((id - 1) % RoadLevels, phase);
            point.x += ContinuousMapRoute.Warp(chunk, point.y);
            return new Vector2(point.x * roadWidth, -(chunk - roadBase + Band(point.y)) * roadHeight);
        }

        Vector2 WalkPosition(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (walkFrom == walkTo) return RoadPosition(walkTo, 0);
            if (TileOf(walkFrom) != TileOf(walkTo))
            {
                // Follow the painted bridge to the join, ending at the next terrace's first stone.
                if (progress < .65f) return RoadPosition(walkFrom, Mathf.Lerp(WalkStartFraction, 1, progress / .65f));
                var join = ContinuousMapRoute.Join((progress - .65f) / .35f);
                return new Vector2(join.x * roadWidth, -(TileOf(walkFrom) - roadBase + join.y) * roadHeight);
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
            if (roadWater != null) roadWater.SetFloat("_EnvironmentTime", EnvironmentTime);
            if (screenWidth != Screen.width || screenHeight != Screen.height || nativeSafe != Screen.safeArea) ApplySafeArea();
            if (lastSafeSize != SafeArea.rect.size)
                LayoutRoad(journeyRunning ? walkFrom : visibleTile >= 0 ? visibleTile * RoadLevels + 1 : progression.CurrentId);
            if (awaitingJourney && L.Ready) BeginRoadWalk();
            float dt = Mathf.Min(Time.unscaledDeltaTime, .07f);
            if (focusReposition)
            {
                focusFadeTime += dt;
                focusCurtain.alpha = 1 - Mathf.Abs(Mathf.Clamp01(focusFadeTime / .36f) * 2 - 1);
                if (focusFadeTime >= .18f && !focusMoved)
                {
                    focusMoved = true;
                    focusFadeTime = .18f;
                    focusCurtain.alpha = 1;
                    RebaseRoad(Mathf.Max(0, focusTile - 1), .45f);
                    focusGoal = (focusTile - roadBase) * roadHeight;
                    following = true;
                }
                if (focusFadeTime >= .36f) focusReposition = false;
            }
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
                    float value = Mathf.SmoothDamp(Content.anchoredPosition.y, focusGoal, ref cameraVelocity, .22f, Mathf.Infinity, dt);
                    Content.anchoredPosition = Vector2.up * value;
                    if (!focusReposition && Mathf.Abs(value - focusGoal) < 1)
                    {
                        Content.anchoredPosition = Vector2.up * focusGoal;
                        following = false;
                        HighlightRoadCurrent();
                    }
                }
                else
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
                if (previous < JourneyStoneRing.ArrivalDuration)
                {
                    var tile = Array.Find(roadTiles, part => part.index == TileOf(arrivalLevel));
                    float lightTime = reducedJourneyMotion ? JourneyStoneRing.ArrivalDuration : arrivalElapsed;
                    tile?.nodes[(arrivalLevel - 1) % RoadLevels].GetComponent<MapNodeBadge>().SetArrivalLight(lightTime);
                }
            }
        }

        void HighlightRoadCurrent()
        {
            if (reducedJourneyMotion) return;
            if (arrivalLevel != progression.CurrentId)
            {
                stepCount = 0;
                pawTrail.Clear();
            }
            arrivalLevel = progression.CurrentId;
            arrivalElapsed = 0;
        }

        void FocusRoadCurrent()
        {
            if (progression == null) return;
            if (journeyRunning || awaitingJourney) { FinishRoadWalk(); return; }
            focusTile = TileOf(progression.CurrentId);
            Scroll.StopMovement();
            cameraVelocity = 0;
            if (reducedJourneyMotion)
            {
                RoadJump(progression.CurrentId);
                return;
            }
            if (focusTile < roadBase || focusTile >= roadBase + roadWindowSize)
            {
                following = false;
                focusReposition = true;
                focusMoved = false;
                focusFadeTime = 0;
                return;
            }
            focusGoal = (focusTile - roadBase) * roadHeight;
            focusReposition = false;
            focusCurtain.alpha = 0;
            following = true;
        }
    }
}
