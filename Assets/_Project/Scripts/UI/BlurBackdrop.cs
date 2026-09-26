using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The frosted backdrop of a modal: a full-stretch RawImage that shows a blurred snapshot of
    /// whatever was on screen when the modal opened (<see cref="ScreenBlur"/>). Because the snapshot
    /// is taken at the end of the frame the modal is built in, the modal itself must not be in it:
    /// <see cref="HideRoot"/> is switched off for that one frame — its Canvas component when it has
    /// one (no lifecycle side effects), else a CanvasGroup at alpha 0. Owns the texture; releases it
    /// when the overlay is destroyed. Built by <see cref="UIFactory.CreateOverlay"/>.
    /// </summary>
    public class BlurBackdrop : MonoBehaviour
    {
        /// <summary>What to hide while the snapshot is taken (the modal's canvas, or the overlay itself
        /// when the modal's content is a child of it).</summary>
        public GameObject HideRoot { get; set; }

        private RawImage _image;
        private RenderTexture _texture;

        private void Start()
        {
            _image = GetComponent<RawImage>();
            _image.enabled = false;

            Canvas canvas = HideRoot != null ? HideRoot.GetComponent<Canvas>() : null;
            CanvasGroup group = null;
            if (canvas != null) canvas.enabled = false;
            else if (HideRoot != null)
            {
                group = HideRoot.GetComponent<CanvasGroup>() ?? HideRoot.AddComponent<CanvasGroup>();
                group.alpha = 0f;
            }

            ScreenBlur.Capture(texture =>
            {
                if (canvas != null) canvas.enabled = true;
                else if (group != null) group.alpha = 1f;

                if (this == null || _image == null) { texture.Release(); Destroy(texture); return; }
                _texture = texture;
                _image.texture = texture;
                // The backbuffer read lands upside down on APIs whose UV origin is the top (D3D,
                // Metal); flip the rect rather than the texture.
                _image.uvRect = SystemInfo.graphicsUVStartsAtTop && UIStyles.MODAL_BLUR_FLIP_ON_TOP_ORIGIN
                    ? new Rect(0f, 1f, 1f, -1f)
                    : new Rect(0f, 0f, 1f, 1f);
                _image.enabled = true;
            });
        }

        private void OnDestroy()
        {
            if (_texture == null) return;
            _texture.Release();
            Destroy(_texture);
            _texture = null;
        }
    }
}
