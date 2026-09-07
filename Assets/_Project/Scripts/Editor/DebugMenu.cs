using UnityEditor;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Editor-menu debug tools — unlike the in-game R hotkey these need no Game-view focus and
    /// work even OUTSIDE play mode (the R press silently goes to whatever editor panel was
    /// clicked last, which reads as "R does nothing").
    /// </summary>
    public static class DebugMenu
    {
        [MenuItem("Tools/Dogtor/Reset Shop + Tutorial + Consumables")]
        private static void ResetProgress()
        {
            if (Application.isPlaying && SaveDataManager.Instance != null)
            {
                SaveDataManager.Instance.DebugResetShop();
                Theme.DebugResetToDefaults();
            }
            else
            {
                SaveDataManager.DebugWipeProgressKeys();
            }
            Debug.Log("[DebugMenu] Wiped: owned skins, equips, remove-ads, consumables, tutorial-seen.");
        }
    }
}
