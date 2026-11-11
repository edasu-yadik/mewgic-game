using System;
using System.Collections.Generic;
using System.Linq;

namespace Mewgic.Core
{
    public enum ShopCategory { Boosters, Cats, Bottles, Effects, Packs }
    public enum ShopItem
    {
        Undo = 1, ExtraBottle = 2, Hint = 3, Shuffle = 4, ColorSwap = 5,
        ClassicCat = 10, CrystalCat = 11, GoldCat = 12, NightCat = 13, LeagueCat = 14,
        PrincessCat = 15, ExplorerCat = 16, FairyCat = 17, RoyalCat = 18,
        ClassicBottle = 20, CrystalBottle = 21, GoldBottle = 22,
        ClassicEffect = 30, RoseEffect = 31, CrystalEffect = 32
    }
    public enum ShopPurchaseResult { Purchased, Invalid, AlreadyOwned, Duplicate, PriceChanged, InsufficientGold, StockFull, SaveFailed, InsufficientGems }

    public static class ShopCatalog
    {
        public static IReadOnlyList<ShopItem> Products { get; } = Array.AsReadOnly(new[]
        {
            ShopItem.Undo, ShopItem.ExtraBottle, ShopItem.Hint, ShopItem.Shuffle, ShopItem.ColorSwap,
            ShopItem.ClassicCat, ShopItem.CrystalCat, ShopItem.GoldCat, ShopItem.NightCat, ShopItem.LeagueCat,
            ShopItem.PrincessCat, ShopItem.ExplorerCat, ShopItem.FairyCat, ShopItem.RoyalCat,
            ShopItem.ClassicBottle, ShopItem.CrystalBottle, ShopItem.GoldBottle,
            ShopItem.ClassicEffect, ShopItem.RoseEffect, ShopItem.CrystalEffect
        });

        public static bool Known(ShopItem item) => Products.Contains(item);
        public static bool RewardOnly(ShopItem item) => item == ShopItem.LeagueCat;
        public static bool UsesGems(ShopItem item) => Category(item) == ShopCategory.Cats;
        public static bool Included(ShopItem item) =>
            item is ShopItem.ClassicCat or ShopItem.ClassicBottle or ShopItem.ClassicEffect;

        public static ShopCategory Category(ShopItem item) => item switch
        {
            ShopItem.Undo or ShopItem.ExtraBottle or ShopItem.Hint or ShopItem.Shuffle or ShopItem.ColorSwap => ShopCategory.Boosters,
            ShopItem.ClassicCat or ShopItem.CrystalCat or ShopItem.GoldCat or ShopItem.NightCat or ShopItem.LeagueCat or
            ShopItem.PrincessCat or ShopItem.ExplorerCat or ShopItem.FairyCat or ShopItem.RoyalCat => ShopCategory.Cats,
            ShopItem.ClassicBottle or ShopItem.CrystalBottle or ShopItem.GoldBottle => ShopCategory.Bottles,
            ShopItem.ClassicEffect or ShopItem.RoseEffect or ShopItem.CrystalEffect => ShopCategory.Effects,
            _ => throw new ArgumentOutOfRangeException(nameof(item))
        };

        public static BoosterKind Booster(ShopItem item) => item switch
        {
            ShopItem.Undo => BoosterKind.Undo, ShopItem.ExtraBottle => BoosterKind.ExtraBottle,
            ShopItem.Hint => BoosterKind.Hint, ShopItem.Shuffle => BoosterKind.Shuffle, ShopItem.ColorSwap => BoosterKind.ColorSwap,
            _ => throw new ArgumentOutOfRangeException(nameof(item))
        };

        public static ShopItem Product(BoosterKind kind) => kind switch
        {
            BoosterKind.Undo => ShopItem.Undo, BoosterKind.ExtraBottle => ShopItem.ExtraBottle,
            BoosterKind.Hint => ShopItem.Hint, BoosterKind.Shuffle => ShopItem.Shuffle, BoosterKind.ColorSwap => ShopItem.ColorSwap, _ => 0
        };

