using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.Networking;

namespace Mewgic
{
    sealed class AdMobRewardedVictoryAd : MonoBehaviour, IRewardedVictoryAd
    {
        const double RetrySeconds = 30, MaxAdSeconds = 180;
        SynchronizationContext unityThread;
        string unitId, receipt;
        Uri verificationEndpoint;
        RewardedAd loaded, playing;
        TaskCompletionSource<RewardedVictoryAdResult> attempt;
        bool consentRequested, privacyOpen, initializing, initialized, loading, earned;
        double retryAt, expiresAt, showDeadline;

        static bool Offline => Application.internetReachability == NetworkReachability.NotReachable;
        static double Now => Time.realtimeSinceStartupAsDouble;

        public bool IsPresenting => attempt != null;

        public RewardedVictoryAdState State => Offline ? RewardedVictoryAdState.Offline :
            privacyOpen || !ConsentInformation.CanRequestAds() ? RewardedVictoryAdState.Unavailable :
            loaded?.CanShowAd() == true && Now < expiresAt ? RewardedVictoryAdState.Ready :
            initializing || loading ? RewardedVictoryAdState.Loading : RewardedVictoryAdState.Unavailable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            var config = RewardedAdSettings.Load();
            string id = config.UnitId(Application.platform == RuntimePlatform.IPhonePlayer, Debug.isDebugBuild);
            if (id == null) return;
            var host = new GameObject("AdMob Rewarded Ads");
            DontDestroyOnLoad(host);
            var provider = host.AddComponent<AdMobRewardedVictoryAd>();
            provider.unitId = id;
            provider.verificationEndpoint = Debug.isDebugBuild ? null : config.VerificationEndpoint;
            RewardedVictoryAds.Provider = provider;
            RewardedVictoryAds.NeedsPrivacyOptions = () =>
                ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
            RewardedVictoryAds.OpenPrivacyOptions = provider.ShowPrivacyOptions;
#endif
        }

        void Awake() => unityThread = SynchronizationContext.Current;

        void Update()
        {
            if (attempt != null)
            {
                if (Now >= showDeadline)
                    Finish(earned ? RewardedVictoryAdResult.Rewarded : FailureResult);
                return;
            }
            if (Offline || privacyOpen || Now < retryAt) return;
            if (!consentRequested)
            {
                RequestConsent();
                return;
            }
            if (ConsentInformation.CanRequestAds()) PrepareAd();
        }

        void RequestConsent()
        {
            consentRequested = true;
            ConsentInformation.Update(new ConsentRequestParameters(), error => OnMainThread(() =>
            {
                if (error != null)
                {
                    consentRequested = ConsentInformation.CanRequestAds();
                    retryAt = Now + RetrySeconds;
                    return;
                }
                privacyOpen = true;
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => OnMainThread(() =>
                {
                    privacyOpen = false;
                    if (formError != null)
                    {
                        consentRequested = ConsentInformation.CanRequestAds();
                        retryAt = Now + RetrySeconds;
                    }
                }));
            }));
        }

        void ShowPrivacyOptions()
        {
            if (privacyOpen || playing != null) return;
            privacyOpen = true;
            loaded?.Destroy();
            loaded = null;
            ConsentForm.ShowPrivacyOptionsForm(_ => OnMainThread(() => privacyOpen = false));
        }

        void PrepareAd()
        {
            if (!initialized)
            {
                if (initializing) return;
                initializing = true;
                MobileAds.Initialize(status => OnMainThread(() =>
                {
                    initializing = false;
                    initialized = status != null;
                    if (!initialized) retryAt = Now + RetrySeconds;
                }));
                return;
            }

            if (loading || loaded?.CanShowAd() == true && Now < expiresAt) return;
            loaded?.Destroy();
            loaded = null;
            loading = true;
            RewardedAd.Load(unitId, new AdRequest(), (ad, error) => OnMainThread(() =>
            {
                loading = false;
                if (error != null || ad == null || privacyOpen || !ConsentInformation.CanRequestAds())
                {
                    ad?.Destroy();
                    retryAt = Now + RetrySeconds;
                    return;
                }
                loaded = ad;
                expiresAt = Now + 50 * 60;
            }));
        }

        public Task<RewardedVictoryAdResult> ShowAsync(string victoryReceipt)
        {
            if (attempt != null || State != RewardedVictoryAdState.Ready)
                return Task.FromResult(FailureResult);

            var ad = playing = loaded;
            loaded = null;
            earned = false;
            receipt = victoryReceipt;
            showDeadline = Now + MaxAdSeconds;
            var completion = attempt = new TaskCompletionSource<RewardedVictoryAdResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                ad.SetServerSideVerificationOptions(new ServerSideVerificationOptions { CustomData = victoryReceipt });
                ad.OnAdFullScreenContentClosed += () => OnMainThread(() =>
                {
                    if (playing == ad) Finish(earned ? RewardedVictoryAdResult.Rewarded : RewardedVictoryAdResult.Closed);
                });
                ad.OnAdFullScreenContentFailed += _ => OnMainThread(() =>
                {
                    if (playing == ad) Finish(earned ? RewardedVictoryAdResult.Rewarded : FailureResult);
                });
                ad.Show(reward =>
                {
                    if (reward != null) OnMainThread(() => { if (playing == ad) earned = true; });
                });
            }
            catch (Exception)
            {
                Finish(FailureResult);
            }
            return completion.Task;
        }

        static RewardedVictoryAdResult FailureResult =>
            Offline ? RewardedVictoryAdResult.Offline : RewardedVictoryAdResult.Unavailable;

        void Finish(RewardedVictoryAdResult result)
        {
            var ad = playing;
            var completion = attempt;
            playing = null;
            attempt = null;
            ad?.Destroy();
            if (completion == null) return;
            if (result == RewardedVictoryAdResult.Rewarded && verificationEndpoint != null)
                StartCoroutine(VerifyReward(receipt, completion));
            else completion.TrySetResult(result);
        }

        IEnumerator VerifyReward(string token, TaskCompletionSource<RewardedVictoryAdResult> completion)
        {
            string url = verificationEndpoint.AbsoluteUri.TrimEnd('/') + "/rewarded/" + token;
            double deadline = Now + 20;
            while (!Offline && Now < deadline)
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = 4;
                    request.redirectLimit = 0;
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == token)
                    {
                        completion.TrySetResult(RewardedVictoryAdResult.Rewarded);
                        yield break;
                    }
                }
                yield return new WaitForSecondsRealtime(2);
            }
            completion.TrySetResult(Offline ? RewardedVictoryAdResult.Offline : RewardedVictoryAdResult.Unverified);
        }

        void OnMainThread(Action action) => unityThread.Post(_ => { if (this != null) action(); }, null);

        void OnDestroy()
        {
            Finish(FailureResult);
            loaded?.Destroy();
        }
    }
}
