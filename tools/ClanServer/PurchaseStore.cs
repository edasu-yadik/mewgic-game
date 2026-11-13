using System.Text.RegularExpressions;

// Internal, post-verification contract. Never bind this type to an HTTP request.
// TransactionId is the Apple transaction ID or sha256:<Google purchase token hash>.
sealed record VerifiedStorePurchase(string Platform, string Environment, string TransactionId,
    string ProductId, string Owner, int Quantity);

enum PurchaseGrantResult { Applied, AlreadyApplied, Rejected }

sealed partial class ClanStore
{
    const string StorePrefix = "com.mewgic.game.";
    static readonly Regex AppleTransaction = new(@"\A[0-9]{1,128}\z");
    static readonly Regex GoogleTransaction = new(@"\Asha256:[0-9a-f]{64}\z");

    static (int Gold, int Extra, int Hammer, int Shuffle, int Swap)? StoreReward(string product) => product switch
    {
        StorePrefix + "gold_250" => (250,0,0,0,0),
        StorePrefix + "gold_1000" => (1100,0,0,0,0),
        StorePrefix + "gold_2500" => (2875,0,0,0,0),
        StorePrefix + "gold_5000" => (6000,0,0,0,0),
        StorePrefix + "gold_10000" => (13000,0,0,0,0),
        StorePrefix + "starter" => (1000,3,2,0,0),
        StorePrefix + "powers" => (0,5,3,3,1),
        _ => null
    };

    // No production path or HTTP endpoint until verification and the game wallet are integrated.
    static bool IsSandboxPurchase(VerifiedStorePurchase order) => order.Environment == "sandbox" &&
        order.Quantity is >= 1 and <= 100 &&
        Guid.TryParseExact(order.Owner,"N",out var owner) && owner != Guid.Empty &&
        (order.Platform switch
        {
            "apple" => AppleTransaction.IsMatch(order.TransactionId),
            "google" => GoogleTransaction.IsMatch(order.TransactionId),
            _ => false
        });

    static (string,object?)[] StoreOrderArgs(VerifiedStorePurchase order) =>
        [("$platform",order.Platform),("$env",order.Environment),("$tx",order.TransactionId),
         ("$sku",order.ProductId),("$player",order.Owner),("$qty",order.Quantity)];

    internal PurchaseGrantResult GrantVerifiedSandboxPurchase(string authenticatedPlayer, VerifiedStorePurchase order)
    {
        if (authenticatedPlayer != order.Owner || !IsSandboxPurchase(order) ||
            StoreReward(order.ProductId) is not { } reward) return PurchaseGrantResult.Rejected;
        var args = StoreOrderArgs(order);
        return Write(db =>
        {
            if (db.Value<string>("SELECT id FROM players WHERE id=$player",("$player",authenticatedPlayer)) == null)
                return PurchaseGrantResult.Rejected;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int inserted = db.Run("""
                INSERT INTO store_purchases(platform,environment,transaction_id,product_id,player,quantity,
                    purchase_state,reward_state,confirmation_state,created_utc,updated_utc)
                VALUES($platform,$env,$tx,$sku,$player,$qty,'verified','delivered','waiting',$now,$now)
                ON CONFLICT(platform,environment,transaction_id) DO NOTHING
                """, [..args,("$now",now)]);
            if (inserted == 0)
                return db.Value<long>("""
                    SELECT 1 FROM store_purchases WHERE platform=$platform AND environment=$env AND transaction_id=$tx
                        AND player=$player AND product_id=$sku AND quantity=$qty
                    """,args) == 1 ? PurchaseGrantResult.AlreadyApplied : PurchaseGrantResult.Rejected;
            db.Run("""
                INSERT INTO store_reward_grants(platform,environment,transaction_id,player,gold,extra_bottle,bottle_hammer,shuffle,color_swap)
                VALUES($platform,$env,$tx,$player,$gold,$extra,$hammer,$shuffle,$swap)
                """, [..args,("$gold",(long)reward.Gold*order.Quantity),("$extra",reward.Extra*order.Quantity),
                    ("$hammer",reward.Hammer*order.Quantity),("$shuffle",reward.Shuffle*order.Quantity),("$swap",reward.Swap*order.Quantity)]);
            return PurchaseGrantResult.Applied;
        });
    }

    // Call only from a trusted store reconciliation path, never from a client-supplied success flag.
    internal bool RecordSandboxStoreConfirmation(VerifiedStorePurchase order, bool confirmed)
    {
        if (!IsSandboxPurchase(order)) return false;
        return Write(db => db.Run("""
            UPDATE store_purchases SET confirmation_state=$state, updated_utc=$now
            WHERE platform=$platform AND environment=$env AND transaction_id=$tx AND player=$player
                AND product_id=$sku AND quantity=$qty AND confirmation_state<>'confirmed'
            """,[..StoreOrderArgs(order),("$state",confirmed ? "confirmed" : "retry"),
                ("$now",DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())]) == 1);
    }
}
