# Dogtor Burguer! - Architecture Document

## Unity Project Structure

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   └── Game.unity          # Main game scene
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs   # Game loop, score, state machine
│   │   │   ├── GameState.cs     # Enum: Menu, Playing, Paused, GameOver
│   │   │   └── Constants.cs     # All game constants
│   │   │
│   │   ├── Grid/
│   │   │   ├── GridManager.cs   # Column management, match/burger detection
│   │   │   └── Column.cs        # Stack of ingredients per column
│   │   │
│   │   ├── Ingredients/
│   │   │   ├── Ingredient.cs       # Fall behavior, animations
│   │   │   ├── IngredientType.cs   # Enum + extension methods
│   │   │   └── IngredientSpawner.cs # Random spawning logic
│   │   │
│   │   ├── Chef/
│   │   │   └── ChefController.cs   # Movement, swap trigger
│   │   │
│   │   └── Input/
│   │       └── TouchInputHandler.cs # New Input System handler
│   │
│   ├── Prefabs/
│   │   └── Ingredients/
│   │       └── Ingredient.prefab   # Generic ingredient prefab
│   │
│   └── Sprites/
│       ├── Ingredients/            # All ingredient sprites
│       └── Chef/                   # Chef sprite
│
├── Plugins/
│   └── DOTween/
│
└── TextMesh Pro/
```

---

## Core Systems Architecture

### 1. Grid System

```
GridManager (Singleton)
├── Column[] _columns (4 columns)
├── List<Ingredient> _fallingIngredients
├── SwapColumns() - Full column swap logic
├── OnIngredientLanded() - Match/burger detection
└── Events: OnGameOver, OnMatchEliminated, OnBurgerCompleted

Column
├── List<Ingredient> _ingredients (stack)
├── GetNextLandingPosition()
├── TakeAllIngredients() / SetAllIngredients() - For swaps
├── CheckForMatch() - Compare top two
└── CollapseFromRow() - After burger completion
```

### 2. Ingredient System

```
IngredientType (Enum)
├── Regular: Meat, Cheese, Tomato, Onion, Pickle, Lettuce, Egg
├── Buns: BunBottom, BunTop
└── Extension: IsRegularIngredient(), IsBun()

Ingredient (MonoBehaviour)
├── Type, CurrentColumn, CurrentRow
├── IsFalling, IsLanded states
├── StartFalling() - Begin fall with step animation
├── SwapToColumn() - Instant X swap, continue falling
├── AnimateToCurrentPosition() - After stack swap
└── DestroyWithAnimation() - Scale + rotate out

IngredientSpawner
├── Spawn rate (configurable)
├── Random column selection
├── Sprite assignment per type
└── Spawns via Instantiate + Initialize()
```

### 3. Chef Controller

```
ChefController
├── _currentPosition (0, 1, or 2)
├── LeftColumnIndex / RightColumnIndex properties
├── MoveToPosition() / MoveLeft() / MoveRight()
├── SwapPlates() - Triggers GridManager.SwapColumns()
└── DOTween movement with OutBack ease
```

### 4. Input System

```
TouchInputHandler (New Unity Input System)
├── EnhancedTouchSupport for mobile
├── Mouse.current for editor/standalone
├── HandleMouseInput() / HandleTouchInput()
├── ProcessTap() - Determines chef move vs swap
└── Swipe detection for horizontal movement
```

### 5. Match & Burger Detection (in GridManager)

```
CheckAndProcessMatches(Column)
├── If top two ingredients match (same type, regular only)
├── Remove both from column
├── Trigger destruction animation
├── Award POINTS_MATCH
└── Recursively check for more matches

CheckAndProcessBurger(Column)
├── Scan from top for BunTop
├── Collect ingredients going down
├── Find BunBottom to complete
├── Calculate score + generate name
├── Destroy all burger ingredients
└── Collapse remaining ingredients
```

---

## Key Algorithms

### Ingredient Fall Logic
```
1. Spawn at column.GetSpawnPosition() (above screen)
2. Register in GridManager._fallingIngredients
3. FallOneStep():
   - Calculate current visual row from Y position
   - If at or below target row → Land()
   - Else → DOMove one cell down, repeat
4. Land():
   - Unregister from falling list
   - Add to column stack
   - Snap to exact position
   - Squash animation
   - Notify GridManager.OnIngredientLanded()
```

### Column Swap Logic
```
1. ChefController.SwapPlates() called
2. GridManager.SwapColumns(leftCol, rightCol):
   a. Calculate threshold Y (higher column top + 20% buffer)
   b. TakeAllIngredients() from both columns
   c. SetAllIngredients() swapped
   d. Animate stacked ingredients to new positions
   e. For each falling ingredient below threshold:
      - SwapToColumn() (instant X, resume fall)
   f. Check matches/burgers on both columns
```

### Burger Completion Logic
```
1. Scan column from top to bottom
2. Find BunTop → mark start index
3. Continue down, collecting ingredients
4. If BunBottom found → burger complete!
   - Score = (ingredientCount * 10) + size bonus
   - Generate funny name
   - Destroy all ingredients in range
   - Collapse items above
5. If another BunTop found → stop (incomplete)
```

---

## DOTween Usage

| Animation | Code | Ease |
|-----------|------|------|
| Ingredient fall step | `DOMove(pos, stepDuration)` | Linear |
| Landing squash | `DOPunchScale(0.2, -0.2)` | Default |
| Chef movement | `DOMove(pos, 0.15f)` | OutBack |
| Chef swap action | `DOPunchScale(0.2, 0.2)` | Default |
| Stack swap | `DOMove(pos, 0.2f)` | OutBack |
| Ingredient destroy | `DOScale(0) + DORotate(180)` | InBack |
| Collapse fall | `DOMove(pos, 0.15f)` | OutBounce |

---

## Event Flow

```
IngredientSpawner.SpawnIngredient()
    ↓
Ingredient.Initialize() → StartFalling()
    ↓
GridManager.RegisterFallingIngredient()
    ↓
Ingredient.FallOneStep() [repeats]
    ↓
Ingredient.Land()
    ↓
GridManager.UnregisterFallingIngredient()
    ↓
Column.AddIngredient()
    ↓
GridManager.OnIngredientLanded()
    ├── CheckAndProcessMatches()
    │   └── OnMatchEliminated event
    ├── CheckAndProcessBurger()
    │   └── OnBurgerCompleted event
    └── [If overflow] OnGameOver event
```

---

## Future Systems (Not Yet Implemented)

### Phase 3: Visual Feedback
- Particle effects on match
- Score popup floating text
- Screen shake

### Phase 4: Burger Polish
- Expanded name generator
- Celebration animations

### Phase 5: Game Flow
- DifficultyManager (speed/ingredient progression)
- Game over screen
- Restart functionality

### Phase 6+: Polish & Monetization
- UI system
- Audio system
- Ads/IAP
