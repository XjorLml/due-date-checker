# F01 implementation plan

[Spec](spec.md) owns DT01–DT06; [tasks](tasks.md) owns execution status.

1. Add transient DueDetailViewModel and DueDetailPage, reusing IDueRepository, DatabaseDispatcher, DueListItem and PageLayout. Read via GetAsync, reject tombstones, and clear content before refresh so unavailable/failure states never expose stale verified-looking fields. Format timestamp labels explicitly in UTC.
2. Register an ID-only Shell detail route. Saved-row selection pushes Details; Edit pushes the existing editor. Hide the bottom bar in both. Use one-step pops to preserve the existing stack; deletion returns to unavailable Details. Use destination-neutral return text in the editor because its parent can now be Dues or Details.
3. Share the detail refresh task across appearance/resume; refresh only the currently visible saved-data page from App's resume handler, preserving unfinished editor input. Ignore repeated query delivery on a page instance. Keep page navigation guards and retryable errors.
4. Add meaningful SQLite-backed tests for no-write reads, edit/Cancel/delete/reopen outcomes, malformed/unknown IDs, missing notes, read/initialization failures, retry and overlapping load prevention. Run Core/App sequentially, then Android Release build and available device checks.
5. Update PRD, SDD, DESIGN adaptation, roadmap/tasks, delivery status and test plan. Preserve historical prototype/sample and verification records; no package, migration, signing/CI activation or release.

Primary risk is the changed editor parent stack: verify Save, Cancel, Back, delete and failed return state with Details below the editor, plus direct Add. A details-only read failure must not enable Edit on stale content. Do not log or capture existing user financial data during phone verification.
