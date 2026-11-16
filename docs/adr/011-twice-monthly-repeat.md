# ADR 011: Twice-a-month repeat as a fifth rule with a second day column

Status: accepted for the PRD TM slice. Decision date: 2026-11-16.

## Context and provenance

On 2026-11-16 the user decided a "Twice a month" repeat for Philippine pay cutoffs: the user picks two days, 15 and 30 by default, and the scope is authorized for implementation ([PRD TM](../PRD.md#twice-a-month-repeat-2026-11-16)). The remaining product rules were proposed by Claude and are listed for review in the [spec's provenance](../../specs/f07-twice-monthly/spec.md#provenance). The storage choices below were proposed by Claude while implementing. Their rationale is recorded here for the learner's review and has not been separately approved. [ADR 009](009-recurring-dues.md) stays in force: one rule per row, option A, a source link with a duplicate guard, and one transaction.

## Decision

1. **A fifth rule, not a second series.** `RepeatRule.TwiceMonthly = 4` is appended, and each occurrence is still an ordinary due created on payment.
2. **Reuse `repeat_day` and add `repeat_day_2`.** The earlier day stays in `repeat_day` and the later one goes in a new nullable `repeat_day_2` (1–31). The domain requires first < second, so equal pairs compare equal.
3. **Rebuild the table (migration 007).** Migration 005 declared `CHECK (repeat_rule IN (0, 1, 2, 3))`, and SQLite cannot change a CHECK in place. Migration 007 therefore uses the rebuild pattern of migration 004: create `dues_v7`, copy every column plus `rowid`, drop the old table, rename, and recreate both indexes, all in the initializer's one transaction.
4. **Next date from both days.** The next date is the earliest of the two days (clamped to the month) strictly after the current date, so the date the user entered may be off schedule and short months never drift.
5. **Export format 3.** It adds a required `repeatDay2`; formats 1 and 2 keep their exact field sets.
6. **Fingerprint compatibility.** `ReminderText.Fingerprint` appends the second day only when it exists, so upgrading does not make existing notification actions stale.

## Rationale

- Two days fit the existing one-row-per-occurrence model, so Details, edit, delete, reminders, filters and the ADR 009 guard need no change.
- Reusing `repeat_day` keeps Monthly/Yearly rows untouched and keeps the anchor logic where it is. A separate pair of columns would leave `repeat_day` meaning different things for different rules anyway.
- Dropping the CHECK instead of rebuilding would make the database accept any code and would contradict the RC01 contract. Leaving code 4 outside the CHECK is impossible.
- Making the fingerprint change conditional avoids a surprise "Open Duewise to check this due" on every reminder delivered before the upgrade.

## Alternatives considered

- **Two linked Monthly dues per series.** This needs no schema change, but it doubles every bill in lists and totals, and option A would need two sources per payment. Rejected.
- **A list or bit mask of days.** It would be more general (three or more days), but that scope is excluded by the PRD, and it is harder to validate and export. Rejected.
- **Every 14 days.** That would be a Weekly variant, but it drifts away from fixed cutoff days. Excluded by the PRD.
- **Bump the fingerprint for every due.** Simpler, but it would make every already-delivered payment action stale. Rejected.

## Consequences

- Schema version 7. Older app builds refuse the upgraded database as newer (existing rule) and refuse format-3 files as newer.
- The rebuild copies every row once during the upgrade; failure leaves schema 6.
- Adding another day or interval later needs a new migration and ADR.

Links: [spec TM01–TM10](../../specs/f07-twice-monthly/spec.md), [SDD](../SDD.md#twice-a-month-repeat-contract), [evidence](../delivery/evidence/TWICE-MONTHLY-2026-11-16.md).
