# F05 completion verification, 2026-10-18

Observer: agent-observed. This follow-up preserves the [initial F05 evidence](F05-REMINDERS-2026-10-18.md) unchanged. User direction: "complete f05"; release/signing/CI activation remain held. HEAD remains c0ab21c61040c11ec170561f1f60b9753b5c9cf3 plus preserved uncommitted work.

## Change and environment

[RM01-RM08](../../../specs/f05-reminders/spec.md#acceptance-scenarios) and [PRD policy](../../PRD.md#f05-reminder-rules) govern acceptance. Added Android-reported background restriction/Battery Saver/optimization status and explicit app/battery settings access. Immediate jobs use API-31 expedited scheduling with same-ID normal fallback on quota rejection; no exact-alarm permission, foreground service or automatic power exemption was added.

Windows, .NET SDK 10.0.400. Physical M2101K6G Android 13/API 33, plus a fresh workspace-owned `DuewiseF05Check` API-36 Google-APIs x86_64 emulator (1080x1920, density 420), with no accounts/personal data. Original Pixel5_API36 AVD data was not reused or cleared. The new AVD lives under ignored artifacts/f05-completion/avd. Initial emulator boot under AOT compilation load produced a System UI ANR; selecting Wait recovered it. A verification-helper screen-off launch issue was corrected by explicitly waking before foreground checks. These were environment/harness issues, not passing feature checks or app-code changes.

## Automated/build/artifact

- Core: **61 passed**, 0 failed/skipped; Core source is unchanged in this follow-up.
- Final App: **125 passed**, 0 failed/skipped; **186 total**. Two new clock-boundary regression tests failed before the fix (expired queue after a delayed read; a batch crossing quiet-hour start), then the full App suite passed after moving planning clock capture after reads and rechecking time before each post/next scheduling. The initial completion suite had 123 App / 184 total. New cases cover independent battery/background states, explicit settings access/draft preservation/status-read failures and expedited quota fallback. The cached NU1900 vulnerability-metadata warning remains a network limitation, not a dependency security audit.
- Initial completion Android Release build: **0 warnings/errors**, **00:04:51.82**. Final clock-fix build passed with **0 warnings/errors** in **00:01:54.66**. MAUI/NuGet SDK access used reviewed escalation. The emulator-only Java helper was separately recompiled after an initial sandbox SDK-jar access error; its Java-8 deprecation notices are not Android app build warnings.
- Initial completion APK SHA-256: **FAC00967EF0A17226434F84CC944930736939566D50D031D00CDC1AB76C58816** (`src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`). Intended signing identity is not established by this filename/build.

## Phone observations

- Data-preserving upgrade passed; private summary membership hashes matched. No financial titles/amounts/count values were logged. The new background-status label and Android app/battery settings action work.
- Original vendor setting was **Battery saver (recommended)**. Temporarily selected **No restrictions** using the app's native power settings for the delivery test; restoration is tracked below. No app code changes power policy automatically.
- A uniquely tagged zero-amount task due and due-date-only **18:35 Asia/Manila** schedule were enabled explicitly. App backgrounded, screen off, airplane mode on, Wi-Fi/mobile data off. **An actual queued notification was present by 18:36:40** without a forced job, foreground refresh or device-clock change. This closes the initial natural-delivery observation gap under the documented unrestricted vendor policy; it is not an exact-time or default-OEM-policy guarantee.
- Tapping that actual notification opened the correct task Details. Saved Paid through the real editor; replaying its delivered ID/key safely remained on Dues. User records were not edited.
- The current background/status/settings UI passed Light/Dark at 200% text: status, Save, app/battery settings and Back. Agent inspected settings-only screenshots: wrapping labels/actions are readable. The harness was adjusted to use slow overlapping swipes after fast flings skipped a button; this was not an app-code change. The task was confirmed deleted, original 09:00/day-before/quiet-hour schedule and reminders-off restored, and private baseline membership matched after restart. Original vendor Battery saver (recommended), denied notification permission/flags, font 0.81, Dark theme, airplane mode off and Wi-Fi/mobile data on were restored and checked. No app notifications remain; installed phone APK hash matches the completion artifact.

## Isolated native observations

- Real release app initializes schema 3 with an empty isolated database. A standalone emulator-guarded Java/DEX SQLite helper configures task-only fixtures and controlled faults; it is never packaged into the production APK. It checks native database effects without waking the app or forcing jobs.
- **Natural offline/screen-off delivery passed**: target 18:35 Manila; native receipt recorded at 18:36:23, notification and receipt observed at 18:36:40. No forced job, foreground refresh or clock manipulation occurred during this observation.
- **Deep Doze delivery passed:** after queuing 18:42 work, unplugged the emulated battery and forced deep idle. Actual receipt/notification observed by 18:43:57 without a forced job/foreground refresh; `mState=IDLE` was verified before and after observation. Unforced idle and reset emulated battery afterwards.
- Foreground/restart reconciliation preserved delivered receipts. Same-artifact package replacement rebuilt pending scheduling without UI launch and preserved the occurrence key. Force-stop prevented delivery after advancing only the emulator clock; reopening caught up the existing queued reminder before expiry.
- Device timezone change to Pacific/Honolulu preserved Manila occurrence identity. System clock-change receiver delivered eligible queued work without foreground launch. Moving past the due date removed expired pending work without a notification. Emulator clock/zone were restored between cases; phone clock was never changed.
- Permission revocation prevented clock-event delivery; the native page showed blocked permission while retaining app opt-in. Regrant did not newly catch up cleared elapsed occurrences.
- Channel-disable UI correctly reported the blocked channel separately from app permission and cleared pending work; re-enabling was reflected on return.
- Controlled native table unavailability verified failed Save visibility, retained draft, no committed preference change, successful retry, failed read/retry recovery and preserved task data. The table was restored in finally blocks. This is a controlled SQLite-unavailable boundary, not a disk-full or migration-failure certification.
- Native paid Save canceled pending work and Unpaid Save requeued future work. Delete Cancel preserved the due; confirmed native deletion retained a tombstone and cleared pending work. An actual reboot preserved occurrence identity and rebuilt/delivered without foregrounding Duewise; notification delivery after reboot was also observed. The first reboot assertion raced asynchronous managed startup, so the harness now waits up to 45 seconds for recovery after boot/unlock. Keyboard Tab reached Save and Enter activated it. Landscape at the wider API-36 layout exposed status and scrollable actions. A real editor date change replaced the pending key, canceled the old displayed notification, and rejected an obsolete delivered-date intent. Final-artifact regression is recorded separately below.

## Remaining verification and cleanup

Phone cleanup/restoration passed. After the final APK upgrade, installed SHA-256 and private summary baseline matched, reminders remained off, original optimized-policy status was visible, original font/theme/connectivity and denied permission were rechecked, and no app notification remained. Isolated cleanup removed only the guarded task fixtures, restored reminders-off/09:00/day-before/22:00-08:00 defaults, unforced idle, reset battery emulation and restored host time/Manila zone with automatic time enabled. No test dues or notifications remained. The task emulator was powered off; its serial disappeared from adb and its recorded host process exited. Broad TalkBack/spoken/switch/full cross-device accessibility/OEM certification and intended-key release acceptance are separate gates; no release is authorized. Force-stop, Doze and inexact-delivery limits remain product constraints rather than guarantees removed by this work.


## Artifact attribution

Phone natural delivery, current Light/Dark 200% settings checks and isolated natural/Doze/reboot/update/force-stop/clock/permission/channel/storage/editor/keyboard/landscape checks above ran on initial completion APK FAC00967...58816. The final APK adds the coordinator clock-boundary fix; platform permission/alarm/job/tap code and page/view-model sources are unchanged from that artifact. The final App harness passes 125 tests, including the two previously failing clock races. Final APK installation/status/delivery results are recorded separately below; earlier observations are not silently relabeled as final-artifact passes.


## Final artifact regression

Final APK SHA-256 **762986368B9640E982F10A2BC260B8CAEDDEF4061FCBA0A4190B3BFC42F57C0B**. Data-preserving phone upgrade and installed-hash verification passed; restored opt-in off and original power-policy status remained visible.

The isolated final APK queued **19:12 Asia/Manila** work while offline/backgrounded/screen off. Its receipt was written at **19:13:39.7429483**, and the actual notification plus receipt were observed at **19:14:21** without forced dispatch, foreground refresh or clock change. Observations at 19:11:40 and 19:12:48 preceded delivery; this is an inexact observation, not an exact-minute guarantee.

On that same final APK, repeated foreground reconciliation preserved the receipt; package replacement preserved the pending key/rebuilt recovery; force-stop prevented delivery until reopening; timezone change preserved Manila identity; clock-change dispatch caught up eligible queued work; advancing beyond the due date pruned expired work without notifying; and permission revoke/regrant retained opt-in, showed blocked access and did not newly catch up cleared elapsed occurrences. The native runner records the installed APK hash for each of these final cases. Layout/channel/storage/editor/Doze/reboot checks remain attributed to the intermediate artifact above, whose platform/UI code is unchanged by the final coordinator-only fix. No release/signing/CI action occurred.

## Source and documentation validation

The [completion manifest](F05-COMPLETION-2026-10-18.sha256) contains **97 source/test/assets/verification files**, SHA-256 **05FFD4DAB5CBFEE76239986260DF17E9D27E059BF443F9CB10787F0247D712D8**. Text is normalized CRLF to LF; font bytes are unchanged. All entries were rehashed against the final working tree. Build outputs, ignored device state/screenshots/AVD data and signing secrets are excluded. Verification helpers are not part of the production APK.

All **159** pre-completion docs/specs/src/tests files remain; **19** changed files are confined to the reviewed F05 contracts/source/tests/current status/roadmap. Immutable migrations 001-003, unrelated work and historical evidence (including the initial F05 evidence/manifest) retain their checkpoint hashes. R01-R08 requirement and traceability rows remain unchanged. **650** local documentation links/anchors resolve. Python verification helpers compile, and `git diff --check` passes; Git's pre-existing LF-to-CRLF notices are not whitespace failures. The full automated suites and clean Android build above establish application verification; documentation checks do not substitute for those results.
