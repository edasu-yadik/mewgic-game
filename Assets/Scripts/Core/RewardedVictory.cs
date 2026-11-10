using System.Linq;

namespace Mewgic.Core
{
    public sealed partial class ProgressLedger
    {
        public const int VictoryAdMultiplier = 5;

        public bool CanBoostVictory(int level, string receipt) => TryGetVictoryAdTotal(level, receipt, out _);

        public bool TryGetVictoryAdTotal(int level, string receipt, out int total)
        {
            var record = FindBoostableVictory(state, level, receipt);
            total = record == null ? 0 : record.rewardedVictoryBase * VictoryAdMultiplier;
            return record != null && state.presentation.coins <= long.MaxValue - (total - record.rewardedVictoryBase);
        }

        public bool TryBoostVictory(int level, string receipt, out int bonus)
        {
            bonus = 0;
            if (!TryGetVictoryAdTotal(level, receipt, out int total)) return false;
            var copy = state.Copy();
            var record = FindBoostableVictory(copy, level, receipt);
            int amount = total - record.rewardedVictoryBase;
            copy.presentation.coins += amount;
            record.rewardedVictoryReceipt = null;
            if (!Commit(copy)) return false;
            bonus = amount;
            return true;
        }

        static LevelRecord FindBoostableVictory(ProgressData data, int level, string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return null;
            return data.levels.FirstOrDefault(record => record.level == level && record.rewardClaimed &&
                record.rewardedVictoryReceipt == receipt && record.rewardedVictoryBase > 0 &&
                record.rewardedVictoryBase <= int.MaxValue / VictoryAdMultiplier);
        }
    }
}
