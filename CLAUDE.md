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
                 FeedbackManager, Rng, SceneLoader, Singleton<T> (manager base)
  Grid/          GridManager, Column, MatchDetector, MatchResult, BurgerAnimator, BurgerData
  Ingredients/   Ingredient, IngredientState, IngredientSpawner, SpawnerState,
                 IngredientType, WavePreviewManager, WaveComposer, WaveSlot, IngredientBag
  Input/         TouchInputHandler
  Scoring/       Scoring (points/tiers), BurgerTier, BurgerNamer
  Skins/         Skin (ScriptableObject), SkinSlot, UnlockMethod, SkinMap, Theme (static accessor)
  UI/            MainMenuUI, GameHUD, GameOverPanel, SettingsPanel, ShopPanel,
                 BurgerChallenge, BurgerPopup, FloatingText, ScorePopup,
                 Background, GameLayout, OrderType, UIFactory
    Factory/     SpriteFactory (cached procedural sprites), WorldTextFactory (world-space TMP)
  Audio/         AudioManager, MusicManager, MusicCategory
  Monetization/  AdManager, GemProduct, BurgerFairy, BurgerFairySpawner
  Consumables/   ConsumableType, ConsumableEffect (+ Ketchup/Mustard/Skewer), ConsumableEffects,
                 ConsumableFaller, ConsumableInventory, ConsumableInventoryView,
                 ConsumableDragController, ConsumableSlotLayout, FairyPayload, FairyPayloadKind,
                 RewardArt, SpriteFit
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
8. **ConsumableInventory** -- per-run consumable slots (2, FIFO); created by GameManager
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
- `SaveDataManager.OnGemsChanged(int)` -- currency updates
- `ConsumableInventory.OnChanged` -- consumable slots changed (add / consume)

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
**1/2/3** grant Ketchup/Mustard/Skewer to the inventory (`#if UNITY_EDITOR`).

**Input-area debug gizmos** (editor-only, `#if UNITY_EDITOR`): each clickable zone is drawn by the
component owning its hit-test, one color per interaction (`GizmoStyles`): falling fast-drop (green,
`IngredientSpawner`), preview tap (yellow, `WavePreviewManager`), chef flip circle (magenta) + move
sides (cyan, `TouchInputHandler` — mode-aware, Tap mode only), fairy tap (orange, `BurgerFairy` —
play-mode only, runtime-spawned). Toggle per-script via Unity's Gizmos menu.

The Settings panel also has a **Start Level** stepper (`[−] Start Level: N [+]`) → persists
`SaveDataManager.StartingLevel`; clamped 1..`SETTINGS_LEVEL_CAP`. See Difficulty.

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
- Panel shows "Special Order!" title, requirement text, and visual with silhouette placeholder sprites
- Silhouette sprite (`_spritePlaceholder` SerializeField) with text overlays ("+N" or "?")
- On match: popup shows "Order Complete!" instead of generated burger name (via GridManager `IsOrderMatch` check)
- 3x challenge multiplier on match; global multiplier: `1 + (level - 1) * 5`
- Level up requires `level + 1` matches; star label "★ X" (distinguishes from difficulty level)

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

### Audio
- **AudioManager**: All SFX procedurally generated (sin waves, envelopes, harmonics). No audio asset
  files. Each sound has an optional `_*Override` clip slot for authored audio. Consumable hooks:
  `PlayConsumableCollect` / `PlayConsumableUse(type)` / `PlayConsumableFizzle` (placeholder tones).
- **MusicManager**: Loads tracks from Resources/Music/. Random selection per scene

### Monetization
- Gems currency (earned via ads, Burger Fairy drops, IAP)
- Continue after game over: 50 gems or watch ad
- Interstitial ads every 3 games
- **Burger Fairy** drops during gameplay (`FAIRY_SPAWN_CHANCE` 0.20 / 10s) carry gems (~40%) or
  a consumable (~60%) — see Core Systems → Consumables. Gem fairies award `GEM_PACK_VALUE`.
- AdManager is currently mock (simulated delays). IAP buttons grant gems for testing

