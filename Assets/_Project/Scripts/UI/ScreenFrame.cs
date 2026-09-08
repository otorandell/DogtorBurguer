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

        // Squares in the cropped ui_checker_border texture (measured by scratchpad/frame_snap.py;
        // re-measure if the art changes). Gives the exact per-square world size, so bands can snap
        // to whole squares.
        private const int SquaresX = 38;
        private const int SquaresY = 34;

        private Camera _cam;
        private SpriteRenderer[] _bars; // top, bottom, left, right
        private float _squareW;
        private float _squareH;
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

            _squareW = checker.bounds.size.x / SquaresX;
            _squareH = checker.bounds.size.y / SquaresY;
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

            // Each band: pattern anchored at its screen-border corner (the sprite pivots at its
            // bottom-left; mirror flips for the top/right bands), and BOTH axes scaled a hair so
            // a whole number of squares exactly fills the band — every visible edge (screen
            // borders AND the inner edge against the game) lands on a square boundary, so no
            // half-cut squares anywhere.
            float vSurplus = halfH - innerHalfH; // > 0 on tall phones
            float hSurplus = halfW - innerHalfW; // > 0 on tablets / wide screens
            SetBar(_bars[0], vSurplus, new Vector2(halfW * 2f, vSurplus),
                new Vector3(left, top), new Vector2(1f, -1f));
            SetBar(_bars[1], vSurplus, new Vector2(halfW * 2f, vSurplus),
                new Vector3(left, bottom), Vector2.one);
            SetBar(_bars[2], hSurplus, new Vector2(hSurplus, halfH * 2f),
                new Vector3(left, bottom), Vector2.one);
            SetBar(_bars[3], hSurplus, new Vector2(hSurplus, halfH * 2f),
                new Vector3(right, bottom), new Vector2(-1f, 1f));

            // Publish the inner window in screen px (the camera view is screen-centered).
            float wPx = Mathf.Min(Screen.width * (innerHalfW / halfW), Screen.width);
            float hPx = Mathf.Min(Screen.height * (innerHalfH / halfH), Screen.height);
            InnerScreenRect = new Rect(
                (Screen.width - wPx) * 0.5f, (Screen.height - hPx) * 0.5f, wPx, hPx);
        }

        private void SetBar(SpriteRenderer bar, float surplus, Vector2 size,
            Vector3 cornerPos, Vector2 dir)
        {
            bool visible = surplus > 0.001f;
            bar.enabled = visible;
            if (!visible) return;

            // Whole-square snap: round each axis to a square count, then scale the pattern so
            // that count exactly fills the requested size (a distortion of a few percent at
            // most — invisible on a checker).
            float countX = Mathf.Max(1f, Mathf.Round(size.x / _squareW));
            float countY = Mathf.Max(1f, Mathf.Round(size.y / _squareH));
            bar.size = new Vector2(countX * _squareW, countY * _squareH);
            bar.transform.localScale = new Vector3(
                size.x / bar.size.x * dir.x, size.y / bar.size.y * dir.y, 1f);
            bar.transform.position = cornerPos;
        }

        private void OnDestroy() => InnerScreenRect = Rect.zero;
    }
}
