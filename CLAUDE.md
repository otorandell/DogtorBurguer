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
                 ShopRowScroll (nested h-scroll), ShopService (purchase rules), ShopCatalog
  UI/            MainMenuUI, GameHUD, GameOverPanel, SettingsPanel,
                 BurgerChallenge, BurgerChallengeView, BurgerPopup, FloatingText, ScorePopup,
                 Background, OrderType, NumberFormat, UIFactory
    Factory/     SpriteFactory (cached procedural sprites), WorldTextFactory (world-space TMP)
  Audio/         AudioManager, MusicManager, MusicCategory
  Monetization/  AdManager (facade), MockAdProvider, GemProduct, StarProduct, ConsumablePack,
                 BurgerFairy, BurgerFairySpawner
    Abstractions/ IAdProvider (the ad-network contract)
  Consumables/   ConsumableType, ConsumableEffect (+ Ketchup/Mustard/Skewer), ConsumableEffects,
                 ConsumableFaller, ConsumableVfx (use effects), ConsumableInventory,
                 ConsumableInventoryView, ConsumableSlotWidget, ConsumableDragController,
                 FairyPayload, FairyPayloadKind, RewardArt, SpriteFit
```

## Singletons
These classes use the singleton pattern. Initialization order matters:
1. **SaveDataManager** -- player data persistence (PlayerPrefs)
2. **MusicManager** -- background music, DontDestroyOnLoad
3. **AdManager** -- ad integration (currently mock)
4. **GameManager** -- central game state, creates missing managers via EnsureComponent
5. **GridManager** -- column/grid state
6. **AudioManager** -- procedural SFX generation
7. **BurgerChallenge** -- challenge UI and tracking
8. **ConsumableInventory** -- gameplay facade over the persistent consumable stock (SaveDataManager); created by GameManager
9. **ConsumableInventoryView** -- world-space inventory slot icons
10. **ConsumableDragController** -- the drag-to-column carry interaction
11. **PlateManager** -- the four decorative under-column plates; slides two to swap on the chef flip

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
- **Standing preview queue**: a target number of upcoming pieces (= the next wave's size) is always reserved and shown as blinking ghosts, one per column. Topped up every frame (`TopUpPreviews`) and seeded at `StartSpawning` (hidden through the initial delay). The reserved previews **are** the next wave — when the current wave lands, they become the real pieces, dropping in their columns.
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
**1/2/3** grant Ketchup/Mustard/Skewer to the inventory, **4** grants 500 stars (`#if UNITY_EDITOR`).

**Input-area debug gizmos** (editor-only, `#if UNITY_EDITOR`): each clickable zone is drawn by the
component owning its hit-test, one color per interaction (`GizmoStyles`): falling fast-drop (green,
`IngredientSpawner`), preview tap (yellow, `WavePreviewManager`), chef flip circle (magenta) + move
sides (cyan, `TouchInputHandler` — mode-aware, Tap mode only), fairy tap (orange, `BurgerFairy` —
play-mode only, runtime-spawned). Toggle per-script via Unity's Gizmos menu.

The Settings panel also has a **Start Level** stepper (`[−] Start Level: N [+]`) → persists
`SaveDataManager.StartingLevel`; clamped 1..`SETTINGS_LEVEL_CAP`. See Difficulty.
**Settings opens in-game too** (top-bar gear, `GameHUD.OnConfigClicked`): same pause pattern as
the shop — pauses a running game, panel on its own canvas (`SETTINGS_CANVAS_SORT` 110, above
game-over, below shop), resumes via `SettingsPanel.OnClosed`. Sound/control-mode apply live
mid-run; Start Level applies next run. The in-game variant (`Initialize(canvas, showRunButtons:
true)`) is taller and adds a **Restart | Quit to Menu** row — restart runs the same interstitial
cadence as the game-over restart (`AdManager.MaybeShowInterstitial`, the shared gate); quitting
keeps live-earned order stars but forfeits the end-of-run score payout.

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

### Burger Challenge (BurgerChallenge) — "Special Orders"
- Two order types, randomly chosen:
  - **Size**: "N+ Ingredients" — any burger with at least N ingredients matches
  - **Contains**: "Has: Meat, Cheese" — burger must include required ingredients (extras OK)
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
- Level up requires `level + 1` matches (`Level` still exposed; the in-panel ★ readout was dropped)
- Each match also **awards stars** (the currency faucet — see Monetization & Currencies)

### Scoring
- Match: 10 pts per matched pair
- Burger bonuses: 5 (poor) to 500 (9+ ingredients)
- Per ingredient: 10 pts
- Challenge multipliers stack with global multiplier

