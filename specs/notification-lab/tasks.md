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

- [x] Native implementation authorized by the user, 2026-11-08; execution [plan](plan.md), scenarios NA01–NA08 in the [spec](spec.md#native-acceptance--n1-a--n2-a).
- [x] Privacy-aware text, generic public versions, distinct-due grouping and immutable payment actions.
- [x] Serialized delivered-receipt/snapshot guards, existing atomic recurrence write, guarded 10-second Undo and reminder reconciliation. Editor payment conflict retains the draft.
- [x] Android 12+ authentication-required receiver and Android 8–11 dedicated unlock activity; lock state rechecked before writing.
- [x] Automated verification: 170 Core + 304 App tests passed on Windows/.NET 10.0.400, 2026-11-08, agent-observed. Existing xUnit1031 and unavailable NuGet advisory metadata warnings recorded separately.
- [x] Android Release package build: 0 warnings/errors, agent-observed. Artifact/source and native checks: [evidence](../../docs/delivery/evidence/NOTIFICATIONS-2026-11-08.md).
- [x] Update PRD, SDD, DESIGN, documentation index and TEST-PLAN.
- [x] API-36 emulator: real sample shade cards, no notes, grouped summary, private/public/authentication metadata, actual payment action with one next occurrence, and final-build actual Undo tap restoring unpaid while retaining that next due. [Evidence](../../docs/delivery/evidence/NOTIFICATIONS-2026-11-08.md) retains earlier failed attempts and the visible-window fix.
- [ ] Actual-phone acceptance: MIUI privacy/unlock/actions, grouping, Undo, cold-start taps and natural background delivery. API 26–30 unlock UI also needs runtime acceptance.
