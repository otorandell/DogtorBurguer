using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogtorBurguer
{
    /// <summary>
    /// Keeps the playfield framed across phone aspect ratios. The game is designed for WIDTH: the
    /// orthographic size is chosen so the play area (the 4 columns) always fills the screen width and
    /// is never clipped, with a floor so wide screens don't zoom in past the design height. Pair this
    /// with the canvas scaler matching width (UIStyles.MATCH_WIDTH_OR_HEIGHT = 0) so the HUD scales by
    /// the same rule and stays locked to the playfield.
    ///
    /// Self-attaches to Camera.main at load (no scene wiring needed) and re-fits on resolution change.
    /// At the reference aspect this is a no-op (orthographic size stays 5).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        private Camera _cam;
        private int _lastWidth;
        private int _lastHeight;

        // Runs after the scene's Awakes but before any Start, so Background (which caches the camera
        // size in Start) reads the already-fitted orthographic size. Re-attaches on each scene load.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Attach();
            SceneManager.sceneLoaded += (_, __) => Attach();
        }

        private static void Attach()
        {
            Camera cam = Camera.main;
            if (cam != null && cam.GetComponent<CameraFit>() == null)
                cam.gameObject.AddComponent<CameraFit>();
        }

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            Apply();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
                Apply();
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            float aspect = (float)Screen.width / Screen.height;
            float widthFit = (Constants.PLAY_AREA_WIDTH * 0.5f) / aspect;
            _cam.orthographicSize = Mathf.Max(Constants.DESIGN_ORTHO_SIZE, widthFit);
        }
    }
}
