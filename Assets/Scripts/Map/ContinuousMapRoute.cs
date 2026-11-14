using UnityEngine;

namespace Mewgic.Map
{
    // Version 1 pins the approved painted route and the deterministic chunk deformation.
    internal static class ContinuousMapRoute
    {
        internal const int Version = 1;
        internal const int LevelsPerChunk = 10;
        internal const float PlateTop = .181f, PlateBottom = .890f;
        internal const float Entrance = .239f, Exit = .857f;

        static readonly Vector2[][] ReferenceWalk =
        {
            new[] { new Vector2(.26f,.239f), new Vector2(.43f,.242f), new Vector2(.61f,.263f) },
            new[] { new Vector2(.61f,.263f), new Vector2(.73f,.287f), new Vector2(.73f,.319f), new Vector2(.62f,.351f), new Vector2(.35f,.365f) },
            new[] { new Vector2(.35f,.365f), new Vector2(.47f,.354f), new Vector2(.64f,.350f) },
            new[] { new Vector2(.64f,.350f), new Vector2(.40f,.357f), new Vector2(.26f,.380f), new Vector2(.234f,.413f), new Vector2(.257f,.445f) },
            new[] { new Vector2(.257f,.445f), new Vector2(.44f,.466f), new Vector2(.60f,.485f), new Vector2(.71f,.507f) },
            new[] { new Vector2(.71f,.507f), new Vector2(.78f,.534f), new Vector2(.72f,.558f), new Vector2(.54f,.581f), new Vector2(.25f,.609f) },
            new[] { new Vector2(.25f,.609f), new Vector2(.38f,.596f), new Vector2(.55f,.584f), new Vector2(.67f,.573f) },
            new[] { new Vector2(.67f,.573f), new Vector2(.49f,.584f), new Vector2(.26f,.605f), new Vector2(.238f,.642f), new Vector2(.34f,.687f) },
            new[] { new Vector2(.34f,.687f), new Vector2(.48f,.706f), new Vector2(.62f,.723f), new Vector2(.705f,.758f) },
            new[] { new Vector2(.705f,.758f), new Vector2(.62f,.788f), new Vector2(.47f,.810f), new Vector2(.31f,.838f), new Vector2(.206f,.857f) },
        };

        internal static float Band(float y) => (y - PlateTop) / (PlateBottom - PlateTop);

        internal static float Warp(int chunk, float y)
        {
            // Preserve the approved first ten terraces. Beyond them, match both seams exactly.
            if (chunk < 10 || y <= Entrance || y >= Exit) return 0;
            uint hash = unchecked((uint)chunk * 2654435761u ^ (uint)Version * 2246822519u);
            hash ^= hash >> 16;
            float envelope = Mathf.Sin(Mathf.PI * Mathf.InverseLerp(Entrance, Exit, y));
            return ((hash & 255) / 255f - .5f) * .064f * envelope * envelope;
        }

        internal static Vector2 Node(int slot) => ReferenceWalk[slot][0];

        internal static Vector2 Road(int slot, float phase)
        {
            var route = ReferenceWalk[slot];
            float f = Mathf.Clamp01(phase) * (route.Length - 1);
            int i = Mathf.Min(Mathf.FloorToInt(f), route.Length - 2);
            var a = route[i];
            var b = route[i + 1];
            float t = f - i;
            var before = i > 0 ? route[i - 1] :
                slot > 0 ? ReferenceWalk[slot - 1][^2] : a - new Vector2(.10f, .10f);
            var after = i < route.Length - 2 ? route[i + 2] :
                slot < LevelsPerChunk - 1 ? ReferenceWalk[slot + 1][1] : b + new Vector2(-.03f, .10f);
            return Hermite(a, b, (b - before) * .5f, (after - a) * .5f, t);
        }

        internal static Vector2 Join(float t)
        {
            var start = new Vector2(.206f, Band(Exit));
            var end = new Vector2(.26f, 1 + Band(Entrance));
            float length = end.y - start.y;
            // Match the painted road's slopes at the seams in normalized chunk space.
            var exitDelta = new Vector2(-.134f, .119f);
            var entryDelta = new Vector2(.27f, .103f);
            return Hermite(start, end, new Vector2(length * exitDelta.x * (PlateBottom - PlateTop) / exitDelta.y, length),
                new Vector2(length * entryDelta.x * (PlateBottom - PlateTop) / entryDelta.y, length), t);
        }

        static Vector2 Hermite(Vector2 a, Vector2 b, Vector2 entry, Vector2 exit, float t)
        {
            var chord = b - a;
            return Vector2.Lerp(a, b, t) + ((entry - chord) * (1 - t) - (exit - chord) * t) * (t * (1 - t));
        }
    }
}
