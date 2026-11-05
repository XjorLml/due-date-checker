# F07 optional amounts execution tracking

Status: the 2026-11-05 source inspection found no implementation (migrations 001–003 only, non-nullable `Due.Amount`, no OA tests). Every item below remains open; the [SDD contract](../../docs/SDD.md#f07-optional-amount-contract) is labeled planned until this work lands.

- [ ] Inspect authorities/source/tests and preserve the pre-feature checkpoint.
- [ ] Define OA01-OA06 and record the Bill/Subscription-required, Task/Other-optional, NULL-versus-zero and "No amount" decisions.
- [ ] Implement nullable amount domain validation, schema 004, repository NULL mapping and active-index preservation.
- [ ] Implement category-aware editor validation/UI and "No amount" projections/semantics.
- [ ] Add/update domain, migration, repository, editor and projection tests.
- [ ] Run Core and App tests sequentially, then the Android Release build.
- [ ] Perform targeted native blank/required/zero/clear/restart checks and clean up.
- [ ] Update canonical docs, roadmap, STATUS/evidence and validate links/diff.

## Remaining acceptance and scope

- [ ] Full TalkBack/keyboard/switch, other widths/devices/landscape and visual acceptance.
- [ ] Controlled native migration/read/write/navigation failures and intended-key acceptance.
- [ ] Recurrence and timed dues remain separate unimplemented F07 slices.
- [ ] Amount totals/statistics/budgets remain separately specified future work.

Release/signing and optional CI activation remain held.
