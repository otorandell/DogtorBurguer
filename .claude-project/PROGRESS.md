# Dogtor Burguer! - Development Progress

## Current Status: Phase 8 (Polish) - In Progress

**Last Session:** 2026-01-23

---

## What's Done

### Phase 1: Core Grid & Ingredients
- Grid system (4 columns, 13 max rows)
- Ingredients fall step-by-step with DOTween
- Ingredients stack on columns
- Match detection (same ingredients eliminate, including BunBottom pairs)
- Burger detection (bun bottom + ingredients + bun top)
- Basic scoring
- GameManager with state machine

### Phase 2: Chef Controller & Input
- `ChefController.cs` - 3 positions, movement with DOTween
- `TouchInputHandler.cs` - New Unity Input System (touch + mouse)
- Full column swap mechanic with wave effects
- Falling ingredient swap below threshold
- Chef 180 flip animation on swap

### Phase 3: Match Detection Polish
- `FeedbackManager.cs` - Centralized effect manager
- Ingredient blink effect before destruction
- Camera shake on matches
- Position-aware events (OnMatchEffect, OnBurgerEffect)

### Phase 4: Burger Completion Polish
- Expanded burger name generator (size-aware, 60+ combos)
- Screen flash on burger completion
- Stronger camera shake for burgers

### Phase 5: Game Flow & Difficulty
- `DifficultyManager.cs` - 10-level progression based on ingredients placed
- Easier starting difficulty (3s spawn, 0.3s fall, 3 ingredient types)
- Ramps to max difficulty (1.2s spawn, 0.1s fall, 7 types)
- Level thresholds: 0, 3, 7, 12, 18, 25, 33, 42, 52, 64 ingredients
- Game over + restart

