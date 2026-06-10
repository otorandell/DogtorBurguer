using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class SaveDataManager : Singleton<SaveDataManager>
    {
        private const string KEY_GEMS = "gems";
        private const string KEY_HIGH_SCORE = "highScore";
        private const string KEY_SOUND_ON = "soundOn";
        private const string KEY_GAMES_PLAYED = "gamesPlayed";
        private const string KEY_CONTROL_MODE = "controlMode";
        private const string KEY_STARTING_LEVEL = "startingLevel";

        // Canonical first-run defaults. Single source of truth — referenced by
        // LoadData and by consumers that need a fallback when Instance is null.
        public const bool DEFAULT_SOUND_ON = true;
        public const ControlMode DEFAULT_CONTROL_MODE = ControlMode.Drag;
        public const int DEFAULT_STARTING_LEVEL = 1;

        public event Action<int> OnGemsChanged;

        public int Gems { get; private set; }
        public int HighScore { get; private set; }
        public bool SoundOn { get; private set; }
        public int GamesPlayed { get; private set; }
        public ControlMode ControlMode { get; private set; }
        public int StartingLevel { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            DontDestroyOnLoad(gameObject);

            LoadData();
        }

        private void LoadData()
        {
            Gems = PlayerPrefs.GetInt(KEY_GEMS, 0);
            HighScore = PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
            SoundOn = PlayerPrefs.GetInt(KEY_SOUND_ON, DEFAULT_SOUND_ON ? 1 : 0) == 1;
            GamesPlayed = PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
            ControlMode = (ControlMode)PlayerPrefs.GetInt(KEY_CONTROL_MODE, (int)DEFAULT_CONTROL_MODE);
            StartingLevel = Mathf.Clamp(
                PlayerPrefs.GetInt(KEY_STARTING_LEVEL, DEFAULT_STARTING_LEVEL), 1, GameplayConfig.SETTINGS_LEVEL_CAP);
        }

        public void AddGems(int amount)
        {
            Gems += amount;
            PlayerPrefs.SetInt(KEY_GEMS, Gems);
            PlayerPrefs.Save();
            OnGemsChanged?.Invoke(Gems);
        }

        public bool SpendGems(int amount)
        {
            if (Gems < amount) return false;

            Gems -= amount;
            PlayerPrefs.SetInt(KEY_GEMS, Gems);
            PlayerPrefs.Save();
            OnGemsChanged?.Invoke(Gems);
            return true;
        }

        public void SetHighScore(int score)
        {
            if (score <= HighScore) return;

            HighScore = score;
            PlayerPrefs.SetInt(KEY_HIGH_SCORE, HighScore);
            PlayerPrefs.Save();
        }

        public void SetSoundOn(bool on)
        {
            // Persistence only — applying it to the AudioListener/music is SoundSettings.Apply (F-78).
            SoundOn = on;
            PlayerPrefs.SetInt(KEY_SOUND_ON, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void IncrementGamesPlayed()
        {
            GamesPlayed++;
            PlayerPrefs.SetInt(KEY_GAMES_PLAYED, GamesPlayed);
            PlayerPrefs.Save();
        }

        public void SetControlMode(ControlMode mode)
        {
            ControlMode = mode;
            PlayerPrefs.SetInt(KEY_CONTROL_MODE, (int)mode);
            PlayerPrefs.Save();
        }

        public void SetStartingLevel(int level)
        {
            StartingLevel = Mathf.Clamp(level, 1, GameplayConfig.SETTINGS_LEVEL_CAP);
            PlayerPrefs.SetInt(KEY_STARTING_LEVEL, StartingLevel);
            PlayerPrefs.Save();
        }
    }
}
