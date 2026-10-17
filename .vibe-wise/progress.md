# Learning Progress

> Historical learning/project record, preserved during the 2026-10-17 documentation refactor. For current scope, contracts, and implementation/verification state use the [documentation index](../docs/README.md), [PRD](../docs/PRD.md), [SDD](../docs/SDD.md), and [STATUS](../docs/delivery/STATUS.md). Dated "current", "not implemented", and next-stage statements below retain their original context; they are not a competing source of current status.

## Current handoff

V1.1–V1.3 local CRUD implementations are complete. Current validation (2026-10-17): 70 passing cases (28 Core + 42 App), a Release build with zero warnings/errors, and native V1.3 confirmation/Cancel/Delete/process-restart checks using temporary verification dues. V1.2 functional checks were previously user-accepted. See the [V1.3 completion record](../docs/phases/V1.3.md) for evidence and remaining signing/phone cases.

V1.4 (2026-10-17): a GitHub Actions workflow runs the Core + App tests on a Linux runner on every push; the first GitHub run is pending a push. Decisions (Core + App tests, Linux runner, phone checks stay manual) are in the [V1.4 spec](../specs/v1.4-ci/spec.md). Learning checkpoint: explain why CI targets the test projects rather than the solution, and why a green check is not phone acceptance.

Next: finish release acceptance/signing. Learning checkpoint: explain why soft deletion updates a timestamp instead of removing the row, why repeated deletion preserves the first timestamp, and how the confirmation response reaches the view model. No new demonstrated understanding was exercised in this slice.

## Pending acceptance
- V1.2 edit/paid/restart/offline checks are user-accepted. Intended-key signing, the V1.0 launcher check, V1.1's separate acceptance checklist, and unchecked phone hardening items remain tracked. local.signing.props was absent at the last inspection.

## V1.3 confirmed soft delete (implemented 2026-10-17)

- Added a destructive Delete due action to the existing editor. DueEditPage presents the native dialog and passes its Boolean result to DeleteCommand; a false confirmation returns before storage. Confirmation names the original stored title rather than an unsaved form edit.
- View-model state allows deletion only on a loaded existing due. Pending writes block Save/Delete; failures retain input for a newly confirmed retry. HasCompleted covers saved/deleted outcomes, so navigation retry never resubmits a mutation. Deleting does not validate or save unfinished form edits.
- Reused SoftDeleteAsync and DatabaseDispatcher without a schema/repository implementation change. A parameterized primary-key UPDATE retains the row and fields, sets deletion/update timestamps together, and ignores missing/already deleted rows. No extra index is needed.
- Added 8 App cases and 2 Core cases; the first App run failed to compile before implementation. Final results: 42 App + 28 Core cases pass. The earlier NuGet metadata warning remains in test restore assets; cached dependencies worked.
- Destructive button tokens/outline/minimum height follow docs/DESIGN.md. A source-generation build error with a type-key style reference was fixed by using an explicit BaseButton resource shared by default/destructive styles. Final Release build: zero warnings/errors.
- Installed with adb install -r and exercised only agent-created zero-amount verification dues. Native Cancel retained the editor and due; confirmed Delete returned to the list and hid it; after force-stop/relaunch it stayed hidden. Existing user dues were not edited/deleted. The app was left on the list; verification rows remain hidden tombstones as intended.
- Remaining V1.3 phone cases: airplane mode, Back/dismissal, both themes/large text, repeated taps during a write, and intended-key signing. The broader DESIGN.md Undo interaction is deferred under the approved v1 SDD scope (no restore flow).
- OOP links: IDueRepository hides persistence details (abstraction); the fake and SQLite implementation share that contract (polymorphism); the view model inherits binding notifications from ObservableObject (inheritance); the delete command's state/confirmation guards control valid transitions (encapsulation).

## V1.2 edit/paid (implemented 2026-10-16, functional checks user-accepted)

