# Prototype comparison — 2026-10-18

Historical comparison snapshot taken before the later F01/F02/F03 implementation requests. These extensions have since been authorized and implemented; use [current STATUS](STATUS.md), [F01 tasks](../../specs/f01-item-details/tasks.md), [F02 tasks](../../specs/f02-dashboard/tasks.md) and [F03 tasks](../../specs/f03-calendar/tasks.md) for verification state. The subsequent [F07 category slice](../../specs/f07-categories/spec.md) also implements category selection/labels/filtering; its tasks own current verification. The table below preserves what was missing at review time.

Source: the Duewise overview image attached by the user on 2026-10-18, with numbered sections 01–13, palette, typography and component samples. This identifies the conversation attachment; no repository image file is claimed. The earlier Duely branding image is a different reference.

Provenance: agent-observed image and working-tree source/test inspection on 2026-10-18. This is a documentation review, with no new application tests, build or phone execution. [STATUS](STATUS.md) owns implementation/evidence; the [roadmap priority order](../../specs/future-pages/plan.md#priority-order-after-prototype-review) owns sequencing. The user requested comparison, documentation updates and prioritization; implementation of the remaining prototype features and release are not part of this task.

## Feature comparison

Implemented means a production code path exists, not that every release gate is accepted. Preview means fixed sample data, with no saved-due integration. Missing means no corresponding production feature was found in the inspected pages, view models, domain, schema or service registration.

| Prototype area | Current implementation and gap | Existing slice / governing scope |
|---|---|---|
| Brand, palette, typography and components | Duewise identity, local fonts, semantic light/dark colors, cards, form states and native controls exist. The image's Inter/SF Pro and bright blue appearance differ from adopted Plus Jakarta Sans and semantic indigo resources. No token/font replacement follows from this review. | Design adoption; [DESIGN scope](../DESIGN.md#scope-and-authority) |
| 01 Onboarding, four screens | Missing: no first-run flow, seen-state or onboarding pages. Reminder/task/subscription claims depend on those features shipping. | F06 |
| 02 Home/dashboard, light and dark | Preview only: sample counts, progress and filters. Missing saved-data summaries, live date status, title search, real row navigation and refresh after writes. Greeting/profile are decorative proposals; no user profile/account exists. | F02; F01 navigation |
| 03 Add due item | Core form works: title, required PHP amount, DateOnly due date, optional notes, validation and Save. Category/type selection and Repeat are missing. Tasks with no amount would require a product/model change. | R02; F07 extensions |
| 04 Edit due item | Edit fields, payment-on-Save and confirmed soft delete work. Category/type and Repeat remain missing. The switch does not persist immediately. | R04–R06; F07 |
| 05 Due details | Missing separate read-only detail page: selecting a saved due currently opens the editor. Stored fields can support initial details; category, recurrence and computed Overdue need separate rules. | F01 |
| 06 Calendar view | Preview month arrows, date selection, day counts, Today and agenda/empty state work against fixtures. Missing saved-dues integration and navigation to a real item. Sample Today is fixed at 2026-10-17, even after that date. | F03 |
| 07 Filters and categories | Dashboard preview has All/Upcoming/Overdue/Completed filters only. Missing production search/status filtering, a Due Today filter, category selection, combined filters, Reset/Apply semantics and persistence policy. Samples have category labels; real Due has no category field. | F02 search/status; F07 categories |
| 08 Recurring options | Missing repeat rule, recurrence storage, generation/update behavior and repeat picker. Existing timezone/currency columns do not supply recurrence. | F07, separate recurrence slice |
| 09 Reminder settings | Missing scheduler, permissions/channels, reminder preferences, lifecycle reconciliation and reminder editor. No notifications are scheduled. Recurrence alone would not implement reminders. | F05 |
| 10 Statistics/insights | Missing monthly totals, paid/pending/overdue aggregates and category chart. Preview completion progress is not this feature. Original roadmap omitted a dedicated statistics slice; F12 is added without renumbering F01–F11. | F12 |
| 11 Settings | Missing settings page, stored theme override, currency/timezone selectors, data/storage/export flows and About/Privacy pages. System theme following already works. PHP and Asia/Manila remain fixed product rules. | F04; F05 notification entry |
| 12 Android/iOS widgets | Missing widget provider, update/privacy behavior and iOS app support. Current app targets Android only. | F11 |
| 13 Empty/error states | Real Dues already has loading, empty, retry and stale-list error states; editor preserves input on failure. Preview calendar has empty agenda. Prototype illustrations are absent. An offline banner with Try Again must not block functional local CRUD: no internet is not a local-storage error. Future pages need equivalent real states. | R02–R07; F08 across slices |
| Bottom navigation shown within screens | Native Dues/Dashboard/Calendar tabs exist, with editor bar hidden. The image proposes Home/Calendar/Add/Stats/Settings, including a central Add action. Stats/Settings destinations and this composition are missing; retain working tabs until replacement destinations/actions are implemented. | BN01–BN05; future F08 |

## Source and test traceability

- Production list/read/empty/error state: [DuesListViewModel](../../src/DueDateChecker.App/ViewModels/DuesListViewModel.cs), [DuesListPage](../../src/DueDateChecker.App/Pages/DuesListPage.xaml) and [row selection](../../src/DueDateChecker.App/Pages/DuesListPage.xaml.cs).
- Real fields and persistence boundary: [Due](../../src/DueDateChecker.Core/Domain/Due.cs), [schema v1](../../src/DueDateChecker.Core/Data/Migrations/001_create_dues.sql), [editor state](../../src/DueDateChecker.App/ViewModels/DueEditViewModel.cs) and [native actions](../../src/DueDateChecker.App/Pages/DueEditPage.xaml.cs).
- Original fixture classes/tests were subsequently removed by F03. Current replacements are [CalendarViewModel](../../src/DueDateChecker.App/ViewModels/CalendarViewModel.cs), [CalendarTests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs) and [F02 DashboardViewModel](../../src/DueDateChecker.App/ViewModels/DashboardViewModel.cs). Mock observations below remain historical; current behavior/evidence belongs in STATUS and the dedicated F01/F02/F03 specs.
- Routes/services/platforms: [AppShell](../../src/DueDateChecker.App/AppShell.xaml), [composition root](../../src/DueDateChecker.App/MauiProgram.cs) and [App project](../../src/DueDateChecker.App/DueDateChecker.App.csproj). Appearance: [colors](../../src/DueDateChecker.App/Resources/Styles/Colors.xaml) and [styles](../../src/DueDateChecker.App/Resources/Styles/Styles.xaml).
- Inspected core-flow tests: [create/list](../../tests/DueDateChecker.App.Tests/CreateAndListTests.cs), [edit/payment](../../tests/DueDateChecker.App.Tests/EditDueTests.cs) and [delete](../../tests/DueDateChecker.App.Tests/DeleteDueTests.cs). Latest recorded execution remains the [2026-10-17 bottom-navigation evidence](evidence/BOTTOM-NAVIGATION-2026-10-17.md); inspection does not refresh its date or acceptance.

## Decisions before production integration

The [future-page spec](../../specs/future-pages/spec.md#product-decisions-to-resolve-per-slice) owns the unresolved product decisions. Resolve these before exposing matching controls:

- Paid versus Completed; date/status boundaries and whether Upcoming includes today; all-active date order versus grouped/completed-last order.
- Category versus item type, optional task amounts, recurring occurrence identity and payment history. A paid flag and UpdatedAt alone do not establish when payment happened.
- Sunday-first in the supplied image versus Monday-first in current mocks; retain Asia/Manila dates until a timezone change is separately defined.
- Reminder timing/delivery limits, privacy of exported/widget data and supported Android/iOS scope.
- Statistics period, totals and category denominators. A due-date-period paid total and an actual cash-payment-period total are different metrics; define which is intended before collecting new history.

Read-only details and real-data dashboard/calendar provide the earliest visible improvement without requiring all future schema/platform changes. Their priority and acceptance boundaries are maintained in the linked roadmap, rather than inferred from the image's screen numbering.

## Documentation validation

Agent-observed on 2026-10-18, Windows/PowerShell: compared with a file-hash/content checkpoint taken before this task. Only ten Markdown files changed; application source/tests, other existing files and historical evidence were preserved. Checked 235 link references in affected documents with no newly broken local paths/heading anchors or duplicate headings. Confirmed unchanged R01–R08 requirement rows, retained F01–F11 identities, added F12, coverage of all thirteen numbered prototype areas and P0–P4 priorities. Reviewed the task diff; `git diff --check -- docs specs` passed. No application tests/build/device acceptance were performed for these documentation changes.
