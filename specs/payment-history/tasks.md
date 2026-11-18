# Payment history tasks

See the [spec](spec.md) and [plan](plan.md).

- [x] Review the current paid model and PRD constraints; record the recommendations (2026-11-19).
- [x] User decisions recorded: one payment per due, optional full balance with chips, dialog asks only for the amount.
- [x] Proposed PRD section and spec PY01–PY12.
- [x] Prototype preview (2026-11-19, agent-observed). `node --test` of model, PH catalog and essential-model: **17 passed** (2 new). `browser-check.cjs` in headless Edge: all five suites passed, including the new payment flow, and the editor suite passed twice more after a test race was fixed. Screenshots of the dialog and history were reviewed.
- [ ] User checks the prototype and approves, changes or declines the proposal.
- [ ] Native implementation (plan steps 1–8), only after approval.
- [ ] Phone check of the native feature.

Notes on the prototype run:
- **Two failed runs, both fixed.** The first browser run failed because the essential-first test requires title, amount and date as the first editor fields, and Full balance had been placed between amount and date. It was moved into a collapsed section after the date. A later run failed because the test checked state before the dialog's close callback had applied it; the test now waits on the state.
- **Seed-size updates.** Six existing assertions counted the sample dues (two in `model.test.cjs`, four in `ph-browser-flows.cjs`) and were updated for the added credit card.
