# Implementation plan — V1.3

This as-built plan records the completed implementation and its remaining acceptance work on 2026-10-17. [Spec/scenarios](spec.md) own feature detail; [tasks](tasks.md) own execution tracking.

## Affected modules

| Module | Change |
|---|---|
| App/Pages/DueEditPage | Add editor-only Delete button; await native confirmation; pass the result to the command; return after success; preserve page action guard. |
| App/ViewModels/DueEditViewModel | Eligibility/visibility, original-title confirmation message, guarded Boolean delete command, deletion busy/error/completion states, navigation retry without another mutation. |
| App/Resources/Styles | Semantic destructive/surface resources and outlined destructive button style; explicit BaseButton key for source generation. |
| Core/Data/SqliteDueRepository | Reuse existing SoftDeleteAsync unchanged; idempotent active-row UPDATE already satisfies the data contract. |
| Core.Tests and App.Tests | Add meaningful cancellation, retained-data, repeat/unknown-id, retry, concurrency, and reopened-file checks. |

## Dependencies and choices

- Reuse IDueRepository, DatabaseDispatcher, TimeProvider, and native DisplayAlertAsync; introduce no package or dialog-service abstraction.
- Keep platform dialog presentation in the page and deletion/state rules in the view model.
- Do not validate/save unfinished form values when deleting the stored record. Confirm against the loaded identity/title.
- Reuse the existing primary-key lookup and schema version 1. No migration or added index is needed.
- Use DESIGN semantic destructive styling while retaining the PRD's approved no-Undo scope.

## Verification sequence and outcomes

1. Added App tests before implementation; the initial run failed to compile against absent delete members.
2. Implemented view-model/page flow and added Core idempotency/isolation checks. All 70 cases passed (28 Core, 42 App).
3. Built Android Release. Corrected an initial type-key StaticResource incompatibility using an explicit BaseButton resource; final build passed with zero warnings/errors.
4. Installed the APK with data preserved. Used temporary dues to verify native confirmation, Cancel, confirmed deletion, and hidden state after process restart.
5. Recorded remaining airplane-mode, native edge/accessibility, and intended-key signing acceptance separately.

## Risks and limits

No production data migration or hard delete is introduced. Ordinary SQLite/app-private storage and current signing limitations remain. The phone tests intentionally leave only hidden test tombstones. A future restore/sync feature requires its own approved contract; this slice does not implement one.

## Contract and source navigation

Use [SDD domain/mutation rules](../../docs/SDD.md#51-domain-types-and-mutation-rules), [repository contracts](../../docs/SDD.md#53-repository-contracts), [navigation](../../docs/SDD.md#6-ui-and-navigation), and [dispatcher/startup](../../docs/SDD.md#7-dependency-lifetimes-and-startup) as governing contracts.

Affected source: [page XAML](../../src/DueDateChecker.App/Pages/DueEditPage.xaml), [page handler](../../src/DueDateChecker.App/Pages/DueEditPage.xaml.cs), [view model](../../src/DueDateChecker.App/ViewModels/DueEditViewModel.cs), [styles](../../src/DueDateChecker.App/Resources/Styles/Styles.xaml), [colors](../../src/DueDateChecker.App/Resources/Styles/Colors.xaml), [App tests](../../tests/DueDateChecker.App.Tests/DeleteDueTests.cs), and [Core tests](../../tests/DueDateChecker.Core.Tests/SqliteDueRepositoryTests.cs).

## Remaining verification dependencies

The existing editor loading, dispatcher, clock, repository guards, and list refresh are prerequisites for the implemented flow. Remaining native checks depend on a phone/test artifact; final acceptance additionally depends on intended-key signing. The App test harness cannot validate dialogs, XAML, page handlers, lifecycle, hardware Back, or Release trimming.

Use [TP04-TP07](../../docs/delivery/TEST-PLAN.md#device-procedures) for remaining delete/offline/hardening checks and TP01 for the final artifact. Retain completed evidence and record only new outcomes in [STATUS](../../docs/delivery/STATUS.md) and linked execution records.
