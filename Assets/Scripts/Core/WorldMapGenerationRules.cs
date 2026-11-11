namespace Mewgic.Core
{
    public static partial class WorldMapRules
    {
        public const int LevelsPerIsland = 50;
        public const int MaxLevel = int.MaxValue / LevelsPerIsland * LevelsPerIsland;
        public const int FrontierFirstLevel = 10_001;
        public const int LastIsland = MaxLevel / LevelsPerIsland - 1;

        // Existing archive pins are checked before calling this for a newly generated level.
        public static GeneratorRequest Request(int level)
        {
            if (level > MaxLevel) throw new System.ArgumentOutOfRangeException(nameof(level));
            var request = GeneratorRequest.ForLevel(level);
            if (level >= FrontierFirstLevel)
            {
                request.version = GeneratorRequest.FrontierVersion;
                int beat = (level - 1) % 10;
                request.target = beat switch
                {
                    0 => LevelDifficulty.Easy,
                    1 or 2 or 5 => LevelDifficulty.Normal,
                    4 or 9 => LevelDifficulty.Expert,
                    _ => LevelDifficulty.Hard
                };
                request.colors = beat <= 2 ? 3 : 4;
            }
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
