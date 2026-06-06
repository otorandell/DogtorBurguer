# Code Review — Notes

Chronological log of the review. Per-file/area headings as we go.
See [`README.md`](README.md) for conventions + the live index of findings.

---

## Session 2026-05-26 — kickoff

Folder scaffolded. Review order: alphabetical by folder
(`Audio/` → `Burger/` → `Chef/` → `Core/` → `Grid/` →
`Ingredients/` → `Input/` → `Monetization/` → `Scoring/` → `UI/`).

Note: `Burger/` and `Scoring/` exist on disk but aren't documented in
`CLAUDE.md`'s "Project Structure" section. Worth reconciling when we
reach those folders.

---

## `Audio/AudioManager.cs`

### F-1 — `EnsureComponent<T>` helper should be reusable
**Found in:** `Core/GameManager.cs:200-207` (helper); `UI/MainMenuUI.cs:18-31` (re-implementation)
**What:** `GameManager.EnsureComponent<T>()` is `private` — `MainMenuUI` re-implements the same "spawn if missing" logic inline for SaveDataManager / AdManager / MusicManager.
**Severity:** design-quality
**Status:** noted
**Next step:** lift `EnsureComponent<T>` into a `Core/MonoBehaviourUtil` static helper, make it `public static`. Replace `MainMenuUI:18-31` with three calls. ~10 lines removed; no behavior change.

### F-2 — No path from procedural clips to authored audio assets
**Found in:** `Audio/AudioManager.cs:13-25` (clip fields), `48-52` (sources), `114-129` (generation)
**What:** Every clip is generated procedurally with no escape hatch. To swap in a real `.wav`, all 13 field declarations and the `GenerateClips()` ritual would need rewriting.
**Severity:** design-quality
**Status:** fix-now
**Next step:** add a `[SerializeField] AudioClip _xxxClipOverride` parallel to each generated `_xxxClip`. In `GenerateClips()` (or a unified accessor), prefer the override if assigned, else fall back to the generated clip. Procedural code stays as the default; assets become opt-in per slot.

### F-3 — `HandleBurger` if-elseif chain → switch expression
**Found in:** `Audio/AudioManager.cs:83-94`
**What:** Linear if-elseif on `ingredientCount` thresholds reads more naturally as a C# 8+ switch expression with relational patterns (CLAUDE.md is C# 10+).
**Severity:** cosmetic
**Status:** noted
**Next step:** replace with:
```csharp
AudioClip clip = ingredientCount switch
{
    >= 9 => _burgerMaxClip,
    >= 7 => _burgerMegaClip,
    >= 5 => _burgerLargeClip,
    >= 3 => _burgerMediumClip,
    >= 1 => _burgerSmallClip,
    _ => _burgerPoorClip,
};
```
Pairs naturally with the burger-tier-thresholds concern (see open question below) — if a shared `GetBurgerTier(int)` helper lands, this collapses to one line.

### F-4 — Sample-generation skeleton repeats across `Generate*Samples`
**Found in:** `Audio/AudioManager.cs:149-375` (13 methods)
**What:** Three skeleton patterns repeat:
- **Arpeggio + envelope** (8 methods: `BurgerPoor/Small/Medium/Large/Mega/Max`, `LevelUp`, `GameOver`): `notes[] = {...}; noteLength = duration / notes.Length; noteIndex = floor(t/noteLength); envelope; sin sum`
- **Frequency sweep + envelope** (4 methods: `Match`, `Squeeze`, `FastDrop`, `EarlySpawn`): `freq = Lerp(start, end, t/duration); envelope; sin`
- **Harmonic stack** (5 methods): adds `Sin(freq*N*t)*wN` layers on the fundamental.
**Severity:** design-quality
**Status:** noted (gated on F-2 staying option A; dissolves if procedural code is fully replaced by authored assets)
**Next step:** extract `SfxHelpers.Arpeggio(notes, t, duration, envelopeShape)`, `SfxHelpers.Sweep(startFreq, endFreq, t, duration, envelopeShape)`, `SfxHelpers.Harmonics(freq, t, weights)`. Each `Generate*Samples` shrinks to 1-3 lines. Skeleton lives once.

### Open question — burger tier thresholds duplicated with scoring?
`HandleBurger` thresholds (`>= 9, 7, 5, 3, 1`) look like they mirror the scoring tiers CLAUDE.md mentions ("5 (poor) to 500 (9+ ingredients)"). Deferred: confirm when we walk `BurgerAnimator`. If confirmed, the fix is a shared `BurgerTier GetBurgerTier(int count)` consumed by both audio and scoring — would simplify F-3 too.

### Discarded
- **SubscribeEvents / OnDestroy duplication** — considered, discarded 2026-05-26. ~7 cosmetic lines, no real bug, no future-bug prevention. Per anti-speculation principle in global CLAUDE.md.

---

## `Audio/MusicManager.cs`

### F-5 — Two category bools should be a `MusicCategory` enum
**Found in:** `Audio/MusicManager.cs:13-14, 66-69, 76-77`
**What:** `_playingMenuCategory` and `_playingGameCategory` encode one piece of state ("which category is currently playing") as two tautologically-opposed booleans. Line 76-77 (`_playingMenuCategory = isMenu; _playingGameCategory = !isMenu;`) makes the mutual exclusion explicit — nothing structural enforces it. Direct hit on CLAUDE.md's "Explicit state machines — never use multiple boolean flags to represent state. Use an enum with named states."
**Severity:** design-quality
**Status:** noted
**Next step:** introduce `private enum MusicCategory { None, Menu, Game }` + single `private MusicCategory _currentCategory` field. `PlayTrackForCurrentScene` becomes:
```csharp
MusicCategory target = isMenu ? MusicCategory.Menu : MusicCategory.Game;
if (_currentCategory == target && _source.isPlaying) return;
// ... pick + play ...
_currentCategory = target;
```
One field, mutual exclusion enforced by type.

