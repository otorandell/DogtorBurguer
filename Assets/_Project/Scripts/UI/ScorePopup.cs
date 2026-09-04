using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class ScorePopup : MonoBehaviour
    {
        private TextMeshPro _text;
        private SpriteRenderer _plate;

        /// <summary>Creates a "+points" popup at the world position and starts its rise-fade.</summary>
        public static ScorePopup Spawn(Vector3 position, int points, Color color)
        {
            GameObject obj = new GameObject("ScorePopup");
            obj.transform.position = position;

            TextMeshPro text = WorldTextFactory.Create(obj, $"{points}!",
                UIStyles.WORLD_SCORE_POPUP_SIZE, color, Constants.SORT_SCORE_POPUP,
                UIStyles.SCORE_POPUP_RECT, FontStyles.Normal, UIStyles.OUTLINE_WIDTH_WORLD);

            ScorePopup popup = obj.AddComponent<ScorePopup>();
            popup._text = text;
            popup._plate = WorldTextFactory.AttachPlate(obj, "ui_popup_plate",
                UIStyles.PLATE_SCORE_H, Constants.SORT_SCORE_POPUP, Vector2.zero);
            popup.Animate();
            return popup;
        }

        private void Animate()
        {
            Vector3 targetPos = transform.position + Vector3.up * AnimConfig.POPUP_RISE_DISTANCE;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(targetPos, AnimConfig.POPUP_DURATION).SetEase(Ease.OutCubic));
            seq.Join(_text.DOFade(0f, AnimConfig.POPUP_DURATION).SetEase(Ease.InQuad));
            seq.Join(_plate.DOFade(0f, AnimConfig.POPUP_DURATION).SetEase(Ease.InQuad));
            seq.Join(transform.DOScale(AnimConfig.POPUP_FADE_SCALE, AnimConfig.POPUP_DURATION).SetEase(Ease.InQuad));
            seq.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (_text != null) _text.DOKill();
            if (_plate != null) _plate.DOKill();
        }
    }
}
