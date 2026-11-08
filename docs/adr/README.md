# Architecture decision records

Authority: the ADR process and index of recorded decision rationale. Current technical contracts remain in the [SDD](../SDD.md); product constraints remain in the [PRD](../PRD.md).

## Current record

No individual ADR files were present at the 2026-10-17 documentation audit. Source comments and the [historical project map](../../.vibe-wise/project-map.md#adrs-learner-proposed-none-yet-written-to-the-repo) reference proposed ADR numbers 001–007. Those references do not establish that approved ADRs exist.

Supported decision history remains in [PLAN decisions](../PLAN.md#decisions-made-2026-10-06): raw SQL was chosen to understand and structure each query; Visual Studio was chosen for its integrated toolchain. The solution-layout rationale and reason for the MVVM toolkit switch were not recorded. Do not fill those gaps with invented rationale.

On 2026-10-18, [007: Local reminder reconciliation](007-local-reminder-reconciliation.md) records the F05 implementation choice and explicitly reconciles the historical proposed 007 identity. Other legacy proposals remain unwritten; this record does not establish their approval.

On 2026-11-05, [008: Focus design baseline and native Add action](008-focus-design-baseline.md) records the DESIGN 2.0.0 baseline change, the five-slot Shell bar with a renderer-drawn Add disc, and the alternatives rejected. The number is new; no historical proposal reserved it.

On 2026-11-06, [009: Recurring dues as linked occurrences](009-recurring-dues.md) records the UX03 storage and consistency choices (rule on each row, anchor day, source link with a duplicate guard, one transaction). The user decided the product rules; the technical choices are marked as not separately approved. The number is new.

On 2026-11-06, [010: Export and import as a user-chosen, versioned JSON file](010-export-import-file.md) records the UX05 choices (system file dialogs, format `duewise.dues` version 1 with strict reading, one-transaction import that may only replace deleted rows, dues only). The user decided the product rules; the technical choices are marked as not separately approved. The number is new.

## Recording a decision

Create an ADR for a significant architecture choice or change, using `NNN-short-title.md`. Check the historical number references before assigning an ID; reconcile them explicitly rather than silently reusing them.

Include title, status (proposed/accepted/superseded), decision date if known, context, decision, supported rationale and alternatives, consequences, requirement/spec links, and evidence of approval. Unknown information stays unknown. Mark retrospectively reconstructed records as retrospective and cite their sources.

Add a link here when a record exists. When superseding a decision, link both directions and update the SDD. An ADR preserves why a decision was made; it must not become a competing definition of the current contract.
