# Google Play listing — Dogtor Burguer

Copy-ready text for the Play Console (Store presence → Main store listing) plus the checklist of
assets and questionnaire answers. Character limits are Google's; the counts are in brackets.

## App details
- **App name** (30): `Dogtor Burger` [13] — the store name (decision 2026-09-13; matches the icon
  art). The package/namespace keep the `Burguer` spelling — they are permanent.
- **Package**: `com.proximacentaury.dogtorburguer` (already set in ProjectSettings — must match
  the Play Console app exactly; permanent once uploaded)
- **Developer name**: Oscar Torandell (personal account 7277266853178408719; the console shows the
  legal name — ProximaCentaury is only the package/company label)
- **Category**: Game → Arcade. Tags: Arcade, Casual, Puzzle, Single player, Offline
- **Contact email**: oscar.plk@gmail.com
- **Privacy policy URL**: HOSTED — https://otorandell.github.io/proximacentaury-legal/
  (GitHub Pages, repo `otorandell/proximacentaury-legal`, source `Docs/privacy-policy.html`;
  the same URL goes in the LevelPlay dashboard). To update it: edit `Docs/privacy-policy.md`,
  regenerate the html, push to that repo.

## Console status (walkthrough 2026-09-13, Claude driving Chrome)
App id in the console: `4972657403600944405` (developer `7277266853178408719`). Console UI is
in Spanish.

**Done** — main store listing (icon, feature graphic, screenshots, texts; "ready for review"),
content rating questionnaire, target audience 13+, privacy-policy URL, ads declaration, data
safety, app access, advertising-ID / government / financial / health declarations, category
(Arcade), app price = free, payments profile with a bank account, closed-test (Alpha) countries
= all 177, Play Games on PC form factor **disabled** (was accidentally on — it adds PC review
requirements), account group "Oscar Torandell" created (step 1 of the 15 % fee enrollment).

**Done 2026-09-13 (evening)** — `.aab` 1.0 (vc1) built via the menu item and **published to Internal
testing** (release "1.0 (1) - internal test"); the **5 one-time products are created and ACTIVE**
(purchase option id `buy` on each, backwards-compatible, all 173 regions); email list
**"Dogtor testers"** (oscar.plk@gmail.com) is both the internal-track tester list and the
**license-tester** list (test purchases are free — Settings → Licencia para testing, response
RESPOND_NORMALLY). Tester opt-in link (internal track — testers must be on the list first, then open it signed in
with that Google account → Become a tester → Install from Play):
**https://play.google.com/apps/internaltest/4701257598160915314**. Play sends no emails — share
the link yourself. Sideloaded tester APKs must be uninstalled first (different signing key).

**Pending, in order**
1. **Install + play the internal build on a phone** (opt-in link above → Play Store install).
   Verify: real store prices show in the shop, a test purchase grants gems, Remove Ads restores
   after reinstall, ads test suite (`LEVELPLAY_TEST_SUITE`). Note the console's warning that
   license testing is not compatible with *automatic integrity protection* (currently ON for
   the app) — if test purchases misbehave, toggle it off under Protegida con Play for the test.
2. **15 % service fee — step 2**: the "Review and enroll" banner appears on Settings → Developer
   account → Associated developer accounts once the group propagates; accept the terms there.
   Until then revenue is charged at 30 %.
3. **Closed test (Alpha)** for the production gate below: pick the tester list, upload (or promote)
   a build, get 12 testers to opt in.
4. **Play Games Services** (leaderboard) — section below.
5. **Every new upload**: `Tools → Dogtor → Build Android App Bundle (Play upload)` (it bumps
   `AndroidBundleVersionCode` itself since 2026-09-14 — commit the ProjectSettings change with the
   build; Play refuses a code it has already seen) → Internal testing → *Crear nueva versión* →
   drop the .aab → release name → next → *Guardar y publicar*. Testers get it as a normal update.

**Production gate (personal account created after Nov 2023)**: production access is only
granted after a **closed test with ≥ 12 opted-in testers running for 14 days**, then an
application form. Plan the tester group (friends/testers list) early — this is the longest
lead-time item of the launch.

**Licensing key** (Monetize → Monetization setup → Licensing): the Base64 RSA public key for
`UnityIapProvider` receipt validation (Unity IAP Receipt Validation Obfuscator). Copy it from
the console when generating the tangle — do not paste it into docs.

