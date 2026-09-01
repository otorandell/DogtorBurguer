using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// The shared chrome of the small modal screens (Settings, Credits): a dim overlay, an authored
    /// full-canvas panel sheet (orange title tab + dotted cream body — each screen passes its own),
    /// the title word on the tab, the round red X over its corner, and the pop-in tween. Screens
    /// build their content under <see cref="Panel"/> and drive visibility through Show / Hide.
    /// Knobs: UIStyles.MODAL_* (title/X positions are for the Settings sheet; a screen whose sheet
    /// draws the tab elsewhere passes a chrome offset).
    /// </summary>
    public sealed class ModalPanel
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        /// <summary>The overlay root — everything lives under it; toggled by Show / Hide.</summary>
        public GameObject Root { get; }
        /// <summary>The panel's content root (the art sits under it). Parent screen content here so
        /// the pop-in scales it too; positions are in panel px (canvas-centered + panelOffset).</summary>
        public Transform Panel { get; }

        private readonly CanvasGroup _group;

        private ModalPanel(GameObject root, CanvasGroup group, Transform panel)
        {
            Root = root;
            _group = group;
            Panel = panel;
        }

        /// <summary>Builds the chrome into <paramref name="canvas"/>. It comes back ACTIVE — screens
        /// build lazily from their Show, add content, then call <see cref="Show"/>. (TMP assigns its
        /// default font in Awake, which never runs on an inactive hierarchy, so texts added to a
        /// deactivated panel would have a null font — StyleFillAndBorder reads it.)
        /// <paramref name="panelArt"/> is the screen's full-canvas sheet; <paramref name="panelOffset"/>
        /// nudges the whole panel (canvas-centered px); <paramref name="chromeOffset"/> moves just the
        /// title + X for a sheet whose tab sits elsewhere than the Settings sheet's.</summary>
        public static ModalPanel Build(Canvas canvas, string title, string panelArt, Vector2 panelOffset,
            Vector2 chromeOffset, UnityAction onClose)
        {
            GameObject root = UIFactory.CreateOverlay(canvas.transform, UIStyles.MODAL_OVERLAY);
            CanvasGroup group = root.AddComponent<CanvasGroup>();

            // Content root: a plain rect at the reference size; everything (art included) is a child so
            // the pop-in scales the whole panel and screen content is laid out in panel px.
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(root.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Center;
            panelRect.anchorMax = Center;
            panelRect.anchoredPosition = panelOffset;
            panelRect.sizeDelta = UIStyles.REFERENCE_RESOLUTION;
            Transform panel = panelObj.transform;

            // The panel art is a full-phone canvas: shown at the reference resolution it lands exactly
            // where the artist drew it.
            UIFactory.CreateImage(panel, "Art", UiArt.Load(panelArt), Center, Vector2.zero, UIStyles.REFERENCE_RESOLUTION);

            TextMeshProUGUI titleText = UIFactory.CreateText(panel, title, UIStyles.MODAL_TITLE_POS + chromeOffset,
                UIStyles.MODAL_TITLE_RECT, UIStyles.MODAL_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(titleText);

            Sprite close = UiArt.Load("ui_btn_close_x");
            UIFactory.CreateSpriteButton(panel, "Close", close, Center, UIStyles.MODAL_CLOSE_POS + chromeOffset,
                UIFactory.SizeByHeight(close, UIStyles.MODAL_CLOSE_H), onClose);

            return new ModalPanel(root, group, panel);
        }

        /// <summary>Activates the root and replays the pop-in (unscaled — the in-game opener pauses the run).</summary>
        public void Show()
        {
            Root.SetActive(true);
            Kill();
            _group.alpha = 0f;
            Panel.localScale = Vector3.one * AnimConfig.PANEL_START_SCALE;

            DOTween.Sequence()
                .Append(_group.DOFade(1f, AnimConfig.PANEL_FADE_DURATION))
                .Join(Panel.DOScale(1f, AnimConfig.PANEL_SCALE_DURATION).SetEase(Ease.OutBack))
                .SetUpdate(true);
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        /// <summary>Stops the pop-in tweens; call from the owner's OnDestroy.</summary>
        public void Kill()
        {
            if (_group != null) _group.DOKill();
            if (Panel != null) Panel.DOKill();
        }
    }
}
