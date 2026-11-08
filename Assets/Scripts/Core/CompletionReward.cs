namespace Mewgic.Core
{
    public readonly struct CompletionReward
    {
        public readonly int Base, Stars, FirstAttempt, NoHint;
        public int Coins => Base + Stars + FirstAttempt + NoHint;

        public CompletionReward(int previousStars, int stars, bool first, bool firstAttempt, bool hintUsed)
        {
            var config = EconomyConfig.Current;
            Base = first ? config.levelBase : 0;
            Stars = System.Math.Max(0, stars - previousStars) * config.perNewStar;
            FirstAttempt = first && firstAttempt ? config.firstAttempt : 0;
            NoHint = first && !hintUsed ? config.withoutHint : 0;
        }
    }
}
