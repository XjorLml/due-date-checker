# Local developer dashboard

Status: **Proposed; awaiting architecture approval. No implementation authorized by this proposal.**

Requested scope: the user's developer-dashboard prompt, invoked on 2026-11-08. This is local developer tooling, separate from the native F02 Dashboard. [Repository analysis and technical proposal](plan.md); [execution tracking](tasks.md).

## Authority and boundaries

- [PRD](../../docs/PRD.md#approved-requirements) retains R01–R08 and approved extensions. The dashboard displays their progress; it does not add a native product requirement or change acceptance.
- [SDD](../../docs/SDD.md#3-source-boundaries) retains Android/Core boundaries, offline storage and privacy contracts. No native application, database, notification or signing change is proposed.
- [DESIGN scope](../../docs/DESIGN.md#scope-and-authority) applies to the adopted product system. Use compatible typography, semantic colors, accessible labels and focus in the tool; native dp layouts and deferred product concepts do not become browser requirements.
- [STATUS](../../docs/delivery/STATUS.md) remains the delivery authority. [TEST-PLAN](../../docs/delivery/TEST-PLAN.md) owns native acceptance. Dashboard records are attributed projections of linked evidence, not a competing status document.
- The [HTML system guide](../../prototypes/design-exploration/SYSTEM.md), [audit](../../prototypes/design-exploration/AUDIT.md) and [interaction audit](../../prototypes/design-exploration/INTERACTION-AUDIT.md) govern preservation of this sample-data artifact.

## Observable acceptance scenarios

| ID | Scenario and expected result | Boundary |
|---|---|---|
| DD01 | Start one documented local command; overview, feature list and interactive prototype load without network services or AI inference. | Local server/browser |
| DD02 | Select an approved scope/milestone. Counts and percentages use unique linked acceptance gates, show numerator/denominator and unknown/stale results, and exclude deferred/unapproved scope. Empty scope shows N/A. No partial-completion estimates. | Projection tests/UI |
| DD03 | Inspect a feature. Show stable ID, phase, workflow status, canonical criteria links, implementation, automated verification, emulator/phone evidence, acceptance and last checkpoint independently. Historical passes never satisfy a changed artifact silently. | Projection tests/UI |
| DD04 | Use the embedded prototype at mobile/tablet/desktop viewport widths, fullscreen or a separate tab. Existing sample navigation, validation, dialogs and draft protection still work. Viewport size does not establish native acceptance. | Browser/regression |
| DD05 | Change a prototype dependency, including shared mobile-review assets. Only the affected preview reloads; dashboard changes never reload it. A dirty draft or open modal defers automatic reload and offers an explicit reload action. | Watcher/browser |
| DD06 | Select Preview Feature for a mapped root, Add, Details, Edit or catalog overlay. The existing guarded navigation opens the exact mapped target or reports cancellation/unavailability; no sample writes occur. Current feature/screen update only after confirmed navigation. | Bridge/browser |
| DD07 | Navigate inside the iframe, including modal open/close and guarded cancellation. Dashboard selection follows actual state without feedback loops or iframe replacement. Shared-screen features remain a list of related features; the tool does not guess one uniquely. | Bridge/browser |
| DD08 | Send invalid messages, wrong-origin/wrong-window messages, unsupported targets or stale acknowledgements. They cannot navigate or mutate tracking records. Missing mappings show a reason and no unrelated screen. | Protocol/browser |
| DD09 | Approve a visual review or request changes, then restart. The review persists locally with its preview-source fingerprint; changed sources make it stale. Neither action verifies native/backend behavior or release acceptance. | CLI/storage/browser |
| DD10 | Record a valid CLI checkpoint referencing canonical evidence. Dashboard refreshes incrementally and history survives process/session restart. Malformed updates, concurrent writes and partial saves preserve the previous valid state and report errors. | CLI/storage/integration |
| DD11 | Inspect session activity. Show task/checkpoints, changed-file paths, blockers and verification references. Explicit session start/update/end records remain last-reported activity; task In Progress or a connected dashboard never proves Codex is running. Missing/expired activity shows unknown/stale. | Projection/browser |
| DD12 | Inspect Git/test panels. Local branch/commit/dirty state and observed test outcomes carry source identity, time and observer. No test executes on viewing or source changes. A blocked/failed run is visible separately from historical passing runs. | Fixed-command/integration |
| DD13 | Open the tool or request files/API operations outside its allowlist. It binds only to loopback, rejects unsafe host/origin/path requests, and never serves signing files, .git contents, databases, environment files or arbitrary repository paths. | HTTP/security |
| DD14 | Use search, status/phase filters, keyboard navigation and a narrow browser. Information remains readable, statuses have text, and preview/feature selection has visible focus and clear active state. | Browser/accessibility |

## Completion policy

Workflow statuses: Backlog, In Progress, In Review, Blocked and Verified. Implementation may be complete while required verification or acceptance is pending. Verified requires all applicable, current gates with attributed evidence; a visual-only pass cannot satisfy another verification boundary.

Gate results are pass, fail, blocked, not-run, stale or not-applicable. Not-applicable requires an explicit scope reason. An approved scope lists its unique gate IDs and required verification boundaries. Completion is `100 * current passing applicable gates / applicable gates`; mandatory unknown, blocked, failed and stale gates remain in the denominator. Show implementation and each verification boundary separately rather than averaging them into one misleading percentage. Optional R08 and deferred features have separate scope groups. Freeze and version each scope baseline; display added/removed gates when it changes. Report registered coverage, never imply that an incomplete registry measures the whole app.

Evidence must identify date, observer (agent-observed, user-reported or historical), environment and source/artifact identity. Source changes invalidate matching fingerprints conservatively; unmatched historical artifacts remain historical. An evidence-document edit prompts reconciliation, never an automatic pass. Existing task checkboxes and Git commits alone are not verification evidence.

## Deferred and excluded

No hosting, cloud account, external AI/API integration, production database access, repository-wide background analysis, automatic tests, arbitrary command runner or assumed access to Codex private session internals. No prototype redesign or native feature implementation. A reliable Codex lifecycle adapter may be added only if an actual supported integration is identified; the initial tool uses explicit checkpoint reporting.

## Approval boundary

The invoked prompt explicitly says to present the architecture and wait for approval before implementing. This proposal supplies that reviewable result. Approval of the plan authorizes its three implementation stages; it does not authorize publishing, native data changes, or accepting unperformed device tests.
