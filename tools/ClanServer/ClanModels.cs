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
sealed record Session(string Token,string RefreshToken,string PlayerId,int ExpiresIn);
sealed record RefreshInput(string? RefreshToken);
sealed record DeleteAccountInput(string? Password);
sealed record AccountProgress(string PlayerId,int[] Levels,int Next);
sealed record AccountWin(int Level,string Day);
sealed record AccountLeague(int Season,long Points,int Rank);
sealed record AccountEvent(string Event,long Points);
sealed record AccountExport(string PlayerId,string Handle,string? ClanId,AccountWin[] Wins,
    AccountLeague[] League,AccountEvent[] Events,string[] EventReceipts);
sealed record ClanInput(string? Name, string? Description, string? Announcement, int Emblem, bool Open, int MinimumWins);
sealed record InviteInput(string? Handle);
sealed record WinInput(int Level, int[]? Moves, string? ClanId);
sealed record WinResult(int Points, bool Duplicate);
sealed record ActionResult(string State);
sealed record PlayerCard(string Id, string Handle, int Role, long Points);
sealed record ClanCard(string Id, string Name, string Description, int Emblem, bool Open, int MinimumWins,
    int Members, int Capacity, long Points, long Level, long WeeklyPoints, int WeeklyGoal, int LeagueEmblem);
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
    sealed record PuzzleSpec(int[][] Bottles,int ThreeStars,int TwoStars,int Difficulty);
    readonly PuzzleSpec[] levels;
    readonly int maxLevel;
    readonly Microsoft.Extensions.Caching.Memory.MemoryCache cache = new(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions { SizeLimit = 100 });
    readonly SemaphoreSlim generators = new(2);
    public ClanPuzzles(int maxLevel)
    {
        this.maxLevel = maxLevel;
        string Load(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Levels", name + ".json"));
        levels = new[] { "Level01", "Level02" }.Select((name,index) =>
        {
            var json = Newtonsoft.Json.Linq.JObject.Parse(Load(name));
            return new PuzzleSpec(json["bottles"]!.Select(b => b["layers"]!.ToObject<int[]>()!).ToArray(),
                json.Value<int>("threeStarMoves"),json.Value<int>("twoStarMoves"),index);
        }).ToArray();
        fallbacks = Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(Load("GeneratedFallbacksV2"))!;
        foreach (var fallback in fallbacks) fallback.Validate();
    }

    public VerifiedWin Validate(WinInput proof)
    {
        if (proof.Level < 1 || proof.Level > maxLevel || proof.Moves is not { Length: >= 2 and <= 1024 } moves ||
            moves.Length % 2 != 0 || moves.Any(m => m < 0 || m > 64))
            throw ClanError.Invalid("proof");
        var layout = Layout(proof.Level);
        var puzzle = new Puzzle(layout.Bottles);
        for (int i = 0; i < moves.Length; i += 2)
            if (puzzle.Won || puzzle.Pour(moves[i], moves[i + 1]) == 0) throw ClanError.Invalid("proof");
        if (!puzzle.Won) throw ClanError.Invalid("proof");
        return new VerifiedWin(layout.Difficulty, puzzle.Moves <= layout.ThreeStars ? 3 : puzzle.Moves <= layout.TwoStars ? 2 : 1);
    }

    PuzzleSpec Layout(int level)
    {
        if (level <= levels.Length) return levels[level - 1];
        if (cache.TryGetValue(level, out var stored)) return (PuzzleSpec)stored!;
        if (!generators.Wait(0)) throw new ClanError(503, "retry");
        try
        {
            var result = LevelGenerator.Resolve(WorldMapRules.Request(level), null, fallbacks);
            if (result.level == null) throw ClanError.Invalid("level");
            int moves = result.level.difficulty.optimalMoves;
            var layout = new PuzzleSpec(result.level.Export(),moves,moves+Math.Max(2,moves/2),(int)result.level.difficulty.measured);
            using (var entry = cache.CreateEntry(level)) { entry.Value = layout; entry.Size = 1; }
            return layout;
        }
        finally { generators.Release(); }
    }
}
