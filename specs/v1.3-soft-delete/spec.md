# V1.3 — Confirmed soft deletion

Status: Implemented and verified at automated/basic phone boundaries on 2026-10-17; remaining acceptance is tracked in [tasks](tasks.md).  
Governing requirements: [PRD R06, R07](../../docs/PRD.md#approved-requirements); list integration also exercises R03.  
Technical contracts: [SDD repository contracts](../../docs/SDD.md#53-repository-contracts) and [delete flow](../../docs/SDD.md#6-ui-and-navigation).  
Implementation approach: [plan](plan.md). Evidence: [V1.3 record](../../docs/phases/V1.3.md).

## Scope

Expose the existing soft-delete contract through an editor-only Delete action and explicit native confirmation. The PRD owns the product rule: hide the due while retaining its database row. No Undo, restore, hard delete, schema migration, dependency addition, or notification work is included.

## Acceptance scenarios

| ID | Scenario | Expected observable result | Verification |
|---|---|---|---|
| SD01 | Open a new form, an unavailable target, or an unloaded editor. | Delete cannot be invoked; a new form has no Delete action. | App tests; native new-form check. |
| SD02 | Request Delete on a loaded existing due. | A dialog names the stored due and offers Cancel/Delete. Unsaved form changes do not change its target. | App message-target check; native dialog check. |
| SD03 | Cancel or dismiss confirmation. | No storage call; editor/input and stored row remain unchanged. | False-result App test; native Cancel button check. Native Back/dismissal remains pending. |
| SD04 | Confirm Delete. | One SoftDeleteAsync call with the due id and one injected clock instant; no unfinished form edit is saved. | App/SQLite tests. |
| SD05 | Deletion succeeds. | Return to the reloaded list; due is hidden. Reopen storage/restart app and it remains hidden; the row/other fields remain stored. | Reopened-file App/Core tests; native list/process-restart visibility check. |
| SD06 | Repeat Delete, including after another writer deleted the same row. | No resurrection or changed original tombstone timestamps. Pending/completed operations prevent duplicate mutation. | App/Core tests. |
| SD07 | Storage fails or return navigation fails. | Storage failure keeps input and allows a newly confirmed retry. Successful deletion followed by failed navigation offers return without another mutation. | App tests; native controlled-failure coverage remains pending. |
| SD08 | Execute delete/cancel/restart in airplane mode. | Same functional behavior with no network dependency. | Native airplane-mode acceptance remains pending under R07. |
| SD09 | Confirmation display fails. | Report the error and perform no storage call; allow a new confirmation attempt. | Page error handler present; controlled native confirmation-display failure remains pending under TP05. |

## State and concurrency

The page action guard spans confirmation, command execution, and return navigation. The view model independently checks the Boolean response and eligibility before storage. Busy state disables Save/Delete; successful deletion marks HasDeleted/HasCompleted so a navigation retry cannot save or delete again. There is no product state that restores a tombstone.

## Evidence limits

Automated/build results and artifact/source hashes are recorded in the [execution evidence](../../docs/delivery/evidence/V1.3-2026-10-17.md), separately from this documentation refactor. Phone checks used only agent-created zero-amount dues; existing user dues were not edited/deleted. Row retention was verified through SQLite tests, not by reading the phone's private database. Remaining offline, native edge/accessibility, and intended-key signing checks are listed in the phase record/tasks. These are observed results, not inferred acceptance of unchecked cases.
