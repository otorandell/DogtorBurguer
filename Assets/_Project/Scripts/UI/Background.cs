using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Builds the scene background. The game background is three stacked layers: a camera-filling
    /// base, the diner-scene strip pinned to the top, and the blue play-mat cells centred over the
    /// grid columns. The menu background is just the base. An optional dim filter sits over the base.
    /// </summary>
    public class Background : MonoBehaviour
    {
        [SerializeField] private BackgroundType _type = BackgroundType.Game;

        private SpriteRenderer _renderer;
        private Camera _cam;
        private float _camWidth;
        private float _camHeight;

        private void Start()
        {
            CacheCameraDimensions();
            BuildBase();

            if (_type == BackgroundType.Game)
            {
                BuildRestaurant();
                BuildGridCells();
            }
        }

        // Camera-filling base layer (themed sprite, or a gradient fallback if none is authored).
        private void BuildBase()
        {
            _renderer = CreateLayer("BackgroundSprite", Constants.SORT_BACKGROUND);

            Sprite themeSprite = Theme.Background(_type);
            if (themeSprite != null)
            {
                _renderer.sprite = themeSprite;
            }
            else
            {
                Color top = _type == BackgroundType.Menu ? UIStyles.BG_MENU_TOP : UIStyles.BG_GAME_TOP;
                Color bottom = _type == BackgroundType.Menu ? UIStyles.BG_MENU_BOTTOM : UIStyles.BG_GAME_BOTTOM;
                _renderer.sprite = SpriteFactory.VerticalGradient(bottom, top);
            }

            // Uniform scale fills the camera while preserving the sprite's aspect.
            Vector2 spriteSize = _renderer.sprite.bounds.size;
            float scale = _cam != null ? Mathf.Max(_camWidth / spriteSize.x, _camHeight / spriteSize.y) : 1f;
            FitToCamera(_renderer.transform, Constants.Z_BACKGROUND, new Vector3(scale, scale, 1f));
        }

        // Diner scene scaled to fill the camera width and pinned to the top edge.
        private void BuildRestaurant()
        {
            Sprite sprite = Theme.Restaurant;
            if (sprite == null || _cam == null) return;

            SpriteRenderer layer = CreateLayer("RestaurantLayer", Constants.SORT_RESTAURANT);
            layer.sprite = sprite;

            Vector2 size = sprite.bounds.size;
            float scale = _camWidth / size.x;
            float scaledHalfHeight = size.y * scale * 0.5f;
            float camTop = _cam.transform.position.y + _camHeight * 0.5f;

            layer.transform.localScale = new Vector3(scale, scale, 1f);
            layer.transform.position = new Vector3(
                _cam.transform.position.x,
                camTop - scaledHalfHeight + UIStyles.RESTAURANT_Y_NUDGE,
                Constants.Z_BACKGROUND);
        }

        // Blue play-mat scaled to a target world width and centred over the grid columns.
        private void BuildGridCells()
        {
            Sprite sprite = Theme.GridCells;
            if (sprite == null) return;

            SpriteRenderer layer = CreateLayer("GridCellsLayer", Constants.SORT_GAME_PANEL);
            layer.sprite = sprite;

            Vector2 size = sprite.bounds.size;
            float scale = UIStyles.GRID_CELLS_WIDTH / size.x;
            float gridCenterX = Constants.GRID_ORIGIN_X + (Constants.COLUMN_COUNT - 1) * Constants.CELL_WIDTH * 0.5f;

            layer.transform.localScale = new Vector3(scale, scale, 1f);
            layer.transform.position = new Vector3(
                gridCenterX + UIStyles.GRID_CELLS_X_NUDGE, UIStyles.GRID_CELLS_Y, Constants.Z_BACKGROUND);
        }

        private SpriteRenderer CreateLayer(string name, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private void CacheCameraDimensions()
        {
            _cam = Camera.main;
            if (_cam == null) return;

            _camHeight = 2f * _cam.orthographicSize;
            _camWidth = _camHeight * _cam.aspect;
        }

        /// <summary>Centers a layer on the camera at world depth z with the given scale.</summary>
        private void FitToCamera(Transform layer, float z, Vector3 scale)
        {
            layer.localScale = scale;
            float x = _cam != null ? _cam.transform.position.x : 0f;
            float y = _cam != null ? _cam.transform.position.y : 0f;
            layer.position = new Vector3(x, y, z);
        }
    }
}
