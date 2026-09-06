// The whole file is gated: it only compiles once the Google Play Games plugin v2
// (com.google.play.games) is imported AND the PLAY_GAMES scripting define is set — the plugin
// defines no symbol of its own (unlike Unity IAP), so the define is a manual Player Settings
// step. Until then this file is empty and LeaderboardManager uses the mock.
#if PLAY_GAMES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Google Play Games Services leaderboard (v2 plugin): silent sign-in at init (v2 has no
    /// sign-out; a failed silent sign-in just leaves the feature dormant), report + platform UI
    /// against the one leaderboard in SocialConfig.
    /// </summary>
    public sealed class PlayGamesLeaderboardProvider : MonoBehaviour, ILeaderboardProvider
    {
        private bool _authenticated;

        public bool IsAvailable => _authenticated;

        public void Initialize()
        {
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                _authenticated = status == SignInStatus.Success;
                if (!_authenticated)
                    Debug.LogWarning($"[Leaderboard] Play Games sign-in failed: {status}");
            });
        }

        public void ReportScore(int score)
        {
            if (!_authenticated) return;
            PlayGamesPlatform.Instance.ReportScore(score, SocialConfig.PLAY_GAMES_LEADERBOARD_ID, null);
        }

        public void ShowLeaderboard()
        {
            if (!_authenticated) return;
            PlayGamesPlatform.Instance.ShowLeaderboardUI(SocialConfig.PLAY_GAMES_LEADERBOARD_ID);
        }
    }
}
#endif
