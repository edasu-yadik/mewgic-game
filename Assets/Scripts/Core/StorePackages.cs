using System;
using System.Linq;

namespace Mewgic.Core
{
    [Serializable]
    public sealed class StorePackage
    {
        public string id, name, art, appleId, googleId;
        public int amount, bonus;
        public StorePackagePower[] powers;
        public ShopItem permanentItem;

        public int Gold => checked(amount + (int)((long)amount * bonus / 100));
        public bool Permanent => permanentItem != 0;
        public string PlatformId(string platform) => platform == "apple" ? appleId : platform == "google" ? googleId : null;

        public bool Valid =>
            !string.IsNullOrWhiteSpace(id) && id.Length <= 80 && amount >= 0 && amount <= 1_000_000 &&
            bonus >= 0 && bonus <= 100 && (powers == null || powers.All(power =>
                power != null && power.count > 0 && power.count <= 100 && Enum.IsDefined(typeof(BoosterKind), power.kind))) &&
            (Permanent
                ? amount == 0 && bonus == 0 && (powers == null || powers.Length == 0) &&
                  ShopCatalog.Known(permanentItem) && !ShopCatalog.Included(permanentItem) &&
                  ShopCatalog.Category(permanentItem) != ShopCategory.Boosters
                : amount > 0 || powers?.Length > 0);
    }

    [Serializable]
    public sealed class StorePackagePower
    {
        public BoosterKind kind;
        public int count;
        public string label, icon;
    }

    [Serializable]
    public sealed class StoreDelivery
    {
        public string key, product;
        public int quantity;
    }

    public enum StoreDeliveryResult { Applied, AlreadyApplied, Rejected, Retry }

    public sealed partial class ProgressLedger
    {
        internal StoreDeliveryResult DeliverStorePackage(StorePackage package, string key, int quantity)
        {
            if (package == null || !package.Valid || !ValidStoreKey(key) || quantity < 1 || quantity > 100 ||
                (package.Permanent && quantity != 1)) return StoreDeliveryResult.Rejected;
            var existing = state.storeDeliveries.Find(delivery => delivery.key == key);
            if (existing != null)
                return existing.product == package.id && existing.quantity == quantity
                    ? StoreDeliveryResult.AlreadyApplied : StoreDeliveryResult.Rejected;

            var next = state.Copy();
            try
            {
                next.presentation.coins = checked(next.presentation.coins + (long)package.Gold * quantity);
                if (package.powers != null)
                    foreach (var power in package.powers)
                        Set(next, power.kind, checked(Get(next, power.kind) + power.count * quantity));
                if (package.Permanent) next.shop.Grant(package.permanentItem);
            }
            catch (OverflowException) { return StoreDeliveryResult.Retry; }
            next.storeDeliveries.Add(new StoreDelivery { key = key, product = package.id, quantity = quantity });
            return Commit(next, "iap:" + key) ? StoreDeliveryResult.Applied : StoreDeliveryResult.Retry;
        }

        internal static bool ValidStoreKey(string key) => key?.Length == 64 &&
            key.All(character => character >= '0' && character <= '9' || character >= 'a' && character <= 'f');
    }
}
