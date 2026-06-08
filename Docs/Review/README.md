# Code Review

Human-led, file-by-file walk-through of the codebase. Same structure
as the AutoDeck M5 review (`../../../AutoDeck/_WorkingDocs/M5_Review/`)
adapted to a single-developer Unity project.

## What this folder is

A working space for the review. Two files:

- [`notes.md`](notes.md) — chronological log. Notes per file/area as we
  walk through them, in the order we look at them. Findings (`### F-N`)
  emerge inline; everything else is free-form prose.
- This `README.md` — process notes + the live index of findings (below).

[`EXECUTION.md`](EXECUTION.md) — landing-wave ordering for the 86 findings
(written at review completion; implementation sequences off it).

## Conventions

**Findings** are tagged `F-N` starting at F-1. Each one in `notes.md`
follows this template:

```markdown
### F-N — Short label
**Found in:** `path/to/file.cs:LINE` (or "across X")
**What:** One-line description.
**Severity:** blocker | design-quality | cosmetic | nit
**Status:** noted | fix-now | fix-later | accept | resolved
**Next step:** brief plan

(Optional longer discussion.)
```

**Severity** = how badly does this hurt us if we ship it as-is?
- *blocker* — broken behavior players will hit (crash, softlock, scoring wrong)
- *design-quality* — works, but a future-us will regret it (drift bait, hidden coupling, dead surface)
- *cosmetic* — drift, naming, dead comments
- *nit* — micro-style, not actionable

**Status** = where the item sits in the queue:
- *noted* — logged, no decision yet
- *fix-now* — will be addressed in this review pass
- *fix-later* — explicit defer to a future pass
- *accept* — decided to live with it; documented for transparency
- *resolved* — fixed during this review pass

**`notes.md` is findings-only.** Reviewed files with nothing to flag get
no entry. The README index here is the record that we walked through
them (by the absence of findings from that file). Decisions on findings
or open questions live inside the finding's own entry — not as standalone
"reviewed X" prose.

## What we're looking for

The cross-project principles in the global CLAUDE.md, applied to this
codebase:

- **No silent defaults** — inline `= 20` initializers on DTOs/configs drift; canonical defaults live in factory methods, consumers populate every field.
- **No duplicate sources of truth** — same value/constant/logic in two places is a drift bug waiting to happen. (CLAUDE.md mentions `GameplayConfig`, `UIStyles`, `AnimConfig`, `Constants` — verify nothing leaks back inline.)
- **Algorithmic vs balance constants** — balance lives in tunable config; algorithmic primes/multipliers live with the implementation that uses them.
- **No speculative API surface** — dead methods/fields/types get deleted; re-add when a real consumer needs them.
- **Typed identifiers over magic strings** — closed sets of strings become enums.
- **Polymorphic types when behavior differs per discriminator** — `switch (x.Type) { case "A": ...; case "B": ...; }` with substantially different work per case wants type-level dispatch.
- **Pure predicates don't mutate** — `IsX` / `HasX` / `CanX` answer questions without side effects.

Plus DogtorBurguer-specific concerns from the project CLAUDE.md:
- **Class size / responsibility split** — anything > ~300 lines or handling 2+ concerns
- **Explicit state machines vs boolean flag soup** — flag CSPs hiding what should be enums
- **Event-driven cross-system communication** — direct method calls across unrelated systems
- **Magic numbers in logic** — anything in code that should be in a config class
- **Singleton sprawl** — singletons that aren't true global services
- **DOTween hygiene** — tweens not killed on destroy
- **Rng usage** — any leak of `UnityEngine.Random` instead of `Rng`
- **One top-level type per file + `Abstractions/`** — file-org convention
- **`[SerializeField] private` over `public`** — Unity field convention

## Review order

Alphabetical by folder (matches on-disk listing):

