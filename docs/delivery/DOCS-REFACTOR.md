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
| Brand/design adoption | DESIGN uses Duewise/Plus Jakarta Sans and token standards; App project/Shell still use Due Date Checker, MauiProgram registers OpenSans, and pages use their existing layout. The name is decided: Duewise (user decision, 2026-10-17). The unreferenced branding image's "Duely" name is superseded, and its palette (primary `#4F6BFF`) is non-normative inspiration. | Decide app/project renaming and font/token adoption scope and acceptance in a UI change; this refactor does not rename the binary, replace assets, or assert design compliance. |
| Future reminder policy | Historical project map proposes 3-days/1-day/due-day plus an overdue follow-up and permission on first due; DESIGN proposes different schedules and opt-in timing. | Reconcile when reminders are approved. Neither proposal is a v1 requirement. |
| Legacy ADR references | Comments cite ADR 001/002; project map lists 001–007 as unwritten proposals; some rationale is missing. | Confirm number assignments, status, and rationale when writing retrospective ADRs; do not infer approval from comments. |
| V1.3 remaining acceptance | The concurrent feature task supplied automated/build/basic phone results and source/artifact hashes in the [execution record](evidence/V1.3-2026-10-17.md). Offline, native edge/failure/accessibility, phone-private tombstone, and intended-key gates remain unchecked. | Complete only the outstanding gates; preserve completed results and their attribution. Documentation inspection does not close device acceptance. |

These decisions do not block the documentation refactor: approved v1 authority is preserved, proposals are labeled, and missing evidence stays pending.

## Files and structure

Created by this refactor: `AGENTS.md`, `docs/README.md`, `docs/PRD.md`, `docs/adr/README.md`, the four files under `docs/delivery/`, and `specs/v1.3-soft-delete/{spec,plan,tasks}.md`.

Modified: root `README.md`, `docs/SDD.md`, `docs/DESIGN.md`, `docs/PLAN.md`, the phase-record authority notices, and `.vibe-wise/{profile,progress,project-map}.md`. No pre-existing file was deleted or moved; the new SDD snapshot was added with rebased links. The concurrent task added `docs/phases/V1.3.md` and `docs/delivery/evidence/V1.3-2026-10-17.md`, and contributed updates to STATUS and feature specs; these are retained and attributed separately.

The 2026-10-17 validation follow-up modified root `README.md`, `docs/README.md`, `docs/SDD.md`, `docs/delivery/STATUS.md`, this record, `specs/v1.3-soft-delete/tasks.md`, and the V1.3 execution record (an appended section only). A same-day alignment edit then labeled the branding image as non-normative and recorded the Duewise name decision in `docs/DESIGN.md`, `docs/README.md`, root `README.md`, and this record.

```text
AGENTS.md
README.md
docs/
  README.md
  PRD.md
  SDD.md
  DESIGN.md
  Duely App Branding Showcase - chatgpt.png   (design inspiration; not normative)
  PLAN.md                         (historical)
  docs-revamp.md                  (refactor task brief; not authority)
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

The original run stopped before this step. Validation was completed on 2026-10-17 by a follow-up agent session (agent-observed). By then the refactor output, the pre-existing application work, and the V1.3 feature changes had been committed together in `59cf578` and `7c22b4b`. The follow-up therefore validated the clean `7c22b4b` tree plus its own Markdown edits; the original initial-working-tree hash baseline was not available to it.

| Check | Result |
|---|---|
| Required documents | All target files exist. Additional delivery files (this record, the SDD snapshot, `evidence/`) are justified above; PLAN and phase records stay in place as history. The [task brief](../docs-revamp.md) remains in `docs/` as an instruction record, linked from the index and not treated as authority. |
| Local links and anchors | 276 local links in 23 tracked Markdown files resolve to existing files and GitHub-style heading anchors. A temporary dependency-free Node script outside the repository was used; it was self-tested against deliberately broken file and anchor links. External URLs were not checked. |
| Requirement accounting | R01–R08 are defined once in the [PRD](../PRD.md#approved-requirements). Requirement, slice, and acceptance text is identical to [snapshot §3](SDD-2026-10-17.md#3-requirements-and-traceability); the component column moved to the [SDD mapping](../SDD.md#requirement-to-component-mapping). Other ID tables are mappings, test coverage, or the labeled snapshot. Every ID appears in PRD traceability. |
| Technical contracts | The nine preserved decisions (DateOnly, DateTimeOffset, TimeProvider, signed 64-bit centavos, immutable Due, DeletedAt, numbered migrations, DatabaseDispatcher, domain/data/App separation) are in SDD §§4–7. Every original SDD section has a destination in the migration map. |
| Source-claim spot check | Matched source: DI lifetimes including transient AppShell; declared package versions, `$(MauiVersion)`, no global.json; Money rounding and `₱`/`N2` formatting; SemaphoreSlim dispatcher with post-success initialization flag; `ORDER BY due_date, created_at`; active-row guards on update and soft delete; schema defaults and partial index without CHECK constraints; initializer rejection of negative/newer versions; `allowBackup="false"` with template network permissions; OpenSans font registration; `dd MMM yyyy`, `yyyy-MM-dd`, "Amount (PHP)", and the unavailable-due message; no CI workflow. |
| Deferred scope | PRD exclusions, SDD extension points, the DESIGN scope notice, and STATUS limits consistently keep reminders, notifications, recurrence, authentication, sync, budgets, savings, export, iOS, and Play distribution deferred. |
| Competing authority | Each canonical document has an authority statement; historical files carry notices pointing to it. The follow-up merged a duplicated V1.3 entry in the index. |
| Stale claims | "Uncommitted working tree" / "source is not committed" statements in STATUS, SDD, and README were updated to the committed state. The execution record's original statement is retained, with an appended [source-commit check](evidence/V1.3-2026-10-17.md#post-record-source-commit-check): all eight recorded hashes match the committed files (four after line-ending normalization). The tasks scenario range now matches the spec's SD09. |
| File boundary | The follow-up changed only Markdown files listed under [Files and structure](#files-and-structure). Commit `7c22b4b` also contains non-documentation files: colors/styles and SqliteDueRepositoryTests (matching V1.3 recorded hashes), App/AppShell wiring (described by the [V1.1 record](../phases/V1.1.md), dated 2026-10-16), and an unrelated `.vscode/launch.json`. The refactor record states it changed no application files; that cannot be independently re-proven from the combined commit. |
| Not performed | No application tests, builds, device checks, or external-policy checks. None are required for documentation-only changes, and none of the evidence above is runtime verification. |

## Next work

Complete the current implementation's [verification and acceptance tasks](../../specs/v1.3-soft-delete/tasks.md), intended-key signing, and outstanding phone checks listed in STATUS. Resolve future design/ADR questions only when their corresponding scope is taken up; do not expand v1 to clear a documentation conflict.
