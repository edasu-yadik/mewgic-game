#nullable enable
using System.Diagnostics;
using System.Globalization;
using Mewgic.Core;
using Newtonsoft.Json;

const string resources = "Assets/Resources/Levels/";
const string fallbackPath = resources + "GeneratedFallbacksV2.json";
const string output = "docs/ProductionReports/Part16A/";
var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
Directory.CreateDirectory(output);

if (args.Contains("--build-fallbacks"))
{
    var pool = new List<GeneratedLevel>();
    var signatures = new HashSet<string>(StringComparer.Ordinal);
    var buckets = new[] { (3, LevelDifficulty.Easy), (3, LevelDifficulty.Normal), (4, LevelDifficulty.Normal),
        (4, LevelDifficulty.Hard), (4, LevelDifficulty.Expert) };
    foreach (var (colors, tier) in buckets)
    {
        for (int bucket = 0; bucket < LevelGenerator.PartitionCount(colors); bucket++)
        {
            int found = 0;
            for (int attempt = 1; attempt <= 480 && found < (tier == LevelDifficulty.Easy ? 5 : 8); attempt++)
            {
                var request = new GeneratorRequest
                {
                    level = 100000 + bucket, version = 2, colors = colors, target = tier,
                    seed = (StablePrng.Mix((uint)(attempt + bucket * 1024 + colors * 20000 + (int)tier * 40000)) & ~15u) | (uint)bucket
                };
                var result = LevelGenerator.Generate(request, budget: new GenerationBudget { candidates = 16 });
                if (result.level == null || !signatures.Add(result.level.signature)) continue;
                pool.Add(result.level);
                found++;
            }
            if (found == 0) throw new InvalidOperationException($"Yetersiz yedek: {colors}/{tier}/{bucket}: {found}");
        }
        Console.WriteLine($"Yedek havuzu {colors}/{tier}: {pool.Count}");
    }

    for (int id = 3; id <= 20; id++)
    {
        var request = WorldMapRules.Request(id);
        var minimal = new GenerationBudget { candidates = 1, statesPerCandidate = 1, totalStates = 1 };
        if (LevelGenerator.Resolve(request, null, pool.ToArray(), minimal).level != null) continue;
        uint partition = request.seed & 15;
        bool found = false;
        for (uint trial = 1; trial < 500 && !found; trial++)
        {
            request.seed = (StablePrng.Mix(trial + (uint)id * 1024) & ~15u) | partition;
            var generated = LevelGenerator.Generate(request).level;
            if (generated == null) continue;
            if (signatures.Add(generated.signature)) pool.Add(generated);
            found = true;
        }
        Require(found, "Öğretici için uygun yedek " + id);
    }
    File.WriteAllText(fallbackPath, JsonConvert.SerializeObject(pool));
    return;
}

var fallback = JsonConvert.DeserializeObject<GeneratedLevel[]>(File.ReadAllText(fallbackPath))!;
var legacy = JsonConvert.DeserializeObject<GeneratedLevel[]>(File.ReadAllText(resources + "GeneratedFallbacks.json"))!;
foreach (var level in fallback.Concat(legacy))
{
    level.Validate();
    Require(LevelGenerator.Exact(level.Export(), LevelGenerator.Reproduce(level)), "Yedek tarif aynı tahtayı üretmeli");
    Require(LevelSignatures.CanonicalReference(level.Export()) == level.signature, "Eski ve paketli imza eşitliği");
    var verified = PuzzleSolver.Solve(level.Export(), 20000);
    Require(verified.Status == SearchStatus.Solved && verified.Moves.Count == level.difficulty.optimalMoves,
        "Yedek havuzu en kısa çözümü bağımsız aramayla doğrulandı");
}

