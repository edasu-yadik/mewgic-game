using System;
using Mewgic.Core;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic
{
    public sealed class GameplayEndScreen : MonoBehaviour
    {
        const float ArtWidth = 941, ArtHeight = 1672, RevealEnd = 1.65f;
        static readonly float[] RevealTimes = { .18f, .49f, .83f };
        static readonly Vector2[] StarCenters = { new(329, 400), new(471, 368), new(613, 400) };
        static readonly Color Cream = new(1f, .94f, .77f);
        readonly RawImage[] stars = new RawImage[3];
        readonly VictoryLight[] halos = new VictoryLight[3], sparks = new VictoryLight[12];
        readonly RawImage[] bleed = new RawImage[8];
        readonly float[] edgesX = new float[4], edgesY = new float[4];
        RectTransform safeArea, overlay, root;
        Rect lastSafe;
        Vector2 lastSize;
        Texture2D plate;
        VictoryLight sweep, coinGlint;
        VictoryEdgeArt skyEdge, gardenEdge;
        TMP_Text reward, adTitle, adTotal, notice, skipLabel, nextLabel;
        Button next, replay, map, watch, skip;
        Func<bool> paused;
        ProgressLedger ledger;
        string receipt, message;
        int level, awarded, coins, sounded, displayedCoins;
        float clock;
        RewardedVictoryAdState? lastAdState;
        IRewardedVictoryAd activeAd;
        bool lastPresenting;
        bool showing, saveOk, canAdvance, preparing, verifiedAd, boosted, starsSettled;

        public GameObject Panel => root.gameObject;
        public bool IsShowing => showing;
        public bool IsWatchingAd => activeAd?.IsPresenting == true;

        public void Mount(RectTransform safe, Action replayLevel, Action advance, Action returnToMap,
            Func<bool> isPaused, Action retrySave)
        {
            paused = isPaused;
            plate = Resources.Load<Texture2D>("Gameplay/Victory/ReferencePlate");
            var star = Resources.Load<Texture2D>("Gameplay/Victory/ReferenceStar");
            safeArea = safe;
            overlay = GameplayTools.Rect(safe.parent, "VictoryBackdrop", Vector2.zero, Vector2.one);
            var blocker = overlay.gameObject.AddComponent<Image>();
            blocker.color = new Color(.075f, .033f, .15f);
            blocker.raycastTarget = true;
            for (int i = 0; i < bleed.Length; i++)
            {
                bleed[i] = GameplayTools.Rect(overlay, "VictoryEdge", Vector2.zero, Vector2.zero)
                    .gameObject.AddComponent<RawImage>();
                bleed[i].texture = plate;
                bleed[i].raycastTarget = false;
            }
            root = GameplayTools.Rect(overlay, "Win panel", Vector2.zero, Vector2.one);

            Art("ReferenceArt", plate, 0, 0, ArtWidth, ArtHeight);
            for (int i = 0; i < stars.Length; i++)
            {
                Vector2 c = StarCenters[i];
                float size = i == 1 ? 173 : 135;
                halos[i] = Light("StarHalo", c, size * 1.67f, false);
                var starSlot = Place("StarSlot" + i, c.x - size/2, c.y - size/2, c.x + size/2, c.y + size/2);
                stars[i] = GameplayTools.Rect(starSlot, "AwardStar" + (i + 1), Vector2.zero, Vector2.one)
                    .gameObject.AddComponent<RawImage>();
                stars[i].texture = star;
                stars[i].raycastTarget = false;
                var fit = stars[i].gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectRatio = 1;
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                stars[i].rectTransform.localRotation = Quaternion.Euler(0, 0, i == 0 ? 10 : i == 2 ? -10 : 0);
            }
            for (int i = 0; i < sparks.Length; i++)
                sparks[i] = Light("GoldParticle" + i, StarCenters[i / 4], 14 + i % 4 * 4, true);

            var title = Caption("Success", L.V("victory.title"), 75, 219, 451, 723, 599);
            title.fontStyle = FontStyles.Bold | FontStyles.Italic;
            title.textWrappingMode = TextWrappingModes.Normal;
            title.lineSpacing = -13;
            reward = Caption("Reward", "", 49, 243, 800, 696, 878);
            reward.richText = true;

            watch = Control("WatchRewardedAd", WatchAd, 154, 932, 788, 1100);
            var watchColors = watch.colors;
            watchColors.disabledColor = Color.white;
            watch.colors = watchColors;
            adTitle = Caption("WatchTitle", "", 49, 318, 950, 641, 1014);
            adTotal = Caption("WatchTotal", "", 31, 317, 1015, 647, 1064);
            adTotal.richText = true;
            sweep = Light("AdGoldSweep", new Vector2(200, 939), 72, true);
            coinGlint = Light("AdCoinGlint", new Vector2(727, 990), 44, true);

            next = Control("NextLevel", advance, 284, 1148, 728, 1267);
            nextLabel = Caption("NextLabel", "", 48, 328, 1167, 652, 1243);
            replay = Control("Replay", replayLevel, 262, 1301, 504, 1374);
            Caption("ReplayLabel", L.V("victory.replay"), 33, 327, 1306, 491, 1368);
            map = Control("ResultHome", returnToMap, 522, 1301, 786, 1374);
            Caption("MapLabel", GetComponent<PrototypeApp>()?.IsTutorialReplay == true ?
                L.V("common.home") : L.V("victory.map"), 29, 599, 1306, 774, 1368);
            notice = Caption("RewardBreakdown", "", 25, 266, 1374, 771, 1412);
            skip = Control("SkipReward", () =>
            {
                if (!saveOk) retrySave();
                else
                {
                    clock = RevealEnd;
                    sounded = awarded;
                    Animate();
                }
            }, 275, 888, 666, 931, false);
            skipLabel = Caption("SkipLabel", "", 24, 282, 891, 659, 930);
            skyEdge = MakeEdge("ReferenceSkyEdge", true);
            gardenEdge = MakeEdge("ReferenceGardenEdge", false);
            Hide();
        }

        VictoryEdgeArt MakeEdge(string name, bool top)
        {
            var obj = GameplayTools.Rect(overlay, name, Vector2.zero, Vector2.one).gameObject;
            obj.AddComponent<CanvasRenderer>();
            var edge = obj.AddComponent<VictoryEdgeArt>();
            edge.texture = Resources.Load<Texture2D>("Gameplay/Victory/" + name);
            edge.Top = top;
            edge.raycastTarget = false;
            return edge;
        }

        static Vector2 Point(float x, float y) => new(x / ArtWidth, 1 - y / ArtHeight);
        RectTransform Place(string name, float x0, float y0, float x1, float y1) =>
            GameplayTools.Rect(root, name, Point(x0, y1), Point(x1, y0));

        RawImage Art(string name, Texture2D texture, float x0, float y0, float x1, float y1)
        {
            var art = Place(name, x0, y0, x1, y1).gameObject.AddComponent<RawImage>();
            art.texture = texture;
            art.raycastTarget = false;
            return art;
        }

        Button Control(string name, Action action, float x0, float y0, float x1, float y1, bool painted = true)
        {
            var face = Art(name, painted ? plate : null, x0, y0, x1, y1);
            face.uvRect = new Rect(Point(x0, y1), new Vector2((x1 - x0) / ArtWidth, (y1 - y0) / ArtHeight));
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            if (painted)
            {
                var colors = button.colors;
                colors.pressedColor = new Color(.87f, .76f, .94f);
                colors.disabledColor = new Color(.66f, .58f, .71f);
                button.colors = colors;
            }
            else
            {
                face.color = Color.clear;
                button.transition = Selectable.Transition.None;
            }
            button.onClick.AddListener(() => { if (!paused() && button.IsInteractable()) action(); });
            return button;
        }

        TMP_Text Caption(string name, string text, int size, float x0, float y0, float x1, float y1)
        {
            var label = Place(name, x0, y0, x1, y1).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();
            label.font = MewgicTypographySettings.Current.heading;
            label.text = text;
            label.fontSize = label.fontSizeMax = size;
            label.fontSizeMin = 14;
            label.enableAutoSizing = true;
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Truncate;
            label.color = Cream;
            label.extraPadding = true;
            label.raycastTarget = false;
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.17f, .025f, .21f, .85f);
            shadow.effectDistance = new Vector2(1, -2);
            return label;
        }

        VictoryLight Light(string name, Vector2 point, float size, bool rays)
        {
            var obj = Place(name, point.x - size/2, point.y - size/2, point.x + size/2, point.y + size/2).gameObject;
            obj.AddComponent<CanvasRenderer>();
            var light = obj.AddComponent<VictoryLight>();
            light.Rays = rays;
            light.raycastTarget = false;
            return light;
        }

        public void Show(int completedLevel, int starCount, bool saved, bool hasNext,
            CompletionReward payout, ProgressLedger progress, string victoryReceipt)
        {
            if (showing) return;
            level = completedLevel;
            ledger = progress;
            receipt = victoryReceipt;
            coins = saved ? payout.Coins : 0;
            awarded = Mathf.Clamp(starCount, 0, stars.Length);
            displayedCoins = -1;
            showing = true;
            saveOk = saved;
            canAdvance = hasNext;
            verifiedAd = boosted = starsSettled = false;
            clock = GameFeel.Reduced ? RevealEnd : 0;
            sounded = GameFeel.Reduced ? awarded : 0;
            overlay.gameObject.SetActive(true);
            overlay.SetAsLastSibling();
            Layout();

            nextLabel.text = L.V(hasNext ? "victory.next" : "gameplay.last");
            activeAd = null;
            lastAdState = null;
            skipLabel.text = L.V(saved ? "economy.skip" : "economy.retry_save");
            SetTransition(false, saved ? null : L.V("error.save"));
            if (saved) GameFeel.Play(GameCue.Victory);
            Animate();
        }

        bool CanWatch => saveOk && !boosted && ledger?.CanBoostVictory(level, receipt) == true;

        public void SetTransition(bool loading, string status = null)
        {
            preparing = loading;
            message = status;
            notice.text = status ?? "";
            RefreshAdPanel(true);
            UpdateSkip();
        }

        void RefreshAdPanel(bool force = false)
        {
            var state = RewardedVictoryAds.Provider?.State ?? RewardedVictoryAdState.Unavailable;
            bool presenting = IsWatchingAd;
            if (!force && lastAdState == state && lastPresenting == presenting) return;
            lastAdState = state;
            lastPresenting = presenting;
            next.interactable = canAdvance && !preparing && !presenting;
            replay.interactable = map.interactable = !preparing && !presenting;
            watch.interactable = CanWatch && !preparing && activeAd == null &&
                (verifiedAd || state == RewardedVictoryAdState.Ready);
            adTitle.text = L.V(boosted ? "victory.ad_claimed" : verifiedAd ? "economy.retry_save" : "victory.ad");
            adTotal.text = boosted ? L.V("victory.coins", coins) :
                activeAd != null ? L.V(presenting ? "victory.ad_playing" : "victory.ad_verifying") :
                !CanWatch ? L.V("victory.ad_first") :
                verifiedAd || state == RewardedVictoryAdState.Ready
                    ? L.V("victory.ad_total", ProgressLedger.RewardedVictoryTotal)
                    : L.V(state switch
                    {
                        RewardedVictoryAdState.Loading => "victory.ad_loading",
                        RewardedVictoryAdState.Offline => "victory.ad_offline",
                        _ => "victory.ad_unavailable"
                    });
        }

        void WatchAd()
        {
            if (!watch.interactable) return;
            if (verifiedAd)
            {
                ApplyAdReward();
                return;
            }
            var provider = RewardedVictoryAds.Provider;
            if (provider != null) Watch(provider);
        }

        async void Watch(IRewardedVictoryAd provider)
        {
            activeAd = provider;
            var progress = ledger;
            int completedLevel = level;
            string token = receipt;
            var result = RewardedVictoryAdResult.Unavailable;
            try
            {
                var task = provider.ShowAsync(token);
                SetTransition(false);
                result = await task;
            }
            catch (Exception) { /* Playback or verification failed. */ }

            if (this == null || !showing || receipt != token)
            {
                if (result == RewardedVictoryAdResult.Rewarded)
                    progress.TryBoostVictory(completedLevel, token, out _);
                return;
            }

            activeAd = null;
            if (result == RewardedVictoryAdResult.Rewarded)
            {
                verifiedAd = true;
                ApplyAdReward();
                return;
            }
            SetTransition(false, L.V(result switch
            {
                RewardedVictoryAdResult.Closed => "victory.ad_closed",
                RewardedVictoryAdResult.Offline => "victory.ad_offline",
                RewardedVictoryAdResult.Unverified => "victory.ad_unverified",
                _ => "victory.ad_unavailable"
            }));
        }

        void ApplyAdReward()
        {
            if (!ledger.TryBoostVictory(level, receipt, out _))
            {
                SetTransition(false, L.V("error.save"));
                return;
            }
            boosted = true;
            clock = Mathf.Max(clock, RevealEnd);
            sounded = awarded;
            coins = ProgressLedger.RewardedVictoryTotal;
            SetTransition(false);
            GameFeel.Play(GameCue.Coin);
            Animate();
        }

        public void Hide()
        {
            showing = false;
            if (overlay != null) overlay.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!showing) return;
            Layout();
            RefreshAdPanel();
            UpdateSkip();
            if (paused() || GameFeel.Reduced) return;
            clock += Time.unscaledDeltaTime;
            Animate();
        }

        void Layout()
        {
            Vector2 screenSize = overlay.rect.size;
            var safe = new Rect(safeArea.anchorMin, safeArea.anchorMax - safeArea.anchorMin);
            if (screenSize.x <= 0 || screenSize.y <= 0 || (lastSize == screenSize && lastSafe == safe)) return;
            lastSize = screenSize;
            lastSafe = safe;
            Vector2 available = Vector2.Scale(screenSize, safe.size);
            float width = Mathf.Min(available.x, available.y * ArtWidth / ArtHeight);
            float height = Mathf.Min(available.y, width * (18.6f / 9));
            Vector2 half = new(width / screenSize.x * .5f, height / screenSize.y * .5f);
            root.anchorMin = safe.center - half;
            root.anchorMax = safe.center + half;
            edgesX[1] = root.anchorMin.x;
            edgesX[2] = root.anchorMax.x;
            edgesY[1] = root.anchorMin.y;
            edgesY[2] = root.anchorMax.y;
            edgesX[3] = edgesY[3] = 1;

            int index = 0;
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                {
                    if (x == 1 && y == 1) continue;
                    var image = bleed[index++];
                    var min = new Vector2(edgesX[x], edgesY[y]);
                    var max = new Vector2(edgesX[x+1], edgesY[y+1]);
                    image.rectTransform.anchorMin = min;
                    image.rectTransform.anchorMax = max;
                    image.uvRect = new Rect(x == 0 ? .025f : x == 1 ? 0 : 1,
                        y == 0 ? .07f : y == 1 ? 0 : 1,
                        x == 1 ? 1 : -.025f, y == 0 ? -.07f : y == 1 ? 1 : -.006f);
                    image.enabled = max.x > min.x && max.y > min.y;
                }
            skyEdge.Place(root.anchorMax.y);
            gardenEdge.Place(root.anchorMin.y);
        }

        void UpdateSkip()
        {
            bool visible = !saveOk || clock < RevealEnd && message == null;
            skip.interactable = !preparing && !IsWatchingAd;
            skip.gameObject.SetActive(visible);
            skipLabel.gameObject.SetActive(visible);
        }

        void Animate()
        {
            bool reduced = GameFeel.Reduced;
            float reveal = reduced ? RevealEnd : clock;
            int visibleCoins = Mathf.RoundToInt(coins * Mathf.SmoothStep(0, 1, Mathf.Clamp01((reveal - .36f) / .84f)));
            if (displayedCoins != visibleCoins)
            {
                displayedCoins = visibleCoins;
                reward.text = L.V("victory.coins", visibleCoins);
            }
            UpdateSkip();

            if (!starsSettled)
            {
                AnimateStars(reveal, reduced);
                starsSettled = reveal >= RevealEnd;
            }

            float phase = Mathf.Repeat(reveal - RevealEnd, 3.9f);
            float scan = Mathf.Clamp01(phase / 1.2f);
            bool active = !reduced && CanWatch && reveal > RevealEnd;
            Position(sweep.rectTransform, new Vector2(Mathf.Lerp(171, 772, scan), 939));
            sweep.CanvasAlpha(active ? Mathf.Sin(scan * Mathf.PI) * .7f : 0);
            coinGlint.CanvasAlpha(active ? Mathf.Sin(Mathf.PI * Mathf.Clamp01((phase - .78f) / .66f)) * .65f : 0);
        }

        void AnimateStars(float reveal, bool reduced)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                float age = reveal - RevealTimes[i];
                bool earned = i < awarded;
                float pop = Mathf.Clamp01(age / .32f);
                float overshoot = Mathf.Sin(Mathf.PI * pop) * (i == 1 ? .32f : .23f);
                float scale = earned ? Mathf.Lerp(.30f, 1, Mathf.SmoothStep(0, 1, pop)) + overshoot : 1;
                stars[i].rectTransform.localScale = Vector3.one * scale;
                stars[i].color = earned ? new Color(1, 1, 1, Mathf.Clamp01(age / .11f)) :
                    new Color(.37f, .19f, .44f, .43f);
                float flash = earned && age >= 0 && !reduced ? Mathf.Clamp01(1 - age / .48f) : 0;
                halos[i].CanvasAlpha(earned ? .49f + flash * (i == 1 ? .48f : .34f) : 0);
                if (earned && age >= 0 && !reduced && sounded <= i)
                {
                    sounded = i + 1;
                    if (saveOk) GameFeel.Play(i == 1 ? GameCue.Chest : GameCue.Coin);
                }
                for (int j = 0; j < 4; j++)
                {
                    var spark = sparks[i * 4 + j];
                    float travel = Mathf.Clamp01(age / .64f);
                    float angle = .4f + j * Mathf.PI / 2 + i * .28f;
                    Vector2 point = StarCenters[i] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                        Mathf.Lerp(38, i == 1 ? 114 : 87, travel);
                    Position(spark.rectTransform, point);
                    float glint = age >= .64f && j == 1 ? .34f : Mathf.Sin(travel * Mathf.PI) * (1 - travel * .4f);
                    spark.CanvasAlpha(earned && !reduced ? glint : 0);
                }
            }
        }

        static void Position(RectTransform rect, Vector2 design)
        {
            Vector2 center = Point(design.x, design.y);
            Vector2 half = (rect.anchorMax - rect.anchorMin) * .5f;
            rect.anchorMin = center - half;
            rect.anchorMax = center + half;
        }
    }

    sealed class VictoryEdgeArt : RawImage
    {
        public bool Top { get; set; }
        float feather;

        public void Place(float boundary)
        {
            float outside = Top ? 1 - boundary : boundary;
            enabled = outside > .002f;
            if (!enabled) return;
            float overlap = Mathf.Min(outside * .4f, Top ? .006f : .023f);
            feather = overlap / (outside + overlap);
            rectTransform.anchorMin = new Vector2(0, Top ? boundary - overlap : 0);
            rectTransform.anchorMax = new Vector2(1, Top ? 1 : boundary + overlap);
            float sample = Mathf.Clamp01((outside + overlap) / (Top ? .073f : .09f));
            uvRect = new Rect(0, Top ? 0 : 1 - sample, 1, sample);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            AddRow(0, Top ? 0 : 1);
            AddRow(Top ? feather : 1 - feather, 1);
            AddRow(1, Top ? 1 : 0);
            for (int i = 0; i < 4; i += 2)
            {
                mesh.AddTriangle(i, i + 2, i + 3);
                mesh.AddTriangle(i, i + 3, i + 1);
            }

            void AddRow(float t, float alpha)
            {
                var tint = new Color(1, 1, 1, alpha);
                float y = Mathf.Lerp(bounds.yMin, bounds.yMax, t);
                float v = Mathf.Lerp(uvRect.yMin, uvRect.yMax, t);
                mesh.AddVert(new Vector2(bounds.xMin, y), tint, new Vector2(uvRect.xMin, v));
                mesh.AddVert(new Vector2(bounds.xMax, y), tint, new Vector2(uvRect.xMax, v));
            }
        }
    }

    sealed class VictoryLight : MaskableGraphic
    {
        public bool Rays { get; set; }
        public void CanvasAlpha(float value) => canvasRenderer.SetAlpha(value);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 half = rectTransform.rect.size * .5f;
            var core = new Color(1, .91f, .60f, Rays ? .9f : .55f);
            var soft = new Color(1, .55f, .11f, Rays ? .22f : .32f);
            var clear = new Color(1, .39f, .05f, 0);
            mesh.AddVert(Vector2.zero, core, Vector2.zero);
            int segments = Rays ? 8 : 24;
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                float radius = Rays && i % 2 != 0 ? .13f : 1;
                Vector2 edge = Vector2.Scale(direction, half) * radius;
                mesh.AddVert(edge * (Rays ? .25f : .52f), soft, Vector2.zero);
                mesh.AddVert(edge, clear, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
            {
                int a = 1 + i * 2, b = 1 + (i + 1) % segments * 2;
                mesh.AddTriangle(0, a, b);
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
            }
        }
    }
}
