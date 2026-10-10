# Project Map

## Purpose
Offline-first Android app (.NET MAUI) that tracks dues (later budgets and savings) and reminds the user before a due date with local scheduled notifications. Core promise: reminders fire with no internet; an in-app Overdue list is the fallback when a notification fails. Users: students and employees in the Philippines; PHP and Asia/Manila only in v1.

## v1 scope (approved 2026-10-10)
Local CRUD of dues on the phone: create, list (by due date), edit (incl. paid flag), soft delete; data survives restarts. No login, no Supabase, no reminders in v1. Slices V1.0 (toolchain) → V1.1 create+list → V1.2 edit → V1.3 delete → V1.4 optional CI. Design is proposed, not yet confirmed or implemented.

## Requirements
From the learner's plan (product requirements; they do not settle technical choices):
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
None implemented. Project folder is empty; no Git repository.
Chosen layout (learner, 2026-10-06; not created yet):
- `src/DueDateChecker.Core/` — .NET 10 class library, no MAUI reference. `Domain/` (Money, DueDate, Due, reminder rules, INotificationScheduler, IDueRepository), `Data/` (SQLite repository and migrations using Microsoft.Data.Sqlite + raw SQL; chosen 2026-10-06, not implemented).
- `src/DueDateChecker.App/` — MAUI, Android only; references Core. `Platforms/Android/` holds the AlarmManager scheduler and BroadcastReceivers. View models use CommunityToolkit.Mvvm (chosen 2026-10-06 after revising from hand-written INotifyPropertyChanged; not implemented).
- `tests/DueDateChecker.Core.Tests/` — xUnit; references Core only.
Boundary rule (discipline, not compiler-enforced): Domain/ must not use SQLite types.

## Main Flow
Proposed by the learner's plan, not implemented:

  UI (MAUI page) --> domain (rules, Money, DueDate, TimeProvider) --> SQLite (source of truth)
                                                                         |
                                                                 reconcile(desired reminders)
                                                                         v
                                                            AlarmManager alarms (disposable cache)
                                                                         v
                                                            BroadcastReceiver --> NotificationManager --> tap --> due detail
  (sync) --> Supabase: deferred past day 7; data model prepared with UUID ids, updated_at, deleted_at. [proposed]

Alarm lifecycle events the reconcile job must handle: launch, BOOT_COMPLETED, MY_PACKAGE_REPLACED (verify), optional TIME/TIMEZONE_CHANGED. Force-stop cannot be survived; recovery is on next launch.

## Data and Trust Boundaries
- Storage: SQLite in app-private storage (chosen by the learner). Encryption (SQLCipher) undecided (ADR 006).
- Backup: Android Auto Backup to be disabled in S-1 (proposed).
- Auth / server: Supabase (Postgres, Auth, RLS) chosen for later slices; tokens in SecureStorage (ADR 005, proposed). Not in the 7-day window.
- Notification content on lock screen: sensitive (amounts); visibility handling deferred to S10.

## Build and Deployment
Nothing verified in the repository yet. Targets (from today's verification): .NET 10 MAUI, targetSdk 36, Windows laptop + real Android phone, GitHub Actions. Play publishing is outside the 7-day window (12-tester / 14-day rule).

## Backlog for the 7-day window (revised with the learner's approval of the review)
- S-1 (day 1): toolchain (.NET 10 SDK, MAUI workload, JDK, SDK 36, USB debugging), git init + .gitignore, keystore outside git, allowBackup=false, migrations scaffold, UUID/updated_at/deleted_at/currency/timezone columns, signed Release hello-world on the phone.
- S0 (day 2): domain lib + tests; SQLite with migrations; one screen writes a due; CI = tests on push, APK on manual trigger. Exit test in Release.
- S1 (days 3–4): permission, channel, one alarm → notification, tap deep-link, "test reminder in 60 s" button; define "app closed"; doze test via adb.
- S2 (day 5): BOOT_COMPLETED + MY_PACKAGE_REPLACED receivers; reconcile on launch; "recovers after force-stop on next launch".
- S3 (day 6): mark paid cancels; Overdue list from DB + TimeProvider; permission-denied banner (checks channel importance too); 10-line threat model.
- Day 7: buffer; re-run all exit tests in Release + airplane mode.
- S4+ unchanged from the learner's backlog, outside the window.

## Unknowns
Open decisions (learner's to make; tradeoffs recorded in the approved plan file):
- SQLite access: Microsoft.Data.Sqlite + raw SQL vs sqlite-net-pcl (S0).
- MVVM: hand-written INotifyPropertyChanged vs CommunityToolkit.Mvvm (S0).
- Solution/project layout (S-1).
- Notification scheduling: raw AlarmManager/BroadcastReceiver vs Plugin.LocalNotification behind INotificationScheduler (S1).
- Exact vs inexact alarms (S1). Alarm fan-out: next reminder per due vs all (S1).
- Signing: Play App Signing vs self-managed upload key (before S12).
- Supabase access: HttpClient/PostgREST vs supabase-csharp (S4).
To verify when relevant: MAUI InvariantGlobalization default; sqlite-net DateOnly mapping; alarm survival across app update; which VS / VS Code version carries the .NET 10 MAUI workload.
