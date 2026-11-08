# Launcher icon palette correction — 2026-11-08

Small visual fix under R01 / [FA01](../../../specs/focus-adoption/spec.md#acceptance-scenarios); the existing [DESIGN brand/palette](../../DESIGN.md#2-brand-identity) remains authoritative. The user reported a purple launcher after changing the app's theme. Source inspection found `Resources/AppIcon/appicon.svg` still used `#4338CA`, despite the project `MauiIcon.Color`, splash background and in-app brand mark already using brand ink.

## Change

Changed the launcher background SVG fill to `#253C20`. Foreground geometry, white calendar/check, coral dot, splash, package identity and signing configuration were preserved. The icon uses the fixed brand palette in both in-app appearances. Other uncommitted notification/release work was retained.

## Agent-observed verification

Windows, .NET SDK 10.0.400, Android SDK pack 36.1.69. Built:

```powershell
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release -f net10.0-android -t:PackageForAndroid -p:AndroidPackageFormats=apk -p:AndroidKeyStore=false --nologo -v:minimal
```

- Build succeeded: **0 warnings, 0 errors**. Log: ignored `artifacts/icon-palette-check/release-build.log`.
- Visually inspected the generated xxxhdpi legacy icon before and after: purple replaced with deep green, existing symbol retained.
- Opened the actual APK as a ZIP and decoded all 15 `appicon.png`, `appicon_round.png` and `appicon_background.png` resources across mdpi, hdpi, xhdpi, xxhdpi and xxxhdpi. The top-center background sample in each was opaque RGB `(37, 60, 32)`, exactly `#253C20`.
- Generated adaptive-icon XML references the corrected `appicon_background` and existing foreground/monochrome resources.
- No unit tests added or rerun for this reversible asset recolor. Previously recorded application tests are not a new result for this build. No device was changed.

Artifact: ignored `artifacts/icon-palette-check/Duewise-green-icon-release.apk`; SHA-256 `3c08f6757308cdef9f8c7706cfd85db1bab8a8db5134c8c81ae4f9cf39ca3c77`. This is a Release configuration test package signed with the default Debug certificate, not a new upload-key artifact. Existing notification verification artifacts are retained separately.

## Remaining acceptance

Observe the launcher icon on the user's phone after a compatible signed update. No installed-phone appearance, launcher-cache behavior or system themed-icon color is claimed by the asset inspection.
