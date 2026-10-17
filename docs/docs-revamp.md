# Task: Refactor Duewise Documentation for Spec-Driven Development

## Objective

Refactor this repository’s documentation into a maintainable, traceable system that supports human developers and AI coding agents.

Perform the documentation changes, then validate them. This is a documentation-only task: preserve approved scope, technical decisions, historical evidence, and existing application behavior.

Act as a software architect and technical documentation maintainer familiar with .NET MAUI, MVVM, SQLite, and spec-driven development. Prioritize accuracy and clear ownership over document volume.

## 1. Scope and Guardrails

You may create, edit, or reorganize Markdown documentation and documentation navigation, including `README.md` and `AGENTS.md`.

Do not:
- Modify application code, tests, schemas, migrations, dependencies, or build configuration.
- Change approved requirements or redesign technical contracts.
- Implement features described in the documentation.
- Invent requirements, rationale, implementation details, or verification results.
- Discard historical information or overwrite unrelated working-tree changes.
- Introduce new tooling or dependencies solely for this refactor.

Read applicable repository instructions before editing. Preserve existing agent instructions when updating `AGENTS.md`.

Proceed independently with reversible documentation decisions. If evidence cannot resolve a substantive conflict, record the conflict and the decision needed, then continue unaffected work. Do not silently select an interpretation.

## 2. Project Context to Verify

Duewise is described as an offline-first Android due-date tracker using:
- .NET 10 and .NET MAUI.
- C#, MVVM, and CommunityToolkit.Mvvm.
- SQLite in app-private storage.
- `Due`, `Money`, and `IDueRepository`.
- Asia/Manila product timezone and PHP currency.

Approved v1 scope includes creating, listing, editing, marking paid/unpaid, and soft-deleting due items.

Reminders, notifications, recurring dues, authentication, synchronization, and other advanced features are deferred.

Treat these statements as discovery inputs. Verify them against repository evidence and flag discrepancies. Product timezone is independent of the development machine’s timezone.

Preserve the distinction between:
- Approved behavior.
- Implemented behavior.
- Historically reported verification.
- Verification performed during this task.
- Proposed or deferred behavior.

Source code establishes what exists; it does not automatically redefine approved requirements. Tests establish only the behavior they cover.

## 3. Discovery and Audit

Before editing:

1. Inspect repository structure, applicable agent instructions, and working-tree changes.
2. Locate and read the existing Software Design Document completely.
3. Read `PLAN.md`, relevant phase records, `DESIGN.md`, existing specifications, and documentation indexes.
4. Inspect relevant project files, domain models, repository interfaces and implementations, database initialization and migrations, and tests.
5. Identify authoritative documents, historical records, duplicated definitions, and inbound links to sections that may move.

Use targeted source inspection. Expand it when a claim or conflict requires more evidence.

Produce a concise audit covering:
- Duplicate or conflicting definitions.
- Outdated or unsupported implementation claims.
- Mixed product, architecture, design, and delivery responsibilities.
- Missing traceability.
- Broken links or anchors.
- Opportunities to reduce unnecessary agent context loading.

For each material finding, identify its source and proposed treatment. Present the audit before editing; an approval checkpoint is not required.

## 4. Target Documentation Structure

Use this structure unless an existing equivalent is established and should be retained:

docs/
  README.md
  PRD.md
  SDD.md
  DESIGN.md
  adr/
    README.md
  delivery/
    STATUS.md
    TEST-PLAN.md

specs/
  v1.3-soft-delete/
    spec.md
    plan.md
    tasks.md

AGENTS.md
README.md

Adapt paths to established repository conventions when necessary and explain deviations. Do not create competing specifications.

Give each document a brief purpose and authority statement. Use relative links for supporting detail rather than copying authoritative definitions.

### docs/README.md — Documentation index

Explain:
- Which document owns each kind of information.
- Where to start for common development tasks.
- How requirements, feature specs, technical contracts, and verification relate.
- Where historical plans and phase records remain available.

### docs/PRD.md — Product requirements

Owns what the application must accomplish and why.

Include:
- Product objectives and documented users/use cases.
- Approved release scope.
- Functional requirements.
- Business rules and validation.
- User workflows and observable outcomes.
- User-facing errors and recovery.
- Non-functional requirements.
- Acceptance criteria.
- Explicit exclusions and deferred capabilities.
- Links to relevant specifications and verification procedures.

Preserve requirement IDs `R01` through `R08`, their meaning, and any other existing IDs. Do not renumber requirements for convenience.

Write requirements as observable, testable behavior. Keep implementation details in the SDD unless they are approved product constraints.

Do not invent missing personas, metrics, performance thresholds, or acceptance rules. Mark genuine gaps as unresolved.

### docs/SDD.md — Technical design

Owns technical contracts and how approved requirements are supported.

Include, where supported by evidence:
- Architecture and project responsibilities.
- Technology stack and declared dependency versions.
- Domain models and invariants.
- Database schema and migration strategy.
- Repository contracts.
- MVVM, navigation, dependency injection, and service lifetimes.
- Startup and initialization.
- Persistence and concurrency.
- Error handling and recovery.
- Security/privacy mechanisms and limitations.
- Verification strategy.
- Mapping from requirements to technical components.

Distinguish implemented design from approved but unimplemented design.

Preserve these documented decisions without redesigning them:
- `DateOnly` for calendar due dates.
- `DateTimeOffset` for mutation timestamps.
- `TimeProvider` for testable time calculations.
- `Money` stored as signed 64-bit centavos.
- Immutable `Due` domain records.
- Soft deletion through `DeletedAt`.
- Numbered SQLite migration scripts.
- Serialized database operations through `DatabaseDispatcher`.
- Separation of domain, data, and application responsibilities.

