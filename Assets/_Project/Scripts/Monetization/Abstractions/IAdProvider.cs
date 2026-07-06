using System;

namespace DogtorBurguer
{
    /// <summary>
    /// The ad-network contract, shaped like real SDKs behave (LevelPlay / AdMob / MAX):
    /// an async init, a preload lifecycle (ads must be LOADED before they can SHOW, and
    /// reload after each show), and callback-driven results. Game code talks to
    /// <see cref="AdManager"/>, which owns one provider — swapping the mock for a real
    /// SDK is one implementation + one construction line, no caller changes.
    ///
    /// Contract rules implementations must honor:
    /// - The Ready flags reflect actual load state; Show* on a not-ready ad must fail
    ///   gracefully (fire onClosed immediately, no display).
    /// - onRewarded fires ONLY when the reward was actually earned — never on a skip or
    ///   an early close. Granting on close is the classic ad-fraud bug.
    /// - onClosed always fires exactly once per Show* call, after the screen is back.
    /// - Implementations re-load automatically after a show and retry failed loads.
    /// </summary>
    public interface IAdProvider
    {
        bool IsInitialized { get; }
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }

        /// <summary>Begins SDK init and starts the first loads. Safe to call once at boot.</summary>
        void Initialize();

        /// <summary>Shows the loaded interstitial. onClosed always fires (immediately when not ready).</summary>
        void ShowInterstitial(Action onClosed);

        /// <summary>Shows the loaded rewarded ad. onRewarded only on an earned reward;
        /// onClosed always fires after (immediately when not ready).</summary>
        void ShowRewarded(Action onRewarded, Action onClosed);
    }
}
