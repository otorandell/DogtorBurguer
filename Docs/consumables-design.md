# Consumables System — Design (WORKING DRAFT)

Status: **IMPLEMENTED** (2026-06-12). The last big system. This doc is the design of record;
see `CLAUDE.md` → Core Systems → Consumables for the as-built summary.
Branch context: Phase 2 (Polish), `polish`.

The goal of this doc is to separate **what we've decided** from **what we still need to
figure out**, so we can focus our thinking on the open questions. Nothing here is built.

---

## Concept

Reframe the current **GemPack / GemPackSpawner** into a **Burger Fairy** that flies across
the screen carrying a *payload*:
- **Gems** → same effect as today (award gems on collect).
- **A consumable** → goes into a per-run inventory (top-left box); the player later drags it
  onto a column to use it.

Three consumables, each acting on a single column.

---

## DECIDED / KNOWN

### Delivery — Burger Fairy
- Replaces the GemPack concept. A fairy flies across the screen (reuse the existing fly-path +
  tap-to-collect from `GemPack`).
- Carries one payload: **gems** or **one consumable**.
- Tap to collect. Gems → award gems (unchanged). Consumable → add to inventory.
- Implementation direction: `GemPack` → `BurgerFairy` (add a payload field);
  `GemPackSpawner` → `BurgerFairySpawner` (rolls the payload). Tap routing already exists
  (`GemPack.TryTapAt` is called from `TouchInputHandler`).

### Inventory
- **Per-run only.** No persistence, no save-schema change, **not** buyable. Collected from
  fairies during a run, lost on game over.
- Lives in the **top-left box where the score currently sits** (shares that HUD region —
  layout integration with `GameHUD`).
- **2 slots.** Collecting a consumable while both are full **evicts the oldest** (FIFO ring of
  capacity 2).
- **World-space sprites, manually hit-tested** (like `GemPack`), to stay on the project's
  single raw-input pipeline rather than UGUI EventSystem.

