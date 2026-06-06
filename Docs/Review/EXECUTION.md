# Execution Plan

Landing waves for the 86 review findings (F-1–F-86). Details live in
[`notes.md`](notes.md); the index is in [`README.md`](README.md). This file
is **ordering only** — what to land, in what sequence, and why.

Principles:
- Land low-risk, high-leverage cleanups first; defer high-risk refactors until
  the structural groundwork (and config homes) exist.
- Respect the dependency annotations recorded on findings ([ANCHOR] /
  "execute as part of F-N" / "independent").
- The two synthesis notes (UI Construction Layer, UI Layout Magic sweep) and
  the config architecture (F-16) are multi-finding work-items, not single edits.
- No-code-yet rule applied during review; each wave below is a commit cluster.

---

## Wave 0 — Trivial deletions / zero-risk (warm-ups)

Pure removals and doc fixes. No behavior change. Do these first to shrink noise.

- **F-85** — delete stray `nul` file (untracked).
- **F-84** — delete `Assets/_Recovery/` + `.meta`s; `git rm` the two tracked scenes; gitignore.
- **F-83** — move `Menu_2.wav` into `Resources/Music/MenuTrack/` (one-file fix; restores a menu track).
- **F-7** — delete empty `Burger/` folder (leave `Scoring/` — created by F-26/F-34).
- **F-13** — delete unused `Constants.CELL_HEIGHT` (or derive from it — decide during F-14).
- **F-29** — delete dead `SwapColumns`/`SwapColumnTops` in GridManager (~70 lines).
- **F-76** — delete dead `_prefab` field in FloatingText.
- **F-86** — fix CLAUDE.md `Core/` structure line (add config files; note Burger/Scoring).

## Wave 1 — Config architecture + mechanical sweeps

Establishes the homes that later waves reference. Mostly mechanical, wide but shallow.

- **F-16 / F-14** — adopt the 6-config-file split; move balance out of `Constants`;
  create `MonetizationConfig` + `AudioConfig`. Folds in F-4, F-6, F-11, F-15.
  *Routing rule (per config-policy audit): structural→`Constants`,
  algorithmic→stays at impl, balance/feel→by who-tunes-it.*
- **F-50 sorting-order sweep** — all inline `sortingOrder` literals (7 sightings) → named `Constants`.
- **UI Layout Magic sweep** (F-64, F-66, F-69, F-72, F-79, F-82) — inline UI
  position/size literals → `UIStyles`; add button-stack `start + i*spacing` helpers.
- **F-24 / F-25** — inline UI colors → `UIStyles`; constructor-consistency.
- **F-81** — model IAP/reward products as data in `MonetizationConfig`; derive labels.
- **F-12** — split `LAND_PUNCH_ELASTICITY` → add `WAVE_PUNCH_ELASTICITY`.

## Wave 2 — Blockers & correctness bugs

- **F-28** (blocker) — GridManager: run match check before overflow check ("saving matches").
- **F-47** (blocker) — GemPack invisible: assign a sprite / route via a prefab.
- **F-17** (blocker, test path) — DifficultyManager test-mode level reset on first ingredient.
- **F-80** — ShopPanel stale gem balance → subscribe `OnGemsChanged` / refresh.
- **F-48** — GemPack `OnMouseDown` bypasses New Input System → route via TouchInputHandler.
- **F-51** — GemPackSpawner subscribe-once silent no-op → read state in Update / late-subscribe.

## Wave 3 — State machines, flags, small design cleanups

Many enable later waves (esp. F-21 + F-27 for the Grid refactors).

- **F-21** — `Paused` → `_isPaused` modifier; GameState becomes `{Menu, Playing, GameOver}`. *(prereq for F-31)*
- **F-27** — collapse `Column.CheckForMatch` to `top.Type == second.Type`; delete dead `IsRegularIngredient`/`IsBun`/`IngredientTypeExtensions`. *(prereq for F-31)*
- **F-33** — Ingredient `_isFalling`/`_isLanded` → `IngredientState` enum.
- **F-36** — delete dead `Idle` spawner state (+ F-35 enum → own file).
- **F-39** — `IngredientType` values non-load-bearing via `GameplayConfig.REGULAR_INGREDIENTS`.
- **F-37** — push triple-wave-chance from DifficultyManager; drop spawner's raw level.
- **F-5 / F-9 / F-10 / F-20 / F-32 / F-40 / F-58 / F-74** — assorted enum/struct/comment cleanups.

