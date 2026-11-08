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

## UI consistency slice (2026-11-05, later)
Decision: the learner chose to reword the PRD about optional amounts and then fix the status-badge inconsistency and duplicate titles first, ahead of the other review findings.

Introduced (explained by Claude):
- A MAUI `ContentView` with a `BindableProperty` is the lightweight way to make a reusable piece of UI that pages can bind to; the XAML inside reacts to that one property through data triggers, so four pages share one definition instead of four copies.
- `Shell.NavBarIsVisible` is per page: roots can drop the toolbar while pushed pages keep it for the Back arrow.
- Fast Deployment keeps Debug assemblies in the app's private files folder; `pm clear` deletes them, and MSBuild's deploy stamp then believes they are still there, so only a reinstall pushes them again.

OOP pillar links:
- `DueListItem.Create` hides how a status is derived behind one factory and `DashboardItem` delegates to the same row → encapsulation of the rule and a single source of truth.
- `StatusBadge` exposes only `Status`; the colours and glyphs are internal → abstraction. Pages depend on the property, not on the pill's internals.
- The optional `TimeProvider` constructor parameter keeps the view models open to a test clock while closed to changes in callers → polymorphism through the injected abstraction rather than a subclass.

## Focus design adoption (2026-11-05)
Decision: the learner chose scope "look + navigation" (recommended) over "look only" and "full mockup", and "install emulator" over phone/build-only. Implemented by Claude; learner review of the diff is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- MAUI Shell on Android renders its TabBar as a Material BottomNavigationView. Shell owns the menu items and selection; a renderer subclass (DuewiseShellRenderer) can only observe and decorate. We disable one menu item natively and wrap the bar in our own FrameLayout to float the lime Add disc. Why not a plain fifth tab: BottomNavigationView applies one icon tint list to every item, so a lime icon would render in the primary colour.
- Android View measure/layout: a parent ViewGroup decides its own size in OnMeasure and places children in OnLayout. The dock reports zero height when Shell sets the bar to Gone, which is how the disc disappears on hidden-bar pages without a second visibility flag.
- Java trust stores: the JDK ships its own cacerts, separate from Windows. A TLS-inspecting proxy or antivirus root certificate is trusted by Windows but not by Java, hence sdkmanager's "PKIX path building failed". `-Djavax.net.ssl.trustStoreType=Windows-ROOT` tells Java to use the Windows store instead.
- Android SDK discovery: the .NET Android workload prefers `%LOCALAPPDATA%\Android\Sdk` when it exists. A partial SDK there (emulator only) broke aapt2 discovery, so the emulator SDK lives at `Android\EmulatorSdk` instead.
- jarsigner reads the keystore with the store password before it can sign; "keystore password was incorrect" is a credential problem, not a build problem, and `-p:AndroidKeyStore=false` separates the two.
- Debug Android builds use Fast Deployment: the APK carries no managed assemblies, they are pushed to the app's files directory by the Install target. Installing the Debug APK with plain `adb install` therefore aborts at startup with "No assemblies found".
- Android's view hierarchy dispatches touches to the child with the highest Z (elevation) first, not simply the last-added child. Material's bottom bar carries 8 dp elevation, so a 6 dp disc overlapping it never received taps until its elevation was raised above the bar's.
- `debug.hwui.drawing_enabled=0` tells Android's hardware UI renderer to skip drawing; layout, accessibility and input keep working, which is why `uiautomator dump` showed a correct tree while every screenshot was black. It is a `debug.` property, so the shell may set it on userdebug images.
- The app bar area under a transparent status bar is Material's `AppBarLayout`, whose background is `?attr/colorPrimary`; with no SystemUI there is no opaque status bar to cover it, so the emulator showed an ink/lime strip that a phone would not.

