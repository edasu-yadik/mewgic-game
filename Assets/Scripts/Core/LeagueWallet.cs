using System;
using System.Collections.Generic;
using System.Linq;

namespace Mewgic.Core
{
    [Serializable]
    public sealed class LeagueWalletData
    {
        public int version = 1;
        public string walletId;
        public List<string> receipts = new();

        public LeagueWalletData Copy() => new() { version=version, walletId=walletId, receipts=new List<string>(receipts) };

        public void Validate()
        {
            if (version != 1 || receipts == null || (!string.IsNullOrEmpty(walletId) && !IsId(walletId)) ||
                (receipts.Count > 0 && string.IsNullOrEmpty(walletId)) || receipts.Any(id => !IsReceipt(id)) ||
                receipts.Count != receipts.Distinct().Count())
                throw new ArgumentException("Geçersiz lig ödülü kaydı");
        }

        static bool IsId(string id) => Guid.TryParseExact(id,"N",out var value) && value.ToString("N") == id;

        public static bool IsReceipt(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 55) return false;
            string[] parts = id.Split('-');
            if (parts.Length == 2 && parts[0] == "event") return IsId(parts[1]);
            return parts.Length == 3 && parts[0] == "league" && int.TryParse(parts[1],out int season) && season > 0 && IsId(parts[2]);
        }
    }

    public sealed partial class ProgressLedger
    {
        public string GetOrCreateLeagueWallet()
        {
            if (!string.IsNullOrEmpty(state.leagueWallet.walletId)) return state.leagueWallet.walletId;
            var copy = state.Copy();
            copy.leagueWallet.walletId = Guid.NewGuid().ToString("N");
            return Commit(copy) ? copy.leagueWallet.walletId : null;
        }

        public bool HasLeagueReward(string receipt) => state.leagueWallet.receipts.Contains(receipt);

        public bool ApplyLeagueReward(string wallet,string receipt,int gold,string cosmetic)
        {
            if (string.IsNullOrEmpty(wallet) || state.leagueWallet.walletId != wallet ||
                !LeagueWalletData.IsReceipt(receipt) || gold < 0 || gold > 100000) return false;
            if (HasLeagueReward(receipt)) return true;
            ShopItem item = RewardCosmetic(cosmetic);
            if ((int)item < 0 || state.presentation.coins > long.MaxValue - gold) return false;
            var copy = state.Copy();
            copy.presentation.coins += gold;
            if (item != 0) copy.shop.Grant(item);
            copy.leagueWallet.receipts.Add(receipt);
            return Commit(copy, receipt);
        }

        static ShopItem RewardCosmetic(string cosmetic) => cosmetic switch
        {
            "rose_effect" => ShopItem.RoseEffect,
            "crystal_effect" => ShopItem.CrystalEffect,
            "crystal_cat" => ShopItem.CrystalCat,
            "league_cat" => ShopItem.LeagueCat,
            "" => 0,
            _ => (ShopItem)(-1)
        };
    }
}
