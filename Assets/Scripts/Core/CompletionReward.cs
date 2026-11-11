namespace Mewgic.Core
{
    public readonly struct CompletionReward
    {
        public readonly int Base, Stars, FirstAttempt, NoHint, Gems;
        public int Coins => Base + Stars + FirstAttempt + NoHint;

        public CompletionReward(int previousStars, int stars, bool first, bool firstAttempt, bool hintUsed, int level = 0)
        {
            var config = EconomyConfig.Current;
            Gems = first ? config.FirstClearGems(level) : 0;
            Base = first ? config.levelBase : 0;
            Stars = System.Math.Max(0, stars - previousStars) * config.perNewStar;
            FirstAttempt = first && firstAttempt ? config.firstAttempt : 0;
            NoHint = first && !hintUsed ? config.withoutHint : 0;
        }
    }
}
