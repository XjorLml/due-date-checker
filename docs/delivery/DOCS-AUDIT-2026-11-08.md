# Documentation and toolkit consolidation — 2026-11-08

Authority: findings, dispositions, migration map and observed verification for this audit. [PRD](../PRD.md), [SDD](../SDD.md), [DESIGN](../DESIGN.md), [STATUS](STATUS.md) and [TEST-PLAN](TEST-PLAN.md) retain their existing ownership. [Authorized scope/DTK scenarios](../../specs/docs-toolkit/spec.md), [plan](../../specs/docs-toolkit/plan.md), [tasks](../../specs/docs-toolkit/tasks.md).

## Inspected scope and attribution

Agent-observed on 2026-11-08, Windows/PowerShell, Python 3.14.6. Read repository instructions, documentation index, relevant authorities/specs, the reusable template tree, native project/Shell/domain and applicable test definitions. The actual reusable toolkit is docs/templates/maui-starter, with documentation and a proposed application layout. Developer-dashboard files are a separate proposal; no implementation approval was recorded by this task. Executable native/release utilities are cataloged with their existing preconditions, not redesigned or executed.

Initial local-link baseline with the new checker: **171 Markdown files, 1,935 local link occurrences, one broken target**; 57 external URL occurrences and 31 declared JavaScript route occurrences were not executed. This baseline includes the newly drafted audit spec/plan/tasks; it is not a count of only pre-task tracked files. The initial working tree already modified STATUS and contained untracked installation-conflict evidence and the dashboard proposal. Those existing contents are preserved. A task-local ignored preservation manifest records 227 source/contract/status/history/evidence hashes and 110 prior index link occurrences for comparison; it contains no signing values or user data.

## Findings and disposition

| Priority / ID | Finding and source | Disposition |
|---|---|---|
| High A01 | Historical kit REVIEW linked a signing example missing from the current checkout; root README told users to copy it. App's conditional import still supports an ignored local config. | Remove the dead link while preserving the historical filename/finding; replace obsolete copy instructions with [local setup](../SETUP.md#local-signing-and-artifacts) and existing release procedure. Do not recreate credentials or change signing. |
| High A02 | docs index said all round-2 UX findings were undecided, contrary to the existing round-2/alignment specs. F02/F04/F07 and roadmap text combined old screen/amount/export/recurrence deferrals with later approvals. | Correct feature-navigation summaries; add explicit historical/supersession notes linking the already approved owning scopes. Preserve every DB/PF/CT/PA scenario ID and original evidence. No product decision was made here. |
| Medium A03 | Root README repeated a dated feature summary and build/test procedures. docs index mixed ownership with 24 feature-navigation rows. | Make root README a task menu. Keep ownership in docs/README and move all feature destinations to [FEATURES](../FEATURES.md). TEST-PLAN remains the test/build procedure owner; STATUS owns delivery state. |
| Medium A04 | Toolkit's first action was reading a long dated review; copied setup/automation recovery was mostly generic guidance, and no checker shipped. | Add short copy → check → customize → readiness flow, clear expected outcomes, actionable recovery and a reusable standard-library checker with behavior tests. Keep app scaffolding, versions and acceptance unresolved until actually chosen/performed. |
| Medium A05 | Project-native/release helpers were easy to mistake for general status tooling; dashboard proposal lacked a clear navigation entry. | Add [tools guide](../../tools/README.md), task/prerequisite table and explicit proposal-only dashboard entry. Keep device and fixture guards in their existing guide. |
| Medium A06 | Old refactor/review and roadmap passages can look current despite describing earlier artifacts. | Add dated pointers and historical framing rather than deleting evidence or replacing old counts with newer totals. |
| Remaining A07 | No pinned SDK, current native acceptance, hosted verification and legacy ADR rationale cannot be established by a doc cleanup. | Explain setup/evidence limits; continue using existing STATUS/ADR authorities. No new pin, approval, ADR identity, native pass or hosted result is invented. |

## Migration and preservation map

