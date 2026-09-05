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
        public const float OUTLINE_WIDTH_WORLD = 0.25f; // > 0 = world text gets the sticker lettering (value itself unused since 2026-09-04)
        // HUD text palette (sampled from the look reference IMG_1645): cream fill + thick dark-brown
        // border, applied via UIFactory.StyleHudText (a real TMP outline material, not the broken
        // per-component outlineWidth path). Used on the big numbers and all red-box labels.
        public static readonly Color HUD_TEXT_FILL = new(0.988f, 0.980f, 0.945f);      // #FCFAF1 cream white
        public static readonly Color32 HUD_TEXT_BORDER = new(0x49, 0x26, 0x11, 0xFF);  // #492611 dark brown — the SHADOW ring (and legacy border for colored headings)
        public static readonly Color32 HUD_TEXT_STROKE = new(0x88, 0x46, 0x2A, 0xFF);  // #88462A mid brown — the inner stroke (sampled off the mock's PLAY)
        public const float HUD_TEXT_BORDER_WIDTH = 0.25f;                               // TMP outline width (0..1) — tune live
        public const float TEXT_FACE_DILATE = 0.2f;                                     // fattens the fill itself (pushes the stroke outward instead of eating the white)
        // The sticker drop shadow (TMP Underlay) applied by StyleFillAndBorder to EVERY bordered
        // text, matching the artist's Photoshop stroke+shadow recipe (Look Reference/Font info.png).
        // Offsets/dilate are in SDF *spread* units (-1..1): the on-screen reach = value × atlas
        // padding / sampling point size. ⚠️ The shadow only gets properly chunky once the SDF atlas
        // is regenerated with a bigger spread — 2048 atlas / padding 24 / sampling 144 / SDFAA
        // (the original 12-padding atlas caps the whole effect at ~1px per 30px of text).
        public const float TEXT_SHADOW_OFFSET_X = 0f;
        public const float TEXT_SHADOW_OFFSET_Y = -0.5f;                                // straight down; big values read as the dark layers sagging
        public const float TEXT_SHADOW_DILATE = 0.6f;                                   // thickens the shadow into the outer ring
        public const float TEXT_SHADOW_SOFTNESS = 0f;                                   // hard edge — a sticker, not a blur
        #endregion

        #region Text Colors
        public static readonly Color TEXT_HUD = Color.black;
        public static readonly Color TEXT_UI = Color.white;
        public static readonly Color TEXT_TOO_BAD = Color.red;      // the one non-cream popup: failure must read differently
        #endregion

        #region Popup Colors
        // World popups are uniformly cream (HUD_TEXT_FILL) since 2026-09-04 — the glow PLATES
        // carry the meaning (green = score, yellow = multiplier/stars), not the text color.
        // The relic per-mechanic colors (yellow/cyan/orange) are gone.
        public static readonly Color GOLD = new(1f, 0.85f, 0f);     // game-over "stars earned", shop badges
        #endregion

        #region Panel / Overlay Colors
        public static readonly Color OVERLAY_DIM = new(0, 0, 0, 0.7f);
        public static readonly Color MODAL_OVERLAY = new(0f, 0f, 0f, 0.55f);   // stand-in for the mocks' blurred game behind a full-canvas panel (game over, settings)
        public static readonly Color SCREEN_FLASH = new(1f, 1f, 1f, 0.6f);
        #endregion

        #region Button Colors
        public static readonly Color BTN_DEV_STEPPER = new(0.3f, 0.5f, 0.7f);    // dev-only start-level stepper (Settings)
        #endregion

        #region Consumable / Fairy Sizes (world-space heights)
        // Reward sprites import large (100 PPU); everything is normalized to a target world height
        // via SpriteFit rather than a raw scale, so the source pixel size doesn't matter.
        // Full-body per-payload illustration (the cargo is drawn in) — a touch bigger than the
        // old body-only sprite so the payload stays readable without the badge overlay.
        public const float FAIRY_BODY_HEIGHT = 1.5f;
        public const float PREVIEW_ARROW_HEIGHT = 1.05f;                   // arrow back-picture behind a preview ghost
        // World popup plates (the 2026-09-04 halftone blobs, set 1) — heights in world units.
        public const float PLATE_BURGER_H = 2.0f;                          // wide green ellipse behind the burger name + points
        public static readonly Vector2 PLATE_BURGER_OFFSET = new(0f, -0.35f); // centered between the name and the score line
        public const float PLATE_SCORE_H = 1.1f;                           // green round behind "N!" score popups
        public const float PLATE_FLOAT_H = 1.0f;                           // round blob behind floating texts that ask for one
        public const float CONSUMABLE_FALLER_HEIGHT = 2.0f;  // default falling item (badge art)
        public const float CONSUMABLE_GHOST_HEIGHT = 1.4f;   // column preview (nozzle art; 30% down from 2.0)
        public const float CONSUMABLE_GHOST_Y_OFFSET = -0.35f;// ghost sits a touch below the column-top anchor
        public const float CONSUMABLE_GHOST_ALPHA = 0.5f;    // translucency of the column preview
        // Use-effect art (ConsumableVfx): the lingering ghost plays the locked-on nozzle.
        public const float FX_STREAM_FLOOR_OVERLAP = 0.2f;   // ketchup stream reaches a touch below row 0
        public const float FX_MUSTARD_DROP_HEIGHT = 1.2f;    // the falling mustard drop
        public const float FX_SKEWER_FALLING_HEIGHT = 2f;    // the full skewer while falling
        // The stick falls to the BUN's row; lift = FALLING_HEIGHT/2 + bun half-height so the
        // tip meets the bun's top edge.
        public const float FX_SKEWER_IMPACT_LIFT = 1.2f;
        public const float FX_SKEWER_HEAD_HEIGHT = 0.7f;     // the head pinned into the bun
        // Head center above the bun's row, while riding it down and at rest: PIN_Y −
        // HEAD_HEIGHT/2 ≈ the bun's top edge, so the head base touches the bread.
        public const float FX_SKEWER_HEAD_PIN_Y = 0.55f;
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
        // Grown 15% downwards from the native-aspect 122x92 (top edge kept in place: the panel
        // POS y dropped by half the added height, and the tab/number offsets compensate).
        public static readonly Vector2 HUD_PANEL_SIZE = new(122f, 106f);   // the cream card (stretched taller)
        // Placeholder title tab: the blank no_tex red tab (ui_title_tab) with the word written on it
        // as TMP — swapped for the artist's final per-word art when it arrives. Sized by HEIGHT with
        // width following the native aspect (≈1.84:1) so it never stretches; raise the height to widen.
        public const float HUD_PANEL_TITLE_HEIGHT = 56f;                   // red title tab height (width follows aspect; +16%)
        public const float HUD_PANEL_TITLE_Y = 37f;                        // tab offset up within the card
        public const float HUD_TITLE_LABEL_SIZE = 22f;                     // tab word TMP font (auto-size max)
        public const float HUD_TITLE_LABEL_SIZE_MIN = 8f;                  // auto-size floor for the tab word
        public const float HUD_PANEL_NUMBER_SIZE = 54f;                    // the big number font (auto-size max)
        public const float HUD_PANEL_NUMBER_W = 100f;                      // number auto-fit rect — inside the card art's margins, so long scores shrink to fit
        public const float HUD_PANEL_NUMBER_SIZE_MIN = 14f;                // auto-size floor for the number
        public const float HUD_PANEL_NUMBER_Y = -11f;                      // number offset down within the card
        // Shared left-column zone: 18px left margin → right edge at half the screen (270). Two 122-wide
        // cards, ~8 gap. The consumable row spans this same zone (start/end aligned).
        public static readonly Vector2 HUD_LEVEL_PANEL_POS = new(79f, -132f);
        public static readonly Vector2 HUD_SCORE_PANEL_POS = new(209f, -132f);
        #endregion

        #region HUD Special Order panel (screen-space UGUI, top-right — anchored top-right, reference px)
        // Card + the SPECIAL ORDER banner overhanging its top-left, the required-burger stack (on a
        // plate), and a multiplier badge. The card is stretched taller than its native aspect to about
        // the height of the left column (Score + consumables) — deliberate, it's a completed burger.
        // Height matches the left column: top aligns with the Level/Score top, bottom with the
        // consumables bottom (≈194 tall here) — stretched past native aspect on purpose.
        public static readonly Vector2 SPECIAL_CARD_SIZE = new(228f, 194f);   // cream card (stretched taller)
        public static readonly Vector2 SPECIAL_CARD_POS = new(-128f, -176f);  // anchored top-right
        public const float SPECIAL_MODE_TAB_H = 58f;                          // red mode tab (CLASSIC/RELAX) straddling the card's bottom edge — the Level/Score tab recipe
        public const float SPECIAL_MODE_TAB_Y = 2f;                           // tab center vs the card's bottom edge
        public const float SPECIAL_MODE_TAB_TEXT = 20f;
        public const float SPECIAL_BANNER_H = 60f;                            // SPECIAL ORDER banner (width follows aspect)
        public const float SPECIAL_BANNER_STRETCH_X = 1.15f;                  // widen the red banner past native aspect (deliberate)
        public static readonly Vector2 SPECIAL_BANNER_OFFSET = new(-40f, 78f);// banner offset within the card (overhangs top-left)
        public const float SPECIAL_BANNER_LABEL_SIZE = 18f;                   // "SPECIAL ORDER" TMP (auto-size max)
        public const float SPECIAL_BANNER_LABEL_SIZE_MIN = 7f;                // auto-size floor
        public static readonly Vector2 SPECIAL_BANNER_LABEL_OFFSET = new(6f, 5f); // right + up a touch to sit in the bubble
        // Stack sprites (ingredients/buns/plate) are sized from their WORLD dimensions (pixel rect /
        // PPU × this factor) — the same per-file normalization the playfield uses, so the stack's
        // proportions match the game (pieces ~1.2 world units wide → ~60px, buns 1.38 → ~69px).
        // Sizing by raw pixel aspect ignored the per-file PPU tuning and transparent padding, which
        // made the plate tiny and the ingredient ratios inconsistent.
        public const float SPECIAL_STACK_PX_PER_UNIT = 62f;                   // screen px per world unit (bigger burger, 2026-09-04 pass)
        public const float SPECIAL_INGREDIENT_SPACING = 22f;                  // vertical stack spacing (tighter overlap)
        public const float SPECIAL_STACK_X = -10f;                            // stack center X within the card (nudged left)
        public const float SPECIAL_STACK_Y = -12f;                            // stack center Y within the card
        public const float SPECIAL_PLACEHOLDER_LABEL_SIZE = 18f;              // "+N" on the mystery silhouette
        public const float SPECIAL_MYSTERY_H = 48f;                           // mystery silhouette (UI art, no tuned PPU — sized by height; matches the 62px/unit stack)
        public const float SPECIAL_PLATE_Y_OFFSET = 13f;                      // plate drop below the bottom bun
        public const float SPECIAL_MULT_BADGE_H = 42f;                        // multiplier badge (reuses the red num box)
        // Badge sits on the meter's bottom cap (meter bottom ≈ offset.y − H/2 = -123): reference shows
        // the xN badge overlapping the capsule's bottom end, not floating mid-meter.
        public static readonly Vector2 SPECIAL_MULT_BADGE_OFFSET = new(86f, -110f); // badge offset within the card (on the meter bottom)
        public const float SPECIAL_MULT_TEXT_SIZE = 24f;
        public const float SPECIAL_GHOST_ALPHA = 1f;                          // the "?" mystery layer on Contains orders — opaque by decision (2026-09-04); lower for a faded ghost
        // Mult meter — a vertical capsule (back well + green fill + frame), 3 stacked layers at one rect.
        // A child of the card, built before the mult badge so the badge renders on top (meter sits under
        // it, sharing its x). Taller than the card so it overflows top/bottom a touch. Eyeball defaults.
        public const float MULT_METER_H = 215f;                              // meter height (width follows the 302:1011 aspect)
        public static readonly Vector2 MULT_METER_OFFSET = new(86f, -8f);    // within the card, x aligned to the mult badge
        public const float MULT_METER_FILL_BOTTOM_EXTEND = 20f;              // extend the green fill's rect down to meet the well bottom (closes the gap)
        #endregion

        #region Top Bar (shared TopBar component: authored currency widgets + buttons — anchored top-left, reference px)
        public const float TOPBAR_Y = -38f;                                // vertical center of the bar
        // Pills widened 25% (88→110) keeping each pill's LEFT box edge in place — the centers below
        // are left edge + 55. The buttons shifted right to clear the wider gem pill.
        public static readonly Vector2 TOPBAR_BOX_SIZE = new(110f, 42f);   // currency pill (ui_currency_box native ≈ 2.12:1)
        // Per-icon HEIGHT — width follows the sprite's native aspect (forcing a square distorted the
        // non-square trophy/star). The art has different visual weight; all overhang the pill's left.
        public const float TOPBAR_SCORE_ICON_H = 69f; // high-score trophy (wide art)
        public const float TOPBAR_STAR_ICON_H = 62f;  // star — a touch smaller
        public const float TOPBAR_GEM_ICON_H = 78f;   // gem/diamond — a touch bigger
        public const float TOPBAR_ICON_X = -46f;                           // icon offset within the widget — steps onto the pill, hiding its left edge
        public const float TOPBAR_NUMBER_X = 12f;                          // number RECT center — the free zone right of the icon
        public const float TOPBAR_NUMBER_Y = 1.5f;                           // lifted a touch above the pill's vertical center
        public const float TOPBAR_NUMBER_SIZE = 20f;                       // auto-size max
        public const float TOPBAR_NUMBER_SIZE_MIN = 10f;                   // auto-size floor
        public static readonly Color TOPBAR_NUMBER_COLOR = new(0.28f, 0.17f, 0.1f); // dark brown — plain small labels (shop subtitles etc.; the pill numbers use the HUD palette since 2026-09-03)
        public static readonly Vector2 TOPBAR_NUMBER_RECT = new(78f, 40f); // digits center in this rect regardless of count (auto-size shrinks long ones)
        // Order left→right: high-score, star, gem (wider spacing to clear the bigger icons).
        public static readonly Vector2 TOPBAR_SCORE_POS = new(101f, TOPBAR_Y); // high-score trophy widget (leftmost; box left edge 46)
        public static readonly Vector2 TOPBAR_STAR_POS = new(236f, TOPBAR_Y);  // star widget (box left edge 181)
        public static readonly Vector2 TOPBAR_GEM_POS = new(371f, TOPBAR_Y);   // gem widget (box left edge 316)
        public static readonly Vector2 TOPBAR_BUTTON_SIZE = new(54f, 54f); // in-game pair (shop + gear); the menu's lone gear overrides (MENU_GEAR_*)
        public static readonly Vector2 TOPBAR_HELP_POS = new(455f, TOPBAR_Y);   // the "?" help button (left of settings; replaced the shop button 2026-09-05)
        public static readonly Vector2 TOPBAR_CONFIG_POS = new(511f, TOPBAR_Y); // settings/gear button (rightmost; in-game the shop button sits left of it)
        public static readonly Vector2 MENU_GEAR_POS = new(498f, TOPBAR_Y);      // the MENU's gear: alone on the right, so bigger and nudged toward center
        public static readonly Vector2 MENU_GEAR_SIZE = new(62f, 62f);
        public static readonly Vector2 MENU_HELP_POS = new(430f, TOPBAR_Y);      // the MENU's "?" help button, left of the gear (same size)
        #endregion

        #region Font Sizes - Panels
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
        public const float WORLD_STAR_POPUP_SIZE = 3f;   // "+N!" star award on an order match (below the xN)
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
        // Blue play-mat: scaled to this world width and centred over the grid, nudged by X/Y (world
        // units). The mat LEADS the layout — CELL_WIDTH is derived from its painted lane pitch at
        // this width (see Constants.CELL_WIDTH); change them together.
        public const float GRID_CELLS_WIDTH = 6.33f;
        // Measured: the painted lanes sit 45.75px left of the sprite's center (asymmetric
        // transparent padding), so the sprite shifts right by that in world units to compensate.
        public const float GRID_CELLS_X_NUDGE = 0.111f;
        public const float GRID_CELLS_Y = -0.8f;
        #endregion

        #region Chef Tap Radius
        // World radius around the cook that registers a tap-to-flip (see ChefController).
        public const float BUBBLE_RADIUS = 0.5f;
        #endregion

        // Screen layout — element positions (POS), rect sizes (RECT), and button
        // stacks (start Y + per-index spacing). Button stacks are consumed inline as
        // `new Vector2(0, START_Y + SPACING * i)`, matching MainMenuUI's idiom.

        #region Layout — Main Menu (authored art — reference px; sized by WIDTH, height follows native aspect)
        // Logo is top-anchored so it clears the top bar on tall screens; plaque/play are
        // center-anchored; the checkered strip and its buttons are bottom-anchored. Eyeball
        // defaults from the artist's mock — tune live.
        public static readonly Vector2 MENU_LOGO_POS = new(23f, -185f);      // logo CENTER below the top edge (x: the art's opaque pixels sit left of its canvas center — +23 recenters the visible lettering)
        public const float MENU_LOGO_W = 604f;                               // sized to the 2026-09-04 pass (DesiredMenu.png)
        public static readonly Vector2 MENU_PLAY_POS = new(0f, -196f);       // authored PLAY (text baked in)
        public const float MENU_PLAY_W = 400f;
        public const float MENU_BOTTOM_STRIP_W = 680f;                       // the checker strip: the ART has ~150px clear margins per side, so it must
                                                                             // outsize the canvas for the squares to reach the screen edges
        public const float MENU_BOTTOM_BTN_Y = 108f;                         // CREDITS/SHOP center height from the bottom edge
        public const float MENU_BOTTOM_BTN_X = 129f;                         // ± from center
        public const float MENU_BOTTOM_BTN_W = 245f;                         // authored red/yellow blanks sized by width (canvas incl. shadow)
        public const float MENU_CREDITS_LABEL_SIZE = 38f;                    // CREDITS word (HUD palette)
        public const float MENU_SHOP_LABEL_SIZE = 50f;                       // SHOP word — way bigger (short word, mock draws it huge)
        public static readonly Vector2 MENU_BOTTOM_LABEL_NUDGE = new(-3f, 5f); // word toward the face center (shadow is bottom-right)
        // No red/orange blank exists in the kit — the cream blank is runtime-tinted (multiply).
        public const float MENU_SUPPORT_LABEL_Y = -16f;                      // "Support the devs!" sits ON the SHOP button's face top (canvas has shadow margins)
        public const float MENU_SUPPORT_LABEL_W = 205f;                      // auto-fit rect — narrower than the button = slightly smaller text
        public const float MENU_SUPPORT_LABEL_SIZE = 40f;                    // auto-size max
        public const float MENU_SUPPORT_LABEL_MIN = 12f;
        // The flashy green: a vertical gradient (sampled off DesiredMenu.png) under the dark
        // outline + downward shadow ring.
        public static readonly Color MENU_SUPPORT_TOP = new(0.66f, 0.85f, 0.36f);
        public static readonly Color MENU_SUPPORT_BOTTOM = new(0.42f, 0.62f, 0.0f);
        #endregion

        #region Layout — Game Over Screen (authored art — reference px, canvas-centered, y up)
        // The panel art (ui_gameover_panel) is a full-phone canvas (2327x4138 ≈ 9:16, the reference
        // aspect): shown at REFERENCE_RESOLUTION it lands exactly where the artist drew it — the red
        // title bar, the cream body and the darker "Continue" band are all baked in. Everything below
        // is placed over that art (positions measured off the mock, Look Reference/GameOver.png).
        // Authored buttons are sized by WIDTH, height following native aspect; the blanks' canvases
        // include their drop shadow, so the visible face is ~10% smaller than the width given.
        public static readonly Vector2 GAMEOVER_TITLE_POS = new(0f, 214f);           // "GAME OVER..." on the red bar
        public static readonly Vector2 GAMEOVER_TITLE_RECT = new(420f, 80f);
        public const float GAMEOVER_TITLE_SIZE = 40f;
        public const float GAMEOVER_CARD_SCALE = 1.25f;                               // the HUD stat cards, enlarged
        public static readonly Vector2 GAMEOVER_LEVEL_CARD_POS = new(-94f, 70f);
        public static readonly Vector2 GAMEOVER_SCORE_CARD_POS = new(94f, 70f);
        public static readonly Vector2 GAMEOVER_CONTINUE_LABEL_POS = new(0f, -34f);  // "Continue" heading, top of the band
        public static readonly Vector2 GAMEOVER_CONTINUE_LABEL_RECT = new(300f, 44f);
        public const float GAMEOVER_CONTINUE_LABEL_SIZE = 32f;
        public static readonly Color32 GAMEOVER_CONTINUE_BORDER = new(0xFC, 0xFA, 0xF1, 0xFF); // cream edge on the brown word
        public const float GAMEOVER_CONTINUE_BORDER_WIDTH = 0.2f;
        public const float GAMEOVER_CONTINUE_BTN_Y = -113f;                           // gem (left) / watch (right) pair
        public const float GAMEOVER_CONTINUE_BTN_X = 103f;                            // ± from center
        public const float GAMEOVER_CONTINUE_BTN_W = 200f;                            // cream / blue blanks
        public const float GAMEOVER_GEM_ICON_H = 44f;                                 // gem on the cream button
        public const float GAMEOVER_GEM_ICON_X = -48f;
        public static readonly Vector2 GAMEOVER_GEM_COST_POS = new(20f, 2f);          // the cost, right of the gem
        public static readonly Vector2 GAMEOVER_GEM_COST_RECT = new(100f, 50f);
        public const float GAMEOVER_GEM_COST_SIZE = 32f;
        public static readonly Vector2 GAMEOVER_WATCH_LABEL_POS = new(26f, 2f);      // "Watch", right of the baked TV icon
        public static readonly Vector2 GAMEOVER_WATCH_LABEL_RECT = new(120f, 50f);
        public const float GAMEOVER_WATCH_LABEL_SIZE = 28f;
        public const float GAMEOVER_WATCH_LABEL_SIZE_MIN = 12f;                       // "Loading..." shrink floor
        public const float GAMEOVER_NAV_BTN_Y = -244f;                                // Main Menu (left) / Retry (right)
        public const float GAMEOVER_NAV_BTN_X = 108f;                                 // ± from center
        public const float GAMEOVER_NAV_BTN_W = 215f;                                 // green / yellow blanks
        public static readonly Vector2 GAMEOVER_NAV_LABEL_NUDGE = new(-3f, 5f);      // word toward the face center (shadow is bottom-right)
        public static readonly Vector2 GAMEOVER_NAV_LABEL_RECT = new(190f, 100f);
        public const float GAMEOVER_NAV_LABEL_SIZE = 30f;
        public static readonly Vector2 GAMEOVER_STARS_POS = new(0f, -350f);          // "N stars earned!" below the panel
        public static readonly Vector2 GAMEOVER_STARS_RECT = new(400f, 40f);
        public const float GAMEOVER_STARS_SIZE = 24f;
        #endregion

        #region Layout — Modal Panels (the shared Settings / Credits chrome, ModalPanel)
        // Each screen's panel sheet (ui_modal_panel for Settings, ui_credits_panel for Credits) is a
        // full-phone canvas like the game-over one: shown at REFERENCE_RESOLUTION the orange title
        // tab and the dotted cream body land where drawn. Title/X positions below are for the
        // Settings sheet (measured off Look Reference/settings.png, 536x948 ≈ the reference); a
        // screen whose sheet draws the tab elsewhere passes a chrome offset (CREDITS_CHROME_OFFSET).
        public static readonly Vector2 MODAL_TITLE_POS = new(0f, 176f);              // the title word on the orange tab
        public static readonly Vector2 MODAL_TITLE_RECT = new(400f, 70f);
        public const float MODAL_TITLE_SIZE = 40f;
        public static readonly Vector2 MODAL_CLOSE_POS = new(216f, 215f);            // round X, over the tab's top-right corner
        public const float MODAL_CLOSE_H = 84f;
        #endregion

        #region Layout — Settings Panel
        // In-game it gets its own canvas: above the game-over panel (100), below the shop (120).
        public const int SETTINGS_CANVAS_SORT = 110;
        // Rows: full-width blue blanks stacked down the body (Sound, Controls, then the in-game
        // Restart | Quit pair). The blank's canvas includes its drop shadow (face ~10% smaller).
        public const float SETTINGS_ROW_W = 380f;
        public const float SETTINGS_ROW_TOP_Y = 47f;                                 // first row center
        public const float SETTINGS_ROW_PITCH = 112f;                                // row-to-row spacing
        public const float SETTINGS_ROW_LABEL_SIZE = 34f;
        public const float SETTINGS_ROW_LABEL_SIZE_MIN = 14f;                        // AutoFit floor
        public static readonly Vector2 SETTINGS_ROW_LABEL_NUDGE = new(-3f, 4f);     // word toward the face center (shadow is bottom-right)
        // Dev-only start-level stepper ([−] Lv N [+]) below the panel: flat placeholder widgets.
        public const float SETTINGS_DEV_STEPPER_Y = -330f;
        public const float SETTINGS_DEV_STEPPER_X = 135f;                            // ± for the −/+ buttons
        public static readonly Vector2 SETTINGS_STEPPER_LABEL_SIZE = new(210f, 55f);
        public static readonly Vector2 SETTINGS_STEPPER_BTN_SIZE = new(55f, 55f);
        public const float SETTINGS_DEV_TEXT_SIZE = 22f;

        // How-to-play panel (the "?" top-bar button, in-game + menu; on the modal chrome)
        public const float HOWTO_TOP_Y = 120f;                                       // first rule line center
        public const float HOWTO_PITCH = 118f;                                       // line-to-line spacing (rect height too; max 3 lines/page)
        public const float HOWTO_LINE_W = 370f;
        public const float HOWTO_TEXT_SIZE = 22f;
        public const float HOWTO_BTN_TEXT_SIZE = 30f;                                // the "?" on the top-bar button
        public const float HOWTO_PAGER_Y = -255f;                                    // "1/3" + arrows row
        public const float HOWTO_PAGER_SIZE = 26f;
        public const float HOWTO_ARROW_X = 120f;                                     // arrows flank the pager
        public const float HOWTO_ARROW_H = 56f;                                      // ui_arrow_yellow sized by height, then rotated
        public const float HOWTO_ARROW_ROT_LEFT = -90f;                              // z-rotations turning the arrow art sideways —
        public const float HOWTO_ARROW_ROT_RIGHT = 90f;                              // flip both signs if the art points the other way
        #endregion

        #region Layout — Credits Panel
        // Its own sheet (ui_credits_panel — a taller, wider panel than Settings', tab ~38 px higher;
        // body from +268 down to −276) on the modal chrome, to Look Reference/Credits.png. Three
        // entries: a colored role heading over the kit's checkered band (text-free, translucent)
        // carrying the name in the HUD palette. Positions are canvas-centered; each entry hangs off
        // its heading center.
        public static readonly Vector2 CREDITS_CHROME_OFFSET = new(0f, 38f);         // title + X up to the credits sheet's tab
        public const float CREDITS_FIRST_Y = 113f;                                   // first heading center
        public const float CREDITS_PITCH = 124f;                                     // entry-to-entry spacing
        public const float CREDITS_ROLE_SIZE = 30f;
        public static readonly Vector2 CREDITS_ROLE_RECT = new(400f, 44f);
        public const float CREDITS_BAND_W = 392f;                                    // band canvas width (face ≈ 360 — the art has a ~4% margin); ≈ 120 tall
        public const float CREDITS_BAND_DY = -70f;                                   // band center below the heading
        public static readonly Vector2 CREDITS_NAME_NUDGE = new(0f, -2f);            // name center vs band center
        public static readonly Vector2 CREDITS_NAME_INSET = new(56f, 36f);           // name rect = band canvas minus this (inside the face; auto-fit bounds)
        public const float CREDITS_NAME_SIZE = 34f;                                  // single names; multi-line lists auto-fit down
        public const float CREDITS_NAME_SIZE_MIN = 14f;
        public static readonly Color CREDITS_GAME_ROLE = new(0.55f, 0.78f, 0.25f);   // lime heading (green band)
        public static readonly Color CREDITS_ART_ROLE = new(0.25f, 0.66f, 0.96f);    // sky heading (blue band)
        public static readonly Color CREDITS_MUSIC_ROLE = new(0.93f, 0.62f, 0.13f);  // orange heading (orange band)
        #endregion

        #region Layout — Shop Screen (a tall page over the dimmed screen; header + vertical page scroll)
        public const int SHOP_CANVAS_SORT = 120;                                // above every in-game canvas (HUD 50, slots 90, game-over 100)
        // Page art (ui_shop_page, Shop_Background): a full-phone canvas like the other screens — the
        // striped awning with SHOP baked in and the dotted cream body (awning 38→142 px from the top,
        // body to 928) — shown at REFERENCE_RESOLUTION. Positions measured off
        // Look Reference/Shop_example_*.png (573x966 ≈ the 540x960 reference).
        public static readonly Vector2 SHOP_CLOSE_POS = new(192f, -47f);        // round X over the awning's corner (from top-center)
        public const float SHOP_CLOSE_H = 78f;
        public const float SHOP_TOPBAR_DROP = 141f;                             // the shared TopBar pills, moved down into the page
        public const float SHOP_TOPBAR_X_NUDGE = 40f;                           // …and nudged right to center the 3 pills (the bar recipe hugs the left)
        // Scroll viewport: the page body between the pills and the page bottom. Side inset + content
        // padding center the 3-column grids (3 × SHOP_CELL_W + 2 × SHOP_CELL_SPACING) in the body.
        public const float SHOP_SCROLL_TOP = 215f;
        public const float SHOP_SCROLL_BOTTOM = 60f;
        public const float SHOP_SCROLL_SIDE = 62f;
        public const float SHOP_SCROLL_SENSITIVITY = 30f;
        public const int SHOP_CONTENT_PADDING = 8;                              // page edges (RectOffset — int)
        public const int SHOP_CONTENT_BOTTOM_PADDING = 24;
        public const float SHOP_SECTION_SPACING = 8f;
        // Text: section titles in the HUD palette; names/amounts in the mock's lime accent.
        public const float SHOP_SECTION_TITLE_H = 54f;
        public const float SHOP_SECTION_TITLE_SIZE = 34f;
        public const float SHOP_SUBTITLE_SIZE = 18f;                            // small brown text (Restore Purchases)
        public const float SHOP_RESTORE_H = 34f;                                // "Restore Purchases" text button under the gem grid
        public static readonly Color SHOP_PURCHASE_BLOCKER = new(0f, 0f, 0f, 0.25f); // input shield while a store purchase is in flight
        public static readonly Color SHOP_ACCENT = new(0.62f, 0.75f, 0.20f);    // lime: cell names, pack amounts, THANK YOU
        // Cells (skins, power-ups, currency packs): an authored box (skin checker / item box, sized by
        // width at native aspect) with an optional lime label line above and a wide green price pill
        // below; the whole cell is the button. Skin rows sit on the 9-sliced cream slab.
        public const float SHOP_CELL_W = 125f;                                  // 3 × W + 2 × spacing ≈ the content width, so grids align with the full-width boxes
        public const float SHOP_CELL_SPACING = 12f;
        public const float SHOP_CELL_LABEL_H = 24f;                             // the name line half-overlaps the box top edge (mock)
        public const float SHOP_CELL_LABEL_SIZE = 17f;
        public const float SHOP_CELL_PILL_OVERLAP = 16f;                        // the pill rides over the box bottom (mock)
        public const float SHOP_ROW_SLAB_PAD = 14f;                             // cells inset inside the slab
        public const float SHOP_CELL_PILL_W = 125f;                             // green wide blank width …
        public const float SHOP_CELL_PILL_H = 46f;                              // … stretched thicker than its native ≈36 (mock pills are chunky)
        public const float SHOP_PILL_TEXT_SIZE = 20f;
        public const float SHOP_PILL_ICON_H = 27f;                              // currency icon on a pill, right of the number
        public const float SHOP_PILL_ICON_GAP = 3f;
        public const float SHOP_PILL_ICON_Y_NUDGE = -2f;                        // icon down vs the text (digits sit high above the baseline)
        public static readonly Vector2 SHOP_PILL_LABEL_NUDGE = new(0f, 4f);    // centered; lifted off the art's baked bottom shadow
        public const float SHOP_WATCH_LABEL_MIN = 8f;                           // WATCH / LOADING... / TOMORROW! shrink floor on the FREE gems pill
        public const float SHOP_SKIN_CHEF_H = 118f;                             // chef previews fill the box (no plate)
        public const float SHOP_SKIN_CHEF_Y = 2f;
        public const float SHOP_SKIN_PREVIEW_H = 66f;                           // ingredient previews sit ON the plate
        public const float SHOP_SKIN_PREVIEW_MAX_W = 92f;
        public const float SHOP_SKIN_PREVIEW_Y = -6f;                           // preview center vs box center (resting on the plate)
        public const float SHOP_SKIN_PLATE_W = 106f;                            // the plate under ingredient previews
        public const float SHOP_SKIN_PLATE_Y = -34f;
        public const float SHOP_SKIN_BUN_W = 88f;                               // bun-pair preview: BOTH halves sized by width (equal heights made the squat bottom read smaller)
        public const float SHOP_SKIN_BUN_GAP = 2f;                              // vertical gap between bottom and top (tight — reads as one bun)
        public const float SHOP_SKIN_BUN_BOTTOM_Y = -22f;                       // bottom bun center (pair centered in the box, seated on the plate)
        public const float SHOP_ITEM_ICON_H = 86f;                              // power-up / currency icons inside the box
        public static readonly Vector2 SHOP_COUNT_BADGE_POS = new(-50f, 48f);  // owned-count badge on the box's top-left corner
        public const float SHOP_COUNT_BADGE_H = 34f;
        public const float SHOP_COUNT_BADGE_TEXT = 17f;
        public static readonly Vector2 SHOP_QTY_POS = new(40f, -44f);          // "xN" bottom-right of the box
        public static readonly Vector2 SHOP_QTY_RECT = new(60f, 30f);
        public const float SHOP_QTY_SIZE = 24f;
        // Wide rows spanning the content width: the authored Remove-Ads banner (price, bonus and the
        // ONE TIME BUY tag are baked in — keep MonetizationConfig's REMOVE_ADS_* in step with the art),
        // the THANK YOU box and the Pro Cook Pack bundle (9-sliced item box).
        public static float SHOP_CONTENT_W => REFERENCE_RESOLUTION.x - 2f * (SHOP_SCROLL_SIDE + SHOP_CONTENT_PADDING);
        public const float SHOP_BANNER_H = 110f;                                // remove-ads offer / THANK YOU box
        public const float SHOP_BANNER_TEXT_SIZE = 26f;
        public static readonly Vector2 SHOP_BANNER_TEXT_INSET = new(24f, 12f);
        // Remove-Ads offer banner (composed from widgets 2026-09-05 — the baked ui_shop_remove_ads
        // mock is retired; texts anchor to the box's LEFT edge, the price pill to its RIGHT edge)
        public static readonly Vector2 SHOP_BANNER_TITLE_POS = new(34f, 30f);
        public static readonly Vector2 SHOP_BANNER_TITLE_RECT = new(240f, 30f);
        public const float SHOP_BANNER_TITLE_SIZE = 26f;
        public static readonly Vector2 SHOP_BANNER_TAG_POS = new(34f, 8f);     // small line, ~as wide as the title above
        public static readonly Vector2 SHOP_BANNER_TAG_RECT = new(240f, 16f);
        public const float SHOP_BANNER_TAG_SIZE = 11f;
        public static readonly Vector2 SHOP_BANNER_BONUS_POS = new(34f, -26f); // the BIG "+100 ◆" line — as heavy as the two texts above together
        public static readonly Vector2 SHOP_BANNER_BONUS_RECT = new(210f, 44f);
        public const float SHOP_BANNER_BONUS_SIZE = 30f;
        public const float SHOP_BANNER_BONUS_ICON_H = 38f;                      // gem icon a touch taller than the font
        public const float SHOP_BANNER_PILL_W = 150f;                           // ui_btn_green_big on the banner …
        public const float SHOP_BANNER_PILL_H = 90f;                            // … stretched as tall as the whole left column
        public const float SHOP_BANNER_PILL_X = -88f;
        public const float SHOP_BANNER_PILL_TEXT = 34f;                         // the price fills the pill (auto-fit, floor 14)
        public const float SHOP_BANNER_DOT_H = 44f;                             // red ONE TIME BUY tag
        public static readonly Vector2 SHOP_BANNER_DOT_POS = new(-4f, -4f);    // centered on the PILL's top-right corner
        public const float SHOP_BANNER_DOT_TEXT = 9f;
        public const float SHOP_BUNDLE_H = 110f;
        public const float SHOP_BUNDLE_ICON_H = 92f;                            // the condiment tray
        public const float SHOP_BUNDLE_ICON_X = 104f;                           // tray center from the left edge
        public const float SHOP_BUNDLE_ICON_Y = 6f;
        public static readonly Vector2 SHOP_BUNDLE_QTY_POS = new(186f, 0f);     // "xN" right of the tray (from the left edge)
        public static readonly Vector2 SHOP_BUNDLE_NAME_POS = new(118f, -44f);  // "PRO COOK PACK" under the tray (center, from the left edge)
        public static readonly Vector2 SHOP_BUNDLE_NAME_RECT = new(240f, 28f);
        public const float SHOP_BUNDLE_NAME_SIZE = 22f;
        public const float SHOP_BUNDLE_PILL_W = 150f;                           // ui_btn_green_big sized by width (≈ 82 tall)
        public const float SHOP_BUNDLE_PILL_X = -88f;
        public const float SHOP_BUNDLE_PILL_TEXT_SIZE = 26f;
        public const float SHOP_BUNDLE_PILL_ICON_H = 30f;
        // Confirm dialog (gem spends only): the authored card (inner text box baked in its top half)
        // with the offer lines and the BUY / CANCEL blanks. Widths are the sprites' canvases (they
        // include the drop shadows): 530 puts the card's face at ~400 px.
        public const float SHOP_CONFIRM_CARD_W = 530f;
        public const float SHOP_CONFIRM_LINE1_Y = 70f;                          // "Buy N ★" (inner box center ≈ +53)
        public const float SHOP_CONFIRM_LINE2_Y = 36f;                          // "for N ◆"
        public static readonly Vector2 SHOP_CONFIRM_LINE_RECT = new(340f, 40f);
        public const float SHOP_CONFIRM_TEXT_SIZE = 28f;
        public const float SHOP_CONFIRM_ICON_H = 34f;
        public const float SHOP_CONFIRM_BTN_X = 100f;                           // pills flank the center
        public const float SHOP_CONFIRM_BTN_Y = -58f;
        public const float SHOP_CONFIRM_BTN_W = 195f;
        public const float SHOP_CONFIRM_BTN_TEXT = 24f;
        #endregion
    }
}
