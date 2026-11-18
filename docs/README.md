# Duewise documentation

This index owns documentation navigation and authority. Read the documents relevant to the task; historical records are evidence, not current specifications.

## Start here

- New checkout: [local setup](SETUP.md), then the [agent workflow](../AGENTS.md).
- Current progress or release readiness: [STATUS](delivery/STATUS.md); a prototype or build alone is not acceptance. For a quick deterministic summary or the live prototype beside the feature list, use the [developer dashboard](../tools/developer-dashboard/README.md).
- Work on a feature: find its spec in the [feature catalog](FEATURES.md), then use the authority map below.
- Check docs or find a utility: [developer tools](../tools/README.md).
- Start a different MAUI project: [reusable starter kit](templates/maui-starter/README.md). It contains templates and a local checker, not a generated app.

## Document ownership

| Document | Owns | Read when |
|---|---|---|
| [PRD](PRD.md) | Approved product requirements, business rules, acceptance outcomes, scope | Defining or changing user-visible behavior |
| [SDD](SDD.md) | Technical contracts, architecture, data, startup, concurrency, implementation constraints | Implementing or reviewing code |
| [DESIGN](DESIGN.md) | UI/UX standards, brand, tokens, appearance, interaction, accessibility; the branding inspiration image is non-normative | Changing the interface; apply its v1 scope notice |
| [Delivery status](delivery/STATUS.md) | Current implementation state, evidence attribution, remaining acceptance | Resuming delivery or reporting completion |
| [Test plan](delivery/TEST-PLAN.md) | Verification procedures, environments, expected outcomes, acceptance gates | Planning or performing verification |
| [Feature catalog](FEATURES.md) | Feature/spec/plan/task navigation; no copied authority | Finding an existing slice or proposal |
| [Local setup](SETUP.md) | Repository prerequisites, first-use routes and setup recovery | Opening this checkout or diagnosing setup |
| [Developer tools](../tools/README.md) | Tool entry points, safe prerequisites and exit meanings | Checking docs or finding release/native utilities |
| [ADR index](adr/README.md) | Recorded architecture decision rationale and ADR process | Making or revisiting a significant architecture decision |
| [Agent instructions](../AGENTS.md) | Development workflow and documentation maintenance rules | Working as a coding agent |

The PRD owns approved behavior; source inspection establishes implemented behavior. Tests establish only the behavior exercised by their recorded execution. None silently overrides another: record discrepancies and resolve scope or contract changes explicitly. DESIGN owns the UI system but cannot expand approved product scope. Feature specs refine requirements through linked scenarios rather than becoming a second product definition.

## Mobile HTML design review

Two sample-data HTML studies under [prototypes/](../prototypes/) explore layout and visual directions: the [page-by-page mobile review](../prototypes/mobile-review/index.html) with its [review notes](../prototypes/mobile-review/REVIEW.md), and the [design exploration](../prototypes/design-exploration/index.html), whose selected Focus mockup now includes the Philippine catalog, the [interaction lab](../prototypes/design-exploration/index.html#interaction-lab), sticky actions with context-aware navigation and the Essential-first editor. Read its [audit](../prototypes/design-exploration/AUDIT.md), [system guide](../prototypes/design-exploration/SYSTEM.md) and [interaction audit](../prototypes/design-exploration/INTERACTION-AUDIT.md) before changing it.

The dated register of each exploration, its selection status and governing scenario IDs is the [design explorations table in DESIGN](DESIGN.md#design-explorations-non-normative). Scope and agent-observed browser evidence live in the [mobile prototype](../specs/mobile-ui-prototype/spec.md), [PH catalog](../specs/ph-catalog-prototype/spec.md) and [interaction lab](../specs/interaction-lab/spec.md) specs and their tasks files. These artifacts are non-normative: selections and exports in them do not change production UI, PRD/SDD contracts or release acceptance. The one exception is explicit: on 2026-11-05 the user authorized the [Focus design adoption](../specs/focus-adoption/spec.md), which made the selected look and navigation the native baseline ([DESIGN 2.0.0](DESIGN.md), [ADR 008](adr/008-focus-design-baseline.md)).

## Task routes

- Product change: PRD → relevant feature spec → SDD or DESIGN as needed → test plan.
- Data or repository fix: relevant PRD IDs → SDD data contracts → affected source/tests.
- UI work: PRD → DESIGN scope notice and relevant components → SDD navigation → feature spec.
- Release check: delivery status → test plan → referenced evidence records.
- Documentation-only change: relevant canonical documents → local links and traceability validation; application builds are unnecessary unless another change requires them.

## Templates for future projects

- [Reusable MAUI starter kit](templates/maui-starter/README.md): documentation templates, a proposed project structure, development standards, and a reusable setup prompt for a new application. Includes an optional client–server path (API contract template, ASP.NET Core layout) added 2026-11-09 under the [API add-on spec](../specs/starter-kit-api/spec.md).
- [Repository review and template validation](templates/maui-starter/REVIEW.md): source-backed findings, improvement priorities, evidence limits, and documentation checks for the kit.

These are adaptation materials for future projects. They do not expand Duewise requirements, change its architecture, or close its release gates.

## History and migration

- [Current consolidation audit and migration map](delivery/DOCS-AUDIT-2026-11-08.md): findings, dispositions, preserved history and actual toolkit verification.
- [Documentation refactor audit and migration map](delivery/DOCS-REFACTOR.md): findings, unresolved decisions, preservation map, and validation. The refactor's task brief is retained as [docs-revamp.md](docs-revamp.md); it is an instruction record, not a product or technical authority.
- [Historical plan](PLAN.md): approved 2026-10-10 re-scope provenance, original schedule, toolchain history, decisions, and deferred backlog.
- [V1.1 record](phases/V1.1.md), [V1.2 record](phases/V1.2.md), and [V1.3 record](phases/V1.3.md): dated implementation/test evidence and attributed phone acceptance.
- [V1.3 execution evidence](delivery/evidence/V1.3-2026-10-17.md): 2026-10-17 soft-delete environment, artifact, and source hashes, plus the post-record check of those hashes against committed source. Current feature tracking is in the linked spec/tasks and delivery status.
- [Pre-refactor SDD snapshot](delivery/SDD-2026-10-17.md): original review, requirements, contracts, and evidence before responsibility separation.
- [Learning profile](../.vibe-wise/profile.md), [progress](../.vibe-wise/progress.md), and [project map](../.vibe-wise/project-map.md): learning preferences and historical context; not current product or delivery authority.

Retain stable requirement IDs and historical evidence. When a section moves, update inbound links and the migration map. Do not delete historical documents merely because newer canonical documents supersede them.
