using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Applies the two persisted audio toggles — SFX (SoundOn) and music (MusicOn) — to the
    /// sources that own them. One home for what was duplicated across MainMenuUI, GameManager,
    /// and SettingsPanel (F-78).
    /// </summary>
    public static class SoundSettings
    {
        public static void Apply()
        {
            // The listener stays at full volume: each toggle mutes its OWN sources, so SFX and
            // music are independent. Muting the listener (the pre-2026-09-08 route) is all-or-
            // nothing and would silence music whenever SFX went off. It's a global that survives
            // scene loads, so an old session's 0 is cleared here rather than left stuck.
            AudioListener.volume = 1f;
            AudioManager.Instance?.ApplySoundSetting();
            MusicManager.Instance?.ApplySoundSetting();
        }

        /// <summary>SFX on? Safe before the save layer exists.</summary>
        public static bool SoundOn => SaveDataManager.Instance != null
            ? SaveDataManager.Instance.SoundOn
            : SaveDataManager.DEFAULT_SOUND_ON;

        /// <summary>Music on? Safe before the save layer exists.</summary>
        public static bool MusicOn => SaveDataManager.Instance != null
            ? SaveDataManager.Instance.MusicOn
            : SaveDataManager.DEFAULT_MUSIC_ON;
    }
}
