using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The shared top status bar: high-score trophy + star + gem currency pills at fixed
    /// positions (UIStyles.TOPBAR_*), plus optional help ("?")/settings icon buttons. One recipe
    /// for every screen (game HUD, main menu, shop header) so the bar looks identical and
    /// stays put when screens change. Binds itself to the SaveDataManager currency events
    /// and punches a pill on change (unscaled time — the shop header sits on a paused run).
    /// </summary>
    public class TopBar : MonoBehaviour
    {
        private TextMeshProUGUI _starNumber;
        private TextMeshProUGUI _gemNumber;

        /// <summary>The gem pill transform — deny-shake target for failed gem spends.</summary>
        public Transform GemPill => _gemNumber.transform.parent;

        /// <summary>Builds the bar under a canvas. Null callbacks omit their icon button;
        /// <paramref name="settingsPos"/>/<paramref name="settingsSize"/> override the gear's slot
        /// (the menu's lone gear sits bigger and nearer center than the in-game shop+gear pair).</summary>
        public static TopBar Build(Transform canvas, Action onHelp = null, Action onSettings = null,
            Vector2? settingsPos = null, Vector2? settingsSize = null)
        {
            GameObject obj = new GameObject("TopBar");
            obj.transform.SetParent(canvas, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TopBar bar = obj.AddComponent<TopBar>();
            bar.BuildContents(onHelp, onSettings, settingsPos ?? UIStyles.TOPBAR_CONFIG_POS,
                settingsSize ?? UIStyles.TOPBAR_BUTTON_SIZE);
            return bar;
        }

        private void BuildContents(Action onHelp, Action onSettings, Vector2 settingsPos, Vector2 settingsSize)
        {
            TextMeshProUGUI highScoreNumber = BuildCurrencyWidget("HighScore", "ui_score_trophy",
                UIStyles.TOPBAR_SCORE_POS, UIStyles.TOPBAR_SCORE_ICON_H);
            _starNumber = BuildCurrencyWidget("Stars", "ui_star",
                UIStyles.TOPBAR_STAR_POS, UIStyles.TOPBAR_STAR_ICON_H);
            _gemNumber = BuildCurrencyWidget("Gems", "ui_gem",
                UIStyles.TOPBAR_GEM_POS, UIStyles.TOPBAR_GEM_ICON_H);

            if (onHelp != null)
            {
                // The "?" help button (replaced the in-game shop button 2026-09-05): the kit's
                // blank green square with a HUD-palette question mark on it.
                Button help = UIFactory.CreateSpriteButton(transform, "HelpButton", UiArt.Load("ui_btn_square_green"),
                    new Vector2(0f, 1f), UIStyles.TOPBAR_HELP_POS, UIStyles.TOPBAR_BUTTON_SIZE, () => onHelp());
                TextMeshProUGUI mark = UIFactory.CreateText(help.transform, "?", Vector2.zero,
                    UIStyles.TOPBAR_BUTTON_SIZE, UIStyles.HOWTO_BTN_TEXT_SIZE, FontStyles.Bold);
                UIFactory.StyleHudText(mark);
            }
            if (onSettings != null)
                UIFactory.CreateSpriteButton(transform, "ConfigButton", UiArt.Load("ui_config_button"),
                    new Vector2(0f, 1f), settingsPos, settingsSize, () => onSettings());

            SaveDataManager save = SaveDataManager.Instance;
            // High score only changes at game over, so a one-time seed is enough (no live event).
            highScoreNumber.text = NumberFormat.Abbreviate(save != null ? save.HighScore : 0);
            _starNumber.text = NumberFormat.Abbreviate(save != null ? save.Stars : 0);
            _gemNumber.text = NumberFormat.Abbreviate(save != null ? save.Gems : 0);
            if (save != null)
            {
                save.OnStarsChanged += HandleStarsChanged;
                save.OnGemsChanged += HandleGemsChanged;
            }
        }

        // A currency pill (baked box) with an overhanging icon and a number; returns the number label.
        private TextMeshProUGUI BuildCurrencyWidget(string name, string iconArt, Vector2 pos, float iconHeight)
        {
            Image box = UIFactory.CreateImage(transform, name, UiArt.Load("ui_currency_box"),
                new Vector2(0f, 1f), pos, UIStyles.TOPBAR_BOX_SIZE);

            // Size the icon by height, width following native aspect — never force a square (distorts).
            Sprite iconSprite = UiArt.Load(iconArt);
            float aspect = iconSprite != null ? iconSprite.rect.width / iconSprite.rect.height : 1f;
            UIFactory.CreateImage(box.transform, "Icon", iconSprite,
                new Vector2(0.5f, 0.5f), new Vector2(UIStyles.TOPBAR_ICON_X, 0f),
                new Vector2(iconHeight * aspect, iconHeight));

            TextMeshProUGUI number = UIFactory.CreateText(box.transform, "0",
                new Vector2(UIStyles.TOPBAR_NUMBER_X, UIStyles.TOPBAR_NUMBER_Y), UIStyles.TOPBAR_NUMBER_RECT,
                UIStyles.TOPBAR_NUMBER_SIZE, FontStyles.Bold, UIStyles.HUD_TEXT_FILL,
                TextAlignmentOptions.Center); // centered in the free zone regardless of digit count
            // HUD palette (cream + border + sticker shadow): the plain brown read poorly on the
            // busy pill art once everything else got the sticker lettering.
            UIFactory.StyleHudText(number);
            // Shrink-to-fit: the box stays fixed and the text scales down to stay inside it.
            number.textWrappingMode = TextWrappingModes.NoWrap;
            number.enableAutoSizing = true;
            number.fontSizeMin = UIStyles.TOPBAR_NUMBER_SIZE_MIN;
            number.fontSizeMax = UIStyles.TOPBAR_NUMBER_SIZE;
            return number;
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

        // Runs on unscaled time — the shop header's bar sits on a paused (timeScale 0) run.
        private static void Punch(Transform target)
        {
            target.DOKill(true);
            target.DOPunchScale(Vector3.one * AnimConfig.SHOP_PILL_PUNCH_SCALE,
                    AnimConfig.SHOP_PILL_PUNCH_DURATION, 6, 0.7f)
                .SetUpdate(true).SetLink(target.gameObject);
        }

        private void OnDestroy()
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save != null)
            {
                save.OnStarsChanged -= HandleStarsChanged;
                save.OnGemsChanged -= HandleGemsChanged;
            }
        }
    }
}
