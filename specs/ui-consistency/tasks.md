# UI consistency — task tracking

Authorization: user request on 2026-11-05 ("reword the PRD then start the status-badge and duplicate title fixes"). Scenarios: [spec](spec.md). Approach: [plan](plan.md). Results are agent-observed unless stated otherwise.

## Implementation

- [x] PRD reworded so optional amounts read as authorized-but-unimplemented (R02, R04, business rules, F07 status note); no rule removed.
- [x] Shared `DueListItem` status projection with `ManilaCalendar.Today`; `DashboardItem` delegates to it; Dues and Details view models accept an optional clock.
- [x] `StatusBadge` ContentView with tint, glyph and text colour triggers; `OverdueSurface` and `DueSoonSurface` tints added to Colors.xaml.
- [x] Badge used on Dues cards, Dashboard rows, Calendar agenda rows and Details ("Status" label).
- [x] Root pages hide the Shell navigation bar; Dues brand lockup moved into content; pushed pages drop the duplicate in-page heading.
- [x] Two App tests updated from the literal "Unpaid" to the three unpaid date statuses.

## Verification — 2026-11-05

Environment: as in the [Focus adoption evidence](../../docs/delivery/evidence/FOCUS-ADOPTION-2026-11-05.md) (AOSP Android 35 emulator, Pixel 7 profile, HWUI drawing enabled). Record: [UI-CONSISTENCY-2026-11-05](../../docs/delivery/evidence/UI-CONSISTENCY-2026-11-05.md).

- [x] `dotnet test` App: **158 passed** after the projection change (Core untouched).
- [x] Debug Android build succeeded; Release compiles with **0 warnings, 0 errors** with upload-key signing bypassed (keystore password still wrong in `local.signing.props`).
- [x] Emulator: device clock moved to seed Water bill and Phone load (03 Nov), Internet (07 Nov) and Rent (05 Nov), then Phone load marked paid on 05 Nov. Dues shows Overdue, Paid, Due today and Upcoming pills with their tints; Dashboard counts Overdue 2, Due today 1, Upcoming 1 for the same data; Details shows the Overdue pill under a "Status" label (UC01, UC02, UC03).
- [x] Emulator tree: Dues, Dashboard, Calendar and Settings expose no toolbar title node and exactly one heading; Add/Edit, Details and Reminders expose the Shell title and no second heading; the editor keeps its supporting line, header Delete and fixed footer (UC04, UC05).
- [x] Create, edit, mark paid, Save and Back round trips persisted through the pass (UC06).

A first run of the seeded pass wiped the app data with `pm clear`, which also removes Fast Deployment assemblies and aborts the app at launch; the pass was repeated after a full redeploy. That failed run is not evidence.

## Remaining acceptance

- [ ] Phone verification: badge legibility at 200% system text, status-bar area above the hidden-bar roots, TalkBack reading of "Status: word" and of pushed-page titles (assistive review remains deferred by user direction).
- [ ] Intended-key signing and release gates remain held.
