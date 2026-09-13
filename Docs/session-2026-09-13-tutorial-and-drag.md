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
Fast-drop is LIVE during this step (Oscar) — reaching for a falling piece must not be a dead tap.

Added a beat at the top of the coroutine: `ChefController.OnFlipped` fires *before*
`SwapColumnsWithWaveEffect` runs, so clearing the board immediately would yank it out from under
the animation.

### Fast-drop is now taught
New `TutorialStep.FastDrop` between Match and Burger. Pieces drift at `TeachFall` (0.9 s/step,
slower than anything else in the tutorial) and keep coming until one is tapped. It still runs
when the player already discovered tapping during Match - the step names the trick.

Hook: `IngredientSpawner.OnFastDrop`, raised at the single point the tap resolves
(`TryTapFallingIngredient`).

### Localization
`TutMatchBody` regenericized (the type rotates now, so it can't name "the patty") + a round
counter; new `TutFastDrop{Title,Body,Done}`; `TutPowerUp{Body,Done}` split into
`Tut{Ketchup,Mustard,Skewer}{Body,Done}`. The "Burger Fairies bring more power-ups" line moved to
the LAST power-up step. All 7 tables filled — verified programmatically: 115 keys, no missing, no
extras, no duplicates in any table.

Tip text got BIGGER (Oscar): `TUT_BODY_SIZE` 22 → 28, `TUT_TITLE_SIZE` 24 → 30. Both AutoFit
(floors `TUT_BODY_SIZE_MIN` 16 / `TUT_TITLE_SIZE_MIN` 16) so the fixed box art can't be
overflowed by a long translation — watch the German power-up lines, they're the longest.

### Wrapped text was unreadable (the real cause of the cramped tips)
`UIFactory.AddStyledText` set `lineSpacing = TEXT_LINE_SPACING` (**-45**, a percentage of font
size) on EVERY text. That trim exists because Baloo's native leading is loose, and it is right for
single-line labels — but on a WRAPPED paragraph it pulls lines into each other, so descenders hit
the next line's caps. The tutorial tips and the How to Play bullets were both rendering as solid
blocks; the bigger tip font just made it obvious.

Paragraphs now take `TEXT_LINE_SPACING_WRAP` (-10), chosen by `UIFactory` on the `wrap` flag — so
the fix reaches both call sites. `TUT_BODY_H` also grew 120 → 150 (the plate is ~219 tall, so
there was unused room inside it).

⚠️ The How-to bullets get TALLER as a result. They sit in a VerticalLayoutGroup +
ContentSizeFitter so they re-flow on their own, but the 6 pages should be checked for overflow.

### ...and then AutoFit silently un-wrapped them
Adding `UIFactory.AutoFit` to the tutorial body (to stop long translations clipping) **turned
wrapping off**: its first line is `tmp.textWrappingMode = NoWrap`, which is deliberate — it is
built for single-line labels. On a paragraph that means the text stops wrapping entirely and grows
sideways as one endless line, which autosize then shrinks to fit horizontally. Oscar spotted it as
"the tips do not wrap properly, maybe they have too much horizontal space to grow" — exactly right.

Fixed with `UIFactory.AutoFitWrapped` (same fixed rect and size floor, wrapping left ON) and a
warning on `AutoFit`'s summary. Audited every other `AutoFit` call site: all ten are genuine
single-line labels (prices, button words, HUD numbers, credits names), so NoWrap is correct there
— the tutorial body was the only paragraph.

`TUT_BODY_INSET` also went 90 -> 120. The plate is an ELLIPSE (~400x219), so it narrows toward the
top and bottom of the text block: at the body rect's extremes (+/-75) it is only ~291 wide, so a
310 column ran past the art. 280 sits inside it.

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
- How to Play: check all 6 pages still fit now that bullets have real leading.
- Tips: long translations (the German power-up lines) will AutoFit down; if they shrink too far,
  the fairy sentence on `TutSkewerDone` is the longest tail and belongs on the Ready step anyway.
- `TEXT_LINE_SPACING_WRAP` (-10) is an estimate against Baloo's native leading — tune live.
- Consider whether Match should clear the board between rounds at all — it currently does, which
  is clean but abrupt.