If evidence contradicts any listed decision, document the discrepancy rather than presenting it as an implemented fact.

Use diagrams only when they add clarity and accurately reflect the documented state.

### docs/DESIGN.md — UI/UX specification

Preserve this document as the authority for:
- Brand identity.
- Color and typography tokens.
- Component appearance.
- Layout and spacing.
- Interaction and motion.
- Accessibility.
- Light and dark themes.

Limit edits to navigation, factual corrections supported by evidence, and necessary scope/status clarification.

Clearly label design concepts outside approved v1 scope. Their presence does not authorize implementation.

### docs/delivery/STATUS.md — Delivery and evidence

Owns current delivery status and references to historical evidence.

Track release slices, capabilities, remaining work, limitations, and acceptance gates.

Represent implementation and verification separately. A capability can be implemented while device acceptance remains pending.

For each relevant item, record:
- Implementation state.
- Automated verification state and evidence.
- Device verification state and evidence.
- Remaining acceptance work.
- Evidence date or “not recorded.”

Do not assign today’s date to historical results or treat a build as proof of feature acceptance.

### docs/delivery/TEST-PLAN.md — Verification procedures

Owns:
- Test scenarios and related requirement IDs.
- Preconditions and environments.
- Procedures and expected outcomes.
- Automated versus manual/device checks.
- Acceptance gates.

Keep procedures separate from execution results. Link to recorded evidence in delivery or historical records.

### docs/adr/ — Architecture decisions

Create an index explaining when and how to record significant architecture decisions.

Create individual ADRs only when the decision and its rationale are supported by existing evidence. Do not fabricate dates, alternatives, approval, or rationale. Label retrospective records as such.

## 5. Initial Feature Specification

Use existing V1.3 soft-delete work as the initial example if no equivalent feature specification exists.

Do not assume that V1.3 is the current development priority.

Each feature directory contains:

### spec.md
- Purpose and status.
- Scope and exclusions.
- Related PRD requirement IDs.
- User scenarios.
- Functional behavior.
- Acceptance criteria and edge cases.
- Unresolved questions.

### plan.md
- Relevant SDD contracts.
- Technical approach.
- Affected files/modules.
- Migration impact.
- Dependencies and risks.
- Verification approach.

### tasks.md
- Small, actionable tasks.
- Dependencies.
- Completion criteria.
- Required verification.
- Evidence supporting completed tasks.

Link to authoritative requirements and contracts instead of redefining them.

For soft deletion, distinguish existing repository behavior from confirmation UI and remaining acceptance work. Check off only tasks supported by evidence.

## 6. Agent Workflow and Governance

Update root `AGENTS.md` as a concise entry point while preserving existing applicable instructions.

Document this workflow:
1. Read the documentation index and task-relevant documents.
2. Identify governing requirements and technical/design contracts.
3. Create or update a feature spec for independently implementable changes.
4. Plan affected components and verification.
5. Implement within approved scope.
6. Run relevant validation.
7. Update affected canonical documentation and delivery evidence.

Require:
- Requirement traceability.
- Explicit separation of existing and proposed behavior.
- No undocumented scope expansion.
- PRD updates when product requirements change.
- SDD updates when technical contracts change.
- DESIGN updates when the UI system changes.
- ADRs for significant architecture decisions when appropriate.
- Evidence-based completion reporting.

Use progressive disclosure. Do not require every agent to read all documentation for every task.

## 7. Migration and Historical Preservation

Maintain an old-to-new section map covering original requirements, technical contracts, and historical evidence.

For existing `PLAN.md` and phase records:
- Retain historical content with clear status labels and links, or archive it with navigation preserved.
- Move current authoritative definitions into their owning documents.
- Avoid leaving active duplicate definitions.
- Update internal inbound links when sections move.
- Use pointer documents or a documented anchor map where needed to preserve discoverability.

Record unresolved conflicts with:
- The competing statements and their sources.
- Available implementation or test evidence.
- The decision needed.

Use an existing suitable location for the audit and migration record; otherwise create one concise migration record linked from `docs/README.md`.

## 8. Validation

After editing:

1. Check that required documents or justified equivalents exist.
2. Validate local Markdown file links and heading anchors.
3. Account for every original requirement, including `R01`–`R08`.
4. Confirm technical contracts and historical evidence remain discoverable.
5. Check for competing authoritative definitions.
6. Confirm deferred features remain deferred.
7. Verify documentation claims against the source inspected.
8. Check that `AGENTS.md` and indexes point to the correct locations.
9. Review the final diff for accidental scope or requirement changes.
10. Confirm that this task changed only permitted documentation files.

Use existing documentation validation tools where available. Do not install tooling just for this task.

Do not run application builds or tests merely to validate Markdown changes. If verification is not performed, report that accurately. Reading a test is not equivalent to running it.

Compare against the initial working-tree state so pre-existing user changes are not attributed to this task.

## 9. Final Report

Provide:
- Concise audit findings.
- Final documentation tree.
- Created, modified, moved, and retained historical files.
- Old-to-new migration map or a link to it.
- Requirement traceability summary.
- Unresolved conflicts and decisions.
- Validation performed, results, and limitations.
- Any remaining work.

The refactor is complete when document ownership is clear, requirements and evidence are preserved, navigation resolves, claims reflect available evidence, and no application behavior has been changed.

If an unresolved decision prevents completion, identify the exact gap rather than claiming full completion.

Begin with discovery and the audit, then perform and validate the documentation refactor.