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

        [Header("Debug")]
        [SerializeField] private bool _autoStartGame = true;

        private GameState _currentState = GameState.Menu;
        private int _score;

        public GameState CurrentState => _currentState;
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

        public void StartGame()
        {
            _score = 0;
            OnScoreChanged?.Invoke(_score);

            SetState(GameState.Playing);
            _spawner?.StartSpawning();

            Debug.Log("[GameManager] Game Started!");
        }

        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;

            SetState(GameState.Paused);
            _spawner?.StopSpawning();
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;

            SetState(GameState.Playing);
            _spawner?.StartSpawning();
            Time.timeScale = 1f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
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

#if UNITY_EDITOR
        private void OnGUI()
        {
            // Debug GUI
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.Label($"State: {_currentState}");
            GUILayout.Label($"Score: {_score}");

            if (_currentState == GameState.GameOver)
            {
                if (GUILayout.Button("Restart"))
                {
                    RestartGame();
                }
            }
            else if (_currentState == GameState.Playing)
            {
                if (GUILayout.Button("Pause"))
                {
                    PauseGame();
                }
            }
            else if (_currentState == GameState.Paused)
            {
                if (GUILayout.Button("Resume"))
                {
                    ResumeGame();
                }
            }

            GUILayout.EndArea();
        }
#endif
    }
}
