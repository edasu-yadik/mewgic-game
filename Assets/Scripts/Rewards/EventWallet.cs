using System;

namespace Mewgic.Core
{
    public sealed partial class ProgressLedger
    {
        public bool ApplyEventReward(string wallet,string receipt,int gold,string booster,int count,string cosmetic)
        {
            if (receipt == null || !receipt.StartsWith("event-",StringComparison.Ordinal) || !LeagueWalletData.IsReceipt(receipt) ||
                string.IsNullOrEmpty(wallet) || wallet != state.leagueWallet.walletId || gold < 0 || gold > 100000 ||
                count < 0 || count > 100) return false;
            if (HasLeagueReward(receipt)) return true;
            ShopItem item = RewardCosmetic(cosmetic);
            BoosterKind? kind = booster switch
            {
                "hint" => BoosterKind.Hint, "undo" => BoosterKind.Undo, "extra" => BoosterKind.ExtraBottle,
                "shuffle" => BoosterKind.Shuffle, _ => null
            };
            if ((int)item < 0 || (kind.HasValue ? count == 0 : count != 0 || booster != "") ||
                gold == 0 && count == 0 && item == 0 || state.presentation.coins > long.MaxValue-gold ||
                kind.HasValue && Get(state,kind.Value) > int.MaxValue-count) return false;
            var copy = state.Copy();
            copy.presentation.coins += gold;
            if (item != 0) copy.shop.Grant(item);
            if (kind.HasValue) Set(copy,kind.Value,Get(copy,kind.Value)+count);
            copy.leagueWallet.receipts.Add(receipt);
            return Commit(copy);
        }
    }
}
