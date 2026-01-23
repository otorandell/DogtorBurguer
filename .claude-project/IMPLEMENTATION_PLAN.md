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

## Phase 6: UI & Polish ✅

### Scripts Created:
- [x] `GameHUD.cs` - Live score + level + gem counter UI
- [x] `GameOverPanel.cs` - End screen with continue/restart/menu
- [x] `AudioManager.cs` - Procedural sound effects (match, burger, level up, game over)

### Features Implemented:
- [x] Programmatic UI Canvas (no prefabs)
- [x] TextMeshPro score, level, and gem displays
- [x] Procedural audio via AudioClip.Create + SetData
- [x] EventSystem + InputSystemUIInputModule for button input

### Milestone: ✅
Polished game feel with UI and audio feedback.

---

## Phase 7: Monetization & Menus ✅
**Goal: Gem economy, ads, menus**

### Scripts Created:
- [x] `SaveDataManager.cs` - PlayerPrefs persistence (gems, high score, sound, games played)
- [x] `AdManager.cs` - Mock Unity Ads (interstitial + rewarded)
- [x] `GemPackSpawner.cs` - Timer-based flying collectible spawner
- [x] `GemPack.cs` - Tappable flying gem collectible with DOTween path
- [x] `SceneLoader.cs` - Static scene navigation utility
- [x] `MainMenuUI.cs` - Full main menu (Play, Shop, Settings, Leaderboard, Credits)
- [x] `ShopPanel.cs` - Watch ad / buy gems (IAP placeholder)
- [x] `SettingsPanel.cs` - Sound toggle

### Features Implemented:
- [x] MainMenu scene (scene 0) with programmatic UI
- [x] Gem currency persists across sessions
- [x] Continue after game over (spend gems or watch ad)
- [x] Interstitial ads every 3rd game
- [x] Flying gem packs during gameplay (8% spawn chance)
- [x] Sound on/off toggle with AudioListener.volume
- [x] High score tracking and display

### Milestone: ✅
Full menu system, gem economy, and ad integration (mock).

---

## Phase 8: Art & Audio Integration (NEXT)
**Goal: Replace placeholders with final assets**

### Tasks:
- [ ] Import final sprites (cartoon style)
- [ ] Sprite animations (idle wobble, land squash)
- [ ] Background art
- [ ] Final sound effects
- [ ] Music track

---

## Implementation Order Summary

```
Phase 1 ✅ ──► Phase 2 ✅ ──► Phase 3 ✅ ──► Phase 4 ✅
   │             │              │              │
   ▼             ▼              ▼              ▼
 Grid          Chef          Matches        Burgers
 Spawn         Move          Polish         Polish

    ──► Phase 5 ✅ ──► Phase 6 ✅ ──► Phase 7 ✅ ──► Phase 8
           │              │              │              │
           ▼              ▼              ▼              ▼
       Difficulty       UI/UX       Monetize          Art
```

---

## Notes for Claude Code Sessions

- Always test after each phase before moving on
- Keep scripts small and focused
- Use `[SerializeField]` for editor-configurable values
- Comment complex algorithms
- Use DOTween for ALL animations (no Update-based lerps)
- Commit after each major feature
