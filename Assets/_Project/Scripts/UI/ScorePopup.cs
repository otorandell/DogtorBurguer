using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;

        public void Initialize(int points, Color color)
        {
            if (_text == null)
                _text = GetComponent<TextMeshPro>();

            _text.text = $"+{points}";
            _text.color = color;
            _text.sortingOrder = 100;

            Animate();
        }

        private void Animate()
        {
            Vector3 targetPos = transform.position + Vector3.up * AnimConfig.POPUP_RISE_DISTANCE;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(targetPos, AnimConfig.POPUP_DURATION).SetEase(Ease.OutCubic));
            seq.Join(DOTween.To(() => _text.alpha, x => _text.alpha = x, 0f, AnimConfig.POPUP_DURATION).SetEase(Ease.InQuad));
            seq.Join(transform.DOScale(0.8f, AnimConfig.POPUP_DURATION).SetEase(Ease.InQuad));
            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
