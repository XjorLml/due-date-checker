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

- Title is required, must contain a non-whitespace character, and is trimmed when saved. There is no approved title or notes maximum length. A new limit requires an explicit product decision.
- Amount is required and nonnegative; zero is valid. Input uses digits and an optional decimal point with digits on both sides, without grouping separators, exponent notation, or currency symbols. Leading/trailing whitespace is ignored. Label it “Amount (PHP)” and provide a format hint.
- Round the amount to centavos, with midpoint values rounded away from zero, and preview the amount that will be saved. The approved storage constraint is a signed 64-bit centavo count; reject values outside that range after rounding. Technical representation and overflow handling are in the SDD.
- Due date is a required calendar date. Past dates are valid. A new form defaults to today's date in Asia/Manila. Dates must be displayed explicitly; the implemented list uses `dd MMM yyyy` and the editor uses `yyyy-MM-dd`.
- Notes are optional. Whitespace-only notes become absent; otherwise preserve their text.
- New dues start unpaid. The paid switch is available for an existing due and persists only on Save.
- Soft deletion retains the stored record, removes it from the active list, and provides no in-app restore, recycle bin, or Undo in v1.

## User workflows

| Flow | Expected outcome |
|---|---|
| Open the app | Initialize local storage, then show active dues or an empty state. Show a retryable error if storage cannot open. |
| Add → enter fields → Save | Validate before writing. Return to the list after successful persistence; show the saved due in date order. |
| Select a row → edit → Save | Load the existing due, persist edited fields with its identity retained, return and refresh the list. |
| Change paid state → Save | Persist the selected state; both Paid and Unpaid remain visible in date order. |
| Back/Cancel | Discard unsaved fields without a database write. An unsaved-changes prompt is not part of approved v1 behavior. |
| Delete → Cancel | Do not change the stored due or the unsaved form. |
| Delete → confirm | Hide the due after successful persistence; retain its row. See the [soft-delete spec](../specs/v1.3-soft-delete/spec.md) for feature scenarios and remaining acceptance. |
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

- Offline operation and restart persistence apply to all mandatory CRUD capabilities, including deletion.
- Database work must not block the UI; loading and error states must remain understandable.
- Paid state must have a text indication. Fields and actions need accessible labels. [DESIGN accessibility](DESIGN.md#11-accessibility) owns the broader accessibility standard; compliance has not been established by this refactor.
- Local privacy constraints include app-private storage and disabled Android backup. No account recovery, export, or server backup exists in v1; uninstalling or clearing app data loses dues. See [SDD privacy and limits](SDD.md#8-failure-handling-privacy-and-practical-limits).
- Requirements are accepted using the [test plan](delivery/TEST-PLAN.md), including real-phone checks with the intended signed Release APK. Builds or unit tests alone do not establish release acceptance.

## Exclusions and deferred capabilities

Reminders, notification permissions/scheduling, the Overdue fallback, recurrence, authentication, Supabase/synchronization, budgets, savings, and Play publishing remain outside v1. CI is optional under R08.

DESIGN contains broader concepts for categories, tasks/subscriptions, timed dues, dashboards, calendar, onboarding, widgets, export, settings, localization, iOS, and deletion Undo. They are not approved v1 features. Its proposed title limit, optional amounts, completed-last sorting, immediate payment persistence, and automatic timezone behavior do not replace this PRD. See [documented conflicts](delivery/DOCS-REFACTOR.md#unresolved-decisions).

## Requirement traceability

| IDs | Technical mapping | Feature detail | Verification procedures and evidence |
|---|---|---|---|
| R01 | [SDD requirement mapping](SDD.md#requirement-to-component-mapping) | No separate existing spec | [TP01](delivery/TEST-PLAN.md#device-procedures); [status](delivery/STATUS.md) |
| R02, R03 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Historical V1.1](phases/V1.1.md) | [TP02, TP06, TP07](delivery/TEST-PLAN.md#device-procedures); [V1.1 evidence](phases/V1.1.md#verification-recorded-on-2026-10-16) |
| R04, R05 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Historical V1.2](phases/V1.2.md) | [TP03, TP06, TP07](delivery/TEST-PLAN.md#device-procedures); [V1.2 evidence](phases/V1.2.md#automated-verification-2026-10-16) |
| R06 | [SDD repository contract](SDD.md#53-repository-contracts) | [Soft-delete spec](../specs/v1.3-soft-delete/spec.md) | [TP04, TP05](delivery/TEST-PLAN.md#device-procedures); [status](delivery/STATUS.md) and [V1.3 evidence](phases/V1.3.md#automated-verification-2026-10-17) |
| R07 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [Soft-delete offline scenario](../specs/v1.3-soft-delete/spec.md#acceptance-scenarios) | [TP06](delivery/TEST-PLAN.md#device-procedures); attributed evidence in [status](delivery/STATUS.md) |
| R08 | [SDD mapping](SDD.md#requirement-to-component-mapping) | [V1.4 CI spec](../specs/v1.4-ci/spec.md) | [Optional CI gate](delivery/TEST-PLAN.md#acceptance-gates); [status](delivery/STATUS.md) |
