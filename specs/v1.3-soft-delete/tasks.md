# V1.3 task tracking

Implementation and basic verification recorded 2026-10-17. See [spec scenarios](spec.md#acceptance-scenarios), [as-built plan](plan.md), and [detailed evidence](../../docs/phases/V1.3.md).

## Completed implementation and verification

- [x] Link approved R06/R07 scope and existing repository/navigation contracts.
- [x] Define observable scenarios SD01–SD08; distinguish implemented behavior from remaining acceptance.
- [x] Add tests before delete implementation; record the initial missing-feature compilation failure.
- [x] Add editor-only destructive action and original-title native confirmation.
- [x] Keep false confirmation/dismissal away from storage; preserve unfinished input.
- [x] Implement confirmed deletion, busy/error/retry state, and completed-operation navigation guard.
- [x] Reuse the existing atomic/idempotent soft-delete SQL without a schema/index/package change.
- [x] Verify 28 Core and 42 App cases pass, including prior-slice regression coverage.
- [x] Verify final Android Release build passes with zero warnings/errors.
- [x] Install update without clearing user data.
- [x] Verify native new-form visibility, named dialog, Cancel retention, confirmed list removal, and hidden state after process restart using temporary dues only.
- [x] Record observed evidence separately from unperformed release/device checks.

## Remaining acceptance

- [ ] SD08: repeat native delete/cancel/restart in airplane mode.
- [ ] Exercise native dialog Back/dismissal and repeated taps during a write.
- [ ] Exercise controlled native confirmation-display, storage-write, and return-navigation failures from TP05.
- [ ] Verify both themes, large text, accessibility focus, and the destructive action on-device.
- [ ] Inspect retained tombstones in an authorized phone test database; automated SQLite retention evidence does not establish this device-level gate.
- [ ] Complete intended-key signing and rerun release acceptance with that artifact.

The unchecked items do not negate the completed implementation/observed checks. They remain explicit acceptance work; no user acceptance is inferred for them.

## Dependencies and completion criteria

Implementation dependency order: repository contract -> view-model confirmation/state guards -> page confirmation/navigation -> automated/build verification -> native verification. The checked steps above retain the feature task's evidence in the [execution record](../../docs/delivery/evidence/V1.3-2026-10-17.md).

Remaining tasks use [TEST-PLAN](../../docs/delivery/TEST-PLAN.md) expected outcomes: native edge/failure/accessibility checks require a controlled test build and phone; SD08 requires airplane-mode cancel/confirm/restart; physical tombstone checks require an authorized test database. Intended-key signing precedes the final signed-artifact pass. Record environment, artifact/source identity, date, expected/actual outcome, and observer before checking off an item. Feature release acceptance remains open until all required remaining checks have evidence; old passes do not satisfy a changed implementation automatically.
