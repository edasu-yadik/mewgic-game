using System.Threading.Tasks;
using UnityEngine;

namespace Mewgic
{
    public enum RewardedVictoryAdResult { Unavailable, Closed, Rewarded }

    public interface IRewardedVictoryAd
    {
        // Complete the task on every SDK exit path. Return Rewarded only from its earned-reward callback.
        Task<RewardedVictoryAdResult> ShowAsync();
    }

    public static class RewardedVictoryAds
    {
        public static IRewardedVictoryAd Provider { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Provider = null;
    }
}