### Phase 6: UI & Audio
- `GameHUD.cs` - Canvas overlay with score, level, gems (positioned inside top-left panel)
- `GameOverPanel.cs` - Game over screen with restart/continue options
- `AudioManager.cs` - Procedural SFX generation (sine waves, no audio files needed)
  - Match sound (ascending beep)
  - Burger sounds (5 tiers: poor, small, medium, large, mega, max)
  - Level up sound (cheerful ascending A4-C#6)
  - Game over sound (descending A4-C4)
  - Squeeze sound (descending 800-500Hz, pitched per step)
  - Fast drop whoosh (1200-300Hz)
  - Early spawn pop (500-1000Hz)
- `MusicManager.cs` - Loads tracks from Resources/Music/ folders, random selection, persists across scenes

### Phase 7: Monetization & Menus
- `SaveDataManager.cs` - PlayerPrefs persistence (gems, high score, games played, sound toggle)
- `GemPackSpawner.cs` - In-game gem packs that fall and can be tapped
- `GameOverPanel.cs` - Continue with gems, watch ad, restart options
- `SceneLoader.cs` - Main menu scene with play button
- `MainMenuController.cs` - Menu UI with high score display and sound toggle

### Phase 8: Polish (Current)
- **Controls polish:**
  - Position bubble indicators (semi-transparent circles at each chef position)
  - Forgiving tap detection (screen divided into horizontal thirds)
  - Tap priority: preview > falling ingredient > chef > position thirds

- **Gameplay polish:**
  - Top buns destroy when placed without a bottom bun below ("Too bad!" text)
  - Top buns can't spawn if no bottom buns exist on grid
  - Bottom buns cancel each other when matched
  - Double ingredient spawn at higher difficulty levels
  - Blinking ingredient preview before spawn (configurable duration/blinks)
  - Forced bun spawn after ingredient_types x 2 spawns without bun

- **Burger compress animation:**
  - Top bun pushes down collecting ingredients one by one
  - Travel spacing: 20% of regular cell height
  - Pause between each compress step
  - Final smack slams everything tight (15% spacing) against bottom bun
  - Rising pitch squeeze sound per step (0.6 to 1.8)

- **Fast drop & early spawn:**
  - Tap falling ingredient = fast drop + distance-based bonus (cyan text)
  - Tap spawn preview = immediate spawn + countdown bonus (green text)
  - Preview shows live countdown of decreasing bonus points

- **Burger tiers:**
  - Poor (0 ingredients): 5 bonus, "Just Bread...", sad descending sound
  - Small (1-2): 20 bonus, random name, 2-note ascending
  - Medium (3-4): 50 bonus, random name, 4-note arpeggio
  - Large (5-6): 100 bonus, random name, 5-note with harmonics
  - Mega (7-8): 200 bonus, random name, 6-note two-octave
  - Max (9+): 500 bonus, always "DOKTOR BURGUER!", triumphant fanfare

- **Layout panels:**
  - `GameLayout.cs` - Three 9-sliced rounded-rect panels (grid, top-left, top-right)
  - Grid panel: centered below, covers the ingredient area
  - Top-left panel: contains HUD (score, level, gems)
  - Top-right panel: reserved for Burger Challenge feature

- **Test modes:**
  - `TestSettings`: Forces specific spawn sequence
  - `TestBurgerColumn`: All spawns on rightmost column (BunBottom, Meat/Cheese alternating, BunTop at MAX_ROWS)
  - `TestDualColumn`: Left column = Meat/Cheese alternating, Right column = buns (BunBottom first, then BunTop), starts at configurable high level

- **FloatingText utility:**
  - Static `Spawn()` method for world-space text
  - Floats up 1.5 units, fades out over 0.8s

---

## What's Next

### Burger Challenge System (Top-Right Panel)
- See BURGER_CHALLENGE.md for full spec

### Remaining Polish
- Final sprites and visual art
- Background art
- Sprite animations (idle wobble, land squash)
- Juice: particles on match/burger

---

## Scripts (Current)

```
Assets/_Project/Scripts/
├── Core/
│   ├── Constants.cs           # Grid, timing, scoring, difficulty, monetization
│   ├── GameState.cs           # Enum: Menu, Playing, Paused, GameOver
│   ├── GameManager.cs         # Game loop, score, state, test toggles
│   ├── DifficultyManager.cs   # Level progression, spawner control
│   ├── FeedbackManager.cs     # Score popups, burger popups, shake, flash
│   └── SceneLoader.cs         # Scene transition helper
├── Grid/
│   ├── GridManager.cs         # Columns, match/burger detection, swap, compress animation
│   └── Column.cs              # Ingredient stack, swap helpers, match check
├── Ingredients/
│   ├── IngredientType.cs      # Enum: Meat..Egg + BunBottom, BunTop
│   ├── Ingredient.cs          # Fall, land, FastDrop, DestroyWithFlash
│   └── IngredientSpawner.cs   # Spawning, preview, early spawn, fast drop, test modes
├── Chef/
│   └── ChefController.cs      # Movement, position bubbles, swap trigger
├── Input/
│   └── TouchInputHandler.cs   # New Input System (EnhancedTouch), tap/swipe
├── Audio/
│   ├── AudioManager.cs        # Procedural SFX (all sounds generated at runtime)
│   └── MusicManager.cs        # Background music, per-scene tracks
├── UI/
│   ├── GameHUD.cs             # Score, level, gems display (Canvas overlay)
│   ├── GameOverPanel.cs       # Game over UI with continue/restart
│   ├── GameLayout.cs          # Rounded-rect panel borders (9-slice sprites)
│   ├── FloatingText.cs        # Static utility for floating score text
│   └── MainMenuController.cs  # Main menu UI
└── Monetization/
    ├── SaveDataManager.cs     # PlayerPrefs persistence (gems, scores, settings)
    └── GemPackSpawner.cs      # In-game gem pack drops
```

---

## Key Constants

- 4 columns, 13 max rows
- Cell: 1.4 wide x 0.4 visual height (60% overlap)
- Grid origin: (-2.1, -4.5)
- Spawn interval: 3.0s (start) to 1.2s (max)
- Fall step: 0.3s (start) to 0.1s (max)
- 3 starting ingredient types, max 7
- 10 difficulty levels
- Chef has 3 positions

---

## Technical Notes

### Procedural Audio
- All sounds generated via `AudioClip.Create()` + `SetData()`
- Sine wave synthesis with frequency sweeps and envelope shaping
- Dedicated AudioSource for pitched sounds (squeeze)
- Burger sounds increase in complexity per tier (more notes, more harmonics)

### Burger Compress Animation
- Coroutine-based: pauses spawning, animates, resumes
- Travel spacing: 20% of CELL_VISUAL_HEIGHT
- Smack spacing: 15% of CELL_VISUAL_HEIGHT
- Squeeze SFX pitch: lerps 0.6 to 1.8 across steps

### Spawn System
- Preview blinks before actual spawn (configurable duration)
- Early spawn: tap preview for time-based bonus
- Fast drop: tap falling ingredient for distance-based bonus
- Forced bun spawn prevents long stretches without bun pieces
- Double spawn at higher levels (chance scales with level)

### Layout System
- 9-slice sprites generated at runtime (rounded rectangles)
- Three panels: grid area, top-left (HUD), top-right (challenge)
- Sorting order -50 (behind game elements)
