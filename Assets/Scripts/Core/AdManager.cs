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
        [SerializeField] private bool testMode = true; // MUST BE FALSE FOR PRODUCTION

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

            Debug.Log($"Initializing Unity Ads with Game ID: {gameId}");
            Advertisement.Initialize(gameId, testMode, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
            LoadAd(rewardedId);
            LoadAd(interstitialId);
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }

        private bool isRewardedLoaded = false;
        private bool isInterstitialLoaded = false;

        private void LoadAd(string adUnitId)
        {
            Debug.Log($"Loading Ad: {adUnitId}");
            Advertisement.Load(adUnitId, this);
        }

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            Debug.Log($"Ad Loaded: {adUnitId}");
            if (adUnitId == rewardedId) isRewardedLoaded = true;
            else if (adUnitId == interstitialId) isInterstitialLoaded = true;
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
            if (adUnitId == rewardedId) isRewardedLoaded = false;
            else if (adUnitId == interstitialId) isInterstitialLoaded = false;
        }

        /// <summary>
        /// Ödüllü reklam gösterir.
        /// </summary>
        public void ShowRewardedAd(Action onSuccess, Action onFailed = null)
        {
            onRewardedAdSuccess = onSuccess;
            onRewardedAdFailed = onFailed;

            if (isRewardedLoaded)
            {
                Debug.Log("Showing Rewarded Ad.");
                isRewardedLoaded = false; // Kullanıldı
                Advertisement.Show(rewardedId, this);
            }
            else
            {
                Debug.LogWarning("Rewarded Ad not ready!");
                onRewardedAdFailed?.Invoke();
                LoadAd(rewardedId); // Yeniden yüklemeyi dene
            }
        }

        /// <summary>
        /// Geçiş (Zorunlu) reklamı gösterir.
        /// </summary>
        public void ShowInterstitialAd(Action onCompleted = null)
        {
            onInterstitialAdCompleted = onCompleted;

            if (isInterstitialLoaded)
            {
                Debug.Log("Showing Interstitial Ad.");
                isInterstitialLoaded = false; // Kullanıldı
                Advertisement.Show(interstitialId, this);
            }
            else
            {
                Debug.LogWarning("Interstitial Ad not ready!");
                onInterstitialAdCompleted?.Invoke();
                LoadAd(interstitialId); // Yeniden yüklemeyi dene
            }
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
            
            if (adUnitId == rewardedId) onRewardedAdFailed?.Invoke();
            else if (adUnitId == interstitialId) onInterstitialAdCompleted?.Invoke();
            
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
                    Debug.Log("Rewarded Ad completed successfully.");
                    onRewardedAdSuccess?.Invoke();
                }
                else
                {
                    Debug.Log("Rewarded Ad skipped or failed.");
                    onRewardedAdFailed?.Invoke();
                }
            }
            else if (adUnitId == interstitialId)
            {
                Debug.Log("Interstitial Ad closed.");
                onInterstitialAdCompleted?.Invoke();
            }

            LoadAd(adUnitId); // Gösterimden sonra yenisini yükle
        }
    }
}
