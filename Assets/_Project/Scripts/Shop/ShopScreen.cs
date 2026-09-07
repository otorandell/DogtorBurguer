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
        private RectTransform _page;
        private TopBar _topBar;
        private GameObject _dialog;
        private GameObject _purchaseBlocker;

        /// <summary>The gem pill transform — deny-shake target for failed gem spends.</summary>
        public Transform GemPill => _topBar.GemPill;

        private RectTransform _scrollContent;

        /// <summary>The POWER-UPS section title — set by ShopSections each build; the consumable
        /// plus-box deep link jumps the scroll here.</summary>
        public RectTransform PowerUpsAnchor { get; set; }

        public static void Open(Action onClosed = null)
        {
            if (_openInstance != null) return;

            GameObject obj = new GameObject("ShopScreen");
            _openInstance = obj.AddComponent<ShopScreen>();
            _openInstance._onClosed = onClosed;
            _openInstance.Build();
        }

        /// <summary>Opens from in-game: pauses a running game, resumes it on close. The
        /// consumable plus-box deep link passes <paramref name="scrollToPowerUps"/> to open the
        /// page already POSITIONED at the POWER-UPS section (instant, no scroll animation).</summary>
        public static void OpenInGame(bool scrollToPowerUps = false)
        {
            GameManager manager = GameManager.Instance;
            bool pause = manager != null && manager.CurrentState == GameState.Playing && !manager.IsPaused;
            if (pause) manager.PauseGame();
            Open(() => { if (pause) GameManager.Instance?.ResumeGame(); });
            if (scrollToPowerUps) _openInstance?.JumpToPowerUps();
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

#if UNITY_EDITOR
        private bool _rWasDown;
        private bool _announcedR;
#endif

        private void Update()
        {
#if UNITY_EDITOR
            // Debug: R wipes shop purchases/consumables/tutorial-seen. Manual edge detection on
            // isPressed — wasPressedThisFrame proved unreliable in the paused-shop context
            // (2026-09-07: V/F in TouchInputHandler fired, this handler never did).
            if (!_announcedR)
            {
                _announcedR = true;
                Debug.Log("[ShopScreen] R listener alive (Game view must be focused).");
            }
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool rDown = keyboard != null && keyboard.rKey.isPressed;
            if (rDown && !_rWasDown)
            {
                SaveDataManager.Instance?.DebugResetShop();
                Theme.DebugResetToDefaults();
                NotifyChanged();
                Debug.Log("[ShopScreen] Debug reset: skins, equips, remove-ads, consumables and tutorial-seen wiped.");
            }
            _rWasDown = rDown;
#endif
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

        /// <summary>Blocks all shop input while a store purchase is in flight. Real store sheets
        /// block input themselves, but the editor's fake-store dialog is IMGUI — clicks pass
        /// through it onto the UGUI shop (e.g. its Buy button sits over the STARS grid) — and on
        /// device this also kills double-tap races while the sheet animates in.</summary>
        public void SetPurchaseBlocker(bool on)
        {
            if (on && _purchaseBlocker == null)
                _purchaseBlocker = UIFactory.CreateOverlay(_canvas.transform, UIStyles.SHOP_PURCHASE_BLOCKER);
            else if (!on && _purchaseBlocker != null)
            {
                Destroy(_purchaseBlocker);
                _purchaseBlocker = null;
            }
        }

        /// <summary>Denied-transaction feedback (insufficient funds): shake the offender.</summary>
        public static void Deny(Transform target)
        {
            AudioManager.Instance?.PlayDeny();
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

            _scrollContent = ShopWidgets.CreateVerticalScroll(_page,
                UIStyles.SHOP_SCROLL_TOP, UIStyles.SHOP_SCROLL_BOTTOM, UIStyles.SHOP_SCROLL_SIDE);
            ShopSections.BuildAll(_scrollContent, this);

            // Store grants can land outside a tap (a restore, a replayed purchase) — re-render then too.
            if (IapManager.Instance != null)
                IapManager.Instance.OnGranted += HandleGranted;
        }

        private void HandleGranted(string storeId) => NotifyChanged();

        // Instant jump so the POWER-UPS title sits at the viewport top. Layout is forced first —
        // the shop was built this same frame and the vertical layout hasn't run yet.
        private void JumpToPowerUps()
        {
            if (PowerUpsAnchor == null || _scrollContent == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollContent);
            float childTop = PowerUpsAnchor.localPosition.y
                + PowerUpsAnchor.rect.height * (1f - PowerUpsAnchor.pivot.y);
            RectTransform viewport = (RectTransform)_scrollContent.parent;
            float maxScroll = Mathf.Max(0f, _scrollContent.rect.height - viewport.rect.height);
            Vector2 pos = _scrollContent.anchoredPosition;
            pos.y = Mathf.Clamp(-childTop, 0f, maxScroll);
            _scrollContent.anchoredPosition = pos;
        }

        // The page art is a full-phone canvas (SHOP baked on the awning): shown at the reference
        // resolution it lands where drawn. The round X over the awning's corner, and the shared
        // TopBar dropped into the page below it. Everything hangs off a PAGE ROOT at the
        // reference size (the ModalPanel pattern), not the canvas: on phones taller than 9:16 the
        // canvas outgrows the page (match-width scaling), and canvas-anchored chrome floated above
        // the awning while the scroll viewport spilled past the page (first device test, 2026-09-06).
        private void BuildPage()
        {
            GameObject pageObj = new GameObject("Page");
            pageObj.transform.SetParent(_canvas.transform, false);
            _page = pageObj.AddComponent<RectTransform>();
            _page.anchorMin = Center;
            _page.anchorMax = Center;
            _page.sizeDelta = UIStyles.REFERENCE_RESOLUTION;

            UIFactory.CreateImage(_page, "Art", UiArt.Load("ui_shop_page"), Center, Vector2.zero,
                UIStyles.REFERENCE_RESOLUTION);

            Sprite close = UiArt.Load("ui_btn_close_x");
            UIFactory.CreateSpriteButton(_page, "Close", close, TopCenter, UIStyles.SHOP_CLOSE_POS,
                UIFactory.SizeByHeight(close, UIStyles.SHOP_CLOSE_H), Close);

            _topBar = TopBar.Build(_page);
            _topBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(UIStyles.SHOP_TOPBAR_X_NUDGE, -UIStyles.SHOP_TOPBAR_DROP);
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
