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
        public static readonly Color32 HUD_TEXT_BORDER = new(0x12, 0x0A, 0x05, 0xFF);  // near-black — the OUTER ring/shadow layer (was #492611 brown; black third layer per Oscar 2026-09-08; also the border of colored headings)
        public static readonly Color32 HUD_TEXT_STROKE = new(0x88, 0x46, 0x2A, 0xFF);  // #88462A mid brown — the inner stroke (sampled off the mock's PLAY)
        public const float HUD_TEXT_BORDER_WIDTH = 0.19f;                               // TMP outline width (0..1) — tune live (0.25 pre-Baloo; thicker per Oscar 2026-09-08)
        public const float TEXT_LINE_SPACING = -45f;                                    // global leading trim — Baloo's native line height is huge (overflowed How-to, wrapped the one-time-buy tag); negative tightens
        // ...but that trim is for SINGLE-LINE labels. On a WRAPPED paragraph it pulls the lines
        // into each other (descenders hitting the next line's caps) - the tutorial tips and the
        // How-to bullets were unreadable blocks (Oscar, 2026-09-13). Paragraphs get their own,
        // far gentler value; UIFactory picks between the two on the `wrap` flag.
        public const float TEXT_LINE_SPACING_WRAP = -10f;
        public const float TEXT_CHARACTER_SPACING = -2f;                                // global tracking, TMP units (~0.01 em) — negative tightens; Baloo tracks looser than Panton did
        public const float TEXT_FACE_DILATE = 0.06f;                                    // weight trim (negative thins) — slightly positive so the thicker outline pushes OUTWARD instead of eating the fill. Tune live; styled AND plain texts
        // The sticker drop shadow (TMP Underlay) applied by StyleFillAndBorder to EVERY bordered
        // text, matching the artist's Photoshop stroke+shadow recipe (Look Reference/Font info.png).
        // Offsets/dilate are in SDF *spread* units (-1..1): the on-screen reach = value × atlas
        // padding / sampling point size. ⚠️ The shadow only gets properly chunky once the SDF atlas
        // is regenerated with a bigger spread — 2048 atlas / padding 24 / sampling 144 / SDFAA
        // (the original 12-padding atlas caps the whole effect at ~1px per 30px of text).
        public const float TEXT_SHADOW_OFFSET_X = 0f;
        public const float TEXT_SHADOW_OFFSET_Y = -0.25f;                                // straight down; big values read as the dark layers sagging
        public const float TEXT_SHADOW_DILATE = 0.65f;                                   // black outer ring reach — 0.5 matches the pre-rebake max; headroom to 1.0 (knobs are padding-normalized: rescaled /2 for the 16px atlas, 2026-09-08)
        public const float TEXT_SHADOW_SOFTNESS = 0f;                                   // hard edge — a sticker, not a blur
        #endregion

        #region Text Colors
        public static readonly Color TEXT_HUD = Color.black;
        public static readonly Color TEXT_UI = Color.white;
        public static readonly Color TEXT_TOO_BAD = Color.red;      // the one non-cream popup: failure must read differently
        #endregion

        #region Popup Colors
        // World popups are cream (HUD_TEXT_FILL) with two identity exceptions (Oscar,
        // 2026-09-05): the burger's FINAL score pops gold and fast-drop bonuses pop sky blue,
        // so neither is confused with match popups. "Too bad!" stays red. The glow PLATES still
        // carry the rest of the meaning (green = score, yellow = multiplier/stars).
        public static readonly Color GOLD = new(1f, 0.85f, 0f);     // game-over "stars earned", shop badges
        public static readonly Color BURGER_SCORE_POPUP = new(1f, 0.85f, 0f);   // the burger popup's score line
        public static readonly Color FAST_DROP_POPUP = new(0.5f, 0.85f, 1f);    // fast-drop bonus "N!"
        #endregion

        #region Panel / Overlay Colors
        public static readonly Color OVERLAY_DIM = new(0, 0, 0, 0.7f);
        public static readonly Color MODAL_OVERLAY = new(0f, 0f, 0f, 0.35f);   // tint over the frosted backdrop behind a full-canvas panel (0.55 when it was the only dim, pre-blur)
        // Frosted backdrop behind modals (2026-09-17, ScreenBlur/BlurBackdrop): the screen is snapshotted,
        // downscaled by DOWNSCALE, then Gaussian-blurred PASSES times with RADIUS (in downscaled texels).
        public const bool MODAL_BLUR_ENABLED = true;
        public const int MODAL_BLUR_DOWNSCALE = 4;
        public const float MODAL_BLUR_RADIUS = 1.5f;
        public const int MODAL_BLUR_PASSES = 2;
        public const bool MODAL_BLUR_FLIP_ON_TOP_ORIGIN = true;                // the backbuffer read is upside down on D3D/Metal; flip there. Toggle if it shows inverted.
        public static readonly Color SCREEN_FLASH = new(1f, 1f, 1f, 0.6f);
        #endregion

        #region Button Colors
        #endregion

        #region Consumable / Fairy Sizes (world-space heights)
        // Reward sprites import large (100 PPU); everything is normalized to a target world height
        // via SpriteFit rather than a raw scale, so the source pixel size doesn't matter.
        // Full-body per-payload illustration (the cargo is drawn in) — a touch bigger than the
        // old body-only sprite so the payload stays readable without the badge overlay.
        public const float FAIRY_BODY_HEIGHT = 1.5f;
        // The cargo in the fairy's hand (world units, vs the fairy's centre) — the gem's spot on the
        // old red gem fairy (+0.181/-0.034 × body height, 0.25 × body height tall). Tune live.
        public static readonly Vector3 FAIRY_CARGO_POS = new(0.27f, -0.05f, 0f);
        public const float FAIRY_CARGO_HEIGHT = 0.37f;                    // base VISIBLE height of the cargo art…
        public const float FAIRY_CARGO_SCALE_GEMS = 1.1f;                 // …× this per payload (Oscar 2026-09-26: gems a touch bigger,
        public const float FAIRY_CARGO_SCALE_STARS = 1f;                  //  stars as they were,
        public const float FAIRY_CARGO_SCALE_CONSUMABLE = 1.4f;           //  consumables clearly bigger)
        public const float PREVIEW_ARROW_HEIGHT = 1.05f;                   // arrow back-picture behind a preview ghost
        // World popup plates (the 2026-09-04 halftone blobs, set 1) — heights in world units.
        public const float PLATE_BURGER_H = 2.0f;                          // wide green ellipse behind the burger name + points
        public static readonly Vector2 PLATE_BURGER_OFFSET = new(0f, -0.35f); // centered between the name and the score line
        public const float PLATE_SCORE_H = 1.1f;                           // green round behind "N!" score popups
        public const float PLATE_FLOAT_H = 1.0f;                           // round blob behind floating texts that ask for one
        public const float FLOAT_ICON_H = 0.28f;                           // inline reward icon on a floating popup (fairy loot)  // 0.45 → 0.28: star/gem cropped 2026-09-17
        public const float FLOAT_ICON_GAP = 0.28f;                         // gap between the amount and its icon
        public const float FLOAT_ICON_Y = 0.02f;                           // icon vertical trim vs the text line
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
        public const float CONSUMABLE_SLOT_ICON_H = 68f;                        // consumable icon (width follows aspect; 58 → 68, artist 2026-09-17)
        public static readonly Vector2 CONSUMABLE_ICON_OFFSET = new(0f, -1f);   // icon offset within the plate (sat a touch high — artist 2026-09-17)
        public const float CONSUMABLE_BADGE_H = 40f;                            // num/plus badge (width follows aspect; 34 → 40, artist 2026-09-17)
        public static readonly Vector2 CONSUMABLE_BADGE_OFFSET = new(28f, -28f);// badge offset (bottom-right of plate)
        public const float CONSUMABLE_COUNT_SIZE = 30f;                         // count number on the num box (scaled with the badge)
        public const float CONSUMABLE_ROW_Y = -233f;                            // row Y (margin below the Level/Score cards)
        // Span the same zone as Level/Score: 80-wide plates at 58/144/230 → left edge 18, right edge 270.
        public const float CONSUMABLE_SLOT_X_START = 58f;                       // first slot center X
        public const float CONSUMABLE_SLOT_SPACING = 86f;                       // gap between slot centers
        // Grab radius padding around the plate (2026-09-13). The count badge is drawn at
        // CONSUMABLE_BADGE_OFFSET and overhangs the plate by ~5px, so plate-only hit-testing left
        // the NUMBER itself un-grabbable and a dead band between slots — pressing there fell
        // through to chef/preview logic instead of starting a carry. Padding past half the
        // spacing is fine: overlapping slots are resolved by nearest center, never by order.
        public const float CONSUMABLE_SLOT_HIT_PADDING = 15f;                   // +3 with the 40px badge so the number stays grabbable
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
        // Minimum side gap between a red-box word and its border (fraction of the label rect width),
        // applied as a TMP margin so auto-size shrinks the word BEFORE it touches the red (artist
        // note 2026-09-17). Only the LONG strings (ES "PUNTOS") ever sit at this minimum — short
        // ones (EN) fit at full size with slack — so judge it in Spanish/German, not English. The
        // sticker border + shadow draw outside the measured glyph box and eat a few px of it.
        public const float HUD_RED_LABEL_SIDE_PAD_FRAC = 0.2f;
        // Minimum side gap between a button's word and the edge of its FACE (fraction of the face
        // width — see ButtonFace / UIFactory.CreateFaceLabel). Every menu + shop button shares it.
        public const float BUTTON_LABEL_SIDE_PAD_FRAC = 0.08f;
        // Vertical pad each side of a word's CAP HEIGHT inside the face (fraction of the face height):
        // sets the size ceiling — short words grow until their caps fill the face minus this.
        public const float BUTTON_LABEL_VERTICAL_PAD_FRAC = 0.18f;
        public const float BUTTON_LABEL_CHARACTER_SPACING = -6f;                 // tighter tracking on button words — safe now that the outline wraps the WHOLE word (MergedOutlineText); global text is -2
        public const float BUTTON_LABEL_MIN_FONT_SIZE = 10f;                     // shrink floor for a word on a face when the site gives none (icon-line pills)
        // The kit's blanks carry a darker bottom bevel; this fraction of the body height is treated
        // as bevel, lifting words onto the lit face above it — the same on every button. 0 = centre
        // on the whole body inside the outline (0.1 read as "still high", Oscar 2026-09-17).
        public const float BUTTON_FACE_LIP_FRAC = 0f;
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
        public const float SPECIAL_BANNER_H = 54f;                            // SPECIAL ORDER banner (width follows aspect; 60 → 54, artist 2026-09-17)
        public const float SPECIAL_BANNER_STRETCH_X = 1.15f;                  // widen the red banner past native aspect (deliberate)
        public static readonly Vector2 SPECIAL_BANNER_OFFSET = new(-40f, 78f);// banner offset within the card (overhangs top-left)
        public const float SPECIAL_BANNER_LABEL_SIZE = 18f;                   // "SPECIAL ORDER" TMP (auto-size max)
        public const float SPECIAL_BANNER_LABEL_W_FRAC = 0.8f;                       // label rect vs the banner art (its canvas has transparent margins)
        public const float SPECIAL_BANNER_LABEL_SIZE_MIN = 7f;                // auto-size floor
        // Minimum side gap inside the band, as a fraction of the LABEL rect (which W_FRAC already
        // shrank to the visible band — so this is smaller than the tabs' HUD_RED_LABEL_SIDE_PAD_FRAC).
        public const float SPECIAL_BANNER_LABEL_SIDE_PAD_FRAC = 0.07f;
        public static readonly Vector2 SPECIAL_BANNER_LABEL_OFFSET = new(6f, 3f); // right + up a touch to sit in the bubble (Capline-centred since 2026-09-17)
        // Stack sprites (ingredients/buns/plate) are sized from their WORLD dimensions (pixel rect /
        // PPU × this factor) — the same per-file normalization the playfield uses, so the stack's
        // proportions match the game (pieces ~1.2 world units wide → ~60px, buns 1.38 → ~69px).
        // Sizing by raw pixel aspect ignored the per-file PPU tuning and transparent padding, which
        // made the plate tiny and the ingredient ratios inconsistent.
        // The stack FIT-SCALES (2026-09-17, artist pass): drawn at PX_PER_UNIT and shrunk uniformly
        // only when the whole order (top bun → plate bottom) is taller than the free area under the
        // banner — so small orders fill the card as in the reference and the max order always fits.
        public const float SPECIAL_STACK_PX_PER_UNIT = 76f;                   // screen px per world unit at full size (62 → 76)
        public const float SPECIAL_INGREDIENT_SPACING = 27f;                  // vertical stack spacing at full size (tight overlap)
        public const float SPECIAL_CARD_INNER_PAD = 8f;                       // the card art's margin: free area inset from the card's left/bottom edges
        public const float SPECIAL_STACK_METER_GAP = 4f;                      // free area ends this far left of the meter tube
        // The stack is centred in the free area (card left → tube left, banner bottom → card bottom),
        // so "centred" already accounts for the meter eating the card's right side. Nudge from there.
        public static readonly Vector2 SPECIAL_STACK_NUDGE = new(8f, 0f);     // a touch right (artist 2026-09-17)
        public const float SPECIAL_PLACEHOLDER_LABEL_SIZE = 18f;              // "+N" on the mystery silhouette
        public const float SPECIAL_MYSTERY_H = 58f;                           // mystery silhouette at full size (UI art, no tuned PPU — sized by height; matches the 76px/unit stack)
        public const float SPECIAL_PLATE_Y_OFFSET = 14f;                      // plate drop below the bottom bun (at full size)
        // The bone pin (ui_burger_pin, artist 2026-09-17) standing in the top bun. Sized by height at
        // full scale; its stick sinks EMBED px below the bun's top edge; X offsets it off the bun's centre.
        public const float SPECIAL_PIN_H = 40f;
        public const float SPECIAL_PIN_EMBED = 18f;                           // stick visibly INTO the bun (artist 2026-09-17)
        public const float SPECIAL_PIN_X = 8f;                                // off the bun's centre, to the right
        public const float SPECIAL_PLATE_SCALE = 1.2f;                        // plate vs its playfield proportion — the reference dish is wider than the bun (also costs stack height: the ellipse is tall)
        public const float SPECIAL_MULT_BADGE_H = 52f;                        // multiplier badge (reuses the red num box; 42 → 52 so "x1.25" fits)
        // Badge centre relative to the tube's BOTTOM-CENTRE: the xN badge sits on the capsule's
        // bottom cap, on the gauge's own axis. With BOTTOM_INSET −6 and +6 here its centre lands
        // exactly on the card's bottom edge — half in, half out (artist 2026-09-17).
        public static readonly Vector2 SPECIAL_MULT_BADGE_OFFSET = new(0f, 6f);
        public const float SPECIAL_MULT_TEXT_SIZE = 26f;
        public const float SPECIAL_MULT_TEXT_SIZE_MIN = 12f;                  // auto-size floor for "x1.25"
        public const float SPECIAL_GHOST_ALPHA = 1f;                          // the "?" mystery layer on Contains orders — opaque by decision (2026-09-04); lower for a faded ghost
        // Mult meter — the kit's blank red MULT box (ui_mult_box, rect cropped to the visible box)
        // capping a vertical capsule (back well + green fill + frame, 3 stacked layers at one rect).
        // Children of the card, built before the mult badge so the badge renders on top. The tube's
        // height is DERIVED: from under the box down past the card's bottom edge, so it always
        // spans the card. REFERENCE LINE (artist 2026-09-17): the box's top edge sits on the SPECIAL
        // ORDER banner's top edge — the tube hangs off that; the xN badge straddles the card's
        // bottom edge, half in / half out.
        public const float MULT_METER_X = 86f;                               // box + tube + badge centre x within the card
        public const float MULT_TAB_H = 37f;                                 // MULT box height (cropped art — this IS the visible box; width follows its 274:160 aspect)
        public const float MULT_TAB_Y_NUDGE = -5f;                           // box top vs the banner CANVAS top: the banner art carries ~5px of transparent margin above its visible edge
        public const float MULT_TAB_LABEL_SIZE = 20f;
        public const float MULT_TAB_LABEL_SIZE_MIN = 8f;
        public const float MULT_TAB_LABEL_SIDE_PAD_FRAC = 0.1f;              // MULT word vs the box edge — a short word on a cropped box needs less than the shared red-label pad
        public const float MULT_METER_TAB_OVERLAP = 28f;                     // tube top tucks up under the box by this much (the box covers the tube's top cap)
        public const float MULT_METER_BOTTOM_INSET = -6f;                    // tube bottom vs the card bottom edge (+ = inside; a small overhang so the cap covers the card's corner)
        public const float MULT_METER_FILL_BOTTOM_EXTEND = 20f;              // extend the green fill's rect down to meet the well bottom (closes the gap)
        #endregion

        #region Top Bar (shared TopBar component: authored currency widgets + buttons — reference px)
        // TopBar LAYS ITSELF OUT (artist note, 2026-09-26): trophy · star · gem · ? · gear between two
        // EQUAL side margins measured on VISIBLE edges, evenly spaced; icons bigger than the pills,
        // each number centred between its icon and the pill's right end. No per-item positions.
        public const float TOPBAR_Y = -38f;                                // vertical center of the bar
        public const float TOPBAR_SIDE_MARGIN = 12f;                       // screen edge → first/last visible edge, same both sides
        public static readonly Vector2 TOPBAR_BOX_SIZE = new(116f, 42f);   // currency pill (ui_currency_box native ≈ 2.12:1)
        // Per-icon HEIGHT (sprites cropped to their art incl. the drop shadow) — width follows the
        // native aspect. All taller than the pill, as in the concept.
        // Balanced BY EYE on the tight crops (2026-09-26 Result.png review): the star is a solid,
        // wide shape and reads heaviest, so it runs smallest; the trophy's thin handles need the most.
        public const float TOPBAR_SCORE_ICON_H = 54f;                      // high-score trophy
        public const float TOPBAR_STAR_ICON_H = 47f;
        public const float TOPBAR_GEM_ICON_H = 51f;
        public const float TOPBAR_ICON_X = -48f;                           // icon centre vs pill centre — steps onto the pill, hiding its left end
        public const float TOPBAR_ICON_Y = 6f;                             // …and raised, so the icon sticks out more over the pill's TOP (artist note 2)
        public const float TOPBAR_NUMBER_SIDE_PAD = 7f;                    // breathing room at BOTH ends of the number zone (icon edge ↔ pill end) — auto-size filled it wall to wall at 0
        public const float TOPBAR_NUMBER_Y = 1.5f;                         // lifted a touch above the pill's vertical center
        public const float TOPBAR_NUMBER_SIZE = 19f;                       // auto-size max — low enough that a 5-char "99.3K" fits every pill's zone, so all three read the same size
        public const float TOPBAR_NUMBER_SIZE_MIN = 10f;                   // auto-size floor
        public static readonly Color TOPBAR_NUMBER_COLOR = new(0.28f, 0.17f, 0.1f); // solid dark brown — the top-bar pill numbers (artist note 2026-09-26; were HUD-palette 2026-09-03→26) + plain small labels
        public static readonly Vector2 TOPBAR_BUTTON_SIZE = new(58f, 58f); // the "?" + gear pair — same size on every screen
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
        // "NEW INGREDIENT!" callout (world units; NewIngredientPopup) — mid-board, under the order card
        public const float NEW_INGREDIENT_Y = -0.4f;                      // popup centre (world y; the board spans ≈ -4.2 → 1.0)
        public const float NEW_INGREDIENT_ICON_H = 1.3f;                  // the newcomer's sprite
        public const float NEW_INGREDIENT_TEXT_DY = 1.05f;                // words above the sprite
        public const float NEW_INGREDIENT_TEXT_SIZE = 5f;
        public const float NEW_INGREDIENT_TEXT_W = 5f;                    // auto-size rect (world units) — long translations shrink
        public const float NEW_INGREDIENT_PLATE_H = 1.25f;                // yellow glow plate behind the words
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
        public const float MENU_PLAY_FACE_BOTTOM = 212f;                    // PLAY's visible bottom edge, px above the screen bottom — just clears the "Support the devs!" line over SHOP (artist note 2026-09-26: PLAY low, the Dogtor's face visible)
        public const float MENU_PLAY_W = 400f;
        public const float MENU_PLAY_LABEL_SIZE = 84f;                       // PLAY word (HUD palette; face-centred via CreateFaceLabel)
        public const float MENU_PLAY_LABEL_SIZE_MIN = 40f;                   // AutoFit floor ("JOUER", "SPIELEN")
        // RESUME — shown only when a run is waiting (RunSnapshotStore). Sits ON TOP of PLAY and
        // never moves it: PLAY stays put whether or not there's a run to come back to.
        public const float MENU_RESUME_GAP = 24f;                            // RESUME's visible bottom sits this far above PLAY's visible top
        public const float MENU_RESUME_W = 330f;                             // deliberately smaller than PLAY — the secondary action
        public const float MENU_RESUME_LABEL_SIZE_MIN = 18f;                 // AutoFit floor (long words: "FORTSETZEN", "DEVAM ET")
        public const float MENU_BOTTOM_STRIP_W = 680f;                       // the checker strip: the ART has ~150px clear margins per side, so it must
                                                                             // outsize the canvas for the squares to reach the screen edges
        public const float MENU_BOTTOM_BTN_Y = 108f;                         // CREDITS/SHOP center height from the bottom edge
        public const float MENU_BOTTOM_BTN_X = 129f;                         // ± from center
        public const float MENU_BOTTOM_BTN_W = 245f;                         // authored red/yellow blanks sized by width (canvas incl. shadow)
        public const float MENU_CREDITS_LABEL_SIZE = 38f;                    // CREDITS word (HUD palette)
        public const float MENU_SHOP_LABEL_SIZE = 50f;                       // SHOP word — way bigger (short word, mock draws it huge)
        public const float MENU_BOTTOM_LABEL_SIZE_MIN = 18f;                 // AutoFit floor for CREDITS / SHOP (face-centred via CreateFaceLabel)
        // No red/orange blank exists in the kit — the cream blank is runtime-tinted (multiply).
        public const float MENU_SUPPORT_LABEL_Y = -16f;                      // "Support the devs!" sits ON the SHOP button's face top (canvas has shadow margins)
        public const float MENU_SUPPORT_LABEL_W = 205f;                      // auto-fit rect — narrower than the button = slightly smaller text
        public const float MENU_SUPPORT_LABEL_SIZE = 40f;                    // auto-size max
        public const float MENU_SUPPORT_LABEL_MIN = 12f;
        // The flashy green: a vertical gradient (sampled off DesiredMenu.png) under the dark
        // outline + downward shadow ring.
        public static readonly Color MENU_SUPPORT_TOP = new(0.66f, 0.85f, 0.36f);
        public static readonly Color MENU_SUPPORT_BOTTOM = new(0.42f, 0.62f, 0.0f);
        // The TEST BUILD stamp (TestBuild): between the logo and PLAY, loud red.
        public static readonly Vector2 MENU_TEST_BUILD_POS = new(0f, 20f);
        public static readonly Vector2 MENU_TEST_BUILD_RECT = new(400f, 50f);
        public const float MENU_TEST_BUILD_SIZE = 34f;
        public static readonly Color MENU_TEST_BUILD_COLOR = new(1f, 0.25f, 0.2f);
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
        public const float GAMEOVER_GEM_ICON_H = 27f;                                 // gem on the cream button  // 44 → 27: ui_gem cropped 2026-09-17
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
        public const int TUT_CANVAS_SORT = 105;                                      // tutorial callouts: over the game + game-over (100), UNDER settings/how-to (110) and the shop (120) — at 115 it drew over a panel opened mid-tutorial (2026-09-26)

        #region Tutorial Callout (the artist's cream box + red speech-bubble tag, 2026-09-26; yellow arrow)
        // Measured off Fixes/TutorialReference.png (1125 px wide -> 540 canvas). Both sprites are
        // cropped to their art, so rects are the visible shapes.
        // The callout lives in the HUD band ABOVE the board (2026-09-26): canvas y from the screen
        // centre at 9:16 — board top ≈ 170, top bar bottom ≈ 412. Full band width, or one half so the
        // order card (right) / consumable row (left) the step is about stays visible (TutorialBoxSlot).
        public const float TUT_BAND_TOP = 406f;                                      // the tag's top edge — just under the top bar
        public const float TUT_BAND_BOTTOM = 176f;                                   // the box's bottom edge — just above the board
        public const float TUT_BAND_SIDE_MARGIN = 14f;
        public const float TUT_BAND_HALF_GAP = 12f;                                  // space left between a half box and the screen centre line (×2)
        public const float TUT_BOX_ART_SCALE = 0.23f;                                // 9-slice border draw scale: the art's corners/bevel at the size the reference shows them
        public const float TUT_TAG_W = 288f;                                         // ui_tut_tag at full size (≈95 tall)
        public const float TUT_TAG_MAX_W_FRAC = 0.8f;                                // …shrunk to this share of a narrower (half) box
        public static readonly Vector2 TUT_TAG_POS = new(30f, 49f);                  // tag's TOP-LEFT vs the box's top-left (right, up) at full size; scales with the tag
        public const float TUT_TITLE_W_FRAC = 0.78f;                                 // title rect, as a share of the tag width (the tail takes the rest)
        public static readonly Vector2 TUT_TITLE_NUDGE = new(8f, 10f);               // title centre vs the tag centre (full size; scales) — onto the bubble's body, off the tail
        public const float TUT_TITLE_SIZE = 40f;                                     // at full tag size; scales with the tag
        public const float TUT_TITLE_SIZE_MIN = 16f;                                 // AutoFit floor: long titles ("ACELEN MI VAR?") must not outgrow the tag
        public const float TUT_BODY_TAG_CLEARANCE = 4f;                              // body text starts this far below the tag's bottom
        public const float TUT_BODY_BOTTOM_INSET = 30f;                              // …and ends above the tap-to-continue line
        public const float TUT_BODY_SIDE_INSET = 22f;                                // inside the box's border
        public const float TUT_BODY_SIZE = 30f;                                      // full-width box
        public const float TUT_BODY_SIZE_HALF = 24f;                                 // half box (narrower column)
        public const float TUT_BODY_SIZE_MIN = 14f;                                  // AutoFit floor: the box is fixed, so long translations shrink rather than clip
        public const float TUT_CONTINUE_ABOVE_BOTTOM = 16f;                          // tap-to-continue line's centre, above the box's bottom edge
        public const float TUT_CONTINUE_SIZE = 16f;
        public const float TUT_ARROW_H = 64f;
        public const float TUT_ARROW_BOB = 14f;                                      // idle bob amplitude (the child image tweens)
        public static readonly Vector2 TUT_ARROW_SLOT_POS = new(-212f, 320f);       // over the Ketchup slot, pointing down
        public static readonly Vector2 TUT_SKIP_POS = new(52f, -110f);
        public const float TUT_SKIP_SIZE = 22f;
        #endregion
        // Rows: full-width blue blanks stacked down the body (Sound, Controls, then the in-game
        // Restart | Quit pair). The blank's canvas includes its drop shadow (face ~10% smaller).
        public const float SETTINGS_ROW_W = 380f;
        public const float SETTINGS_ROW_TOP_Y = 47f;                                 // first row center
        public const float SETTINGS_ROW_PITCH = 112f;                                // row-to-row spacing
        public const float SETTINGS_ROW_LABEL_SIZE = 34f;
        public const float SETTINGS_ROW_LABEL_SIZE_MIN = 14f;                        // AutoFit floor
        public static readonly Vector2 SETTINGS_ROW_LABEL_NUDGE = new(-3f, 4f);     // word toward the face center (shadow is bottom-right)
        // Dev-only start-level stepper ([−] Lv N [+]) below the panel: flat placeholder widgets.
        // The START level row (menu, third row — replaced the mode toggle 2026-09-07): the same
        // blue blank, "START: LVL N" centered, a yellow arrow button INSIDE each end.
        public static readonly Vector2 SETTINGS_PANEL_OFFSET = Vector2.zero;         // whole-panel nudge for the 4-row sheet (tune live)
        public static readonly Vector2 SETTINGS_CHROME_OFFSET = Vector2.zero;        // title + X nudge if the 4-row sheet tab sits elsewhere

        // How-to-play panel (the "?" top-bar button, in-game + menu; on the modal chrome)
        // Page layout to the artist's reference (2026-09-26): canvas px from the panel centre on the
        // ui_settings_panel sheet (cream body spans y 122 -> -386, x -223 -> 219).
        public const float HOWTO_SUBPANEL_TOP = 72f;                                 // the darker rounded panel holding the page
        public const float HOWTO_SUBPANEL_BOTTOM = -318f;
        public const float HOWTO_SUBPANEL_W = 404f;
        public const float HOWTO_SUBPANEL_PAD = 22f;                                 // text inset from its sides
        public const float HOWTO_SUBPANEL_RADIUS = 18f;
        public static readonly Color HOWTO_SUBPANEL_COLOR = new(0.925f, 0.894f, 0.816f); // #ECE4D0 — sampled off the reference
        public const float HOWTO_HEADER_SIZE = 36f;                                  // lime page header, caps centred ON the subpanel's top edge
        public const float HOWTO_HEADER_SIZE_MIN = 18f;
        public const float HOWTO_BULLETS_TOP_Y = 38f;                                // top edge of the auto-height bullet list (grows downward)
        public const float HOWTO_BULLETS_BOTTOM_Y = -250f;                           // ...must end above this (the pager) — a long page shrinks to fit
        public const float HOWTO_BULLET_GAP = 16f;                                   // constant gap between bullets (each auto-sizes to its wrapped height)
        public const float HOWTO_TEXT_SIZE = 24f;                                    // bigger than before (22), as in the reference; long pages step down...
        public const float HOWTO_TEXT_SIZE_MIN = 16f;                                // ...to no smaller than this
        public static readonly Color HOWTO_TEXT_COLOR = new(0.286f, 0.149f, 0.067f); // #492611 — the reference's deep brown (was TOPBAR_NUMBER_COLOR)
        public const float HOWTO_BTN_TEXT_SIZE = 30f;                                // the "?" on the top-bar button
        public const float HOWTO_TUTORIAL_Y = -386f;                                 // PLAY TUTORIAL's face centre — ON the sheet's bottom edge
        public const float HOWTO_TUTORIAL_W = 300f;                                  // the menu PLAY's green blank, smaller
        public const float HOWTO_TUTORIAL_LABEL_SIZE = 40f;
        public const float HOWTO_TUTORIAL_LABEL_SIZE_MIN = 18f;                      // "TUTORIAL SPIELEN" etc. shrink
        public const float HOWTO_PAGER_Y = -284f;                                    // "1/6" + arrows, inside the subpanel's bottom
        public const float HOWTO_PAGER_SIZE = 28f;
        public const float HOWTO_ARROW_X = 84f;                                      // arrows flank the pager
        public const float HOWTO_ARROW_H = 40f;                                      // ui_arrow_pager (green, points right) sized by height
        // z-rotations turning ui_arrow_yellow sideways (a property of the art — shared by the
        // How-to pager and the Settings START level row); flip both signs if the art points the other way.
        #endregion

        #region Layout — Credits Panel
        // Its own sheet (ui_credits_panel — a taller, wider panel than Settings', tab ~38 px higher;
        // body from +268 down to −276) on the modal chrome, to Look Reference/Credits.png. Three
        // entries: a colored role heading over the kit's checkered band (text-free, translucent)
        // carrying the name in the HUD palette. Positions are canvas-centered; each entry hangs off
        // its heading center.
        public static readonly Vector2 CREDITS_CHROME_OFFSET = new(0f, 38f);         // title + X up to the credits sheet's tab
        // Measured off ui_credits_panel at the reference size (art px × 960/4138, y from the panel
        // centre): the orange header strip spans 944→1398 art px, the cream body 1400→3212.
        public const float CREDITS_TITLE_Y = 208f;                                  // CREDITS caps centred on the header strip
        public const float CREDITS_BODY_TOP = 155f;                                 // the three sections share the body top→bottom
        public const float CREDITS_BODY_BOTTOM = -265f;                             //   with equal gaps (CreditsPanel derives them)
        public const float CREDITS_BAND_FACE_W = 370f;                              // every band's VISIBLE face width (canvas margins differ per art)
        public const float CREDITS_BAND_FACE_ASPECT = 410f / 1601f;                 // face h / w — identical on all three band arts
        public const float CREDITS_ROLE_SIZE = 40f;                                 // A GAME BY … — caps centred on the band's top edge
        public const float CREDITS_ROLE_SIZE_MIN = 20f;                             // long translations shrink to the band width
        public const float CREDITS_ROLE_OVERHANG = 16f;                             // how far the heading's caps rise over the band (≈ half cap height + stroke)
        public static readonly Vector4 CREDITS_NAME_PAD = new(24f, 18f, 24f, 8f);   // name box inside the face (l, t, r, b) — top clears the heading's lower half
        public const float CREDITS_NAME_SIZE = 50f;                                 // single names fill the face; the multi-line music list auto-fits down
        public const float CREDITS_NAME_SIZE_MIN = 12f;
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
        public static readonly Vector2 SHOP_CLOSE_POS = new(226f, -45f);        // round X centred on the AWNING's top-right corner (from top-center; the awning is wider than the body — corner measured at art px 2140,195 → ×0.232)
        public const float SHOP_CLOSE_H = 78f;
        public const float SHOP_TOPBAR_DROP = 141f;                             // the shared TopBar pills, moved down into the page
        public static readonly Vector2 SHOP_TITLE_POS = new(0f, 400f);          // SHOP word centered on the awning roof — tune live
        public const float SHOP_TITLE_SIZE = 92f;
        // Scroll viewport: the page body between the pills and the page bottom. Side inset + content
        // padding center the 3-column grids (3 × SHOP_CELL_W + 2 × SHOP_CELL_SPACING) in the body.
        public const float SHOP_SCROLL_TOP = 215f;
        public const float SHOP_SCROLL_BOTTOM = 60f;
        public const float SHOP_SCROLL_SIDE = 62f;
        public const float SHOP_SCROLL_SENSITIVITY = 30f;
        public const int SHOP_CONTENT_PADDING = 8;                              // page edges (RectOffset — int)
        public const int SHOP_CONTENT_BOTTOM_PADDING = 24;
        // Page rhythm (Oscar, 2026-09-26): section — GAP — title/section. Blocks sit SPACING apart
        // (tight: a title sits right on its own section, rows of one section stay close); the air
        // before each title is SECTION_GAP, about double the title→section distance.
        public const float SHOP_SECTION_SPACING = 6f;
        public const float SHOP_SECTION_GAP = 12f;
        // Text: section titles in the HUD palette; names/amounts in the mock's lime accent.
        public const float SHOP_SECTION_TITLE_H = 44f;                          // the band the title's CAPS occupy (≈ cap height + stroke at full size); the word is Capline-centred on it
        public const float SHOP_SECTION_TITLE_SIZE = 56f;                       // was 34 → 44 → 56 — big section titles (Oscar, 2026-09-26)
        public const float SHOP_SECTION_TITLE_SIZE_MIN = 28f;                   // long translations shrink to stay within the section's width
        public const float SHOP_SECTION_TITLE_SIDE_INSET = 10f;                 // title kept this far inside the content edges
        public const float SHOP_SUBTITLE_SIZE = 18f;                            // small brown text (Restore Purchases)
        public const float SHOP_RESTORE_H = 34f;                                // "Restore Purchases" text button under the gem grid
        public static readonly Color SHOP_PURCHASE_BLOCKER = new(0f, 0f, 0f, 0.25f); // input shield while a store purchase is in flight
        public static readonly Color SHOP_ACCENT = new(0.62f, 0.75f, 0.20f);    // lime: cell names, pack amounts, THANK YOU
        // Cells (skins, power-ups, currency packs): an authored box (skin checker / item box, sized by
        // width at native aspect) with an optional lime label line above and a wide green price pill
        // below; the whole cell is the button. Skin rows sit on the 9-sliced cream slab.
        public const float SHOP_CELL_W = 125f;                                  // 3 × W + 2 × spacing ≈ the content width, so grids align with the full-width boxes
        public const float SHOP_CELL_SPACING = 12f;
        public const float SHOP_CELL_LABEL_H = 24f;                             // the name line is centred on the box's VISIBLE top border (the art carries a transparent margin — ShopWidgets.BoxTopMarginFrac; anchored there 2026-09-21, it used to float on the rect top)
        public const float SHOP_CELL_LABEL_SIZE = 17f;
        public const float SHOP_CELL_LABEL_SIZE_MIN = 11f;                      // long localized skin names shrink to this, never overflow
        public const float SHOP_CELL_PILL_OVERLAP = 16f;                        // the pill rides over the box bottom (mock)
        public const float SHOP_ROW_SLAB_PAD = 22f;                             // cells inset inside the slab (was 14 — roomier panels, Oscar 2026-09-26)
        public const float SHOP_CELL_PILL_W = 125f;                             // green wide blank width …
        public const float SHOP_CELL_PILL_H = 46f;                              // … stretched thicker than its native ≈36 (mock pills are chunky)
        public const float SHOP_PILL_ICON_H = 17f;                              // currency icon on a pill, right of the number  // 27 → 17: star/gem cropped 2026-09-17 (now the same visible size)
        public const float SHOP_PILL_ICON_GAP = -3f;                            // number → currency icon, everywhere the icon is used as a "character". Negative: the digits' own right side bearing (~2px) already reads as gap, so the icon tucks into it (artist 2026-09-17)
        public const float SHOP_PILL_ICON_Y_NUDGE = 0f;                         // icon vs the text line (Capline-centred digits since 2026-09-17 — was -2 to chase Center-aligned ones)
        public const float SHOP_WATCH_LABEL_MIN = 8f;                           // WATCH / LOADING... / TOMORROW! shrink floor on the FREE gems pill
        // Chef previews (no plate) are sized by WIDTH — the dogtor art is wider than tall, so a height
        // knob only ever hit the width clamp — and OVERFLOW the box: wider than the checker's visible
        // face, head over its top edge, feet hanging below its bottom where the green pill clips them
        // (a RectMask2D from the box top + HEAD_ROOM down to the pill's face top). Oscar, 2026-09-21.
        public const float SHOP_SKIN_CHEF_W = 132f;                             // > the box rect (125) and well over its visible face (~92)
        public const float SHOP_SKIN_CHEF_BOTTOM = -2f;                         // chef's bottom edge vs the box rect bottom (negative = below; the pill face starts ~12 above it)
        public const float SHOP_SKIN_CHEF_CELL_DROP = 6f;                 // dogtor cells (box + chef + name + pill) sit this much lower in their row
        public const float SHOP_SKIN_CHEF_HEAD_ROOM = 24f;                      // the clip lets the head rise this far over the box rect top (the name line sits there — keep it under the label height)
        public const float SHOP_SKIN_PREVIEW_H = 66f;                           // ingredient previews sit ON the plate
        public const float SHOP_SKIN_PREVIEW_MAX_W = 92f;
        public const float SHOP_SKIN_PREVIEW_Y = -6f;                           // preview center vs box center (resting on the plate)
        public const float SHOP_SKIN_PLATE_W = 106f;                            // the plate under ingredient previews
        public const float SHOP_SKIN_PLATE_Y = -34f;
        public const float SHOP_SKIN_BUN_W = 88f;                               // bun-pair preview: BOTH halves sized by width (equal heights made the squat bottom read smaller)
        public const float SHOP_SKIN_BUN_GAP = -8f;                             // bottom rect top → top rect bottom. Negative: the art's transparent padding (~4 under the top bun + ~6 over the bottom) made +2 read as a ~12px gap; -8 leaves ~2-4 visible. Only the TOP bun moves (the bottom stays seated on the plate)
        public const float SHOP_SKIN_BUN_BOTTOM_Y = -22f;                       // bottom bun center (pair centered in the box, seated on the plate)
        public const float SHOP_ITEM_ICON_H = 86f;                              // power-up / pack icons inside the box
        public const float SHOP_ITEM_CURRENCY_ICON_H = 53f;                     // a bare ui_gem/ui_star inside a box (the FREE gems cell) — 86 × the 2026-09-17 crop factor
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
        public const float SHOP_BANNER_H = 124f;                                // remove-ads offer / THANK YOU box
        public const float SHOP_BANNER_TEXT_SIZE = 26f;
        public static readonly Vector2 SHOP_BANNER_TEXT_INSET = new(24f, 12f);
        // Remove-Ads offer banner (composed from widgets 2026-09-05 — the baked ui_shop_remove_ads
        // mock is retired; texts anchor to the box's LEFT edge, the price pill to its RIGHT edge)
        // One text column, left padding → a gap short of the pill's visible face (derived in
        // ShopSections from the PILL_* knobs); title and tag auto-size to fill its width in every
        // language, the bonus line centres on it (Oscar, 2026-09-26).
        public const float SHOP_BANNER_TEXT_LEFT = 34f;                         // column's left edge vs the box rect (inside the box's visible border)
        public const float SHOP_BANNER_TEXT_PILL_GAP = 12f;                     // column's right edge → the pill's face
        public const float SHOP_BANNER_TITLE_Y = 30f;                           // REMOVE ADS — caps centred here (vs box centre)
        public const float SHOP_BANNER_TITLE_H = 50f;                           // tall enough that only WIDTH ever shrinks it
        public const float SHOP_BANNER_TITLE_SIZE = 44f;                        // cap: English fills the column below this
        public const float SHOP_BANNER_TITLE_SIZE_MIN = 16f;
        public const float SHOP_BANNER_TAG_Y = 5f;                              // REWARD ADS STILL AVAILABLE — as wide as the title
        public const float SHOP_BANNER_TAG_H = 22f;
        public const float SHOP_BANNER_TAG_SIZE = 20f;
        public const float SHOP_BANNER_TAG_SIZE_MIN = 7f;
        public const float SHOP_BANNER_BONUS_Y = -30f;                          // the BIG "+100 ◆" line, centred on the column
        public const float SHOP_BANNER_BONUS_H = 44f;
        public const float SHOP_BANNER_BONUS_SIZE = 30f;
        public const float SHOP_BANNER_BONUS_ICON_H = 24f;                      // gem icon a touch taller than the font  // 38 → 24: ui_gem cropped 2026-09-17
        public const float SHOP_BANNER_PILL_W = 150f;                           // ui_btn_green_big on the banner …
        public const float SHOP_BANNER_PILL_H = 90f;                            // … stretched as tall as the whole left column
        public const float SHOP_BANNER_PILL_X = -88f;
        public const float SHOP_BANNER_PILL_TEXT = 34f;                         // the price fills the pill (auto-fit)
        public const float SHOP_BANNER_PILL_TEXT_MIN = 14f;                     // … down to this for long store strings
        public const float SHOP_BANNER_DOT_H = 44f;                             // red ONE TIME BUY tag
        public static readonly Vector2 SHOP_BANNER_DOT_POS = new(-4f, -4f);    // centered on the PILL's top-right corner
        public const float SHOP_BANNER_DOT_TEXT = 9f;
        public const float SHOP_BUNDLE_H = 124f;
        public const float SHOP_BUNDLE_ICON_H = 92f;                            // the condiment tray
        public const float SHOP_BUNDLE_ICON_X = 104f;                           // tray center from the left edge
        public const float SHOP_BUNDLE_ICON_Y = 6f;
        public static readonly Vector2 SHOP_BUNDLE_QTY_POS = new(186f, 0f);     // "xN" right of the tray (from the left edge)
        public static readonly Vector2 SHOP_BUNDLE_NAME_POS = new(118f, -44f);  // "PRO COOK PACK" under the tray (center, from the left edge)
        public static readonly Vector2 SHOP_BUNDLE_NAME_RECT = new(240f, 28f);
        public const float SHOP_BUNDLE_NAME_SIZE = 22f;
        public const float SHOP_BUNDLE_PILL_W = 150f;                           // ui_btn_green_big sized by width (≈ 82 tall)
        public const float SHOP_BUNDLE_PILL_X = -88f;
        public const float SHOP_BUNDLE_PILL_ICON_H = 26f;                       // star on the PRO COOK PACK pill (cropped art; bigger, artist 2026-09-17)
        public const float SHOP_BUNDLE_PILL_TEXT_MAX = 30f;                     // the big pill's cap-fit ceiling (~54) is too much for a price — capped
        // Confirm dialog (gem spends only): the authored card (inner text box baked in its top half)
        // with the offer lines and the BUY / CANCEL blanks. Widths are the sprites' canvases (they
        // include the drop shadows): 530 puts the card's face at ~400 px.
        public const float SHOP_CONFIRM_CARD_W = 530f;
        public const float SHOP_CONFIRM_LINE1_Y = 74f;                          // "Buy N ★" (inner box center ≈ +53; lines spread for the 38px text)
        public const float SHOP_CONFIRM_LINE2_Y = 32f;                          // "for N ◆"
        public static readonly Vector2 SHOP_CONFIRM_LINE_RECT = new(340f, 52f);
        public const float SHOP_CONFIRM_TEXT_SIZE = 38f;                        // 28 → 38 (artist 2026-09-17)
        public const float SHOP_CONFIRM_ICON_H = 27f;                         // ≈ the 38px text's cap height + a touch (cropped icon)
        public const float SHOP_CONFIRM_BTN_X = 100f;                           // pills flank the center
        public const float SHOP_CONFIRM_BTN_Y = -58f;
        public const float SHOP_CONFIRM_BTN_W = 195f;
        public const float SHOP_CONFIRM_BTN_TEXT = 24f;
        #endregion
    }
}
