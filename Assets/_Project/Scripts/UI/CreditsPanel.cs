using UnityEngine;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The Credits panel (menu only), built to the mock on the shared ModalPanel chrome with its own
    /// (taller) sheet: three entries, each a colored role heading over the kit's checkered band
    /// (green / blue / orange, text-free and translucent) with the name on it.
    /// Layout knobs: UIStyles.CREDITS_*.
    /// </summary>
    public class CreditsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        // The credits themselves. Player-facing strings: letters, digits, space and ! , - . : ; ?
        // only (trial-font glyph limits — see CLAUDE.md). A name may span lines with explicit
        // newlines — it auto-fits inside the band (down to CREDITS_NAME_SIZE_MIN).
        // The music names are a CC-BY attribution requirement — see Docs/music-attribution.md
        // before editing them.
        private static readonly CreditsEntry[] Entries =
        {
            new("A GAME BY", "Oscar Torandell", UIStyles.CREDITS_GAME_ROLE, "ui_credits_band_game"),
            new("ART BY", "Lucia Varona", UIStyles.CREDITS_ART_ROLE, "ui_credits_band_art"),
            new("MUSIC BY", "SketchyLogic, BossLevelVGM,\nMartin Nilsson, Alex McCulloch,\nSpring Spring. Thanks!",
                UIStyles.CREDITS_MUSIC_ROLE, "ui_credits_band_music"),
        };

        private Canvas _canvas;
        private ModalPanel _modal;

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

            _modal.Show();
        }

        public void Hide()
        {
            _modal?.Hide();
        }

        private void CreatePanel()
        {
            _modal = ModalPanel.Build(_canvas, "CREDITS", "ui_credits_panel", Vector2.zero,
                UIStyles.CREDITS_CHROME_OFFSET, Hide);

            for (int i = 0; i < Entries.Length; i++)
                BuildEntry(Entries[i], UIStyles.CREDITS_FIRST_Y - i * UIStyles.CREDITS_PITCH);
        }

        // Heading, then the band art (sized by width, native aspect — its canvas includes a
        // transparent margin), then the name auto-fitting inside the band's face.
        private void BuildEntry(CreditsEntry entry, float headingY)
        {
            Transform root = _modal.Panel;

            TextMeshProUGUI role = UIFactory.CreateText(root, entry.Role, new Vector2(0f, headingY),
                UIStyles.CREDITS_ROLE_RECT, UIStyles.CREDITS_ROLE_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(role, entry.RoleColor, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);

            Sprite bandArt = UiArt.Load(entry.BandArt);
            Vector2 bandSize = UIFactory.SizeByWidth(bandArt, UIStyles.CREDITS_BAND_W);
            Vector2 bandPos = new(0f, headingY + UIStyles.CREDITS_BAND_DY);
            UIFactory.CreateImage(root, "Band", bandArt, Center, bandPos, bandSize);

            TextMeshProUGUI name = UIFactory.CreateText(root, entry.Name,
                bandPos + UIStyles.CREDITS_NAME_NUDGE, bandSize - UIStyles.CREDITS_NAME_INSET,
                UIStyles.CREDITS_NAME_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(name);
            UIFactory.AutoFit(name, UIStyles.CREDITS_NAME_SIZE_MIN, UIStyles.CREDITS_NAME_SIZE);
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }
    }
}
