using System;
using System.Linq;

namespace Mewgic.Core
{
    [Serializable]
    public sealed class StorePackage
    {
        public string id, name, description, art, appleId, googleId;
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
    public enum StorePurchaseStatus { Pending, Verified, Rejected, Deferred, Cancelled, Failed }
    public enum StoreRewardStatus { NotDelivered, Retry, Delivered }
    public enum StoreConfirmationStatus { NotRequested, Waiting, Retry, Confirmed }

    [Serializable]
    public sealed class StorePurchaseEntry
    {
        public string platform, transactionId, storeOrderId, productId, updatedUtc;
        public int quantity;
        public StorePurchaseStatus purchase;
        public StoreRewardStatus reward;
        public StoreConfirmationStatus confirmation;

        internal bool Valid => platform is "apple" or "google" &&
            !string.IsNullOrWhiteSpace(transactionId) && transactionId.Length <= 512 && !transactionId.Any(char.IsControl) &&
            (string.IsNullOrEmpty(storeOrderId) || storeOrderId.Length <= 150 && !storeOrderId.Any(char.IsControl)) &&
            !string.IsNullOrWhiteSpace(productId) && productId.Length <= 150 && quantity is >= 1 and <= 100 &&
            DateTimeOffset.TryParseExact(updatedUtc, "O", null, System.Globalization.DateTimeStyles.None, out _) &&
            Enum.IsDefined(typeof(StorePurchaseStatus), purchase) &&
            Enum.IsDefined(typeof(StoreRewardStatus), reward) &&
            Enum.IsDefined(typeof(StoreConfirmationStatus), confirmation) &&
            (reward == StoreRewardStatus.Delivered
                ? purchase == StorePurchaseStatus.Verified
                : confirmation == StoreConfirmationStatus.NotRequested);

        internal StorePurchaseEntry Copy() => (StorePurchaseEntry)MemberwiseClone();
    }

    public sealed partial class ProgressLedger
    {
        internal StorePurchaseEntry StorePurchase(string platform, string transactionId) =>
            state.storePurchases.Find(entry => entry.platform == platform && entry.transactionId == transactionId)?.Copy();

        internal bool SaveStorePurchase(StorePurchaseEntry entry)
        {
            var next = state.Copy();
            return UpdateStorePurchase(next, entry) && Commit(next);
        }

        static bool UpdateStorePurchase(ProgressData next, StorePurchaseEntry entry)
        {
            if (!entry.Valid) return false;
            int index = next.storePurchases.FindIndex(old =>
                old.platform == entry.platform && old.transactionId == entry.transactionId);
            if (index < 0) next.storePurchases.Add(entry.Copy());
            else
            {
                var old = next.storePurchases[index];
                if (old.productId != entry.productId || old.quantity != entry.quantity ||
                    old.reward == StoreRewardStatus.Delivered && entry.reward != StoreRewardStatus.Delivered ||
                    old.confirmation == StoreConfirmationStatus.Confirmed && entry.confirmation != StoreConfirmationStatus.Confirmed)
                    return false;
                next.storePurchases[index] = entry.Copy();
            }
            return true;
        }

        internal StoreDeliveryResult DeliverStorePackage(StorePackage package, string key, int quantity,
            StorePurchaseEntry entry = null)
        {
            if (package == null || !package.Valid || !ValidStoreKey(key) || quantity < 1 || quantity > 100 ||
                (package.Permanent && quantity != 1)) return StoreDeliveryResult.Rejected;
            var existing = state.storeDeliveries.Find(delivery => delivery.key == key);
            if (existing != null)
            {
                if (existing.product != package.id || existing.quantity != quantity) return StoreDeliveryResult.Rejected;
                return entry == null || SaveStorePurchase(entry)
                    ? StoreDeliveryResult.AlreadyApplied : StoreDeliveryResult.Retry;
            }

            var next = state.Copy();
            if (entry != null && !UpdateStorePurchase(next, entry)) return StoreDeliveryResult.Rejected;
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
