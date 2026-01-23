# Dogtor Burguer! - Implementation Plan

## Phase 0: Project Setup ✅
**Owner: User**

- [x] Create new Unity 2D project (Unity 2022.3 LTS)
- [x] Import DOTween from Asset Store
- [x] Create folder structure as per ARCHITECTURE.md
- [x] Set up 9:16 portrait resolution
- [x] Configure new Unity Input System

---

## Phase 1: Core Grid & Basic Ingredients ✅

### Scripts Created:
- [x] `Constants.cs` - Grid dimensions, timing values
- [x] `IngredientType.cs` - Enum for all ingredient types
- [x] `GridManager.cs` - Manages 4 columns
- [x] `Column.cs` - Stack logic for one column
- [x] `Ingredient.cs` - Base ingredient behavior
- [x] `IngredientSpawner.cs` - Spawns ingredients periodically
- [x] `GameState.cs` - State enum
- [x] `GameManager.cs` - Game state machine

### Milestone: ✅
Ingredients spawn randomly, fall step-by-step, and stack on columns.

---

## Phase 2: Chef Controller & Plate Swap ✅

### Scripts Created:
- [x] `ChefController.cs` - Position management, movement
- [x] `TouchInputHandler.cs` - New Input System (touch + mouse)

### Features Implemented:
- [x] Chef moves between 3 positions
- [x] Full column swap (all stacked ingredients)
- [x] Falling ingredient swap (below threshold)
- [x] Match/burger detection after swap

### Editor Setup:
- [x] Chef GameObject with sprite
- [x] TouchInputHandler on GameManager
- [x] References assigned

### Milestone: ✅
Player can tap to move chef, tap chef to swap entire columns.

---

## Phase 3: Match Detection Polish ✅
**Goal: Visual feedback and juice**

### Scripts Created:
- [x] `ScorePopup.cs` - Floating score text (TMPro + DOTween)
- [x] `FeedbackManager.cs` - Centralized effect spawning

### Features Implemented:
- [x] Score popup when match occurs (+N floating text)
- [x] Visual blink on matching ingredients (DestroyWithFlash)
- [x] Screen shake on match (subtle, stronger for burgers)
- [x] Position-aware events (OnMatchEffect, OnBurgerEffect)

### Milestone: ✅
Matching ingredients have satisfying visual feedback.

---

## Phase 4: Burger Completion Polish ✅
**Goal: Celebrate burger completion**

### Scripts Created:
- [x] `BurgerPopup.cs` - Scale-pop name + score display

### Features Implemented:
- [x] Expanded funny names pool (60+ English combos, size-aware)
- [x] Burger name popup display with scale-pop animation
- [x] Screen flash celebration (runtime white sprite)
- [x] Bonus score visual feedback (orange colored popup)

### Milestone: ✅
Complete burgers are celebrated with funny name and effects.

---

## Phase 5: Game Flow & Difficulty ✅
**Goal: Complete game loop with progression**

### Scripts Created:
- [x] `DifficultyManager.cs` - 10-level ingredient-based progression

### Features Implemented:
- [x] Speed increases based on ingredients placed
- [x] Start with 3 ingredients, unlock up to 7
- [x] 10 levels with score thresholds (3, 7, 12, 18, 25, 33, 42, 52, 64)
- [x] Game over detection (column overflow) + restart
- [x] Level display in HUD
- [x] Auto-resolving references

### Milestone: ✅
Game starts easy, speeds up by level, ends when column overflows, can restart.

---

## Phase 6: UI & Polish (NEXT)
**Goal: Full UI, feedback, juice**

### Scripts to Create:
1. `ScoreDisplay.cs` - Live score UI
2. `GameOverPanel.cs` - End screen
3. `AudioManager.cs` - Sound effects

### Editor Setup:
- [ ] UI Canvas setup
- [ ] TextMeshPro for texts
- [ ] Sound effects (placeholder beeps ok)

### Milestone:
Polished game feel with UI and audio feedback.

---

## Phase 7: Art & Audio Integration
**Goal: Replace placeholders with final assets**

### Tasks:
- [ ] Import final sprites (cartoon style)
- [ ] Sprite animations (idle wobble, land squash)
- [ ] Background art
- [ ] Final sound effects
- [ ] Music track

---

## Phase 8: Monetization & Menus
**Goal: Complete product**

### Scripts to Create:
1. `AdsManager.cs` - Ad integration
2. `CurrencyManager.cs` - Gems system
3. `MainMenuController.cs` - Main menu logic
4. `ContinuePanel.cs` - Watch ad to continue

### Editor Setup:
- [ ] Main menu scene
- [ ] Ad SDK integration

---

## Implementation Order Summary

```
Phase 1 ✅ ──► Phase 2 ✅ ──► Phase 3 ✅ ──► Phase 4 ✅
   │             │              │              │
   ▼             ▼              ▼              ▼
 Grid          Chef          Matches        Burgers
 Spawn         Move          Polish         Polish

         ──► Phase 5 ✅ ──► Phase 6 ──► Phase 7 ──► Phase 8
                │              │           │           │
                ▼              ▼           ▼           ▼
            Difficulty       UI/UX       Art        Launch
```

---

## Notes for Claude Code Sessions

- Always test after each phase before moving on
- Keep scripts small and focused
- Use `[SerializeField]` for editor-configurable values
- Comment complex algorithms
- Use DOTween for ALL animations (no Update-based lerps)
- Commit after each major feature
