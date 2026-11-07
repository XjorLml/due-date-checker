# Live prototype alignment plan

Scope and decisions: [spec](spec.md). Progress: [tasks](tasks.md). The approach keeps every view-model rule that already has tests and changes presentation in XAML; the only logic that moves is search and filtering (Dashboard → Dues), and the only new logic is the Dashboard sections, the calendar count words and the editor's unsaved-changes check.

## Order

1. **Shared pieces.** App-owned SVG glyphs from the prototype (bell, back, pencil, bin, check, repeat, arrow, search, shield and the four type icons Bill, Task, Subscription, Other). A `RootHeader` (wordmark and bell) and a `PushedHeader` (Back, centred title, one icon action) as reusable views; pushed pages hide Shell's title bar. The shared `DueRow` gains the type icon, the repeat line and a plain status text (glyph and word in the status colour); Details keeps the pill.
2. **Dues and Dashboard together** (they trade search). `DuesListViewModel` takes Query, the status filter, the type filter, the match count and Reset from `DashboardViewModel`; UX02's fold applies only while no search or filter is active. `DashboardViewModel` keeps its summary and gains `Attention` and `Ahead` lists (first three each). AppShell gets `ShowDues(status)` so the counters land on Dues with that status. Dashboard tests for search and filters move to Dues tests; new tests cover the sections and the fold/filter interplay.
3. **Calendar.** `CalendarDay` gets a count label ("1 due", "2 dues"); the page moves "Today" into a row under the grid and adds the agenda heading and footnote.
4. **Details.** Header pencil replaces "Edit due"; "Back to list" goes; facts gain Currency / timezone; Notes becomes a card.
5. **Editor.** `DueEditViewModel.HasChanges` compares the form with the loaded due (or a blank one); the page asks before discarding on Cancel, the header and system Back (UX22). Type becomes a "Type: Bill · Change" row (an action sheet with the four types); Repeat and Notes become collapsible rows; Paid becomes a check row; the bin replaces the "Delete" toolbar text.
6. **Settings.** Cards and a fixed bottom-right Save; the note and draft line from the prototype.
7. **Reminders.** Prototype layout with the status card, two cards, the "Notification readiness & privacy" section and a state-dependent footer; the view model gains nothing but visibility flags.
8. **Verification and documents.** Both suites, Debug build, Release compile, an emulator pass per page (light, dark, 200% text, 320 dp), a phone build in Downloads; DESIGN as-built sections, SDD, STATUS, TEST-PLAN and evidence.

## Risks

- **Shell title bar.** Hiding it on pushed pages removes Android's own back arrow, so the custom Back and system Back must share one path, and the editor's busy guard (`IsIdle`) must still block both.
- **Moving tests.** Search and filter tests change owner; their assertions stay the same so behavior does not drift.
- **Row height.** The prototype row (icon, up to three lines) is taller than today's; round 2 item UX20 (more dues per screen) stays open and may revisit it.
