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
        public static readonly Color GOLD = new(1f, 0.85f, 0f, 1f);
        #endregion

        #region Panel / Overlay Colors
        public static readonly Color OVERLAY_DIM = new(0, 0, 0, 0.7f);
        public static readonly Color OVERLAY_DARK = new(0, 0, 0, 0.85f);
        public static readonly Color PANEL_BG = new(0.15f, 0.15f, 0.2f, 0.95f);
        public static readonly Color INNER_PANEL_BG = new(0.18f, 0.18f, 0.25f, 1f);
        public static readonly Color SCREEN_FLASH = new(1f, 1f, 1f, 0.6f);
        #endregion

        #region Button Colors
        public static readonly Color BTN_PLAY = new(0.2f, 0.8f, 0.3f);
        public static readonly Color BTN_SHOP = new(0.9f, 0.7f, 0.1f);
        public static readonly Color BTN_SETTINGS = new(0.4f, 0.6f, 0.9f);
        public static readonly Color BTN_LEADERBOARD = new(0.6f, 0.4f, 0.8f);
        public static readonly Color BTN_CLOSE = new(0.5f, 0.5f, 0.5f);
        public static readonly Color BTN_CONTINUE_GEMS = new(0.9f, 0.7f, 0.1f, 1f);
        public static readonly Color BTN_CONTINUE_AD = new(0.3f, 0.5f, 0.9f, 1f);
        public static readonly Color BTN_RESTART = new(0.2f, 0.7f, 0.3f, 1f);
        public static readonly Color BTN_SETTINGS_TOGGLE = new(0.3f, 0.5f, 0.7f);
        public static readonly Color BTN_SHOP_AD = new(0.3f, 0.5f, 0.9f);
        public static readonly Color BTN_SHOP_BUY = new(0.2f, 0.7f, 0.3f);
        public static readonly Color BTN_GEM_PACK = new(1f, 0.85f, 0f);
        #endregion

        #region Font Sizes - HUD
        public const float HUD_SCORE_SIZE = 22f;
        public const float HUD_LEVEL_SIZE = 18f;
        public const float HUD_GEM_SIZE = 16f;
        public const float HUD_LINE_SPACING = 40f;
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
        public static readonly Vector2 SETTINGS_PANEL_SIZE = new(350, 350);
        public static readonly Vector2 SHOP_PANEL_SIZE = new(380, 420);
        #endregion

        #region Background Gradients
        public static readonly Color BG_MENU_TOP = new(0.08f, 0.06f, 0.18f);
        public static readonly Color BG_MENU_BOTTOM = new(0.18f, 0.08f, 0.25f);
        public static readonly Color BG_GAME_TOP = new(0.04f, 0.08f, 0.14f);
        public static readonly Color BG_GAME_BOTTOM = new(0.06f, 0.14f, 0.18f);
        #endregion

        #region Chef Bubbles
        public static readonly Color BUBBLE_INACTIVE = new(1f, 1f, 1f, 0.25f);
        public static readonly Color BUBBLE_ACTIVE = new(1f, 1f, 1f, 0.45f);
        public const float BUBBLE_RADIUS = 0.5f;
        #endregion
    }
}
