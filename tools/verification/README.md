# Android native verification helpers

## F04 preference checks

[F04 runner](f04_preferences.py) uses the shared explicit-device UI helper and a reviewed ignored `artifacts/f04-preferences/phone-state.json` checkpoint containing original font/theme/connectivity, reminder field values and a private summary hash. Capture that checkpoint before the first F04 upgrade; do not invent/overwrite it. This fixture assumes the pre-F04 default System/Monday preference state. Run `initial`, `calendar-restart`, `themes`, then `cleanup`, each with explicit `--serial`. Check each exit code. Only Settings screenshots are persisted; raw UI/financial data remain in memory. No due is created or edited. Theme canvas checking decodes adb RGB/RGBA PNG with the Python standard library. Cleanup restores default preferences and original device settings; independently verify restoration. [F04 evidence](../../docs/delivery/evidence/F04-PREFERENCES-2026-10-18.md) owns observed results, not this procedure.

If the APK changes after initial checks, `final-calendar` seeds only UI preferences and repeats relevant offline/restart/cached-calendar behavior on the newly installed artifact. It records that installed hash separately. Complete `cleanup` on the final artifact afterwards.

## F05 isolated/native checks

These tools are developer verification utilities, not production app components. They never ship in the APK. [F05 procedures](../../docs/delivery/TEST-PLAN.md#f05-reminder-verification) own expected behavior; [completion evidence](../../docs/delivery/evidence/F05-COMPLETION-2026-10-18.md) records actual results.

Use a fresh **DuewiseF05Check** Android API-36 Google-APIs AVD at port **5580**, with its data under ignored `artifacts/f05-completion/avd`. Do not reuse an AVD containing personal data or wipe an existing profile. The runner checks the guest AVD name before database access. The Java helper additionally rejects physical hardware and accesses only the Duewise test database. `adb root` is for this userdebug emulator; never root the user's phone.

After building/installing the Release APK and starting the isolated AVD, compile the helper from the repository root (PowerShell):

```powershell
$taskSdk = "$env:LOCALAPPDATA/Android/Sdk"
New-Item -ItemType Directory -Force artifacts/f05-completion/java/classes, artifacts/f05-completion/java/dex
javac --release 8 -cp "$taskSdk/platforms/android-36/android.jar" -d artifacts/f05-completion/java/classes tools/verification/F05Database.java
& "$taskSdk/build-tools/36.0.0/d8.bat" --lib "$taskSdk/platforms/android-36/android.jar" --output artifacts/f05-completion/java/dex artifacts/f05-completion/java/classes/F05Database.class
& "$taskSdk/platform-tools/adb.exe" -s emulator-5580 root
& "$taskSdk/platform-tools/adb.exe" -s emulator-5580 push artifacts/f05-completion/java/dex/classes.dex /data/local/tmp/f05-database.dex
python tools/verification/f05_lifecycle.py setup
python tools/verification/f05_lifecycle.py seed --case natural --ahead 3
python tools/verification/f05_lifecycle.py observe
```

Check each command's exit code before proceeding. SDK/JDK access can require reviewed sandbox escalation. Run `observe` after the scheduled time without launching Duewise or forcing a job; an observation before delivery is not a failed feature assertion. Keep native receipt/notification and actual power/idle state separate in evidence. After natural delivery, `fast` exercises update, force-stop/reopen, clock/zone, expiry and permission transitions. `storage-ui` exercises channel blocking, recoverable table-unavailability faults and real editor cancellation. Restore Doze/clock state before another case. `reboot-prepare` and `reboot-check` verify an actual isolated reboot without manually launching the app.

`ui-checks` covers keyboard Save, landscape actions and date-edit cancellation. After observations, `cleanup` verifies that the isolated database contains only task fixtures, removes those fixtures, restores reminders-off defaults, unforces idle, resets battery emulation and restores host time/Manila zone with automatic time enabled. It clears app notifications by normal reconciliation. Shut down the task-owned emulator afterwards; keep ignored evidence/state checkpoints as needed.

`f05_phone.py` requires an explicitly selected `--serial` and an existing reviewed `artifacts/f05-completion/phone-state.json` checkpoint containing the original power/schedule/permission/font/theme/connectivity values and private-summary comparison hash. Do not invent or overwrite that checkpoint. Its task prefix must uniquely identify the task created in this run. Current fixtures assume original reminder opt-in was off. `cleanup` deletes only the uniquely matched task, restores preferences/vendor policy/permission/connectivity and checks baseline membership without logging financial details. Verify restoration after cleanup, including any exceptional exit.

`android_ui.py` selects devices explicitly and keeps live UI XML in memory. Only capture screenshots of the Reminders/settings page or isolated test data; never capture personal due lists/details. Native fault cases stay in the fresh emulator. Keep raw financial data, signing material and private device dumps out of source control/evidence.
