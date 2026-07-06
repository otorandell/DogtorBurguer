using System;
using System.Collections;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The stand-in <see cref="IAdProvider"/> until a real ad SDK lands. Deliberately
    /// production-shaped: simulated init and load delays, a simulated no-fill rate (so the
    /// "ad not ready" UI paths get exercised in the editor), auto-reload after each show,
    /// and retry-after-failure — the same lifecycle real networks impose. Showing freezes
    /// the game like a real fullscreen ad, saving and restoring the previous timeScale so
    /// an ad over a paused game doesn't un-pause it.
    /// </summary>
    public class MockAdProvider : MonoBehaviour, IAdProvider
    {
        // Simulation tuning — mock-only knobs, deliberately NOT in MonetizationConfig
        // (they tune the placeholder's realism, not the game).
        private const float INIT_DELAY = 0.5f;
        private const float LOAD_DELAY_MIN = 1f;
        private const float LOAD_DELAY_MAX = 2.5f;
        private const float NO_FILL_CHANCE = 0.15f;   // load failure rate; raise to test not-ready UI
        private const float RETRY_DELAY = 4f;         // pause before retrying a failed load
        private const float INTERSTITIAL_DURATION = 1f;
        private const float REWARDED_DURATION = 2f;

        public bool IsInitialized { get; private set; }
        public bool IsInterstitialReady { get; private set; }
        public bool IsRewardedReady { get; private set; }

        private bool _isShowing;
        private bool _interstitialLoading;
        private bool _rewardedLoading;

        public void Initialize()
        {
            if (IsInitialized) return;
            StartCoroutine(InitRoutine());
        }

        private IEnumerator InitRoutine()
        {
            yield return new WaitForSecondsRealtime(INIT_DELAY);
            IsInitialized = true;
            Debug.Log("[MockAdProvider] Initialized (mock)");

            LoadInterstitial();
            LoadRewarded();
        }

        private void LoadInterstitial()
        {
            if (_interstitialLoading || IsInterstitialReady) return;
            _interstitialLoading = true;
            StartCoroutine(LoadRoutine("interstitial",
                ready => { IsInterstitialReady = ready; _interstitialLoading = false; },
                LoadInterstitial));
        }

        private void LoadRewarded()
        {
            if (_rewardedLoading || IsRewardedReady) return;
            _rewardedLoading = true;
            StartCoroutine(LoadRoutine("rewarded",
                ready => { IsRewardedReady = ready; _rewardedLoading = false; },
                LoadRewarded));
        }

        private IEnumerator LoadRoutine(string kind, Action<bool> setReady, Action retry)
        {
            yield return new WaitForSecondsRealtime(Rng.Range(LOAD_DELAY_MIN, LOAD_DELAY_MAX));

            if (Rng.Value < NO_FILL_CHANCE)
            {
                setReady(false);
                Debug.Log($"[MockAdProvider] {kind} no-fill (mock), retrying in {RETRY_DELAY}s");
                yield return new WaitForSecondsRealtime(RETRY_DELAY);
                retry();
            }
            else
            {
                setReady(true);
                Debug.Log($"[MockAdProvider] {kind} loaded (mock)");
            }
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!IsInterstitialReady || _isShowing)
            {
                onClosed?.Invoke();
                return;
            }

            IsInterstitialReady = false;
            StartCoroutine(ShowRoutine("interstitial", INTERSTITIAL_DURATION, null, onClosed, LoadInterstitial));
        }

        public void ShowRewarded(Action onRewarded, Action onClosed)
        {
            if (!IsRewardedReady || _isShowing)
            {
                onClosed?.Invoke();
                return;
            }

            IsRewardedReady = false;
            StartCoroutine(ShowRoutine("rewarded", REWARDED_DURATION, onRewarded, onClosed, LoadRewarded));
        }

        // Simulated fullscreen takeover. Saves and restores the PREVIOUS timeScale (not a
        // hardcoded 1) — an ad shown over a paused game must hand the pause back intact.
        private IEnumerator ShowRoutine(string kind, float duration, Action onRewarded, Action onClosed, Action reload)
        {
            _isShowing = true;
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Debug.Log($"[MockAdProvider] Showing {kind} (mock, {duration}s)");

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = previousTimeScale;
            _isShowing = false;

            // Reward first, then close — matches real SDK callback order.
            onRewarded?.Invoke();
            onClosed?.Invoke();
            reload();
        }
    }
}
