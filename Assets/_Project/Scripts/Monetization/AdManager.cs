using System;
using System.Collections;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Ad manager wrapper. Currently uses mock ads (simulated delays).
    /// Replace mock methods with Unity Ads SDK calls when ready for production.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private bool _isShowingAd;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAds();
        }

        private void InitializeAds()
        {
            // TODO: Replace with Unity Ads initialization
            // UnityAds.Initialize(gameId, testMode);
            Debug.Log("[AdManager] Initialized (mock mode)");
        }

        /// <summary>
        /// Shows an interstitial ad. Calls onComplete when done.
        /// </summary>
        public void ShowInterstitial(Action onComplete = null)
        {
            if (_isShowingAd)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(MockInterstitial(onComplete));
        }

        /// <summary>
        /// Shows a rewarded ad. Calls onResult(true) if watched, (false) if skipped/failed.
        /// </summary>
        public void ShowRewarded(Action<bool> onResult)
        {
            if (_isShowingAd)
            {
                onResult?.Invoke(false);
                return;
            }

            StartCoroutine(MockRewarded(onResult));
        }

        public bool IsAdAvailable()
        {
            // TODO: Check Unity Ads availability
            return true;
        }

        private IEnumerator MockInterstitial(Action onComplete)
        {
            _isShowingAd = true;
            Debug.Log("[AdManager] Showing interstitial ad (mock - 1s delay)");

            // Simulate ad display
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;

            _isShowingAd = false;
            Debug.Log("[AdManager] Interstitial complete");
            onComplete?.Invoke();
        }

        private IEnumerator MockRewarded(Action<bool> onResult)
        {
            _isShowingAd = true;
            Debug.Log("[AdManager] Showing rewarded ad (mock - 2s delay)");

            // Simulate watching a rewarded ad
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(2f);
            Time.timeScale = 1f;

            _isShowingAd = false;
            Debug.Log("[AdManager] Rewarded ad complete - granting reward");
            onResult?.Invoke(true);
        }
    }
}
