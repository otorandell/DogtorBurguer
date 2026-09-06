using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Game-facing leaderboard facade, sibling of AdManager/IapManager: owns one
    /// <see cref="ILeaderboardProvider"/> — Play Games on a configured device build (plugin +
    /// PLAY_GAMES define + SocialConfig id), the logging mock everywhere else. CLASSIC scores
    /// only: the mode gate lives at the game-over call site, next to the high-score write.
    /// Created by AppBootstrap; the TopBar trophy pill opens the board on every screen.
    /// </summary>
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        private ILeaderboardProvider _provider;

        public bool IsAvailable => _provider != null && _provider.IsAvailable;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            DontDestroyOnLoad(gameObject);

#if PLAY_GAMES && !UNITY_EDITOR
            if (SocialConfig.LeaderboardConfigured)
            {
                _provider = gameObject.AddComponent<PlayGamesLeaderboardProvider>();
            }
            else
            {
                Debug.LogWarning("[Leaderboard] PLAY_GAMES set but SocialConfig has no leaderboard id — using the mock.");
                _provider = gameObject.AddComponent<MockLeaderboardProvider>();
            }
#else
            _provider = gameObject.AddComponent<MockLeaderboardProvider>();
#endif
            _provider.Initialize();
        }

        /// <summary>Reports a finished Classic run's score (the caller gates the mode).</summary>
        public void ReportScore(int score) => _provider?.ReportScore(score);

        /// <summary>Opens the platform leaderboard UI (the mock logs).</summary>
        public void ShowLeaderboard() => _provider?.ShowLeaderboard();
    }
}
