using System;
using System.Threading.Tasks;

namespace Mewgic.Clans
{
    [Serializable] internal sealed class LeagueClaim { public string walletId; }
    [Serializable] internal sealed class LeagueSeason
    {
        public int id;
        public long startsAt, endsAt;
        public bool closed;
    }
    [Serializable] internal sealed class LeagueRank
    {
        public string id, name;
        public int tier, wins, rank, nextTier, frame;
        public long points;
    }
    [Serializable] internal sealed class LeagueTier
    {
        public int gold, minimumPromotion;
        public string cosmetic;
    }
    [Serializable] internal sealed class LeagueReward
    {
        public string id, cosmetic;
        public int season, tier, rank, nextTier, gold;
        public bool claimed;
    }
    [Serializable] internal sealed class LeagueBoard
    {
        public long serverNow;
        public LeagueSeason season;
        public LeagueSeason[] previous;
        public LeagueRank[] top, nearby;
        public LeagueRank self;
        public LeagueTier[] tiers;
        public LeagueReward reward;
        public int currentTier, clanEmblem, milestoneWins;
    }

    internal static partial class ClanClient
    {
        public static async Task<LeagueBoard> League(bool clans,int? season)
        {
            var board = await Send<LeagueBoard>("GET","/league?scope=" + (clans ? "clans" : "players") +
                (season.HasValue ? "&season=" + season.Value : ""));
            if (string.IsNullOrEmpty(board.self?.id)) board.self = null;
            if (string.IsNullOrEmpty(board.reward?.id)) board.reward = null;
            return board;
        }
        public static Task<LeagueReward> Claim(int season,string walletId) =>
            Send<LeagueReward>("POST","/league/" + season + "/claim",new LeagueClaim { walletId=walletId });
    }
}
