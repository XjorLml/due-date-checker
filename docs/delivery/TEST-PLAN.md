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
| R02–R06 / persistence | [SqliteDueRepositoryTests](../../tests/DueDateChecker.Core.Tests/SqliteDueRepositoryTests.cs): initialization/idempotence, round trips, date sorting, missing IDs, active-row update safeguards, retained tombstones, repeated/unknown delete, newer-schema data preservation | No explicit controlled failing-migration rollback test or equal-due-date/created_at ordering test found. Add during relevant hardening work, not this documentation refactor. |
| Domain rules | [MoneyTests](../../tests/DueDateChecker.Core.Tests/MoneyTests.cs), [DueTests](../../tests/DueDateChecker.Core.Tests/DueTests.cs): rounding, negative amounts, formatting/value equality, create defaults, title validation/trim, `with` validation, identity | No inferred coverage beyond these tests. |

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

## Documentation checks

For documentation changes, check required files, local Markdown paths and heading anchors, stable requirement IDs, technical-contract coverage, history preservation, feature scope, and the final diff against the initial working tree. Existing platform references in historical records are not evidence of current external policy; revalidate them when that work begins.
