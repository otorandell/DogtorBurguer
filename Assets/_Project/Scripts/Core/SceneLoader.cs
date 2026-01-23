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
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        public static void LoadGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SCENE_GAME);
        }
    }
}
