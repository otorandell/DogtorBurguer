using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The Shop screen, to the mock: the authored page (striped awning with SHOP baked in, dotted
    /// cream body, our round X) over the dimmed game/menu, the shared currency pills inside the
    /// page, and one vertically
    /// scrolling body (skins, power-ups, currency packs, remove-ads). Code-built on its own canvas,
    /// openable from the main menu and from the in-game HUD (<see cref="OpenInGame"/> pauses the
    /// run and resumes it on close). Rebuilt each open, destroyed on close — no stale state.
    /// Frame + orchestration only: sections live in ShopSections, purchase rules in ShopService,
    /// low-level construction in ShopWidgets.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);
        private static readonly Vector2 TopCenter = new(0.5f, 1f);

        private static ShopScreen _openInstance;

        private readonly List<Action> _refreshers = new();
        private readonly List<Action> _perFrameTicks = new();
        private Action _onClosed;
        private Canvas _canvas;
        private TopBar _topBar;
        private GameObject _dialog;

        /// <summary>The gem pill transform — deny-shake target for failed gem spends.</summary>
        public Transform GemPill => _topBar.GemPill;

        public static void Open(Action onClosed = null)
        {
            if (_openInstance != null) return;

            GameObject obj = new GameObject("ShopScreen");
            _openInstance = obj.AddComponent<ShopScreen>();
            _openInstance._onClosed = onClosed;
            _openInstance.Build();
        }

        /// <summary>Opens from the in-game HUD: pauses a running game, resumes it on close.</summary>
        public static void OpenInGame()
        {
            GameManager manager = GameManager.Instance;
            bool pause = manager != null && manager.CurrentState == GameState.Playing && !manager.IsPaused;
            if (pause) manager.PauseGame();
            Open(() => { if (pause) GameManager.Instance?.ResumeGame(); });
        }

        /// <summary>Sections register their state re-render here; see <see cref="NotifyChanged"/>.</summary>
        public void RegisterRefresh(Action refresh) => _refreshers.Add(refresh);

        /// <summary>For state that changes while the shop just sits open (ad availability) —
        /// runs once now and then every frame. Keep ticks cheap and change-guarded.</summary>
        public void RegisterPerFrame(Action tick)
        {
            tick();
            _perFrameTicks.Add(tick);
        }

        private void Update()
        {
            foreach (Action tick in _perFrameTicks)
                tick();
        }

        /// <summary>Call after any successful transaction: re-renders every section's state.
        /// (Balance labels update separately, via the SaveDataManager currency events.)</summary>
        public void NotifyChanged()
        {
            foreach (Action refresh in _refreshers)
                refresh();
            AudioManager.Instance?.PlayConsumableCollect(); // placeholder purchase sound
        }

        /// <summary>Denied-transaction feedback (insufficient funds): shake the offender.</summary>
        public static void Deny(Transform target)
        {
            target.DOKill(true);
            target.DOShakePosition(AnimConfig.SHOP_DENY_DURATION, AnimConfig.SHOP_DENY_STRENGTH)
                .SetUpdate(true).SetLink(target.gameObject);
        }

        /// <summary>Confirm gate for hard-currency (gem) spends — "Buy N ★ / for N ◆", BUY / CANCEL
        /// on a cream card. Soft spends never see this.</summary>
        public void ShowConfirm(int amount, string amountIcon, int cost, string costIcon, Action onConfirm)
        {
            CloseDialog();
            _dialog = UIFactory.CreateOverlay(_canvas.transform, UIStyles.MODAL_OVERLAY);

            Sprite cardArt = UiArt.Load("ui_shop_confirm_card");
            Image card = UIFactory.CreateImage(_dialog.transform, "Card", cardArt, Center, Vector2.zero,
                UIFactory.SizeByWidth(cardArt, UIStyles.SHOP_CONFIRM_CARD_W));
            card.raycastTarget = true; // taps on the card don't fall through to the page
            Transform root = card.transform;

            ShopWidgets.CreateIconLine(root, "Line1", new Vector2(0f, UIStyles.SHOP_CONFIRM_LINE1_Y), UIStyles.SHOP_CONFIRM_LINE_RECT,
                $"Buy {amount}", UIStyles.SHOP_CONFIRM_TEXT_SIZE, amountIcon, UIStyles.SHOP_CONFIRM_ICON_H);
            ShopWidgets.CreateIconLine(root, "Line2", new Vector2(0f, UIStyles.SHOP_CONFIRM_LINE2_Y), UIStyles.SHOP_CONFIRM_LINE_RECT,
                $"for {cost}", UIStyles.SHOP_CONFIRM_TEXT_SIZE, costIcon, UIStyles.SHOP_CONFIRM_ICON_H);

            Button buy = ShopWidgets.CreatePill(root, "Buy", "ui_btn_confirm_buy", Center,
                new Vector2(-UIStyles.SHOP_CONFIRM_BTN_X, UIStyles.SHOP_CONFIRM_BTN_Y), UIStyles.SHOP_CONFIRM_BTN_W,
                () => { CloseDialog(); onConfirm(); });
            ShopWidgets.SetPillLabel(buy, "BUY", null);
            Button cancel = ShopWidgets.CreatePill(root, "Cancel", "ui_btn_confirm_cancel", Center,
                new Vector2(UIStyles.SHOP_CONFIRM_BTN_X, UIStyles.SHOP_CONFIRM_BTN_Y), UIStyles.SHOP_CONFIRM_BTN_W, CloseDialog);
            ShopWidgets.SetPillLabel(cancel, "CANCEL", null);
        }

        private void CloseDialog()
        {
            if (_dialog == null) return;
            Destroy(_dialog);
            _dialog = null;
        }

        private void Build()
        {
            _canvas = UIFactory.CreateCanvas(transform, "Shop_Canvas", UIStyles.SHOP_CANVAS_SORT);
            UIFactory.EnsureEventSystem();

            UIFactory.CreateOverlay(_canvas.transform, UIStyles.MODAL_OVERLAY);
            BuildPage();

            RectTransform content = ShopWidgets.CreateVerticalScroll(_canvas.transform,
                UIStyles.SHOP_SCROLL_TOP, UIStyles.SHOP_SCROLL_BOTTOM, UIStyles.SHOP_SCROLL_SIDE);
            ShopSections.BuildAll(content, this);

            // Store grants can land outside a tap (a restore, a replayed purchase) — re-render then too.
            if (IapManager.Instance != null)
                IapManager.Instance.OnGranted += HandleGranted;
        }

        private void HandleGranted(string storeId) => NotifyChanged();

        // The page art is a full-phone canvas (SHOP baked on the awning): shown at the reference
        // resolution it lands where drawn. The round X over the awning's corner, and the shared
        // TopBar dropped into the page below it.
        private void BuildPage()
        {
            UIFactory.CreateImage(_canvas.transform, "Page", UiArt.Load("ui_shop_page"), Center, Vector2.zero,
                UIStyles.REFERENCE_RESOLUTION);

            Sprite close = UiArt.Load("ui_btn_close_x");
            UIFactory.CreateSpriteButton(_canvas.transform, "Close", close, TopCenter, UIStyles.SHOP_CLOSE_POS,
                UIFactory.SizeByHeight(close, UIStyles.SHOP_CLOSE_H), Close);

            _topBar = TopBar.Build(_canvas.transform);
            _topBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -UIStyles.SHOP_TOPBAR_DROP);
        }

        private void Close() => Destroy(gameObject);

        private void OnDestroy()
        {
            if (IapManager.Instance != null)
                IapManager.Instance.OnGranted -= HandleGranted;
            if (_openInstance == this) _openInstance = null;
            _onClosed?.Invoke();
        }
    }
}
