using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Diner-checker letterbox (the artist's red/white mosaic): covers the world revealed beyond
    /// the designed 9:16 window. The width-framing camera (CameraFit) guarantees exactly one axis
    /// ever has surplus — tall phones get top + bottom bands, tablets get left + right, the
    /// reference aspect gets none. Also publishes the inner window (screen px) so SafeAreaRoot
    /// lays the HUD chrome out inside the bands — a phone's HUD then matches the editor 9:16
    /// layout exactly. Created by Background in the game scene; change-guarded per frame.
    /// </summary>
    public class ScreenFrame : MonoBehaviour
    {
        /// <summary>The designed window in screen pixels while a frame is active (zero when not).
        /// SafeAreaRoot intersects the notch safe area with this.</summary>
        public static Rect InnerScreenRect { get; private set; }

        // Statics survive disabled domain reload — reset per play session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => InnerScreenRect = Rect.zero;

        private Camera _cam;
        private SpriteRenderer[] _bars; // top, bottom, left, right
        private int _lastWidth;
        private int _lastHeight;
        private float _lastOrtho;

        private void Awake()
        {
            _cam = Camera.main;
            Sprite checker = UiArt.Load("ui_checker_border");
            if (checker == null || _cam == null)
            {
                enabled = false;
                return;
            }

            _bars = new SpriteRenderer[4];
            for (int i = 0; i < _bars.Length; i++)
            {
                GameObject obj = new GameObject("FrameBar");
                obj.transform.SetParent(transform, false);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = checker;
                sr.drawMode = SpriteDrawMode.Tiled;      // squares repeat at fixed size, never stretch
                sr.tileMode = SpriteTileMode.Continuous; // (needs the sprite's FullRect mesh type)
                sr.sortingOrder = Constants.SORT_SCREEN_FRAME;
                sr.enabled = false;
                _bars[i] = sr;
            }
        }

        private void LateUpdate()
        {
            if (Screen.width == _lastWidth && Screen.height == _lastHeight
                && Mathf.Approximately(_cam.orthographicSize, _lastOrtho))
                return;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastOrtho = _cam.orthographicSize;
            Apply();
        }

        private void Apply()
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            float innerHalfH = Constants.DESIGN_ORTHO_SIZE;
            float innerHalfW = Constants.DESIGN_ORTHO_SIZE
                * (UIStyles.REFERENCE_RESOLUTION.x / UIStyles.REFERENCE_RESOLUTION.y);
            Vector3 c = _cam.transform.position;

            float left = c.x - halfW;
            float right = c.x + halfW;
            float bottom = c.y - halfH;
            float top = c.y + halfH;

            // The sprite pivots at its bottom-left corner and the texture is cropped to whole
            // squares, so each band's pattern is anchored at its SCREEN border (mirror flips for
            // the top/right bands): full squares start at the outer edge and grow inward — the
            // one cut row lands against the game content, never at the visible border.
            float vSurplus = halfH - innerHalfH; // > 0 on tall phones
            float hSurplus = halfW - innerHalfW; // > 0 on tablets / wide screens
            SetBar(_bars[0], vSurplus, new Vector2(halfW * 2f, vSurplus),
                new Vector3(left, top), new Vector3(1f, -1f, 1f));
            SetBar(_bars[1], vSurplus, new Vector2(halfW * 2f, vSurplus),
                new Vector3(left, bottom), Vector3.one);
            SetBar(_bars[2], hSurplus, new Vector2(hSurplus, halfH * 2f),
                new Vector3(left, bottom), Vector3.one);
            SetBar(_bars[3], hSurplus, new Vector2(hSurplus, halfH * 2f),
                new Vector3(right, bottom), new Vector3(-1f, 1f, 1f));

            // Publish the inner window in screen px (the camera view is screen-centered).
            float wPx = Mathf.Min(Screen.width * (innerHalfW / halfW), Screen.width);
            float hPx = Mathf.Min(Screen.height * (innerHalfH / halfH), Screen.height);
            InnerScreenRect = new Rect(
                (Screen.width - wPx) * 0.5f, (Screen.height - hPx) * 0.5f, wPx, hPx);
        }

        private static void SetBar(SpriteRenderer bar, float surplus, Vector2 size,
            Vector3 cornerPos, Vector3 scale)
        {
            bool visible = surplus > 0.001f;
            bar.enabled = visible;
            if (!visible) return;
            bar.size = size;
            bar.transform.position = cornerPos;
            bar.transform.localScale = scale;
        }

        private void OnDestroy() => InnerScreenRect = Rect.zero;
    }
}
