using System.Threading;
using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public enum LevelDifficulty{Tutorial,Easy,Normal,Hard,Expert}
    [Serializable]public sealed class GeneratorRequest
    {
        public const int CurrentVersion = 2;
        public const int FrontierVersion = 3;
        public int level,version=1,capacity=4,colors=3,emptyBottles=2;
        public uint seed;
        public LevelDifficulty target=LevelDifficulty.Normal;

        public void Validate()
        {
            if (level < 3 || (version != 1 && version != CurrentVersion && version != FrontierVersion) || capacity != Puzzle.Capacity ||
                colors < 2 || colors > 4 || emptyBottles < 1 || emptyBottles > 2 || !Enum.IsDefined(typeof(LevelDifficulty), target))
                throw new ArgumentException("Üretici parametreleri desteklenmiyor.");
        }

        public static GeneratorRequest ForLevel(int id)
        {
            if (id < 3) throw new ArgumentOutOfRangeException(nameof(id));
            int beat = (id % 10 + 7) % 10;
            LevelDifficulty tier = id <= 8 ? LevelDifficulty.Easy :
                id <= 20 ? LevelDifficulty.Normal :
                id < 100 ? (beat < 6 ? LevelDifficulty.Normal : LevelDifficulty.Hard) :
                id < 500 ? (beat < 3 ? LevelDifficulty.Normal : LevelDifficulty.Hard) :
                id < 1000 ? (beat < 2 ? LevelDifficulty.Normal : beat < 8 ? LevelDifficulty.Hard : LevelDifficulty.Expert) :
                id < 5000 ? (beat < 6 ? LevelDifficulty.Hard : LevelDifficulty.Expert) :
                (beat < 3 ? LevelDifficulty.Hard : LevelDifficulty.Expert);
            return new GeneratorRequest
            {
                level = id, version = CurrentVersion, seed = SeedForLevel(id),
                target = tier, colors = id <= 20 || tier == LevelDifficulty.Easy ? 3 : 4
            };
        }

        public static GeneratorRequest ForLegacyLevel(int id)
        {
            if (id < 3) throw new ArgumentOutOfRangeException(nameof(id));
            var cycle = new[] { LevelDifficulty.Normal, LevelDifficulty.Normal, LevelDifficulty.Hard,
                LevelDifficulty.Easy, LevelDifficulty.Normal, LevelDifficulty.Hard, LevelDifficulty.Normal,
                LevelDifficulty.Expert, LevelDifficulty.Easy, LevelDifficulty.Normal };
            var tier = id < 8 ? LevelDifficulty.Easy : id < 18 ? LevelDifficulty.Normal : cycle[(id - 18) % cycle.Length];
            return new GeneratorRequest { level = id, version = 1, seed = StablePrng.Mix((uint)id ^ 0x4D455747u),
                target = tier, colors = tier == LevelDifficulty.Easy || tier == LevelDifficulty.Normal && id % 3 != 0 ? 3 : 4 };
        }

        // The odd multiplier permutes the 28-bit blocks, and preserves the level's partition bits.
        static uint SeedForLevel(int id) => unchecked((((uint)(id >> 4) * 0x9E3779B9u + 0x0A17C9E5u) << 4) | (uint)(id & 15));

        internal int EarlyMoveCeiling => version == 1 || level > 20 ? int.MaxValue : level <= 5 ? 5 : level <= 8 ? 6 : 10;
    }

    // Xorshift32: unsigned 32-bit shifts/xor. No runtime-dependent Random or GetHashCode.
    public struct StablePrng
    {
        uint state;public StablePrng(uint seed){state=seed==0?0x6D2B79F5u:seed;}
        public uint Next(){unchecked{state^=state<<13;state^=state>>17;state^=state<<5;return state;}}
        public int Below(int exclusive){if(exclusive<1)throw new ArgumentOutOfRangeException(nameof(exclusive));uint bound=(uint)exclusive,threshold=unchecked(0u-bound)%bound;uint x;do{x=Next();}while(x<threshold);return(int)(x%bound);}
        public static uint Mix(uint x){unchecked{x^=x>>16;x*=0x7FEB352Du;x^=x>>15;x*=0x846CA68Bu;return x^(x>>16);}}
    }
    [Serializable]public sealed class GeneratedBottle{public int[] layers;}
    [Serializable]public sealed class GeneratedMove{public int source,target;}
    [Serializable]public sealed class DifficultyMetrics
    {public DifficultyMetrics Copy()=>(DifficultyMetrics)MemberwiseClone();public int optimalMoves,mixedBoundaries,initialChoices,branchSum,pathStates,visited,stored,peakQueue,deadEndStates,score,bottleCount,freeSlots,specialMechanics;public LevelDifficulty measured;}
    [Serializable]public sealed class GeneratedLevel
    {
        public int format=1,level,version=1,requestVersion,capacity=4,colors,emptyBottles,attempt;public uint seed,requestSeed;public LevelDifficulty recipeDifficulty;public string signature;public bool fallback;public LevelDifficulty target;public DifficultyMetrics difficulty;public GeneratedBottle[] bottles;public GeneratedMove[] solution;
        public GeneratedLevel Copy(){var c=(GeneratedLevel)MemberwiseClone();c.bottles=bottles.Select(b=>new GeneratedBottle{layers=(int[])b.layers.Clone()}).ToArray();c.solution=solution.Select(m=>new GeneratedMove{source=m.source,target=m.target}).ToArray();c.difficulty=difficulty.Copy();return c;}
        public int[][] Export()=>bottles.Select(b=>(int[])b.layers.Clone()).ToArray();
        public void Validate(){if(format!=1||level<3||version<1||capacity!=4||colors<2||colors>4||emptyBottles<1||emptyBottles>2||bottles==null||bottles.Length!=colors+emptyBottles||bottles.Any(b=>b?.layers==null||b.layers.Any(c=>c<0||c>=colors))||bottles.Count(b=>b.layers.Length==0)!=emptyBottles||solution==null||solution.Length<1||solution.Length>200||difficulty==null||difficulty.optimalMoves!=solution.Length)throw new ArgumentException("Üretilmiş bölüm kaydı geçersiz.");var data=Export();if(data.SelectMany(b=>b).GroupBy(c=>c).Count()!=colors||data.SelectMany(b=>b).GroupBy(c=>c).Any(g=>g.Count()!=4)||signature!=LevelSignatures.Canonical(data))throw new ArgumentException("Bölüm renkleri/imzası geçersiz.");var model=new Puzzle(data);if(model.Won)throw new ArgumentException("Başlangıç zaten çözülmüş.");foreach(var m in solution)if(m==null||model.Pour(m.source,m.target)==0)throw new ArgumentException("Bölüm çözüm sertifikası geçersiz.");if(!model.Won)throw new ArgumentException("Çözüm kazanmayla bitmiyor.");}
    }
    public static class LevelSignatures
    {
        public static string Canonical(int[][] data)
        {
            if (data.Length is >= 1 and <= 8 && data.All(b => b.Length == 0 || b.Length == Puzzle.Capacity) &&
                data.Count(b => b.Length != 0) <= 4 && data.All(b => b.All(c => c >= 0 && c < 4)))
                return PackedSignature(data);
            return CanonicalReference(data);
        }
        internal static string CanonicalReference(int[][] data)
        {
            var colors=data.SelectMany(b=>b).Distinct().OrderBy(c=>c).ToArray();if(colors.Length>4)throw new ArgumentException("En fazla dört renk.");string best=null;var perm=Enumerable.Range(0,colors.Length).ToArray();Permute(data,colors,perm,0,ref best);return best??string.Join("|",data.Select(b=>""));
        }
        static readonly int[][] SignaturePermutations = MakePermutations();

        static int[][] MakePermutations()
        {
            var result = new List<int[]>();
            for (int a = 0; a < 4; a++)
                for (int b = 0; b < 4; b++)
                    for (int c = 0; c < 4; c++)
                        for (int d = 0; d < 4; d++)
                            if (a != b && a != c && a != d && b != c && b != d && c != d)
                                result.Add(new[] { a + 1, b + 1, c + 1, d + 1 });
            return result.ToArray();
        }

        static string PackedSignature(int[][] data)
        {
            Span<int> sorted = stackalloc int[4];
            ulong best = ulong.MaxValue;
            int count = 0;
            foreach (var map in SignaturePermutations)
            {
                count = 0;
                foreach (var row in data)
                {
                    if (row.Length == 0) continue;
                    int value = (map[row[0]] << 12) | (map[row[1]] << 8) | (map[row[2]] << 4) | map[row[3]];
                    int j = count++;
                    while (j > 0 && sorted[j - 1] > value) { sorted[j] = sorted[j - 1]; j--; }
                    sorted[j] = value;
                }
                ulong key = 0;
                for (int i = 0; i < count; i++) key = (key << 16) | (uint)sorted[i];
                if (key < best) best = key;
            }

            var output = new char[data.Length - 1 + count * Puzzle.Capacity];
            int at = 0;
            for (int i = 0; i < data.Length - count; i++)
                if (i < data.Length - 1) output[at++] = '|';
            for (int i = count - 1; i >= 0; i--)
            {
                int row = (int)(best >> (i * 16)) & 65535;
                for (int shift = 12; shift >= 0; shift -= 4)
                    output[at++] = (char)('A' + ((row >> shift) & 15) - 1);
                if (i > 0) output[at++] = '|';
            }
            return new string(output);
        }

        static void Permute(int[][] data,int[] colors,int[] map,int at,ref string best){if(at==map.Length){var rows=data.Select(b=>string.Concat(b.Select(c=>(char)('A'+map[Array.IndexOf(colors,c)])))).OrderBy(b=>b,StringComparer.Ordinal);string key=string.Join("|",rows);if(best==null||string.CompareOrdinal(key,best)<0)best=key;return;}for(int i=at;i<map.Length;i++){int t=map[i];map[i]=map[at];map[at]=t;Permute(data,colors,map,at+1,ref best);t=map[i];map[i]=map[at];map[at]=t;}}
    }
    public sealed class GenerationBudget{public int candidates=32,statesPerCandidate=6000,totalStates=30000;public void Validate(){if(candidates<1||candidates>128||statesPerCandidate<1||statesPerCandidate>20000||totalStates<1||totalStates>100000)throw new ArgumentException("Üretim bütçesi geçersiz.");}}
    public sealed class GenerationResult{public GeneratedLevel level;public int candidates,limits,duplicates,difficultyRejected,shapeRejected,partitionRejected,unsolvable,states;public long solverTicks;public string failure;}
    public static class LevelGenerator
    {
        struct Reverse{public int a,b,k;}
        public static GenerationResult Generate(GeneratorRequest request,IEnumerable<string> recent=null,GenerationBudget budget=null,CancellationToken cancellation=default)
        {
            if(request==null)throw new ArgumentNullException(nameof(request));request.Validate();budget=budget??new GenerationBudget{candidates=request.version>=2?64:32};budget.Validate();var history=new HashSet<string>(request.version==1?(recent??Array.Empty<string>()):Array.Empty<string>(),StringComparer.Ordinal);var result=new GenerationResult();
            for(int attempt=0;attempt<budget.candidates&&result.states<budget.totalStates;attempt++){
                cancellation.ThrowIfCancellationRequested();
                result.candidates++;var data=BuildCandidate(request,attempt);if(data==null){result.shapeRejected++;continue;}string sig=LevelSignatures.Canonical(data);if(history.Contains(sig)){result.duplicates++;continue;}if(!PartitionMatches(request,sig)){result.partitionRejected++;continue;}
                long started=System.Diagnostics.Stopwatch.GetTimestamp();
                var search=PuzzleSolver.Solve(data,Math.Min(budget.statesPerCandidate,budget.totalStates-result.states),cancellation);result.solverTicks+=System.Diagnostics.Stopwatch.GetTimestamp()-started;result.states+=Math.Max(search.Visited,search.Stored);if(search.Status!=SearchStatus.Solved){if(search.Status==SearchStatus.LimitReached)result.limits++;else if(request.version==1)throw new InvalidOperationException("Ters hamle sertifikasıyla çelişen solver sonucu.");else result.unsolvable++;continue;}
                var metrics=Measure(data,search,request);if(metrics.measured!=request.target||metrics.optimalMoves>request.EarlyMoveCeiling){result.difficultyRejected++;continue;}
                result.level=new GeneratedLevel{level=request.level,version=request.version,requestVersion=request.version,seed=CandidateSeed(request.seed,attempt),requestSeed=request.seed,recipeDifficulty=request.target,colors=request.colors,emptyBottles=request.emptyBottles,target=request.target,attempt=attempt,signature=sig,difficulty=metrics,bottles=data.Select(b=>new GeneratedBottle{layers=b}).ToArray(),solution=search.Moves.Select(m=>new GeneratedMove{source=m.Source,target=m.Target}).ToArray()};result.level.Validate();return result;
            }
            result.failure="Doğrulanmış ve hedefe uygun yeni aday bütçe içinde bulunamadı.";return result;
        }
        public static GenerationResult Resolve(GeneratorRequest request,IEnumerable<string> recent,GeneratedLevel[] fallbackPool,GenerationBudget budget=null,CancellationToken cancellation=default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var history = new HashSet<string>(request.version == 1 ? recent ?? Array.Empty<string>() : Array.Empty<string>(),
                StringComparer.Ordinal);
            var result = Generate(request, history, budget, cancellation);
            cancellation.ThrowIfCancellationRequested();
            if (result.level != null) return result;
            var choices = (fallbackPool ?? Array.Empty<GeneratedLevel>()).Where(l =>
                l != null && l.difficulty != null && l.colors == request.colors && l.emptyBottles == request.emptyBottles &&
                !history.Contains(l.signature) && PartitionMatches(request, l.signature) &&
                (request.version == 1 || (l.difficulty.optimalMoves <= request.EarlyMoveCeiling &&
                    (int)l.difficulty.measured <= (int)request.target &&
                    (int)l.difficulty.measured >= (int)request.target - 1)))
                .OrderBy(l => Math.Abs((int)l.difficulty.measured - (int)request.target))
                .ThenBy(l => l.signature, StringComparer.Ordinal).ToArray();
            if (choices.Length == 0) return result;
            int index = 0;
            if (request.version >= 2)
            {
                int eligible = choices.Count(l => l.difficulty.measured == choices[0].difficulty.measured);
                index = new StablePrng(StablePrng.Mix(request.seed ^ 0xB1425EEDu)).Below(eligible);
            }
            var chosen = choices[index].Copy();
            chosen.Validate();
            chosen.level = request.level;
            chosen.requestSeed = request.seed;
            chosen.requestVersion = request.version;
            chosen.target = request.target;
            chosen.fallback = true;
            chosen.attempt = -1;
            chosen.Validate();
            result.level = chosen;
            result.failure = null;
            return result;
        }

        public const int RepeatWindowV2 = 16;

        public static int SignaturePartition(string signature)
        {
            uint hash = 2166136261u;
            foreach (char c in signature) hash = unchecked((hash ^ c) * 16777619u);
            return (int)(StablePrng.Mix(hash) % RepeatWindowV2);
        }

        public static int PartitionCount(int colors) => colors <= 3 ? 8 : RepeatWindowV2;

        static bool PartitionMatches(GeneratorRequest request, string signature)
        {
            if (request.version == 1) return true;
            int partitions = PartitionCount(request.colors);
            return signature != null && SignaturePartition(signature) % partitions == request.seed % (uint)partitions;
        }

        // Every selected predecessor is replayed through the actual maximal-pour rule.
        public static uint CandidateSeed(uint seed,int attempt)=>StablePrng.Mix(unchecked(seed^((uint)attempt*0x9E3779B9u)^0xA17C9E5Du));
        public static int[][] Reproduce(GeneratedLevel record){record.Validate();var request=new GeneratorRequest{level=record.level,version=record.version,capacity=record.capacity,colors=record.colors,emptyBottles=record.emptyBottles,target=record.recipeDifficulty};request.Validate();return BuildSeeded(request,record.seed);}
        static int[][] BuildCandidate(GeneratorRequest r,int attempt)=>BuildSeeded(r,CandidateSeed(r.seed,attempt));
        static int[][] BuildSeeded(GeneratorRequest r,uint seed)
        {
            if (r.version >= 2) return BuildMixedBoard(r, seed);
            var rng=new StablePrng(seed);var board=Enumerable.Range(0,r.colors+r.emptyBottles).Select(i=>i<r.colors?Enumerable.Repeat(i,4).ToArray():Array.Empty<int>()).ToArray();int[][] last=null;var inverse=new List<PuzzleMove>();int validSteps=0;var seen=new HashSet<string>{PuzzleSolver.StateKey(board)};
            int steps=r.target==LevelDifficulty.Tutorial?8:r.target==LevelDifficulty.Easy?12:r.target==LevelDifficulty.Normal?28:r.target==LevelDifficulty.Hard?52:84;
            for(int step=0;step<steps;step++){
                var options=new List<Reverse>();for(int a=0;a<board.Length;a++){var row=board[a];if(row.Length==0)continue;int c=row[row.Length-1],run=1;while(run<row.Length&&row[row.Length-run-1]==c)run++;for(int k=1;k<=run;k++){int remaining=row.Length-k;if(remaining>0&&row[remaining-1]!=c)continue;for(int b=0;b<board.Length;b++){if(a==b||board[b].Length+k>4)continue;int oldRun=0;while(oldRun<board[b].Length&&board[b][board[b].Length-oldRun-1]==c)oldRun++;if(Math.Min(k+oldRun,4-remaining)==k)options.Add(new Reverse{a=a,b=b,k=k});}}}
                bool moved=false;while(options.Count>0){int pick=rng.Below(options.Count);var o=options[pick];options.RemoveAt(pick);var predecessor=board.Select(b=>(int[])b.Clone()).ToArray();int c=board[o.a][board[o.a].Length-1];predecessor[o.a]=board[o.a].Take(board[o.a].Length-o.k).ToArray();predecessor[o.b]=board[o.b].Concat(Enumerable.Repeat(c,o.k)).ToArray();if(!seen.Add(PuzzleSolver.StateKey(predecessor)))continue;
                    var proof=new Puzzle(predecessor);if(proof.Pour(o.b,o.a)!=o.k||!Exact(proof.Export(),board))throw new InvalidOperationException("Geçersiz ters dönüşüm.");board=predecessor;inverse.Add(new PuzzleMove(o.b,o.a));moved=true;break;}
                if(!moved)break;if(board.Count(b=>b.Length==0)==r.emptyBottles&&!new Puzzle(board).Won){last=board.Select(b=>(int[])b.Clone()).ToArray();validSteps=inverse.Count;}
            }
            if(last==null)return null;var replay=new Puzzle(last);for(int i=validSteps-1;i>=0;i--)if(replay.Pour(inverse[i].Source,inverse[i].Target)==0)throw new InvalidOperationException("Ters yol geri oynatılamadı.");if(!replay.Won)throw new InvalidOperationException("Ters yol çözüme ulaşmadı.");return last;
        }
        static int[][] BuildMixedBoard(GeneratorRequest request, uint seed)
        {
            var random = new StablePrng(seed);
            var layers = Enumerable.Range(0, request.colors * Puzzle.Capacity).Select(i => i / Puzzle.Capacity).ToArray();
            if (request.target == LevelDifficulty.Easy || request.target == LevelDifficulty.Tutorial ||
                request.colors == 4 && request.target == LevelDifficulty.Normal)
            {
                int swaps = 1 + random.Below(8);
                for (int i = 0; i < swaps; i++)
                {
                    int a = random.Below(layers.Length);
                    int b = (a + Puzzle.Capacity + random.Below(layers.Length - Puzzle.Capacity)) % layers.Length;
                    (layers[a], layers[b]) = (layers[b], layers[a]);
                }
            }
            else
            {
                for (int i = layers.Length - 1; i > 0; i--)
                {
                    int j = random.Below(i + 1);
                    (layers[i], layers[j]) = (layers[j], layers[i]);
                }
            }
            var board = new int[request.colors + request.emptyBottles][];
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = i < request.colors ? new int[Puzzle.Capacity] : Array.Empty<int>();
                if (i < request.colors) Array.Copy(layers, i * Puzzle.Capacity, board[i], 0, Puzzle.Capacity);
            }
            return new Puzzle(board).Won ? null : board;
        }

        public static bool Exact(int[][] a,int[][] b)=>a.Length==b.Length&&a.Select((r,i)=>r.SequenceEqual(b[i])).All(x=>x);
        public static int ProductiveChoices(int[][] data){var keys=new HashSet<string>();string before=PuzzleSolver.StateKey(data);for(int a=0;a<data.Length;a++)for(int b=0;b<data.Length;b++){var p=new Puzzle(data);if(p.Pour(a,b)>0){string next=PuzzleSolver.StateKey(p.Export());if(next!=before)keys.Add(next);}}return keys.Count;}
        static DifficultyMetrics Measure(int[][] data,PuzzleSolution search,GeneratorRequest request){int colors=request.colors;var m=new DifficultyMetrics{bottleCount=data.Length,freeSlots=data.Length*Puzzle.Capacity-data.Sum(b=>b.Length),specialMechanics=0,optimalMoves=search.Moves.Count,visited=search.Visited,stored=search.Stored,peakQueue=search.PeakQueue,deadEndStates=search.DeadEndStates,initialChoices=ProductiveChoices(data)};foreach(var row in data)for(int i=1;i<row.Length;i++)if(row[i]!=row[i-1])m.mixedBoundaries++;var model=new Puzzle(data);foreach(var move in search.Moves){m.branchSum+=ProductiveChoices(model.Export());m.pathStates++;model.Pour(move.Source,move.Target);}int log=0;for(int n=search.Visited;n>1;n>>=1)log++;m.score=m.optimalMoves*3+m.mixedBoundaries*2+colors*2+m.branchSum/Math.Max(1,m.pathStates)+log+(request.version>=2?Math.Max(0,8-m.freeSlots):0);m.measured=colors==2&&m.optimalMoves<=3?LevelDifficulty.Tutorial:m.score<=40?LevelDifficulty.Easy:m.score<=60?LevelDifficulty.Normal:m.score<=78?LevelDifficulty.Hard:LevelDifficulty.Expert;return m;}
    }
}
