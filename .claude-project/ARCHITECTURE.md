# Dogtor Burguer! - Architecture Document

## Unity Project Structure

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Game.unity
│   │   └── GameOver.unity
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── GameState.cs
│   │   │   └── Constants.cs
│   │   │
│   │   ├── Grid/
│   │   │   ├── GridManager.cs
│   │   │   ├── Column.cs
│   │   │   └── Cell.cs
│   │   │
│   │   ├── Ingredients/
│   │   │   ├── Ingredient.cs
│   │   │   ├── IngredientType.cs
│   │   │   ├── IngredientSpawner.cs
│   │   │   └── IngredientStack.cs
│   │   │
│   │   ├── Chef/
│   │   │   ├── ChefController.cs
│   │   │   └── PlateSwapper.cs
│   │   │
│   │   ├── Burger/
│   │   │   ├── BurgerDetector.cs
│   │   │   ├── BurgerNameGenerator.cs
│   │   │   └── BurgerData.cs
│   │   │
│   │   ├── Scoring/
│   │   │   ├── ScoreManager.cs
│   │   │   └── ComboSystem.cs
│   │   │
│   │   ├── UI/
│   │   │   ├── ScoreDisplay.cs
│   │   │   ├── GameOverPanel.cs
│   │   │   └── BurgerNamePopup.cs
│   │   │
│   │   ├── Input/
│   │   │   └── TouchInputHandler.cs
│   │   │
│   │   └── Audio/
│   │       └── AudioManager.cs
│   │
│   ├── Prefabs/
│   │   ├── Ingredients/
│   │   │   ├── Ingredient_Meat.prefab
│   │   │   ├── Ingredient_Cheese.prefab
│   │   │   ├── Ingredient_Tomato.prefab
│   │   │   ├── Ingredient_Onion.prefab
│   │   │   ├── Ingredient_Pickle.prefab
│   │   │   ├── Ingredient_Lettuce.prefab
│   │   │   ├── Ingredient_Egg.prefab
│   │   │   ├── Bun_Top.prefab
│   │   │   └── Bun_Bottom.prefab
│   │   │
│   │   ├── Chef/
│   │   │   └── Dogtor.prefab
│   │   │
│   │   └── UI/
│   │       └── BurgerNamePopup.prefab
│   │
│   ├── Sprites/
│   │   ├── Ingredients/
│   │   ├── Chef/
│   │   ├── Background/
│   │   └── UI/
│   │
│   ├── Audio/
│   │   ├── SFX/
│   │   └── Music/
│   │
│   ├── Animations/
│   │   ├── Ingredients/
│   │   └── Chef/
│   │
│   └── ScriptableObjects/
│       ├── IngredientData/
│       └── GameSettings.asset
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
├── Manages 4 Columns
├── Handles ingredient placement
└── Coordinates with ChefController for swaps

Column
├── Stack of Ingredients
├── Max height tracking
└── Overflow detection (game over condition)

Cell
├── Visual position in world
└── Reference to ingredient (if any)
```

### 2. Ingredient System

```
IngredientType (Enum)
├── Meat, Cheese, Tomato, Onion, Pickle, Lettuce, Egg
├── BunBottom, BunTop

Ingredient (MonoBehaviour)
├── Type
├── Current Column/Row position
├── Fall animation (DOTween)
├── Match detection
└── Destruction animation

IngredientSpawner
├── Spawn rate (increases over time)
├── Available ingredient pool (starts with 4, grows to 7)
├── Random column selection
└── Bun spawn logic (periodic)
```

### 3. Chef Controller

```
ChefController
├── Current position (0, 1, or 2 = between columns 0-1, 1-2, 2-3)
├── Movement via touch
├── Plate swap action (touch on chef)
└── Animation states

PlateSwapper
├── Swap two adjacent column tops
├── Animation during swap
└── Chain reaction detection after swap
```

### 4. Match & Burger Detection

```
MatchDetector
├── After each ingredient lands
├── Check if top 2 of column are same type
├── If match → destroy both, award points

BurgerDetector
├── Triggered after ingredient lands
├── Scan column from top to bottom
├── Find BunTop → collect ingredients → find BunBottom
├── If complete → create burger, award points, show name
```

### 5. Game Flow

```
GameState (Enum)
├── Menu
├── Playing
├── Paused
├── GameOver

GameManager
├── State machine
├── Difficulty progression
├── Game over detection
└── Restart/Continue logic
```

---

## Key Algorithms

### Ingredient Fall Logic
```
1. Spawn ingredient at column top (off-screen)
2. DOTween move to next cell position
3. Wait for step interval
4. Check if cell below is empty
   - Yes: continue falling
   - No: land on current cell
5. On land:
   - Add to column stack
   - Check for matches
   - Check for burger completion
   - Check for overflow (game over)
```

### Plate Swap Logic
```
1. Get top ingredients of columns at chef position
2. Swap their column references
3. Animate swap (DOTween)
4. Update column stacks
5. Trigger match/burger detection on both columns
```

### Burger Completion Logic
```
1. Iterate column from top
2. If BunTop found:
   - Start collecting ingredients below
   - Continue until BunBottom or empty/another BunTop
3. If BunBottom found:
   - Burger complete!
   - Calculate score (10 * ingredient count + bonus)
   - Generate funny name
   - Destroy all involved items
   - Items above fall down
```

---

## DOTween Usage

- **Ingredient Fall**: `transform.DOMove(targetPos, stepDuration).SetEase(Ease.OutBounce)`
- **Plate Swap**: `DOTween.Sequence()` with parallel moves
- **Ingredient Destroy**: `transform.DOScale(0, 0.2f)` + particle effect
- **Burger Complete**: Screen shake + scale pop
- **Score Popup**: `DOMove` up + `DOFade` out
- **Chef Movement**: Instant or short tween between positions