- User requested the next phase after V1.1. Added row selection and id-only Shell navigation; DueEditPage uses IQueryAttributable and awaits loading on appearance. Clear selection after each tap so the same due can reopen; do not reload an already populated form and overwrite unsaved input.
- Reused DueEditViewModel for create/edit. Edit mode loads all fields, shows a paid switch, persists status only on Save, and preserves Id/CreatedAt. Fresh records remain unpaid by default. Missing/deleted/invalid targets cannot save or become accidental inserts. Load/read/write failures support retry while preserving input.
- Save re-reads and checks the record inside one dispatcher operation. SqliteDueRepository.UpdateAsync additionally uses `WHERE id = @id AND deleted_at IS NULL` and stops writing deleted_at. It preserves CreatedAt/DeletedAt and throws for missing/deleted ids. The primary-key lookup remains appropriate; no extra index or schema migration was needed.
- Tests-first: new App tests initially failed to compile against the absent feature; two new repository tests failed against the old update behavior. Final results: 26 Core and 34 App cases pass. Added 17 App cases and 3 Core cases for edit/paid persistence, id/timestamp preservation, cancellation, invalid/stale/deleted targets, failures/retries, and repeated saves. Existing V1.1 checks still pass.
- Release build: zero warnings/errors. Successful `adb install -r` on the connected phone, explicit MainActivity launch returned Status: ok, and the process remained alive. Native UI verification was blocked by the phone lock screen; an unlock request was sent. Existing saved dues were not edited. Intended-key signing is not configured.
- Follow-up after the user unlocked the phone: the list and editor rendered without storage errors, saved fields populated, the paid switch responded, Cancel returned to the list, and the same row reopened with the original fields/status. A second Cancel left the app on the list. No Save was used. Saved mutations, process-restart persistence, and airplane-mode checks remain pending.
- Acceptance follow-up: the user replied “checks accepted” after the remaining V1.2 saved-edit/paid/restart/offline checks were identified. Record those functional checks as user-accepted; retain the earlier limited agent-observed UI results as history. V1.3 is the next stage; intended-key signing is not covered by this confirmation.
- OOP links: the view model continues to depend on IDueRepository (abstraction); real SQLite and controlled failure repositories satisfy the same contract in tests (polymorphism). Immutable Due copies preserve identity while changing editable fields (encapsulation of the title rule). ObservableObject supplies binding notifications (inheritance).
- No new learner explanation was exercised. Authorization to implement is not evidence of demonstrated understanding. NuGet vulnerability metadata retains the earlier NU1900 warning; cached dependencies and the tests themselves worked.

## V1.1 screen half (implemented 2026-10-16, not yet phone-accepted)

- User authorized implementation with “proceed” after the create/list and startup-wiring next step was described. Used docs/SDD.md as the design baseline; did not require another implementation approval.
- Implemented DuesListPage and DueEditPage (create path), toolkit view models, display rows, DI, Shell navigation, and dues.db under FileSystem.AppDataDirectory. Replaced the template MainPage. Load on appearance/resume; initialize before repository use; keep Add disabled on startup failure.
- DatabaseDispatcher uses SemaphoreSlim plus Task.Run because Microsoft.Data.Sqlite's Task methods still perform synchronous database work. Bound state updates resume on the UI context. All operations use short-lived connections and the same injected path.
- Validation: trimmed required title, required nonnegative decimal PHP amount (no grouping/symbols), centavo rounding/overflow handling, required calendar date defaulted from TimeProvider in Asia/Manila, optional notes. Preserve input on failure and preserve the successful save identity to prevent duplicate inserts on navigation retry.
- Added tests/DueDateChecker.App.Tests: a net10.0 harness linking the actual platform-independent App sources and using the same toolkit package. No MAUI dependencies added to Core.Tests. Initial test run failed because implementation was missing; 17 App cases now pass. Core has 23 passing cases after adding a failing-then-fixed test for newer-schema rejection.
- OOP links: view models inherit ObservableObject for property notifications (inheritance); they depend on IDueRepository, satisfied by SQLite in production and a controlled fake in failure tests (abstraction/polymorphism); Money factories keep conversion/range rules together (encapsulation).
- No learner reasoning was exercised in this turn; implementation authorization is not evidence of demonstrated understanding. Review the database-path flow and why await alone does not move SQLite off the UI thread at the next learning checkpoint.
- Device check: ADB started successfully after sandbox escalation; no devices attached. Intended signing and phone exit tests remain outstanding. NuGet vulnerability metadata was unavailable during the App test restore; cached packages restored and the tests themselves passed.
- Final Android Release build passed with zero warnings/errors after allowing the MAUI cache writes required outside the sandbox. APK: src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk. No local.signing.props exists on this machine, so intended-key release signing is still not configured.

