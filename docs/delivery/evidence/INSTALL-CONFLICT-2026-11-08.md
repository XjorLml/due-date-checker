# Signed APK installation conflict — 2026-11-08

User reported “app not installed as package conflicts with existing package”. Agent performed read-only diagnostics on the connected M2101K6G; no database, dues, preferences or financial UI were read.

## Observed

- `pm path com.lem.duedatechecker`, followed by copying only its installed base APK to ignored `artifacts/install-conflict-check/installed-base.apk`.
- Installed APK SHA-256: `f25baaf5b317cafb7a39ad77db2f7d1183841ef82e5ebcc0446fa0a21b19fa40`, exactly matching the earlier Release performance artifact. Package `com.lem.duedatechecker`, version 1.0/code 1, non-debuggable. Public signing certificate SHA-256: `3ae0b536ba5aae747e1815ff2f7ac843c32aaee75500e70f2320df7f5563a1c1`.
- Current `artifacts/release/com.lem.duedatechecker-Signed.apk` has a valid signature with certificate SHA-256 `4c04a240da3ca247accd96d2ce82916c6dc8f2cdd82fa261fc83cb20e690a90e`, the new v2 upload keystore recorded earlier.

The certificates differ for the same package ID. This confirms a signing mismatch preventing a normal update ([Android signing contract](https://developer.android.com/studio/publish/app-signing#considerations)); no claim is made about other installer/OEM checks after resolving it.

## Recovery choices and limits

For the v2 identity: export dues through Settings → Your data → Export dues, save outside app-private storage and verify the JSON backup before any uninstall. Then the user may uninstall, install the v2-signed APK, import into the empty app and re-enable reminders. The export/import source covers dues, not all application preferences. Keep using the same intended key for subsequent updates.

For preserving the current installation directly: use a Release APK signed with its existing key. This does not migrate it to the v2 identity.

No export, uninstall, install, import, key change or build was performed. All actual migration and post-install data/reminder acceptance remain unverified. Prior notification/emulator evidence remains attributed to its own artifacts; this inspection only establishes the installed package identity.

## Follow-up after reported uninstall — 2026-11-08

User reported that installation still failed after uninstalling. Agent-observed, read-only ADB checks on the connected M2101K6G established:

- `pm list users`: user 0 (Owner) and user 10 (`security space`).
- `dumpsys package com.lem.duedatechecker`: version 1.0/code 1 remains registered, with `installed=false` for user 0 and `installed=true` for user 10.
- Separate `pm list packages --user 0 com.lem.duedatechecker` and `--user 10` checks returned no package for Owner and the package for security space, respectively.

The uninstall removed the Owner installation but left an installation in security space. Together with the earlier signing-certificate mismatch, this explains why uninstalling from the main profile did not remove the update conflict. The exact latest installer error and successful recovery remain unverified. No app-private data was read, and no package, profile or data was removed or installed during this follow-up.

Recovery: export any needed dues from the remaining installation before removing it. Uninstall Duewise in that space too, or use an all-users package uninstall after accepting loss of this app's remaining local data, then retry the intended signed APK. Android documents [per-user versus all-users uninstall](https://source.android.com/docs/devices/admin/multi-user-testing#adb-interactions-across-users). Removing the entire space is unnecessary.
