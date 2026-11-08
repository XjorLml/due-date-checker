# F07 optional amounts

Authorized by the user's F07-extension direction and the 2026-11-03 product decisions: Bill and Subscription keep a required amount, while Task and Other may omit it; blank is distinct from zero and displays as "No amount". This is a separate F07 slice. It does not authorize recurrence, timed dues, per-due reminders, amount totals or statistics. [PRD](../../docs/PRD.md#f07-optional-amount-rules) owns policy, [SDD](../../docs/SDD.md#f07-optional-amount-contract) owns storage/contracts and [DESIGN](../../docs/DESIGN.md#f07-optional-amount-adaptation) owns presentation. [Plan](plan.md) and [tasks](tasks.md) track delivery.

## Acceptance scenarios

| ID | Outcome |
|---|---|
| OA01 | Upgrade schema 3 to 4 without changing existing IDs, titles, amounts (including zero), categories, notes, payment, currency/timezone, timestamps or tombstones. Existing rows remain readable; the active-date index is recreated. Fresh databases reach schema 4; initialization is idempotent and newer versions remain rejected. |
| OA02 | Bill and Subscription require a nonnegative PHP amount. Task and Other may be saved without one. Blank/whitespace amount becomes SQL NULL, while explicit zero remains a valid amount. Invalid syntax, negatives and overflow are rejected with no write; blank on a required category is a field error. |
| OA03 | The editor shows category-aware amount requirement text, placeholder and semantics. A blank Task/Other saves NULL; a blank Bill/Subscription does not save. Changing category updates the requirement and clears a blank-only error when the new category permits no amount. Existing Task/Other amounts can be cleared; existing Bill/Subscription amounts cannot. Save/Cancel/Back, busy/completed/unavailable and storage-failure guards are preserved. |
| OA04 | Dues, Dashboard, Details and Calendar agenda show the formatted amount when present and literal "No amount" when absent. Accessibility row text includes "No amount". Present amounts retain the existing `₱N2` formatting. Status, date order, counts/progress, filtering and reminder eligibility are unchanged. |
| OA05 | Reopening the database distinguishes NULL from zero. Editing NULL to an amount and an amount to NULL round-trips with the same ID/CreatedAt; soft deletion preserves the absent amount. Legacy amount values are not rewritten. |
| OA06 | Automated Core/App coverage exercises domain, migration, repository, editor and projection behavior. The Android Release build passes. Targeted native checks distinguish Task/Other blank, Bill/Subscription required, zero, edit-clearing and restart; broader accessibility/device/ intended-key acceptance remains separate. |

## Decisions and boundaries

- Amount is required for `Bill` and `Subscription`; it is optional for `Task` and `Other`. The editor starts at Bill, so a new form still requires an amount until the user selects an optional category.
- Blank is stored as SQL NULL, not zero. Zero remains an explicit amount for every category and displays as `₱0.00`; "No amount" is never used for zero.
- The existing amount grammar, centavo rounding, non-negative rule and overflow rejection remain unchanged for entered values. Leading/trailing whitespace is ignored; a blank value is not parsed as zero.
- The category/amount requirement is enforced in `Due.Create`, `Due.Validate()` and the editor, and the repository calls `Validate()` before writing. SQLite stores NULL/values but does not add a category-dependent CHECK constraint; this matches the existing application-enforced integrity model.
- SQLite cannot drop the existing `NOT NULL` constraint in place. Migration 004 rebuilds `dues` while preserving all stored columns and recreates `ix_dues_active_due_date`. It is the only schema change in this slice.
- No amount totals, statistics, budgets or inferred amounts are introduced. A future amount-based feature must define how absent amounts are excluded or represented.
- Notifications remain generic and never include amounts. The amount requirement does not affect reminder scheduling.
- Recurrence and timed dues remain separate unimplemented slices. Release/signing and optional CI remain held.
