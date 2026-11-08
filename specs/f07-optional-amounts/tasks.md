# F07 optional amounts execution tracking

Status: implemented 2026-11-06 with automated and emulator evidence ([evidence](../../docs/delivery/evidence/F07-OPTIONAL-AMOUNTS-2026-11-06.md)). The 2026-11-05 source inspection that found no implementation is superseded. Phone acceptance is open.

- [x] Inspect authorities/source/tests and preserve the pre-feature checkpoint. Baseline `d89ce9d`, 61 Core + 161 App passed; the unrelated `.vscode/launch.json` change was left alone.
- [x] Define OA01-OA06 and record the Bill/Subscription-required, Task/Other-optional, NULL-versus-zero and "No amount" decisions (2026-11-03 spec; wording for the editor and the Dashboard exclusion taken from the selected Focus mockup).
- [x] Implement nullable amount domain validation, schema 004, repository NULL mapping and active-index preservation.
- [x] Implement category-aware editor validation/UI and "No amount" projections/semantics, including the Dashboard upcoming-total exclusion count.
- [x] Add/update domain, migration, repository, editor and projection tests (22 Core + 18 App new).
- [x] Run Core and App tests sequentially, then the Android Release build: 83 Core + 179 App passed; Release compile 0 warnings with signing bypassed.
- [ ] Perform targeted native blank/required/zero/clear/restart checks and clean up. **Emulator done 2026-11-06** (including a real schema 3→4 upgrade of the emulator database); **phone pending** until the phone's app is restored from the laptop that holds its signing key.
- [x] Update canonical docs, roadmap, STATUS/evidence and validate links/diff.

## Remaining acceptance and scope

- [ ] Phone run of the [test plan](../../docs/delivery/TEST-PLAN.md#f07-optional-amount-verification) procedure, both themes at 100%/200%, then delete the temporary records.
- [ ] Full TalkBack/keyboard/switch, other widths/devices/landscape and visual acceptance.
- [ ] Controlled native migration/read/write/navigation failures and intended-key acceptance.
- [ ] Design question: should Details show "No amount" in a quieter style than a peso amount? (See [DESIGN](../../docs/DESIGN.md#f07-optional-amount-adaptation).)
- [ ] Recurrence and timed dues remain separate unimplemented F07 slices.
- [ ] Amount totals/statistics/budgets remain separately specified future work.

Release/signing and optional CI activation remain held.
