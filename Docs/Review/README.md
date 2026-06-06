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
| F-1 | `EnsureComponent<T>` helper should be reusable (lift to Util, replace `MainMenuUI` re-implementation) | design-quality | noted |
| F-2 | No path from procedural clips to authored audio assets (add per-slot `[SerializeField]` override) | design-quality | fix-now |
| F-3 | `HandleBurger` if-elseif → switch expression | cosmetic | noted |
| F-4 | Sample-generation skeleton repeats across `Generate*Samples` (gated on F-2 path) | design-quality | noted |
| F-5 | Two category bools in `MusicManager` → `MusicCategory` enum | design-quality | noted |
| F-6 | `MusicManager._source.volume = 0.5f` magic number (move to config) | cosmetic | noted |
| F-7 | Empty `Burger/` and `Scoring/` placeholder folders — populate or delete | cosmetic | noted |
| F-8 | `ChefController._startPosition` SerializeField never varies; collapse to constant | cosmetic | noted |
| F-9 | Position bubbles likely effectively invisible (low alpha + sortingOrder behind chef) — decide UX cue vs delete | design-quality | noted |
| F-10 | `SwapPlates` flip-snap needs one-line explanatory comment | cosmetic | noted |
| F-11 | Inline `0.4f` duration in `BurgerChallenge:468` should be a named AnimConfig const | design-quality | noted |
| F-12 | `LAND_PUNCH_ELASTICITY` misnamed (used in wave punches too) — rename or split | cosmetic | noted |
| F-13 | `Constants.CELL_HEIGHT` is unused — delete or derive `CELL_VISUAL_HEIGHT` from it | cosmetic | noted |
| F-14 | `Constants.cs` mixes structural with balance/tuning — move 17 entries to `GameplayConfig` (+ possible new `MonetizationConfig`) | design-quality | noted |
| F-15 | `CHEF_POSITION_COUNT` should be derived from `COLUMN_COUNT - 1` | design-quality | noted |
| F-16 | Adopt 6-config-file architecture (`Constants` + `GameplayConfig` + `MonetizationConfig` + `AnimConfig` + `UIStyles` + `AudioConfig`) — implementation strategy for F-4/F-6/F-11/F-14 | design-quality | noted |
| F-17 | `DifficultyManager.EvaluateLevel` resets test-mode level on first ingredient placed | **blocker** (test feature) | noted |
| F-18 | `FeedbackManager` mixes orchestration with asset construction — extract `SpriteUtils` + `ScreenFlashOverlay` + popup `Spawn` factories | design-quality | noted |
| F-19 | `GameManager.Start` doing four distinct phases — extract `ResolveDependencies` / `EnsureManagers` / `ApplyPersistedSettings` / `SubscribeEvents` | cosmetic | noted |
| F-20 | Game-flow methods need explanatory comments for state-transition clarity | cosmetic | noted |
| F-21 | `Paused` is structurally a modifier on `Playing` — refactor to internal `_isPaused` bool | design-quality | noted |
| F-22 | `ShouldShowInterstitial` mixes persistence with ad-cadence logic — move to `AdManager` | design-quality | noted |
| F-23 | `GameManager.RestartGame` bypasses `SceneLoader` — route through `SceneLoader.LoadGame()` | design-quality | noted |
| F-24 | Inline UI style colors in `GameLayout` / `BurgerChallenge` / `BurgerPopup` should reference `UIStyles` | design-quality | noted |
| F-25 | `Color` constructor inconsistency in `UIStyles` (redundant `1f` alpha on 5 entries) | cosmetic | noted |
| F-26 | Split `BurgerAnimator` into Animation + `Scoring/Scoring` (consolidated per F-34) + `Scoring/BurgerNamer` + `Grid/BurgerData` top-level; closes F-1 + F-7-Scoring-half | design-quality | noted |
| F-27 | `Column.CheckForMatch` collapses to `top.Type == second.Type` (+ delete now-dead `IsRegularIngredient` / `IsBun` / `IngredientTypeExtensions`) | design-quality | noted |
| F-28 | `GridManager` overflow check fires before match check — kills "saving matches" | **blocker** (UX bug) | noted |
| F-29 | Dead `SwapColumns` + `SwapColumnTops` methods in `GridManager` (~70 lines) | design-quality | noted |
| F-30 | **[HIGH-RISK]** Extract `SwapAnimator` + event-based completion (replaces time-coupled `DelayedMatchCheck`) — test plan in notes | design-quality | noted |
| F-31 | Formalize game-input freeze during burger resolution; collapse burger detection logic (delete `DetectBurger` / `HasBunBelow` / `BurgerDetection` / `_columnsWithActiveBurger` / cascade re-check) | design-quality | noted |
| F-32 | Promote `MatchResult` struct to top-level type | cosmetic | noted |
| F-33 | `Ingredient` two-bool `_isFalling` + `_isLanded` encoding of a 3-state lifecycle → `enum IngredientState { Spawned, Falling, Landed }` | design-quality | noted |
| F-34 | Amend F-26: consolidate scoring into `Scoring/Scoring.cs` (general static class, not burger-specific) and absorb `Ingredient.FastDrop` calc into it | design-quality | noted |
| F-35 | `SpawnerState` enum nested in `IngredientSpawner` → promote to own file | cosmetic | noted |
| F-36 | Dead `Idle` state redundant with `_active` (`case Idle` unreachable) — delete it (Option A); `_active` stays as F-21-style modifier | design-quality | noted |
| F-37 | Spawner consumes raw level only for `GetWaveSize` triple-chance — move computation to DifficultyManager, push value, delete `SetCurrentLevel`/`_currentLevel` (upgrades :285 note) | design-quality | noted |
| F-38 | Extract `WaveComposer` (wave-composition logic) from 378-line `IngredientSpawner` — not animation; SRP split of "what to spawn" | design-quality | noted |
| F-39 | `IngredientType` int values load-bearing in 4 sites — introduce `GameplayConfig.REGULAR_INGREDIENTS` list; enum order then free, buns→0,1 optional. Kills extensions (w/ F-27) | design-quality | noted |
| F-40 | Promote `(IngredientType, int columnIndex)` wave-slot tuple to named `WaveSlot` struct (across preview mgr + spawner; F-38 return type) | design-quality | noted |
| F-41 | `_data`/`_previews` parallel lists can desync (latent `TryTap` index bug) — collapse to one `(preview, slot)` list; folds in DOTween/teardown nits | design-quality | noted |
| F-42 | `CreatePreview` takes `Column` but uses only `ColumnIndex` — drop to `int`, remove sole `GridManager` coupling | design-quality | noted |
| F-43 | `sortingOrder = 90` → named constant in `Constants` (structural layering; F-14/F-16, not F-24) | cosmetic | noted |
| F-44 | Defensive Awake dependency resolution (3rd sighting → codebase-wide) — kill `FindAnyObjectByType` scene-scan variant; standardize on explicit injection | design-quality | noted |
| F-45 | `TouchInputHandler` gesture-vs-mode tangle — dedup swipe-move + preview/falling tap, rename `ProcessTap`/`ProcessTapMode`; folds in magic-number + default + `_isDragging` nits | design-quality | noted |
| F-46 | Singleton guard boilerplate duplicated across ~7 managers + missing `Instance` null-on-destroy → `Singleton<T>` base (guard/teardown only, NO lazy auto-create — preserves documented init order) | design-quality | noted |
| F-47 | GemPack invisible — `SpriteRenderer` created but `.sprite` never assigned (confirmed bug, not placeholder) | **blocker** | noted |
| F-48 | GemPack `OnMouseDown` bypasses New Input System — device-blocker if input handling is New-only; route through `TouchInputHandler` | design-quality | noted |
| F-49 | GemPack animation interleaved with construction/logic in `Initialize` + `Collect` (F-18/26/30 lineage) | design-quality | noted |
| F-50 | Gem-pack magic numbers routed **by kind** — `MonetizationConfig` (interval/chance/value) + `AnimConfig` (wobble/duration) + `Constants` (radius/geometry/sorting); advances F-14/F-16, promotes F-43 sweep; folds dead `_collider`, redundant kill, UI-color-on-world | design-quality | noted |
| F-51 | `GemPackSpawner` subscribe-once init-order assumption (`Start` only subscribes if `GameManager.Instance` ready) → silent permanent no-op; read state directly in `Update` instead | design-quality | noted |
| F-52 | `BackgroundType` second top-level type in `Background.cs` → own file | cosmetic | noted |
| F-53 | Camera-fill sizing/positioning duplicated across `FitToCamera`/`CreateFilter` → shared helper; cache `Camera.main`; drop dead `:24` position write | design-quality | noted |
| F-54 | `Background` magic numbers routed **by kind** — `Constants` (z-depths/sorting, 3rd sighting → F-50 sweep) + stay-at-impl (algorithmic texture dims/PPUs) + `UIStyles` (filter opacity) | design-quality | noted |
| F-55 | `Background` generated textures/sprites leak (no `OnDestroy`) + not cached (global cache-assets rule); destroy on teardown + share 1×1 white sprite | design-quality | noted |
| F-56 | **[ANCHOR]** Split `BurgerChallenge` god-class → challenge model (logic) + view (UI/animation); GridManager talks to model; scoring via Scoring/GameManager | design-quality | noted |
| F-57 | Dedup `CreateUI` world-TMP setups + visual builders → new `WorldTextFactory` (UIFactory is UGUI-only) — *execute as part of F-56* | design-quality | noted |
| F-58 | Nested `OrderType` enum → own file — *independent* | cosmetic | noted |
| F-59 | Repeated `FindAnyObjectByType<IngredientSpawner>()` (3 sites/regen) → resolve once — *execute as part of F-56* (F-44 theme) | design-quality | noted |
| F-60 | `BurgerChallenge` magic numbers + dead math (`:414` no-op, misleading `_meterX/Y`) routed **by kind** — `UIStyles` (layout/sizes) + `Constants` (sorting) + stay-at-impl (rect dims, algorithmic); *independent* (colors=F-24, flash dur=F-11) | design-quality | noted |
| F-61 | `GenerateRectSprite` texture leak + not cached — *independent*; 2nd sighting → codebase-wide generated-asset hygiene (w/ F-55) | design-quality | noted |
| F-62 | `BurgerPopup` no `OnDestroy` DOTween kill (untargeted alpha `DOTween.To`) + `sizeDelta` magics → `UIStyles` + `SetParent(false)` nit | design-quality | noted |
| F-63 | `GameHUD` event-wiring: cache `DifficultyManager` (double `FindAnyObjectByType` + unsubscribe symmetry) + subscribe-once silent-failure (2nd F-51 sighting) + init/null-check inconsistencies | design-quality | noted |
| F-64 | `GameHUD` layout magic numbers → `UIStyles`; + add anchor params to `UIFactory.CreateText` (centered-only at `:63-64`) so `CreateHUDText` can be deleted | design-quality | noted |
| F-65 | **[codebase-wide]** Scattered runtime sprite/texture generators leak (no disposal) → new `SpriteFactory` sibling (gen + cache + dispose; not UIFactory — it's UGUI-only). Instances: F-55, F-61, GameLayout 9-slice (3rd sighting → formalized) | design-quality | noted |
| F-66 | `GameLayout` magic numbers — `z`/`sortingOrder` → `Constants` (5th sorting sighting → F-50 sweep); border/corner/panel geometry → `UIStyles` (colors=F-24; `TEX_SIZE`/`1.5f` algorithmic) | design-quality | noted |
| F-67 | `GameOverPanel` high-score persistence (`SetHighScore`) inside UI `Show()` → move to game-over flow (F-56 cousin) | design-quality | noted |
| F-68 | `DifficultyManager.CurrentLevel` reached via `FindAnyObjectByType` in 3 sites (not a singleton) → expose via `GameManager`; supersedes F-63 local cache (F-44 theme) | design-quality | noted |
| F-69 | `GameOverPanel` layout magic numbers → `UIStyles`; button y-stack `30/-45/-120/-195` is a derivable start+spacing sequence | design-quality | noted |
| F-70 | `UIFactory` internal dup (`CreateButton` label reuse `CreateText`; shared `ConfigureRect`) + `:22` `SetParent(false)` nit | cosmetic | noted |
| F-71 | `MainMenuUI` bootstraps managers + sets `AudioListener.volume` in `Start` (`:18-37`) → extract to bootstrap entry point (links F-1, F-56/F-67) | design-quality | noted |
| F-72 | `MainMenuUI` layout magic → `UIStyles`; gem-counter anchor post-patch = 2nd F-64 consumer + dead `(0,400)` arg; `using UnityEngine.UI` nit | design-quality | noted |
| F-73 | **[codebase-wide]** DOTween-kill-on-destroy hygiene — missing `OnDestroy` kill + untargeted `DOTween.To` closures. Instances: F-62 (BurgerPopup), GameOverPanel, ScorePopup (3rd → formalized) | design-quality | noted |
| F-74 | `ScorePopup` `0.8f` fade scale → `AnimConfig` | cosmetic | noted |
| F-75 | Consolidate 3 world-space rise-fade-destroy popups (`FloatingText`/`ScorePopup`/`BurgerPopup`) → `WorldTextFactory` + shared animation helper | design-quality | noted |
| F-76 | Dead `_prefab` static field in `FloatingText` (`:9`) → delete | cosmetic | noted |
| F-77 | `SettingsPanel` `FindAnyObjectByType<Canvas>()` grabs arbitrary canvas → inject from `MainMenuUI` (F-44 theme) | design-quality | noted |
| F-78 | Duplicate "apply sound setting" (`AudioListener.volume` + `ApplySoundSetting`) in `SettingsPanel` + `MainMenuUI` → one audio-service method (links F-71) | design-quality | noted |
| F-79 | `SettingsPanel` layout magic → `UIStyles`; empty `""` button labels, silent `true`/`Drag` defaults, `_canvas` local | design-quality | noted |
| F-80 | `ShopPanel` gem-balance display goes stale after grants (no `OnGemsChanged` sub/refresh) | design-quality | noted |
| F-81 | `ShopPanel` IAP/reward amounts+prices hardcoded & duplicated across labels and grants → product table in `MonetizationConfig` (drift bait) | design-quality | noted |
| F-82 | `ShopPanel` layout magic → `UIStyles`; `_canvas` local (mirrors F-79) | design-quality | noted |
| F-83 | Orphaned `Menu_2.wav` in `Music/` root — MusicManager only loads `MenuTrack/`+`GameTrack/` (`:32-33`) → never plays | design-quality | noted |
| F-84 | `_Recovery/` junk scenes committed to repo (4 `0*.unity`, 2 tracked) → delete + gitignore | cosmetic | noted |
| F-85 | Stray 0-byte `nul` file at repo root → delete | cosmetic | noted |
| F-86 | CLAUDE.md Project Structure drift — `Core/` line omits `AnimConfig`/`GameplayConfig`/`UIStyles` | cosmetic | noted |

**Synthesis (in `notes.md`):**
- *UI Construction Layer* — keep `UIFactory` (screen-space), add `WorldTextFactory` + `SpriteFactory` siblings in a `UI/Factory/` folder; links F-56/F-57/F-64/F-65/F-70.
- *UI Layout Magic sweep* — batch the inline position/size literals across F-64/F-66/F-69/F-72/F-79/F-82 into one `UIStyles` pass.

Next finding tag: **F-87**.

**Review status: COMPLETE** — all script folders (Audio, Chef, Core, Grid, Ingredients, Input, Monetization, UI) + project root/assets reviewed. 86 findings (F-1–F-86) + 3 synthesis notes (UI Construction Layer, UI Layout Magic sweep, plus the F-16 config architecture). Implementation is a separate pass.

**Tracking patterns** (not yet findings):
- ~~Defensive `if (_x == null) _x = GetComponent<X>()` Awake pattern~~ — **formalized as F-44** at the third sighting (`TouchInputHandler`, the `FindAnyObjectByType` variant; earlier: `ChefController`, `Ingredient`).
- **Subscribe-once-if-ready silent-failure** — `SubscribeEvents`/`Start` wires events only `if (source != null)` at startup; if the source isn't alive yet the subscription is silently skipped forever. Two sightings: F-51 (`GemPackSpawner`), F-63 (`GameHUD`). Formalize codebase-wide on the third (cf. F-44).
- ~~DOTween-kill-on-destroy hygiene~~ — **formalized as F-73** at the third sighting (`ScorePopup`; earlier: `BurgerPopup`/F-62, `GameOverPanel`). Untargeted `DOTween.To`-closure variant + missing `OnDestroy` kill.
- ~~World-space rise-fade-destroy popup duplication~~ — **formalized as F-75** at the third sighting (`FloatingText`; earlier: `ScorePopup`, `BurgerPopup`). Consolidate via `WorldTextFactory` + a shared rise-fade-destroy animation helper.

**Discarded:** SubscribeEvents/OnDestroy duplication (considered 2026-05-26; ~7 cosmetic lines, no real bug — see `notes.md` "Discarded" section).

---

## What we acted on

This section grows as we fix things during the review. (Empty until
the first fix lands.)
