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

## Phase 3: Match Detection Polish (NEXT)
**Goal: Visual feedback and juice**

### Scripts to Create:
1. `ScorePopup.cs` - Floating score text
2. `MatchEffects.cs` - Particle/visual effects (optional)

### Features to Implement:
- [ ] Score popup when match occurs (+10 floating text)
- [ ] Visual flash/highlight on matching ingredients
- [ ] Screen shake on match (subtle)
- [ ] Particle effect on destruction (optional)

### Milestone:
Matching ingredients have satisfying visual feedback.

---

## Phase 4: Burger Completion Polish
**Goal: Celebrate burger completion**

### Scripts to Modify:
1. `GridManager.cs` - Expand GenerateBurgerName()

### Scripts to Create:
1. `BurgerPopup.cs` - Shows burger name with animation

### Features to Implement:
- [ ] Expanded funny names pool (20+ names)
- [ ] Burger name popup display
- [ ] Celebration animation (screen flash, scale pop)
- [ ] Bonus score visual feedback

### Milestone:
Complete burgers are celebrated with funny name and effects.

---

## Phase 5: Game Flow & Difficulty
**Goal: Complete game loop with progression**

### Scripts to Create:
1. `DifficultyManager.cs` - Speed/ingredient progression

### Scripts to Modify:
1. `IngredientSpawner.cs` - Variable speed, ingredient pool
2. `GameManager.cs` - Game over handling, restart

### Features to Implement:
- [ ] Speed increases over time
- [ ] Start with 4 ingredients, unlock more
- [ ] Game over detection (column overflow)
- [ ] Game over screen with final score
- [ ] Restart functionality

### Milestone:
Game starts, speeds up, ends when column overflows, can restart.

---

## Phase 6: UI & Polish
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
Phase 1 ✅ ──► Phase 2 ✅ ──► Phase 3 ──► Phase 4
   │             │              │           │
   ▼             ▼              ▼           ▼
 Grid          Chef          Matches     Burgers
 Spawn         Move          Polish      Polish

         ──► Phase 5 ──► Phase 6 ──► Phase 7 ──► Phase 8
                │           │           │           │
                ▼           ▼           ▼           ▼
            Game Loop     UI/UX       Art        Launch
```

---

## Notes for Claude Code Sessions

- Always test after each phase before moving on
- Keep scripts small and focused
- Use `[SerializeField]` for editor-configurable values
- Comment complex algorithms
- Use DOTween for ALL animations (no Update-based lerps)
- Commit after each major feature
