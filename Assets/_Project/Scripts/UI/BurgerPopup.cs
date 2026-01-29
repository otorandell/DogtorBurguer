using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class BurgerPopup : MonoBehaviour
    {
        private TextMeshPro _nameText;
        private TextMeshPro _scoreText;

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
            _nameText.fontSize = UIStyles.WORLD_BURGER_NAME_SIZE;
            _nameText.color = nameColor;
            _nameText.alignment = TextAlignmentOptions.Center;
            _nameText.textWrappingMode = TextWrappingModes.NoWrap;
            _nameText.overflowMode = TextOverflowModes.Overflow;
            _nameText.fontStyle = FontStyles.Bold;
            _nameText.sortingOrder = 110;
            _nameText.outlineWidth = UIStyles.OUTLINE_WIDTH_WORLD;
            _nameText.outlineColor = UIStyles.OUTLINE_COLOR;
            _nameText.rectTransform.sizeDelta = new Vector2(6f, 2f);

            // Score text (child object, below name)
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(transform);
            scoreObj.transform.localPosition = new Vector3(0, AnimConfig.BURGER_POPUP_SCORE_OFFSET_Y, 0);

            _scoreText = scoreObj.AddComponent<TextMeshPro>();
            _scoreText.text = $"+{points}";
            _scoreText.fontSize = UIStyles.WORLD_BURGER_SCORE_SIZE;
            _scoreText.color = Color.white;
            _scoreText.alignment = TextAlignmentOptions.Center;
            _scoreText.textWrappingMode = TextWrappingModes.NoWrap;
            _scoreText.overflowMode = TextOverflowModes.Overflow;
            _scoreText.sortingOrder = 110;
            _scoreText.outlineWidth = UIStyles.OUTLINE_WIDTH_WORLD;
            _scoreText.outlineColor = UIStyles.OUTLINE_COLOR;
            _scoreText.rectTransform.sizeDelta = new Vector2(4f, 1.5f);
        }

        private void Animate()
        {
            // Start at zero scale
            transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();

            // Pop in with overshoot
            seq.Append(transform.DOScale(AnimConfig.BURGER_POPUP_OVERSHOOT_SCALE, AnimConfig.BURGER_POPUP_POP_DURATION).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(1f, AnimConfig.BURGER_POPUP_SETTLE_DURATION).SetEase(Ease.InOutQuad));

            // Hold
            seq.AppendInterval(AnimConfig.BURGER_POPUP_HOLD_DURATION);

            // Fade out and rise
            seq.Append(transform.DOMove(transform.position + Vector3.up * AnimConfig.BURGER_POPUP_RISE, AnimConfig.BURGER_POPUP_FADE_DURATION).SetEase(Ease.InCubic));
            seq.Join(DOTween.To(() => _nameText.alpha, x => _nameText.alpha = x, 0f, AnimConfig.BURGER_POPUP_FADE_DURATION));
            seq.Join(DOTween.To(() => _scoreText.alpha, x => _scoreText.alpha = x, 0f, AnimConfig.BURGER_POPUP_FADE_DURATION));
            seq.Join(transform.DOScale(AnimConfig.BURGER_POPUP_FADE_SCALE, AnimConfig.BURGER_POPUP_FADE_DURATION).SetEase(Ease.InCubic));

            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
