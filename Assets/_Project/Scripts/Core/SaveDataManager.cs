using System;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class SaveDataManager : Singleton<SaveDataManager>
    {
        private const string KEY_GEMS = "gems";
        private const string KEY_STARS = "stars";
        private const string KEY_HIGH_SCORE = "highScore";
        private const string KEY_SOUND_ON = "soundOn";
        private const string KEY_GAMES_PLAYED = "gamesPlayed";
        private const string KEY_CONTROL_MODE = "controlMode";
        private const string KEY_STARTING_LEVEL = "startingLevel";
        private const string KEY_ADS_REMOVED = "adsRemoved";
        private const string KEY_TUTORIAL_SEEN = "tutorialSeen";
        private const string KEY_OWNED_SKINS = "ownedSkins";          // CSV of skin ids
        private const string KEY_EQUIPPED_PREFIX = "equippedSkin_";   // + (int)SkinSlot → skin id
        private const string KEY_CONSUMABLE_PREFIX = "consumable_";   // + (int)ConsumableType → count
        private const string KEY_GEM_AD_DATE = "gemAdDate";           // yyyymmdd of the last rewarded-gem ad
        private const string KEY_GEM_AD_COUNT = "gemAdCount";         // rewarded-gem ads watched on that day

        // Canonical first-run defaults. Single source of truth — referenced by
        // LoadData and by consumers that need a fallback when Instance is null.
        public const bool DEFAULT_SOUND_ON = true;
        public const ControlMode DEFAULT_CONTROL_MODE = ControlMode.Drag;
        public const int DEFAULT_STARTING_LEVEL = 1;

        public event Action<int> OnGemsChanged;
        public event Action<int> OnStarsChanged;
        public event Action OnConsumablesChanged;

        public int Gems { get; private set; }
        public int Stars { get; private set; }
        public int HighScore { get; private set; }
        public bool SoundOn { get; private set; }
        public int GamesPlayed { get; private set; }
        public ControlMode ControlMode { get; private set; }
        public int StartingLevel { get; private set; }
        public bool AdsRemoved { get; private set; }
        public bool TutorialSeen { get; private set; }

        private readonly HashSet<string> _ownedSkins = new();
        private readonly int[] _consumableCounts = new int[ConsumableInventory.TypeCount];

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
            Stars = PlayerPrefs.GetInt(KEY_STARS, 0);
            HighScore = PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
            SoundOn = PlayerPrefs.GetInt(KEY_SOUND_ON, DEFAULT_SOUND_ON ? 1 : 0) == 1;
            GamesPlayed = PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
            ControlMode = (ControlMode)PlayerPrefs.GetInt(KEY_CONTROL_MODE, (int)DEFAULT_CONTROL_MODE);
            StartingLevel = Mathf.Clamp(
                PlayerPrefs.GetInt(KEY_STARTING_LEVEL, DEFAULT_STARTING_LEVEL), 1, GameplayConfig.SETTINGS_LEVEL_CAP);
            AdsRemoved = PlayerPrefs.GetInt(KEY_ADS_REMOVED, 0) == 1;
            TutorialSeen = PlayerPrefs.GetInt(KEY_TUTORIAL_SEEN, 0) == 1;

            _ownedSkins.Clear();
            string owned = PlayerPrefs.GetString(KEY_OWNED_SKINS, "");
            foreach (string id in owned.Split(',', StringSplitOptions.RemoveEmptyEntries))
                _ownedSkins.Add(id);

            for (int i = 0; i < _consumableCounts.Length; i++)
                _consumableCounts[i] = PlayerPrefs.GetInt(KEY_CONSUMABLE_PREFIX + i, 0);
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

        public void AddStars(int amount)
        {
            Stars += amount;
            PlayerPrefs.SetInt(KEY_STARS, Stars);
            PlayerPrefs.Save();
            OnStarsChanged?.Invoke(Stars);
        }

        public bool SpendStars(int amount)
        {
            if (Stars < amount) return false;

            Stars -= amount;
            PlayerPrefs.SetInt(KEY_STARS, Stars);
            PlayerPrefs.Save();
            OnStarsChanged?.Invoke(Stars);
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

        /// <summary>One-way: set on tutorial completion OR skip — it never auto-plays twice.</summary>
        public void SetTutorialSeen()
        {
            TutorialSeen = true;
            PlayerPrefs.SetInt(KEY_TUTORIAL_SEEN, 1);
            PlayerPrefs.Save();
        }

        public void SetStartingLevel(int level)
        {
            StartingLevel = Mathf.Clamp(level, 1, GameplayConfig.SETTINGS_LEVEL_CAP);
            PlayerPrefs.SetInt(KEY_STARTING_LEVEL, StartingLevel);
            PlayerPrefs.Save();
        }

        /// <summary>One-way: the remove-ads purchase can't be un-bought (store restore re-grants it).</summary>
        public void SetAdsRemoved()
        {
            AdsRemoved = true;
            PlayerPrefs.SetInt(KEY_ADS_REMOVED, 1);
            PlayerPrefs.Save();
        }

        // --- free gem ads (the shop's FREE rung, capped per local calendar day — GEM_AD_DAILY_CAP) ---

        /// <summary>Rewarded gem ads watched today. Resets implicitly when the stored day differs.</summary>
        public int GemAdsWatchedToday =>
            PlayerPrefs.GetInt(KEY_GEM_AD_DATE, 0) == TodayStamp() ? PlayerPrefs.GetInt(KEY_GEM_AD_COUNT, 0) : 0;

        public void RecordGemAdWatched()
        {
            int count = GemAdsWatchedToday + 1; // reads through the date check, so a stale count restarts at 1
            PlayerPrefs.SetInt(KEY_GEM_AD_DATE, TodayStamp());
            PlayerPrefs.SetInt(KEY_GEM_AD_COUNT, count);
            PlayerPrefs.Save();
        }

        private static int TodayStamp()
        {
            DateTime now = DateTime.Now;
            return now.Year * 10000 + now.Month * 100 + now.Day;
        }

        // --- skins (ownership is by skin id; default skins are implicitly owned — see ShopService) ---

        public bool OwnsSkin(string skinId) => _ownedSkins.Contains(skinId);

#if UNITY_EDITOR
        /// <summary>Debug (editor-only): wipes shop purchases — owned skins, per-slot equips and
        /// remove-ads — plus the consumable stock and the tutorial-seen flag, so the next Play
        /// exercises the first-run tutorial. Bound to R while the shop is open (ShopScreen).</summary>
        public void DebugResetShop()
        {
            TutorialSeen = false;
            PlayerPrefs.SetInt(KEY_TUTORIAL_SEEN, 0);

            _ownedSkins.Clear();
            PlayerPrefs.SetString(KEY_OWNED_SKINS, "");
            foreach (SkinSlot slot in System.Enum.GetValues(typeof(SkinSlot)))
                PlayerPrefs.SetString(KEY_EQUIPPED_PREFIX + (int)slot, "");
            AdsRemoved = false;
            PlayerPrefs.SetInt(KEY_ADS_REMOVED, 0);
            for (int i = 0; i < _consumableCounts.Length; i++)
            {
                _consumableCounts[i] = 0;
                PlayerPrefs.SetInt(KEY_CONSUMABLE_PREFIX + i, 0);
            }
            PlayerPrefs.Save();
            OnConsumablesChanged?.Invoke();
        }

        /// <summary>Editor-menu wipe usable OUTSIDE play mode (no live instance): deletes the
        /// same keys DebugResetShop clears, straight through PlayerPrefs.</summary>
        public static void DebugWipeProgressKeys()
        {
            PlayerPrefs.DeleteKey(KEY_TUTORIAL_SEEN);
            PlayerPrefs.DeleteKey(KEY_OWNED_SKINS);
            foreach (SkinSlot slot in System.Enum.GetValues(typeof(SkinSlot)))
                PlayerPrefs.DeleteKey(KEY_EQUIPPED_PREFIX + (int)slot);
            PlayerPrefs.DeleteKey(KEY_ADS_REMOVED);
            for (int i = 0; i < ConsumableInventory.TypeCount; i++)
                PlayerPrefs.DeleteKey(KEY_CONSUMABLE_PREFIX + i);
            PlayerPrefs.Save();
        }
#endif

        public void GrantSkin(string skinId)
        {
            if (!_ownedSkins.Add(skinId)) return;
            PlayerPrefs.SetString(KEY_OWNED_SKINS, string.Join(",", _ownedSkins));
            PlayerPrefs.Save();
        }

        /// <summary>The equipped skin id for a slot, or "" when the default is equipped.</summary>
        public string GetEquippedSkinId(SkinSlot slot) =>
            PlayerPrefs.GetString(KEY_EQUIPPED_PREFIX + (int)slot, "");

        /// <summary>Persist a slot's equipped skin. Pass "" to fall back to the default.</summary>
        public void SetEquippedSkinId(SkinSlot slot, string skinId)
        {
            PlayerPrefs.SetString(KEY_EQUIPPED_PREFIX + (int)slot, skinId ?? "");
            PlayerPrefs.Save();
        }

        // --- consumables (persistent stock — fairy drops and shop purchases feed the same pool) ---

        public int ConsumableCount(ConsumableType type) => _consumableCounts[(int)type];

        public void AddConsumables(ConsumableType type, int quantity)
        {
            int i = (int)type;
            _consumableCounts[i] += quantity;
            PlayerPrefs.SetInt(KEY_CONSUMABLE_PREFIX + i, _consumableCounts[i]);
            PlayerPrefs.Save();
            OnConsumablesChanged?.Invoke();
        }

        /// <summary>Uses one if available. Returns false (no-op) when that type is out of stock.</summary>
        public bool TryConsumeConsumable(ConsumableType type)
        {
            int i = (int)type;
            if (_consumableCounts[i] <= 0) return false;

            _consumableCounts[i]--;
            PlayerPrefs.SetInt(KEY_CONSUMABLE_PREFIX + i, _consumableCounts[i]);
            PlayerPrefs.Save();
            OnConsumablesChanged?.Invoke();
            return true;
        }
    }
}
