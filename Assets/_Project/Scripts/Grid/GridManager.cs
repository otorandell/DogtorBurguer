using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
        public event Action<Vector3, int> OnMatchEffect;    // Position, points
        public event Action<Vector3, int, string> OnBurgerEffect; // Position, points, name
        public event Action OnIngredientPlaced;

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

            OnIngredientPlaced?.Invoke();

            // Top bun validation: destroy if no bottom bun below
            if (ingredient.Type == IngredientType.BunTop)
            {
                if (!ColumnHasBunBelow(column, ingredient))
                {
                    Vector3 pos = ingredient.transform.position;
                    column.RemoveIngredient(ingredient);
                    ingredient.DestroyWithFlash();
                    FloatingText.Spawn(pos, "Too bad!", Color.red, 4f);
                    return;
                }
            }

            // Check for matches (includes bottom bun cancellation)
            CheckAndProcessMatches(column);

            // Check for burger completion
            CheckAndProcessBurger(column);
        }

        private bool ColumnHasBunBelow(Column column, Ingredient topBun)
        {
            var ingredients = column.GetAllIngredients();
            int topBunIndex = ingredients.IndexOf(topBun);

            for (int i = topBunIndex - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunBottom)
                    return true;
            }
            return false;
        }

        private void CheckAndProcessMatches(Column column)
        {
            if (column.CheckForMatch(out Ingredient top, out Ingredient second))
            {
                Vector3 effectPos = (top.transform.position + second.transform.position) / 2f;
                bool isBunMatch = top.Type == IngredientType.BunBottom;

                // Remove both ingredients
                column.RemoveIngredient(top);
                column.RemoveIngredient(second);

                // Flash then destroy
                top.DestroyWithFlash();
                second.DestroyWithFlash();

                if (isBunMatch)
                {
                    // Bottom buns cancel each other - no score
                    FloatingText.Spawn(effectPos, "Too bad!", Color.red, 4f);
                }
                else
                {
                    // Award points and fire effect event
                    OnMatchEliminated?.Invoke(Constants.POINTS_MATCH);
                    OnMatchEffect?.Invoke(effectPos, Constants.POINTS_MATCH);
                }

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
                if (ingredients[i].Type == IngredientType.BunTop)
                {
                    break;
                }
            }

            if (bunBottomIndex < 0) return;

            // Collect burger ingredients (top to bottom)
            List<Ingredient> burgerParts = new List<Ingredient>();
            for (int i = bunTopIndex; i >= bunBottomIndex; i--)
            {
                burgerParts.Add(ingredients[i]);
            }

            int ingredientCount = bunTopIndex - bunBottomIndex - 1;
            int points = CalculateBurgerPoints(ingredientCount);
            string burgerName = GenerateBurgerName(ingredientCount);

            StartCoroutine(BurgerCompressAnimation(column, burgerParts, bunBottomIndex, bunTopIndex, points, burgerName));
        }

        private System.Collections.IEnumerator BurgerCompressAnimation(
            Column column, List<Ingredient> burgerParts,
            int bunBottomIndex, int bunTopIndex,
            int points, string burgerName)
        {
            // Pause spawning during animation
            GameManager.Instance?.PauseSpawning();

            // burgerParts[0] = top bun, burgerParts[last] = bottom bun
            Ingredient bottomBun = burgerParts[burgerParts.Count - 1];
            Vector3 bottomBunPos = bottomBun.transform.position;

            float stepDuration = 0.12f;
            float travelSpacing = Constants.CELL_VISUAL_HEIGHT * 0.2f;
            float smackSpacing = Constants.CELL_VISUAL_HEIGHT * 0.15f;

            // Group of ingredients being pushed down (starts with just the top bun)
            List<Ingredient> movingGroup = new List<Ingredient> { burgerParts[0] };

            // Push through each ingredient until reaching the bottom bun
            for (int i = 1; i < burgerParts.Count - 1; i++)
            {
                Ingredient target = burgerParts[i];
                Vector3 targetPos = target.transform.position;

                // Move the group down, each member offset by travel spacing
                for (int g = 0; g < movingGroup.Count; g++)
                {
                    Vector3 dest = targetPos + Vector3.up * ((movingGroup.Count - g) * travelSpacing);
                    movingGroup[g].transform.DOMove(dest, stepDuration).SetEase(Ease.InQuad);
                }
                yield return new WaitForSeconds(stepDuration);

                // This ingredient joins the moving group
                movingGroup.Add(target);

                // Squeeze sound
                AudioManager.Instance?.PlaySqueeze();

                // Pause between each compress step
                yield return new WaitForSeconds(0.1f);
            }

            // Group moves down to bottom bun with travel spacing
            for (int g = 0; g < movingGroup.Count; g++)
            {
                Vector3 dest = bottomBunPos + Vector3.up * ((movingGroup.Count - g) * travelSpacing);
                movingGroup[g].transform.DOMove(dest, stepDuration).SetEase(Ease.InQuad);
            }
            yield return new WaitForSeconds(stepDuration);

            // Smack: slam everything tight against bottom bun
            AudioManager.Instance?.PlaySqueeze();
            for (int g = 0; g < movingGroup.Count; g++)
            {
                Vector3 dest = bottomBunPos + Vector3.up * ((movingGroup.Count - g) * smackSpacing);
                movingGroup[g].transform.DOMove(dest, 0.08f).SetEase(Ease.InBack);
            }
            yield return new WaitForSeconds(0.08f);

            // Destroy all burger parts at once
            foreach (var part in burgerParts)
            {
                part.DestroyWithFlash();
            }

            // Remove from column and collapse
            column.RemoveIngredientsInRange(bunBottomIndex, bunTopIndex);
            column.CollapseFromRow(bunBottomIndex);

            OnBurgerCompleted?.Invoke(points, burgerName);
            OnBurgerEffect?.Invoke(bottomBunPos, points, burgerName);

            // Resume spawning
            GameManager.Instance?.ResumeSpawning();
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
            string[] smallPrefixes = { "The", "Lil'", "Mini", "Baby" };
            string[] mediumPrefixes = { "Super", "Big", "Double", "Triple" };
            string[] largePrefixes = { "Mega", "Ultra", "Giga", "Hyper" };
            string[] megaPrefixes = { "ULTRA", "LEGENDARY", "EPIC", "GODLIKE" };

            string[] adjectives = {
                "Explosive", "Deluxe", "Supreme", "Wild", "Savage",
                "Brutal", "Infernal", "Cosmic", "Atomic", "Turbo",
                "Divine", "Furious", "Volcanic", "Radical", "Blazing"
            };

            string[] nouns = {
                "Tower", "Monster", "Beast", "Titan", "Colossus",
                "Skyscraper", "Tsunami", "Quake", "Volcano", "Hurricane",
                "Avalanche", "Tornado", "Meteor", "Dragon", "Kraken"
            };

            string[] prefixes;
            if (ingredientCount <= 2) prefixes = smallPrefixes;
            else if (ingredientCount <= 4) prefixes = mediumPrefixes;
            else if (ingredientCount <= 6) prefixes = largePrefixes;
            else prefixes = megaPrefixes;

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string adj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
            string noun = nouns[UnityEngine.Random.Range(0, nouns.Length)];

            return $"{prefix} {noun} {adj}";
        }

        public void SwapColumnTops(int columnA, int columnB)
        {
            SwapColumns(columnA, columnB);
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
            ) + (Constants.CELL_VISUAL_HEIGHT * 0.2f);

            // Swap all stacked ingredients
            List<Ingredient> ingredientsA = colA.TakeAllIngredients();
            List<Ingredient> ingredientsB = colB.TakeAllIngredients();

            colA.SetAllIngredients(ingredientsB);
            colB.SetAllIngredients(ingredientsA);

            // Wave delay settings
            const float waveDelayPerRow = 0.04f;

            // Animate with wave effect - stagger by row (bottom to top)
            foreach (var ing in ingredientsA)
            {
                float delay = ing.CurrentRow * waveDelayPerRow;
                ing.AnimateToCurrentPositionWithWave(delay);
            }
            foreach (var ing in ingredientsB)
            {
                float delay = ing.CurrentRow * waveDelayPerRow;
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
                        falling.SwapToColumn(colB, Constants.INITIAL_FALL_STEP_DURATION);
                        falling.DoWaveEffect(0f);
                    }
                    else if (falling.CurrentColumn == colB)
                    {
                        falling.SwapToColumn(colA, Constants.INITIAL_FALL_STEP_DURATION);
                        falling.DoWaveEffect(0f);
                    }
                }
            }

            // Check for matches and burgers after swap (with slight delay for animation)
            float maxDelay = Mathf.Max(ingredientsA.Count, ingredientsB.Count) * waveDelayPerRow + 0.3f;
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
            ) + (Constants.CELL_VISUAL_HEIGHT * 0.2f);

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
            foreach (var falling in new List<Ingredient>(_fallingIngredients))
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
