using System;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class GridManager : Singleton<GridManager>
    {
        [SerializeField] private Column[] _columns;

        private List<Ingredient> _fallingIngredients = new List<Ingredient>();
        private BurgerAnimator _burgerAnimator;
        private SwapAnimator _swapAnimator;

        public event Action OnGameOver;
        public event Action<int> OnMatchEliminated;         // Points earned
        public event Action<int, string> OnBurgerCompleted; // Points earned, burger name
        public event Action<Vector3, int> OnMatchEffect;    // Position, points
        public event Action<Vector3, int, string, int> OnBurgerEffect; // Position, points, name, ingredientCount
        public event Action<Vector3, int, string, int, List<IngredientType>> OnBurgerWithIngredients;
        public event Action OnIngredientPlaced;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            _burgerAnimator = gameObject.AddComponent<BurgerAnimator>();
            _swapAnimator = gameObject.AddComponent<SwapAnimator>();
            _swapAnimator.OnSwapComplete += HandleSwapComplete;
            InitializeColumns();
        }

        private void InitializeColumns()
        {
            if (_columns == null || _columns.Length == 0)
            {
                // Create columns dynamically if not assigned
                _columns = new Column[Constants.COLUMN_COUNT];
                for (int i = 0; i < Constants.COLUMN_COUNT; i++)
                {
                    GameObject columnObj = new GameObject($"Column_{i}");
                    columnObj.transform.SetParent(transform);
                    Column column = columnObj.AddComponent<Column>();
                    column.Initialize(i);
                    _columns[i] = column;
                }
            }
            else
            {
                for (int i = 0; i < _columns.Length; i++)
                {
                    _columns[i].Initialize(i);
                }
            }
        }

        public Column GetColumn(int index)
        {
            if (index < 0 || index >= _columns.Length) return null;
            return _columns[index];
        }

        public List<Ingredient> GetFallingIngredients()
        {
            return _fallingIngredients;
        }

        public void RegisterFallingIngredient(Ingredient ingredient)
        {
            if (!_fallingIngredients.Contains(ingredient))
            {
                _fallingIngredients.Add(ingredient);
            }
        }

        public void UnregisterFallingIngredient(Ingredient ingredient)
        {
            _fallingIngredients.Remove(ingredient);
        }

        public void OnIngredientLanded(Ingredient ingredient)
        {
            Column column = ingredient.CurrentColumn;

            // Top bun: check burger before overflow
            if (ingredient.Type == IngredientType.BunTop)
            {
                int topIndex = column.StackHeight - 1; // the BunTop we just landed
                int bottomIndex = FindBunBottomBelow(column, topIndex);
                if (bottomIndex >= 0)
                {
                    OnIngredientPlaced?.Invoke();
                    ProcessBurger(column, topIndex, bottomIndex);
                }
                else
                {
                    // No bottom bun below — destroy the lone top bun.
                    Vector3 pos = ingredient.transform.position;
                    column.RemoveIngredient(ingredient);
                    ingredient.DestroyWithFlash();
                    FloatingText.Spawn(pos, "Too bad!", UIStyles.TEXT_TOO_BAD, UIStyles.WORLD_FLOATING_TEXT_SIZE);
                }
                return;
            }

            OnIngredientPlaced?.Invoke();

            // Process matches BEFORE checking overflow — a landing that completes a
            // match can clear the column and "save" the player from a momentary overflow.
            CheckAndProcessMatches(column); // includes bottom bun cancellation; may reduce height

            // Now check overflow (game over condition). A regular landing can't complete a
            // burger — that only happens when a BunTop lands (handled above) — so no burger
            // check here.
            if (column.IsOverflowing)
            {
                OnGameOver?.Invoke();
                return;
            }
        }

        private void CheckAndProcessMatches(Column column)
        {
            while (MatchDetector.TryProcessMatch(column, out var result))
            {
                if (result.IsBunMatch)
                {
                    FloatingText.Spawn(result.EffectPosition, "Too bad!", UIStyles.TEXT_TOO_BAD, UIStyles.WORLD_FLOATING_TEXT_SIZE);
                }
                else
                {
                    OnMatchEliminated?.Invoke(GameplayConfig.POINTS_MATCH);
                    OnMatchEffect?.Invoke(result.EffectPosition, GameplayConfig.POINTS_MATCH);
                }
            }
        }

        /// <summary>Scans down from a just-landed BunTop for the nearest BunBottom; -1 if none.</summary>
        private int FindBunBottomBelow(Column column, int topIndex)
        {
            var ingredients = column.GetAllIngredients();
            for (int i = topIndex - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunBottom)
                    return i;
            }
            return -1;
        }

        private void ProcessBurger(Column column, int topIndex, int bottomIndex)
        {
            var ingredients = column.GetAllIngredients();

            var parts = new List<Ingredient>();
            for (int i = topIndex; i >= bottomIndex; i--)
                parts.Add(ingredients[i]);

            var ingredientTypes = new List<IngredientType>();
            for (int i = bottomIndex + 1; i < topIndex; i++)
                ingredientTypes.Add(ingredients[i].Type);

            int ingredientCount = topIndex - bottomIndex - 1;

            // Remove burger parts from column data immediately so they can't be re-detected
            // or moved by a swap.
            column.RemoveIngredientsInRange(bottomIndex, topIndex);

            var data = new BurgerData
            {
                Column = column,
                Parts = parts,
                BunBottomIndex = bottomIndex,
                BunTopIndex = topIndex,
                IngredientCount = ingredientCount,
                IngredientTypes = ingredientTypes,
                Points = Scoring.CalculateBurgerPoints(ingredientCount),
                Name = BurgerNamer.Generate(ingredientCount)
            };

            _burgerAnimator.PlayCompress(data, _fallingIngredients, HandleBurgerAnimationComplete);
        }

        private void HandleBurgerAnimationComplete(BurgerData data, Vector3 pos)
        {
            // Collapse now that animation is done
            data.Column.CollapseFromRow(data.BunBottomIndex);

            // Let BurgerChallenge override display name if order matches
            string displayName = data.Name;
            if (BurgerChallenge.Instance != null &&
                BurgerChallenge.Instance.IsOrderMatch(data.IngredientTypes, data.IngredientCount))
                displayName = "Order Complete!";

            OnBurgerCompleted?.Invoke(data.Points, displayName);
            OnBurgerEffect?.Invoke(pos, data.Points, displayName, data.IngredientCount);
            OnBurgerWithIngredients?.Invoke(pos, data.Points, displayName, data.IngredientCount, data.IngredientTypes);
        }

        public void SwapColumnsWithWaveEffect(int columnA, int columnB)
        {
            Column colA = GetColumn(columnA);
            Column colB = GetColumn(columnB);

            if (colA == null || colB == null) return;

            // Falling ingredients below this level swap columns too.
            float thresholdY = Mathf.Max(
                colA.GetNextLandingPosition().y,
                colB.GetNextLandingPosition().y
            ) + (Constants.CELL_VISUAL_HEIGHT * GameplayConfig.SWAP_THRESHOLD_BUFFER_MULT);

            // Data swap (instant).
            List<Ingredient> ingredientsA = colA.TakeAllIngredients();
            List<Ingredient> ingredientsB = colB.TakeAllIngredients();
            colA.SetAllIngredients(ingredientsB);
            colB.SetAllIngredients(ingredientsA);

            // Reassign falling ingredients below the threshold to the other column.
            List<Ingredient> swappedFalling = new List<Ingredient>();
            foreach (var falling in new List<Ingredient>(_fallingIngredients))
            {
                if (falling == null || falling.State == IngredientState.Landed) continue;
                if (falling.CurrentY > thresholdY) continue;

                if (falling.CurrentColumn == colA)
                {
                    falling.SwapToColumn(colB, GameplayConfig.INITIAL_FALL_STEP_DURATION);
                    swappedFalling.Add(falling);
                }
                else if (falling.CurrentColumn == colB)
                {
                    falling.SwapToColumn(colA, GameplayConfig.INITIAL_FALL_STEP_DURATION);
                    swappedFalling.Add(falling);
                }
            }

            // Animate; match/burger checks run when the wave settles (HandleSwapComplete).
            _swapAnimator.PlaySwap(colA, colB, ingredientsA, ingredientsB, swappedFalling);
        }

        private void HandleSwapComplete(Column colA, Column colB)
        {
            // Only matches can result from a swap — a column never holds a sitting BunTop
            // (they resolve or self-destruct on landing), so no burger can form here.
            if (!colA.IsEmpty) CheckAndProcessMatches(colA);
            if (!colB.IsEmpty) CheckAndProcessMatches(colB);
        }

        /// <summary>
        /// Clears the top half of all columns (used for continue after game over)
        /// </summary>
        public void ClearTopHalf()
        {
            for (int c = 0; c < _columns.Length; c++)
            {
                Column col = _columns[c];
                int height = col.StackHeight;
                if (height <= 1) continue;

                int removeFrom = height / 2;
                // Destroy ingredients from top down
                for (int r = height - 1; r >= removeFrom; r--)
                {
                    Ingredient ingredient = col.GetIngredientAtRow(r);
                    if (ingredient != null)
                        ingredient.DestroyWithAnimation();
                }
                col.RemoveIngredientsInRange(removeFrom, height - 1);
            }

            // Also kill any currently falling ingredients
            foreach (Ingredient falling in new List<Ingredient>(_fallingIngredients))
            {
                if (falling != null)
                    falling.DestroyWithAnimation();
            }
            _fallingIngredients.Clear();
        }

        // ---- Consumable effects ----
        // Granular grid primitives the ConsumableEffect subclasses orchestrate. Each removes
        // ingredients, collapses survivors, and re-runs the standard match cascade. Non-bun
        // removals score POINTS_CONSUMABLE_PER_INGREDIENT (flat, no multiplier); buns score nothing.

        private static bool IsBun(IngredientType type) =>
            type == IngredientType.BunBottom || type == IngredientType.BunTop;

        /// <summary>Ketchup: removes the entire column. State clears at once; the flashes stagger
        /// top→bottom so pieces vanish as the squirt stream sweeps down them (ConsumableVfx).</summary>
        public void ConsumableClearColumn(Column column)
        {
            if (column == null || column.IsEmpty) return;

            Vector3 pos = column.GetWorldPositionForRow(column.StackHeight);
            List<Ingredient> all = column.TakeAllIngredients(); // index == row (bottom → top)

            int nonBun = 0;
            int topRow = all.Count - 1;
            for (int row = 0; row < all.Count; row++)
            {
                Ingredient ing = all[row];
                if (ing == null) continue;
                if (!IsBun(ing.Type)) nonBun++;
                ing.DestroyWithFlash(AnimConfig.KETCHUP_CLEAR_START_DELAY
                    + (topRow - row) * AnimConfig.KETCHUP_CLEAR_STAGGER);
            }
            AwardConsumablePoints(nonBun, pos);
        }

        /// <summary>Mustard: removes every ingredient of <paramref name="type"/> across all columns.</summary>
        public void ConsumableSweepType(IngredientType type, Column originColumn)
        {
            int nonBun = 0;
            foreach (Column col in _columns)
            {
                bool removedAny = false;
                foreach (Ingredient ing in col.GetAllIngredients())
                {
                    if (ing == null || ing.Type != type) continue;
                    col.RemoveIngredient(ing);
                    ing.DestroyWithFlash();
                    if (!IsBun(type)) nonBun++;
                    removedAny = true;
                }

                if (removedAny)
                {
                    col.CollapseFromRow(0);          // survivors above the gaps drop
                    CheckAndProcessMatches(col);     // new adjacencies → standard cascade
                }
            }

            Vector3 pos = originColumn != null
                ? originColumn.GetWorldPositionForRow(originColumn.StackHeight)
                : Vector3.zero;
            AwardConsumablePoints(nonBun, pos);
        }

        /// <summary>
        /// Skewer: keeps one bottom bun at row 0, destroys the rest, regulars collapse on top.
        /// </summary>
        public void ConsumableSkewer(Column column)
        {
            if (column == null) return;

            List<Ingredient> taken = column.TakeAllIngredients();

            // Keep the topmost bottom bun (they're identical, so which survives is cosmetic).
            Ingredient keep = null;
            for (int i = taken.Count - 1; i >= 0; i--)
            {
                if (taken[i].Type == IngredientType.BunBottom) { keep = taken[i]; break; }
            }

            if (keep == null)
            {
                column.SetAllIngredients(taken); // no bun (CanApply guards this) — restore untouched
                return;
            }

            List<Ingredient> rebuilt = new List<Ingredient> { keep };
            foreach (Ingredient ing in taken)
            {
                if (ing == keep) continue;
                if (ing.Type == IngredientType.BunBottom) { ing.DestroyWithFlash(); continue; } // extra bottoms (no points)
                rebuilt.Add(ing);
            }

            column.SetAllIngredients(rebuilt);
            column.CollapseFromRow(0);
            CheckAndProcessMatches(column);
        }

        private void AwardConsumablePoints(int nonBunCount, Vector3 pos)
        {
            if (nonBunCount <= 0) return;

            int points = nonBunCount * GameplayConfig.POINTS_CONSUMABLE_PER_INGREDIENT;
            GameManager.Instance?.AddExtraScore(points);
            FloatingText.Spawn(pos, $"{points}!", UIStyles.TEXT_FAST_DROP, UIStyles.WORLD_FLOATING_TEXT_SIZE);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw grid for debugging
            Gizmos.color = Color.green;

            for (int col = 0; col < Constants.COLUMN_COUNT; col++)
            {
                for (int row = 0; row <= Constants.MAX_ROWS; row++)
                {
                    float x = Constants.GRID_ORIGIN_X + (col * Constants.CELL_WIDTH);
                    float y = Constants.GRID_ORIGIN_Y + (row * Constants.CELL_VISUAL_HEIGHT);
                    Vector3 pos = new Vector3(x, y, 0);
                    Gizmos.DrawWireCube(pos, new Vector3(Constants.CELL_WIDTH * 0.9f, Constants.CELL_VISUAL_HEIGHT * 0.9f, 0.1f));
                }
            }
        }
#endif
    }
}
