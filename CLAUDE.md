# Dogtor Burguer

2D mobile arcade game in Unity 6. Player controls a chef catching falling ingredients between columns to build burgers. Match-3 style mechanics with burger challenge objectives.

## Tech Stack
- Unity 6, URP, New Input System
- DOTween for animations
- TextMeshPro for text rendering
- System.Random via shared `Rng` utility (NOT UnityEngine.Random)
- C# 10+, namespace: `DogtorBurguer`

## Project Structure
```
Assets/_Project/Scripts/
  Chef/          ChefController
  Core/          GameManager, GameState, Constants, DifficultyManager,
                 SaveDataManager, ControlMode, FeedbackManager, Rng, SceneLoader
  Grid/          GridManager, Column, MatchDetector, BurgerAnimator
  Ingredients/   Ingredient, IngredientSpawner, IngredientType
  Input/         TouchInputHandler
  UI/            MainMenuUI, GameHUD, GameOverPanel, SettingsPanel, ShopPanel,
                 BurgerChallenge, BurgerPopup, FloatingText, ScorePopup,
                 Background, GameLayout
  Audio/         AudioManager, MusicManager
  Monetization/  AdManager, GemPack, GemPackSpawner
```

## Singletons
These classes use the singleton pattern. Initialization order matters:
1. **SaveDataManager** -- player data persistence (PlayerPrefs)
2. **MusicManager** -- background music, DontDestroyOnLoad
3. **AdManager** -- ad integration (currently mock)
4. **GameManager** -- central game state, creates missing managers via EnsureComponent
5. **GridManager** -- column/grid state
6. **AudioManager** -- procedural SFX generation
7. **BurgerChallenge** -- challenge UI and tracking

## Event System
Key events for cross-system communication:
- `GameManager.OnStateChanged(GameState)` -- game flow changes
- `GameManager.OnScoreChanged(int)` -- score updates
- `GridManager.OnGameOver` -- grid overflow
- `GridManager.OnMatchEliminated`, `OnBurgerCompleted` -- gameplay events
- `GridManager.OnMatchEffect`, `OnBurgerEffect` -- audio/visual triggers
- `GridManager.OnBurgerWithIngredients(List)` -- challenge matching
- `GridManager.OnIngredientPlaced` -- post-landing
- `DifficultyManager.OnLevelChanged(int)` -- difficulty progression
- `SaveDataManager.OnGemsChanged(int)` -- currency updates

## Core Systems

### Wave-Based Spawning (IngredientSpawner)
- Waves of 2 ingredients (triple chance starting at level 8, up to 35% at level 20)
- Next wave waits until all current wave ingredients land
- Pre-rolls next wave and shows blinking previews once current wave clears top cell
- Tap preview to immediately spawn that ingredient
- Forced bun spawn threshold: `activeCount * 1.5`
- Unified ingredient pool: bun is one extra slot in the random range

### Bun Type Selection (GetBunType)
- No bottom bun on grid: always BunBottom
- Otherwise: `topChance = 0.5 + bottomCount * 0.08` (capped 0.8)

### Controls (TouchInputHandler)
Two modes, configurable in Settings (saved via PlayerPrefs):
- **Drag**: Swipe = move chef, Tap = swap plates, Tap falling = fast-drop, Tap preview = spawn
- **Tap**: Tap near chef = swap, Tap left/right of chef = move, Swipe = move, Tap falling = fast-drop, Tap preview = spawn

### Difficulty (DifficultyManager)
- 20 levels scaling fall speed and active ingredient count
- Level 1: 3 ingredients, 0.5s fall step
- Level 20: 7 ingredients, faster fall
- Thresholds defined as static array in DifficultyManager

### Burger Challenge (BurgerChallenge)
- Target burger recipe displayed with ingredient sprites and progress meter
- Match exact ingredients (order irrelevant) for 3x multiplier
- Global multiplier: `1 + (level - 1) * 5`
- Level up requires `level + 1` matches
- Generates unique combinations (no repeats until pool exhausted)

### Scoring
- Match: 10 pts per matched pair
- Burger bonuses: 5 (poor) to 500 (9+ ingredients)
- Per ingredient: 10 pts
- Challenge multipliers stack with global multiplier

### Grid
- 4 columns, 13 max rows
- Cell: 1.4w x 0.4h visual height (60% overlap)
- Grid origin: (-2.1, -4.2)
- Chef has 3 positions (between the 4 columns)

### Audio
- **AudioManager**: All SFX procedurally generated (sin waves, envelopes, harmonics). No audio asset files
- **MusicManager**: Loads tracks from Resources/Music/. Random selection per scene

### Monetization
- Gems currency (earned via ads, gem packs, IAP)
- Continue after game over: 50 gems or watch ad
- Interstitial ads every 3 games
- Gem pack drops during gameplay (8% chance every 10s)
- AdManager is currently mock (simulated delays). IAP buttons grant gems for testing

## Randomness
All randomness uses `Rng` static class, never `UnityEngine.Random`:
```csharp
Rng.Range(0, max)      // int, exclusive max
Rng.Range(0f, 1f)      // float
Rng.Value              // float 0-1
```

## Known Issues

### Text Outline Rendering (UNRESOLVED)
Setting `tmp.outlineWidth`/`tmp.outlineColor` after `AddComponent<TextMeshPro>()` doesn't reliably render outlines. Shader keyword `OUTLINE_ON` isn't enabled and material may not be initialized. Current workaround: black text color for readability.

**Fix when ready:**
1. Enable `ShaderUtilities.Keyword_Outline` ("OUTLINE_ON") on the material
2. Wait one frame for TMP to initialize before modifying `fontMaterial`
3. Call `tmp.UpdateMeshPadding()` and `tmp.ForceMeshUpdate()`

### UI Text Color Convention
- Main Menu + In-game HUD: black text
- Buttons/popups/panels: white with outline (when outline fix is applied)
- World-space popups: white with outline

## Architecture Refactoring Roadmap
Active work to improve AI maintainability:

1. ~~**Centralize config values**~~ DONE -- `GameplayConfig.cs`, `UIStyles.cs`, `AnimConfig.cs` alongside `Constants.cs`
2. ~~**Split GridManager**~~ DONE -- `MatchDetector` (static, match/burger detection), `BurgerAnimator` (compress animation, scoring, naming), `GridManager` (column state, events, orchestration)
3. **Split IngredientSpawner** (~457 lines) -- Extract `WavePreviewManager`, replace boolean flags with explicit enum state machine
4. **Restructure UI code** -- Separate layout/construction from behavior/logic in all UI scripts, reference UIStyles for visual properties

## Pending Features
- Text outline shader fix
- Leaderboard integration (button exists, logs "Coming Soon")
- IAP integration (buttons exist, currently grant gems for testing)
- Ad SDK integration (AdManager is placeholder)
