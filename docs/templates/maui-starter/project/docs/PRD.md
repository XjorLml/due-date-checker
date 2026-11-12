# {{ProjectName}} product requirements

**Template. Authority:** approved product behavior, business rules, scope, and acceptance outcomes. Technical implementation belongs in [SDD](SDD.md); presentation in [DESIGN](DESIGN.md); evidence in [STATUS](delivery/STATUS.md).

Status: {{DraftOrApprovedWithEvidence}}. Decision owner: {{Owner}}. Last reviewed: {{Date}}.

## Problem, users, and success

{{Describe the user's problem, intended users, primary workflow, and observable benefit.}}

Success measures: {{ApprovedMeasureOrNotYetDefined}}. Do not invent adoption targets, performance thresholds, or acceptance.

## Release scope

| Decision | Selected scope / unresolved question |
|---|---|
| Target platforms | {{AndroidIOSMacCatalystWindowsSelection}} |
| Offline and connectivity | {{OfflineRequiredOnlineRequiredOrHybrid; behavior during failures}} |
| Data and accounts | {{Data stored; account requirement; retention/deletion/recovery expectations}} |
| Server and older app versions | {{NoServer, OwnApi or ThirdPartyApi; how long older app versions must keep working; forced-update policy}} |
| Languages and locales | {{Initial languages; formatting and RTL scope}} |
| Date, time, and money | {{Calendar dates versus instants; product/device timezone; currency if applicable}} |
| Distribution | {{InternalSideloadStore; intended audience}} |
| Explicit exclusions | {{Capabilities deferred from this release}} |

Implementation capabilities and technical OS baselines are recorded in the SDD platform matrix. With a server, the network rules that implement these choices belong in [API](API.md). Choose them to satisfy this product scope; do not assume every MAUI-supported platform is a release target.

## Requirements

Keep IDs stable when wording changes. Add rows only for real agreed requirements; these rows are writing examples, not approved product behavior.

| ID | User need and required behavior | Acceptance outcome | Priority / decision state |
|---|---|---|---|
| R01 | {{User action and resulting behavior}} | {{Externally observable result}} | {{RequiredOptional / DraftApproved}} |
| R02 | {{Recovery, offline, privacy, or accessibility need}} | {{Observable success/failure outcome}} | {{Priority / State}} |

## Business rules

| Rule ID | Requirement | Rule and edge cases | Decision evidence |
|---|---|---|---|
| BR01 | R01 | {{Validation, normalization, identity, ordering, or status rule}} | {{Source or pending}} |

Define zero/negative/empty/boundary handling where applicable. Make date/timezone, rounding, deletion/restore, and retention policies explicit when the domain needs them. Do not inherit PHP, Manila, soft deletion, or fixed categories from the reference app.

## Workflows and recovery

| Workflow / requirement | Successful result | Failure / cancellation / restart result |
|---|---|---|
| {{Workflow / R01}} | {{Persisted and visible state}} | {{Retained input/data, retry, and navigation outcome}} |

Distinguish an empty result from a failed read, and a failed mutation from a successful commit followed by a failed follow-up action. State what drafts survive navigation, resume, and process loss.

## Non-functional outcomes

{{Record approved accessibility, privacy, reliability, data size, and performance expectations. Link technical mechanisms to SDD and procedures to TEST-PLAN. Mark unmeasured targets as proposals.}}

## Decisions and acceptance

Unresolved decisions: {{Question, owner, affected work}}. Approved changes: {{Links to actual decision evidence}}.

Acceptance uses [TEST-PLAN](delivery/TEST-PLAN.md), with outcomes recorded in [STATUS](delivery/STATUS.md). Approval of this document does not imply implementation or release acceptance.
