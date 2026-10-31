using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public readonly struct PuzzleMove
    {public readonly int Source,Target;public PuzzleMove(int source,int target){Source=source;Target=target;}}
    public enum SearchStatus {Solved,Unsolvable,LimitReached}
    public sealed class PuzzleSolution
    {public SearchStatus Status;public List<PuzzleMove> Moves=new List<PuzzleMove>();public int Visited;public int Stored,PeakQueue,DeadEndStates;}
    public static class PuzzleSolver
    {
        sealed class Node{public int[][] Data;public Node Previous;public PuzzleMove Move;}
        public static string StateKey(int[][] data)=>string.Join("|",data.Select(b=>string.Join(",",b)).OrderBy(b=>b,StringComparer.Ordinal));
        public static PuzzleSolution Solve(int[][] data,int limit=10000)
        {
            if(limit<1)throw new ArgumentOutOfRangeException(nameof(limit));
            var result=new PuzzleSolution();var first=new Node{Data=new Puzzle(data).Export()};
            var queue=new Queue<Node>();queue.Enqueue(first);var seen=new HashSet<string>{StateKey(first.Data)};
            while(queue.Count>0)
            {
                result.Stored=seen.Count;result.PeakQueue=Math.Max(result.PeakQueue,queue.Count);var node=queue.Dequeue();var current=new Puzzle(node.Data);result.Visited++;
                if(current.Won){result.Status=SearchStatus.Solved;for(var n=node;n.Previous!=null;n=n.Previous)result.Moves.Add(n.Move);result.Moves.Reverse();return result;}
                int productive=0;string currentKey=StateKey(node.Data);
                for(int s=0;s<current.Count;s++)for(int t=0;t<current.Count;t++)
                {
                    if(s==t||node.Data[s].Length==0||node.Data[t].Length==Puzzle.Capacity)continue;
                    var next=new Puzzle(node.Data);if(next.Pour(s,t)==0)continue;var snapshot=next.Export();var key=StateKey(snapshot);if(key!=currentKey)productive++;if(seen.Contains(key))continue;
                    if(seen.Count>=limit){result.Stored=seen.Count;result.Status=SearchStatus.LimitReached;return result;}
                    seen.Add(key);queue.Enqueue(new Node{Data=snapshot,Previous=node,Move=new PuzzleMove(s,t)});
                }
                if(productive==0)result.DeadEndStates++;
            }
            result.Status=SearchStatus.Unsolvable;return result;
        }
    }
}
