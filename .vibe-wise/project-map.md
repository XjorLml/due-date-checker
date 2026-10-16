# Project Map

> Historical learning/project record, preserved during the 2026-10-17 documentation refactor. For current scope, contracts, and implementation/verification state use the [documentation index](../docs/README.md), [PRD](../docs/PRD.md), [SDD](../docs/SDD.md), and [STATUS](../docs/delivery/STATUS.md). Dated "current", "not implemented", and next-stage statements below retain their original context; they are not a competing source of current status.

## Purpose
Offline-first Android app (.NET MAUI) for tracking dues. Users: students and employees in the Philippines; PHP and Asia/Manila only in v1. Local reminders, an Overdue fallback, budgets, and savings belong to later releases.

## v1 scope (approved 2026-10-10)
Local CRUD of dues on the phone: create, list (by due date), edit (incl. paid flag), soft delete; data survives restarts. No login, no Supabase, no reminders in v1. Slices V1.0 (toolchain) → V1.1 create+list → V1.2 edit → V1.3 delete → V1.4 optional CI.

## Current implementation (2026-10-17)

- V1.1 create/list is implemented: Pages/DuesListPage and DueEditPage (create path), ViewModels/, Services/DatabaseDispatcher, DI in MauiProgram, Shell navigation, and initialization of FileSystem.AppDataDirectory/dues.db before data access. Template MainPage removed. List refreshes on return/resume; errors allow retry without resetting data.
- Domain/Data remain in Core. DatabaseInitializer additionally blocks unsupported schema versions. No schema migration was needed for the screens.
- V1.2 is implemented: select a row, pass its id to DueEditPage, load through the repository, edit fields, and save paid/unpaid status. The App checks for tombstones on load/save; UpdateAsync also rejects deleted rows and preserves CreatedAt/DeletedAt.
- V1.3 is implemented: editor-only Delete action, native confirmation naming the stored due, no-write cancellation, soft deletion through the dispatcher, and list refresh on return. Pending writes block duplicate actions; navigation retries do not repeat deletion. A reusable destructive button style uses the design's semantic colors and dimensions.
- tests/DueDateChecker.App.Tests covers validation, dates, local CRUD persistence, retries, cancellation, and duplicate actions. Current totals: 28 Core and 42 App cases passing.
- Agent phone checks verified V1.3 confirmation, Cancel, confirmed deletion, and hidden state after process restart using only temporary verification dues. V1.2 functional checks remain user-accepted. Intended-key signing and unchecked phone cases remain tracked; optional CI is not implemented.
- [docs/SDD.md](../docs/SDD.md) records contracts. [V1.1](../docs/phases/V1.1.md), [V1.2](../docs/phases/V1.2.md), and [V1.3](../docs/phases/V1.3.md) record completion/acceptance. Mandatory local CRUD implementation is complete; next is release acceptance/signing and optional V1.4 CI.

## Later-release requirements and original process notes
From the learner's original plan, retained for later slices. Reminder/settings behavior and the original seven-day schedule are superseded for v1 by the local CRUD scope above:
- Reminders per due at 9:00 AM Asia/Manila (adjustable): 3 days before, 1 day before, due day; one overdue follow-up next morning, then stop.
- Notification title = what and when; body = amount and date in PHP format. Tap opens that due's detail screen.
- Mark paid cancels pending reminders; editing the date reschedules.
- Request POST_NOTIFICATIONS on first due with a one-line reason; if denied, banner on dues list.
- Settings (small): reminder time, days-before toggles, hide amount on lock screen, send test reminder.
- Domain rules: Money as integer centavos value object (MidpointRounding.AwayFromZero); DueDate = DateOnly; reminder = date + local time converted to an instant only at scheduling; injected TimeProvider, never DateTime.Now; soft deletes; paid dues cancel reminders.
- Non-goals (MVP): bank linking, multi-currency, holidays, iOS, web, Realtime, notification action buttons, AI.
- Process: vertical slices through all layers; spec → failing test → minimal implementation → one reviewed PR; Definition of Done includes CI green, Release build on a real phone, airplane-mode test, per-slice security checklist, ADR update when a decision changes.
- Timeline: 7 days from 2026-10-06; day-7 deliverable = S0–S3 local-only, sideloaded Release APK.

## ADRs (learner-proposed; none yet written to the repo)
001 Android-first, iOS deferred. 002 PHP + Asia/Manila only; keep currency and timezone columns. 003 Local scheduled notifications for MVP; FCM optional after S5; never both channels in MVP. 004 No AI in MVP. 005 Auth tokens in SecureStorage. 006 Local DB encryption (SQLCipher) — no decision recorded yet. 007 Alarms are a disposable cache rebuilt from SQLite (reconcile job).

## Components

Implemented layout:

