# Dogtor Burguer

2D mobile arcade game in Unity 6. Player controls a chef catching falling ingredients between columns to build burgers. Match-3 style mechanics with burger challenge objectives.

## Tech Stack
- Unity 6, URP, New Input System
- DOTween for animations
- TextMeshPro for text rendering
- System.Random via shared `Rng` utility (NOT UnityEngine.Random)
- C# 10+, namespace: `DogtorBurguer`

## Code Conventions (project overrides / additions)

These extend or override the global rules in `~/.claude/CLAUDE.md`.

- **No nested types.** Promote any struct/class/enum nested inside a parent type to its own top-level file in the same folder. Overrides the global "nested types are fine" allowance — this project prefers them flat for navigability and avoiding awkward `Parent.Nested` qualified access at consumer sites.

## Project Structure
```
Assets/_Project/Scripts/
  Chef/          ChefController, PlateManager
  Core/          GameManager, GameState, Constants, GameplayConfig, MonetizationConfig,
                 UIStyles, AnimConfig, DifficultyManager, SaveDataManager, ControlMode,
                 FeedbackManager, Rng, SceneLoader, CameraFit, Singleton<T> (manager base)
  Grid/          GridManager, Column, MatchDetector, MatchResult, BurgerAnimator, BurgerData
  Ingredients/   Ingredient, IngredientState, IngredientSpawner, SpawnerState,
                 IngredientType, WavePreviewManager, WaveComposer, WaveSlot, IngredientBag
  Input/         TouchInputHandler
  Scoring/       Scoring (points/tiers), BurgerTier, BurgerNamer
  Skins/         Skin (ScriptableObject), SkinSlot, UnlockMethod, SkinMap, Theme (static accessor)
  Shop/          ShopScreen (full-screen overlay), ShopSections, ShopWidgets, ShopSkinCell,
                 ShopRowScroll (nested h-scroll), ShopCell (cell parts), ShopService (purchase
                 rules), ShopCatalog
  UI/            MainMenuUI, GameHUD, TopBar (shared status bar), StatCard (shared Level/Score
                 card), GameOverPanel, SettingsPanel, HowToPlayPanel, CreditsPanel (+ CreditsEntry),
                 ModalPanel (shared Settings/Credits chrome: panel art + title + X + pop-in),
                 BurgerChallenge, BurgerChallengeView, BurgerPopup, FloatingText, ScorePopup,
                 Background, OrderType, NumberFormat, UIFactory
    Factory/     SpriteFactory (cached procedural sprites), WorldTextFactory (world-space TMP)
  Audio/         AudioManager, MusicManager, MusicCategory
  Monetization/  AdManager (facade), MockAdProvider, LevelPlayAdProvider, IapManager (facade),
                 MockIapProvider, UnityIapProvider, IapResult, GemProduct, StarProduct,
                 ConsumablePack, BurgerFairy, BurgerFairySpawner
    Abstractions/ IAdProvider (the ad-network contract), IIapProvider (the store contract)
  Consumables/   ConsumableType, ConsumableEffect (+ Ketchup/Mustard/Skewer), ConsumableEffects,
                 ConsumableFaller, ConsumableVfx (use effects), ConsumableInventory,
                 ConsumableInventoryView, ConsumableSlotWidget, ConsumableDragController,
                 FairyPayload, FairyPayloadKind, RewardArt, SpriteFit
```

## Singletons
These classes use the singleton pattern. Initialization order matters:
1. **SaveDataManager** -- player data persistence (PlayerPrefs)
2. **MusicManager** -- background music, DontDestroyOnLoad
3. **AdManager** -- ad integration (LevelPlay on device, mock in the editor)
4. **IapManager** -- in-app purchases (Unity IAP when the package is installed, mock otherwise), DontDestroyOnLoad
5. **GameManager** -- central game state, creates missing managers via EnsureComponent
6. **GridManager** -- column/grid state
7. **AudioManager** -- procedural SFX generation
8. **BurgerChallenge** -- challenge UI and tracking
9. **ConsumableInventory** -- gameplay facade over the persistent consumable stock (SaveDataManager); created by GameManager
10. **ConsumableInventoryView** -- world-space inventory slot icons
11. **ConsumableDragController** -- the drag-to-column carry interaction
12. **PlateManager** -- the four decorative under-column plates; slides two to swap on the chef flip

## Event System
Key events for cross-system communication:
- `GameManager.OnStateChanged(GameState)` -- game flow changes
- `GameManager.OnScoreChanged(int)` -- score updates
- `GridManager.OnGameOver` -- grid overflow
- `GridManager.OnMatchEliminated`, `OnBurgerCompleted` -- gameplay events
- `GridManager.OnMatchEffect`, `OnBurgerEffect` -- audio/visual triggers
- `GridManager.OnBurgerWithIngredients(List)` -- challenge matching
- `GridManager.OnIngredientPlaced` -- post-landing
- `DifficultyManager.OnLevelChanged(int)` -- difficulty progression
- `SaveDataManager.OnGemsChanged(int)`, `OnStarsChanged(int)` -- currency updates
- `SaveDataManager.OnConsumablesChanged` -- persistent consumable stock changed
- `ConsumableInventory.OnChanged` -- consumable slots changed (forwards the SaveDataManager event)

## Core Systems

