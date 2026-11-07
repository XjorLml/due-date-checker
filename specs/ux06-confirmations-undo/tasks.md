# Confirmation messages and Undo task tracking

See the [spec](spec.md) and [plan](plan.md).

- [x] Decision recorded: option B, 5-second Undo after Delete (2026-11-06).
- [x] Spec CM01–CM08 and plan written before code.
- [x] Core tests (compile-failing first), then `RestoreAsync` (2026-11-06).
- [x] App tests written before the App code (not run red first), then App implementation and the Android message host.
- [x] Suites (162 Core + 242 App), Debug build, Release compile, 0 warnings.
- [x] Emulator checks and cleanup ([evidence](../../docs/delivery/evidence/UX-WAVE4-2026-11-06.md)).
- [x] PRD, SDD, DESIGN, V1.3 spec, STATUS, TEST-PLAN, evidence and UX tasks updated.
- [ ] Phone and TalkBack check; see the [test plan](../../docs/delivery/TEST-PLAN.md#ux-wave-4-verification-messages-undo-and-polish-ux06ux12).

Deviations from the [plan](plan.md):

- Step 4: Details also gained `ShowsDue`/`ReloadAsync`, because a Details page that already showed the due as unavailable would otherwise not load it again after Undo.
- Step 5: found during the emulator pass and fixed — Material's letter spacing made the action read "U n d o".
- Step 6: Undo was exercised from Details only; deleting always returns there.
