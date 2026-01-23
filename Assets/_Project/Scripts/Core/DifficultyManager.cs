using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private IngredientSpawner _spawner;

        private static readonly int[] LevelThresholds = {
            0, 50, 120, 200, 300, 450, 650, 900, 1200, 1600
        };

        private int _currentLevel = 1;

        public int CurrentLevel => _currentLevel;
        public event Action<int> OnLevelChanged;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged += EvaluateLevel;

            ApplyDifficulty();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged -= EvaluateLevel;
        }

        private void EvaluateLevel(int score)
        {
            int newLevel = 1;
            for (int i = LevelThresholds.Length - 1; i >= 0; i--)
            {
                if (score >= LevelThresholds[i])
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
                Debug.Log($"[Difficulty] Level up! Now level {_currentLevel}");
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
        }
    }
}
