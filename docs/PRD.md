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
| R02 | Create a due with required title, date and nonnegative PHP amount; notes are optional. Since the [F07 optional amount rules](#f07-optional-amount-rules) (implemented 2026-11-06) the amount is optional for Task/Other. | V1.1 / F07 | Two saved dues survive process termination and relaunch. A Task/Other due with no amount is acceptance for the F07 optional-amount slice, not for V1.1. |
| R03 | List all active dues by ascending due date. Since [UX02](#ux-review-decisions-2026-11-06) (2026-11-06) paid dues dated before today fold into one expandable row at the top. | V1.1 | Earlier dates precede later dates; empty state appears with no active dues; every active due stays reachable. |
| R04 | Edit title, amount, date, and notes while keeping the same identity. Clearing a Task/Other amount belongs to the F07 optional-amount slice. | V1.2 / F07 | Saved values survive relaunch; Id and CreatedAt remain unchanged. Clearing an optional amount persisting as absent is acceptance for the F07 slice. |
| R05 | Set paid/unpaid status. | V1.2 | Both status transitions persist and remain visible in the list. |
| R06 | Confirm deletion, hide the due, and retain its database row. | V1.3 | Cancel keeps the row active; confirm records DeletedAt and removes it from the list. |
| R07 | All v1 operations work offline. | All mandatory slices | CRUD acceptance tests pass in airplane mode. |
| R08 | Optionally run Core tests on push. | V1.4 | A push runs the test project successfully; CI is not a mandatory v1 feature. |

## Business rules and validation

Approved extension **F01**, authorized 2026-10-18: a saved row opens read-only details for the selected ID, showing existing title, amount, due date, computed status (Paid, Overdue, Due today or Upcoming since 2026-11-05), notes and created/updated timestamps (explicit UTC labels until [UX12](#ux-review-decisions-2026-11-06); Asia/Manila time labelled "Manila" since 2026-11-06). Details offers Mark as paid / Mark as unpaid (since [UX01](#ux-review-decisions-2026-11-06), 2026-11-06), Edit and Back to dues. Editor Save/Cancel/Back returns to reloaded Details; deletion returns to an unavailable state. Add remains direct create → Dues. Missing/deleted IDs cannot be inserted/restored; reads are retryable and never write, and the payment action writes only the payment status of an active due. [DT01–DT06](../specs/f01-item-details/spec.md#acceptance-scenarios) define feature scenarios. R01–R08 and their data/payment rules are preserved.

- Title is required, must contain a non-whitespace character, and is trimmed when saved. There is no approved title or notes maximum length. A new limit requires an explicit product decision.
- Amount is required for Bill and Subscription and optional for Task and Other, per the [F07 optional amount rules](#f07-optional-amount-rules) below. An entered amount is nonnegative; zero is valid. Input uses digits and an optional decimal point with digits on both sides, without grouping separators, exponent notation, or currency symbols. Leading/trailing whitespace is ignored. Label it “Amount (PHP)” and provide a format hint plus the category-aware requirement text; blank optional input is absent, not zero.
- Round an entered amount to centavos, with midpoint values rounded away from zero, and preview the amount that will be saved. The approved storage constraint is a signed 64-bit centavo count; reject values outside that range after rounding. An absent amount is stored as SQL NULL and displayed as “No amount” per the F07 optional-amount rules; zero is never shown that way. Technical representation and overflow handling are in the SDD.
- Due date is a required calendar date. Past dates are valid. A new form defaults to today's date in Asia/Manila. Dates must be displayed explicitly; the implemented list uses `dd MMM yyyy`, and since [UX07](#ux-review-decisions-2026-11-06) (2026-11-06) the editor does too (it used `yyyy-MM-dd` before), with English month abbreviations whatever the phone's language.
- Notes are optional. Whitespace-only notes become absent; otherwise preserve their text.
- New dues start unpaid. The paid switch is available for an existing due and persists only on Save.
- Soft deletion retains the stored record, removes it from the active list, and provides no in-app restore or recycle bin in v1. The one exception, since [UX06](#ux-review-decisions-2026-11-06) (2026-11-06): right after Delete, a message offers Undo for 5 seconds, which restores exactly that deletion.

## User workflows

### F02 Dashboard rules

F02 is authorized by “Implement f02” on 2026-10-18. The Dashboard now uses local saved dues; Dues remains the initial root and its R03 date-sorted list/payment contracts remain unchanged. Since the [prototype alignment](#prototype-alignment-decisions-2026-11-07) (decided 2026-11-07) the search and filter rules below apply on Dues instead of Dashboard. [DB01–DB07](../specs/f02-dashboard/spec.md#acceptance-scenarios) refine verification.

- Include every non-deleted due, including Paid. A deleted due has no dashboard status or count. Status is derived, never stored: Paid takes precedence; unpaid dates before today's Asia/Manila calendar date are Overdue, equal dates are Due today, and later dates are Upcoming. These four groups are mutually exclusive; Upcoming excludes today. Keep Paid terminology, not the mock's Completed lifecycle.
- Summary counts and paid/total progress use all active saved dues across dates, regardless of search/filter. Empty progress is zero. They are not monthly spending or payment-history statistics.
- The summary's upcoming total (adopted 2026-11-05 with the [Focus page content](../specs/focus-pages/spec.md)) sums the known amounts of unpaid dues dated from today through the last day of today's Asia/Manila month and states that count; the "next date" names the earliest unpaid due on or after today and opens its Details. It is a planning figure from saved dues, not spending, a bank balance or payment history; paid dues are excluded, and a due without an amount ([F07 optional amount rules](#f07-optional-amount-rules)) is excluded from the sum but still counted among the unpaid dues, and the summary states how many were excluded.
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

- Defaults are System appearance and Monday week start. Settings offers System/Light/Dark and Monday/Sunday only. Both choices are local UI preferences, separate from dues and reminder settings. Since 2026-11-05 the Settings Reminders entry also shows the saved reminder state (on/off, time and day-before) read-only; it never changes reminder settings, and an unreadable state falls back to a plain "Open due-date reminders" label.
- Changes are drafts until explicit Save. Successful Save persists both choices together and applies the selected appearance immediately. Failed Save keeps the draft, stored values and applied preferences unchanged. Back discards unsaved choices; returning from Reminders or resuming preserves the active Settings draft.
- Apply stored appearance before constructing the main pages. System follows the OS light/dark theme without restart on supported Android versions; Light/Dark override it. Saved choices persist offline and across process restart. Missing preferences use defaults without a write; unreadable/unknown values retain the last good/default state, show a retryable error in Settings and are replaced only by explicit Save.
- Calendar uses the saved week start on its next appearance/refresh, changing headers, date alignment and explanatory caption together. Preserve current viewed month/selected day, exact date membership/counts/agenda/order and Asia/Manila rules. Week start does not alter due dates or reminder timing.
- Settings is a root tab since the [2026-11-05 Focus adoption exception](#exclusions-and-deferred-capabilities); it is reachable even if dues storage cannot initialize and shows the bottom bar. Its pushed Reminders page hides the bar and Back returns to Settings; Back from Settings selects Dues. Opening Reminders does not opt in or change its schedule. The existing direct Reminders shortcut on Dues remains. Reminder taps defer while Settings is open so its unsaved preferences are not discarded.
- No export/restore, account, currency/timezone selector, delete-all or nonfunctional About/privacy row ships in this slice (export and import later shipped as [UX05](#ux-review-decisions-2026-11-06)). The Settings tab itself was added later by the Focus adoption exception, not by F04. These remain separately specified work; release stays held.

### F07 category rules

The "implement f07" request on 2026-10-18 authorizes the next discussed category/type slice, refined by [CT01-CT06](../specs/f07-categories/spec.md#acceptance-scenarios). The broader F07 umbrella remains split into independent features.

- Every saved due has one due type: Bill, Task, Subscription or Other. Its persisted legacy name is `category`; numeric type codes remain unchanged. The separate Philippine preset field below was authorized 2026-11-08. Existing untyped dues, including tombstones, become Other without guessing their meaning or changing timestamps/other stored fields. New forms default to Bill; users can choose another due type.
- Category changes persist only on Save. Cancel/Back discard them; invalid/missing selections do not write. Bill and Subscription require a nonnegative PHP amount; Task and Other may omit it as refined below. Current date/title/notes/payment/deletion rules, ID and creation time remain.
- Show the saved category on Dues, Details, Dashboard and Calendar agenda, with literal text in accessible row descriptions. Category never changes status, date order, global counts/progress or calendar date membership.
- Dashboard's single All categories/Bill/Task/Subscription/Other filter combines with title search and status using AND. Counts/progress remain global. Clear filters resets all three; category selection is retained within the cached Shell session and resets on recreation/restart, without a stored preference. Calendar remains independent of Dashboard filtering.
- Recurrence, timed dues and reminders are not implemented by this category slice. Optional amounts are a separate authorized F07 slice below. Release remains held.

### Philippine preset category rules

Authorized 2026-11-08 by the user's implementation request and explicit choice **preset Philippine categories only**. [PC01-PC08](../specs/ph-categories-native/spec.md#acceptance) refine acceptance; the native [catalog](../src/DueDateChecker.Core/Domain/PhilippineCategories.cs) owns the 53 stable IDs/labels in 10 groups, copied from the HTML reference.

- A due may select a detailed Philippine category independently of its due type. Other is the optional fallback and the default for existing records; never infer a category from titles or alter saved amounts, statuses or timestamps during upgrade.
- The searchable grouped picker includes common Filipino search terms. Selection changes the editor draft; picker Cancel/Back keeps the preceding choice. Save commits it; form dirty-exit protection includes category-only changes. Core title/amount/date fields and sticky actions remain early/reachable.
- Choosing Electricity, SSS, Rent or another preset must not change Bill/Subscription/Task/Other, amount requirements or repeat eligibility. Existing type filters retain their meaning.
- Saved rows and Details identify the detailed category, while type remains explicit in Details/accessibility. Recurring next occurrences retain it; changing it invalidates stale notification payment actions.
- Export/import preserves detailed categories, including tombstones. Earlier exports remain readable with Other as the detailed fallback; new exports require a version capable of reading the extended format.
- Custom categories, managers, billers/providers, payment connections and category-derived financial/schedule rules remain outside this preset-only authorization.

### F07 optional amount rules

The 2026-11-03 F07-extension decision authorizes optional amounts, refined by [OA01-OA06](../specs/f07-optional-amounts/spec.md#acceptance-scenarios). This does not authorize recurrence, timed dues or per-due reminders.

**Status, 2026-11-06: implemented** with automated and emulator verification; phone acceptance is pending (the phone currently holds a build signed by another computer). See the [SDD contract](SDD.md#f07-optional-amount-contract), [tasks](../specs/f07-optional-amounts/tasks.md) and [STATUS](delivery/STATUS.md).

- Bill and Subscription require a nonnegative PHP amount. Task and Other may be saved without an amount. Blank or whitespace-only optional input is absent, not zero; explicit zero remains valid for every category.
- The editor shows whether the selected category requires an amount. Saving a blank Bill/Subscription is a field error and performs no write; saving a blank Task/Other persists SQL NULL. Changing category re-evaluates a blank amount, and an existing Task/Other amount can be cleared on Save.
- Present amounts retain the existing centavo rounding/validation and `₱N2` display. An absent amount displays literally as “No amount” on Dues, Dashboard, Details and Calendar agenda, and the literal text is included in accessible row descriptions.
- Absent amounts do not change status, date order, counts/progress, filtering or reminder eligibility. No amount totals, statistics, budgets or inferred amounts are introduced; a future amount-based feature must define how absent values are excluded or represented.
- Schema 4 makes the stored centavo value nullable while preserving all existing rows, categories, timestamps, tombstones and the active-date index. The requirement is enforced by the domain, editor and repository write guard, not by a new category-dependent SQL CHECK.
- Recurrence and timed dues remain deferred. Release remains held.

### F05 reminder rules

The "implement next f05" request on 2026-10-18 authorizes local Android reminders, refined by [RM01-RM08](../specs/f05-reminders/spec.md#acceptance-scenarios). This resolves the earlier conflicting reminder proposals for current date-only dues.

- Reminders start off for every installation/upgrade. No permission request on launch; Enable is explicit opt-in for all saved active unpaid dues. Category does not change reminder eligibility. Denial/revocation or disabled app/channel notifications must be visible and must not block CRUD.
- Default (user-confirmed 2026-10-18): 09:00 Asia/Manila the day before and on the due date. Due-date-only is selectable; a global time is editable. Quiet hours default 22:00-08:00 Manila and are editable; equal endpoints mean no quiet period. Move reminders inside quiet hours to the next permitted time, skipping any result after the due date. Date-only dues remain dates; no timed/recurring obligations are introduced.
- New elapsed reminder times are skipped. Previously queued reminders may arrive late, only while their due date has not passed and outside quiet hours. Never deliver paid/deleted/obsolete date/settings occurrences. Delivered occurrences are not deliberately replayed by restart, repeated reconciliation or a paid/unpaid toggle.
- Settings changes are drafts until Save; Save and enable explicitly saves the draft schedule as well as opting in. A denied permission request does not save a new opt-in. Disable retains the saved schedule and cancels pending/displayed app reminders. Save/enable/disable/retry reconcile committed data. Successful due writes also reconcile; scheduler failure must not report the due write as failed. Launch/resume, reboot after unlock, app replacement and clock/zone changes rebuild scheduling from SQLite.
- Use inexact background scheduling, with no exact-alarm permission or delivery guarantee. Limit delivery to ten notifications per reconciliation pass; remaining ready work is retried no earlier than one minute later. Android delays, battery restrictions and force-stop can prevent or defer delivery; reopening retries reconciliation. No server/network is needed.
- Show Android-reported background restriction, Battery Saver and optimization state separately from notification permission. Provide an explicit Android app/battery settings action; returning refreshes status without committing drafts. Never change power policy or request a battery exemption automatically. Reported unrestricted status does not guarantee delivery, particularly under manufacturer power controls.
- Reminder content and payment actions follow [N1/N2](#notification-decisions-2026-11-07): details when unlocked, a generic public lock-screen version, and authenticated Mark as paid with guarded Undo. Notes are never shown. A reminder body tap validates its delivered ID/key before opening F01 Details; a payment confirmation also validates the saved snapshot and may open the now-paid due. An active editor draft is retained, with navigation deferred until leaving it. Obsolete/deleted/invalid targets are ignored safely. If payment status changed while an editor was open, Save retains the draft and asks the user to reopen the latest version instead of overwriting that payment. Test notification remains explicit, static and read-only. Routine refresh preserves it even with no eligible dues; disabling reminders or blocking access clears app notifications.
- No per-due reminder editor, timed dues, cloud push or new lifecycle status is authorized. Recurring dues come only from the [UX03 rule](#ux-review-decisions-2026-11-06); each occurrence is an ordinary due for reminders. Release remains held.

### Existing dues workflows

| Flow | Expected outcome |
|---|---|
| Open the app | Initialize local storage, then show active dues or an empty state. Show a retryable error if storage cannot open. |
| Add → enter fields → Save | Validate before writing. Return to the list after successful persistence; show the saved due in date order. |
| Select a row → Details → Edit → Save | Read the selected due, persist edited fields with identity retained, return to refreshed Details. Back to dues reloads the list. |
| Change paid state → Save, or Details → Mark as paid / Mark as unpaid | Persist the selected state; paid and unpaid dues remain in date order with their status badge (paid dues dated before today fold under UX02). |
| Any list or Details | Dues rows, Dashboard rows, Calendar agenda rows and Details show one computed status, Paid, Overdue, Due today or Upcoming, under the [F02 rules](#f02-dashboard-rules) for the current Asia/Manila date (UI consistency slice, 2026-11-05). Paid remains a text indication; the status is derived on load and never stored. |
| Back/Cancel | Discard unsaved fields without a database write. An unsaved-changes prompt is not part of approved v1 behavior. |
| Delete → Cancel | Do not change the stored due or the unsaved form. |
| Delete → confirm | Hide the due after successful persistence; retain its row. When editing from Details, return to unavailable Details, then Back to dues. See the [soft-delete spec](../specs/v1.3-soft-delete/spec.md) for mutation scenarios and remaining acceptance. |
| Return to list or resume on list | Reload committed data without appending duplicate rows. |
| Open missing/deleted due | Show that it is unavailable, prevent saving, and provide a return-to-list action. |

### UX review decisions (2026-11-06)

Decided by the user on 2026-11-06 from the [UX review](delivery/UX-REVIEW-2026-11-06.md) options ([decision log](../specs/ux-improvements/spec.md#decision-log)). **Status: UX01 and UX02 implemented 2026-11-06 ([Wave 1 evidence](delivery/evidence/UX-WAVE1-2026-11-06.md)); UX03 and UX04 implemented 2026-11-06 ([Wave 2 evidence](delivery/evidence/UX-WAVE2-2026-11-06.md)); UX05 implemented 2026-11-06 ([Wave 3 evidence](delivery/evidence/UX-WAVE3-2026-11-06.md)); UX06–UX12 implemented 2026-11-06, UX09 needing no change ([Wave 4 evidence](delivery/evidence/UX-WAVE4-2026-11-06.md)); automated and emulator evidence for all four waves, phone pending. UX13–UX16 implemented 2026-11-07 ([Wave 5 evidence](delivery/evidence/UX-WAVE5-2026-11-07.md)); every UX decision has now shipped, with phone checks pending.** Each rule below takes effect when its item ships under the [UX improvements plan](../specs/ux-improvements/plan.md); until then the rule it replaces stays in force.

| ID | Approved behavior | Replaces when shipped |
|---|---|---|
| UX01 | Details shows a primary "Mark as paid" action for an unpaid due and "Mark as unpaid" for a paid one. One tap persists the payment status with the same identity and creation time and refreshes the status; Edit due stays available. | F01 "Details offers Edit and Back to dues" (adds this action) |
| UX02 | Dues folds paid dues dated before today's Asia/Manila date into one "Show N earlier paid dues" row at the top; expanding shows them in place in date order. Unpaid dues are never folded. The rest of the list keeps ascending date order. Expansion resets on restart. | R03 presentation (all active dues remain reachable) |
| UX03 | A Bill or Subscription can repeat Weekly, Monthly or Yearly (default Never); Task and Other do not repeat. Marking a repeating due paid creates the next occurrence with the same title, amount, type and notes on the next date; a day that does not exist in the next month falls back to that month's last day. Each due creates at most one next occurrence, so paid → unpaid → paid adds nothing more. Scenarios: [RC01–RC08](../specs/f07-recurrence/spec.md#acceptance-scenarios). | Recurrence exclusion below (shipped 2026-11-06) |
| UX04 | With reminders on, the Reminders page adds plain steps for phone makers known to stop background work, starting with Xiaomi, Redmi and POCO, beside the existing status and settings action. | None (shipped 2026-11-06) |
| UX05 | Settings offers "Export dues" to a file the user chooses (all dues including deleted ones, with absent amounts kept absent) and "Import" from such a file into an empty app only. Nothing leaves the phone unless the user moves the file. The file is JSON carrying a format version number; files from an unknown or newer version are refused without changing data. "Empty" means no dues on screen (decided 2026-11-06): hidden deleted dues do not block an import, and a file due with the same ID replaces the hidden one. The whole file is checked before anything is written and is imported completely or not at all. Reminder settings and preferences are not part of the file. Scenarios: [EX01–EX08](../specs/f04-export-import/spec.md#acceptance-scenarios). | Export exclusion below (shipped 2026-11-06) |
| UX06 | After Save, Mark as paid/unpaid and Delete a short confirmation appears and is announced to screen readers. After Delete it offers **Undo for 5 seconds**, which restores the due with its saved details. There is still no recycle bin or later restore. Scenarios: [CM01–CM08](../specs/ux06-confirmations-undo/spec.md#acceptance-scenarios). | "No in-app restore, recycle bin, or Undo in v1" (Undo only, within 5 seconds; shipped 2026-11-06) |
| UX07 | The editor shows the due date as `dd MMM yyyy`. | "the editor uses `yyyy-MM-dd`" (shipped 2026-11-06) |
| UX12 | Details shows Created and Last updated in Asia/Manila time, labelled as Manila. | F01 "explicit UTC labels" (shipped 2026-11-06) |

UX08, UX10, UX11, UX13–UX16 are presentation decisions recorded in [DESIGN](DESIGN.md#ux-review-decisions-2026-11-06); UX09 kept the Dues and Dashboard roles until the prototype alignment below reversed it.

### Prototype alignment decisions (2026-11-07)

Decided by the user on 2026-11-07 when asking to bring the app in line with the live HTML prototype ([prototype alignment spec](../specs/prototype-alignment/spec.md)). The scope is look and layout of existing features; the prototype's new features (detailed categories, billers, per-due reminder offsets, the Calendar Week view, Reschedule) are not approved. Where the prototype predates a UX01–UX16 decision, the decision stands. **Status: implemented 2026-11-07 with automated and emulator evidence ([evidence](delivery/evidence/PROTOTYPE-ALIGNMENT-2026-11-07.md)); phone check pending.** The rules below are in force.

| ID | Approved behavior | Replaces when shipped |
|---|---|---|
| UX09 (revised) | Search and the status and type filters move from Dashboard to Dues and keep the [F02](#f02-dashboard-rules) search and filter rules (trimmed case-insensitive title substring; one status; one type; AND; matching count; Reset clears all; retained for the session). Dashboard keeps its summary, counts and next date, and adds two short lists: unpaid dues dated today or earlier ("Needs attention") and unpaid dues after today up to month end ("A little further ahead"), three each; its counters open Dues filtered to that status. On Dues, UX02's fold applies only while no search or filter is active. | F02 search and filters on Dashboard; UX09 "keep as is" |
| UX22 | When the editor holds changes, Cancel, the header and system Back ask "Discard changes?" with Keep editing (the default) and Discard. Without changes they leave at once. A reminder tap still waits until the editor is left. | Workflow "Back/Cancel": "An unsaved-changes prompt is not part of approved v1 behavior" |

UX19 (Reminders layout), UX24 (calendar counts as "1 due" / "2 dues"), UX25 (Settings keeps its Save; the F04 draft rule is unchanged) and the lowercase "duewise" header are presentation decisions recorded in [DESIGN](DESIGN.md#prototype-alignment-2026-11-07). The other round 2 items (UX17, UX18, UX20, UX21, UX23, UX26, UX27) remain open in [UX improvements, round 2](../specs/ux-improvements-2/spec.md).

### Notification decisions (2026-11-07)

Decided by the user on 2026-11-07 from the [notification lab](../specs/notification-lab/spec.md) (export pasted in the chat, no notes). **Native implementation authorized and added 2026-11-08.** Verification and remaining device acceptance are tracked in [delivery evidence](delivery/evidence/NOTIFICATIONS-2026-11-08.md); implementation does not establish lock-screen or phone acceptance.

| ID | Approved behavior | Replaces when shipped |
|---|---|---|
| N1 | A reminder names its due when the phone is unlocked. The title reads "Rent is due today" or "… is due tomorrow". The text gives the amount, or "Task, no amount", then the date ("Thu 05 Nov") and, for a repeating due, "repeats monthly". Notes are never shown. On the lock screen, Android shows a public version that says only "Duewise · You have a due today" ("… 2 dues …" when several arrive together) until the phone is unlocked. Reminders arriving together are grouped. | F05 "Notification text is generic and contains no due title, amount or notes." |
| N2 | A reminder has one **Mark as paid** action. After the phone is unlocked (Android asks first), it saves the due as paid without opening Duewise. This is the same guarded save as Details' Mark as paid: same identity, a repeating due adds its next occurrence once, and reminders reconcile. It acts only if the due is still active, unpaid and on the reminder's date; otherwise nothing is written and the notification says "Open Duewise to check this due." On success the notification reads "Rent marked paid", plus "Next one added for 05 Dec 2026" for a repeating due, with **Undo** for 10 seconds. Undo marks the due unpaid only if nothing changed it since; a next occurrence already added stays, as with Details' Mark as unpaid. Tapping the notification still opens the due's Details. | F05: a reminder tap is the only notification interaction (adds an action) |

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
- Local privacy constraints include app-private storage and disabled Android backup. No account recovery or server backup exists in v1. Since UX05 (2026-11-06) the user can export every due to an unencrypted JSON file they choose and import it later; dues not exported are lost when the app is uninstalled or its data cleared. See [SDD privacy and limits](SDD.md#8-failure-handling-privacy-and-practical-limits).
- Requirements are accepted using the [test plan](delivery/TEST-PLAN.md), including real-phone checks with the intended signed Release APK. Builds or unit tests alone do not establish release acceptance.

## Exclusions and deferred capabilities

F05 is the separately authorized reminder exception above. Recurrence beyond the shipped [UX03 rule](#ux-review-decisions-2026-11-06) (series editing, skipping, end dates, custom intervals, per-series reminders), authentication, Supabase/synchronization, budgets, savings, and Play publishing remain outside v1. F02 permits a computed Overdue dashboard filter; this does not implement the historical notification-reliability fallback/scheduler. CI is optional under R08.

DESIGN contains broader concepts for categories, tasks/subscriptions, timed dues, calendar, onboarding, widgets, export, settings, localization, iOS, and deletion Undo. They remain deferred; F01 Details, F02 Dashboard, F03 Calendar, the F07 category slice, F07 optional amounts, F05 local reminders, F04 preferences and the UX01–UX12 decisions are separately authorized exceptions. DESIGN's proposed title limit, completed-last sorting, immediate payment persistence and automatic timezone behavior do not replace this PRD. The optional-amount concept is now governed by the [F07 optional amount rules](#f07-optional-amount-rules), not the future design wording. See [documented conflicts](delivery/DOCS-REFACTOR.md#unresolved-decisions).

The [2026-11-06 UI/UX review](delivery/UX-REVIEW-2026-11-06.md) led to user decisions on the same day, recorded in [UX review decisions](#ux-review-decisions-2026-11-06). UX01–UX12 have shipped (UX09 needed no change), including export and import and the 5-second Undo after Delete; the rules they replaced are marked above. UX13–UX16, presentation only, shipped on 2026-11-07.

A [second review on 2026-11-07](delivery/UX-REVIEW-2026-11-07.md) recorded UX17–UX27 with options in [UX improvements, round 2](../specs/ux-improvements-2/spec.md). **None is decided, and no rule in this PRD changes until the user decides.** The rules the options would touch are F05 "Reminders start off" (UX17 B/C), the F02 summary rules (UX18, UX23), UX05 (UX21), the "Back/Cancel" workflow row (UX22), the F04 draft-until-Save rule (UX25) and F01 "Back to dues" (UX26).

The user authorized a [future-page roadmap](../specs/future-pages/plan.md) on 2026-10-17, then requested dashboard/calendar mock pages. The narrow [mock scope MP01–MP05](../specs/future-pages/spec.md#mock-acceptance-scenarios) permits labeled, interactive previews with fixed sample data and no saved-due integration. Preview categories, Completed/Overdue labels, optional task amounts and filters are illustrative, not new v1 business rules. Production future implementation still requires its own approved scope and requirement review. Release remains on hold.

The subsequent main-page bottom-navigation request authorized [BN01–BN05](../specs/future-pages/spec.md#bottom-navigation-acceptance): Dues initial, with two labeled sample tabs. The 2026-11-05 Focus adoption below extends that bar to five slots. The later F02 request replaces only Dashboard with saved data under the rules above. The subsequent F03 request replaces Calendar under its rules above; the editor/Details keep the bar hidden. The tab bar itself does not approve other future capabilities.

The [2026-10-18 prototype comparison](delivery/PROTOTYPE-REVIEW-2026-10-18.md) preserves the pre-F01 snapshot. Subsequent “implement f01” and “Implement f02” requests authorize Details and the saved-data Dashboard under the rules above. The subsequent F03 request authorizes saved-data Calendar under the rules above. The subsequent F07 category request authorizes the rules above. F05 subsequently authorizes the reminder rules above; F04 now authorizes the preference rules above. The 2026-11-03 F07-extension decision authorizes the optional-amount rules above; recurrence and timed dues remain separate. Statistics, export/restore and remaining general settings, onboarding and widgets remain separate future slices. R01–R08 retain their identities and core data/payment rules; the [roadmap](../specs/future-pages/plan.md#priority-order-after-prototype-review) owns sequence. Release remains held.

The 2026-11-04 HTML design exploration under `prototypes/design-exploration/` proposed capabilities not authorized by that artifact alone: four-tab Dashboard-first navigation with a centered Add action, a Philippine category/provider catalog with custom categories, repeat controls, a weekly Calendar view, amount totals and charts, multiple reminder offsets and quick-paid confirmation. They are recorded as proposals in the [DESIGN register](DESIGN.md#design-explorations-non-normative) and governed by HTML-only specs. The artifact itself did not authorize native implementation or change the requirements above. Subsequent explicit authorizations govern implemented features, including the preset-only Philippine categories; custom categories and billers/providers remain deferred.

**Focus adoption exception, 2026-11-05.** The user authorized adopting the exploration's selected Focus look and navigation natively, scope look and navigation only ([spec](../specs/focus-adoption/spec.md), [ADR 008](adr/008-focus-design-baseline.md)). The bottom bar has five slots, Dashboard / Calendar / Add / Dues / Settings: Add is an action that opens the editor on the current root and returns there on Save or Cancel, and Settings is a root tab that shows the bar. This supersedes the F04 wording that Settings is pushed from Dues with hidden tabs and F04's "no new bottom tab" exclusion; Settings stays reachable when dues storage cannot initialize, Reminders pushed from Settings still hides the bar, and reminder taps still defer while Settings is current. Dues remains the start tab and the root Back target. The editor orders title, amount and date first, keeps Cancel and Save in a fixed footer and moves Delete to the header; its validation, Save-on-payment and confirmed-deletion rules are unchanged. That adoption alone did not authorize the catalog, repeats, weekly Calendar, totals, multi-offset reminders or Dashboard-first entry. Later feature approvals above supersede this historical deferral list, including the 2026-11-08 preset-only Philippine category extension.

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
| F07 Philippine presets | [SDD preset contract](SDD.md#philippine-preset-category-contract) | [Native preset spec](../specs/ph-categories-native/spec.md) | [Preset verification](delivery/TEST-PLAN.md#native-philippine-preset-verification); [tasks](../specs/ph-categories-native/tasks.md) |
| F07 optional amounts | [SDD optional amount contract](SDD.md#f07-optional-amount-contract) | [Optional amount spec](../specs/f07-optional-amounts/spec.md) | [Optional amount verification](delivery/TEST-PLAN.md#f07-optional-amount-verification); [tasks](../specs/f07-optional-amounts/tasks.md) |
| F05 reminders | [SDD reminder contract](SDD.md#f05-reminder-contract) | [Reminders spec](../specs/f05-reminders/spec.md) | [Reminder verification](delivery/TEST-PLAN.md#f05-reminder-verification); [tasks](../specs/f05-reminders/tasks.md) |
| F04 preferences | [SDD preference contract](SDD.md#f04-preference-contract) | [Preferences spec](../specs/f04-preferences/spec.md) | [Preference verification](delivery/TEST-PLAN.md#f04-preference-verification); [tasks](../specs/f04-preferences/tasks.md) |
