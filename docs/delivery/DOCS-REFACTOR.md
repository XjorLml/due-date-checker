# Documentation refactor record

Authority: audit findings, migration mapping, unresolved documentation decisions, and validation for the 2026-10-17 refactor. Current requirements, contracts, design, and status are owned by the documents in the [index](../README.md).

## Discovery and audit

Discovery covered the complete original SDD, PLAN, DESIGN, V1.1/V1.2 records and the subsequently added V1.3 record, learning notes, project files, Core domain/data/migration code, App startup/navigation/pages/view models/dispatcher, and relevant tests. No applicable AGENTS.md was found before the refactor. The initial working tree already contained substantial modified and untracked application/test/documentation work; it was preserved. A concurrent feature task subsequently updated SDD/PLAN/learning notes and added V1.3 phase/execution evidence, then reconciled STATUS/specs. Those updates were read and retained. The archived SDD is the 2026-10-17 version available immediately before extraction, rather than the earlier discovery version.

| Finding / evidence | Treatment |
|---|---|
| SDD sections 1–3, 9–10 mixed requirements, architecture, review history, status, and acceptance; PLAN/README/learning notes repeated status. | PRD, SDD, STATUS, and TEST-PLAN now have explicit ownership; indexes link them. Original records remain historical. |
| At initial discovery SDD/PLAN/README and learning notes said V1.3 UI was unimplemented, while DueEditPage/ViewModel and DeleteDueTests contain it. Concurrent feature work subsequently supplied phase/execution evidence. | Preserve the new V1.3 evidence: 28 Core + 42 App passed and basic phone checks, attributed to that feature task. Keep remaining gates open; the documentation refactor ran no application verification. |
| DESIGN declares broad normative product behavior: optional amount, title cap, completed-last sorting, immediate toggle persistence, automatic timezone, unsaved prompt, Undo, and future capabilities. | Add an explicit scope/authority notice and section annotations; approved v1 remains governed by PRD. Preserve future design content for a separate decision. |
| DESIGN contents links omitted numeric heading prefixes, making 13 anchors invalid. | Correct links to the existing numbered headings; retain visual standards and token JSON unchanged. |
| DESIGN implementation examples use Dart/TypeScript and Flutter/React Native; actual projects use .NET MAUI. | Label examples as illustrative design handoff material, not a framework decision. |
| Original SDD described AppShell as per-window, but MauiProgram registers it transient and App retains one instance for its single window. | Document the actual registration/ownership; no code change. |
| Original SDD review describes older missing DI and permissive UpdateAsync behavior. | Preserve the dated review in an archived SDD; describe current DI/update guards in canonical SDD. |
| Source comments and learning notes cite ADR numbers, but no individual ADR files exist. | Add an ADR index/process and identify missing rationale/legacy numbering; invent no ADRs. |
| Scope, technical mappings, test definitions, recorded passes, and device acceptance lacked a single navigable chain. | PRD retains R01–R08; link SDD mappings, feature scenarios/tasks, TP procedures, and STATUS evidence. |
| Tests and a Release build were sometimes adjacent to broader completion claims. | Separate implementation, automated execution, device evidence, and release acceptance; retain user versus agent attribution and unknown timestamps. |
| PLAN contains old platform/store claims and an expired seven-day schedule; learning notes retain stale “not implemented” statements. | Mark complete bodies historical without erasing them. External claims were not revalidated and are not promoted to current guidance. |
| No existing feature specification or documentation validation tooling was found in the inspected file inventory. | Add the soft-delete spec/plan/tasks; use temporary, dependency-free documentation checks. |

## Migration map

The [pre-refactor SDD snapshot](SDD-2026-10-17.md) retains every original section and its historical statements. Only relative link destinations were rebased and a historical notice added. Original section anchors remain in that snapshot.

