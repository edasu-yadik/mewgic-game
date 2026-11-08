using System;
using System.Threading.Tasks;

namespace Mewgic.Clans
{
    [Serializable] internal sealed class EventPrize { public int gold, count; public string booster, cosmetic; }
    [Serializable] internal sealed class EventDrop { public int weight; public EventPrize prize; }
    [Serializable] internal sealed class EventRank { public string name; public long points; public int rank; }
    [Serializable] internal sealed class EventTier
    {
        public int index, points, maxRank;
        public bool eligible, claimed;
        public string chest, receipt;
        public EventPrize prize;
        public EventDrop[] pool;
    }
    [Serializable] internal sealed class EventCard
    {
        public string id, kind, titleTr, titleEn, infoTr, infoEn, mode;
        public long startsAt, endsAt, points, personal;
        public int status, icon, rank;
        public EventTier[] milestones;
        public EventRank[] leaders;
    }
    [Serializable] internal sealed class EventBoard { public long serverNow; public int historyPage; public bool olderHistory; public EventCard[] cards; }
    [Serializable] internal sealed class EventClaim { public int milestone; public string walletId; }
    [Serializable] internal sealed class EventGrant
    {
        public string id, eventId, chest;
        public int milestone;
        public EventPrize prize;
    }

    internal static partial class ClanClient
    {
        public static async Task<EventBoard> Events(int page = 0)
        {
            string owner = token;
            await Flush();
            var board = await Send<EventBoard>("GET", (owner == null ? "/event-calendar" : "/events")+"?page="+page,
                authenticate:owner != null);
            if (owner != token) throw new ClanClientException("unauthorized");
            return board;
        }

        public static async Task<EventGrant> ClaimEvent(string id,int tier,string wallet)
        {
            string owner = token;
            var grant = await Send<EventGrant>("POST","/events/"+Uri.EscapeDataString(id)+"/claim",
                new EventClaim { milestone=tier,walletId=wallet });
            if (owner != token) throw new ClanClientException("unauthorized");
            return grant;
        }
    }
}
