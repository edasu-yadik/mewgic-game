using System;
using Mewgic.Core;
namespace Mewgic
{
    [Serializable] public sealed class GameplayLevelInfo
    {
        public int id;
        public int threeStarMoves;
        public int twoStarMoves;
        public string goal;
        public void Validate()
        {
            if(id<1 || threeStarMoves<1 || twoStarMoves<threeStarMoves || string.IsNullOrWhiteSpace(goal))
                throw new InvalidOperationException("Bölüm HUD verisi eksik veya geçersiz.");
        }
    }
    // A live target is not an awarded result; awards only exist once the puzzle is won.
    public readonly struct GameplayHudState
    {
        public readonly int Level,Moves,Completed,Total,PotentialStars,AwardedStars;
        public readonly string Goal;
        public GameplayHudState(GameplayLevelInfo info,Puzzle puzzle)
        {
            Level=info.id;Moves=puzzle.Moves;Goal=info.goal;
            var colors=new System.Collections.Generic.HashSet<int>();int complete=0;
            for(int i=0;i<puzzle.Count;i++)
            {
                var b=puzzle.Bottle(i);foreach(int c in b)colors.Add(c);
                bool single=b.Count==Puzzle.Capacity;
                for(int n=1;n<b.Count;n++)single&=b[n]==b[0];
                if(single)complete++;
            }
            Completed=complete;Total=colors.Count;
            PotentialStars=Moves<=info.threeStarMoves?3:Moves<=info.twoStarMoves?2:1;
            AwardedStars=puzzle.Won?PotentialStars:0;
        }
    }
}
