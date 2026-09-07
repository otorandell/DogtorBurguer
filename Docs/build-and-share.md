# Building a tester APK and sharing it (2026-09-06)

How to hand the game to a colleague without the store: an Android **.apk**, signed either way,
sent as a file link. Assumes the tester is on Android — iPhone sideloading needs a Mac, Xcode
and TestFlight, which is a different guide.

## One-time: Android Build Support

The project's editor (**6000.3.23f1**) was installed with the Windows player only.
Unity Hub → Installs → 6000.3.23f1 → ⋮ → **Add modules** → tick **Android Build Support** plus
its two children (**OpenJDK**, **Android SDK & NDK Tools**). Restart the editor.

## The Test Build switch

`MainMenuUI` (menu scene, Testing header) has a **Test Build** checkbox. Tick it before
building a tester APK. It sets `TestBuild.IsEnabled`, which:

- Skips ad init entirely: no interstitials, rewarded ads reward instantly (continue, free gems).
- Routes IAP to the mock store: gem packs and Remove Ads grant for free (the real products
  don't exist in the Play Console yet anyway).
- Treats every skin as owned (EQUIP everywhere; nothing is persisted, so unticking restores
  real ownership).
- Tops stars/gems/consumables up to the stash in `TestBuild` on every launch.
- Stamps a red **TEST BUILD** on the menu.

**Untick it before any release build.** The red label on the menu is the guard.

## Build

1. File → Build Profiles → **Android** → Switch Platform (the first switch reimports every
   texture for Android — minutes, the 4096 backgrounds/chef are slow).
2. Player Settings → Publishing Settings. The project points at `Keys/upload.keystore`
   (alias `upload`, password in `Keys/KEYSTORE-INFO.txt`). Unity asks for the password each
   editor session — enter it, or untick **Custom Keystore** for a debug-signed test APK.
   Both install on a phone. (Signature mismatch note: a phone with a debug-signed install must
   uninstall before installing a keystore-signed one, and vice versa.)
3. Build Profiles → make sure **Build App Bundle (Google Play)** is OFF — an .aab cannot be
   installed directly; you want an **.apk**.
4. Build → `Builds/Android/DogtorBurguer-<version>.apk` (`Builds/` is gitignored).

Already set: ARM64 only, IL2CPP, min SDK 25, package `com.proximacentaury.dogtorburguer`,
both scenes in the build list. Bump `AndroidBundleVersionCode` in Player Settings for each
new build you send so phones accept it as an update.

## Share

- Upload the .apk to Google Drive / WeTransfer and send the link (≈ 60–120 MB).
- On the phone: open the link in Chrome, download, tap the file. Android asks to allow
  installs from Chrome — accept once. No developer mode needed.
- Same room: `adb install file.apk` with USB debugging also works.

## Tell the tester

- Everything is unlocked and the store is fake — that's the switch, not a bug.
- The trophy pill / leaderboard does nothing yet (Play Games isn't wired).
- Feedback wanted on feel, difficulty, readability, and anything confusing (the tester is
  the first non-dev pair of hands — see the Phase 2 focus areas in `CLAUDE.md`).
