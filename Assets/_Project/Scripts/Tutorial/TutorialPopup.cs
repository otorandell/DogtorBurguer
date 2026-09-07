using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The tutorial callout, composed from existing art: the red SPECIAL ORDER banner blank as
    /// the title strip, the wide green glow plate as the text box, and a rotated yellow preview
    /// arrow as the pointer. One instance, restyled per step; also owns the SKIP button and the
    /// full-screen tap-to-continue overlay. Layout knobs: UIStyles.TUT_*.
    /// </summary>
    public class TutorialPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _box;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _body;
        private Image _arrow;
        private GameObject _continueOverlay;
        private TextMeshProUGUI _continueLabel;
        private Action _onContinue;

        public void Build(Action onSkip)
        {
            _canvas = UIFactory.CreateCanvas(transform, "TutorialCanvas", UIStyles.TUT_CANVAS_SORT);

            // The callout box: green plate + red title strip overhanging its top + body text.
            Sprite plate = UiArt.Load("ui_popup_plate_wide");
            Image boxImg = UIFactory.CreateImage(_canvas.transform, "TutorialBox", plate,
                new Vector2(0.5f, 0.5f), Vector2.zero, UIFactory.SizeByWidth(plate, UIStyles.TUT_BOX_W));
            _box = boxImg.rectTransform;

            Sprite banner = UiArt.Load("ui_special_title");
            Image bannerImg = UIFactory.CreateImage(_box, "Title", banner, new Vector2(0.5f, 1f),
                new Vector2(0f, UIStyles.TUT_TITLE_Y), UIFactory.SizeByWidth(banner, UIStyles.TUT_TITLE_W));
            _title = UIFactory.CreateText(bannerImg.transform, "", UIStyles.TUT_TITLE_NUDGE,
                bannerImg.rectTransform.sizeDelta, UIStyles.TUT_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_title);

            _body = UIFactory.CreateText(_box, "", new Vector2(0f, UIStyles.TUT_BODY_Y),
                new Vector2(UIStyles.TUT_BOX_W - UIStyles.TUT_BODY_INSET, UIStyles.TUT_BODY_H),
                UIStyles.TUT_BODY_SIZE, FontStyles.Bold, null, TextAlignmentOptions.Center, wrap: true);
            UIFactory.StyleHudText(_body);

            Sprite arrowArt = UiArt.Load("ui_arrow_yellow");
            _arrow = UIFactory.CreateImage(_canvas.transform, "Pointer", arrowArt,
                new Vector2(0.5f, 0.5f), Vector2.zero, UIFactory.SizeByHeight(arrowArt, UIStyles.TUT_ARROW_H));

            // Tap-to-continue: an invisible full-screen button + a pulsing prompt line.
            GameObject overlay = UIFactory.CreateOverlay(_canvas.transform, Color.clear);
            overlay.name = "ContinueOverlay";
            overlay.AddComponent<Button>().onClick.AddListener(() =>
            {
                Action cb = _onContinue;
                _onContinue = null;
                _continueOverlay.SetActive(false);
                _continueLabel.gameObject.SetActive(false);
                cb?.Invoke();
            });
            _continueOverlay = overlay;
            _continueLabel = UIFactory.CreateText(_box, "TAP TO CONTINUE",
                new Vector2(0f, UIStyles.TUT_CONTINUE_Y), new Vector2(UIStyles.TUT_BOX_W, 26f),
                UIStyles.TUT_CONTINUE_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(_continueLabel);
            _continueOverlay.SetActive(false);
            _continueLabel.gameObject.SetActive(false);

            // SKIP — small, always available, top-left (clear of the pills and the order card).
            TextMeshProUGUI skip = UIFactory.CreateText(_canvas.transform, "SKIP", Vector2.zero,
                new Vector2(90f, 36f), UIStyles.TUT_SKIP_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(skip);
            RectTransform skipRect = skip.rectTransform;
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(0f, 1f);
            skipRect.anchoredPosition = UIStyles.TUT_SKIP_POS;
            skip.raycastTarget = true;
            skip.gameObject.AddComponent<Button>().onClick.AddListener(() => onSkip?.Invoke());
        }

        /// <summary>Shows a step callout. The arrow points at the subject — pass a canvas
        /// position + z-rotation (0 keeps the art's native down-pointing direction).</summary>
        public void Show(string title, string body, Vector2 boxPos, Vector2 arrowPos, float arrowRot, bool arrowVisible = true)
        {
            _box.anchoredPosition = boxPos;
            _title.text = title;
            _body.text = body;
            _arrow.gameObject.SetActive(arrowVisible);
            _arrow.rectTransform.anchoredPosition = arrowPos;
            _arrow.rectTransform.localEulerAngles = new Vector3(0f, 0f, arrowRot);
            _continueOverlay.SetActive(false);
            _continueLabel.gameObject.SetActive(false);
        }

        /// <summary>Arms the full-screen tap: the next tap anywhere runs <paramref name="onTap"/>.</summary>
        public void ArmContinue(Action onTap)
        {
            _onContinue = onTap;
            _continueOverlay.SetActive(true);
            _continueLabel.gameObject.SetActive(true);
        }
    }
}
