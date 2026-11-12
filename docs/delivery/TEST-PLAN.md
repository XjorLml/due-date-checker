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

The native Philippine preset extension adds Core/App `PhilippineCategoryTests` and a category-only stale-action case in `ReminderPaymentTests`. They cover catalog IDs/search, type independence, schema-5 upgrade/preservation/failure, CRUD/recurrence/restore, version-1/2 archives, draft/dirty handling and saved projections. Initial 2026-11-08 execution was blocked by Windows Application Control. In the later CI assertion follow-up, 185 Core + 313 App tests passed locally; a hosted CI pass for the corrected revision remains pending. See [evidence](evidence/PH-CATEGORIES-2026-11-08.md).

| Requirements / contract | Sources and coverage | Remaining limits |
|---|---|---|
| R02, R03, R07 | [CreateAndListTests](../../tests/DueDateChecker.App.Tests/CreateAndListTests.cs): Manila date boundary; reopened-file persistence/order; PHP/timezone defaults; required fields; amount limits/overflow; retries; duplicate Save; startup/read failures | Android lifecycle, input controls, and airplane-mode acceptance require a phone. |
| R04, R05 | [EditDueTests](../../tests/DueDateChecker.App.Tests/EditDueTests.cs): load/no-write, edit/paid reopen, identity/timestamps, refreshed order/status, invalid IDs/fields, stale deletion, read/write retry, repeated load/Save | Native navigation, XAML binding, and trimming need Release/device checks. |
| R06 | [DeleteDueTests](../../tests/DueDateChecker.App.Tests/DeleteDueTests.cs): cancel without storage access, persisted tombstone/reopen/list refresh, invalid unsaved fields, missing/deleted targets, failure/retry, repeated Delete/Save, navigation-retry state, concurrent deletion | Native confirmation display/dismissal, page action handling, and lifecycle are outside this harness. |
| R02–R06 / persistence | [SqliteDueRepositoryTests](../../tests/DueDateChecker.Core.Tests/SqliteDueRepositoryTests.cs): initialization/idempotence, round trips, date sorting, missing IDs, active-row update safeguards, retained tombstones, repeated/unknown delete, newer-schema data preservation | F07 CategoryTests add legacy upgrade and duplicate-column migration-failure preservation. Since 2026-11-06 `OptionalAmountTests` covers a mid-script migration failure (rollback after an earlier DDL statement succeeded) and `SqliteDueRepositoryTests` covers equal due dates ordered by `created_at` (UTC writes; rows with mixed historical offsets sort by stored text). |
| Domain rules | [MoneyTests](../../tests/DueDateChecker.Core.Tests/MoneyTests.cs), [DueTests](../../tests/DueDateChecker.Core.Tests/DueTests.cs): rounding, negative amounts, formatting/value equality, create defaults, title validation/trim, `with` validation, identity | No inferred coverage beyond these tests. |

F05 automated contract: Core [ReminderTests](../../tests/DueDateChecker.Core.Tests/ReminderTests.cs) cover default/off and due-date-only planning, Manila instants, quiet-hour variants/bounds, culture-invariant identities, invalid settings, schema-2 preservation, receipt persistence and transactional pending rollback. App [ReminderTests](../../tests/DueDateChecker.App.Tests/ReminderTests.cs) cover reconciliation/restart replay, late versus newly elapsed work, paid/date/delete/disable/revocation cancellation, bounded posting, controlled platform failures, post-commit write safety, delivered-key validation and settings draft/permission/test boundaries. These use a fake platform; they do not execute Android APIs, Shell or permission dialogs.

Temporary database tests should close/clear pooled connections before cleanup, and only for their own files: use `TestDatabase.ClearPool(path)` or `ClearPoolsUnder(directory)`. `SqliteConnection.ClearAllPools()` is process-wide and, with xUnit running test classes in parallel, caused an intermittent `ObjectDisposedException` in another class (found and fixed 2026-11-06). Test failure behavior and persistent outcomes rather than generated MVVM boilerplate. A future migration failure test needs a controlled seam; do not modify shipped migrations to simulate failure.

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

## Native Philippine preset verification

