# Notification lab tasks

See the [spec](spec.md).

## HTML lab — 2026-11-07

- [x] Notifications tab in the design exploration: six sample dues and a two-on-one-day case, three privacy modes, shade and lock screen, day before and on the day, interactive Mark as paid with Undo and a simulated unlock, a table of every reminder, decisions N1 and N2 with notes and export.
- [x] Node data tests: `node --test tests/notification-lab.test.cjs` → 4 passed (NL01–NL04). Agent-observed, Node 22.12.0.
- [x] Browser checks: `node tests/browser-check.cjs --notifications` passed (NL05–NL10), then the full suite with the other labs. Agent-observed in headless Edge, Windows 11.
- [x] Visual review of the generated screenshots (ignored `.qa/notification-*.png`). Fixed during review: the phone always showed Saturday 7 November; it now shows the day the reminder arrives.
- [x] The user chose N1 A and N2 A and pasted the export (2026-11-07); recorded in the [decision log](spec.md#decision-log), the PRD and DESIGN.

Limits: drawings are light-theme illustrations; the lab changes no app behavior and schedules no notification.

## Implementation

- [ ] Not started; see the [implementation plan](spec.md#implementation-plan-not-started). It starts when the user gives the go-ahead.
