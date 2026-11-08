using System;
using System.Linq;

namespace Mewgic.Core
{
    public sealed partial class ProgressLedger
    {
        public static int RewardedVictoryTotal => EconomyConfig.Current.rewardedTotal;

        public bool CanBoostVictory(int level, string receipt) =>
            FindBoostableVictory(state, level, receipt) != null;

        public bool TryBoostVictory(int level, string receipt, out int bonus)
        {
            bonus = 0;
            if (!CanBoostVictory(level, receipt)) return false;
            var copy = state.Copy();
            var record = FindBoostableVictory(copy, level, receipt);
            int amount = RewardedVictoryTotal - record.rewardedVictoryBase;
            if (copy.presentation.coins > long.MaxValue - amount) return false;
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
                record.rewardedVictoryBase < RewardedVictoryTotal);
        }
    }
}
