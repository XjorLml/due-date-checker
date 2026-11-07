# Confirmation messages and Undo implementation plan

1. Write failing Core tests for `IDueRepository.RestoreAsync(id, deletedAt, now)`: restores only a row deleted at exactly that stamp, keeps every other field, sets `updated_at`, and returns false for an active, unknown or differently stamped row (CM04, CM05).
2. Core: `RestoreAsync` as `UPDATE dues SET deleted_at = NULL, updated_at = @now WHERE id = @id AND deleted_at = @deleted_at`; the interface default throws so a storage implementation must provide it.
3. Write failing App tests: editor completion messages and the deletion record (CM01, CM03, CM07); `DeletionUndo` restore, once-only, newer offer, changed stamp, storage failure, reminder refresh and the refresh event (CM04–CM06).
4. App: `IMessageHost` (show, show with Undo); `DeletionUndo` service; `ReminderAwareDueRepository` forwards `RestoreAsync`; the editor exposes `CompletionMessage` and `Deletion`; the editor and Details pages show messages after their action (the editor after returning to its parent, so the message sits above the bar that reappears); AppShell refreshes the current page after a restore, and Details reloads a due it had marked unavailable.
5. Android: `AndroidMessageHost` with Material `Snackbar` on the activity content, anchored above the bottom bar when it shows, colours inverted per theme, Jakarta font, up to four lines.
6. Suites, builds, emulator pass (each message, Undo from the list and from Details, the stored row), docs.