## Short description (80)
`Catch falling ingredients, stack burgers, chase Special Orders. Crazy and fun!` [78]

## Full description (4000)
Live in the console since 2026-09-13 (Oscar's wording + the stale-mechanics fixes):
```
Dogtor Burger is a fast, colorful arcade game about one very busy dog chef.

Ingredients rain down over four lanes. Slide the chef between the columns, catch what falls,
and stack it into burgers: a bottom bun starts one, a top bun finishes it — the taller the
burger, the bigger the score. Matching ingredients side by side clears them, so keep the
counter tidy or the stacks reach the ceiling and it's game over!

FEATURES
- Simple one-thumb controls: swipe or tap to move, tap the chef to flip.
- Special Orders: build the burger the customer wants for big multipliers.
- Power-ups delivered by Burger Fairies: Ketchup clears a column, Mustard sweeps ingredients
  off the whole board, the Skewer pins a burger together.
- Rising speed and new ingredients as the game progresses.
- Earn Stars as you play and spend them on cool rewards and new ingredients or cooks from
  all around the world.
- Offline, no account needed.

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
- **In-app products** — CREATED + ACTIVE 2026-09-13. Play treats the base price as
  tax-EXCLUSIVE and grosses it up per country with charm rounding, so the base is set BELOW the
  intended shelf price (decision 2026-09-13: classic .99 shelf tiers):

  | id | name | base (EUR, tax-excl) | ES/DE/FR shelf | US | UK |
  |---|---|---|---|---|---|
  | `gems_100` | 100 Gems | 0,82 | 0,99 € | $0.99 | £0.79 |
  | `gems_550` | 550 Gems | 4,12 | 4,99 € (DE 4,89) | $4.79 | £4.19 |
  | `gems_1200` | 1200 Gems | 8,25 | 9,99 € | $9.49 | £8.49 |
  | `gems_2600` | 2600 Gems | 16,52 | 19,99 € | $18.99 | £16.99 |
  | `remove_ads` | Remove Ads | 2,47 | 2,99 € (DE 2,89) | $2.89 | £2.49 |

  Other currencies are Play's auto-conversion (per-country override any time). The in-code
  `$` labels in `MonetizationConfig` remain placeholders — the shop shows the store's string.
  Play's product model has no consumable flag any more: the app decides by consuming (gems) or
  not (remove_ads); Unity IAP does that from the product type in its catalog.
  ⚠️ **Locale trap**: the console runs in Spanish — type prices with a COMMA (`0,99`); `0.99`
  is read as 99 €.

## Play Games Services (leaderboard — code scaffolded 2026-09-06)
- Grow → Play Games Services → Setup and management → Configuration: **create a new Play Games
  Services project** (name: Dogtor Burguer) and link this app.
- Credentials: add an **Android credential** for the app (it wants an OAuth client — the console
  walks you into Google Cloud: create the OAuth consent screen [External, app name, your email,
  no scopes beyond the defaults] and an Android OAuth client with the package id + the SHA-1 of
  the **Play App Signing key** [Play Console → Setup → App signing → App signing key certificate]).
- Leaderboards → Create: name "High Score", format Numeric, order Larger is better. Copy the
  **leaderboard ID** → paste into `SocialConfig.PLAY_GAMES_LEADERBOARD_ID`.
- Publish the PGS configuration (its own Review/Publish button) and add yourself under
  Testers while it's in review.
- Unity side (see CLAUDE.md Pending Manual Steps): import plugin v2 `com.google.play.games`,
  run its Android setup with the resources XML (Configuration → "Get resources"), add the
  `PLAY_GAMES` scripting define.

## Before submitting
- Hosted privacy policy URL entered (listing + LevelPlay dashboard).
- Play App Signing enrolled; upload an **.aab** (Unity: Build App Bundle), IL2CPP, ARM64.
- Internal testing track first: verify IAP (license testers get free test purchases) and ads
  with `LEVELPLAY_TEST_SUITE` (no consent prompt exists — v1 serves non-personalized ads to
  everyone by policy).
- `GameplayConfig.SETTINGS_LEVEL_CAP` flipped to `MAX_LEVEL`.
