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
  Chef/          ChefController
  Core/          GameManager, GameState, Constants, GameplayConfig, MonetizationConfig,
                 UIStyles, AnimConfig, DifficultyManager, SaveDataManager, ControlMode,
                 FeedbackManager, Rng, SceneLoader, Singleton<T> (manager base)
  Grid/          GridManager, Column, MatchDetector, MatchResult, BurgerAnimator, BurgerData
  Ingredients/   Ingredient, IngredientState, IngredientSpawner, SpawnerState,
                 IngredientType, WavePreviewManager
  Input/         TouchInputHandler
  Scoring/       Scoring (points/tiers), BurgerTier, BurgerNamer
  Skins/         Skin (ScriptableObject), SkinSlot, UnlockMethod, SkinMap, Theme (static accessor)
  UI/            MainMenuUI, GameHUD, GameOverPanel, SettingsPanel, ShopPanel,
                 BurgerChallenge, BurgerPopup, FloatingText, ScorePopup,
                 Background, GameLayout, OrderType, UIFactory
    Factory/     SpriteFactory (cached procedural sprites), WorldTextFactory (world-space TMP)
  Audio/         AudioManager, MusicManager, MusicCategory
  Monetization/  AdManager, GemPack, GemProduct, GemPackSpawner
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

## Core Systems

### Wave-Based Spawning (IngredientSpawner)
- A wave = 2 ingredients (3 on a triple roll; triple chance starts at level 8, up to 35% at level 20), each in a distinct column.
- **Standing preview queue**: a target number of upcoming pieces (= the next wave's size) is always reserved and shown as blinking ghosts, one per column. Topped up every frame (`TopUpPreviews`) and seeded at `StartSpawning` (hidden through the initial delay). The reserved previews **are** the next wave — when the current wave lands, they become the real pieces, dropping in their columns.
- **Next-wave trigger**: fires only when the ORIGINAL auto-spawned wave lands. Tapped previews are "fire and forget" (not counted toward wave completion), so a desired preview can't be frozen by tapping the others — a held preview force-drops within ~one fall.
- **Tap a preview** to spawn that ingredient immediately; only revealed (visible) previews are tappable.
- **Column choice is unbiased** (random, one preview per column, height/stack ignored). A per-preview clearance (`AnimConfig.PREVIEW_SPAWN_CLEARANCE`) delays only the ghost's visual reveal so it never overlaps a falling sprite — it does NOT influence which column is chosen (`ColumnsWithPieceInPreviewZone` + `WavePreviewManager.RevealCleared`).
- Forced bun spawn threshold: `activeCount * FORCED_BUN_MULTIPLIER` (1.5); unified pool — bun is one extra slot in the random range.
- `WaveComposer` owns ingredient-type + bun-pacing rolls (`RollSlot` / `RollWaveSize`); `IngredientSpawner` owns column selection (the queue).

### Bun Type Selection (GetBunType)
- No bottom bun on grid: always BunBottom
- Otherwise: `topChance = 0.5 + bottomCount * 0.08` (capped 0.8)

### Controls (TouchInputHandler)
Two modes, configurable in Settings (saved via PlayerPrefs):
- **Drag**: Swipe = move chef, Tap = swap plates, Tap falling = fast-drop, Tap preview = spawn
- **Tap**: Tap near chef = swap, Tap left/right of chef = move, Swipe = move, Tap falling = fast-drop, Tap preview = spawn

### Difficulty (DifficultyManager)
- 20 levels scaling fall speed and active ingredient count
- Level 1: 3 ingredients, 0.5s fall step; Level 20: 7 ingredients, 0.1s fall step
- Thresholds in `GameplayConfig.LEVEL_THRESHOLDS` — slow start (gap of 10), ramp up (gap grows by 2 per level)
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
- **AudioManager**: All SFX procedurally generated (sin waves, envelopes, harmonics). No audio asset files
- **MusicManager**: Loads tracks from Resources/Music/. Random selection per scene

### Monetization
- Gems currency (earned via ads, gem packs, IAP)
- Continue after game over: 50 gems or watch ad
- Interstitial ads every 3 games
- Gem pack drops during gameplay (8% chance every 10s)
- AdManager is currently mock (simulated delays). IAP buttons grant gems for testing

### Skins & Theme (cosmetics)
All gameplay sprites flow through one place: `Theme` (static) reads `Skin` ScriptableObject
assets from `Resources/Skins/` and serves the active sprite per `SkinSlot`. Consumers
(`IngredientSpawner`, `ChefController`, `Background`) call `Theme.Ingredient(type)` /
`Theme.Chef` / `Theme.Background(type)` — there is **no** per-scene sprite wiring anymore.
- **Slots** (`SkinSlot`, suffixed `…Skin`): the 7 ingredients + `BunSkin` (carries top **and**
  bottom — the one slot with two sprites) + `ChefSkin` + `GameBackgroundSkin` + `MenuBackgroundSkin`.
  `SkinMap.SlotFor(IngredientType)` maps ingredients → slot (both buns collapse to `BunSkin`).
- **Reskin the game** = edit the slot's asset in `Resources/Skins/` (set its **Sprite** field; for
  `bun_default` also **Secondary Sprite** = bottom bun), or just replace a PNG's contents keeping its
  filename. Works from the Project window with any scene open — no more opening `Game.unity`.
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

## Pending Manual Steps
- **Verify skin import (Phase 1)**: open Unity, confirm a clean compile and that `Resources/Skins/*.asset`
  each show their sprite (not "None"); the game should look identical to before.
- **Assign placeholder sprite**: In Unity Inspector, select BurgerChallenge component → set `_spritePlaceholder` field to the silhouette PNG in `Assets/_Project/Sprites/Ingredients/`

## Pending Features
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
