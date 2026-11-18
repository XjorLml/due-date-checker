# F07 twice-a-month repeat task tracking

See the [spec](spec.md) and [plan](plan.md).

- [x] Decisions recorded: "Twice a month" label, user-picked days with default 15 and 30, decided and authorized (user, 2026-11-16). Claude-proposed rules are listed in the [spec's provenance](spec.md#provenance).
- [x] PRD rule, spec TM01–TM10 and plan written before code (2026-11-16, documentation only).
- [x] Prototype preview added to the [design exploration](../../prototypes/design-exploration/index.html) (2026-11-16, agent-observed): `node --test` of model, PH catalog and essential-model tests, **15 passed** (one new TM date test). `browser-check.cjs` in headless Edge **passed** all five suites, with a new twice-a-month editor/Details flow. The first run failed because a new assertion assumed a `dd MMM yyyy` date while the prototype uses the browser locale; the assertion was corrected, not the feature. Native app unchanged.
- [x] Core implementation, then Core tests (domain, recurrence, migration 007, repository, archive v3, reminder text), 2026-11-16. Written in that order, so there is no failing-first run; see the [evidence](../../docs/delivery/evidence/TWICE-MONTHLY-2026-11-16.md).
- [x] App implementation, then App tests (editor, rows, Details); the tests found the switch-to-Monthly anchor defect, now fixed.
- [x] Core 211/212 (the one failure is the pre-existing runtime-11 MoneyTests case) and App 322/322 on a Linux copy; Android Debug build 0 warnings/errors. Windows test execution was blocked by Application Control.
- [x] Release compile with signing bypassed; result in the evidence.
- [x] Emulator checks: real 6→7 upgrade, editor, equal-day error, save, Details, payment and no duplicate, dark at 200% (found and fixed a Repeat row layout defect); cleanup.
- [x] SDD, DESIGN, TEST-PLAN, ADR 011, STATUS and PRD status line updated.
- [ ] Phone check with the intended signed build, including TalkBack on the day pickers (pending the phone's signing-key situation).
- [x] Suites on .NET 10 in CI: run `44335469445` (push to `main`, `c3a2019`, 2026-11-18) passed the Core and App tests, observed through GitHub's public API. The MoneyTests case that fails only on runtime 11 passes there.

Deviations from the [plan](plan.md):

- Steps 1–4: implementation came before tests (see above).
- `Recurrence.DaysFor` and `DefaultTwiceMonthlyDays` were added so the editor and `Due.Create` share the ordering and default; `ReminderText.RepeatPhrase` replaced both `ToString().ToLowerInvariant()` uses.
- The picker binds `RepeatLabels` by `SelectedRepeatIndex`, because the enum's own text cannot read "Twice a month".
- The Repeat row's columns changed to `Auto,*,Auto` after the emulator showed the label squeezed at 200% text.
