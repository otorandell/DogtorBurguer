using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private int _activeIngredientCount = 4;
        [SerializeField] private float _bunSpawnChance = 0.15f;

        private float _spawnTimer;
        private bool _isSpawning = true;
        private Dictionary<IngredientType, Sprite> _spriteMap;

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
                SpawnRandomIngredient();
                _spawnTimer = _spawnInterval;
            }
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

        private void SpawnRandomIngredient()
        {
            // Pick random column
            int columnIndex = Random.Range(0, Constants.COLUMN_COUNT);

            if (GridManager.Instance == null)
            {
                Debug.LogError("[Spawner] GridManager.Instance is null!");
                return;
            }

            Column column = GridManager.Instance.GetColumn(columnIndex);

            if (column == null)
            {
                Debug.LogError($"[Spawner] Column {columnIndex} is null!");
                return;
            }

            if (column.IsOverflowing)
            {
                Debug.Log($"[Spawner] Column {columnIndex} is overflowing, skipping");
                return;
            }

            // Decide type
            IngredientType type = GetRandomIngredientType();

            Debug.Log($"[Spawner] Spawning {type} in column {columnIndex}");
            SpawnIngredient(type, column);
        }

        private IngredientType GetRandomIngredientType()
        {
            // Chance to spawn a bun
            if (Random.value < _bunSpawnChance)
            {
                // Alternate between bottom and top buns for variety
                return Random.value < 0.5f ? IngredientType.BunBottom : IngredientType.BunTop;
            }

            // Spawn regular ingredient from active pool
            int typeIndex = Random.Range(0, _activeIngredientCount);
            return (IngredientType)typeIndex;
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
