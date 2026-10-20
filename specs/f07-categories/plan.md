# F07 categories implementation plan

1. Preserve the working tree and record CT01-CT06 and PRD decisions. Reuse F01-F03 refresh/navigation and shared row projections.
2. Add a validated Core DueCategory field with stable numeric codes. Append migration 002 (never edit 001), adding a constrained category column defaulting to Other. Extend parameterized insert/update/map; preserve tombstone guards and all old fields.
3. Add a native category picker, field validation/error focus and draft state to the editor. Add saved category labels/semantics to all saved-data projections. Add a Dashboard category picker whose constraint ANDs with title/status while summaries remain global.
4. Verify legacy active/paid/deleted migration, rollback on controlled schema conflict, invalid domain/storage values, reopened persistence, editor cancel/save/failure/delete, projection refresh and combined filters. Run Core/App sequentially, then Android Release build and available data-preserving native/offline/theme/text checks using task-only records.
5. Update canonical docs, roadmap, execution tasks and source/APK evidence. Validate links, preservation and diff. F07 umbrella remains partial; no recurrence/timing/amount-policy/signing/CI/release expansion.
