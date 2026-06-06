# Pre-Launch Checklist

Items that need to be addressed before a public / commercial release.
Separate from the code review (`Docs/Review/`) — these are
platform-readiness, monetization-safety, and launch-logistics concerns
rather than code-quality findings.

Each item: concern → impact → mitigation → status.

---

## Save layer (PlayerPrefs)

### Plain-text storage is exploitable on rooted Android
**Impact:** PlayerPrefs is backed by SharedPreferences XML at `/data/data/<package>/shared_prefs/<package>.v2.playerprefs.xml`. On a rooted Android device the file is readable and editable — players can manually inflate `gems`, `highScore`, etc.
**Mitigation:** encrypt sensitive fields (gems balance especially) OR server-side validate any operation that consumes them. Local-only is acceptable for cosmetic stats (high score, games played); financial values need protection.
**Status:** pending

### No cloud save — uninstall or device change loses all data
**Impact:** Player loses purchased gems, high score, settings on uninstall/reinstall or device switch. For a monetized game this is a refund-bait situation.
**Mitigation:** integrate Google Play Games Services Saved Games on Android (and Game Center on iOS if cross-platform). Sync the same five keys + any future ones.
**Status:** pending

### No save schema versioning
**Impact:** Today's `LoadData()` reads `KEY_*` with hardcoded defaults. If a future change renames a key, removes a field, or changes a type (e.g., `Gems: int` → `Gems: long`), old saves silently load defaults. No upgrade path.
**Mitigation:** add `KEY_SCHEMA_VERSION` field. On `LoadData`, if stored version < current, run a migration step that transforms old keys to new shape. Bump version on every breaking change.
**Status:** pending

### AndroidManifest `allowBackup` behavior is implicit
**Impact:** Android Auto Backup to Google Drive is controlled by `android:allowBackup` in AndroidManifest.xml. Default behavior depends on target SDK. If true, players' PlayerPrefs (including gem balance) back up to their Drive — survives uninstall, restored on reinstall. If false, total loss on uninstall. Either behavior should be a deliberate choice.
**Mitigation:** explicitly set `android:allowBackup` in AndroidManifest. If true, consider what fields to exclude via `android:fullBackupContent` rules.
**Status:** pending

### No IAP receipt validation
**Impact:** If gems become purchasable via IAP, the client receives a receipt that can be forged or replayed. Granting gems on client-side receipt acceptance is the standard fraud pattern.
**Mitigation:** validate receipts server-side OR use a service like Unity Gaming Services / RevenueCat that bundles validation. Don't grant gems on client receipt alone.
**Status:** pending (blocker for IAP launch)

---

## Monetization (Ads)

### Mock `AdManager` must be replaced with a real ad SDK
**Impact:** `AdManager` simulates ads with coroutine delays (`MockInterstitial`/`MockRewarded`) — no ad revenue, and the rewarded "continue after game over" grants for free. The show-on-demand API also doesn't match real SDKs, which require ads to be **pre-loaded** before showing; callers don't currently handle an "ad not ready" path.
**Mitigation:**
- Choose SDK / mediation: Unity LevelPlay (current default for Unity Ads), Google AdMob, or AppLovin MAX.
- Implement the async load/ready lifecycle: pre-load interstitial + rewarded, reload after each show, and make `IsAdAvailable()` reflect real load state (today it hardcodes `true`).
- Wire the full callback set (`OnInitialized`, `OnAdLoaded`/`OnAdFailedToLoad`, `OnAdShown`, `OnAdClosed`, `OnUserEarnedReward`). **Grant rewarded payout only from the reward callback — never on ad close.**
- Fix `Time.timeScale` handling: save the previous value and restore *that* on ad close. Current mock hardcodes restore to `1f`, which would un-pause the game if an ad ever shows while paused. Coordinate with whoever owns pause (see Review F-21).
- Per-platform setup: register the app in the network dashboard; obtain App ID + interstitial/rewarded ad-unit IDs for iOS and Android **separately**; add a test-mode toggle.
**Dependencies:** real fill typically requires a registered, near-launch app (often in store review). Also gated on the consent/privacy item below.
**Effort:** ~1–2 days of code once accounts + ad-unit IDs exist. The long pole is platform registration and on-device testing, not the code itself (ads can't be meaningfully tested in the editor).
**Recommended split:** harden the mock into a production-shaped interface *now* (load/ready state, reward-only-on-callback, `timeScale` save/restore) so the rest of the game already codes against the real contract; then the launch task is a body-swap of the mock methods for SDK calls rather than re-architecting callers. Interface hardening is deliberately **out of scope for the current code review** — tracked here, not as a review finding.
**Status:** pending (interface hardening optional-now; SDK wiring launch-gated)

### Ad consent & privacy prerequisites (GDPR / ATT)
**Impact:** EU GDPR consent and iOS App Tracking Transparency are required before ads will serve and for store compliance. Missing them → ads don't fill and/or the app violates platform policy.
**Mitigation:** integrate the SDK's consent management (e.g. Google/Unity UMP) for GDPR and trigger the ATT prompt on iOS. Depends on a published privacy policy (see below).
**Status:** pending (blocker for ad launch)

---

## (Future categories — fill as we surface them)

- Analytics & telemetry (event tracking, funnel metrics)
- Privacy policy + GDPR / consent flows
- Store listing assets (screenshots, descriptions, ASO keywords)
- Crash reporting (Firebase Crashlytics or similar)
- Performance budget / target device tier verification
