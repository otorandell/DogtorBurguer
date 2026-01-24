using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class SaveDataManager : MonoBehaviour
    {
        public static SaveDataManager Instance { get; private set; }

        private const string KEY_GEMS = "gems";
        private const string KEY_HIGH_SCORE = "highScore";
        private const string KEY_SOUND_ON = "soundOn";
        private const string KEY_GAMES_PLAYED = "gamesPlayed";
        private const string KEY_CONTROL_MODE = "controlMode";

        public event Action<int> OnGemsChanged;

        public int Gems { get; private set; }
        public int HighScore { get; private set; }
        public bool SoundOn { get; private set; }
        public int GamesPlayed { get; private set; }
        public ControlMode ControlMode { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadData();
        }

        private void LoadData()
        {
            Gems = PlayerPrefs.GetInt(KEY_GEMS, 0);
            HighScore = PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
            SoundOn = PlayerPrefs.GetInt(KEY_SOUND_ON, 1) == 1;
            GamesPlayed = PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
            ControlMode = (ControlMode)PlayerPrefs.GetInt(KEY_CONTROL_MODE, (int)ControlMode.Drag);
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
            SoundOn = on;
            PlayerPrefs.SetInt(KEY_SOUND_ON, on ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = on ? 1f : 0f;
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

        public bool ShouldShowInterstitial()
        {
            return GamesPlayed > 0 && GamesPlayed % Constants.INTERSTITIAL_EVERY_N_GAMES == 0;
        }
    }
}
