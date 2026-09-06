namespace DogtorBurguer
{
    /// <summary>
    /// The leaderboard-service contract (sibling of IAdProvider/IIapProvider): async sign-in at
    /// Initialize, score reporting, and the platform leaderboard UI. Implementations:
    /// PlayGamesLeaderboardProvider (device, behind the PLAY_GAMES define) and the logging mock.
    /// </summary>
    public interface ILeaderboardProvider
    {
        /// <summary>True when signed in and able to report/show. May flip after async init.</summary>
        bool IsAvailable { get; }

        void Initialize();

        /// <summary>Reports a finished run's score. No-op while unavailable.</summary>
        void ReportScore(int score);

        /// <summary>Opens the platform leaderboard UI. No-op while unavailable.</summary>
        void ShowLeaderboard();
    }
}
