# Verification plan

Authority: verification procedures and acceptance gates. Results belong in [STATUS](STATUS.md) and linked execution/phase records. Requirements are defined in [PRD](../PRD.md); contracts in [SDD](../SDD.md).

## Environments and evidence

- Automated: .NET 10 SDK, isolated temporary SQLite files, and the repository's Core/App test projects. The App harness links platform-independent view models and the dispatcher; it does not load MAUI pages.
- Native acceptance: a real supported Android phone, intended-key signed Release APK, and controlled test dues. Record device model/Android version, APK identity, signing verification, source revision or working-tree snapshot, date, procedure, expected/actual outcome, and observer.
- Use an authorized test database/build for destructive or storage-failure scenarios. Preserve unrelated dues. Physical tombstone verification requires access to that test database; inability to inspect it leaves that evidence pending.
- Record agent-observed and user-reported results separately. Historical test counts do not prove the current tree passes. Do not check off tests merely because definitions exist.

## Automated execution

From the repository root, when validating an implementation change:

```powershell
dotnet test tests/DueDateChecker.Core.Tests/DueDateChecker.Core.Tests.csproj
dotnet test tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release
```

Core checks do not require restoring the MAUI App. Android builds are separate from unit tests. Expected outcome: relevant tests and Release build pass; warnings and environment limitations are recorded. A build does not establish intended-key signing or phone acceptance. Documentation-only changes require documentation checks, not these commands.

## Automated coverage inventory

These are inspected test definitions, not new execution results. Historical runs are linked from STATUS.

| Requirements / contract | Sources and coverage | Remaining limits |
|---|---|---|
| R02, R03, R07 | [CreateAndListTests](../../tests/DueDateChecker.App.Tests/CreateAndListTests.cs): Manila date boundary; reopened-file persistence/order; PHP/timezone defaults; required fields; amount limits/overflow; retries; duplicate Save; startup/read failures | Android lifecycle, input controls, and airplane-mode acceptance require a phone. |
| R04, R05 | [EditDueTests](../../tests/DueDateChecker.App.Tests/EditDueTests.cs): load/no-write, edit/paid reopen, identity/timestamps, refreshed order/status, invalid IDs/fields, stale deletion, read/write retry, repeated load/Save | Native navigation, XAML binding, and trimming need Release/device checks. |
| R06 | [DeleteDueTests](../../tests/DueDateChecker.App.Tests/DeleteDueTests.cs): cancel without storage access, persisted tombstone/reopen/list refresh, invalid unsaved fields, missing/deleted targets, failure/retry, repeated Delete/Save, navigation-retry state, concurrent deletion | Native confirmation display/dismissal, page action handling, and lifecycle are outside this harness. |
| R02–R06 / persistence | [SqliteDueRepositoryTests](../../tests/DueDateChecker.Core.Tests/SqliteDueRepositoryTests.cs): initialization/idempotence, round trips, date sorting, missing IDs, active-row update safeguards, retained tombstones, repeated/unknown delete, newer-schema data preservation | F07 CategoryTests add legacy upgrade and duplicate-column migration-failure preservation; mid-script rollback and explicit repository equal-due-date/created_at ordering remain unverified. |
| Domain rules | [MoneyTests](../../tests/DueDateChecker.Core.Tests/MoneyTests.cs), [DueTests](../../tests/DueDateChecker.Core.Tests/DueTests.cs): rounding, negative amounts, formatting/value equality, create defaults, title validation/trim, `with` validation, identity | No inferred coverage beyond these tests. |

F05 automated contract: Core [ReminderTests](../../tests/DueDateChecker.Core.Tests/ReminderTests.cs) cover default/off and due-date-only planning, Manila instants, quiet-hour variants/bounds, culture-invariant identities, invalid settings, schema-2 preservation, receipt persistence and transactional pending rollback. App [ReminderTests](../../tests/DueDateChecker.App.Tests/ReminderTests.cs) cover reconciliation/restart replay, late versus newly elapsed work, paid/date/delete/disable/revocation cancellation, bounded posting, controlled platform failures, post-commit write safety, delivered-key validation and settings draft/permission/test boundaries. These use a fake platform; they do not execute Android APIs, Shell or permission dialogs.

