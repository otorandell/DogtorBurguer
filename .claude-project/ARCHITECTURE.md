# Dogtor Burguer! - Architecture Document

## Unity Project Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs       # Game loop, score, state, test toggles
│   │   │   ├── GameState.cs         # Enum: Menu, Playing, Paused, GameOver
│   │   │   ├── Constants.cs         # All game constants
│   │   │   ├── DifficultyManager.cs # Level progression
│   │   │   ├── FeedbackManager.cs   # Popups, shake, flash
│   │   │   └── SceneLoader.cs       # Scene transitions
│   │   │
│   │   ├── Grid/
│   │   │   ├── GridManager.cs       # Columns, match/burger/compress
│   │   │   └── Column.cs            # Ingredient stack per column
│   │   │
│   │   ├── Ingredients/
│   │   │   ├── Ingredient.cs        # Fall, land, FastDrop, animations
│   │   │   ├── IngredientType.cs    # Enum + extensions
│   │   │   └── IngredientSpawner.cs # Spawning, preview, test modes
│   │   │
│   │   ├── Chef/
│   │   │   └── ChefController.cs    # Movement, bubbles, swap trigger
│   │   │
│   │   ├── Input/
│   │   │   └── TouchInputHandler.cs # Touch/mouse/keyboard input
│   │   │
│   │   ├── Audio/
│   │   │   ├── AudioManager.cs      # Procedural SFX
│   │   │   └── MusicManager.cs      # Per-scene background music
│   │   │
│   │   ├── UI/
│   │   │   ├── GameHUD.cs           # Score/level/gems overlay
│   │   │   ├── GameOverPanel.cs     # End screen
│   │   │   ├── GameLayout.cs        # Rounded-rect panel borders
│   │   │   ├── FloatingText.cs      # World-space floating text
│   │   │   └── MainMenuController.cs # Menu UI
│   │   │
│   │   └── Monetization/
│   │       ├── SaveDataManager.cs   # PlayerPrefs persistence
│   │       └── GemPackSpawner.cs    # In-game gem drops
│   │
│   ├── Prefabs/
│   │   └── Ingredients/
│   │       └── Ingredient.prefab
│   │
│   └── Sprites/
│       ├── Ingredients/
│       └── Chef/
│
├── Resources/
│   └── Music/
│       ├── MenuTrack/              # Menu background music files
│       └── GameTrack/              # Game background music files
│
├── Scenes/
│   ├── MainMenu.unity
│   └── Game.unity
│
├── Plugins/
│   └── DOTween/
│
└── TextMesh Pro/
```

---

## Core Systems

### 1. Grid System

```
GridManager (Singleton)
├── Column[] _columns (4 columns)
├── List<Ingredient> _fallingIngredients
├── OnIngredientLanded() - Match/burger detection + overflow check
│   ├── BunTop special handling (burger check before overflow)
│   ├── BunTop without BunBottom below = destroy ("Too bad!")
│   └── BunBottom match = both cancel ("Too bad!")
├── CheckAndProcessBurger() - Finds and scores burgers
├── BurgerCompressAnimation() - Coroutine squeeze animation
├── SwapColumnsWithWaveEffect() - Full column swap
├── GetFallingIngredients() - For tap detection
├── ClearTopHalf() - For continue feature
└── Events:
    ├── OnGameOver
    ├── OnMatchEliminated(int points)
    ├── OnBurgerCompleted(int points, string name)
    ├── OnBurgerEffect(Vector3 pos, int points, string name, int ingredientCount)
    ├── OnMatchEffect(Vector3 pos, int points)
    └── OnIngredientPlaced

Column
├── List<Ingredient> _ingredients (stack)
├── IsOverflowing property (stack >= MAX_ROWS)
├── GetNextLandingPosition()
├── GetSpawnPosition() - Top of column (preview position)
├── TakeAllIngredients() / SetAllIngredients() - For swaps
├── CheckForMatch() - Top two same type (including BunBottom pairs)
├── RemoveIngredientsInRange() - For burger completion
└── CollapseFromRow() - After removal
```

### 2. Ingredient System

```
IngredientType (Enum)
├── Regular: Meat(0), Cheese(1), Tomato(2), Onion(3), Pickle(4), Lettuce(5), Egg(6)
└── Buns: BunBottom(7), BunTop(8)

Ingredient (MonoBehaviour)
├── Type, CurrentColumn, CurrentRow, IsLanded, IsFalling
├── Initialize(type, column, sprite)
├── StartFalling(stepDuration) - Begin step-by-step fall
├── FastDrop() - Tap to instant-land with bonus
├── SwapToColumn() - Instant X swap during column swap
├── AnimateToCurrentPosition() - After stack swap
└── DestroyWithAnimation() - Blink + scale out

