using System;using System.Linq;using System.Collections.Generic;using System.Threading.Tasks;using System.IO;using Mewgic.Core;using UnityEngine;
namespace Mewgic
{
    public sealed class VerifiedGameplayLevel
    {public GameplayLevelInfo Info;public int[][] Bottles;public int OptimalMoves;public GeneratedLevel Generated;}
    public static class GameplayLevelCatalog
    {
        static readonly int[] ids={1,2};
#if UNITY_EDITOR
        public static ILevelArchive TestArchive;
#endif
        static ProgressLedger owner;static ILevelArchive archive;static readonly object worker=new object();static readonly Dictionary<int,Task<VerifiedGameplayLevel>> pending=new Dictionary<int,Task<VerifiedGameplayLevel>>();static GeneratedLevel[] fallbacks;
        static ILevelArchive Archive {get{var ledger=GameplayProgress.Current;if(!ReferenceEquals(owner,ledger)){owner=ledger;archive=CreateArchive();pending.Clear();}return archive;}}
        static ILevelArchive CreateArchive()
        {
#if UNITY_EDITOR
            if(GameplayProgress.IsTestProfile)return TestArchive??new MemoryLevelArchive();
#endif
            return new FileLevelArchive(Path.Combine(Application.persistentDataPath,"GeneratedLevels"));
        }
        static VerifiedGameplayLevel Wrap(GeneratedLevel level){level.Validate();int moves=level.difficulty.optimalMoves;return new VerifiedGameplayLevel{Info=new GameplayLevelInfo{id=level.level,threeStarMoves=moves,twoStarMoves=moves+Math.Max(2,moves/2),goal="Renkleri ayır"},Bottles=level.Export(),OptimalMoves=moves,Generated=level};}
        public static Task<VerifiedGameplayLevel> PrepareAsync(int id)
        {
            if(id<1)throw new ArgumentOutOfRangeException(nameof(id));if(id<3)return Task.FromResult(Load(id));var store=Archive;var asset=Resources.Load<TextAsset>("Levels/GeneratedFallbacksV2");string poolJson=asset!=null?asset.text:"[]";if(pending.TryGetValue(id,out var task)&&!task.IsFaulted&&!task.IsCanceled)return task;
            foreach(var key in pending.Where(p=>p.Value.IsCompleted).Select(p=>p.Key).ToArray())pending.Remove(key);
            if(pending.Count>=2)return Task.FromException<VerifiedGameplayLevel>(new InvalidOperationException("Hazırlık sürüyor; tekrar deneyebilirsin."));
            task=Task.Run(()=>{lock(worker){if(fallbacks==null){fallbacks=Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(poolJson);foreach(var l in fallbacks)l.Validate();}if(store.TryRead(id,out var saved))return Wrap(saved);var request=WorldMapRules.Request(id);var result=LevelGenerator.Resolve(request,null,fallbacks);if(result.level==null)throw new InvalidOperationException("Doğrulanmış yeni bölüm hazırlanamadı; tekrar deneyebilirsin.");store.Write(result.level);return Wrap(result.level);}});pending[id]=task;return task;
        }
        public static void Prewarm(int current){if(current>=2&&current<int.MaxValue){var unused=PrepareAsync(current+1);}}
        public static bool CanEnter(int id,ProgressLedger ledger)=>ledger!=null&&(id==1||id>1&&ledger.Completed(id-1));
        public static int SelectedId {get;private set;}=1;
        public static bool FromMap {get;private set;}
        public static string ReturnScene=>FromMap?"LevelMap":"Home";
        public static void ResetSelection(){SelectedId=1;FromMap=false;}
        public static bool Select(int id,ProgressLedger ledger){if(ledger==null||!CanEnter(id,ledger))return false;try{Load(id);}catch{return false;}if(!ledger.SetCurrentLevel(id))return false;SelectedId=id;FromMap=true;return true;}
        public static VerifiedGameplayLevel Load(int id)
        {
            if(id>=3){if(Archive.TryRead(id,out var saved))return Wrap(saved);throw new InvalidOperationException("Bölüm henüz hazırlanmadı.");}
            if(!ids.Contains(id))throw new InvalidOperationException("Bölüm katalogda yok.");
            var asset=Resources.Load<TextAsset>("Levels/Level"+id.ToString("D2"));if(asset==null)throw new InvalidOperationException("Bölüm dosyası yok.");
            var data=JsonUtility.FromJson<PrototypeApp.LevelData>(asset.text);var info=JsonUtility.FromJson<GameplayLevelInfo>(asset.text);info.Validate();if(info.id!=id)throw new InvalidOperationException("Bölüm kimliği uyuşmuyor.");
            var puzzle=new Puzzle(Array.ConvertAll(data.bottles,b=>b.layers));var all=puzzle.Export().SelectMany(b=>b).ToArray();
            if(all.Length==0||all.GroupBy(c=>c).Any(g=>g.Count()!=Puzzle.Capacity))throw new InvalidOperationException("Bölüm renk miktarları geçersiz.");
            var solution=PuzzleSolver.Solve(puzzle.Export());if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0||info.threeStarMoves<solution.Moves.Count)throw new InvalidOperationException("Bölüm çözülebilirliği veya yıldız eşiği doğrulanmadı.");
            return new VerifiedGameplayLevel{Info=info,Bottles=puzzle.Export(),OptimalMoves=solution.Moves.Count};
        }
        public static void ResumeFromSave()
        {
            var ledger=GameplayProgress.Current;int saved=ledger.Presentation.currentLevel;
            SelectedId=CanEnter(saved,ledger)?saved:1;FromMap=false;
        }
        public static VerifiedGameplayLevel Current()=>Load(SelectedId);
        public static bool CanAdvance(int current,ProgressLedger ledger)=>current>=1&&current<int.MaxValue&&ledger.Completed(current);
        public static bool Advance(int current,ProgressLedger ledger)
        {if(!CanAdvance(current,ledger))return false;int next=current+1;try{Load(next);}catch{return false;}if(!ledger.SetCurrentLevel(next))return false;SelectedId=next;return true;}
    }
}
