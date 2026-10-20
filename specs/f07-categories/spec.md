# F07 categories

Authorized by "implement f07" on 2026-10-18 following the next-phase discussion identifying categories/types as the next concrete slice. F07 is an umbrella: this independently deliverable category slice does not claim recurrence, timed dues or optional amounts. [PRD](../../docs/PRD.md#f07-category-rules) owns policy, [SDD](../../docs/SDD.md#f07-category-contract) owns storage/contracts and [DESIGN](../../docs/DESIGN.md#f07-category-adaptation) owns presentation. [Plan](plan.md) and [tasks](tasks.md) track delivery.

## Acceptance scenarios

| ID | Outcome |
|---|---|
| CT01 | Upgrade schema 1 to 2 without changing existing IDs, values, timestamps, payment, currency/timezone or tombstones. All legacy rows become Other. Fresh databases reach schema 2; initialization is idempotent and newer versions remain rejected. |
| CT02 | Bill, Task, Subscription and Other are the only categories. Invalid enum values are rejected by construction/with and storage constraints. New editor defaults to Bill; legacy records load Other. All categories retain required nonnegative PHP amounts, DateOnly Manila dates, payment-on-Save and soft deletion. |
| CT03 | Category selection is an unsaved draft. Save persists it with the same ID/CreatedAt; Cancel/Back/validation/storage failure do not commit it. Missing selection is a field error, and busy/completed/unavailable editors cannot write. Delete ignores unsaved category and retains stored fields. |
| CT04 | Dues, Details, Dashboard and Calendar agenda show literal saved category labels and include category in row semantics. Existing status/date order, counts and navigation remain unchanged. Returning from a category edit refreshes all affected visible projections. |
| CT05 | Dashboard category filter combines with title query and status using AND; global counts/progress remain global. All categories resets the category constraint; Clear filters resets all three. Retain filter for the current Shell session, reset on restart. Stable header/observable results preserve live search when results become empty. |
| CT06 | Offline create/edit/restart/delete retain categories. Themes and 200% text keep category controls/labels reachable. Separate automated/build evidence from actual device checks and remaining full accessibility/native-failure/intended-key gates. |

## Decisions and deferred work

One fixed category taxonomy represents the prototype's types; no separate user-defined category/type tables or icons are introduced. Other is the legacy fallback because old data did not declare a type. Bill is the new-form default; domain callers without an explicit category retain Other for compatibility. Category is independent of status, sorting and calendar membership. Calendar counts/agenda are not filtered by Dashboard preferences. F07 recurrence/timed/optional-amount decisions, scheduler F05, settings F04, and release remain deferred.
