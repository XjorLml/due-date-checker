# Duewise documentation

This index owns documentation navigation and authority. Read the documents relevant to the task; historical records are evidence, not current specifications.

## Document ownership

| Document | Owns | Read when |
|---|---|---|
| [PRD](PRD.md) | Approved product requirements, business rules, acceptance outcomes, scope | Defining or changing user-visible behavior |
| [SDD](SDD.md) | Technical contracts, architecture, data, startup, concurrency, implementation constraints | Implementing or reviewing code |
| [DESIGN](DESIGN.md) | UI/UX standards, tokens, appearance, interaction, accessibility | Changing the interface; apply its v1 scope notice |
| [Delivery status](delivery/STATUS.md) | Current implementation state, evidence attribution, remaining acceptance | Resuming delivery or reporting completion |
| [Test plan](delivery/TEST-PLAN.md) | Verification procedures, environments, expected outcomes, acceptance gates | Planning or performing verification |
| [Feature specifications](../specs/v1.3-soft-delete/spec.md) | Feature scenarios, approach, tasks, links to governing contracts | Working on an independently implementable change |
| [ADR index](adr/README.md) | Recorded architecture decision rationale and ADR process | Making or revisiting a significant architecture decision |
| [Agent instructions](../AGENTS.md) | Development workflow and documentation maintenance rules | Working as a coding agent |

The PRD owns approved behavior; source inspection establishes implemented behavior. Tests establish only the behavior exercised by their recorded execution. None silently overrides another: record discrepancies and resolve scope or contract changes explicitly. DESIGN owns the UI system but cannot expand approved product scope. Feature specs refine requirements through linked scenarios rather than becoming a second product definition.

## Task routes

- Product change: PRD → relevant feature spec → SDD or DESIGN as needed → test plan.
- Data or repository fix: relevant PRD IDs → SDD data contracts → affected source/tests.
- UI work: PRD → DESIGN scope notice and relevant components → SDD navigation → feature spec.
- Release check: delivery status → test plan → referenced evidence records.
- Documentation-only change: relevant canonical documents → local links and traceability validation; application builds are unnecessary unless another change requires them.

## History and migration

- [Documentation refactor audit and migration map](delivery/DOCS-REFACTOR.md): findings, unresolved decisions, preservation map, and validation.
- [Historical plan](PLAN.md): approved 2026-10-10 re-scope provenance, original schedule, toolchain history, decisions, and deferred backlog.
- [V1.1 record](phases/V1.1.md), [V1.2 record](phases/V1.2.md), and [V1.3 record](phases/V1.3.md): dated implementation/test evidence and attributed phone acceptance. [V1.3 execution evidence](delivery/evidence/V1.3-2026-10-17.md) identifies the environment, artifact, and source hashes.
- [V1.3 record](phases/V1.3.md) and [execution evidence](delivery/evidence/V1.3-2026-10-17.md): 2026-10-17 soft-delete automated/build/native checks, artifact/source identity, and explicit remaining gates. Current feature tracking is in the linked spec/tasks and delivery status.
- [Pre-refactor SDD snapshot](delivery/SDD-2026-10-17.md): original review, requirements, contracts, and evidence before responsibility separation.
- [Learning profile](../.vibe-wise/profile.md), [progress](../.vibe-wise/progress.md), and [project map](../.vibe-wise/project-map.md): learning preferences and historical context; not current product or delivery authority.

Retain stable requirement IDs and historical evidence. When a section moves, update inbound links and the migration map. Do not delete historical documents merely because newer canonical documents supersede them.