### Consumables ("Burger Fairy" deliveries)
Per-run consumable items delivered by fairies; drag onto a column to use. Design doc:
`Docs/consumables-design.md`. **Feature-complete.**
- **Delivery**: `BurgerFairy` (replaced the old GemPack) flies across the screen carrying a
  **payload** — gems or a consumable (`FAIRY_CONSUMABLE_CHANCE` 0.60; which one per
  `CONSUMABLE_SPAWN_WEIGHTS`, even thirds). `BurgerFairySpawner` rolls it. Tap to collect
  (routed **first** in `ProcessInput`, above preview/falling, since it's on top of the playfield).
  Gems → `AddGems`; consumable → `ConsumableInventory`.
- **Inventory** (`ConsumableInventory`): per-run, **2 slots, FIFO** (a 3rd collect evicts the
  oldest); not persisted. `ConsumableInventoryView` renders world-space icons in the score panel.
- **Use — drag-to-column**: press a slot icon → carry (icon follows finger, a translucent ghost
  snaps to the nearest column) → release **over the playfield** to use, **off it** to cancel.
  Owned by `ConsumableDragController`, driven by `TouchInputHandler` (origin disambiguates: a
  press on a slot becomes a carry and suppresses chef gestures). **World keeps moving while
  carrying** — a cancellable pause would be a free stop-time exploit.
- **Faller + effects**: on release a `ConsumableFaller` drops fast and **resolves on impact**;
  reaching the floor with no target **fizzles** (item still spent). Each `ConsumableEffect`
  supplies a target rule + on-impact behavior (polymorphic, no switch) calling granular
  `GridManager` helpers:
  - **Ketchup** → clears the whole targeted column (`ConsumableClearColumn`).
  - **Mustard** → removes the targeted column's top type **board-wide** (`ConsumableSweepType`),
    per-column collapse + cascade (chain reactions reuse the normal pair-match loop).
  - **Skewer** → drives one `BunBottom` to row 0, destroys the rest, regulars collapse on top
    (`ConsumableSkewer`).
- **Scoring**: non-bun removals score `POINTS_CONSUMABLE_PER_INGREDIENT` (10), flat, no
  multiplier; **buns score nothing** (Ketchup-cleared and Skewer-destroyed alike). Cascades
  score normally via `OnMatchEliminated`.
- **Art** (`RewardArt` + `SpriteFit`): one badge per payload from `Resources/Rewards/`
  (`gem`/`ketchup`/`mustard`/`skewer`) doubles as fairy badge, inventory icon, column ghost
  (alpha) and faller; the carrier from `Resources/Fairy/fairy`. `SpriteFit.Height` normalizes
  every sprite to a world-height so source PPU/size doesn't matter. Sizes/positions/sorts live in
  `UIStyles` (`*_HEIGHT`, `CONSUMABLE_SLOT_*`) and `Constants` (`SORT_CONSUMABLE_*`).
- **Status**: audio is placeholder procedural tones on the hooks (`PlayConsumableCollect` /
  `PlayConsumableUse(type)` / `PlayConsumableFizzle`) — real sound design deferred. Slot
  layout/sizes are placeholder; eyeball-tune via `UIStyles`. Editor debug: keys **1/2/3** grant
  Ketchup/Mustard/Skewer.

### Skins & Theme (cosmetics)
All gameplay sprites flow through one place: `Theme` (static) reads `Skin` ScriptableObject
assets from `Resources/Skins/` and serves the active sprite per `SkinSlot`. Consumers
(`IngredientSpawner`, `ChefController`, `Background`) call `Theme.Ingredient(type)` /
`Theme.Chef` / `Theme.Background(type)` — there is **no** per-scene sprite wiring anymore.
- **Slots** (`SkinSlot`, suffixed `…Skin`): the 8 ingredients + `BunSkin` + `ChefSkin` +
  `PlateSkin` + `GameBackgroundSkin` + `MenuBackgroundSkin`. Two slots carry **two sprites**:
  `BunSkin` (top **+** bottom bun) and `ChefSkin` (front **+** flipped facing; `Theme.Chef` /
  `Theme.ChefFlipped`). `SkinMap.SlotFor(IngredientType)` maps ingredients → slot (both buns
  collapse to `BunSkin`).
- **Chef flip**: `ChefController.SwapPlates` keeps the 3D Y-rotation but swaps the SpriteRenderer
  Front↔Flipped at the edge-on midpoint (+ toggles `flipX` to cancel the 180° mirror so the
  Flipped art reads un-mirrored). Single renderer, so no second sprite to hide.
- **Plates** (`PlateManager`, `Theme.Plate`): four decorative plates, one under each column
  (`Constants.PLATE_Y_OFFSET` below row 0), purely cosmetic. Sort: `SORT_PLATE (-2)` at the back,
  ingredients (0+), then `SORT_CHEF (50)` — the chef renders **over the plates and the ingredients**
  (intentional final look). The bottom ingredient still sits on the plate. On a flip the two active
  plates **slide** to swap columns (`PlateManager.SwapColumns`, position-only DOMove — sprite never flips).
- **Reskin the game** = edit the slot's asset in `Resources/Skins/` (set its **Sprite** field; for
  `bun_default` also **Secondary Sprite** = bottom bun, for `chef_default` = flipped facing), or
  just replace a PNG's contents keeping its filename. Works from the Project window with any scene
  open — no more opening `Game.unity`.
