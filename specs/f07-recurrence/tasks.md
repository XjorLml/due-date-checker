# F07 recurring dues task tracking

See the [spec](spec.md), [plan](plan.md) and [ADR 009](../../docs/adr/009-recurring-dues.md).

- [x] Decisions recorded: option A, Never/Weekly/Monthly/Yearly, month-end fallback, Bill and Subscription only (2026-11-06).
- [x] Spec RC01–RC08, plan and ADR 009 written before code.
- [x] Core tests and Core implementation (domain, recurrence, migration 005, repository), 2026-11-06. A failing-first run was recorded only for the rollback test, which exposed the migration-runner defect (fixed: the runner now steps through every statement's result; see [SDD §5.4](../../docs/SDD.md#54-initialization-and-migrations)).
- [x] App tests (compile-failing first), then App implementation (Details, editor, rows, reminders forwarding), 2026-11-06.
- [x] Core and App suites (110 + 216), Debug build, Release compile with signing bypassed.
- [x] Emulator checks and cleanup ([evidence](../../docs/delivery/evidence/UX-WAVE2-2026-11-06.md)).
- [x] PRD, SDD, DESIGN, STATUS, TEST-PLAN, evidence and UX tasks updated.
- [ ] Phone check (pending the phone's signing-key situation); see the [test plan](../../docs/delivery/TEST-PLAN.md#ux-wave-2-verification-recurrence-ux03-and-maker-steps-ux04).

Deviations from the [plan](plan.md):

- Step 4: the reminder wrapper requests one reconciliation after the combined write instead of one per ID; reconciliation re-plans every due, the new one included.
- Step 5: the emulator pass used a monthly Bill on 06 Nov (next 06 Dec) rather than the 31st; the month-end dates are covered by Core and App tests. Dashboard and Calendar rows, dark mode and 200% text were not rechecked on the emulator for the new text (shared projection, covered by tests).
- Added during implementation: `Recurrence.AnchorAfterEdit` and `Recurrence.NextOnPayment`, so the editor's anchor rule and the "only unpaid → paid" rule live in the domain with their own tests; a visible "Next one added for …" line on Details next to the announcement.
