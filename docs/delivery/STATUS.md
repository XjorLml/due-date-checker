# Delivery status

Authority: current delivery state and attributed verification evidence. Approved scope is in the [PRD](../PRD.md); procedures and gates are in the [test plan](TEST-PLAN.md). Historical phase records remain evidence, not current status definitions.

Last documentation/source inspection: **2026-10-17**. Latest automated/build and V1.3 device evidence: **2026-10-17**, from the authorized soft-delete feature task, recorded in the [execution evidence](evidence/V1.3-2026-10-17.md). The documentation refactor itself performed no application tests/build/device checks; the feature-task evidence below is separately attributed.

## Release slices

| Slice / requirements | Implementation state | Automated evidence | Device evidence / remaining acceptance |
|---|---|---|---|
| V1.0 / R01 | Android project and conditional signing configuration present | Historical Release build passed; manifest recorded minSdk 26, targetSdk 36, allowBackup=false | V1.2 install and explicit activity-start observed historically. Intended-key signing and launcher acceptance remain pending; activity-start is not a launcher check. |
| V1.1 / R02, R03, R07 | Create/list implementation present | Historical 23 Core + 17 App cases passed at V1.1; included in later V1.2 regression run | Separate create/list/restart/offline phone checklist remains open in [V1.1](../phases/V1.1.md#remaining-acceptance). |
| V1.2 / R04, R05, R07 | Edit/paid implementation present | Historical 26 Core + 34 App cases passed (60 total); Release build had zero warnings/errors | Navigation/loading/Cancel observed by agent; saved edits, paid/unpaid, restart, and airplane-mode checks user-accepted. Intended-key run and unchecked hardening remain open in [V1.2](../phases/V1.2.md#phone-acceptance-record). |
| V1.3 / R06, R07 | Confirmed soft-delete implementation complete; committed through `7c22b4b` after the evidence run, with [matching feature-file hashes](evidence/V1.3-2026-10-17.md#post-record-source-commit-check) | Agent-observed 28 Core + 42 App passed (70 total); final Release build: zero warnings/errors. See [artifact/environment evidence](evidence/V1.3-2026-10-17.md). | Agent verified named confirmation, native Cancel retention, confirmed list removal, and hidden state after process restart using temporary dues only. Offline, Back/dismissal, physical phone tombstone inspection, hardening, and intended-key gates remain open. See [feature tasks](../../specs/v1.3-soft-delete/tasks.md). |
| V1.4 optional / R08 | GitHub Actions workflow committed locally (not yet pushed) per the [V1.4 spec](../../specs/v1.4-ci/spec.md): Core + App tests on a Linux runner on every push | Its two test commands passed locally on 2026-10-17 (28 Core + 42 App). No GitHub run yet. | Optional; does not block mandatory v1 acceptance. R08 closes on an observed passing GitHub run. |

“Implementation present” is a source-inspection finding, not a claim of passing tests or release acceptance. Historical test totals must not be presented as results for the newer working tree.

## Evidence register

| Evidence | Date / provenance | Result and limits |
|---|---|---|
| [V1.1 verification](../phases/V1.1.md#verification-recorded-on-2026-10-16) | 2026-10-16, implementation record | 23 Core + 17 App passed; Release build clean; native SQLite libraries recorded for arm64-v8a/x86_64. No device attached at that check. |
| [V1.2 automated verification](../phases/V1.2.md#automated-verification-2026-10-16) | 2026-10-16, implementation record | 26 Core + 34 App passed; build clean. NU1900 vulnerability-metadata warning retained; cached dependencies used. |
| [V1.2 device verification](../phases/V1.2.md#device-verification) | V1.2 record; a separate device timestamp was not recorded | Agent observed install/activity-start, loaded fields, switch interaction, repeat selection, and Cancel without saving. User subsequently accepted saved-edit/paid/restart/offline checks; exact confirmation timestamp not recorded. |
| V1.3 source inspection | 2026-10-17, this documentation audit | [Page](../../src/DueDateChecker.App/Pages/DueEditPage.xaml.cs), [view model](../../src/DueDateChecker.App/ViewModels/DueEditViewModel.cs), [App tests](../../tests/DueDateChecker.App.Tests/DeleteDueTests.cs), and [Core tests](../../tests/DueDateChecker.Core.Tests/SqliteDueRepositoryTests.cs) contain deletion behavior and coverage. Not executed here. |
| [V1.3 automated/build execution](evidence/V1.3-2026-10-17.md#executed-checks) | 2026-10-17, agent-observed feature implementation | 28 Core + 42 App passed; clean final Release build. Source hashes, base revision, device environment, and APK SHA-256 are recorded; the source was uncommitted at execution time. |
| [V1.3 source-commit check](evidence/V1.3-2026-10-17.md#post-record-source-commit-check) | 2026-10-17, agent-observed while completing the documentation refactor | All eight recorded feature-file hashes match committed content at `7c22b4b` (four after line-ending normalization). No tests, build, device checks, or APK re-hash were performed; unhashed files are not proven identical to the tested tree. |
| [V1.3 phone execution](evidence/V1.3-2026-10-17.md#executed-checks) | 2026-10-17, agent-observed on model M2101K6G / Android 13 API 33 | Installed with data preserved; named confirmation, native Cancel, confirmed deletion, and process-restart absence passed on temporary verification dues. No existing user due changed. Phone-private tombstones and unperformed gates are not claimed. |
| Local test run on committed source | 2026-10-17, agent-observed; Windows, .NET SDK 10.0.401 | 28 Core + 42 App passed on `7c22b4b` plus uncommitted Markdown and workflow files (no application source changes). Run while adding [V1.4](../../specs/v1.4-ci/tasks.md); not a CI run, Release build, or device check. |
| Documentation validation | 2026-10-17 | See [refactor validation](DOCS-REFACTOR.md#validation-results); does not validate application runtime behavior. |

## Remaining release work

1. Current delete implementation has recorded automated/build evidence. Keep that evidence tied to its source/artifact hashes; rerun relevant checks after further implementation changes.
2. Configure and verify intended-key signing using the ignored local signing configuration. Historical absence of `local.signing.props` is not a fresh inspection of signing identity.
3. Complete [TP01–TP07](TEST-PLAN.md#device-procedures), preserving already attributed evidence and recording the outstanding V1.1/V1.2 checks explicitly.
4. Complete remaining [V1.3 acceptance tasks](../../specs/v1.3-soft-delete/tasks.md), including offline/native edge cases and phone tombstone evidence in an authorized test environment. Native named confirmation, Cancel, confirmed removal, and restart visibility already have agent-observed evidence.
5. Optional R08: push the V1.4 workflow and record the first passing GitHub run in [V1.4 tasks](../../specs/v1.4-ci/tasks.md).

Known verification gaps include controlled migration rollback and explicit equal-due-date ordering coverage. See the [automated coverage inventory](TEST-PLAN.md#automated-coverage-inventory).

## Limits and deferred work

The [SDD](../SDD.md#8-failure-handling-privacy-and-practical-limits) documents plain local SQLite, data-loss limits, no restore flow, schema-integrity limitations, and concurrency boundaries. Full DESIGN compliance, including accessibility, has not been established; brand adoption and design/product conflicts are recorded in the [audit](DOCS-REFACTOR.md#unresolved-decisions).

Reminders, recurrence, authentication, synchronization, budgets, savings, export, iOS, and Play distribution remain deferred under the PRD. The old seven-day schedule is historical, not a renewed commitment.