### Wave-Based Spawning (IngredientSpawner)
- A wave = 2 ingredients (3 on a triple roll; triple chance per `TRIPLE_CHANCE_BY_LEVEL` — see Difficulty), each in a distinct column.
- **Standing preview queue**: a target number of upcoming pieces (= the next wave's size) is always reserved and shown as blinking ghosts, one per column, each on an
  **arrow back-picture** (yellow = regular, orange = bottom bun, red = top bun;
  `PREVIEW_ARROW_HEIGHT`; the ghost row sits `Constants.PREVIEW_Y_OFFSET` lower to fit). Topped up every frame (`TopUpPreviews`) and seeded at `StartSpawning` (hidden through the initial delay). The reserved previews **are** the next wave — when the current wave lands, they become the real pieces, dropping in their columns.
- **Next-wave trigger**: fires only when the ORIGINAL auto-spawned wave lands. Tapped previews are "fire and forget" (not counted toward wave completion), so a desired preview can't be frozen by tapping the others — a held preview force-drops within ~one fall.
- **Tap a preview** to spawn that ingredient immediately; only revealed (visible) previews are tappable.
- **Column choice is unbiased** (random, one preview per column, height/stack ignored). A per-preview clearance (`AnimConfig.PREVIEW_SPAWN_CLEARANCE`) delays only the ghost's visual reveal so it never overlaps a falling sprite — it does NOT influence which column is chosen (`ColumnsWithPieceInPreviewZone` + `WavePreviewManager.RevealCleared`).
- **Composition**: each slot is either a bun or a regular ingredient. Regulars come from
  `IngredientBag` (shuffle-bag, even spread); buns are a decoupled grid-aware economy (see below).
  `WaveComposer.RollSlot` decides type, `RollWaveSize` decides 2-vs-3; `IngredientSpawner` owns
  column selection (the queue).

### Ingredient distribution & Bun Economy (WaveComposer / IngredientBag)
Buns are **decoupled** from regular ingredients (own chances) and from level/type count.
- **Regular ingredients** (`IngredientBag`): a shuffle-bag of `1 of each active type +
  BAG_RANDOM_EXTRAS (3)` random extras, drawn without replacement, rebuilt from the current
  active count when empty or when the count changes. Guarantees no droughts/streaks, no weights.
- **Bottom bun**: flat `BOTTOM_BUN_CHANCE` (0.12) per slot — the "start a burger" resource.
  Surplus bottoms cancel each other on the grid, so it can flow freely. A drought guard forces
  a bottom after `BUN_DROUGHT_LIMIT` (15) bun-less pieces.
- **Top bun**: only when ≥1 open bottom exists on the grid (a lone top self-destructs). Chance
  `min(TOP_BUN_BASE_CHANCE + TOP_BUN_CHANCE_PER_EXTRA_BOTTOM·(open−1), TOP_BUN_CHANCE_CAP)`
  = 8% at one open bottom, +4% each additional, cap 40%. So it crosses the 12% bottom rate at
  2 open bottoms → the board self-balances around ~2 unclosed bottoms instead of accumulating.
- **Mechanic** (in `GridManager`, unchanged): a burger completes when a `BunTop` lands above a
  `BunBottom` in its column; a lone top self-destructs ("Too bad!"); two adjacent bottoms cancel.
- Per-slot order: drought-forced bottom → flat bottom → scaling top (if eligible) → bag draw.
  The bun roll reads grid state when the preview is *reserved* (a wave ahead), same as before.

### Controls (TouchInputHandler)
Two modes, configurable in Settings (saved via PlayerPrefs):
- **Drag**: Swipe = move chef, **Tap the cook = swap plates**, Tap falling = fast-drop, Tap preview = spawn
- **Tap**: Tap the cook = swap, **Tap a side (below the grid floor) = move**, Swipe = move, Tap falling = fast-drop, Tap preview = spawn

**Chef tap-control is bounded** so taps up in the playfield (e.g. a near-miss reaching for a
falling piece) never move or swap the cook (`ProcessInput`):
- **Swap (flip)** = tap within the cook's circle (`BubbleRadius × CHEF_TAP_RADIUS_MULT`) — both modes.
- **Move (Tap mode only)** = tap a side with `worldPos.y < GRID_ORIGIN_Y + CHEF_MOVE_ZONE_TOP_OFFSET`
  (below / into the bottom of the playfield; the offset gives the side-taps vertical room).
- **Swipe** moves in both modes (gesture, checked before any tap logic).

**Consumable carry**: a press that *starts on an inventory slot* becomes a drag-to-column carry
(see Core Systems → Consumables) — `TouchInputHandler` hands the gesture to
`ConsumableDragController` and suppresses chef logic for its duration. **Editor-only debug**: keys
**1/2/3** grant Ketchup/Mustard/Skewer to the inventory, **4** grants 500 stars, **F** spawns a
fairy, **R** (while the shop is open) wipes shop purchases/equips/remove-ads (`#if UNITY_EDITOR`).

**Input-area debug gizmos** (editor-only, `#if UNITY_EDITOR`): each clickable zone is drawn by the
component owning its hit-test, one color per interaction (`GizmoStyles`): falling fast-drop (green,
`IngredientSpawner`), preview tap (yellow, `WavePreviewManager`), chef flip circle (magenta) + move
sides (cyan, `TouchInputHandler` — mode-aware, Tap mode only), fairy tap (orange, `BurgerFairy` —
play-mode only, runtime-spawned). Toggle per-script via Unity's Gizmos menu.

The Settings panel also has a **Start Level** stepper (`[−] Lv N [+]`) → persists
`SaveDataManager.StartingLevel`; clamped 1..`SETTINGS_LEVEL_CAP`. See Difficulty. **Testing-only,
off by default**: the `MainMenuUI` inspector bool **Show Level Stepper** (menu scene) adds it
below the panel art in flat placeholder widgets — the artist's Settings has no level selector.
Menu panel only (the in-game panel never shows it; the value applies to the next run anyway).
**Settings opens in-game too** (top-bar gear, `GameHUD.OnConfigClicked`): same pause pattern as
the shop — pauses a running game, panel on its own canvas (`SETTINGS_CANVAS_SORT` 110, above
game-over, below shop), resumes via `SettingsPanel.OnClosed`. Sound/control-mode apply live
mid-run; Start Level applies next run. The in-game variant (`Initialize(canvas, showRunButtons:
true)`) fills the third row with a full-width **Quit to Menu** (the Restart half was dropped
2026-09-05 — game over already offers Retry); quitting keeps live-earned order stars but
forfeits the end-of-run score payout.

### Difficulty (DifficultyManager)
- 20 levels scaling fall speed, active ingredient (type) count, and triple-wave chance.
- **Table-driven** (not formula-derived): three per-level arrays in `GameplayConfig`, each
  length `MAX_LEVEL`, indexed `level - 1`: `FALL_STEP_BY_LEVEL`, `INGREDIENT_COUNT_BY_LEVEL`,
  `TRIPLE_CHANCE_BY_LEVEL`. `ApplyDifficulty` indexes them directly — edit one cell to retune
  one level. An `Awake` assert enforces all tables (+ `LEVEL_THRESHOLDS`) stay `MAX_LEVEL` long.
- **Curve shape** (front-loaded): L1 fall 0.45s / 4 types; L10 ≈ 0.205s (the old L15 speed);
  L20 0.10s / 8 types. Triple waves start L6, ramp to 0.50 at L20.
- **Pacing**: `LEVEL_THRESHOLDS` (ingredients placed per level) — reaches L20 at 394 placements
  (longer early levels than before, much shorter late ones).
- **Killer level (21)** — Tetris-style kill screen above the curve, NOT in the tables: always-triple
  waves at `MIN_FALL_STEP_DURATION` (0.06s, the absolute fall floor) with 8 types. Entered by
  sustained survival past `KILLER_LEVEL_THRESHOLD` (434 placements); selectable from Settings only
  while `SETTINGS_LEVEL_CAP == KILLER_LEVEL` (testing — see Pending Manual Steps).
- **Starting level**: `SaveDataManager.StartingLevel` (persisted, set via the Settings stepper)
  seeds `_currentLevel`; `DifficultyManager` runs at `[DefaultExecutionOrder(-100)]` so the seed
  is applied before the HUD/spawner init. Initial level is pull-state (no init-time `OnLevelChanged`).
- HUD shows "Level X" (full word, distinguishes from challenge star)

### Game Modes (2026-09-05)
`GameMode` (Classic/Relax), persisted in SaveDataManager, toggled in the **menu** Settings only
(third row, "Mode: Classic/Relax"; applies to the NEXT run — managers read it once at scene load;
the in-game panel's third row stays Quit to Menu). **Relax** = the identical speed/type curve but
every threshold × `RELAX_LENGTH_SCALE` (3 — runs ~3× longer, kill screen included, no level cap),
ALL in-run star income halved (`RELAX_STAR_SCALE`, applied in `GameManager.AwardStars` — the one
faucet; shop purchases unaffected), **no high-score writes** (easier long runs would inflate the
Classic trophy), and the mode always labelled on a red tab straddling the
Special Order card's bottom edge — CLASSIC or RELAX, the Level/Score tab recipe
(`SPECIAL_MODE_TAB_*`, built in `BurgerChallengeView.BuildPanel`).

### Burger Challenge (BurgerChallenge) — "Special Orders"
- Two order types, randomly chosen:
  - **Size**: "N+ Ingredients" — any burger with at least N ingredients matches
  - **Contains**: the burger must include the required ingredients — extras OK. (Exact-match was
    tried and reverted the same day, 2026-09-04.) The card communicates "must include" without
    words: a **ghosted "?" mystery layer** (`SPECIAL_GHOST_ALPHA`) sits between the required
    ingredients and the top bun — the same silhouette Size orders use for "any ingredients", so
    it reads as "these, plus whatever else". FlashOrder restores per-image REST colors so the
    ghost stays translucent after the gold flash.
- **Screen-space UGUI panel** (top-right, `BurgerChallengeView`): authored card + the SPECIAL ORDER
  banner (blank art + TMP word) + the **required-burger stack** (bun → "+N" mystery silhouette /
  required ingredients → bun, on a `Theme.Plate`) + a **multiplier badge** (`GetGlobalMultiplier`).
  No requirement text (the burger conveys the order; matches the art).
- **Mult meter**: a vertical capsule (`ui_mult_meter_back/fill/front` — brown well + green fill + frame,
  3 layers stacked at one rect) whose green fill is a UGUI `Image.Filled` (vertical, bottom-origin). Fill =
  `BurgerChallenge.ChallengeFill` (`_challengeProgress / ProgressTarget`, progress to the next challenge
  level). Climbs on each Special Order match → tops out on the leveling match → the post-level new order
  drains it to empty. Animated via `DOFillAmount` (`AnimConfig.MULT_METER_FILL_DURATION`). A **child of the
  card**, built **before** the mult badge so the badge renders on top (meter sits under it, sharing its x);
  taller than the card so it overflows a touch. The green capsule's rect is extended downward
  (`MULT_METER_FILL_BOTTOM_EXTEND`) to seat the fill on the well bottom. Layout: `UIStyles.MULT_METER_*`.
- `BurgerChallenge` is logic + read-only state only; the view owns all layout (`UIStyles.SPECIAL_*`).
- On match: the burger flashes gold; popup shows "Order Complete!" instead of the generated name (via
  GridManager `IsOrderMatch`). Level-up flashes + punches the card, then rolls the next order.
- 3x challenge multiplier on match; global multiplier: `1 + (level - 1) * 5`
- **World popups ride glow plates** (2026-09-04 kit, set 1; `WorldTextFactory.AttachPlate`,
  heights `UIStyles.PLATE_*`): burger name + points on the wide green ellipse
  (`ui_popup_plate_wide`), score "N!" and fast-drops on the green round (`ui_popup_plate`),
  multiplier "xN" and star awards on the yellow (`ui_popup_plate_mult`); "Too bad!" stays bare.
- Level up requires `level + 1` matches (`Level` still exposed; the in-panel ★ readout was dropped)
- Each match also **awards stars** (the currency faucet — see Monetization & Currencies)

### Scoring
- Match: 10 pts per matched pair
- Burger bonuses: 5 (poor) to 500 (9+ ingredients)
- Per ingredient: 10 pts
- Challenge multipliers stack with global multiplier

### Grid
- 4 columns, 13 max rows
- Cell: **1.234w** x 0.4h visual height (60% overlap). The width equals the play-mat's painted
  lane pitch at its current scale (measured 507.7px × `GRID_CELLS_WIDTH`/2604px) so every column
  sits exactly on its painted lane — **the mat leads, the grid follows**; re-derive `CELL_WIDTH`
  if `GRID_CELLS_WIDTH` changes. `GRID_CELLS_X_NUDGE` (0.111) is the measured correction for the
  mat texture's asymmetric transparent padding.
- Grid origin: (-1.851, -4.2) — always `-(COLUMN_COUNT-1)*CELL_WIDTH/2` to stay centered on x=0
- Pieces are wider than the lanes (buns 1.38): adjacent stacks slightly overlap — accepted look
- Chef has 3 positions (between the 4 columns)

### Camera & UI scaling (CameraFit) — design for WIDTH
The game is framed by **width** so it fits any phone aspect. `Core/CameraFit` self-attaches to
`Camera.main` (no scene wiring) and sets `orthographicSize = max(DESIGN_ORTHO_SIZE, (PLAY_AREA_WIDTH
/2)/aspect)` — the 4 columns always fill the screen width and never clip; wide screens fall back to
the design height. The HUD canvas **matches width** (`UIStyles.MATCH_WIDTH_OR_HEIGHT = 0`), so the
UI scales by the same rule and stays locked to the playfield. No-op at the reference 9:16.
- **All in-game HUD is screen-space UGUI** (top bar, Level/Score cards, consumable row, Special
  Order panel) — so it scales with the camera. The consumable **carry/drop** and gameplay sprites
  stay world-space; the drag controller hit-tests the screen-space slot, then works in world space.
- **Aspect-preserving sizing**: size authored sprites by a target **height**; width =
  `height × sprite.aspect`. Forcing a square or a fixed `Vector2` distorts non-square art (caused the
  "trophy/star look weird" bug). Baked dotted cards must display at their **native aspect** or the
  halftone dots smear (the Special Order card is a deliberate exception — stretched taller).
- Tunables: `Constants.PLAY_AREA_WIDTH` / `DESIGN_ORTHO_SIZE`; all HUD layout in `UIStyles`.

### Render Order Convention
- The in-game HUD canvas is **Screen Space - Camera** (sorting order 50, via
  `UIFactory.CreateCanvas(..., Camera.main)`) so world sprites with a higher sorting order render
  in front of it — fairies (100), floating text/popups (100–110), and the screen flash (200)
  intentionally fly over the HUD. Don't switch it back to Overlay.
- ChallengeCanvas (60) and ConsumableCanvas (90) are also **Camera** mode (since 2026-09-04),
  so fairies (100) and popups fly OVER the Special Order panel and the consumable slots too;
  `ConsumableSlotWidget.Contains` passes the canvas camera to `RectangleContainsScreenPoint`
  accordingly (null would hit-test wrong in camera mode). The remaining canvases (Menu 10,
  Settings, Shop, GameOver 100) are Screen Space **Overlay** and always cover world sprites —
  intentional (GameOver correctly hides fairies).
- World sprite sorting orders are centralized in `Constants.SORT_*` (background −100 …
  screen flash 200).

### Audio
- **AudioManager**: All SFX procedurally generated (sin waves, envelopes, harmonics). No audio asset
  files. Each sound has an optional `_*Override` clip slot for authored audio. Consumable hooks:
  `PlayConsumableCollect` / `PlayConsumableUse(type)` / `PlayConsumableFizzle` (placeholder tones).
- **MusicManager**: Loads tracks from Resources/Music/. Random selection per scene

### Monetization & Currencies
- **Two currencies**, both persisted in SaveDataManager and spendable in the Shop:
  - **Gems** (hard/premium): rare — Burger Fairy drops (~40% of fairies), rewarded ads, IAP packs.
  - **Stars** (soft/free): earned by playing — **per completed Special Order**
    (`STARS_PER_ORDER_BASE + PER_LEVEL·(challengeLevel−1)`, awarded live with a gold "N!"
    popup), an **end-of-run score payout** (1★ per `STAR_SCORE_DIVISOR` score; continues pay
    only the un-paid delta), and **star fairies** (`STAR_PACK_VALUE` 25). `GameManager.AwardStars`
    grants + tracks `StarsEarnedThisRun` (shown on the game-over panel). Also from gem→star shop
    packs; editor debug key **4** grants 500.
- One-directional exchange: gems buy stars, never the reverse (standard freemium convention).
- **Economy pass (2026-09-05)** — full money/time study in `Docs/economy-2026-09-05.md`. Retunes:
  consumables 100★/270★/700★ (were 150/400/1000 — hoard prevention), gem-cheap skins 30◆→60◆ and
  chef European 50◆→80◆ (the premium tier undercut the 400★ star tier), the FREE gems rung capped
  at `GEM_AD_DAILY_CAP` (3/day — uncapped, 4 views ≈ the 0.99 gem pack).
- Continue after game over: 50 gems or watch ad
- Interstitial ads every 3 games, shown only on the game-over **Retry** button
  (`AdManager.MaybeShowInterstitial`; the in-game settings Restart was removed 2026-09-05, so
  Retry is the one interstitial spot). Menu Play and Quit-to-Menu are ad-free —
  yes, quit→menu→Play can dodge a restart ad; **deliberate decision (2026-07-05)**: the dodge
  costs more friction than it saves and gating Play felt hostile. Don't "fix" it; if ad pacing
  ever needs rework, use a time cooldown at break points instead. **Suppressed once Remove Ads
  is bought** (`SaveDataManager.AdsRemoved`, in `ShouldShowInterstitial`). Rewarded ads stay.
- **Burger Fairy** drops during gameplay (`FAIRY_SPAWN_CHANCE` 0.20 / 10s) carry a consumable
  (~60%) or currency (~40%, split gems/stars by `FAIRY_STAR_SHARE` → ~20% each) — see Core
  Systems → Consumables. Gem fairies award `GEM_PACK_VALUE` (5), star fairies `STAR_PACK_VALUE`
  (25, routed via `GameManager.AwardStars` so it counts toward the run total).
- **Real ad SDK (LevelPlay) — code landed 2026-08-30, Android credentials wired 2026-08-31**:
  `com.unity.services.levelplay` 9.5.1 is in the package manifest (compiles clean on
  6000.3.23f1) and `LevelPlayAdProvider` implements `IAdProvider` over the 9.x ad-unit API.
  `AdManager.Awake` auto-selects it on device builds when `MonetizationConfig.LEVELPLAY_APP_KEY`
  + ad-unit IDs are filled in (editor and unconfigured builds keep `MockAdProvider`). The
  **Android** App Key + Interstitial + Rewarded IDs are filled in; **iOS slots are still empty**
  (separate app on the dashboard). The dashboard is the ironSource platform
  (https://platform.ironsrc.com — *not* Unity Cloud): Apps page = App Key, Ad Units page =
  ad-unit IDs. Package ID is `com.proximacentaury.dogtorburguer` (all platforms; company
  `ProximaCentaury`; permanent once uploaded to Play). Next: **device test** with
  `LEVELPLAY_TEST_SUITE = true` on an Android build. **Consent: no prompt for v1** (decision 2026-09-01) —
  `MonetizationConfig.ADS_PERSONALIZED = false` makes `LevelPlayAdProvider` call
  `LevelPlayPrivacySettings.SetGDPRConsent(false)` / `SetCCPA(true)` / `SetCOPPA(false)` before
  `Init`, so every user gets non-personalized ads and iOS never shows ATT (no IDFA). Turning
  personalization on later requires an EEA/UK consent prompt + the ATT prompt first (see
  `Docs/pre-launch-checklist.md`). Privacy policy: `Docs/privacy-policy.md`.
- **Ad architecture** (production-shaped, 2026-07-05): `AdManager` is a facade owning one
  `IAdProvider` (contract in `Monetization/Abstractions/`) plus the ad policy (cadence,
  remove-ads suppression). The provider models the real SDK lifecycle: async init, **preload**
  (`IsInterstitialReady`/`IsRewardedReady` are real load state), auto-reload after show, retry
  on failure, **reward only from the reward callback**, and timeScale save/restore (an ad over
  a paused game hands the pause back). `MockAdProvider` simulates it all, including a
  `NO_FILL_CHANCE` so not-ready UI paths get exercised — ad buttons (game-over continue, shop
  FREE) disable + relabel while no ad is loaded. **Swapping in the real SDK = one new provider
  class + one line in `AdManager.Awake`** (SDK choice leaning Unity LevelPlay — see checklist).
- **IAP (Unity In-App Purchasing 5.4.2, code landed 2026-09-01, migrated to IAP 5 on 2026-09-02 — IAP 4 left support June 2026)**: `IapManager` is the
  store facade (twin of `AdManager`) owning one `IIapProvider` (`Monetization/Abstractions/`):
  `UnityIapProvider` when the purchasing package is installed (Unity defines
  `ENABLE_CLOUD_SERVICES_PURCHASING` then; the provider file compiles to nothing without it),
  `MockIapProvider` otherwise (editor /
  package-less checkouts — purchases succeed a frame later, free). Catalog =
  `MonetizationConfig.GEM_PRODUCTS` (consumables, store ids `gems_100` … `gems_2600`) +
  `REMOVE_ADS_STORE_ID` (`remove_ads`, non-consumable) — **create the same ids in the Play
  Console** (`Docs/play-store-listing.md`). **Grant only from the store callback**
  (IAP 5 flow: `OnPurchasePending` → grant every cart item → `ConfirmPurchase`; the store replays
  unconfirmed orders and owned non-consumables at init/`FetchPurchases`/Restore through the same
  event): `IapManager.Grant` → `ShopService.GrantGemPack` / `GrantRemoveAds` (idempotent);
  `IapManager.OnGranted` re-renders an open shop. The shop shows `IapManager.PriceLabel` (the store's localized string once known,
  else the config placeholder minus "$"), the App Store-mandatory **Restore Purchases** text
  button sits under the gem grid. Still pending: local receipt validation (Unity's obfuscated
  tangle classes are editor-generated — `UnityIapProvider.IsReceiptValid` passes everything
  until then) and the on-device purchase test on the internal track.

### Shop (`Scripts/Shop/` — authored page, 2026-09-01)
Built to the artist's mock (`Look Reference/Shop_example_1..3.png` + `Shop buy confirm.png`) with
the **2026-09-01 kit's `Assets/Shop` pieces** (`scratchpad/gen_shop_art.ps1`): the full-canvas
**page** `ui_shop_page` (striped awning with SHOP baked in + dotted cream body, shown at
`REFERENCE_RESOLUTION` like the other screens) over the dimmed game/menu, our round X on the
awning's corner, the shared **TopBar pills inside the page** (dropped by `SHOP_TOPBAR_DROP`, centered by `SHOP_TOPBAR_X_NUDGE`), and
one vertically scrolling body inset to the page (`SHOP_SCROLL_*`). Own canvas (`SHOP_CANVAS_SORT` 120, above
everything). Opened via `ShopScreen.Open()` (menu Shop button) or `ShopScreen.OpenInGame()`
(the consumable slots' green plus box — the top-bar shop button became the "?" help button
2026-09-05) — the in-game path
**pauses** the run (`GameManager.PauseGame`) and resumes on close; all shop tweens run unscaled.
Rebuilt each open, destroyed on close (no stale state).
- **Sections, top → bottom**: support banner (**composed from shop widgets since 2026-09-05**:
  lime REMOVE ADS + "REWARD ADS STILL AVAILABLE" + a "+100 ◆" line (the plus renders through the
  LiberationSans sticker material — Panton slivers it) + the green price pill (store price via
  `IapManager.PriceLabel`, auto-fit big, red ONE TIME BUY tag on its corner — the kit has no
  blank red dot, the close button's X is baked, so `ui_consumable_num` stands in); the
  baked-values mock `ui_shop_remove_ads` is retired/unused; once bought it becomes
  the mock's **THANK YOU FOR SUPPORTING US!** box, knobs `SHOP_BANNER_*`) → DOGTOR SKINS (one h-scroll row on the
  9-sliced cream slab `ui_shop_row_slab`) → INGREDIENT SKINS (**one sub-row per ingredient type**, unlabelled
  since 2026-09-05 — via `ShopCatalog.IngredientSkinRows()`; `ShopRowScroll` rows,
  vertical drags route to the page scroll) → POWER-UPS (a **3-column grid**: one row per
  `CONSUMABLE_PACKS` rung × one column per consumable; each cell = owned-count badge
  (`ui_consumable_num`), the icon — single `ui_consumable_*` for x1, `ui_shop_trio_*` from x3 —
  "xN", star pill) + the **PRO COOK PACK** bundle row (`ui_shop_condiment_pack` tray, the big
  green `ui_btn_green_big` pill; `MonetizationConfig.PRO_COOK_PACK` — N of *each* type for one
  star price, `ShopService.TryBuyProCookPack`) → STARS (grid, `ui_pack_stars_1..3` by ladder
  position; gem-priced, **confirm dialog** — the only confirm; soft spends and equips are
  instant) → GEMS (grid; the free rewarded-ad cell first — its pill is the STANDARD green
  one with a centered WATCH label (the authored `ui_shop_watch` never matched the pill shape
  however sized — unused since 2026-09-05); the label tracks ad availability and the **daily
  cap** (`GEM_AD_DAILY_CAP` 3/day — reads TOMORROW! once spent; date + count persist in
  SaveDataManager), then IAP packs with `ui_pack_gems_1..4` by position —
  `Gem_Pack_5` is in the kit for a 5th tier, not imported; the gold MOST POPULAR / BEST VALUE
  badges were dropped 2026-09-05 — clutter; the strings stay on the products).
- **Cells** (`ShopWidgets.CreateCell(…, boxArt, …)` → `ShopCell`): an authored box sized by width
  at native aspect — `ui_shop_item_box` (packs/power-ups; also the 9-sliced banner/bundle box,
  border 200 — ⚠️ **9-slice gotcha**: UGUI draws sprite borders at their *native* pixel size, so
  a 2000px art's borders dwarf a 400px rect and the center collapses to nothing; `ShopWidgets.
  SetupSliced` sets `Image.pixelsPerUnitMultiplier = sprite height / rect height` so the edges
  render true and only the flat middle stretches — do the same for any future 9-slice) or the
  skin checkers `ui_shop_skin_box` / `ui_shop_skin_equipped` (swapped on the
  equipped state) — with an optional **lime** label line (`SHOP_ACCENT`, `StyleAccent`) above and
  a wide **green pill** below; the whole cell is one button. Pill faces are `CreateIconLine`s — a
  HUD-palette number followed by the currency icon, layout-centered as one — the same line the
  confirm dialog uses ("Buy 200 ★ / for 40 ◆" on the authored card `ui_shop_confirm_card`, whose
  canvas width `SHOP_CONFIRM_CARD_W` includes its shadow; BUY/CANCEL = `ui_btn_confirm_*`).
  **Skin cells**: 3 states — EQUIPPED (green checker) / EQUIP (tap equips instantly) / price +
  icon (tap buys **and auto-equips**; insufficient funds shakes the cell). The shop *is* the
  wardrobe — no separate skins screen.
- **One derived stand-in remains**: the wide green cell pill `ui_btn_green_wide` — the kit has no
  wide green blank (the button sheet has words baked in), so it's the wide blue blank hue-shifted
  (`scratchpad/build_shop_art.py`, outline + highlight preserved). Ask the artist for a wide green
  blank and it's a file swap.
- Money price labels show digits + separators ONLY (`ShopWidgets.MoneyLabel` strips currency
  symbols from config placeholders AND store-localized strings — showing them via the
  LiberationSans fallback was tried 2026-09-05 and rejected as off-style; the store purchase
  sheet has the real symbol). The hand-authored `LiberationSans SDF - Sticker.mat` (TMP
  Fonts & Materials, HUD stroke/shadow, font-name prefix required for TMP's rich-text lookup)
  still renders the banner's "+". Revisit both at the font swap.
- **Layer split**: `ShopScreen` (frame/orchestration + confirm dialog + pills),
  `ShopSections` (page composition), `ShopWidgets` (low-level UGUI builders), `ShopCell` (the
  parts of one cell), `ShopSkinCell` (skin cell states), `ShopService` (atomic purchase rules,
  UI-free), `ShopCatalog` (groups skins; a slot appears only once it has a non-default skin).
  **2026-09-05 polish pass** (to Oscar's screenshot notes): cells 125 wide / 12 apart so the
  3-col grids fill the same lateral span as the full-width boxes; skin-name labels half-overlap
  the box top; pills stretched thick (`SHOP_CELL_PILL_H` 46) riding over the box bottom
  (`SHOP_CELL_PILL_OVERLAP`), faces centered; ingredient previews SIT on a bigger plate; bun
  pairs sized by width (equal heights made the squat bottom bun read smaller); chef previews
  keep their own height (`SHOP_SKIN_CHEF_*`); bigger pack/power-up icons + qty text; watch pill replaced
  by the standard green pill.
  Layout knobs: `UIStyles.SHOP_*` (eyeballed off the mocks — tune live); prices:
  `MonetizationConfig` (packs/ladders/bundle) + per-skin `_starCost` on the Skin asset.

### Consumables ("Burger Fairy" deliveries)
Per-run consumable items delivered by fairies; drag onto a column to use. Design doc:
`Docs/consumables-design.md`. **Feature-complete.**
- **Delivery**: `BurgerFairy` (replaced the old GemPack) flies across the screen carrying a
  **payload** — a consumable (`FAIRY_CONSUMABLE_CHANCE` 0.60; which one per
  `CONSUMABLE_SPAWN_WEIGHTS`, even thirds), gems, or stars (currency split:
  `FAIRY_STAR_SHARE`). `BurgerFairySpawner` rolls it. Tap to collect (routed **first** in
  `ProcessInput`, above preview/falling, since it's on top of the playfield). Gems → `AddGems`;
  stars → `GameManager.AwardStars`; consumable → `ConsumableInventory`.
- **Inventory** (`ConsumableInventory`): **persistent quantity per type** — 3 fixed slots
  (Ketchup/Mustard/Skewer), `Add` increments, `TryConsume(type)` decrements. The counts live in
  `SaveDataManager` (fairy drops and shop purchases feed the same pool, stock carries across
  runs); the inventory is a thin gameplay facade forwarding `OnConsumablesChanged` as `OnChanged`.
  `ConsumableInventoryView` + `ConsumableSlotWidget` render a **screen-space UGUI** row below
  Level/Score: round plate + icon + corner badge (**red num box with the live count**, or **green
  plus box** when empty — the plus box **opens the Shop** paused, the "buy more" deep link).
- **Use — drag-to-column**: press a slot (a stocked one) → carry (the slot icon hides; the only
  visual is the translucent targeting ghost snapped to the nearest column — **nothing follows
  the finger**) → release **over the playfield** to use, **off it** to cancel. `ConsumableDragController.TryBegin` hit-tests the **screen-space** slot
  (only stocked slots carry); the carry/drop are world-space. Driven by `TouchInputHandler` (origin
  disambiguates: a press on a slot becomes a carry and suppresses chef gestures). **World keeps
  moving while carrying** — a cancellable pause would be a free stop-time exploit.
- **Faller + effects**: on release a `ConsumableFaller` drops fast and **resolves on impact**;
  reaching the floor with no target **fizzles** (item still spent). Each `ConsumableEffect`
  supplies a target rule + on-impact behavior + its falling visual (`FallerSprite`/`FallerHeight`;
  mustard drops its authored blob, the skewer the full stick; **ketchup's is null → nothing
  falls and it resolves instantly on release** — all polymorphic, no switch) calling granular
  `GridManager` helpers:
  - **Ketchup** → clears the whole targeted column (`ConsumableClearColumn`).
  - **Mustard** → removes the targeted column's top type **board-wide** (`ConsumableSweepType`),
    per-column collapse + cascade (chain reactions reuse the normal pair-match loop).
  - **Skewer** → drives one `BunBottom` to row 0, destroys the rest, regulars collapse on top
    (`ConsumableSkewer`).
- **Scoring**: non-bun removals score `POINTS_CONSUMABLE_PER_INGREDIENT` (10), flat, no
  multiplier; **buns score nothing** (Ketchup-cleared and Skewer-destroyed alike). Cascades
  score normally via `OnMatchEliminated`.
- **Use VFX** (`ConsumableVfx` + the drag ghost — cosmetic, non-blocking, self-destroying).
  Art direction: the **column ghost IS the nozzle** (ketchup/mustard ghosts are the nozzle art
  via `GhostSprite`); for `GhostLingers` effects it survives the release, holding "locked on"
  over the column (`GHOST_LINGER_DURATION`) before fading. **Ketchup**: nothing falls — on
  release the stream (`fx_ketchup_stream`) extends linearly from the ghost's tip down the column
  while the clear flashes **stagger top→bottom in step with the stream front**
  (`KETCHUP_CLEAR_START_DELAY`/`_STAGGER` paired with `FX_STREAM_EXTEND_DURATION`). **Mustard**:
  the authored blob (`fx_mustard_drop`) falls from the lingering ghost; the sweep resolves on its
  impact. **Skewer**: the full stick falls tip-first (`fx_skewer_falling`, `FallerImpactLift`
  seats the tip), then only its **head stays pinned** at the base (`fx_skewer_head`,
  `ConsumableVfx.SkewerPin`). Sizes `UIStyles.FX_*`/ghost knobs, timings `AnimConfig.FX_*`,
  sorts `SORT_CONSUMABLE_FX_*`.
- **Art** (`RewardArt` + `SpriteFit`): the **fairy is one full-body illustration per payload**
  (`Resources/Fairy/fairy_{gems,stars,ketchup,mustard,skewer}` — the cargo is drawn into the
  art; the old body+badge overlay is gone). The `Resources/Rewards/` badges
  (`ketchup`/`mustard`/`skewer`) remain the column ghost (alpha) + faller sprites; the
  **inventory slot icons** are the splashy kit versions (`Resources/UI/ui_consumable_{name}`,
  via `UiArt`). `SpriteFit.Height` normalizes every sprite to a world-height so source PPU/size
  doesn't matter. Sizes/positions/sorts live in
  `UIStyles` (`*_HEIGHT`, `CONSUMABLE_SLOT_*`) and `Constants` (`SORT_CONSUMABLE_*`).
- **Status**: audio is placeholder procedural tones on the hooks (`PlayConsumableCollect` /
  `PlayConsumableUse(type)` / `PlayConsumableFizzle`) — real sound design deferred. Slot
  layout/sizes are placeholder; eyeball-tune via `UIStyles`. Editor debug: keys **1/2/3** grant
  Ketchup/Mustard/Skewer.

### Skins & Theme (cosmetics)
All gameplay sprites flow through one place: `Theme` (static) reads `Skin` ScriptableObject
assets from `Resources/Skins/` and serves the **active** sprite per `SkinSlot` — the persisted
equipped skin when one is set (`SaveDataManager.GetEquippedSkinId`, applied lazily so early Theme
access can't beat the save layer), otherwise the slot's default. `Theme.Equip(skin)` switches +
persists (ownership is `ShopService`'s concern). Consumers (`IngredientSpawner`, `ChefController`,
`Background`) call `Theme.Ingredient(type)` / `Theme.Chef` / `Theme.Background(type)` — there is
**no** per-scene sprite wiring anymore. Consumers read at spawn/build time, so an equip applies to
everything created afterwards (an in-game chef equip shows on the next scene load, not the live
sprite — acceptable; menu equips always show in-game).
- **Slots** (`SkinSlot`, suffixed `…Skin`): the 8 ingredients + `BunSkin` + `ChefSkin` +
  `PlateSkin` + `GameBackgroundSkin` + `MenuBackgroundSkin` + `RestaurantSkin` + `GridCellsSkin`.
  Two slots carry **two sprites**: `BunSkin` (top **+** bottom bun) and `ChefSkin` (front **+**
  flipped facing; `Theme.Chef` / `Theme.ChefFlipped`). `SkinMap.SlotFor(IngredientType)` maps
  ingredients → slot (both buns collapse to `BunSkin`).
- **Chef flip**: `ChefController.SwapPlates` keeps the 3D Y-rotation but swaps the SpriteRenderer
  Front↔Flipped at the edge-on midpoint (+ toggles `flipX` to cancel the 180° mirror so the
  Flipped art reads un-mirrored). Single renderer, so no second sprite to hide.
- **Plates** (`PlateManager`, `Theme.Plate`): four decorative plates, one under each column
  (`Constants.PLATE_Y_OFFSET` below row 0), purely cosmetic. Sort: `SORT_PLATE (-2)` at the back,
  ingredients (0+), then `SORT_CHEF (50)` — the chef renders **over the plates and the ingredients**
  (intentional final look). The bottom ingredient still sits on the plate. On a flip the two active
  plates **slide** to swap columns (`PlateManager.SwapColumns`, position-only DOMove — sprite never flips).
- **Game background** (`Background`): three stacked layers built in code — base (`GameBackgroundSkin`,
  camera-fill, `SORT_BACKGROUND`), diner strip (`RestaurantSkin`, fills camera width pinned to the top,
  `SORT_RESTAURANT -90`), and the blue play-mat (`GridCellsSkin`, scaled to `UIStyles.GRID_CELLS_WIDTH`
  centred over the columns at `GRID_CELLS_Y` + `GRID_CELLS_X_NUDGE`, `SORT_GAME_PANEL`). Layout tunables
  (`RESTAURANT_Y_NUDGE`, `GRID_CELLS_WIDTH`, `GRID_CELLS_X_NUDGE`, `GRID_CELLS_Y`) live in `UIStyles`. The
  old dim-filter overlay was removed entirely. The menu background uses the base layer only.
- **UI chrome & font** (outside the Theme/Skins pipeline): authored UI sprites load directly from
  `Resources/UI` via `UiArt.Load(name)`; build UGUI Images with `UIFactory.CreateImage`. The game
  font is **Panton-Trial-ExtraBold** (SDF in `Assets/_Project/Fonts/`), wired as the **TMP default
  font** (`TMP Settings.asset`), so all TMP text follows it — no per-component font is set. `GameHUD`
  builds the authored **Level/Score panels** (card + title tab + TMP number). ⚠️ Panton is a **trial**
  font (replace before release); regenerate any SDF at **1024 atlas / ~12 padding / SDFAA** (a
  512/low-padding atlas renders pixelated).
  ⚠️ **Trial-font placeholder glyphs**: the trial TTF maps most symbols — `" # $ % & ' ( ) * + / <
  = > @ [ \ ] ^ _ ` + backtick + `{ | } ~` — to a single tall sliver glyph (tiny vertical "trial"
  lettering baked into the SDF atlas). It renders as a weird vertical word, immune to wrapping
  settings (discovered 2026-07-23 — was misread as a text-wrap bug for a while). **Player-facing
  strings may only use letters, digits, space, and `! , - . : ; ?`** until the font is replaced.
  Where a plus sign is needed, use art (`ui_consumable_plus`, e.g. the Settings stepper's
  increment button) — the score/star popups use `N!` instead of `+N` for this reason. Glyphs
  Panton lacks ENTIRELY (e.g. **€** in localized IAP prices) fall back to **LiberationSans SDF**
  (`TMP Settings.asset` → m_fallbackFontAssets, wired 2026-09-05): they render, but in the
  fallback font's own plain material — no sticker stroke/shadow (TMP fallback glyphs can't use
  the styled material). Acceptable for price labels; the sliver-glyph symbols above do NOT fall
  back (Panton claims them), so their workarounds stay until the font swap.
- **Reskin the game** = edit the slot's asset in `Resources/Skins/` (set its **Sprite** field; for
  `bun_default` also **Secondary Sprite** = bottom bun, for `chef_default` = flipped facing), or
  just replace a PNG's contents keeping its filename. Works from the Project window with any scene
  open — no more opening `Game.unity`.
- **Purchasable skins (curated catalog, 2026-09-05)**: every slot = default + 2 star skins
  (cheap 400★ / "golden" end-game 5000★) + 2 gem skins (cheap 60◆ / expensive 100◆). Dogtors:
  Burgerchain 500★ (cheap), Royale 10000★ (end-game), European 80◆, Japanese/Mexican 150◆ each.
  Star-cheap = the old gourmet set (Chicken, Cheddar, Cherry Tomato, Caramelized Onion, Relish,
  Shredded Lettuce, **Boiled Egg** — new; the gourmet Quail Egg moved to gems-expensive) and
  Brioche Buns; star-expensive = the gold set. Gem skins (2026-09-04 kit art): Vegan/Wagyu patty,
  Shredded cheese, Pico de Gallo/Kumato, Pickled/Crispy onion, Bell Pepper, Avocado/Purple
  Cabbage, Omelet/Quail, Pulled Pork/Iberic Ham, Blue Cheese, Rustic (Bocata) / Black Bread buns
  — sourced from `RawArt/iNGREDEINTS REVISED` (proper names; the earlier kit misnamed several).
  The star-cheap bun is **Integral** (top + real bottom, delivered 2026-09-05; the
  asset keeps the `bun_gourmet` id). **Deleted placeholders**: meat_alt,
  chef_alt, chef_happy (assets + sprites). Jalapeño (gems 100◆) landed 2026-09-05 — no missing skin art remains. Shop rows are
  ordered buns → level-1 ingredients → the rest by appearance (`ShopCatalog.IngredientSlots`,
  keep in step with `REGULAR_INGREDIENTS`). Shop cells: ingredient previews sit on a plate
  (`SHOP_SKIN_PLATE_*`), buns preview as the top+bottom pair (`SHOP_SKIN_BUN_*`).
- **Status**: selection + star-unlock shipped with the Shop (2026-07-05); ingredient + dogtor skin
  content shipped 2026-07-21. Gem/IAP-priced skins and Pack bundles remain unbuilt
  (`UnlockMethod.Gems/Iap` exist; `ShopService.TryBuySkin` already handles Gems). Prices are eyeball
  tiers (`_starCost` per asset) — revisit in the economy balance pass.

## Randomness
All randomness uses `Rng` static class, never `UnityEngine.Random`:
```csharp
Rng.Range(0, max)      // int, exclusive max
Rng.Range(0f, 1f)      // float
Rng.Value              // float 0-1
```

## Known Issues

### Text Outline Rendering (RESOLVED for UGUI — playtested 2026-07-05)
Setting `tmp.outlineWidth`/`tmp.outlineColor` on a runtime-created TMP component doesn't reliably
render outlines (keyword `OUTLINE_ON` never enabled; material init timing). **The working path is
`UIFactory.StyleFillAndBorder`**: clone the font's material once per style (cached per
font+color+width, shared — batched), `EnableKeyword(ShaderUtilities.Keyword_Outline)`, set
`ID_OutlineColor`/`ID_OutlineWidth`, assign via `fontSharedMaterial`, then `UpdateMeshPadding()`.
Since 2026-09-03 the same material also carries the **sticker drop shadow** (TMP Underlay pass,
border-colored, hard-edged, offset down — `UIStyles.TEXT_SHADOW_*`), replicating the artist's
Photoshop stroke+shadow lettering (`Look Reference/Font info.png`) on every bordered text. The
SDF atlas padding (12 @ 144pt) caps outline+shadow reach — regenerate with more padding if
glyph edges clip.
The broken per-component setters were removed from `UIFactory.AddStyledText`. World-space
(`WorldTextFactory`) was migrated to the same shared-material path on 2026-09-04 — world popups
now carry the full sticker lettering (fill + stroke + shadow), same as the UI.

### UI Text Wrapping Convention
Runtime-created TMP text defaults to wrapping ON (TMP Settings), which renders one character
per line on label-sized rects ("vertical text"). `UIFactory.AddStyledText` therefore forces
**NoWrap on every text it builds**; pass `wrap: true` to `CreateText` only for genuinely
auto-wrapping paragraphs (currently just the shop confirm dialog). Explicit `\n` still breaks
lines under NoWrap (e.g. the game-over "Main\nMenu" word). Overlong single-line text now overflows its rect instead of
wrapping — fix by shortening the string or widening the rect, never by re-enabling wrap on labels.

### UI Text Color Convention
- **HUD numbers + all red-box labels** (Level/Score numbers, tab words, SPECIAL ORDER banner,
  mult badge, consumable counts): the reference palette — cream fill `#FCFAF1` + dark-brown border
  `#492611` via `UIFactory.StyleHudText` (`UIStyles.HUD_TEXT_*`).
- Top-bar currency pill numbers: HUD palette too (since 2026-09-03 — plain brown read poorly on
  the pill art once everything gained the sticker shadow). `TOPBAR_NUMBER_COLOR` (plain dark
  brown) remains for small plain labels (shop subtitles, Restore Purchases).
- Buttons/popups/panels (menu): white text.
- World-space popups: **cream sticker lettering** (`HUD_TEXT_FILL` + stroke + shadow, shared
  material — 2026-09-04). The glow plates carry the popup's meaning (green = score, yellow =
  multiplier/stars); the one non-cream popup is "Too bad!" (red = failure).

## Roadmap

### Phase 1 — Architecture refactoring + code review (COMPLETE)
The full human-led code review (`Docs/Review/`, findings F-1…F-87) is implemented:
**86 / 87 resolved**, every behavioral change playtested. Highlights:
- 6-config-file split — `Constants` / `GameplayConfig` / `MonetizationConfig` / `AnimConfig` / `UIStyles` / `AudioConfig` (by who-tunes-it)
- `Singleton<T>` base for the 7 managers; `MonoBehaviourUtil`, `AppBootstrap`, `SoundSettings`
- `Scoring/` (scoring + `BurgerTier` + `BurgerNamer`); `Grid/` → `MatchDetector` (match only) + `BurgerAnimator` + `SwapAnimator` + `BurgerData`
- `BurgerChallenge` → model + `BurgerChallengeView`; `IngredientSpawner` → `WaveComposer` + `WavePreviewManager`
- `UI/Factory/` — `UIFactory` (UGUI) + `WorldTextFactory` (world TMP) + `SpriteFactory` (cached procedural sprites)

See `Docs/Review/README.md` for the per-finding index. **Only F-4 remains** —
the procedural-SFX skeleton dedup, deliberately deferred (verify-by-ear, low value;
revisit only if the SFX get reworked).

### Phase 2 — Polish & final (CURRENT)
Making the game feel and look finished. **Playtest-driven**: the developer plays and
notes what's off; changes land as edits — mostly one-line tweaks, since feel / visual /
balance values are centralized in `AnimConfig` / `UIStyles` / `GameplayConfig`. Focus areas:
- **Game feel / juice** — animation timing, screen shake, squash/stretch, popups (`AnimConfig`)
- **Difficulty & balance** — level curve, wave speed, triple-wave chance, Special Order difficulty, scoring (`GameplayConfig`)
- **Audio** — real SFX/music via the authored-clip override path (`AudioManager._*Override` fields) or procedural/mix tuning (`AudioConfig`)
- **Visual** — readability/contrast, the placeholder sprite (`UIStyles`); the text-outline fix landed 2026-07-05 (see Known Issues)

### Phase 3 — Backlog (potential next work; noted 2026-06-12, not yet started)
Dev-flagged directions for future sessions, roughly in priority order. Nothing here is designed
or committed yet — capture only.
- **Gem & Star economy** — DONE 2026-07-05 (Shop session): both currencies persisted, spendable
  (skins/consumables in stars, star packs in gems, gem packs via mock IAP, remove-ads) and
  **earnable** (stars per Special Order + end-of-run score payout; gems from fairies/ads/IAP).
  Still open: **balance pass** on the earn rates vs prices once real runs generate data
  (all knobs in `MonetizationConfig`).
- **Shop** — DONE 2026-07-05 (full-screen `ShopScreen`, replaces the old `ShopPanel`): skins
  (buy+equip), consumables (persistent stock), currency bundles, remove-ads. Still open: real
  IAP SDK, authored shop art, on-device layout pass.
- **Prepare for real assets** — replace placeholder art across the game. Pipeline is ready
  (`Theme` / `Resources/Skins` for gameplay art, `RewardArt` for consumables) — mostly a content swap.
- **Prepare for real UI components** — UI is currently code-built/procedural (`UIFactory`,
  `WorldTextFactory`, `SpriteFactory`); move toward authored UI components / polish.
- **Full advertisement implementation** — Phase 1 (production-shaped interface: `IAdProvider`,
  preload/ready lifecycle, reward-on-callback, timeScale fix) DONE 2026-07-05. Remaining =
  launch-gated SDK body-swap: pick network (leaning LevelPlay), register app, ad-unit IDs,
  `LevelPlayAdProvider`, consent (GDPR UMP + iOS ATT, needs privacy policy), on-device testing.
- **Sound / music** — optional; dev is fairly happy with the current procedural SFX. Real
  SFX/music can drop in via the `AudioManager._*Override` slots and `Resources/Music/` when wanted.

### Skin System
Configured centrally via `Theme` / `Resources/Skins` (see Core Systems → Skins & Theme).
Granular: one skin = one slot = one sprite (bun = top+bottom).
- **Phase 1 (DONE)** — built primarily as a **content-swap convenience**: all gameplay art now flows
  through `Theme`, so swapping a sprite means editing one asset (or replacing a PNG), never touching a
  scene. `Skins/` foundation + 11 default skin assets; `IngredientSpawner`, `ChefController`, `Background`
  refactored off scattered SerializeFields. All art renamed to short names (files **and** sprite
  sub-assets), fileIDs preserved.
- **Phase 2 (DONE 2026-07-05, via the Shop)** — runtime per-slot selection + persistence
  (`Theme.Equip` / `SaveDataManager` equipped-per-slot) and the star-unlock buy flow, surfaced in
  the Shop's skin rows (the shop is the wardrobe — no separate Skins UI).
- **Phase 3 (DEFERRED)** — gem/IAP/ad-unlock skins and Pack bundles. `UnlockMethod` and
  `ShopService.TryBuySkin` already support Gems; the rest is authoring + shop surfacing.

## Asset Conventions
- **Renaming a sprite**: rename the file *and* the sprite **sub-asset** (the fold-out child) — the latter
  lives in the `.png.meta` in three spots (`internalIDToNameTable.second`, `spriteSheet.sprites[].name`,
  and the `nameFileIdTable` key). The sprite's `internalID`/fileID is stored explicitly, so editing the
  name preserves it — `.asset` and scene references by fileID survive. Always move the `.png.meta` with the
  `.png` so the texture GUID is kept.
- **Hand-authored metas**: this project writes minimal `.cs.meta` (just `fileFormatVersion` + chosen `guid`)
  and `.asset` YAML directly, since the AI can't drive the Editor.
- **⚠️ Meta/asset YAML must be CRLF + end with a trailing newline.** Unity's meta parser fails at
  EOF on the final line otherwise — *"Parser Failure at line N: Expect ':' between key and value"*
  + *"does not have a valid GUID… will be ignored"* — silently dropping every hand-written `.meta`
  (both `.png.meta` and `.asset.meta`). LF-only with no trailing newline is the trap (bit the
  2026-07-21 skin batch). When generating via script, normalise: convert `\n`→`\r\n` and append one
  trailing `\r\n` before writing (see `scratchpad/gen_skins.ps1`'s `Nl` helper).
- **Consumable/reward sprites** live in `Resources/Fairy/` + `Resources/Rewards/` (loaded by name via
  `RewardArt`, outside the `Theme`/Skins pipeline). They must import as **Single** sprite mode — the
  project's default is **Multiple**, which auto-slices multi-blob images into fragments and breaks
  `Resources.Load<Sprite>` (it returns only the first fragment). `SpriteFit` sizes them by world-height,
  so PPU / source size doesn't matter.
- **Sizing gameplay sprites (ingredients/chef/plate)**: they render at `localScale=1`, so on-screen size =
  `pixelWidth / spritePixelsToUnits`. Normalise by setting per-file `spritePixelsToUnits = pixelWidth /
  targetWorldWidth` in the `.png.meta` (no pixel editing, no distortion — PPU scales both axes equally).
  ⚠️ Normalise by the **visible art**, not the canvas: a sprite whose art floats in a padded canvas
  (chef_alt sat in a corner of 3240x4000 → rendered at 43% size when equipped, found 2026-09-03) needs
  its sprite **rect cropped to the opaque bbox** in the meta AND the PPU computed from that rect
  against the default skin's visible size (chef_alt + meat_alt fixed this way).
  Current targets: ingredients ~1.2 wide (some hand-tuned ±%), buns ~1.38, chef PPU 663, plate PPU 756.
- **Large sources (>2048 px)**: bump `maxTextureSize` to **4096** in the `.png.meta` (top-level + the
  non-512 platform entries). Otherwise Unity downscales the texture below the hand-authored sprite rect →
  *"sprite rect out of bounds"* import error, which **wipes the meta's sprite definition** (`sprites: []`),
  leaving dangling `_sprite`/scene refs → invisible. The chef (2387px) and the backgrounds (2604px) need this.
- **Crispy / aliased fine-lined sprites shown small**: enable **mipmaps** (`enableMipMap: 1`) so minified
  thin lines don't shimmer/pixelate. All gameplay sprites have it on; the plate is also `textureCompression: 0`
  (uncompressed) as it's a single fine-lined hero sprite.
- **Sprite internalID collisions**: never create a sprite by copying another's `.png.meta` and only changing
  the `guid` — the sprite `internalID` (fileID) gets duplicated, and two textures claiming one fileID
  cross-wire references (this caused the chef↔chef_alt bug). Each sprite needs a **unique** internalID across
  `internalIDToNameTable`, `spriteSheet.sprites[].internalID`, and `nameFileIdTable`.

## Pending Manual Steps
- **Chef size/position tuning**: knobs are the chef sprite **PPU** (990 as of the last tuning pass; an older note said 2009 — trust the meta, not this file, and update here when retuned) for the original
  import) for size, and `Constants.CHEF_BOTTOM_OFFSET` (1.76) for the feet line — `GetWorldPosition` anchors
  the feet and derives the centre from the live sprite height, so resizing keeps the chef on the bottom border.
- **Verify skin import (Phase 1)**: open Unity, confirm a clean compile and that `Resources/Skins/*.asset`
  each show their sprite (not "None"); the game should look identical to before.
- **Before release: lower `SETTINGS_LEVEL_CAP` to `MAX_LEVEL`** (`GameplayConfig`). It's currently
  `KILLER_LEVEL` (21) so the kill screen is reachable from the Settings stepper for testing; players
  should not be able to *start* on the kill screen. One-line flip (comment marks it).

## Pending Features
- **HUD done so far** (authored, screen-space UGUI): the shared **TopBar** (`UI/TopBar.cs` —
  trophy/star/gem pills + optional help-"?"/settings buttons; one recipe used by the game HUD, the
  main menu, and the shop header, so the bar looks identical and stays put across screens; it
  self-binds to the SaveDataManager currency events and punches a pill on change),
  Level/Score cards, the 3-slot consumable row, the Special Order panel. The HUD scales with the
  camera (both frame by width — see Camera & UI scaling). **Boxes are baked art at native aspect**
  (the 9-slice route was dropped — fixed-size HUD boxes don't need it).
- **Main Menu (authored, 2026-08-30)**: rebuilt to the artist's mock — logo (top-anchored),
  the authored PLAY button (the high-score plaque was dropped 2026-09-03 as redundant — the
  TopBar trophy pill shows the high score; `ui_hs_plaque` stays imported but unused),
  checkered bottom strip with CREDITS + SHOP,
  TopBar with the settings gear (shop stays a bottom button). Knobs: `UIStyles.MENU_*`; art in
  `Resources/UI` (`ui_logo`, `ui_hs_plaque`, `ui_play_button`, `ui_menu_bottom`, `ui_btn_cream`,
  `ui_btn_yellow` — the last reserved for the Game Over screen). **Completed with the
  2026-09-01 kit**: the colored illustration (`Main Illustration Complete`, resized to 2048 px)
  now IS `Sprites/Background/bg_menu.png` — replaced in place so the `menu_bg_default` skin and
  its GUID stayed put; `Background` covers the camera with it (uniform scale, center crop of the
  near-square art). CREDITS/SHOP are the authored red/yellow blanks (`ui_menu_btn_credits` /
  `ui_menu_btn_shop`, sized by `MENU_BOTTOM_BTN_W`, words overlaid). The mock's top-right red
  bow button (in the kit's `UI Buttons.png` icon sheet) still has no known function — skipped;
  the kit's `Notification Icon` ("!" tile) likewise.
- **Game Over screen (authored, 2026-08-31)**: rebuilt to the artist's mock (`Look
  Reference/GameOver.png`). The panel art (`ui_gameover_panel`) is a **full-phone canvas**
  (2327x4138 ≈ the 9:16 reference aspect) shown at `REFERENCE_RESOLUTION`, so the baked red
  title bar / cream body / "Continue" band land exactly where drawn; all text and buttons are
  overlaid at positions measured off the mock. Level/Score are the **same authored HUD cards**
  via the shared `UI/StatCard.cs` (extracted from `GameHUD`, takes a scale — game over uses
  1.25x). Continue row: cream blank + gem icon + cost, blue `ui_btn_blue_watch` (TV icon baked)
  with a "Watch"/"Loading..." auto-fit label; one continue per run — afterwards the heading
  reads "No more continues" and the buttons hide. Nav row: `ui_btn_green` Main Menu /
  `ui_btn_yellow` Retry (words overlaid — no per-word art delivered). "N stars earned!" sits
  below the panel in gold. Knobs: `UIStyles.GAMEOVER_*` (eyeball defaults from the mock — tune
  live); helpers `UIFactory.SizeByWidth/SizeByHeight/AutoFit` are now shared (MainMenuUI's
  private copy removed). Note: the button blanks' canvases include their drop shadow, so the
  visible face is ~10% smaller than the width knob.
- **Modal chrome (`UI/ModalPanel.cs`, 2026-09-01)**: the Settings and Credits screens share one
  plain-class builder — dim overlay (`UIStyles.MODAL_OVERLAY`), the full-canvas panel art
  `ui_modal_panel` (orange title tab + dotted cream body, same 2327x4138 sheet as the game-over
  panel) at `REFERENCE_RESOLUTION` (+ a per-screen offset), the title word on the tab, the round
  red X (`ui_btn_close_x`) over the tab's corner, and the game-over pop-in (`AnimConfig.PANEL_*`).
  Screens parent their content under `ModalPanel.Panel` and call `Show`/`Hide`/`Kill`. Knobs:
  `UIStyles.MODAL_*`. A new modal screen = `ModalPanel.Build(canvas, "TITLE", sheetArt,
  panelOffset, chromeOffset, Hide)` + content under `Panel` (a plain content root at the
  reference size with the sheet image under it). Each screen passes its own full-canvas sheet
  (`ui_modal_panel` for Settings, `ui_credits_panel` for Credits); `chromeOffset` moves the
  title + X when a sheet draws its tab elsewhere than the Settings one.
- **Settings panel (authored, 2026-09-01)**: rebuilt to the mock (`Look Reference/settings.png`)
  on the modal chrome: wide blue rows (`ui_btn_blue_wide`, sized by width, HUD-palette auto-fit
  labels) stacked down the body: **Sound: ON/OFF**, **Controls: Drag/Tap**, then the third row is
  **Mode: Classic/Relax** in the menu (see Core Systems → Game Modes) or the full-width
  **Quit to Menu** in-game (Restart dropped 2026-09-05 — game over already offers Retry). Both openers (menu gear, in-game
  gear) share the one class. Knobs: `UIStyles.SETTINGS_*` (eyeball defaults — tune live). Deliberate gaps:
  the mock's third **"Language: ENG"** row is **not built** — there is no localization system,
  and a button that does nothing is worse than none; add it as one `CreateRowButton` call when
  localization exists (in-game it would then need a 4th row or a tighter pitch). The **level
  stepper** is an inspector opt-in on `MainMenuUI` (see Controls).
- **How to Play panel (2026-09-05, `UI/HowToPlayPanel.cs`)**: the in-game top bar's **"?"
  button** (replaced the shop button; the kit's blank green square `ui_btn_square_green` + a HUD
  question mark) opens the modal chrome ("HOW TO PLAY") with five brown rule lines — strings in
  the class (trial-font-safe), layout `UIStyles.HOWTO_*`. Same pause/resume pattern as the
  in-game Settings. `ui_shop_button` stays imported but unused.
- **Credits panel (authored, 2026-09-01, `UI/CreditsPanel.cs`)**: menu-only, to the mock (`Look
  Reference/Credits.png`) on the modal chrome with **its own sheet** from the 2026-09-01 kit
  (`ui_credits_panel` — taller and wider than the Settings sheet, tab ~38 px higher →
  `CREDITS_CHROME_OFFSET`). Three `CreditsEntry` lines — **A GAME BY** Oscar Torandell / **ART
  BY** Lucia Varona / **MUSIC BY** the five OpenGameArt artists — each a colored role heading
  (`StyleFillAndBorder`, accent + HUD border) over the kit's **checkered band**
  (`ui_credits_band_game/art/music`: text-free, translucent, sized by `CREDITS_BAND_W` — the
  canvas has a ~4% margin around the face) with the name in the HUD palette. Entries live at the
  top of `CreditsPanel`; colors/layout in `UIStyles.CREDITS_*`. A name may span explicit `
`
  lines and auto-fits inside the band (down to `CREDITS_NAME_SIZE_MIN`). **MUSIC BY lists five
  names** (SketchyLogic, BossLevelVGM, Martin Nilsson, Alex McCulloch, Spring Spring) — three are
  **CC-BY 3.0, so the credit is a license requirement**; the track ↔ source ↔ license table is
  `Docs/music-attribution.md` (keep it and the entry in sync; BossLevelVGM also wants the credit
  in the store listing).
- **★ glyph**: Panton (ASCII) lacks U+2605; add a fallback font or the `Star` sprite where needed.
- **UI integration ≠ pure art-swap** — remaining wiring that implies real code:
  - **Mult meter**: a filling capsule gauge (right of Special Order) showing progress to the next
    challenge level. **Built** (`BurgerChallengeView.BuildMultMeter`, `ChallengeFill`) — slot
    position/size are eyeball defaults in `UIStyles.MULT_METER_*`, tune live.
  Plan each of these as its own code task alongside the visual wiring.
- **Shop**: authored art throughout except the wide green cell pill (derived — see Core Systems →
  Shop). Layout knobs in `UIStyles.SHOP_*`, untested on-device — eyeball defaults, tune live.
- **IAP**: Unity IAP wired (see Monetization) — verify in the editor once the package imports
  (the Unity IAP fake store dialogs), then on the Play internal track with license testers;
  generate the receipt-validation tangle before launch.
- Consumable polish: real SFX (override slots ready) + final slot layout/sizes (placeholders in `UIStyles`)
- Ad SDK integration: code + Android credentials + package ID done (see Monetization → Real ad SDK); needs device test, iOS credentials

## Pre-Launch Checklist
Platform-readiness / launch-logistics items tracked separately from
code-review findings. See `Docs/pre-launch-checklist.md` for the full
list (save layer security, cloud save, schema versioning, IAP receipt
validation, analytics, privacy policy, etc.). Ready-to-use copy:
`Docs/privacy-policy.md` (+ `.html` to host, e.g. GitHub Pages — the URL goes in the Play
listing and the LevelPlay dashboard) and `Docs/play-store-listing.md` (descriptions,
questionnaire answers, product ids, asset specs). ⚠️ **Font**: the "licensed" Panton zip
(`Fuentes/panton.zip`, 2026-09-01 kit) contains the same **trial** TTFs plus Fontfabric's
**free** weights (`Commercial/PantonDemo-Black.otf` = "Panton Black Caps", caps-only, and
Light; FF Free Font EULA allows apps). The ExtraBold in use is still trial — decide: buy
Panton ExtraBold, or switch to the free Black Caps (all-caps UI). Either way the SDF is
regenerated in the editor (1024 atlas / ~12 padding / SDFAA) and the symbol workarounds go.
