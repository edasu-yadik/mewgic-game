using System;
using System.Collections.Generic;
using Mewgic.Core;
using Mewgic.Daily;
using Mewgic.Rewards;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic
{
    public sealed class MagicShopPanel : MonoBehaviour
    {
        sealed class ProductRow
        {
            public RectTransform Root;
            public TMP_Text Info, ActionLabel;
            public PremiumButton Action;
        }

        readonly Dictionary<ShopItem, ProductRow> products = new();
        readonly PremiumButton[] tabs = new PremiumButton[5];
        RectTransform safeArea, root, confirmation, packs;
        Sprite walletCoin;
        ScrollRect scroll;
        PremiumButton ownedTab;
        TMP_Text balance, notice, confirmationText, confirmationNotice, ownedLabel;
        Image confirmationArt;
        Sprite frame, cat, bottle, star, chest;
        ProgressLedger ledger;
        ShopCategory category;
        ShopItem pendingItem;
        long pendingRequest;
        int pendingPrice;
        bool ownedOnly;

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        public void Mount(RectTransform safe, Sprite coin)
        {
            safeArea = safe;
            walletCoin = coin;
            ledger = GameplayProgress.Current;
        }

        void Build()
        {
            frame = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            bottle = Resources.Load<Sprite>("Gameplay/Gameplay_GlassShell_v001");
            star = Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            chest = Resources.Load<RewardHubArt>("Rewards/Art").closedChest;
            var catPrefab = Resources.Load<DailyArt>("Daily/Art").catPrefab;
            foreach (var image in catPrefab.GetComponentsInChildren<Image>(true))
                if (image.name == "Body") { cat = image.sprite; break; }

            root = Rect(safeArea, "MagicShop", 0, 0, 1, 1);
            var shade = root.gameObject.AddComponent<Image>();
            shade.color = new Color(.05f, .013f, .115f, .965f);
            var header = Panel(root, "ShopHeader", .025f, .873f, .975f, .986f);
            Art(header, "ShopCrest", chest, .022f, .12f, .18f, .92f);
            Text(header, "ShopTitle", L.V("shop.title"), 49, .19f, .11f, .76f, .90f);
            Button(header, "ShopClose", L.V("common.back"), ButtonRole.Secondary, Close, .775f, .25f, .97f, .78f);

            var wallet = Panel(root, "ShopWallet", .033f, .80f, .495f, .864f);
            Art(wallet, "WalletCoin", walletCoin, .035f, .10f, .23f, .90f);
            balance = Text(wallet, "ShopBalance", "", 34, .24f, .13f, .96f, .87f);
            ownedTab = Button(root, "ShopOwned", "", ButtonRole.Navigation, () => Select(category, true), .51f, .80f, .969f, .864f);
            ownedLabel = ownedTab.transform.Find("Label").GetComponent<TMP_Text>();

            string[] keys = { "shop.boosters", "shop.cats", "shop.bottles", "shop.effects", "shop.packs" };
            for (int i = 0; i < tabs.Length; i++)
            {
                var target = (ShopCategory)i;
                float x = i < 3 ? .034f + i * .314f : .034f + (i - 3) * .472f;
                float y = i < 3 ? .735f : .669f;
                tabs[i] = Button(root, "ShopTab_" + target, L.V(keys[i]), ButtonRole.Navigation,
                    () => Select(target, false), x, y, x + (i < 3 ? .301f : .458f), y + .055f);
            }

            var viewport = Rect(root, "ShopViewport", .032f, .085f, .968f, .652f);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = viewport;
            var content = Rect(viewport, "ShopProducts", 0, 1, 1, 1);
            content.pivot = new Vector2(.5f, 1);
            scroll.content = content;
            var list = content.gameObject.AddComponent<VerticalLayoutGroup>();
            list.padding = new RectOffset(3, 3, 4, 5);
            list.spacing = 12;
            list.childControlWidth = list.childForceExpandWidth = list.childControlHeight = true;
            list.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            foreach (var item in ShopCatalog.Products) AddProduct(content, item);

            packs = Panel(root, "ShopSpecialPacks", .04f, .22f, .96f, .63f);
            Art(packs, "PackChest", chest, .27f, .48f, .73f, .95f);
            Text(packs, "PackTitle", L.V("shop.packs"), 47, .08f, .30f, .92f, .48f);
            Text(packs, "PackStatus", L.V("shop.packs_unavailable"), 30, .09f, .07f, .91f, .29f);
            var footer = Panel(root, "ShopFooter", .032f, .016f, .968f, .075f);
            notice = Text(footer, "ShopNotice", "", 24, .04f, .10f, .96f, .90f);
            BuildConfirmation();
            Close();
        }

        void AddProduct(RectTransform content, ShopItem item)
        {
            var card = Panel(content, "ShopProduct_" + item, 0, 0, 1, 1);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 183;
            var art = Art(card, "ProductArt", null, .027f, .10f, .235f, .91f);
            PresentArt(art, item);
            Text(card, "ProductName", L.V(Name(item)), 37, .26f, .67f, .97f, .95f, TextAlignmentOptions.MidlineLeft);
            Text(card, "ProductPurpose", L.V(Purpose(item)), 25, .26f, .32f, .97f, .68f, TextAlignmentOptions.MidlineLeft);
            var info = Text(card, "ProductInfo", "", 25, .26f, .065f, .635f, .31f, TextAlignmentOptions.MidlineLeft);
            string actionName = ShopCatalog.Category(item) == ShopCategory.Boosters
                ? "Buy" + ShopCatalog.Booster(item) : "ShopAction_" + item;
            var action = Button(card, actionName, "", ButtonRole.Primary, () => Choose(item),
                .66f, .048f, .967f, .33f);
            products.Add(item, new ProductRow { Root = card, Info = info, Action = action,
                ActionLabel = action.transform.Find("Label").GetComponent<TMP_Text>() });
        }

        void BuildConfirmation()
        {
            confirmation = Rect(root, "ShopConfirmation", 0, 0, 1, 1);
            confirmation.gameObject.AddComponent<Image>().color = new Color(.02f, .005f, .06f, .87f);
            var card = Panel(confirmation, "ShopConfirmationCard", .07f, .25f, .93f, .75f);
            Text(card, "ConfirmTitle", L.V("shop.confirm_title"), 43, .07f, .83f, .93f, .97f);
            confirmationArt = Art(card, "ConfirmArt", null, .32f, .45f, .68f, .82f);
            confirmationText = Text(card, "ConfirmSummary", "", 34, .08f, .29f, .92f, .46f);
            confirmationNotice = Text(card, "ConfirmNotice", "", 26, .07f, .17f, .93f, .29f);
            Button(card, "ShopCancel", L.V("shop.cancel"), ButtonRole.Secondary,
                () => confirmation.gameObject.SetActive(false), .06f, .04f, .47f, .16f);
            Button(card, "ShopConfirm", L.V("shop.buy"), ButtonRole.Primary, Confirm,
                .53f, .04f, .94f, .16f);
        }

        public void Open()
        {
            if (root == null) Build();
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            Select(ShopCategory.Boosters, false);
        }

        public void Close()
        {
            if (root != null) root.gameObject.SetActive(false);
            if (confirmation != null) confirmation.gameObject.SetActive(false);
            pendingRequest = 0;
        }

        void Select(ShopCategory target, bool owned)
        {
            category = target;
            ownedOnly = owned;
            notice.text = L.V(owned ? "shop.owned_note" : "shop.note");
            if (owned)
                foreach (var item in ShopCatalog.Products)
                    if (ShopCatalog.Category(item) == ShopCategory.Boosters)
                        products[item].Root.SetAsLastSibling();
            Refresh();
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1;
        }

        void Refresh()
        {
            balance.text = L.V("shop.gold", HomeTopBar.FormatAmount(ledger.Presentation.coins));
            int owned = 0;
            foreach (var pair in products)
            {
                var item = pair.Key;
                var row = pair.Value;
                var group = ShopCatalog.Category(item);
                bool has = ledger.Owns(item);
                if (has) owned++;
                row.Root.gameObject.SetActive(ownedOnly ? has : group == category);
                if (!row.Root.gameObject.activeSelf) continue;
                bool booster = group == ShopCategory.Boosters;
                bool equipped = !booster && ledger.Equipped(group) == item;
                bool buying = !ownedOnly && (booster || !has);
                row.Info.text = booster
                    ? L.V("shop.stock", ledger.Remaining(ShopCatalog.Booster(item)))
                    : L.V(equipped ? "shop.equipped" : has ? "shop.owned" : "shop.gold", EconomyConfig.Current.Price(item));
                row.Action.gameObject.SetActive(buying || !booster);
                row.ActionLabel.text = buying
                    ? (booster ? L.V("shop.buy_for", EconomyConfig.Current.Price(item)) : L.V("shop.buy"))
                    : L.V(equipped ? "shop.equipped" : "shop.equip");
                row.Action.interactable = !equipped;
                row.Action.SetRole(buying ? ButtonRole.Primary : ButtonRole.Secondary);
            }
            ownedLabel.text = L.V("shop.owned_tab", owned);
            ownedTab.SetRole(ownedOnly ? ButtonRole.Selected : ButtonRole.Navigation);
            for (int i = 0; i < tabs.Length; i++)
                tabs[i].SetRole(!ownedOnly && i == (int)category ? ButtonRole.Selected : ButtonRole.Navigation);
            packs.gameObject.SetActive(!ownedOnly && category == ShopCategory.Packs);
            scroll.gameObject.SetActive(!packs.gameObject.activeSelf);
        }

        void Choose(ShopItem item)
        {
            if (ShopCatalog.Category(item) != ShopCategory.Boosters && ledger.Owns(item))
            {
                notice.text = L.V(ledger.Equip(item) ? "shop.equipped_note" : "error.save");
                Refresh();
                return;
            }
            if (ownedOnly || confirmation.gameObject.activeSelf) return;
            pendingItem = item;
            pendingPrice = EconomyConfig.Current.Price(item);
            pendingRequest = ledger.NextShopPurchase;
            if (pendingRequest == 0) { notice.text = L.V("error.save"); return; }
            PresentArt(confirmationArt, item);
            int quantity = ShopCatalog.Category(item) == ShopCategory.Boosters ? EconomyConfig.Current.boosterQuantity : 1;
            confirmationText.text = L.V("shop.confirm_line", L.V(Name(item)), quantity, pendingPrice);
            confirmationNotice.text = "";
            confirmation.gameObject.SetActive(true);
            confirmation.SetAsLastSibling();
        }

        void Confirm()
        {
            if (!confirmation.gameObject.activeSelf || pendingRequest <= 0) return;
            var result = ledger.Purchase(pendingItem, pendingPrice, pendingRequest);
            switch (result)
            {
                case ShopPurchaseResult.Purchased:
                    pendingRequest = 0;
                    confirmation.gameObject.SetActive(false);
                    notice.text = L.V("shop.purchased");
                    Refresh();
                    GameFeel.Play(GameCue.Coin);
                    break;
                case ShopPurchaseResult.SaveFailed:
                    confirmationNotice.text = L.V("error.save");
                    break;
                case ShopPurchaseResult.InsufficientGold:
                    confirmationNotice.text = L.V("shop.insufficient");
                    break;
                case ShopPurchaseResult.StockFull:
                    confirmationNotice.text = L.V("shop.stock_full");
                    break;
                default:
                    pendingRequest = 0;
                    confirmation.gameObject.SetActive(false);
                    notice.text = L.V("shop.offer_changed");
                    Refresh();
                    break;
            }
        }

        void PresentArt(Image image, ShopItem item)
        {
            image.material = null;
            image.color = Color.white;
            switch (ShopCatalog.Category(item))
            {
                case ShopCategory.Boosters:
                    string key = item == ShopItem.ExtraBottle ? "Extra" : item.ToString();
                    image.sprite = Resources.Load<Sprite>("Gameplay/Tools/Tool_" + key + "_v001");
                    break;
                case ShopCategory.Cats:
                    image.sprite = cat;
                    image.material = ShopCosmetics.CostumeMaterial(item);
                    break;
                case ShopCategory.Bottles:
                    image.sprite = bottle;
                    image.color = ShopCosmetics.BottleTint(item);
                    break;
                default:
                    image.sprite = star;
                    image.color = ShopCosmetics.EffectTint(item);
                    break;
            }
        }

        public static string Name(ShopItem item) => item switch
        {
            ShopItem.Undo => "booster.undo", ShopItem.Hint => "booster.hint",
            ShopItem.ExtraBottle => "booster.extra", ShopItem.Shuffle => "booster.shuffle",
            _ => "shop.item." + item.ToString().ToLowerInvariant()
        };

        static string Purpose(ShopItem item) => item switch
        {
            ShopItem.Undo => "shop.use_undo", ShopItem.Hint => "shop.use_hint",
            ShopItem.ExtraBottle => "shop.use_extra", ShopItem.Shuffle => "shop.use_shuffle",
            _ => ShopCatalog.Category(item) switch
            {
                ShopCategory.Cats => "shop.use_cat", ShopCategory.Bottles => "shop.use_bottle",
                _ => "shop.use_effect"
            }
        };

        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h) =>
            GameplayTools.Rect(parent, name, new Vector2(x, y), new Vector2(w, h));

        RectTransform Panel(Transform parent, string name, float x, float y, float w, float h) =>
            GameplayTools.Art(parent, name, frame, new Vector2(x, y), new Vector2(w, h), true).rectTransform;

        static Image Art(Transform parent, string name, Sprite sprite, float x, float y, float w, float h)
        {
            var image = GameplayTools.Art(parent, name, sprite, new Vector2(x, y), new Vector2(w, h));
            image.preserveAspect = true;
            return image;
        }

        static TMP_Text Text(Transform parent, string name, string value, int size, float x, float y, float w, float h,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var text = GameplayTools.Label(parent, name, value, size, new Vector2(x, y), new Vector2(w, h));
            text.color = new Color(1, .935f, .81f);
            text.alignment = alignment;
            return text;
        }

        static PremiumButton Button(Transform parent, string name, string label, ButtonRole role, Action action,
            float x, float y, float w, float h)
        {
            var graphic = GameplayTools.Art(parent, name, null, new Vector2(x, y), new Vector2(w, h));
            graphic.color = new Color(.34f, .16f, .49f);
            graphic.raycastTarget = true;
            var button = graphic.gameObject.AddComponent<PremiumButton>();
            button.targetGraphic = graphic;
            button.onClick.AddListener(() => action());
            button.SetRole(role);
            Text(graphic.transform, "Label", label, 27, .04f, .12f, .96f, .88f);
            return button;
        }
    }
}