1. `Audio/` — `AudioManager`, `MusicManager`
2. `Burger/` — (undocumented in CLAUDE.md; reconcile when reached)
3. `Chef/` — `ChefController`
4. `Core/` — primitives (`Constants`, `GameplayConfig`, `UIStyles`, `AnimConfig`, `Rng`, `GameState`, `ControlMode`); then managers (`SaveDataManager`, `DifficultyManager`, `FeedbackManager`, `SceneLoader`, `GameManager`)
5. `Grid/` — `MatchDetector`, `BurgerAnimator`, `Column`, `GridManager`
6. `Ingredients/` — `IngredientType`, `Ingredient`, `WavePreviewManager`, `IngredientSpawner`
7. `Input/` — `TouchInputHandler`
8. `Monetization/` — `AdManager`, `GemPack`, `GemPackSpawner`
9. `Scoring/` — (undocumented in CLAUDE.md; reconcile when reached)
10. `UI/` — `UIFactory` first; then `GameLayout`, `Background`, popups (`FloatingText`, `ScorePopup`, `BurgerPopup`), panels (`MainMenuUI`, `GameHUD`, `GameOverPanel`, `SettingsPanel`, `ShopPanel`, `BurgerChallenge`)
11. Project root + assets — `CLAUDE.md`, scene/prefab layout, folder hygiene

Not binding — re-order based on what we find.

---

## Live index of findings