### Grid
- 4 columns, 13 max rows
- Cell: 1.4w x 0.4h visual height (60% overlap)
- Grid origin: (-2.1, -4.2)
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

### Audio
- **AudioManager**: All SFX procedurally generated (sin waves, envelopes, harmonics). No audio asset
  files. Each sound has an optional `_*Override` clip slot for authored audio. Consumable hooks:
  `PlayConsumableCollect` / `PlayConsumableUse(type)` / `PlayConsumableFizzle` (placeholder tones).
- **MusicManager**: Loads tracks from Resources/Music/. Random selection per scene

### Monetization & Currencies
- **Two currencies**, both persisted in SaveDataManager and spendable in the Shop:
  - **Gems** (hard/premium): rare — Burger Fairy drops (~40% of fairies), rewarded ads, IAP packs.
  - **Stars** (soft/free): earned by playing — **per completed Special Order**
    (`STARS_PER_ORDER_BASE + PER_LEVEL·(challengeLevel−1)`, awarded live with a gold "+N STARS"
    popup), an **end-of-run score payout** (1★ per `STAR_SCORE_DIVISOR` score; continues pay
    only the un-paid delta), and **star fairies** (`STAR_PACK_VALUE` 25). `GameManager.AwardStars`
    grants + tracks `StarsEarnedThisRun` (shown on the game-over panel). Also from gem→star shop
    packs; editor debug key **4** grants 500.
- One-directional exchange: gems buy stars, never the reverse (standard freemium convention).
- Continue after game over: 50 gems or watch ad
- Interstitial ads every 3 games, shown only on the two **restart** buttons (game over + in-game
  settings, both via `AdManager.MaybeShowInterstitial`). Menu Play and Quit-to-Menu are ad-free —
  yes, quit→menu→Play can dodge a restart ad; **deliberate decision (2026-07-05)**: the dodge
  costs more friction than it saves and gating Play felt hostile. Don't "fix" it; if ad pacing
  ever needs rework, use a time cooldown at break points instead. **Suppressed once Remove Ads
  is bought** (`SaveDataManager.AdsRemoved`, in `ShouldShowInterstitial`). Rewarded ads stay.
- **Burger Fairy** drops during gameplay (`FAIRY_SPAWN_CHANCE` 0.20 / 10s) carry a consumable
  (~60%) or currency (~40%, split gems/stars by `FAIRY_STAR_SHARE` → ~20% each) — see Core
  Systems → Consumables. Gem fairies award `GEM_PACK_VALUE` (5), star fairies `STAR_PACK_VALUE`
  (25, routed via `GameManager.AwardStars` so it counts toward the run total).
- **Ad architecture** (production-shaped, 2026-07-05): `AdManager` is a facade owning one
  `IAdProvider` (contract in `Monetization/Abstractions/`) plus the ad policy (cadence,
  remove-ads suppression). The provider models the real SDK lifecycle: async init, **preload**
  (`IsInterstitialReady`/`IsRewardedReady` are real load state), auto-reload after show, retry
  on failure, **reward only from the reward callback**, and timeScale save/restore (an ad over
  a paused game hands the pause back). `MockAdProvider` simulates it all, including a
  `NO_FILL_CHANCE` so not-ready UI paths get exercised — ad buttons (game-over continue, shop
  FREE) disable + relabel while no ad is loaded. **Swapping in the real SDK = one new provider
  class + one line in `AdManager.Awake`** (SDK choice leaning Unity LevelPlay — see checklist).
- IAP flows (gem packs, remove-ads) are stubs that grant immediately — see
  `ShopService.BuyGemPack` / `BuyRemoveAds`.

### Shop (full-screen overlay — `Scripts/Shop/`)
One vertically scrolling page under a fixed header (SHOP title + star/gem pills + close), on its
own canvas (`SHOP_CANVAS_SORT` 120, above everything). Opened via `ShopScreen.Open()` (menu Shop
button) or `ShopScreen.OpenInGame()` (in-game top-bar shop button and the consumable slots' green
plus box) — the in-game path **pauses** the run (`GameManager.PauseGame`) and resumes on close;
all shop tweens run unscaled. Rebuilt each open, destroyed on close (no stale state).
- **Sections, top → bottom** (order follows freemium-shop research: offer banner up top, currency
  near the bottom): Remove-Ads banner (hidden once bought) → DOGTOR SKINS → INGREDIENT SKINS
  (horizontal `ShopRowScroll` rows; vertical drags route to the page scroll) → POWER-UPS (3
  consumable cards, star-priced pack ladder) → GET STARS (gem-priced, **confirm dialog** — the
  only confirm; soft spends and equips are instant) → GET GEMS (free rewarded-ad rung first, then
  IAP packs with MOST POPULAR / BEST VALUE badges).
