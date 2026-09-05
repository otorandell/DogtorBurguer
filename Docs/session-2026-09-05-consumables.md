# Session 2026-09-05 (b) — Consumable balance: mustard

## Context
Oscar flagged mustard as the weak consumable. Assessment (structural, not a tuning knob):
- The shuffle bag spreads regular types evenly and the pair rule keeps adjacent pieces
  distinct, so a single type has ~board/activeTypes copies. With 4→8 active types across the
  curve, mustard's yield HALVED late game (≈2-3 pieces on a 20-piece board) — exactly when
  ketchup (yield = column height) gets stronger.
- Targeting the danger column removed exactly one piece from it (its top).
- Bun trap: a bottom bun on top of the targeted column swept EVERY open bottom board-wide —
  0 points and every in-progress burger destroyed. Unguarded.
- Roles are otherwise sound: Ketchup = rescue, Skewer = mega-burger setup (not a rescue tool,
  left as is), Mustard = board thinning + cascades.

## Changes
- `MustardEffect.SweepTypes(column)`: the top `GameplayConfig.MUSTARD_SWEEP_TYPES` (2) distinct
  REGULAR types read top-down, buns skipped. `CanApply` = at least one regular in the column
  (else the drop fizzles, item spent — the uniform fizzle rule).
- `GridManager.ConsumableSweepType(type)` → `ConsumableSweepTypes(IReadOnlyList<IngredientType>)`,
  same per-column remove → `CollapseFromRow(0)` → cascade.
- Escalating mustard score: `Scoring.MustardSweepPoints(n)` = Σ (10 + i·`MUSTARD_POINTS_STEP`
  5) → 5 pops = 100, 10 = 325. Ketchup stays flat via `Scoring.ConsumablePoints`.
  `GridManager.AwardConsumablePoints` now takes the final points.
- Stale "Mustard (strongest)" note on `CONSUMABLE_SPAWN_WEIGHTS` replaced with the role split.

Verified: `dotnet build Assembly-CSharp.csproj --no-incremental` clean (the two pre-existing
CS0162 warnings in LevelPlayAdProvider only). Not yet playtested.

## Pending
- Playtest the two-type mustard; if it now overshadows ketchup early (both ≈10 pieces at
  L1-3), knobs are `MUSTARD_SWEEP_TYPES`, `MUSTARD_POINTS_STEP`, `CONSUMABLE_SPAWN_WEIGHTS`.
- Expected yield reference (20-piece board): L1-3 ~10, L4-7 ~8, L8-11 ~7, L16-20 ~5 pieces.
