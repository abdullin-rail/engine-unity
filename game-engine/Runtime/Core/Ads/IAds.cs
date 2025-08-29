// Engine.Core.Ads/IAds.cs
using System;

namespace Engine.Core.Ads
{
    public interface IAds
    {
        void LoadRewarded();
        void ShowRewarded(Action onReward);

        void LoadInterstitial();
        void ShowInterstitial();

        void ShowBanner();
        void HideBanner();

        void DestroyAll();
    }
}
