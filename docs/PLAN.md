# due-date-checker: plan and status

## v1 re-scope (2026-10-10, approved)

v1 = the simplest foundation: **local CRUD of dues on the phone**. No login, no Supabase, no reminders yet.
Learner's answers: no login in v1; dues live in SQLite only; Supabase deferred.

| Slice | User-visible behavior | Exit test on the phone |
|---|---|---|
| V1.0 | Toolchain finish (S-1 remainder below) | Hello-world Release APK launches from the launcher |
| V1.1 | Add a due (title, amount, date); see it in a list sorted by date | Add two, kill app, relaunch → both listed |
| V1.2 | Edit a due, toggle paid | Edits persist after relaunch |
| V1.3 | Delete (soft) with confirm | Gone from list; row kept with `deleted_at` |
| V1.4 (opt.) | CI runs tests on push | Green check |

Design proposal (confirmed/changed at checkpoints): `Money` (long centavos, AwayFromZero), `Due` record (Guid id, title, amount, DateOnly due date, notes, is_paid, created/updated/deleted timestamps), `IDueRepository` + `SqliteDueRepository` (Microsoft.Data.Sqlite, raw SQL, `PRAGMA user_version` migrations, embedded `001_create_dues.sql`), two MAUI pages with CommunityToolkit.Mvvm view models, xUnit tests for Money, Due, repository (temp-file DB). Full plan text lives in the learner's Claude plan file; this table is the resume point.

The original S0–S3 schedule below is superseded by v1; S1–S3 (reminders) and login/sync come after v1 on the same schema.

Offline-first Android app (.NET MAUI) that tracks dues and reminds the user before a due date with
local scheduled notifications. Users: students and employees in the Philippines (PHP, Asia/Manila).
Core promise: reminders fire with no internet; the in-app Overdue list is the fallback.

## Status (updated 2026-10-07, day 2 of 7)

Done on the laptop:
- Git repo (branch `main`), `.gitignore`, `DueDateChecker.slnx`
- `src/DueDateChecker.Core` (net10.0 class library, nullable on, no packages)
- `tests/DueDateChecker.Core.Tests` (xUnit 2.9.3, references Core), `SmokeTests.cs` → `dotnet test` = 1 passed
- Learner notes in `.vibe-wise/` (profile, progress, project map) are committed so learn mode resumes on any machine

Not done (S-1 remainder), in order:
1. Install Visual Studio Community 2026 with the .NET MAUI workload. From an **admin** PowerShell:
   `winget install --id Microsoft.VisualStudio.Community --source winget --override "--add Microsoft.VisualStudio.Workload.NetCrossPlat --includeRecommended --passive --norestart --installPath D:\VisualStudio\2026"`
   Done when a new terminal's `dotnet workload list` shows `maui-android` / `maui`. Verify current docs if the workload name differs.
2. Generate the signing keystore (keytool ships with the JDK the workload installs; find it under `C:\Program Files\Microsoft\jdk-*\bin` or wherever VS placed it):
   `keytool -genkeypair -v -keystore <keys folder outside the repo>\duedatechecker-upload.jks -alias upload -keyalg RSA -keysize 2048 -validity 10000`
   Keep both passwords; back the `.jks` up outside the machine. Never commit it.
3. Claude: create `src/DueDateChecker.App` (`dotnet new maui`), trim to `net10.0-android` only, `SupportedOSPlatformVersion` 26, reference Core, add CommunityToolkit.Mvvm.
4. Claude: `AndroidManifest.xml` → `android:allowBackup="false"`.
5. Claude: `local.signing.props` (gitignored) imported by `App.csproj` when present; `dotnet publish -f net10.0-android -c Release`; `adb install`; launch from the phone's launcher.
6. Claude: fix the stale `ANDROID_SDK_ROOT` user env var (currently points to a missing folder) to the SDK VS installed.
7. Commit "S-1: walking skeleton".

S-1 acceptance criteria: AC1 `dotnet test` passes (met); AC2 App builds for net10.0-android; AC3 signed Release APK installs and launches from the launcher; AC4 first commit, clean status, secrets ignored; AC5 merged manifest shows `allowBackup="false"`.

## Decisions made (2026-10-06)

| Decision | Choice | Learner's reason |
|---|---|---|
| Solution layout | 3 projects: Core (Domain/ + Data/), App (MAUI Android-only), Core.Tests (xUnit) | chosen from a comparison; reason not stated |
| SQLite access | `Microsoft.Data.Sqlite` + raw SQL | structure it myself and understand every query first-hand; Claude flags a better-performing query whenever one exists |
| MVVM | CommunityToolkit.Mvvm (revised from hand-written INotifyPropertyChanged) | reason for the switch still to be written as an ADR line |
| Toolchain | Visual Studio Community + MAUI workload | complete package; what C# teams the learner knows use; accepted cost: heavy install |
| Day-7 deliverable | S0–S3 local-only, sideloaded Release APK; Supabase sync after the window | timeline |

