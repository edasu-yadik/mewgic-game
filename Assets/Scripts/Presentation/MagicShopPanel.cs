using System;
using System.Collections.Generic;
using System.Globalization;
using Mewgic.Core;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic
{
    public sealed class MagicShopPanel : MonoBehaviour
    {
        enum Page { Offers, Gold, Powers, Costumes }

        [Serializable] sealed class ArtRegion { public string name; public int x, y, width, height, border; }
        [Serializable] sealed class ArtRegions { public ArtRegion[] regions; }
        [Serializable] sealed class Package
        {
            public string id, name, art, price, previous;
            public int amount, bonus, discount;
            public string[] items, icons;
        }
        [Serializable] sealed class PreviewPower { public string id, name; public int price; }
        [Serializable] sealed class Catalogue { public Package[] offers, offerGold, gold; public PreviewPower[] previewPowers; }

        sealed class Row
        {
            public ShopItem item;
            public Button button;
            public TMP_Text stock, action;
        }

        static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
        static readonly Color Ink = new(.033f, .035f, .16f);
        static readonly Color Ivory = new(1f, .96f, .83f);
        static readonly string[] Tabs = { "Özel Teklifler", "Altın Paketleri", "Oyun Güçleri", "Kostümler" };
        static readonly string[] TabIcons = { "crown", "stack", "potion", "shirt" };
        static readonly string[] PageIds = { "Offers", "Gold", "Powers", "Costumes" };
        static readonly int[] PageHeights = { 1097, 1145, 1111, 1320 };
        static readonly (ShopItem item, string name, string art)[] Costumes =
        {
            (ShopItem.PrincessCat, "Prenses Kedi", "princess"), (ShopItem.ExplorerCat, "Kaşif Kedi", "explorer"),
            (ShopItem.CrystalCat, "Kristal Kedi", "crystal"), (ShopItem.NightCat, "Gece Yıldızı", "night"),
            (ShopItem.FairyCat, "Orman Perisi", "fairy"), (ShopItem.RoyalCat, "Kraliyet Kedisi", "royal")
        };

        readonly Dictionary<string, Sprite> art = new();
        readonly List<Row> rows = new();
        readonly RectTransform[] pages = new RectTransform[4];
        readonly Image[] tabFaces = new Image[4];
        readonly Image[] tabPoints = new Image[4];
        readonly TMP_Text[] tabLabels = new TMP_Text[4];
        RectTransform safeArea, root, stage, viewport, scrollContent, garden, gardenMist, modal, collection;
        TMP_Text balance, selectedCostume, modalTitle, modalMessage, modalConfirmLabel, costumeDescription;
        Image selectedArt, modalArt;
        Button modalConfirm, classicEquip;
        ScrollRect scroll;
        ProgressLedger ledger;
        Page current;
        ShopItem pendingItem;
        long pendingRequest;
        int pendingPrice, observedRevision;
        Vector2 lastSize;

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        public void Mount(RectTransform safe)
        {
            safeArea = safe;
            ledger = GameplayProgress.Current;
        }

        public void Open()
        {
            if (root == null) Build();
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            modal.gameObject.SetActive(false);
            Select(Page.Offers);
            Resize();
        }

        public void Close()
        {
            if (root != null) root.gameObject.SetActive(false);
            pendingRequest = 0;
        }

        void LateUpdate()
        {
            if (!IsOpen) return;
            if (root.rect.size != lastSize) Resize();
            if (observedRevision != ledger.Revision) Refresh();
        }

        void OnDestroy()
        {
            foreach (var sprite in art.Values) if (sprite != null) Destroy(sprite);
        }

        void LoadArt()
        {
            var texture = Resources.Load<Texture2D>("ShopReference/Art");
            var definitions = JsonUtility.FromJson<ArtRegions>(Resources.Load<TextAsset>("ShopReference/ArtRegions").text);
            foreach (var region in definitions.regions)
            {
                var border = region.name switch
                {
                    "green" => new Vector4(44, 31, 44, 31),
                    _ => Vector4.one * region.border
                };
                var bounds = new Rect(region.x, texture.height - region.y - region.height, region.width, region.height);
                art.Add(region.name, Sprite.Create(texture, bounds, new Vector2(.5f, .5f), 100, 0,
                    SpriteMeshType.FullRect, border));
            }
        }

        void Build()
        {
            LoadArt();
            var catalog = JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("ShopReference/Offers").text);
            root = GameplayTools.Rect(safeArea, "MagicShop", Vector2.zero, Vector2.one);
            var background = root.gameObject.AddComponent<Image>();
            background.sprite = art["back"];
            stage = Box(root, "ReferenceLayout", 0, 0, 941, 1672);
            gardenMist = Picture(stage, "GardenMist", "gardenMist", 0, 1548, 941, 0).rectTransform;
            garden = Picture(stage, "Garden", "garden", 0, 1548, 941, 124).rectTransform;
            BuildHeader();
            viewport = Box(stage, "ShopViewport", 0, 458, 941, 1214);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = viewport;
            scrollContent = Box(viewport, "ShopProducts", 0, 0, 941, 1145);
            scroll.content = scrollContent;
            for (int i = 0; i < pages.Length; i++) pages[i] = Box(scrollContent, "ShopPage_" + PageIds[i], 0, 0, 941, PageHeights[i]);
            BuildOffers(pages[0], catalog);
            BuildGold(pages[1], catalog.gold);
            BuildPowers(pages[2], catalog.previewPowers);
            BuildCostumes(pages[3]);
            BuildModal();
            Close();
        }

        void BuildHeader()
        {
            Picture(stage, "MascotAndScenery", "scenery", 0, 0, 941, 343);
            Picture(stage, "MeWGicLogo", "logo", 327, 2, 272, 139);
            Picture(stage, "ShopTitleFrame", "heading", 257, 143, 500, 105);
            Text(stage, "ShopTitle", "Büyülü Mağaza", 48, 384, 152, 355, 79, serif: true);
            Picture(stage, "TaglineFrame", "tagline", 250, 248, 505, 85);
            Text(stage, "ShopTagline", "Oyununu daha eğlenceli hale getirecek\nözel paketleri keşfet!", 24, 322, 258, 358, 68, Ink);
            Surface(stage, "ShopWallet", "active", 669, 25, 244, 75);
            Picture(stage, "WalletCoin", "coin", 649, 24, 76, 74);
            balance = Text(stage, "ShopBalance", "", 37, 729, 33, 124, 54);
            Control(stage, "ShopGoldPlus", "+", "active", 848, 23, 78, 80, () => Select(Page.Gold), 45);
            Control(stage, "ShopClose", "‹", "active", 12, 13, 54, 55, Close, 39);
            for (int i = 0; i < tabsCount; i++)
            {
                var page = (Page)i;
                float x = 24 + i * 225;
                var button = Control(stage, "ShopTab_" + PageIds[i], "", "tab", x, 339, 224, 111, () => Select(page));
                tabFaces[i] = button.image;
                tabPoints[i] = Picture(button.transform, "SelectedPoint", "tabPoint", 89, 95, 46, 26);
                Picture(button.transform, "Icon", TabIcons[i], 74, 10, 76, 51, true);
                tabLabels[i] = Text(button.transform, "TabTitle", Tabs[i], 26, 7, 55, 210, 46, Ink);
            }
        }

        const int tabsCount = 4;

        void BuildOffers(RectTransform parent, Catalogue catalog)
        {
            Offer(parent, catalog.offers[0], 9, 10, 923, 378, true);
            Offer(parent, catalog.offers[1], 9, 395, 923, 322, false);
            for (int i = 0; i < catalog.offerGold.Length; i++)
                GoldTile(parent, catalog.offerGold[i], 12 + i * 306, 730, 305, 365);
        }

        void Offer(Transform parent, Package package, float x, float y, float width, float height, bool featured)
        {
            var card = Surface(parent, "Offer_" + package.id, "panel", x, y, width, height).rectTransform;
            Picture(card, "PackageArt", package.art, 18, featured ? 106 : 22, featured ? 357 : 333, featured ? 249 : 275);
            Text(card, "OfferTitle", package.name, featured ? 48 : 44, 351, 13, 391, 66, serif: true);
            var details = Surface(card, "OfferContents", "tab", featured ? 376 : 359, 94 - (featured ? 0 : 18),
                featured ? 271 : 265, featured ? 173 : 199);
            details.color = new Color(.31f, .10f, .45f, .86f);
            for (int i = 0; i < package.items.Length; i++)
            {
                float row = 10 + i * 46;
                Picture(details.transform, "RewardIcon", package.icons[i], 13, row, 47, 43, true);
                Text(details.transform, "Reward", package.items[i], 25, 70, row, featured ? 190 : 169, 43, left: true);
            }
            if (featured) Ribbon(card, "FeaturedRibbon", "EN AVANTAJLI\nBAŞLANGIÇ PAKETİ!", 23, 13, 319, 102, -9, 29);
            Ribbon(card, "Discount", $"%{package.discount}\nİNDİRİM", 732, featured ? 38 : 32, 159, 108, 5, 37);
            var price = Surface(card, "OfferPrice", "cream", featured ? 657 : 652, height - 211 + (featured ? 0 : 31), 235, 100);
            Text(price.transform, "PreviousPrice", "₺" + package.previous, 27, 8, 10, 218, 32, Ink);
            var strike = Box(price.transform, "PreviousPriceStrike", 64, 27, 106, 3).gameObject.AddComponent<Image>();
            strike.color = new Color(.78f, .05f, .23f);
            strike.raycastTarget = false;
            strike.rectTransform.localRotation = Quaternion.Euler(0, 0, 4);
            Text(price.transform, "Price", "₺" + package.price, 43, 6, 39, 222, 52, Ink);
            Control(card, "ShopBuy_" + package.id, "Paketi Önizle", "green",
                featured ? 542 : 608, height - (featured ? 110 : 97), featured ? 359 : 303, featured ? 88 : 79,
                () => ShowPackage(package), featured ? 35 : 31);
        }

        void BuildGold(RectTransform parent, Package[] gold)
        {
            var featured = gold[^1];
            var hero = Surface(parent, "Featured10000Gold", "panel", 9, 12, 923, 379).rectTransform;
            Picture(hero, "GoldChest", featured.art, 29, 99, 419, 262);
            Ribbon(hero, "FeaturedRibbon", "EN AVANTAJLI\nALTIN PAKETİ", 33, 16, 360, 105, -9, 32);
            Text(hero, "GoldAmount", GoldAmount(featured), 69, 444, 18, 439, 92, serif: true);
            Ribbon(hero, "Bonus", Bonus(featured), 484, 111, 337, 68, 0, 40);
            Price(hero, "₺" + featured.price, null, 509, 180, 310, 83, 46);
            Control(hero, "ShopBuy_gold_featured", "Paketi Önizle", "green", 475, 260, 410, 94,
                () => ShowPackage(featured), 40);
            for (int i = 0; i < 4; i++)
                GoldTile(parent, gold[i], 8 + i % 2 * 465, 394 + i / 2 * 269, 457, 266);
            var last = Surface(parent, "Repeat10000Gold", "panel", 9, 935, 923, 206).rectTransform;
            Picture(last, "GoldChest", featured.art, 30, 22, 275, 175);
            Text(last, "GoldAmount", GoldAmount(featured), 42, 288, 22, 281, 62, serif: true);
            Ribbon(last, "Bonus", Bonus(featured), 297, 81, 266, 56, 0, 30);
            Price(last, "₺" + featured.price, null, 595, 43, 272, 62, 37);
            Control(last, "ShopBuy_" + featured.id, "Önizle", "green", 577, 104, 302, 74,
                () => ShowPackage(featured), 35);
        }

        void GoldTile(Transform parent, Package package, float x, float y, float width, float height)
        {
            var card = Surface(parent, "GoldPackage_" + package.id, "panel", x, y, width, height).rectTransform;
            Text(card, "GoldAmount", GoldAmount(package), 34, 12, 10, width - 24, 46, serif: true);
            float artTop = height * (package.bonus > 0 ? .32f : .23f);
            Picture(card, "GoldArt", package.art, width * .09f, artTop, width * .82f, height * .625f - artTop, true);
            if (package.bonus > 0)
                Ribbon(card, "Bonus", Bonus(package), width * .24f, 55, width * .52f, 44, 0, 25);
            Surface(card, "PriceArea", "cream", width * .052f, height * .62f, width * .896f, height * .356f);
            Text(card, "Price", "₺" + package.price, 32, 10, height * .618f, width - 20, height * .135f, Ink);
            Control(card, "ShopBuy_" + package.id, "Önizle", "green", width * .108f, height * .75f,
                width * .784f, height * .205f, () => ShowPackage(package), 33);
        }

        void BuildPowers(RectTransform parent, PreviewPower[] previews)
        {
            var intro = Surface(parent, "PowersHeader", "active", 15, 3, 910, 173).rectTransform;
            Picture(intro, "PowerArt", "powers", 17, 0, 261, 166);
            Text(intro, "PowersTitle", "Oyun Güçleri Mağazası", 52, 279, 10, 599, 76, serif: true);
            Strap(intro, "PowersTagline", "Zorlu seviyeleri daha kolay geçmeni sağlayan\ngüçlü oyun desteklerini keşfet!", 278, 84, 580, 75);
            Power(parent, 0, previews[0].name, "Hamle sınırına 1 ek hamle.", "move", default, previews[0].price);
            Power(parent, 1, "Ek Şişe", "Seviyeye fazladan 1 boş şişe ekle.", "extra", ShopItem.ExtraBottle);
            Power(parent, 2, "İpucu", "Çözüme götüren geçerli bir döküşü gösterir.", "hint", ShopItem.Hint);
            Power(parent, 3, "Karıştır", "Katmanları koruyarak şişelerin yerini değiştirir.", "shuffle", ShopItem.Shuffle);
            Power(parent, 4, "Renk Değiştir", "İki şişenin üst rengini takas eder.", "swap", ShopItem.ColorSwap);
            Power(parent, 5, previews[1].name, "Şişe kaldırma desteği için önizleme.", "hammer", default, previews[1].price);
        }

        void Power(RectTransform parent, int index, string title, string description, string icon, ShopItem item, int previewPrice = 0)
        {
            var row = Surface(parent, "Power_" + icon, "panel", 6, 175 + index * 155, 929, 157).rectTransform;
            Picture(row, "ProductArt", icon, 19, 8, 213, 134);
            Text(row, "ProductName", title, 39, 246, 11, 364, 45, serif: true, left: true);
            Text(row, "ProductPurpose", description, 24, 248, 53, 360, 42, left: true);
            Surface(row, "StockPlate", "badge", 245, 96, 244, 44);
            var stock = Text(row, "ProductInfo", "Sahip Olduğun: 0", 23, 260, 102, 214, 32, Ink);
            var separator = Box(row, "ColumnDivider", 615, 30, 1.5f, 91).gameObject.AddComponent<Image>();
            separator.color = new Color(1, .79f, .55f, .7f);
            separator.raycastTarget = false;
            Price(row, Number(item == default ? previewPrice : EconomyConfig.Current.Price(item)), "coin", 656, 10, 215, 66, 35);
            bool implemented = item != default;
            string action = implemented ? "Buy" + ShopCatalog.Booster(item) : "ShopPreview_" + icon;
            var button = Control(row, action, implemented ? "Satın Al" : "Önizle", "green", 635, 72, 259, 72,
                () => { if (implemented) Choose(item); else Inform(title, "Bu güç şu anda önizlemede. Satın alma ve stok ekleme kapalı.", icon); }, 34);
            if (implemented) rows.Add(new Row { item = item, button = button, stock = stock });
        }

        void BuildCostumes(RectTransform parent)
        {
            var hero = Surface(parent, "CurrentCostume", "panel", 7, 7, 927, 309).rectTransform;
            selectedArt = Picture(hero, "EquippedCostumeArt", "wizard", 22, 12, 364, 274, true);
            selectedCostume = Text(hero, "EquippedCostumeTitle", "", 49, 390, 18, 414, 62, serif: true, left: true);
            Surface(hero, "CostumeStatusRibbon", "active", 403, 82, 255, 64);
            Text(hero, "CostumeStatus", "Mevcut Kostüm", 27, 422, 89, 217, 49);
            Picture(hero, "CostumeLaurelLeft", "laurelLeft", 401, 85, 35, 55);
            Picture(hero, "CostumeLaurelRight", "laurelRight", 626, 85, 35, 55);
            costumeDescription = Text(hero, "CostumeDescription", "", 24, 410, 151, 268, 104, left: true);
            classicEquip = Control(hero, "ShopEquipClassic", "Kuşan", "green", 673, 128, 235, 90,
                () => Equip(ShopItem.ClassicCat), 41);
            for (int i = 0; i < Costumes.Length; i++)
            {
                var (item, name, portrait) = Costumes[i];
                float x = 9 + i % 3 * 306, y = 317 + i / 3 * 384;
                var card = Surface(parent, "ShopProduct_" + item, "panel", x, y, 307, 377).rectTransform;
                Text(card, "ProductName", name, 33, 11, 13, 285, 47, serif: true);
                Picture(card, "CostumeArt", portrait, 10, 54, 287, 215, true);
                Price(card, Number(EconomyConfig.Current.Price(item)), "diamond", 14, 251, 279, 112, 35, compact: true);
                var button = Control(card, "ShopAction_" + item, "Satın Al", "green", 32, 300, 243, 66, () => Choose(item), 32);
                rows.Add(new Row { item = item, button = button, action = button.transform.Find("Label").GetComponent<TMP_Text>() });
            }
            Control(parent, "ShopOwned", "Sahip Olduklarım", "active", 288, 1250, 365, 69, ShowCollection, 27);
        }

        void ShowCollection()
        {
            if (collection != null) Destroy(collection.gameObject);
            collection = Box(stage, "ShopCollection", 0, 0, 941, stage.rect.height);
            collection.gameObject.AddComponent<Image>().color = new Color(.062f, .016f, .125f, .98f);
            Text(collection, "Title", "Sahip Olduklarım", 52, 80, 20, 780, 75, serif: true);
            Control(collection, "ShopCollectionClose", "Geri", "active", 24, 101, 150, 65, () => Destroy(collection.gameObject));
            float y = 174;
            foreach (var item in ShopCatalog.Products)
            {
                if (!ledger.Owns(item) || ShopCatalog.Category(item) == ShopCategory.Boosters) continue;
                float row = y;
                Text(collection, "Owned_" + item, ItemTitle(item), 28, 34, row, 570, 54, left: true);
                Control(collection, "Equip_" + item, "Kuşan", "green", 648, row, 241, 59,
                    () => { if (ledger.Equip(item)) ShowCollection(); else Inform("Kuşan", "Kayıt güncellenemedi.", "wizard"); });
                y += 62;
            }
            modal.SetAsLastSibling();
        }

        void BuildModal()
        {
            modal = Box(stage, "ShopConfirmation", 0, 0, 941, 1672);
            modal.gameObject.AddComponent<Image>().color = new Color(.025f, .009f, .08f, .89f);
            var card = Surface(modal, "ConfirmationCard", "panel", 100, 423, 741, 650).rectTransform;
            modalTitle = Text(card, "ConfirmTitle", "", 47, 23, 21, 695, 94, serif: true);
            modalArt = Picture(card, "ConfirmArt", "stack", 209, 128, 323, 212, true);
            modalMessage = Text(card, "ConfirmMessage", "", 33, 51, 347, 639, 146);
            Control(card, "ShopCancel", "Geri", "active", 45, 529, 299, 84, Cancel, 35);
            modalConfirm = Control(card, "ShopConfirm", "Satın Al", "green", 396, 529, 299, 84, Confirm, 35);
            modalConfirmLabel = modalConfirm.transform.Find("Label").GetComponent<TMP_Text>();
            modal.gameObject.SetActive(false);
        }

        void ShowPackage(Package package)
        {
            var title = string.IsNullOrEmpty(package.name) ? GoldAmount(package) : package.name;
            Inform(title, $"{package.price} TL · Önizleme fiyatı\nGerçek para ile satın alma şu anda kapalı.", package.art);
        }

        void Inform(string title, string message, string picture)
        {
            pendingRequest = 0;
            ShowModal(title, message, picture, "Tamam");
        }

        void ShowModal(string title, string message, string picture, string buttonText)
        {
            modalTitle.text = title;
            modalMessage.text = message;
            modalArt.sprite = art[picture];
            modalConfirmLabel.text = buttonText;
            modal.SetAsLastSibling();
            modal.gameObject.SetActive(true);
        }

        void Choose(ShopItem item)
        {
            if (modal.gameObject.activeSelf) return;
            bool costume = ShopCatalog.UsesGems(item);
            if (costume && ledger.Owns(item)) { Equip(item); return; }
            if (item == ShopItem.ColorSwap && ledger.CurrentUnlockedLevel < EconomyConfig.Current.colorSwapUnlock)
            {
                Inform(ItemTitle(item), $"{EconomyConfig.Current.colorSwapUnlock}. bölümde açılır.", "swap");
                return;
            }
            pendingItem = item;
            pendingPrice = EconomyConfig.Current.Price(item);
            pendingRequest = ledger.NextShopPurchase;
            ShowModal(ItemTitle(item), $"{Number(pendingPrice)} {(costume ? "Elmas" : "Altın")} karşılığında satın al?",
                ProductArt(item), "Satın Al");
        }

        void Cancel()
        {
            pendingRequest = 0;
            modal.gameObject.SetActive(false);
        }

        void Confirm()
        {
            if (pendingRequest == 0) { modal.gameObject.SetActive(false); return; }
            var result = ledger.Purchase(pendingItem, pendingPrice, pendingRequest);
            if (result == ShopPurchaseResult.Purchased)
            {
                pendingRequest = 0;
                modal.gameObject.SetActive(false);
                Refresh();
                GameFeel.Play(GameCue.Coin);
                return;
            }
            modalMessage.text = result switch
            {
                ShopPurchaseResult.InsufficientGems => "Yeterli elmasın yok.",
                ShopPurchaseResult.InsufficientGold => "Yeterli altının yok.",
                ShopPurchaseResult.SaveFailed => "Kayıt güncellenemedi. Tekrar dene.",
                ShopPurchaseResult.StockFull => "Güç stoğu dolu.",
                _ => "Teklif değişti. Geri dönüp tekrar dene."
            };
        }

        void Equip(ShopItem item)
        {
            if (ledger.Equip(item)) Refresh();
            else Inform("Kuşan", "Kayıt güncellenemedi.", "wizard");
        }

        void Select(Page page)
        {
            scroll.StopMovement();
            // Reset the shared viewport before changing content height or activating the next page.
            scrollContent.anchoredPosition = Vector2.zero;
            current = page;
            if (collection != null) { collection.gameObject.SetActive(false); Destroy(collection.gameObject); }
            for (int i = 0; i < pages.Length; i++)
            {
                bool selected = i == (int)page;
                pages[i].gameObject.SetActive(selected);
                tabFaces[i].sprite = art[selected ? "active" : "tab"];
                tabPoints[i].enabled = selected;
                tabLabels[i].color = selected ? Ivory : Ink;
            }
            Resize();
            Refresh();
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 1;
        }

        void Refresh()
        {
            observedRevision = ledger.Revision;
            var player = ledger.Presentation;
            balance.text = Number(current == Page.Costumes ? player.gems : player.coins);
            var wallet = stage.Find("WalletCoin").GetComponent<Image>();
            wallet.sprite = art[current == Page.Costumes ? "diamond" : "coin"];
            foreach (var row in rows)
            {
                if (row.stock != null) row.stock.text = "Sahip Olduğun: " + Number(ledger.Remaining(ShopCatalog.Booster(row.item)));
                else
                {
                    bool equipped = ledger.Equipped(ShopCategory.Cats) == row.item;
                    row.action.text = equipped ? "Kuşanıldı" : ledger.Owns(row.item) ? "Kuşan" : "Satın Al";
                    row.button.interactable = !equipped;
                }
            }
            var worn = ledger.Equipped(ShopCategory.Cats);
            var classic = worn == ShopItem.ClassicCat;
            selectedArt.sprite = art[ProductArt(worn)];
            selectedCostume.text = ItemTitle(worn);
            classicEquip.transform.Find("Label").GetComponent<TMP_Text>().text = classic ? "Kuşan" : "Büyücüye Dön";
            costumeDescription.text = classic ? "MeWGic’in ikonik kostümü!\nGerçek bir büyücünün\nışıltısını taşır." : "Şu anda bu kostümü kullanıyorsun.";
        }

        void Resize()
        {
            lastSize = root.rect.size;
            float scale = lastSize.x / 941;
            if (scale <= 0) return;
            float height = lastSize.y / scale;
            stage.anchorMin = stage.anchorMax = new Vector2(.5f, 1);
            stage.pivot = new Vector2(.5f, 1);
            stage.anchoredPosition = Vector2.zero;
            stage.localScale = Vector3.one * scale;
            stage.sizeDelta = new Vector2(941, height);
            viewport.sizeDelta = new Vector2(941, Mathf.Max(90, height - 458));
            float gardenTop = Mathf.Max(458, height - 124);
            garden.anchoredPosition = new Vector2(0, -gardenTop);
            gardenMist.anchoredPosition = new Vector2(0, -1548);
            gardenMist.sizeDelta = new Vector2(941, Mathf.Max(0, gardenTop - 1548 + 22));
            gardenMist.gameObject.SetActive(gardenTop > 1560);
            scrollContent.sizeDelta = new Vector2(941, PageHeights[(int)current]);
            modal.sizeDelta = new Vector2(941, height);
        }

        static string Number(long value) => value.ToString("N0", Turkish);
        static string GoldAmount(Package package) => Number(package.amount) + " Altın";
        static string Bonus(Package package) => $"%{package.bonus} BONUS";
        static string ItemTitle(ShopItem item)
        {
            foreach (var costume in Costumes) if (costume.item == item) return costume.name;
            return item switch
            {
                ShopItem.ClassicCat => "Büyücü Kedi", ShopItem.ExtraBottle => "Ek Şişe", ShopItem.Hint => "İpucu",
                ShopItem.Shuffle => "Karıştır", ShopItem.ColorSwap => "Renk Değiştir", _ => L.Resolve(L.V(Name(item)))
            };
        }

        static string ProductArt(ShopItem item)
        {
            foreach (var costume in Costumes) if (costume.item == item) return costume.art;
            return item switch
            {
                ShopItem.ExtraBottle => "extra", ShopItem.Hint => "hint", ShopItem.Shuffle => "shuffle",
                ShopItem.ColorSwap => "swap", _ => "wizard"
            };
        }

        public static string Name(ShopItem item) => item switch
        {
            ShopItem.Undo => "booster.undo", ShopItem.Hint => "booster.hint",
            ShopItem.ExtraBottle => "booster.extra", ShopItem.Shuffle => "booster.shuffle", ShopItem.ColorSwap => "booster.color_swap",
            _ => "shop.item." + item.ToString().ToLowerInvariant()
        };

        void Price(Transform parent, string value, string icon, float x, float y, float w, float h, int size, bool compact = false)
        {
            var plate = Surface(parent, "PricePlate", "cream", x, y, w, h).rectTransform;
            if (icon != null) Picture(plate, "CurrencyIcon", icon, w * .16f, compact ? 5 : 8, 56, 49, true);
            Text(plate, "Price", value, size, icon == null ? 6 : w * .35f, 4,
                icon == null ? w - 12 : w * .60f, compact ? 52 : h - 8, Ink);
        }

        void Ribbon(Transform parent, string id, string value, float x, float y, float w, float h, float angle, int font)
        {
            var ribbon = Surface(parent, id, id == "Discount" ? "discount" : "ribbon", x, y, w, h).rectTransform;
            ribbon.localRotation = Quaternion.Euler(0, 0, -angle);
            Text(ribbon, "Label", value, font, 12, 7, w - 24, h - 14);
        }

        void Strap(Transform parent, string id, string value, float x, float y, float w, float h)
        {
            var plate = Surface(parent, id, "cream", x, y, w, h).rectTransform;
            Picture(plate, "LeftPaw", "paw", 21, (h - 47) / 2, 47, 47, true);
            Picture(plate, "RightPaw", "paw", w - 68, (h - 47) / 2, 47, 47, true);
            Text(plate, "Text", value, 25, 65, 9, w - 130, h - 18, Ink);
        }

        static RectTransform Box(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        Image Picture(Transform parent, string name, string key, float x, float y, float w, float h, bool aspect = false)
        {
            var image = Box(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.sprite = art[key];
            image.raycastTarget = false;
            image.preserveAspect = aspect;
            if (aspect)
            {
                image.rectTransform.pivot = new Vector2(.5f, .5f);
                image.rectTransform.anchoredPosition = new Vector2(x + w * .5f, -(y + h * .5f));
            }
            return image;
        }

        Image Surface(Transform parent, string name, string key, float x, float y, float w, float h)
        {
            var image = Picture(parent, name, key, x, y, w, h);
            if (image.sprite.border.sqrMagnitude > 0) image.type = Image.Type.Sliced;
            if (key is "panel" or "active")
            {
                float glowHeight = key == "panel" ? 34 : 23;
                Picture(image.transform, "GoldReflection", "edgeglow", w * .12f, -4, w * .76f, glowHeight);
            }
            return image;
        }

        static TMP_Text Text(Transform parent, string name, string value, int size, float x, float y, float w, float h,
            Color? color = null, bool serif = false, bool left = false)
        {
            var label = Box(parent, name, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            var typography = MewgicTypographySettings.Current;
            label.font = serif ? typography.heading : typography.body;
            label.fontStyle = serif || color.HasValue || label.name is "Label" or "TabTitle" ? FontStyles.Bold : FontStyles.Normal;
            if (label.name is "Price" or "PreviousPrice")
                label.font = Resources.Load<TMP_FontAsset>("ShopReference/ShopPrice") ?? label.font;
            label.enableAutoSizing = true;
            label.fontSizeMin = 15;
            label.fontSizeMax = size;
            label.fontSize = size;
            label.alignment = left ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
            label.color = color ?? Ivory;
            label.raycastTarget = false;
            label.text = value;
            label.extraPadding = true;
            if (name is "Price" or "PreviousPrice" or "ShopBalance")
                label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Truncate;
            return label;
        }

        Button Control(Transform parent, string name, string title, string surface, float x, float y, float w, float h,
            Action action, int size = 29)
        {
            var face = Surface(parent, name, surface, x, y, w, h);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            var colors = button.colors;
            colors.pressedColor = new Color(.82f, .86f, .82f);
            colors.disabledColor = new Color(.74f, .73f, .72f, .92f);
            button.colors = colors;
            button.onClick.AddListener(() => action());
            if (title.Length != 0) Text(face.transform, "Label", title, size, 10, 5, w - 20, h - 10);
            return button;
        }
    }
}
