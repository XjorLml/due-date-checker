# Documentation consolidation and toolkit usability

Authorized: user's audit/consolidation and existing-toolkit improvement request, 2026-11-08. This is documentation and developer tooling; no native product requirement or dashboard implementation is added. [Plan](plan.md); [tasks](tasks.md).

Authorities: [AGENTS](../../AGENTS.md), [documentation ownership](../../docs/README.md), [PRD R01–R08](../../docs/PRD.md#approved-requirements), [SDD boundaries](../../docs/SDD.md#3-source-boundaries), [DESIGN scope](../../docs/DESIGN.md#scope-and-authority) and [verification boundaries](../../docs/delivery/TEST-PLAN.md#environments-and-evidence).

## Scope and acceptance

| ID | Observable outcome | Verification |
|---|---|---|
| DTK01 | Root README gives one clear route to local setup, current evidence, tools and the reusable kit without duplicating requirements or test procedures. | Link/ownership review |
| DTK02 | Documentation index separates authority from the feature catalog; all former feature destinations remain discoverable. | Migration map and link check |
| DTK03 | Outdated summaries are corrected or explicitly scoped as historical; stable IDs, decision rationale, original evidence and attribution remain. Unresolved issues are recorded. | Source/spec comparison and diff |
| DTK04 | Starter kit has a short first-use path, adoption order, placeholder/ready-state checks, expected outcomes and troubleshooting. It remains a documentation kit, with application generation/build/acceptance explicitly pending. | Template review; isolated copy check |
| DTK05 | Reusable dependency-free checker validates local Markdown file links, heading/HTML anchors and references, with actionable file/line errors and nonzero failure exit. Fenced examples and external URLs are distinguished from checked links; unexecuted runtime routes are reported. | Synthetic unit/integration fixtures |
| DTK06 | A beginner can run the repository checker with one command and the copied kit checker independently. Missing inputs, unresolved template placeholders and invalid links have clear recovery instructions. | CLI invocations and isolated copy |
| DTK07 | Dated audit lists prioritized findings, disposition, migration/preservation map and actual verification, without inheriting native passes. | Audit/task evidence review |

## Limits

Do not publish, execute device-mutating utilities, inspect private data/signing inputs, redesign the prototype, or implement the proposed developer dashboard. Do not delete historical documents to eliminate contradictory old statements. Native acceptance and test execution restrictions remain unchanged. External URLs and current platform-support policies are not part of the local checker and are not claimed revalidated.