## Wave 4 — Cross-cutting consolidations & structural splits

The big refactors. Build shared infrastructure first, then migrate consumers.

- **F-44** — standardize explicit dependency injection; kill `FindAnyObjectByType` scene scans
  (consumers: ChefController, Ingredient, TouchInputHandler, BurgerChallenge/F-59, SettingsPanel/ShopPanel/F-77).
- **F-68** — expose `CurrentLevel`/`OnLevelChanged` via `GameManager`; supersedes F-63 local cache.
- **F-46** — `Singleton<T>` base (guard + `Instance` null-on-destroy; NO lazy auto-create).
- **F-71 / F-78** — extract an audio-apply service (one method owns `AudioListener.volume` + music);
  used by bootstrap and SettingsPanel. F-71 also extracts app bootstrap out of MainMenuUI.
- **F-18 / F-19 / F-22 / F-23** — Core orchestration splits (FeedbackManager, GameManager.Start, interstitial→AdManager, RestartGame→SceneLoader).
- **UI Construction Layer** (synthesis) — `UI/Factory/` folder:
  - `UIFactory` kept (screen-space) + anchor params (**F-64**) + internal dedup (**F-70**).
  - new `WorldTextFactory` (**F-57**) — consumers: BurgerChallenge, BurgerPopup, FloatingText.
  - new `SpriteFactory` (**F-65**) — consumers: Background (F-55), BurgerChallenge (F-61), GameLayout.
- **F-73** — DOTween-kill-on-destroy hygiene (BurgerPopup/F-62, GameOverPanel, ScorePopup); FloatingText is the reference pattern.
- **F-75** — consolidate the 3 rise-fade-destroy popups onto `WorldTextFactory` + a shared animation helper.
- **F-26 / F-34** — split `BurgerAnimator` → Animation + `Scoring/Scoring` + `BurgerNamer` + `Grid/BurgerData`; absorb FastDrop calc.
- **F-56** [ANCHOR] — split `BurgerChallenge` → model (logic) + view; GridManager talks to model; scoring via Scoring/GameManager. Pulls in **F-57** (WorldTextFactory) and **F-59** (inject spawner). Depends on Wave-4 factories.
- **F-38** — extract `WaveComposer` from IngredientSpawner (consumes F-37's pushed chance + F-40 `WaveSlot`).
- **F-41 / F-42** — WavePreviewManager: collapse parallel lists into `(preview, slot)` pairs; drop `Column`→`int`.

## Wave 5 — High-risk Grid refactors (playtest gated)

Land last, individually, with manual playtests. Depend on Wave-3 groundwork.

- **F-30** [HIGH-RISK] — extract `SwapAnimator` + event-based completion (replaces time-coupled `DelayedMatchCheck`). Full test plan in notes.
- **F-31** — formalize game-input freeze during burger resolution; collapse burger detection (delete `DetectBurger`/`HasBunBelow`/`BurgerDetection`/`_columnsWithActiveBurger`/cascade re-check). *Depends on F-21 + F-27.*

---

## Key dependency edges (don't violate)

- F-21 + F-27 → **F-31**
- Wave-1 config homes → most magic-number consumers
- `WorldTextFactory` (F-57) + `SpriteFactory` (F-65) → **F-56** view side
- F-37 → F-38 (pushed triple-chance) ; F-40 `WaveSlot` → F-38/F-41
- F-26/F-34 `Scoring` home → F-56 scoring extraction
- F-68 (GameManager level access) supersedes F-63 — do F-68, skip F-63's local cache

## Not in waves (tracked elsewhere)

- AdManager real-SDK integration → `Docs/pre-launch-checklist.md` (Monetization (Ads)).
- SaveDataManager security / cloud save / schema versioning / IAP receipts → pre-launch checklist.
