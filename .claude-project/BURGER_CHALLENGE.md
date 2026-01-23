# Burger Challenge System - Feature Spec

## Overview

The top-right panel displays a **target burger** that the player must replicate. Completing the challenge burger grants a score multiplier and fills a progress meter. Leveling up the challenge adds a permanent score multiplier to all burgers.

---

## Visual Layout (Top-Right Panel)

The panel (center: 1.35, 2.4 / size: 2.5, 2.6) contains:

```
+------------------+
|   [Burger Name]  |
|                  |
|   BunTop         |
|   Ingredient 3   |
|   Ingredient 2   |  ← Tight spacing (squeeze-like, 15-20% of CELL_VISUAL_HEIGHT)
|   Ingredient 1   |
|   BunBottom      |
|                  |
|  [===---] Lv.1   |  ← Vertical meter + level indicator
+------------------+
```

### Burger Display
- Shows a miniature burger with actual ingredient sprites
- Uses tight spacing similar to the squeeze animation (15-20% of CELL_VISUAL_HEIGHT)
- Burger has: BunBottom + N ingredients + BunTop
- Randomly generated each time a new challenge starts

### Burger Name
- Displayed above or below the burger visual
- Generated using the same name system (size-aware prefixes + adj + noun)

### Progress Meter
- Vertical bar on the side or horizontal bar at the bottom
- Shows progress toward next level-up
- "Lv.N" text displayed just below/beside the meter

---

## Challenge Mechanics

### Matching
- A completed burger **matches** the challenge if it contains the **same ingredients** (types and counts), regardless of order
- Example: Challenge shows [Meat, Cheese, Tomato]. Player makes [Tomato, Meat, Cheese] → MATCH
- Only the ingredient composition matters, not the stacking order

### Scoring on Match
- Challenge burger scores **x3** of its normal value
- The floating score text shows the multiplied amount
- The meter fills by 1 unit

### Leveling Up
- Level 1 requires **1** challenge burger to level up
- Level 2 requires **2** challenge burgers
- Level N requires **N** challenge burgers
- On level-up:
  - A permanent **x5 multiplier** is added to ALL burger scores
  - A new (harder) challenge burger is generated
  - The meter resets

### Global Multiplier
- All burger scores are multiplied by: `1 + (challengeLevel - 1) * 5`
  - Level 1: x1 (no bonus yet)
  - Level 2: x6
  - Level 3: x11
  - Level 4: x16
  - etc.
- This applies to ALL burgers, not just challenge matches
- Challenge matches still get the additional x3 on top

### Final Score Formula
```
baseScore = ingredientCount * POINTS_PER_INGREDIENT + tierBonus
globalMultiplier = 1 + (challengeLevel - 1) * 5
challengeMultiplier = isChallengeMatch ? 3 : 1
finalScore = baseScore * globalMultiplier * challengeMultiplier
```

---

## Challenge Burger Generation

### Ingredient Count by Challenge Level
| Challenge Level | Ingredients in Target |
|-----------------|----------------------|
| 1-2 | 1 |
| 3-4 | 2 |
| 5-6 | 3 |
| 7-8 | 4 |
| 9+ | 5 |

Formula: `ingredientCount = min(5, (challengeLevel + 1) / 2)`

### Generation Rules
- Always has BunBottom + BunTop
- Ingredients are randomly selected from the **currently active ingredient pool** (respects DifficultyManager)
- Duplicates allowed (e.g., [Meat, Meat, Cheese] is valid)
- New challenge generated on:
  - Game start
  - Level-up

---

## Implementation Plan

### New File: `Assets/_Project/Scripts/UI/BurgerChallenge.cs`

MonoBehaviour placed on a child of the Layout or on its own GameObject.

**Responsibilities:**
- Generate random target burger (list of IngredientType)
- Display miniature burger using SpriteRenderers (tight spacing)
- Display burger name (TextMeshPro, world-space)
- Display progress meter (SpriteRenderer bar fill)
- Display level indicator (TextMeshPro)
- Subscribe to `GridManager.OnBurgerCompleted` or `OnBurgerEffect` to check matches
- Track challenge level, progress, and apply multipliers

**Key Fields:**
```csharp
[SerializeField] private Vector2 _panelCenter = new Vector2(1.35f, 2.4f);
[SerializeField] private float _ingredientSpacing = 0.08f; // tight like squeeze
[SerializeField] private float _ingredientScale = 0.6f;    // smaller than grid

private List<IngredientType> _targetIngredients;
private int _challengeLevel = 1;
private int _challengeProgress;  // burgers matched this level
private string _challengeName;
```

**Key Methods:**
- `GenerateNewChallenge()` - Pick random ingredients, create visual
- `CheckBurgerMatch(List<IngredientType> completedIngredients)` - Compare sets
- `GetScoreMultiplier()` - Returns global multiplier (1 + (level-1)*5)
- `GetChallengeBonus()` - Returns 3 if match, 1 otherwise
- `UpdateMeter()` - Fill progress bar
- `LevelUp()` - Increment level, reset progress, generate new challenge

### Modify: `GridManager.cs`
- Pass the list of ingredient types in the burger to `OnBurgerEffect` or a new event
- Or: BurgerChallenge reads the burger parts from the animation

### Modify: `GridManager.cs` (scoring)
- Apply `BurgerChallenge.GetScoreMultiplier() * BurgerChallenge.GetChallengeBonus()` to burger points
- OR: BurgerChallenge modifies the score via `GameManager.AddExtraScore()`

### Modify: `IngredientSpawner.cs`
- Expose `GetSpriteForType(IngredientType)` so BurgerChallenge can get sprites for display

---

## UI Details

### Meter Bar
- Vertical bar (or horizontal) showing progress/required
- Fill color: green or gold
- Background: dark/transparent
- Segments or smooth fill
- "Lv.N" text in white below the bar

### On Challenge Match
- Flash the panel border (gold highlight)
- Play a special sound (can reuse level-up or create new)
- Show "x3!" floating text near the panel

### On Level Up
- Brief celebration animation on the panel
- New burger slides in / old one fades out
- Level number animates up

---

## Edge Cases

- If player has not unlocked enough ingredient types for the challenge level's target count, use only available types
- "Just Bread" (0 ingredients) never counts as a challenge match
- Poor burgers (0 ingredients) cannot be challenge targets
- If the challenge has [Meat, Meat] the player needs exactly 2 Meats (not just 1)
