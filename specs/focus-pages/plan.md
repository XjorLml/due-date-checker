# Implementation plan — Focus page content

Scope and scenarios: [spec](spec.md). Tracking: [tasks](tasks.md).

## Affected modules

| Area | Files | Change |
|---|---|---|
| Shared row | new `Pages/DueRow.xaml` (+ `.cs`), new `ViewModels/IDueRow.cs`, `DueListItem.cs`, `DashboardItem.cs` | A `ContentView` ledger row bound to a small `IDueRow` interface that both projections implement, so Dues, Dashboard and Calendar share one template. `DueListItem` gains `ShortDateText` ("dd MMM"). |
| Dashboard | `ViewModels/DashboardViewModel.cs`, `Pages/DashboardPage.xaml` (+ `.cs`) | New derived members `UpcomingTotalText`, `UpcomingMetaText`, `MonthLabel`, `NextDue`, `HasNext`; summary block, three counter buttons wired to `SelectFilter`, chips restyled, progress ring removed, Refresh bound to `HasError`. |
| Calendar | `Pages/CalendarPage.xaml` (+ `.cs`) | Controls row with `IconButton` arrows and a `Today` chip; day cells rebuilt as transparent 44×48 buttons with count captions; agenda section line; row template. |
| Settings | `ViewModels/SettingsViewModel.cs`, `Pages/SettingsPage.xaml` (+ `.cs`), new `Pages/SegmentedControl.xaml` (+ `.cs`) | `SegmentedControl` with `Options` and two-way `SelectedIndex`; optional `ReminderCoordinator` to read the saved reminder state into `ReminderSummary`; `HasLoadError` for the Refresh rule. |
| Details | `Pages/DueDetailPage.xaml` | Title, amount, badge, divided fact rows, closing timestamp caption. |
| Dues | `Pages/DuesListPage.xaml` | Row template, caption, Refresh bound to `HasError`. |
| Styles | `Resources/Styles/Styles.xaml` | New `HeroAmount`, `RowTitle`, `RowMeta`, `SectionTitle`, `FilterChip`, `IconButton`, `CounterButton`, `LinkButton`, `Divider` styles. |
| Docs | PRD F02 rule, DESIGN §7/§8, SDD §6, STATUS, TEST-PLAN, README, evidence record | Record the upcoming-total rule, the row and segmented components, the as-built screens, and the emulator evidence. |

Unchanged: Core, repositories, migrations, status rules, reminder scheduling, Shell structure, renderer, editor, tests other than additions.

## Dependencies and choices

- **Upcoming total.** Sums `Money.Centavos` of unpaid dues dated today through the last day of today's Manila month; formatted through `Money.FromCentavos` so the fixed PHP format stays in one place. All stored amounts are known in v1; the "excluded" note from the mockup applies only once F07 optional amounts ship.
- **Counters as filters.** The mockup's status links set the Dues filter; natively the Dashboard already owns the filter, so counters call `SelectFilter`. Upcoming stays available through the chip row.
- **Why an interface for the row.** `DashboardItem` and `DueListItem` already expose the same members; an interface lets the shared `ContentView` use `x:DataType` without merging the two records, which the Dashboard and Calendar tests construct directly.
- **Segmented control as a `ContentView`.** Buttons inside a muted border, styles swapped in code like the Dashboard chips; a two-way `SelectedIndex` keeps the Settings view model's existing index properties and tests.
- **Reminder state on Settings.** Read through the existing `ReminderCoordinator` on page appearance; read failures leave the row usable with "Open reminders" rather than an error, because the row is navigation, not a setting.
- **Progress ring.** Removed from the page only; `DueProgressView` remains in the project for the DESIGN §7 component.

## Verification

App tests for the new Dashboard members; `dotnet build` Debug and Release (signing bypassed); emulator pass with seeded dates in light and dark and at 200% text, recorded in the evidence file named in [tasks](tasks.md).
