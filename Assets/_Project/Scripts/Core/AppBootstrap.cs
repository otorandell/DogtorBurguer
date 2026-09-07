using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// App-startup entry point: ensures the persistent global managers exist. Invoked from
    /// the first scene's setup (the menu) and defensively by GameManager, so this app-init
    /// logic lives outside any UI class (F-71).
    /// </summary>
    public static class AppBootstrap
    {
        public static void EnsureCoreManagers()
        {
            ApplyFrameRate();
            MonoBehaviourUtil.EnsureComponent<SaveDataManager>();
            MonoBehaviourUtil.EnsureComponent<AdManager>();
            MonoBehaviourUtil.EnsureComponent<IapManager>();
            MonoBehaviourUtil.EnsureComponent<LeaderboardManager>();
            MonoBehaviourUtil.EnsureComponent<MusicManager>();
        }

        /// <summary>
        /// Runs the game at the display's refresh rate (60 fps floor). Without this, Android and
        /// iOS default to 30 fps regardless of vSync settings — the editor never shows it.
        /// </summary>
        private static void ApplyFrameRate()
        {
            int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
            Application.targetFrameRate = Mathf.Max(Constants.MIN_TARGET_FRAME_RATE, refresh);
        }
    }
}
