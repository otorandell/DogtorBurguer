using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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
        private int _score;

        public GameState CurrentState => _currentState;
        public bool IsPaused => _isPaused;
        public int Score => _score;

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_difficultyManager == null)
                _difficultyManager = GetComponent<DifficultyManager>();
            if (_difficultyManager == null)
                _difficultyManager = FindAnyObjectByType<DifficultyManager>();

            // Ensure UI, audio, and monetization components exist
            EnsureComponent<GameHUD>();
            EnsureComponent<GameOverPanel>();
            EnsureComponent<AudioManager>();
            EnsureComponent<GemPackSpawner>();
            EnsureComponent<BurgerChallenge>();

            // Ensure SaveDataManager persists from menu
            if (SaveDataManager.Instance == null)
            {
                GameObject saveObj = new GameObject("SaveDataManager");
                saveObj.AddComponent<SaveDataManager>();
            }

            // Ensure MusicManager persists from menu
            if (MusicManager.Instance == null)
            {
                GameObject musicObj = new GameObject("MusicManager");
                musicObj.AddComponent<MusicManager>();
            }

            // Apply sound setting
            AudioListener.volume = SaveDataManager.Instance.SoundOn ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();

            // Subscribe to events
            if (_gridManager != null)
            {
                _gridManager.OnGameOver += HandleGameOver;
                _gridManager.OnMatchEliminated += AddScore;
                _gridManager.OnBurgerCompleted += HandleBurgerCompleted;
            }

            if (_autoStartGame)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            if (_gridManager != null)
            {
                _gridManager.OnGameOver -= HandleGameOver;
                _gridManager.OnMatchEliminated -= AddScore;
                _gridManager.OnBurgerCompleted -= HandleBurgerCompleted;
            }
        }

        /// <summary>Fresh game start (any → Playing). Resets score and starts spawning.</summary>
        public void StartGame()
        {
            _score = 0;
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

        /// <summary>Full reset via scene reload (any → fresh). Increments games-played.</summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.IncrementGamesPlayed();

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
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

        private void EnsureComponent<T>() where T : MonoBehaviour
        {
            if (FindAnyObjectByType<T>() == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                obj.AddComponent<T>();
            }
        }

    }
}
