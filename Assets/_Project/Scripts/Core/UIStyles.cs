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
        public const float MATCH_WIDTH_OR_HEIGHT = 0.5f;
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

        #region Game Layout / Challenge Colors
        public static readonly Color LAYOUT_BORDER = new(0f, 0f, 0f, 0.8f);
        public static readonly Color LAYOUT_FILL = new(0f, 0f, 0f, 0.15f);
        public static readonly Color CHALLENGE_METER_BG = new(0.2f, 0.2f, 0.2f, 0.8f);
        public static readonly Color CHALLENGE_METER_FILL = new(0.2f, 0.9f, 0.3f);
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
        public const float CONSUMABLE_CARRY_HEIGHT = 1.8f;   // matches the inventory icon (same item lifted)
        public const float CONSUMABLE_ICON_HEIGHT = 1.8f;    // inventory slot icon (+100%)
        // Inventory slots live in the top-left score panel (world space). Tune these to taste.
        public static readonly Vector2 CONSUMABLE_SLOT_0_POS = new(-2.0f, 1.55f);
        public static readonly Vector2 CONSUMABLE_SLOT_1_POS = new(-1.1f, 1.55f);
        #endregion

        #region Font Sizes - HUD
        public const float HUD_GEM_SIZE = 16f; // temporary gem readout (moves to the status bar)
        #endregion

        #region HUD Stat Panels (authored Level/Score cards — anchored top-left, reference px)
        // Card + title tab are 9-sliced flat art (ui_panel_card / ui_title_tab); the title text
        // and number are TMP, so the boxes size freely without the corners distorting.
        public static readonly Vector2 HUD_PANEL_SIZE = new(150f, 115f);   // the cream card
        public const float HUD_CARD_PPU_MULT = 4f;                         // card 9-slice corner scale (higher = thinner)
        public static readonly Vector2 HUD_PANEL_TITLE_SIZE = new(130f, 46f); // red title tab box
        public const float HUD_TITLE_PPU_MULT = 5f;                        // tab 9-slice corner scale
        public const float HUD_PANEL_TITLE_Y = 44f;                        // tab offset up within the card
        public const float HUD_TITLE_LABEL_SIZE = 26f;                     // "Level"/"Score" TMP font
        public static readonly Color HUD_TITLE_LABEL_COLOR = new(0.96f, 0.93f, 0.82f); // cream
        public const float HUD_PANEL_NUMBER_SIZE = 40f;                    // the big number font
        public const float HUD_PANEL_NUMBER_Y = -16f;                      // number offset down within the card
        public static readonly Color HUD_PANEL_NUMBER_COLOR = new(0.28f, 0.17f, 0.1f);
        public static readonly Vector2 HUD_LEVEL_PANEL_POS = new(100f, -80f);
        public static readonly Vector2 HUD_SCORE_PANEL_POS = new(265f, -80f);
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
        public const float WORLD_CHALLENGE_NAME_SIZE = 2.2f;
        public const float WORLD_CHALLENGE_LEVEL_SIZE = 2f;
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

        #region Layout — HUD
        public static readonly Vector2 HUD_ANCHOR_MIN = new(0.06f, 0.93f);
        public static readonly Vector2 HUD_ANCHOR_MAX = new(0.46f, 0.93f);
        public static readonly Vector2 HUD_PIVOT = new(0f, 1f);
        public const float HUD_TEXT_X = 10f;
        public static readonly Vector2 HUD_TEXT_RECT = new(0f, 35f);
        #endregion

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

        #region Layout — Challenge Panel (world-space)
        public const float CHALLENGE_TITLE_Y = 1.25f;
        public const float CHALLENGE_NAME_Y = 1.05f;
        public const float CHALLENGE_METER_X = 0.9f;
        public const float CHALLENGE_METER_Y = -0.2f;
        public const float CHALLENGE_LEVEL_GAP = 0.2f; // level text gap below the meter
        #endregion

        #region Layout — Game Layout Panels (world-space frames)
        public static readonly Vector2 GRID_PANEL_CENTER = new(0f, -1.9f);
        public static readonly Vector2 GRID_PANEL_SIZE = new(5.2f, 5.8f);
        public static readonly Vector2 TOP_LEFT_PANEL_CENTER = new(-1.35f, 2.4f);
        public static readonly Vector2 TOP_LEFT_PANEL_SIZE = new(2.5f, 2.6f);
        public static readonly Vector2 TOP_RIGHT_PANEL_CENTER = new(1.35f, 2.4f);
        public static readonly Vector2 TOP_RIGHT_PANEL_SIZE = new(2.5f, 2.6f);
        public const int PANEL_BORDER_WIDTH = 4;
        public const int PANEL_CORNER_RADIUS = 24;
        #endregion
    }
}
