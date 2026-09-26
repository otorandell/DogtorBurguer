using System;
using System.Collections;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Snapshots the screen and blurs it — the "frosted" backdrop behind a modal (shop, settings,
    /// credits, how-to, game over, the shop's confirm dialog). One capture per open: the frame is
    /// grabbed at end-of-frame (<see cref="ScreenCapture.CaptureScreenshotIntoRenderTexture"/>,
    /// so it includes overlay canvases and works with timeScale 0), downscaled, then blurred with a
    /// separable Gaussian (Resources/Materials/UIBlur). A static snapshot is right here: everything
    /// behind a modal is paused or idle. Knobs: <c>UIStyles.MODAL_BLUR_*</c>. Persistent, created on
    /// first use.
    /// </summary>
    public class ScreenBlur : MonoBehaviour
    {
        private static ScreenBlur _instance;
        private static Material _material;

        private static ScreenBlur Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ScreenBlur") { hideFlags = HideFlags.HideAndDontSave };
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<ScreenBlur>();
                }
                return _instance;
            }
        }

        /// <summary>Captures the NEXT end-of-frame and hands back the blurred RenderTexture (the
        /// caller owns it — release it when done). Hide anything that must not be in the shot
        /// before this frame ends; <see cref="BlurBackdrop"/> does that for you.</summary>
        public static void Capture(Action<RenderTexture> onReady) => Instance.StartCoroutine(Instance.CaptureRoutine(onReady));

        private IEnumerator CaptureRoutine(Action<RenderTexture> onReady)
        {
            yield return new WaitForEndOfFrame();

            int w = Screen.width, h = Screen.height;
            RenderTexture full = RenderTexture.GetTemporary(w, h, 0);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(full);

            int scale = Mathf.Max(1, UIStyles.MODAL_BLUR_DOWNSCALE);
            RenderTexture small = new RenderTexture(Mathf.Max(1, w / scale), Mathf.Max(1, h / scale), 0)
            {
                name = "ScreenBlur",
                filterMode = FilterMode.Bilinear,
            };
            Graphics.Blit(full, small);
            RenderTexture.ReleaseTemporary(full);

            Material mat = BlurMaterial;
            if (mat != null)
            {
                RenderTexture ping = RenderTexture.GetTemporary(small.width, small.height, 0);
                mat.SetFloat("_Radius", UIStyles.MODAL_BLUR_RADIUS);
                for (int i = 0; i < UIStyles.MODAL_BLUR_PASSES; i++)
                {
                    mat.SetVector("_Direction", new Vector4(1f, 0f, 0f, 0f));
                    Graphics.Blit(small, ping, mat);
                    mat.SetVector("_Direction", new Vector4(0f, 1f, 0f, 0f));
                    Graphics.Blit(ping, small, mat);
                }
                RenderTexture.ReleaseTemporary(ping);
            }

            onReady?.Invoke(small);
        }

        private static Material BlurMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = Resources.Load<Material>("Materials/UIBlur");
                    if (_material == null)
                        Debug.LogWarning("[ScreenBlur] Resources/Materials/UIBlur missing — backdrop will be downscaled only.");
                }
                return _material;
            }
        }
    }
}