int count = args.Length == 0 ? 10000 : int.Parse(args[0], CultureInfo.InvariantCulture);
var rows = new AuditRow[count];
Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, Environment.ProcessorCount) }, index =>
{
    int id = index + 3;
    var request = WorldMapRules.Request(id);
    var timer = Stopwatch.StartNew();
    var result = LevelGenerator.Resolve(request, null, fallback);
    double wall = timer.Elapsed.TotalMilliseconds;
    var level = result.level;
    if (level != null)
    {
        level.Validate();
        Require(LevelGenerator.Exact(level.Export(), LevelGenerator.Reproduce(level)), $"Tarif seed {id}");
        Require(LevelGenerator.SignaturePartition(level.signature) % LevelGenerator.PartitionCount(request.colors) == id % LevelGenerator.PartitionCount(request.colors), $"İmza dilimi {id}");
        Require(level.difficulty.optimalMoves <= request.EarlyMoveCeiling, $"Öğretici üst sınırı {id}");
        Require((int)level.difficulty.measured <= (int)request.target, $"Hedeften zor {id}");
        var other = LevelGenerator.Resolve(request, new[] { level.signature }, fallback.Reverse().ToArray()).level;
        Require(other != null && LevelGenerator.Exact(level.Export(), other.Export()) && level.signature == other.signature &&
            level.difficulty.score == other.difficulty.score, $"Geçmiş ve havuz sırası sonucu değiştirdi {id}");
    }
    rows[index] = new AuditRow(id, request.seed, request.target.ToString(), level?.difficulty.measured.ToString(), level?.signature,
        level?.fallback ?? false, level?.seed, level == null ? null : PuzzleSolverLayout(level.Export()), level?.difficulty.optimalMoves ?? 0, request.colors, request.colors + request.emptyBottles,
        request.emptyBottles * Puzzle.Capacity, level?.difficulty.mixedBoundaries ?? 0, level?.difficulty.initialChoices ?? 0,
        level?.difficulty.branchSum ?? 0, level?.difficulty.pathStates ?? 0, level?.difficulty.score ?? 0,
        result.candidates, result.states, result.partitionRejected, result.limits, result.unsolvable, result.shapeRejected,
        result.difficultyRejected, result.solverTicks * 1000d / Stopwatch.Frequency, wall, result.failure);
});

var accepted = rows.Where(row => row.Signature != null).ToArray();
var last = new Dictionary<string, int>();
int repeated = 0, repeatedWithin64 = 0, closest = int.MaxValue;
foreach (var row in accepted)
{
    if (last.TryGetValue(row.Signature!, out int prior))
    {
        repeated++;
        int gap = row.Level - prior;
        if (gap < 65) repeatedWithin64++;
        closest = Math.Min(closest, gap);
    }
    last[row.Signature!] = row.Level;
}
Require(closest >= 8, "Dilimler arası tekrar koruması");
Require(rows.Select(row => row.Seed).Distinct().Count() == count, "Ayrı istek seed sayısı");

var parity = accepted.Where(row => row.Level < 23 || row.Level % 37 == 0).Take(120).ToArray();
foreach (var row in parity)
{
    var level = LevelGenerator.Resolve(WorldMapRules.Request(row.Level), null, fallback).level!;
    var data = level.Export();
    var packed = PuzzleSolver.Solve(data, 20000);
    var reference = PuzzleSolver.SolveReference(data, 20000);
    Require(packed.Status == SearchStatus.Solved && packed.Status == reference.Status && packed.Visited == reference.Visited &&
        packed.Stored == reference.Stored && packed.PeakQueue == reference.PeakQueue &&
        packed.DeadEndStates == reference.DeadEndStates && packed.Moves.SequenceEqual(reference.Moves), $"BFS eşitliği {row.Level}");
}
var impossible = new[] { new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 } };
Require(PuzzleSolver.Solve(impossible).Status == SearchStatus.Unsolvable, "Çözümsüz durum");
Require(PuzzleSolver.Solve(fallback[0].Export(), 1).Status == SearchStatus.LimitReached, "Durum sınırı");
var tiny = new GenerationBudget { candidates = 1, statesPerCandidate = 1, totalStates = 1 };
var forcedCase = Enumerable.Range(4900, 201).Select(WorldMapRules.Request)
    .Select(request => (request, result: LevelGenerator.Resolve(request, null, fallback, tiny)))
    .First(pair => pair.result.limits > 0);
Require(forcedCase.result.level != null && forcedCase.result.level.fallback, "Gerçek arama sınırı sonrası güvenli yedek");
var closed = LevelGenerator.Resolve(forcedCase.request, null, Array.Empty<GeneratedLevel>(), tiny);
Require(closed.level == null && closed.limits > 0, "Yedeksiz kapalı hata");
forcedCase.result.level!.Validate();

var old = legacy[0].Copy();
string folder = Path.Combine(Path.GetTempPath(), "Mewgic-Part16A-" + Guid.NewGuid().ToString("N"));
var archive = new FileLevelArchive(folder);
archive.Write(old);
Require(archive.TryRead(old.level, out var pinned) && pinned.version == 1 &&
    LevelGenerator.Exact(old.Export(), pinned.Export()), "Eski pin birebir");
string primary = Path.Combine(folder, "level-" + old.level + ".json");
File.WriteAllText(primary, "broken");
Require(archive.TryRead(old.level, out pinned) && pinned.signature == old.signature, "Eski yedek korundu");
File.WriteAllText(primary + ".backup", "broken");
bool refused = false;
try { archive.TryRead(old.level, out _); } catch (InvalidDataException) { refused = true; }
Require(refused, "İki bozuk pin yeni bölümle değiştirilmez");