Temporary database tests should close/clear pooled connections before cleanup. Test failure behavior and persistent outcomes rather than generated MVVM boilerplate. A future migration failure test needs a controlled seam; do not modify shipped migrations to simulate failure.

## Device procedures

Precondition for the final release pass: verified intended-key signed Release APK installed on a real phone. Historical checks with other artifacts remain useful evidence but do not close that gate.

| ID / requirements | Procedure | Expected outcome |
|---|---|---|
| TP01 / R01 | Verify APK signing identity and merged manifest; install/update, then launch from the launcher. | App opens; intended signing identity confirmed; allowBackup=false and supported Android configuration confirmed. Explicit activity-start alone is insufficient. |
| TP02 / R02, R03 | Add two test dues in reverse date order, including zero amount, a past date, and optional notes. Terminate/relaunch. Also view a test database with no active dues. | Both saved records retain correct fields and ascending date order; valid past/zero values persist; empty state appears only with no active rows and no error. |
| TP03 / R04, R05 | Edit every field; Save and relaunch. Mark paid, Save/relaunch, then unpaid and repeat. Separately change text/date and Cancel/back, then reopen. | Saved changes persist; identity/creation time retained; both states remain listed in date order; canceled changes are absent. |
| TP04 / R06 | Open an existing due, alter unsaved fields, choose Delete, then Cancel or dismiss. Reopen confirmation and confirm; return to list, terminate/relaunch. Inspect the authorized test database. | Cancel/dismiss does not write or discard form input. Confirm hides the row; original stored fields remain with deletion/update timestamps. It stays hidden after restart. No Undo/restore is offered. |
| TP05 / R06 | Delete the last active due; repeat taps while confirmation/write is pending. In a controlled test environment, fail confirmation display, storage write, and return navigation separately, then retry. | Correct empty state; no competing mutation; confirmation failure makes no write; storage failure retains form and permits retry; navigation retry after successful delete does not mutate again. |
| TP06 / R07 | Enable airplane mode and repeat TP02–TP05, including restart after each successful mutation. | Create/list/edit/paid/delete work locally and persist; no network or account is required. |
| TP07 / R02–R06 | Enter invalid/overflow amounts, blank title, missing date; inspect preview, keyboard/date picker, field errors and Cancel. Repeat Save taps. Exercise controlled initialization/read/write failures and retry. Check labels, paid text, TalkBack, large text, and themes against DESIGN for these screens. | No invalid or duplicate write, false success, or lost form input on failure; startup blocks unsafe access; refresh failure keeps rows with a stale-data error; controls remain understandable. Record design/accessibility gaps rather than claiming full compliance. |

Unknown/deleted targets must not become a new due. Process termination before a write completes must reload committed state without promising recovery of unsaved fields.

## Acceptance gates

Mandatory v1 acceptance requires passing relevant automated checks, a Release build, TP01 with the intended signing identity, and completion of the CRUD/restart/offline/native checks. Keep failures and unchecked items visible in STATUS. User acceptance of V1.2 does not close V1.1 or V1.3 gates.

