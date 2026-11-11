using System;
using System.Collections.Generic;
using System.Linq;

namespace Mewgic.Core
{
    // Layers are stored bottom to top. This model has no Unity dependency.
    public sealed class Puzzle
    {
        public const int Capacity = 4;
        readonly int[][] initial;
        List<int>[] bottles;
        readonly Stack<int[][]> history = new Stack<int[][]>();
        public int Moves { get; private set; }
        public int Count => bottles.Length;
        public bool ExtraBottleUsed { get; private set; }
        public bool BottleHammerUsed { get; private set; }
        public bool ColorSwapUsed { get; private set; }
        public bool CanUndo => history.Count > 0;
        public IReadOnlyList<int> Bottle(int index) => bottles[index].AsReadOnly();
        public bool Won => bottles.All(b => b.Count == 0 || (b.Count == Capacity && b.All(c => c == b[0])));
        public Puzzle(int[][] data)
        {
            if (data == null || data.Length < 2 || data.Any(b => b == null || b.Length > Capacity || b.Any(c => c < 0)))
                throw new ArgumentException("Invalid level");
            initial = data.Select(b => b.ToArray()).ToArray();
            Restart();
        }
        int[][] Snapshot() => bottles.Select(b => b.ToArray()).ToArray();
        void Restore(int[][] state) => bottles = state.Select(b => new List<int>(b)).ToArray();
        public int Pour(int source, int target)
        {
            if (source < 0 || target < 0 || source >= Count || target >= Count || source == target) return 0;
            var a = bottles[source]; var b = bottles[target];
            if (a.Count == 0 || b.Count == Capacity) return 0;
            int color = a[a.Count - 1];
            if (b.Count > 0 && b[b.Count - 1] != color) return 0;
            int run = 0;
            for (int i = a.Count - 1; i >= 0 && a[i] == color; i--) run++;
            int amount = Math.Min(run, Capacity - b.Count);
            history.Push(Snapshot());
            a.RemoveRange(a.Count - amount, amount);
            for (int i = 0; i < amount; i++) b.Add(color);
            Moves++;
            return amount;
        }
        // Boosters do not count as pours. Existing pour history is mapped into
        // the new topology so Undo still means the last successful pour.
        public bool AddEmptyBottle(int maximum)
        {
            if(ExtraBottleUsed||Count>=maximum)return false;
            bottles=bottles.Concat(new[]{new List<int>()}).ToArray();
            MapHistory(state=>state.Concat(new[]{new int[0]}).ToArray());ExtraBottleUsed=true;return true;
        }
        public bool RotateBottles()
        {
            var before=Snapshot();var after=Rotate(before);
            if(before.Select((b,i)=>b.SequenceEqual(after[i])).All(equal=>equal))return false;
            Restore(after);MapHistory(Rotate);return true;
        }
        static int[][] Rotate(int[][] state)=>new[]{state[state.Length-1].ToArray()}.Concat(state.Take(state.Length-1).Select(b=>b.ToArray())).ToArray();
        void MapHistory(Func<int[][],int[][]> map)
        {
            var states=history.ToArray();history.Clear();
            for(int i=states.Length-1;i>=0;i--)history.Push(map(states[i]));
        }
        internal void ApplyColorSwap(int source,int target)
        {
            var first=bottles[source];
            var second=bottles[target];
            (first[first.Count-1],second[second.Count-1])=(second[second.Count-1],first[first.Count-1]);
            // Earlier pours describe a different arrangement of layers.
            history.Clear();
            ColorSwapUsed=true;
        }

        internal void ApplyBottleHammer(int[][] layout)
        {
            Restore(layout);
            history.Clear();
            BottleHammerUsed = true;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            Restore(history.Pop()); Moves--; return true;
        }
        public void Restart() { Restore(initial); history.Clear(); Moves = 0; ColorSwapUsed = BottleHammerUsed = ExtraBottleUsed = false; }
        public int[][] Export() => Snapshot();
    }
}
