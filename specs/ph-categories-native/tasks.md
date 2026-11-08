# Native Philippine preset category tasks

See [spec](spec.md) and [plan](plan.md).

- [x] Inspect prototype/native category, editor, persistence, recurrence and archive contracts.
- [x] User clarified preset categories only; record scope and acceptance.
- [x] Implement preset catalog, domain field and schema 006.
- [x] Preserve field through CRUD, recurrence, notification guards and format-2 export/import (with format-1 compatibility).
- [x] Implement native picker/editor and saved category presentation.
- [x] Add catalog/domain/migration/archive/editor/projection and stale-payment test definitions; both test projects compile.
- [x] Build Android Release APK (0 warnings/errors), verify signature and test on the task-owned emulator.
- [x] Execute Core/App regression suites successfully: follow-up CI assertion fix on 2026-11-08, agent-observed 185 Core + 313 App passed locally. Initial Application Control failures remain historical evidence.
- [ ] Observe a passing GitHub CI run containing the recurrence assertion fix; the user-provided pre-fix log had 184 Core passes and one failure.
- [x] Record targeted emulator behavior and gaps separately from phone acceptance in [dated evidence](../../docs/delivery/evidence/PH-CATEGORIES-2026-11-08.md).
- [x] Update PRD/SDD/DESIGN, source traceability, existing archive/deferred specs and delivery evidence; validate local links/diff.
- [ ] Actual-phone/TalkBack acceptance on the intended-key artifact.