## V1.1 Core half (implemented 2026-10-10, commit "V1.1 core")
- Learner chose option A (Core half first, tests-first) to overlap with the VS install; approved the implementation checkpoint.
- Implemented: Domain/Money.cs, Due.cs, IDueRepository.cs; Data/SqliteConnections.cs, DatabaseInitializer.cs, Migrations/001_create_dues.sql (embedded), SqliteDueRepository.cs; Microsoft.Data.Sqlite 10.0.12. Tests: MoneyTests, DueTests, SqliteDueRepositoryTests = 22 passed. Red run (compile errors) confirmed before implementation.
- Introduced: PRAGMA user_version as the migration cursor, transactional migration + version bump; parameterized SQL; DateOnly as ISO text, timestamps as "O"; partial index for the active-list query; Microsoft.Data.Sqlite async is synchronous underneath; connection pooling keeps files open (ClearAllPools in tests).
- Demonstrated understanding: none exercised yet (learner approved the proposal; reasoning not yet asked back). Needs reinforcement: ask the learner to explain the migration mechanism and why Money is a struct before V1.2.
- OOP pillar links: Money private ctor + factories = encapsulation; Due.Title init validation = encapsulation on `with`; IDueRepository vs SqliteDueRepository = abstraction/polymorphism (tests could substitute a fake). Claude's proposal from the approved v1 plan: Money as long centavos value object; Due as a record with Guid id, DateOnly due date, soft-delete timestamp; raw-SQL repository with user_version migrations.
- v1 re-scope approved 2026-10-10 (plan mode): local CRUD only; learner chose no login in v1, SQLite-only storage, Supabase deferred. Reminders/login/sync come after v1.

## Toolchain finding (2026-10-10)
- VS Community 2022 17.14 is installed at D:\VisualStudio\2022 (Desktop, Web, UWP, CoreEditor workloads; no MAUI packs; .NET 9 SDK present). The VS 2026 winget install (run by Claude, `--passive`) was auto-cancelled at the "VS processes running" pre-check because VS 2022 was open; winget still reported success. Pending learner decision: A install VS 2026 alongside (recommended), B add MAUI to VS 2022 (retarget to net9.0), C CLI .NET 10 workload with VS 2022 as editor only.
- Resolved 2026-10-10: learner chose A. VS 2026 18.10.3 installed at D:\VisualStudio\2026 (installer had to run interactively; `--passive` auto-cancelled on the VS-processes pre-check, `--norestart` without `--passive` is exit 87). `dotnet workload list`: android 36.1.69, maui-windows, ios, maccatalyst. JDK 21.0.8 under C:\Program Files\Android\openjdk; SDK API 36 under C:\Program Files (x86)\Android\android-sdk. Stale ANDROID_SDK_ROOT left alone: build works regardless.
- S-1 steps 3-5 done by Claude (approved 2026-10-06): App project (Android-only, minSdk 26, com.lem.duedatechecker, references Core, CommunityToolkit.Mvvm 8.4.2), allowBackup=false verified in the merged manifest, local.signing.props import + example file. Debug build green; 22 tests green. Keystore and on-phone launch still with the learner.

