# {{ProjectName}} documentation

**Template:** this index owns document navigation and authority. Replace placeholders and link new feature/decision/evidence records as they are created. Keep the root README short so it does not become a second status or requirements document.

## Start here

- First checkout: [SETUP](SETUP.md). Start with its prerequisite record, not unfinished build examples.
- First product decisions: [PRD](PRD.md), then the [feature template](../specs/feature-template/spec.md).
- Current progress and blockers: [STATUS](delivery/STATUS.md).
- Documentation check or an error to fix: [tools guide](../tools/README.md).

## Document ownership

| Document | Owns | Update when |
|---|---|---|
| [PRD](PRD.md) | Product problem, approved requirements, business rules, platform scope, acceptance outcomes | User-visible behavior or scope changes |
| [SDD](SDD.md) | Architecture, dependency directions, data/navigation/lifecycle contracts, technical platform capability matrix | A technical contract changes |
| [DESIGN](DESIGN.md) | UI tokens, interactions, accessibility, responsive and localized presentation | UI-system decisions change |
| [API](API.md), if the app calls a server | Client–server rules: environments, compatibility, sign-in, errors, retries, data encoding, offline; endpoint shapes stay in the committed OpenAPI document | An API rule or released contract changes |
| [SETUP](SETUP.md) | Exact toolchain, build hosts, environment configuration, developer commands | Toolchain or setup changes |
| [DEVELOPMENT](DEVELOPMENT.md) | Coding, review, and maintenance practices | Team practices change |
| [RELEASE](RELEASE.md) | Packaging, signing, distribution and recovery procedures | Delivery method changes |
| [STATUS](delivery/STATUS.md) | Current implementation and evidence-linked verification/acceptance state | New work or evidence changes the state |
| [TEST-PLAN](delivery/TEST-PLAN.md) | Procedures, environments and gate definitions | Behavior or verification boundaries change |
| [Evidence](delivery/evidence/TEMPLATE.md) | Dated actual results and attribution | A check is executed |
| [Feature specs](../specs/feature-template/spec.md) | Scenario refinement, bounded plans and tasks linked to requirements | A feature is planned or changed |
| [ADRs](adr/README.md) | Significant decisions and rationale | A consequential choice is made or superseded |
| [REFERENCES](REFERENCES.md) | External technical guidance and review dates | Guidance is revalidated |
| [AGENTS](../AGENTS.md) | Agent workflow | Workflow changes |
| [Tools guide](../tools/README.md) | Safe automation entry points, documentation checker and exit meanings | A helper or its prerequisites change |

## Task routes

- Product change: PRD -> feature spec -> SDD/DESIGN -> TEST-PLAN.
- Data/platform change: SDD -> affected source/tests -> relevant feature plan -> evidence.
- API change: API compatibility rules -> OpenAPI diff -> API and client tests -> TEST-PLAN remote procedures -> STATUS.
- First checkout or toolchain upgrade: SETUP -> REFERENCES -> platform builds/tests -> STATUS.
- Release: STATUS -> TEST-PLAN -> RELEASE -> artifact-specific evidence.
- Documentation-only task: affected authority -> local path/anchor and consistency checks -> diff.

## Current feature and decision indexes

Replace this example link with real features as they are created: [feature template](../specs/feature-template/spec.md), [plan](../specs/feature-template/plan.md), [tasks](../specs/feature-template/tasks.md). The [ADR index](adr/README.md) starts empty.

## History and unresolved conflicts

{{Link retained historical plans, retired designs, and migration maps if they exist.}}

| Conflict ID | Conflicting sources | Effect | Owner / resolution |
|---|---|---|---|
| {{C01}} | {{Links and exact discrepancy}} | {{Work affected}} | {{Decision or pending owner}} |

Requirements describe intended behavior, source shows implementation, and execution records show verified behavior. Record discrepancies explicitly. Archived records retain provenance and do not override current authorities.
