using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The full-screen Shop overlay — one vertically scrolling page (skins, power-ups, currency
    /// packs, remove-ads) under a fixed header with both currency balances. Code-built on its own
    /// canvas, openable from the main menu and from the in-game HUD (<see cref="OpenInGame"/>
    /// pauses the run and resumes it on close). Rebuilt each open, destroyed on close — no stale
    /// state. Frame + orchestration only: sections live in ShopSections, purchase rules in
    /// ShopService, low-level construction in ShopWidgets.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        private static ShopScreen _openInstance;

        private readonly List<Action> _refreshers = new();
        private Action _onClosed;
        private Canvas _canvas;
        private TextMeshProUGUI _starNumber;
        private TextMeshProUGUI _gemNumber;
        private GameObject _dialog;

        /// <summary>The gem pill transform — deny-shake target for failed gem spends.</summary>
        public Transform GemPill => _gemNumber.transform.parent;

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

        private readonly List<Action> _perFrameTicks = new();

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

        /// <summary>Confirm gate for hard-currency (gem) spends. Soft spends never see this.</summary>
        public void ShowConfirm(string message, Action onConfirm)
        {
            CloseDialog();
            _dialog = UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DARK);

            GameObject panel = UIFactory.CreatePanel(_dialog.transform, UIStyles.SHOP_CONFIRM_PANEL_SIZE, UIStyles.PANEL_BG);
            UIFactory.CreateText(panel.transform, message, UIStyles.SHOP_CONFIRM_TEXT_POS,
                UIStyles.SHOP_CONFIRM_TEXT_RECT, UIStyles.SHOP_CONFIRM_TEXT_SIZE);
            UIFactory.CreateButton(panel.transform, "Buy", new Vector2(-UIStyles.SHOP_CONFIRM_BTN_X, UIStyles.SHOP_CONFIRM_BTN_Y),
                UIStyles.SHOP_CONFIRM_BTN_SIZE, UIStyles.BTN_SHOP_BUY, UIStyles.PANEL_BUTTON_TEXT_SIZE,
                () => { CloseDialog(); onConfirm(); });
            UIFactory.CreateButton(panel.transform, "Cancel", new Vector2(UIStyles.SHOP_CONFIRM_BTN_X, UIStyles.SHOP_CONFIRM_BTN_Y),
                UIStyles.SHOP_CONFIRM_BTN_SIZE, UIStyles.BTN_CLOSE, UIStyles.PANEL_BUTTON_TEXT_SIZE, CloseDialog);
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

            UIFactory.CreateOverlay(_canvas.transform, UIStyles.SHOP_BG);
            BuildHeader();

            RectTransform content = ShopWidgets.CreateVerticalScroll(_canvas.transform, UIStyles.SHOP_HEADER_H);
            ShopSections.BuildAll(content, this);

            SaveDataManager save = SaveDataManager.Instance;
            if (save != null)
            {
                save.OnStarsChanged += HandleStarsChanged;
                save.OnGemsChanged += HandleGemsChanged;
            }
            RefreshBalances();
        }

        private void BuildHeader()
        {
            TextMeshProUGUI title = UIFactory.CreateText(_canvas.transform, "SHOP", Vector2.zero,
                UIStyles.SHOP_TITLE_RECT, UIStyles.SHOP_TITLE_SIZE, FontStyles.Bold);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = UIStyles.SHOP_TITLE_POS;
            UIFactory.StyleHudText(title);

            _starNumber = ShopWidgets.CreateCurrencyPill(_canvas.transform, "ui_star",
                new Vector2(0.5f, 1f), UIStyles.SHOP_STAR_PILL_POS);
            _gemNumber = ShopWidgets.CreateCurrencyPill(_canvas.transform, "ui_gem",
                new Vector2(0.5f, 1f), UIStyles.SHOP_GEM_PILL_POS);

            (GameObject closeObj, Button _, TextMeshProUGUI _) = UIFactory.CreateButton(
                _canvas.transform, "X", Vector2.zero, UIStyles.SHOP_CLOSE_SIZE, UIStyles.BTN_CLOSE,
                UIStyles.SHOP_CLOSE_TEXT_SIZE, Close);
            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = Vector2.one;
            closeRect.anchoredPosition = UIStyles.SHOP_CLOSE_POS;
        }

        private void HandleStarsChanged(int stars)
        {
            _starNumber.text = NumberFormat.Abbreviate(stars);
            Punch(_starNumber.transform.parent);
        }

        private void HandleGemsChanged(int gems)
        {
            _gemNumber.text = NumberFormat.Abbreviate(gems);
            Punch(_gemNumber.transform.parent);
        }

        private void RefreshBalances()
        {
            SaveDataManager save = SaveDataManager.Instance;
            _starNumber.text = NumberFormat.Abbreviate(save != null ? save.Stars : 0);
            _gemNumber.text = NumberFormat.Abbreviate(save != null ? save.Gems : 0);
        }

        // Runs on unscaled time — the in-game shop sits on a paused (timeScale 0) run.
        private static void Punch(Transform target)
        {
            target.DOKill(true);
            target.DOPunchScale(Vector3.one * AnimConfig.SHOP_PILL_PUNCH_SCALE,
                    AnimConfig.SHOP_PILL_PUNCH_DURATION, 6, 0.7f)
                .SetUpdate(true).SetLink(target.gameObject);
        }

        private void Close() => Destroy(gameObject);

        private void OnDestroy()
        {
            if (_openInstance == this) _openInstance = null;

            SaveDataManager save = SaveDataManager.Instance;
            if (save != null)
            {
                save.OnStarsChanged -= HandleStarsChanged;
                save.OnGemsChanged -= HandleGemsChanged;
            }

            _onClosed?.Invoke();
        }
    }
}
