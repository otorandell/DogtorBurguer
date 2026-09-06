using System;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Game-facing ad facade. Owns one <see cref="IAdProvider"/> (the mock today; the real
    /// SDK provider at launch — swap it in <see cref="Awake"/>, callers don't change) and the
    /// ad POLICY: the interstitial cadence and the remove-ads suppression. Availability is
    /// real load state — callers should disable ad buttons when the checks are false.
    /// </summary>
    public class AdManager : Singleton<AdManager>
    {
        private IAdProvider _provider;

        /// <summary>True when a rewarded ad is loaded and can actually show (always, on a test build).</summary>
        public bool IsRewardedAvailable => TestBuild.IsEnabled || (_provider != null && _provider.IsRewardedReady);

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            DontDestroyOnLoad(gameObject);

            // Test build: no provider at all — no SDK init, no ads. The public surface below
            // short-circuits (interstitials skipped, rewarded ads reward instantly).
            if (TestBuild.IsEnabled) return;

            // Provider selection: LevelPlay on device builds once its credentials are filled in
            // (MonetizationConfig.LEVELPLAY_*); the editor and unconfigured builds get the mock.
#if UNITY_EDITOR
            _provider = gameObject.AddComponent<MockAdProvider>();
#else
            if (MonetizationConfig.LevelPlayConfigured)
            {
                _provider = gameObject.AddComponent<LevelPlayAdProvider>();
            }
            else
            {
                Debug.LogWarning("[AdManager] LevelPlay credentials not set — using MockAdProvider (no revenue).");
                _provider = gameObject.AddComponent<MockAdProvider>();
            }
#endif
            _provider.Initialize();
        }

        /// <summary>
        /// Ad-cadence policy: an interstitial is due every Nth completed game, and never once
        /// remove-ads is bought. Owns the decision; SaveDataManager only owns the counters.
        /// </summary>
        public bool ShouldShowInterstitial()
        {
            if (TestBuild.IsEnabled) return false;

            // Remove-ads kills forced ads only — rewarded ads (continue, freebies) stay available.
            if (SaveDataManager.Instance != null && SaveDataManager.Instance.AdsRemoved)
                return false;

            int played = SaveDataManager.Instance != null ? SaveDataManager.Instance.GamesPlayed : 0;
            return played > 0 && played % MonetizationConfig.INTERSTITIAL_EVERY_N_GAMES == 0;
        }

        /// <summary>
        /// The restart gate: shows an interstitial when the cadence says one is due AND one is
        /// loaded, then continues. Both restart paths (game over, in-game settings) route
        /// through this so the ad policy can't drift between them. Never blocks the restart —
        /// a due-but-unloaded ad is skipped, not waited for.
        /// </summary>
        public void MaybeShowInterstitial(Action then)
        {
            if (_provider != null && _provider.IsInterstitialReady && ShouldShowInterstitial())
                _provider.ShowInterstitial(then);
            else
                then?.Invoke();
        }

        /// <summary>
        /// Shows a rewarded ad. onResult(true) only when the reward was actually earned;
        /// (false) when no ad is available or it was skipped. Check
        /// <see cref="IsRewardedAvailable"/> to disable the button instead of dead-clicking.
        /// </summary>
        public void ShowRewarded(Action<bool> onResult)
        {
            if (TestBuild.IsEnabled)
            {
                onResult?.Invoke(true);
                return;
            }

            if (_provider == null || !_provider.IsRewardedReady)
            {
                onResult?.Invoke(false);
                return;
            }

            bool rewarded = false;
            _provider.ShowRewarded(
                onRewarded: () => rewarded = true,
                onClosed: () => onResult?.Invoke(rewarded));
        }
    }
}
