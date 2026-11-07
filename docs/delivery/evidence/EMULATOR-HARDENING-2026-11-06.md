# Remaining non-phone checks — emulator and automated evidence (2026-11-06)

Scope: the remaining work that does not need the phone, authorized on 2026-11-06 with "then do the rest" after [F07 optional amounts](F07-OPTIONAL-AMOUNTS-2026-11-06.md). All observations are agent-observed on 2026-11-06. This is emulator and automated evidence; it does not replace phone, TalkBack or intended-key acceptance.

## Environment

Same as the [optional-amount evidence](F07-OPTIONAL-AMOUNTS-2026-11-06.md#source-and-environment): base `d89ce9d` plus the uncommitted working tree, Windows 11, .NET SDK 10.0.403, AVD `duewise_api35_default` (Android 35 `userdebug`, x86_64), Debug Fast Deployment to `emulator-5554` only. Display size, density, rotation, font scale, night mode and the clock were changed for individual checks and restored afterwards (1080×2400, 420 dpi, rotation free, font 1.0, light, host UTC time; `adb unroot`).

## Final automated and build results

| Check | Result |
|---|---|
| `dotnet test tests/DueDateChecker.Core.Tests` | 85 passed |
| `dotnet test tests/DueDateChecker.App.Tests` | 181 passed; 5 further repeat runs passed |
| `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false` | 0 warnings, 0 errors (signing bypassed) |

## Defects found and fixed

| # | Defect | Fix | Evidence |
|---|---|---|---|
| 1 | **Keyboard covered Cancel/Save.** With any editor field focused, the on-screen keyboard hid the fixed footer (screenshot `kb-03`); a tap on Save landed on the keyboard. The selected interaction design reserves space for actions instead. | `DueEditPage` switches the window to `Resize` soft-input mode while it is visible (back to `Pan` when it disappears) and its root grid uses `SafeAreaEdges="None,None,None,SoftInput"`. On the Android 35 emulator neither change alone moved the footer; together they do. | Footer above the numeric keyboard (`kb-05`) and the text keyboard; focusing Notes scrolls it into view above the footer (`kb-07`); tapping Save with the keyboard open saved the due once. Dashboard search still pans, so the tab bar is not lifted onto the keyboard (`kb-08`). |
| 2 | **Add opened during a failed startup.** With an unreadable database, the bottom-bar Add disc (added by the Focus adoption) opened the editor because it bypassed the list page's `CanAdd` guard; Save then failed safely. This broke the SDD startup contract ("retry must complete initialization before Add becomes available"). | `DatabaseDispatcher.TryInitializeAsync()` (never throws) and `AppShell.OpenAddAsync` call it first: on failure an alert "Couldn't open your saved dues" appears and the editor does not open; on success Add proceeds, so Add also retries initialization. | New App test `TryInitialize_reports_failure_without_throwing_and_succeeds_once_fixed`; emulator: alert shown and no editor with a corrupt file (`fail-07`); Add opens normally once the database is healthy. |
| 3 | **Dashboard total overflow.** Two dues at the largest accepted amount (`long.MaxValue` centavos each) made `UpcomingTotalText` throw `OverflowException`. | The display total is summed in `decimal` and formatted with the new `Money.Format(decimal)`, which `Money.ToString()` also uses, so the peso format still has one home. | New App test `Upcoming_total_beyond_the_largest_single_amount_does_not_throw`; emulator Dashboard showed ₱184,467,440,737,097,215.14 with two maximum dues (`max-02`). |

Also fixed during the optional-amount slice: the flaky `ClearAllPools` test cleanup and the cut-off optional placeholder (see that evidence).

## Known automated gaps closed

- **Equal due dates:** `SqliteDueRepositoryTests.Equal_due_dates_list_in_creation_order_regardless_of_insert_order` (passed without code changes; App writes store UTC, so text order matches time order).
- **Mid-script migration rollback:** `OptionalAmountTests.Failure_after_earlier_statements_rolls_back_the_whole_script` plants a row that breaks the category CHECK so migration 004 fails after `CREATE TABLE dues_v4` has succeeded; version, rows and the whole `sqlite_master` are unchanged afterwards. While writing it: PRAGMAs belong to a connection and pooled connections are reused, so a test must switch `ignore_check_constraints` off again before the initializer borrows the same connection.

## Controlled failures, empty state and repeated taps

The database was backed up first (pulled to the scratchpad and copied on the device) and restored after each check.

| Scenario | Method | Observation |
|---|---|---|
| Empty database | Real file moved aside | Dues "No dues yet. Tap Add in the bottom bar to add a bill, subscription or task."; Dashboard ₱0.00, "0 unpaid", "No unpaid dues coming up.", 0/0; Calendar all zero counts; new file created at schema 4 |
| Unreadable database | File replaced with text | Dues "Couldn't open your saved dues. Check available storage or update the app, then try again." with Refresh / Retry; Add blocked after fix 2 |
| Retry after the file is repaired | Real file put back while the app stayed open, then Retry | Still failed until the process restarted, then all rows returned. Cause: the connection pool kept a handle to the deleted bad file. A genuinely corrupt file cannot be repaired by Retry either, so this is recorded as a limit, not changed |
| Write failure | App folder made read-only (`chmod 555`) so SQLite cannot create its journal; restored to 777 | Save showed "Couldn't save this due. Your entries are still here. Check available storage and try again." with input kept; after restoring, Save stored exactly one row |
| Repeated taps | Three concurrent `input tap` on Save | One row stored |

## Clock boundary (Manila midnight)

`adb root` on the emulator only; device clock set to 15:59:20 UTC (23:59:20 Asia/Manila) and left untouched for about 105 s.

- **Dashboard:** without any interaction the header changed from "Friday, 06 Nov 2026" to "Saturday, 07 Nov 2026"; total ₱1,711.00 → ₱1,699.00, "3 unpaid" → "1 unpaid", "Next date · 07 Nov" (DB02 visible-minute timer).
- **Calendar:** crossing into 08 Nov kept 07 Nov selected, moved the today marker to 08 Nov, changed the Today button to 08 Nov and the agenda row for Internet from "Due today" to "Overdue" (CA03).

## Screen sizes, landscape and maximum amounts

Display overrides with `wm size` / `wm density` (rotation through the window manager did not change this image's app frame, so landscape used a 2400×1080 override).

- **320 × 568 dp:** Dues (14 dp margins), Dashboard (two-line total), editor fit. Calendar keeps 44 dp day cells, so its seven columns are wider than the 292 dp content and the grid scrolls sideways (deliberate `MinimumWidthRequest="322"`); the Sunday column needs a sideways swipe. The bar caption "Dashboard" truncates to "Dashbo…" when selected. The first Dues frame after a cold start appeared before the margins were applied.
- **800 × 1280 dp (tablet):** content centred at the readable width; Calendar shows all seven columns.
- **914 × 411 dp (landscape):** pages scroll with their headers; the editor keyboard uses Android's full-screen landscape entry with Done.
- **Maximum amount (₱92,233,720,368,547,758.07):** wraps to a second line on Dues rows, Calendar agenda and Details without clipping or overlapping the badge.

## Tombstones (V1.3, emulator equivalent)

All eight records soft-deleted during these passes remain in `dues` with every field (title, amount or NULL, currency, date, notes, paid flag, timezone, category, `created_at`) and `deleted_at = updated_at`. This is emulator evidence; the V1.3 task names an authorized phone database.

## Optional CI (R08)

Read through the public GitHub REST API (no authentication; `gh` is not installed): the `CI` workflow has 9 runs, all `completed/success`, from #1 (`c0ab21c`, 2026-10-17) to #9 (`d89ce9d`, 2026-11-05, push to `develop`, [run 42019688557](https://github.com/XjorLml/due-date-checker/actions/runs/42019688557)). Run #9's job ran on `ubuntu-latest` with steps "Core tests" and "App tests" both `success` (CI01, CI04). The workflow declares `permissions: contents: read` and uses no secrets (CI03, file review). These runs tested `d89ce9d`, not today's uncommitted changes. CI02 and landing the workflow on `main` were not attempted (they need a push).

## Limits

- No phone checks; the keyboard fix in particular is verified only on Android 35. Android only enforces edge-to-edge from Android 15, and the phone runs Android 13, so the window may resize there without the safe-area padding; whether the two combine cleanly (no double gap) must be checked on the phone.
- No TalkBack (not on the emulator image; deferred by the user).
- Calendar-specific and preference-specific controlled failures, navigation failures and delete-path failures were not run.
- Screenshots `kb-*`, `fail-*`, `clock-*`, `max-*`, `w320-*`, `tab800-*`, `land-*` are in the session scratchpad, not committed.
