sealed partial class ClanStore
{
    const int PersonalLeague = 0, ClanLeague = 1;
    readonly LeagueOptions league;
    readonly TimeProvider leagueClock;

    public void RollLeague() => Write(db => AdvanceLeague(db, leagueClock.GetUtcNow()));

    int AdvanceLeague(ClanDb db, DateTimeOffset now)
    {
        int id = league.Id(now);
        if (id < 1) return id;
        long time = now.ToUnixTimeSeconds();
        var due = db.Read("SELECT id FROM league_seasons WHERE closed=0 AND \"end\"<=$now ORDER BY id",
            r => r.GetInt32(0), ("$now",time));
        foreach (int season in due) CloseLeague(db,season);
        var start = league.Start(id);
        db.Run("INSERT INTO league_seasons(id,start,\"end\") VALUES($s,$a,$b) ON CONFLICT DO NOTHING",
            ("$s",id),("$a",start.ToUnixTimeSeconds()),("$b",start.AddDays(LeagueOptions.Days).ToUnixTimeSeconds()));
        // A backward clock must never reopen an already settled season.
        return db.Value<long>("SELECT closed FROM league_seasons WHERE id=$s",("$s",id)) == 0 ? id : 0;
    }

    void CloseLeague(ClanDb db,int season)
    {
        var entries = db.Read("""
            SELECT scope,entity,tier,points,
                COUNT(*) OVER(PARTITION BY scope,tier), RANK() OVER(PARTITION BY scope,tier ORDER BY points DESC),
                COUNT(*) OVER(PARTITION BY scope,tier,points), RANK() OVER(PARTITION BY scope ORDER BY points DESC)
            FROM league_entries WHERE season=$s
            """, r => new
            {
                Scope=r.GetInt32(0), Entity=r.GetString(1), Tier=r.GetInt32(2), Points=r.GetInt64(3),
                Pool=r.GetInt32(4), TierRank=r.GetInt32(5), Tied=r.GetInt32(6), Rank=r.GetInt32(7)
            }, ("$s",season));
        foreach (var entry in entries)
        {
            int tier = entry.Tier;
            int next = tier;
            int slots = entry.Pool < league.MinimumPool ? 0 : Math.Min(entry.Pool/2,
                (int)Math.Ceiling(entry.Pool * league.MovePercent / 100.0));
            // Never split tied players and never move a member of overlapping top/bottom bands.
            if (slots > 0 && entry.TierRank + entry.Tied - 1 <= slots && tier < 5 &&
                entry.Points >= league.Tiers[tier].MinimumPromotion) next++;
            else if (slots > 0 && entry.TierRank > entry.Pool - slots && tier > 0) next--;
            db.Run("UPDATE league_entries SET rank=$r,next_tier=$n WHERE season=$s AND scope=$o AND entity=$e",
                ("$r",entry.Rank),("$n",next),("$s",season),("$o",entry.Scope),("$e",entry.Entity));
            db.Run("""
                INSERT INTO league_divisions VALUES($o,$e,$n)
                ON CONFLICT(scope,entity) DO UPDATE SET tier=excluded.tier
                """, ("$o",entry.Scope),("$e",entry.Entity),("$n",next));
            if (entry.Scope == PersonalLeague)
            {
                var reward = league.Tiers[tier];
                db.Run("INSERT INTO league_rewards(season,player,tier,rank,next_tier,gold,cosmetic) VALUES($s,$p,$t,$r,$n,$g,$c) ON CONFLICT DO NOTHING",
                    ("$s",season),("$p",entry.Entity),("$t",tier),("$r",entry.Rank),("$n",next),("$g",reward.Gold),("$c",reward.Cosmetic));
            }
            else
            {
                db.Run("""
                    INSERT INTO league_clan_emblems VALUES($e,$t)
                    ON CONFLICT(clan) DO UPDATE SET tier=CASE WHEN league_clan_emblems.tier>excluded.tier THEN league_clan_emblems.tier ELSE excluded.tier END
                    """, ("$e",entry.Entity),("$t",tier));
            }
        }
        db.Run("UPDATE league_seasons SET closed=1 WHERE id=$s",("$s",season));
    }

