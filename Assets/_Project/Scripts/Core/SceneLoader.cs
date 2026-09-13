using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogtorBurguer
{
    public static class SceneLoader
    {
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_GAME = "Game";

        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            RunSnapshotStore.ClearPending();
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        /// <summary>Loads a FRESH run. Clearing the pending snapshot here is what makes the
        /// resume handoff ordering-proof: every system reads RunSnapshotStore.Pending during its
        /// own init (their Start order relative to each other is undefined), so nothing may clear
        /// it mid-scene. Whoever starts a scene decides instead — a plain load means fresh.</summary>
        public static void LoadGame()
        {
            Time.timeScale = 1f;
            RunSnapshotStore.ClearPending();
            SceneManager.LoadScene(SCENE_GAME);
        }

        /// <summary>Loads the Game scene rebuilding <paramref name="snapshot"/> (menu RESUME).</summary>
        public static void ResumeGame(RunSnapshot snapshot)
        {
            Time.timeScale = 1f;
            RunSnapshotStore.SetPending(snapshot);
            SceneManager.LoadScene(SCENE_GAME);
        }
    }
}