Scope: [PC01–PC08](../../specs/ph-categories-native/spec.md#acceptance). Run the Core/App suites above once the environment permits assembly execution; do not disable application-control policy to obtain a pass. Compare the 53 IDs/names/groups/icons with the prototype catalog. Use schema-5 fixtures containing explicit zero, absent amounts, tombstones and repeat values to verify schema 6 preserves prior stored fields and adds Other without guessing. Exercise idempotence, failed migration and newer-schema refusal with isolated files.

On the task-owned emulator, [ph_categories.py](../../tools/verification/ph_categories.py) guards the profile/empty database before its controlled migration fixture. Check search by label/group/Filipino aliases, current-selection feedback, selection/Cancel/header Back/system Back, draft retention, category-only dirty exits, Save/edit/restart and detailed labels across Dues/Dashboard/Calendar/Details. Confirm all four due types retain their amount and repeat policies. Mark a repeating categorized due paid and verify its one next occurrence retains the category; repeat through the notification payment/Undo path on a suitable device. Verify stale actions after category-only edits are refused.

Export via Android's file dialog, inspect format 2/categoryId and stored NULL/zero/tombstone values. With only controlled test data and no active dues, import a genuine format-1 fixture (Other fallback), then format 2 (detailed selection restored). Missing/null/unknown version-2 IDs must fail before writes. Preserve the user's data and reminder choices; never clear a phone to simplify testing.

Inspect the picker/editor in light/dark, 320–430dp widths, 100%/200% text and with the keyboard open. Search, wrapping rows, Back and Cancel must remain operable; the picker must scroll without adding root navigation. Restore settings/remove task fixtures after checks. Emulator/build evidence is separate from intended-key phone acceptance, TalkBack and timed-user observation. Record exact checks and unperformed cases in the evidence; do not infer process-recreation draft recovery from theme switching.

## GitHub draft release verification

Scenarios [GR01–GR07](../../specs/github-release/spec.md#acceptance), [setup](GITHUB-RELEASE.md). Run `python -m unittest discover -s tools/release/tests -v` and actionlint against both workflow files. Local helper tests cover version/path/injection rejection, missing/malformed secrets without logging values, package/version/debug/certificate validation, archive selection, explicit public deliverables, checksum/source-marker tampering, annotated tags, published/tag/draft conflicts and same-source retry policy. These tests fake GitHub/tool responses and do not use repository signing credentials.

For the first hosted run, record selected branch and exact SHA, version/code, runner/.NET/workload/JDK versions, Core/App/tool test outcomes, signed build, public certificate fingerprint and resulting draft URL. Confirm downloaded APK signature, version, checksum and notes against the run. Confirm only APK/checksum assets are attached, draft remains unpublished, no credential appears in logs/artifacts, and cleanup ran. Exercise missing-secret and conflicting-version failures with controlled versions; do not overwrite a published release for testing. Test a same-source rerun against its own draft before publication. Secrets/default-branch setup and workflow availability must be verified, not inferred from local lint.

The intended-key APK still requires relevant phone acceptance. A hosted build, a draft release and local verifier success are distinct outcomes. Publishing remains a maintainer action; finish known exposed-secret/history cleanup first.

## Developer dashboard verification

Scenarios [DD01–DD14](../../specs/developer-dashboard/spec.md#observable-acceptance-scenarios); [tool guide](../../tools/developer-dashboard/README.md#test).
- **Unit, CLI and HTTP tests:** `node --test tools/developer-dashboard/tests/*.test.mjs` runs against temporary fixture repositories. It covers parsing, status precedence, staleness, record locking and torn lines, hooks, and the HTTP boundaries (host, path, origin, token).
- **Browser check:** `node tools/developer-dashboard/tests/browser-check.mjs` drives the real prototype in headless Edge or Chrome. It writes to temporary record copies.
- **Prototype regressions:** after any bridge change, rerun the prototype's own model tests and `prototypes/design-exploration/tests/browser-check.cjs`.

Run suites through `cli.mjs test <suite>` to record their results. Dashboard results describe developer tooling and the HTML prototype only. They are not evidence for native behavior, device checks or release acceptance.

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

## Focus adoption verification

Scope: [FA01–FA10](../../specs/focus-adoption/spec.md#acceptance-scenarios), [DESIGN 2.0.0](../DESIGN.md) and the [SDD navigation contract](../SDD.md#6-ui-and-navigation). No view-model or Core code changed, so the Core and App suites are regression gates only; run them sequentially, then a Debug Android build. Styles and Shell markup are not covered by the test projects.

Emulator procedure (new for this slice). Tools live under `%LOCALAPPDATA%\Android\EmulatorSdk` (emulator, platform-tools, Android 35 Google APIs x86_64 image), deliberately outside the auto-detected `Android\Sdk` path so the Visual Studio SDK keeps building the app. Create a Pixel-class AVD with `avdmanager`, boot it, install the Debug APK with `adb install -r`, and launch `com.lem.duedatechecker`. Toggle dark mode with `adb shell cmd uimode night yes|no` and text scale with `adb shell settings put system font_scale 2.0` (restore `1.0`). Both Android 35 images used so far boot with `debug.hwui.drawing_enabled=0`, which makes every app draw nothing: run `adb shell setprop debug.hwui.drawing_enabled 1` after boot or every capture is black. Capture with `adb shell screencap -p /data/local/tmp/s.png` then `adb pull` (PowerShell redirection corrupts binary output) into a folder outside the repository. The Google APIs ATD image cannot render at all and is only useful for `uiautomator dump` structure checks; the AOSP default image renders but has no SystemUI, so status-bar colour cannot be judged on it.

Checks: cold start lands on Dues with the five-slot bar and the lime disc centred; each destination switches and retains state; the Add slot never highlights; the disc opens the editor from all four roots and Save/Cancel return to the same root; the editor shows title, amount, date, type, notes in that order with a fixed Cancel/Save footer and Delete in the header only for an existing due; the bar and disc are absent on the editor, Details and Reminders; Settings shows the bar and Back from it selects Dues; Reminders from Settings hides the bar and returns; light and dark palettes match DESIGN §3; at 200% text the footer, bar and disc remain reachable; empty-state copy points to the bar. Smoke-test create, edit, pay, cancel and delete.

Record environment, APK identity, exact checks and screenshots as **emulator-observed** in the [tasks](../../specs/focus-adoption/tasks.md) and STATUS. Emulator results do not close phone, TalkBack, keyboard-overlap or gesture-inset acceptance; those remain in Remaining acceptance.

## UI consistency verification

Scope: [UC01–UC06](../../specs/ui-consistency/spec.md#acceptance-scenarios). Run the App suite: the Dues, Details, Dashboard and Calendar projection tests now assert the F02 status words on every surface. On the emulator, seed one due per state by moving the device clock (`adb root`, `settings put global auto_time 0`, `date MMDDhhmmYYYY`) rather than editing dates through the picker, then mark one paid. Check that each Dues card, Dashboard and Calendar row and the Details page show one pill with the status word and that `uiautomator dump` reports its description as "Status: word"; check that Dues, Dashboard, Calendar and Settings expose no toolbar title and that the editor, Details and Reminders expose the Shell title and no second heading. Restore `auto_time 1`. Do not `pm clear` a Debug install on an emulator: it deletes the Fast Deployment assemblies. Phone, 200% text on the pills and TalkBack remain separate.

## Focus page content verification

Scenarios: [FP01–FP08](../../specs/focus-pages/spec.md#acceptance-scenarios). Tracking: [tasks](../../specs/focus-pages/tasks.md).

- Unit: `DashboardSummaryTests` cover the upcoming total (unpaid, today through month end, paid and next-month dues excluded), the next due, the zero case and the shared `IDueRow` contract. Existing Dashboard, Calendar, Details and preference tests must stay green.
- Build: Debug Android build; Release with `-p:AndroidKeyStore=false` while the keystore password is wrong.
- Emulator (AOSP Android 35, `debug.hwui.drawing_enabled=1`, device clock moved to seed Overdue, Due today, Upcoming and Paid): Dashboard summary text and counters through the accessibility tree; a counter tap changes the results heading; the next-due link opens Details; ledger rows expose the projection description; Calendar shows ‹ › Today controls and 44 × 48 day cells; Settings segments announce selected state and the Reminders row shows the saved state; no Refresh/Retry node on an error-free page; screenshots in light, dark and at 200% text.
- Still separate: phone acceptance, TalkBack reading order, intended-key signing.

## Documentation checks

The [mobile HTML study](../../specs/mobile-ui-prototype/spec.md) defines HP01–HP09 for `prototypes/mobile-review/` and HP10–HP18 for the four-concept exploration, Minimal Fintech floating Add and its refinements. Run `node --test prototypes/mobile-review/tests/model.test.cjs` for the original study. For the exploration, run `node --test` over `model.test.cjs`, `ph-catalog.test.cjs`, `essential-model.test.cjs`, `decision-lab.test.cjs` and `notification-lab.test.cjs` under `prototypes/design-exploration/tests/`, then `node prototypes/design-exploration/tests/browser-check.cjs`, which drives `ph-browser-flows.cjs`, `interaction-browser-flows.cjs`, `essential-browser-flows.cjs`, `decision-browser-flows.cjs` and `notification-browser-flows.cjs` through a loopback server and an installed headless Edge. `--notifications` runs only the notification lab flow ([NL05–NL10](../../specs/notification-lab/spec.md#acceptance-lab)); `--decisions` runs only the UX decision lab flow (both rounds, [DL01–DL08](../../specs/ux-improvements/spec.md#html-decision-lab) and [DL09–DL13](../../specs/ux-improvements-2/spec.md#html-decision-lab)). [Observed results](../../specs/mobile-ui-prototype/tasks.md) are agent-observed browser evidence only.

The HTML-only [Philippine catalog prototype](../../specs/ph-catalog-prototype/spec.md) has separate PH01–PH09 scenarios and [observed browser/helper evidence](../../specs/ph-catalog-prototype/tasks.md). Run its documented Node and local Edge checks for catalog/picker/schedule changes. These results do not count as MAUI builds, Android persistence, TalkBack or reminder-delivery acceptance.

The subsequent [Focus interaction lab](../../specs/interaction-lab/spec.md) defines IN01–IN09 for the independent 3×3 action/navbar comparison. The same dependency-free browser harness runs both the PH flows and `interaction-browser-flows.cjs`, covering roots, long forms, dirty exits, modal focus, duplicate Save, preferences and simulated keyboard reservation. [Observed results and limits](../../specs/interaction-lab/tasks.md) remain separate from final user choice and actual mobile keyboard/Back/accessibility acceptance.

IN10 now covers the explicitly selected Sticky bottom actions/Context-aware navigation in the normal HTML mockup. The PH flow checks default footer targets/anchoring and Add/Edit Save/Cancel across widths/themes; the comparison flow verifies return to the selected mockup after previewing another combination. Native acceptance remains separate.

IN11–IN15 cover approved Essential-first Add/Edit. The same browser harness now includes `essential-browser-flows.cjs`; `--essential` runs that focused suite alone. Existing PH fixtures explicitly discard edited drafts under the new normal confirmation, and browser helpers reveal real native disclosures before field interaction. Pure `essential-model.test.cjs` covers safe UI-only PHP normalization and global reminder summaries. Check core-field fold, clean/dirty exits, hidden-error reveal/focus, state retention, amount policies and root return. Automated elapsed time is not evidence of the approximately 30-second user target.

For documentation changes, check required files, local Markdown paths and heading anchors, stable requirement IDs, technical-contract coverage, history preservation, feature scope, and the final diff against the initial working tree. Existing platform references in historical records are not evidence of current external policy; revalidate them when that work begins.

## F03 Calendar verification

Run [CalendarTests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs) for CA01-CA06: isolated SQLite counts/agenda/order/tombstones, Monday alignment, leap/year/date bounds, Manila midnight/read completion without selection jumps, empty states, real editor writes, stale retry/init failure, concurrent loads, stable read-only agenda and reopened-file persistence. The harness does not load XAML or establish native acceptance.

On a phone, preserve existing dues and use only uniquely named zero-amount task records. In airplane mode, record today's and adjacent-day counts in memory, create through Calendar Add and compare count deltas. Select dates, verify exact task IDs/status text and Details -> Edit Save/Cancel/Back/delete return with month/date retained. Change payment and due date on Save; verify both affected day counts, no duplicate rows, refresh from another tab and background/resume. Next/Previous select day one, Today restores current Manila date, root Back/restart default to Dues, and restart preserves committed changes while resetting Calendar selection. Delete only task records and verify their absence after restart.

Exercise month/Today/selection/empty-date/agenda/Add Cancel in Light/Dark at 100%/200% text; keep controls and bottom tabs reachable. Restore original theme/font/radio settings. Never log private counts/titles or capture a live global calendar; assert in memory. Date bounds/clock boundaries and controlled storage/navigation failures remain automated-only unless separately executed natively. Record artifact/source identity and the exact observed checks in STATUS/evidence. Full TalkBack/keyboard/switch/other-width and intended-key gates remain separate; release remains held.

## F07 category verification

Run Core [CategoryTests](../../tests/DueDateChecker.Core.Tests/CategoryTests.cs) for legacy schema-1 active/paid/deleted preservation, all category round trips, invalid domain/SQL values, corrupt-read rejection and controlled migration conflict with unchanged version/data. F05 subsequently advances current schema assertions to version 3; immutable category migration 002 remains unchanged. App [CategoryTests](../../tests/DueDateChecker.App.Tests/CategoryTests.cs) cover native-independent draft/default/required-selection rules, Save/failure/retry/delete, saved labels on every projection, title AND status AND category, global-summary independence, stable collections, clear/session reset and category edits refreshing Dashboard/Calendar. Run Core/App sequentially, then Android Release build.

On the phone, preserve app data and compare pre/post-upgrade active summary membership in memory. Legacy rows should read Other without changed counts; automated SQLite tests establish exact old-field/tombstone preservation, not a private phone DB inspection. Use only uniquely tagged zero-amount task dues. Verify the new Bill default, category selection/Save, Details and row labels, edit Cancel/Back, saved category/payment changes, Dashboard combined filters/empty transitions/clear, retained session state, Calendar refresh and offline restart. Delete only task records and verify absence after restart. Exercise category dialogs and return/filter controls in Light/Dark at 100%/200%, restoring all original settings. Keep private titles/amounts/count values out of logs/screenshots. Controlled native migration/write/read errors, full TalkBack/keyboard/other-device and intended-key gates remain separate; release stays held.

## F07 optional amount verification

Use [OA01-OA06](../../specs/f07-optional-amounts/spec.md#acceptance-scenarios), [PRD rules](../PRD.md#f07-optional-amount-rules) and [SDD contract](../SDD.md#f07-optional-amount-contract). Run Core/App suites sequentially and an Android Release build; automated SQLite tests establish schema/data preservation, not a private phone database inspection. Implemented 2026-11-06: `OptionalAmountTests` in both test projects cover the automated list below, and the [emulator evidence](evidence/F07-OPTIONAL-AMOUNTS-2026-11-06.md) includes a real schema 3→4 upgrade dumped with the device's `sqlite3` through `run-as` (possible only for a debuggable build). The phone steps below are still open.

Automated checks must cover schema-3-to-4 active/paid/deleted preservation with the active index recreated, NULL-versus-zero round trips, Bill/Subscription rejection of null (including copied/`with` records at the repository boundary), Task/Other acceptance, editor blank/required/category-switch/clear/failure paths, and Dues/Details/Dashboard/Calendar "No amount" projections and semantics. Confirm amount-free dues do not alter status, counts, sort, filters or reminder planning.

On the phone, preserve data and use only uniquely tagged temporary records. Verify Bill blank blocks Save; Task/Other blank saves and survives restart; zero displays `₱0.00`; clear an existing Task/Other amount and verify "No amount" on Dues/Details/Dashboard/Calendar; reopen after restart. Check both themes at normal and 200% text, retain unrelated settings and delete only the temporary records. Keep private titles/amounts out of logs/screenshots. Full TalkBack/keyboard/switch, other devices/widths, controlled native migration/write/read failures and intended-key acceptance remain separate; release stays held.

## UX Wave 2 verification: recurrence (UX03) and maker steps (UX04)

Use [RC01–RC08](../../specs/f07-recurrence/spec.md#acceptance-scenarios), [PRD UX03/UX04](../PRD.md#ux-review-decisions-2026-11-06), the [SDD recurrence contract](../SDD.md#f07-recurrence-contract) and [ADR 009](../adr/009-recurring-dues.md). Automated: Core [RecurrenceTests](../../tests/DueDateChecker.Core.Tests/RecurrenceTests.cs) (date rules and bounds, domain rules, migration 005 fresh/upgrade/rollback, one-transaction write and one occurrence per source) and App [RecurrenceTests](../../tests/DueDateChecker.App.Tests/RecurrenceTests.cs) (Details, editor, row text), plus App [ReminderTests](../../tests/DueDateChecker.App.Tests/ReminderTests.cs) for the forwarding wrapper and maker steps. The rollback test also guards the migration runner: a failing statement after an `ALTER … CHECK` must throw and leave the previous schema. Executed 2026-11-06 with an emulator pass: [evidence](evidence/UX-WAVE2-2026-11-06.md).

On the phone, after reinstalling from the laptop that holds its key: confirm the schema 4→5 upgrade keeps every due. With a uniquely named temporary Bill, choose Monthly on a date late in a month, mark it paid on Details, and check the visible "Next one added" line and the new row's date, type, amount and notes. Mark it unpaid and paid again and confirm no second occurrence. Switch a temporary Bill to Task and confirm Repeat disappears and the saved due no longer repeats. With reminders on, open Reminders and compare the Xiaomi/Redmi/POCO steps with the phone's own app-info labels (Autostart, Battery saver → No restrictions), following them from "Android app / battery settings". Check both themes at 100% and 200% text, then delete only the temporary dues and restore the reminder setting.

## UX Wave 3 verification: export and import (UX05)

Use [EX01–EX08](../../specs/f04-export-import/spec.md#acceptance-scenarios), [PRD UX05](../PRD.md#ux-review-decisions-2026-11-06), the [SDD contract](../SDD.md#ux05-export-and-import-contract) and [ADR 010](../adr/010-export-import-file.md). Automated: Core [ExportImportTests](../../tests/DueDateChecker.Core.Tests/ExportImportTests.cs) cover the format, round trip, every refusal, the empty rule with tombstone replacement and the single transaction. App [ExportImportTests](../../tests/DueDateChecker.App.Tests/ExportImportTests.cs) cover the Settings flow with a fake file service, messages, cancels, races and reminder refresh. The Android file dialogs exist only on a device. Executed 2026-11-06 on the emulator: [evidence](evidence/UX-WAVE3-2026-11-06.md).

On the phone, after reinstalling from the laptop that holds its key:

1. Export to Downloads and to a second location the phone offers (its own file manager, and Drive only if the user agrees to put financial data there), and confirm the file opens as text.
2. Try Import while dues show and confirm it is refused before any dialog.
3. In an authorized test only, with a fresh install or after exporting and deleting every due, import the file and compare the Dues, Dashboard and Calendar contents. Check Cancel in the dialog and in the confirmation.
4. Check both themes at 100% and 200% text.
5. Delete the exported test files.

Never import over a personal phone's real data without a fresh export first.

## UX Wave 4 verification: messages, Undo and polish (UX06–UX12)

Use [CM01–CM08](../../specs/ux06-confirmations-undo/spec.md#acceptance-scenarios), [PRD UX06/UX07/UX12](../PRD.md#ux-review-decisions-2026-11-06), the [SDD contract](../SDD.md#ux06-confirmation-and-undo-contract) and the [DESIGN decisions](../DESIGN.md#ux-review-decisions-2026-11-06). Automated tests:

- Core [RestoreTests](../../tests/DueDateChecker.Core.Tests/RestoreTests.cs): the guarded restore.
- App [ConfirmationUndoTests](../../tests/DueDateChecker.App.Tests/ConfirmationUndoTests.cs): editor messages and deletion record, Undo once/latest/changed/failed, Details reload and Manila timestamps.
- The updated [DueDetailTests](../../tests/DueDateChecker.App.Tests/DueDetailTests.cs) and [RecurrenceTests](../../tests/DueDateChecker.App.Tests/RecurrenceTests.cs).

The snackbar, keyboard, date text and counter role exist only on a device. Executed 2026-11-06 on the emulator: [evidence](evidence/UX-WAVE4-2026-11-06.md).

On the phone, with a uniquely named temporary due:

1. Save and check "Due saved." above the bottom bar.
2. Mark it paid on Details.
3. Delete it and tap Undo within 5 seconds; check it returns with its fields. Delete it again and let the message expire; check it stays deleted.
4. With TalkBack, confirm each message is read once, that Undo is reachable and that the time is extended when the phone's accessibility timeout is longer.
5. Check the amount keyboard of the phone's own keyboard app and that a minus or comma cannot be entered.
6. Check that the date field reads `06 Nov 2026`, that the Dashboard counters are announced as buttons and that Details timestamps say "Manila".
7. Check both themes at 100% and 200%, then delete the temporary due.

## UX Wave 5 verification: presentation (UX13–UX16)

Use the [DESIGN decisions](../DESIGN.md#ux-review-decisions-2026-11-06). Automated tests: App [LayoutRulesTests](../../tests/DueDateChecker.App.Tests/LayoutRulesTests.cs) cover the caption scale, day cell width, landscape heading size and the amount flag. The rendered sizes exist only on a device. Executed 2026-11-07 on the emulator: [evidence](evidence/UX-WAVE5-2026-11-07.md).

On the phone:

1. At the phone's largest font and display size, confirm all five bar captions are whole and the bar does not crowd its icons.
2. On Calendar, confirm the month shows seven columns without scrolling sideways and that every day can be tapped.
3. Rotate to landscape on each root page and confirm the heading is smaller and more rows show; rotate back.
4. Open a Task or Other without an amount and confirm "No amount" is quiet text.
5. Check both themes, then restore the phone's display settings.

## Prototype alignment verification (PA01–PA10)

Use the [prototype alignment spec](../../specs/prototype-alignment/spec.md#page-targets-and-acceptance), the [PRD decisions](../PRD.md#prototype-alignment-decisions-2026-11-07) and the [SDD contract](../SDD.md). Automated tests: App [DuesFilterTests](../../tests/DueDateChecker.App.Tests/DuesFilterTests.cs) cover search and filters on Dues and how they meet the UX02 fold; [DashboardTests](../../tests/DueDateChecker.App.Tests/DashboardTests.cs) cover the summary and the two lists; [PrototypeAlignmentTests](../../tests/DueDateChecker.App.Tests/PrototypeAlignmentTests.cs) cover the editor's unsaved-changes check, editor rows, calendar count words and row projection; CategoryTests, OptionalAmountTests and RecurrenceTests carry the moved assertions. Layout, headers, glyphs and dialogs exist only on a device.

On the emulator and then the phone, in light and dark, at 100% and 200% text, and on a narrow phone:

1. Each root page shows the wordmark header; the bell opens Reminders (PA01).
2. Details and Reminders show "‹ Back" and a centred title; Details' pencil opens the editor; the editor shows the bin only when editing (PA02).
3. Dashboard shows the summary, counters, "Needs attention", "A little further ahead" and the footnote; a counter opens Dues with that status; "All dues" opens Dues unfiltered; a row opens Details (PA03).
4. On Dues, search, chips, the Type filter and Reset filters work, and the earlier paid row appears only with no filter (PA04).
5. Rows show the type icon, the repeat line for repeating dues and the plain status (PA05).
6. Calendar shows "1 due" / "2 dues", the Today link and the footnote, seven columns on a narrow phone (PA06).
7. In the editor, type something and press Cancel, then system Back (after the keyboard closes): both ask "Discard changes?"; Keep editing keeps the text; Discard leaves without saving; with no changes both leave at once. "Change" offers the four types (PA07).
8. Settings shows the cards and the fixed "✓ Save"; Reminders shows the state footer and the readiness section, open by default when reminders are on and maker steps exist (PA09, PA10).
9. On the phone only: the floating bar's gap above the system navigation in the phone's own navigation mode.

## F04 preference verification

Use [PF01-PF07](../../specs/f04-preferences/spec.md#acceptance-scenarios), [PRD preferences](../PRD.md#f04-preference-rules) and [SDD contract](../SDD.md#f04-preference-contract). Run Core/App suites and Android Release build; do not treat a fake preference store as an Android persistence/theme pass.

App [preference tests](../../tests/DueDateChecker.App.Tests/AppPreferencesTests.cs) exercise missing/round-trip/unknown-format values, last-good read retry, invalid selectors, failed Save without publication, retained drafts and new-page reset. [Calendar tests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs) verify both layouts/header alignment, leap years/DateOnly bounds and unchanged month/selection/agenda/saved fields. Native checks: data-preserving upgrade; default/selected pickers; draft Back versus Save; System runtime OS changes and explicit override; restart/offline persistence; cached Calendar re-layout with selection retained; Settings-to-Reminders Back without opt-in; both themes at 100%/200% and scrollable actions. Record installed APK hash and actual observations, restore original preferences/device state, and keep financial data out of logs/screenshots. Full assistive/cross-device and intended-key gates stay separate.

## F05 reminder verification

Use [RM01-RM08](../../specs/f05-reminders/spec.md#acceptance-scenarios), [PRD policy](../PRD.md#f05-reminder-rules) and [SDD contract](../SDD.md#f05-reminder-contract). Run Core/App sequentially and Android Release build. Record exact source/artifact hashes; do not infer device passes from fake-platform tests.

1. Upgrade the existing schema-2 app without clearing data. Compare existing saved-data membership in memory, verify Reminders off, default day-before + due-date/09:00/22:00-08:00, and no launch permission prompt. Confirm all original core routes still work.
2. Open Reminders from Dues; verify tabs hidden/Back return, draft edits retained on Refresh but discarded on leaving without Save, saved settings persist offline/restart, and no implicit permission request. Enable explicitly: deny and verify no opt-in, then grant via Android settings/Enable. Check app and channel blocking separately; CRUD remains usable. Test notification is generic and requires opt-in/access. With no eligible dues, post Test, then Refresh/leave/reopen Reminders and verify its card remains in the actual notification drawer. Disable or block access and verify test/due notifications are cleared. Record the APK and distinguish Android posting, actual drawer visibility and optional banner/sound observations.
3. With controlled task dues and a near-future Manila schedule, verify a queued inexact alarm/recovery job and real notification after screen-off/background/offline. N1/N2: inspect sample shade details (amount including zero, absent amount, date, repeat; never notes), generic action-free public lock-screen version, and grouped distinct-due count. Never log personal financial content. Tap validates ID/Details including cold start; payment confirmation taps can open the now-paid due. During an editor draft navigation defers and preserves input. Ignore obsolete/missing/deleted/invalid targets.
4. Save payment/date/settings changes, soft-delete task dues, and disable; verify obsolete pending/displayed notifications are removed. Restart/reconcile repeatedly without deliberate replay. Quiet-hour shifts/expired/new elapsed times and large batches require controlled dates/clocks; do not alter the user's device clock for convenience.
5. Check Light/Dark at 100%/200% text, scrolling, time dialogs, busy states, status labels, Android settings return and Back. Full TalkBack/keyboard/switch/focus/other widths require separate observation.
6. In an authorized test environment verify reboot-after-unlock, package update, Doze/OEM restrictions, force-stop/reopen, clock/timezone events, permission/channel revocation and controlled native database/post/navigation failures. A forced JobScheduler run is not proof of natural alarm timing or reboot delivery.
7. Clean only task-created dues/notifications; restore original reminder opt-in/saved schedule, phone theme/font/connectivity and permission/channel state when changed. Verify restart absence and preserved unrelated dues. Record unperformed lifecycle/assistive/intended-key gates explicitly. Release remains held.


F05 completion additions: verify independent Android-reported background restriction/Battery Saver/optimization text, explicit app/battery settings and draft retention on return. Do not automatically request an exemption or infer delivery from an allowed notification channel. Test expedited dispatch success/quota fallback in the App harness; native inexact delivery and Doze remain separately observed. Two clock-interleaving regressions cover expiry during a repository read and quiet-hour start within a notification batch; capture clock after reads, before each post and before choosing the next check.

N1/N2 additions ([NA01–NA08](../../specs/notification-lab/spec.md#native-acceptance--n1-a--n2-a)): pay from a real notification while unlocked, locked, and after cancelling system authentication. Verify the app does not navigate, saved state refreshes, and recurrence creates only one next due. Try duplicate taps and reminders retained across edits/deletion/payment/settings changes; no stale write. Undo immediately, after 10 seconds, after edit/delete/payment changes, and after relocking; next occurrence remains. Leave an editor draft open while paying and verify Save retains entries with a conflict message. Check feedback survives reconciliation for its lifetime, expires, and disappears on disable/block. Test API 31+ authentication and API 26–30 dedicated unlock separately; record tested API, security configuration and artifact. MIUI lock-screen visibility/action authentication and natural background delivery require phone acceptance, not a unit-test or build claim.

[Native verification helpers](../../tools/verification/README.md) provide an explicitly selected fresh API-36 AVD path, guarded SQLite fixture/fault tooling and phone restoration controls. Never use controlled clock/database faults on a personal phone. Actual reboot/update, permission/channel transitions, storage retry, paid/delete/date cancellation, keyboard and landscape observations must identify the APK that ran them. [Completion evidence](evidence/F05-COMPLETION-2026-10-18.md) preserves the initial-artifact versus final-clock-fix attribution; full TalkBack/switch/cross-device/intended-key acceptance remains separately recorded.
