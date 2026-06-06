using System;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private Column[] _columns;

        private List<Ingredient> _fallingIngredients = new List<Ingredient>();
        private BurgerAnimator _burgerAnimator;
        private HashSet<Column> _columnsWithActiveBurger = new HashSet<Column>();

        public event Action OnGameOver;
        public event Action<int> OnMatchEliminated;         // Points earned
        public event Action<int, string> OnBurgerCompleted; // Points earned, burger name
        public event Action<Vector3, int> OnMatchEffect;    // Position, points
        public event Action<Vector3, int, string, int> OnBurgerEffect; // Position, points, name, ingredientCount
        public event Action<Vector3, int, string, int, List<IngredientType>> OnBurgerWithIngredients;
        public event Action OnIngredientPlaced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _burgerAnimator = gameObject.AddComponent<BurgerAnimator>();
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
                if (MatchDetector.HasBunBelow(column, ingredient))
                {
                    OnIngredientPlaced?.Invoke();
                    CheckAndProcessBurger(column);
                    return;
                }
                else
                {
                    // No bottom bun below — destroy top bun
                    Vector3 pos = ingredient.transform.position;
                    column.RemoveIngredient(ingredient);
                    ingredient.DestroyWithFlash();
                    FloatingText.Spawn(pos, "Too bad!", UIStyles.TEXT_TOO_BAD, UIStyles.WORLD_FLOATING_TEXT_SIZE);
                    return;
                }
            }

            // Check for overflow (game over condition)
            if (column.IsOverflowing)
            {
                OnGameOver?.Invoke();
                return;
            }

            OnIngredientPlaced?.Invoke();

            // Check for matches (includes bottom bun cancellation)
            CheckAndProcessMatches(column);

            // Check for burger completion
            CheckAndProcessBurger(column);
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

        private void CheckAndProcessBurger(Column column)
        {
            if (_columnsWithActiveBurger.Contains(column)) return;

            var detection = MatchDetector.DetectBurger(column);
            if (!detection.Found) return;

            _columnsWithActiveBurger.Add(column);

            // Remove burger parts from column data IMMEDIATELY
            // so they can't be re-detected or moved by swaps
            column.RemoveIngredientsInRange(detection.BunBottomIndex, detection.BunTopIndex);

            int points = BurgerAnimator.CalculatePoints(detection.IngredientCount);
            string burgerName = BurgerAnimator.GenerateName(detection.IngredientCount);

            var data = new BurgerAnimator.BurgerData
            {
                Column = column,
                Parts = detection.Parts,
                BunBottomIndex = detection.BunBottomIndex,
                BunTopIndex = detection.BunTopIndex,
                IngredientCount = detection.IngredientCount,
                IngredientTypes = detection.IngredientTypes,
                Points = points,
                Name = burgerName
            };

            _burgerAnimator.PlayCompress(data, _fallingIngredients, HandleBurgerAnimationComplete);
        }

        private void HandleBurgerAnimationComplete(BurgerAnimator.BurgerData data, Vector3 pos)
        {
            _columnsWithActiveBurger.Remove(data.Column);

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

            // Re-check column for stacked burgers
            CheckAndProcessBurger(data.Column);
        }

        public void SwapColumnsWithWaveEffect(int columnA, int columnB)
        {
            Column colA = GetColumn(columnA);
            Column colB = GetColumn(columnB);

            if (colA == null || colB == null) return;

            // Get the Y threshold - falling ingredients below this level get swapped too
            float thresholdY = Mathf.Max(
                colA.GetNextLandingPosition().y,
                colB.GetNextLandingPosition().y
            ) + (Constants.CELL_VISUAL_HEIGHT * GameplayConfig.SWAP_THRESHOLD_BUFFER_MULT);

            // Swap all stacked ingredients
            List<Ingredient> ingredientsA = colA.TakeAllIngredients();
            List<Ingredient> ingredientsB = colB.TakeAllIngredients();

            colA.SetAllIngredients(ingredientsB);
            colB.SetAllIngredients(ingredientsA);

            // Animate with wave effect - stagger by row (bottom to top)
            foreach (var ing in ingredientsA)
            {
                float delay = ing.CurrentRow * GameplayConfig.SWAP_WAVE_DELAY_PER_ROW;
                ing.AnimateToCurrentPositionWithWave(delay);
            }
            foreach (var ing in ingredientsB)
            {
                float delay = ing.CurrentRow * GameplayConfig.SWAP_WAVE_DELAY_PER_ROW;
                ing.AnimateToCurrentPositionWithWave(delay);
            }

            // Swap falling ingredients that are below the threshold
            foreach (var falling in new List<Ingredient>(_fallingIngredients))
            {
                if (falling == null || falling.IsLanded) continue;

                if (falling.CurrentY <= thresholdY)
                {
                    if (falling.CurrentColumn == colA)
                    {
                        falling.SwapToColumn(colB, GameplayConfig.INITIAL_FALL_STEP_DURATION);
                        falling.DoWaveEffect(0f);
                    }
                    else if (falling.CurrentColumn == colB)
                    {
                        falling.SwapToColumn(colA, GameplayConfig.INITIAL_FALL_STEP_DURATION);
                        falling.DoWaveEffect(0f);
                    }
                }
            }

            // Check for matches and burgers after swap (with slight delay for animation)
            float maxDelay = Mathf.Max(ingredientsA.Count, ingredientsB.Count) * GameplayConfig.SWAP_WAVE_DELAY_PER_ROW + GameplayConfig.SWAP_POST_ANIM_DELAY;
            StartCoroutine(DelayedMatchCheck(colA, colB, maxDelay));
        }

        private System.Collections.IEnumerator DelayedMatchCheck(Column colA, Column colB, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!colA.IsEmpty)
            {
                CheckAndProcessMatches(colA);
                CheckAndProcessBurger(colA);
            }
            if (!colB.IsEmpty)
            {
                CheckAndProcessMatches(colB);
                CheckAndProcessBurger(colB);
            }
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
