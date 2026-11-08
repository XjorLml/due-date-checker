# Release rebuild for performance comparison — 2026-11-08

User request: rebuild a new Release APK after reporting lag on every page switch in the installed Debug app. All build/package observations below are agent-observed on 2026-11-08. Phone navigation lag is user-reported; performance improvement is not yet measured.

## Source and environment

- Source commit: `a67e06914cf427ab5e0448a147103080428d6ac8`, branch `develop`; application source unchanged by this task.
- Windows workspace, .NET SDK 10.0.400, MAUI workload 10.0.20, Android SDK tools 36.0.0, Microsoft JDK 21.
- Existing signing configuration and the old `artifacts/release/Duewise-v1.0.apk` were absent here. The prior bin/Release signed APK matched the older November 4 notification-fix checksum, not current source.

## Completed Release performance artifact

`artifacts/release/Duewise-v1.0-2026-11-08-release-performance.apk` (gitignored), with adjacent `.apk.sha256` file.

- SHA-256: `f25baaf5b317cafb7a39ad77db2f7d1183841ef82e5ebcc0446fa0a21b19fa40`.
- Size: 33,218,978 bytes.
- Package: `com.lem.duedatechecker`, versionName 1.0, versionCode 1; minSdk 26, targetSdk 36.
- **Release configuration, not debuggable**: manifest/package inspection found no debuggable flag. Trimming and AOT compilation ran; the archive contains 196 compiled `.dll.so` entries across arm64-v8a and x86_64.
- **Signed with the existing Debug certificate**, not the upload key. Certificate SHA-256: `3ae0b536ba5aae747e1815ff2f7ac843c32aaee75500e70f2320df7f5563a1c1`.
- That certificate matches the phone signer recorded on November 7. This is the intended update-compatible build for comparing Release-mode navigation without changing signing identity. The current phone installation/signature was not freshly inspected in this task.
- `apksigner verify`: v2 and v3 signatures passed. ZIP integrity and the copied APK checksum passed.
- This is a performance-test APK, not a production/upload-key release. A signing certificate named Debug does not make this APK a Debug-configuration build.

Build command:

```text
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release -f net10.0-android -t:PackageForAndroid -p:AndroidPackageFormats=apk -p:AndroidKeyStore=false --nologo -v:minimal
```

Result: **0 warnings, 0 errors**, about 2m02s. The resulting `com.lem.duedatechecker-Signed.apk` was copied into the explicit performance-test filename above before attempting upload-key signing. That preserved artifact was reverified afterwards.

## Regression verification

- Core: **162 passed**, no failures or skips.
- App: **279 passed**, no failures or skips. Total: **441 passed**.
- Test-build warnings: existing xUnit1031 in `RecurrenceTests.cs`; NU1900 because NuGet vulnerability metadata could not be fetched. These are separate from the zero-warning APK build.
- No installation, uninstall, phone-data mutation, device performance test or publishing was performed.

## Upload-key signing remains incomplete

The user located the existing upload keystore under their private keys directory. An ignored `src/DueDateChecker.App/local.signing.props` was prepared with that path and local-only password placeholders; the user filled it locally. Both the earlier example password and the updated local store password were rejected by `keytool` with “keystore password was incorrect.” Password values were not copied into this evidence or into command arguments.

A `SignAndroidPackage` build was started before the failed metadata verification was acted on; it was stopped without producing an upload-key release. Output from that attempt was redacted against the local password values. The completed, independently verified performance APK remains intact. Correct local upload-key credentials are needed for a production-signed artifact; there is no new production `Duewise-v1.0.apk` from this task.

The user subsequently chose to build the upload-key version themselves and requested steps. No further signing attempts are authorized by this handoff; the completed compatible performance APK remains available.

## Remaining acceptance

After the handoff, the user created a replacement `duedatechecker-upload-v2.jks` and reported `MSB6006: java.exe exited with code 2` from their publish command. SDK target inspection identified `AndroidApkSigner`; local configuration still pointed to the old keystore. Agent-observed: the replacement keystore accepts the current configured store password and contains alias `upload` (certificate SHA-256 `4c04a240da3ca247accd96d2ce82916c6dc8f2cdd82fa261fc83cb20e690a90e`). Only the ignored local keystore path was corrected; alias/password values were preserved. No new signing/build attempt was run by the agent. The user's publish retry and signed-artifact verification remain pending.

Install the matching-key Release performance APK as an update and compare repeated navigation with Debug on the same phone/data. Verify saved dues, reminders and export after update. No faster-navigation claim is established by compilation or tests. Production signing, phone checks, publication and the release tag remain separate.
