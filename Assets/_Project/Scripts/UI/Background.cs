using UnityEngine;

namespace DogtorBurguer
{
    public class Background : MonoBehaviour
    {
        [SerializeField] private BackgroundType _type = BackgroundType.Game;
        [SerializeField, Range(0f, 1f)] private float _filterOpacity = UIStyles.BG_FILTER_OPACITY;

        private SpriteRenderer _renderer;
        private SpriteRenderer _filter;
        private Camera _cam;
        private float _camWidth;
        private float _camHeight;

        private void Start()
        {
            CacheCameraDimensions();

            GameObject bgObj = new GameObject("BackgroundSprite");
            bgObj.transform.SetParent(transform, false);
            _renderer = bgObj.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = Constants.SORT_BACKGROUND;

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

            CreateFilter();
        }

        private void CreateFilter()
        {
            if (_filterOpacity <= 0f || _cam == null) return;

            GameObject filterObj = new GameObject("BackgroundFilter");
            filterObj.transform.SetParent(transform, false);
            _filter = filterObj.AddComponent<SpriteRenderer>();
            _filter.sortingOrder = Constants.SORT_BACKGROUND_FILTER;
            _filter.sprite = SpriteFactory.White();
            _filter.color = new Color(1f, 1f, 1f, _filterOpacity);

            // Stretch the 1x1 white sprite to exactly cover the camera.
            FitToCamera(_filter.transform, Constants.Z_BACKGROUND_FILTER, new Vector3(_camWidth, _camHeight, 1f));
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
