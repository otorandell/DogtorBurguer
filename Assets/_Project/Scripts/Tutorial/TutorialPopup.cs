using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The tutorial callout, the artist's own art since 2026-09-26 (Fixes/TutorialReference.png):
    /// the cream text box (ui_tut_panel) with the red speech-bubble tag (ui_tut_tag) overlapping
    /// its top-left and carrying the step title, and a yellow preview arrow as the pointer —
    /// idle-bobbing, and steerable at a world object via
    /// <see cref="PointAtWorld"/>. One instance, restyled per step; also owns the SKIP button
    /// and the full-screen tap-to-continue overlay. Layout knobs: UIStyles.TUT_*.
    /// </summary>
    public class TutorialPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private Camera _camera;
        private RectTransform _box;
        private RectTransform _tag;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _body;
        private RectTransform _arrowRoot;
        private GameObject _continueOverlay;
        private TextMeshProUGUI _continueLabel;
        private Action _onContinue;

        public void Build(Action onSkip)
        {
            _canvas = UIFactory.CreateCanvas(transform, "TutorialCanvas", UIStyles.TUT_CANVAS_SORT);

            // The callout box: the cream panel, 9-SLICED (corners + bottom bevel keep their shape) so
            // it can take the band's full width or one half of it — see ApplySlot, which sizes and
            // places everything per step.
            Image boxImg = UIFactory.CreateImage(_canvas.transform, "TutorialBox", UiArt.Load("ui_tut_panel"),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one);
            boxImg.type = Image.Type.Sliced;
            boxImg.pixelsPerUnitMultiplier = 1f / UIStyles.TUT_BOX_ART_SCALE;
            _box = boxImg.rectTransform;

            // The red speech-bubble tag over the box's top-left, its tail dipping onto the box.
            Image tag = UIFactory.CreateImage(_box, "TitleTag", UiArt.Load("ui_tut_tag"), new Vector2(0f, 1f),
                Vector2.zero, Vector2.one);
            _tag = tag.rectTransform;
            _tag.pivot = new Vector2(0f, 1f);
            // The title sits on the bubble's body (the tail takes the lower-left, hence the nudge).
            _title = UIFactory.CreateText(_tag, "", Vector2.zero, Vector2.one,
                UIStyles.TUT_TITLE_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(_title);
            // The tag is fixed art and the titles are translated: shrink, never overflow.
            UIFactory.AutoFit(_title, UIStyles.TUT_TITLE_SIZE_MIN, UIStyles.TUT_TITLE_SIZE);

            // The body fills the box below the tag's overlap, above the tap-to-continue line
            // (insets set per slot in ApplySlot — the tag shrinks on a half-width box).
            _body = UIFactory.CreateText(_box, "", Vector2.zero, Vector2.one,
                UIStyles.TUT_BODY_SIZE, FontStyles.Bold, null, TextAlignmentOptions.Center, wrap: true);
            _body.rectTransform.anchorMin = Vector2.zero;
            _body.rectTransform.anchorMax = Vector2.one;
            UIFactory.StyleHudText(_body);
            // The box is a fixed piece of art and the step texts vary wildly in length across
            // seven languages, so the body shrinks to fit instead of overflowing its plate.
            // MUST be the Wrapped variant: plain AutoFit forces NoWrap, which turned the tips
            // into one endless sideways line (Oscar, 2026-09-13).
            UIFactory.AutoFitWrapped(_body, UIStyles.TUT_BODY_SIZE_MIN, UIStyles.TUT_BODY_SIZE);

            // The pointer: a positioned root + a child image that idle-bobs (so per-frame
            // follow and the bob tween never fight over one transform).
            GameObject arrowObj = new GameObject("Pointer");
            arrowObj.transform.SetParent(_canvas.transform, false);
            _arrowRoot = arrowObj.AddComponent<RectTransform>();
            _arrowRoot.anchorMin = _arrowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _arrowRoot.sizeDelta = Vector2.zero;
            Sprite arrowArt = UiArt.Load("ui_arrow_yellow");
            Image arrowImg = UIFactory.CreateImage(_arrowRoot, "ArrowArt", arrowArt,
                new Vector2(0.5f, 0.5f), Vector2.zero, UIFactory.SizeByHeight(arrowArt, UIStyles.TUT_ARROW_H));
            arrowImg.rectTransform.DOAnchorPosY(-UIStyles.TUT_ARROW_BOB, AnimConfig.TUT_ARROW_BOB_DURATION)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(arrowImg.gameObject);
            _camera = Camera.main;

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
            _continueLabel = UIFactory.CreateText(_box, Loc.Get(LocKey.TutTapToContinue),
                Vector2.zero, new Vector2(0f, 26f), UIStyles.TUT_CONTINUE_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(_continueLabel);
            RectTransform cont = _continueLabel.rectTransform;
            cont.anchorMin = new Vector2(0f, 0f);
            cont.anchorMax = new Vector2(1f, 0f);
            cont.anchoredPosition = new Vector2(0f, UIStyles.TUT_CONTINUE_ABOVE_BOTTOM);
            cont.sizeDelta = new Vector2(0f, 26f);
            ApplySlot(TutorialBoxSlot.Top);
            _continueOverlay.SetActive(false);
            _continueLabel.gameObject.SetActive(false);

            // SKIP — small, always available, top-left (clear of the pills and the order card).
            TextMeshProUGUI skip = UIFactory.CreateText(UIFactory.SafeRoot(_canvas), Loc.Get(LocKey.TutSkip), Vector2.zero,
                new Vector2(90f, 36f), UIStyles.TUT_SKIP_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(skip);
            RectTransform skipRect = skip.rectTransform;
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(0f, 1f);
            skipRect.anchoredPosition = UIStyles.TUT_SKIP_POS;
            skip.raycastTarget = true;
            skip.gameObject.AddComponent<Button>().onClick.AddListener(() => onSkip?.Invoke());
        }

        /// <summary>Shows a step callout in a band <paramref name="slot"/>. The arrow points at the
        /// subject — pass a canvas position + z-rotation (0 keeps the art's native down-pointing
        /// direction).</summary>
        public void Show(string title, string body, TutorialBoxSlot slot, Vector2 arrowPos, float arrowRot, bool arrowVisible = true)
        {
            ApplySlot(slot);
            _title.text = title;
            _body.text = body;
            _arrowRoot.gameObject.SetActive(arrowVisible);
            _arrowRoot.anchoredPosition = arrowPos;
            _arrowRoot.localEulerAngles = new Vector3(0f, 0f, arrowRot);
            _continueOverlay.SetActive(false);
            _continueLabel.gameObject.SetActive(false);
        }

        // Sizes and places the box in the HUD band above the board (TUT_BAND_*): the band's full
        // width, or one half of it. The tag keeps its aspect and shrinks on a half box; the box's
        // top drops by the tag's overhang so the tag's top stays inside the band.
        private void ApplySlot(TutorialBoxSlot slot)
        {
            float bandW = UIStyles.REFERENCE_RESOLUTION.x - 2f * UIStyles.TUT_BAND_SIDE_MARGIN;
            float boxW = slot == TutorialBoxSlot.Top ? bandW : (bandW - UIStyles.TUT_BAND_HALF_GAP) * 0.5f;

            Sprite tagArt = _tag.GetComponent<Image>().sprite;
            float tagW = Mathf.Min(UIStyles.TUT_TAG_W, boxW * UIStyles.TUT_TAG_MAX_W_FRAC);
            float tagScale = tagW / UIStyles.TUT_TAG_W;
            float tagH = tagW * tagArt.rect.height / tagArt.rect.width;
            float overhang = UIStyles.TUT_TAG_POS.y * tagScale;

            float boxTop = UIStyles.TUT_BAND_TOP - overhang;
            float boxH = boxTop - UIStyles.TUT_BAND_BOTTOM;
            float x = slot switch
            {
                TutorialBoxSlot.Left => -(bandW - boxW) * 0.5f,
                TutorialBoxSlot.Right => (bandW - boxW) * 0.5f,
                _ => 0f,
            };
            _box.sizeDelta = new Vector2(boxW, boxH);
            _box.anchoredPosition = new Vector2(x, boxTop - boxH * 0.5f);

            _tag.sizeDelta = new Vector2(tagW, tagH);
            _tag.anchoredPosition = new Vector2(UIStyles.TUT_TAG_POS.x * tagScale, overhang);
            _title.rectTransform.sizeDelta = new Vector2(tagW * UIStyles.TUT_TITLE_W_FRAC, tagH);
            _title.rectTransform.anchoredPosition = UIStyles.TUT_TITLE_NUDGE * tagScale;
            _title.fontSizeMax = UIStyles.TUT_TITLE_SIZE * tagScale;

            float bodyTopInset = tagH - overhang + UIStyles.TUT_BODY_TAG_CLEARANCE;
            _body.rectTransform.offsetMin = new Vector2(UIStyles.TUT_BODY_SIDE_INSET, UIStyles.TUT_BODY_BOTTOM_INSET);
            _body.rectTransform.offsetMax = new Vector2(-UIStyles.TUT_BODY_SIDE_INSET, -bodyTopInset);
            _body.fontSizeMax = slot == TutorialBoxSlot.Top ? UIStyles.TUT_BODY_SIZE : UIStyles.TUT_BODY_SIZE_HALF;
        }

        /// <summary>Shows/hides the pointer without disturbing the rest of the callout.</summary>
        public void SetArrowVisible(bool visible) => _arrowRoot.gameObject.SetActive(visible);

        /// <summary>Steers the arrow over a world position (call per frame to follow the chef).</summary>
        public void PointAtWorld(Vector3 worldPos)
        {
            if (_camera == null || !_arrowRoot.gameObject.activeSelf) return;
            Vector3 screen = _camera.WorldToScreenPoint(worldPos);
            _arrowRoot.anchoredPosition = new Vector2(
                (screen.x - Screen.width * 0.5f) / _canvas.scaleFactor,
                (screen.y - Screen.height * 0.5f) / _canvas.scaleFactor);
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
