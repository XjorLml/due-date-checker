# {{CheckOrFeature}} execution evidence

**Unexecuted template.** Copy to a dated descriptive filename after a check is performed. Procedure definitions belong in [TEST-PLAN](../TEST-PLAN.md); current state in [STATUS](../STATUS.md).

## Identity and attribution

- Date/time and timezone: {{TimestampAndZone}}.
- Observer: {{Agent-observed, user-reported, or historical; identify source}}.
- Requirements/scenarios/procedures: {{IDsAndLinks}}.
- Source revision and working-tree changes: {{RevisionAndChangeIdentity}}.
- Artifact: {{PathOrReleaseID; cryptographic hash; configuration; public signing identity if relevant}}.
- Environment: {{HostOS, SDK/workload, target, device/emulator model/OS/architecture}}.
- Preconditions/fixtures: {{State, consent/authorization scope where relevant, synthetic data identifiers}}.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| {{Procedure or exact command}} | {{Expected}} | {{Observed, not inferred}} | {{NotRunPassFailBlocked}} |

Artifacts/logs: {{Sanitized locations; excluded sensitive data}}. Failures and subsequent retries: {{Keep both results and identify changed artifacts}}.

## Restoration and limits

{{Record fixture cleanup and restored device settings if changed, with verification. If restoration is incomplete, identify exactly what remains.}}

Not exercised: {{Boundaries/scenarios}}. Historical evidence reused: {{Source and exact applicability}}. Acceptance decision: {{PendingOrActualDecisionEvidence}}. Automated/build/device evidence must remain distinguishable.

For documentation checks, use document scope/diff identity in place of an application artifact and record path/anchor/consistency validation. Do not fabricate device or application results.