IngredientSpawner
├── SpawnWithPreview() - Coroutine: preview blink then spawn
├── GetSpawnType() - Random with bun rules + forced bun logic
├── TryTapPreview() - Early spawn with bonus
├── TryTapFallingIngredient() - Fast drop delegation
├── Test modes: TestBurgerColumn, TestDualColumn
├── Configurable: spawn interval, fall speed, active count
└── Double spawn at higher levels
```

### 3. Chef Controller

```
ChefController
├── _currentPosition (0, 1, or 2)
├── LeftColumnIndex / RightColumnIndex properties
├── MoveToPosition() / MoveLeft() / MoveRight()
├── SwapPlates() - Triggers GridManager swap
├── Position bubbles (semi-transparent circles)
│   ├── GenerateCircleSprite() - Runtime texture
│   └── Active position highlighted (0.45 vs 0.25 alpha)
└── DOTween movement with OutBack ease
```

### 4. Input System

```
TouchInputHandler (New Unity Input System)
├── EnhancedTouchSupport for mobile
├── Mouse.current + Keyboard.current for editor
├── Tap priority:
│   1. Spawn preview (early spawn)
│   2. Falling ingredient (fast drop)
│   3. Chef (swap plates)
│   4. Screen thirds (position movement)
├── Swipe detection (horizontal only)
└── Keyboard: A/D movement, Space swap
```

### 5. Audio System

```
AudioManager (Singleton)
├── Procedural generation via AudioClip.Create() + SetData()
├── SFX clips (all sine-wave based):
│   ├── Match: ascending 600-900Hz
│   ├── Burger Poor: descending E4-C4
│   ├── Burger Small: C5-G5
│   ├── Burger Medium: C5-E5-G5-C6
│   ├── Burger Large: C5-E5-G5-B5-C6 + harmonics
│   ├── Burger Mega: C5-E5-G5-C6-E6-G6 + 3 harmonics
│   ├── Burger Max: C5-G5-C6-E6-G6-C7 + 4 harmonics
│   ├── Level Up: A4-C#5-E5-A5-C#6
│   ├── Game Over: A4-F#4-Eb4-C4
│   ├── Squeeze: 800-500Hz (dedicated pitched AudioSource)
│   ├── Fast Drop: 1200-300Hz whoosh
│   └── Early Spawn: 500-1000Hz pop
└── Event-driven: subscribes to GridManager/DifficultyManager/GameManager

MusicManager (Singleton, DontDestroyOnLoad)
├── Resources.LoadAll<AudioClip>() from Music/ subfolders
├── Random track selection per category
├── Separate menu vs game tracks
└── Respects SaveDataManager sound toggle
```

### 6. Layout System

```
GameLayout (MonoBehaviour)
├── Generate9SliceSprite() - Runtime rounded-rect texture
│   └── DistanceInside() - SDF for corner rounding
├── CreatePanel(name, center, size) - SpriteRenderer with Sliced draw mode
├── Panels:
│   ├── GridPanel: (0, -1.9), size (5.2, 5.8)
│   ├── TopLeftPanel: (-1.35, 2.4), size (2.5, 2.6) - HUD
│   └── TopRightPanel: (1.35, 2.4), size (2.5, 2.6) - Challenge
└── Configurable: border color, fill color, border width, corner radius
```

### 7. Difficulty System

```
DifficultyManager
├── Level thresholds: [0, 3, 7, 12, 18, 25, 33, 42, 52, 64]
├── Lerps between initial and max values:
│   ├── Spawn interval: 3.0s → 1.2s
│   ├── Fall step: 0.3s → 0.1s
│   └── Ingredient count: 3 → 7
├── TestDualColumn override: starts at configured level
└── Event: OnLevelChanged(int level)
```

---

## Burger Scoring

| Tier | Ingredients | Bonus | Name | Sound |
|------|-------------|-------|------|-------|
| Poor | 0 | 5 | "Just Bread..." | Sad descending |
| Small | 1-2 | 20 | Random | 2-note |
| Medium | 3-4 | 50 | Random | 4-note arpeggio |
| Large | 5-6 | 100 | Random | 5-note + harmonics |
| Mega | 7-8 | 200 | Random | 6-note two-octave |
| Max | 9+ | 500 | "DOKTOR BURGUER!" | Fanfare |

Formula: `points = ingredientCount * 10 + bonus`

---

## Event Flow

```
IngredientSpawner.SpawnWithPreview() [coroutine]
    ↓ (blink preview)
Ingredient.Initialize() → StartFalling()
    ↓
GridManager.RegisterFallingIngredient()
    ↓
Ingredient.FallOneStep() [repeats]
    ↓
Ingredient.Land()
    ↓
GridManager.OnIngredientLanded()
    ├── [BunTop] → CheckAndProcessBurger() first
    │   └── BurgerCompressAnimation() [coroutine]
    │       ├── Pause spawning
    │       ├── Squeeze steps with rising pitch
    │       ├── Smack to bottom bun
    │       ├── Remove + collapse
    │       ├── OnBurgerCompleted / OnBurgerEffect
    │       └── Resume spawning
    ├── CheckAndProcessMatches()
    │   └── OnMatchEliminated / OnMatchEffect
    ├── OnIngredientPlaced
    └── [If overflow] OnGameOver
```

---

## Test Modes (GameManager toggles)

| Toggle | Behavior |
|--------|----------|
| TestSettings | Spawn 1 = BunBottom, Spawn 8 = BunTop |
| TestBurgerColumn | All on rightmost: BunBottom, Meat/Cheese alt, BunTop at MAX_ROWS |
| TestDualColumn | Left = Meat/Cheese alt, Right = BunBottom then BunTop, high start level |

---

## Scene Hierarchy

```
Game Scene
├── Main Camera (ortho size 5, pos 0,-0.46,-10)
├── Layout (GameLayout.cs → creates 3 panel sprites)
├── GameManager (GameObject)
│   ├── GameManager.cs
│   ├── GridManager.cs
│   ├── IngredientSpawner.cs
│   ├── TouchInputHandler.cs
│   ├── FeedbackManager.cs
│   └── DifficultyManager.cs
├── Chef (GameObject)
│   ├── SpriteRenderer
│   └── ChefController.cs (creates position bubbles)
├── Column_0..3 (auto-generated)
└── [Runtime created:]
    ├── HUD_Canvas (GameHUD)
    ├── AudioManager
    ├── MusicManager (DontDestroyOnLoad)
    ├── SaveDataManager (DontDestroyOnLoad)
    └── GemPackSpawner
```
