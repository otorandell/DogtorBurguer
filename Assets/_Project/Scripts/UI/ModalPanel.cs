using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// The shared chrome of the small modal screens (Settings, Credits): a dim overlay, the
    /// authored full-canvas panel art (orange title tab + dotted cream body), the title word on
    /// the tab, the round red X over its corner, and the pop-in tween. Screens build their content
    /// under <see cref="Panel"/> and drive visibility through Show / Hide. Knobs: UIStyles.MODAL_*.
    /// </summary>
    public sealed class ModalPanel
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        /// <summary>The overlay root — everything lives under it; toggled by Show / Hide.</summary>
        public GameObject Root { get; }
        /// <summary>The panel art. Parent screen content here so the pop-in scales it too.</summary>
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
        /// <paramref name="panelOffset"/> nudges the whole panel art (canvas-centered px).</summary>
        public static ModalPanel Build(Canvas canvas, string title, Vector2 panelOffset, UnityAction onClose)
        {
            GameObject root = UIFactory.CreateOverlay(canvas.transform, UIStyles.MODAL_OVERLAY);
            CanvasGroup group = root.AddComponent<CanvasGroup>();

            // The panel art is a full-phone canvas: shown at the reference resolution it lands exactly
            // where the artist drew it. Everything else is a child so the pop-in scales the whole panel.
            Image art = UIFactory.CreateImage(root.transform, "Panel", UiArt.Load("ui_modal_panel"),
                Center, panelOffset, UIStyles.REFERENCE_RESOLUTION);
            Transform panel = art.transform;

            TextMeshProUGUI titleText = UIFactory.CreateText(panel, title, UIStyles.MODAL_TITLE_POS,
                UIStyles.MODAL_TITLE_RECT, UIStyles.MODAL_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(titleText);

            Sprite close = UiArt.Load("ui_btn_close_x");
            UIFactory.CreateSpriteButton(panel, "Close", close, Center, UIStyles.MODAL_CLOSE_POS,
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
