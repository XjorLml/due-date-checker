# {{FeatureName}} tasks

**Unexecuted template.** Scope/scenarios: [spec](spec.md). Sequence/dependencies: [plan](plan.md). Record actual evidence links before completing verification tasks.

## Definition

- [ ] Link governing requirements and authorization; record baseline and exclusions.
- [ ] Resolve blocking decisions and define observable scenarios.
- [ ] Identify source/test/platform dependencies and meaningful checks.

## Implementation

- [ ] {{Implement domain/data behavior if needed.}}
- [ ] {{Implement presentation and native adapters.}}
- [ ] {{Implement failure/retry/concurrency/lifecycle behavior.}}
- [ ] Update affected PRD/SDD/DESIGN contracts and ADRs where appropriate.

## Automated and build verification

- [ ] {{Relevant tests pass; link result with revision/environment.}}
- [ ] {{Each applicable App target builds; record host, version and outcome.}}
- [ ] Documentation links, traceability and diff are checked.

## Native verification

- [ ] {{Required scenarios pass on selected devices/artifacts.}}
- [ ] {{Accessibility/layout and lifecycle checks pass at the required boundary.}}
- [ ] Restore fixtures/device settings and record any remaining changes.

## Acceptance

- [ ] Required gates are satisfied or exceptions explicitly decided.
- [ ] Acceptance decision is recorded with its owner and scope.
- [ ] [STATUS](../../docs/delivery/STATUS.md) links the evidence and remaining limitations.

Blocked/not-applicable tasks: {{Exact reason, owner, next action}}. Implementation completion alone does not complete the later sections. A checkbox must not imply a test/device result that has not occurred.
