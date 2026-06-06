using UnityEngine;

namespace DogtorBurguer
{
    public class GameLayout : MonoBehaviour
    {
        [Header("Grid Panel")]
        [SerializeField] private Vector2 _gridPanelCenter = UIStyles.GRID_PANEL_CENTER;
        [SerializeField] private Vector2 _gridPanelSize = UIStyles.GRID_PANEL_SIZE;

        [Header("Top Left Panel")]
        [SerializeField] private Vector2 _topLeftCenter = UIStyles.TOP_LEFT_PANEL_CENTER;
        [SerializeField] private Vector2 _topLeftSize = UIStyles.TOP_LEFT_PANEL_SIZE;

        [Header("Top Right Panel")]
        [SerializeField] private Vector2 _topRightCenter = UIStyles.TOP_RIGHT_PANEL_CENTER;
        [SerializeField] private Vector2 _topRightSize = UIStyles.TOP_RIGHT_PANEL_SIZE;

        [Header("Border Style")]
        [SerializeField] private Color _borderColor = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private Color _fillColor = new Color(0f, 0f, 0f, 0.15f);
        [SerializeField] private int _borderWidth = UIStyles.PANEL_BORDER_WIDTH;
        [SerializeField] private int _cornerRadius = UIStyles.PANEL_CORNER_RADIUS;

        private const int TEX_SIZE = 128;
        private Sprite _panelSprite;

        private void Start()
        {
            _panelSprite = Generate9SliceSprite();

            CreatePanel("GridPanel", _gridPanelCenter, _gridPanelSize);
            CreatePanel("TopLeftPanel", _topLeftCenter, _topLeftSize);
            CreatePanel("TopRightPanel", _topRightCenter, _topRightSize);
        }

        private void CreatePanel(string name, Vector2 center, Vector2 size)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(transform, false);
            panelObj.transform.position = new Vector3(center.x, center.y, Constants.Z_GAME_PANEL);

            SpriteRenderer sr = panelObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Constants.SORT_GAME_PANEL;
            sr.sprite = _panelSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
        }

        private Sprite Generate9SliceSprite()
        {
            int w = TEX_SIZE;
            int h = TEX_SIZE;
            int r = Mathf.Min(_cornerRadius, w / 2, h / 2);
            int bw = _borderWidth;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dist = DistanceInside(x, y, w, h, r);

                    if (dist < 0f)
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                    else if (dist < bw)
                    {
                        float t = Mathf.Clamp01(dist / 1.5f);
                        Color c = _borderColor;
                        c.a *= t;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, _fillColor);
                    }
                }
            }

            tex.Apply();

            // 9-slice border: corners are protected from stretching
            float border = r;
            Vector4 spriteBorder = new Vector4(border, border, border, border);

            return Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),
                TEX_SIZE,
                0,
                SpriteMeshType.FullRect,
                spriteBorder
            );
        }

        private float DistanceInside(int px, int py, int w, int h, int r)
        {
            float x = px;
            float y = py;
            float cr = r;

            bool left = x < cr;
            bool right = x > w - 1 - cr;
            bool bottom = y < cr;
            bool top = y > h - 1 - cr;

            if ((left || right) && (bottom || top))
            {
                float cx = left ? cr : w - 1 - cr;
                float cy = bottom ? cr : h - 1 - cr;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                return cr - dist;
            }

            float dL = x;
            float dR = w - 1 - x;
            float dB = y;
            float dT = h - 1 - y;
            return Mathf.Min(dL, dR, dB, dT);
        }
    }
}