- **Spare art ready for the catalog**: `meat_alt`, `chef_happy`, `chef_alt` (renamed, not yet wired).
- **Status**: Phase 1 only = one default skin per slot (`_isDefault = true`). Runtime *selection* and
  *unlock/buy* are not built yet — see the Skin System roadmap entry.

## Randomness
All randomness uses `Rng` static class, never `UnityEngine.Random`:
```csharp
Rng.Range(0, max)      // int, exclusive max
Rng.Range(0f, 1f)      // float
Rng.Value              // float 0-1
```

## Known Issues

### Text Outline Rendering (UNRESOLVED)
Setting `tmp.outlineWidth`/`tmp.outlineColor` after `AddComponent<TextMeshPro>()` doesn't reliably render outlines. Shader keyword `OUTLINE_ON` isn't enabled and material may not be initialized. Current workaround: black text color for readability.

**Fix when ready:**
1. Enable `ShaderUtilities.Keyword_Outline` ("OUTLINE_ON") on the material
2. Wait one frame for TMP to initialize before modifying `fontMaterial`
3. Call `tmp.UpdateMeshPadding()` and `tmp.ForceMeshUpdate()`

### UI Text Color Convention
- Main Menu + In-game HUD: black text
- Buttons/popups/panels: white with outline (when outline fix is applied)
- World-space popups: white with outline

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
- **Visual** — the text-outline fix (see Known Issues), readability/contrast, the placeholder sprite (`UIStyles`)

### Phase 3 — Backlog (potential next work; noted 2026-06-12, not yet started)
Dev-flagged directions for future sessions, roughly in priority order. Nothing here is designed
or committed yet — capture only.
- **Gem & Star economy** — make the currencies real: where each is earned/spent, balancing,
  persistence. Gems already exist (`SaveDataManager`, `OnGemsChanged`); the challenge "★" is
  currently just challenge progress — decide whether it becomes a spendable/meta currency.
- **Shop** — flesh out `ShopPanel` against the real economy (today the IAP buttons just grant
  gems for testing). Ties into the gem economy + IAP + (deferred) skin unlocks.
- **Prepare for real assets** — replace placeholder art across the game. Pipeline is ready
  (`Theme` / `Resources/Skins` for gameplay art, `RewardArt` for consumables) — mostly a content swap.
- **Prepare for real UI components** — UI is currently code-built/procedural (`UIFactory`,
  `WorldTextFactory`, `SpriteFactory`); move toward authored UI components / polish, plus the
  text-outline fix (see Known Issues).
- **Full advertisement implementation** — replace the mock `AdManager` with a real ad SDK
  (rewarded continue, interstitials every 3 games, gem-pack rewards).
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
- **Phases 2-3 (DEFERRED)** — the cosmetic *feature* (runtime per-slot selection + persistence, then
  unlock/buy flows via gems/IAP/ad, a Skins UI, and Pack bundles) is intentionally parked. The foundation
  is built to support it (`UnlockMethod`, `_isDefault`, per-slot model), so it can be picked up later if
  cosmetic monetization is wanted — but it is **not** active work.

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
- **★ glyph in `ChallengeLevel`**: LiberationSans SDF lacks U+2605, so the challenge star renders as □
  (harmless warning). Fix later via a sprite/glyph swap or a font with the star.
- **Verify skin import (Phase 1)**: open Unity, confirm a clean compile and that `Resources/Skins/*.asset`
  each show their sprite (not "None"); the game should look identical to before.
- **Assign placeholder sprite**: In Unity Inspector, select BurgerChallenge component → set `_spritePlaceholder` field to the silhouette PNG in `Assets/_Project/Sprites/Ingredients/`
- **Before release: lower `SETTINGS_LEVEL_CAP` to `MAX_LEVEL`** (`GameplayConfig`). It's currently
  `KILLER_LEVEL` (21) so the kill screen is reachable from the Settings stepper for testing; players
  should not be able to *start* on the kill screen. One-line flip (comment marks it).

## Pending Features
- Consumable polish: real SFX (override slots ready) + final slot layout/sizes (placeholders in `UIStyles`)
- Skin selection + unlock/buy UI (DEFERRED — foundation done; see Skin System roadmap)
- Text outline shader fix
- Leaderboard integration (button exists, logs "Coming Soon")
- IAP integration (buttons exist, currently grant gems for testing)
- Ad SDK integration (AdManager is placeholder)

## Pre-Launch Checklist
Platform-readiness / launch-logistics items tracked separately from
code-review findings. See `Docs/pre-launch-checklist.md` for the full
list (save layer security, cloud save, schema versioning, IAP receipt
validation, analytics, privacy policy, etc.).
