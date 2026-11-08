# {{FeatureName}} implementation plan

**Template.** [Spec](spec.md) owns feature scenarios; [tasks](tasks.md) owns execution tracking. Status: {{ProposedOrAsBuilt}}.

## Affected modules and contracts

| Module / source path | Planned change | Governing contract / scenario |
|---|---|---|
| {{Module}} | {{Small concrete change}} | {{SDD/PRD/FT ID}} |

## Dependencies and approach

{{Explain the sequence, existing components to reuse, platform integrations, DI lifetimes, and any new package with its reason. Distinguish required prerequisites from optional improvements.}}

{{If persisted data changes: describe old-schema fixtures, migration transaction/version behavior, upgrade/downgrade limits and recovery. If there is no migration, state that explicitly.}}

## Verification

1. {{Define meaningful behavior/failure tests and fixtures.}}
2. {{Implement and run relevant automated checks.}}
3. {{Build actual selected targets on available hosts.}}
4. {{Perform required native scenarios with controlled data.}}
5. {{Update evidence, owning contracts, and remaining acceptance.}}

Use [TEST-PLAN](../../docs/delivery/TEST-PLAN.md). Expected results belong in the spec/procedures; actual outcomes belong in evidence records. Do not write planned results as completed work.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation / owner |
|---|---|---|
| {{Risk}} | {{Observable effect}} | {{Action}} |

Significant rationale: {{ADR link when a record exists}}. Documentation updates: {{Owning documents affected}}.