var authored = Enumerable.Range(1, 2).Select(id =>
{
    var data = JsonConvert.DeserializeObject<AuthoredLevel>(File.ReadAllText(resources + $"Level{id:D2}.json"))!;
    var solved = PuzzleSolver.Solve(data.bottles.Select(b => b.layers).ToArray());
    Require(solved.Status == SearchStatus.Solved, "El yapımı bölüm çözülebilir " + id);
    return new { level = id, moves = solved.Moves.Count, colors = data.bottles.SelectMany(b => b.layers).Distinct().Count() };
}).ToArray();
double Percentile(IEnumerable<double> data, double p)
{
    var sorted = data.OrderBy(x => x).ToArray();
    return sorted[(int)Math.Ceiling(p * sorted.Length) - 1];
}
var report = new
{
    success = accepted.Length == count,
    distinctSeeds = count,
    delivered = accepted.Length,
    failed = count - accepted.Length,
    direct = accepted.Count(row => !row.Fallback),
    fallback = accepted.Count(row => row.Fallback),
    fallbackPool = fallback.Length,
    forcedLimitLevel = forcedCase.request.level,
    verifiedFallbacksIncludingLegacy = fallback.Length + legacy.Length,
    maxOptimalMoves = accepted.Max(row => row.Moves),
    aboveTarget = accepted.Count(row => (int)Enum.Parse<LevelDifficulty>(row.Measured!) > (int)Enum.Parse<LevelDifficulty>(row.Target)),
    uniqueLayouts = last.Count,
    repeats = repeated,
    repeatsWithin64 = repeatedWithin64,
    shortestRepeatGap = closest == int.MaxValue ? (int?)null : closest,
    candidateTotals = new { candidates = rows.Sum(r => r.Candidates), states = rows.Sum(r => (long)r.States),
        limits = rows.Sum(r => r.Limits), unsolvable = rows.Sum(r => r.Unsolvable), shapeRejected = rows.Sum(r => r.ShapeRejected),
        difficultyRejected = rows.Sum(r => r.DifficultyRejected), partitionRejected = rows.Sum(r => r.PartitionRejected) },
    timingsMs = new { solverMedian = Percentile(rows.Select(r => r.SolverMs), .5), solverP95 = Percentile(rows.Select(r => r.SolverMs), .95),
        solverP99 = Percentile(rows.Select(r => r.SolverMs), .99), solverMax = rows.Max(r => r.SolverMs),
        resolveMedian = Percentile(rows.Select(r => r.ResolveMs), .5), resolveP95 = Percentile(rows.Select(r => r.ResolveMs), .95) },
    equivalentReferenceSolves = parity.Length,
    authored,
    early = rows.Where(row => row.Level <= 20),
    examples = rows.Where(row => row.Level is 100 or 500 or 1000 or 5000),
    phases = new[] { (3, 20), (21, 99), (100, 499), (500, 999), (1000, 4999), (5000, count + 2) }
        .Where(p => p.Item1 <= count + 2).Select(p =>
        {
            var phase = accepted.Where(r => r.Level >= p.Item1 && r.Level <= p.Item2).ToArray();
            return new { first = p.Item1, last = p.Item2, averageMoves = phase.Length == 0 ? 0 : phase.Average(r => r.Moves),
                averageScore = phase.Length == 0 ? 0 : phase.Average(r => r.Score) };
        }),
    failures = rows.Where(row => row.Failure != null)
};
File.WriteAllText(output + "AUDIT.json", JsonConvert.SerializeObject(report, settings));
File.WriteAllLines(output + "SEEDS.ndjson", rows.Select(row => JsonConvert.SerializeObject(row)));
Console.WriteLine(JsonConvert.SerializeObject(report, settings));
if (accepted.Length != count) Environment.ExitCode = 1;

static string PuzzleSolverLayout(int[][] board) => string.Join("|", board.Select(b => string.Join(",", b)));

static void Require(bool okay, string label)
{
    if (!okay) throw new InvalidOperationException(label);
}
record AuditRow(int Level, uint Seed, string Target, string? Measured, string? Signature, bool Fallback, uint? RecipeSeed, string? Layout, int Moves, int Colors,
    int Bottles, int FreeSlots, int MixedBoundaries, int InitialChoices, int BranchSum, int PathStates, int Score,
    int Candidates, int States, int PartitionRejected, int Limits, int Unsolvable, int ShapeRejected, int DifficultyRejected,
    double SolverMs, double ResolveMs, string? Failure);
sealed class AuthoredLevel { public GeneratedBottle[] bottles = Array.Empty<GeneratedBottle>(); }
