# {{FeatureID}}: {{FeatureName}}

**Unapproved feature template.** Copy this directory to `specs/<feature>/` and customize all three files. Governing requirements: {{IDs linked to [PRD](../../docs/PRD.md)}}. Technical/UI contracts: [SDD](../../docs/SDD.md), [DESIGN](../../docs/DESIGN.md). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

{{User problem and observable outcome.}}

Existing behavior: {{Source-backed baseline}}. Proposed behavior: {{Authorized change}}. Exclusions/deferred work: {{Explicit limits}}. Authorization/decision evidence: {{Actual source or pending}}.

Do not create a second copy of product rules here; link to PRD IDs and refine the feature's acceptance scenarios.

## Acceptance scenarios

The following IDs/rows are examples to replace with feature-specific cases.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| FT01 / {{R01}} | {{Valid initial state and user action}} | {{Visible and durable outcome}} | {{Test + native procedure}} |
| FT02 / {{R01}} | {{Cancel or invalid input}} | {{Preserved state and absence of unintended writes}} | {{Test/native}} |
| FT03 / {{R02}} | {{Failure, retry or repeated action}} | {{Recovery, idempotency and retained input}} | {{Controlled failure/native}} |
| FT04 / {{Requirement}} | {{Resume/restart or platform difference}} | {{Contract-defined retained/reloaded state}} | {{Relevant platform}} |

## State, dependencies, and unresolved decisions

{{State transitions, concurrency constraints, data/platform dependencies, migrations, draft ownership, and unresolved decisions affecting scope. Link authoritative contracts.}}

## Evidence limits

No execution or acceptance is established by this template. Track implementation, tests, platform builds, native checks and acceptance separately in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md). Link dated records when available.
