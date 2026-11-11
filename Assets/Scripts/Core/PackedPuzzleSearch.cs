using System;
using System.Threading;
using System.Collections.Generic;

namespace Mewgic.Core
{
    // Four nibbles per bottle, zero for an empty layer and 1–15 for a color.
    internal static class PackedPuzzleSearch
    {
        readonly struct Board : IEquatable<Board>
        {
            readonly ulong left, right;
            Board(ulong left, ulong right) { this.left = left; this.right = right; }
            internal ushort At(int index) => (ushort)((index < 4 ? left : right) >> ((index & 3) * 16));
            internal Board With(int index, int value)
            {
                int shift = (index & 3) * 16;
                ulong mask = 65535UL << shift, bits = (ulong)value << shift;
                return index < 4 ? new Board((left & ~mask) | bits, right) : new Board(left, (right & ~mask) | bits);
            }
            internal Board Canonical(int count)
            {
                Span<ushort> sorted = stackalloc ushort[8];
                for (int i = 0; i < count; i++)
                {
                    ushort value = At(i);
                    int j = i;
                    while (j > 0 && sorted[j - 1] > value) { sorted[j] = sorted[j - 1]; j--; }
                    sorted[j] = value;
                }
                var key = new Board();
                for (int i = 0; i < count; i++) key = key.With(i, sorted[i]);
                return key;
            }
            public bool Equals(Board other) => left == other.left && right == other.right;
            public override bool Equals(object value) => value is Board other && Equals(other);
            public override int GetHashCode() => unchecked(left.GetHashCode() * 397 ^ right.GetHashCode());
        }

        struct Node
        {
            internal Board board;
            internal int parent;
            internal PuzzleMove move;
        }

        internal static bool TrySolve(int[][] data, int limit, out PuzzleSolution result, CancellationToken cancellation=default)
        {
            result = null;
            int count = data.Length;
            if (count > 8) return false;
            var board = new Board();
            for (int i = 0; i < count; i++)
            {
                int bottle = 0;
                for (int j = 0; j < data[i].Length; j++)
                {
                    if (data[i][j] >= 15) return false;
                    bottle |= (data[i][j] + 1) << (4 * j);
                }
                board = board.With(i, bottle);
            }

            result = new PuzzleSolution();
            var nodes = new List<Node>(Math.Min(limit, 128)) { new Node { board = board, parent = -1 } };
            var seen = new HashSet<Board> { board.Canonical(count) };
            Span<ushort> bottles = stackalloc ushort[8];
            Span<int> lengths = stackalloc int[8];
            for (int head = 0; head < nodes.Count; head++)
            {
                cancellation.ThrowIfCancellationRequested();
                result.Stored = nodes.Count;
                result.PeakQueue = Math.Max(result.PeakQueue, nodes.Count - head);
                result.Visited++;
                board = nodes[head].board;
                bool won = true;
                for (int i = 0; i < count; i++)
                {
                    ushort b = board.At(i);
                    bottles[i] = b;
                    lengths[i] = b >= 4096 ? 4 : b >= 256 ? 3 : b >= 16 ? 2 : b > 0 ? 1 : 0;
                    if (b != 0 && (lengths[i] != 4 || b != (b & 15) * 0x1111)) won = false;
                }
                if (won)
                {
                    result.Status = SearchStatus.Solved;
                    for (int i = head; nodes[i].parent >= 0; i = nodes[i].parent) result.Moves.Add(nodes[i].move);
                    result.Moves.Reverse();
                    return true;
                }

                var before = board.Canonical(count);
                bool productive = false;
                for (int source = 0; source < count; source++)
                {
                    int length = lengths[source];
                    if (length == 0) continue;
                    int color = bottles[source] >> (4 * (length - 1)) & 15;
                    int run = 1;
                    while (run < length && (bottles[source] >> (4 * (length - run - 1)) & 15) == color) run++;
                    for (int target = 0; target < count; target++)
                    {
                        int space = Puzzle.Capacity - lengths[target];
                        if (source == target || space == 0 ||
                            (lengths[target] > 0 && (bottles[target] >> (4 * (lengths[target] - 1)) & 15) != color)) continue;
                        int amount = Math.Min(run, space);
                        int remainingBits = (length - amount) * 4;
                        int left = bottles[source] & ((1 << remainingBits) - 1);
                        int right = bottles[target] | ((bottles[source] >> remainingBits) << (lengths[target] * 4));
                        var next = board.With(source, left).With(target, right);
                        var key = next.Canonical(count);
                        if (!key.Equals(before)) productive = true;
                        if (seen.Contains(key)) continue;
                        if (nodes.Count >= limit)
                        {
                            result.Stored = nodes.Count;
                            result.Status = SearchStatus.LimitReached;
                            return true;
                        }
                        seen.Add(key);
                        nodes.Add(new Node { board = next, parent = head, move = new PuzzleMove(source, target) });
                    }
                }
                if (!productive) result.DeadEndStates++;
            }
            result.Status = SearchStatus.Unsolvable;
            return true;
        }
    }
}
