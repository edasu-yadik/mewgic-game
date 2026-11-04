namespace Mewgic.Core
{
    public readonly struct CompletionReward
    {
        public readonly int Base, Stars, FirstAttempt, NoHint;
        public int Coins => Base + Stars + FirstAttempt + NoHint;

        public CompletionReward(int previousStars, int stars, bool first, bool firstAttempt, bool hintUsed)
        {
            Base = first ? 20 : 0;
            Stars = System.Math.Max(0, stars - previousStars) * 5;
            FirstAttempt = first && firstAttempt ? 10 : 0;
            NoHint = first && !hintUsed ? 5 : 0;
        }
    }
}
