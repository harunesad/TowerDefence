using UnityEngine;
using UnityEngine.Advertisements;
using System;

namespace TowerDefence.Core
{
    public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public static AdManager Instance { get; private set; }

        [Header("Ad IDs")]
        [SerializeField] private string androidGameId = "6176639";
        [SerializeField] private string iosGameId = "6176638";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool testMode = true;
#else
        [SerializeField] private bool testMode = false;
#endif

        [Header("Ad Units")]
        [SerializeField] private string androidRewardedId = "Rewarded_Android";
        [SerializeField] private string iosRewardedId = "Rewarded_iOS";
        [SerializeField] private string androidInterstitialId = "Interstitial_Android";
        [SerializeField] private string iosInterstitialId = "Interstitial_iOS";

        private string gameId;
        private string rewardedId;
        private string interstitialId;

        private Action onRewardedAdSuccess;
        private Action onRewardedAdFailed;
        private Action onInterstitialAdCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAds();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAds()
        {
            gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iosGameId : androidGameId;
            rewardedId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iosRewardedId : androidRewardedId;
            interstitialId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iosInterstitialId : androidInterstitialId;

            Advertisement.Initialize(gameId, testMode, this);
        }

        public void OnInitializationComplete()
        {
            LoadAd(rewardedId);
            TryPresentRewarded();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
        }

        private bool isRewardedLoaded = false;
        private bool isInterstitialLoaded = false;
        private bool rewardShowPending;
        private int rewardLoadAttempts;
        const int RewardLoadLimit = 8;

        private void LoadAd(string adUnitId)
        {
            Advertisement.Load(adUnitId, this);
        }

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            if (adUnitId == rewardedId)
            {
                isRewardedLoaded = true;
                TryPresentRewarded();
            }
            else if (adUnitId == interstitialId) isInterstitialLoaded = true;
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            if (adUnitId == rewardedId)
            {
                isRewardedLoaded = false;
                if (!rewardShowPending) return;
                rewardLoadAttempts++;
                if (rewardLoadAttempts >= RewardLoadLimit)
                {
                    rewardShowPending = false;
                    Action failed = onRewardedAdFailed;
                    onRewardedAdSuccess = null;
                    onRewardedAdFailed = null;
                    failed?.Invoke();
                    return;
                }
                CancelInvoke(nameof(ReloadRewarded));
                Invoke(nameof(ReloadRewarded), 1.5f);
            }
            else if (adUnitId == interstitialId) isInterstitialLoaded = false;
        }

        /// <summary>
        /// odullu reklam gosterir.
        /// </summary>
        public void ShowRewardedAd(Action onSuccess, Action onFailed = null)
        {
            onRewardedAdSuccess = onSuccess;
            onRewardedAdFailed = onFailed;
            rewardShowPending = true;
            rewardLoadAttempts = 0;
            CancelInvoke(nameof(ReloadRewarded));
            TryPresentRewarded();
        }

        void TryPresentRewarded()
        {
            if (!rewardShowPending) return;
            if (!Advertisement.isInitialized) return;
            if (!isRewardedLoaded)
            {
                LoadAd(rewardedId);
                return;
            }
            rewardShowPending = false;
            isRewardedLoaded = false;
            Advertisement.Show(rewardedId, this);
        }

        void ReloadRewarded()
        {
            if (rewardShowPending) LoadAd(rewardedId);
        }

        /// <summary>
        /// Gecis (Zorunlu) reklami gosterir.
        /// </summary>
        public void ShowInterstitialAd(Action onCompleted = null)
        {
            onInterstitialAdCompleted = onCompleted;

            if (isInterstitialLoaded)
            {
                isInterstitialLoaded = false; // Kullanildi
                Advertisement.Show(interstitialId, this);
            }
            else
            {
                FinishInterstitial();
                LoadAd(interstitialId); // Yeniden yuklemeyi dene
            }
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            
            if (adUnitId == rewardedId) FinishRewarded(false);
            else if (adUnitId == interstitialId) FinishInterstitial();
            
            LoadAd(adUnitId);
        }

        public void OnUnityAdsShowStart(string adUnitId) { }
        public void OnUnityAdsShowClick(string adUnitId) { }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            if (adUnitId == rewardedId)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    FinishRewarded(true);
                }
                else
                {
                    FinishRewarded(false);
                }
            }
            else if (adUnitId == interstitialId)
            {
                FinishInterstitial();
            }

            LoadAd(adUnitId); // Gosterimden sonra yenisini yukle
        }

        private void FinishRewarded(bool success)
        {
            Action callback = success ? onRewardedAdSuccess : onRewardedAdFailed;
            onRewardedAdSuccess = null;
            onRewardedAdFailed = null;
            callback?.Invoke();
        }

        private void FinishInterstitial()
        {
            Action callback = onInterstitialAdCompleted;
            onInterstitialAdCompleted = null;
            callback?.Invoke();
        }
    }
}