Optional CI gate (R08): a push triggers `.github/workflows/ci.yml`, which runs the Core and App test projects on a GitHub-hosted Linux runner and reports the result on the commit. The gate passes on an observed passing run; see the [V1.4 spec](../../specs/v1.4-ci/spec.md#acceptance-scenarios). A passing run is not device or release acceptance. Notification, Doze, boot-receiver, sync, and notification deep-link checks are excluded from v1.

## Design-adoption verification

Scope and expected outcomes are owned by [DA01–DA10](../../specs/design-adoption/spec.md#acceptance-scenarios). Use the automated commands above (sequentially, since test projects share Core output) and a fresh Android Release build after UI changes. Inspect the APK for the four local font weights/license and the merged manifest for Duewise display identity, unchanged ApplicationId and disabled backup.

On a supported phone, update with data preserved; never uninstall/clear data to bypass signing failure. Inspect list, new form, existing editor and validation errors in both system themes at normal and 200% text, scrolling all actions into view. Check focus/error outlines, wrapped amounts/dates/action labels, persistent labels, paid text, field preview, native picker, primary/secondary/destructive roles, loading/error/empty state and TalkBack/keyboard navigation. Use temporary dues only for create/edit/paid/delete/restart/offline checks. Restore original theme/font/radio settings after testing. Capture only controlled verification UI, without persisting existing user financial details.

Record actual source/artifact hashes, SDK/device, theme/text scale, exact checks and observer in the design-adoption evidence linked from STATUS. Unperformed widths, TalkBack, controlled failures and final intended-key acceptance remain open. Historical V1.3/core-flow evidence is not a native pass for the changed UI artifact. Building a Release configuration is local verification; it does not release the app.

## Dashboard/calendar mock verification

Original scope: [MP01-MP05](../../specs/future-pages/spec.md#mock-acceptance-scenarios). F02/F03 replaced both mock pages. The removed PagePreviewTests and the original procedure below are historical; use production Dashboard and Calendar procedures for the current tree. Fixture test counts do not establish current behavior.

On the phone, open the Dashboard and Calendar bottom tabs from Dues. Verify the visible sample label, 2/8 completed summary, each filter and retained selection after switching tabs. Select October 17 (two samples), October 15 (one), and an empty day; check month arrows and Today reset. Test root Back returning to Dues, repeated tab selections, offline reopening, both themes and 200% text; use the bottom-navigation procedure below for editor/stack checks. Capture only sample UI; never dump saved user dues. Restore original theme/font/radio settings. Record source/artifact/environment, observer and exact checks in STATUS/evidence. TalkBack, keyboard/switch access and other widths require separate recorded acceptance. Mock checks do not accept production F02/F03 or release. Earlier push/pop/explicit-exit results apply only to the old artifact in its evidence record.

## Bottom-navigation verification

Scope: [BN01–BN05](../../specs/future-pages/spec.md#bottom-navigation-acceptance). After existing regression tests and a new Android build, verify the installed artifact starts on Dues with three labeled/icon tabs. Switch/reselect/rapidly tap roots; retain a non-default dashboard filter and a calendar month/date across switches. Back from either preview selects Dues; no preview push stack or obsolete switch/exit control remains. Restart begins on Dues with fresh sample state. Check preview labels so sample items cannot be mistaken for saved dues.

From Dues, open Add and an existing temporary verification due's Details → Edit: the bottom bar is hidden on Details/editor. Exercise Cancel/system Back without saving; direct Add Save returns to refreshed Dues, while an edit entered through Details returns to reloaded Details under F01. Back to dues restores the refreshed list/tab bar. Clean up only the temporary sample; existing payment/delete behavior needs regression tests and no unrelated due may be changed. Inspect both themes at normal and 200% text, native selected labels, safe-area placement and scroll reachability. Capture only controlled sample preview/detail or blank form UI, excluding notification overlays; restore original settings. Record source/APK/environment and observed results separately from full TalkBack/keyboard/other-width acceptance. Release remains held.

## Future production-page verification planning

F01, F02 and F03 are authorized; use their dedicated procedures below. Subsequent production slices remain planning work. Original mock and bottom-navigation procedures above describe historical artifacts; neither tab now uses fixtures.

The [prototype comparison](PROTOTYPE-REVIEW-2026-10-18.md) distinguishes present code from samples and missing features; the [roadmap](../../specs/future-pages/plan.md#priority-order-after-prototype-review) owns P0–P4 order and per-slice expected outcomes. These are planned verification boundaries, not new tests or passing results:

- P0: preserve R02–R07 regression coverage and close the relevant open controlled-failure/native/accessibility checks recorded in STATUS as affected modules change. Intended-key release acceptance stays separately open during the release hold.
- P1: F01 uses real selected IDs and rejects missing/deleted targets. F02/F03 need real repository/clock integration checks: saved changes update summaries, title-search/status-filter results, day markers and selected agenda; paid/deleted/empty/error cases agree. Include date rollover/resume, agreed week start, month/year/leap boundaries and no-match reset. Preview fixture tests do not cover these outcomes. Verify offline/restart, native navigation, both themes, large text and accessibility for each shipped page.
- P2: schema extensions need old-data migration/round-trip and cancellation tests per field; recurrence needs occurrence identity and boundary tests. Reminders need denied/revoked permission, channels, reconciliation, paid/delete cancellation, offline/screen-off/reboot/update and deep-link device checks. Export needs approved privacy/format and failure/cancel checks. Do not promise restore or exact delivery without a separate contract.
- P3: F12 aggregation tests must use approved reporting periods/denominators, centavo arithmetic and overflow rules, paid/deleted/category cases and text equivalents for charts. Onboarding copy/first-run state and widgets' update/privacy/platform checks follow their actual shipped dependencies.

Future acceptance becomes executable only after the corresponding product rules/spec are defined; no new feature or test pass is established by listing it here.

## F01 saved-item details verification

Scope: [DT01–DT06](../../specs/f01-item-details/spec.md#acceptance-scenarios). [DueDetailTests](../../tests/DueDateChecker.App.Tests/DueDetailTests.cs) covers selected persisted data/no writes, notes/UTC formatting, unknown/deleted/malformed IDs, editor save/cancel/delete refresh, read/init failure retry, navigation failure and overlapping-read guards. Run Core/App sequentially and build Android Release after UI changes; fixture preview tests do not accept F01.

On the phone, use a uniquely named temporary zero-amount due only. Add must still return directly to Dues; select the new row to open Details and check title/amount/date/Paid or Unpaid/notes/UTC timestamps and hidden tabs. Edit, cancel changed fields and system Back; verify stored values return. Edit and Save changed fields/payment; Details must refresh the same identity. Back to dues refreshes its row. Background/resume Details and the editor separately, verifying reload versus preserved unsaved input. Restart/reopen offline to check persisted fields. Repeated row/Edit/Back taps must not duplicate pages or writes.

Delete that temporary due through the editor, checking confirmation Cancel first. Confirm should pop to unavailable Details with no previous fields and Edit disabled; Back to dues must omit it, including after restart. Inspect both themes at normal/200% text, long notes/amount wrapping and reachable Edit/Back controls. Keep user dues out of screenshots/logs; restore theme/font/radio settings. Record artifact/source identity and exact observed outcomes separately from full TalkBack, other widths and intended-key release acceptance. Controlled route/read failures are automated unless separately exercised in an authorized native test build.

## F02 Dashboard verification

The Calendar-sample assertion in the original F02 procedure describes its historical artifact; current Calendar uses the F03 procedure below.

Scope: [DB01–DB07](../../specs/f02-dashboard/spec.md#acceptance-scenarios). [DueStatusTests](../../tests/DueDateChecker.Core.Tests/DueStatusTests.cs) covers Paid/date/deleted/extreme-date classification. [DashboardTests](../../tests/DueDateChecker.App.Tests/DashboardTests.cs) uses SQLite, controlled failures and a mutable clock for count partitions, title AND status, no-match/empty differences, clear, read-completion/midnight rollover, editor-write refresh, stale retry, initialization failure, overlapping reads and session reset. Run Core/App sequentially, then build Android.

On a phone, preserve data and use uniquely tagged zero-amount temporary dues for past/today/future dates. Compare summary membership/deltas in memory without printing existing values. Search that tag, exercise all filters/no-match/reset, switch tabs and background/resume. Check Dashboard Add returns to Dashboard; row → Details → Edit Save/Cancel/Back/delete returns through the same parent with query/filter retained. Payment changes membership only after Save. Delete only task records, then verify their query absence after restart. Root Back/cold start remain Dues; Calendar stays a labeled sample.

Repeat search/filter/selection/Add Cancel and scrolling in both themes at 100%/200% text. Restore all phone theme/font/radio settings. Do not capture a live global Dashboard or log existing financial data; use in-memory assertions or an isolated controlled build for visual evidence. Date rollover is automated unless a native clock-boundary run is recorded. Full TalkBack/keyboard/other-width and intended-key gates remain separate. Record artifact/source identity, observer, exact checks and unchecked cases in STATUS/evidence; test definitions/builds are not native passes.

## Documentation checks

For documentation changes, check required files, local Markdown paths and heading anchors, stable requirement IDs, technical-contract coverage, history preservation, feature scope, and the final diff against the initial working tree. Existing platform references in historical records are not evidence of current external policy; revalidate them when that work begins.

## F03 Calendar verification

Run [CalendarTests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs) for CA01-CA06: isolated SQLite counts/agenda/order/tombstones, Monday alignment, leap/year/date bounds, Manila midnight/read completion without selection jumps, empty states, real editor writes, stale retry/init failure, concurrent loads, stable read-only agenda and reopened-file persistence. The harness does not load XAML or establish native acceptance.

On a phone, preserve existing dues and use only uniquely named zero-amount task records. In airplane mode, record today's and adjacent-day counts in memory, create through Calendar Add and compare count deltas. Select dates, verify exact task IDs/status text and Details -> Edit Save/Cancel/Back/delete return with month/date retained. Change payment and due date on Save; verify both affected day counts, no duplicate rows, refresh from another tab and background/resume. Next/Previous select day one, Today restores current Manila date, root Back/restart default to Dues, and restart preserves committed changes while resetting Calendar selection. Delete only task records and verify their absence after restart.

Exercise month/Today/selection/empty-date/agenda/Add Cancel in Light/Dark at 100%/200% text; keep controls and bottom tabs reachable. Restore original theme/font/radio settings. Never log private counts/titles or capture a live global calendar; assert in memory. Date bounds/clock boundaries and controlled storage/navigation failures remain automated-only unless separately executed natively. Record artifact/source identity and the exact observed checks in STATUS/evidence. Full TalkBack/keyboard/switch/other-width and intended-key gates remain separate; release remains held.

## F07 category verification

Run Core [CategoryTests](../../tests/DueDateChecker.Core.Tests/CategoryTests.cs) for legacy schema-1 active/paid/deleted preservation, all category round trips, invalid domain/SQL values, corrupt-read rejection and controlled migration conflict with unchanged version/data. F05 subsequently advances current schema assertions to version 3; immutable category migration 002 remains unchanged. App [CategoryTests](../../tests/DueDateChecker.App.Tests/CategoryTests.cs) cover native-independent draft/default/required-selection rules, Save/failure/retry/delete, saved labels on every projection, title AND status AND category, global-summary independence, stable collections, clear/session reset and category edits refreshing Dashboard/Calendar. Run Core/App sequentially, then Android Release build.

On the phone, preserve app data and compare pre/post-upgrade active summary membership in memory. Legacy rows should read Other without changed counts; automated SQLite tests establish exact old-field/tombstone preservation, not a private phone DB inspection. Use only uniquely tagged zero-amount task dues. Verify the new Bill default, category selection/Save, Details and row labels, edit Cancel/Back, saved category/payment changes, Dashboard combined filters/empty transitions/clear, retained session state, Calendar refresh and offline restart. Delete only task records and verify absence after restart. Exercise category dialogs and return/filter controls in Light/Dark at 100%/200%, restoring all original settings. Keep private titles/amounts/count values out of logs/screenshots. Controlled native migration/write/read errors, full TalkBack/keyboard/other-device and intended-key gates remain separate; release stays held.

## F04 preference verification

Use [PF01-PF07](../../specs/f04-preferences/spec.md#acceptance-scenarios), [PRD preferences](../PRD.md#f04-preference-rules) and [SDD contract](../SDD.md#f04-preference-contract). Run Core/App suites and Android Release build; do not treat a fake preference store as an Android persistence/theme pass.

App [preference tests](../../tests/DueDateChecker.App.Tests/AppPreferencesTests.cs) exercise missing/round-trip/unknown-format values, last-good read retry, invalid selectors, failed Save without publication, retained drafts and new-page reset. [Calendar tests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs) verify both layouts/header alignment, leap years/DateOnly bounds and unchanged month/selection/agenda/saved fields. Native checks: data-preserving upgrade; default/selected pickers; draft Back versus Save; System runtime OS changes and explicit override; restart/offline persistence; cached Calendar re-layout with selection retained; Settings-to-Reminders Back without opt-in; both themes at 100%/200% and scrollable actions. Record installed APK hash and actual observations, restore original preferences/device state, and keep financial data out of logs/screenshots. Full assistive/cross-device and intended-key gates stay separate.

## F05 reminder verification

Use [RM01-RM08](../../specs/f05-reminders/spec.md#acceptance-scenarios), [PRD policy](../PRD.md#f05-reminder-rules) and [SDD contract](../SDD.md#f05-reminder-contract). Run Core/App sequentially and Android Release build. Record exact source/artifact hashes; do not infer device passes from fake-platform tests.

1. Upgrade the existing schema-2 app without clearing data. Compare existing saved-data membership in memory, verify Reminders off, default day-before + due-date/09:00/22:00-08:00, and no launch permission prompt. Confirm all original core routes still work.
2. Open Reminders from Dues; verify tabs hidden/Back return, draft edits retained on Refresh but discarded on leaving without Save, saved settings persist offline/restart, and no implicit permission request. Enable explicitly: deny and verify no opt-in, then grant via Android settings/Enable. Check app and channel blocking separately; CRUD remains usable. Test notification is generic and requires opt-in/access.
3. With controlled task dues and a near-future Manila schedule, verify a queued inexact alarm/recovery job and real notification after screen-off/background/offline. Inspect only app-owned generic text/internal metadata; never log user financial content. Tap validates the correct ID/Details, including cold start; tap while an editor draft is active defers navigation and preserves input. Ignore expired/missing/paid/deleted/obsolete keys.
4. Save payment/date/settings changes, soft-delete task dues, and disable; verify obsolete pending/displayed notifications are removed. Restart/reconcile repeatedly without deliberate replay. Quiet-hour shifts/expired/new elapsed times and large batches require controlled dates/clocks; do not alter the user's device clock for convenience.
5. Check Light/Dark at 100%/200% text, scrolling, time dialogs, busy states, status labels, Android settings return and Back. Full TalkBack/keyboard/switch/focus/other widths require separate observation.
6. In an authorized test environment verify reboot-after-unlock, package update, Doze/OEM restrictions, force-stop/reopen, clock/timezone events, permission/channel revocation and controlled native database/post/navigation failures. A forced JobScheduler run is not proof of natural alarm timing or reboot delivery.
7. Clean only task-created dues/notifications; restore original reminder opt-in/saved schedule, phone theme/font/connectivity and permission/channel state when changed. Verify restart absence and preserved unrelated dues. Record unperformed lifecycle/assistive/intended-key gates explicitly. Release remains held.


F05 completion additions: verify independent Android-reported background restriction/Battery Saver/optimization text, explicit app/battery settings and draft retention on return. Do not automatically request an exemption or infer delivery from an allowed notification channel. Test expedited dispatch success/quota fallback in the App harness; native inexact delivery and Doze remain separately observed. Two clock-interleaving regressions cover expiry during a repository read and quiet-hour start within a notification batch; capture clock after reads, before each post and before choosing the next check.

[Native verification helpers](../../tools/verification/README.md) provide an explicitly selected fresh API-36 AVD path, guarded SQLite fixture/fault tooling and phone restoration controls. Never use controlled clock/database faults on a personal phone. Actual reboot/update, permission/channel transitions, storage retry, paid/delete/date cancellation, keyboard and landscape observations must identify the APK that ran them. [Completion evidence](evidence/F05-COMPLETION-2026-10-18.md) preserves the initial-artifact versus final-clock-fix attribution; full TalkBack/switch/cross-device/intended-key acceptance remains separately recorded.
