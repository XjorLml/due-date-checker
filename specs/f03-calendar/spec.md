# F03 - saved-data Calendar and agenda

Authorized by the user's "implement f03" request on 2026-10-18. [PRD rules](../../docs/PRD.md#f03-calendar-rules) own behavior; [SDD](../../docs/SDD.md#f03-calendar-data-and-lifecycle) owns contracts and [DESIGN](../../docs/DESIGN.md#scope-and-authority) owns presentation. [Plan](plan.md) and [tasks](tasks.md) track delivery. R01-R08 and F01/F02 remain intact; release stays held.

## Scope

Replace the last sample tab with active local dues, numeric day counts, selected-date agenda, real Manila Today, month navigation, Add and selected-ID Details/editor navigation. Reuse F02's pure status classifier and saved-item projection. No categories, recurrence, reminders, stored preferences, timezone settings, new query API, migration or release. The Sunday-first prototype differs from the existing Monday-first grid: this slice deliberately retains Monday as a fixed default; a configurable preference belongs to F04.

## Acceptance scenarios

Subsequent extension: [F04 preferences](../f04-preferences/spec.md) now adds a saved Sunday-first option while retaining Monday as default. Its PF04 scenarios extend CA02's original layout contract; original F03 evidence remains artifact-specific history. Current behavior is authoritative in the linked PRD.

| ID | Observable outcome |
|---|---|
| CA01 | Calendar reads saved non-deleted dues, includes Paid, and shows each day's exact total. The selected day's agenda contains precisely those IDs, ordered by creation time for that date. No fixture/sample labels or data remain. Calendar reads and selections never write. |
| CA02 | Start on today's Asia/Manila date/month. Use Monday-first 42-cell grids; blank padding is not selectable. Month arrows cross years/leap years, select day one, and stop at DateOnly bounds. Today re-reads the clock and selects today's date/month. Reject out-of-month selections and invalid month steps. |
| CA03 | Agenda uses F02 Overdue/Due today/Upcoming/Paid rules. Visible-minute, appearance/resume and read-completion refresh update today's marker/statuses without moving the user's selected date/month. Today alone deliberately jumps the selection. No exact-midnight/background promise. |
| CA04 | First loading, initial failure/retry, no active dues and no dues on selected day are distinct. A failed refresh keeps the last successful counts/agenda with a stale warning; retry replaces them without duplicates. Initialization failure blocks page navigation; overlapping reads do not duplicate work. |
| CA05 | Rows open ID-only F01 Details -> editor; Save/Cancel/Back/delete return through refreshed Details to Calendar with selected date/month retained. Add uses the existing form's today default and returns to Calendar; it does not silently prefill the selected day or move selection after Save. Deletion removes markers/counts and agenda entries on refresh. |
| CA06 | Tab switches and resume reload committed dues, retaining session month/date; changes made through Dues/Dashboard appear on Calendar. Root Back selects Dues. Restart defaults to Dues and a fresh today selection when Calendar opens. Offline/restart retains dues, payment and deletion. |
| CA07 | Themes, 200% text, explicit status text, numeric day counts, selected/today date semantics, minimum date targets and reachable controls/agenda remain usable. Record targeted native checks separately from full TalkBack/keyboard/other-width acceptance. |

## Dependencies and limits

Uses F01 routes and F02 classifier/projection, existing DatabaseDispatcher/IDueRepository/TimeProvider, PageLayout and semantic resources. All saved dues are read with the existing local repository; defer a range-query optimization until a measured need exists. No private financial data is logged or captured in screenshots. Unit tests/builds do not establish device acceptance; actual evidence belongs in delivery status.
