# Credit card payment history tasks

See the [spec](spec.md) and [plan](plan.md).

- [x] Review the current paid model and PRD constraints; record the recommendations (2026-11-19).
- [x] First preview for all dues, with full balance and chips (commit `0be0cf4`).
- [x] User revision recorded: credit cards only, minimum as the default, history for that card; the rest of the design reverted.
- [x] Proposed PRD section and spec PY01–PY10 updated to the revision.
- [x] Revised prototype preview (2026-11-19, agent-observed). `node --test` of model, PH catalog and essential-model: **17 passed**. `browser-check.cjs` in headless Edge: all five suites passed on the first run, including the credit card flow. Screenshots of the dialog and history were reviewed.
- [ ] User checks the revised prototype and approves, changes or declines the proposal.
- [ ] Native implementation (plan steps 1–8), only after approval.
- [ ] Phone check of the native feature.
