# Shop — design + implementation report (2026-07-05)

The Shop shipped this session: a full-screen store selling **skins** (buy + equip in place),
**consumables** (now persistent across runs), **currency bundles** (stars and gems), and
**Remove Ads** ($2.99). This doc is the "what and why" so you can change anything you disagree
with — every decision lists its knob.

## Decisions you made before leaving
- **Full-screen overlay, not a new scene** — `ShopScreen` builds its own canvas on demand,
  openable from the menu and mid-run (pauses). No scene load, no singleton juggling.
- **Skins equip from the shop** — the shop is the wardrobe. Buying auto-equips; tapping an owned
  skin re-equips. No separate Skins screen.
- **All consumables persist** — one inventory: fairy drops and purchases feed the same
  SaveDataManager-backed stock; unused items carry to the next run.
- Star **earning** deliberately left open ("not important yet") — see Open items.

## Decisions I made for you (and why)

### Economy shape (research-driven — patterns from Subway Surfers / Brawl Stars / Archero / Crossy Road)
I researched how casual freemium shops are laid out and priced; the headline conventions applied:
- **One-directional exchange**: gems buy stars, never the reverse. Star packs are the gem sink;
  stars are the skin/consumable currency. Nothing is dual-priced.
- **Ladders improve monotonically** (better value per unit at higher tiers), with
  **MOST POPULAR** / **BEST VALUE** badges:
  - Gem packs (IAP, mock): 100/$0.99 · 550/$4.99 ★pop · 1200/$9.99 · 2600/$19.99 ★best
  - Star packs (gems): 200/40 · 550/100 · 1200/200 ★best (~5 stars/gem baseline)
  - Consumables (stars, same ladder all 3 types): x1 = 150 · x3 = 400 (~11% off)
  - Skins (stars): Deluxe Patty 500 · Happy Dogtor 800 · Dogtor Deluxe 1000
    (anchor: 100 gems ≈ $0.99 ≈ ~500 stars, so a skin ≈ $1–2 of grind-or-pay)
- **Remove Ads $2.99, bundled** with +100 gems (the "+sweetener" framing converts far better than
  a bare toggle in top-100 casual games). It kills **interstitials only** — rewarded ads
  (continue, free gems) stay, which is what players expect. The banner disappears once bought.
- **Confirm dialog only for gem spends** (star packs). Soft-currency spends and equips are
  instant — confirming everything is friction, confirming nothing is a dark pattern.
- All numbers in `MonetizationConfig` (packs, ladders, remove-ads) and per-skin `_starCost` on
  the Skin assets — retune without touching logic.

### Layout (your "scroll vertically, sections scroll horizontally" instinct = the industry default)
Single vertical page, fixed header (SHOP + star/gem pills + close X). Section order follows the
standard merchandising stack — offer banner on top, currency near the bottom:
1. **REMOVE ADS** banner (the one "special offer")
2. **DOGTOR SKINS** — horizontal row (default + Happy Dogtor + Dogtor Deluxe)
3. **INGREDIENT SKINS** — horizontal row (Classic Patty + Deluxe Patty; grows as art lands)
4. **POWER-UPS** — 3 consumable cards with owned count + x1/x3 buy buttons
5. **GET STARS** — gem-priced bars (confirm-gated)
6. **GET GEMS** — free watch-ad rung first, then the 4 IAP bars
Horizontal rows use `ShopRowScroll` (a ScrollRect subclass): horizontal drags scroll the row,
vertical drags pass through to the page — the stock nested-ScrollRect deadlock solved.

### Architecture (mirrors the house style)
- `Scripts/Shop/`: **ShopScreen** (frame, header, confirm dialog, open/close + pause glue) ·
  **ShopSections** (page composition) · **ShopWidgets** (UGUI builders) · **ShopSkinCell**
  (3-state cell: EQUIPPED / tap-to-equip / priced) · **ShopService** (atomic purchase rules,
  UI-free) · **ShopCatalog** (skin grouping) · **ShopRowScroll**.
- **Persistence** went into `SaveDataManager`: `Stars` (+`OnStarsChanged`), `AdsRemoved`, owned
  skin ids (CSV), equipped skin per slot, consumable counts (+`OnConsumablesChanged`).
  `ConsumableInventory` became a thin facade over it (public API unchanged — zero consumer edits).
- **Theme** now resolves the equipped skin per slot (persisted, applied lazily so early access
  can't beat the save layer) and exposes `Equip`/`IsEquipped`/`AllSkins`.
- The old menu `ShopPanel` is **deleted**; menu Shop button, in-game top-bar shop button, and the
  consumable slots' green **plus box** all open the same `ShopScreen` (in-game = paused; the
  input handler already ignores gameplay taps while paused).
- HUD/menu star readouts are live now (the top-bar star placeholder is a real counter).

### Skin assets + import fixes
Authored the three spare-art skins as `Resources/Skins/*.asset` (star-priced, non-default). Two
import metas needed fixing to be usable at all: `meat_alt` was PPU 100 (would render ~9 world
units wide — normalized to 760 ≈ the 1.2-unit ingredient standard) and `chef_alt` is a 3240px
source at max texture size 2048 (the known "sprite rect out of bounds → meta wipe" trap — bumped
to 4096). Chef alts have no flipped-facing art yet, so the secondary sprite reuses the front
(shows mirrored after a flip — placeholder until flipped art exists).
`UnlockMethod` gained `Stars` (append-only), `Skin` gained `_starCost`.

## Star earning (added later the same day — closes the economy loop)
Two faucets, both routed through `GameManager.AwardStars` (grants immediately + tracks
`StarsEarnedThisRun` for the game-over panel):
- **Per completed Special Order** (in `BurgerChallenge`): `3 + 2·(challengeLevel−1)` stars,
  awarded live with a gold "+N STARS" world popup under the xN multiplier text. Scaling with
  challenge level makes the mult meter fantasy pay out in currency, not just score.
- **End-of-run score payout** (in `GameManager.HandleGameOver`): 1★ per 500 score. A continue
  keeps the run going, so a second game over pays only the score slice not already paid.
- Game-over panel shows **"+N Stars earned!"** (gold, under Level).
- Expected yields: casual run ~10–20★, a good run reaching challenge level 4 ~60–70★ →
  consumable (150★) every few runs, first skin (500★) in ~10–25 runs. Knobs:
  `MonetizationConfig.STARS_PER_ORDER_BASE / STARS_PER_ORDER_PER_LEVEL / STAR_SCORE_DIVISOR`.

## Open items (deliberate, not forgotten)
- **Economy balance pass** — earn rates vs prices are reasoned guesses; tune from real runs
  (all knobs in `MonetizationConfig`).
- **Unity verify pass** — I can't compile/run Unity. Open the editor, check console, then:
  menu → Shop (buy/equip a skin, buy consumables, star pack confirm, remove-ads hides + no
  interstitials), in-game shop button pauses/resumes, plus-box deep link, stock persists across
  a restart, equipped skin shows after Play.
- **Shop look** is placeholder (flat cards + authored pills/icons); `UIStyles.SHOP_*` are eyeball
  defaults. Purchase celebration is minimal (pill punch + placeholder collect sound).
- **Real IAP** still pending (stubs grant instantly); Remove Ads will need Restore Purchases on iOS.
- `Assets/_Project/Sprites/ChatGPT Image 12 jun 2026, 12_46_50.png` left untracked — unidentified
  raw drop; move to RawArt or delete.

## Also this session
The parked polish pass (mult meter + text fill/border fix + kit icons) was committed separately
first — see `session-2026-07-05.md` for what's left of it (all one-line tuning knobs).
