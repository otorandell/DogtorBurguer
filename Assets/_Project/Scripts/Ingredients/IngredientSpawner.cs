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

        [Header("Wave Settings")]
        [SerializeField] private float _initialDelay = GameplayConfig.INITIAL_SPAWN_DELAY;

        private bool _active;
        private SpawnerState _state = SpawnerState.Delaying;
        private float _tripleWaveChance;
        private int _previewTarget; // how many previews to keep shown for the current wave

        // Wave state
        private List<Ingredient> _currentWaveIngredients = new();
        private float _delayTimer;

        private WavePreviewManager _previewManager;
        private WaveComposer _composer;
        private IngredientRoster _roster;

        // Reused per-frame scratch buffers to avoid per-frame allocations.
        private readonly List<int> _eligibleColumns = new();
        private readonly HashSet<int> _previewZoneBlocked = new();

        private void Awake()
        {
            _previewManager = gameObject.AddComponent<WavePreviewManager>();
            _previewManager.Initialize(GetSpriteForType);
            _roster = new IngredientRoster(); // this run's random unlock order
            _composer = new WaveComposer(_roster);
        }

        private void Update()
        {
            if (!_active || TutorialMode.IsActive) return; // belt and braces for the tutorial

            switch (_state)
            {
                case SpawnerState.Delaying:
                    _delayTimer -= Time.deltaTime;
                    if (_delayTimer <= 0)
                        SpawnNextWave();
                    break;

                case SpawnerState.WaveFalling:
                    // Previews are a standing lookahead queue: continuously refilled into random columns
                    // (Change A), independent of the wave. The next wave fires when the ORIGINAL wave
                    // lands — which the player can't stall — force-dropping any held preview.
                    TopUpPreviews();
                    // Purely visual: reveal each reserved ghost once its column's spawn zone is clear.
                    _previewManager.RevealCleared(ColumnsWithPieceInPreviewZone());
                    if (AllCurrentWaveLanded())
                    {
                        // Wave grace — ALWAYS a flat beat between a wave landing and the next
                        // one (Oscar, 2026-09-08), announced by the previews' fast blink.
                        _state = SpawnerState.Delaying;
                        _delayTimer = GameplayConfig.SPAWN_GRACE_SECONDS;
                        _previewManager.SetUrgent(true); // fast blink: incoming!
                    }
                    break;
            }
        }

        public void SetTripleWaveChance(float chance)
        {
            _tripleWaveChance = chance;
        }

        public void StartSpawning()
        {
            // Tutorial runs on scripted spawns only — no waves, no previews. ShouldRun covers the
            // same-frame ordering: GameManager.Start calls this before TutorialManager.Start has
            // flipped IsActive.
            if (TutorialMode.IsActive || TutorialMode.ShouldRun) return;

            _active = true;
            _state = SpawnerState.Delaying;
            _delayTimer = _initialDelay;
            _currentWaveIngredients.Clear();
            _previewManager.ClearPreviews();
            // Seed the first wave's queue now. It stays hidden until WaveFalling reveals previews, so
            // nothing shows during the initial delay; SpawnNextWave then consumes it like any other wave.
            _previewTarget = _composer.RollWaveSize(_tripleWaveChance);
            TopUpPreviews();
        }

        public void StopSpawning()
        {
            _active = false;
        }

        public void ResumeSpawning()
        {
            // Same gate as StartSpawning: closing an overlay (settings/shop/help) resumes the
            // game, and this un-gated _active=true started REAL waves mid-tutorial (2026-09-07).
            if (TutorialMode.IsActive) return;
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

        /// <summary>This run's ingredient at unlock position <paramref name="index"/> (the
        /// per-run random roster — see IngredientRoster).</summary>
        public IngredientType ActiveTypeAt(int index) => _roster.At(index);

        private void SpawnNextWave()
        {
            if (GridManager.Instance == null) return;
            _previewManager.SetUrgent(false);

            // The standing preview queue IS the wave (seeded in StartSpawning, refilled every frame).
            // ConsumeRemainingData also clears the previews.
            List<WaveSlot> waveData = _previewManager.ConsumeRemainingData();

            _currentWaveIngredients.Clear();
            foreach (WaveSlot slot in waveData)
            {
                Column col = GridManager.Instance.GetColumn(slot.ColumnIndex);
                if (col == null || col.IsOverflowing) continue;
                Ingredient ing = SpawnIngredient(slot.Type, col);
                if (ing != null)
                    _currentWaveIngredients.Add(ing);
            }

            // How many previews to keep shown for the upcoming wave — this becomes its size.
            _previewTarget = _composer.RollWaveSize(_tripleWaveChance);
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

        public bool TryTapPreview(Vector2 worldPos)
        {
            WaveSlot? result = _previewManager.TryTap(worldPos);
            if (result == null) return false;

            WaveSlot slot = result.Value;
            Column col = GridManager.Instance?.GetColumn(slot.ColumnIndex);
            if (col != null && !col.IsOverflowing)
            {
                // Tapped previews are "fire and forget": deliberately NOT added to the wave-completion
                // list. Otherwise a player could hold a preview forever by tapping the other refills to
                // keep a piece always falling, never letting the gate close. The next wave still fires
                // when the ORIGINAL wave lands (which can't be stalled), force-dropping any held preview.
                // The queue refills via Update's TopUpPreviews on the next frame.
                SpawnIngredient(slot.Type, col);
            }
            return true;
        }

        /// <summary>
        /// Refills the preview queue up to <see cref="_previewTarget"/>. Each new preview goes into a
        /// column that doesn't already have one; placement is otherwise unbiased (see PickPreviewColumn).
        /// </summary>
        private void TopUpPreviews()
        {
            while (_previewManager.Count < _previewTarget)
            {
                int column = PickPreviewColumn();
                if (column < 0) break; // every column already has a preview
                if (!_previewManager.AddPreview(_composer.RollSlot(_activeIngredientCount, column)))
                    break; // preview couldn't be created (e.g. missing sprite) — don't spin
            }
        }

        private int PickPreviewColumn()
        {
            // Only constraint: one preview per column, so a wave never spawns two pieces into one column
            // at once. Height and falling pieces are deliberately ignored — placement stays unbiased (a
            // busy column is still eligible). Clearance only delays the GHOST visual, handled separately
            // by ColumnsWithPieceInPreviewZone + WavePreviewManager.RevealCleared.
            _eligibleColumns.Clear();
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                if (!_previewManager.HasPreviewInColumn(c))
                    _eligibleColumns.Add(c);
            }

            if (_eligibleColumns.Count == 0) return -1;
            return _eligibleColumns[Rng.Range(0, _eligibleColumns.Count)];
        }

        /// <summary>
        /// Columns that currently have a piece falling through the preview's footprint (just below the
        /// spawn line). Used ONLY to delay the ghost's visual reveal — never to choose wave columns.
        /// </summary>
        private HashSet<int> ColumnsWithPieceInPreviewZone()
        {
            _previewZoneBlocked.Clear();
            if (GridManager.Instance == null) return _previewZoneBlocked;

            float spawnY = Constants.GRID_ORIGIN_Y + (Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT)
                           - Constants.PREVIEW_Y_OFFSET; // the ghost row — keep in step with Column.GetSpawnPosition
            float clearedBelowY = spawnY - AnimConfig.PREVIEW_SPAWN_CLEARANCE;
            foreach (Ingredient falling in GridManager.Instance.GetFallingIngredients())
            {
                if (falling != null && falling.CurrentColumn != null && falling.CurrentY > clearedBelowY)
                    _previewZoneBlocked.Add(falling.CurrentColumn.ColumnIndex);
            }
            return _previewZoneBlocked;
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

        public Ingredient SpawnIngredient(IngredientType type, Column column, float? stepDuration = null)
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
            ingredient.StartFalling(stepDuration ?? _fallStepDuration);

            return ingredient;
        }

        /// <summary>Tutorial: one piece into one column at a scripted pace (the auto spawner is
        /// standing down, so these are the only pieces on the board).</summary>
        public Ingredient SpawnScripted(IngredientType type, int columnIndex, float stepDuration)
        {
            Column column = GridManager.Instance?.GetColumn(columnIndex);
            return column != null ? SpawnIngredient(type, column, stepDuration) : null;
        }

        public void SpawnSpecificIngredient(IngredientType type, int columnIndex)
        {
            Column column = GridManager.Instance?.GetColumn(columnIndex);
            if (column != null)
            {
                SpawnIngredient(type, column);
            }
        }

#if UNITY_EDITOR
        // Debug gizmo: the fast-drop tap radius around each falling piece. Mirrors
        // TryTapFallingIngredient exactly (same source, filter, and radius).
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || GridManager.Instance == null) return;

            float radius = Constants.CELL_WIDTH * GameplayConfig.FALLING_TAP_RADIUS_MULT;
            Gizmos.color = GizmoStyles.FallingTap;
            foreach (var ingredient in GridManager.Instance.GetFallingIngredients())
            {
                if (ingredient == null || ingredient.State == IngredientState.Landed) continue;
                Gizmos.DrawWireSphere(ingredient.transform.position, radius);
            }
        }
#endif
    }
}
