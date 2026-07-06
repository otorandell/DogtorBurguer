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
        public const float OUTLINE_WIDTH_WORLD = 0.25f;
        public static readonly Color32 OUTLINE_COLOR = new(0, 0, 0, 255);
        // HUD text palette (sampled from the look reference IMG_1645): cream fill + thick dark-brown
        // border, applied via UIFactory.StyleHudText (a real TMP outline material, not the broken
        // per-component outlineWidth path). Used on the big numbers and all red-box labels.
        public static readonly Color HUD_TEXT_FILL = new(0.988f, 0.980f, 0.945f);      // #FCFAF1 cream white
        public static readonly Color32 HUD_TEXT_BORDER = new(0x49, 0x26, 0x11, 0xFF);  // #492611 dark brown
        public const float HUD_TEXT_BORDER_WIDTH = 0.25f;                              // TMP outline width (0..1) — tune live
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
        // Full-body per-payload illustration (the cargo is drawn in) — a touch bigger than the
        // old body-only sprite so the payload stays readable without the badge overlay.
        public const float FAIRY_BODY_HEIGHT = 1.5f;
        public const float CONSUMABLE_FALLER_HEIGHT = 2.0f;  // matches the column ghost (what it previews)
        public const float CONSUMABLE_GHOST_HEIGHT = 2.0f;   // column preview (+100%)
        public const float CONSUMABLE_GHOST_ALPHA = 0.5f;    // translucency of the column preview
        public const float CONSUMABLE_CARRY_HEIGHT = 1.8f;   // the lifted item that follows the finger
        // Use-effect art (ConsumableVfx): nozzles lock over the used column; per-type fallers.
        public const float FX_NOZZLE_HEIGHT = 1.5f;          // nozzle world height
        public const float FX_NOZZLE_TOP_OFFSET = 0.6f;      // nozzle center above the top row
        public const float FX_STREAM_FLOOR_OVERLAP = 0.2f;   // ketchup stream reaches a touch below row 0
        public const float FX_MUSTARD_DROP_HEIGHT = 1.2f;    // the falling mustard drop
        public const float FX_SKEWER_FALLING_HEIGHT = 4f;    // the full skewer while falling
        public const float FX_SKEWER_IMPACT_LIFT = 1.4f;     // raises the fall end so the TIP meets the floor
        public const float FX_SKEWER_HEAD_HEIGHT = 0.8f;     // the pinned head left at the base
        public const float FX_SKEWER_HEAD_PIN_Y = 0.9f;      // head rest height above row 0
        public const float FX_SKEWER_HEAD_DROP_FROM = 0.7f;  // head slam start, above the rest height
        #endregion

        #region HUD Consumable Slots (screen-space UGUI, below Level/Score — anchored top-left, reference px)
        // Three slots (Ketchup, Mustard, Skewer): a round plate + the consumable icon + a corner badge
        // (red num box with the count, or green plus box when empty). Left-aligned ≈ the Level/Score width.
        public static readonly Vector2 CONSUMABLE_SLOT_SIZE = new(80f, 80f);    // round plate
        public const float CONSUMABLE_SLOT_ICON_H = 58f;                        // consumable icon (width follows aspect)
        public static readonly Vector2 CONSUMABLE_ICON_OFFSET = new(0f, 3f);    // icon offset within the plate
        public const float CONSUMABLE_BADGE_H = 34f;                            // num/plus badge (width follows aspect)
        public static readonly Vector2 CONSUMABLE_BADGE_OFFSET = new(28f, -28f);// badge offset (bottom-right of plate)
        public const float CONSUMABLE_COUNT_SIZE = 26f;                         // count number on the num box
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
        public const float HUD_TITLE_LABEL_SIZE = 22f;                     // tab word TMP font (auto-size max)
        public const float HUD_TITLE_LABEL_SIZE_MIN = 8f;                  // auto-size floor for the tab word
        public const float HUD_PANEL_NUMBER_SIZE = 54f;                    // the big number font (auto-size max)
        public const float HUD_PANEL_NUMBER_SIZE_MIN = 14f;                // auto-size floor for the number
        public const float HUD_PANEL_NUMBER_Y = -18f;                      // number offset down within the card
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
        public const float SPECIAL_BANNER_LABEL_SIZE = 18f;                   // "SPECIAL ORDER" TMP (auto-size max)
        public const float SPECIAL_BANNER_LABEL_SIZE_MIN = 7f;                // auto-size floor
        public static readonly Vector2 SPECIAL_BANNER_LABEL_OFFSET = new(0f, 5f); // up a touch to clear the bubble tail
        public const float SPECIAL_INGREDIENT_H = 38f;                        // each stack ingredient (width follows aspect)
        public const float SPECIAL_INGREDIENT_SPACING = 26f;                  // vertical stack spacing (overlap)
        public const float SPECIAL_STACK_Y = -12f;                            // stack center Y within the card
        public const float SPECIAL_PLACEHOLDER_LABEL_SIZE = 18f;              // "+N" on the mystery silhouette
        public const float SPECIAL_PLATE_H = 18f;                             // plate under the bottom bun (width follows aspect)
        public const float SPECIAL_PLATE_Y_OFFSET = 13f;                      // plate drop below the bottom bun
        public const float SPECIAL_MULT_BADGE_H = 42f;                        // multiplier badge (reuses the red num box)
        // Badge sits on the meter's bottom cap (meter bottom ≈ offset.y − H/2 = -123): reference shows
        // the xN badge overlapping the capsule's bottom end, not floating mid-meter.
        public static readonly Vector2 SPECIAL_MULT_BADGE_OFFSET = new(86f, -110f); // badge offset within the card (on the meter bottom)
        public const float SPECIAL_MULT_TEXT_SIZE = 24f;
        // Mult meter — a vertical capsule (back well + green fill + frame), 3 stacked layers at one rect.
        // A child of the card, built before the mult badge so the badge renders on top (meter sits under
        // it, sharing its x). Taller than the card so it overflows top/bottom a touch. Eyeball defaults.
        public const float MULT_METER_H = 215f;                              // meter height (width follows the 302:1011 aspect)
        public static readonly Vector2 MULT_METER_OFFSET = new(86f, -16f);   // within the card, x aligned to the mult badge
        public const float MULT_METER_FILL_BOTTOM_EXTEND = 20f;              // extend the green fill's rect down to meet the well bottom (closes the gap)
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
        public static readonly Color TOPBAR_NUMBER_COLOR = new(0.28f, 0.17f, 0.1f); // dark brown, no border (pill numbers)
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
        public const float WORLD_STAR_POPUP_SIZE = 3f;   // "+N STARS" on an order match (below the xN)
        #endregion

        #region Button Sizes
        public static readonly Vector2 MENU_BUTTON_SIZE = new(300, 65);
        public const float MENU_BUTTON_SPACING = -85f;
        public static readonly Vector2 PANEL_BUTTON_SIZE = new(320, 55);
        public static readonly Vector2 SETTINGS_BUTTON_SIZE = new(280, 55);
        public static readonly Vector2 CLOSE_BUTTON_SIZE = new(200, 50);
        #endregion

        #region Panel Sizes
        public static readonly Vector2 GAMEOVER_PANEL_SIZE = new(400, 500);
        public static readonly Vector2 SETTINGS_PANEL_SIZE = new(350, 430);
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
        public static readonly Vector2 GAMEOVER_STARS_POS = new(0f, 65f);   // "+N Stars earned!"
        public static readonly Vector2 GAMEOVER_TEXT_RECT = new(350f, 50f);
        public const float GAMEOVER_BTN_START_Y = 30f;
        public const float GAMEOVER_BTN_SPACING = -75f;
        #endregion

        #region Layout — Settings Panel
        // In-game it gets its own canvas: above the game-over panel (100), below the shop (120).
        public const int SETTINGS_CANVAS_SORT = 110;
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
        // In-game variant: a taller panel with a Restart | Quit-to-menu row above Close.
        public static readonly Vector2 SETTINGS_PANEL_SIZE_INGAME = new(350f, 470f);
        public static readonly Vector2 SETTINGS_RUN_BTN_SIZE = new(135f, 55f);
        public static readonly Vector2 SETTINGS_RESTART_POS = new(-73f, -125f);
        public static readonly Vector2 SETTINGS_QUIT_POS = new(73f, -125f);
        public static readonly Vector2 SETTINGS_CLOSE_POS_INGAME = new(0f, -195f);
        #endregion

        #region Layout — Shop Screen (full-screen overlay; header + vertical page scroll)
        public const int SHOP_CANVAS_SORT = 120;                                // above every in-game canvas (HUD 50, slots 90, game-over 100)
        public static readonly Color SHOP_BG = new(0.11f, 0.08f, 0.06f, 0.99f); // warm dark brown page
        public static readonly Color SHOP_CARD_BG = new(0.22f, 0.16f, 0.12f);   // item cells / offer bars
        public static readonly Color SHOP_CARD_BG_EQUIPPED = new(0.16f, 0.28f, 0.16f);  // equipped skin cell
        public static readonly Color SHOP_CARD_BG_HIGHLIGHT = new(0.32f, 0.20f, 0.10f); // remove-ads banner
        public static readonly Color SHOP_SECTION_TITLE_COLOR = new(0.95f, 0.85f, 0.65f);
        public static readonly Color SHOP_SUBTEXT_COLOR = new(0.75f, 0.70f, 0.62f);
        public static readonly Color SHOP_BADGE_COLOR = new(1f, 0.85f, 0f);     // "BEST VALUE" tags
        public static readonly Color SHOP_EQUIPPED_COLOR = new(0.35f, 0.9f, 0.4f);

        // Header (fixed above the scroll): title, both currency pills, close button.
        public const float SHOP_HEADER_H = 150f;
        public static readonly Vector2 SHOP_TITLE_POS = new(0f, -42f);
        public static readonly Vector2 SHOP_TITLE_RECT = new(300f, 60f);
        public const float SHOP_TITLE_SIZE = 40f;
        public static readonly Vector2 SHOP_STAR_PILL_POS = new(-95f, -105f);   // anchored top-center
        public static readonly Vector2 SHOP_GEM_PILL_POS = new(95f, -105f);
        public const float SHOP_PILL_ICON_H = 56f;                              // header pill icon height
        public static readonly Vector2 SHOP_CLOSE_POS = new(-40f, -40f);        // anchored top-right
        public static readonly Vector2 SHOP_CLOSE_SIZE = new(52f, 52f);
        public const float SHOP_CLOSE_TEXT_SIZE = 24f;

        // Page scroll + sections.
        public const float SHOP_SCROLL_SENSITIVITY = 30f;
        public const int SHOP_CONTENT_PADDING = 16;                             // page edges (RectOffset — int)
        public const int SHOP_CONTENT_BOTTOM_PADDING = 32;
        public const float SHOP_SECTION_SPACING = 10f;
        public const float SHOP_SECTION_TITLE_H = 44f;
        public const float SHOP_SECTION_TITLE_SIZE = 22f;
        public const float SHOP_CELL_SPACING = 10f;                             // between row cells

        // Skin cells (horizontal rows).
        public const float SHOP_SKIN_ROW_H = 190f;
        public static readonly Vector2 SHOP_SKIN_CELL_SIZE = new(124f, 182f);
        public const float SHOP_SKIN_PREVIEW_H = 82f;
        public const float SHOP_SKIN_PREVIEW_MARGIN = 12f;
        public const float SHOP_SKIN_NAME_SIZE = 15f;
        public const float SHOP_SKIN_NAME_Y = -104f;                            // below the preview
        public const float SHOP_SKIN_STATE_SIZE = 16f;                          // EQUIPPED / EQUIP / price
        public const float SHOP_SKIN_STATE_Y = 22f;                             // state row, from cell bottom
        public const float SHOP_SKIN_STATE_X = 10f;                             // label shift right of the icon
        public const float SHOP_SKIN_PRICE_ICON_X = -34f;
        public const float SHOP_SKIN_PRICE_ICON_H = 26f;

        // Consumable cards (horizontal row).
        public const float SHOP_CONSUMABLE_ROW_H = 240f;
        public static readonly Vector2 SHOP_CONSUMABLE_CARD_SIZE = new(154f, 232f);
        public const float SHOP_CONSUMABLE_ICON_H = 64f;
        public const float SHOP_CONSUMABLE_ICON_Y = -44f;                       // icon center, from card top
        public const float SHOP_CONSUMABLE_NAME_Y = -92f;
        public const float SHOP_CONSUMABLE_OWNED_Y = -114f;
        public const float SHOP_CONSUMABLE_BTN_Y = -150f;                       // first buy button center
        public const float SHOP_CONSUMABLE_BTN_SPACING = 48f;
        public static readonly Vector2 SHOP_CONSUMABLE_BTN_SIZE = new(134f, 40f);

        // Offer bars (star packs, gem packs, remove-ads) + their price buttons.
        public const float SHOP_OFFER_BAR_H = 84f;
        public const float SHOP_REMOVE_ADS_BAR_H = 110f;
        public const float SHOP_OFFER_ICON_X = 46f;                             // icon center, from bar left
        public const float SHOP_OFFER_ICON_H = 54f;
        public const float SHOP_OFFER_TEXT_X = 88f;                             // title/subtitle left edge
        public const float SHOP_OFFER_TITLE_SIZE = 20f;
        public static readonly Vector2 SHOP_OFFER_TITLE_RECT = new(240f, 30f);
        public const float SHOP_OFFER_SUB_SIZE = 13f;
        public static readonly Vector2 SHOP_OFFER_SUB_RECT = new(260f, 40f);
        public const float SHOP_BADGE_SIZE = 12f;
        public static readonly Vector2 SHOP_BADGE_RECT = new(140f, 20f);
        public static readonly Vector2 SHOP_BADGE_POS = new(-12f, 34f);         // above the price button
        public static readonly Vector2 SHOP_PRICE_BTN_POS = new(-80f, 0f);      // anchored right-center
        public static readonly Vector2 SHOP_PRICE_BTN_SIZE = new(132f, 46f);
        public const float SHOP_PRICE_TEXT_SIZE = 18f;
        public const float SHOP_PRICE_ICON_X = 18f;                             // currency icon inside the button
        public const float SHOP_PRICE_ICON_H = 26f;
        public const float SHOP_PRICE_TEXT_MARGIN = 26f;                        // label shift right of the icon

        // Gem-spend confirm dialog.
        public static readonly Vector2 SHOP_CONFIRM_PANEL_SIZE = new(400f, 240f);
        public static readonly Vector2 SHOP_CONFIRM_TEXT_POS = new(0f, 40f);
        public static readonly Vector2 SHOP_CONFIRM_TEXT_RECT = new(360f, 110f);
        public const float SHOP_CONFIRM_TEXT_SIZE = 22f;
        public const float SHOP_CONFIRM_BTN_X = 92f;                            // buttons flank the center
        public const float SHOP_CONFIRM_BTN_Y = -75f;
        public static readonly Vector2 SHOP_CONFIRM_BTN_SIZE = new(160f, 52f);
        #endregion
    }
}
