# Google Play listing — Dogtor Burguer

Copy-ready text for the Play Console (Store presence → Main store listing) plus the checklist of
assets and questionnaire answers. Character limits are Google's; the counts are in brackets.

## App details
- **App name** (30): `Dogtor Burguer` [14]
- **Package**: `com.proximacentaury.dogtorburguer` (already set in ProjectSettings — must match
  the Play Console app exactly; permanent once uploaded)
- **Developer name**: ProximaCentaury
- **Category**: Game → Arcade. Tags: Arcade, Casual, Puzzle, Single player, Offline
- **Contact email**: oscar.plk@gmail.com
- **Privacy policy URL**: HOSTED — https://otorandell.github.io/proximacentaury-legal/
  (GitHub Pages, repo `otorandell/proximacentaury-legal`, source `Docs/privacy-policy.html`;
  the same URL goes in the LevelPlay dashboard). To update it: edit `Docs/privacy-policy.md`,
  regenerate the html, push to that repo.

## Short description (80)
`Catch falling ingredients, stack burgers, chase Special Orders. Quick arcade fun!` [79]

## Full description (4000)
```
Dogtor Burguer is a fast, colorful arcade game about one very busy dog chef.

Ingredients rain down over four lanes. Slide the Dogtor between the columns, catch what falls,
and stack it into burgers: a bottom bun starts one, a top bun finishes it — the taller the
burger, the bigger the score. Matching ingredients side by side clears them, so keep the
counter tidy or the stacks reach the ceiling and it's game over!

FEATURES
- Simple one-thumb controls: swipe to move, tap to flip. Drag or Tap mode — your choice.
- Special Orders: build the burger the customer wants for big multipliers.
- Power-ups delivered by Burger Fairies: Ketchup clears a column, Mustard sweeps an ingredient
  off the whole board, the Skewer pins a burger together.
- 20 levels of rising speed and new ingredients — and a secret kill screen for the brave.
- Earn Stars as you play and spend them on skins: new Dogtors and gourmet ingredient sets.
- Offline, no account needed. Short rounds, made for a quick break.

Music by SketchyLogic, BossLevelVGM, Martin Nilsson, Alex McCulloch and Spring Spring
(OpenGameArt).
```
The last line is a **license requirement** (CC-BY tracks — see `Docs/music-attribution.md`); keep
it in every store listing.

## Graphics (Play Console requirements)
- **App icon**: 512×512 PNG, no alpha (Unity's icon settings feed the APK; upload the same art).
- **Feature graphic**: 1024×500 JPG/PNG — the logo over the menu illustration works.
- **Phone screenshots**: 2–8, 16:9 or 9:16, min 320 px, max 3840 px. Suggested set: gameplay
  mid-stack, a Special Order match, the Burger Fairy, the Shop, the main menu.
- Optional: 7" and 10" tablet screenshots (same shots), a 30s–2min promo video (YouTube URL).

## Questionnaires (answers that match this build)
- **Content rating (IARC)**: no violence, no sexual content, no profanity, no gambling; "Users
  can purchase digital goods" = Yes; "Displays ads" = Yes. Expect Everyone / PEGI 3.
- **Target audience & content**: target age groups **13+ and up** (the privacy policy declares
  the game is not directed at children; do NOT tick under-13 unless you also adopt the Families
  policy and child-directed ad settings).
- **Ads**: Yes — "This app contains ads".
- **Data safety**: no data collected by the app itself; third-party SDKs (LevelPlay) collect
  Device or other IDs + Advertising data for Advertising; purchases handled by Google Play. Data
  is not encrypted in transit by us (we send none); users can request deletion via the ad
  provider. Mirror the wording of the privacy policy.
- **App access**: all functionality available without special access.
- **Government apps / News / COVID**: No.
- **In-app products**: create the products from `MonetizationConfig` with the exact IDs
  (`gems_100`, `gems_550`, `gems_1200`, `gems_2600`, `remove_ads`) and the intended prices
  ($0.99 / $4.99 / $9.99 / $19.99 / $2.99); activate them before the first test track upload.

## Before submitting
- Hosted privacy policy URL entered (listing + LevelPlay dashboard).
- Play App Signing enrolled; upload an **.aab** (Unity: Build App Bundle), IL2CPP, ARM64.
- Internal testing track first: verify IAP (license testers get free test purchases) and ads
  with `LEVELPLAY_TEST_SUITE` (no consent prompt exists — v1 serves non-personalized ads to
  everyone by policy).
- `GameplayConfig.SETTINGS_LEVEL_CAP` flipped to `MAX_LEVEL`.
