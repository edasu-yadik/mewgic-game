using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public enum LevelDifficulty{Tutorial,Easy,Normal,Hard,Expert}
    [Serializable]public sealed class GeneratorRequest
    {
        public int level,version=1,capacity=4,colors=3,emptyBottles=2;public uint seed;public LevelDifficulty target=LevelDifficulty.Normal;
        public void Validate(){if(level<3||version!=1||capacity!=Puzzle.Capacity||colors<2||colors>4||emptyBottles<1||emptyBottles>2||!Enum.IsDefined(typeof(LevelDifficulty),target))throw new ArgumentException("Üretici parametreleri desteklenmiyor.");}
        public static GeneratorRequest ForLevel(int id){if(id<3)throw new ArgumentOutOfRangeException(nameof(id));var cycle=new[]{LevelDifficulty.Normal,LevelDifficulty.Normal,LevelDifficulty.Hard,LevelDifficulty.Easy,LevelDifficulty.Normal,LevelDifficulty.Hard,LevelDifficulty.Normal,LevelDifficulty.Expert,LevelDifficulty.Easy,LevelDifficulty.Normal};var d=id<8?LevelDifficulty.Easy:id<18?LevelDifficulty.Normal:cycle[(id-18)%cycle.Length];return new GeneratorRequest{level=id,seed=StablePrng.Mix((uint)id^0x4D455747u),target=d,colors=d==LevelDifficulty.Easy?3:d==LevelDifficulty.Normal&&id%3!=0?3:4};}
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
    {public DifficultyMetrics Copy()=>(DifficultyMetrics)MemberwiseClone();public int optimalMoves,mixedBoundaries,initialChoices,branchSum,pathStates,visited,stored,peakQueue,deadEndStates,score;public LevelDifficulty measured;}
    [Serializable]public sealed class GeneratedLevel
    {
        public int format=1,level,version=1,capacity=4,colors,emptyBottles,attempt;public uint seed,requestSeed;public LevelDifficulty recipeDifficulty;public string signature;public bool fallback;public LevelDifficulty target;public DifficultyMetrics difficulty;public GeneratedBottle[] bottles;public GeneratedMove[] solution;
        public GeneratedLevel Copy(){var c=(GeneratedLevel)MemberwiseClone();c.bottles=bottles.Select(b=>new GeneratedBottle{layers=(int[])b.layers.Clone()}).ToArray();c.solution=solution.Select(m=>new GeneratedMove{source=m.source,target=m.target}).ToArray();c.difficulty=difficulty.Copy();return c;}
        public int[][] Export()=>bottles.Select(b=>(int[])b.layers.Clone()).ToArray();
        public void Validate(){if(format!=1||level<3||version<1||capacity!=4||colors<2||colors>4||emptyBottles<1||emptyBottles>2||bottles==null||bottles.Length!=colors+emptyBottles||bottles.Any(b=>b?.layers==null||b.layers.Any(c=>c<0||c>=colors))||bottles.Count(b=>b.layers.Length==0)!=emptyBottles||solution==null||solution.Length<1||solution.Length>200||difficulty==null||difficulty.optimalMoves!=solution.Length)throw new ArgumentException("Üretilmiş bölüm kaydı geçersiz.");var data=Export();if(data.SelectMany(b=>b).GroupBy(c=>c).Count()!=colors||data.SelectMany(b=>b).GroupBy(c=>c).Any(g=>g.Count()!=4)||signature!=LevelSignatures.Canonical(data))throw new ArgumentException("Bölüm renkleri/imzası geçersiz.");var model=new Puzzle(data);if(model.Won)throw new ArgumentException("Başlangıç zaten çözülmüş.");foreach(var m in solution)if(m==null||model.Pour(m.source,m.target)==0)throw new ArgumentException("Bölüm çözüm sertifikası geçersiz.");if(!model.Won)throw new ArgumentException("Çözüm kazanmayla bitmiyor.");}
    }
    public static class LevelSignatures
    {
        public static string Canonical(int[][] data)
        {
            var colors=data.SelectMany(b=>b).Distinct().OrderBy(c=>c).ToArray();if(colors.Length>4)throw new ArgumentException("En fazla dört renk.");string best=null;var perm=Enumerable.Range(0,colors.Length).ToArray();Permute(data,colors,perm,0,ref best);return best??string.Join("|",data.Select(b=>""));
        }
        static void Permute(int[][] data,int[] colors,int[] map,int at,ref string best){if(at==map.Length){var rows=data.Select(b=>string.Concat(b.Select(c=>(char)('A'+map[Array.IndexOf(colors,c)])))).OrderBy(b=>b,StringComparer.Ordinal);string key=string.Join("|",rows);if(best==null||string.CompareOrdinal(key,best)<0)best=key;return;}for(int i=at;i<map.Length;i++){int t=map[i];map[i]=map[at];map[at]=t;Permute(data,colors,map,at+1,ref best);t=map[i];map[i]=map[at];map[at]=t;}}
    }
    public sealed class GenerationBudget{public int candidates=32,statesPerCandidate=6000,totalStates=30000;public void Validate(){if(candidates<1||candidates>128||statesPerCandidate<1||statesPerCandidate>20000||totalStates<1||totalStates>100000)throw new ArgumentException("Üretim bütçesi geçersiz.");}}
    public sealed class GenerationResult{public GeneratedLevel level;public int candidates,limits,duplicates,difficultyRejected,shapeRejected,states;public string failure;}
    public static class LevelGenerator
    {
        struct Reverse{public int a,b,k;}
        public static GenerationResult Generate(GeneratorRequest request,IEnumerable<string> recent=null,GenerationBudget budget=null)
        {
            if(request==null)throw new ArgumentNullException(nameof(request));request.Validate();budget=budget??new GenerationBudget();budget.Validate();var history=new HashSet<string>(recent??Array.Empty<string>(),StringComparer.Ordinal);var result=new GenerationResult();
            for(int attempt=0;attempt<budget.candidates&&result.states<budget.totalStates;attempt++){
                result.candidates++;var data=BuildCandidate(request,attempt);if(data==null){result.shapeRejected++;continue;}string sig=LevelSignatures.Canonical(data);if(history.Contains(sig)){result.duplicates++;continue;}
                var search=PuzzleSolver.Solve(data,Math.Min(budget.statesPerCandidate,budget.totalStates-result.states));result.states+=Math.Max(search.Visited,search.Stored);if(search.Status!=SearchStatus.Solved){if(search.Status==SearchStatus.LimitReached)result.limits++;else throw new InvalidOperationException("Ters hamle sertifikasıyla çelişen solver sonucu.");continue;}
                var metrics=Measure(data,search,request.colors);if(metrics.measured!=request.target){result.difficultyRejected++;continue;}
                result.level=new GeneratedLevel{level=request.level,version=request.version,seed=CandidateSeed(request.seed,attempt),requestSeed=request.seed,recipeDifficulty=request.target,colors=request.colors,emptyBottles=request.emptyBottles,target=request.target,attempt=attempt,signature=sig,difficulty=metrics,bottles=data.Select(b=>new GeneratedBottle{layers=b}).ToArray(),solution=search.Moves.Select(m=>new GeneratedMove{source=m.Source,target=m.Target}).ToArray()};result.level.Validate();return result;
            }
            result.failure="Doğrulanmış ve hedefe uygun yeni aday bütçe içinde bulunamadı.";return result;
        }
        public static GenerationResult Resolve(GeneratorRequest request,IEnumerable<string> recent,GeneratedLevel[] fallbackPool,GenerationBudget budget=null)
        {
            var history=new HashSet<string>(recent??Array.Empty<string>(),StringComparer.Ordinal);var result=Generate(request,history,budget);if(result.level!=null)return result;
            var choices=(fallbackPool??Array.Empty<GeneratedLevel>()).Where(l=>l.colors==request.colors&&l.emptyBottles==request.emptyBottles&&!history.Contains(l.signature)).OrderBy(l=>Math.Abs((int)l.difficulty.measured-(int)request.target)).ThenBy(l=>l.signature,StringComparer.Ordinal).ToArray();
            if(choices.Length==0)return result;var chosen=choices[0].Copy();chosen.Validate();chosen.level=request.level;chosen.requestSeed=request.seed;chosen.version=request.version;chosen.target=request.target;chosen.fallback=true;chosen.attempt=-1;chosen.Validate();result.level=chosen;result.failure=null;return result;
        }
        // Every selected predecessor is replayed through the actual maximal-pour rule.
        public static uint CandidateSeed(uint seed,int attempt)=>StablePrng.Mix(unchecked(seed^((uint)attempt*0x9E3779B9u)^0xA17C9E5Du));
        public static int[][] Reproduce(GeneratedLevel record){record.Validate();var request=new GeneratorRequest{level=record.level,version=record.version,capacity=record.capacity,colors=record.colors,emptyBottles=record.emptyBottles,target=record.recipeDifficulty};request.Validate();return BuildSeeded(request,record.seed);}
        static int[][] BuildCandidate(GeneratorRequest r,int attempt)=>BuildSeeded(r,CandidateSeed(r.seed,attempt));
        static int[][] BuildSeeded(GeneratorRequest r,uint seed)
        {
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
        public static bool Exact(int[][] a,int[][] b)=>a.Length==b.Length&&a.Select((r,i)=>r.SequenceEqual(b[i])).All(x=>x);
        public static int ProductiveChoices(int[][] data){var keys=new HashSet<string>();string before=PuzzleSolver.StateKey(data);for(int a=0;a<data.Length;a++)for(int b=0;b<data.Length;b++){var p=new Puzzle(data);if(p.Pour(a,b)>0){string next=PuzzleSolver.StateKey(p.Export());if(next!=before)keys.Add(next);}}return keys.Count;}
        static DifficultyMetrics Measure(int[][] data,PuzzleSolution search,int colors){var m=new DifficultyMetrics{optimalMoves=search.Moves.Count,visited=search.Visited,stored=search.Stored,peakQueue=search.PeakQueue,deadEndStates=search.DeadEndStates,initialChoices=ProductiveChoices(data)};foreach(var row in data)for(int i=1;i<row.Length;i++)if(row[i]!=row[i-1])m.mixedBoundaries++;var model=new Puzzle(data);foreach(var move in search.Moves){m.branchSum+=ProductiveChoices(model.Export());m.pathStates++;model.Pour(move.Source,move.Target);}int log=0;for(int n=search.Visited;n>1;n>>=1)log++;m.score=m.optimalMoves*3+m.mixedBoundaries*2+colors*2+m.branchSum/Math.Max(1,m.pathStates)+log;m.measured=colors==2&&m.optimalMoves<=3?LevelDifficulty.Tutorial:m.score<=40?LevelDifficulty.Easy:m.score<=60?LevelDifficulty.Normal:m.score<=78?LevelDifficulty.Hard:LevelDifficulty.Expert;return m;}
    }
}