| Tag | Label | Severity | Status |
|---|---|---|---|
| F-1 | `EnsureComponent<T>` helper should be reusable (lift to Util, replace `MainMenuUI` re-implementation) | design-quality | **partial** (burger-tier dup closed via Scoring.GetBurgerTier; the EnsureComponent lift itself is still open — folds into F-71) |
| F-2 | No path from procedural clips to authored audio assets (add per-slot `[SerializeField]` override) | design-quality | **resolved** |
| F-3 | `HandleBurger` if-elseif → switch expression | cosmetic | **resolved** |
| F-4 | Sample-generation skeleton repeats across `Generate*Samples` (gated on F-2 path) | design-quality | noted (deferred — risky DSP-math dedup, can't verify generated sound by ear; low value) |
| F-5 | Two category bools in `MusicManager` → `MusicCategory` enum | design-quality | **resolved** |
| F-6 | `MusicManager._source.volume = 0.5f` magic number (move to config) | cosmetic | **resolved** (new AudioConfig) |
| F-7 | Empty `Burger/` and `Scoring/` placeholder folders — populate or delete | cosmetic | **resolved** |
| F-8 | `ChefController._startPosition` SerializeField never varies; collapse to constant | cosmetic | **resolved** |
| F-9 | Position bubbles likely effectively invisible (low alpha + sortingOrder behind chef) — decide UX cue vs delete | design-quality | **resolved** |
| F-10 | `SwapPlates` flip-snap needs one-line explanatory comment | cosmetic | **resolved** |
| F-11 | Inline `0.4f` duration in `BurgerChallenge:468` should be a named AnimConfig const | design-quality | **resolved** |
| F-12 | `LAND_PUNCH_ELASTICITY` misnamed (used in wave punches too) — rename or split | cosmetic | **resolved** |
| F-13 | `Constants.CELL_HEIGHT` is unused — delete or derive `CELL_VISUAL_HEIGHT` from it | cosmetic | **resolved** |
| F-14 | `Constants.cs` mixes structural with balance/tuning — move 17 entries to `GameplayConfig` (+ possible new `MonetizationConfig`) | design-quality | **resolved** |
| F-15 | `CHEF_POSITION_COUNT` should be derived from `COLUMN_COUNT - 1` | design-quality | **resolved** |
| F-16 | Adopt 6-config-file architecture (`Constants` + `GameplayConfig` + `MonetizationConfig` + `AnimConfig` + `UIStyles` + `AudioConfig`) — implementation strategy for F-4/F-6/F-11/F-14 | design-quality | **resolved** (AudioConfig deferred until F-4/F-6) |
| F-17 | `DifficultyManager.EvaluateLevel` resets test-mode level on first ingredient placed | **blocker** (test feature) | **resolved** |
| F-18 | `FeedbackManager` mixes orchestration with asset construction — extract `SpriteUtils` + `ScreenFlashOverlay` + popup `Spawn` factories | design-quality | **resolved** (sprite gen via SpriteFactory instead of a new SpriteUtils) |
| F-19 | `GameManager.Start` doing four distinct phases — extract `ResolveDependencies` / `EnsureManagers` / `ApplyPersistedSettings` / `SubscribeEvents` | cosmetic | **resolved** |
| F-20 | Game-flow methods need explanatory comments for state-transition clarity | cosmetic | **resolved** |
| F-21 | `Paused` is structurally a modifier on `Playing` — refactor to internal `_isPaused` bool | design-quality | **resolved** |
| F-22 | `ShouldShowInterstitial` mixes persistence with ad-cadence logic — move to `AdManager` | design-quality | **resolved** |
| F-23 | `GameManager.RestartGame` bypasses `SceneLoader` — route through `SceneLoader.LoadGame()` | design-quality | **resolved** |
| F-24 | Inline UI style colors in `GameLayout` / `BurgerChallenge` / `BurgerPopup` should reference `UIStyles` | design-quality | **resolved** |
| F-25 | `Color` constructor inconsistency in `UIStyles` (redundant `1f` alpha on 5 entries) | cosmetic | **resolved** |
| F-26 | Split `BurgerAnimator` into Animation + `Scoring/Scoring` (consolidated per F-34) + `Scoring/BurgerNamer` + `Grid/BurgerData` top-level; closes F-1 + F-7-Scoring-half | design-quality | **resolved** |
| F-27 | `Column.CheckForMatch` collapses to `top.Type == second.Type` (+ delete now-dead `IsRegularIngredient` / `IsBun` / `IngredientTypeExtensions`) | design-quality | **resolved** |
| F-28 | `GridManager` overflow check fires before match check — kills "saving matches" | **blocker** (UX bug) | **resolved** |
| F-29 | Dead `SwapColumns` + `SwapColumnTops` methods in `GridManager` (~70 lines) | design-quality | **resolved** |
| F-30 | **[HIGH-RISK]** Extract `SwapAnimator` + event-based completion (replaces time-coupled `DelayedMatchCheck`) — test plan in notes | design-quality | noted |
| F-31 | Formalize game-input freeze during burger resolution; collapse burger detection logic (delete `DetectBurger` / `HasBunBelow` / `BurgerDetection` / `_columnsWithActiveBurger` / cascade re-check) | design-quality | noted |
| F-32 | Promote `MatchResult` struct to top-level type | cosmetic | **resolved** |
| F-33 | `Ingredient` two-bool `_isFalling` + `_isLanded` encoding of a 3-state lifecycle → `enum IngredientState { Spawned, Falling, Landed }` | design-quality | **resolved** |
| F-34 | Amend F-26: consolidate scoring into `Scoring/Scoring.cs` (general static class, not burger-specific) and absorb `Ingredient.FastDrop` calc into it | design-quality | **resolved** |
| F-35 | `SpawnerState` enum nested in `IngredientSpawner` → promote to own file | cosmetic | **resolved** |
| F-36 | Dead `Idle` state redundant with `_active` (`case Idle` unreachable) — delete it (Option A); `_active` stays as F-21-style modifier | design-quality | **resolved** |
| F-37 | Spawner consumes raw level only for `GetWaveSize` triple-chance — move computation to DifficultyManager, push value, delete `SetCurrentLevel`/`_currentLevel` (upgrades :285 note) | design-quality | **resolved** |
| F-38 | Extract `WaveComposer` (wave-composition logic) from 378-line `IngredientSpawner` — not animation; SRP split of "what to spawn" | design-quality | **resolved** |
| F-39 | `IngredientType` int values load-bearing in 4 sites — introduce `GameplayConfig.REGULAR_INGREDIENTS` list; enum order then free, buns→0,1 optional. Kills extensions (w/ F-27) | design-quality | **resolved** |
| F-40 | Promote `(IngredientType, int columnIndex)` wave-slot tuple to named `WaveSlot` struct (across preview mgr + spawner; F-38 return type) | design-quality | **resolved** |
| F-41 | `_data`/`_previews` parallel lists can desync (latent `TryTap` index bug) — collapse to one `(preview, slot)` list; folds in DOTween/teardown nits | design-quality | **resolved** |
| F-42 | `CreatePreview` takes `Column` but uses only `ColumnIndex` — drop to `int`, remove sole `GridManager` coupling | design-quality | **resolved** |
| F-43 | `sortingOrder = 90` → named constant in `Constants` (structural layering; F-14/F-16, not F-24) | cosmetic | **resolved** (via F-50 sweep) |
| F-44 | Defensive Awake dependency resolution (3rd sighting → codebase-wide) — kill `FindAnyObjectByType` scene-scan variant; standardize on explicit injection | design-quality | noted |
| F-45 | `TouchInputHandler` gesture-vs-mode tangle — dedup swipe-move + preview/falling tap, rename `ProcessTap`/`ProcessTapMode`; folds in magic-number + default + `_isDragging` nits | design-quality | noted |
| F-46 | Singleton guard boilerplate duplicated across ~7 managers + missing `Instance` null-on-destroy → `Singleton<T>` base (guard/teardown only, NO lazy auto-create — preserves documented init order) | design-quality | **resolved** |
| F-47 | GemPack invisible — `SpriteRenderer` created but `.sprite` never assigned (confirmed bug, not placeholder) | **blocker** | **resolved** |
| F-48 | GemPack `OnMouseDown` bypasses New Input System — device-blocker if input handling is New-only; route through `TouchInputHandler` | design-quality | **resolved** |
| F-49 | GemPack animation interleaved with construction/logic in `Initialize` + `Collect` (F-18/26/30 lineage) | design-quality | noted |
| F-50 | Gem-pack magic numbers routed **by kind** — `MonetizationConfig` (interval/chance/value) + `AnimConfig` (wobble/duration) + `Constants` (radius/geometry/sorting); advances F-14/F-16, promotes F-43 sweep; folds dead `_collider`, redundant kill, UI-color-on-world | design-quality | **partial** (sorting sweep done; gem-pack geometry/anim literals + color still inline) |
| F-51 | `GemPackSpawner` subscribe-once init-order assumption (`Start` only subscribes if `GameManager.Instance` ready) → silent permanent no-op; read state directly in `Update` instead | design-quality | **resolved** |
| F-52 | `BackgroundType` second top-level type in `Background.cs` → own file | cosmetic | **resolved** |
| F-53 | Camera-fill sizing/positioning duplicated across `FitToCamera`/`CreateFilter` → shared helper; cache `Camera.main`; drop dead `:24` position write | design-quality | **resolved** |
| F-54 | `Background` magic numbers routed **by kind** — `Constants` (z-depths/sorting, 3rd sighting → F-50 sweep) + stay-at-impl (algorithmic texture dims/PPUs) + `UIStyles` (filter opacity) | design-quality | **resolved** |
| F-55 | `Background` generated textures/sprites leak (no `OnDestroy`) + not cached (global cache-assets rule); destroy on teardown + share 1×1 white sprite | design-quality | **resolved** (via SpriteFactory cache) |
| F-56 | **[ANCHOR]** Split `BurgerChallenge` god-class → challenge model (logic) + view (UI/animation); GridManager talks to model; scoring via Scoring/GameManager | design-quality | **resolved** (playtested) |
| F-57 | Dedup `CreateUI` world-TMP setups + visual builders → new `WorldTextFactory` (UIFactory is UGUI-only) — *execute as part of F-56* | design-quality | **resolved** |
| F-58 | Nested `OrderType` enum → own file — *independent* | cosmetic | **resolved** |
| F-59 | Repeated `FindAnyObjectByType<IngredientSpawner>()` (3 sites/regen) → resolve once — *execute as part of F-56* (F-44 theme) | design-quality | **resolved** |
| F-60 | `BurgerChallenge` magic numbers + dead math (`:414` no-op, misleading `_meterX/Y`) routed **by kind** — `UIStyles` (layout/sizes) + `Constants` (sorting) + stay-at-impl (rect dims, algorithmic); *independent* (colors=F-24, flash dur=F-11) | design-quality | **resolved** (dead math + naming fixed by the F-56 split; positions → UIStyles) |
| F-61 | `GenerateRectSprite` texture leak + not cached — *independent*; 2nd sighting → codebase-wide generated-asset hygiene (w/ F-55) | design-quality | **resolved** (via SpriteFactory.White) |
| F-62 | `BurgerPopup` no `OnDestroy` DOTween kill (untargeted alpha `DOTween.To`) + `sizeDelta` magics → `UIStyles` + `SetParent(false)` nit | design-quality | **resolved** |
| F-63 | `GameHUD` event-wiring: cache `DifficultyManager` (double `FindAnyObjectByType` + unsubscribe symmetry) + subscribe-once silent-failure (2nd F-51 sighting) + init/null-check inconsistencies | design-quality | noted |
| F-64 | `GameHUD` layout magic numbers → `UIStyles`; + add anchor params to `UIFactory.CreateText` (centered-only at `:63-64`) so `CreateHUDText` can be deleted | design-quality | **resolved** |
| F-65 | **[codebase-wide]** Scattered runtime sprite/texture generators leak (no disposal) → new `SpriteFactory` sibling (gen + cache + dispose; not UIFactory — it's UGUI-only). Instances: F-55, F-61, GameLayout 9-slice (3rd sighting → formalized) | design-quality | **resolved** (ChefController/GemPack/FeedbackManager generators not yet migrated) |
| F-66 | `GameLayout` magic numbers — `z`/`sortingOrder` → `Constants` (5th sorting sighting → F-50 sweep); border/corner/panel geometry → `UIStyles` (colors=F-24; `TEX_SIZE`/`1.5f` algorithmic) | design-quality | **resolved** |
| F-67 | `GameOverPanel` high-score persistence (`SetHighScore`) inside UI `Show()` → move to game-over flow (F-56 cousin) | design-quality | **resolved** |
| F-68 | `DifficultyManager.CurrentLevel` reached via `FindAnyObjectByType` in 3 sites (not a singleton) → expose via `GameManager`; supersedes F-63 local cache (F-44 theme) | design-quality | **resolved** |
| F-69 | `GameOverPanel` layout magic numbers → `UIStyles`; button y-stack `30/-45/-120/-195` is a derivable start+spacing sequence | design-quality | **resolved** |
| F-70 | `UIFactory` internal dup (`CreateButton` label reuse `CreateText`; shared `ConfigureRect`) + `:22` `SetParent(false)` nit | cosmetic | noted |
| F-71 | `MainMenuUI` bootstraps managers + sets `AudioListener.volume` in `Start` (`:18-37`) → extract to bootstrap entry point (links F-1, F-56/F-67) | design-quality | noted |
| F-72 | `MainMenuUI` layout magic → `UIStyles`; gem-counter anchor post-patch = 2nd F-64 consumer + dead `(0,400)` arg; `using UnityEngine.UI` nit | design-quality | **resolved** |
| F-73 | **[codebase-wide]** DOTween-kill-on-destroy hygiene — missing `OnDestroy` kill + untargeted `DOTween.To` closures. Instances: F-62 (BurgerPopup), GameOverPanel, ScorePopup (3rd → formalized) | design-quality | **resolved** |
| F-74 | `ScorePopup` `0.8f` fade scale → `AnimConfig` | cosmetic | **resolved** |
| F-75 | Consolidate 3 world-space rise-fade-destroy popups (`FloatingText`/`ScorePopup`/`BurgerPopup`) → `WorldTextFactory` + shared animation helper | design-quality | noted |
| F-76 | Dead `_prefab` static field in `FloatingText` (`:9`) → delete | cosmetic | **resolved** |
| F-77 | `SettingsPanel` `FindAnyObjectByType<Canvas>()` grabs arbitrary canvas → inject from `MainMenuUI` (F-44 theme) | design-quality | noted |
| F-78 | Duplicate "apply sound setting" (`AudioListener.volume` + `ApplySoundSetting`) in `SettingsPanel` + `MainMenuUI` → one audio-service method (links F-71) | design-quality | noted |
| F-79 | `SettingsPanel` layout magic → `UIStyles`; empty `""` button labels, silent `true`/`Drag` defaults, `_canvas` local | design-quality | **resolved** |
| F-80 | `ShopPanel` gem-balance display goes stale after grants (no `OnGemsChanged` sub/refresh) | design-quality | **resolved** |
| F-81 | `ShopPanel` IAP/reward amounts+prices hardcoded & duplicated across labels and grants → product table in `MonetizationConfig` (drift bait) | design-quality | **resolved** |
| F-82 | `ShopPanel` layout magic → `UIStyles`; `_canvas` local (mirrors F-79) | design-quality | **resolved** |
| F-83 | Orphaned `Menu_2.wav` in `Music/` root — MusicManager only loads `MenuTrack/`+`GameTrack/` (`:32-33`) → never plays | design-quality | **resolved** |
| F-84 | `_Recovery/` junk scenes committed to repo (4 `0*.unity`, 2 tracked) → delete + gitignore | cosmetic | **resolved** |
| F-85 | Stray 0-byte `nul` file at repo root → delete | cosmetic | **resolved** |
| F-86 | CLAUDE.md Project Structure drift — `Core/` line omits `AnimConfig`/`GameplayConfig`/`UIStyles` | cosmetic | **resolved** |
| F-87 | `MainMenuUI:135` credits-overlay size literal `(400,300)` missed by F-72 (outside its enumerated lines) — found during the Wave-1 layout sweep | cosmetic | **resolved** |

**Synthesis (in `notes.md`):**
- *UI Construction Layer* — keep `UIFactory` (screen-space), add `WorldTextFactory` + `SpriteFactory` siblings in a `UI/Factory/` folder; links F-56/F-57/F-64/F-65/F-70.
- *UI Layout Magic sweep* — batch the inline position/size literals across F-64/F-66/F-69/F-72/F-79/F-82 into one `UIStyles` pass.

Next finding tag: **F-88**.

**Review status: COMPLETE** — all script folders (Audio, Chef, Core, Grid, Ingredients, Input, Monetization, UI) + project root/assets reviewed. 86 findings (F-1–F-86) + 3 synthesis notes (UI Construction Layer, UI Layout Magic sweep, plus the F-16 config architecture). Implementation is a separate pass.

**Tracking patterns** (not yet findings):
- ~~Defensive `if (_x == null) _x = GetComponent<X>()` Awake pattern~~ — **formalized as F-44** at the third sighting (`TouchInputHandler`, the `FindAnyObjectByType` variant; earlier: `ChefController`, `Ingredient`).
- **Subscribe-once-if-ready silent-failure** — `SubscribeEvents`/`Start` wires events only `if (source != null)` at startup; if the source isn't alive yet the subscription is silently skipped forever. Two sightings: F-51 (`GemPackSpawner`), F-63 (`GameHUD`). Formalize codebase-wide on the third (cf. F-44).
- ~~DOTween-kill-on-destroy hygiene~~ — **formalized as F-73** at the third sighting (`ScorePopup`; earlier: `BurgerPopup`/F-62, `GameOverPanel`). Untargeted `DOTween.To`-closure variant + missing `OnDestroy` kill.
- ~~World-space rise-fade-destroy popup duplication~~ — **formalized as F-75** at the third sighting (`FloatingText`; earlier: `ScorePopup`, `BurgerPopup`). Consolidate via `WorldTextFactory` + a shared rise-fade-destroy animation helper.

**Discarded:** SubscribeEvents/OnDestroy duplication (considered 2026-05-26; ~7 cosmetic lines, no real bug — see `notes.md` "Discarded" section).

---

## What we acted on

Implementation by landing wave (see [`EXECUTION.md`](EXECUTION.md) for the
plan). **73 of 87 findings resolved** (+2 partial: F-1, F-50). Waves 0–3 are on
`main` (pushed to `origin`); Wave 4 is in progress on `impl-wave-4` (also
pushed).

**Wave 0 — trivial deletions / asset fixes** (on `main`):
F-7, F-13, F-29, F-76, F-83, F-84, F-85, F-86.

**Wave 1 — config architecture + mechanical sweeps:**
F-14, F-15, F-16 (AudioConfig deferred until F-4/F-6), F-50 (sorting-order
portion only — see F-50 row), F-43, the UI Layout Magic sweep (F-64, F-66,
F-69, F-72, F-79, F-82), F-24, F-25, F-11, F-12, F-81.

**Wave 2 — blockers & correctness bugs:**
F-17, F-28 (playtested), F-47 + F-48 + F-51 (gem packs end-to-end,
playtested), F-80.

**Wave 3 — state machines, flags, small cleanups:**
F-21, F-27 (both F-31 prerequisites), F-33, F-35, F-36, F-32, F-5, F-58,
F-74, F-10, F-20, F-37, F-39, F-9.

**Wave 4 — cross-cutting consolidations & structural splits** (in progress, on
`impl-wave-4`):
- Shared infra: **F-46** `Singleton<T>` base; the **`UI/Factory/` folder** —
  **F-65** `SpriteFactory` (+ instances F-55, F-61) and **F-57 (partial)**
  `WorldTextFactory` (+ BurgerPopup migrated).
- The `BurgerAnimator` → `Scoring/` split: **F-26 / F-34** (also closed **F-1**
  tier-dup and **F-3** the switch; populated the `Scoring/` folder).
- Independent cleanups: **F-52** (BackgroundType→file), **F-8** (chef start
  const), **F-23** (RestartGame→SceneLoader), **F-22** (interstitial→AdManager),
  **F-87** (credits rect→UIStyles), **F-53/F-54** (Background camera-fill +
  constants — Background now fully closed).
- **The anchor: F-56** — split `BurgerChallenge` into model + view (playtested);
  closed **F-57** (WorldTextFactory), **F-59** (spawner resolved once), **F-60**
  (panel layout literals → UIStyles) in the same pass.
- Core orchestration: **F-19** (GameManager.Start → four phases), **F-67**
  (high-score persistence → game-over flow), **F-68** (level via GameManager;
  drops DifficultyManager scans in GameOverPanel/GameHUD/AudioManager).
- DOTween hygiene **F-62/F-73** (popups); **F-18** FeedbackManager split
  (ScreenFlashOverlay + ScorePopup/BurgerPopup Spawn factories).
- Spawner cluster: **F-40** WaveSlot struct, **F-41** preview paired-list (fixes
  the latent desync), **F-42** CreatePreview→int, **F-38** WaveComposer extraction.

**Still open (14 noted):** **F-44** DI sweep; F-70/F-71/F-78/F-77 (factory
dedup + bootstrap + audio service); F-63 HUD event-wiring; F-75 popup
consolidation; Wave 5 high-risk Grid F-30/F-31; **F-4** (deferred — risky DSP
dedup); misc F-45/F-49; plus the two partials (F-1 EnsureComponent lift, F-50
gem-pack literals).
