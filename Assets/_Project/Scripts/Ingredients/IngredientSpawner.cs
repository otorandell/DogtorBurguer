using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class IngredientSpawner : MonoBehaviour
    {
        private enum SpawnerState { Idle, Delaying, WaveFalling, WaitingForLand }

        [Header("Prefabs")]
        [SerializeField] private GameObject _ingredientPrefab;

        [Header("Sprites")]
        [SerializeField] private Sprite _spriteMeat;
        [SerializeField] private Sprite _spriteCheese;
        [SerializeField] private Sprite _spriteTomato;
        [SerializeField] private Sprite _spriteOnion;
        [SerializeField] private Sprite _spritePickle;
        [SerializeField] private Sprite _spriteLettuce;
        [SerializeField] private Sprite _spriteEgg;
        [SerializeField] private Sprite _spriteBunBottom;
        [SerializeField] private Sprite _spriteBunTop;

        [Header("Settings")]
        [SerializeField] private float _fallStepDuration = Constants.INITIAL_FALL_STEP_DURATION;
        [SerializeField] private int _activeIngredientCount = Constants.STARTING_INGREDIENT_COUNT;

        [Header("Forced Bun Spawn")]
        [SerializeField] private bool _enableForcedBunSpawn = true;
        [SerializeField] private float _forceBunMultiplier = GameplayConfig.FORCED_BUN_MULTIPLIER;

        [Header("Wave Settings")]
        [SerializeField] private float _initialDelay = GameplayConfig.INITIAL_SPAWN_DELAY;

        private bool _active;
        private SpawnerState _state = SpawnerState.Idle;
        private Dictionary<IngredientType, Sprite> _spriteMap;
        private int _spawnsSinceLastBun;
        private int _currentLevel = 1;

        // Wave state
        private List<Ingredient> _currentWaveIngredients = new();
        private List<(IngredientType type, int columnIndex)> _nextWaveData = new();
        private float _delayTimer;

        private WavePreviewManager _previewManager;

        private void Awake()
        {
            BuildSpriteMap();
            _previewManager = gameObject.AddComponent<WavePreviewManager>();
            _previewManager.Initialize(GetSpriteForType);
        }

        private void BuildSpriteMap()
        {
            _spriteMap = new Dictionary<IngredientType, Sprite>
            {
                { IngredientType.Meat, _spriteMeat },
                { IngredientType.Cheese, _spriteCheese },
                { IngredientType.Tomato, _spriteTomato },
                { IngredientType.Onion, _spriteOnion },
                { IngredientType.Pickle, _spritePickle },
                { IngredientType.Lettuce, _spriteLettuce },
                { IngredientType.Egg, _spriteEgg },
                { IngredientType.BunBottom, _spriteBunBottom },
                { IngredientType.BunTop, _spriteBunTop }
            };
        }

        private void Update()
        {
            if (!_active) return;

            switch (_state)
            {
                case SpawnerState.Idle:
                    break;

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

        public void SetCurrentLevel(int level)
        {
            _currentLevel = level;
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
            _fallStepDuration = Mathf.Max(stepDuration, Constants.MIN_FALL_STEP_DURATION);
        }

        public void SetActiveIngredientCount(int count)
        {
            _activeIngredientCount = Mathf.Clamp(count, 1, 7);
        }

        public int ActiveIngredientCount => _activeIngredientCount;

        public Sprite GetSpriteForType(IngredientType type)
        {
            if (_spriteMap != null && _spriteMap.TryGetValue(type, out Sprite sprite))
                return sprite;
            return null;
        }

        private void SpawnNextWave()
        {
            if (GridManager.Instance == null) return;

            // Consume remaining preview data (entries may have been tapped)
            var waveData = _previewManager.HasPreviews
                ? _previewManager.ConsumeRemainingData()
                : (_nextWaveData.Count > 0 ? _nextWaveData : RollWaveData());

            _previewManager.ClearPreviews();

            _currentWaveIngredients.Clear();
            foreach (var (type, colIdx) in waveData)
            {
                Column col = GridManager.Instance.GetColumn(colIdx);
                if (col == null || col.IsOverflowing) continue;
                Ingredient ing = SpawnIngredient(type, col);
                if (ing != null)
                    _currentWaveIngredients.Add(ing);
            }

            // Pre-roll next wave (previews shown once wave clears top)
            _nextWaveData = RollWaveData();
            _state = SpawnerState.WaveFalling;
        }

        private bool AllCurrentWaveLanded()
        {
            foreach (var ing in _currentWaveIngredients)
            {
                if (ing == null) continue; // destroyed
                if (!ing.IsLanded) return false;
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

        private List<(IngredientType type, int columnIndex)> RollWaveData()
        {
            int waveSize = GetWaveSize();
            var data = new List<(IngredientType, int)>();
            var usedColumns = new List<int>();

            for (int i = 0; i < waveSize; i++)
            {
                int col = GetUnusedColumn(usedColumns);
                if (col < 0) break;
                usedColumns.Add(col);
                IngredientType type = GetSpawnType();
                data.Add((type, col));
            }
            return data;
        }

        private int GetWaveSize()
        {
            if (_currentLevel >= GameplayConfig.TRIPLE_WAVE_START_LEVEL)
            {
                float tripleT = (_currentLevel - (float)GameplayConfig.TRIPLE_WAVE_START_LEVEL) / (Constants.MAX_LEVEL - (float)GameplayConfig.TRIPLE_WAVE_START_LEVEL);
                float tripleChance = tripleT * GameplayConfig.TRIPLE_WAVE_MAX_CHANCE;
                if (Rng.Value < tripleChance) return 3;
            }
            return 2;
        }

        private int GetUnusedColumn(List<int> usedColumns)
        {
            List<int> available = new List<int>();
            for (int i = 0; i < Constants.COLUMN_COUNT; i++)
            {
                if (!usedColumns.Contains(i))
                    available.Add(i);
            }
            if (available.Count == 0) return -1;
            return available[Rng.Range(0, available.Count)];
        }

        private IngredientType GetSpawnType()
        {
            if (_enableForcedBunSpawn)
            {
                int threshold = (int)(_activeIngredientCount * _forceBunMultiplier);
                if (_spawnsSinceLastBun >= threshold)
                {
                    _spawnsSinceLastBun = 0;
                    return GetBunType();
                }
            }

            int roll = Rng.Range(0, _activeIngredientCount + 1);
            if (roll < _activeIngredientCount)
            {
                _spawnsSinceLastBun++;
                return (IngredientType)roll;
            }

            _spawnsSinceLastBun = 0;
            return GetBunType();
        }

        private IngredientType GetBunType()
        {
            if (!GridHasBottomBun())
                return IngredientType.BunBottom;

            int bottomCount = CountBottomBunsOnGrid();
            float topChance = Mathf.Min(GameplayConfig.BUN_TOP_BASE_CHANCE + bottomCount * GameplayConfig.BUN_TOP_CHANCE_PER_BOTTOM, GameplayConfig.BUN_TOP_CHANCE_CAP);
            return Rng.Value < topChance ? IngredientType.BunTop : IngredientType.BunBottom;
        }

        private int CountBottomBunsOnGrid()
        {
            if (GridManager.Instance == null) return 0;

            int count = 0;
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance.GetColumn(c);
                if (col == null) continue;

                foreach (var ing in col.GetAllIngredients())
                {
                    if (ing.Type == IngredientType.BunBottom)
                        count++;
                }
            }
            return count;
        }

        private bool GridHasBottomBun()
        {
            if (GridManager.Instance == null) return false;

            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance.GetColumn(c);
                if (col == null) continue;

                var ingredients = col.GetAllIngredients();
                foreach (var ing in ingredients)
                {
                    if (ing.Type == IngredientType.BunBottom)
                        return true;
                }
            }
            return false;
        }

        public bool TryTapPreview(Vector2 worldPos)
        {
            var result = _previewManager.TryTap(worldPos);
            if (result == null) return false;

            var (type, colIdx) = result.Value;
            Column col = GridManager.Instance?.GetColumn(colIdx);
            if (col != null && !col.IsOverflowing)
            {
                Ingredient ing = SpawnIngredient(type, col);
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
                if (ingredient == null || ingredient.IsLanded) continue;

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

            Sprite sprite = null;
            _spriteMap?.TryGetValue(type, out sprite);

            ingredient.Initialize(type, column, sprite);
            ingredient.StartFalling(_fallStepDuration);

            return ingredient;
        }

        private void OnDestroy()
        {
            _previewManager?.ClearPreviews();
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
