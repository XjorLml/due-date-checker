# Duewise documentation

This index owns documentation navigation and authority. Read the documents relevant to the task; historical records are evidence, not current specifications.

## Document ownership

| Document | Owns | Read when |
|---|---|---|
| [PRD](PRD.md) | Approved product requirements, business rules, acceptance outcomes, scope | Defining or changing user-visible behavior |
| [SDD](SDD.md) | Technical contracts, architecture, data, startup, concurrency, implementation constraints | Implementing or reviewing code |
| [DESIGN](DESIGN.md) | UI/UX standards, brand, tokens, appearance, interaction, accessibility; the branding inspiration image is non-normative | Changing the interface; apply its v1 scope notice |
| [Delivery status](delivery/STATUS.md) | Current implementation state, evidence attribution, remaining acceptance | Resuming delivery or reporting completion |
| [Test plan](delivery/TEST-PLAN.md) | Verification procedures, environments, expected outcomes, acceptance gates | Planning or performing verification |
| Feature specifications: [V1.3 soft delete](../specs/v1.3-soft-delete/spec.md), [V1.4 CI](../specs/v1.4-ci/spec.md) | Feature scenarios, approach, tasks, links to governing contracts | Working on an independently implementable change |
| [Design adoption](../specs/design-adoption/spec.md), [implementation plan](../specs/design-adoption/plan.md), [tasks](../specs/design-adoption/tasks.md) | Existing list/editor adoption of DESIGN within v1 scope | Styling, branding, responsive layout, field states and native verification |
| [Future-page roadmap](../specs/future-pages/plan.md), [scope](../specs/future-pages/spec.md), [tasks](../specs/future-pages/tasks.md) | Priorities/dependencies for production slices, authorized F01/F02/F03, F07 categories, F05 reminders and F04 preferences, historical samples and historical mock/navigation milestones | Reviewing current versus deferred pages |
| [Prototype comparison, 2026-10-18](delivery/PROTOTYPE-REVIEW-2026-10-18.md) | Supplied prototype versus inspected implementation, missing features and design differences; priorities remain owned by the roadmap | Understanding which pictured features are real, sample-only or missing |
| [F01 item details](../specs/f01-item-details/spec.md), [plan](../specs/f01-item-details/plan.md), [tasks](../specs/f01-item-details/tasks.md) | Authorized read-only saved-item page, editor return/refresh and unavailable/error states | Working on production Details and Dues → Details → editor navigation |
| [F02 Dashboard](../specs/f02-dashboard/spec.md), [plan](../specs/f02-dashboard/plan.md), [tasks](../specs/f02-dashboard/tasks.md) | Saved-data summaries, date status, title search, filters and Dashboard navigation | Working on real Dashboard data, clock boundaries and refresh behavior |
| [F03 Calendar](../specs/f03-calendar/spec.md), [plan](../specs/f03-calendar/plan.md), [tasks](../specs/f03-calendar/tasks.md) | Saved-data month counts, selected-day agenda, Manila Today and Calendar navigation | Working on production Calendar data, selection and refresh |
| [F07 categories](../specs/f07-categories/spec.md), [plan](../specs/f07-categories/plan.md), [tasks](../specs/f07-categories/tasks.md) | Fixed category taxonomy, legacy migration, editor/labels and Dashboard filter; other F07 extensions deferred | Working on categories and schema 2 |
| [F05 reminders](../specs/f05-reminders/spec.md), [plan](../specs/f05-reminders/plan.md), [tasks](../specs/f05-reminders/tasks.md) | Opt-in date-only Android reminders, schema 3, battery/background readiness, reconciliation and generic notifications | Working on reminder policy, delivery, permissions or taps |
| [F04 preferences](../specs/f04-preferences/spec.md), [plan](../specs/f04-preferences/plan.md), [tasks](../specs/f04-preferences/tasks.md) | Saved appearance/week start, Settings and existing Reminders access; export separately deferred | Working on app preferences, startup theme or calendar layout |
| [ADR index](adr/README.md) | Recorded architecture decision rationale and ADR process | Making or revisiting a significant architecture decision |
| [Agent instructions](../AGENTS.md) | Development workflow and documentation maintenance rules | Working as a coding agent |

The PRD owns approved behavior; source inspection establishes implemented behavior. Tests establish only the behavior exercised by their recorded execution. None silently overrides another: record discrepancies and resolve scope or contract changes explicitly. DESIGN owns the UI system but cannot expand approved product scope. Feature specs refine requirements through linked scenarios rather than becoming a second product definition.

## Task routes

- Product change: PRD → relevant feature spec → SDD or DESIGN as needed → test plan.
- Data or repository fix: relevant PRD IDs → SDD data contracts → affected source/tests.
- UI work: PRD → DESIGN scope notice and relevant components → SDD navigation → feature spec.
- Release check: delivery status → test plan → referenced evidence records.
- Documentation-only change: relevant canonical documents → local links and traceability validation; application builds are unnecessary unless another change requires them.

## Templates for future projects

- [Reusable MAUI starter kit](templates/maui-starter/README.md): documentation templates, a proposed project structure, development standards, and a reusable setup prompt for a new application.
- [Repository review and template validation](templates/maui-starter/REVIEW.md): source-backed findings, improvement priorities, evidence limits, and documentation checks for the kit.

These are adaptation materials for future projects. They do not expand Duewise requirements, change its architecture, or close its release gates.

## History and migration

- [Documentation refactor audit and migration map](delivery/DOCS-REFACTOR.md): findings, unresolved decisions, preservation map, and validation. The refactor's task brief is retained as [docs-revamp.md](docs-revamp.md); it is an instruction record, not a product or technical authority.
- [Historical plan](PLAN.md): approved 2026-10-10 re-scope provenance, original schedule, toolchain history, decisions, and deferred backlog.
- [V1.1 record](phases/V1.1.md), [V1.2 record](phases/V1.2.md), and [V1.3 record](phases/V1.3.md): dated implementation/test evidence and attributed phone acceptance.
- [V1.3 execution evidence](delivery/evidence/V1.3-2026-10-17.md): 2026-10-17 soft-delete environment, artifact, and source hashes, plus the post-record check of those hashes against committed source. Current feature tracking is in the linked spec/tasks and delivery status.
- [Pre-refactor SDD snapshot](delivery/SDD-2026-10-17.md): original review, requirements, contracts, and evidence before responsibility separation.
- [Learning profile](../.vibe-wise/profile.md), [progress](../.vibe-wise/progress.md), and [project map](../.vibe-wise/project-map.md): learning preferences and historical context; not current product or delivery authority.

Retain stable requirement IDs and historical evidence. When a section moves, update inbound links and the migration map. Do not delete historical documents merely because newer canonical documents supersede them.
