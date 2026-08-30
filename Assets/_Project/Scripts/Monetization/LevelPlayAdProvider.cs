using System;
using System.Collections;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The real-SDK <see cref="IAdProvider"/> over Unity LevelPlay (ironSource mediation).
    /// Selected by <see cref="AdManager"/> on device builds once the
    /// MonetizationConfig.LEVELPLAY_* credentials are filled in; the editor and unconfigured
    /// builds keep <see cref="MockAdProvider"/>. Honors the contract: Ready flags are the
    /// SDK's real load state, rewards fire only from the SDK's reward callback, onClosed
    /// fires exactly once per show, ads auto-reload after a show and retry after a failed
    /// load, and the previous timeScale is saved/restored around the fullscreen takeover.
    /// </summary>
    public class LevelPlayAdProvider : MonoBehaviour, IAdProvider
    {
        // Implementation timing — SDK-retry plumbing, deliberately NOT in MonetizationConfig.
        private const float LOAD_RETRY_DELAY = 6f;
        private const float INIT_RETRY_DELAY = 10f;

        public bool IsInitialized { get; private set; }
        public bool IsInterstitialReady => _interstitial != null && _interstitial.IsAdReady();
        public bool IsRewardedReady => _rewarded != null && _rewarded.IsAdReady();

        private LevelPlayInterstitialAd _interstitial;
        private LevelPlayRewardedAd _rewarded;

        private bool _isShowing;
        private float _previousTimeScale = 1f;
        private Action _pendingInterstitialClosed;
        private Action _pendingRewarded;
        private Action _pendingRewardedClosed;

        public void Initialize()
        {
            if (IsInitialized) return;

            if (!MonetizationConfig.LevelPlayConfigured)
            {
                Debug.LogError("[LevelPlayAdProvider] LEVELPLAY_* credentials are empty — no ads will serve. " +
                               "AdManager should have selected MockAdProvider instead.");
                return;
            }

            LevelPlay.OnInitSuccess += HandleInitSuccess;
            LevelPlay.OnInitFailed += HandleInitFailed;

            if (MonetizationConfig.LEVELPLAY_TEST_SUITE)
                LevelPlay.SetMetaData("is_test_suite", "enable");

            LevelPlay.Init(MonetizationConfig.LEVELPLAY_APP_KEY);
        }

        private void HandleInitSuccess(LevelPlayConfiguration config)
        {
            IsInitialized = true;
            Debug.Log("[LevelPlayAdProvider] Initialized");

            _interstitial = new LevelPlayInterstitialAd(MonetizationConfig.LEVELPLAY_INTERSTITIAL_ID);
            _interstitial.OnAdLoadFailed += error => RetryLoad("interstitial", error, () => _interstitial.LoadAd());
            _interstitial.OnAdDisplayFailed += (info, error) => FinishInterstitial($"display failed: {error}");
            _interstitial.OnAdClosed += info => FinishInterstitial(null);

            _rewarded = new LevelPlayRewardedAd(MonetizationConfig.LEVELPLAY_REWARDED_ID);
            _rewarded.OnAdLoadFailed += error => RetryLoad("rewarded", error, () => _rewarded.LoadAd());
            _rewarded.OnAdDisplayFailed += (info, error) => FinishRewarded($"display failed: {error}");
            _rewarded.OnAdClosed += info => FinishRewarded(null);
            // The contract's core rule: the reward is granted HERE and nowhere else.
            _rewarded.OnAdRewarded += (info, reward) =>
            {
                _pendingRewarded?.Invoke();
                _pendingRewarded = null;
            };

            _interstitial.LoadAd();
            _rewarded.LoadAd();

            if (MonetizationConfig.LEVELPLAY_TEST_SUITE)
                LevelPlay.LaunchTestSuite();
        }

        private void HandleInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning($"[LevelPlayAdProvider] Init failed ({error}), retrying in {INIT_RETRY_DELAY}s");
            StartCoroutine(RetryInitRoutine());
        }

        private IEnumerator RetryInitRoutine()
        {
            yield return new WaitForSecondsRealtime(INIT_RETRY_DELAY);
            LevelPlay.Init(MonetizationConfig.LEVELPLAY_APP_KEY);
        }

        private void RetryLoad(string kind, LevelPlayAdError error, Action reload)
        {
            Debug.Log($"[LevelPlayAdProvider] {kind} load failed ({error}), retrying in {LOAD_RETRY_DELAY}s");
            StartCoroutine(RetryLoadRoutine(reload));
        }

        private IEnumerator RetryLoadRoutine(Action reload)
        {
            yield return new WaitForSecondsRealtime(LOAD_RETRY_DELAY);
            reload();
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!IsInterstitialReady || _isShowing)
            {
                onClosed?.Invoke();
                return;
            }

            BeginShow();
            _pendingInterstitialClosed = onClosed;
            _interstitial.ShowAd();
        }

        public void ShowRewarded(Action onRewarded, Action onClosed)
        {
            if (!IsRewardedReady || _isShowing)
            {
                onClosed?.Invoke();
                return;
            }

            BeginShow();
            _pendingRewarded = onRewarded;
            _pendingRewardedClosed = onClosed;
            _rewarded.ShowAd();
        }

        // Freeze the game under the fullscreen takeover, remembering the PREVIOUS timeScale —
        // an ad shown over a paused game must hand the pause back intact.
        private void BeginShow()
        {
            _isShowing = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // Shared end-of-show path for close and display-failure: unfreeze, fire onClosed exactly
        // once, and start the next preload.
        private void FinishInterstitial(string failure)
        {
            if (failure != null) Debug.LogWarning($"[LevelPlayAdProvider] interstitial {failure}");
            EndShow();

            Action closed = _pendingInterstitialClosed;
            _pendingInterstitialClosed = null;
            closed?.Invoke();

            _interstitial.LoadAd();
        }

        private void FinishRewarded(string failure)
        {
            if (failure != null) Debug.LogWarning($"[LevelPlayAdProvider] rewarded {failure}");
            EndShow();

            _pendingRewarded = null; // an unearned reward never pays out
            Action closed = _pendingRewardedClosed;
            _pendingRewardedClosed = null;
            closed?.Invoke();

            _rewarded.LoadAd();
        }

        private void EndShow()
        {
            if (!_isShowing) return;
            _isShowing = false;
            Time.timeScale = _previousTimeScale;
        }

        private void OnDestroy()
        {
            LevelPlay.OnInitSuccess -= HandleInitSuccess;
            LevelPlay.OnInitFailed -= HandleInitFailed;
        }
    }
}