### F-6 — Default music volume `0.5f` is an inline magic number
**Found in:** `Audio/MusicManager.cs:30`
**What:** `_source.volume = 0.5f` in Awake. Mix level (balance constant) inline at initialization site rather than in config.
**Severity:** cosmetic / design-quality
**Status:** noted
**Next step:** move to `Core/UIStyles.cs` or `Core/GameplayConfig.cs` as `MusicConfig.DefaultVolume = 0.5f` (or wherever audio-mix constants want to live — possibly a future `AudioConfig` once F-4's per-clip volume table lands). Same family as F-4 (per-clip mix volumes inline at call sites).

### Other observations (not findings)

- **`Start` + `OnSceneLoaded` both call `PlayTrackForCurrentScene`** — looks redundant but isn't. `sceneLoaded` doesn't fire for the scene already loaded when subscribed; `Start` is the bootstrap for the initial scene, `OnSceneLoaded` handles subsequent transitions. Standard Unity pattern.
- **Tracks loaded eagerly** in Awake (both categories at once, kept in memory via `DontDestroyOnLoad`). Fine at arcade-game scale; revisit only if track count grows large.
- **`Resources.LoadAll<AudioClip>("Music/MenuTrack")`** — path strings could become consts paired with `SceneLoader.SCENE_MAIN_MENU`, but not impactful at this scale.

---

## `Burger/` and `Scoring/`

Both folders are empty (only `.meta` files; no `.cs` files; no git history of any tracked content under them). They're tracked placeholders that have never held code.

### F-7 — Empty `Burger/` and `Scoring/` placeholder folders
**Found in:** `Assets/_Project/Scripts/Burger/` (empty); `Assets/_Project/Scripts/Scoring/` (empty)
**What:** Two folders exist with `.meta` files committed to git, but no source files have ever lived inside them. CLAUDE.md doesn't document them either (lists 8 folders; on-disk has 10). Speculative structure with no current consumer.
**Severity:** cosmetic
**Status:** noted
**Next step:** delete both folders + their `.meta` files. If a later finding's implementation extracts code into `Burger/` or `Scoring/`, that finding's commit recreates the folder + adds the CLAUDE.md "Project Structure" entry at the same time. Default is delete because nothing today justifies the placeholder (anti-speculation principle).

---

## `Chef/ChefController.cs`

### F-8 — `_startPosition` SerializeField never varies; collapse to constant
**Found in:** `Chef/ChefController.cs:10, 41`
**What:** `[SerializeField] private int _startPosition = 1;` is read exactly once in `Start()` (line 41: `_currentPosition = Mathf.Clamp(_startPosition, 0, ...)`) and never assigned anywhere else. The designer-tweakable surface exists but is never tweaked — dead flexibility.
**Severity:** cosmetic / design-quality
**Status:** noted
**Next step:** delete the SerializeField; replace usage with `Constants.CHEF_START_POSITION = 1` (or similar — pick a name that says "default middle position"). The `Mathf.Clamp` becomes redundant against a known-good constant — delete it too. If start position IS supposed to vary per level/mode in future, re-add at that time per CLAUDE.md anti-speculation principle.

### F-9 — Position bubbles likely effectively invisible
**Found in:** `Chef/ChefController.cs:118-148`; `Core/UIStyles.cs:113-116`
**What:** Bubbles are created (3 white circles, one per chef position) but:
- Both colors are highly transparent (`BUBBLE_INACTIVE` alpha 0.25, `BUBBLE_ACTIVE` alpha 0.45)
- `sortingOrder = -1` (renders behind everything including the chef sprite)
The active bubble is likely entirely hidden under the chef; inactive bubbles at other positions are at most barely-visible white blobs. User reports not seeing them at runtime.
**Severity:** design-quality
**Status:** noted
**Next step:** make the bubbles actually visible. In `UIStyles.cs`, raise alpha on both colors (`BUBBLE_INACTIVE` from `0.25f` → `0.6f`, `BUBBLE_ACTIVE` from `0.45f` → `0.85f`). In `CreatePositionBubbles:133`, change `sr.sortingOrder = -1` to a value that renders the bubble in front of the background but behind the chef (likely `0` with the chef on a higher order — confirm chef's sortingOrder when applying). Verify in-game post-change: inactive bubbles at unoccupied positions clearly visible; active bubble visibly different on the chef's position.

### F-10 — `SwapPlates` flip-snap needs explanatory comment
**Found in:** `Chef/ChefController.cs:98-105`
**What:** The pre-tween snap (`transform.rotation = Quaternion.Euler(0, _isFlipped ? 180f : 0f, 0)`) exists to prevent partial-rotation artifacts when SwapPlates is called mid-flip — without the snap, the new tween would start from an in-between rotation and look weird. The intent is non-obvious from reading the code.
**Severity:** cosmetic
**Status:** noted
**Next step:** add a one-line comment: `// Snap to logical state first — prevents weird interpolation if called mid-flip`. Pure documentation, no behavior change.

### Tracking (not yet a finding)

- **Defensive `if (_x == null) _x = GetComponent<X>()` pattern** in Awake — `ChefController:33-36` is the first instance. Likely repeats in `BurgerAnimator`, possibly others. Will formalize as a codebase-wide finding (replace with `[RequireComponent(typeof(X))]` + unconditional `GetComponent`) once a second instance confirms the pattern.

### Other observations (not findings)

- **`_currentPosition` vs `_startPosition`** distinction (initial seed vs live state) is correct shape — answered as a question, not a finding (F-8 addresses the dead-flexibility concern separately).
- **`_moveTween` / `_flipTween` storage + kill** follows DOTween best practices (kill before re-assignment, kill in OnDestroy per CLAUDE.md). Correct.

---

## `Core/AnimConfig.cs`

Verified: all 65 constants are consumed somewhere — no dead entries.

### F-11 — Inline `0.4f` duration in `BurgerChallenge` should be a named AnimConfig const
**Found in:** `UI/BurgerChallenge.cs:468`
**What:** `sr.DOColor(original, 0.4f);` — inline duration literal for restoring sprite color after the level-up flash. Doesn't match any existing AnimConfig const (closest is `LEVELUP_FADE_COLOR_DURATION = 0.2f`, but that's the fade-TO direction; this fades BACK). Looks like a missed extraction during the AnimConfig consolidation pass.
**Severity:** design-quality
**Status:** noted
**Next step:** add `LEVELUP_COLOR_RESTORE_DURATION = 0.4f` to AnimConfig (separate from `LEVELUP_FADE_COLOR_DURATION = 0.2f` since the two directions have already diverged in value — fade-to is snappier, fade-back is gentler). Replace the inline literal at `BurgerChallenge.cs:468`.

### F-12 — `LAND_PUNCH_ELASTICITY` is misnamed (used in wave punches too)
**Found in:** `Core/AnimConfig.cs:13`; consumed at `Ingredients/Ingredient.cs:141, 179, 197`
**What:** Despite the `LAND_` prefix promising landing-only use, this constant is referenced from wave-punch tweens at lines 179 and 197 (both in wave effect code). The name lies about its scope.
**Severity:** cosmetic / design-quality
**Status:** noted
**Next step:** add `WAVE_PUNCH_ELASTICITY = 0.5f` to AnimConfig (initial value matches `LAND_PUNCH_ELASTICITY` for behavior continuity). Update `Ingredient.cs:179, 197` to reference the new wave-specific const. Follows the existing pattern for paired land/wave punch constants (`LAND_PUNCH_DURATION` / `WAVE_PUNCH_DURATION` already separate per the 2026-05-26 decision).

### Decided — paired identical-value constants kept separate (2026-05-26)
Three pairs declare the same value under context-specific names:

| Pair | Value | Consumers |
|---|---|---|
| `POPUP_RISE_DISTANCE` / `FLOATING_TEXT_RISE` | both `1.5f` | ScorePopup vs FloatingText |
| `POPUP_DURATION` / `FLOATING_TEXT_DURATION` | both `0.8f` | ScorePopup vs FloatingText |
| `LAND_PUNCH_DURATION` / `WAVE_PUNCH_DURATION` | both `0.2f` | Ingredient land vs wave |

**Decision:** keep separate. Intentional independent tuning surface — designer can diverge any pair without touching the other context.

### Tracking (not yet a finding)

- **`Constants.INITIAL_FALL_STEP_DURATION` and `Constants.MIN_FALL_STEP_DURATION`** — subsumed by F-14 (the broader Constants.cs structural-vs-balance split).

---

## `Core/Constants.cs`

24 constants. All used except one (CELL_HEIGHT). The bigger issue is categorical mixing.

### F-13 — `CELL_HEIGHT` is unused (dead constant)
**Found in:** `Core/Constants.cs:11`
**What:** `public const float CELL_HEIGHT = 1.0f;` — declared but never referenced. Grep confirms only the declaration site. `CELL_VISUAL_HEIGHT = 0.40f` is the actual cell rendering height (with the comment "60% overlap").
**Severity:** cosmetic
**Status:** noted
**Next step:** delete the `CELL_HEIGHT` line. No consumer, no documented relationship to other constants. Anti-speculation principle: re-add if a real consumer appears. The existing inline comment on `CELL_VISUAL_HEIGHT` ("60% overlap between rows") already documents the intent.

### F-14 — `Constants.cs` mixes structural with balance/tuning (CLAUDE.md violation)
**Found in:** `Core/Constants.cs` (lines 18-46 — 17 of 24 entries)
**What:** Global CLAUDE.md ("Structural constants — rarely changed; Gameplay config — tuned often; different homes") AND the comment at the top of `GameplayConfig.cs` ("Structural constants remain in Constants.cs") both say balance constants belong in GameplayConfig. Today's `Constants.cs` is mixing them.
**Severity:** design-quality
**Status:** noted

**Stays in Constants.cs (structural, 7 entries):**
- `COLUMN_COUNT`, `MAX_ROWS`, `CELL_WIDTH`, `CELL_HEIGHT` (if kept after F-13), `CELL_VISUAL_HEIGHT`, `GRID_ORIGIN_X`, `GRID_ORIGIN_Y`, `CHEF_POSITION_COUNT`

**Moves to GameplayConfig (gameplay tuning, 11 entries):**
- `INITIAL_FALL_STEP_DURATION`, `MIN_FALL_STEP_DURATION` (difficulty curve)
- `MAX_LEVEL`, `STARTING_INGREDIENT_COUNT`, `MAX_INGREDIENT_COUNT` (difficulty caps)
- `POINTS_MATCH`, `POINTS_PER_INGREDIENT` (scoring)
- `BONUS_POOR/SMALL/MEDIUM/LARGE/MEGA/MAX_BURGER` (6 scoring tiers)

**Moves to GameplayConfig OR new MonetizationConfig (monetization tuning, 6 entries):**
- `CONTINUE_GEM_COST`, `GEM_REWARD_AD`, `GEM_PACK_VALUE`, `INTERSTITIAL_EVERY_N_GAMES`, `GEM_PACK_SPAWN_CHANCE`, `GEM_PACK_SPAWN_INTERVAL`

**Next step:** big sweep — move the 17 entries, update ~12 consumer files (mostly mechanical Constants.X → GameplayConfig.X renames; grep results from this review give the full list). Sub-decision on monetization (own file vs `#region` in GameplayConfig). Pairs with the discussion needed on derived constants (F-15) since both touch Constants.cs.

### F-15 — `CHEF_POSITION_COUNT` should be derived from `COLUMN_COUNT`
**Found in:** `Core/Constants.cs:6, 38`
**What:** `CHEF_POSITION_COUNT = 3` is hardcoded; by construction it's always `COLUMN_COUNT - 1` (chef stands between two columns). If `COLUMN_COUNT` ever changes, this silently drifts.
**Severity:** design-quality
**Status:** noted
**Next step:** `public const int CHEF_POSITION_COUNT = COLUMN_COUNT - 1;` — legal const expression in C#. Documents the structural relationship; auto-updates on column-count change.

### Decided — `MAX_INGREDIENT_COUNT` and `CHALLENGE_MAX_SIZE` kept independent (2026-05-26)
Same value (`7`) today, but kept as separate constants — challenge ceiling and spawn ceiling are tuned independently. Same call as the AnimConfig paired-constants decision.

### Other observations (not findings)
- The scoring tier BONUS_* values are used by `BurgerAnimator` (already grep-confirmed at lines 160-165). The tier BOUNDARIES (`>= 9, 7, 5, 3, 1`) appear inline in `AudioManager.HandleBurger` — already covered by F-1. When we walk `BurgerAnimator` we'll likely find matching inline thresholds there too.

---

## Config architecture decision (2026-05-26)

### F-16 — Adopt 6-config-file architecture
**Found in:** project-wide (decision, not a code location)
**What:** Several findings touch config organization (F-4 per-clip volumes, F-6 music default volume, F-14 Constants.cs split, the open monetization sub-decision). Decided to consolidate by adopting a 6-file split organized by "who would sit down to tune this":

| File | Concern | Tuner |
|---|---|---|
| `Constants.cs` | Structural (grid layout, never tuned) | Programmer, once |
| `GameplayConfig.cs` | Rules + difficulty + scoring | Game designer |
| `MonetizationConfig.cs` (new) | Gems, ads, IAP, drop rates | Business / PM |
| `AnimConfig.cs` | Animation feel | Motion designer |
| `UIStyles.cs` | Visual style | Visual designer |
| `AudioConfig.cs` (new) | Mix volumes, music defaults | Audio designer |

Scoring stays in `GameplayConfig` (8 entries — too small for its own file today; promote later if scoring complexity grows, per anti-speculation principle).

**Severity:** design-quality
**Status:** noted
**Next step:** this finding is the implementation strategy several others land against. Coordinated work:
- **F-4** (per-clip volumes) → land in new `AudioConfig` as `SfxVolumes` table or per-clip pairs
- **F-6** (music default volume) → land in new `AudioConfig.DEFAULT_MUSIC_VOLUME`
- **F-14** (Constants.cs split) → balance entries split between `GameplayConfig` (11) and `MonetizationConfig` (6); structural stays in `Constants` (7-8 depending on F-13 outcome)
- **F-11** (`LEVELUP_COLOR_RESTORE_DURATION`) — adds to AnimConfig as planned, no change to that finding

When F-16 lands as commits, the affected findings (F-4, F-6, F-11, F-14) close together as one architectural sweep. F-13 (CELL_HEIGHT) and F-15 (CHEF_POSITION_COUNT) are independent — can land before/after.

---

## `Core/ControlMode.cs`

Reviewed. No findings.

---

## `Core/DifficultyManager.cs`

### F-17 — `EvaluateLevel` resets test-mode level on first ingredient placed
**Found in:** `Core/DifficultyManager.cs:28-32, 49-67`
**What:** Test-dual-column mode sets `_currentLevel` to `TestDualColumnLevel` (e.g., 8) in Start without touching `_ingredientsPlaced` (stays at 0). On the first ingredient placed, `EvaluateLevel` computes `newLevel = 1` (since 0→1 placements only crosses `LEVEL_THRESHOLDS[0]`), sees `newLevel != _currentLevel`, and **overwrites the test level back to 1**. The test mode only "sticks" for the first frame of gameplay.

**Scope:** affects only the test-dual-column debug path. Normal play is safe because `_ingredientsPlaced` is monotonically increasing AND `LEVEL_THRESHOLDS` is monotonically ascending → `newLevel` is monotonically non-decreasing → can never compute a value LOWER than `_currentLevel`. The bug surfaces only when something manually sets `_currentLevel` higher without syncing `_ingredientsPlaced`.

**Severity:** blocker (for the test-dual-column debug feature); no impact on normal play
**Status:** noted
**Next step:** change `if (newLevel != _currentLevel)` to `if (newLevel > _currentLevel)` at `:61`. Doesn't change normal-play behavior (the `!=` already behaves like `>` there); states the intent ("we only raise the level") explicitly; robust against future manual overrides.

### Other observations (not findings)

- **Lazy `if (_x == null) _x = ...` pattern** at `:19-22` is the second/third sighting of the cross-scene variant (also `TouchInputHandler:23-28`). Different shape from the same-GameObject variant tracked from `ChefController`. Both shapes are reasonable for this codebase scale — not formalizing.
- **`_spawner` consumes both derived values AND the raw level** (`SetFallSpeed`, `SetActiveIngredientCount`, `SetCurrentLevel`). Slightly redundant (spawner could derive its own from the level), but the current shape keeps the lerp authority in one place (DifficultyManager). Acceptable.
- **`Constants.INITIAL_FALL_STEP_DURATION` / `MIN_FALL_STEP_DURATION` / `STARTING_INGREDIENT_COUNT` / `MAX_INGREDIENT_COUNT` / `MAX_LEVEL`** all consumed here — these are part of the F-14 sweep (moving to `GameplayConfig`). The lerp will read `GameplayConfig.*` after F-14 lands. No separate finding.
- **`OnLevelChanged?.Invoke` at `:31`** in test-mode path fires before other Start methods may have subscribed (Unity Start order is undefined). Subscribers could miss the initial-level event in test mode. Small fragility, only affects the debug path — accept.

---

## `Core/FeedbackManager.cs`

### F-18 — `FeedbackManager` mixes orchestration with asset/popup construction
**Found in:** `Core/FeedbackManager.cs:26-52` (sprite/flash construction), `:85-102` (TMP popup setup); also `Chef/ChefController.cs:150-174` (sister case)
**What:** 140-line class doing both event routing AND procedural asset generation + inline TMP component configuration. The class wants to be an orchestrator that says "match happened → spawn popup + shake; burger happened → spawn popup + shake + flash"; today it also knows how to build a flash overlay GameObject, generate a white sprite, and configure TextMeshPro fields for popups.

`ChefController` has the sister problem with `GenerateCircleSprite` — second instance of procedural sprite generation inline in a behavior class. Formalizes the pattern we were tracking.

**Severity:** design-quality
**Status:** noted
**Next step:** three-step extraction landing together (each step builds on the prior; all three in one commit):

1. **Create `Core/SpriteUtils.cs` static helper**:
   - `public static Sprite CreateSolidColorSprite(Color color, int size = 4)` — replaces `FeedbackManager.CreateWhiteSprite`
   - `public static Sprite CreateCircleSprite(int size = 64)` — replaces `ChefController.GenerateCircleSprite`
   - Both methods are pure, no Unity scene references.

2. **Create `Core/ScreenFlashOverlay.cs` MonoBehaviour** (or `UI/ScreenFlashOverlay.cs`):
   - Owns the flash GameObject construction (parent to camera, sortingOrder 200, camera-fit scaling)
   - Owns the flash sprite (uses `SpriteUtils.CreateSolidColorSprite(Color.white)`)
   - Public method: `Trigger()` — runs the DOColor fade
   - Owns its own tween kill in OnDestroy
   - FeedbackManager creates one in Awake (`_flash = gameObject.AddComponent<ScreenFlashOverlay>()`) and calls `_flash.Trigger()` from `HandleBurgerEffect`

3. **Add static `Spawn` factories on `ScorePopup` and `BurgerPopup`**:
   - `public static ScorePopup Spawn(Vector3 position, int points, Color color)` — creates GameObject, configures TMP, calls Initialize, returns the popup
   - `public static BurgerPopup Spawn(Vector3 position, int points, string name)` — same pattern
   - FeedbackManager's `SpawnScorePopup` / `SpawnBurgerPopup` reduce to one-liner calls; the ~25 lines of TMP setup move onto the popup classes themselves
   - `ChefController.CreatePositionBubbles` could follow the same pattern in a future pass (not part of this finding's scope)

After all three: FeedbackManager is ~50 lines of pure orchestration (subscribe → dispatch → shake), no construction details. Single responsibility achieved.

---

## `Core/GameManager.cs`

### F-19 — `Start` method is doing four distinct things in one 44-line block
**Found in:** `Core/GameManager.cs:46-90`
**What:** Start runs four conceptually separate phases — dependency resolution, manager-component ensure, persisted-settings apply, event subscription — followed by an optional auto-start. Hard to scan; reader has to manually demarcate the sections.
**Severity:** cosmetic
**Status:** noted
**Next step:** extract four private methods, leaving Start as an orchestration list:
```csharp
private void Start()
{
    ResolveDependencies();
    EnsureManagers();
    ApplyPersistedSettings();
    SubscribeEvents();
    if (_autoStartGame) StartGame();
}
```
- `ResolveDependencies()` — lines 48-51 (the `_difficultyManager` lookup chain)
- `EnsureManagers()` — lines 53-72 (the 5 `EnsureComponent` calls + the inline SaveData/Music ensure blocks; the latter two collapse to `EnsureComponent` calls after F-1 lands)
- `ApplyPersistedSettings()` — lines 74-76 (sound volume + music)
- `SubscribeEvents()` — lines 78-84 (GridManager events)

Lands cleanly with F-1 (the EnsureComponent extraction makes `EnsureManagers` even tighter).

### F-20 — Game-flow methods need explanatory comments for state-transition clarity
**Found in:** `Core/GameManager.cs:102-163` — seven flow methods with overlapping verbs (`StartGame`, `PauseGame`, `ResumeGame`, `PauseSpawning`, `ResumeSpawning`, `RestartGame`, `ContinueGame`)
**What:** The verbs partially collide (Pause/Resume/Continue/Restart/Start) and the methods operate at different scopes (full game vs spawner-only vs scene reload). A reader landing on `ContinueGame` can't tell from-which-state without tracing call sites.
**Severity:** cosmetic
**Status:** noted
**Next step:** add a one-line `///` summary above each flow method, naming the FROM-state, the TO-state, and any key side effect. Concrete text:

```csharp
/// <summary>Initial play start. State: any → Playing. Resets score; starts spawning.</summary>
public void StartGame() { ... }

/// <summary>Pause-menu entry. State: Playing → Paused. Sets timeScale=0; stops spawning.</summary>
public void PauseGame() { ... }

/// <summary>Pause-menu exit. State: Paused → Playing. Sets timeScale=1; resumes spawning.</summary>
public void ResumeGame() { ... }

/// <summary>Spawner-only pause (used by BurgerAnimator during burger compress). No state change.</summary>
public void PauseSpawning() { ... }

/// <summary>Spawner-only resume (paired with PauseSpawning). Only resumes if state==Playing.</summary>
public void ResumeSpawning() { ... }

/// <summary>Full reset via scene reload. State: any → fresh. Increments games-played counter.</summary>
public void RestartGame() { ... }

/// <summary>Revive from game-over (gem cost or ad). State: GameOver → Playing. Clears top half; score preserved.</summary>
public void ContinueGame() { ... }
```

Same-file change, pure documentation, no behavior touched.

### Other observations (not findings)

- **`public X => _x` properties over `[SerializeField] private _x`** is the standard pattern per CLAUDE.md. Correct, consistent.
- **Score reset inside `StartGame`** is correct given StartGame is the fresh-game entry point. A separate `ResetGame()` would only earn its keep if non-scene-reload restart becomes a thing; anti-speculation says wait.
- **`Time.timeScale = 0` in PauseGame** is the standard Unity pause approach (freezes scaled time + DOTween by default, leaves UI input responsive). The explicit `_spawner.StopSpawning()` alongside is defensive belt-and-suspenders — fine.
- **`StartGame` vs `RestartGame`** chain naturally (RestartGame → scene reload → fresh GameManager → StartGame fires via `_autoStartGame`). Not duplicates.
- **`EnsureComponent` location** is covered by F-1 (move to `Core/MonoBehaviourUtil` as `public static`). F-1's scope already absorbs the GameManager Start-block inline duplications (the SaveData/Music ensures at `:60-72`).

---

## `Core/GameState.cs`

### F-21 — `Paused` is structurally a modifier on `Playing`, not a peer state
**Found in:** `Core/GameState.cs:7`; `Core/GameManager.cs:117, 124`
**What:** `GameState.Paused` is the only enum value that requires a precondition on another state — you can only enter Paused FROM Playing, and Resume goes back to Playing. There's no Menu→Paused or GameOver→Paused path. Structurally, Paused is "Playing, suspended" — a modifier, not a peer mode. The implicit invariant ("Paused only from Playing") is enforced today by guards in `PauseGame`/`ResumeGame` rather than by the type system. Zero external consumers subscribe to `OnStateChanged` for Paused-specific behavior — pause is currently a GameManager-internal mechanism wearing state-machine clothing.
**Severity:** design-quality
**Status:** noted
**Next step:**
1. Remove `Paused` from `GameState` enum (leaves `Menu`, `Playing`, `GameOver`)
2. Add `private bool _isPaused` field to `GameManager`
3. Add `public bool IsPaused => _isPaused` read-only accessor (cheap; future pause-overlay UI is likely to want it)
4. Rewrite `PauseGame` / `ResumeGame` to toggle `_isPaused` instead of calling `SetState`:
   ```csharp
   public void PauseGame()
   {
       if (_currentState != GameState.Playing || _isPaused) return;
       _isPaused = true;
       _spawner?.StopSpawning();
       Time.timeScale = 0f;
   }
   public void ResumeGame()
   {
       if (!_isPaused) return;
       _isPaused = false;
       _spawner?.StartSpawning();
       Time.timeScale = 1f;
   }
   ```
5. Skip an `OnPauseChanged` event for now (no subscribers today; add when a consumer appears, per anti-speculation principle)

Net change: ~5 lines in GameManager + one line removed from GameState. No external consumers affected (grep confirms zero references to `GameState.Paused` outside GameManager).

Affects F-20's `PauseGame` / `ResumeGame` summary comments — drop the "Paused" state mentions; the comments instead describe the `_isPaused` toggle and timeScale flip.

---

## `Core/Rng.cs`

Reviewed. No findings.

---

## `Core/SaveDataManager.cs`

### F-22 — `ShouldShowInterstitial` mixes persistence with monetization-cadence logic
**Found in:** `Core/SaveDataManager.cs:96-99`; caller at `UI/GameOverPanel.cs:148`
**What:** SaveDataManager is otherwise pure persistence (load fields from PlayerPrefs, getters, write-through setters). `ShouldShowInterstitial()` encodes the ad-cadence rule ("every Nth completed game show an interstitial") which is monetization policy, not persistence. The class owns the data, but not the decision of when to show ads.
**Severity:** design-quality
**Status:** noted
**Next step:** move the method to `AdManager` (logical owner of ad lifecycle):
```csharp
// AdManager:
public bool ShouldShowInterstitial()
{
    int played = SaveDataManager.Instance?.GamesPlayed ?? 0;
    return played > 0 && played % MonetizationConfig.INTERSTITIAL_EVERY_N_GAMES == 0;
}
```
Update `GameOverPanel:148` to call `AdManager.Instance.ShouldShowInterstitial()`. Delete the method from SaveDataManager. The constant reference (`Constants.INTERSTITIAL_EVERY_N_GAMES` today → `MonetizationConfig.INTERSTITIAL_EVERY_N_GAMES` after F-14/F-16) lands as part of the same edit if F-16 hasn't shipped yet.

Note: SaveDataManager has a broader production-readiness concern (insecurity, no cloud sync, no schema versioning) — out of scope for this review; tracked in `Docs/pre-launch-checklist.md`.

---

## `Core/SceneLoader.cs`

### F-23 — `GameManager.RestartGame` bypasses `SceneLoader`
**Found in:** `Core/GameManager.cs:142-152` (offender); `Core/SceneLoader.cs:17-21` (the utility being bypassed)
**What:** `SceneLoader` exists as the dedicated scene-loading utility (resets `Time.timeScale = 1f`, loads scene by name), but `RestartGame` reaches around it and calls `SceneManager.LoadScene(...GetActiveScene().buildIndex)` directly. Two consequences: (1) drift — there are now two patterns for scene loading in the codebase, and (2) duplication of the defensive `Time.timeScale = 1f` reset.
**Severity:** design-quality
**Status:** noted
**Next step:** rewrite `RestartGame` to use `SceneLoader.LoadGame()`:
```csharp
public void RestartGame()
{
    if (SaveDataManager.Instance != null)
        SaveDataManager.Instance.IncrementGamesPlayed();
    SceneLoader.LoadGame();
}
```
Drops the manual `Time.timeScale = 1f` (SceneLoader handles it) and the manual `SceneManager.LoadScene(...)` call. RestartGame is only meaningful from the Game scene, so loading "Game" by name is equivalent to reloading the active scene by index. SceneLoader becomes the single entry point for scene transitions.

---

## `Core/UIStyles.cs`

### F-24 — Inline UI style colors should reference UIStyles
**Found in:** `UI/GameLayout.cs:20-21` (border + fill colors); `UI/BurgerChallenge.cs:25-26` (meter bg + fill colors); `UI/BurgerPopup.cs:42`, `UI/BurgerChallenge.cs:292` (`Color.white` for text)
**What:** Four UI styling colors are declared inline as `new Color(...)` SerializeField defaults (`GameLayout._borderColor`, `_fillColor`; `BurgerChallenge._meterBgColor`, `_meterFillColor`). Two more spots set `tmp.color = Color.white` directly instead of `UIStyles.TEXT_UI`. UIStyles is the canonical home for UI colors; these inline declarations bypass it and become drift bait.
**Severity:** design-quality
**Status:** noted
**Next step:**
1. Add four new entries to `UIStyles.cs`:
   ```csharp
   public static readonly Color LAYOUT_BORDER = new(0f, 0f, 0f, 0.8f);
   public static readonly Color LAYOUT_FILL = new(0f, 0f, 0f, 0.15f);
   public static readonly Color CHALLENGE_METER_BG = new(0.2f, 0.2f, 0.2f, 0.8f);
   public static readonly Color CHALLENGE_METER_FILL = new(0.2f, 0.9f, 0.3f, 1f);
   ```
2. Update SerializeField defaults to reference the new entries:
   - `GameLayout._borderColor = UIStyles.LAYOUT_BORDER;`
   - `GameLayout._fillColor = UIStyles.LAYOUT_FILL;`
   - `BurgerChallenge._meterBgColor = UIStyles.CHALLENGE_METER_BG;`
   - `BurgerChallenge._meterFillColor = UIStyles.CHALLENGE_METER_FILL;`
3. Replace `Color.white` with `UIStyles.TEXT_UI` at `BurgerPopup.cs:42` and `BurgerChallenge.cs:292`.

Editor Gizmos colors in ChefController / GridManager are intentionally out of scope (editor-only, not part of the UI design system).

### F-25 — `Color` constructor convention inconsistency in UIStyles
**Found in:** `Core/UIStyles.cs` (5 entries)
**What:** Some `Color` entries use 3-arg constructor (implicit alpha=1), others use 4-arg with explicit `1f` trailing alpha. Functionally identical, stylistically inconsistent. Reader has to second-guess whether the explicit `1f` means anything.
**Severity:** cosmetic
**Status:** noted
**Next step:** drop the redundant `1f` alpha from 5 entries — use 3-arg form when alpha=1, 4-arg only when alpha < 1:
- `INNER_PANEL_BG = new(0.18f, 0.18f, 0.25f);` (was `..., 1f)`)
- `GOLD = new(1f, 0.85f, 0f);`
- `BTN_CONTINUE_GEMS = new(0.9f, 0.7f, 0.1f);`
- `BTN_CONTINUE_AD = new(0.3f, 0.5f, 0.9f);`
- `BTN_RESTART = new(0.2f, 0.7f, 0.3f);`

Also apply to `CHALLENGE_METER_FILL` when it lands as part of F-24 (`new(0.2f, 0.9f, 0.3f);` instead of `..., 1f)`).

---

## `Grid/BurgerAnimator.cs`

### F-26 — `BurgerAnimator` handles 3 responsibilities; split into Animation + Scoring + Naming
**Found in:** `Grid/BurgerAnimator.cs` (whole file); cross-references `Audio/AudioManager.cs:86-91` (F-1 duplication); `Grid/GridManager.cs:158, 173` (`BurgerData` external usage); F-7 (empty `Scoring/` folder)
**What:** The class doc admits the bundling: *"Handles burger compress animation, scoring, and name generation."* Three distinct responsibilities. Per project CLAUDE.md SRP rule, this should split. Bonus: the natural homes for the scoring/naming pieces populate the empty `Scoring/` folder (closes F-7's Scoring half).

Additional issues found in the same pass:
- **F-1 confirmed.** `CalculatePoints` thresholds (`== 0`, `1-2`, `3-4`, `5-6`, `7-8`, `9+`) match `AudioManager.HandleBurger`'s (`>= 9, >= 7, >= 5, >= 3, >= 1`, else). Tier boundaries duplicated across two systems — exactly what F-1 flagged as the open question. Confirmed.
- **`BurgerData` struct is nested** but accessed externally via awkward `BurgerAnimator.BurgerData` qualified path at `GridManager.cs:158, 173`. It's a domain type, not animator-private.

**Severity:** design-quality
**Status:** noted
**Next step:** coordinated 5-file refactor landing as one commit:

1. **Create `Scoring/BurgerTier.cs`** — `public enum BurgerTier { Poor, Small, Medium, Large, Mega, Max }`
2. **Create `Scoring/Scoring.cs`** — `public static class Scoring`, consolidated scoring home (mirrors `Core/Rng.cs` pattern — one static class per cross-cutting utility). Amended by F-34 to absorb FastDrop too. Initial contents:
   ```csharp
   public static BurgerTier GetBurgerTier(int ingredientCount)  // single source of truth for tier boundaries
   public static int CalculateBurgerPoints(int ingredientCount)  // moved from BurgerAnimator.CalculatePoints; internally uses GetBurgerTier
   ```
3. **Create `Scoring/BurgerNamer.cs`** — `public static class` with `GenerateName(int ingredientCount)` moved verbatim from `BurgerAnimator.GenerateName`. Name pools (smallPrefixes, mediumPrefixes, etc.) live as private static readonly arrays.
4. **Create `Grid/BurgerData.cs`** — promote the nested struct to a top-level type (matches the project preference for no nested types; also resolves the awkward qualified-access pattern).
5. **Trim `Grid/BurgerAnimator.cs`** to animation-only — keep `PlayCompress` + `CompressCoroutine`. Optionally extract three non-coroutine helpers (`ValidateParts`, `PauseScene`, `ResumeScene`) for clarity. Delete `CalculatePoints` and `GenerateName`.
6. **Update `Audio/AudioManager.HandleBurger`** to dispatch via `Scoring.GetBurgerTier(ingredientCount)` switch instead of inline `>= 9` chain. Closes F-1.
7. **Update `Grid/GridManager.cs:158, 173`** to use unqualified `BurgerData` (top-level type after step 4).
8. **CLAUDE.md update:** the "Project Structure" `Scripts/` listing gains a `Scoring/` entry. The Burger/ folder remains empty and follows F-7's delete path (only Scoring/ gets populated by this refactor).

After F-26 lands: F-1 closes (replaced by shared `GetTier` lookup), F-7's Scoring half closes (folder populated), F-7's Burger half still goes to delete per its existing next step.

---

## `Grid/Column.cs`

### F-27 — `CheckForMatch` over-engineered; collapses to `top.Type == second.Type` + helper cleanup
**Found in:** `Grid/Column.cs:132-151`; `Ingredients/IngredientType.cs:19-30`
**What:** `CheckForMatch` has two branches (regular+regular same-type, BunBottom+BunBottom cancel) that together handle a strict subset of the simple predicate `top.Type == second.Type`. Enumerating all combinations:

| top | second | current | simple `==` |
|---|---|---|---|
| regular, regular (same) | true | true ✓ |
| regular, regular (different) | false | false ✓ |
| regular, any bun | false | false ✓ |
| BunBottom, BunBottom | true | true ✓ |
| BunBottom, BunTop | false | false ✓ |
| BunTop, BunTop | false | true (differs!) |
| BunTop, BunBottom | false | false ✓ |

The only divergence is `BunTop+BunTop`, which is **unreachable by invariant**: `GridManager.OnIngredientLanded` destroys lone BunTops on land (no BunBottom below → "Too bad!" + DestroyWithFlash), so two BunTops can never end up adjacent in a column. Column swap moves contents intact, can't create new bun adjacencies. Therefore the simple `top.Type == second.Type` is equivalent in all reachable cases.

**Cascade:** `IsRegularIngredient` is consumed only by this one place — after simplification it becomes dead. `IsBun` is **already** dead (declared, zero consumers anywhere). Both dead → the entire `IngredientTypeExtensions` static class becomes empty.

**Severity:** design-quality
**Status:** noted
**Next step:** bundled refactor:

1. **Simplify `Column.CheckForMatch`:**
   ```csharp
   public bool CheckForMatch(out Ingredient top, out Ingredient second)
   {
       top = null;
       second = null;
       if (_ingredients.Count < 2) return false;

       top = _ingredients[_ingredients.Count - 1];
       second = _ingredients[_ingredients.Count - 2];

       // BunTop+BunTop can't occur: lone BunTops self-destruct on land
       // (see GridManager.OnIngredientLanded). Column swaps move contents
       // intact and can't create new bun adjacencies.
       return top.Type == second.Type;
   }
   ```
2. **Delete `IsRegularIngredient`** from `IngredientType.cs` (dead after step 1).
3. **Delete `IsBun`** from `IngredientType.cs` (already dead).
4. **Delete the `IngredientTypeExtensions` static class** entirely (becomes empty after steps 2-3). `IngredientType.cs` shrinks to just the enum.

Net: `CheckForMatch` collapses ~15 lines → ~5; `IngredientType.cs` ~30 lines → ~15.

---

## `Grid/GridManager.cs`

### F-28 — Overflow check fires before match check, killing "saving matches"
**Found in:** `Grid/GridManager.cs:86-124` (`OnIngredientLanded`)
**What:** For non-BunTop landings, the overflow check at `:111` runs BEFORE `CheckAndProcessMatches` at `:120`. Concrete bug: column with 12 ingredients ending in `[..., A, B]`, a `B` lands as the 13th → `count == 13` → `IsOverflowing` returns true → `OnGameOver` fires. The landing CREATED a match (top two B's) that would have eliminated them and dropped count to 11 — but game over preempted. Players who land a "saving match" get punished. The BunTop branch's defensive early-return (and the misleading `// Top bun: check burger before overflow` comment at `:90`) exists for the same overflow-preemption reason — once overflow check moves to the end, BunTop doesn't need the defensive structure either.
**Severity:** **blocker** (UX-affecting gameplay bug)
**Status:** noted
**Next step:** restructure `OnIngredientLanded` so overflow check runs AFTER match processing:
```csharp
public void OnIngredientLanded(Ingredient ingredient)
{
    Column column = ingredient.CurrentColumn;

    if (ingredient.Type == IngredientType.BunTop)
    {
        if (MatchDetector.HasBunBelow(column, ingredient))
        {
            OnIngredientPlaced?.Invoke();
            CheckAndProcessBurger(column);
        }
        else
        {
            // Lone BunTop self-destructs (no OnIngredientPlaced)
            Vector3 pos = ingredient.transform.position;
            column.RemoveIngredient(ingredient);
            ingredient.DestroyWithFlash();
            FloatingText.Spawn(pos, "Too bad!", UIStyles.TEXT_TOO_BAD, UIStyles.WORLD_FLOATING_TEXT_SIZE);
        }
        return;
    }

    OnIngredientPlaced?.Invoke();
    CheckAndProcessMatches(column);   // may reduce count

    if (column.IsOverflowing)         // NOW check overflow
    {
        OnGameOver?.Invoke();
        return;
    }

    CheckAndProcessBurger(column);
}
```
Also delete the now-misleading `// Top bun: check burger before overflow` comment at `:90`.

### F-29 — `SwapColumns` and `SwapColumnTops` are dead code
**Found in:** `Grid/GridManager.cs:194-197` (wrapper); `:272-331` (offender)
**What:** Grep confirms only one caller in the swap family — `ChefController:108` calls `SwapColumnsWithWaveEffect`. `SwapColumns` (60 lines, near-duplicate of the wave version without the stagger) has zero external callers; `SwapColumnTops` is a 4-line wrapper that calls only `SwapColumns` and also has zero external callers. Both are dead per anti-speculation.
**Severity:** design-quality (dead-code cleanup)
**Status:** noted
**Next step:** delete both methods (`SwapColumnTops` at `:194-197`, `SwapColumns` at `:272-331`). Net: ~70 lines removed. The "two near-identical swap methods" structural concern dissolves to one method (`SwapColumnsWithWaveEffect`).

### F-30 — Extract `SwapAnimator` + event-based completion (replaces `DelayedMatchCheck` coupling)

**HIGH-RISK REFACTOR — manual playtest required before merge. See test plan below.**

**Found in:** `Grid/GridManager.cs:199-254` (`SwapColumnsWithWaveEffect`); `:256-270` (`DelayedMatchCheck`)
**What:** Wave-stagger animation iteration is inline in GridManager (lines 220-229), and `DelayedMatchCheck` is a time-based coroutine that gates gameplay logic (match/burger check) on animation timing parameters. The smell: GAME LOGIC depends on ANIMATION TIMING — coupling between gameplay and presentation. The duration math `Math.Max(rows) * SWAP_WAVE_DELAY_PER_ROW + SWAP_POST_ANIM_DELAY` exists because the wave staggers visually; if matches process the instant the data swap completes, ingredients pop while still mid-wave.
**Severity:** design-quality (high-risk refactor)
**Status:** noted
**Next step:** extract `Grid/SwapAnimator.cs` (sister to `BurgerAnimator`) that owns the wave animation and fires an `OnSwapComplete` event:

```csharp
public class SwapAnimator : MonoBehaviour
{
    public event Action<Column, Column> OnSwapComplete;

    public void PlaySwap(Column colA, Column colB,
        List<Ingredient> stackedA, List<Ingredient> stackedB,
        List<Ingredient> swappedFalling)
    {
        StartCoroutine(SwapCoroutine(colA, colB, stackedA, stackedB, swappedFalling));
    }

    private IEnumerator SwapCoroutine(...)
    {
        // wave-stagger animate stackedA + stackedB
        // DoWaveEffect(0) on swappedFalling
        // wait maxDelay (Option A semantics: matches current timing exactly)
        OnSwapComplete?.Invoke(colA, colB);
    }
}
```

`GridManager`:
- Adds `_swapAnimator = gameObject.AddComponent<SwapAnimator>();` in Awake (same shape as `_burgerAnimator`)
- Subscribes: `_swapAnimator.OnSwapComplete += HandleSwapComplete;`
- `SwapColumnsWithWaveEffect` does only the DATA swap (instant) + collects the falling list + calls `_swapAnimator.PlaySwap(...)`
- `HandleSwapComplete(colA, colB)` does what `DelayedMatchCheck` does today (CheckAndProcessMatches + CheckAndProcessBurger guarded by `!IsEmpty`)
- DELETE `DelayedMatchCheck` coroutine

**Risks to mitigate:**
1. **Race on rapid swap input** — SwapAnimator should track in-flight state. Simplest guard: ignore new `PlaySwap` calls if a swap is already in flight (matches today's pragmatic behavior; input gating in ChefController is the long-term answer).
2. **`OnSwapComplete` firing semantics** — use Option A: fire after the `maxDelay` timer expires (behaviorally identical to today's `DelayedMatchCheck` waiting period). Any regression is clearly the refactor, not the timing semantics.
3. **Swap during burger compress** — `BurgerAnimator` already pauses spawning + freezes falling ingredients during compress. Verify input gating in ChefController prevents swap during compress (read `_isMoving` check at `ChefController.SwapPlates:93`). If not gated, this refactor doesn't change the behavior — pre-existing risk.
4. **Stacked-burger detection** in `HandleBurgerAnimationComplete:191` — calls `CheckAndProcessBurger` recursively. Must coexist with `OnSwapComplete` firing concurrently. The `!colA.IsEmpty` / `!colB.IsEmpty` guards in `HandleSwapComplete` (carried over from `DelayedMatchCheck`) handle the case where a burger ate the column mid-wave.

**Test plan (must execute manually before merge):**

| # | Scenario | Verify |
|---|---|---|
| 1 | Swap with no matches in either column | Wave animation plays, no false match/burger triggers |
| 2 | Swap that creates a match in one column | Match plays AFTER wave completes; score updates; floating text renders at correct position |
| 3 | Swap that creates a burger | Burger forms after wave; compress animation plays; score awarded; popup renders |
| 4 | Rapid swap-swap-swap (double/triple tap) | No crashes; no duplicate match checks; columns end in consistent state |
| 5 | Swap while a falling ingredient is mid-fall | Falling ingredient correctly reassigned to other column; visual continues smoothly |
| 6 | Swap attempted during burger compress | Either input gated (no swap during compress) OR swap defers until compress done — verify which today's behavior is |
| 7 | Swap-then-game-over | No stray match check after game over fires |
| 8 | Swap during chef flip animation | Visual interaction is sensible; no orphaned tweens |

Land independently of F-26 (different files). Land AFTER F-28 + F-29 (cleanest possible baseline before this high-risk refactor).

---

## `Grid/MatchDetector.cs`

### F-31 — Formalize game-input freeze during burger resolution; collapse burger detection logic
**Found in:** `Grid/MatchDetector.cs` (`HasBunBelow:55-66`, `DetectBurger:72-125`, `BurgerDetection` struct); `Grid/GridManager.cs:144` (`_columnsWithActiveBurger` guard); `Grid/GridManager.cs:191` (cascade re-check); `Chef/ChefController.cs` (swap input not gated against active burger)

**What:** The current burger-detection logic is over-engineered defensive code for unreachable states. Tracing actual reachability under today's freeze (`BurgerAnimator.CompressCoroutine` does `PauseSpawning()` + `PauseFalling()` on all falling ingredients):

- No new ingredients spawn during burger animation ✓
- No in-flight ingredients land during burger animation ✓
- Unity main-thread is synchronous between "BunTop lands" and "animation starts" ✓
- **One leak:** `ChefController.SwapPlates` is NOT gated against active burger — only checks `_isMoving`. A swap CAN trigger mid-burger-animation, potentially corrupting state.

So the "cascade burger" scenarios that justify `DetectBurger`'s general-purpose design (find-topmost-BunTop loop, break-on-intermediate-BunTop defense), the `_columnsWithActiveBurger` flag, `HasBunBelow` as a separate method, and the `HandleBurgerAnimationComplete:191` cascade re-check are all defensive code for states that can't actually be reached — EXCEPT for the unguarded swap-during-burger window, which is a real bug.

**Design proposal:** formalize the freeze model — during burger resolution, ALL game-affecting inputs are blocked, with ONE exception: pause input always works (pause takes priority over everything else, and pausing during resolution naturally pauses the animation via `Time.timeScale = 0`).

**Severity:** design-quality (significant simplification + closes a real swap-during-burger bug)
**Status:** noted

**Next step:** coordinated refactor:

1. **Add `IsResolving` flag to `GameManager`** (mirrors the modifier-bool pattern F-21 establishes for `_isPaused`):
   ```csharp
   private bool _isResolving;
   public bool IsResolving => _isResolving;
   public void BeginResolution() { _isResolving = true; }
   public void EndResolution() { _isResolving = false; }
   ```
2. **`BurgerAnimator.CompressCoroutine`** calls `GameManager.Instance?.BeginResolution()` at start and `EndResolution()` at end (around the existing `PauseSpawning`/`ResumeSpawning` calls).
3. **`TouchInputHandler`** checks `GameManager.Instance.IsResolving` at the start of every input handler. If true, return early — EXCEPT for the pause button handling, which runs first and bypasses the gate.
4. **Delete `MatchDetector.HasBunBelow`** entirely.
5. **Delete `MatchDetector.DetectBurger`** entirely. The struct `BurgerDetection` disappears with it.
6. **Inline burger detection in `GridManager.OnIngredientLanded` BunTop branch:**
   ```csharp
   if (ingredient.Type == IngredientType.BunTop)
   {
       int topIndex = column.StackHeight - 1;  // we just landed here
       int bottomIndex = FindBunBottomBelow(column, topIndex);
       if (bottomIndex >= 0)
           ProcessBurger(column, topIndex, bottomIndex);
       else
           DestroyLoneBunTop(ingredient, column);
       return;
   }
   ```
   Where `FindBunBottomBelow` is a private helper that just scans downward for `BunBottom` (no defensive break — the F-27 invariant guarantees no intermediate BunTops).
7. **Delete `GridManager._columnsWithActiveBurger`** field and the guard at `:144` — IsResolving covers the same case, globally.
8. **Delete the cascade re-check at `HandleBurgerAnimationComplete:191`** — under the freeze, no new state is reachable post-collapse that wouldn't already be detected by the inline BunTop logic on the next landing.
9. **Delete the unused-now `CheckAndProcessBurger` method** if it becomes orphaned (or trim it to the inline shape).
10. `MatchDetector.cs` reduces to just `TryProcessMatch` and `MatchResult` — pure match detection, no burger logic.

**Risks to mitigate:**
1. **In-flight gestures when resolution starts** — if the player is mid-drag when a burger animation begins, the drag should cancel cleanly. TouchInputHandler implementation detail.
2. **Pause-during-resolution semantics** — verify `Time.timeScale = 0` properly pauses BurgerAnimator's coroutine (`WaitForSeconds` respects timeScale by default — should work). Resume continues animation correctly.
3. **The order-match override in `HandleBurgerAnimationComplete:182-184`** (the GridManager → BurgerChallenge coupling I noted earlier) is unaffected — that's a separate concern.

**Test plan (must execute manually before merge):**

| # | Scenario | Verify |
|---|---|---|
| 1 | Land a BunTop with BunBottom below — normal burger | Burger forms, animation plays, score awarded |
| 2 | Land a lone BunTop (no BunBottom) | BunTop self-destructs with "Too bad!" |
| 3 | Tap chef-swap during burger animation | Input ignored; no swap occurs |
| 4 | Tap falling ingredient during burger animation | Input ignored; no fast-drop |
| 5 | Press pause during burger animation | Pause works; animation freezes; resume continues correctly |
| 6 | After burger collapse, land another ingredient | New detection works normally |
| 7 | After burger collapse, land a BunTop directly | Detects new burger correctly |
| 8 | Trigger game-over during burger animation | Game over fires correctly after resolution ends (or as currently behaves) |

**Sequence:** AFTER F-21 (Paused → bool establishes the modifier-flag pattern this uses), AFTER F-27 (BunTop invariant formally established by the IsRegularIngredient cleanup). Can land alongside or after F-26.

### F-32 — Promote `MatchResult` struct to top-level type
**Found in:** `Grid/MatchDetector.cs:13-17`
**What:** `MatchResult` is a nested struct used externally via `out` param of `TryProcessMatch`. Per the no-nested-types CLAUDE.md rule, should be a top-level type. (`BurgerDetection` from this file disappears with F-31 — no need to promote it.)
**Severity:** cosmetic
**Status:** noted
**Next step:** move `MatchResult` to its own file `Grid/MatchResult.cs` as a top-level `public struct`. Update the `out` param type at the consumer (`GridManager.CheckAndProcessMatches:128`).


### Other observations (not findings)

- **No `DontDestroyOnLoad`** — each scene load destroys and rebuilds AudioManager (regenerates all 13 clips, ~few ms). Likely intentional (MusicManager is the persistent one per CLAUDE.md). Revisit at `SceneLoader`.
- **`_squeezSource` uses `.Play()` (overwrites on repeat); `_sfxSource` uses `PlayOneShot` (overlaps).** Intentional behavior split — squeeze shouldn't stack.
- **`_squeezSource` is a typo** (missing trailing `e`) — fix opportunistically next time the field is touched. Not worth its own commit.

---

## `Ingredients/Ingredient.cs`

### F-33 — Two-bool flag encoding of a 3-state lifecycle → enum
**Found in:** `Ingredients/Ingredient.cs:13-14` (`_isLanded`, `_isFalling`); state transitions at `:36-37` (Initialize), `:66-67` (StartFalling), `:124-125` (Land)
**What:** `_isFalling` and `_isLanded` together encode three reachable states, not two:

| State | `_isFalling` | `_isLanded` |
|---|---|---|
| Spawned, awaiting fall start (preview window) | `false` | `false` |
| Falling | `true` | `false` |
| Landed | `false` | `true` |

Classic flag-soup smell flagged by the project CLAUDE.md ("Explicit state machines. Never use multiple boolean flags to represent state"). Same pattern F-21 fixes for `Paused` on GameManager. The fourth combination (`true`/`true`) is unreachable but currently unguarded — invalid state representable in the type system.

**Severity:** design-quality
**Status:** noted
**Next step:**

1. **Create `Ingredients/IngredientState.cs`** — `public enum IngredientState { Spawned, Falling, Landed }`.
2. **Replace `_isFalling` + `_isLanded`** with a single `private IngredientState _state = IngredientState.Spawned;` field on `Ingredient`.
3. **Replace public properties** `IsLanded` / `IsFalling` with `public IngredientState State => _state;`. Consumers checking `IsFalling` become `State == IngredientState.Falling`, etc. Grep for both property names to update callers.
4. **Convert state transitions:**
   - `Initialize` → `_state = IngredientState.Spawned` (replaces lines 36-37)
   - `StartFalling` guard becomes `if (_state == IngredientState.Falling) return;` then `_state = IngredientState.Falling`
   - `Land` becomes `_state = IngredientState.Landed`
   - `FallOneStep` guard `if (_isLanded || !_isFalling)` → `if (_state != IngredientState.Falling) return;`
   - `FastDrop` guard `if (!_isFalling || _isLanded)` → `if (_state != IngredientState.Falling) return;`
   - `SwapToColumn` guard `if (_isFalling && !_isLanded)` → `if (_state == IngredientState.Falling)`

Net: -2 fields, -2 properties, +1 field, +1 property, +1 enum file. Invalid-state combinations become unrepresentable; switch-on-state becomes exhaustively checkable.

### F-34 — Amend F-26: consolidate scoring into `Scoring/Scoring.cs`; absorb FastDrop calc
**Found in:** `Ingredients/Ingredient.cs:229-256` (`FastDrop`); amends F-26's step 2 + step 6
**What:** `FastDrop` mixes scoring (`points = Mathf.RoundToInt(distance * GameplayConfig.FAST_DROP_POINTS_PER_UNIT)` at line 238) with animation. By F-26's principle, scoring computation belongs outside the animator/entity class.

But: a dedicated `FastDropScoring` class for a one-line multiplication is the speculative-API smell from the global CLAUDE.md. The constant `GameplayConfig.FAST_DROP_POINTS_PER_UNIT` already names the rate; `Mathf.RoundToInt(distance * rate)` is the entire computation.

Better: **consolidate** F-26's `Scoring/BurgerScoring.cs` into a general `Scoring/Scoring.cs` (`public static class Scoring`, mirrors `Core/Rng.cs` pattern) and put FastDrop's calc there too. Designers/programmers looking for "where do scoring numbers come from" find one file, not N tiny per-concern files.

What stays separate from the consolidated class:
- **`Scoring/BurgerTier.cs`** — enum, one-top-level-type-per-file rule
- **`Scoring/BurgerNamer.cs`** — naming, not scoring. Different concern, different file.

Test for future scoring additions: **extract into `Scoring` when non-trivial OR duplicated; otherwise inline at the trigger site is fine.** FastDrop crosses the bar only because we already have a `Scoring` class to put it in — if F-26 hadn't created one, leaving FastDrop's one-liner inline would be the right call.

**Severity:** design-quality (refactor)
**Status:** noted
**Next step:** lands as part of F-26's commit (or as a tiny follow-on after):

1. **Rename** F-26's planned `Scoring/BurgerScoring.cs` → `Scoring/Scoring.cs`; class name `Scoring`. Burger methods become `Scoring.GetBurgerTier(int)` and `Scoring.CalculateBurgerPoints(int)`.
2. **Add `Scoring.CalculateFastDropPoints(float distance)`** as a one-line static: `Mathf.RoundToInt(distance * GameplayConfig.FAST_DROP_POINTS_PER_UNIT)`.
3. **Update `Ingredient.FastDrop:238`** to call `Scoring.CalculateFastDropPoints(distanceToLand)`. Keep the `GameManager.AddExtraScore` call and `FloatingText.Spawn` inline (orchestration stays at the trigger site, matching F-26's BurgerAnimator pattern).
4. **Update `AudioManager.HandleBurger`** (F-26 step 6) to use `Scoring.GetBurgerTier(...)` (already adjusted in F-26's text).

Net: one consolidated scoring file across burger + fast-drop; future scoring concerns (match points, gem pickup, challenge multipliers) land there too if they grow past one-liners.

### Other observations (not findings)

- **`Debug.Log` in `Initialize:53`** — leftover spawn log, noisy in production. Strip when the file is next touched (likely with F-33). Not worth its own commit.
- **Defensive `if (_spriteRenderer == null)` pattern repeated** in Awake (`:26-29`) and Initialize (`:40-43`) — same shape as the ChefController defensive-Awake pattern noted in the README tracking section. **Second sighting** — this would normally formalize into a codebase-wide finding, but both Awake+Initialize re-fetching is also redundant. Tracker for a future folder if a third instance appears.

---

## `Ingredients/IngredientSpawner.cs`

### F-35 — `SpawnerState` enum should be a top-level file
**Found in:** `Ingredients/IngredientSpawner.cs:8`
**What:** `private enum SpawnerState { Idle, Delaying, WaveFalling, WaitingForLand }` is nested inside `IngredientSpawner`. Violates the project no-nested-types rule (CLAUDE.md "Code Conventions").
**Severity:** cosmetic
**Status:** noted
**Next step:** promote to `Ingredients/SpawnerState.cs`. Co-lands with F-36, which edits the same enum's membership.

### F-36 — Dead `Idle` state is redundant with `_active`; delete it
**Found in:** `Ingredients/IngredientSpawner.cs:8` (enum), `:36` (field init), `:73` (active gate), `:77-78` (`case Idle`)
**What:** Two mechanisms encode "not running": the `_active` bool (master switch) and `SpawnerState.Idle`. The `case SpawnerState.Idle: break;` is **unreachable**:
- `_state == Idle` only at construction (`:36`). `StartSpawning` immediately moves it to `Delaying`; nothing ever sets it back to `Idle`.
- `StopSpawning` sets `_active = false` but leaves `_state` intact, so `ResumeSpawning` continues mid-lifecycle.
- `Update` runs `if (!_active) return;` (`:73`) *before* the switch — so whenever `_state` could be `Idle`, we've already returned.

`_active` is the legitimate orthogonal pause/stop modifier — consistent with the **F-21** decision (pause is a *modifier*, not a state). `Idle` is just a never-ticking construction sentinel.
**Severity:** design-quality
**Status:** noted
**Decision:** Option A — delete `Idle`.
**Next step:**
1. Drop `Idle` from the enum (with F-35, `SpawnerState.cs` becomes `{ Delaying, WaveFalling, WaitingForLand }`).
2. Default the field to `_state = SpawnerState.Delaying` (`:36`). Inert before `StartSpawning` because `_active` gates `Update`; `StartSpawning` re-arms it anyway.
3. Delete `case SpawnerState.Idle: break;` (`:77-78`).

Result: the enum matches the actual runtime cycle (`Delaying` one-shot → `WaveFalling` ⇄ `WaitingForLand`); `_active` stays the orthogonal pause modifier.

### F-37 — Spawner consumes raw level only for wave-size; consolidate into DifficultyManager
**Found in:** `Ingredients/IngredientSpawner.cs:39` (`_currentLevel`), `:102-105` (`SetCurrentLevel`), `:210-219` (`GetWaveSize`); relates to `notes.md:285`
**What:** The spawner takes three difficulty setters from DifficultyManager: `SetFallSpeed` + `SetActiveIngredientCount` (already-*derived* values) and `SetCurrentLevel` (the *raw* level). `_currentLevel` is consumed by exactly one method — `GetWaveSize()`'s triple-wave-chance curve (`:212-217`). So the spawner is the lone remaining place that maps level→value, while DifficultyManager owns every other level→value lerp. Split source of truth. Upgrades the "acceptable" note at `:285` now that the spawner side makes the asymmetry visible.
**Severity:** design-quality
**Status:** noted
**Next step:** move the triple-wave-chance computation (`:212-217`) into DifficultyManager and push it via a value setter (`SetTripleWaveChance(float)`, or fold into the existing per-level push). Delete `SetCurrentLevel` + `_currentLevel` from the spawner; `GetWaveSize()` reads the pushed chance (returns 3 if `Rng.Value < chance`, else 2). Spawner becomes purely value-driven and no longer references the "level" concept; all level→value authority lives in DifficultyManager.

### F-38 — Extract `WaveComposer` (wave-composition logic) from IngredientSpawner
**Found in:** `Ingredients/IngredientSpawner.cs` — `RollWaveData:193`, `GetWaveSize:210`, `GetUnusedColumn:221`, `GetSpawnType:233`, `GetBunType:256`, `CountBottomBunsOnGrid:266`, `GridHasBottomBun:285`
**What:** At 378 lines the spawner carries a self-contained "*what* to spawn" chunk — column/type selection, bun-pacing (`_spawnsSinceLastBun`), and bun-type rules — distinct from its "*when/how* to spawn" orchestration (state machine, `SpawnIngredient`, tap handling). This is where the spawn-distribution balance rules live.

Not an animation split — the F-26/F-30 animation theme has **no** third instance here: `Ingredient` owns its fall anim, `WavePreviewManager` owns preview-blink. This is the SRP principle applied to composition logic instead.
**Severity:** design-quality (optional split)
**Status:** noted
**Next step:** extract a `WaveComposer` (the listed methods) that returns wave data given `(activeCount, grid state, tripleChance)`. Spawner orchestrates + spawns; composer decides composition. Drops the spawner under ~300 lines and isolates the tuning surface. Composes cleanly with F-37 (composer takes the pushed triple-chance instead of the raw level).

---

## `Ingredients/IngredientType.cs`

### F-39 — Enum int values are load-bearing in 4 places; introduce explicit `REGULAR_INGREDIENTS` list
**Found in:** `Ingredients/IngredientType.cs:6-16`; consumed at `IngredientSpawner.cs:249` (`(IngredientType)roll`), `IngredientSpawner.cs:134` (`Clamp(count, 1, 7)`), `BurgerChallenge.cs:188` (`(IngredientType)available[idx]`), `IngredientType.cs:28` (`(int)type >= 0 && <= 6`)
**What:** The integer layout (regulars `0..6` contiguous, gap `7-9`, buns `10,11`) is load-bearing in four sites, none referencing a constant — a duplicate source of truth for "there are 7 regular ingredients indexed 0..6":
1. `IngredientSpawner:249` — `(IngredientType)roll`, `roll ∈ [0, activeCount)`. Works only because regulars start at 0 and are contiguous.
2. `BurgerChallenge:188` — `(IngredientType)available[idx]`, `available = [0, activeCount)`. Same hidden assumption.
3. `IngredientSpawner:134` — `Mathf.Clamp(count, 1, 7)`; the `7` is just "regular count."
4. `IngredientType:28` — `(int)type >= 0 && (int)type <= 6` range check.

The reserved gap `7-9` before the buns is a fragile "magic gap" reserved for regular-ingredient growth. Reordering buns to `0,1` (a tempting "special cases first" layout) would *break* sites 1 & 2 unless a `+ BUN_COUNT` offset is added — which just relocates the coupling. **No serialization risk:** `SaveDataManager` does not persist `IngredientType` ints (verified).
**Severity:** design-quality
**Status:** noted
**Next step:**
1. Add `public static readonly IngredientType[] REGULAR_INGREDIENTS = { Meat, Cheese, Tomato, Onion, Pickle, Lettuce, Egg }` to `GameplayConfig` (balance/progression order; matches the "Egg appears in advanced phases" intent).
2. `IngredientSpawner:249` → `return GameplayConfig.REGULAR_INGREDIENTS[roll];`
3. `BurgerChallenge:188` → index `GameplayConfig.REGULAR_INGREDIENTS` instead of casting.
4. `IngredientSpawner:134` → clamp upper bound `GameplayConfig.REGULAR_INGREDIENTS.Length` (kills magic `7`).
5. Delete `IngredientTypeExtensions` (`IsBun`, `IsRegularIngredient`) — already covered by **F-27**; the `0..6` range check has nothing left to justify it after step 2-4. `IngredientType.cs` collapses to just the enum.

Result: enum int values stop being load-bearing — adding an ingredient = append one line to the list; renumbering buns to `0,1` becomes optional/cosmetic, not required. Composes with **F-38** (`WaveComposer` consumes the same list) and **F-37**.

---

## `Ingredients/WavePreviewManager.cs`

### F-40 — Promote the `(IngredientType type, int columnIndex)` wave-slot tuple to a named type
**Found in:** `WavePreviewManager.cs:14, 29, 57, 68`; also `IngredientSpawner.cs:43` (`_nextWaveData`), `:193` (`RollWaveData` return), `:89` (`ShowPreviews` call)
**What:** The anonymous tuple `(IngredientType type, int columnIndex)` is the "wave slot" concept, repeated across `_data` + all four public signatures here and three spawner sites. It's also the natural return type of F-38's `WaveComposer`. Anonymous structure smeared across many signatures with no single source of truth for its shape.
**Severity:** design-quality
**Status:** noted
**Shared with:** **F-32** (promote `MatchResult` struct to top-level) — same pattern.
**Next step:** introduce `struct WaveSlot { IngredientType Type; int ColumnIndex; }` (own file, `Ingredients/WaveSlot.cs`). Replace the tuple across the preview manager + spawner; make it F-38's `WaveComposer` return type. Composes with F-41.

### F-41 — Parallel lists `_data` / `_previews` can desync (latent index-misalignment bug)
**Found in:** `WavePreviewManager.cs:14-15` (the two lists), `:32` vs `:40` (unequal population), `:70-88` (`TryTap` index assumption)
**What:** `TryTap` assumes `_data[i]` pairs with `_previews[i]`, but `ShowPreviews` populates them unequally: `_data` gets **every** entry (`:32`), while `_previews` only gets one per entry surviving `col != null` (`:37`) **and** `CreatePreview != null` (sprite present, `:40/:113`). Any skipped entry diverges the indices → `TryTap` reads/removes the wrong `_data[i]`. Doesn't fire today only because columns are always valid and sprites always assigned — a correctness landmine behind an invariant, not a live crash.
**Severity:** design-quality (latent bug)
**Status:** noted
**Shared with:** global "no duplicate sources of truth / fix the structure, not the sync" — same root as the parallel-state findings F-21, F-33.
**Next step:** collapse to a single `List<(GameObject preview, WaveSlot slot)>` (uses F-40's type). Desync becomes unrepresentable. Fold in the minor cleanups below while here:
- Delete dead `transform.DOKill()` calls (`:80, :100`) — only `sr.DOFade` (`:46`) is ever tweened; nothing tweens `transform`.
- Extract a `DestroyPreview(...)` helper for the duplicated DOKill+Destroy block (`:80-83` / `:100-103`) — falls out naturally from the paired struct.
- Drop the redundant `_previewManager?.ClearPreviews()` in `IngredientSpawner.OnDestroy:366` — the manager's own `OnDestroy:130` already covers it (same GameObject).

### F-42 — `CreatePreview` takes a `Column` but uses only `ColumnIndex`; drop it and decouple `GridManager`
**Found in:** `WavePreviewManager.cs:110` (`CreatePreview(type, column)`), `:116` (only use: `column.ColumnIndex`), `:36` (`GridManager.Instance?.GetColumn(colIdx)`)
**What:** `CreatePreview` reads only `column.ColumnIndex` (`:116`) — which `ShowPreviews` already holds as the loop's `colIdx`. The `GetColumn` lookup at `:36` exists purely to pass back an object whose only-used field the caller already had, and is the manager's **sole** `GridManager` coupling. The null-check guards an invariant that already holds (`GetUnusedColumn` only emits valid `0..COLUMN_COUNT-1`).
**Severity:** design-quality
**Status:** noted
**Shared with:** project "minimize singleton coupling / hidden dependencies."
**Next step:** change to `CreatePreview(IngredientType type, int columnIndex)`; delete the `GridManager.Instance?.GetColumn` call in `ShowPreviews`. Manager is left with one clean injected dependency (`_getSprite`) and no singleton reach.

### F-43 — `sortingOrder = 90` magic number → named constant
**Found in:** `WavePreviewManager.cs:122`
**What:** Inline `sr.sortingOrder = 90`. Sorting orders are set inline in several places (ingredients, previews, popups); a bare `90` in logic with no named home.
**Severity:** cosmetic
**Status:** noted
**Shared with:** **F-14 / F-16** (structural-vs-balance constants split) — render-layer order is structural, *not* an F-24 UI-color concern.
**Next step:** move to a named constant in **`Constants`** — render-layer/sorting order is **structural** (programmer-owned, never tuned for feel), not `UIStyles` (visual style) or a balance config. ~~If a second inline `sortingOrder` turns up in a later folder, promote to a codebase-wide "sorting-order constants" sweep.~~ **Second sighting hit** (`GemPack.cs:19`, `sortingOrder = 100`) — promoted to a codebase-wide sorting-order-constants sweep; see **F-50**.

---

## `Input/TouchInputHandler.cs`

### F-44 — Defensive Awake dependency resolution (3rd sighting → codebase-wide); kill the `FindAnyObjectByType` variant
**Found in:** `Input/TouchInputHandler.cs:21-29`; prior sightings `Chef/ChefController` + `Ingredients/Ingredient` (README "Tracking patterns")
**What:** Component self-resolves its dependencies at startup behind null-check fallbacks: `if (_chef == null) _chef = FindAnyObjectByType<ChefController>();` (+ `Camera.main`, `_spawner`). Third instance of the tracked defensive-resolve pattern → promote to a codebase-wide finding.

Two sub-variants now distinguished:
- **`GetComponent<X>()` variant** (ChefController, Ingredient) — cheap, local, self-resolving. Mild.
- **`FindAnyObjectByType<X>()` variant** (here, `:24, :28`) — scene-wide scan, slow, order-dependent. The dangerous one.

Deeper smell in a code-first project (CLAUDE.md: "no manual scene configuration"): the `[SerializeField]` refs are almost certainly never wired in the inspector, so the "fallback" scan is actually the *primary* resolution path — the `[SerializeField]` advertises injection that never happens.
**Severity:** design-quality
**Status:** noted
**Shared with:** README tracker (now formalized); project "minimize singleton coupling / hidden dependencies."
**Next step:** standardize on **explicit code injection** — the owner that creates these objects passes references at creation time; delete the `FindAnyObjectByType` fallbacks (kill the scene-scan variant first). Document the convention in CLAUDE.md and apply across `ChefController`, `Ingredient`, `TouchInputHandler`. Dependency flow becomes visible and deterministic.

### F-45 — Gesture (swipe/tap) vs mode (Drag/Tap) axes tangled; duplicated logic + colliding method names
**Found in:** `Input/TouchInputHandler.cs` — `ProcessInput:122`, `ProcessDragMode:143`, `ProcessTapMode:162`, `ProcessTap:207`
**What:** Gesture detection is correctly shared (`isSwipe` computed once at `:130-131`), but everything after the mode fork is tangled:
1. **Swipe→move duplicated** — `ProcessDragMode:147-154` and `ProcessTapMode:167-174` are the same horizontal-swipe-to-move block. Both modes treat swipe identically.
2. **Preview/falling world-tap detection duplicated** — the `ScreenToWorldPoint` + `TryTapPreview` + `TryTapFalling` sequence appears in both `ProcessTap:214-221` and `ProcessTapMode:180-189`. Shared by both modes.
3. **Colliding names** — `ProcessTap` (`:207`) is the *Drag*-mode tap handler; `ProcessTapMode` (`:162`) is the *Tap*-control-mode handler. Two "tap" methods meaning different things; `ProcessTap` is reached *from Drag mode*.

The two axes are orthogonal: gesture (swipe vs tap) is mode-independent; mode only changes what a *tap* means (Drag → always `SwapPlates`; Tap → near-chef ? swap : move toward tapped side).
**Severity:** design-quality
**Status:** noted
**Next step:** restructure `ProcessInput`: (1) compute gesture once; (2) if swipe → shared `MoveChefHorizontal(dir)` helper; (3) if tap → shared world-pos + `TryTapPreview`/`TryTapFalling` (return if consumed); (4) single mode switch for the remaining tap-intent. Collapses the 3 `Process*` methods into one flow + small helpers, removes both duplications, and disambiguates naming. Fold in while here:
- Inline magic numbers, routed **by kind** (not lumped into "constants"): `10f` `ScreenToWorldPoint` z (`:180, :214`, ×2) is an **algorithmic** projection distance → keep as a named local/const at the call site, not a tuning config. `2f` in `_chef.BubbleRadius * 2f` (`:193`) is a **tap-tolerance multiplier → `GameplayConfig`**, beside its siblings `FALLING_TAP_RADIUS_MULT` / `PREVIEW_TAP_RADIUS_MULT`. (Not an F-24 concern — F-24 is UI colors.)
- `ControlMode.Drag` hardcoded fallback (`:128`) duplicates SaveDataManager's canonical default — no-silent-defaults; reference the one canonical default instead.
- Rename `_isDragging` (`:19`) → `_pressActive` / `_gestureInProgress` (it's true between press and release in *both* modes, so "dragging" is misleading).

---

## `Monetization/AdManager.cs`

(Mock internals — `Debug.Log`s, `TODO`s, `1f`/`2f` delays, `IsAdAvailable() => true` — are intentional placeholders, not review findings; they're replaced wholesale when the real SDK lands. Ad-realism work is tracked in `Docs/pre-launch-checklist.md` → "Monetization (Ads)", deliberately out of review scope. The `Time.timeScale` save/restore concern is captured there too, not as a finding.)

### F-46 — Singleton guard boilerplate duplicated across all managers (codebase-wide)
**Found in:** `Monetization/AdManager.cs:17-28`; same pattern across the documented singleton list — `SaveDataManager`, `MusicManager`, `GameManager`, `GridManager`, `AudioManager`, `BurgerChallenge` (CLAUDE.md "Singletons")
**What:** Every manager repeats the identical Awake guard:
```csharp
if (Instance != null && Instance != this) { Destroy(gameObject); return; }
Instance = this;
DontDestroyOnLoad(gameObject); // (some)
```
~7 copies of the same source of truth for "how a singleton initializes." AdManager also exposes the bug that hides in this boilerplate: **no `OnDestroy` nulls `Instance`**, so a destroyed-and-reloaded singleton can leave a stale `Instance` reference (likely true of the others too).
**Severity:** design-quality
**Status:** noted
**Shared with:** project "minimize singleton coupling"; same DRY/duplicate-source spirit as F-1 (`EnsureComponent` lift).
**Next step:** consolidate into a `Singleton<T>` MonoBehaviour base (or a shared init helper) that owns the guard + `Instance` assignment + `OnDestroy` null-out in one place; managers derive from it. **Caveat — do NOT add lazy auto-creation:** Unity `Singleton<T>` bases commonly bolt on access-before-Awake auto-spawn, which breaks the project's *documented* init order (CLAUDE.md). Base provides the guard/teardown only; explicit creation + ordering stays as-is. Verify each class's exact boilerplate (incl. `DontDestroyOnLoad` vs not) before collapsing — not all are identical.

---

## `Monetization/GemPack.cs` + `GemPackSpawner.cs`

### F-47 — GemPack is invisible: no sprite is ever assigned (confirmed bug)
**Found in:** `GemPack.cs:18-20` (SpriteRenderer created, `sortingOrder` + `color` set, **no `.sprite`**); `GemPackSpawner.cs:53` (built from bare `new GameObject`, not a prefab)
**What:** `Initialize` adds a `SpriteRenderer` and sets its color but never assigns `.sprite`, and the spawner constructs the object from scratch (no prefab carrying a sprite). A `SpriteRenderer` with a color and no sprite **renders nothing** — the gem pack is invisible. The `CircleCollider2D` still works, so it's invisible-but-tappable: players are expected to tap a collectible they can't see. The "Create a simple diamond shape via scale" comment (`:22`) is misleading — it scales uniformly, there is no diamond.
**Severity:** blocker
**Status:** noted
**Note:** confirmed with the developer this is **not** an intentional placeholder — it was unknown until this review (contrast the deliberate Special Orders silhouette placeholder).
**Next step:** assign a sprite in `Initialize` (placeholder or real gem art), or route construction through a prefab that owns the sprite. If a placeholder is acceptable short-term, make it deliberate + documented rather than a silently-empty renderer.

### F-48 — `OnMouseDown` bypasses the New Input System (potential device bug + inconsistency)
**Found in:** `GemPack.cs:58` (`OnMouseDown`)
**What:** Gem-pack tap detection uses Unity's legacy `OnMouseDown`, while the rest of the game routes through `TouchInputHandler` + `EnhancedTouch` (New Input System). `OnMouseDown` only fires when Active Input Handling is "Both" (or Old); under "Input System Package (New)" **only**, it never fires → gem packs become uncollectable on device. Even when it works, it's a second input paradigm that skips the central handler used everywhere else (and the preview/falling taps already demonstrate the routed pattern).
**Severity:** design-quality (device-blocker if input handling is New-only)
**Status:** noted
**Next step:** confirm the project's Active Input Handling setting. Regardless, route gem-pack taps through `TouchInputHandler` (world-pos hit-test against active gem packs, same shape as `TryTapPreview`/`TryTapFalling`) so all tap input flows through one path.

### F-49 — Animation interleaved with construction/logic in `Initialize` + `Collect`
**Found in:** `GemPack.cs:13-56` (`Initialize`: sprite/collider construction + 3 tween setups), `:63-82` (`Collect`: gem-award logic + log + collect tween sequence)
**What:** Same animation-vs-logic mixing flagged in F-18 / F-26 / F-30. `Initialize` interleaves object construction with three DOTween animation setups; `Collect` interleaves the award/persist logic with the collect-and-destroy animation.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-18, F-26, F-30 (animation decoupled from logic/state).
**Next step:** separate the visual/animation concern from the gem-pack logic — e.g. a small animation helper (fly-in path + spin + pulse + collect sequence) distinct from `Collect`'s award/persist logic. Given the class is ~90 lines, a full MonoBehaviour split may be over-engineering; minimally group construction → animation → logic into named methods (`BuildVisual`, `PlayFlyIn`, `PlayCollect`) so the logic isn't buried in tween chains. Match the altitude chosen for F-26/F-30.

### F-50 — Gem-pack magic numbers → `MonetizationConfig` (advances F-14/F-16); promotes F-43 sorting-order sweep
**Found in:** `GemPack.cs:19` (`sortingOrder = 100`), `:27` (`col.radius = 0.8f`), `:31` (wobble `Rng.Range(-1f,1f)`); `GemPackSpawner.cs:43` (`> 0.5f`), `:45` (`screenEdge = 5f`), `:46` (`yPos 0..3`), `:49` (wobble), `:51` (`duration 3..5f`); plus `Constants.GEM_PACK_*`
**What:** Spawn geometry, durations, and feel values are raw literals in logic; the `GEM_PACK_*` values that *are* named live in `Constants` — exactly the entries F-14 marked for relocation. This is the concrete justification for the **MonetizationConfig** file F-16 proposed.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-14 (move balance out of `Constants`), F-16 (adopt `MonetizationConfig`), F-43 (sorting-order constants).
**Next step:** create `MonetizationConfig` (per F-16) and route the gem-pack literals **by who tunes them** — *not* all into one file (correcting an earlier over-bundling):
- **`MonetizationConfig`** (business/PM — drop economics): spawn interval, spawn chance, gem value.
- **`AnimConfig`** (motion feel): wobble range, fly-across duration range.
- **`Constants`** (structural): collider radius, spawn-edge X geometry, Y range, direction-split coin-flip, and `sortingOrder` (via the sweep below).

Replace the inline literals in both files with the routed references. Fold in while here:
- `sortingOrder = 100` is the **second** inline `sortingOrder` (preview = `90`, F-43) → **promote F-43 to a codebase-wide sorting-order-constants sweep** (named layer constants, no bare ints).
- Delete the dead `_collider` field (`GemPack.cs:9, :28`) — assigned, never read; make it a local.
- Collapse the redundant `_moveTween?.Kill()` + `DOTween.Kill(transform)` (`:75-76, :86-87`) — the path tween is on `transform`, so `DOTween.Kill(transform)` already covers it; drop the `_moveTween` field unless it's killed selectively (it isn't).
- `UIStyles.BTN_GEM_PACK` (a button color) is reused for a world-space sprite (`:20`) — UI style leaking into a world object (F-24/F-25 theme); give the gem pack its own color in `MonetizationConfig`/`UIStyles`.

### F-51 — `GemPackSpawner` subscribe-once init-order assumption → silent permanent no-op
**Found in:** `GemPackSpawner.cs:10-19` (`Start`)
**What:** The spawner learns when to start/stop only via `GameManager.OnStateChanged`, but it subscribes **once** in `Start` and **only if** `GameManager.Instance != null` at that instant (`:14`). If `Start` ever runs before `GameManager` initializes, it never subscribes and `_isActive` (set false at `:17-18`) never changes again — nothing else flips it. Result: the spawner silently no-ops for the **entire session** (no gem packs, no error, no recovery). Doesn't bite today only because `GameManager` inits early in the documented singleton order — but it's an unguarded init-order assumption with a silent-failure mode.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44 (components reaching for dependencies that may not be ready).
**Next step:** remove the subscribe-timing dependency — either subscribe defensively (retry/late-subscribe), or drop the cached `_isActive` + one-shot subscription and read `GameManager.Instance?.CurrentState == GameState.Playing` directly in `Update`. The latter also deletes `HandleStateChanged` and the `OnDestroy` unsubscribe — simpler and order-independent.

---

## `UI/Background.cs`

### F-52 — `BackgroundType` is a second top-level type in the file → own file
**Found in:** `UI/Background.cs:5-9` (`BackgroundType` enum) alongside `Background` (`:11`)
**What:** Two top-level types share one file. Not nested (beside the class, not inside), but still violates the project "one top-level type per file" rule.
**Severity:** cosmetic
**Status:** noted
**Shared with:** F-35 (same file-org rule).
**Next step:** promote `BackgroundType` to `UI/BackgroundType.cs`.

### F-53 — Camera-fill sizing/positioning duplicated across `FitToCamera` + `CreateFilter`
**Found in:** `UI/Background.cs:107-123` (`FitToCamera`), `:79-105` (`CreateFilter`); `Camera.main` at `:97` and `:109`; redundant position write at `:24`
**What:** `CreateFilter` reimplements `FitToCamera`'s camera-fill logic instead of sharing it — `camHeight = 2f * cam.orthographicSize; camWidth = camHeight * cam.aspect;` (+ scale-to-fill + position-to-cam-with-z) appears in both (`:112-113` and `:100-101`). This is the shared responsibility behind the "Generate/CreateFilter feel similar" observation (the texture generation differs; the *fill* logic is the dup). Also: `Camera.main` is fetched twice (tagged scene lookup each call), and `bgObj`'s position set at `:24` is immediately overwritten by `FitToCamera:122` (dead assignment).
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44 (reaching for a global dependency — `Camera.main` — at runtime).
**Next step:** extract one helper — "size + position this `SpriteRenderer` to fill the camera at z=Z" — called by both the bg and the filter. Cache `Camera.main` once in `Start`, pass it in. Drop the redundant `:24` position write.

### F-54 — Magic numbers (z-depths, texture params, filter-opacity default) → named constants
**Found in:** `UI/Background.cs:24, 122` (z `10f`), `:104` (z `9.9f`), `:27` (`sortingOrder = -100`), `:87` (`-99`), `:44-45` (gradient `width=2, height=256`), `:76` (PPU `100f`), `:94` (filter PPU `1f`), `:15` (`_filterOpacity = 0.35f` inline default)
**What:** Layering depths, texture dimensions, pixels-per-unit, and the filter-opacity default are raw literals. The BG colors already come from `UIStyles` (good); the literals below route to **different** homes (see Next step) — they do *not* all belong in `UIStyles`.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-43 + F-50 — `sortingOrder = -100/-99` is the **third** inline-sorting-order sighting (preview `90`, gem `100`); roll these into the codebase-wide sorting-order-constants sweep.
**Next step:** route **by kind**:
- **`Constants`** (structural layering): z-depths (`10f`, `9.9f`) and sorting orders (`-100`, `-99`) — sorting orders fold into the F-50 sweep.
- **stay at the implementation** (algorithmic): gradient `width=2, height=256` and PPUs (`100f`, `1f`) — texture-resolution constants nobody tunes for feel; keep as named locals/consts in `Background`, *not* a config file.
- **`UIStyles`** (visual feel): the `_filterOpacity` default (`0.35f`), beside the BG colors already there.

### F-55 — Generated textures/sprites leak (no cleanup) + not cached
**Found in:** `UI/Background.cs:47, 76` (gradient `Texture2D` + `Sprite`), `:91, 94` (1×1 white filter `Texture2D` + `Sprite`); no `OnDestroy`
**What:** Two issues, one asset-lifecycle theme:
- **Leak:** `new Texture2D(...)` / `Sprite.Create(...)` create unmanaged resources Unity does not GC. There's no `OnDestroy` destroying them, so each scene reload (Menu↔Game) allocates fresh textures while the old ones leak.
- **No caching:** violates the global "cache generated assets — don't regenerate identical assets" rule. The 1×1 white pixel is the textbook reusable (other UI likely needs it; `UIFactory` may already provide one).
**Severity:** design-quality
**Status:** noted
**Shared with:** **F-65** (now the codebase-wide home for "scattered runtime sprite generators leak"); this is instance #1. Asset/resource-hygiene cousin of the DOTween-kill-on-destroy theme; check `UIFactory` for an existing shared white sprite when implementing.
**Next step:** add `OnDestroy` that `Destroy()`s the generated `Texture2D` + `Sprite` (only the runtime-generated ones, not an inspector-assigned `_backgroundSprite`). Share a single cached 1×1 white sprite (lift to `UIFactory` if not already there) instead of regenerating per-`Background`.

---

## `UI/BurgerChallenge.cs`

485-line god-class mixing ~5 responsibilities: singleton lifecycle, challenge **logic** (order generation, `IsOrderMatch`, level/progress, multipliers), **scoring/award** (`HandleBurgerCompleted`), **UI construction**, and **animation**. The findings below split into one anchor (F-56) + two execute-with-the-anchor items (F-57, F-59) + three independent quick-wins (F-58, F-60, F-61).

### F-56 — Split `BurgerChallenge` into challenge model (logic) + view (UI/animation) [ANCHOR]
**Found in:** `UI/BurgerChallenge.cs` — logic: `:151-213, :339-407, :439-456`; scoring: `:355-392`; UI: `:79-149, :215-318, :473-482`; animation: `:409-471`
**What:** One MonoBehaviour owns challenge state/logic, score computation, UI construction, and animation. The fusion tell: `IsOrderMatch` (`:339`) is a public **logic** API that **GridManager calls** (`:359` is its own consumer; GridManager calls it externally per CLAUDE.md) — an external system reaches into a UI class for match-checking. Also `HandleBurgerCompleted` (`:360-367`) computes `basePoints * globalMult * challengeMult` and calls `GameManager.AddExtraScore` — scoring logic embedded in UI.
**Severity:** design-quality (large refactor)
**Status:** noted
**Shared with:** F-18, F-26/F-34 (scoring home), F-30, project "no god classes / separate layout from behavior."
**Next step:** extract a plain challenge **model** (OrderType, required size/targets, `GenerateNewChallenge`, `GenerateContainsIngredients`, `BuildContainsName`, `IsOrderMatch`, level/progress, `GetGlobalMultiplier`) with no MonoBehaviour/UI deps; GridManager talks to the model. The MonoBehaviour becomes the **view** (construction + animation) reacting to model events. Route the score award through the Scoring home (F-26/F-34) + GameManager rather than computing it here. **F-57 and F-59 land as part of this split.**

### F-57 — Dedup `CreateUI` world-TMP setups + visual builders via shared helper [execute as part of F-56 — do not land standalone]
**Found in:** `UI/BurgerChallenge.cs:79-149` (`CreateUI`), `:269-318` (`CreatePlaceholderVisual`/`CreateIngredientVisual`)
**What:** `_titleText`/`_nameText`/`_levelText` each repeat the same ~10-line world-`TextMeshPro` setup (AddComponent → fontSize → color → alignment → sortingOrder → outline → RectTransform sizeDelta). `CreatePlaceholderVisual`/`CreateIngredientVisual` are near-identical. This is exactly what the `UIFactory` refactor (roadmap item 4) was meant to absorb, but this class hand-rolls it.
**Severity:** design-quality
**Status:** noted
**Sequence:** lands with **F-56**'s view side; do not refactor standalone (the split restructures these methods anyway).
**Next step:** **UIFactory does NOT cover world-space `TextMeshPro`** (confirmed by review — it's `TextMeshProUGUI`/screen-space only). So add a new **`WorldTextFactory`** sibling (in the `UI/Factory/` folder — see the UI Construction Layer synthesis) for world-space TMP construction, plus a single ingredient/placeholder visual builder; the view calls them. Do not extend `UIFactory` for this — different component domain.
**Second consumer:** `BurgerPopup.CreateTexts` (`:18-50`) repeats the same ~8-line world-TMP boilerplate for `_nameText` + `_scoreText` — migrate it to `WorldTextFactory` when it lands (see F-62).

### F-58 — Nested `OrderType` enum → own file [independent, any order]
**Found in:** `UI/BurgerChallenge.cs:28`
**What:** `private enum OrderType { Size, Contains }` nested in the class — violates the no-nested-types rule.
**Severity:** cosmetic
**Status:** noted
**Shared with:** F-35, F-52 (file-org rule).
**Next step:** promote to `UI/OrderType.cs` (or co-locate with the extracted model from F-56).

### F-59 — Repeated `FindAnyObjectByType<IngredientSpawner>()` (3 sites, per regen) → resolve once [execute as part of F-56 — do not land standalone]
**Found in:** `UI/BurgerChallenge.cs:209` (`GetActiveIngredientCount`), `:217` (`CreateSizeVisual`), `:235` (`CreateContainsVisual`)
**What:** Three scene-wide `FindAnyObjectByType<IngredientSpawner>()` scans, re-run on **every** `GenerateNewChallenge` (i.e. every match/level-up). F-44 expensive-scan/defensive-resolution theme plus a perf smell.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44.
**Sequence:** subsumed by **F-56** — the split should inject the spawner (or just its sprite-lookup + active-count) into the model/view once. Doing it standalone first risks being thrown away by the split.
**Next step:** resolve the spawner dependency once at init and pass it in; delete the per-call lookups.

### F-60 — Magic numbers + dead math → constants/`UIStyles` [independent, any order]
**Found in:** `UI/BurgerChallenge.cs:414` (no-op math), `:46-48` (misleading `_meterX/_meterY` "Cached" comment), plus layout literals at `:88, :99, :104, :106, :113, :128, :134, :139, :148, :240, :374` and `GenerateRectSprite` dims `:475-481`
**What:** Layout positions, sizeDeltas, spacing math, sorting-order arithmetic (`_sortingOrder + 1/+2+i`), and `GenerateRectSprite` dimensions are raw literals. Two specific items:
- **Dead math** (`:414`): `float fullBottom = _meterY - _meterHeight * 0.5f + _meterHeight * 0.5f;` cancels to `_meterY` — a confusing no-op.
- **Misleading comment** (`:46-48`): `_meterX`/`_meterY` labeled "Cached meter positions" but they're hardcoded defaults, nothing is cached.
**Severity:** design-quality
**Status:** noted
**Already covered elsewhere (do not re-flag):** inline `_meterBgColor`/`_meterFillColor` (`:25-26`) → **F-24**; the `0.4f` flash duration (`:468`) → **F-11**.
**Next step:** route **by kind**: layout positions/sizeDeltas/spacing → **`UIStyles`** (it owns spacing/sizes); the `_sortingOrder + 1/+2+i` arithmetic → **`Constants`** (structural layering, via the F-50 sweep); `GenerateRectSprite` dims (`4×4`, `4f` PPU) are **algorithmic** → keep at the implementation (and they likely vanish under F-61's shared-sprite swap, *not* a config file). Simplify `:414` to `_meterY`; rename/relocate `_meterX/_meterY` (drop the false "cached" comment).

### F-61 — `GenerateRectSprite` texture leak + not cached [independent, any order]
**Found in:** `UI/BurgerChallenge.cs:473-482` (`GenerateRectSprite`), called at `:120, :130`
**What:** Generates a 4×4 white `Texture2D` + `Sprite` per meter element (×2), never destroyed (no `OnDestroy` cleanup of these) — and a white rect is a reusable. **Second sighting** of the runtime-generated-texture-leak-and-no-caching pattern (first: F-55, Background).
**Severity:** design-quality
**Status:** noted
**Shared with:** **F-65** — instance #2 of the codebase-wide "scattered runtime sprite generators leak" finding (F-65 is the home; F-55 is #1). Share one cached white sprite via the shared generator; destroy generated textures on teardown. Also add `OnDestroy` `DOKill` for the `LevelUpEffect`/meter tweens while here.
**Next step:** replace `GenerateRectSprite` with a shared cached white sprite (lift to `UIFactory`); ensure generated textures are destroyed on teardown. (The `4×4`/`4f` dims are **algorithmic** — if any generated-texture path survives the shared-sprite swap, those stay as named locals at the implementation, never a tuning config.)

---

## `UI/BurgerPopup.cs`

Otherwise clean — construction (`CreateTexts`) and animation (`Animate`) are already separated into distinct methods (the good pattern F-49/F-56 want elsewhere), and all animation values correctly live in `AnimConfig`.

**Already covered / rides existing sweeps (no new tag):**
- `Color.white` for score text (`:42`) → already named in **F-24** (replace with `UIStyles.TEXT_UI`).
- `sortingOrder = 110` (`:29, :46`, duplicated literal) → **fourth** inline-sorting-order sighting; rides the F-43/F-50 `Constants` sweep.
- Duplicated ~8-line world-TMP setup for `_nameText` + `_scoreText` → second consumer of F-57's world-TMP helper (noted on F-57).

### F-62 — `BurgerPopup` DOTween cleanup gap + `sizeDelta` magic numbers + `SetParent` nit
**Found in:** `UI/BurgerPopup.cs:52-73` (`Animate`, no `OnDestroy`), `:68-69` (untargeted `DOTween.To`), `:32, :49` (`sizeDelta` literals), `:36` (`SetParent` without `false`)
**What:** Three this-file items:
1. **DOTween hygiene (headline):** the popup self-destroys via `OnComplete` (`:72`) but has **no `OnDestroy`** killing the sequence. The risk spot is the two custom alpha tweens at `:68-69` — `DOTween.To(() => _nameText.alpha, x => _nameText.alpha = x, ...)` captures the text in a closure with **no `SetTarget`**, so if the object is destroyed mid-animation (e.g. scene unload before the hold/fade ends) DOTween can't auto-kill them and the setter runs against a destroyed object. Every other tweening class here (`GemPack`, `WavePreviewManager`, `BurgerChallenge`) kills on teardown — BurgerPopup is the odd one out.
2. **`sizeDelta` magic numbers** — `new Vector2(6f, 2f)` (`:32`), `(4f, 1.5f)` (`:49`); the only literals left (animation values are already in `AnimConfig`). UI sizes → **`UIStyles`** (spacing/sizes home).
3. **`SetParent(transform)` without `false`** (`:36`) — defaults `worldPositionStays=true`, inconsistent with the `SetParent(x, false)` used in `Background`/`BurgerChallenge`; harmless (localPosition set right after at `:37`) but a consistency nit.
**Severity:** design-quality
**Status:** noted
**Shared with:** DOTween-kill-on-destroy hygiene theme (GemPack/WavePreviewManager); `sizeDelta`→`UIStyles` config-home theme.
**Next step:** add `OnDestroy` that kills the sequence (or `.SetTarget(...)` on the `:68-69` tweens); move the two `sizeDelta` literals to `UIStyles`; add `, false` to the `SetParent` at `:36`.
**Reclassified:** the DOTween-cleanup half is now instance #1 of the codebase-wide **F-73** (DOTween-kill-on-destroy hygiene). The `sizeDelta`/`SetParent` items stay local to F-62.

---

## `UI/GameHUD.cs`

Mostly clean — uses `UIFactory.CreateCanvas`, has a real `CreateHUDText` helper, and subscribe/unsubscribe are symmetric.

### F-63 — Event-wiring robustness in `SubscribeEvents`/`OnDestroy`
**Found in:** `UI/GameHUD.cs:62-73` (`SubscribeEvents`), `:91-102` (`OnDestroy`); `FindAnyObjectByType<DifficultyManager>()` at `:67` and `:96`; init at `:18-19, :36-37`; null-checks at `:77, :82, :87`
**What:** Several event-wiring issues in one place:
1. **Double `FindAnyObjectByType<DifficultyManager>()`** (`:67` subscribe, `:96` unsubscribe) — two scene scans (F-44 theme), and the unsubscribe only works if the *same* instance is returned at destroy time. `GameManager`/`SaveDataManager` use cached `.Instance` singletons so they're symmetric; `DifficultyManager` isn't a singleton, so it's the outlier that should be cached in a field.
2. **Subscribe-once-if-ready silent-failure** — `SubscribeEvents` wires each source only `if (… != null)` at `Start`. If a source isn't alive yet at `Start`, the subscription is silently skipped and that HUD line never updates all session. **Second sighting** of the F-51 pattern (GemPackSpawner) — one more → formalize codebase-wide (cf. F-44).
3. **Inconsistent initialization** — score via `UpdateScore(0)` and level via `UpdateLevel(1)` in `Start`, but gems set *inline* in `CreateHUDElements` (`:36-37`). Two patterns for three texts; the hardcoded `0`/`1` also duplicate the canonical initial values (should read `GameManager` score / `DifficultyManager` level).
4. **Null-check asymmetry** — `UpdateGems` null-checks `_gemText` (`:87`) but `UpdateScore`/`UpdateLevel` don't (`:77, :82`).
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44 (FindAnyObjectByType), F-51 (subscribe-once init-order silent-failure — 2nd sighting).
**Next step:** cache the `DifficultyManager` in a field resolved once; subscribe/unsubscribe against it. Make subscription order-independent (subscribe defensively, or read sources lazily). Route all three texts through their `Update*` method from a single init path seeded from the real sources (not hardcoded `0`/`1`). Apply the null-check (or drop it) consistently across the three updaters.

### F-64 — HUD layout magic numbers → `UIStyles`
**Found in:** `UI/GameHUD.cs:24` (`startY = -10f`), `:46-47` (anchors `0.06f/0.93f`, `0.46f/0.93f`), `:49` (`anchoredPosition (10f, …)`), `:50` (`sizeDelta (0, 35)`)
**What:** Anchor/offset/size layout literals are raw in `CreateHUDText`/`CreateHUDElements`. Font sizes + line-spacing already live in `UIStyles` (good) — these should join them.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-24-adjacent config-home theme (UI spacing/sizes → `UIStyles`).
**Next step:** move the anchor/offset/`sizeDelta`/`startY` literals to `UIStyles` (UI layout/sizes home). **UIFactory review done:** `UIFactory.CreateText` exists but **hardcodes center anchors** (`UIFactory.cs:63-64`, `anchorMin/Max = 0.5,0.5`) — that's exactly why `GameHUD` rolled its own `CreateHUDText` (it needs top-corner anchoring). So this is an **in-domain extension**: add `anchorMin`/`anchorMax`/`pivot` params (or an overload) to `UIFactory.CreateText`, then delete `CreateHUDText` and have GameHUD use the factory. (See UI Construction Layer synthesis.)
**Second consumer:** `MainMenuUI:61-68` calls `CreateText` then **post-patches** the RectTransform anchors to top-right (gem counter) — same limitation; the anchor params delete that block too (see F-72).

---

## `UI/GameLayout.cs`

Pure presentation (three 9-slice panel frames) — no logic mixing. Credit: `TEX_SIZE = 128` is correctly a `const` at the implementation (`:25`) — algorithmic-constant-stays-at-impl done right.

### F-65 — [codebase-wide] Scattered runtime sprite/texture generators leak → consolidate into a shared generator
**Found in:** `UI/GameLayout.cs:50-101` (`Generate9SliceSprite`, `Texture2D` `:57` + `Sprite` `:92`, no `OnDestroy`); instances also at **F-55** (`Background` gradient + 1×1 white), **F-61** (`BurgerChallenge.GenerateRectSprite` 4×4 white)
**What:** Third sighting of runtime-generated-texture-leak. Each UI class hand-rolls `new Texture2D(...)` + `Sprite.Create(...)` and never destroys it (no `OnDestroy`), leaking on scene reload. GameLayout caches its sprite within the class (generated once, shared across 3 panels — good) but still leaks it. Three sightings → formalize like the sorting-order sweep: the fix is **one shared generator** (a `SpriteFactory` sibling — *not* `UIFactory`, which the review confirmed is screen-space UGUI only) owning generation + caching + disposal, rather than four classes each rolling and leaking their own.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-55, F-61 (now retro-classified as instances of this codebase-wide finding); F-65 is the home (mirrors how F-50 became the sorting-order sweep home).
**Next step:** introduce a **`SpriteFactory`** sibling (in the `UI/Factory/` folder — see the UI Construction Layer synthesis) that generates white-pixel / gradient / rounded-rect-9-slice sprites, caches reusable results, and exposes/handles disposal. Confirmed by the UIFactory review that this is **out of UIFactory's domain** (UIFactory is screen-space UGUI only) → a sibling, not an extension. Migrate `Background`, `BurgerChallenge`, `GameLayout`. Until it lands, each instance needs an `OnDestroy` that `Destroy()`s its generated `Texture2D` + `Sprite`.

### F-66 — GameLayout magic numbers → routed homes
**Found in:** `UI/GameLayout.cs:41` (`z = 5f`), `:44` (`sortingOrder = -50`), `:22-23` (`_borderWidth = 4`, `_cornerRadius = 24`), `:8-17` (panel centers/sizes SerializeField defaults)
**What:** Layering, border-style, and panel-geometry literals are raw/inline-defaulted.
**Severity:** design-quality
**Status:** noted
**Already covered / classified (do not re-flag):** `_borderColor`/`_fillColor` (`:20-21`) → **F-24**; `TEX_SIZE` (already an impl `const`) and the `dist / 1.5f` AA falloff (`:74`) are **algorithmic** → stay at impl.
**Shared with:** F-43/F-50 sorting-order sweep (`-50` is the **fifth** sighting).
**Next step:** route **by kind**: `z = 5f` + `sortingOrder = -50` → **`Constants`** (structural layering; sorting folds into the F-50 sweep); `_borderWidth`/`_cornerRadius` + panel centers/sizes → **`UIStyles`** (visual style + UI layout sizes).

---

## `UI/GameOverPanel.cs`

One of the cleanest files — all construction delegated to `UIFactory`, clean event hygiene, `SetUpdate(true)` correct for the paused fade.

**Already covered (do not re-flag):** `Constants.CONTINUE_GEM_COST` (`:54, :104, :105, :126`) → **F-14/F-16** (monetization → `MonetizationConfig`); `ShouldShowInterstitial()` (`:148`) → **F-22** (its named consumer).
**Tracked (DOTween-hygiene):** no `OnDestroy` killing the `Show()` sequence; `DOTween.To` closure captures `_canvasGroup` with no `SetTarget` (`:113`) — instance #2 of the codebase-wide **F-73**.

### F-67 — High-score persistence lives inside the panel's `Show()`
**Found in:** `UI/GameOverPanel.cs:96` (`SaveDataManager.Instance.SetHighScore(score)` in `Show`)
**What:** `Show()` is a UI display method, but it performs a game-flow **persistence side-effect** — committing the high score. The panel becoming visible shouldn't own "save the high score"; that's a game-over flow concern. Re-entering `GameState.GameOver` (continue → die again) re-runs it (harmless since it's a max, but conceptually misplaced). Lighter cousin of the F-56 logic-in-UI smell.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-56 (logic/persistence misplaced in UI).
**Next step:** move `SetHighScore` into the game-over flow (e.g. `GameManager` when it transitions to `GameOver`); the panel reads the already-persisted value for display only.

### F-68 — `DifficultyManager.CurrentLevel` reached via `FindAnyObjectByType` in 3 sites (not a singleton)
**Found in:** `UI/GameOverPanel.cs:89` (`Show`); also `GameHUD.cs:67, :96` (F-63)
**What:** `DifficultyManager` isn't a singleton, so consumers scan for it with `FindAnyObjectByType` to read `CurrentLevel`/subscribe to `OnLevelChanged` — three sites now. F-44 expensive-scan theme, but the root is architectural: the current level has no cheap, stable access point.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44 (FindAnyObjectByType), F-63 (per-file cache was the local fix), F-37 (level authority lives in DifficultyManager).
**Next step:** give the current level a single stable access point — expose `CurrentLevel` + `OnLevelChanged` through `GameManager` (already a singleton) which forwards from `DifficultyManager`, OR make `DifficultyManager` a singleton. Consumers then read/subscribe without scanning. Supersedes F-63's local-cache workaround if adopted.

### F-69 — GameOverPanel layout magic numbers → `UIStyles` (button y-stack is a derivable sequence)
**Found in:** `UI/GameOverPanel.cs:44-75` (element positions/sizes)
**What:** Element positions/sizes are inline literals. The button y-positions `30 / -45 / -120 / -195` (`:54, :63, :69, :74`) are a hardcoded arithmetic sequence (75px spacing) — derivable from a start + spacing constant rather than four magic offsets. Sizes `(350, 50)` / `PANEL_BUTTON_SIZE` etc. are partly in `UIStyles` already.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-64, F-66 (UI layout magic → `UIStyles`).
**Next step:** move positions/sizes to `UIStyles`; express the button stack as `start - i * spacing` from named constants instead of four literals.

---

## `UI/UIFactory.cs`

Clean, cohesive, single-domain (screen-space UGUI: `CreateCanvas`, `EnsureEventSystem`, `CreateText`, `CreateButton`, `CreateOverlay`, `CreatePanel` — all `RectTransform`/`Image`/`TextMeshProUGUI`). **Not** a god class. Plus: outline styling is centralized here (`:74-75, :117-118`), so the documented TMP-outline fix becomes a one-place change later. The decision this review unblocks is captured in the synthesis below.

### F-70 — UIFactory internal duplication + one `SetParent` nit
**Found in:** `UI/UIFactory.cs:111-118` (button label TMP setup duplicates `CreateText:68-75`); `RectTransform` setup repeated across `CreateText:62-66`, `CreateButton:91-95`, `CreatePanel:150-153`, `CreateOverlay:131-134`; `SetParent(parent)` without `false` at `:22`
**What:** `CreateButton` re-rolls the same TMP configuration `CreateText` already does, and the anchor/position/size `RectTransform` block repeats across four methods. Also `CreateCanvas` is the lone `SetParent(parent)` missing `, false` (everything else passes it) — same nit family as F-62.
**Severity:** cosmetic
**Status:** noted
**Next step:** have `CreateButton` build its label via `CreateText`; extract a tiny `ConfigureRect(rect, anchorMin, anchorMax, pos, size)` helper used by the four constructors. Add `, false` to `CreateCanvas`'s `SetParent` (`:22`). Low risk — internal-only, no API change.

---

## Synthesis — UI Construction Layer (links F-56, F-57, F-64, F-65, F-70)

**Decision (after reviewing `UIFactory`):** `UIFactory` is small, cohesive, and screen-space-UGUI only — **keep it, do not split it.** The construction gaps live in *other* domains it deliberately doesn't cover. Resolve by forming a **`UI/Factory/` folder** = the existing factory + domain siblings, each single-concern:

| Class | Domain | Status | Closes |
|---|---|---|---|
| `UIFactory` (keep; maybe rename `CanvasFactory`) | Screen-space UGUI (Canvas/RectTransform/Image/UGUI text) | exists; **extend** with anchor params on `CreateText` | F-64 |
| `WorldTextFactory` (new sibling) | World-space `TextMeshPro` construction | new | F-57 (BurgerChallenge + BurgerPopup consumers) |
| `SpriteFactory` (new sibling) | Procedural sprite gen + cache + disposal | new | F-65 (Background + BurgerChallenge + GameLayout) |

**Rationale:** matches the project's "no god classes" + "folder structure mirrors responsibility domains." The three domains have genuinely different component models (Canvas/RectTransform vs world Transform+TMP vs Texture2D/Sprite), so one mega-factory would be the wrong call. This is *not* speculative API — each sibling is justified by real, already-logged duplication (F-56/F-57/F-64/F-65).

**Sequencing:** `WorldTextFactory` + `SpriteFactory` are prerequisites for the **F-56** BurgerChallenge split's view side (F-57 builds on `WorldTextFactory`; the meter sprites build on `SpriteFactory`). Build the factories first, then migrate consumers (GameHUD→F-64, BurgerChallenge/BurgerPopup→`WorldTextFactory`, Background/BurgerChallenge/GameLayout→`SpriteFactory`). `UIFactory`'s own cleanup (F-70) is independent and can land anytime. The F-64 anchor-param extension has **two** consumers waiting (GameHUD `CreateHUDText`, MainMenuUI gem-counter post-patch).

---

## `UI/MainMenuUI.cs`

Uses `UIFactory` well; clean gem-event hygiene. **Already covered:** the `if (X.Instance == null) { new GameObject + AddComponent }` ×3 (`:18-34`) is the re-implementation **F-1** named (lift `EnsureComponent` to Util).

### F-71 — Bootstrap + audio-init logic lives in the menu UI
**Found in:** `UI/MainMenuUI.cs:18-34` (bootstraps `SaveDataManager`/`AdManager`/`MusicManager`), `:36-37` (`AudioListener.volume` + `MusicManager.ApplySoundSetting()`)
**What:** `MainMenuUI.Start` brings up three persistent global managers and applies the global sound setting. That's app-init / audio-system logic in a *UI* class. The menu is the first scene, but "bootstrap persistent managers + apply sound settings" is an app-startup concern, not the menu's view code. Lighter cousin of F-56/F-67 (logic misplaced in UI).
**Severity:** design-quality
**Status:** noted
**Shared with:** F-1 (the `EnsureComponent` re-implementation here), F-56/F-67 (logic-in-UI placement).
**Next step:** extract a dedicated bootstrap entry point (an `AppBootstrap`/`Bootstrapper` MonoBehaviour, or route through `GameManager`'s manager-ensuring) that brings up the managers and applies sound settings on app/scene start; have it use F-1's lifted `EnsureComponent` helper. The menu UI then only builds UI.

### F-72 — MainMenuUI layout magic numbers + gem-counter anchor post-patch
**Found in:** `UI/MainMenuUI.cs:51, 56, 61` (positions `(0,300)/(0,230)/(0,400)` + sizes `(400,60)/(200,40)`), `:71` (`btnY = 80f`), `:61-68` (gem-counter post-patch), `:13, :128` (fully-qualified types)
**What:** Title/high-score/gem positions+sizes and the `btnY` start are inline literals → `UIStyles`. The gem counter calls `CreateText` then overrides anchors/pivot/position to top-right (`:63-68`) — second consumer of F-64's anchor-param extension; the `new Vector2(0, 400)` arg (`:61`) is **dead** (overwritten by `(-20,-20)` at `:67`). **Credit:** the button stack already uses `btnY + SPACING * n` (`:75-85`) — the derivable pattern F-69 wants for GameOverPanel; only `btnY` needs a home.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-64 (anchor params — 2nd consumer), F-69 (button-stack derivation, done right here), F-66 (layout → `UIStyles`).
**Next step:** move positions/sizes/`btnY` to `UIStyles`; after F-64's anchor params land, replace the gem-counter post-patch (`:61-68`) with a single `CreateText(..., anchorMin/Max: topRight)` call and drop the dead `(0,400)` arg. Minor: add `using UnityEngine.UI;` and drop the `UnityEngine.GameObject`/`UnityEngine.UI.Button` qualifiers (`:13, :128`).

---

## `UI/ScorePopup.cs`

Tiny rise-and-fade "+points" popup. **Cross-refs (no new tag):** `sortingOrder = 100` (`:18`) → **6th** sorting-order sighting, F-50 sweep; defensive `if (_text == null) _text = GetComponent<TextMeshPro>()` (`:13-14`) → instance of the **F-44** defensive-resolution pattern (GetComponent variant).

### F-73 — [codebase-wide] DOTween-kill-on-destroy hygiene (formalized at 3rd sighting)
**Found in:** `UI/ScorePopup.cs:27-31` (sequence, untargeted `DOTween.To` at `:29`, no `OnDestroy`); instances also **F-62** (`BurgerPopup`), `GameOverPanel.Show` (`:113`)
**What:** Classes that build DOTween tweens/sequences but don't kill them on teardown — the dangerous variant being `DOTween.To(() => obj.alpha, x => obj.alpha = x, …)` with **no `SetTarget`**, whose getter/setter closure captures a renderer/text DOTween can't auto-associate and kill. If the GameObject is destroyed mid-animation (scene unload before the tween completes), the setter runs against a destroyed object. Most tweening classes (`GemPack`, `WavePreviewManager`, `BurgerChallenge`) kill correctly; the popups/panels are the gaps. Third sighting → formalize codebase-wide (cf. F-44, F-65). F-73 is the home; F-62 is instance #1.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-62 (BurgerPopup — now an instance), GameOverPanel (`:113`, instance), ScorePopup (this).
**Next step:** establish the convention — any class building tweens kills them in `OnDestroy` (`transform.DOKill()` / `sequence.Kill()`), and custom `DOTween.To` tweens always set `.SetTarget(obj)` so target-based kill works. Apply to `ScorePopup`, `BurgerPopup` (F-62), `GameOverPanel`. **Reference pattern:** `FloatingText:31` already does it right (`tmp.DOFade(...)` is target-based → auto-killed); the fix is to make the others look like that. Folds naturally into the F-75 popup consolidation (shared helper would centralize correct teardown).

### F-74 — ScorePopup `0.8f` scale literal → `AnimConfig`
**Found in:** `UI/ScorePopup.cs:30` (`transform.DOScale(0.8f, …)`)
**What:** The fade-out scale-down target `0.8f` is the lone inline animation literal — rise-distance and duration already live in `AnimConfig` (good). Same flavor as F-11.
**Severity:** cosmetic
**Status:** noted
**Next step:** add `POPUP_FADE_SCALE = 0.8f` (or shared with the other popups) to `AnimConfig`; reference it.

---

## `UI/FloatingText.cs`

Static `Spawn` factory for a world-space rise-fade "+text" popup. **Positive example for F-73:** fades via `tmp.DOFade(...)` (`:31`) — **target-based**, so DOTween auto-kills it on destroy. That's the reference pattern F-73 wants for the untargeted-`DOTween.To` offenders. **Cross-refs:** `sortingOrder = 100` (`:21`) → 7th sorting-order sighting (F-50 sweep); world-TMP construction (`:16-26`) → `WorldTextFactory` consumer.

### F-75 — Consolidate the three world-space rise-fade-destroy popups
**Found in:** `UI/FloatingText.cs` (static `Spawn`), `UI/ScorePopup.cs`, `UI/BurgerPopup.cs`
**What:** Three classes duplicate the same shape — build a world-space `TextMeshPro` (outline/alignment/sortingOrder/sizeDelta) → rise (`DOMove`/`DOMoveY`) → fade (`DOFade`/alpha) → `Destroy` on complete. Confirmed at the third instance (watch raised after ScorePopup). The construction half is a `WorldTextFactory` consumer; the animation half is its own duplicated concern.
**Severity:** design-quality
**Status:** noted
**Shared with:** UI Construction Layer synthesis (`WorldTextFactory`), F-57 (world-TMP helper), F-62/F-74 (popup specifics).
**Next step:** build the text via `WorldTextFactory`, and extract a shared **popup-rise-fade-destroy** animation helper (parameterized by rise distance, duration, optional fade-scale F-74, fade-delay) that all three call. Keep the target-based tween approach FloatingText already uses (F-73). Fold the `sizeDelta = (4f,1f)` default (`FloatingText:26`) and `BurgerPopup` sizeDeltas (F-62) into the factory/`UIStyles`. Note the API difference: FloatingText is a static `Spawn`, the others are `Initialize`-on-component — unify or keep two thin entry points over the shared helper.

### F-76 — Dead `_prefab` static field in FloatingText
**Found in:** `UI/FloatingText.cs:9` (`private static GameObject _prefab;`)
**What:** Declared, never read — `Spawn` builds a `new GameObject` from scratch. Leftover from an intended prefab/pooling approach that never landed.
**Severity:** cosmetic
**Status:** noted
**Shared with:** global "no speculative API surface."
**Next step:** delete the field. (Not a cue to add pooling — re-add only if a real perf need appears.)

---

## `UI/SettingsPanel.cs`

Uses `UIFactory`; clean lazy `Show`/`Hide` (no DOTween → no F-73 concern).

### F-77 — `FindAnyObjectByType<Canvas>()` grabs an arbitrary canvas
**Found in:** `UI/SettingsPanel.cs:35`
**What:** `CreatePanel` parents its overlay onto `FindAnyObjectByType<Canvas>()` — the first canvas found, which may not be the intended one. F-44 scan theme plus a correctness fragility. SettingsPanel is `AddComponent`'d by `MainMenuUI`, which owns `_canvas`.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-44 (FindAnyObjectByType).
**Next step:** inject the canvas — have `MainMenuUI` pass its `_canvas` (e.g. an `Initialize(Canvas)` or a `Show(Canvas)` param) instead of scanning. **Second instance confirmed:** `ShopPanel.cs:31` does the identical `FindAnyObjectByType<Canvas>()` — same fix applies to both.

### F-78 — Duplicate "apply sound setting" logic across SettingsPanel + MainMenuUI
**Found in:** `UI/SettingsPanel.cs:73-74`; duplicated at `UI/MainMenuUI.cs:36-37`
**What:** `AudioListener.volume = <bool> ? 1f : 0f;` + `MusicManager.Instance?.ApplySoundSetting()` appears in **both** places — "how to apply the sound setting" has two sources of truth, both in UI. `MusicManager.ApplySoundSetting()` exists but doesn't own the `AudioListener.volume` mapping, so each caller re-does it.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-71 (audio init in UI), global "no duplicate sources of truth."
**Next step:** make one audio-service method own the full apply (`AudioListener.volume` from `SaveDataManager.SoundOn` **+** music) — fold the `volume = ? 1f : 0f` into `MusicManager.ApplySoundSetting()` (or an `AudioManager` method). `SettingsPanel` and `MainMenuUI`/bootstrap (F-71) then call the single method; the `1f/0f` leaves the UI.

### F-79 — SettingsPanel layout magic + minor cleanups
**Found in:** `UI/SettingsPanel.cs:44-63` (positions/sizes), `:48, :55` (empty `""` button labels), `:81, :100` (silent defaults), `:35/_canvas` field
**What:** Title/button positions `(0,130)/(0,50)/(0,-20)/(0,-110)` + sizes `(300,50)` are inline → `UIStyles`. Plus:
- Sound/control buttons created with empty `""` labels (`:48, :55`) then overwritten by `Update*Label`; the arg is dead and `UIFactory` names the GameObjects `""` (two blank-named objects).
- `UpdateSoundLabel`/`UpdateControlLabel` hardcode `true`/`Drag` fallbacks when `SaveDataManager` is null (`:81, :100`) — silent defaults duplicating SaveDataManager's canonical defaults (F-45 theme).
- `_canvas` is a field used only in `CreatePanel` — could be local.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-69/F-72 (layout → `UIStyles`), F-45 (silent ControlMode default).
**Next step:** move positions/sizes to `UIStyles`; give the toggle buttons a real initial label (or a stable GameObject name); read defaults from SaveDataManager's canonical source rather than re-hardcoding; make `_canvas` local.

---

## `UI/ShopPanel.cs`

Last UI file. Mirrors `SettingsPanel` structurally (lazy `Show`/`Hide`, `UIFactory`, no DOTween). **Cross-refs:** `FindAnyObjectByType<Canvas>()` (`:31`) → instance #2 of **F-77**; `Constants.GEM_REWARD_AD` (`:73`) → F-14/F-16; IAP `AddGems` stubs (`:82, :88`) → documented placeholder, pre-launch checklist (IAP receipt validation).

### F-80 — Shop gem-balance display goes stale after grants
**Found in:** `UI/ShopPanel.cs:45` (`"Your gems: {gems}"` set once), grants at `:73, :82, :88`
**What:** The balance text is set at panel creation and never updated, but the panel's own buttons add gems while it's open (watch-ad reward, IAP buys). After any grant the displayed balance is stale until the panel is reopened. `ShopPanel` doesn't subscribe to `SaveDataManager.OnGemsChanged` (MainMenuUI does for its counter, but not this panel).
**Severity:** design-quality (visible stale UI)
**Status:** noted
**Next step:** keep a reference to the balance text; subscribe to `OnGemsChanged` (unsubscribe on destroy/hide) or refresh it after each grant and on `Show`.

### F-81 — IAP/reward amounts + prices hardcoded and duplicated (drift bait)
**Found in:** `UI/ShopPanel.cs:49` ("+25" label) vs `:73` (`GEM_REWARD_AD` grant); `:53` ("100 / $0.99") vs `:82` (`AddGems(100)`); `:57` ("500 / $3.99") vs `:88` (`AddGems(500)`)
**What:** Each pack's amount (and IAP price) is hardcoded in the button label string **and** again in the grant call — two sources of truth. The watch-ad label literally hardcodes "+25" while the grant uses `Constants.GEM_REWARD_AD`; the label lies if the constant drifts. IAP amounts (100/500) and prices ($0.99/$3.99) are pure inline magic.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-16/F-50 (`MonetizationConfig`), global "no duplicate sources of truth."
**Next step:** model the products as data — a small IAP/reward product table in `MonetizationConfig` (amount, price, label-or-derive). Derive button labels from the amount/price; grants read the same entries. (Distinct from the `GemPack` *collectible* class — these are store products.) The IAP stubs stay until real IAP lands (checklist), but the **data** should be de-duplicated now.

### F-82 — ShopPanel layout magic + `_canvas` local
**Found in:** `UI/ShopPanel.cs:40-62` (positions/sizes), `:31/_canvas` field
**What:** Title/balance/button positions `(0,160)/(0,110)/(0,40)/(0,-40)/(0,-120)/(0,-190)` + sizes `(350,50)` inline → `UIStyles`; `_canvas` used only in `CreatePanel` → local. Mirrors F-79.
**Severity:** design-quality
**Status:** noted
**Shared with:** F-64/F-66/F-69/F-72/F-79 (UI layout magic → `UIStyles`) — see the UI Layout Magic synthesis below.
**Next step:** move positions/sizes to `UIStyles`; make `_canvas` local. Land as part of the layout-magic sweep.

---

## Synthesis — UI Layout Magic sweep (F-64, F-66, F-69, F-72, F-79, F-82)

Inline UI position/size/offset literals were flagged in six files: `GameHUD` (F-64), `GameLayout` (F-66), `GameOverPanel` (F-69), `MainMenuUI` (F-72), `SettingsPanel` (F-79), `ShopPanel` (F-82). Like the sorting-order (F-50) and generated-sprite (F-65) patterns, this is recurring and best executed as **one `UIStyles` pass** rather than six isolated edits — define the panel layout constants (and reusable button-stack `start + i*spacing` helpers, per F-69/F-72) together so spacing stays consistent across screens. Not a separate finding tag; a sequencing note so an implementer batches the six.

---

## Project root + assets

Final pass — `CLAUDE.md` accuracy, scene/prefab/folder hygiene, stray assets. (No interfaces exist in the codebase, so the `Abstractions/` convention has nothing to place — no finding.)

### F-83 — Orphaned music track: `Menu_2.wav` can never play
**Found in:** `Assets/_Project/Resources/Music/Menu_2.wav`; `Audio/MusicManager.cs:32-33` (`Resources.LoadAll<AudioClip>("Music/MenuTrack")` / `"Music/GameTrack"`)
**What:** MusicManager loads only from the `MenuTrack/` and `GameTrack/` subfolders, but `Menu_2.wav` sits in the `Music/` root (its sibling `Menu_1.wav` is correctly under `MenuTrack/`). So `Menu_2.wav` is never loaded — dead asset, and the menu has less music variety than intended.
**Severity:** design-quality (silent functional loss)
**Status:** noted
**Next step:** move `Menu_2.wav` into `Resources/Music/MenuTrack/` (or delete if unwanted). Consider whether MusicManager should warn on stray audio files directly under `Music/`.

### F-84 — `_Recovery/` junk scenes committed to the repo
**Found in:** `Assets/_Recovery/0.unity`, `0 (1).unity` (both **git-tracked**), `0 (2).unity`, `0 (3).unity` (untracked) + `.meta`s
**What:** Four Unity scene-recovery backups live in `Assets/_Recovery/`; two are committed. They're not real scenes (the project's scenes are `Assets/Scenes/Game.unity` + `MainMenu.unity`) — pure clutter that ships in the asset database and bloats the repo.
**Severity:** cosmetic (repo hygiene)
**Status:** noted
**Next step:** delete `Assets/_Recovery/` (and its `.meta`s); `git rm` the two tracked scenes. Add `_Recovery/` (or a recovery-glob) to `.gitignore` if the editor regenerates it.

### F-85 — Stray `nul` file at repo root
**Found in:** `./nul` (0 bytes, untracked)
**What:** A file literally named `nul` — a Windows reserved device name, almost certainly created by a `> nul` redirect run in a non-cmd shell. Zero bytes, no purpose.
**Severity:** cosmetic
**Status:** noted
**Next step:** delete it. (Untracked, so just a filesystem remove.)

### F-86 — CLAUDE.md Project Structure drift (Core/ config files omitted)
**Found in:** `CLAUDE.md` "Project Structure" → `Core/` line
**What:** The `Core/` listing names `GameManager, GameState, Constants, DifficultyManager, SaveDataManager, ControlMode, FeedbackManager, Rng, SceneLoader` but omits the three config files that live in `Core/` and are referenced everywhere — `AnimConfig`, `GameplayConfig`, `UIStyles` (the roadmap section even marks them DONE). Minor doc drift.
**Severity:** cosmetic
**Status:** noted
**Next step:** add `AnimConfig`, `GameplayConfig`, `UIStyles` to the `Core/` line. While there, reconcile the empty `Burger/`/`Scoring/` folders (F-7): note `Scoring/` is created by F-26/F-34 and `Burger/` should be deleted.

### Cross-ref — empty `Burger/` + `Scoring/` folders
Confirmed both `Assets/_Project/Scripts/Burger/` and `Scoring/` exist on disk with no files → **F-7** (delete `Burger/`; `Scoring/` gets populated by F-26/F-34). No new tag.