- **Skin cells**: 3 states — EQUIPPED (green highlight), owned (tap = equip instantly), priced
  (currency icon + cost; tap = buy **and auto-equip**; insufficient funds shakes the cell).
  The shop *is* the wardrobe — no separate skins screen.
- **Layer split**: `ShopScreen` (frame/orchestration + confirm dialog + balance pills),
  `ShopSections` (page composition), `ShopWidgets` (low-level UGUI builders), `ShopSkinCell`
  (cell widget), `ShopService` (atomic purchase rules, UI-free), `ShopCatalog` (groups skins;
  a slot appears only once it has a non-default skin). Layout knobs: `UIStyles.SHOP_*`;
  prices: `MonetizationConfig` (packs/ladders) + per-skin `_starCost` on the Skin asset.

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
- **Use — drag-to-column**: press a slot (a stocked one) → carry (a world-space icon follows the
  finger, a translucent ghost snaps to the nearest column) → release **over the playfield** to use,
  **off it** to cancel. `ConsumableDragController.TryBegin` hit-tests the **screen-space** slot
  (only stocked slots carry); the carry/drop are world-space. Driven by `TouchInputHandler` (origin
  disambiguates: a press on a slot becomes a carry and suppresses chef gestures). **World keeps
  moving while carrying** — a cancellable pause would be a free stop-time exploit.
- **Faller + effects**: on release a `ConsumableFaller` drops fast and **resolves on impact**;
  reaching the floor with no target **fizzles** (item still spent). Each `ConsumableEffect`
  supplies a target rule + on-impact behavior + its falling visual (`FallerSprite`/`FallerHeight`;
  ketchup drops its badge bottle, mustard an authored drop, the skewer the full stick — all
  polymorphic, no switch) calling granular `GridManager` helpers:
  - **Ketchup** → clears the whole targeted column (`ConsumableClearColumn`).
  - **Mustard** → removes the targeted column's top type **board-wide** (`ConsumableSweepType`),
    per-column collapse + cascade (chain reactions reuse the normal pair-match loop).
  - **Skewer** → drives one `BunBottom` to row 0, destroys the rest, regulars collapse on top
    (`ConsumableSkewer`).
- **Scoring**: non-bun removals score `POINTS_CONSUMABLE_PER_INGREDIENT` (10), flat, no
  multiplier; **buns score nothing** (Ketchup-cleared and Skewer-destroyed alike). Cascades
  score normally via `OnMatchEliminated`.
- **Use VFX** (`ConsumableVfx`, fired via the polymorphic `ConsumableEffect.PlayVfx` alongside
  `Apply` — cosmetic, non-blocking, self-destroying). Art direction: **the nozzle locks onto the
  used column**; the per-type faller is the "thing that drops". Ketchup = nozzle + a stream
  squirted down the column (`fx_ketchup_nozzle/stream`); Mustard = nozzle burst in place
  (`fx_mustard_nozzle`) with the authored drop as its faller (`fx_mustard_drop`); Skewer = the
  full stick falls tip-first (`fx_skewer_falling`, `FallerImpactLift` seats the tip on the
  floor), then only its **head stays pinned** at the base for the went-through depth read
  (`fx_skewer_head`). Sizes `UIStyles.FX_*`, timings `AnimConfig.FX_*`, sorts
  `SORT_CONSUMABLE_FX_*` (between ghost and faller).
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
- **Reskin the game** = edit the slot's asset in `Resources/Skins/` (set its **Sprite** field; for
  `bun_default` also **Secondary Sprite** = bottom bun, for `chef_default` = flipped facing), or
  just replace a PNG's contents keeping its filename. Works from the Project window with any scene
  open — no more opening `Game.unity`.
- **Purchasable skins (live)**: `meat_alt` ("Deluxe Patty", 500★), `chef_happy` ("Happy Dogtor",
  800★), `chef_alt` ("Dogtor Deluxe", 1000★) — non-default Skin assets with `_unlock: Stars` +
  `_starCost`, sold and equipped in the Shop. Chef alts reuse their front sprite as the
  flipped-facing secondary (mirrored on flip) until flipped art exists. **Adding a shop skin =
  authoring one Skin asset** (id, slot, sprite, star cost) — no code.
