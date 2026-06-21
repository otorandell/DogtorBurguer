using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Spawns the four decorative plates — one under each column — that the bottom ingredient
    /// appears to rest on. Purely cosmetic and static: a chef flip swaps the columns' contents
    /// above them, but the plates are identical per column so they stay put.
    /// </summary>
    public class PlateManager : MonoBehaviour
    {
        private void Awake()
        {
            CreatePlates();
        }

        private void CreatePlates()
        {
            Sprite sprite = Theme.Plate;
            for (int col = 0; col < Constants.COLUMN_COUNT; col++)
            {
                GameObject go = new GameObject($"Plate_{col}");
                go.transform.SetParent(transform, false);
                go.transform.position = PlatePosition(col);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = Constants.SORT_PLATE;
            }
        }

        private static Vector3 PlatePosition(int column)
        {
            float x = Constants.GRID_ORIGIN_X + (column * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y - Constants.PLATE_Y_OFFSET;
            return new Vector3(x, y, 0f);
        }
    }
}
