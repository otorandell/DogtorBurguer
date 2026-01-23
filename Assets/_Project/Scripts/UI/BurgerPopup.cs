using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class BurgerPopup : MonoBehaviour
    {
        private TextMeshPro _nameText;
        private TextMeshPro _scoreText;

        private const float POP_DURATION = 0.3f;
        private const float HOLD_DURATION = 1.0f;
        private const float FADE_DURATION = 0.4f;

        public void Initialize(string burgerName, int points, Color nameColor)
        {
            CreateTexts(burgerName, points, nameColor);
            Animate();
        }

        private void CreateTexts(string burgerName, int points, Color nameColor)
        {
            // Burger name (main text)
            _nameText = gameObject.AddComponent<TextMeshPro>();
            _nameText.text = burgerName;
            _nameText.fontSize = 4f;
            _nameText.color = nameColor;
            _nameText.alignment = TextAlignmentOptions.Center;
            _nameText.enableWordWrapping = false;
            _nameText.overflowMode = TextOverflowModes.Overflow;
            _nameText.fontStyle = FontStyles.Bold;
            _nameText.sortingOrder = 110;
            _nameText.rectTransform.sizeDelta = new Vector2(6f, 2f);

            // Score text (child object, below name)
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(transform);
            scoreObj.transform.localPosition = new Vector3(0, -0.6f, 0);

            _scoreText = scoreObj.AddComponent<TextMeshPro>();
            _scoreText.text = $"+{points}";
            _scoreText.fontSize = 3.5f;
            _scoreText.color = Color.white;
            _scoreText.alignment = TextAlignmentOptions.Center;
            _scoreText.enableWordWrapping = false;
            _scoreText.overflowMode = TextOverflowModes.Overflow;
            _scoreText.sortingOrder = 110;
            _scoreText.rectTransform.sizeDelta = new Vector2(4f, 1.5f);
        }

        private void Animate()
        {
            // Start at zero scale
            transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();

            // Pop in with overshoot
            seq.Append(transform.DOScale(1.2f, POP_DURATION).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));

            // Hold
            seq.AppendInterval(HOLD_DURATION);

            // Fade out and rise
            seq.Append(transform.DOMove(transform.position + Vector3.up * 1f, FADE_DURATION).SetEase(Ease.InCubic));
            seq.Join(DOTween.To(() => _nameText.alpha, x => _nameText.alpha = x, 0f, FADE_DURATION));
            seq.Join(DOTween.To(() => _scoreText.alpha, x => _scoreText.alpha = x, 0f, FADE_DURATION));
            seq.Join(transform.DOScale(0.5f, FADE_DURATION).SetEase(Ease.InCubic));

            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
