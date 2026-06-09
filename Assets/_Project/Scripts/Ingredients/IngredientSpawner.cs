using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class IngredientSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _ingredientPrefab;

        [Header("Settings")]
        [SerializeField] private float _fallStepDuration = GameplayConfig.INITIAL_FALL_STEP_DURATION;
        [SerializeField] private int _activeIngredientCount = GameplayConfig.STARTING_INGREDIENT_COUNT;

        [Header("Forced Bun Spawn")]
        [SerializeField] private bool _enableForcedBunSpawn = true;
        [SerializeField] private float _forceBunMultiplier = GameplayConfig.FORCED_BUN_MULTIPLIER;

        [Header("Wave Settings")]
        [SerializeField] private float _initialDelay = GameplayConfig.INITIAL_SPAWN_DELAY;

        private bool _active;
        private SpawnerState _state = SpawnerState.Delaying;
        private float _tripleWaveChance;

        // Wave state
        private List<Ingredient> _currentWaveIngredients = new();
        private List<WaveSlot> _nextWaveData = new();
        private float _delayTimer;

        private WavePreviewManager _previewManager;
        private WaveComposer _composer;

        private void Awake()
        {
            _previewManager = gameObject.AddComponent<WavePreviewManager>();
            _previewManager.Initialize(GetSpriteForType);
            _composer = new WaveComposer(_enableForcedBunSpawn, _forceBunMultiplier);
        }

        private void Update()
        {
            if (!_active) return;

            switch (_state)
            {
                case SpawnerState.Delaying:
                    _delayTimer -= Time.deltaTime;
                    if (_delayTimer <= 0)
                        SpawnNextWave();
                    break;

                case SpawnerState.WaveFalling:
                    if (WaveClearedTop())
                    {
                        _previewManager.ShowPreviews(_nextWaveData);
                        _nextWaveData.Clear();
                        _state = SpawnerState.WaitingForLand;
                    }
                    break;

                case SpawnerState.WaitingForLand:
                    if (AllCurrentWaveLanded())
                        SpawnNextWave();
                    break;
            }
        }

        public void SetTripleWaveChance(float chance)
        {
            _tripleWaveChance = chance;
        }

        public void StartSpawning()
        {
            _active = true;
            _state = SpawnerState.Delaying;
            _delayTimer = _initialDelay;
            _currentWaveIngredients.Clear();
            _nextWaveData.Clear();
            _previewManager.ClearPreviews();
        }

        public void StopSpawning()
        {
            _active = false;
        }

        public void ResumeSpawning()
        {
            _active = true;
        }

        public void SetFallSpeed(float stepDuration)
        {
            _fallStepDuration = Mathf.Max(stepDuration, GameplayConfig.MIN_FALL_STEP_DURATION);
        }

        public void SetActiveIngredientCount(int count)
        {
            _activeIngredientCount = Mathf.Clamp(count, 1, GameplayConfig.REGULAR_INGREDIENTS.Length);
        }

        public int ActiveIngredientCount => _activeIngredientCount;

        public Sprite GetSpriteForType(IngredientType type) => Theme.Ingredient(type);

        private void SpawnNextWave()
        {
            if (GridManager.Instance == null) return;

            // Consume remaining preview data (entries may have been tapped)
            var waveData = _previewManager.HasPreviews
                ? _previewManager.ConsumeRemainingData()
                : (_nextWaveData.Count > 0 ? _nextWaveData : _composer.RollWave(_activeIngredientCount, _tripleWaveChance));

            _previewManager.ClearPreviews();

            _currentWaveIngredients.Clear();
            foreach (WaveSlot slot in waveData)
            {
                Column col = GridManager.Instance.GetColumn(slot.ColumnIndex);
                if (col == null || col.IsOverflowing) continue;
                Ingredient ing = SpawnIngredient(slot.Type, col);
                if (ing != null)
                    _currentWaveIngredients.Add(ing);
            }

            // Pre-roll next wave (previews shown once wave clears top)
            _nextWaveData = _composer.RollWave(_activeIngredientCount, _tripleWaveChance);
            _state = SpawnerState.WaveFalling;
        }

        private bool AllCurrentWaveLanded()
        {
            foreach (var ing in _currentWaveIngredients)
            {
                if (ing == null) continue; // destroyed
                if (ing.State != IngredientState.Landed) return false;
            }
            return true;
        }

        private bool WaveClearedTop()
        {
            float threshold = Constants.GRID_ORIGIN_Y + ((Constants.MAX_ROWS - 1) * Constants.CELL_VISUAL_HEIGHT);
            foreach (var ing in _currentWaveIngredients)
            {
                if (ing == null) continue;
                if (ing.CurrentY > threshold) return false;
            }
            return true;
        }

        public bool TryTapPreview(Vector2 worldPos)
        {
            WaveSlot? result = _previewManager.TryTap(worldPos);
            if (result == null) return false;

            WaveSlot slot = result.Value;
            Column col = GridManager.Instance?.GetColumn(slot.ColumnIndex);
            if (col != null && !col.IsOverflowing)
            {
                Ingredient ing = SpawnIngredient(slot.Type, col);
                if (ing != null)
                    _currentWaveIngredients.Add(ing);
            }
            return true;
        }

        public bool TryTapFallingIngredient(Vector2 worldPos)
        {
            if (GridManager.Instance == null) return false;

            foreach (var ingredient in GridManager.Instance.GetFallingIngredients())
            {
                if (ingredient == null || ingredient.State == IngredientState.Landed) continue;

                float dist = Vector2.Distance(worldPos, ingredient.transform.position);
                if (dist < Constants.CELL_WIDTH * GameplayConfig.FALLING_TAP_RADIUS_MULT)
                {
                    ingredient.FastDrop();
                    AudioManager.Instance?.PlayFastDrop();
                    return true;
                }
            }
            return false;
        }

        public Ingredient SpawnIngredient(IngredientType type, Column column)
        {
            if (_ingredientPrefab == null)
            {
                Debug.LogError("Ingredient prefab not assigned!");
                return null;
            }

            GameObject obj = Instantiate(_ingredientPrefab, transform);
            Ingredient ingredient = obj.GetComponent<Ingredient>();

            if (ingredient == null)
            {
                ingredient = obj.AddComponent<Ingredient>();
            }

            ingredient.Initialize(type, column, Theme.Ingredient(type));
            ingredient.StartFalling(_fallStepDuration);

            return ingredient;
        }

        public void SpawnSpecificIngredient(IngredientType type, int columnIndex)
        {
            Column column = GridManager.Instance?.GetColumn(columnIndex);
            if (column != null)
            {
                SpawnIngredient(type, column);
            }
        }
    }
}