| Original source | Current destination / preservation |
|---|---|
| SDD §1 Purpose and scope | [PRD product/scope](../PRD.md#product-and-users); technical purpose in SDD §1; original in snapshot §1. |
| SDD §2 Plan review | Snapshot §2 preserves all findings and their 2026-10-13 context; current discrepancies are in this audit. |
| SDD §3 Requirements/traceability and input rules | [PRD](../PRD.md#approved-requirements) owns unchanged R01–R08 and business rules; [SDD mapping](../SDD.md#requirement-to-component-mapping) links implementation. |
| SDD §4 Architecture | Current [SDD §4](../SDD.md#4-architecture), including boundaries and dispatcher; dependency declarations expanded from project files. |
| SDD §5 Data design | Current [SDD §5](../SDD.md#5-data-design): domain, mutation rules, schema, repository contracts, migrations, constraints and concurrency. |
| SDD §6 UI/navigation | Current [SDD §6](../SDD.md#6-ui-and-navigation) owns mechanics; [PRD workflows](../PRD.md#user-workflows) own outcomes; [feature spec](../../specs/v1.3-soft-delete/spec.md) owns delete scenarios. |
| SDD §7 Lifetimes/startup | Current [SDD §7](../SDD.md#7-dependency-lifetimes-and-startup), with AppShell lifetime clarified from source. |
| SDD §8 Failures/privacy/limits | [PRD recovery](../PRD.md#error-and-recovery-requirements) and [SDD §8](../SDD.md#8-failure-handling-privacy-and-practical-limits); original table retained in snapshot. |
| SDD §9 Delivery status | [STATUS](STATUS.md) with dated/attributed phase evidence; original counts/build/artifact/signing notes retained in snapshot and phase records. |
| SDD §10 Verification/acceptance | [TEST-PLAN](TEST-PLAN.md) procedures, environments, inventory, and gaps; original historical assertions retained in snapshot. |
| SDD §11 Deferred decisions | [PRD exclusions](../PRD.md#exclusions-and-deferred-capabilities), [SDD extension points](../SDD.md#deferred-extension-points), ADR index; learner history remains in original records. |
| SDD §12 References | [SDD references](../SDD.md#implementation-references), explicitly retained without new external validation; original links also retained in snapshot. |
| PLAN, all sections | Retained in place, including re-scope provenance, updated 2026-10-17 status from the concurrent task, older toolchain history, decisions, seven-day schedule, claimed external facts, gaps, tradeoffs, and backlog. New historical notice points to canonical docs. |
| V1.1/V1.2/V1.3 phase records, all sections | Retained in place with notices. No test totals, acceptance checks, attribution, or historical next-stage text rewritten. STATUS links outstanding checks. |
| DESIGN, all sections | Visual content/tokens retained; scope notices and corrected contents links added. Future product proposals remain identifiable. |
| README | Build/test instructions retained; duplicated status replaced by evidence-aware summary and index links. Historical counts/evidence remain in phase records and snapshot. |
| `.vibe-wise` profile/progress/project map | Entire original bodies retained with authority/history notices. Learning preferences and demonstrated-understanding records unchanged. |

## Requirement and contract accounting

All original requirement IDs R01–R08 are defined once in the canonical PRD with their original requirement text and acceptance outcomes. R08 remains optional. [PRD traceability](../PRD.md#requirement-traceability) links every ID to technical mappings and verification; historical copies are labeled evidence, not alternate definitions.

DateOnly, DateTimeOffset, TimeProvider, signed 64-bit centavos, immutable Due records, DeletedAt, numbered transactional migrations, DatabaseDispatcher serialization, repository behavior, and domain/data/App boundaries remain in SDD §§4–7. Schema version remains 1. No application contract or migration was changed.

## Unresolved decisions

| Item | Competing evidence / gap | Required future decision |
|---|---|---|
| Future design behavior | DESIGN proposes optional amounts, title 1–120, completed-last sorting, immediate switch persistence, unsaved-change prompts, Undo, locale-driven currency/timezone; approved v1 rules differ. | Product owner must approve any change through PRD/feature specs before implementation. Current v1 authority is explicit; refactor does not adopt these proposals. |
| Brand/design adoption | DESIGN uses Duewise/Plus Jakarta Sans and token standards; App project/Shell still use Due Date Checker, MauiProgram registers OpenSans, and pages use their existing layout. | Decide adoption scope and acceptance in a UI change; this refactor does not rename the binary, replace assets, or assert design compliance. |
| Future reminder policy | Historical project map proposes 3-days/1-day/due-day plus an overdue follow-up and permission on first due; DESIGN proposes different schedules and opt-in timing. | Reconcile when reminders are approved. Neither proposal is a v1 requirement. |
| Legacy ADR references | Comments cite ADR 001/002; project map lists 001–007 as unwritten proposals; some rationale is missing. | Confirm number assignments, status, and rationale when writing retrospective ADRs; do not infer approval from comments. |
| V1.3 remaining acceptance | The concurrent feature task supplied automated/build/basic phone results and source/artifact hashes in the [execution record](evidence/V1.3-2026-10-17.md). Offline, native edge/failure/accessibility, phone-private tombstone, and intended-key gates remain unchecked. | Complete only the outstanding gates; preserve completed results and their attribution. Documentation inspection does not close device acceptance. |

These decisions do not block the documentation refactor: approved v1 authority is preserved, proposals are labeled, and missing evidence stays pending.

## Files and structure

Created by this refactor: `AGENTS.md`, `docs/README.md`, `docs/PRD.md`, `docs/adr/README.md`, the four files under `docs/delivery/`, and `specs/v1.3-soft-delete/{spec,plan,tasks}.md`.

Modified: root `README.md`, `docs/SDD.md`, `docs/DESIGN.md`, `docs/PLAN.md`, the phase-record authority notices, and `.vibe-wise/{profile,progress,project-map}.md`. No pre-existing file was deleted or moved; the new SDD snapshot was added with rebased links. The concurrent task added `docs/phases/V1.3.md` and `docs/delivery/evidence/V1.3-2026-10-17.md`, and contributed updates to STATUS and feature specs; these are retained and attributed separately.

```text
AGENTS.md
README.md
docs/
  README.md
  PRD.md
  SDD.md
  DESIGN.md
  PLAN.md                         (historical)
  adr/
    README.md
  delivery/
    STATUS.md
    TEST-PLAN.md
    DOCS-REFACTOR.md
    SDD-2026-10-17.md             (historical snapshot)
    evidence/
      V1.3-2026-10-17.md         (feature-task execution evidence)
  phases/
    V1.1.md                     (historical evidence)
    V1.2.md                     (historical evidence)
    V1.3.md                     (feature-task evidence)
specs/
  v1.3-soft-delete/
    spec.md
    plan.md
    tasks.md
.vibe-wise/                      (retained learning records)
```

The existing branding image and all other non-documentation files remain in place.

## Validation results

Validation is pending final local-link, preservation, requirement, and file-boundary checks. Concurrent updates to historical documentation are checked against their reconciled content separately from the initial file-hash baseline; application files remain compared to the initial baseline. No application tests, builds, device checks, or external-policy validation are part of this documentation refactor.

## Next work

Complete the current implementation's [verification and acceptance tasks](../../specs/v1.3-soft-delete/tasks.md), intended-key signing, and outstanding phone checks listed in STATUS. Resolve future design/ADR questions only when their corresponding scope is taken up; do not expand v1 to clear a documentation conflict.
