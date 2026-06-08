using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// Full-screen white flash parented to the camera. Created by FeedbackManager; Trigger()
    /// pulses it from opaque to clear. Owns its own construction and tween cleanup (F-18).
    /// </summary>
    public class ScreenFlashOverlay : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        public void Initialize(Camera cam)
        {
            transform.SetParent(cam.transform);
            transform.localPosition = new Vector3(0f, 0f, 1f);

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.White();
            _renderer.sortingOrder = Constants.SORT_SCREEN_FLASH;
            _renderer.color = Color.clear;

            // Cover the camera view (+1 margin so edges never show through).
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;
            transform.localScale = new Vector3(camWidth + 1f, camHeight + 1f, 1f);
        }

        public void Trigger()
        {
            if (_renderer == null) return;

            _renderer.DOKill();
            _renderer.color = UIStyles.SCREEN_FLASH;
            _renderer.DOColor(Color.clear, AnimConfig.SCREEN_FLASH_DURATION).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            if (_renderer != null) _renderer.DOKill();
        }
    }
}
