using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The authored stat card — dotted cream panel (ui_panel_card) + blank red title tab
    /// (ui_title_tab) with the word as TMP + the big HUD-palette number. One recipe shared by the
    /// in-game HUD (Level/Score) and the Game Over screen so the cards match everywhere;
    /// <c>scale</c> enlarges the whole card proportionally (art, offsets and fonts).
    /// Layout knobs: UIStyles.HUD_PANEL_* / HUD_TITLE_*. The blank-tab + TMP word is a placeholder
    /// until the artist's per-word tab art arrives.
    /// </summary>
    public static class StatCard
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        /// <summary>Builds a card under <paramref name="parent"/> and returns its number label.</summary>
        public static TextMeshProUGUI Build(Transform parent, string name, string title,
            Vector2 anchor, Vector2 pos, float scale = 1f)
        {
            Image card = UIFactory.CreateImage(parent, name, UiArt.Load("ui_panel_card"),
                anchor, pos, UIStyles.HUD_PANEL_SIZE * scale);

            // Size the tab by height, width following native aspect — never force a size (distorts).
            Vector2 tabSize = UIFactory.SizeByHeight(UiArt.Load("ui_title_tab"), UIStyles.HUD_PANEL_TITLE_HEIGHT * scale);
            Image tab = UIFactory.CreateImage(card.transform, "Title", UiArt.Load("ui_title_tab"),
                Center, new Vector2(0f, UIStyles.HUD_PANEL_TITLE_Y * scale), tabSize);

            TextMeshProUGUI titleLabel = UIFactory.CreateText(tab.transform, title, Vector2.zero,
                tabSize, UIStyles.HUD_TITLE_LABEL_SIZE * scale, FontStyles.Bold);
            UIFactory.StyleHudText(titleLabel);
            UIFactory.AutoFit(titleLabel, UIStyles.HUD_TITLE_LABEL_SIZE_MIN * scale, UIStyles.HUD_TITLE_LABEL_SIZE * scale);

            TextMeshProUGUI number = UIFactory.CreateText(card.transform, "0",
                new Vector2(0f, UIStyles.HUD_PANEL_NUMBER_Y * scale),
                new Vector2(UIStyles.HUD_PANEL_NUMBER_W * scale, UIStyles.HUD_PANEL_SIZE.y * scale),
                UIStyles.HUD_PANEL_NUMBER_SIZE * scale, FontStyles.Bold);
            number.gameObject.name = "Number";
            UIFactory.StyleHudText(number);
            UIFactory.AutoFit(number, UIStyles.HUD_PANEL_NUMBER_SIZE_MIN * scale, UIStyles.HUD_PANEL_NUMBER_SIZE * scale);
            return number;
        }
    }
}
