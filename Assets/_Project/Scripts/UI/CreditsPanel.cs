using System.Collections.Generic;
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

        // The credits themselves — roles localized, names literal. A name may span explicit
        // newlines and auto-fits inside the band (down to CREDITS_NAME_SIZE_MIN). The music
        // names are a CC-BY attribution requirement — see Docs/music-attribution.md before
        // editing them. A property (not a cached static) so a language change is live.
        private static CreditsEntry[] Entries => new[]
        {
            new CreditsEntry(Loc.Get(LocKey.CreditsGameBy), "Oscar Torandell", UIStyles.CREDITS_GAME_ROLE, "ui_credits_band_game"),
            new CreditsEntry(Loc.Get(LocKey.CreditsArtBy), "Lucia Varona", UIStyles.CREDITS_ART_ROLE, "ui_credits_band_art"),
            new CreditsEntry(Loc.Get(LocKey.CreditsMusicBy), "SketchyLogic, BossLevelVGM,\nMartin Nilsson, Alex McCulloch,\nSpring Spring. Thanks!",
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

        // The visible FACE of each band inside its canvas, as fractions (x = left, y = top,
        // z = right, w = bottom) — alpha bbox, measured 2026-09-26. The three canvases differ in
        // size and margin (music's is far wider), so sizing them by canvas width drew the last band
        // visibly smaller and off-centre. Every band is now sized and placed by its face.
        private static readonly Dictionary<string, Vector4> BandFaces = new()
        {
            ["ui_credits_band_game"] = new Vector4(0.0429f, 0.1615f, 0.0423f, 0.1340f),
            ["ui_credits_band_art"] = new Vector4(0.0347f, 0.1168f, 0.0404f, 0.1111f),
            ["ui_credits_band_music"] = new Vector4(0.0874f, 0.1573f, 0.0801f, 0.1259f),
        };

        private void CreatePanel()
        {
            _modal = ModalPanel.Build(_canvas, Loc.Get(LocKey.CreditsTitle), "ui_credits_panel", Vector2.zero,
                UIStyles.CREDITS_CHROME_OFFSET, Hide);

            // CREDITS: caps centred on the header strip (Capline — the line box's ascender room
            // would sit the word off-centre). Credits-only; Settings keeps its own placement.
            _modal.Title.alignment = TextAlignmentOptions.Capline;
            _modal.Title.rectTransform.anchoredPosition = new Vector2(0f, UIStyles.CREDITS_TITLE_Y);

            // Every section (heading overhang + band face) gets the same share of the body: equal
            // gaps above the first, between each, and below the last.
            float faceH = UIStyles.CREDITS_BAND_FACE_W * UIStyles.CREDITS_BAND_FACE_ASPECT;
            float sectionH = UIStyles.CREDITS_ROLE_OVERHANG + faceH;
            float bodyH = UIStyles.CREDITS_BODY_TOP - UIStyles.CREDITS_BODY_BOTTOM;
            float gap = (bodyH - Entries.Length * sectionH) / (Entries.Length + 1);
            for (int i = 0; i < Entries.Length; i++)
            {
                float faceTop = UIStyles.CREDITS_BODY_TOP - gap - UIStyles.CREDITS_ROLE_OVERHANG - i * (sectionH + gap);
                BuildEntry(Entries[i], faceTop, faceH);
            }
        }

        // The band (placed so its FACE spans CREDITS_BAND_FACE_W, top edge at faceTop), the role
        // heading centred on that top edge, and the name auto-fitting inside the face below it.
        private void BuildEntry(CreditsEntry entry, float faceTop, float faceH)
        {
            Transform root = _modal.Panel;
            Sprite bandArt = UiArt.Load(entry.BandArt);
            Vector4 m = BandFaces.TryGetValue(entry.BandArt, out Vector4 inset) ? inset : Vector4.zero;

            float canvasW = UIStyles.CREDITS_BAND_FACE_W / (1f - m.x - m.z);
            Vector2 canvasSize = new(canvasW, canvasW * bandArt.rect.height / bandArt.rect.width);
            float faceCenterY = faceTop - faceH * 0.5f;
            // Face centre = canvas centre + the margin imbalance; solve for the canvas centre.
            Vector2 faceOffset = new((m.x - m.z) * 0.5f * canvasSize.x, (m.w - m.y) * 0.5f * canvasSize.y);
            UIFactory.CreateImage(root, "Band", bandArt, Center, new Vector2(0f, faceCenterY) - faceOffset, canvasSize);

            Vector4 pad = UIStyles.CREDITS_NAME_PAD; // left, top, right, bottom inside the face
            Vector2 nameSize = new(UIStyles.CREDITS_BAND_FACE_W - pad.x - pad.z, faceH - pad.y - pad.w);
            Vector2 namePos = new((pad.x - pad.z) * 0.5f, faceCenterY + (pad.w - pad.y) * 0.5f);
            // Single names centre their caps (Capline); the multi-line music list centres its block.
            TextAlignmentOptions nameAlign = entry.Name.Contains('\n') ? TextAlignmentOptions.Center : TextAlignmentOptions.Capline;
            TextMeshProUGUI name = UIFactory.CreateText(root, entry.Name, namePos, nameSize,
                UIStyles.CREDITS_NAME_SIZE, FontStyles.Bold, alignment: nameAlign);
            UIFactory.StyleHudText(name);
            UIFactory.AutoFit(name, UIStyles.CREDITS_NAME_SIZE_MIN, UIStyles.CREDITS_NAME_SIZE);

            // Built last so it draws over the band's top edge.
            TextMeshProUGUI role = UIFactory.CreateText(root, entry.Role, new Vector2(0f, faceTop),
                new Vector2(UIStyles.CREDITS_BAND_FACE_W, UIStyles.CREDITS_ROLE_SIZE * 2f),
                UIStyles.CREDITS_ROLE_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleFillAndBorder(role, entry.RoleColor, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);
            UIFactory.AutoFit(role, UIStyles.CREDITS_ROLE_SIZE_MIN, UIStyles.CREDITS_ROLE_SIZE);
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }
    }
}
