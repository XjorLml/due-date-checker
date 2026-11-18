# Developer dashboard task tracking

Status: approved 2026-11-10 and implemented the same day; see [open items](#open-after-implementation) and [evidence](#implementation-evidence--2026-11-10). [Scenarios](spec.md#observable-acceptance-scenarios); [analysis/plan](plan.md).

## Proposal evidence — 2026-11-08

Agent-observed source/document inspection in the Windows PowerShell workspace. Initial branch develop / HEAD 7adbe52; dirty working tree, with concurrent changes observed during the pass. Node v24.16.0 and Python 3.14.6 were reported by local commands. These observations do not establish a prototype runtime pass, native test pass or device acceptance.

- [x] Read docs/README and applicable AGENTS workflow.
- [x] Inspect relevant PRD/SDD/DESIGN authority/scope, delivery focus/test gates and existing soft-delete spec structure.
- [x] Inspect prototype shell, renderer/navigation, dirty guard, dialogs, shared dependencies and existing test harness.
- [x] Inspect native Shell, relevant view models, test definitions/project boundaries and existing CI definition.
- [x] Prepare scoped acceptance scenarios, architecture, registry/evidence contract, navigation protocol, staged work and limitations.
- [x] Validate proposal local links, scenario traceability and scoped diff; record outcome below.

## Review evidence — 2026-11-09

Agent-observed by Claude Code on the second computer (`develop` at `2cc92c1`, clean tree; Node v22.12.0, npm 10.9.0, no Python on PATH). The pass re-read this spec, plan and tasks, the prototype's `app.js` navigation/hash/render code and the `index.html` dialogs. It confirmed the prototype's navigation code is unchanged since the proposal. Local commands counted 29 spec folders with `tasks.md` (395 checked and 100 open tasks), 306 scenario-ID rows and 7,676 words in STATUS. Nothing was run or built. The [proposed revision R1–R6](plan.md#review-and-proposed-revision--2026-11-09) awaits the same approval as the original plan.

- [x] Receive architecture approval required by the invoked prompt: the user replied "these plan is approved implement now" on 2026-11-10. The recommended options were applied: R1–R6, a shared `.claude/settings.json` for hooks, and committed records.

## Stage 1 — local overview and preview

- [x] Register approved requirements and feature scenarios: derived from PRD rows and spec tables (R1). The mapping gives all 37 features a phase, a scope, and a prototype screen or an unavailable reason.
- [x] Loopback server, safe asset/document access, schema validation and persistent local state.
- [x] Compact overview, searchable feature/criteria/evidence views and deterministic progress.
- [x] Embed the existing prototype, with viewport, full-screen and new-tab controls, and dependency-aware refresh with dirty/modal protection.
- [x] Validate and record outcomes (see evidence below).

## Stage 2 — connected review

- [x] Opt-in prototype adapter and a validated, request-correlated handshake/navigation/state protocol.
- [x] Map root/pushed screens and overlays using inspected IDs; handle unavailable samples, guarded cancellation and shared-screen features.
- [x] Persist prototype-source-scoped visual reviews separately from functional verification.
- [x] Verify DD06–DD09 and the affected prototype regressions.

## Stage 3 — reporting and verification integration

- [x] Checkpoint/session CLI, history, serialized (locked) appends and torn-line isolation.
- [x] Read-only Git metadata and explicit fixed-suite test-result capture; no automatic tests.
- [x] Last-reported, ended and stale session presentation, without fabricated liveness. Claude Code hooks are configured.
- [x] Versioned SSE updates with reconnect, and narrowed watchers.
- [x] Document startup, the checkpoint/evidence workflow and limits ([tool guide](../../tools/developer-dashboard/README.md)); update AGENTS ("Progress checks").
- [x] Add links and boundaries to docs/README, FEATURES, SDD §3, TEST-PLAN and STATUS while preserving unrelated changes.
- [x] Verify DD10–DD14 and cross-stage regressions, recorded separately from native acceptance.

## Open after implementation

- [ ] DD02: frozen, versioned scope baselines with added/removed-gate display and a selectable milestone view. Deferred by R2 and not built; adding a scenario to a spec changes the denominator immediately.
- [ ] DD03: per-boundary results (automated, emulator, device, acceptance) for each gate. Records currently carry one result plus environment text.
- [ ] DD05: a browser check for the deferred reload while a sample draft is dirty. The logic exists, but only the server-side preview event and the no-reload-on-data-change path are exercised.
- [ ] DD08: automated checks for unsupported targets and stale acknowledgements. Both are implemented in the bridge and page; forged-origin and wrong-nonce messages are tested.
- [ ] DD12 and DD14: assert the tests-panel "last pass" display and visible keyboard focus in the browser check. Both are implemented, and `latestTests` is unit-tested.
- [ ] Manual: the Full screen button; headless Edge refuses full screen, and the page now reports that instead of throwing.
- [ ] User: open `/hooks` once or restart Claude Code, then confirm a session signal appears.
- [ ] User: confirm Codex `notify` behavior before relying on it (see the tool guide).
- [ ] User acceptance: is the status understandable in about 10 seconds?

## Implementation evidence — 2026-11-10

**Environment:**
- Agent-observed by Claude Code on Windows 11 with Node v22.12.0 and Microsoft Edge (headless) for the browser checks.
- Source: `develop` at `2cc92c1`, with uncommitted working-tree changes. These include the earlier starter-kit work and this implementation.
- Test runs were made through `cli.mjs test` and are recorded in the machine-local `.developer-dashboard/test-runs.jsonl`.

| Check | Observed result |
|---|---|
| `node --test tools/developer-dashboard/tests/*.test.mjs` | 31 of 31 passed: Markdown parsing, status precedence, staleness, summaries, sessions, the last-pass display, record locking across 8 processes, torn lines, validators, CLI commands and hooks, and HTTP security/review/SSE tests on temporary fixtures |
| `tests/browser-check.mjs` (Edge headless) | 13 steps passed with no runtime, console or content-policy errors. Full screen was not entered in headless Edge (reported, not counted) |
| Prototype model tests | 25 of 25 passed after the bridge was added |
| Prototype browser flows (`browser-check.cjs`) | All five flow groups passed after the bridge and one-line `app.js` change |
| `cli.mjs check` | PASS: 37 features, 294 gates, 0 data problems |
| Route probes against the real repository | Page, assets, snapshot and prototype returned 200. Prototype tests, traversal, `.git`, a signing file and non-Markdown documents returned 404. A forged Host header and a tokenless POST returned 403 |
| Hook pipe test | Run from another directory: exit 0, no output, transcript path not stored. The test line was then removed from the local journal |

**Defects found and fixed during verification:**
1. Typing in the prototype did not report an unsaved draft. The bridge now also listens for `input` and `change`.
2. CSS `display: flex` overrode `[hidden]`, which left stale "relates to" chips and an empty banner visible. A global `[hidden]` rule fixed it, and the browser check now asserts both stay hidden.
3. A refused full-screen request threw an unhandled rejection. It is now caught and explained.

**Seeded data:**
- R01–R07 carry manual In Review statuses with a source link to [STATUS release slices](../../docs/delivery/STATUS.md#release-slices).
- Three historical verification records (R08, plus v1.4-ci CI01 and CI04) point to the 2026-11-06 [CI evidence](../../docs/delivery/evidence/EMULATOR-HARDENING-2026-11-06.md#optional-ci-r08).

**Scenario coverage:**
- **Fully exercised, with verification records:** DD01, DD06, DD07, DD09, DD10, DD11 and DD13. These records use `--commit none` because the code is not committed yet; re-verify after committing so staleness can be tracked.
- **Implemented, coverage partial:** DD04, DD05, DD08, DD12 and DD14 (see the open items above).
- **Not built:** DD02's frozen baselines and DD03's per-boundary split.

No native source, build, Android test, device or release check was involved or claimed.

## Proposal validation

Agent-observed on 2026-11-08: all 27 Markdown local links (including target anchors) resolved; DD01–DD14 are unique and sequential; the example DB01 criterion exists in the linked F02 spec. New-file Git diff checks reported no whitespace errors; Git emitted its configured LF-to-CRLF conversion notice. The reviewed additions are limited to this feature's spec/plan/tasks. No application tests, builds, server launch, prototype source changes, native changes, publication or implementation completed in this proposal pass.