Open decisions (tradeoffs below): notification mechanism (raw AlarmManager vs Plugin.LocalNotification), exact vs inexact alarms, next-reminder-only vs all reminders scheduled (all S1); Play App Signing vs self-managed key (before S12); HttpClient vs supabase-csharp (S4).

## 7-day schedule (2026-10-06 → 2026-10-12, ~6–8 h/day)

| Day | Work | Exit test on the real phone |
|---|---|---|
| 1 | S-1 toolchain + repo + signed Release hello-world | APK installs and launches from a file |
| 2 | S0: domain lib + tests, SQLite with migrations, one screen writes a due; CI (tests on push, APK on manual trigger) | Add a due → kill app → relaunch → still there, in Release; CI green |
| 3–4 | S1: permission, channel, one alarm → notification, tap deep-links to the due, "test reminder in 60 s" button | Airplane mode, screen off, app swiped away → notification; tap opens the due; doze test via `adb shell dumpsys deviceidle force-idle` |
| 5 | S2: BOOT_COMPLETED + MY_PACKAGE_REPLACED receivers, reconcile on launch | Reboot → fires; update → fires; force-stop → open app → fires |
| 6 | S3: mark paid cancels, Overdue list from DB + TimeProvider, permission-denied banner | Notifications disabled → Overdue list still right; banner; `adb shell dumpsys alarm` shows no alarm after mark paid |
| 7 | Buffer; re-run every exit test in Release + airplane mode | |

S-1 slipped into day 2 (installer not run on day 1); the buffer absorbs it.

## Facts verified 2026-10-06

- Google Play personal accounts (created after 2023-11-13) need a closed test with 12 testers for 14 continuous days before production: no Play release inside the window; deliverable is a sideloaded APK.
- Play requires targetSdk 36 since 2026-08-31 → .NET 10 MAUI (SDK 10.0.400 is installed).
- `SCHEDULE_EXACT_ALARM` is denied by default on Android 14+ for apps targeting 13+; `USE_EXACT_ALARM` is for alarm-clock/calendar apps under Play policy. Inexact-only is the simpler path for a 9 AM bill reminder.
- supabase-csharp is community-maintained and active (v8.0.0, 2026-09-03); session persistence is still our code.

## Gaps found in the original plan (what to keep in mind)

1. Day 0 is a toolchain day; budget it.
2. Sync is a slice, not a layer, in this window. Keep UUID ids, `updated_at`, `deleted_at`, `currency`, `timezone` columns from day 0 so sync bolts on later.
3. Alarm lifecycle: force-stop cannot be survived (stopped state: no broadcasts, alarms cancelled; recover on next launch). App update may clear alarms (`MY_PACKAGE_REPLACED` receiver; verify). Doze needs `*AllowWhileIdle` variants (~1 per 9 min per app).
4. Inexact-only alarms remove the exact-alarm permission flow entirely.
5. Schedule only the next reminder per due; deterministic request codes make cancel/replace idempotent.
6. Define "app closed" in S1 acceptance (swiped from recents ≠ force-stopped).
7. "Test reminder in 60 s" button is a dev tool: S1, not S6.
8. Android Auto Backup is on by default: `allowBackup="false"` in S-1.
9. Schema migrations (`PRAGMA user_version`) from day 0.
10. Keystore never in git; decide Play App Signing before S12; Play needs an AAB, sideloading needs an APK.
11. CI: domain tests on every push (fast); APK on tag/manual (MAUI build ~10–20 min on hosted runners).
12. Test Release on the phone from S0: trimming breaks reflection-based code.
13. Notification channel importance is fixed at creation; the "reminders off" banner must also check channel importance.
14. Small: `DateOnly` stored as ISO text; fixed peso format string; `FLAG_IMMUTABLE` on PendingIntents; MainActivity is SingleTop → handle `OnCreate` and `OnNewIntent`; row-level LWW is enough for S5; ADR 006 (SQLCipher) likely "no" for MVP after a 10-line threat model.

## Open-decision tradeoffs

| Decision | Option A (fewer deps) | Option B (standard) | Weigh |
|---|---|---|---|
| Notification scheduling | Raw AlarmManager + BroadcastReceiver + NotificationManager in Platforms/Android (~100–150 lines) | Plugin.LocalNotification behind INotificationScheduler | A teaches the mechanics and removes a dependency with Android-version bugs; B faster on day 1 but its receiver still has to be debugged in S2 |
| Exact vs inexact | Inexact-only (`setAndAllowWhileIdle`) | Exact + permission flow + fallback | Inexact windows ≥10 min; OEMs may defer further |
| Alarm fan-out | Next reminder per due | All reminders up front | Alarm count and idempotent reconcile |
| Supabase (S4) | HttpClient against PostgREST/GoTrue | supabase-csharp v8 | Session persistence is ours either way |

## Backlog beyond the window (learner's original S4–S13, unchanged)

S4 auth + outbox sync + RLS tests · S5 pull sync, LWW, token expiry, restore · S6 reliability onboarding · S7 recurring dues · S8 budgets · S9 savings · S10 notification polish · S11 hardening · S12 Play closed test (start recruiting testers any time) · S13 optional FCM second channel.
