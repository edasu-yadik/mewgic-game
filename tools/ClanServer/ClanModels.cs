using Mewgic.Core;

sealed class ClanOptions
{
    public int Capacity { get; init; } = 30;
    public int WeeklyGoal { get; init; } = 150;
    public int WinPoints { get; init; } = 10;
    public int DailyPoints { get; init; } = 5;
    public int WeekendPoints { get; init; } = 15;
    public int PointsPerLevel { get; init; } = 100;
    public int MaxLevel { get; init; } = 10000;
    public void Validate()
    {
        if (Capacity is < 2 or > 100 || WeeklyGoal is < 1 or > 10000 || WinPoints is < 1 or > 1000 ||
            DailyPoints is < 0 or > 1000 || WeekendPoints is < 0 or > 1000 ||
            PointsPerLevel is < 1 or > 10000 || MaxLevel is < 2 or > 10000)
            throw new InvalidOperationException("Invalid clan configuration.");
    }
}

sealed record Credentials(string? Handle, string? Password);
sealed record Session(string Token);
sealed record ClanInput(string? Name, string? Description, string? Announcement, int Emblem, bool Open, int MinimumWins);
sealed record InviteInput(string? Handle);
sealed record WinInput(int Level, int[]? Moves, string? ClanId);
sealed record WinResult(int Points, bool Duplicate);
sealed record ActionResult(string State);
sealed record PlayerCard(string Id, string Handle, int Role, long Points);
sealed record ClanCard(string Id, string Name, string Description, int Emblem, bool Open, int MinimumWins,
    int Members, int Capacity, long Points, long Level, long WeeklyPoints, int WeeklyGoal);
sealed record ClanDetail(ClanCard Clan, string Announcement, int Role, PlayerCard[] Members,
    PlayerCard[] Requests, bool Requested, bool Invited, bool FirstWin, bool HundredPoints, bool WeeklyAchievement,
    string Week, bool WeekendEvent);
sealed record PlayerState(string Handle, string ClanId, int Wins, ClanCard[] Invitations, int WeeklyGoal,
    int WinPoints, int DailyPoints, int WeekendPoints);
sealed record SearchResult(ClanCard[] Clans);

sealed class ClanError(int status, string code) : Exception(code)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public static ClanError Invalid(string code) => new(400, code);
    public static ClanError Unauthorized() => new(401, "unauthorized");
    public static ClanError Forbidden() => new(403, "forbidden");
    public static ClanError Missing() => new(404, "not_found");
    public static ClanError Conflict(string code) => new(409, code);
}

sealed class ClanPuzzles
{
    readonly GeneratedLevel[] fallbacks;
    readonly int[][][] levels;
    readonly int maxLevel;
    readonly Microsoft.Extensions.Caching.Memory.MemoryCache cache = new(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions { SizeLimit = 100 });
    readonly SemaphoreSlim generators = new(2);
    public ClanPuzzles(int maxLevel)
    {
        this.maxLevel = maxLevel;
        string Load(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Levels", name + ".json"));
        levels = new[] { "Level01", "Level02" }.Select(name =>
            Newtonsoft.Json.Linq.JObject.Parse(Load(name))["bottles"]!.Select(b => b["layers"]!.ToObject<int[]>()!).ToArray()).ToArray();
        fallbacks = Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(Load("GeneratedFallbacksV2"))!;
        foreach (var fallback in fallbacks) fallback.Validate();
    }

    public void Validate(WinInput proof)
    {
        if (proof.Level < 1 || proof.Level > maxLevel || proof.Moves is not { Length: >= 2 and <= 1024 } moves ||
            moves.Length % 2 != 0 || moves.Any(m => m < 0 || m > 64))
            throw ClanError.Invalid("proof");
        var puzzle = new Puzzle(Layout(proof.Level));
        for (int i = 0; i < moves.Length; i += 2)
            if (puzzle.Won || puzzle.Pour(moves[i], moves[i + 1]) == 0) throw ClanError.Invalid("proof");
        if (!puzzle.Won) throw ClanError.Invalid("proof");
    }

    int[][] Layout(int level)
    {
        if (level <= levels.Length) return levels[level - 1];
        if (cache.TryGetValue(level, out var stored)) return (int[][])stored!;
        if (!generators.Wait(0)) throw new ClanError(503, "retry");
        try
        {
            var result = LevelGenerator.Resolve(WorldMapRules.Request(level), null, fallbacks);
            if (result.level == null) throw ClanError.Invalid("level");
            var layout = result.level.Export();
            using (var entry = cache.CreateEntry(level)) { entry.Value = layout; entry.Size = 1; }
            return layout;
        }
        finally { generators.Release(); }
    }
}
