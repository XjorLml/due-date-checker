# Developer dashboard proposal and repository analysis

Status: proposed, 2026-11-08 (Asia/Taipei). [Scope/scenarios](spec.md); [tasks/evidence](tasks.md). This document describes inspected source and proposed behavior, not executed application results.

## Inspected baseline

| Area | Source-backed finding | Design consequence |
|---|---|---|
| Application | [App project](../../src/DueDateChecker.App/DueDateChecker.App.csproj) targets .NET 10 Android/MAUI; [Shell](../../src/DueDateChecker.App/AppShell.xaml) has Dashboard/Calendar/Add/Dues/Settings slots. | Keep browser tooling under tools; no production dependency. Native F02 is different from this developer dashboard. |
| Prototype | [index.html](../../prototypes/design-exploration/index.html) loads plain JS/CSS, including shared [model](../../prototypes/mobile-review/model.js) and local fonts. No package.json, frontend lockfile or Vite configuration was found in the inspected repository file inventory. | Reuse the buildless approach and serve both prototype directories unchanged at their relative paths. |
| Screens | [app.js](../../prototypes/design-exploration/app.js) has roots dashboard/calendar/dues/settings; content also supports details/add/edit/reminders/catalog. The HTML has confirmation, category-picker and catalog-editor dialogs; the interaction lab can create its own task layer. | Map real screen IDs and overlays; preserve current dialogs and navigation guards. |
| Routes | render() writes hashes with history.replaceState; navigate() accepts all nine screens and awaits dirty-editor protection. loadHash() recognizes only root screens in ordinary concept links. Separate hashes open interaction-lab, ux-decisions and notifications views. | Hash changes alone cannot track all screen changes or open Add/Edit correctly. Use a small opt-in bridge with state notifications at confirmed render/navigation boundaries. |
| Tests | [prototype model tests](../../prototypes/design-exploration/tests/model.test.cjs) use node:test. [browser harness](../../prototypes/design-exploration/tests/browser-check.cjs) starts an owned loopback server and installed Chromium/Edge through CDP without npm packages. Native [App tests](../../tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj) link platform-independent sources; [Dashboard tests](../../tests/DueDateChecker.App.Tests/DashboardTests.cs) exercise saved-data rules. | Reuse the local test style for later tooling verification. View-model results do not verify native pages or the iframe. |
| Evidence | [STATUS current focus](../../docs/delivery/STATUS.md#current-focus) records implemented native presets, emulator/build evidence and a Windows Application Control test-execution block (0x800711C7). Earlier notification test passes are explicitly historical for later changes. Hosted release and intended-key phone acceptance remain separate. | Seed attributed observations with source identity; never initialize everything as Done from code, old test totals or prototype appearance. No new tests were run in this proposal pass. |
| Workflow | [AGENTS](../../AGENTS.md) requires relevant canonical docs, feature spec/plan/tasks, preserved local work and separate implementation/verification/acceptance evidence. | Maintain linked checkpoints at meaningful completion/blocker/verification boundaries. Avoid summaries on every edit. |

Environment observed for this pass: branch develop; initial HEAD 7adbe52; Node v24.16.0 and Python 3.14.6 available. The worktree contained extensive tracked/untracked changes and changed during inspection. Treat this as a working-tree observation, not an immutable revision. Preserve all unrelated work; fingerprint relevant files during implementation and checkpoint recording.

## Existing implementation versus proposal

Native CRUD, Details, Dashboard, Calendar, preferences, reminders and later approved extensions have source/specs/tests and varying evidence recorded in STATUS. For example, [DashboardViewModel](../../src/DueDateChecker.App/ViewModels/DashboardViewModel.cs) implements saved-data projections and the [native category picker](../../src/DueDateChecker.App/ViewModels/PhilippineCategoryPickerViewModel.cs) implements preset search. These source observations do not establish a current test pass or release acceptance. The prototype includes extra custom catalogs, scheduling controls and other sample-only behavior. Those do not approve native features.

No developer dashboard, durable task-reporting CLI, iframe bridge or verified Codex activity integration was found in the inspected files. They are proposed work. Initial registry scope must be audited during Stage 1; this analysis does not claim an exhaustive feature completion inventory or percentage.

Initial mapping candidates, based on inspected screen IDs (mapping is not verification):

| Requirement/feature | Existing preview target | Implementation/evidence distinction |
|---|---|---|
| R02 create | add | Native editor exists; route through navigate, not an unsupported deep hash. |
| R03 list | dues | Native saved list exists; prototype data stays sample-only. |
| R04 edit | details → edit | Native edit exists; resolve an active sample before previewing. |
| R05 payment / R06 deletion | details / edit with confirmation | Native mutation contracts exist; dashboard navigation must never invoke these writes. |
| F01 details | details | Native saved-ID page exists; an empty sample has no valid target. |
| F02 summary / F03 calendar | dashboard / calendar | Native saved-data pages exist; prototype Week view is a separate concept. |
| F04 preferences / export-import | settings | Native preferences and backup extensions exist; show any prototype/native control mismatch explicitly. |
| F05 reminders / N1–N2 | reminders / notifications lab | Native implementation/evidence recorded in STATUS; browser reminders/lab are simulations. |
| Native Philippine presets | add + category-picker overlay | Preset implementation and emulator/build evidence exist; current regression execution is recorded blocked. Custom categories/billers in catalog are sample-only. |
| R01 install / R07 offline / R08 CI | no direct screen | Environment/delivery requirements need evidence, not a substituted visual screen. |
| Onboarding, sync, budgets, statistics and remaining extended scheduling | unavailable unless an exact inspected lab mapping exists | Deferred scope does not enter approved completion totals or become native implementation through this tool. |

Reconciliation notes: older STATUS release-slice rows still say F04 export is deferred, while newer PRD/STATUS extensions record implemented export/import; older category rows also predate the new preset extension. Use the current scoped records and retain the older rows as historical context. The HTML system guide retains historical navigation/default-choice and generic-notification descriptions superseded by later approvals/source; do not treat every paragraph as current native behavior. Surface these conflicts during seed ingestion rather than copying outdated wording into registry statuses.

## Recommended architecture

Use plain HTML/CSS/JavaScript plus a Node standard-library server and CLI. Node is already available and the repository has dependency-free prototype tests; React/Vite/Tailwind would add a second toolchain without an identified need. No npm installation, external fonts or build step is needed for this design.

```text
Canonical specs / STATUS / evidence -- explicit checkpoint references --> CLI
                                                                         |
Tracked feature/scope registry + local append-only checkpoint journal ----+
                                                                         v
Git read-only adapter / explicit test-result adapter --> loopback Node server
                                                        | snapshot + SSE
                                                        v
                                              Dashboard browser UI
                                                        | validated postMessage
                                                        v
                                        Existing prototype + opt-in bridge
```

The server owns read-only HTTP endpoints for a validated snapshot, event stream and safe document links. CLI owns state writes; visual-review actions can use an authenticated same-origin POST restricted to the review schema. Bind to 127.0.0.1, validate Host/Origin, use a per-process write token, reject cross-origin writes and traversal/symlink escapes. Serve only the dashboard assets, required prototype assets and validated documentation references. Never expose the entire repository as a static directory. Resolve references locally and render document text safely without executing embedded HTML.

Proposed layout (none of these implementation files exist yet):

```text
tools/developer-dashboard/
  README.md
  server.mjs
  cli.mjs
  lib/                    # storage, validation, projection, watchers, Git/tests
  public/                 # index.html, styles.css, app.js
  data/registry.json      # tracked mappings, scope/gate references; no copied prose
  tests/                  # node:test and local-browser checks
.developer-dashboard/     # ignored persistent journal, snapshots, owned test results
prototypes/design-exploration/dashboard-bridge.js  # opt-in adapter
specs/developer-dashboard/{spec,plan,tasks}.md
```

The journal persists locally after Codex and server shutdown, but ignored runtime data does not travel with a clone. Durable delivery evidence continues to live in versioned canonical documents. Stage 3 documents backup/recovery of local history without committing financial or secret data.

UI: compact overview, searchable feature list, feature evidence/criteria inspector, resizable interactive preview, and collapsible activity/test panel. Use accessible status text, focus indicators and light/dark semantic colors. Preview controls change iframe viewport size; the prototype's own phone-stage dimensions may remain constrained. Label browser viewport and phone simulation separately rather than claiming a native tablet/desktop layout.

## Registry and evidence contract

The registry owns stable mapping/scope metadata and pointers to canonical criteria, not parallel requirement prose. Example shape, to be validated against inspected scenario IDs when seeded:

```json
{
  "schemaVersion": 1,
  "features": [{
    "id": "F02",
    "name": "Saved-data Dashboard",
    "phase": "Approved extensions",
    "source": "specs/f02-dashboard/spec.md#acceptance-scenarios",
    "relatedFiles": ["src/DueDateChecker.App/ViewModels/DashboardViewModel.cs"],
    "criteria": [{"id": "DB01", "source": "specs/f02-dashboard/spec.md#acceptance-scenarios"}],
    "prototype": {
      "file": "prototypes/design-exploration/index.html",
      "concept": "a",
      "screenId": "dashboard",
      "overlay": null,
      "sampleId": null
    },
    "unavailableReason": null
  }],
  "scopes": []
}
```

Each scope explicitly lists unique criterion/boundary gate pairs and mandatory/optional applicability. Status projections join registry entries with validated checkpoint events. Events carry eventId, featureId, sessionId (optional), kind, occurredAt, observer, environment, source identity (commit plus relevant working-file hashes), criterion/boundary/result and canonical evidence reference. Session events add task/checkpoints/blockers and changed-file paths. Visual reviews identify prototype dependency hashes rather than native artifacts. Empty fields stay unknown; timestamps are never invented from file modification dates.

Source-of-truth rule: update canonical delivery evidence when warranted, then record the CLI checkpoint referencing it. The journal owns local activity, not product truth. On a referenced document/source change, mark affected projections needs-reconciliation/stale and retain the original observation in history. Do not parse arbitrary Markdown checkboxes into passing results or overwrite STATUS from a generated summary. Render source links for audit. An explicit conflict remains visible and cannot contribute to a verified count. Startup validates/replays the journal; writes use one serialized writer/lock, full-event validation and atomic snapshots. A torn final journal event is reported and isolated with recovery preserving the valid prefix; no silent history deletion.

Proposed CLI operations: serve, checkpoint, session start/update/end, review, and verify with named supported suites. Validate all IDs/paths/results before writing. Recording a claimed pass requires a canonical evidence reference and attribution; verify runs capture their own observed outcomes. Do not execute caller-supplied shell strings. Exact command flags will be documented when implemented.

## Preview navigation and communication

Load the real prototype with an explicit dashboard-embed query parameter. Add one adapter script and minimal integration calls at existing render/navigation/overlay boundaries. Standalone behavior stays unchanged when the parameter is absent. Route through existing navigate()/Lab.allowLeave() and picker APIs; never synthesize arbitrary DOM clicks or bypass guards.

Protocol envelope: channel, version, per-frame handshake nonce, requestId, type and validated payload. Allowlisted parent commands: handshake, get-state, navigate. Child messages: ready, state, navigation-result, error. Both sides require exact same origin and the expected event.source (iframe.contentWindow or window.parent). Use explicit targetOrigin, payload schemas and allowlisted screens/overlays; no code/HTML/file execution messages. Track request IDs, timeout and iframe generation to reject stale replies. Guard concurrent navigation; report cancelled/busy/unavailable explicitly.

Publish state after actual rendering/navigation and dialog transitions; history.replaceState does not produce hashchange. Dashboard applies child state without echoing navigation, and waits for acknowledgement before selecting the requested feature. For Details/Edit, resolve a valid existing sample due by configured sample ID or a validated active sample fallback; return unavailable when the sample is empty/deleted. Navigate through Details before Edit so its parent chain remains valid. Category-picker is an overlay on Add/Edit, not a fabricated route; Settings catalog management is sample-only and must carry that label. Confirmation dialogs are observed, not auto-confirmed by the dashboard. UX/notification lab mappings use their existing dedicated view APIs/hashes only when explicitly registered.

Several features may share a screen (validation, amount rules, recurrence). Preserve an explicitly selected related feature while it still matches; otherwise show related candidates instead of choosing arbitrarily. Features with no UI retain null mappings and a specific backend-only/unimplemented/unmapped reason.

## Updates, Git, test results and sessions

- Watch narrow source roots and registry/journal files, debounce bursts and re-establish watches after atomic file replacement. Emit versioned SSE notifications; reconnect fetches the latest complete snapshot rather than relying on missed deltas. Source edits trigger preview refresh separately from dashboard state updates.
- Include dependencies under prototypes/mobile-review (model/fonts) in preview fingerprints and watchers. Defer dirty/modal reload; warn that in-memory sample dues reset on refresh. Routine state updates keep the iframe instance alive.
- Read fixed Git metadata commands on startup/relevant Git events/explicit refresh. Watch resolved Git/common directories for linked-worktree compatibility; never expose raw .git files. Show snapshot time. Full git status may be debounced at checkpoints/refresh, not on every filesystem event. No automatic commit/push or repository scan.
- Test execution is explicit and serialized for shared .NET output, following TEST-PLAN. Store exit/result, timestamps, environment, relevant source hashes and sanitized evidence pointers. Default viewing reads results only. Hosted CI integration is deferred unless a supported local result/import adapter is available; local branch metadata does not prove a hosted run passed.
- Session start/update/end comes from CLI checkpoints. These events mean reported activity. The dashboard server's SSE connection is unrelated to Codex liveness. Initially label tasks last-reported, completed or unknown/stale, and never display confirmed-active Codex without a real verified lifecycle adapter. Staleness uses a documented configurable threshold and preserves last-update time.
- No LLM calls, recurring summaries or repository scans on page refresh. Normal rendering uses validated snapshots and deterministic calculations. Checkpoints occur at task start, meaningful completion/blocker changes, verification results and session end; not every command or file edit.

## Stages and documentation changes

1. **MVP:** local server/UI, registry and approved scope inventory, attributed seed observations, deterministic progress, prototype iframe/viewport controls and safe source refresh. Deliver DD01–DD05 plus storage/security essentials. Display unregistered coverage explicitly.
2. **Connected preview:** guarded adapter, bidirectional state, root/pushed/overlay mappings, shared-screen handling, unavailable states and source-scoped visual reviews. Deliver DD06–DD09 and prototype regression coverage.
3. **Automation:** CLI checkpoints/session journal, fixed Git/test adapters, history/recovery, SSE refinement and AGENTS workflow. Complete DD10–DD14 and cross-stage verification.

Approval covers all three stages in sequence. At each stage, record actual results and remaining acceptance rather than adding approval stops. During implementation, add the spec to docs/README; add a developer-tool boundary/reference in SDD and browser-specific procedures/evidence links in TEST-PLAN/STATUS. PRD/DESIGN review concluded that this proposal adds no native behavior or UI-system contract; link existing authorities instead of copying requirements. Record an ADR only if a significant choice has supplied rationale and meets the repository ADR process. Preserve any concurrently changing canonical text.

## Verification and material limits

Use node:test for deterministic gate aggregation, source invalidation, schema validation, journal concurrency/recovery, message validation and path restrictions. HTTP integration checks cover traversal, wrong origins/hosts, write authentication and SSE reconnect. Browser checks exercise actual root/pushed/modal navigation, cancellation, shared-screen selection, dirty refresh, resizing and iframe preservation. Reuse installed local Edge/Chromium conventions after reading the Sites local preview guidance; do not install new browser tooling just for the proposal.

Run affected prototype model/browser regressions when its bridge is added. Native .NET tests/builds are needed only if implementation actually changes native contracts/source; never rerun them to validate Markdown. If explicitly running native verification, retain the known application-control limitation and record actual outcomes. No security-policy bypass is proposed. No browser pass establishes Android device acceptance.

Initial evidence ingestion needs source/artifact attribution and may leave many gates unknown. Prototype/native differences are expected and must remain labeled. Source hashes can show staleness, not prove behavior. No current percentage is claimed. The dashboard reduces repeated status queries but cannot remove the need to record accurate checkpoints or verify changed behavior.