### Consumables — unified "consumable faller"
All three are the **same kind of thing**: on release over a column, a consumable item (sprite
#3) spawns at the top of that column and **falls fast** (much faster than ingredients) until it
**impacts its target**; the effect **resolves on impact**. Each consumable = a **target rule**
+ an **on-impact effect**:

| Consumable | Target (impact point) | On impact |
|---|---|---|
| **Ketchup** | top of the stack | remove the **whole column** |
| **Mustard** | top of the stack | read the top ingredient's type → remove **all of that type board-wide** |
| **Skewer** | first bun encountered (topmost `BunBottom`) | **attach** the bun, keep falling to **row 0** carrying it, **destroy** other buns passed |

- **Uniform fizzle:** any faller that reaches the **board floor without hitting its target** is
  destroyed with **no effect** — the item is still spent. Covers: Skewer with no bun; Ketchup/
  Mustard on an **empty column** (no stack top). Same "missed = wasted, pops at the bottom" feel
  for all three.
- All eliminated ingredients **grant points** (reuse match/per-ingredient scoring) — **except
  buns destroyed by Skewer, which grant none** (utility tool, not a score engine).
- The faller targets the **landed stack** (ignores currently-falling pieces in that column).
- Resolving on impact (against live state) + the very fast fall makes it robust to a piece
  landing in the column mid-drop — no precomputed outcome to go stale.

### Shared effect rules
- After any effect: **collapse + cascade** via the existing `CheckAndProcessMatches`, run on
  **every column the effect touched** (all columns, for Mustard).

### Mustard chain reactions — keep it simple (reuse, don't build)
- Matches are **strictly top-of-column pairs**, checked per column — there is no cross-column
  matching. So a board-wide sweep is just independent per-column cleanup; **no ordering or
  cross-column logic** needed.
- Sweeping creates new adjacencies → new matches. These are handled **for free** by reusing the
  same `CheckAndProcessMatches` cascade that already runs after every landing. Points
  (`OnMatchEliminated`), match SFX/VFX all come along automatically.
- **The pair rule answers the "3 of the same" case:** matches are always **pairs**. If a
  sweep + collapse leaves 3 of a type stacked, the cascade removes the top 2 and **1 remains**
  — exactly like the rest of the game. No triple/quad special-casing.
- **Sequence per affected column:** remove-by-type → `Column.CollapseFromRow` (survivors above
  a gap drop — Mustard removes from mid-stack, unlike normal top-only matches) → cascade.
  All three steps use existing methods.
- **Invariant we can lean on:** a `BunTop` never *sits* in a stack (lone tops self-destruct on
  land; a top over a bottom completes a burger instantly). So **no consumable can ever
  complete a burger** — the only post-effect check needed is regular matches + bottom-bun
  cancellation.

### Usage interaction — drag-to-column (DECIDED)
The only genuinely new mechanic. The current `TouchInputHandler` acts **only on release**
(records start on press, decides swipe-vs-tap from the two endpoints) and has **no per-frame
drag tracking**. The carry breaks three of its assumptions: it reacts on **press-down**, needs
**continuous tracking** while held, and must **suppress all chef gestures** for the gesture's
duration.

- **Origin disambiguates.** A press that starts **on a consumable icon** = a carry; a press
  starting anywhere else = normal gameplay (unchanged). Decided once at press-down. Arbitration
  is a single "am I carrying?" flag in the one input owner — no two systems fight over a touch.
- **World keeps moving while carrying** — deliberately **no pause/slow**. A cancellable pause
  would be a free "stop time" exploit. Carrying is a real-time decision under pressure (a fumble
  can cost you, even a game-over mid-carry).
- **Cancel on release outside the playfield** — item returns to its slot, nothing consumed.
  Only a release **over the playfield** commits the use. (Forgives accidental grabs.)
- **Ownership:** `TouchInputHandler` stays the single raw-input reader; at Began it asks the
  inventory "did this hit an icon?" — if so it hands the rest of the gesture to a dedicated
  **`ConsumableDragController`** (carry state + ghost visuals) and skips chef logic. Adds a small
  branch + the `Moved`/per-frame path the handler lacks today.

**Carry state machine:**
```
Idle ──(press-down hits an icon)──► Carrying
Carrying (each frame):
    • carried icon (sprite #1) follows the finger
    • target column = round((pointerX − GRID_ORIGIN_X) / CELL_WIDTH), clamped 0..3
    • translucent ghost (sprite #2) snaps to that column
Carrying ──(release over playfield)──► Use: apply effect, drop falling form (#3), consume slot
Carrying ──(release off playfield)──► Cancel: tween item back to slot, consume nothing
```
- The drag controller must **abort cleanly on state change** (pause / game-over mid-carry).

### Visuals / assets (DECIDED)
One sprite does **quadruple duty** per consumable: fairy badge → inventory icon → column ghost
(just the icon drawn semi-transparent — *not* a separate asset) → faller. The fairy is one
universal carrier; the gem is just another payload it carries (peer to the consumables at the
carry layer), so all four payload badges load uniformly.

**Directory layout** (code-first via `Resources.Load`, zero editor wiring — same pattern as
`Resources/Music/` and `Resources/Skins/`):
```
Assets/_Project/Resources/
  Fairy/
    fairy.png          <- the carrier (one sprite, every payload)
  Rewards/             <- payload badges, all peers
    gem.png
    ketchup.png
    mustard.png
    skewer.png
```
- Carrier: `Resources.Load<Sprite>("Fairy/fairy")`.
- Payload badge (gem or any consumable): `Resources.Load<Sprite>("Rewards/" + payloadId)` —
  uniform regardless of payload. Logic diverges downstream (gem → currency; consumable →
  inventory + faller), but carry + badge is one path.
- Consumable systems (inventory icon, alpha column ghost, faller) reuse the same
  `Rewards/{type}.png` — no duplicate art.
- **Naming:** lowercase single-word, per the existing convention (`bun_bottom`, `bg_game`).

**Asset status**
- ✅ Ketchup / Mustard / Skewer art authored (currently loose PascalCase files in the
  `Sprites/` root — move to `Resources/Rewards/`, rename lowercase, bring each `.png.meta`).
- ➕ Fairy + gem being added by the dev (gem is **not** procedural — replacing
  `GemPack.GetGemSprite`'s runtime diamond with `Rewards/gem.png`).
- Loose root files to retire (avoid stale duplicates): `Ketchup.png`, `Mustard.png`,
  `Skewer.png`, `ChatGPT Image 12 jun 2026, 12_46_50.png`.

**Import gotcha (resolved):** the project's default texture import is **Multiple** sprite mode,
which auto-slices multi-blob images into fragments (`ketchup` → 24, `mustard` → 23, `skewer` →
10). `Resources.Load<Sprite>` then returns only the first fragment. Fix = set those metas to
**Single** mode (done, GUIDs preserved). `fairy`/`gem` happened to slice to one sprite so they
load fine. **Any future gameplay sprite with disconnected parts needs Single mode.**

**Optional / deferred (not v1):** distinct faller sprites (if an icon reads poorly falling);
impact splat/splash VFX (start with flash + scale-punch from `AnimConfig`); fairy flap frames.

### Architecture direction (per project principles — polymorphism, not switch)
- **`ConsumableType`** enum (Ketchup / Mustard / Skewer) — typed id for inventory + fairy payload.
- **`ConsumableFaller`** — shared falling behavior: spawn at top of target column, fast fall,
  detect the effect's target, fire the effect **on impact**, fizzle at the floor if no target.
- **Abstract `ConsumableEffect`** + `KetchupEffect` / `MustardEffect` / `SkewerEffect`
  subclasses. Each supplies its **target rule** (top-of-stack vs first-bun) and its
  **on-impact effect**. Skewer adds an extra **carry phase** (ride the bun to row 0). Behavior
  lives here — polymorphic, no `switch`.
- **`GridManager`** exposes a few **granular helpers** (e.g. clear-column, remove-by-type,
  skewer-relocate); effects orchestrate them — GridManager doesn't grow three god-methods.
- **`ConsumableCatalog`** — type → visuals + display name.
- **`ConsumableInventory`** — held items + drives the top-left box UI.

### Existing systems it touches (mapped, read)
- `GemPack` / `GemPackSpawner` (become the fairy), `MonetizationConfig` (spawn interval/chance).
- `GridManager`, `Column`, `MatchDetector` (effect primitives + cascade).
- `TouchInputHandler` (new drag mode).
- `SpriteFactory` / `Theme` pattern (visuals).

---

## UNKNOWN / OPEN QUESTIONS

### 1. The drag-to-column interaction — **RESOLVED** (see "Usage interaction — drag-to-column")
Residual (minor, visual polish — defer): do we show the finger-icon *and* the column ghost at
once, or just one? Ghost's vertical placement / column-highlight style.

### 2. Skewer precise semantics — **RESOLVED** (see the unified faller table)
End state: one bun at row 0, other buns destroyed, regulars collapse on top. Falls fast,
impacts the first bun, attaches it, carries it to the floor. No bun → falls to the board floor
and fizzles. Operates on the landed stack only. No points for destroyed buns.

### 3. Inventory UI — **RESOLVED** (2 slots, FIFO-evict-oldest, in the score box, world-space)
Residual: exact layout within the score region (needs `GameHUD` coordination at build time).

### 4. Fairy payload odds — **RESOLVED** (gems held flat, consumables additive)
- **Spawn rate:** bump to **20% chance every 10s** (from 8%) → ~3.6 fairies/game.
- **Payload split:** **40% gems / 60% consumable** → ~1.4 gem fairies/game (gem income
  *unchanged*) + ~2.2 consumable fairies/game.
- **Consumable pick:** **even 1/3 each** to start. Tune later — Mustard (strongest) is the first
  to down-weight if too strong; Skewer (situational) to up-weight if it feels dead.
- **Homes:** spawn rate / payload split / consumable weights → `GameplayConfig` (balance the
  designer tunes); gem *value* per pack stays in `MonetizationConfig`. Keep the existing 10s
  interval + per-roll structure (minimal change to the spawner).

### 5. The "falling thing" for Ketchup & Mustard — **RESOLVED**
All three fall as items and resolve on impact (unified faller). Every consumable has a
falling-form sprite (#3); Ketchup/Mustard drop onto the stack top and splash.

### 6. Scoring specifics — **RESOLVED**
- Direct consumable removals score **flat, no multiplier** (match-like, not burger-like). The
  global × challenge multipliers are **burger-only** (`BurgerChallenge.HandleBurgerCompleted`),
  so consumables never touch them.
- **Rate:** new `POINTS_CONSUMABLE_PER_INGREDIENT` constant (default **10**), per **non-bun**
  ingredient removed. Generous-but-flat is fine — scarcity (~2 consumables/game, uncontrollable
  drop timing) is the real limiter, so it can't be farmed; the burst is good feedback.
- **Buns never score from consumables** (Ketchup's cleared buns *and* Skewer's destroyed buns).
- **Cascades after the impact score normally** (the flat `POINTS_MATCH` they already fire via
  `OnMatchEliminated`) — no new code; a chain-triggering Mustard is rewarded on top via the
  normal match path.

### 7. Usage restrictions — **MOSTLY RESOLVED**
Empty column (and no-target generally) → faller fizzles, item spent (uniform fizzle rule).
Residual: can a consumable be used while the game is paused? (Lean: no — usable only in live
`Playing`, consistent with the world-keeps-moving decision.)

### 8. Audio — **PLAN SET (real tuning deferred to the audio pass)**
Wire these hooks during implementation (reuse existing tones as placeholders); author/tune the
actual procedural sounds later in the dedicated audio pass (`AudioConfig`):
- `PlayConsumableCollect()` — tapping a consumable-carrying fairy (gems keep existing behavior).
- `PlayConsumableUse(ConsumableType)` — per-consumable impact (Ketchup splat / Mustard squirt /
  Skewer thunk + bun-slam). Value-varying SFX → flat switch inside `AudioManager` (not effect
  polymorphism).
- `PlayConsumableFizzle()` — faller hits the floor with no target.
- Cascades reuse the existing match SFX automatically.
- Optional/deferred: "grab" blip on lift, "deny" blip on cancel.

---

## STATUS — IMPLEMENTED (2026-06-12)
All design questions resolved and built across `Scripts/Consumables/*`,
`Monetization/BurgerFairy*`, plus config/grid/input/audio edits. Deferred to later passes:
real consumable SFX (placeholder tones + `_*Override` slots in place) and slot-layout/size
tuning (placeholders in `UIStyles`).

