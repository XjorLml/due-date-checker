# F07 twice-a-month repeat implementation plan

Proposed approach for [TM01–TM10](spec.md#acceptance-scenarios). Claude proposed these technical choices on 2026-11-16, and they are not yet approved contracts. At implementation the chosen design goes into the [SDD](../../docs/SDD.md#f07-recurrence-contract) and an ADR. That can be an amendment to [ADR 009](../../docs/adr/009-recurring-dues.md) or a new ADR, decided at implementation.

## Affected modules

| Area | Change | Existing code reused |
|---|---|---|
| Domain | Append `RepeatRule.TwiceMonthly = 4`. Never reorder or reuse codes. | `src/DueDateChecker.Core/Domain/RepeatRule.cs` |
| Domain | `Due` gains `RepeatDay2` (nullable). `Due.Validate()` adds the TM02 pair rule. `Due.Create` sets the days. | `src/DueDateChecker.Core/Domain/Due.cs` (`CanRepeat`, `Validate`) |
| Domain | `Recurrence.NextDate` gets a TwiceMonthly case, built from the existing clamp `OnAnchor`. The candidates are both days in the current month and the first day in the next month, clamped. The result is the smallest one after the current date, which also covers TM04 collisions. `AnchorFor` and `AnchorAfterEdit` keep the pair. `NextOccurrence` copies `RepeatDay2`. | `src/DueDateChecker.Core/Domain/Recurrence.cs` |
| Storage | Migration `007_twice_monthly.sql`. The `repeat_rule` CHECK in migration 005 allows only (0, 1, 2, 3), and SQLite cannot change a CHECK with `ALTER TABLE`. So this migration must rebuild `dues` with `repeat_rule IN (0,1,2,3,4)` and add `repeat_day_2 INTEGER CHECK (repeat_day_2 BETWEEN 1 AND 31)`. The rebuild keeps rows, `rowid`, indexes (including `ix_dues_repeat_source`) and constraints. | Rebuild pattern of `004_optional_amounts.sql`; one-transaction runner in `DatabaseInitializer` |
| Storage | `SqliteDueRepository` and `DueRecord` map `repeat_day_2`. `UpdateWithNextAsync` and its `NOT EXISTS` guard stay unchanged. | ADR 009 decisions 3–4 |
| Export | `DueArchive.CurrentVersion` 2 → 3. Write and read `repeatDay2`. Version 1–2 files read with no second day. | `src/DueDateChecker.Core/Data/DueArchive.cs`, `SqliteDueArchive.cs` |
| App | Editor: add `Repeats`, two day pickers shown for TwiceMonthly, `RepeatSummary` and `RepeatHelpText`, the snapshot (dirty check) and the save path. | `src/DueDateChecker.App/ViewModels/DueEditViewModel.cs`, `Pages/DueEditPage.xaml` |
| App | Rows and Details: `RepeatLineText`, `RepeatFactText`, and `Spoken()`. Today `Spoken()` uses `Repeat.ToString().ToLowerInvariant()`, which would say "twicemonthly", so it needs explicit text. | `src/DueDateChecker.App/ViewModels/DueListItem.cs` |
| Reminders | `ReminderText` repeat phrase. Today it also uses `ToString()`, so it needs explicit text. The record hash for stale-action protection must include `RepeatDay2`. | `src/DueDateChecker.Core/Domain/ReminderText.cs` lines 15 and 31–32 |

## Steps

1. Write failing Core tests: the migration (TM01), validation (TM02), the next-date tables (TM03–TM05), the repository round trip and the unchanged guard (TM06), and the archive round trip and old versions (TM09).
2. Core implementation, in the order of the table above.
3. Write failing App tests: the editor pickers, defaults, equal-day error, reset and dirty check (TM07); Details, the editor and reminder payment creating one next occurrence (TM06); the display and spoken text (TM08); no conversion (TM10).
4. App implementation.
5. Run the Core and App suites, the Debug build and a Release compile with signing bypassed. On the emulator: create a Bill on the 30th of a month with days 15/30, mark it paid through February, then toggle unpaid/paid with no duplicate. Check the rows, Details, a reminder, export/import, dark mode and 200% text. Clean up the test rows.
6. Update the SDD (schema 7, contract), DESIGN (editor day pickers, repeat text), TEST-PLAN, the ADR, STATUS, the evidence record and [tasks](tasks.md).

## Dependencies and risks

- The table rebuild is the riskiest step. It needs the same failure and rollback tests as migration 004.
- Changing the reminder record hash makes older notification payment actions stale. That is the intended N2 behavior, so it needs a test, not a workaround.
