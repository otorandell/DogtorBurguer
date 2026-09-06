namespace DogtorBurguer
{
    /// <summary>
    /// Social / Play Games Services configuration. An empty id = not configured:
    /// LeaderboardManager keeps the logging mock even on device builds.
    /// </summary>
    public static class SocialConfig
    {
        // From the Play Console: Grow → Play Games Services → Setup → Leaderboards
        // (create one "High Score" leaderboard and paste its id here).
        public const string PLAY_GAMES_LEADERBOARD_ID = "";

        /// <summary>True once the leaderboard id is filled in.</summary>
        public static bool LeaderboardConfigured => PLAY_GAMES_LEADERBOARD_ID.Length > 0;
    }
}
