using System;
using System.Collections.Generic;
using Mewgic.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mewgic.UI
{
    [AddComponentMenu("Mewgic/Premium Button")]
    public class PremiumButton : Button
    {
        static GameObject facePrefab;
        static readonly Color Cream = new(1, .94f, .80f);
        static readonly Color MutedText = new(.85f, .79f, .90f);
        const double RepeatDelay = .28;
        [SerializeField] ButtonRole role;
        PremiumButtonFace face;
        TMP_Text caption;
        RectTransform icon, lockMark;
        Graphic taskArrow, taskChest;
        readonly List<RectTransform> moving = new(3);
        bool pressed, initialized;
        float scale = 1;
        double lastAction = double.NegativeInfinity, pulseAt = double.NegativeInfinity;
        double deniedAt = double.NegativeInfinity;

        protected virtual bool PreserveArtwork => false;
        protected virtual bool ArtworkLocked => false;
        bool Locked => !IsInteractable() || ArtworkLocked;

        public void SetRole(ButtonRole style)
        {
            role = style;
            if (face != null) face.SetStyle(role, Locked);
            SyncTaskIcon();
        }

        protected virtual void Start()
        {
            if (initialized) return;
            initialized = true;
            if (PreserveArtwork || role == ButtonRole.Artwork || name is "Play" or "PawCatTouch") return;

            facePrefab ??= Resources.Load<GameObject>("UI/PremiumButtonFace");
            if (facePrefab == null) return;
            if (role == ButtonRole.Automatic) role = ResolveRole(name);

            var layer = Instantiate(facePrefab, transform, false);
            layer.name = "PremiumFace";
            layer.transform.SetAsFirstSibling();
            face = layer.GetComponent<PremiumButtonFace>();
            moving.Add(face.rectTransform);

            caption = transform.Find("Label")?.GetComponent<TMP_Text>() ??
                transform.Find("Retry Label")?.GetComponent<TMP_Text>();
            icon = transform.Find("Icon") as RectTransform;
            if (name == "MissionsButton")
            {
                icon = transform.Find("QuestIcon") as RectTransform;
                var oldPlate = transform.Find("QuestLabelPlate")?.GetComponent<Image>();
                if (oldPlate != null) oldPlate.enabled = false;
            }

            Sprite artwork = GetIcon(name);
            var glyph = GetGlyph(name);
            if (icon == null && (artwork != null || glyph != ButtonGlyph.None))
            {
                icon = GameplayTools.Rect(transform, "PremiumIcon", Vector2.zero, Vector2.one);
                if (artwork != null)
                {
                    var image = icon.gameObject.AddComponent<Image>();
                    image.sprite = artwork;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                }
                else
                {
                    var vector = icon.gameObject.AddComponent<PremiumButtonGlyph>();
                    vector.Shape = glyph;
                    vector.color = Cream;
                    vector.raycastTarget = false;
                }
                icon.SetAsLastSibling();
            }
            if (icon != null) moving.Add(icon);
            if (name.StartsWith("DailyAction_", StringComparison.Ordinal) && icon != null)
            {
                taskArrow = icon.GetComponent<PremiumButtonGlyph>();
                var chest = GameplayTools.Art(icon, "TaskRewardIcon", RewardSprite(), Vector2.zero, Vector2.one);
                chest.preserveAspect = true;
                taskChest = chest;
                SyncTaskIcon();
            }
            if (caption != null)
            {
                caption.color = Cream;
                caption.raycastTarget = false;
                caption.textWrappingMode = TextWrappingModes.NoWrap;
                caption.enableAutoSizing = true;
                caption.fontSizeMin = Mathf.Min(caption.fontSizeMin, 13);
                if (name != "MissionsButton") caption.alignment = TextAlignmentOptions.Center;
                moving.Add(caption.rectTransform);
            }

            var outline = GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            transition = Transition.None;
            if (targetGraphic != null) targetGraphic.canvasRenderer.SetAlpha(0);

            if (role == ButtonRole.Reward || name.StartsWith("DailyAction_", StringComparison.Ordinal))
            {
                lockMark = GameplayTools.Rect(transform, "PremiumLock", Vector2.zero, Vector2.one);
                var badge = lockMark.gameObject.AddComponent<Map.MapLockGraphic>();
                badge.raycastTarget = false;
            }
            Layout();
            face.SetStyle(role, Locked);
        }

        static ButtonRole ResolveRole(string name)
        {
            if (name.StartsWith("Claim", StringComparison.Ordinal) || name == "ArrivalOpen") return ButtonRole.Reward;
            if (name.StartsWith("Buy", StringComparison.Ordinal) || name is "WeeklyGo" or "Resume" or "Retry Loading")
                return ButtonRole.Primary;
            if (name == "NavHome") return ButtonRole.Selected;
            if (name.StartsWith("Nav", StringComparison.Ordinal) || name.StartsWith("RewardTab_", StringComparison.Ordinal) ||
                name is "DailyTasksTab" or "DailyGiftTab" or "HubOpenDaily" or "MissionsButton") return ButtonRole.Navigation;
            if (name.EndsWith("Plus", StringComparison.Ordinal) ||
                name is "Pause" or "SettingsButton" or "Undo" or "Hint" or "ExtraBottle" or "Shuffle" or "ColorSwap" or "PawThoughtClose")
                return ButtonRole.Tool;
            return ButtonRole.Secondary;
        }

        Sprite GetIcon(string buttonName)
        {
            if (buttonName is "Pause" or "SettingsButton") return (targetGraphic as Image)?.sprite;
            if (buttonName.StartsWith("Buy", StringComparison.Ordinal))
            {
                string key = buttonName.Substring(3);
                if (Enum.TryParse(key, out BoosterKind booster))
                {
                    string asset = booster switch { BoosterKind.ExtraBottle => "Extra", BoosterKind.ColorSwap => "Shuffle", _ => key };
                    return Resources.Load<Sprite>("Gameplay/Tools/Tool_" + asset + "_v001");
                }
            }
            if (buttonName.StartsWith("Claim", StringComparison.Ordinal) || buttonName == "ArrivalOpen")
                return RewardSprite();
            return null;
        }

        static Sprite RewardSprite() => Resources.Load<Rewards.RewardHubArt>("Rewards/Art")?.closedChest;

        void SyncTaskIcon()
        {
            if (taskArrow == null || taskChest == null) return;
            taskArrow.enabled = role != ButtonRole.Reward;
            taskChest.enabled = role == ButtonRole.Reward;
        }

        static ButtonGlyph GetGlyph(string name)
        {
            if (name == "CoinsPlus") return ButtonGlyph.Shop;
            if (name.EndsWith("Plus", StringComparison.Ordinal)) return ButtonGlyph.Plus;
            if (name.StartsWith("DailyAction_", StringComparison.Ordinal) || name is "WeeklyGo" or "Resume")
                return ButtonGlyph.Play;
            return name switch
            {
                "Restart" or "PauseRestart" or "TutorialReplay" => ButtonGlyph.Replay,
                "JourneyFocus" or "FocusCurrent" => ButtonGlyph.Focus,
                "NextIsland" => ButtonGlyph.Forward,
                "MapHome" or "ChapterClose" or "PreviousIsland" or "Home" or "DailyClose" or "RewardHubClose" or "PauseHome" or "ShopClose" => ButtonGlyph.Back,
                _ => ButtonGlyph.None
            };
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (face != null) Layout();
        }

        void Layout()
        {
            if (face == null) return;
            var bounds = ((RectTransform)transform).rect;
            if (bounds.width <= 0 || bounds.height <= 0) return;
            if (name == "MissionsButton")
                SetAnchors(face.rectTransform, new Vector2(0, .15f), new Vector2(1, .36f));
            bool stacked = name.StartsWith("Nav", StringComparison.Ordinal) ||
                name is "Undo" or "Hint" or "ExtraBottle" or "Shuffle" or "ColorSwap" or "MissionsButton";
            if (stacked && name.StartsWith("Nav", StringComparison.Ordinal) && icon != null)
            {
                SetAnchors(icon, new Vector2(.12f, .28f), new Vector2(.88f, .93f));
                if (caption != null) SetAnchors(caption.rectTransform, new Vector2(.06f, .055f), new Vector2(.94f, .28f));
            }
            else if (!stacked)
            {
                float slot = Mathf.Clamp(bounds.height / bounds.width * .68f, .14f, .27f);
                if (icon != null)
                {
                    if (caption != null)
                        SetAnchors(icon, new Vector2(.045f, .20f), new Vector2(.045f + slot, .80f));
                    else SetAnchors(icon, new Vector2(.17f, .17f), new Vector2(.83f, .83f));
                }
                if (caption != null) SetAnchors(caption.rectTransform,
                    new Vector2(icon == null ? .075f : .08f + slot, .10f), new Vector2(.925f, .90f));
            }

            if (lockMark != null)
            {
                float fraction = Mathf.Clamp(bounds.height / bounds.width * .29f, .09f, .26f);
                SetAnchors(lockMark, new Vector2(.94f - fraction, .69f), new Vector2(.94f, .94f));
            }
        }

        static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        void LateUpdate()
        {
            bool locked = Locked;
            double now = Time.unscaledTimeAsDouble;
            float deny = (float)(now - deniedAt);
            bool reduced = GameFeel.Reduced;
            if (face == null)
            {
                if (PreserveArtwork && targetGraphic != null && targetGraphic.transform != transform)
                    targetGraphic.rectTransform.localScale = Vector3.one * (reduced ? 1 : pressed || deny < .18f ? .969f : 1);
                return;
            }

            float requested = reduced ? 1 : pressed ? .964f : deny < .18f ? .974f : 1;
            float nextScale = Mathf.MoveTowards(scale, requested, Time.unscaledDeltaTime * .56f);
            if (nextScale != scale)
            {
                scale = nextScale;
                foreach (var part in moving) part.localScale = Vector3.one * scale;
            }
            face.SetStyle(role, locked);
            if (caption != null) caption.color = locked ? MutedText : Cream;
            if (lockMark != null) lockMark.gameObject.SetActive(locked);
            float pulse = (float)(now - pulseAt);
            bool ambient = role == ButtonRole.Reward && !locked;
            float sweep = pulse < .4f ? pulse / .4f :
                ambient ? Mathf.Clamp01(Mathf.Repeat(Time.unscaledTime + transform.GetSiblingIndex() * .35f, 4.8f) / 1.1f) : 1;
            face.ShowShine(Mathf.Lerp(.14f, .86f, sweep), reduced ? 0 : Mathf.Sin(sweep * Mathf.PI) * (pulse < .4f ? .86f : .45f));
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (eventData.button != PointerEventData.InputButton.Left) return;
            pressed = true;
            if (Locked && Time.unscaledTimeAsDouble - deniedAt > RepeatDelay)
            {
                deniedAt = Time.unscaledTimeAsDouble;
                GameFeel.Play(GameCue.Invalid);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
            base.OnPointerUp(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            pressed = false;
            base.OnPointerExit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && AcceptInput()) base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (AcceptInput()) base.OnSubmit(eventData);
        }

        bool AcceptInput()
        {
            double now = Time.unscaledTimeAsDouble;
            if (!IsActive() || !IsInteractable() || now - lastAction < RepeatDelay) return false;
            lastAction = now;
            pulseAt = now;
            return true;
        }

        protected override void OnDisable()
        {
            pressed = false;
            lastAction = deniedAt = pulseAt = double.NegativeInfinity;
            scale = 1;
            foreach (var part in moving) if (part != null) part.localScale = Vector3.one;
            if (PreserveArtwork && targetGraphic != null && targetGraphic.transform != transform)
                targetGraphic.rectTransform.localScale = Vector3.one;
            base.OnDisable();
        }
    }
}
