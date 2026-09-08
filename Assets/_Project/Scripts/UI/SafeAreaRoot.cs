using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Shrinks its RectTransform to <see cref="Screen.safeArea"/> (camera notches, punch-holes,
    /// rounded corners). UIFactory.CreateCanvas puts one on every canvas as the "SafeRoot"
    /// child: edge-anchored interactive chrome (top bars, HUD cards, close/skip buttons)
    /// parents THERE instead of the canvas, so a notch can never swallow it. Full-bleed page
    /// art stays on the canvas — drawing under the notch is fine, being tappable there is not.
    /// Change-guarded per frame (safeArea can change on rotation / fold).
    /// </summary>
    public class SafeAreaRoot : MonoBehaviour
    {
        private Rect _applied = Rect.zero;
        private RectTransform _rect;

        private void Awake() => _rect = (RectTransform)transform;

        private void LateUpdate()
        {
            Rect safe = Screen.safeArea;
            if (safe == _applied) return;
            _applied = safe;

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
