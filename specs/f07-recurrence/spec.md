# F07 recurring dues (UX03)

Status: authorized 2026-11-06 ("start wave 2") under the decided [PRD UX03 rule](../../docs/PRD.md#ux-review-decisions-2026-11-06); implemented 2026-11-06 with automated and emulator evidence ([evidence](../../docs/delivery/evidence/UX-WAVE2-2026-11-06.md)); phone check pending. Product decisions: option A of the [UX decision log](../ux-improvements/spec.md#decision-log) (the next occurrence appears when the current one is marked paid; Never/Weekly/Monthly/Yearly; a missing day falls back to the month's last day) and the follow-up that **only Bill and Subscription repeat**. Technical choices are recorded in [ADR 009](../../docs/adr/009-recurring-dues.md). [Plan](plan.md), [tasks](tasks.md).

## Acceptance scenarios

| ID | Outcome |
|---|---|
| RC01 | Schema 4 upgrades to 5 by adding `repeat_rule` (0 Never, 1 Weekly, 2 Monthly, 3 Yearly; NOT NULL, default 0, CHECK), `repeat_day` (nullable, 1–31) and `repeat_source_id` (nullable) plus a partial index on the source. Existing rows read as Never and are otherwise unchanged; initialization is idempotent; a failed upgrade leaves schema 4; newer schemas stay rejected. |
| RC02 | The domain accepts a repeat only for Bill and Subscription. Monthly and Yearly carry an anchor day 1–31; Never and Weekly carry none. Undefined repeat codes are rejected on create, construction, `with` and read. |
| RC03 | Next dates: Weekly adds 7 days. Monthly moves one month to the anchor day, or the month's last day when that day does not exist (31 Jan → 28 Feb → 31 Mar). Yearly keeps the month (29 Feb 2028 → 28 Feb 2029 → 29 Feb 2032). No next date exists beyond 31 Dec 9999. |
| RC04 | Marking a repeating due paid, from Details or the editor, saves it as paid and inserts the next occurrence in one transaction: new ID, same title, amount (or no amount), type, notes, repeat and anchor, unpaid, created now, linked to its source. Details announces "Marked paid. Next one added for dd MMM yyyy." |
| RC05 | Only one next occurrence is ever created per source: marking paid again after unpaid creates nothing more, even if that next occurrence was deleted. Marking unpaid keeps the next occurrence. |
| RC06 | The editor shows "Repeat" (Never, Weekly, Monthly, Yearly; default Never) only for Bill and Subscription; switching to Task or Other resets it to Never. Changing the due date sets a new anchor; saving without changing the date keeps the stored anchor. Identity and other fields are kept. |
| RC07 | Rows on Dues, Dashboard and Calendar add the repeat to their meta line and accessible text ("07 Nov · Bill · Monthly"); Details shows a "Repeats" fact row for repeating dues. New occurrences appear in counts, totals, the Calendar and reminder planning like any due. |
| RC08 | If the transaction fails, neither the paid state nor the next occurrence is written and the error is reported; a due deleted elsewhere becomes unavailable and no occurrence is created. |

## Boundaries

- Each occurrence is an ordinary due: payment, Details, edit, delete, reminders and filters work as before. Deleting an occurrence deletes only that occurrence; if it was unpaid, no next one is created.
- Editing an occurrence changes only that occurrence. Because the next occurrence copies the occurrence that was paid, editing the current one before paying carries the change forward.
- No series editing, skipping, end dates, custom intervals or reminders per series. Export (UX05) must include the three new columns.
- Release, signing and pushing to `main` remain held.
