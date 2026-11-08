using System.Text.Json;
using System.Text.RegularExpressions;

sealed class EventConfig
{
    public int MaxActive { get; init; } = 4;
    public EventDefinition[] Events { get; init; } = [];
    public Dictionary<string, EventTier[]> Ladders { get; init; } = new();
    public Dictionary<string, EventDrop[]> Chests { get; init; } = new();
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static EventConfig Load(string path, int? maxActive)
    {
        var config = JsonSerializer.Deserialize<EventConfig>(File.ReadAllText(path),Json)
            ?? throw new InvalidOperationException("Events configuration is empty.");
        if (maxActive.HasValue)
            config = new EventConfig { MaxActive=maxActive.Value, Events=config.Events, Ladders=config.Ladders, Chests=config.Chests };
        config.Validate();
        return config;
    }

    void Validate()
    {
        if (MaxActive is < 1 or > 8 || Events.Length is < 1 or > 64) Invalid("calendar");
        foreach (var (id,drops) in Chests)
        {
            if (id is not ("normal" or "magic" or "crystal" or "legendary") || drops.Length is < 1 or > 32 || drops.Any(d => d.Weight is < 1 or > 10000))
                Invalid(id);
            foreach (var drop in drops) ValidatePrize(drop.Prize);
        }
        foreach (var (id,tiers) in Ladders)
        {
            if (!Key(id) || tiers.Length is < 1 or > 12 ||
                tiers.Any(t => t.Points is < 1 or > 1000000 || t.MaxRank is < 0 or > 1000) ||
                tiers.Zip(tiers.Skip(1)).Any(pair => pair.First.Points >= pair.Second.Points))
                Invalid(id);
            foreach (var tier in tiers)
            {
                if (tier.Chest.Length == 0) ValidatePrize(tier.Prize);
                else if (tier.Prize != null || !Chests.ContainsKey(tier.Chest)) Invalid(id);
            }
        }
        var keys = new HashSet<string>();
        foreach (var item in Events)
        {
            if (!Key(item.Id) || !keys.Add(item.Id) || item.Lane is < 0 or > 7 ||
                item.Icon is < 0 or > 11 || item.Mode is not ("solo" or "race" or "clan" or "cup") ||
                item.RepeatDays is < 0 or > 56 || item.RepeatDays != 0 && 56 % item.RepeatDays != 0 ||
                item.OffsetMinutes is < 0 or > 20000000 || item.DurationMinutes is < 1 or > 80640 ||
                item.RepeatDays > 0 && (item.OffsetMinutes >= item.RepeatDays*1440 || item.DurationMinutes > item.RepeatDays*1440) ||
                item.TitleTr.Length is < 1 or > 60 || item.TitleEn.Length is < 1 or > 60 ||
                item.InfoTr.Length is < 1 or > 240 || item.InfoEn.Length is < 1 or > 240 ||
                item.Rules.Length is < 1 or > 8 || !Ladders.TryGetValue(item.Ladder,out var tiers) ||
                tiers.Any(t => t.MaxRank > 0 && item.Mode is not ("race" or "cup")))
                Invalid(item.Id);
            foreach (var rule in item.Rules)
            {
                if (rule.Metric is not ("win" or "stars" or "difficulty" or "crystal" or "chain") ||
                    rule.Weight is < 1 or > 1000 || rule.MinStars is < 0 or > 3 ||
                    rule.MinDifficulty is < 0 or > 4 || rule.IslandModulo is < 0 or > 100 ||
                    rule.IslandRemainder < 0 || (rule.IslandModulo == 0 ? rule.IslandRemainder != 0 : rule.IslandRemainder >= rule.IslandModulo) ||
                    (rule.Metric == "chain" && (item.Rules.Length != 1 || item.Team)))
                    Invalid(item.Id);
            }
        }

        foreach (var lane in Events.GroupBy(e => e.Lane))
        {
            const int cycle = 56*1440;
            var recurring = lane.Where(e => e.RepeatDays != 0).SelectMany(e => e.Windows(0,cycle*2)).OrderBy(w => w.Start).ToArray();
            if (recurring.Zip(recurring.Skip(1)).Any(p => p.First.End > p.Second.Start)) Invalid("overlapping lane");
            var specials = lane.Where(e => e.RepeatDays == 0).OrderBy(e => e.OffsetMinutes).ToArray();
            if (specials.Zip(specials.Skip(1)).Any(p => p.First.OffsetMinutes+p.First.DurationMinutes > p.Second.OffsetMinutes) ||
                specials.Any(e => lane.Where(r => r.RepeatDays != 0)
                    .Any(r => r.Windows(e.OffsetMinutes,e.OffsetMinutes+e.DurationMinutes).Any())))
                Invalid("overlapping special event");
        }
    }

