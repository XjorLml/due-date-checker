# Design adoption tasks

Authority: execution tracking for [DA01–DA10](spec.md#acceptance-scenarios).

- [x] Inspect governing documents, source, tests and initial working tree; preserve prior documentation work.
- [x] Define scope, acceptance scenarios and implementation plan before source changes.
- [x] Bundle licensed typography and replace display/icon/splash branding.
- [x] Implement semantic resources and component states.
- [x] Adopt list/editor layouts, responsive spacing, focus/error treatment and blur validation. Also fix app-resource-before-page startup order exposed by native verification.
- [x] Run Core/App regression and Android Release build: 28 Core + 43 App passed; final Release build 0 warnings/errors.
- [x] Perform available native checks: both themes at 100%/200%, validation, retained maximum amount, paid edit, delete Cancel/confirm/restart, and offline operation; creation separately attributed to the preceding artifact.
- [x] Record unperformed TalkBack/device/release gates explicitly.
- [x] Update canonical documentation and validate links/diff.

Execution evidence: [2026-10-17 design adoption](../../docs/delivery/evidence/DESIGN-ADOPTION-2026-10-17.md). The final artifact reused the verification due created on the startup-corrected artifact; do not attribute creation to the final native-chrome artifact.

## Remaining native acceptance

- [ ] Full TalkBack, keyboard and switch-access focus/order review.
- [ ] Compact-width and tablet device verification, including landscape and 200% text.
- [ ] Controlled native failure, empty-state and repeated-tap scenarios on this UI artifact.
- [ ] Intended-key signed artifact pass when release/signing work resumes.

Release and optional CI activation remain deferred. Completion of implementation is separate from native acceptance; evidence belongs in STATUS and linked delivery records.
