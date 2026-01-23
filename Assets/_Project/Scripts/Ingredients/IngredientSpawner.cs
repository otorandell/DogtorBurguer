using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class IngredientSpawner : MonoBehaviour
    {
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
        [SerializeField] private float _spawnInterval = Constants.SPAWN_INTERVAL_INITIAL;
        [SerializeField] private float _fallStepDuration = Constants.INITIAL_FALL_STEP_DURATION;
        [SerializeField] private int _activeIngredientCount = Constants.STARTING_INGREDIENT_COUNT;
        [SerializeField] private float _bunSpawnChance = 0.15f;

        [Header("Forced Bun Spawn")]
        [SerializeField] private bool _enableForcedBunSpawn = true;
        [SerializeField] private int _forceBunMultiplier = 2;

        [Header("Double Spawn")]
        [SerializeField] private float _doubleSpawnMinLevel = 4f;
        [SerializeField] private float _doubleSpawnMaxChance = 0.3f;
        [SerializeField] private float _doubleSpawnDelay = 0.4f;

        [Header("Preview")]
        [SerializeField] private float _previewDuration = 0.8f;
        [SerializeField] private int _previewBlinks = 3;

        private float _spawnTimer;
        private bool _isSpawning = true;
        private Dictionary<IngredientType, Sprite> _spriteMap;
        private int _spawnCount;
        private int _spawnsSinceLastBun;
        private int _currentLevel = 1;

        private void Awake()
        {
            BuildSpriteMap();
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

        private void Start()
        {
            _spawnTimer = _spawnInterval;
            Debug.Log($"[Spawner] Started. Interval: {_spawnInterval}s, Spawning: {_isSpawning}");
        }

        private void Update()
        {
            if (!_isSpawning) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0)
            {
                StartCoroutine(SpawnWithPreview());
                _spawnTimer = _spawnInterval;
            }
        }

        public void SetCurrentLevel(int level)
        {
            _currentLevel = level;
        }

        public void StartSpawning()
        {
            _isSpawning = true;
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = Mathf.Max(interval, Constants.SPAWN_INTERVAL_MIN);
        }

        public void SetFallSpeed(float stepDuration)
        {
            _fallStepDuration = Mathf.Max(stepDuration, Constants.MIN_FALL_STEP_DURATION);
        }

        public void SetActiveIngredientCount(int count)
        {
            _activeIngredientCount = Mathf.Clamp(count, 1, 7);
        }

        private IEnumerator SpawnWithPreview()
        {
            _spawnCount++;

            if (GridManager.Instance == null) yield break;

            int columnIndex;
            IngredientType type;

            // Test burger column mode: all spawns on rightmost column
            if (GameManager.Instance != null && GameManager.Instance.TestBurgerColumn)
            {
                columnIndex = Constants.COLUMN_COUNT - 1;
                Column testCol = GridManager.Instance.GetColumn(columnIndex);
                if (testCol == null || testCol.IsOverflowing) yield break;

                if (_spawnCount == 1)
                    type = IngredientType.BunBottom;
                else if (_spawnCount >= Constants.MAX_ROWS)
                    type = IngredientType.BunTop;
                else
                    type = (_spawnCount % 2 == 0) ? IngredientType.Meat : IngredientType.Cheese;

                GameObject testPreview = CreatePreview(type, testCol);
                if (testPreview != null)
                {
                    yield return StartCoroutine(BlinkPreview(testPreview));
                    Destroy(testPreview);
                }
                if (!_isSpawning) yield break;
                SpawnIngredient(type, testCol);
                yield break;
            }

            columnIndex = Random.Range(0, Constants.COLUMN_COUNT);
            Column column = GridManager.Instance.GetColumn(columnIndex);
            if (column == null || column.IsOverflowing) yield break;

            type = GetSpawnType();

            // Test settings override
            if (GameManager.Instance != null && GameManager.Instance.TestSettings)
            {
                if (_spawnCount == 1) type = IngredientType.BunBottom;
                else if (_spawnCount == 8) type = IngredientType.BunTop;
            }

            // Show blinking preview
            GameObject preview = CreatePreview(type, column);
            if (preview != null)
            {
                yield return StartCoroutine(BlinkPreview(preview));
                Destroy(preview);
            }

            if (!_isSpawning) yield break;

            SpawnIngredient(type, column);

            // Double spawn chance at higher levels
            if (ShouldDoubleSpawn())
            {
                yield return new WaitForSeconds(_doubleSpawnDelay);
                if (!_isSpawning) yield break;

                int col2 = Random.Range(0, Constants.COLUMN_COUNT);
                Column column2 = GridManager.Instance.GetColumn(col2);
                if (column2 != null && !column2.IsOverflowing)
                {
                    IngredientType type2 = GetSpawnType();
                    SpawnIngredient(type2, column2);
                }
            }
        }

        private IngredientType GetSpawnType()
        {
            // Force bun spawn if threshold reached
            if (_enableForcedBunSpawn)
            {
                int threshold = _activeIngredientCount * _forceBunMultiplier;
                if (_spawnsSinceLastBun >= threshold)
                {
                    _spawnsSinceLastBun = 0;
                    // Force bottom bun if none on grid, otherwise either
                    if (!GridHasBottomBun())
                        return IngredientType.BunBottom;
                    return Random.value < 0.5f ? IngredientType.BunBottom : IngredientType.BunTop;
                }
            }

            // Chance to spawn a bun
            if (Random.value < _bunSpawnChance)
            {
                IngredientType bunType = Random.value < 0.5f ? IngredientType.BunBottom : IngredientType.BunTop;

                // Top buns cannot spawn if no bottom buns on grid
                if (bunType == IngredientType.BunTop && !GridHasBottomBun())
                    bunType = IngredientType.BunBottom;

                _spawnsSinceLastBun = 0;
                return bunType;
            }

            _spawnsSinceLastBun++;

            // Spawn regular ingredient from active pool
            int typeIndex = Random.Range(0, _activeIngredientCount);
            return (IngredientType)typeIndex;
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

        private bool ShouldDoubleSpawn()
        {
            if (_currentLevel < _doubleSpawnMinLevel) return false;
            float t = (_currentLevel - _doubleSpawnMinLevel) / (Constants.MAX_LEVEL - _doubleSpawnMinLevel);
            float chance = Mathf.Lerp(0f, _doubleSpawnMaxChance, t);
            return Random.value < chance;
        }

        private GameObject CreatePreview(IngredientType type, Column column)
        {
            Sprite sprite = null;
            _spriteMap?.TryGetValue(type, out sprite);
            if (sprite == null) return null;

            GameObject preview = new GameObject("SpawnPreview");
            // Position at top of the column (just above the grid area)
            float x = Constants.GRID_ORIGIN_X + (column.ColumnIndex * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y + (Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT);
            preview.transform.position = new Vector3(x, y, 0);

            SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 90;
            sr.color = new Color(1f, 1f, 1f, 0.5f);

            return preview;
        }

        private IEnumerator BlinkPreview(GameObject preview)
        {
            if (preview == null) yield break;

            SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            float blinkInterval = _previewDuration / (_previewBlinks * 2f);

            for (int i = 0; i < _previewBlinks; i++)
            {
                sr.enabled = true;
                yield return new WaitForSeconds(blinkInterval);
                sr.enabled = false;
                yield return new WaitForSeconds(blinkInterval);
            }
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

            // Get sprite
            Sprite sprite = null;
            _spriteMap?.TryGetValue(type, out sprite);

            // Initialize and start falling
            ingredient.Initialize(type, column, sprite);
            ingredient.StartFalling(_fallStepDuration);

            return ingredient;
        }

        /// <summary>
        /// Spawns a specific ingredient for testing
        /// </summary>
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