- `src/DueDateChecker.Core/` — net10.0, no MAUI reference. Domain/ contains Money, Due (using DateOnly), and IDueRepository. Data/ contains SqliteConnections, DatabaseInitializer, embedded numbered migrations, and SqliteDueRepository using Microsoft.Data.Sqlite 10.0.12 and parameterized raw SQL. Schema version is 1.
- `src/DueDateChecker.App/` — net10.0-android, minSdk 26, targetSdk 36, id `com.lem.duedatechecker`, Core reference, CommunityToolkit.Mvvm 8.4.2. Pages/ contains the list and create form; ViewModels/ contains the toolkit view models and display rows; Services/ contains DatabaseDispatcher. MauiProgram supplies DI and the database path; AppShell supplies navigation. Android backup is disabled; local.signing.props is imported when present.
- `tests/DueDateChecker.Core.Tests/` — xUnit; references Core. Domain and SQLite/migration behavior; 28 passing cases.
- `tests/DueDateChecker.App.Tests/` — net10.0 xUnit harness linking the platform-independent App sources and toolkit. View-model behavior, controlled failures, and file persistence; 42 passing cases.

Boundary rule (discipline, not compiler-enforced): Domain/ must not use SQLite types.

## Main Flow

Implemented V1.1–V1.3:

```text
MauiProgram --> app-private path + DI --> AppShell --> list/create/edit pages
                                                       |
                                                  view models
                                               /               \
                                      Due + Money + clock   DatabaseDispatcher
                                                                  |
                                                    initialize / IDueRepository
                                                                  |
                                                         app-private SQLite
```

The dispatcher initializes before use and serializes database work on background tasks. View models update bound state after awaiting the result. Row navigation passes only an id; the editor loads and re-checks the active record. A confirmed Delete sets the tombstone through SoftDeleteAsync; Cancel does not call storage. The list reloads after returning from the form and on resume. Notification reconciliation, Android receivers, and Supabase are deferred.

## Data and Trust Boundaries
- Storage: SQLite in app-private storage (chosen by the learner). Encryption (SQLCipher) undecided (ADR 006).
- Backup: `allowBackup=false` is configured and was confirmed in the merged Release manifest on 2026-10-16.
- Auth / server: Supabase (Postgres, Auth, RLS) chosen for later slices; tokens in SecureStorage (ADR 005, proposed). Not in the 7-day window.
- Notification content on lock screen: sensitive (amounts); visibility handling deferred to S10.

## Build and Deployment

Recorded 2026-10-17: V1.3 Android Release build passed with zero warnings/errors; 28 Core and 42 App cases passed. APK: `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`. Installed on the phone; native confirmation/Cancel/Delete/process-restart checks passed with agent-created verification dues. V1.2 functional checks were previously user-accepted. Intended-key signing and unchecked phone cases remain tracked; local.signing.props was absent at the last inspection. GitHub Actions and Play publishing are not implemented. See phase records for evidence attribution.

## Historical backlog for the seven-day window (superseded by v1)
- S-1 (day 1): toolchain (.NET 10 SDK, MAUI workload, JDK, SDK 36, USB debugging), git init + .gitignore, keystore outside git, allowBackup=false, migrations scaffold, UUID/updated_at/deleted_at/currency/timezone columns, signed Release hello-world on the phone.
- S0 (day 2): domain lib + tests; SQLite with migrations; one screen writes a due; CI = tests on push, APK on manual trigger. Exit test in Release.
- S1 (days 3–4): permission, channel, one alarm → notification, tap deep-link, "test reminder in 60 s" button; define "app closed"; doze test via adb.
- S2 (day 5): BOOT_COMPLETED + MY_PACKAGE_REPLACED receivers; reconcile on launch; "recovers after force-stop on next launch".
- S3 (day 6): mark paid cancels; Overdue list from DB + TimeProvider; permission-denied banner (checks channel importance too); 10-line threat model.
- Day 7: buffer; re-run all exit tests in Release + airplane mode.
- S4+ unchanged from the learner's backlog, outside the window.

## Deferred decisions

SQLite access (Microsoft.Data.Sqlite + raw SQL), MVVM (CommunityToolkit.Mvvm), and the production layout are decided and implemented. Decisions still open for later slices:

- Notification scheduling: raw AlarmManager/BroadcastReceiver vs Plugin.LocalNotification behind INotificationScheduler (S1).
- Exact vs inexact alarms (S1). Alarm fan-out: next reminder per due vs all (S1).
- Signing: Play App Signing vs self-managed upload key (before S12).
- Supabase access: HttpClient/PostgREST vs supabase-csharp (S4).
Revalidate notification/platform/store requirements when those slices begin. The current raw-SQL repository maps DateOnly explicitly as ISO text; sqlite-net mapping is not part of this design.
