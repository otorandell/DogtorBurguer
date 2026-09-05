using System;
using UnityEngine;

namespace DogtorBurguer
{
    // Runs ahead of default-order scripts so _currentLevel is seeded and difficulty
    // applied before GameManager starts spawning and GameHUD reads CurrentLevel on init.
    [DefaultExecutionOrder(-100)]
    public class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private IngredientSpawner _spawner;
        [SerializeField] private GridManager _gridManager;

        private int _currentLevel = 1;
        private int _ingredientsPlaced;
        private int _lengthScale = 1; // Relax stretches every threshold (same curve, longer run)

        public int CurrentLevel => _currentLevel;
        public event Action<int> OnLevelChanged;

        private void Awake()
        {
            // The per-level curve tables are indexed by level (1..MAX_LEVEL); a length
            // mismatch would index out of range in ApplyDifficulty. Enforce it once here.
            Debug.Assert(
                GameplayConfig.FALL_STEP_BY_LEVEL.Length == GameplayConfig.MAX_LEVEL &&
                GameplayConfig.INGREDIENT_COUNT_BY_LEVEL.Length == GameplayConfig.MAX_LEVEL &&
                GameplayConfig.TRIPLE_CHANCE_BY_LEVEL.Length == GameplayConfig.MAX_LEVEL &&
                GameplayConfig.LEVEL_THRESHOLDS.Length == GameplayConfig.MAX_LEVEL,
                "Difficulty curve tables must each have MAX_LEVEL entries.");
        }

        private void Start()
        {
            if (_gridManager == null)
                _gridManager = GridManager.Instance;
            if (_spawner == null)
                _spawner = FindAnyObjectByType<IngredientSpawner>();

            if (_gridManager != null)
                _gridManager.OnIngredientPlaced += HandleIngredientPlaced;

            // Seed the starting level. Dual-column test mode overrides; otherwise the
            // player-chosen Settings value (defaults to 1). The '>' guard in EvaluateLevel
            // keeps a raised start from being reset before _ingredientsPlaced catches up.
            int startLevel = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.StartingLevel
                : SaveDataManager.DEFAULT_STARTING_LEVEL;
            if (GameManager.Instance != null && GameManager.Instance.TestDualColumn)
                startLevel = GameManager.Instance.TestDualColumnLevel;

            // Relax runs stretch every threshold — read once per run (the Mode toggle is
            // menu-only, so it can't change mid-run).
            GameMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.Mode : SaveDataManager.DEFAULT_GAME_MODE;
            _lengthScale = mode == GameMode.Relax ? GameplayConfig.RELAX_LENGTH_SCALE : 1;

            _currentLevel = Mathf.Clamp(startLevel, 1, GameplayConfig.KILLER_LEVEL);
            // The initial level is pull-state, not an event: subscribers read CurrentLevel
            // on their own init (GameHUD.RefreshAll, GameOverPanel). Firing OnLevelChanged
            // here would risk a spurious level-up SFX and depend on subscribe ordering.
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
            for (int i = GameplayConfig.LEVEL_THRESHOLDS.Length - 1; i >= 0; i--)
            {
                if (_ingredientsPlaced >= GameplayConfig.LEVEL_THRESHOLDS[i] * _lengthScale)
                {
                    newLevel = i + 1;
                    break;
                }
            }

            // Sustained survival past the top of the curve tips into the kill screen.
            if (_ingredientsPlaced >= GameplayConfig.KILLER_LEVEL_THRESHOLD * _lengthScale)
                newLevel = GameplayConfig.KILLER_LEVEL;

            // Level only ever rises with ingredients placed; using '>' (not '!=') keeps a
            // manually-raised level (e.g. dual-column test mode) from being reset to 1 on
            // the first placement, when _ingredientsPlaced hasn't caught up yet.
            if (newLevel > _currentLevel)
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

            float fallStep;
            int ingredientCount;
            float tripleChance;

            if (_currentLevel >= GameplayConfig.KILLER_LEVEL)
            {
                // Kill screen: every wave is a triple at the absolute speed floor.
                fallStep = GameplayConfig.MIN_FALL_STEP_DURATION;
                ingredientCount = GameplayConfig.MAX_INGREDIENT_COUNT;
                tripleChance = 1f;
            }
            else
            {
                int i = _currentLevel - 1; // 0-based index into the per-level curve tables
                fallStep = GameplayConfig.FALL_STEP_BY_LEVEL[i];
                ingredientCount = GameplayConfig.INGREDIENT_COUNT_BY_LEVEL[i];
                tripleChance = GameplayConfig.TRIPLE_CHANCE_BY_LEVEL[i];
            }

            _spawner.SetFallSpeed(fallStep);
            _spawner.SetActiveIngredientCount(ingredientCount);
            _spawner.SetTripleWaveChance(tripleChance);
        }
    }
}
