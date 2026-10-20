# F04 execution tracking

- [x] Inspect authorities/source/tests, preserve the existing tree and define the preferences scope/PF01-PF07.
- [x] Implement preference storage/service, startup theme and draft Settings UI/navigation.
- [x] Integrate Monday/Sunday calendar headers/offset/caption without selection/data changes.
- [x] Pass 61 Core + 155 App tests (216), Android Release build 0 warnings/errors; see [execution evidence](../../docs/delivery/evidence/F04-PREFERENCES-2026-10-18.md).
- [ ] Perform available native upgrade/theme/week-start/Save/Back/Reminders/offline/restart/theme-text checks; clean and restore settings.
- [ ] Update canonical documents/roadmap/evidence and validate manifest, links, stable requirements and unrelated-work preservation.

## Remaining acceptance

Automated definitions and builds are not device acceptance. Attribute actual native results to the tested APK. Full assistive-technology/cross-device/intended-key acceptance and broader existing release gates remain separate; signing/release and optional CI activation stay held.

- [ ] Full TalkBack/keyboard/switch, landscape and other supported devices/widths.
- [ ] Controlled native preference read/write faults and reminder-tap deferral with an active Settings draft; current failure/overlap coverage is automated plus source inspection.
- [ ] Intended-key release acceptance; no signing/release or optional CI activation is authorized.
