using System.Security.Cryptography;
using System.Text.Json;

sealed partial class ClanStore
{
    readonly EventConfig eventConfig;

    public void RollEvents() => Write(db => AdvanceEvents(db));

    long AdvanceEvents(ClanDb db)
    {
        long now = leagueClock.GetUtcNow().ToUnixTimeSeconds();
        db.Run("""
            INSERT INTO event_clock VALUES(1,$n)
            ON CONFLICT(id) DO UPDATE SET utc=CASE WHEN event_clock.utc>excluded.utc THEN event_clock.utc ELSE excluded.utc END
            """, ("$n",now));
        now = db.Value<long>("SELECT utc FROM event_clock WHERE id=1");
        var active = ReadWindows(db,"WHERE start<=$n AND \"end\">$n",("$n",now));
        var lanes = active.Select(w => w.Snapshot.Definition.Lane).ToHashSet();
        foreach (var candidate in Planned(now,now+1).Where(w => w.Start <= now))
        {
            if (lanes.Count >= eventConfig.MaxActive) break;
            if (!lanes.Add(candidate.Snapshot.Definition.Lane)) continue;
            db.Run("INSERT INTO event_windows VALUES($e,$k,$l,$s,$n,$c) ON CONFLICT DO NOTHING",
                ("$e",candidate.Id),("$k",candidate.Snapshot.Definition.Id),("$l",candidate.Snapshot.Definition.Lane),
                ("$s",candidate.Start),("$n",candidate.End),("$c",JsonSerializer.Serialize(candidate.Snapshot,EventConfig.Json)));
        }
        return now;
    }

    IEnumerable<EventWindow> Planned(long from,long until)
    {
        long epoch = league.EpochUtc.ToUnixTimeSeconds();
        long firstMinute = (long)Math.Floor((from-epoch)/60.0);
        long lastMinute = (long)Math.Ceiling((until-epoch)/60.0);
        foreach (var item in eventConfig.Events.Where(e => e.Lane < eventConfig.MaxActive).OrderBy(e => e.Lane))
            foreach (var window in item.Windows(firstMinute,lastMinute))
            {
                long start = epoch+window.Start*60;
                long end = epoch+window.End*60;
                if (end > from && start < until)
                    yield return new EventWindow($"{item.Id}-{start}",start,end,eventConfig.Snapshot(item));
            }
    }

    static List<EventWindow> ReadWindows(ClanDb db,string where,params (string,object?)[] args) =>
        db.Read("SELECT id,start,\"end\",config FROM event_windows "+where,
            r => new EventWindow(r.GetString(0),r.GetInt64(1),r.GetInt64(2),
                JsonSerializer.Deserialize<EventSnapshot>(r.GetString(3),EventConfig.Json)!),args).ToList();

    void AddEventWin(ClanDb db,string player,string? clan,int level,VerifiedWin proof)
    {
        long now = AdvanceEvents(db);
        foreach (var window in ReadWindows(db,"WHERE start<=$n AND \"end\">$n",("$n",now)))
        {
            var def = window.Snapshot.Definition;
            if (def.Team && clan == null) continue;
            var rules = def.Rules.Where(rule => rule.Matches(level,proof)).ToArray();
            if (rules.Length == 0) continue;
            var prior = db.Read("SELECT clan,points,last_level,streak FROM event_players WHERE event=$e AND player=$p",
                r => (Clan:r.GetString(0), Points:r.GetInt64(1), Level:r.GetInt32(2), Streak:r.GetInt32(3)),
                ("$e",window.Id),("$p",player)).SingleOrDefault();
            string team = def.Team ? clan! : "";
            if (prior.Clan != null && prior.Clan != team) continue;
            int streak = prior.Level == level-1 ? prior.Streak+1 : 1;
            long score = rules[0].Metric == "chain" ? Math.Max(prior.Points,streak*(long)rules[0].Weight) :
                prior.Points+rules.Sum(rule => rule.Score(proof));
            db.Run("""
                INSERT INTO event_players VALUES($e,$p,$c,$s,$l,$t)
                ON CONFLICT(event,player) DO UPDATE SET points=excluded.points,last_level=excluded.last_level,streak=excluded.streak
                """, ("$e",window.Id),("$p",player),("$c",team),("$s",score),("$l",level),("$t",streak));
            string entity = def.Team ? team : player;
            string name = def.Team ? db.Value<string>("SELECT name FROM clans WHERE id=$i",("$i",entity))! :
                db.Value<string>("SELECT handle FROM players WHERE id=$i",("$i",entity))!;
            db.Run("""
                INSERT INTO event_scores VALUES($e,$i,$n,$s)
                ON CONFLICT(event,entity) DO UPDATE SET points=event_scores.points+excluded.points,name=excluded.name
                """, ("$e",window.Id),("$i",entity),("$n",name),("$s",score-prior.Points));
        }
    }

    public EventBoard EventsView(string? player,int page)
    {
        if (page is < 0 or > 100000) throw ClanError.Invalid("events_page");
        const int pageSize = 24;
        long now = Write(db => AdvanceEvents(db));
        return Snapshot(db =>
        {
            var active = ReadWindows(db,"WHERE start<=$n AND \"end\">$n ORDER BY lane",("$n",now));
            var upcoming = Planned(now,now+57*86400L).Where(w => w.Start > now &&
                !active.Any(a => a.Snapshot.Definition.Lane == w.Snapshot.Definition.Lane && a.End > w.Start))
                .OrderBy(w => w.Start).GroupBy(w => w.Snapshot.Definition.Id).Select(g => g.First());
            var recent = ReadWindows(db,"""
                WHERE "end"<=$n AND (id IN(SELECT id FROM event_windows WHERE "end"<=$n ORDER BY "end" DESC,id LIMIT 12)
                  OR EXISTS(SELECT 1 FROM event_players p WHERE p.event=event_windows.id AND p.player=$p))
                ORDER BY "end" DESC,id LIMIT $count OFFSET $offset
                """,("$n",now),("$p",player),("$count",pageSize+1),("$offset",page*pageSize));
            return new EventBoard(now,page,recent.Count>pageSize,active.Concat(upcoming).Concat(recent.Take(pageSize))
                .Select(w => DescribeEvent(db,w,player,now)).ToArray());
        });
    }

