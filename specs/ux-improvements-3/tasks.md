# UX improvements 3 task tracking

See the [spec](spec.md), [plan](plan.md) and [ADR 012](../../docs/adr/012-reminder-channels-and-overdue.md).

- [x] Decisions recorded (user's "implement 1-5", 2026-11-16) with the details Claude proposed listed in the [decision log](spec.md#decision-log).
- [x] Core implementation, migration 008, export format 4 (2026-11-16).
- [x] App implementation: Reminders card, channels, overdue switch and guards, paydays, Dashboard, editor switch.
- [x] Existing tests updated for the contract changes (schema 8, format 4, preference format 2, Dashboard headline, AL04 test behavior; reminder tests pinned to overdue off). New tests were written after the implementation, so there is no failing-first run.
- [x] Core 219/220 (one pre-existing runtime-11 MoneyTests case) and App 337/337 on a Linux copy; Android Debug build 0 warnings/errors after removing an unavailable `SetSilent` call.
- [x] Emulator checks and cleanup ([evidence](../../docs/delivery/evidence/UX-WAVE6-2026-11-16.md)).
- [x] PRD, SDD, DESIGN, TEST-PLAN, ADR 012, STATUS updated.
- [ ] Mark as paid tapped on a live overdue notification (covered by App tests; the emulator tap hit the notification body).
- [ ] Phone check with the intended signed build, including TalkBack on the new rows and a changed ringtone.
- [ ] Suites on Windows/.NET 10 or in CI.