    public EventSnapshot Snapshot(EventDefinition item) => new(item,Ladders[item.Ladder],
        Ladders[item.Ladder].Where(t => t.Chest != "").Select(t => t.Chest).Distinct().ToDictionary(id => id,id => Chests[id]));

    static bool Key(string key) => Regex.IsMatch(key,@"\A[a-z][a-z0-9_]{0,29}\z");
    static void ValidatePrize(EventPrize? prize)
    {
        if (prize == null || prize.Gold is < 0 or > 100000 || prize.Count is < 0 or > 100 ||
            prize.Booster is not ("" or "hint" or "undo" or "extra" or "shuffle") ||
            (prize.Count > 0) != (prize.Booster != "") ||
            prize.Cosmetic is not ("" or "rose_effect" or "crystal_effect" or "crystal_cat" or "league_cat") ||
            prize.Gold == 0 && prize.Count == 0 && prize.Cosmetic == "") Invalid("prize");
    }
    static void Invalid(string field) => throw new InvalidOperationException("Invalid events configuration: " + field);
}

sealed record EventDefinition(string Id,string TitleTr,string TitleEn,string InfoTr,string InfoEn,
    int Icon,string Mode,int Lane,int RepeatDays,int OffsetMinutes,int DurationMinutes,string Ladder,EventRule[] Rules)
{
    public bool Team => Mode is "clan" or "cup";
    public bool Ranked => Mode is "race" or "cup";
    public IEnumerable<(long Start,long End)> Windows(long from,long until)
    {
        if (RepeatDays == 0)
        {
            if (OffsetMinutes < until && OffsetMinutes+DurationMinutes > from)
                yield return (OffsetMinutes,OffsetMinutes+DurationMinutes);
            yield break;
        }
        long repeat = RepeatDays*1440;
        long step = Math.Max(0,(from-OffsetMinutes-DurationMinutes)/repeat);
        for (long start=OffsetMinutes+step*repeat;start<until;start+=repeat)
            if (start+DurationMinutes > from) yield return (start,start+DurationMinutes);
    }
}

sealed record EventRule(string Metric,int Weight,int MinStars=0,int MinDifficulty=0,int IslandModulo=0,int IslandRemainder=0)
{
    public bool Matches(int level,VerifiedWin proof) => proof.Stars >= MinStars && proof.Difficulty >= MinDifficulty &&
        (IslandModulo == 0 || ((level-1)/Mewgic.Core.WorldMapRules.LevelsPerIsland)%IslandModulo == IslandRemainder);

    public int Score(VerifiedWin proof) => Weight * (Metric switch
    {
        "stars" => proof.Stars, "difficulty" or "crystal" => proof.Difficulty+1, _ => 1
    });
}
sealed record EventTier(int Points,EventPrize? Prize=null,string Chest="",int MaxRank=0);
sealed record EventPrize(int Gold=0,string Booster="",int Count=0,string Cosmetic="");
sealed record EventDrop(int Weight,EventPrize Prize);
sealed record EventSnapshot(EventDefinition Definition,EventTier[] Tiers,Dictionary<string,EventDrop[]> Chests);
sealed record EventWindow(string Id,long Start,long End,EventSnapshot Snapshot);
sealed record EventRank(string Name,long Points,int Rank);
sealed record EventMilestone(int Index,int Points,int MaxRank,bool Eligible,bool Claimed,string Receipt,EventPrize? Prize,string Chest,EventDrop[] Pool);
sealed record EventCard(string Id,string Kind,string TitleTr,string TitleEn,string InfoTr,string InfoEn,
    int Icon,string Mode,long StartsAt,long EndsAt,int Status,long Points,long Personal,int Rank,
    EventMilestone[] Milestones,EventRank[] Leaders);
sealed record EventBoard(long ServerNow,int HistoryPage,bool OlderHistory,EventCard[] Cards);
sealed record EventClaimInput(int Milestone,string? WalletId);
sealed record EventGrant(string Id,string EventId,int Milestone,string Chest,EventPrize Prize);
