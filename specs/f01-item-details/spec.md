# F01 — saved due details

Authorized by the user's “implement f01” request on 2026-10-18. This is the first production slice in the [future-page roadmap](../future-pages/plan.md#priority-order-after-prototype-review). [PRD](../../docs/PRD.md) owns product behavior; [SDD](../../docs/SDD.md#6-ui-and-navigation) owns navigation/data contracts; [DESIGN item detail](../../docs/DESIGN.md#item-detail) applies with the scoped adaptation below. [Plan](plan.md) and [tasks](tasks.md) track implementation and verification.

F03 navigation adaptation: Calendar is the third saved-data parent. The same ID-only Details/editor stack and Back to list return preserve Calendar month/date; [F03](../f03-calendar/spec.md) owns that integration.

## Scope

F02 navigation adaptation: Details can now be entered from either saved-data root, Dues or Dashboard. Its explicit return caption is Back to list; one pop preserves the original parent/query/filter. The original Back to dues wording in the scenarios below describes the F01-only artifact; the data/read-only/error contracts remain unchanged. [F02](../f02-dashboard/spec.md) owns Dashboard integration.

Read-only details of an existing active due: title, PHP amount, calendar due date, Paid/Unpaid, notes (explicit absent state), CreatedAt and UpdatedAt displayed in UTC. Saved-row selection opens Details; Add still opens a new editor. Details offers Edit and Back to dues. Edit keeps all existing validation, payment-on-Save and confirmed-delete behavior. Save/Cancel/Back from the editor returns to freshly loaded Details; a deleted or unavailable target shows an unavailable state with Back to dues. Opening Details never writes.

No categories, recurrence, reminders, derived Overdue/Completed, inline payment/delete/Undo, schema change, production dashboard/calendar integration or release. Only the selected ID is routed, never copied fields. No deep-link/process restoration of an unsaved navigation stack is promised; cold restart begins at Dues and reopening the item loads committed state.

## Acceptance scenarios

| ID | Observable outcome |
|---|---|
| DT01 | Selecting a saved row opens that ID's persisted fields in a read-only page, with textual Paid/Unpaid and explicit absent notes. Opening/reloading/Back makes no mutation and preserves identity/timestamps. |
| DT02 | Edit loads the same ID. Save returns to updated Details, including payment and timestamps; Cancel/system Back returns to stored fields without persisting unsaved edits. Back to dues reloads the existing list; Add remains direct create → Dues. |
| DT03 | Missing, malformed, empty or soft-deleted IDs show unavailable, hide previous fields and disable Edit. No insert or restore is possible. Deleting through the editor returns to unavailable Details; Back shows the filtered list. |
| DT04 | Initial/read/refresh failure hides unverified fields, disables Edit, retains target identity and offers retry. Retry reloads the same ID; repeated reads do not overlap. Failed navigation preserves read state and offers another attempt without mutation. |
| DT05 | Appearing after editor return and resuming on Details reload committed data; resume on the editor preserves unsaved input. Repeated route delivery does not retarget an existing detail page. Restart/reopen and viewing/editing saved data work offline. |
| DT06 | Details reuses semantic themes, native controls and responsive layout. The bar is hidden on Details and editor; returning to Dues restores it. Labels, long amounts/notes/timestamps and all actions remain reachable in both themes at 200% text. Full TalkBack/keyboard/other-device acceptance is separately recorded. |

Automated state/persistence tests do not establish native navigation, XAML, lifecycle or accessibility acceptance. Record actual results and artifacts in STATUS/evidence.
