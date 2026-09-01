using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The Credits panel (menu only), built to the mock on the shared ModalPanel chrome: three
    /// entries, each a colored role heading over a pastel checkered band with the name on it.
    /// The band is a procedural checker (SpriteFactory) tinted per entry — no band art in the kit.
    /// Layout knobs: UIStyles.CREDITS_*.
    /// </summary>
    public class CreditsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        // The credits themselves. Player-facing strings: letters, digits, space and ! , - . : ; ?
        // only (trial-font glyph limits — see CLAUDE.md).
        private static readonly CreditsEntry[] Entries =
        {
            new("A GAME BY", "Oscar Torandell", UIStyles.CREDITS_GAME_ROLE, UIStyles.CREDITS_GAME_BAND),
            new("ART BY", "Lucia Varona", UIStyles.CREDITS_ART_ROLE, UIStyles.CREDITS_ART_BAND),
            new("MUSIC BY", "Martin Nilsson", UIStyles.CREDITS_MUSIC_ROLE, UIStyles.CREDITS_MUSIC_BAND),
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
            _modal = ModalPanel.Build(_canvas, "CREDITS", UIStyles.CREDITS_PANEL_OFFSET, Hide);

            for (int i = 0; i < Entries.Length; i++)
                BuildEntry(Entries[i], UIStyles.CREDITS_FIRST_Y - i * UIStyles.CREDITS_PITCH);
        }

        // Heading, then the band, then the name (so the name renders over the band).
        private void BuildEntry(CreditsEntry entry, float headingY)
        {
            Transform root = _modal.Panel;

            TextMeshProUGUI role = UIFactory.CreateText(root, entry.Role, new Vector2(0f, headingY),
                UIStyles.CREDITS_ROLE_RECT, UIStyles.CREDITS_ROLE_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(role, entry.RoleColor, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);

            Image band = UIFactory.CreateImage(root, "Band",
                SpriteFactory.Checker(UIStyles.CREDITS_BAND_COLUMNS, UIStyles.CREDITS_BAND_ROWS),
                Center, new Vector2(0f, headingY + UIStyles.CREDITS_BAND_DY), UIStyles.CREDITS_BAND_SIZE);
            band.color = entry.BandColor;

            TextMeshProUGUI name = UIFactory.CreateText(root, entry.Name,
                new Vector2(0f, headingY + UIStyles.CREDITS_NAME_DY),
                UIStyles.CREDITS_NAME_RECT, UIStyles.CREDITS_NAME_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(name);
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }
    }
}