    static EventCard DescribeEvent(ClanDb db,EventWindow window,string? player,long now)
    {
        var def = window.Snapshot.Definition;
        int status = now < window.Start ? 0 : now < window.End ? 1 : 2;
        var self = db.Read("SELECT clan,points FROM event_players WHERE event=$e AND player=$p",
            r => (Clan:r.GetString(0), Points:r.GetInt64(1)),("$e",window.Id),("$p",player)).SingleOrDefault();
        string? entity = def.Team ? self.Clan ?? (player == null ? null : Membership(db,player)) : player;
        long score = def.Team ? db.Value<long>("SELECT points FROM event_scores WHERE event=$e AND entity=$i",
            ("$e",window.Id),("$i",entity)) : self.Points;
        int rank = !def.Ranked || score == 0 ? 0 : 1+(int)db.Value<long>(
            "SELECT COUNT(*) FROM event_scores WHERE event=$e AND points>$s",("$e",window.Id),("$s",score));
        var receipts = db.Read("SELECT tier,receipt FROM event_claims WHERE event=$e AND player=$p",
            r => (Tier:r.GetInt32(0),Receipt:r.GetString(1)),("$e",window.Id),("$p",player))
            .ToDictionary(x => x.Tier,x => x.Receipt);
        var tiers = window.Snapshot.Tiers.Select((tier,index) =>
        {
            string receipt = receipts.GetValueOrDefault(index,"");
            bool eligible = status != 0 && self.Points > 0 && score >= tier.Points &&
                (tier.MaxRank == 0 || status == 2 && rank > 0 && rank <= tier.MaxRank);
            return new EventMilestone(index,tier.Points,tier.MaxRank,eligible,receipt.Length > 0,receipt,
                tier.Prize,tier.Chest,tier.Chest.Length == 0 ? [] : window.Snapshot.Chests[tier.Chest]);
        }).ToArray();
        var leaders = !def.Ranked || player == null ? [] : db.Read("""
            SELECT name,points,RANK() OVER(ORDER BY points DESC) FROM event_scores WHERE event=$e
            ORDER BY points DESC,entity LIMIT 10
            """,r => new EventRank(r.GetString(0),r.GetInt64(1),r.GetInt32(2)),("$e",window.Id)).ToArray();
        return new EventCard(window.Id,def.Id,def.TitleTr,def.TitleEn,def.InfoTr,def.InfoEn,def.Icon,def.Mode,
            window.Start,window.End,status,score,self.Points,rank,tiers,leaders);
    }

    public EventGrant ClaimEvent(string player,string id,EventClaimInput input)
    {
        string? wallet = input.WalletId;
        if (wallet == null || !Guid.TryParseExact(wallet,"N",out var parsed) || parsed.ToString("N") != wallet)
            throw ClanError.Invalid("events_wallet");
        return Write(db =>
        {
            long now = AdvanceEvents(db);
            var window = ReadWindows(db,"WHERE id=$e",("$e",id)).SingleOrDefault() ?? throw ClanError.Missing();
            var card = DescribeEvent(db,window,player,now);
            if (input.Milestone < 0 || input.Milestone >= card.Milestones.Length) throw ClanError.Missing();
            var tier = card.Milestones[input.Milestone];
            var prior = db.Read("SELECT wallet,\"grant\" FROM event_claims WHERE event=$e AND player=$p AND tier=$t",
                r => (Wallet:r.GetString(0),Grant:r.GetString(1)),("$e",id),("$p",player),("$t",input.Milestone)).SingleOrDefault();
            if (prior.Wallet != null)
            {
                if (prior.Wallet != wallet) throw ClanError.Conflict("events_other_wallet");
                return JsonSerializer.Deserialize<EventGrant>(prior.Grant,EventConfig.Json)!;
            }
            if (!tier.Eligible) throw ClanError.Conflict("events_locked");
            db.Run("INSERT INTO league_wallet_owners VALUES($w,$p) ON CONFLICT DO NOTHING",("$w",wallet),("$p",player));
            if (db.Value<string>("SELECT player FROM league_wallet_owners WHERE wallet=$w",("$w",wallet)) != player)
                throw ClanError.Conflict("events_wallet_account");
            var prize = tier.Prize;
            if (prize == null)
            {
                int roll = RandomNumberGenerator.GetInt32(tier.Pool.Sum(d => d.Weight));
                foreach (var drop in tier.Pool)
                {
                    roll -= drop.Weight;
                    if (roll < 0) { prize = drop.Prize; break; }
                }
            }
            var grant = new EventGrant("event-"+Guid.NewGuid().ToString("N"),id,input.Milestone,tier.Chest,prize!);
            db.Run("INSERT INTO event_claims VALUES($e,$p,$t,$w,$r,$g)",
                ("$e",id),("$p",player),("$t",input.Milestone),("$w",wallet),("$r",grant.Id),
                ("$g",JsonSerializer.Serialize(grant,EventConfig.Json)));
            return grant;
        });
    }
}