| Before | Current destination / treatment | Preservation |
|---|---|---|
| Root README feature/status list | PRD/STATUS through task links | Duplicate summary removed; owning requirements/results remain unchanged |
| Root README test/build commands | TEST-PLAN automated execution, reached through SETUP | Procedure remains in its existing authority; signing limits now explicit |
| docs/README feature rows | docs/FEATURES | 24 rows moved, selected descriptions corrected; all original index destinations must remain reachable in the index/catalog union |
| Document ownership mixed with feature catalog | docs/README authority table and start/task routes | Existing ownership/history anchors remain; feature catalog is navigation only |
| New-checkout and tool-discovery guidance | docs/SETUP and tools/README | New navigation/procedure entry points; no native contract copied/redefined |
| Starter kit's long-first-read workflow | Kit README short start/customization order | Existing contents/ownership/adoption headings and template links retained |
| Disposable historical link check | Copyable project/tools/check_docs.py, tests and thin repository tools/check_docs.py | One implementation source, usable independently after copying; no external dependency |
| Missing signing-example link in historical REVIEW | Historical path as code plus a dated follow-up | Original finding and evidence date retained; absent file is not restored |
| F02/F04/F07/alignment/roadmap older scope wording | Scoped supersession notes pointing to current PRD/feature contracts | Original scenarios/milestones and their evidence retained |
| October refactor audit and prior template review | Remain in place, linked to this follow-up | Original counts and attribution remain historical, not rerun claims |
| Existing STATUS change, execution evidence, PRD/SDD/DESIGN, native/prototype source | Remain in place | Hash comparison required; no native/product scope or acceptance change |

## Checker boundary

The checker handles conventional inline/reference Markdown local links, file existence, Markdown headings (including duplicate/setext headings) and quoted HTML ids/names. Fenced examples/comments/inline code are excluded. Exact known app-route fragments are declared from inspected prototype routers in tools/docs-routes.json, counted separately and never reported as browser passes. External URLs are not fetched. Raw HTML hrefs, arbitrary embedded syntax, semantic contract consistency, visual rendering and runtime behavior still need appropriate review.

Draft mode allows template fields. Readiness mode rejects unresolved named placeholders, including executable examples; the explanatory notation `{{...}}` is not a field. Neither mode grants approval or verifies an application. Exit/error meanings and recovery live in the tools guide.

## Verification results

Agent-observed, same date/environment above; scope is this task's Markdown/Python/route-manifest changes against the ignored preservation baseline. DTK01–DTK07 were checked at documentation/tool boundaries.

| Check | Observed result |
|---|---|
| Full repository local links | PASS: 175 Markdown files; 2,012 local link occurrences; zero errors. 57 external URL occurrences and 32 declared application-route occurrences explicitly not executed. |
| Checker behavior | Nine tests passed: missing paths/anchors/references, fences/comments/code, duplicate/setext/HTML anchors, encoded paths, placeholder readiness, root containment, route allowlisting, CLI failure/recovery and invalid configuration. |
| Full portable-copy check | Copied project tree into an isolated temporary directory, invoked its checker from outside that directory: 19 Markdown files / 95 local links passed, no source-repository dependency. The copied behavior tests also passed. |
| Readiness semantics | Full uncustomized copy correctly returned exit 1 for unresolved fields. A customized minimal copy passed --ready in the CLI recovery test. Draft mode does not imply product readiness. |
| Navigation preservation | All destinations from the prior index's 110 link occurrences remain in the docs index/catalog union; 24 feature rows migrated. |
| Source/evidence preservation | All 227 recorded source/contract/status/history/evidence hashes unchanged, including pre-existing STATUS edits and installation evidence. DB/PF/CT/PA scenario ID sequences preserved. |
| Diff/format | Tracked diff check passed; task files reviewed for scope, whitespace, conflict markers and final newlines. Git's configured LF-to-CRLF notices are not application results. |
| Remaining verification | External URLs, runtime routes, visual rendering and semantics outside the inspected scopes require separate review. No .NET restore/test/build, native helper, device action, hosted release or dashboard implementation was performed. |

## Unresolved decisions and remaining acceptance

- Legacy ADR numbers/rationale remain owned by the [ADR index](../adr/README.md); this task does not reconstruct unwritten decisions.
- SDK pinning, compatible native tools and CI/hosted verification require their own authorized toolchain/delivery evidence, not a cleanup-driven version choice.
- Native test execution blocks, intended signing identity and device/accessibility acceptance remain as attributed in STATUS. They are not failures or passes of the new documentation tool.
- The developer dashboard remains proposed. No live Codex state, generated progress percentage or runnable dashboard is supplied.
- A new project still needs product decisions, compatible tool selection, actual generated source, observed builds/tests and native acceptance. Its copied template is expected to fail readiness until customized.
