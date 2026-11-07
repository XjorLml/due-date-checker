# F07 optional amounts — implementation and emulator evidence (2026-11-06)

Scope: [specs/f07-optional-amounts](../../../specs/f07-optional-amounts/spec.md) OA01–OA06. All observations are agent-observed on 2026-11-06 unless stated otherwise. This is automated and emulator evidence, not phone acceptance: the phone (M2101K6G) currently holds a build signed by another computer's debug key and could not be updated from this machine (see Limits).

## Source and environment

- Base commit `d89ce9d`; the slice is an uncommitted working-tree change on `develop`.
- Windows 11, .NET SDK 10.0.403, MAUI `net10.0-android`.
- Migration file `004_optional_amounts.sql` SHA-256 `5A50C4D9AEDA7049442797431A412A584B51D91B0444C2CA92E65F6D1E19CCE3`.
- Emulator: AVD `duewise_api35_default` (Android 35 x86_64, `-gpu host`, `-no-snapshot`), `debug.hwui.drawing_enabled=1`, `auto_time 0`, device date 06 Nov 2026. Debug build installed with `-t:Install` and `AdbTarget=-s emulator-5554` (Fast Deployment, so the APK hash alone does not identify the deployed assemblies). Every adb command was pinned to `emulator-5554`; the phone was also attached and was not touched.

## Automated and build results

| Check | Result |
|---|---|
| Baseline before changes (`d89ce9d`) | 61 Core + 161 App passed |
| `dotnet test tests/DueDateChecker.Core.Tests` | 83 passed (22 new in `OptionalAmountTests`; 4 existing assertions moved from schema 3 to 4) |
| `dotnet test tests/DueDateChecker.App.Tests` | 179 passed (18 new in `OptionalAmountTests`; one `CategoryTests` line rewritten because it encoded the old "Task requires an amount" rule; four `.Amount.Centavos` reads changed to compare `Money?`) |
| Repeat runs after the pool fix below | App 10/10 and Core 5/5 runs passed |
| `dotnet build -c Debug -f net10.0-android` | 0 warnings, 0 errors |
| `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false` | 0 warnings, 0 errors on the final code (signing bypassed; the stored upload-key password is still wrong) |

New automated coverage: domain requirement per category, zero for every category, `Validate()` on a copied record; fresh schema 4; schema 3→4 upgrade preserving every row and `rowid`, column definitions, NOT NULL flags of the other columns, index SQL, list query plan, reminder settings and the category CHECK; failed upgrade leaving schema 3 untouched; NULL vs zero after reopen; clear and restore with the same Id/CreatedAt; tombstone keeps NULL; repository refusing Bill/Subscription without an amount on add and update; identical reminder plans with and without an amount; editor requirement text, placeholder and description per category; blank optional saves NULL (including whitespace); zero saves zero; blank required is a field error with no write; category switch re-evaluates only a shown error; clearing a Task amount; refusing to clear a Bill amount; NULL Task needing an amount before becoming a Bill; "No amount" on Dues, Dashboard, Details and Calendar with accessibility text; status/count/order unchanged; Dashboard total excluding and counting missing amounts.

## Real upgrade on the emulator (OA01)

The emulator's existing database (four seeded dues from the 2026-11-05 passes) was copied and dumped with the on-device `sqlite3` through `run-as` before installing the new build, then dumped again after the first launch.

- Before: `user_version` 3, `amount_centavos` `notnull` 1.
- After: `user_version` 4, `amount_centavos` `notnull` 0.
- Every other line of the dump was identical: all four rows with `rowid`, id, title, amount, currency, date, notes, paid flag, timezone, timestamps, tombstone and category; the `ix_dues_active_due_date` SQL; the `reminder_settings` row.
- `PRAGMA integrity_check` returned `ok`; no `dues_v4` table remained.

## Native checks (OA02–OA05)

