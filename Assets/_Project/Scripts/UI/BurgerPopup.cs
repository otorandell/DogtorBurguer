using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class BurgerPopup : MonoBehaviour
    {
        private TextMeshPro _nameText;
        private TextMeshPro _scoreText;

        /// <summary>Creates a burger-name + score popup at the world position and animates it.</summary>
        public static BurgerPopup Spawn(Vector3 position, int points, string burgerName, Color nameColor)
        {
            GameObject obj = new GameObject("BurgerPopup");
            obj.transform.position = position;

            BurgerPopup popup = obj.AddComponent<BurgerPopup>();
            popup.CreateTexts(burgerName, points, nameColor);
            popup.Animate();
            return popup;
        }

        private void CreateTexts(string burgerName, int points, Color nameColor)
        {
            // Burger name (main text) — on this popup's own GameObject
            _nameText = WorldTextFactory.Create(gameObject, burgerName,
                UIStyles.WORLD_BURGER_NAME_SIZE, nameColor, Constants.SORT_BURGER_POPUP,
                UIStyles.BURGER_POPUP_NAME_RECT, FontStyles.Bold, UIStyles.OUTLINE_WIDTH_WORLD);

            // Score text (child object, below name)
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(transform, false);
            scoreObj.transform.localPosition = new Vector3(0, AnimConfig.BURGER_POPUP_SCORE_OFFSET_Y, 0);

            _scoreText = WorldTextFactory.Create(scoreObj, $"+{points}",
                UIStyles.WORLD_BURGER_SCORE_SIZE, UIStyles.TEXT_UI, Constants.SORT_BURGER_POPUP,
                UIStyles.BURGER_POPUP_SCORE_RECT, FontStyles.Normal, UIStyles.OUTLINE_WIDTH_WORLD);
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
            seq.Join(_nameText.DOFade(0f, AnimConfig.BURGER_POPUP_FADE_DURATION));
            seq.Join(_scoreText.DOFade(0f, AnimConfig.BURGER_POPUP_FADE_DURATION));
            seq.Join(transform.DOScale(AnimConfig.BURGER_POPUP_FADE_SCALE, AnimConfig.BURGER_POPUP_FADE_DURATION).SetEase(Ease.InCubic));

            seq.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (_nameText != null) _nameText.DOKill();
            if (_scoreText != null) _scoreText.DOKill();
        }
    }
}
