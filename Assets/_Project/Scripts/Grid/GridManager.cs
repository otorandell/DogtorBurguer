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

        public event Action OnGameOver;
        public event Action<int> OnMatchEliminated;         // Points earned
        public event Action<int, string> OnBurgerCompleted; // Points earned, burger name

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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

            // Check for overflow (game over condition)
            if (column.IsOverflowing)
            {
                OnGameOver?.Invoke();
                return;
            }

            // Check for matches
            CheckAndProcessMatches(column);

            // Check for burger completion
            CheckAndProcessBurger(column);
        }

        private void CheckAndProcessMatches(Column column)
        {
            if (column.CheckForMatch(out Ingredient top, out Ingredient second))
            {
                // Remove both ingredients
                column.RemoveIngredient(top);
                column.RemoveIngredient(second);

                // Animate destruction
                top.DestroyWithAnimation();
                second.DestroyWithAnimation();

                // Award points
                OnMatchEliminated?.Invoke(Constants.POINTS_MATCH);

                // Check for more matches after this one
                CheckAndProcessMatches(column);
            }
        }

        private void CheckAndProcessBurger(Column column)
        {
            var ingredients = column.GetAllIngredients();
            if (ingredients.Count < 2) return;

            // Search from top for BunTop
            int bunTopIndex = -1;
            for (int i = ingredients.Count - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunTop)
                {
                    bunTopIndex = i;
                    break;
                }
            }

            if (bunTopIndex < 0) return;

            // Search downward for BunBottom
            int bunBottomIndex = -1;
            for (int i = bunTopIndex - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunBottom)
                {
                    bunBottomIndex = i;
                    break;
                }
                // If we find another BunTop, stop
                if (ingredients[i].Type == IngredientType.BunTop)
                {
                    break;
                }
            }

            if (bunBottomIndex < 0) return;

            // We have a complete burger!
            int ingredientCount = bunTopIndex - bunBottomIndex - 1;
            int points = CalculateBurgerPoints(ingredientCount);
            string burgerName = GenerateBurgerName(ingredientCount);

            // Destroy all burger ingredients
            for (int i = bunTopIndex; i >= bunBottomIndex; i--)
            {
                ingredients[i].DestroyWithAnimation();
            }

            // Remove from column
            column.RemoveIngredientsInRange(bunBottomIndex, bunTopIndex);

            // Collapse remaining ingredients
            column.CollapseFromRow(bunBottomIndex);

            OnBurgerCompleted?.Invoke(points, burgerName);
        }

        private int CalculateBurgerPoints(int ingredientCount)
        {
            int basePoints = ingredientCount * Constants.POINTS_PER_INGREDIENT;
            int bonus;

            if (ingredientCount <= 2) bonus = Constants.BONUS_SMALL_BURGER;
            else if (ingredientCount <= 4) bonus = Constants.BONUS_MEDIUM_BURGER;
            else if (ingredientCount <= 6) bonus = Constants.BONUS_LARGE_BURGER;
            else bonus = Constants.BONUS_MEGA_BURGER;

            return basePoints + bonus;
        }

        private string GenerateBurgerName(int ingredientCount)
        {
            // Simple name generation - can be expanded later
            string[] prefixes = { "La", "El", "Super", "Mega", "Ultra", "Doble" };
            string[] adjectives = { "Explosiva", "Deluxe", "Suprema", "Loca", "Salvaje", "Brutal" };
            string[] nouns = { "Torre", "Monstruo", "Bestia", "Titan", "Coloso", "Trapo" };

            int seed = ingredientCount + Time.frameCount;
            string prefix = prefixes[seed % prefixes.Length];
            string adj = adjectives[(seed * 7) % adjectives.Length];
            string noun = nouns[(seed * 13) % nouns.Length];

            return $"{prefix} {noun} {adj}";
        }

        public void SwapColumnTops(int columnA, int columnB)
        {
            SwapColumns(columnA, columnB);
        }

        public void SwapColumns(int columnA, int columnB)
        {
            Column colA = GetColumn(columnA);
            Column colB = GetColumn(columnB);

            if (colA == null || colB == null) return;

            // Get the Y threshold - falling ingredients below this level get swapped too
            // Use the higher of the two column tops + 20% buffer for forgiveness
            float thresholdY = Mathf.Max(
                colA.GetNextLandingPosition().y,
                colB.GetNextLandingPosition().y
            ) + (Constants.CELL_HEIGHT * 0.2f);

            // Swap all stacked ingredients
            List<Ingredient> ingredientsA = colA.TakeAllIngredients();
            List<Ingredient> ingredientsB = colB.TakeAllIngredients();

            colA.SetAllIngredients(ingredientsB);
            colB.SetAllIngredients(ingredientsA);

            // Animate stacked ingredients to their new positions
            foreach (var ing in ingredientsA)
            {
                ing.AnimateToCurrentPosition();
            }
            foreach (var ing in ingredientsB)
            {
                ing.AnimateToCurrentPosition();
            }

            // Swap falling ingredients that are below the threshold
            foreach (var falling in _fallingIngredients)
            {
                if (falling == null || falling.IsLanded) continue;

                // Only swap if the ingredient is below the threshold Y
                if (falling.CurrentY <= thresholdY)
                {
                    if (falling.CurrentColumn == colA)
                    {
                        falling.SwapToColumn(colB, Constants.INITIAL_FALL_STEP_DURATION);
                    }
                    else if (falling.CurrentColumn == colB)
                    {
                        falling.SwapToColumn(colA, Constants.INITIAL_FALL_STEP_DURATION);
                    }
                }
            }

            // Check for matches and burgers after swap
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
                    float y = Constants.GRID_ORIGIN_Y + (row * Constants.CELL_HEIGHT);
                    Vector3 pos = new Vector3(x, y, 0);
                    Gizmos.DrawWireCube(pos, new Vector3(Constants.CELL_WIDTH * 0.9f, Constants.CELL_HEIGHT * 0.9f, 0.1f));
                }
            }
        }
#endif
    }
}
