# Delivery status

**Template. Authority:** current implementation, verification, and acceptance state. Behavior belongs in [PRD](../PRD.md); procedures in [TEST-PLAN](TEST-PLAN.md). Start with no completed evidence.

Last update: {{Date}}. Source baseline: {{RevisionAndWorkingTreeState}}. Current focus: {{FeatureOrDecision}}. Release readiness: **Not assessed**.

## Feature state

| Requirement / feature | Implementation | Automated verification | Platform build | Device verification | Acceptance |
|---|---|---|---|---|---|
| {{R01 / feature link}} | Not started | Not run | Not run | Not run | Pending |

Suggested states: implementation = not started/in progress/implemented; verification = not run/pass/fail/blocked/not applicable with reason; acceptance = pending/accepted/rejected with decision evidence. Use separate platform rows when outcomes differ. A blocked check requires a concrete reason and next action.

## Evidence register

| Evidence record | Date | Source / artifact | Environment | Attribution | Result / limits |
|---|---|---|---|---|---|
| {{Link after execution}} | {{Date}} | {{Revision/hash}} | {{Host/device/toolchain}} | {{Agent-observed, user-reported, or historical}} | {{Observed result; excluded coverage}} |

Use the [evidence template](evidence/TEMPLATE.md). A template/procedure is not a completed execution record. A previous pass does not automatically apply to changed source or another platform.

## Remaining gates and decisions

| Gate / decision | Owner | Missing evidence or dependency | Next action |
|---|---|---|---|
| {{Gate ID}} | {{Owner}} | {{Specific gap}} | {{Action}} |

## Recent delivery changes

{{Brief current-state update linked to evidence. Keep detailed execution history in dated records, not duplicated across every row.}}

For documentation-only work, record the changed document scope and observed link/consistency/diff checks. Keep application verification and acceptance unchanged unless new application evidence exists.