OOP pillar links:
- DuewiseShellRenderer overrides CreateBottomNavViewAppearanceTracker and SetAppearance → inheritance plus polymorphism: Shell calls the base type's method and our override runs.
- AddDock hides its wrapping, measuring and colour logic behind two public members (Wrap, ApplyColors) → encapsulation; AppShell exposes OpenAddAsync so the native disc never touches Shell navigation internals → abstraction.
- PageLayout.Connect gained optional parameters (footer, extraBottom) instead of a second helper → one abstraction, extended without breaking existing callers.

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

## Focus page content (2026-11-05; explained by Claude, not yet demonstrated)
- `IDueRow` interface: two records (`DueListItem`, `DashboardItem`) already had the same members; naming that shape as an interface lets one XAML `ContentView` (`DueRow`) use compiled bindings against the interface instead of a concrete type → abstraction. The pages keep constructing their own records; the row only depends on the contract → the same polymorphism idea as `IDueRepository`, applied to a view.
- `SegmentedControl`: the selected position lives in one `BindableProperty` with `BindingMode.TwoWay`; the buttons never talk to the view model, they set `SelectedIndex` and the binding carries it → encapsulation of the control's state behind one property. The view model's existing `SelectedThemeIndex` did not change, so the F04 tests did not change.
- Upcoming total: computed from the already loaded snapshot in centavos and formatted through `Money.FromCentavos(...).ToString()`, so the peso format stays inside the value object → encapsulation again (the rule that says "₱ and two decimals" has one home).
- Counters as taps on a `Border` with `TapGestureRecognizer`: cheaper than a Button for two-colour text, but a Border is not a native button role; the chips below remain real Buttons for the same filters. Trade-off noted for the TalkBack review.
- `Refresh / Retry` bound to `HasError`: the PRD already says appearance reloads, so an always-visible refresh was redundant UI; binding visibility to state is the MVVM way to remove it without touching the command.

## F07 optional amounts (2026-11-06; explained by Claude, not yet demonstrated)
Decision provenance: the rules (Bill/Subscription required, Task/Other optional, blank = NULL not zero, "No amount") were the learner's 2026-11-03 product decisions; the learner authorized implementation on 2026-11-06 ("start the remaining task starting with optional amounts then do the rest"). Editor and Dashboard wording came from the learner's selected Focus mockup. Implemented by Claude; learner review of the diff is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- `Money?` (a nullable value type) is `Nullable<Money>`: a small struct holding `HasValue` plus the `Money`. `null` and `Money.FromCentavos(0)` are different values, which is exactly the "no amount vs zero" rule; SQLite mirrors it with NULL vs 0 in the same column.
- Why `Due.Validate()` is a method, not an init check: a `with` expression copies the record and then runs each init accessor one at a time. A rule that spans two properties (category and amount) would see a half-changed record, e.g. `task with { Category = Bill, Amount = 5 }` sets Category first while Amount is still null. So the finished record is checked by `Validate()`, and every repository write calls it (defence in depth: editor, domain, repository).
- SQLite cannot drop `NOT NULL` with `ALTER TABLE`, so migration 004 uses SQLite's documented rebuild: create the new table, copy (including the hidden `rowid`), drop, rename, re-create the index. DDL is transactional in SQLite, so the initializer's single transaction makes the rebuild all-or-nothing; a test proves a failed rebuild leaves schema 3 untouched.
- Query-plan check: the test compares `EXPLAIN QUERY PLAN` for the list query before and after the rebuild, so the partial index `ix_dues_active_due_date` still serves `WHERE deleted_at IS NULL ORDER BY due_date` (the learner asked to be shown query performance where it matters).
- Test flakiness: `SqliteConnection.ClearAllPools()` is process-wide (static state). xUnit runs test classes in parallel, so one class clearing every pool could close another class's open connection mid-query. Fix: clear only your own file's pool (`TestDatabase.ClearPool`). Lesson: global mutable state in tests turns parallelism into intermittent failures.

