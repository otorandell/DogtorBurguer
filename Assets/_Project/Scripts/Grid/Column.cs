using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    public class Column : MonoBehaviour
    {
        [SerializeField] private int _columnIndex;

        private List<Ingredient> _ingredients = new List<Ingredient>();

        public int ColumnIndex => _columnIndex;
        public int StackHeight => _ingredients.Count;
        public bool IsOverflowing => _ingredients.Count >= Constants.MAX_ROWS;
        public bool IsEmpty => _ingredients.Count == 0;

        public void Initialize(int index)
        {
            _columnIndex = index;
        }

        public Vector3 GetWorldPositionForRow(int row)
        {
            float x = Constants.GRID_ORIGIN_X + (_columnIndex * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y + (row * Constants.CELL_VISUAL_HEIGHT);
            return new Vector3(x, y, 0);
        }

        public Vector3 GetNextLandingPosition()
        {
            return GetWorldPositionForRow(_ingredients.Count);
        }

        public Vector3 GetSpawnPosition()
        {
            // Spawn above the visible area (extra offset for flattened grid)
            return GetWorldPositionForRow(Constants.MAX_ROWS + 3);
        }

        public void AddIngredient(Ingredient ingredient)
        {
            _ingredients.Add(ingredient);
            ingredient.SetColumnAndRow(this, _ingredients.Count - 1);
        }

        public Ingredient GetTopIngredient()
        {
            if (_ingredients.Count == 0) return null;
            return _ingredients[_ingredients.Count - 1];
        }

        public Ingredient RemoveTopIngredient()
        {
            if (_ingredients.Count == 0) return null;

            Ingredient top = _ingredients[_ingredients.Count - 1];
            _ingredients.RemoveAt(_ingredients.Count - 1);
            return top;
        }

        public void RemoveIngredient(Ingredient ingredient)
        {
            int index = _ingredients.IndexOf(ingredient);
            if (index >= 0)
            {
                _ingredients.RemoveAt(index);
                // Update row indices for ingredients above
                for (int i = index; i < _ingredients.Count; i++)
                {
                    _ingredients[i].SetColumnAndRow(this, i);
                }
            }
        }

        public void RemoveIngredientsInRange(int startRow, int endRow)
        {
            // Remove from top to bottom to avoid index issues
            for (int i = endRow; i >= startRow; i--)
            {
                if (i < _ingredients.Count)
                {
                    _ingredients.RemoveAt(i);
                }
            }

            // Update remaining ingredients' rows
            for (int i = 0; i < _ingredients.Count; i++)
            {
                _ingredients[i].SetColumnAndRow(this, i);
            }
        }

        public Ingredient GetIngredientAtRow(int row)
        {
            if (row < 0 || row >= _ingredients.Count) return null;
            return _ingredients[row];
        }

        public List<Ingredient> GetAllIngredients()
        {
            return new List<Ingredient>(_ingredients);
        }

        /// <summary>
        /// Clears all ingredients from this column and returns them
        /// </summary>
        public List<Ingredient> TakeAllIngredients()
        {
            List<Ingredient> taken = new List<Ingredient>(_ingredients);
            _ingredients.Clear();
            return taken;
        }

        /// <summary>
        /// Sets all ingredients at once (used during column swap)
        /// </summary>
        public void SetAllIngredients(List<Ingredient> ingredients)
        {
            _ingredients = new List<Ingredient>(ingredients);
            // Update each ingredient's column/row reference
            for (int i = 0; i < _ingredients.Count; i++)
            {
                _ingredients[i].SetColumnAndRow(this, i);
            }
        }

        /// <summary>
        /// Checks if top two ingredients match and should be eliminated
        /// </summary>
        public bool CheckForMatch(out Ingredient top, out Ingredient second)
        {
            top = null;
            second = null;

            if (_ingredients.Count < 2) return false;

            top = _ingredients[_ingredients.Count - 1];
            second = _ingredients[_ingredients.Count - 2];

            // Only regular ingredients can match (not buns)
            if (!top.Type.IsRegularIngredient() || !second.Type.IsRegularIngredient())
                return false;

            return top.Type == second.Type;
        }

        /// <summary>
        /// Makes ingredients above the given row fall down
        /// </summary>
        public void CollapseFromRow(int startRow)
        {
            for (int i = startRow; i < _ingredients.Count; i++)
            {
                Ingredient ingredient = _ingredients[i];
                ingredient.FallToRow(i);
            }
        }
    }
}