        public static ShopItem Default(ShopCategory category) => category switch
        {
            ShopCategory.Cats => ShopItem.ClassicCat, ShopCategory.Bottles => ShopItem.ClassicBottle,
            ShopCategory.Effects => ShopItem.ClassicEffect, _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }

    [Serializable]
    public sealed class ShopData
    {
        public int version = 1;
        public long purchaseRevision;
        public List<ShopItem> owned = new();
        public List<ShopItem> equipped = new();

        public ShopData Copy() => new()
        {
            version = version, purchaseRevision = purchaseRevision,
            owned = new List<ShopItem>(owned), equipped = new List<ShopItem>(equipped)
        };

        public void Validate()
        {
            if (version != 1 || purchaseRevision < 0 || owned == null || equipped == null ||
                owned.Any(item => !ShopCatalog.Known(item) || ShopCatalog.Included(item) ||
                    ShopCatalog.Category(item) == ShopCategory.Boosters) ||
                owned.Distinct().Count() != owned.Count || equipped.Any(item => !owned.Contains(item)) ||
                equipped.Select(ShopCatalog.Category).Distinct().Count() != equipped.Count)
                throw new ArgumentException("Mağaza kaydı geçersiz.");
        }

        internal void Grant(ShopItem item)
        {
            if (!owned.Contains(item)) owned.Add(item);
        }
    }

    public sealed partial class ProgressLedger
    {
        public long NextShopPurchase => state.shop.purchaseRevision == long.MaxValue ? 0 : state.shop.purchaseRevision + 1;

        public bool Owns(ShopItem item)
        {
            if (!ShopCatalog.Known(item)) return false;
            return ShopCatalog.Category(item) == ShopCategory.Boosters
                ? Remaining(ShopCatalog.Booster(item)) > 0
                : ShopCatalog.Included(item) || state.shop.owned.Contains(item);
        }

        public ShopItem Equipped(ShopCategory category)
        {
            foreach (var item in state.shop.equipped)
                if (ShopCatalog.Category(item) == category) return item;
            return ShopCatalog.Default(category);
        }

        public bool Equip(ShopItem item)
        {
            if (!Owns(item) || ShopCatalog.Category(item) == ShopCategory.Boosters) return false;
            ShopCategory category = ShopCatalog.Category(item);
            if (Equipped(category) == item) return true;
            var next = state.Copy();
            next.shop.equipped.RemoveAll(other => ShopCatalog.Category(other) == category);
            if (!ShopCatalog.Included(item)) next.shop.equipped.Add(item);
            return Commit(next);
        }

        public ShopPurchaseResult Purchase(ShopItem item, int quotedPrice, long request)
        {
            if (request <= 0 || !ShopCatalog.Known(item) || ShopCatalog.Included(item) || ShopCatalog.RewardOnly(item)) return ShopPurchaseResult.Invalid;
            if (item == ShopItem.ColorSwap && CurrentUnlockedLevel < EconomyConfig.Current.colorSwapUnlock) return ShopPurchaseResult.Invalid;
            if (request != NextShopPurchase) return ShopPurchaseResult.Duplicate;
            int price = EconomyConfig.Current.Price(item);
            if (price <= 0 || price != quotedPrice) return ShopPurchaseResult.PriceChanged;
            var category = ShopCatalog.Category(item);
            if (category != ShopCategory.Boosters && Owns(item)) return ShopPurchaseResult.AlreadyOwned;
            bool gems = ShopCatalog.UsesGems(item);
            if (gems ? state.presentation.gems < price : state.presentation.coins < price)
                return gems ? ShopPurchaseResult.InsufficientGems : ShopPurchaseResult.InsufficientGold;

            var next = state.Copy();
            if (category == ShopCategory.Boosters)
            {
                var booster = ShopCatalog.Booster(item);
                try { Set(next, booster, checked(Get(next, booster) + EconomyConfig.Current.boosterQuantity)); }
                catch (OverflowException) { return ShopPurchaseResult.StockFull; }
            }
            else next.shop.Grant(item);

            if (gems) next.presentation.gems -= price;
            else next.presentation.coins -= price;
            next.shop.purchaseRevision = request;
            return Commit(next) ? ShopPurchaseResult.Purchased : ShopPurchaseResult.SaveFailed;
        }
    }
}