OOP pillar links:
- `Due.Validate()` + `Due.RequiresAmount()` keep the amount rule inside the domain type; the editor and repository ask the record instead of re-implementing the rule → encapsulation (one home for the rule).
- `DueListItem.NoAmountText` is the single literal every page uses; Dashboard, Details and Calendar reach it through the same `IDueRow` projection → abstraction (pages depend on `AmountText`, not on how absence is represented).
- `SqliteDueRepository` still implements `IDueRepository` unchanged in shape; only its mapping learned about NULL, so no caller changed → abstraction/polymorphism protecting callers from a storage change.

Learning checkpoint: explain why blank must not become zero, why the cross-field rule cannot live in an init accessor, and what would happen to existing rows if the migration forgot to copy `currency` or `rowid`.

## Remaining non-phone checks (2026-11-06; explained by Claude, not yet demonstrated)
Authorized by "then do the rest". Implemented and verified by Claude on the emulator; learner review is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- Android soft-input modes: `Pan` slides the whole window up just enough to show the focused field, so anything pinned to the bottom (our Cancel/Save row) ends up under the keyboard; `Resize` shrinks the window instead. From Android 15 apps draw edge-to-edge, so the keyboard's size arrives as an "inset" the app must pad for itself; MAUI 10's `SafeAreaEdges` `SoftInput` does that padding. On the emulator both were needed. Only the editor switches modes, so root pages keep the tab bar off the keyboard.
- A guard that lives in one page does not protect a second entry point: the Focus adoption added a global Add disc that skipped the list page's `CanAdd` check. The fix asks the source of truth (the database dispatcher) instead of copying the page's flag.
- Connection pooling keeps native SQLite handles open between "closes". After the database file was swapped underneath, Retry kept using the old handle until the process restarted; and PRAGMAs set on a pooled connection follow it to the next borrower.
- `long` vs `decimal`: each amount fits in a 64-bit integer, but a sum of two maximum amounts does not; LINQ `Sum` over `long` is checked and throws. A display total can be summed as `decimal` (28-29 significant digits).
- Emulator `wm size`/`wm density` overrides simulate other phones and tablets without new virtual devices; `adb root` (only on `userdebug` images) allows setting the clock to test midnight.

OOP pillar links:
- `Money.Format(decimal)` keeps the peso display rule inside `Money` even for numbers that are not one `Money` value → encapsulation (one home for the format).
- `DatabaseDispatcher.TryInitializeAsync()` exposes readiness as a small public operation and hides how initialization and retries work → abstraction; AppShell depends on the question "can the database open?", not on SQLite.

Learning checkpoint: explain why the footer was hidden with `Pan`, why a second Add entry point needed its own check, and why summing two valid amounts can fail.

## UI/UX review and improvement plan (2026-11-06; planning only)
Requested by the learner: rate the UI/UX against the documented users, then record the findings and plan improvements from highest to lowest impact. Recorded in docs/delivery/UX-REVIEW-2026-11-06.md and specs/ux-improvements/.

Introduced (explained by Claude, not yet demonstrated):
- Heuristic review: judging a product against its stated users, personas and principles rather than personal taste; it is evidence of risk, not user testing.
- Impact scoring as reach × frequency × severity, with ties broken by effort, and recording the judgement call that does not fit the formula (irreversible data loss ranked by frequency).

Pending learner decisions, in plan order: UX01 (recommended: "Mark as paid" button on Details), UX02 (recommended: collapse earlier paid dues on Dues), then UX03 recurrence rules and UX05 export approach.