- **Status**: selection + star-unlock shipped with the Shop (2026-07-05). Gem/IAP-priced skins and
  Pack bundles remain unbuilt (`UnlockMethod.Gems/Iap` exist; `ShopService.TryBuySkin` already
  handles Gems).

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
The broken per-component setters were removed from `UIFactory.AddStyledText`. World-space
(`WorldTextFactory`) still uses the old setters — migrate the same way if world outlines are wanted.

### UI Text Color Convention
- **HUD numbers + all red-box labels** (Level/Score numbers, tab words, SPECIAL ORDER banner,
  mult badge, consumable counts): the reference palette — cream fill `#FCFAF1` + dark-brown border
  `#492611` via `UIFactory.StyleHudText` (`UIStyles.HUD_TEXT_*`).
- Top-bar currency pill numbers: plain dark brown (`TOPBAR_NUMBER_COLOR`), no border.
- Buttons/popups/panels (menu): white text.
- World-space popups: white (world outline still pending — see above).

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
- **Consumable/reward sprites** live in `Resources/Fairy/` + `Resources/Rewards/` (loaded by name via
  `RewardArt`, outside the `Theme`/Skins pipeline). They must import as **Single** sprite mode — the
  project's default is **Multiple**, which auto-slices multi-blob images into fragments and breaks
  `Resources.Load<Sprite>` (it returns only the first fragment). `SpriteFit` sizes them by world-height,
  so PPU / source size doesn't matter.
- **Sizing gameplay sprites (ingredients/chef/plate)**: they render at `localScale=1`, so on-screen size =
  `pixelWidth / spritePixelsToUnits`. Normalise by setting per-file `spritePixelsToUnits = pixelWidth /
  targetWorldWidth` in the `.png.meta` (no pixel editing, no distortion — PPU scales both axes equally).
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
- **Chef size/position tuning**: knobs are the chef sprite **PPU** (currently 2009 = ~33% of the original
  import) for size, and `Constants.CHEF_BOTTOM_OFFSET` (1.76) for the feet line — `GetWorldPosition` anchors
  the feet and derives the centre from the live sprite height, so resizing keeps the chef on the bottom border.
- **Verify skin import (Phase 1)**: open Unity, confirm a clean compile and that `Resources/Skins/*.asset`
  each show their sprite (not "None"); the game should look identical to before.
- **Before release: lower `SETTINGS_LEVEL_CAP` to `MAX_LEVEL`** (`GameplayConfig`). It's currently
  `KILLER_LEVEL` (21) so the kill screen is reachable from the Settings stepper for testing; players
  should not be able to *start* on the kill screen. One-line flip (comment marks it).

## Pending Features
- **HUD done so far** (authored, screen-space UGUI): top bar (currencies + shop/settings buttons),
  Level/Score cards, the 3-slot consumable row, the Special Order panel. The HUD scales with the
  camera (both frame by width — see Camera & UI scaling). **Boxes are baked art at native aspect**
  (the 9-slice route was dropped — fixed-size HUD boxes don't need it).
- **★ glyph**: Panton (ASCII) lacks U+2605; add a fallback font or the `Star` sprite where needed.
- **UI integration ≠ pure art-swap** — remaining wiring that implies real code:
  - **Mult meter**: a filling capsule gauge (right of Special Order) showing progress to the next
    challenge level. **Built** (`BurgerChallengeView.BuildMultMeter`, `ChallengeFill`) — slot
    position/size are eyeball defaults in `UIStyles.MULT_METER_*`, tune live.
  Plan each of these as its own code task alongside the visual wiring.
- **Shop UI is placeholder-styled** (flat color cards/buttons + authored pills/icons) — restyle
  with authored art when the kit grows shop pieces. Layout knobs in `UIStyles.SHOP_*`, untested
  on-device — eyeball defaults, tune live.
- **IAP**: gem packs + Remove Ads are stubs granting instantly (`ShopService`); need the real IAP
  SDK + a **Restore Purchases** path for Remove Ads (iOS review requirement).
- Consumable polish: real SFX (override slots ready) + final slot layout/sizes (placeholders in `UIStyles`)
- Leaderboard integration (button exists, logs "Coming Soon")
- IAP integration (ShopService stubs grant instantly; see Shop section)
- Ad SDK integration (AdManager is placeholder)

## Pre-Launch Checklist
Platform-readiness / launch-logistics items tracked separately from
code-review findings. See `Docs/pre-launch-checklist.md` for the full
list (save layer security, cloud save, schema versioning, IAP receipt
validation, analytics, privacy policy, etc.).
