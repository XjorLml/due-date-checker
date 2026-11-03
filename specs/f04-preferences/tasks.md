# F04 execution tracking

- [x] Inspect authorities/source/tests, preserve the existing tree and define the preferences scope/PF01-PF07.
- [x] Implement preference storage/service, startup theme and draft Settings UI/navigation.
- [x] Integrate Monday/Sunday calendar headers/offset/caption without selection/data changes.
- [x] Pass 61 Core + 155 App tests (216), Android Release build 0 warnings/errors; see [execution evidence](../../docs/delivery/evidence/F04-PREFERENCES-2026-10-18.md).
- [x] Perform native upgrade/theme/week-start/Save/Back/Reminders/offline/restart/theme-text checks and execute cleanup. The original artifact-attributed evidence and historical `cleanup-restoration` checkpoint were reconciled in the [2026-11-03 closeout](../../docs/delivery/evidence/F04-CLOSEOUT-2026-11-03.md); this is not a new native run.
- [x] Independently confirm current installed artifact and restored preference/device state. The final APK installed on M2101K6G (API 33) matches `5B5153...B29E7`; the read-only audit confirms font/theme/airplane/Wi-Fi/no-notification, reports a pre-existing mobile-data difference (current off, historical on), and preserves current preferences and the old checkpoint. See the [2026-11-03 closeout](../../docs/delivery/evidence/F04-CLOSEOUT-2026-11-03.md#current-device-verification).
- [x] Update canonical documents/roadmap/evidence and validate manifest, links, stable requirements and unrelated-work preservation; results are recorded in the [2026-11-03 closeout](../../docs/delivery/evidence/F04-CLOSEOUT-2026-11-03.md#documentation-validation).

## Remaining acceptance

Automated definitions and builds are not device acceptance. Attribute actual native results to the tested APK. Full assistive-technology/cross-device/intended-key acceptance and broader existing release gates remain separate; signing/release and optional CI activation stay held.

- [ ] Full TalkBack/keyboard/switch, landscape and other supported devices/widths.
- [ ] Controlled native preference read/write faults and reminder-tap deferral with an active Settings draft; current failure/overlap coverage is automated plus source inspection.
- [ ] Intended-key release acceptance; no signing/release or optional CI activation is authorized.
