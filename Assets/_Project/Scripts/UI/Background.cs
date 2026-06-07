using UnityEngine;

namespace DogtorBurguer
{
    public class Background : MonoBehaviour
    {
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private BackgroundType _type = BackgroundType.Game;
        [SerializeField, Range(0f, 1f)] private float _filterOpacity = 0.35f;

        private SpriteRenderer _renderer;
        private SpriteRenderer _filter;

        private void Start()
        {
            GameObject bgObj = new GameObject("BackgroundSprite");
            bgObj.transform.SetParent(transform, false);
            bgObj.transform.position = new Vector3(0, 0, 10f);

            _renderer = bgObj.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = Constants.SORT_BACKGROUND;

            if (_backgroundSprite != null)
            {
                _renderer.sprite = _backgroundSprite;
            }
            else
            {
                Color top = _type == BackgroundType.Menu ? UIStyles.BG_MENU_TOP : UIStyles.BG_GAME_TOP;
                Color bottom = _type == BackgroundType.Menu ? UIStyles.BG_MENU_BOTTOM : UIStyles.BG_GAME_BOTTOM;
                _renderer.sprite = SpriteFactory.VerticalGradient(bottom, top);
            }

            FitToCamera();
            CreateFilter();
        }

        private void CreateFilter()
        {
            if (_filterOpacity <= 0f) return;

            GameObject filterObj = new GameObject("BackgroundFilter");
            filterObj.transform.SetParent(transform, false);

            _filter = filterObj.AddComponent<SpriteRenderer>();
            _filter.sortingOrder = Constants.SORT_BACKGROUND_FILTER;
            _filter.color = new Color(1f, 1f, 1f, _filterOpacity);

            _filter.sprite = SpriteFactory.White();

            // Match the background size
            Camera cam = Camera.main;
            if (cam == null) return;

            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            filterObj.transform.localScale = new Vector3(camWidth, camHeight, 1f);
            filterObj.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 9.9f);
        }

        private void FitToCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector2 spriteSize = _renderer.sprite.bounds.size;

            float scaleX = camWidth / spriteSize.x;
            float scaleY = camHeight / spriteSize.y;
            float scale = Mathf.Max(scaleX, scaleY);

            _renderer.transform.localScale = new Vector3(scale, scale, 1f);
            _renderer.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
        }
    }
}
