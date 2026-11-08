using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewgic
{
    public enum RewardedVictoryAdState { Unavailable, Loading, Offline, Ready }
    public enum RewardedVictoryAdResult { Unavailable, Offline, Closed, Unverified, Rewarded }

    public interface IRewardedVictoryAd
    {
        RewardedVictoryAdState State { get; }
        bool IsPresenting { get; }
        Task<RewardedVictoryAdResult> ShowAsync(string victoryReceipt);
    }

    public static class RewardedVictoryAds
    {
        public static IRewardedVictoryAd Provider { get; set; }
        public static Func<bool> NeedsPrivacyOptions { get; set; }
        public static Action OpenPrivacyOptions { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Provider = null;
            NeedsPrivacyOptions = null;
            OpenPrivacyOptions = null;
        }
    }
}
