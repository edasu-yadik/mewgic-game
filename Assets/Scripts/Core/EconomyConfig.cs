using System;

namespace Mewgic.Core
{
    [Serializable]
    public sealed class EconomyConfig
    {
        public int levelBase = 20, perNewStar = 5, firstAttempt = 10, withoutHint = 5;
        public int firstClearHints = 1;
        public int dailyGiftGold = 100, dailyGiftHints = 1, dailyTaskBooster = 1;
        public int taskBottlesGold = 15, taskWinsGold = 25, taskStarsGold = 30, taskBoosterGold = 10;
        public int weeklyGold = 60, seasonFirstGold = 30, seasonSecondGold = 45, seasonFinalGold = 75, eventBooster = 1;
        public int undoPrice = 35, extraPrice = 800, hintPrice = 1000, shufflePrice = 1200;
        public int crystalCatPrice = 1500, goldCatPrice = 360, nightCatPrice = 1800;
        public int princessCatPrice = 1200, explorerCatPrice = 900, fairyCatPrice = 1200, royalCatPrice = 1800;
        public int crystalBottlePrice = 150, goldBottlePrice = 240, roseEffectPrice = 120, crystalEffectPrice = 180;
        public int boosterQuantity = 1;
        public int colorSwapPrice = 1500, colorSwapUnlock = 101;
        public int bottleHammerPrice = 1800, bottleHammerUnlock = 101;
        public int initialGold;
        public int initialUndo = 3, initialHint = 3, initialExtra = 1, initialShuffle = 1;

        public static EconomyConfig Current { get; private set; } = new();

        internal static void Configure(EconomyConfig config)
        {
            config.Validate();
            Current = config;
        }

        void Validate()
        {
            var grants = new[] { levelBase, perNewStar, firstAttempt, withoutHint, firstClearHints, dailyGiftGold,
                dailyGiftHints, dailyTaskBooster, taskBottlesGold, taskWinsGold, taskStarsGold, taskBoosterGold,
                weeklyGold, seasonFirstGold, seasonSecondGold, seasonFinalGold, eventBooster,
                initialGold, initialUndo, initialHint, initialExtra, initialShuffle };
            foreach (int amount in grants)
                if (amount < 0 || amount > 1_000_000) throw new ArgumentException("Ekonomi ödülü geçersiz.");
            foreach (var product in ShopCatalog.Products)
                if (!ShopCatalog.Included(product) && !ShopCatalog.RewardOnly(product) && (Price(product) <= 0 || Price(product) > 1_000_000))
                    throw new ArgumentException("Mağaza fiyatı geçersiz.");
            if (boosterQuantity < 1 || boosterQuantity > 100 || colorSwapUnlock < 1 || colorSwapUnlock > WorldMapRules.MaxLevel ||
                bottleHammerUnlock < 1 || bottleHammerUnlock > WorldMapRules.MaxLevel)
                throw new ArgumentException("Ekonomi üst sınırı geçersiz.");
        }

        public int Price(ShopItem item) => item switch
        {
            ShopItem.Undo => undoPrice, ShopItem.ExtraBottle => extraPrice, ShopItem.Hint => hintPrice,
            ShopItem.Shuffle => shufflePrice, ShopItem.ColorSwap => colorSwapPrice, ShopItem.BottleHammer => bottleHammerPrice, ShopItem.CrystalCat => crystalCatPrice, ShopItem.GoldCat => goldCatPrice,
            ShopItem.NightCat => nightCatPrice, ShopItem.PrincessCat => princessCatPrice,
            ShopItem.ExplorerCat => explorerCatPrice, ShopItem.FairyCat => fairyCatPrice, ShopItem.RoyalCat => royalCatPrice, ShopItem.CrystalBottle => crystalBottlePrice,
            ShopItem.GoldBottle => goldBottlePrice, ShopItem.RoseEffect => roseEffectPrice,
            ShopItem.CrystalEffect => crystalEffectPrice, _ => 0
        };

        public int UnlockLevel(ShopItem item) => item switch
        {
            ShopItem.ColorSwap => colorSwapUnlock,
            ShopItem.BottleHammer => bottleHammerUnlock,
            _ => 1
        };

        public int DailyGold(DailyTaskKind kind) => kind switch
        {
            DailyTaskKind.Bottles => taskBottlesGold, DailyTaskKind.Wins => taskWinsGold,
            DailyTaskKind.Stars => taskStarsGold, DailyTaskKind.Boosters => taskBoosterGold, _ => 0
        };

        public int SeasonGold(int tier) => tier switch
        {
            0 => seasonFirstGold, 1 => seasonSecondGold, 2 => seasonFinalGold, _ => 0
        };
    }
}
