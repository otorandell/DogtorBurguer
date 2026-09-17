# Session 2026-09-13 (evening) — Play Console setup (Claude driving Chrome)

Ran in parallel with the run-resume session (`session-2026-09-13.md`), which committed the
first `BuildMenu.cs`; this session's password-parse + size-log fixes are on top.

Continuation of the 2026-09-08 pending list: "Play Console (Oscar): first .aab upload → 5 IAP
products". Everything console-side was done live in Oscar's Chrome via the extension; the
authoritative state lives in `Docs/play-store-listing.md` → **Console status**.

## Landed
- **BuildMenu.cs** (`Scripts/Editor/`): Tools → Dogtor → Build Android App Bundle (Play upload).
  Refuses a Test-Build-on menu scene (offers to untick + save), reads the upload-key password
  from `Keys/KEYSTORE-INFO.txt` (first token after `Password:` — the line carries a note),
  builds `Builds/Android/DogtorBurguer-<ver>-vc<code>.aab`. First run failed on the password
  parse (took the whole line); fixed and verified with keytool.
- **Internal testing release 1.0 (vc1)** published (144 MB .aab, min API 25, target 36, arm64).
- **5 one-time products ACTIVE** with .99 shelf tiers (base prices tax-exclusive: 0,82 / 4,12 /
  8,25 / 16,52 / 2,47 EUR). Locale trap: `0.99` typed into the Spanish console = 99 €.
- **Tester + license-tester list** "Dogtor testers" (Oscar's Gmail) wired to the internal track.
- Listing copy fixed (name → Dogtor Burger, Drag-mode line gone, Mustard two-types, typo).
- Play Games on PC form factor disabled (was accidentally on); closed-test countries = all 177;
  account group created (15 % fee step 1 — the "Review and enroll" banner is still pending).
- Docs: listing doc rewritten around the real console state; build doc cross-links the menu item.

## Findings worth remembering
- **Production gate**: personal Play accounts need a 14-day closed test with ≥ 12 opted-in
  testers before production access — the longest lead-time item left.
- Play's product model has no consumable flag; the app's consume/not-consume decides.
- The console's "Activar" on the purchase-option EDIT page discards unsaved price edits —
  save first (Guardar), then activate from the product page.
- License testing warns it is incompatible with automatic integrity protection (ON) — watch
  for it during the device purchase test.

## Next
On-device pass from the internal track (prices, test purchase, restore, ads test suite) →
accept the 15 % fee terms when the banner shows → closed test recruiting → PGS leaderboard.
Uncommitted at session end: BuildMenu.cs (two fixes), CLAUDE.md, `Docs/play-store-listing.md`,
`Docs/build-and-share.md`, this file. Closed-test recruits go in the git-ignored
`Docs/beta-testers.txt` (added by the parallel session).
