using UnityEngine;

namespace DogtorBurguer
{
    public class GameLayout : MonoBehaviour
    {
        [Header("Grid Panel")]
        [SerializeField] private Vector2 _gridPanelCenter = new Vector2(0f, -1.5f);
        [SerializeField] private Vector2 _gridPanelSize = new Vector2(4.6f, 6.2f);

        [Header("Top Left Panel")]
        [SerializeField] private Vector2 _topLeftCenter = new Vector2(-1.2f, 2.5f);
        [SerializeField] private Vector2 _topLeftSize = new Vector2(2.2f, 1.7f);

        [Header("Top Right Panel")]
        [SerializeField] private Vector2 _topRightCenter = new Vector2(1.2f, 2.5f);
        [SerializeField] private Vector2 _topRightSize = new Vector2(2.2f, 1.7f);

        [Header("Border Style")]
        [SerializeField] private Color _borderColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _fillColor = new Color(0f, 0f, 0f, 0.2f);
        [SerializeField] private int _borderWidth = 3;
        [SerializeField] private int _cornerRadius = 20;
        [SerializeField] private int _textureResolution = 128;

        private void Start()
        {
            CreatePanel("GridPanel", _gridPanelCenter, _gridPanelSize);
            CreatePanel("TopLeftPanel", _topLeftCenter, _topLeftSize);
            CreatePanel("TopRightPanel", _topRightCenter, _topRightSize);
        }

        private void CreatePanel(string name, Vector2 center, Vector2 size)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(transform, false);
            panelObj.transform.position = new Vector3(center.x, center.y, 5f);

            SpriteRenderer sr = panelObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -50;

            Texture2D tex = GenerateRoundedRectTexture();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), _textureResolution);
            sr.sprite = sprite;

            // Scale to match desired world-space size
            float spriteWorldWidth = (float)tex.width / _textureResolution;
            float spriteWorldHeight = (float)tex.height / _textureResolution;
            float scaleX = size.x / spriteWorldWidth;
            float scaleY = size.y / spriteWorldHeight;
            panelObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private Texture2D GenerateRoundedRectTexture()
        {
            int w = _textureResolution;
            int h = _textureResolution;
            int r = _cornerRadius;
            int bw = _borderWidth;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dist = DistanceToRoundedRectEdge(x, y, w, h, r);

                    if (dist < 0)
                    {
                        // Outside the rounded rect
                        tex.SetPixel(x, y, transparent);
                    }
                    else if (dist < bw)
                    {
                        // Border region
                        float alpha = Mathf.Clamp01(dist);
                        Color c = _borderColor;
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        // Inside fill
                        tex.SetPixel(x, y, _fillColor);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private float DistanceToRoundedRectEdge(int px, int py, int w, int h, int r)
        {
            // Returns positive distance inward from edge, negative if outside
            float x = px;
            float y = py;

            // Clamp corner radius
            float cr = Mathf.Min(r, w / 2f, h / 2f);

            // Check if we're in a corner region
            bool inLeftCorner = x < cr;
            bool inRightCorner = x > w - 1 - cr;
            bool inBottomCorner = y < cr;
            bool inTopCorner = y > h - 1 - cr;

            if ((inLeftCorner || inRightCorner) && (inBottomCorner || inTopCorner))
            {
                // Corner: distance from corner circle
                float cx = inLeftCorner ? cr : w - 1 - cr;
                float cy = inBottomCorner ? cr : h - 1 - cr;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                return cr - dist;
            }
            else
            {
                // Edge: distance from nearest straight edge
                float distLeft = x;
                float distRight = w - 1 - x;
                float distBottom = y;
                float distTop = h - 1 - y;
                return Mathf.Min(distLeft, distRight, distBottom, distTop);
            }
        }
    }
}
