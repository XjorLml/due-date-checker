# N1 A / N2 A native notifications — 2026-11-08

Scope: the user requested objectives followed by implementation of the recorded notification plan. Source was inspected before editing. The HTML lab, signing configuration, schema and existing local delivery policy are unchanged. [Spec](../../../specs/notification-lab/spec.md), [plan](../../../specs/notification-lab/plan.md), [tasks](../../../specs/notification-lab/tasks.md).

## Implemented

- Unlocked reminder details, generic Android public versions without actions, no rendered notes, and grouped reminders counting distinct dues.
- Immutable authenticated Mark as paid / Undo actions. API 31+ uses a non-exported asynchronous receiver; API 26–30 directly launches a dedicated activity to request system keyguard dismissal. Both recheck lock state before writing.
- Payment validates a delivered receipt, enabled schedule, current occurrence/date and exact persisted record fingerprint under DatabaseDispatcher. The existing reminder-aware repository performs the same atomic recurrence update used by Details. Replays/obsolete actions cannot add another next occurrence.
- Undo validates the paid snapshot and a 10-second UTC window, with an additional Android elapsed-time deadline. Next occurrences remain. Feedback expires, survives short-lived reconciliation and clears on disable/block. Confirmation taps validate the saved snapshot before opening Details.
- Visible data pages refresh after notification writes. Active editor drafts remain; saving over a changed payment status reports a conflict and keeps input. Fresh immutable metadata still follows the existing editor contract.

## Agent-observed automated/build checks

Environment: Windows, .NET SDK 10.0.400, Android SDK pack 36.1.69, JDK 21. Commands:

```powershell
dotnet test tests/DueDateChecker.Core.Tests --no-restore --nologo -v:minimal
dotnet test tests/DueDateChecker.App.Tests --no-restore --nologo -v:minimal
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release -f net10.0-android -t:PackageForAndroid -p:AndroidPackageFormats=apk -p:AndroidKeyStore=false --nologo -v:minimal
```

Observed: **170 Core + 304 App tests passed (474 total)**. Coverage includes text/privacy/amounts/repeats, duplicate concurrent payment, exactly one next occurrence, retained next after Undo, disabled/edited/deleted/paid/overdue/undelivered/mismatched actions, relock before write, Undo expiry/clock rollback/edits, confirmation routing eligibility and editor draft safety. Test warnings: existing xUnit1031 in RecurrenceTests and NU1900 when NuGet advisory metadata was unavailable. Release package build: **0 warnings, 0 errors**. Build verification does not establish action authentication or lock-screen appearance.

## Native and acceptance limits

The connected physical phone was not changed. A fresh task-owned `DuewiseNotificationsCheck` API-36 emulator was created under ignored `artifacts/notification-check/avd`; startup required software graphics. Initial content/metadata/payment checks used candidate SHA-256 `e43cd67b43cd8562465b381a3cde9173a28bbbb636ee7535485b4721719db312`; final payment/Undo verification used `c93bf28c…7866ff5` after the timing fix. Agent-observed:

- Release app installed and launched. Two controlled, prequeued elapsed sample occurrences produced two delivered SQLite receipts and real native shade cards. This checks catch-up/posting, not natural alarm delivery.
- Native payloads and real shade showed sample title, PHP amount or explicit no amount, date and monthly repeat; notes were absent. The generic summary counted two distinct dues. Android 16 additionally auto-grouped the cards; expanding the group and individual child exposed its payment action.
- Actual notification metadata had private visibility, an action-free public version and `isAuthenticationRequired=true`. A standalone rooted-emulator metadata probe checked these; its subsequent PendingIntent-send attempt was killed, so it is not payment/Undo evidence and was not added to the shipped verification tools.
- Tapping the real sample rent action saved it paid and inserted exactly one next occurrence. The initial automated Undo attempt failed to locate the card; the runner was corrected to reopen the shade after Android collapses it. A later attempt located and tapped Undo but did not verify unpaid state; System UI displayed an unresponsive dialog. **Undo UI acceptance was not counted as passed.**
- This exposed a real window issue: reconciliation could shorten the visible Undo duration. The final code starts the window immediately before confirmation posting. Its delayed-confirmation regression passes. Final APK rebuilt with 0 warnings/errors and installed in the emulator. **Follow-up on the final artifact passed:** actual shade action → payment confirmation with Undo → actual Undo tap → SQLite showed the original unpaid and exactly one retained next occurrence. The earlier failures are superseded for this scenario; they are retained here to explain the discovered timing fix.

Final test artifact: `artifacts/notification-check/Duewise-notifications-release.apk`, SHA-256 `c93bf28cb6b0d8ba2bb3687b8dc0b7e80fc7b15cfa0e22c1990e2fa2f7866ff5`. Release configuration, version 1.0/code 1, package `com.lem.duedatechecker`; signed with the existing default Debug certificate for emulator/performance testing, not the upload key. Nothing was published or installed onto the phone.

Cleanup passed: the verifier confirmed task-only membership, removed both sample dues and their generated occurrence, cleared the reminder journal, restored disabled 09:00/day-before/22:00–08:00 defaults, and observed no remaining Duewise notifications. The task-owned emulator was shut down. No existing profile was wiped.

Actual Android 13 MIUI acceptance (including Undo), older Android 8–11 unlock UI, PIN/biometric/cancelled-unlock flows, real lock-screen privacy settings, screen-reader use and natural background delivery remain pending. The API-36 emulator had no secure PIN; the authenticated-action flag and no-write-while-locked service tests do not establish system-authentication UI acceptance. No notification delivery timing guarantee is made.

The user's “all working fine now” report precedes these notification changes and applies to their earlier release/performance build; it is not acceptance of this revision.
