using UnityEngine;

namespace DogtorBurguer
{
    public enum BackgroundType
    {
        Menu,
        Game
    }

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
            _renderer.sortingOrder = -100;

            if (_backgroundSprite != null)
            {
                _renderer.sprite = _backgroundSprite;
            }
            else
            {
                _renderer.sprite = GenerateGradientSprite();
            }

            FitToCamera();
            CreateFilter();
        }

        private Sprite GenerateGradientSprite()
        {
            int width = 2;
            int height = 256;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color topColor, bottomColor;

            if (_type == BackgroundType.Menu)
            {
                topColor = UIStyles.BG_MENU_TOP;
                bottomColor = UIStyles.BG_MENU_BOTTOM;
            }
            else
            {
                topColor = UIStyles.BG_GAME_TOP;
                bottomColor = UIStyles.BG_GAME_BOTTOM;
            }

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color c = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private void CreateFilter()
        {
            if (_filterOpacity <= 0f) return;

            GameObject filterObj = new GameObject("BackgroundFilter");
            filterObj.transform.SetParent(transform, false);

            _filter = filterObj.AddComponent<SpriteRenderer>();
            _filter.sortingOrder = -99;
            _filter.color = new Color(1f, 1f, 1f, _filterOpacity);

            // 1x1 white pixel sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _filter.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

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
