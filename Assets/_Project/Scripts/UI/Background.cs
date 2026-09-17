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
                gameObject.AddComponent<ScreenFrame>(); // checker letterbox outside the 9:16 window
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

        // Diner scene scaled to fill the DESIGN window's width and pinned to its top edge — the
        // 9:16 rect the checker letterbox exposes, not the camera. The width-framing camera grows
        // upward on tall phones; pinned to the camera the strip rode up under the top band and
        // its bottom edge lifted clear of the play mat, exposing the base layer as a dark band
        // (artist note 2026-09-17). Laid out against the design window it lands exactly where it
        // does in the 9:16 editor view on every device; whatever overflows above sits under
        // the band. Same rule SafeAreaRoot follows for the HUD.
        private void BuildRestaurant()
        {
            Sprite sprite = Theme.Restaurant;
            if (sprite == null || _cam == null) return;

            SpriteRenderer layer = CreateLayer("RestaurantLayer", Constants.SORT_RESTAURANT);
            layer.sprite = sprite;

            float designHalfH = Constants.DESIGN_ORTHO_SIZE;
            float designWidth = designHalfH * 2f * (UIStyles.REFERENCE_RESOLUTION.x / UIStyles.REFERENCE_RESOLUTION.y);
            Vector2 size = sprite.bounds.size;
            float scale = designWidth / size.x;
            float scaledHalfHeight = size.y * scale * 0.5f;
            float designTop = _cam.transform.position.y + designHalfH;

            layer.transform.localScale = new Vector3(scale, scale, 1f);
            layer.transform.position = new Vector3(
                _cam.transform.position.x,
                designTop - scaledHalfHeight + UIStyles.RESTAURANT_Y_NUDGE,
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
