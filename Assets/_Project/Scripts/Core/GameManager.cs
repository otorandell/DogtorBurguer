using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private IngredientSpawner _spawner;
        [SerializeField] private DifficultyManager _difficultyManager;

        [Header("Debug")]
        [SerializeField] private bool _autoStartGame = true;
        [SerializeField] private bool _testSettings = false;
        [SerializeField] private bool _testBurgerColumn = false;
        [SerializeField] private bool _testDualColumn = false;
        [SerializeField] private int _testDualColumnLevel = 8;

        public bool TestSettings => _testSettings;
        public bool TestBurgerColumn => _testBurgerColumn;
        public bool TestDualColumn => _testDualColumn;
        public int TestDualColumnLevel => _testDualColumnLevel;

        private GameState _currentState = GameState.Menu;
        private bool _isPaused;
        private int _resolutionDepth;
        private int _score;
        private int _starsEarnedThisRun;
        private int _starsPaidFromScore; // score payout already granted (continues don't double-pay)

        public GameState CurrentState => _currentState;
        public bool IsPaused => _isPaused;
        // True while a burger is resolving (animation in progress). A counter, not a bool, so
        // concurrent resolutions in different columns nest correctly (F-31).
        public bool IsResolving => _resolutionDepth > 0;
        public int Score => _score;
        public int StarsEarnedThisRun => _starsEarnedThisRun;
        public int CurrentLevel => _difficultyManager != null ? _difficultyManager.CurrentLevel : 1;

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLevelChanged; // forwarded from DifficultyManager (F-68)

        private void Start()
        {
            ResolveDependencies();
            EnsureManagers();
            ApplyPersistedSettings();
            SubscribeEvents();

            if (_autoStartGame)
                StartGame();
        }

        private void ResolveDependencies()
        {
            if (_difficultyManager == null)
                _difficultyManager = GetComponent<DifficultyManager>();
            if (_difficultyManager == null)
                _difficultyManager = FindAnyObjectByType<DifficultyManager>();
        }

        private void EnsureManagers()
        {
            // Scene-local managers.
            MonoBehaviourUtil.EnsureComponent<GameHUD>();
            MonoBehaviourUtil.EnsureComponent<GameOverPanel>();
            MonoBehaviourUtil.EnsureComponent<AudioManager>();
            MonoBehaviourUtil.EnsureComponent<BurgerFairySpawner>();
            MonoBehaviourUtil.EnsureComponent<ConsumableInventory>();
            MonoBehaviourUtil.EnsureComponent<ConsumableInventoryView>();
            MonoBehaviourUtil.EnsureComponent<ConsumableDragController>();
            MonoBehaviourUtil.EnsureComponent<BurgerChallenge>();
            MonoBehaviourUtil.EnsureComponent<PlateManager>();

            // Persistent managers (normally already alive from the menu).
            AppBootstrap.EnsureCoreManagers();
        }

        private void ApplyPersistedSettings()
        {
            SoundSettings.Apply();
        }

        private void SubscribeEvents()
        {
            if (_gridManager != null)
            {
                _gridManager.OnGameOver += HandleGameOver;
                _gridManager.OnMatchEliminated += AddScore;
                _gridManager.OnBurgerCompleted += HandleBurgerCompleted;
            }

            if (_difficultyManager != null)
                _difficultyManager.OnLevelChanged += RaiseLevelChanged;
        }

        private void RaiseLevelChanged(int level) => OnLevelChanged?.Invoke(level);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_gridManager != null)
            {
                _gridManager.OnGameOver -= HandleGameOver;
                _gridManager.OnMatchEliminated -= AddScore;
                _gridManager.OnBurgerCompleted -= HandleBurgerCompleted;
            }

            if (_difficultyManager != null)
                _difficultyManager.OnLevelChanged -= RaiseLevelChanged;
        }

        /// <summary>Fresh game start (any → Playing). Resets score and starts spawning.</summary>
        public void StartGame()
        {
            _score = 0;
            _starsEarnedThisRun = 0;
            _starsPaidFromScore = 0;
            OnScoreChanged?.Invoke(_score);

            SetState(GameState.Playing);
            _spawner?.StartSpawning();

            Debug.Log("[GameManager] Game Started!");
        }

        // Pause is a modifier on the Playing state, not a peer state — you can only
        // pause while Playing, and resuming returns to Playing. Kept off GameState so
        // the state machine stays {Menu, Playing, GameOver}.
        /// <summary>Pause-menu entry (Playing &amp; not paused → paused). Stops spawning; timeScale = 0.</summary>
        public void PauseGame()
        {
            if (_currentState != GameState.Playing || _isPaused) return;

            _isPaused = true;
            _spawner?.StopSpawning();
            Time.timeScale = 0f;
        }

        /// <summary>Pause-menu exit (paused → resumed). Resumes spawning; timeScale = 1.</summary>
        public void ResumeGame()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _spawner?.StartSpawning();
            Time.timeScale = 1f;
        }

        /// <summary>Spawner-only pause (used during burger compress). Does not change game state.</summary>
        public void PauseSpawning()
        {
            _spawner?.StopSpawning();
        }

        /// <summary>Spawner-only resume, paired with PauseSpawning. Only resumes while Playing.</summary>
        public void ResumeSpawning()
        {
            if (_currentState == GameState.Playing)
                _spawner?.ResumeSpawning();
        }

        /// <summary>Mark a burger resolution as in progress (freezes game input). Paired with EndResolution.</summary>
        public void BeginResolution() => _resolutionDepth++;

        /// <summary>End a burger resolution. Input unfreezes once all in-flight resolutions end.</summary>
        public void EndResolution()
        {
            if (_resolutionDepth > 0) _resolutionDepth--;
        }

        /// <summary>Full reset via scene reload (any → fresh). Increments games-played.</summary>
        public void RestartGame()
        {
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.IncrementGamesPlayed();

            SceneLoader.LoadGame(); // resets timeScale; Game is the only scene RestartGame runs from
        }

        /// <summary>Revive after game over (GameOver → Playing). Clears top half; score preserved.</summary>
        public void ContinueGame()
        {
            if (_gridManager != null)
                _gridManager.ClearTopHalf();

            SetState(GameState.Playing);
            _spawner?.StartSpawning();

            Debug.Log("[GameManager] Continued! Columns cleared.");
        }

        private void HandleGameOver()
        {
            // Persist the high score as part of the game-over flow (not in the UI panel, F-67).
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.SetHighScore(_score);

            // End-of-run star payout from score — only the slice not paid out by an earlier
            // game over this run (a continue keeps the score, so pay the delta). Awarded before
            // the state change so the game-over panel reads the final run total.
            int payout = _score / MonetizationConfig.STAR_SCORE_DIVISOR - _starsPaidFromScore;
            if (payout > 0)
            {
                _starsPaidFromScore += payout;
                AwardStars(payout);
            }

            SetState(GameState.GameOver);
            _spawner?.StopSpawning();

            Debug.Log($"[GameManager] Game Over! Final Score: {_score}");
        }

        private void HandleBurgerCompleted(int points, string burgerName)
        {
            AddScore(points);
            Debug.Log($"[GameManager] Burger completed: {burgerName} (+{points} pts)");
        }

        public void AddExtraScore(int points)
        {
            AddScore(points);
        }

        /// <summary>Grants stars (persisted immediately) and counts them toward this run's total
        /// (shown on the game-over panel). The award sources are Special Orders and the
        /// end-of-run score payout — rates in MonetizationConfig.</summary>
        public void AwardStars(int amount)
        {
            if (amount <= 0) return;

            _starsEarnedThisRun += amount;
            SaveDataManager.Instance?.AddStars(amount);
        }

        private void AddScore(int points)
        {
            _score += points;
            OnScoreChanged?.Invoke(_score);
        }

        private void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);

            Debug.Log($"[GameManager] State changed to: {_currentState}");
        }


    }
}
