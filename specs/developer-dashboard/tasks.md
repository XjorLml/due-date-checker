# Developer dashboard task tracking

Status: proposal prepared; architecture approval pending. [Scenarios](spec.md#observable-acceptance-scenarios); [analysis/plan](plan.md).

## Proposal evidence — 2026-11-08

Agent-observed source/document inspection in the Windows PowerShell workspace. Initial branch develop / HEAD 7adbe52; dirty working tree, with concurrent changes observed during the pass. Node v24.16.0 and Python 3.14.6 were reported by local commands. These observations do not establish a prototype runtime pass, native test pass or device acceptance.

- [x] Read docs/README and applicable AGENTS workflow.
- [x] Inspect relevant PRD/SDD/DESIGN authority/scope, delivery focus/test gates and existing soft-delete spec structure.
- [x] Inspect prototype shell, renderer/navigation, dirty guard, dialogs, shared dependencies and existing test harness.
- [x] Inspect native Shell, relevant view models, test definitions/project boundaries and existing CI definition.
- [x] Prepare scoped acceptance scenarios, architecture, registry/evidence contract, navigation protocol, staged work and limitations.
- [x] Validate proposal local links, scenario traceability and scoped diff; record outcome below.
- [ ] Receive architecture approval required by the invoked prompt.

## Stage 1 — local overview and preview

- [ ] Audit/register approved requirement/feature scenarios and explicit scope baselines; retain unknown/unmapped/deferred distinctions.
- [ ] Implement loopback server, safe asset/document access, schema validation and persistent local state foundation.
- [ ] Implement compact overview, searchable feature/criteria/evidence views and deterministic boundary-specific progress.
- [ ] Embed existing prototype, viewport/fullscreen/new-tab controls and dependency-aware refresh with dirty/modal protection.
- [ ] Validate DD01–DD05 and applicable DD10/DD13/DD14 foundations; record exact observed outcomes.

## Stage 2 — connected review

- [ ] Implement opt-in prototype adapter and validated, request-correlated handshake/navigation/state protocol.
- [ ] Map root/pushed screens and overlays using inspected IDs; handle unavailable samples, guarded cancellation and shared-screen features.
- [ ] Persist prototype-source-scoped visual reviews separately from functional verification.
- [ ] Verify DD06–DD09 and affected prototype regressions; record actual outcomes and limits.

## Stage 3 — reporting and verification integration

- [ ] Implement checkpoint/session CLI, history, serialized writes, atomic snapshots and corruption recovery.
- [ ] Implement read-only Git metadata and explicit fixed-suite test-result capture/import; no automatic tests.
- [ ] Implement last-reported/completed/stale/unknown session presentation without fabricated Codex liveness.
- [ ] Finish versioned SSE reconnect/update handling and narrowed watcher lifecycle.
- [ ] Document startup, checkpoint/evidence workflow, local history backup and limitations; update AGENTS at meaningful checkpoint granularity.
- [ ] Add relevant links/boundaries/procedures to docs/README, SDD, TEST-PLAN and STATUS while preserving unrelated changes.
- [ ] Verify DD10–DD14 and cross-stage browser/storage/protocol regressions; record results separately from native acceptance.

## Proposal validation

Agent-observed on 2026-11-08: all 27 Markdown local links (including target anchors) resolved; DD01–DD14 are unique and sequential; the example DB01 criterion exists in the linked F02 spec. New-file Git diff checks reported no whitespace errors; Git emitted its configured LF-to-CRLF conversion notice. The reviewed additions are limited to this feature's spec/plan/tasks. No application tests, builds, server launch, prototype source changes, native changes, publication or implementation completed in this proposal pass.
