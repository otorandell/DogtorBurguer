using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Visual style constants for all UI elements.
    /// Change these to adjust colors, sizes, and spacing across the game.
    /// </summary>
    public static class UIStyles
    {
        #region Canvas Setup
        public static readonly Vector2 REFERENCE_RESOLUTION = new(540, 960);
        // Match WIDTH (0) so the HUD scales by the same rule as the camera (CameraFit frames the
        // playfield by width), keeping the UI locked to the playfield across phone aspect ratios.
        public const float MATCH_WIDTH_OR_HEIGHT = 0f;
        #endregion

        #region Text Outlines
        public const float OUTLINE_WIDTH_UI = 0.2f;
        public const float OUTLINE_WIDTH_WORLD = 0.25f;
        public static readonly Color32 OUTLINE_COLOR = new(0, 0, 0, 255);
        #endregion

        #region Text Colors
        public static readonly Color TEXT_HUD = Color.black;
        public static readonly Color TEXT_UI = Color.white;
        public static readonly Color TEXT_FAST_DROP = Color.cyan;
        public static readonly Color TEXT_TOO_BAD = Color.red;
        #endregion

        #region Popup Colors
        public static readonly Color SCORE_POPUP = Color.yellow;
        public static readonly Color BURGER_POPUP = new(1f, 0.5f, 0f);
        public static readonly Color GOLD = new(1f, 0.85f, 0f);
        #endregion

        #region Panel / Overlay Colors
        public static readonly Color OVERLAY_DIM = new(0, 0, 0, 0.7f);
        public static readonly Color OVERLAY_DARK = new(0, 0, 0, 0.85f);
        public static readonly Color PANEL_BG = new(0.15f, 0.15f, 0.2f, 0.95f);
        public static readonly Color INNER_PANEL_BG = new(0.18f, 0.18f, 0.25f);
        public static readonly Color SCREEN_FLASH = new(1f, 1f, 1f, 0.6f);
        #endregion

        #region Button Colors
        public static readonly Color BTN_PLAY = new(0.2f, 0.8f, 0.3f);
        public static readonly Color BTN_SHOP = new(0.9f, 0.7f, 0.1f);
        public static readonly Color BTN_SETTINGS = new(0.4f, 0.6f, 0.9f);
        public static readonly Color BTN_LEADERBOARD = new(0.6f, 0.4f, 0.8f);
        public static readonly Color BTN_CLOSE = new(0.5f, 0.5f, 0.5f);
        public static readonly Color BTN_CONTINUE_GEMS = new(0.9f, 0.7f, 0.1f);
        public static readonly Color BTN_CONTINUE_AD = new(0.3f, 0.5f, 0.9f);
        public static readonly Color BTN_RESTART = new(0.2f, 0.7f, 0.3f);
        public static readonly Color BTN_SETTINGS_TOGGLE = new(0.3f, 0.5f, 0.7f);
        public static readonly Color BTN_SHOP_AD = new(0.3f, 0.5f, 0.9f);
        public static readonly Color BTN_SHOP_BUY = new(0.2f, 0.7f, 0.3f);
        #endregion

        #region Consumable / Fairy Sizes (world-space heights)
        // Reward sprites import large (100 PPU); everything is normalized to a target world height
        // via SpriteFit rather than a raw scale, so the source pixel size doesn't matter.
        public const float FAIRY_BODY_HEIGHT = 1.3f;
        public const float FAIRY_BADGE_HEIGHT = 1.1f;        // payload badge on the fairy (+50%)
        public const float CONSUMABLE_FALLER_HEIGHT = 2.0f;  // matches the column ghost (what it previews)
        public const float CONSUMABLE_GHOST_HEIGHT = 2.0f;   // column preview (+100%)
        public const float CONSUMABLE_GHOST_ALPHA = 0.5f;    // translucency of the column preview
        public const float CONSUMABLE_CARRY_HEIGHT = 1.8f;   // the lifted item that follows the finger
        #endregion

        #region HUD Consumable Slots (screen-space UGUI, below Level/Score — anchored top-left, reference px)
        // Three slots (Ketchup, Mustard, Skewer): a round plate + the consumable icon + a corner badge
        // (red num box with the count, or green plus box when empty). Left-aligned ≈ the Level/Score width.
        public static readonly Vector2 CONSUMABLE_SLOT_SIZE = new(80f, 80f);    // round plate
        public const float CONSUMABLE_SLOT_ICON_H = 58f;                        // consumable icon (width follows aspect)
        public static readonly Vector2 CONSUMABLE_ICON_OFFSET = new(0f, 3f);    // icon offset within the plate
        public const float CONSUMABLE_BADGE_H = 34f;                            // num/plus badge (width follows aspect)
        public static readonly Vector2 CONSUMABLE_BADGE_OFFSET = new(28f, -28f);// badge offset (bottom-right of plate)
        public const float CONSUMABLE_COUNT_SIZE = 22f;                         // count number on the num box
        public static readonly Color CONSUMABLE_COUNT_COLOR = new(0.96f, 0.93f, 0.82f); // cream
        public const float CONSUMABLE_ROW_Y = -233f;                            // row Y (margin below the Level/Score cards)
        // Span the same zone as Level/Score: 80-wide plates at 58/144/230 → left edge 18, right edge 270.
        public const float CONSUMABLE_SLOT_X_START = 58f;                       // first slot center X
        public const float CONSUMABLE_SLOT_SPACING = 86f;                       // gap between slot centers
        #endregion


        #region HUD Stat Panels (authored Level/Score cards — anchored top-left, reference px)
        // Baked fixed-size art (dotted card ui_panel_card). Keep box sizes at the art's native
        // aspect or the halftone dots smear (card art is 500x380 ≈ 1.32:1).
        public static readonly Vector2 HUD_PANEL_SIZE = new(122f, 92f);    // the cream card (≈ native 1.32:1)
        // Placeholder title tab: the blank no_tex red tab (ui_title_tab) with the word written on it
        // as TMP — swapped for the artist's final per-word art when it arrives. Sized by HEIGHT with
        // width following the native aspect (≈1.84:1) so it never stretches; raise the height to widen.
        public const float HUD_PANEL_TITLE_HEIGHT = 56f;                   // red title tab height (width follows aspect; +16%)
        public const float HUD_PANEL_TITLE_Y = 30f;                        // tab offset up within the card
        public const float HUD_TITLE_LABEL_SIZE = 12f;                     // tab word TMP font (auto-size max)
        public const float HUD_TITLE_LABEL_SIZE_MIN = 8f;                  // auto-size floor for the tab word
        public static readonly Color HUD_TITLE_LABEL_COLOR = new(0.96f, 0.93f, 0.82f); // cream
        public const float HUD_PANEL_NUMBER_SIZE = 30f;                    // the big number font (auto-size max)
        public const float HUD_PANEL_NUMBER_SIZE_MIN = 14f;                // auto-size floor for the number
        public const float HUD_PANEL_NUMBER_Y = -18f;                      // number offset down within the card
        public static readonly Color HUD_PANEL_NUMBER_COLOR = new(0.28f, 0.17f, 0.1f);
        // Shared left-column zone: 18px left margin → right edge at half the screen (270). Two 122-wide
        // cards, ~8 gap. The consumable row spans this same zone (start/end aligned).
        public static readonly Vector2 HUD_LEVEL_PANEL_POS = new(79f, -125f);
        public static readonly Vector2 HUD_SCORE_PANEL_POS = new(209f, -125f);
        #endregion

        #region HUD Special Order panel (screen-space UGUI, top-right — anchored top-right, reference px)
        // Card + the SPECIAL ORDER banner overhanging its top-left, the required-burger stack (on a
        // plate), and a multiplier badge. The card is stretched taller than its native aspect to about
        // the height of the left column (Score + consumables) — deliberate, it's a completed burger.
        // Height matches the left column: top aligns with the Level/Score top, bottom with the
        // consumables bottom (≈194 tall here) — stretched past native aspect on purpose.
        public static readonly Vector2 SPECIAL_CARD_SIZE = new(228f, 194f);   // cream card (stretched taller)
        public static readonly Vector2 SPECIAL_CARD_POS = new(-128f, -176f);  // anchored top-right
        public const float SPECIAL_BANNER_H = 60f;                            // SPECIAL ORDER banner (width follows aspect)
        public static readonly Vector2 SPECIAL_BANNER_OFFSET = new(-40f, 78f);// banner offset within the card (overhangs top-left)
        public const float SPECIAL_BANNER_LABEL_SIZE = 13f;                   // "SPECIAL ORDER" TMP (auto-size max)
        public const float SPECIAL_BANNER_LABEL_SIZE_MIN = 7f;                // auto-size floor
        public static readonly Vector2 SPECIAL_BANNER_LABEL_OFFSET = new(0f, 5f); // up a touch to clear the bubble tail
        public const float SPECIAL_INGREDIENT_H = 38f;                        // each stack ingredient (width follows aspect)
        public const float SPECIAL_INGREDIENT_SPACING = 26f;                  // vertical stack spacing (overlap)
        public const float SPECIAL_STACK_Y = -12f;                            // stack center Y within the card
        public const float SPECIAL_PLACEHOLDER_LABEL_SIZE = 18f;              // "+N" on the mystery silhouette
        public const float SPECIAL_PLATE_H = 18f;                             // plate under the bottom bun (width follows aspect)
        public const float SPECIAL_PLATE_Y_OFFSET = 13f;                      // plate drop below the bottom bun
        public const float SPECIAL_MULT_BADGE_H = 42f;                        // multiplier badge (reuses the red num box)
        public static readonly Vector2 SPECIAL_MULT_BADGE_OFFSET = new(86f, -60f); // badge offset within the card (bottom-right)
        public const float SPECIAL_MULT_TEXT_SIZE = 20f;
        #endregion

        #region HUD Top Bar (authored currency widgets + buttons — anchored top-left, reference px)
        public const float TOPBAR_Y = -38f;                                // vertical center of the bar
        public static readonly Vector2 TOPBAR_BOX_SIZE = new(88f, 42f);    // currency pill (ui_currency_box ≈ native 2.12:1) — shrunk 20%
        // Per-icon HEIGHT — width follows the sprite's native aspect (forcing a square distorted the
        // non-square trophy/star). The art has different visual weight; all overhang the pill's left.
        public const float TOPBAR_SCORE_ICON_H = 69f; // high-score trophy (wide art)
        public const float TOPBAR_STAR_ICON_H = 62f;  // star — a touch smaller
        public const float TOPBAR_GEM_ICON_H = 78f;   // gem/diamond — a touch bigger
        public const float TOPBAR_ICON_X = -44f;                           // icon offset within the widget (overhangs the pill's left edge)
        public const float TOPBAR_NUMBER_X = 18f;                          // number offset — left-clamped, just right of the icon
        public const float TOPBAR_NUMBER_SIZE = 20f;                       // auto-size max
        public const float TOPBAR_NUMBER_SIZE_MIN = 10f;                   // auto-size floor
        public static readonly Vector2 TOPBAR_NUMBER_RECT = new(52f, 40f);
        // Order left→right: high-score, star, gem (wider spacing to clear the bigger icons).
        public static readonly Vector2 TOPBAR_SCORE_POS = new(90f, TOPBAR_Y);  // high-score trophy widget (leftmost)
        public static readonly Vector2 TOPBAR_STAR_POS = new(225f, TOPBAR_Y);  // star widget (placeholder — not a real currency yet)
        public static readonly Vector2 TOPBAR_GEM_POS = new(360f, TOPBAR_Y);   // gem widget
        public static readonly Vector2 TOPBAR_BUTTON_SIZE = new(54f, 54f);
        public static readonly Vector2 TOPBAR_SHOP_POS = new(440f, TOPBAR_Y);   // shop button (left of settings)
        public static readonly Vector2 TOPBAR_CONFIG_POS = new(500f, TOPBAR_Y); // settings/gear button (rightmost)
        #endregion

        #region Font Sizes - Menu
        public const float MENU_TITLE_SIZE = 48f;
        public const float MENU_HIGHSCORE_SIZE = 24f;
        public const float MENU_GEM_SIZE = 22f;
        public const float MENU_BUTTON_TEXT_SIZE = 28f;
        #endregion

        #region Font Sizes - Panels
        public const float PANEL_TITLE_SIZE = 36f;
        public const float PANEL_BUTTON_TEXT_SIZE = 20f;
        public const float PANEL_SCORE_SIZE = 30f;
        public const float PANEL_LEVEL_SIZE = 24f;
        public const float GAMEOVER_TITLE_SIZE = 42f;
        public const float SETTINGS_BUTTON_TEXT_SIZE = 22f;
        public const float CREDITS_TEXT_SIZE = 24f;
        #endregion

        #region World-Space Popup Sizes
        public static readonly Vector2 BURGER_POPUP_NAME_RECT = new(6f, 2f);
        public static readonly Vector2 BURGER_POPUP_SCORE_RECT = new(4f, 1.5f);
        public static readonly Vector2 SCORE_POPUP_RECT = new(4f, 2f);
        #endregion

        #region Font Sizes - World Space
        public const float WORLD_SCORE_POPUP_SIZE = 5f;
        public const float WORLD_BURGER_NAME_SIZE = 4f;
        public const float WORLD_BURGER_SCORE_SIZE = 3.5f;
        public const float WORLD_FLOATING_TEXT_SIZE = 4f;
        #endregion

        #region Button Sizes
        public static readonly Vector2 MENU_BUTTON_SIZE = new(300, 65);
        public const float MENU_BUTTON_SPACING = -85f;
        public static readonly Vector2 PANEL_BUTTON_SIZE = new(320, 55);
        public static readonly Vector2 SETTINGS_BUTTON_SIZE = new(280, 55);
        public static readonly Vector2 SHOP_BUTTON_SIZE = new(300, 55);
        public static readonly Vector2 CLOSE_BUTTON_SIZE = new(200, 50);
        #endregion

        #region Panel Sizes
        public static readonly Vector2 GAMEOVER_PANEL_SIZE = new(400, 500);
        public static readonly Vector2 SETTINGS_PANEL_SIZE = new(350, 430);
        public static readonly Vector2 SHOP_PANEL_SIZE = new(380, 420);
        public static readonly Vector2 CREDITS_RECT = new(400, 300);
        #endregion

        #region Background Gradients
        // Fallback gradient colours, used only when a background skin sprite is missing.
        public static readonly Color BG_MENU_TOP = new(0.08f, 0.06f, 0.18f);
        public static readonly Color BG_MENU_BOTTOM = new(0.18f, 0.08f, 0.25f);
        public static readonly Color BG_GAME_TOP = new(0.04f, 0.08f, 0.14f);
        public static readonly Color BG_GAME_BOTTOM = new(0.06f, 0.14f, 0.18f);
        #endregion

        #region Background Layers (game scene — tune to taste in the editor)
        // Restaurant strip: scaled to fill camera width, pinned to the top, nudged by this much (world units).
        public const float RESTAURANT_Y_NUDGE = 0f;
        // Blue play-mat: scaled to this world width and centred over the grid, nudged by X/Y (world units).
        public const float GRID_CELLS_WIDTH = 6.42f;
        public const float GRID_CELLS_X_NUDGE = 0.11f;
        public const float GRID_CELLS_Y = -0.8f;
        #endregion

        #region Chef Tap Radius
        // World radius around the cook that registers a tap-to-flip (see ChefController).
        public const float BUBBLE_RADIUS = 0.5f;
        #endregion

        // Screen layout — element positions (POS), rect sizes (RECT), and button
        // stacks (start Y + per-index spacing). Button stacks are consumed inline as
        // `new Vector2(0, START_Y + SPACING * i)`, matching MainMenuUI's idiom.

        #region Layout — Main Menu
        public static readonly Vector2 MENU_TITLE_POS = new(0f, 300f);
        public static readonly Vector2 MENU_HIGHSCORE_POS = new(0f, 230f);
        public static readonly Vector2 MENU_TEXT_RECT = new(400f, 60f);
        public static readonly Vector2 MENU_GEM_RECT = new(200f, 40f);
        public const float MENU_BTN_START_Y = 80f;
        #endregion

        #region Layout — Game Over Panel
        public static readonly Vector2 GAMEOVER_TITLE_POS = new(0f, 200f);
        public static readonly Vector2 GAMEOVER_SCORE_POS = new(0f, 140f);
        public static readonly Vector2 GAMEOVER_LEVEL_POS = new(0f, 100f);
        public static readonly Vector2 GAMEOVER_TEXT_RECT = new(350f, 50f);
        public const float GAMEOVER_BTN_START_Y = 30f;
        public const float GAMEOVER_BTN_SPACING = -75f;
        #endregion

        #region Layout — Settings Panel
        public static readonly Vector2 SETTINGS_TITLE_POS = new(0f, 165f);
        public static readonly Vector2 SETTINGS_TITLE_RECT = new(300f, 50f);
        public static readonly Vector2 SETTINGS_SOUND_POS = new(0f, 90f);
        public static readonly Vector2 SETTINGS_CONTROL_POS = new(0f, 20f);
        public static readonly Vector2 SETTINGS_LEVEL_POS = new(0f, -55f);
        public static readonly Vector2 SETTINGS_LEVEL_MINUS_POS = new(-135f, -55f);
        public static readonly Vector2 SETTINGS_LEVEL_PLUS_POS = new(135f, -55f);
        public static readonly Vector2 SETTINGS_CLOSE_POS = new(0f, -135f);
        // Stepper row: centered value label flanked by square −/+ buttons.
        public static readonly Vector2 SETTINGS_STEPPER_LABEL_SIZE = new(210f, 55f);
        public static readonly Vector2 SETTINGS_STEPPER_BTN_SIZE = new(55f, 55f);
        #endregion

        #region Layout — Shop Panel
        public static readonly Vector2 SHOP_TITLE_POS = new(0f, 160f);
        public static readonly Vector2 SHOP_BALANCE_POS = new(0f, 110f);
        public static readonly Vector2 SHOP_TEXT_RECT = new(350f, 50f);
        public const float SHOP_BTN_START_Y = 40f;
        public const float SHOP_BTN_SPACING = -80f;
        public static readonly Vector2 SHOP_CLOSE_POS = new(0f, -190f);
        #endregion
    }
}
