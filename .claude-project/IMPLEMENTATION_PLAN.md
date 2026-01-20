# Dogtor Burguer! - Implementation Plan

## Phase 0: Project Setup (Manual in Unity Editor)
**Owner: User**

- [ ] Create new Unity 2D project (Unity 2022.3 LTS recommended)
- [ ] Import DOTween from Asset Store or Package Manager
- [ ] Create folder structure as per ARCHITECTURE.md
- [ ] Set up 9:16 portrait resolution (1080x1920 or 720x1280)
- [ ] Configure build settings for Android/iOS

---

## Phase 1: Core Grid & Basic Ingredients
**Goal: Ingredients fall and stack in columns**

### Scripts to Create:
1. `Constants.cs` - Grid dimensions, timing values
2. `IngredientType.cs` - Enum for all ingredient types
3. `GridManager.cs` - Manages 4 columns
4. `Column.cs` - Stack logic for one column
5. `Ingredient.cs` - Base ingredient behavior
6. `IngredientSpawner.cs` - Spawns ingredients periodically

### Editor Setup Needed:
- Create placeholder sprites (colored squares)
- Set up main camera for vertical view
- Create ingredient prefabs

### Milestone:
Ingredients spawn randomly, fall step-by-step, and stack on columns.

---

## Phase 2: Chef Controller & Plate Swap
**Goal: Player can move chef and swap plates**

### Scripts to Create:
1. `ChefController.cs` - Position management, input
2. `TouchInputHandler.cs` - Touch regions for 3 positions
3. `PlateSwapper.cs` - Swap logic between two columns

### Editor Setup Needed:
- Create chef placeholder sprite
- Set up touch input regions (UI buttons or raycasts)

### Milestone:
Player can tap to move chef, tap chef to swap two column tops.

---

## Phase 3: Match Detection (Same Ingredients)
**Goal: Two same ingredients on top of each other are eliminated**

### Scripts to Modify:
1. `Column.cs` - Add match checking after ingredient lands
2. `Ingredient.cs` - Add destruction animation

### Scripts to Create:
1. `ScoreManager.cs` - Track and display score

### Milestone:
Matching ingredients disappear with animation, +10 points awarded.

---

## Phase 4: Burger Completion
**Goal: Buns + ingredients form complete burgers**

### Scripts to Create:
1. `BurgerDetector.cs` - Detect complete burgers in column
2. `BurgerData.cs` - Data structure for a completed burger
3. `BurgerNameGenerator.cs` - Generate funny names

### Scripts to Modify:
1. `IngredientSpawner.cs` - Add bun spawning logic
2. `ScoreManager.cs` - Burger scoring

### Milestone:
Complete burgers are detected, scored with bonus, funny name shown.

---

## Phase 5: Game Flow & Difficulty
**Goal: Complete game loop with progression**

### Scripts to Create:
1. `GameManager.cs` - Game state machine
2. `GameState.cs` - State enum
3. `DifficultyManager.cs` - Speed/ingredient progression

### Scripts to Modify:
1. `IngredientSpawner.cs` - Variable speed, ingredient pool growth

### Editor Setup Needed:
- Game Over detection (column overflow)
- Restart functionality

### Milestone:
Game starts, speeds up over time, ends when column overflows.

---

## Phase 6: UI & Polish
**Goal: Full UI, feedback, juice**

### Scripts to Create:
1. `ScoreDisplay.cs` - Live score UI
2. `GameOverPanel.cs` - End screen with score
3. `BurgerNamePopup.cs` - Floating text for burger names
4. `AudioManager.cs` - Sound effects

### Editor Setup Needed:
- UI Canvas setup
- TextMeshPro for texts
- Particle effects for matches/burgers
- Sound effects (placeholder beeps ok)

### Milestone:
Polished game feel with feedback on all actions.

---

## Phase 7: Art & Audio Integration
**Goal: Replace placeholders with final assets**

### Tasks:
- Import final sprites (cartoon cutre style)
- Sprite animations (idle wobble, land squash)
- Background art
- Final sound effects
- Music track

---

## Phase 8: Monetization & Menus
**Goal: Complete product**

### Scripts to Create:
1. `AdsManager.cs` - Ad integration
2. `CurrencyManager.cs` - Gems system
3. `MainMenuController.cs` - Main menu logic
4. `ContinuePanel.cs` - Watch ad to continue

### Editor Setup Needed:
- Main menu scene
- Ad SDK integration (Unity Ads or AdMob)

---

## Implementation Order Summary

```
Phase 1 ──► Phase 2 ──► Phase 3 ──► Phase 4
   │           │           │           │
   ▼           ▼           ▼           ▼
 Grid        Chef       Matches     Burgers
 Spawn       Move       Destroy     Complete

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
