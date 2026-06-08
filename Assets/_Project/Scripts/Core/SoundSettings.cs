using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Applies the persisted sound on/off setting to the global AudioListener and music.
    /// One home for what was duplicated across MainMenuUI, GameManager, and SettingsPanel (F-78).
    /// </summary>
    public static class SoundSettings
    {
        public static void Apply()
        {
            bool soundOn = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.SoundOn
                : SaveDataManager.DEFAULT_SOUND_ON;

            AudioListener.volume = soundOn ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();
        }
    }
}
