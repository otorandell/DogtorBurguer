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

        private SpriteRenderer _renderer;

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
                topColor = new Color(0.08f, 0.06f, 0.18f);
                bottomColor = new Color(0.18f, 0.08f, 0.25f);
            }
            else
            {
                topColor = new Color(0.04f, 0.08f, 0.14f);
                bottomColor = new Color(0.06f, 0.14f, 0.18f);
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
