# F02 — saved-data Dashboard

Authorized by the user's “Implement f02” request on 2026-10-18, following F01. [PRD](../../docs/PRD.md#f02-dashboard-rules) owns status/search behavior, [SDD](../../docs/SDD.md#6-ui-and-navigation) owns technical contracts, and [DESIGN](../../docs/DESIGN.md#scope-and-authority) owns scoped presentation. [Plan](plan.md) and [tasks](tasks.md) track delivery.

## Scope

Replace the Dashboard sample page with real local dues, summary counts/paid progress, title search, All/Overdue/Due today/Upcoming/Paid filters, Add and selected-ID Details navigation. Dues stays the initial tab and keeps R03 ordering. Calendar was an explicitly labeled sample in the F02 artifact; the later [F03 slice](../f03-calendar/spec.md) replaces it. Existing payment-on-Save and deletion rules remain. No categories, recurring items, reminders, statistics totals, migration, network service, release or CI activation.

The implementation resolves F02's open rules through the linked PRD policy: mutually exclusive date statuses using Asia/Manila, Paid precedence, global summaries and query-plus-status results. Filter/query persist within the cached Dashboard session, including editor/details return; process restart resets them. They are not stored preferences.

## Acceptance scenarios

| ID | Observable outcome |
|---|---|
| DB01 | Dashboard reads active local dues only; excludes tombstones, includes Paid, sorts by due date/creation time. Counts partition into Overdue/Due today/Upcoming/Paid and paid progress handles zero dues without division by zero. No sample data or mock actions appear. |
| DB02 | Asia/Manila date boundaries and Paid precedence agree with the Core classifier. Appearance/resume/read completion and a visible-page minute timer update the date/statuses without a write. Upcoming excludes today. |
| DB03 | Trimmed, ordinal case-insensitive title substring search combines with exactly one status filter. Summaries remain global; result count matches visible rows. Clear filters restores All and empty query. Unknown filter values are ignored. |
| DB04 | No dues, no matches, initial loading, initialization/read failures and retry are distinct. Refresh failure retains last loaded records/counts with an explicit stale-data error; retries replace rows without duplicates and retain query/filter. No false empty/success state. |
| DB05 | Selected row opens F01 Details for its ID, then the existing editor. Save/Cancel/Back/delete return through Details to refreshed Dashboard with query/filter retained. Details uses parent-neutral Back to list. Add from Dashboard saves back to Dashboard; Dues Add remains direct to Dues. |
| DB06 | Tab switching, returning from writes and resume re-read committed data; editor resume preserves unsaved input. Root Dashboard Back selects Dues. Rapid requests/reads do not duplicate frames, mutations or rows. Restart/reopen works offline and resets only ephemeral query/filter state. |
| DB07 | Native semantic themes, text status cues, accessible search/filter/row labels, selected filters, wrapping and single-owner list scrolling remain usable at 200% text. Record real device checks separately from full TalkBack/keyboard/other-width acceptance. |

Repository reads and classifier/filter/date updates never mutate dues. Tests and builds do not establish native acceptance; actual results belong in STATUS and artifact-specific evidence.
