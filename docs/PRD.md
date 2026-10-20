# Duewise — Product requirements

Authority: approved product scope, user-visible behavior, business rules, and acceptance outcomes. Technical contracts belong in [SDD](SDD.md); UI standards in [DESIGN](DESIGN.md); evidence and completion state in [delivery status](delivery/STATUS.md).

Baseline: the approved 2026-10-10 local CRUD re-scope in the [historical plan](PLAN.md#v1-re-scope-2026-10-10-approved), with requirements R01–R08 migrated from the [prior SDD](delivery/SDD-2026-10-17.md#3-requirements-and-traceability). Documentation reviewed against the working tree on 2026-10-17. Review is not release acceptance.

## Product and users

Duewise helps students and employees in the Philippines keep track of amounts they owe and their calendar due dates. Users can record an obligation, find it in date order, correct its details, track payment, and remove it from the active list. Successful changes must survive restarting the app.

The v1 deliverable is an Android app distributed as a sideloaded Release APK. It works locally without an internet connection or account. PHP is the only currency; Asia/Manila is the product timezone, independent of the developer's or device's timezone. No usage targets or performance thresholds have been approved.

## Approved requirements

“Active” means not soft-deleted. Paid dues are still active and remain visible. The following IDs preserve the original requirement identities and scope. R08 is an optional delivery requirement, not a mandatory user feature.

| ID | Requirement | Slice | Acceptance outcome |
|---|---|---|---|
| R01 | A Release APK installs and launches from the phone launcher. | V1.0 | Intended signing key verified; launcher smoke test passes. |
| R02 | Create a due with required title, amount, and date; notes are optional. | V1.1 | Two saved dues survive process termination and relaunch. |
| R03 | List all active dues by ascending due date. | V1.1 | Earlier dates precede later dates; empty state appears with no active dues. |
| R04 | Edit title, amount, date, and notes while keeping the same identity. | V1.2 | Saved values survive relaunch; Id and CreatedAt remain unchanged. |
| R05 | Set paid/unpaid status. | V1.2 | Both status transitions persist and remain visible in the list. |
| R06 | Confirm deletion, hide the due, and retain its database row. | V1.3 | Cancel keeps the row active; confirm records DeletedAt and removes it from the list. |
| R07 | All v1 operations work offline. | All mandatory slices | CRUD acceptance tests pass in airplane mode. |
| R08 | Optionally run Core tests on push. | V1.4 | A push runs the test project successfully; CI is not a mandatory v1 feature. |

## Business rules and validation

Approved extension **F01**, authorized 2026-10-18: a saved row opens read-only details for the selected ID, showing existing title, amount, due date, payment state, notes and created/updated timestamps (explicit UTC labels). Details offers Edit and Back to dues. Editor Save/Cancel/Back returns to reloaded Details; deletion returns to an unavailable state. Add remains direct create → Dues. Missing/deleted IDs cannot be inserted/restored; reads are retryable and never write. [DT01–DT06](../specs/f01-item-details/spec.md#acceptance-scenarios) define feature scenarios. R01–R08 and their data/payment rules are preserved.

- Title is required, must contain a non-whitespace character, and is trimmed when saved. There is no approved title or notes maximum length. A new limit requires an explicit product decision.
- Amount is required and nonnegative; zero is valid. Input uses digits and an optional decimal point with digits on both sides, without grouping separators, exponent notation, or currency symbols. Leading/trailing whitespace is ignored. Label it “Amount (PHP)” and provide a format hint.
- Round the amount to centavos, with midpoint values rounded away from zero, and preview the amount that will be saved. The approved storage constraint is a signed 64-bit centavo count; reject values outside that range after rounding. Technical representation and overflow handling are in the SDD.
- Due date is a required calendar date. Past dates are valid. A new form defaults to today's date in Asia/Manila. Dates must be displayed explicitly; the implemented list uses `dd MMM yyyy` and the editor uses `yyyy-MM-dd`.
- Notes are optional. Whitespace-only notes become absent; otherwise preserve their text.
- New dues start unpaid. The paid switch is available for an existing due and persists only on Save.
- Soft deletion retains the stored record, removes it from the active list, and provides no in-app restore, recycle bin, or Undo in v1.

## User workflows

### F02 Dashboard rules

F02 is authorized by “Implement f02” on 2026-10-18. The Dashboard now uses local saved dues; Dues remains the initial root and its R03 date-sorted list/payment contracts remain unchanged. [DB01–DB07](../specs/f02-dashboard/spec.md#acceptance-scenarios) refine verification.

- Include every non-deleted due, including Paid. A deleted due has no dashboard status or count. Status is derived, never stored: Paid takes precedence; unpaid dates before today's Asia/Manila calendar date are Overdue, equal dates are Due today, and later dates are Upcoming. These four groups are mutually exclusive; Upcoming excludes today. Keep Paid terminology, not the mock's Completed lifecycle.
- Summary counts and paid/total progress use all active saved dues across dates, regardless of search/filter. Empty progress is zero. They are not monthly spending or payment-history statistics.
- Search trims surrounding whitespace and matches a title substring with ordinal case-insensitive comparison. A single All/Overdue/Due today/Upcoming/Paid filter combines with the query using AND. Results retain ascending due-date/creation-time order; the result heading shows matching count. Clear filters clears both query and status selection.
- Retain query/filter across tabs and Details/editor returns for the current Shell session; reset to All/empty on process restart. An empty database and a query with no matches have distinct states. Read/refresh failures preserve last successful data with a stale-data warning and retry; no internet is required.
- Dashboard appearance/resume and return after writes reload committed data. While visible, a minute check updates date classifications; a manual refresh also reads the current date. Editor resume keeps unsaved input. No exact midnight delivery/background scheduling is promised.
- Dashboard Add opens the existing new form and returns to Dashboard. A selected result opens ID-only Details, then Edit; returning to Dashboard refreshes summaries/results. Details' Back to list returns to its original parent. No inline payment, delete, categories or future Calendar integration is introduced.

### F03 Calendar rules

F03 is authorized by "implement f03" on 2026-10-18. [CA01-CA07](../specs/f03-calendar/spec.md#acceptance-scenarios) refine verification. Calendar replaces its sample tab with local saved dues; Dues remains initial.

- A day count includes all non-deleted dues for exactly that date, including Paid. The selected-day agenda shows those saved IDs in creation-time order and uses the [F02 status policy](#f02-dashboard-rules); there is no separate calendar status lifecycle. Deleted records have no marker or agenda row.
- Use a six-week grid, Monday-first by default. The later [F04 preference](#f04-preference-rules) permits an explicit saved Sunday-first layout; headers and first-date position must agree. Leading/trailing blank cells are not dates. Month arrows select day one and stop at DateOnly bounds; selecting a date outside the visible month is ignored.
- Initial selection and Today use the current Asia/Manila date. Today explicitly moves both month and selection. Appearance/resume, read completion and a visible-page minute check refresh today's marker/statuses while preserving the user's selected date/month, including across midnight. No exact-midnight or background scheduling is promised.
- Retain selected month/date across tabs, Details/editor and refresh for the current cached Shell session; reset on process restart. Loading, initial error/retry, no saved dues and empty selected-day agenda are distinct. Failed refresh retains last successful counts/agenda with a stale warning.
- A row opens F01 Details by ID; Edit Save/Cancel/Back/delete returns through Details to refreshed Calendar with selection retained. Calendar Add uses the existing form's today default, then returns to Calendar without changing the selected day; viewing another date does not silently change the new form. Root Calendar Back selects Dues. Calendar never writes directly.

### F04 preference rules

The "implement f04" request on 2026-10-18 authorizes the discussed preferences slice, refined by [PF01-PF07](../specs/f04-preferences/spec.md#acceptance-scenarios). It does not authorize the complete future Settings/export concept.

- Defaults are System appearance and Monday week start. Settings offers System/Light/Dark and Monday/Sunday only. Both choices are local UI preferences, separate from dues and reminder settings.
- Changes are drafts until explicit Save. Successful Save persists both choices together and applies the selected appearance immediately. Failed Save keeps the draft, stored values and applied preferences unchanged. Back discards unsaved choices; returning from Reminders or resuming preserves the active Settings draft.
- Apply stored appearance before constructing the main pages. System follows the OS light/dark theme without restart on supported Android versions; Light/Dark override it. Saved choices persist offline and across process restart. Missing preferences use defaults without a write; unreadable/unknown values retain the last good/default state, show a retryable error in Settings and are replaced only by explicit Save.
- Calendar uses the saved week start on its next appearance/refresh, changing headers, date alignment and explanatory caption together. Preserve current viewed month/selected day, exact date membership/counts/agenda/order and Asia/Manila rules. Week start does not alter due dates or reminder timing.
- Settings is reachable from Dues even if dues storage cannot initialize. Settings and its pushed Reminders page hide the bottom tabs; Back returns through the same stack. Opening Reminders does not opt in or change its schedule. The existing direct Reminders shortcut remains. Reminder taps defer while Settings is open so its unsaved preferences are not discarded.
- No export/restore, account, currency/timezone selector, delete-all, new bottom tab or nonfunctional About/privacy row ships in this slice. These remain separately specified work; release stays held.

### F07 category rules

The "implement f07" request on 2026-10-18 authorizes the next discussed category/type slice, refined by [CT01-CT06](../specs/f07-categories/spec.md#acceptance-scenarios). The broader F07 umbrella remains split into independent features.

- Every saved due has one category: Bill, Task, Subscription or Other. This fixed taxonomy has no custom categories or separate type field. Existing untyped dues, including tombstones, become Other without guessing their meaning or changing timestamps/other stored fields. New forms default to Bill; users can choose another category.
- Category changes persist only on Save. Cancel/Back discard them; invalid/missing selections do not write. All categories retain required nonnegative PHP amounts (including zero), current date/title/notes/payment/deletion rules, ID and creation time.
- Show the saved category on Dues, Details, Dashboard and Calendar agenda, with literal text in accessible row descriptions. Category never changes status, date order, global counts/progress or calendar date membership.
- Dashboard's single All categories/Bill/Task/Subscription/Other filter combines with title search and status using AND. Counts/progress remain global. Clear filters resets all three; category selection is retained within the cached Shell session and resets on recreation/restart, without a stored preference. Calendar remains independent of Dashboard filtering.
- Recurrence, timed dues, reminders and optional amounts are not implemented by this category slice. Release remains held.

### F05 reminder rules

The "implement next f05" request on 2026-10-18 authorizes local Android reminders, refined by [RM01-RM08](../specs/f05-reminders/spec.md#acceptance-scenarios). This resolves the earlier conflicting reminder proposals for current date-only dues.

- Reminders start off for every installation/upgrade. No permission request on launch; Enable is explicit opt-in for all saved active unpaid dues. Category does not change reminder eligibility. Denial/revocation or disabled app/channel notifications must be visible and must not block CRUD.
- Default (user-confirmed 2026-10-18): 09:00 Asia/Manila the day before and on the due date. Due-date-only is selectable; a global time is editable. Quiet hours default 22:00-08:00 Manila and are editable; equal endpoints mean no quiet period. Move reminders inside quiet hours to the next permitted time, skipping any result after the due date. Date-only dues remain dates; no timed/recurring obligations are introduced.
- New elapsed reminder times are skipped. Previously queued reminders may arrive late, only while their due date has not passed and outside quiet hours. Never deliver paid/deleted/obsolete date/settings occurrences. Delivered occurrences are not deliberately replayed by restart, repeated reconciliation or a paid/unpaid toggle.
- Settings changes are drafts until Save; Save and enable explicitly saves the draft schedule as well as opting in. A denied permission request does not save a new opt-in. Disable retains the saved schedule and cancels pending/displayed app reminders. Save/enable/disable/retry reconcile committed data. Successful due writes also reconcile; scheduler failure must not report the due write as failed. Launch/resume, reboot after unlock, app replacement and clock/zone changes rebuild scheduling from SQLite.
- Use inexact background scheduling, with no exact-alarm permission or delivery guarantee. Limit delivery to ten notifications per reconciliation pass; remaining ready work is retried no earlier than one minute later. Android delays, battery restrictions and force-stop can prevent or defer delivery; reopening retries reconciliation. No server/network is needed.
- Show Android-reported background restriction, Battery Saver and optimization state separately from notification permission. Provide an explicit Android app/battery settings action; returning refreshes status without committing drafts. Never change power policy or request a battery exemption automatically. Reported unrestricted status does not guarantee delivery, particularly under manufacturer power controls.
- Notification text is generic and contains no due title, amount or notes. A reminder tap uses an internal ID/key to open the corresponding active unpaid due through F01 Details after validation. An active editor draft is retained, with the tap deferred until leaving it. Stale/paid/deleted/invalid targets are ignored safely. Test notification is an explicit static action and does not mutate dues.
- No recurrence, per-due reminder editor, timed dues, cloud push or new lifecycle status is authorized. Release remains held.

### Existing dues workflows

| Flow | Expected outcome |
|---|---|
| Open the app | Initialize local storage, then show active dues or an empty state. Show a retryable error if storage cannot open. |
| Add → enter fields → Save | Validate before writing. Return to the list after successful persistence; show the saved due in date order. |
| Select a row → Details → Edit → Save | Read the selected due, persist edited fields with identity retained, return to refreshed Details. Back to dues reloads the list. |
| Change paid state → Save | Persist the selected state; both Paid and Unpaid remain visible in date order. |
| Back/Cancel | Discard unsaved fields without a database write. An unsaved-changes prompt is not part of approved v1 behavior. |
| Delete → Cancel | Do not change the stored due or the unsaved form. |
| Delete → confirm | Hide the due after successful persistence; retain its row. When editing from Details, return to unavailable Details, then Back to dues. See the [soft-delete spec](../specs/v1.3-soft-delete/spec.md) for mutation scenarios and remaining acceptance. |
| Return to list or resume on list | Reload committed data without appending duplicate rows. |
| Open missing/deleted due | Show that it is unavailable, prevent saving, and provide a return-to-list action. |

## Error and recovery requirements

| Condition | Required user-visible result |
|---|---|
| Invalid title, amount, overflow, or missing date | Field errors; retain input; no write. |
| Initialization, migration, or unsupported-schema failure | Preserve the database; prevent unsafe access; show a retry action rather than silently resetting data. |
| Save/delete failure | Retain the form and stored state, end the busy state, allow retry, and do not report success. |
| Refresh failure | Retain previously loaded rows with an error indicating they may be stale. |
| Duplicate taps or a busy operation | Prevent duplicate writes and overlapping actions. |
| Successful write followed by navigation failure | Explain that persistence succeeded and allow return navigation without repeating the mutation. |
| Process termination before completion | Reload committed database state on relaunch; an unsaved form is not guaranteed to survive. |

## Non-functional requirements and limits

- Design adoption for the existing list/editor was authorized on 2026-10-17: Duewise display identity, bundled typography, semantic themes, responsive layouts, and accessible component states. [Design-adoption scenarios](../specs/design-adoption/spec.md) refine verification without adding future pages or changing R01–R08. Field rules run on blur and Save; blur retains input and performs no storage write.
- Offline operation and restart persistence apply to all mandatory CRUD capabilities, including deletion.
- Database work must not block the UI; loading and error states must remain understandable.
- Paid state must have a text indication. Fields and actions need accessible labels. [DESIGN accessibility](DESIGN.md#11-accessibility) owns the broader accessibility standard; compliance has not been established by this refactor.
- Local privacy constraints include app-private storage and disabled Android backup. No account recovery, export, or server backup exists in v1; uninstalling or clearing app data loses dues. See [SDD privacy and limits](SDD.md#8-failure-handling-privacy-and-practical-limits).
- Requirements are accepted using the [test plan](delivery/TEST-PLAN.md), including real-phone checks with the intended signed Release APK. Builds or unit tests alone do not establish release acceptance.

## Exclusions and deferred capabilities

F05 is the separately authorized reminder exception above. Recurrence, authentication, Supabase/synchronization, budgets, savings, and Play publishing remain outside v1. F02 permits a computed Overdue dashboard filter; this does not implement the historical notification-reliability fallback/scheduler. CI is optional under R08.

DESIGN contains broader concepts for categories, tasks/subscriptions, timed dues, calendar, onboarding, widgets, export, settings, localization, iOS, and deletion Undo. They remain deferred; F01 Details, F02 Dashboard, F03 Calendar, the F07 category slice, F05 local reminders and F04 preferences are separately authorized exceptions. DESIGN's proposed title limit, optional amounts, completed-last sorting, immediate payment persistence and automatic timezone behavior do not replace this PRD. See [documented conflicts](delivery/DOCS-REFACTOR.md#unresolved-decisions).

The user authorized a [future-page roadmap](../specs/future-pages/plan.md) on 2026-10-17, then requested dashboard/calendar mock pages. The narrow [mock scope MP01–MP05](../specs/future-pages/spec.md#mock-acceptance-scenarios) permits labeled, interactive previews with fixed sample data and no saved-due integration. Preview categories, Completed/Overdue labels, optional task amounts and filters are illustrative, not new v1 business rules. Production future implementation still requires its own approved scope and requirement review. Release remains on hold.

The subsequent main-page bottom-navigation request authorized [BN01–BN05](../specs/future-pages/spec.md#bottom-navigation-acceptance): Dues initial, with two labeled sample tabs. The later F02 request replaces only Dashboard with saved data under the rules above. The subsequent F03 request replaces Calendar under its rules above; the editor/Details keep the bar hidden. The tab bar itself does not approve other future capabilities.

The [2026-10-18 prototype comparison](delivery/PROTOTYPE-REVIEW-2026-10-18.md) preserves the pre-F01 snapshot. Subsequent “implement f01” and “Implement f02” requests authorize Details and the saved-data Dashboard under the rules above. The subsequent F03 request authorizes saved-data Calendar under the rules above. The subsequent F07 category request authorizes the rules above. F05 subsequently authorizes the reminder rules above; F04 now authorizes the preference rules above. Recurrence, statistics, export/restore and remaining general settings, onboarding and widgets remain separate future slices. R01–R08 retain their identities and core data/payment rules; the [roadmap](../specs/future-pages/plan.md#priority-order-after-prototype-review) owns sequence. Release remains held.

## Requirement traceability

| IDs | Technical mapping | Feature detail | Verification procedures and evidence |
|---|---|---|---|
| R01 | [SDD requirement mapping](SDD.md#requirement-to-component-mapping) | No separate existing spec | [TP01](delivery/TEST-PLAN.md#device-procedures); [status](delivery/STATUS.md) |
| R02, R03 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Historical V1.1](phases/V1.1.md) | [TP02, TP06, TP07](delivery/TEST-PLAN.md#device-procedures); [V1.1 evidence](phases/V1.1.md#verification-recorded-on-2026-10-16) |
| R04, R05 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Historical V1.2](phases/V1.2.md) | [TP03, TP06, TP07](delivery/TEST-PLAN.md#device-procedures); [V1.2 evidence](phases/V1.2.md#automated-verification-2026-10-16) |
| R06 | [SDD repository contract](SDD.md#53-repository-contracts) | [Soft-delete spec](../specs/v1.3-soft-delete/spec.md) | [TP04, TP05](delivery/TEST-PLAN.md#device-procedures); [status](delivery/STATUS.md) and [V1.3 evidence](phases/V1.3.md#automated-verification-2026-10-17) |
| R07 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Soft-delete offline scenario](../specs/v1.3-soft-delete/spec.md#acceptance-scenarios) | [TP06](delivery/TEST-PLAN.md#device-procedures); attributed evidence in [status](delivery/STATUS.md) |
| R08 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [V1.4 CI spec](../specs/v1.4-ci/spec.md) | [Optional CI gate](delivery/TEST-PLAN.md#acceptance-gates); [status](delivery/STATUS.md) |
| F01 extension | [SDD navigation](SDD.md#6-ui-and-navigation) | [Details spec](../specs/f01-item-details/spec.md) | [Details procedure](delivery/TEST-PLAN.md#f01-saved-item-details-verification); [tasks](../specs/f01-item-details/tasks.md) |
| F02 extension | [SDD dashboard](SDD.md#f02-dashboard-data-and-lifecycle) | [Dashboard spec](../specs/f02-dashboard/spec.md) | [Dashboard procedure](delivery/TEST-PLAN.md#f02-dashboard-verification); [tasks](../specs/f02-dashboard/tasks.md) |
| F03 extension | [SDD calendar](SDD.md#f03-calendar-data-and-lifecycle) | [Calendar spec](../specs/f03-calendar/spec.md) | [Calendar procedure](delivery/TEST-PLAN.md#f03-calendar-verification); [tasks](../specs/f03-calendar/tasks.md) |
| F07 categories | [SDD category contract](SDD.md#f07-category-contract) | [Category spec](../specs/f07-categories/spec.md) | [Category verification](delivery/TEST-PLAN.md#f07-category-verification); [tasks](../specs/f07-categories/tasks.md) |
| F05 reminders | [SDD reminder contract](SDD.md#f05-reminder-contract) | [Reminders spec](../specs/f05-reminders/spec.md) | [Reminder verification](delivery/TEST-PLAN.md#f05-reminder-verification); [tasks](../specs/f05-reminders/tasks.md) |
| F04 preferences | [SDD preference contract](SDD.md#f04-preference-contract) | [Preferences spec](../specs/f04-preferences/spec.md) | [Preference verification](delivery/TEST-PLAN.md#f04-preference-verification); [tasks](../specs/f04-preferences/tasks.md) |
