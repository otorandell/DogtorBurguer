# Session 2026-09-13 (part 2) — consumable hitboxes + tutorial expansion

Follow-on from the run-resume work (`session-2026-09-13.md`). Five items from Oscar's device
playtest.

## 1. Consumable drag: overlapping hitboxes

**Reported**: "sometimes more than one hitbox collides, especially on the text areas for the
consumables; in those columns dropping a consumable works weirdly."

Two real, separate bugs in the 3-slot inventory row (plate 80 wide, 86 apart, badge 34 at
offset (28, -28) so it overhangs the plate by ~5px on the right and bottom):

1. **The count number was un-grabbable.** `ConsumableSlotWidget.Contains` hit-tested the PLATE
   rect only. The badge sticks out past it, and the inter-slot gap is 6px — so pressing the
   number's right/bottom edge matched NO slot, and the press fell through to chef/preview logic.
   Fixed: test the plate grown by `CONSUMABLE_SLOT_HIT_PADDING` (12). The grown areas now
   overlap, so `TryGetSlotTypeAt` resolves by **nearest slot centre** instead of iteration order
   (which previously handed any overlap to whichever type came first in the enum — the literal
   "more than one hitbox collides").
2. **The plus box stole presses from the neighbouring slot.** It was a UGUI `Button` with
   `raycastTarget = true`, drawn overhanging toward the next slot — so pressing to pick up
   Mustard or Skewer could open the Shop instead. Two hitboxes, two different gestures,
   overlapping. Fixed: the shop deep-link moved to the **whole plate**, raycast-enabled **only
   while the slot is empty**; the badge is now decoration.

Plus a belt-and-braces rule in `TouchInputHandler.BeginPress`: a press anywhere on the row that
did NOT start a carry is **swallowed** (`ConsumableInventoryView.IsOverRow`). A press meant for
the tray must never end up doing something unrelated elsewhere on screen.

**Note**: the row sits at world y ≈ 2.6, above the playfield top (y = 1.0), so `IsOverPlayfield`
was already correctly refusing drops there — the release side was not broken, only the pick-up.

## 2-5. Tutorial

### Instant board setup (was ~6 s of watching)
`PreparePowerUp` dropped eight junk pieces one at a time, *waiting for each to land* — ~0.78 s
each. Now `PlaceInstantly` → `IngredientSpawner.SpawnRestored`, the already-landed seat added for
run resume, so setup boards appear at once. Applied to every pre-placed stack (Swap too). Only
pieces the player must REACT to still fall.

⚠️ `SpawnRestored` skips the landing match check, so a pre-placed stack must be match-free on its
own — two adjacent same-type pieces would just sit there looking broken. All the scripted boards
alternate types deliberately.

### All three power-ups (was Ketchup only)
`TutorialStep.PowerUp` became `Ketchup → Mustard → Skewer`, sharing one `RunPowerUpStep(step,
type, body, done, next, buildBoard, isDone)`: clear the board, lay out a situation the item
solves, grant a free virtual item, wait for a per-step completion test.

`TutorialMode.VirtualKetchup` (bool) generalized to `VirtualItem` (`ConsumableType?`), so each
step grants its own free item that never depletes — a fizzle or a wrong column just means trying
again, and the persistent stock is never touched.

Boards and completion tests:
- **Ketchup** — one tall alternating junk column; done when the board shrinks.
- **Mustard** — THREE types spread over all four columns, so the sweep takes the top two and
  leaves the third standing. That's the lesson: it is not a board wipe. Done when the board
  shrinks.
- **Skewer** — a bottom bun buried mid-stack. ⚠️ `ConsumableSkewer` **keeps every regular** and
  only destroys SURPLUS bottom buns, so the piece count does NOT drop for a single-bun column.
  Its test is therefore the bun reaching row 0 — which is also exactly what the player is being
  taught to look for. (A uniform "board shrank" rule would have hung this step forever.)

### More room to practise pairs
Match is now `MatchRounds` (3) rounds instead of one. Each round seats a lone piece and drops its
twin over a NEIGHBOURING column, recomputed per attempt, so one swap always solves it; the
ingredient type rotates through `MatchTypes` so it doesn't read as the same drop three times.
Body carries a `{0}/{1}` counter. Still unfailable — wrong landings poof and the twin returns.

Added a beat at the top of the coroutine: `ChefController.OnFlipped` fires *before*
`SwapColumnsWithWaveEffect` runs, so clearing the board immediately would yank it out from under
the animation.

### Fast-drop is now taught
New `TutorialStep.FastDrop` between Match and Burger. Pieces drift at `TeachFall` (0.9 s/step,
slower than anything else in the tutorial) and keep coming until one is tapped. Fast-drop is
masked OFF during Match so this step introduces it alone.

Hook: `IngredientSpawner.OnFastDrop`, raised at the single point the tap resolves
(`TryTapFallingIngredient`).

### Localization
`TutMatchBody` regenericized (the type rotates now, so it can't name "the patty") + a round
counter; new `TutFastDrop{Title,Body,Done}`; `TutPowerUp{Body,Done}` split into
`Tut{Ketchup,Mustard,Skewer}{Body,Done}`. The "Burger Fairies bring more power-ups" line moved to
the LAST power-up step. All 7 tables filled — verified programmatically: 115 keys, no missing, no
extras, no duplicates in any table.

`TutorialPopup`'s body now AutoFits down to `TUT_BODY_SIZE_MIN` (13) — the box is fixed art and
the new bodies are longer, so translations shrink rather than clip.

## Verification
`dotnet build Assembly-CSharp.csproj` — **0 errors** (4 pre-existing CS0162 warnings). Localization
table coverage checked by script.

**Not playtested.** Everything here is untested in the editor or on device.

## Pending / to check on the next playtest
- The whole tutorial is now ~3 min (was ~90 s). If that's too long, `MatchRounds` is one constant
  and the three power-up steps are three `StartCoroutine` lines.
- Tune `CONSUMABLE_SLOT_HIT_PADDING` if grabbing still feels tight or now steals neighbours.
- Mustard step: confirm the third type visibly survives the sweep (that's the whole point).
- Skewer step: confirm the bun visibly travels to the floor.
- Consider whether Match should clear the board between rounds at all — it currently does, which
  is clean but abrupt.
