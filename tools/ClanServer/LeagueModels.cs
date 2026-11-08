using System.Globalization;

sealed class LeagueOptions
{
    public DateTimeOffset EpochUtc { get; init; } = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
    public const int Days = 28;
    public int BasePoints { get; init; } = 10;
    public int DifficultyStep { get; init; } = 4;
    public int StarPoints { get; init; } = 2;
    public int WeekendBonus { get; init; } = 12;
    public int MilestoneWins { get; init; } = 3;
    public int MilestoneBonus { get; init; } = 10;
    public int MovePercent { get; init; } = 20;
    public int MinimumPool { get; init; } = 3;
    public LeagueTier[] Tiers { get; set; } =
    [
        new(100, 40, ""), new(160, 65, ""), new(240, 100, "rose_effect"),
        new(340, 145, "crystal_effect"), new(480, 200, "crystal_cat"), new(650, 0, "league_cat")
    ];

    public void Validate()
    {
        if (EpochUtc.Offset != TimeSpan.Zero || EpochUtc.TimeOfDay != TimeSpan.Zero ||
            Tiers == null || Tiers.Length != 6 || Tiers.Any(t => t == null || t.MinimumPromotion is < 0 or > 1000000 ||
                t.Gold is < 0 or > 100000 || t.Cosmetic is not ("" or "rose_effect" or "crystal_effect" or "crystal_cat" or "league_cat")) ||
            BasePoints is < 1 or > 100 || DifficultyStep is < 0 or > 100 || StarPoints is < 0 or > 100 ||
            WeekendBonus is < 0 or > 100 || MilestoneWins is < 1 or > 1000 ||
            MilestoneBonus is < 0 or > 1000 || MovePercent is < 0 or > 49 || MinimumPool is < 3 or > 100)
            throw new InvalidOperationException("Invalid league configuration.");
    }

    public int Id(DateTimeOffset now) => now < EpochUtc ? 0 : checked((int)((now-EpochUtc).TotalDays / Days) + 1);
    public DateTimeOffset Start(int id) => EpochUtc.AddDays((id-1L)*Days);
}

sealed record LeagueTier(int Gold, int MinimumPromotion, string Cosmetic)
{
    public LeagueTier() : this(0, 0, "") { }
}

readonly record struct VerifiedWin(int Difficulty, int Stars);
sealed record LeagueRow(string Id, string Name, int Tier, long Points, int Wins, int Rank, int NextTier, int Frame);
sealed record LeagueSeason(int Id, long StartsAt, long EndsAt, bool Closed);
sealed record LeagueBoard(long ServerNow, LeagueSeason Season, LeagueSeason[] Previous, LeagueRow[] Top,
    LeagueRow[] Nearby, LeagueRow? Self, int CurrentTier, LeagueTier[] Tiers, LeagueReward? Reward, int ClanEmblem, int MilestoneWins);
sealed record LeagueReward(string Id, int Season, int Tier, int Rank, int NextTier, int Gold, string Cosmetic, bool Claimed);
sealed record LeagueClaimInput(string? WalletId);

sealed class LeagueQaClock(string path) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.Parse(File.ReadAllText(path), CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}

sealed class LeagueSeasonWorker(ClanStore store, ILogger<LeagueSeasonWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try { store.RollLeague(); }
            catch (Exception error) { log.LogError(error, "League rollover failed."); }
            try { store.RollEvents(); }
            catch (Exception error) { log.LogError(error, "Event rollover failed."); }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
