using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private IngredientSpawner _spawner;
        [SerializeField] private GridManager _gridManager;

        // Ingredients placed required to reach each level (1-10)
        private static readonly int[] LevelThresholds = {
            0, 3, 7, 12, 18, 25, 33, 42, 52, 64
        };

        private int _currentLevel = 1;
        private int _ingredientsPlaced;

        public int CurrentLevel => _currentLevel;
        public event Action<int> OnLevelChanged;

        private void Start()
        {
            if (_gridManager == null)
                _gridManager = GridManager.Instance;
            if (_spawner == null)
                _spawner = FindAnyObjectByType<IngredientSpawner>();

            if (_gridManager != null)
                _gridManager.OnIngredientPlaced += HandleIngredientPlaced;

            // Start at high level for dual column test mode
            if (GameManager.Instance != null && GameManager.Instance.TestDualColumn)
            {
                _currentLevel = Mathf.Clamp(GameManager.Instance.TestDualColumnLevel, 1, Constants.MAX_LEVEL);
                OnLevelChanged?.Invoke(_currentLevel);
            }

            ApplyDifficulty();
        }

        private void OnDestroy()
        {
            if (_gridManager != null)
                _gridManager.OnIngredientPlaced -= HandleIngredientPlaced;
        }

        private void HandleIngredientPlaced()
        {
            _ingredientsPlaced++;
            EvaluateLevel();
        }

        private void EvaluateLevel()
        {
            int newLevel = 1;
            for (int i = LevelThresholds.Length - 1; i >= 0; i--)
            {
                if (_ingredientsPlaced >= LevelThresholds[i])
                {
                    newLevel = i + 1;
                    break;
                }
            }

            if (newLevel != _currentLevel)
            {
                _currentLevel = newLevel;
                ApplyDifficulty();
                OnLevelChanged?.Invoke(_currentLevel);
                Debug.Log($"[Difficulty] Level {_currentLevel}! ({_ingredientsPlaced} ingredients placed)");
            }
        }

        private void ApplyDifficulty()
        {
            if (_spawner == null) return;

            float t = (_currentLevel - 1f) / (Constants.MAX_LEVEL - 1f);

            float spawnInterval = Mathf.Lerp(Constants.SPAWN_INTERVAL_INITIAL, Constants.SPAWN_INTERVAL_MIN, t);
            float fallStep = Mathf.Lerp(Constants.INITIAL_FALL_STEP_DURATION, Constants.MIN_FALL_STEP_DURATION, t);
            int ingredientCount = Mathf.RoundToInt(Mathf.Lerp(Constants.STARTING_INGREDIENT_COUNT, Constants.MAX_INGREDIENT_COUNT, t));

            _spawner.SetSpawnInterval(spawnInterval);
            _spawner.SetFallSpeed(fallStep);
            _spawner.SetActiveIngredientCount(ingredientCount);
            _spawner.SetCurrentLevel(_currentLevel);
        }
    }
}
