using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Ads
{
    public class AdCreator : MonoBehaviour
    {
        public static AdCreator Instance;

        private const string _AD_INTER_ID = "";
        private const string _AD_BANNER_ID = "ca-app-pub-6439087888655630/3234664231";
        private const string _AD_REWARD_ID = "ca-app-pub-6439087888655630/1758326846";
    
        private InterstitialAd _interstitialAd;
        private BannerView _bannerView;
        private RewardedAd _rewardedAd;
        private int _loadTries = 1;

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);

            Instance = this;

            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                LoadRewardedAd();
                //LoadInterstitialAd();
                //CreateBannerAd(); create banner before show for all screens
            });
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            DestroyBannerAd();
            DestroyInterstitialAd();
            DestroyRewardAd();
        }

        public void LoadRewardedAd()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            var adRequest = new AdRequest();

            RewardedAd.Load(_AD_REWARD_ID, adRequest, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded_failed_load_ad : " + error);
                    AnalyticsLogger.Instance.SendAnalyticsEvent("Rewarded_failed_load_ad_" + error);

                    return;
                }

                _rewardedAd = ad;
            });
        }

        public void ShowRewardedAd(Action onShow)
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("ShowRewardedAd");

            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    onShow?.Invoke();
                    AnalyticsLogger.Instance.SendAnalyticsEvent("RewardedAdShown");

                    LoadRewardedAd();

                    _rewardedAd.OnAdFullScreenContentClosed += LoadRewardedAd;
                    _rewardedAd.OnAdFullScreenContentFailed += (AdError error) => { LoadRewardedAd(); };
                });
            }
        }

        private void CreateBannerAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("CreateBannerAd");

            if (_bannerView != null)
                DestroyBannerAd();
            AdSize adSize = AdSize.Banner;

            _bannerView = new BannerView(_AD_BANNER_ID, adSize, AdPosition.Top);
            AnalyticsLogger.Instance.SendAnalyticsEvent("BannerAdCreated");

        }

        private IEnumerator LoadCooldown()
        {
            yield return new WaitForSeconds(3);
            LoadInterstitialAd();
            _loadTries--;
        }

        public void LoadInterstitialAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("LoadInterstitialAd");

            if (_interstitialAd != null)
            {
                DestroyInterstitialAd();
            }

            var adRequest = new AdRequest();

            InterstitialAd.Load(_AD_INTER_ID, adRequest, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    AnalyticsLogger.Instance.SendAnalyticsEvent("LoadInterstitialAdFailed");

                    Debug.LogError("interstitial ad failed to load an ad " + error);
                    if (_loadTries > 0)
                        StartCoroutine(LoadCooldown());
                    return;
                }

                _loadTries = 1;
                _interstitialAd = ad;
            });
        }

        public void ShowBannerAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("ShowBannerAd");

            if (_bannerView == null)
                CreateBannerAd();

            var adRequest = new AdRequest();
            _bannerView?.LoadAd(adRequest);
            _bannerView?.Show();
        }

        public void HideBannerAd()
        {
            if (_bannerView == null) return;
            _bannerView.Hide();
        }

        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                if (_interstitialAd.CanShowAd())
                {
                    _interstitialAd.Show();
                    _interstitialAd.OnAdFullScreenContentClosed += LoadInterstitialAd;
                }

            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }
        }

        public void DestroyInterstitialAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("DestroyInterstitialAd");

            if (_interstitialAd != null)
            {
                _interstitialAd.OnAdFullScreenContentClosed -= LoadInterstitialAd;
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        public void DestroyBannerAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("DestroyBannerAd");

            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
        }
        
        public void DestroyRewardAd()
        {
            AnalyticsLogger.Instance.SendAnalyticsEvent("DestroyRewardAd");

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }
    }
}