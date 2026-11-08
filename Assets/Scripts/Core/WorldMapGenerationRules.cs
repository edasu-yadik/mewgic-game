namespace Mewgic.Core
{
    public static partial class WorldMapRules
    {
        public const int LevelsPerIsland = 50;

        // Existing archive pins are checked before calling this for a newly generated level.
        public static GeneratorRequest Request(int level)
        {
            var request = GeneratorRequest.ForLevel(level);
            int offset = (level - 1) % LevelsPerIsland;
            if (level > LevelsPerIsland && offset < 3)
            {
                request.target = offset == 0 ? LevelDifficulty.Easy : LevelDifficulty.Normal;
                request.colors = 3;
            }
            return request;
        }
    }
}
