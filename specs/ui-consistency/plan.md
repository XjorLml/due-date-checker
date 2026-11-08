# Implementation plan — UI consistency

Scope and scenarios: [spec](spec.md). Tracking: [tasks](tasks.md).

## Affected modules

| Area | Files | Change |
|---|---|---|
| Shared projection | `ViewModels/DueListItem.cs`, `ViewModels/DashboardItem.cs`, new `ViewModels/ManilaCalendar.cs` | `DueListItem` now carries a `DueStatus` and exposes `StatusLabel`; `Create(due, today)` classifies through the Core `DueStatusCalculator`. `DashboardItem` delegates its text to the same row. `ManilaCalendar.Today(TimeProvider)` is the one place that turns a clock into the product date. |
| View models | `ViewModels/DuesListViewModel.cs`, `ViewModels/DueDetailViewModel.cs` | Optional `TimeProvider` constructor parameter (DI supplies the registered clock; tests keep their two-argument calls) and projection through `Create`. |
| Badge | new `Pages/StatusBadge.xaml` (+ `.cs`) | `ContentView` with a `Status` bindable property; XAML data triggers pick tint, glyph and text colour from the word. Four new tint keys in `Colors.xaml` (`OverdueSurface`, `DueSoonSurface`, light and dark). |
| Pages | `DuesListPage`, `DashboardPage`, `CalendarPage`, `DueDetailPage` | Inline status markup replaced by the badge; Details label "Payment status" becomes "Status". |
| Titles | `DuesListPage`, `DashboardPage`, `CalendarPage`, `SettingsPage` (`Shell.NavBarIsVisible="False"`); `DueEditPage`, `DueDetailPage`, `ReminderSettingsPage` (in-page level-1 heading removed) | Roots: the page heading is the title and the brand lockup sits in the Dues content header. Pushed pages: the Shell title is the heading. |
| Tests | `EditDueTests`, `DueDetailTests` | Two assertions that expected the literal "Unpaid" now accept any of the three unpaid date statuses. |

Unchanged: Core, repositories, migrations, status rules, reminder code, Shell structure, renderer.

## Dependencies and choices

- **Why a `ContentView` rather than a style.** The pill needs three coordinated changes (tint, glyph, text colour) keyed on one value across four pages; a bindable property with triggers keeps that in one file instead of four copies of the trigger block.
- **Why Details and Dues reuse the Dashboard classification.** The PRD already defines the status for Dashboard and Calendar; computing it in one shared projection means every surface agrees on the same Manila date instead of some rows showing Paid/Unpaid and others Overdue/Due today.
- **Why hide the bar on roots instead of dropping the page heading.** The Focus prototype has no app bar on root screens; the content heading carries the page identity and the bottom bar carries navigation. Pushed pages need the Back arrow, so there the Shell title stays and the content heading goes.
- **Optional clock parameter.** Keeps the existing tests and the DI registration both valid; `TimeProvider.System` is the fallback only when nothing is injected.

## Verification sequence and outcomes

1. App tests: 158 pass after the projection change.
2. Debug Android build succeeds.
3. Emulator (AOSP API 35): seed four dues by moving the device clock so one is Overdue, one Due today, one Upcoming, and mark one Paid; capture Dues, Dashboard, Calendar, Settings, Details, editor and Reminders in light and dark; confirm through the accessibility tree that root pages have no toolbar title and pushed pages have no duplicate heading.
4. Release compile with signing bypassed.
5. Documentation: PRD workflow sentence, DESIGN §7 badge and app-bar rows, SDD §6, STATUS, TEST-PLAN, README, evidence record.

## Risks and limits

- Hiding the Shell navigation bar on roots removes the toolbar from screen readers' page title; the content heading keeps its level-1 semantic. Needs the deferred TalkBack review.
- The Dues page now re-evaluates status only on load/refresh; like Dashboard it does not tick at midnight while visible. Dashboard owns the minute timer; Dues did not have one before either.
- The status bar inset above hidden-bar roots shows the canvas colour on the emulator; on a phone the opaque status bar covers it.
