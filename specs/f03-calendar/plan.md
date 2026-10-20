# F03 implementation plan

1. Preserve the existing working tree; specify CA01-CA07 and resolve week-start/selection/Today/Add behavior in PRD. Keep R01-R08 and existing persistence contracts.
2. Replace CalendarPreviewViewModel with injected CalendarViewModel using active snapshot reads, F02 statuses, immutable day projections and one read-only observable agenda. Retain last successful data on refresh failure and guard concurrent reads. Recalculate Manila date without moving selection.
3. Replace CalendarPreviewPage with a lazy DI-backed CalendarPage. Keep a single vertical CollectionView scroll owner, persistent header, horizontal seven-column month grid, day counts and selected-day cards. Reuse existing styles and DashboardItem projection; remove obsolete fixture-only view/code/tests.
4. Reuse F01/editor routes in Calendar's stack. Refresh on appearance/resume, run a visible minute date timer, preserve selected month/day and retain Dues root Back behavior. Add retains the existing form's today default.
5. Replace fixture tests with SQLite-backed acceptance tests for exact counts/ordering/tombstones, leap/year/bounds, Manila rollover, read completion, selection while loading, editor writes, retry/stale/init failures and offline reopening. Run Core/App sequentially, build Android, then available device checks using task-only zero-amount dues.
6. Update canonical documents, roadmap and artifact-specific evidence; validate links/source preservation. Preserve historical evidence. No package/schema/signing/CI/release changes.