Temporary records were titled "OA temp …" and deleted afterwards.

| Check | Observation |
|---|---|
| Required blank | New form (Bill) with a title and blank amount: Save stayed on "Add due"; error "Enter an amount for this Bill, for example 1250.50."; caption "Required for Bill."; field description "Amount in Philippine pesos, required for Bill" |
| Switch to optional | Choosing Task cleared the error; caption "Optional for Task. Blank means no amount; zero is a recorded amount."; preview "No amount" announced as "Will be saved with no amount" |
| Save blank Task | Row "OA temp task, Task, Due today, due 06 November 2026, No amount. Opens due details."; stored `amount_centavos` NULL |
| Zero | "OA temp zero" (Other, 0): preview ₱0.00; row ₱0.00; stored 0 |
| Clear an amount | "OA temp clear" (Task, 150) edited to blank: Details showed "No amount"; stored NULL; `created_at` unchanged, `updated_at` moved |
| Restart | After force-stop and relaunch, Dues, Calendar agenda (06 Nov) and Details kept "No amount" / ₱0.00 |
| Dashboard | Total ₱1,699.00 with "4 unpaid · today to 30 Nov · 2 without an amount, excluded"; counters 2 overdue, 3 today, 1/7 paid |
| Themes and text size | Dues, Details and editor in dark; Dues and editor at 200% text |
| Cleanup | All three temporary dues soft-deleted through the editor; tombstones keep NULL, 0, NULL; the four seeded dues unchanged; emulator font scale 1.0 and light mode restored |

Screenshots `oa-01` to `oa-11` are in the session scratchpad `shots` folder (not committed).

## Defects found and handled

1. **Optional placeholder cut off at 200% text.** "Leave blank for no amount" was cut to "Leave blank for no amoun" in the single-line field. Shortened to "Optional" (the caption below still explains blank vs zero) and re-verified at 200%.
2. **Flaky App test (pre-existing, made more frequent by the new test classes).** `DeleteDueTests.Unknown_or_deleted_targets_cannot_delete(deleted: True)` failed in 2 of 3 full runs with `ObjectDisposedException` in SQLitePCL. Cause: many test classes called the process-wide `SqliteConnection.ClearAllPools()` while xUnit ran other classes in parallel. Replaced with `TestDatabase.ClearPool(path)` / `ClearPoolsUnder(directory)` in both test projects; 10 consecutive App runs and 5 Core runs then passed. The app never calls `ClearAllPools`, so this was a test-only problem.

## Observed, not changed in this slice

- **Keyboard covers the fixed Cancel/Save footer.** With the amount field focused, `mInputShown=true` and a tap at the Save button's reported bounds hit the keyboard; Save worked after closing the keyboard. This is the keyboard-overlap item already listed under [Focus adoption remaining acceptance](../../../specs/focus-adoption/tasks.md).
- **Details shows "No amount" in the HeroAmount style.** DESIGN only requires the literal; whether an absent amount should look quieter is recorded as an open design question in [DESIGN](../../DESIGN.md#f07-optional-amount-adaptation).
- At 200% text the bottom-bar labels truncate ("Dash…", "Calen…", "Settin…"); not caused by this slice.

## Limits

- No phone evidence. The phone's installed app is signed by the user's laptop debug key (certificate SHA-256 `3AE0B536…63A1C1` per the [2026-11-04 retest](F05-RETEST-2026-11-04.md)); this machine's debug key differs, so a phone update from here would require an uninstall and lose the user's dues. A Fast Deployment attempt from this machine earlier on 2026-11-06 ran `pm uninstall -k` (data kept) before MIUI refused the reinstall; the app is to be restored from the laptop.
- TalkBack was not run (deferred by the user; the emulator images used here have no TalkBack).
- Controlled native migration/read/write failures, other devices/widths and intended-key acceptance remain open in the [tasks](../../../specs/f07-optional-amounts/tasks.md).