Resolved 2026-11-06 (UX decisions, from the learner's decision-lab export): UX01 A, UX02 A, UX03 A, UX04 A, UX05 A, UX06 **B** (Undo after Delete, against the recommendation), UX07 A, UX08 A, UX09 A, UX10 A, UX11 A, UX12 A, UX13 A, UX14 A, UX15 **B** (shrink calendar cells, against the recommendation), UX16 A. Reasons not stated beyond the listed pros and cons. Recorded in specs/ux-improvements/spec.md (decision log), PRD and DESIGN as decided, not implemented. Open sub-decisions: whether Task/Other may repeat (UX03) and the export file format (UX05). Implementation not started.

## UX Wave 1: Mark as paid and earlier paid fold (2026-11-06; explained by Claude, not yet demonstrated)
Follow-up decisions by the learner: recurrence for Bill and Subscription only (UX03); export as JSON with a version number (UX05). Wave 1 implemented by Claude after "then start the wave 1"; learner review of the diff is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- Intent, not a blind flip: the toggle saves the opposite of what the user saw, and re-reads the row first; if another writer already made that change, it writes nothing. This makes a double tap or a stale screen harmless.
- A derived list: the view model keeps one snapshot of all active dues and computes the visible list by filtering. Filtering a sorted list keeps its order, so expanding shows the folded rows exactly where they belong without re-sorting or another query.
- Two "empty" states mean different things (no dues at all vs everything folded); separate properties keep the first-run card from appearing when you simply paid everything.
- Hidden time bombs: tests that build a view model without a test clock silently use the real date; they passed until a rule started depending on today. Every list in the tests now gets its test's clock.

OOP pillar links:
- `TogglePaidCommand` keeps the read-check-write sequence inside the view model and the dispatcher gate; the page only asks for the action and announces the result → encapsulation.
- `DuesListViewModel` exposes `Dues`, `EarlierPaidText` and `IsAllFolded`; the page does not know how folding is decided → abstraction over the folding rule.

Learning checkpoint: explain why the toggle re-reads the row instead of trusting the shown item, and why the fold is a filter on a snapshot rather than a second database query.

## UX Wave 2: recurring dues and maker reminder steps (2026-11-06; explained by Claude, not yet demonstrated)
Implemented by Claude after "start wave 2" (UX03 recurrence for Bill/Subscription, UX04 Xiaomi/Redmi/POCO steps). Learner review of the diff and ADR 009 is the next checkpoint; the ADR's storage choices were Claude's proposals and still need the learner's approval.

Introduced (explained by Claude, not yet demonstrated):
- Anchor day: a monthly due on the 31st stores "31" separately from its date, so 31 Jan → 28 Feb → 31 Mar instead of drifting to the 28th for good.
- One transaction for two writes: "mark paid" and "add the next one" commit together or not at all, so a crash between them cannot end a series silently.
- A database-level guard (`INSERT … WHERE NOT EXISTS`) makes a double tap, a stale screen or paid → unpaid → paid harmless even if app checks are bypassed.
- C# default interface methods: `IDueRepository.UpdateWithNextAsync` has a body so test doubles keep compiling; the real repository and the reminder wrapper override it. A wrapper that forgets to override silently gets the non-atomic default, so a test proves the forwarding.
- A library trap found by a failing test: Microsoft.Data.Sqlite's `ExecuteNonQuery` dropped the error of a statement that followed one reporting result columns (an `ALTER … ADD COLUMN` with a CHECK does). The migration runner now steps through every result. A rollback test that "unexpectedly passes" is a signal to investigate, not to loosen the test.
- Android detail: Redmi and POCO phones report manufacturer "Xiaomi" with their own brand, so the check looks at both.

OOP pillar links:
- `Recurrence.NextOnPayment` and `AnchorAfterEdit` keep the rules in the domain; Details and the editor ask the same question instead of each re-implementing it → encapsulation and abstraction.
- `ReminderAwareDueRepository` decorates `IDueRepository`: callers see one interface while the wrapper adds reminder refreshes → polymorphism through the interface (the decorator pattern).
- `BackgroundDeliveryStatus.MakerSteps` lets the status object decide its own advice from data it already holds → encapsulation.

Learning checkpoint: explain why the next occurrence is created only on unpaid → paid, why the anchor is stored separately from the due date, and why the migration test that passed was actually a bug.

## UX Wave 3: export and import (2026-11-06; explained by Claude, not yet demonstrated)
Learner decision at the start: import works when no dues show; a file due replaces a hidden deleted one with the same ID. Implemented by Claude after "start wave 3". Learner review of the diff and ADR 010 (Claude's technical proposals, not yet approved) is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- Storage Access Framework: the app asks Android to show its own save/open dialog; the user's choice grants access to that one file, so no storage permission is needed. The answer comes back later through the activity's OnActivityResult, which the code turns into an awaitable Task with a TaskCompletionSource.
- A file format is a contract: `format` says what the file is and `version` says which rules it follows. The reader checks those two before anything else, so a file from a newer app is reported as "update the app" instead of "broken".
- Validate everything, then write everything in one transaction: the user gets "all restored" or "nothing changed", never half a backup.
- Checking a condition twice on purpose: once before opening the picker (friendly) and again inside the transaction (correct), because data can change while the user is choosing a file.
- Trimming: the Release build removes unused code and turns off reflection-based JSON, so the export is written and read by hand with Utf8JsonWriter and JsonDocument.
- An upsert with a guard (`ON CONFLICT … DO UPDATE … WHERE deleted_at IS NOT NULL`) can replace only hidden rows; touching a visible one changes 0 rows, which the code treats as "stop and roll back".

OOP pillar links:
- `IDueFileService` hides Android's dialogs behind a small interface; the view model and its tests never see Intents → abstraction, and the fake in tests is polymorphism.
- `DueArchive` keeps the whole file format in one class with a few public operations → encapsulation; the App only maps its problem codes to words.

Learning checkpoint: explain why the import is refused before the picker and checked again in the transaction, why the file carries a version number, and what happens to a hidden deleted due when the file contains the same due.

## UX Wave 4: messages with Undo and the polish batch (2026-11-06; explained by Claude, not yet demonstrated)
Implemented by Claude after "start wave 4": UX06 messages and 5-second Undo, UX07 editor date, UX08 amount keyboard, UX10 counter buttons, UX11 field order, UX12 Manila timestamps (UX09: no work). Learner review of the diff is the next checkpoint.

Introduced (explained by Claude, not yet demonstrated):
- Where a message lives decides whether it survives navigation: a snackbar attached to the activity outlives the editor page that triggered it; one inside the page would vanish with it.
- A guarded restore: `UPDATE … WHERE id = @id AND deleted_at = @deletedAt` can only reverse the one deletion it was offered for. The exact stamp is the "receipt" of that delete.
- A singleton service outlives transient pages, so the Undo callback goes to `DeletionUndo`, not to the closed editor.
- Platform defaults leak through frameworks: MAUI's Numeric keyboard adds a "signed" flag, and it rebuilds the input type from several properties, so a fix must be re-applied after each of them. Verified by reading the real `inputType` (0x2002) instead of trusting the screenshot.
- Accessibility role versus look: an invisible native Button over a drawn card gives screen readers "button" without changing the design.
- Time zones at the edge: 16:30 UTC is 00:30 the next day in Manila; tests pin instants around midnight.

OOP pillar links:
- `IMessageHost` hides Material's Snackbar behind two methods; tests use a fake → abstraction and polymorphism.
- `DeletionUndo` keeps the "latest offer, once" rule and its messages in one place → encapsulation.
- `ReminderAwareDueRepository.RestoreAsync` decorates the repository again: same interface, added reminder refresh → polymorphism (decorator).

Learning checkpoint: explain why Undo needs the deletion stamp, why the editor shows its message after navigating back, and why the counter got an overlay button instead of being rebuilt as a Button.

## UX Wave 5: presentation polish (2026-11-07; explained by Claude, not yet demonstrated)
Implemented by Claude after "start wave 5": UX13 bar captions, UX14 quiet "No amount", UX15 calendar cells fit narrow phones, UX16 compact landscape headings. All sixteen UX decisions have now shipped; phone and TalkBack checks remain for every wave.

Introduced (explained by Claude, not yet demonstrated):
- Pure functions for layout decisions: the sizing rules live in `LayoutRules` with no MAUI or Android types, so they are unit-tested; the platform code only measures and applies.
- Measuring real text: the bar measures each caption with its own Paint (typeface and boldness) before choosing a size, instead of guessing from character counts.
- A layout listener that settles: it changes sizes only when they differ, so the layout it triggers does not trigger it again forever.
- Trade-off recorded, not hidden: UX15 option B goes below DESIGN's 44 dp target (to about 40 dp) but stays above WCAG's 24 px minimum, and DESIGN records the exception.

OOP pillar links:
- `LayoutRules` exposes three small questions (caption scale, cell width, heading size) and hides the arithmetic → abstraction and encapsulation.
- `PageLayout.Connect` gained an optional `heading`; pages that do not pass one behave as before → extension without changing existing callers.

Learning checkpoint: explain why the caption size depends on both the system text size and the slot width, and why the calendar cell rule never goes below 24.

## Prototype alignment: every page in the live prototype's look (2026-11-07; explained by Claude, not yet demonstrated)
Implemented by Claude after "update the documentation based on the current LIVE PROTOTYPE then implement all page changes" and six scoping answers (look and layout only; later decisions kept; prototype settles UX19, UX22, UX24, UX25; lowercase header; search moves to Dues). Also completed the bottom bar's Focus look the same day.

Introduced (explained by Claude, not yet demonstrated):
- Moving behavior with its tests: search and filters changed owner (Dashboard → Dues) with the same assertions, so the rules could not drift during the move.
- Vector icons from data: one `Glyph` view draws any prototype icon from its SVG path text and takes a theme colour, instead of shipping a light and a dark image per icon.
- A "dirty" check by snapshot: the editor remembers the form as it opened and compares, trimming text so whitespace alone is not a change.
- Drawn controls with native buttons on top (headers, calendar days): the drawing can look like the prototype while screen readers still get a real button.
- Measuring before judging performance: two slow launches looked like a regression; timing a third under normal load showed 3 seconds.

OOP pillar links:
- `RootHeader` and `PushedHeader` are small reusable views with properties and events; pages configure them instead of copying markup → encapsulation and abstraction.
- `IDueRow` grew four members, and `DueListItem` and `DashboardItem` both satisfy it, so one row template serves every list → polymorphism through an interface.
- `DashboardItem` delegates to a `DueListItem` instead of repeating its formatting → composition over duplication.

Learning checkpoint: explain why the Dues list updates one collection in place instead of replacing it, why `HasChanges` is false while a due is still loading, and why the calendar day is a drawing with a transparent button over it.

## Notification lab and the first signed release (2026-11-07; explained by Claude, not yet demonstrated)
Built by Claude after "show it via the index html" (notification samples and Mark as paid from a notification), then the user chose N1 A and N2 A in the lab (recorded, not implemented). Then, after "create release apk for github releases" (upload key, build only), the first upload-key Release APK was built and smoke-tested.

Introduced (explained by Claude, not yet demonstrated):
- Local versus push notifications: Duewise schedules reminders on the phone itself; push needs a server. They look the same to the user.
- Privacy by surface: Android can show a detailed notification when unlocked and a separate "public version" on the lock screen.
- Signing keys are identity: Android only updates an app signed by the same key, so the first public key choice is permanent for its users; the debug key is for development.
- Release builds strip unused code (trimming), so a debug-only test can miss a release-only crash; a smoke test on the release APK checks it.
- Secrets in logs: build output was filtered so no password could be printed; readiness was checked by counting placeholders, never by reading values.

OOP pillar links:
- `content(due, when, privacy, surface, paid)` in the lab is one pure function that every view and test uses → abstraction; the phone drawing never decides wording itself → separation of concerns.
- The planned `ReminderPaymentService` reuses the repository's guarded save instead of writing its own SQL → encapsulation and reuse through an interface (`IDueRepository`).

Learning checkpoint: explain why a reminder's Mark as paid must re-check the due before writing, why the lock screen gets a different text, and why the release key cannot change after people install the app.
