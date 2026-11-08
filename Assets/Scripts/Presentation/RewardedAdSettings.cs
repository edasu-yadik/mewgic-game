using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Mewgic
{
    [Serializable]
    public sealed class RewardedAdSettings
    {
        const string SamplePublisher = "ca-app-pub-3940256099942544";
        public bool productionEnabled;
        public string verificationBaseUrl;

        public Uri VerificationEndpoint =>
            Uri.TryCreate(verificationBaseUrl, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.Query) &&
            string.IsNullOrEmpty(uri.Fragment) && string.IsNullOrEmpty(uri.UserInfo) ? uri : null;
        public string androidAppId, iosAppId, androidRewardedUnitId, iosRewardedUnitId;

        public static RewardedAdSettings Load()
        {
            var asset = Resources.Load<TextAsset>("Ads/Rewarded");
            try { return asset == null ? new() : JsonUtility.FromJson<RewardedAdSettings>(asset.text) ?? new(); }
            catch (ArgumentException) { return new(); }
        }

        public static string SampleAppId(bool ios) =>
            SamplePublisher + (ios ? "~1458002511" : "~3347511713");

        public string UnitId(bool ios, bool development)
        {
            if (development) return SamplePublisher + (ios ? "/1712485313" : "/5224354917");
            if (!productionEnabled || VerificationEndpoint == null || !IsProductionAppId(ios ? iosAppId : androidAppId)) return null;
            string unit = ios ? iosRewardedUnitId : androidRewardedUnitId;
            return IsProductionId(unit, '/') ? unit : null;
        }

        public static bool IsProductionAppId(string id) => IsProductionId(id, '~');

        static bool IsProductionId(string id, char separator) =>
            !string.IsNullOrEmpty(id) && !id.StartsWith(SamplePublisher, StringComparison.Ordinal) &&
            Regex.IsMatch(id, @"\Aca-app-pub-[0-9]{16}" + separator + @"[0-9]{10}\z");
    }
}
