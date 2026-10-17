# Architecture decision records

Authority: the ADR process and index of recorded decision rationale. Current technical contracts remain in the [SDD](../SDD.md); product constraints remain in the [PRD](../PRD.md).

## Current record

No individual ADR files were present at the 2026-10-17 documentation audit. Source comments and the [historical project map](../../.vibe-wise/project-map.md#adrs-learner-proposed-none-yet-written-to-the-repo) reference proposed ADR numbers 001–007. Those references do not establish that approved ADRs exist.

Supported decision history remains in [PLAN decisions](../PLAN.md#decisions-made-2026-10-06): raw SQL was chosen to understand and structure each query; Visual Studio was chosen for its integrated toolchain. The solution-layout rationale and reason for the MVVM toolkit switch were not recorded. Do not fill those gaps with invented rationale.

## Recording a decision

Create an ADR for a significant architecture choice or change, using `NNN-short-title.md`. Check the historical number references before assigning an ID; reconcile them explicitly rather than silently reusing them.

Include title, status (proposed/accepted/superseded), decision date if known, context, decision, supported rationale and alternatives, consequences, requirement/spec links, and evidence of approval. Unknown information stays unknown. Mark retrospectively reconstructed records as retrospective and cite their sources.

Add a link here when a record exists. When superseding a decision, link both directions and update the SDD. An ADR preserves why a decision was made; it must not become a competing definition of the current contract.
