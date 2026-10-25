using System;using System.Linq;using Mewgic.Core;using UnityEngine;
namespace Mewgic
{
    public sealed class VerifiedGameplayLevel
    {public GameplayLevelInfo Info;public int[][] Bottles;public int OptimalMoves;}
    public static class GameplayLevelCatalog
    {
        static readonly int[] ids={1,2};
        public static int SelectedId {get;private set;}=1;
        public static bool FromMap {get;private set;}
        public static string ReturnScene=>FromMap?"LevelMap":"Home";
        public static void ResetSelection(){SelectedId=1;FromMap=false;}
        public static bool Select(int id,ProgressLedger ledger){if(ledger==null||!Mewgic.Map.MapProgressionCatalog.Open(ledger).CanEnter(id))return false;try{Load(id);}catch{return false;}SelectedId=id;FromMap=true;return true;}
        public static VerifiedGameplayLevel Load(int id)
        {
            if(!ids.Contains(id))throw new InvalidOperationException("Bölüm katalogda yok.");
            var asset=Resources.Load<TextAsset>("Levels/Level"+id.ToString("D2"));if(asset==null)throw new InvalidOperationException("Bölüm dosyası yok.");
            var data=JsonUtility.FromJson<PrototypeApp.LevelData>(asset.text);var info=JsonUtility.FromJson<GameplayLevelInfo>(asset.text);info.Validate();if(info.id!=id)throw new InvalidOperationException("Bölüm kimliği uyuşmuyor.");
            var puzzle=new Puzzle(Array.ConvertAll(data.bottles,b=>b.layers));var all=puzzle.Export().SelectMany(b=>b).ToArray();
            if(all.Length==0||all.GroupBy(c=>c).Any(g=>g.Count()!=Puzzle.Capacity))throw new InvalidOperationException("Bölüm renk miktarları geçersiz.");
            var solution=PuzzleSolver.Solve(puzzle.Export());if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0||info.threeStarMoves<solution.Moves.Count)throw new InvalidOperationException("Bölüm çözülebilirliği veya yıldız eşiği doğrulanmadı.");
            return new VerifiedGameplayLevel{Info=info,Bottles=puzzle.Export(),OptimalMoves=solution.Moves.Count};
        }
        public static VerifiedGameplayLevel Current()=>Load(SelectedId);
        public static bool CanAdvance(int current,ProgressLedger ledger)
        {if(!ledger.Completed(current))return false;int index=Array.IndexOf(ids,current);if(index<0||index+1>=ids.Length)return false;try{Load(ids[index+1]);return true;}catch{return false;}}
        public static bool Advance(int current,ProgressLedger ledger)
        {if(!CanAdvance(current,ledger))return false;SelectedId=ids[Array.IndexOf(ids,current)+1];return true;}
    }
}
