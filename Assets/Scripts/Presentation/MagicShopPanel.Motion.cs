using UnityEngine;
using UnityEngine.UI;
using Mewgic.Characters;
using Mewgic.Core;

namespace Mewgic
{
    public sealed partial class MagicShopPanel
    {
        const float TabTime = .24f;
        readonly CanvasGroup[] pageFades = new CanvasGroup[tabsCount];
        readonly float[] pageOffsets = new float[tabsCount];
        readonly Image[] sparks = new Image[8];
        readonly Image[] tabGlows = new Image[tabsCount];
        CanvasGroup outgoing;
        int outgoingPage = -1;
        float tabClock, tabDirection, offerClock, nextOfferRefresh;
        bool pageSelected;
        RectTransform effects;
        Image pulse, flyingIcon, previousPortrait, newPortrait, starterShimmer, goldShimmer;
        Row rewardRow;
        float rewardClock, rewardDuration;
        int fromStock, toStock, paintedStock;
        long fromBalance, toBalance, paintedBalance;
        bool countWallet, equipOnHero;

        void BuildMotion()
        {
            for (int i = 0; i < pages.Length; i++) pageFades[i] = pages[i].gameObject.AddComponent<CanvasGroup>();
            outgoing = Box(viewport, "ShopOutgoingPage", 0, 0, 941, 0).gameObject.AddComponent<CanvasGroup>();
            outgoing.blocksRaycasts = false;
            outgoing.interactable = false;
            outgoing.gameObject.SetActive(false);
            effects = Box(stage, "ShopMotion", 0, 0, 941, 0);
            pulse = Picture(effects, "ShopPurchaseGlow", "edgeglow", 0, 0, 1, 1, true);
            pulse.preserveAspect = false;
            flyingIcon = Picture(effects, "ShopFlyingIcon", "coin", 0, 0, 1, 1, true);
            previousPortrait = Picture(effects, "ShopPreviousCostume", "wizard", 0, 0, 1, 1, true);
            newPortrait = Picture(effects, "ShopRevealedCostume", "wizard", 0, 0, 1, 1, true);
            var star = Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i] = Picture(effects, "ShopSpark_" + i, "coin", 0, 0, 1, 1, true);
                sparks[i].sprite = star;
            }
            Control(effects, "ShopSkipMotion", "Atla", "active", 768, 101, 137, 50, SkipMotion, 23);
            previousPortrait.gameObject.SetActive(false);
            newPortrait.gameObject.SetActive(false);
            flyingIcon.gameObject.SetActive(false);
            effects.gameObject.SetActive(false);
        }

        void RememberOffset()
        {
            if (pageSelected) pageOffsets[(int)current] = scrollContent.anchoredPosition.y;
        }

        void BeginTab(Page page, bool animate)
        {
            int before = (int)current;
            RememberOffset();
            CompleteTab();
            if (animate)
            {
                outgoingPage = before;
                outgoing.transform.SetAsLastSibling();
                outgoing.gameObject.SetActive(true);
                pages[before].SetParent(outgoing.transform, false);
                pages[before].anchoredPosition = Vector2.up * pageOffsets[before];
                pageFades[before].blocksRaycasts = false;
                tabDirection = (int)page > before ? 1 : -1;
                tabClock = 0;
            }
            current = page;
            for (int i = 0; i < pages.Length; i++)
            {
                bool selected = i == (int)page;
                pages[i].gameObject.SetActive(selected || i == outgoingPage);
                tabFaces[i].sprite = art[selected ? "active" : "tab"];
                tabPoints[i].enabled = selected;
                tabLabels[i].color = selected ? Ivory : Ink;
            }
            Resize();
            Canvas.ForceUpdateCanvases();
            scrollContent.anchoredPosition = Vector2.up * Mathf.Clamp(pageOffsets[(int)page], 0,
                Mathf.Max(0, scrollContent.rect.height - viewport.rect.height));
            scroll.StopMovement();
            pageSelected = true;
            if (animate) PaintTab(0);
        }

        void PaintTab(float t)
        {
            float eased = Mathf.SmoothStep(0, 1, t);
            var incoming = pageFades[(int)current];
            incoming.alpha = eased;
            incoming.blocksRaycasts = t >= .65f;
            pages[(int)current].anchoredPosition = Vector2.right * (tabDirection * 40 * (1 - eased));
            outgoing.alpha = 1 - eased;
            ((RectTransform)outgoing.transform).anchoredPosition = Vector2.left * (tabDirection * 30 * eased);
            tabGlows[(int)current].color = new Color(1, .84f, .41f, Mathf.Sin(t * Mathf.PI) * .85f);
        }

        void CompleteTab()
        {
            if (outgoingPage < 0) return;
            pages[outgoingPage].SetParent(scrollContent, false);
            pages[outgoingPage].anchoredPosition = Vector2.zero;
            pages[outgoingPage].gameObject.SetActive(false);
            pageFades[outgoingPage].blocksRaycasts = true;
            outgoing.gameObject.SetActive(false);
            outgoingPage = -1;
            pages[(int)current].anchoredPosition = Vector2.zero;
            pageFades[(int)current].alpha = 1;
            pageFades[(int)current].blocksRaycasts = true;
            tabGlows[(int)current].color = Color.clear;
        }

        void StartPurchaseMotion(Row row, long before, int stock)
        {
            if (GameFeel.Reduced || row == null) return;
            var player = ledger.Presentation;
            rewardRow = row;
            countWallet = true;
            paintedBalance = fromBalance = before;
            toBalance = row.stock == null ? player.gems : player.coins;
            paintedStock = fromStock = stock;
            toStock = row.stock == null ? 0 : ledger.Remaining(ShopCatalog.Booster(row.item));
            BeginReward(row.stock == null ? .94f : .67f);
            flyingIcon.sprite = row.icon.sprite;
            flyingIcon.gameObject.SetActive(row.stock != null);
            if (row.stock == null) newPortrait.sprite = Portrait(row.item, CostumePose.Front);
            newPortrait.gameObject.SetActive(row.stock == null);
            PaintReward(0);
        }

        void StartEquipMotion(ShopItem oldItem, ShopItem item)
        {
            if (GameFeel.Reduced || oldItem == item) return;
            BeginReward(.72f);
            float heroY = Center(selectedArt.rectTransform).y;
            equipOnHero = heroY < -600 && heroY > -stage.rect.height + 165;
            if (equipOnHero) selectedArt.enabled = false;
            previousPortrait.sprite = equipOnHero ? art[ProductArt(oldItem)] : Portrait(oldItem, CostumePose.Sitting);
            newPortrait.sprite = equipOnHero ? art[ProductArt(item)] : Portrait(item, CostumePose.Sitting);
            previousPortrait.gameObject.SetActive(true);
            newPortrait.gameObject.SetActive(true);
            PaintReward(0);
        }

        Sprite Portrait(ShopItem item, CostumePose pose) => CostumeArt.Get(item, pose) ?? art[ProductArt(item)];

        void BeginReward(float duration)
        {
            rewardClock = 0;
            rewardDuration = duration;
            effects.SetAsLastSibling();
            effects.gameObject.SetActive(true);
            pulse.gameObject.SetActive(true);
            foreach (var spark in sparks) spark.gameObject.SetActive(true);
        }

        void SkipMotion()
        {
            CompleteTab();
            if (rewardDuration <= 0) return;
            rewardDuration = 0;
            effects.gameObject.SetActive(false);
            previousPortrait.gameObject.SetActive(false);
            newPortrait.gameObject.SetActive(false);
            flyingIcon.gameObject.SetActive(false);
            selectedArt.enabled = true;
            equipOnHero = false;
            if (rewardRow?.stock != null) rewardRow.stock.rectTransform.localScale = Vector3.one;
            rewardRow = null;
            countWallet = false;
            Refresh();
        }

        void PaintReward(float t)
        {
            bool costume = newPortrait.gameObject.activeSelf;
            bool equip = previousPortrait.gameObject.activeSelf;
            var cardCenter = rewardRow == null ? Center(selectedArt.rectTransform) : Center(rewardRow.card);
            var source = rewardRow == null ? cardCenter : Center(rewardRow.icon.rectTransform);
            // Keep the reveal in the viewport even when equipping from a scrolled card.
            var portraitCenter = equipOnHero ? cardCenter :
                new Vector2(470, Mathf.Clamp(source.y + 20, -stage.rect.height + 228, -670));
            float wave = Mathf.Sin(Mathf.PI * t);
            var glowCenter = costume ? Vector2.Lerp(cardCenter, portraitCenter, Mathf.SmoothStep(0, 1, t * 3)) :
                cardCenter + Vector2.up * (rewardRow.card.rect.height * .36f);
            SetMotionImage(pulse, glowCenter, new Vector2(costume ? 490 : 705, costume ? 132 : 44),
                new Color(1, .78f, .38f, wave * .64f));

            if (costume)
            {
                float reveal = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t * 2.6f));
                float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.71f, 1, t));
                float size = Mathf.Lerp(205, 350, reveal) + wave * 16;
                Vector2 revealSize = equipOnHero ? selectedArt.rectTransform.rect.size * (1 + wave * .06f) : new Vector2(size, size);
                SetMotionImage(newPortrait, portraitCenter, revealSize, new Color(1, 1, 1, reveal * (equipOnHero ? 1 : fade)));
                if (equip)
                    SetMotionImage(previousPortrait, portraitCenter, equipOnHero ? selectedArt.rectTransform.rect.size : new Vector2(300, 300),
                        new Color(1, 1, 1, (1 - reveal) * fade));
            }
            if (rewardRow?.stock != null)
            {
                Vector2 target = Center(rewardRow.stock.rectTransform);
                float travel = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .7f, t));
                Vector2 flight = Vector2.Lerp(source, target, travel) + Vector2.up * (52 * Mathf.Sin(travel * Mathf.PI));
                float size = Mathf.Lerp(84, 36, travel);
                SetMotionImage(flyingIcon, flight, Vector2.one * size, new Color(1, 1, 1, 1 - Mathf.InverseLerp(.62f, .78f, t)));
                float stockTime = Mathf.InverseLerp(.66f, .9f, t);
                int shown = fromStock + (int)System.Math.Round((toStock - (long)fromStock) * (decimal)stockTime);
                if (paintedStock != shown)
                {
                    paintedStock = shown;
                    rewardRow.stock.text = "Sahip Olduğun: " + Number(shown);
                }
                rewardRow.stock.rectTransform.localScale = Vector3.one * (1 + .1f * Mathf.Sin(stockTime * Mathf.PI));
            }
            if (countWallet)
            {
                float count = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .87f, t));
                long shown = fromBalance + (long)((toBalance - fromBalance) * (decimal)count);
                if (paintedBalance != shown)
                {
                    paintedBalance = shown;
                    balance.text = Number(shown);
                }
            }

            for (int i = 0; i < sparks.Length; i++)
            {
                float angle = i * 2.4f;
                Vector2 ray = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float travel = Mathf.SmoothStep(0, 1, t);
                Vector2 position = (costume ? portraitCenter : source) + ray * (24 + travel * (70 + i % 3 * 18))
                    + Vector2.up * (t * 32);
                Color tint = i % 3 == 0 ? new Color(.88f, .6f, 1) : new Color(1, .81f, .43f);
                tint.a = wave * .76f;
                SetMotionImage(sparks[i], position, Vector2.one * ((i % 3 + 1) * 9 * (1 - t * .45f)), tint);
            }
        }

        Vector2 Center(RectTransform rect)
        {
            Vector2 position = stage.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
            return position - new Vector2(stage.rect.xMin, stage.rect.yMax);
        }

        static void SetMotionImage(Image image, Vector2 position, Vector2 size, Color color)
        {
            image.rectTransform.anchoredPosition = position;
            image.rectTransform.sizeDelta = size;
            image.color = color;
        }

        Image OfferShimmer(RectTransform card)
        {
            var shimmer = Picture(card, "ShopOfferShimmer", "edgeglow", 48, -3, 142, 26);
            shimmer.color = Color.clear;
            shimmer.transform.SetAsLastSibling();
            return shimmer;
        }

        void TickMotion(float delta)
        {
            if (GameFeel.Reduced)
            {
                SkipMotion();
                starterShimmer.color = goldShimmer.color = Color.clear;
                return;
            }
            if (outgoingPage >= 0)
            {
                tabClock += delta;
                if (tabClock >= TabTime) CompleteTab();
                else PaintTab(tabClock / TabTime);
            }
            if (rewardDuration > 0)
            {
                rewardClock += delta;
                if (rewardClock >= rewardDuration) SkipMotion();
                else PaintReward(rewardClock / rewardDuration);
            }

            offerClock = Mathf.Repeat(offerClock + delta, 5.4f);
            if (current != Page.Offers) starterShimmer.color = Color.clear;
            if (current != Page.Gold) goldShimmer.color = Color.clear;
            var shimmer = current == Page.Offers ? starterShimmer : current == Page.Gold ? goldShimmer : null;
            if (shimmer != null)
            {
                float t = Mathf.Clamp01(offerClock / 1.05f);
                shimmer.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(50, 730, t), 3);
                shimmer.color = new Color(1, .86f, .62f, t < 1 ? Mathf.Sin(t * Mathf.PI) * .46f : 0);
            }
        }
    }
}