## Earlier pending decision (S-1, still open for the toolchain part)
- Decision: S-1 implementation scope (toolchain, git + .gitignore, solution with the three chosen projects, Android-only MAUI target, allowBackup=false, keystore outside the repo, signed Release hello-world on the phone).
- Stage: implementation approved 2026-10-06 ("Implement this step"), in progress. Done: git init (branch main), .gitignore (dotnet template + .vibe-wise/, *.jks, *.keystore, local.signing.props), DueDateChecker.slnx, src/DueDateChecker.Core (net10.0, nullable on), tests/DueDateChecker.Core.Tests (xunit 2.9.3, references Core), SmokeTests.cs; `dotnet test` = 1 passed (AC1 met); git status shows only the 5 source files (AC4 ignore part met). Not yet done, blocked on the learner: install Visual Studio Community with the .NET MAUI workload (step 1) and generate the keystore at D:\LEM\keys\duedatechecker-upload.jks (step 5). 2026-10-07: learner reported both done, but probes found no workload, no JDK, no new VS instance, no keystore; installer logs show only a `setup.exe export` of the VS 2019 config to D:\VisualStudio\vs2019.vsconfig. Learner re-confirmed option A (run the VS installer); still pending. 2026-10-07: learner asked to push to GitHub to continue at home; docs/PLAN.md added (plan + status + remaining S-1 steps), .vibe-wise/ un-ignored and committed, repo email set locally to the learner's personal address, first commit made before S-1 is complete (deviation from the single-commit plan, at the learner's request). GitHub remote still to be created (no gh CLI on this laptop). Remaining for Claude afterwards: App project (Android-only, minSdk 26, references Core), allowBackup=false, local.signing.props + csproj import, Release publish + adb install, fix stale ANDROID_SDK_ROOT, single commit "S-1: walking skeleton". Nothing committed yet.
- Resolved 2026-10-06: toolchain = Visual Studio Community with the .NET MAUI workload (learner installs). Learner's reason: complete package, and what C# developers at companies they know use; accepted downside: heavy install. Probe found .NET SDK 10.0.400, git 2.56, no MAUI workload, no JDK, no adb, stale ANDROID_SDK_ROOT, VS 2019 only.
- Resolved 2026-10-06 (revised the same day): MVVM style = CommunityToolkit.Mvvm ([ObservableProperty], [RelayCommand] source generators); view models stay in App. The learner first chose hand-written INotifyPropertyChanged (reason: learning XAML, wanted the mechanism first), then switched to the toolkit; reason for the switch not yet stated (asked for one line for the ADR). Not implemented.
- Resolved 2026-10-06: SQLite access = Microsoft.Data.Sqlite + raw SQL. Learner's reason: wants to structure the data layer themselves and understand what each query does first-hand; asked Claude to flag a better-performing query whenever one exists. Not implemented.
- Resolved 2026-10-06: solution layout = three projects (Core class library with Domain/ and Data/ folders; App MAUI Android-only; Core.Tests xUnit). Learner chose option A from a comparison; reason not stated. Not implemented.
- Original scope note: S-1 (day 0) project structure and the S0-affecting choices: solution/project layout (domain / data / app / tests), SQLite access library, MVVM style.
- Proposed by Claude (not decided): a "S-1" toolchain day before S0; a decisions table with tradeoffs (raw AlarmManager vs Plugin.LocalNotification; Microsoft.Data.Sqlite vs sqlite-net-pcl; hand-written INotifyPropertyChanged vs CommunityToolkit.Mvvm; inexact-only vs exact-with-fallback alarms; next-reminder-only vs all-reminders scheduling; HttpClient vs supabase-csharp for S4). The S1-specific choices (alarm mechanism, exactness, fan-out) are deferred to the S1 checkpoint.
- Awaited reply: the learner's own description of the project layout and their picks for SQLite library and MVVM style, with reasoning. No code until an Implementation checkpoint is confirmed.

## Plan review (2026-10-06)
Introduced (explained by Claude):
- Force-stop puts an Android app in stopped state: no broadcasts, alarms cancelled, nothing fires until the user opens the app; reconcile-on-launch is the only recovery.
- App update may clear alarms; ACTION_MY_PACKAGE_REPLACED receiver as cheap insurance (behavior across versions to verify).
- Doze: *AllowWhileIdle alarm variants; roughly one per 9 minutes per app while dozing.
- SCHEDULE_EXACT_ALARM denied by default on Android 14+ for apps targeting 13+; USE_EXACT_ALARM reserved for alarm-clock/calendar apps under Play policy.
- Play personal-account rule: 12 testers, 14 continuous days, before production access.
- Play requires targetSdk 36 since 2026-08-31 → .NET 10 MAUI.
- Android Auto Backup is on by default (plaintext app data to Google Drive); allowBackup=false is a one-line manifest change.
- Notification channel importance is fixed at creation.
- Release trimming can strip reflection used by sqlite-net/JSON.
- Schema migrations via PRAGMA user_version; client-generated UUIDs, updated_at, deleted_at from day 0 to keep sync possible later.

Demonstrated understanding: none yet in conversation. The learner's plan document contains reasoning (e.g., ADR 003: never both notification channels in the MVP because of duplicate reminders and double cancel logic; ADR 007: alarms rebuilt from SQLite), authorship of the reasoning not yet exercised at a checkpoint.

Needs reinforcement: to be determined at the S-1 and S1 checkpoints.

## OOP pillar links (requested by the learner 2026-10-06; explained, not yet demonstrated)
- INotifyPropertyChanged: an interface the XAML binding engine depends on, not your concrete class → abstraction. Private backing fields changed only through setters that raise PropertyChanged → encapsulation. With CommunityToolkit.Mvvm, view models derive from ObservableObject, which implements that interface once for all of them → inheritance; the generator writes the setters, the mechanism is unchanged.
- INotificationScheduler (interface in Core) with an Android implementation in App and a fake in tests → abstraction + polymorphism (same call, different object answers at runtime).
- Money value object: centavos kept private, rounding rule enforced in one factory method → encapsulation.
- Raw SQL repository behind IDueRepository → abstraction (Domain talks to the interface, never to SqliteConnection).

## Scoping (2026-10-06)
Learner requirements stated (product decisions, not engineering evidence): day-7 deliverable = S0–S3 local-only, sideloaded Release APK; 6–8 h/day; Supabase sync (S4–S5) outside the 7-day window; learn-mode defaults.
