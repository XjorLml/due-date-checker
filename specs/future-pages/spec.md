# Future pages — roadmap and mock scope

Planning and the original Dashboard/Calendar mocks were authorized on 2026-10-17 after design adoption. On 2026-10-18 the user separately authorized [F01 Details](../f01-item-details/spec.md) and [F02 production Dashboard](../f02-dashboard/spec.md). F02 and the subsequently authorized [F03 Calendar](../f03-calendar/spec.md) replace both mocks. Other future-page behavior remains deferred under the [PRD](../../docs/PRD.md); release stays held. [Plan](plan.md) owns sequencing and [tasks](tasks.md) owns tracking. Original MP/BN scenarios below retain historical artifact context; the feature specs own later extensions.

## Design coverage

Current extension: the user authorized [F02 production Dashboard](../f02-dashboard/spec.md) on 2026-10-18. It replaces the Dashboard mock; the later F03 request also replaces Calendar under its dedicated spec. The MP dashboard scenarios below and original stacked/tab mock evidence are historical. Current Dashboard status/search/date rules are owned by [PRD](../../docs/PRD.md#f02-dashboard-rules), with DB01–DB07 verification. The later F07 request authorizes [categories](../f07-categories/spec.md); timed/recurring work remains deferred. The subsequent F05 request authorizes [local reminders](../f05-reminders/spec.md) under the resolved PRD policy. The later F04 request authorizes [appearance/week-start preferences](../f04-preferences/spec.md); export remains separately deferred.

The page matrix in the plan maps every [DESIGN screen](../../docs/DESIGN.md#8-screen-specifications) to an implementation slice, dependencies and observable acceptance. Future-only product rules stay proposals under [DESIGN scope](../../docs/DESIGN.md#scope-and-authority). Original budgets/savings/auth/sync intentions remain in the [historical backlog](../../docs/PLAN.md#backlog-beyond-the-window-learners-original-s4s13-unchanged); their page designs are not yet specified.

On 2026-10-18 the user supplied a Duewise prototype overview and requested a comparison, documentation updates and important-feature prioritization. The [comparison](../../docs/delivery/PROTOTYPE-REVIEW-2026-10-18.md) records all 13 numbered areas and the shared visual/navigation samples. The [priority order](plan.md#priority-order-after-prototype-review) now owns sequencing. Search is explicit in F02; category filtering depends on F07; F12 adds statistics/insights without renumbering established slices. This authorizes the documentation update, not production implementation of every pictured feature.

## Planning acceptance

| ID | Required planning outcome |
|---|---|
| FP01 | Existing saved-data pages (list/editor, F01 Details, F02 Dashboard and F03 Calendar) are distinguished from proposed pages and historical sample artifacts. |
| FP02 | Every designed screen has a linked design section, proposed owner/module, dependencies and an observable acceptance outcome. |
| FP03 | Dependency order separates pages using existing local dues from status-model, preference, migration, scheduler and cloud work. |
| FP04 | Conflicting business rules are explicit decisions, not silently adopted from DESIGN. |
| FP05 | Each future implementation slice must review/update PRD/SDD as needed, create its own feature spec/tests and record actual device evidence. |
| FP06 | Budgets, savings, auth/sync and widgets are included as later discovery items; absent designs/contracts are not invented. |
| FP07 | Compare every supplied prototype area with inspected implementation, distinguish production/preview/missing behavior, and assign dependency-based priorities including search and statistics. Preserve recorded runtime evidence and release hold. |

## Mock acceptance scenarios

| ID | Observable outcome |
|---|---|
| MP01 | Dues, Dashboard and Calendar are reachable through the authorized bottom bar. Preview pages remain labeled as samples. Tab switching retains local preview state and does not push duplicate pages; Back from a preview returns to Dues. Earlier stacked-preview evidence is historical. |
| MP02 | Dashboard shows eight sample items, two completed, six open, two due today and one overdue. Its progress ring and All/Upcoming/Overdue/Completed filters agree with the fixture; Upcoming includes today. |
| MP03 | Calendar starts on October 17, 2026. Month arrows handle leap years/year boundaries; selecting a date shows precisely that date's samples or an empty agenda. Today restores the fixed sample date/month. |
| MP04 | Preview classes have no repository, database, platform-permission or network dependency. Viewing/filtering/selecting samples cannot insert, edit, complete or delete saved dues. Production payment/status/sort contracts stay unchanged. |
| MP05 | Both pages reuse native design tokens and typography, follow light/dark themes, provide text status cues and accessible date descriptions, and remain scrollable at 200% text. Record actual device checks separately from automated checks. |

Sample categories, optional task amounts, Completed terminology, Monday week start and fixed-date filters demonstrate the design only; they do not resolve the production decisions below. Mock cards are read-only. Settings, add, edit and completion actions are omitted until their real behavior is authorized.

## Bottom navigation acceptance

The subsequent user request, “create a bottom navigation for the main pages,” authorizes this navigation change across the three existing pages. It does not authorize settings or real-data dashboard/calendar features.

| ID | Observable outcome |
|---|---|
| BN01 | A persistent native bottom bar exposes Dues, Dashboard and Calendar with icons, visible labels and a selected state. Dues is the initial destination. No nonfunctional tab is added. |
| BN02 | Switching/reselecting tabs retains dashboard filter and calendar month/selection for the current app session, without duplicate navigation frames. Root preview Back selects Dues; Dues retains native root Back behavior. Process restart starts at Dues with fresh sample state. |
| BN03 | Add/edit open within the Dues stack with the bottom bar hidden. Direct Add returns to refreshed Dues. Under the later [F01 extension](../f01-item-details/spec.md), saved-row Details → Edit returns to reloaded Details, then Back to dues refreshes the list. Cancel/Back discard unsaved fields; deletion/payment mutation rules remain unchanged. Earlier direct-edit navigation evidence is historical. |
| BN04 | Tab icons/selected colors follow the semantic light/dark palette. Labels and content remain reachable at 200% text; native tab selection semantics are retained. Full TalkBack/other-device acceptance is recorded separately. |
| BN05 | Remove the old preview-entry button, duplicate top page switches and footer exits. Preview samples remain isolated from saved dues; no schema/service/signing/release change. |

## Product decisions to resolve per slice

F02 resolved title matching, single-status AND query combination, reset/session retention, global summary scope, Paid precedence, mutually exclusive date groups and Asia/Manila date boundaries in the linked PRD. The list below preserves original discovery topics; F07 category rules are now resolved in PRD; F05 date-only notification rules are now resolved in PRD; remaining task/timezone/recurrence/reporting decisions stay open. Do not reapply the old mock's Completed or today-inclusive Upcoming rules to production Dashboard.

- Whether Paid maps to future Completed, and whether tasks/subscriptions share that lifecycle. Current payment-on-Save remains unchanged until a new requirement replaces it.
- F02/F07 resolve current title matching and combined query/status/category filters; future search expansion, Due Today versus Upcoming membership, Reset/Apply behavior and whether query/filter state survives page switches or restart. Current mock filters are not this production contract.
- Status boundaries, dashboard filters/counts and sort order. Current R03 stays ascending date order, including paid dues. Do not change it to completed-last sorting by accident.
- Manila-only calendar dates versus device-local/selected timezone and timed dues. Define daylight-saving gap/overlap behavior only if timed/timezone work is approved.
- F03 originally resolved week start as fixed Monday. The later F04 preference slice permits saved Monday/Sunday layout, retaining Monday by default; this does not reinterpret historical mock/F03 evidence.
- Required PHP amount versus optional amounts for tasks, custom categories, title cap, unsaved-change prompts and Undo/restore. These DESIGN proposals conflict with current v1 business rules.
- Reminder presets, opt-in/permission timing, quiet hours and delivery limitations. Historical reminder schedules and DESIGN §10 differ; F05 now resolves current date-only policy in [PRD](../../docs/PRD.md#f05-reminder-rules). Recurring/timed/per-due presets remain deferred.
- Recurrence cadence/end conditions, month-end/leap-year handling, occurrence identity, payment per occurrence and edit/delete-series semantics. Recurrence and reminder scheduling are independent contracts that must agree when combined.
- Statistics period, total/paid/pending/overdue definitions and category aggregation. Decide due-month versus actual payment-month reporting; IsPaid/UpdatedAt do not supply payment history. Define optional-amount/category treatment and centavo sum overflow before F12.
- Export content/privacy and restore semantics; account ownership, sync conflict behavior and deletion propagation; budget/savings definitions and calculations. Each requires its own product/technical contract.

These are recorded future decisions, not requests to interrupt the currently authorized design adoption.
