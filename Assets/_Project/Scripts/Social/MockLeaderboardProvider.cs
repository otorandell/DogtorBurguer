using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Editor / unconfigured stand-in: always "available", logs instead of talking to Play
    /// Games — so the trophy-pill button and the game-over report path get exercised anywhere.
    /// </summary>
    public sealed class MockLeaderboardProvider : MonoBehaviour, ILeaderboardProvider
    {
        public bool IsAvailable => true;

        public void Initialize() { }

        public void ReportScore(int score) =>
            Debug.Log($"[Leaderboard] (mock) score reported: {score}");

        public void ShowLeaderboard() =>
            Debug.Log("[Leaderboard] (mock) show leaderboard UI");
    }
}
