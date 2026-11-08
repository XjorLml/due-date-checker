# Export and import task tracking

See the [spec](spec.md), [plan](plan.md) and [ADR 010](../../docs/adr/010-export-import-file.md).

- [x] Decisions recorded: user-chosen file, JSON with a version number, import only while no dues show, hidden deleted dues replaced by the file's copy (2026-11-06).
- [x] Spec EX01–EX08 and plan written before code.
- [x] Core tests (compile-failing first), then Core implementation (`DueRecord`, `DueArchive`, `SqliteDueArchive`), 2026-11-06.
- [x] App tests (build-failing first), then App implementation (`IDueFileService` and its Android SAF implementation, `DataTransferViewModel`, Settings "Your data" card, `ReminderCoordinator.AfterImport`), 2026-11-06.
- [x] Core and App suites (159 + 234), Debug build, Release compile with signing bypassed, 0 warnings.
- [x] Emulator checks and cleanup ([evidence](../../docs/delivery/evidence/UX-WAVE3-2026-11-06.md)).
- [x] ADR 010, PRD, SDD, DESIGN, STATUS, TEST-PLAN, evidence and UX tasks updated.
- [ ] Phone check (pending the phone's signing-key situation); see the [test plan](../../docs/delivery/TEST-PLAN.md#ux-wave-3-verification-export-and-import-ux05).

Deviations from the [plan](plan.md):

- Step 2: the JSON is written and read by hand (`Utf8JsonWriter`/`JsonDocument`) because the trimmed Release build disables reflection-based `JsonSerializer`; see ADR 010.
- Step 5: the emulator restore soft-deleted the seed dues with on-device `sqlite3` instead of four taps on Delete; the stored result is the same shape. A damaged-content file was refused only in tests; on the emulator a newer-version file was used.
- Found during the emulator pass and fixed: the empty result line left a gap in the card; it is now hidden while empty.