    void AddLeagueWin(ClanDb db,string player,string? clan,VerifiedWin proof)
    {
        var now = leagueClock.GetUtcNow();
        int season = AdvanceLeague(db,now);
        if (season == 0) return;
        int points = league.BasePoints + league.DifficultyStep*proof.Difficulty + league.StarPoints*proof.Stars;
        int wins = (int)db.Value<long>("SELECT wins FROM league_entries WHERE season=$s AND scope=0 AND entity=$p",
            ("$s",season),("$p",player)) + 1;
        if (wins == league.MilestoneWins) points += league.MilestoneBonus;
        if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            long week = DateOnly.FromDateTime(now.UtcDateTime).DayNumber - (((int)now.DayOfWeek + 6)%7);
            if (db.Run("INSERT INTO league_event_bonuses VALUES($s,$p,$w) ON CONFLICT DO NOTHING",
                ("$s",season),("$p",player),("$w",week.ToString(System.Globalization.CultureInfo.InvariantCulture))) == 1)
                points += league.WeekendBonus;
        }
        string handle = db.Value<string>("SELECT handle FROM players WHERE id=$p",("$p",player))!;
        AddLeagueEntry(db,season,PersonalLeague,player,handle,points);
        if (clan != null)
        {
            string name = db.Value<string>("SELECT name FROM clans WHERE id=$c",("$c",clan))!;
            AddLeagueEntry(db,season,ClanLeague,clan,name,points);
        }
    }

    static void AddLeagueEntry(ClanDb db,int season,int scope,string entity,string name,int points)
    {
        int tier = Division(db,scope,entity);
        db.Run("""
            INSERT INTO league_entries(season,scope,entity,name,tier,points,wins,next_tier) VALUES($s,$o,$e,$n,$t,$p,1,$t)
            ON CONFLICT(season,scope,entity) DO UPDATE SET points=league_entries.points+excluded.points,wins=league_entries.wins+1,name=excluded.name
            """, ("$s",season),("$o",scope),("$e",entity),("$n",name),("$t",tier),("$p",points));
    }

    static int Division(ClanDb db,int scope,string entity) => (int)db.Value<long>(
        "SELECT tier FROM league_divisions WHERE scope=$o AND entity=$e",("$o",scope),("$e",entity));

    public LeagueBoard LeagueView(string player,string? scope,int? requested)
    {
        if (scope is not (null or "players" or "clans")) throw ClanError.Invalid("league_scope");
        var now = leagueClock.GetUtcNow();
        int active = Write(db => AdvanceLeague(db,now));
        if (active < 1) throw new ClanError(503,"league_unavailable");
        return Snapshot(db =>
        {
            int current = scope == "clans" ? ClanLeague : PersonalLeague;
            string? clan = Membership(db,player);
            string entity = current == PersonalLeague ? player : clan ?? "";
            int id = requested ?? active;
            var season = db.Read("SELECT id,start,\"end\",closed FROM league_seasons WHERE id=$s",
                SeasonRow,("$s",id)).SingleOrDefault() ?? throw ClanError.Missing();
            var previous = db.Read("SELECT id,start,\"end\",closed FROM league_seasons WHERE closed=1 ORDER BY id DESC LIMIT 24",SeasonRow);
            int frame = (int)db.Value<long>("SELECT COALESCE(MAX(tier),-1) FROM league_clan_emblems WHERE clan=$c",("$c",clan));
            const string ranking = """
                WITH ranked AS (
                 SELECT e.entity,e.name,e.tier,e.points,e.wins,
                 CASE WHEN e.rank>0 THEN e.rank ELSE RANK() OVER(ORDER BY e.points DESC) END AS place,
                 e.next_tier,
                 CASE WHEN e.scope=0 THEN COALESCE((SELECT MAX(tier) FROM league_rewards r WHERE r.player=e.entity AND r.season<e.season),-1)
                 ELSE COALESCE((SELECT tier FROM league_clan_emblems c WHERE c.clan=e.entity),-1) END AS frame,
                 ROW_NUMBER() OVER(ORDER BY e.points DESC,e.entity) AS position
                 FROM league_entries e WHERE e.season=$s AND e.scope=$o
                )
                """;
            (string,object?)[] args = [("$s",id),("$o",current),("$e",entity)];
            const string columns = " SELECT entity,name,tier,points,wins,place,next_tier,frame FROM ranked ";
            var top = db.Read(ranking+columns+"ORDER BY position LIMIT 20",RankRow,args);
            var self = db.Read(ranking+columns+"WHERE entity=$e",RankRow,args).SingleOrDefault();
            var nearby = self == null ? [] : db.Read(ranking+columns+
                "WHERE ABS(position-(SELECT position FROM ranked WHERE entity=$e))<=2 ORDER BY position",RankRow,args);
            return new LeagueBoard(now.ToUnixTimeSeconds(),season,previous,top,nearby,self,Division(db,current,entity),league.Tiers,
                season.Closed && current == PersonalLeague ? ReadReward(db,player,id) : null,frame,league.MilestoneWins);
        });
    }

    public LeagueReward ClaimLeagueReward(string player,int season,string? wallet)
    {
        if (wallet == null || !Guid.TryParseExact(wallet,"N",out var parsed) || parsed.ToString("N") != wallet)
            throw ClanError.Invalid("league_wallet");
        return Write(db =>
        {
            AdvanceLeague(db,leagueClock.GetUtcNow());
            var reward = ReadReward(db,player,season) ?? throw ClanError.Missing();
            db.Run("UPDATE league_rewards SET wallet=$w WHERE season=$s AND player=$p AND wallet IS NULL",
                ("$w",wallet),("$s",season),("$p",player));
            if (db.Value<string>("SELECT wallet FROM league_rewards WHERE season=$s AND player=$p",
                ("$s",season),("$p",player)) != wallet) throw ClanError.Conflict("league_other_wallet");
            db.Run("INSERT INTO league_wallet_owners VALUES($w,$p) ON CONFLICT DO NOTHING",("$w",wallet),("$p",player));
            if (db.Value<string>("SELECT player FROM league_wallet_owners WHERE wallet=$w",("$w",wallet)) != player)
                throw ClanError.Conflict("league_wallet_account");
            return reward with { Claimed=true };
        });
    }

    static LeagueReward? ReadReward(ClanDb db,string player,int season) => db.Read("""
        SELECT tier,rank,next_tier,gold,cosmetic,wallet IS NOT NULL FROM league_rewards WHERE season=$s AND player=$p
        """, r => new LeagueReward($"league-{season}-{player}",season,r.GetInt32(0),r.GetInt32(1),r.GetInt32(2),
            r.GetInt32(3),r.GetString(4),r.GetBoolean(5)),("$s",season),("$p",player)).SingleOrDefault();
    static LeagueSeason SeasonRow(System.Data.Common.DbDataReader r) => new(r.GetInt32(0),r.GetInt64(1),r.GetInt64(2),r.GetInt32(3) != 0);
    static LeagueRow RankRow(System.Data.Common.DbDataReader r) => new(r.GetString(0),r.GetString(1),r.GetInt32(2),
        r.GetInt64(3),r.GetInt32(4),r.GetInt32(5),r.GetInt32(6),r.GetInt32(7));
}
