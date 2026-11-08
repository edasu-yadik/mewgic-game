using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mewgic
{
    sealed class RewardedAdBuildCheck : IPreprocessBuildWithReport
    {
        const string NativeSettings =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var target = report.summary.platform;
            if (target != BuildTarget.Android && target != BuildTarget.iOS) return;
            bool ios = target == BuildTarget.iOS;
            bool development = (report.summary.options & BuildOptions.Development) != 0;
            var config = RewardedAdSettings.Load();
            string expected = RewardedAdSettings.SampleAppId(ios);
            if (!development && config.productionEnabled)
            {
                if (config.UnitId(ios, false) == null)
                    throw new BuildFailedException("AdMob: " + target + " üretim uygulama ve ödüllü reklam birimi kimliklerini ve HTTPS doğrulama adresini doldurun.");
                expected = ios ? config.iosAppId : config.androidAppId;
            }

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(NativeSettings);
            string field = ios ? "adMobIOSAppId" : "adMobAndroidAppId";
            if (asset == null || new SerializedObject(asset).FindProperty(field)?.stringValue != expected)
                throw new BuildFailedException("AdMob: " + target + " için Assets > Google Mobile Ads > Settings uygulama kimliği bu build profiliyle eşleşmeli. Beklenen: " + expected);
        }
    }
}
