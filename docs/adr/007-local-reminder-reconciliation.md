# ADR 007: Local reminder reconciliation

Status: accepted implementation choice for authorized F05. Decision date: 2026-10-18.

## Context and provenance

The user authorized F05 implementation on 2026-10-18 and confirmed day-before plus due-date defaults. [PRD](../PRD.md#f05-reminder-rules) owns that behavior. The [historical project map](../../.vibe-wise/project-map.md#adrs-learner-proposed-none-yet-written-to-the-repo) reserved proposed 007 for disposable alarms rebuilt from SQLite. No earlier approved ADR 007 file existed; this record realizes that proposed identity for the current implementation. It does not retroactively approve other unwritten ADRs or claim separate user approval of these technical details.

## Decision and rationale

SQLite dues, saved settings and delivery receipts are authoritative. Reconcile them through the same singleton database dispatcher used by foreground operations. Treat Android alarms/jobs as replaceable delivery machinery. Use one next inexact alarm plus a recovery job; delegate receivers to JobScheduler instead of doing unbounded asynchronous receiver work. Reconcile again after lifecycle events and committed writes.

This allows payment, deletion, settings/date changes and Android lifecycle loss to converge on committed local state, preserves offline operation, and avoids exact-alarm special access for date-only reminders. The repository decorator queues work after commit so scheduling failures do not falsely fail a saved due. Bounded posting limits each pass; receipts prevent normal restart/retry replay.

## Alternatives and consequences

- One OS alarm per occurrence would multiply disposable state and cancellation identities. A single next alarm plus durable queue keeps replacement centralized.
- Exact alarms require additional platform access and do not remove force-stop or permission limits; current product policy permits inexact delivery.
- Doing database work directly inside BroadcastReceiver risks outliving its lifetime. JobScheduler owns the worker lifetime and retry.
- A server/push channel would add network/account/data contracts outside F05 and would not replace offline reminders.

The queue and receipts add schema 3 and journal growth. Posting and receipt storage are not one atomic transaction, so crash-boundary exactly-once delivery is not guaranteed. Android restrictions can delay work; reconciliation is recovery, not a delivery guarantee. Native lifecycle acceptance remains distinct from unit tests/builds.

## Contract and verification

[SDD contract](../SDD.md#f05-reminder-contract), [feature spec](../../specs/f05-reminders/spec.md), [test procedure](../delivery/TEST-PLAN.md#f05-reminder-verification), and [delivery evidence](../delivery/evidence/F05-REMINDERS-2026-10-18.md) track implementation and actual verification.

## Completion follow-up, 2026-10-18

The user subsequently requested completion of F05. Immediate jobs now use API-31 expedited scheduling with normal-job fallback under the same job ID if quota is rejected; older supported APIs retain normal jobs. [Android JobInfo guidance](https://developer.android.com/reference/android/app/job/JobInfo.Builder) states that ordinary deadlines can be delayed by Doze, while expedited requests have stronger execution treatment subject to quota. The short serialized reconciliation is bounded to ten posts per pass; periodic recovery remains inexact. This does not bypass app background restrictions or guarantee delivery.

Android-reported power status and explicit app-settings access make the observed phone limitation actionable without automatically requesting exemptions. [Completion evidence](../delivery/evidence/F05-COMPLETION-2026-10-18.md) records natural phone delivery under a temporarily unrestricted vendor policy, isolated API-36 natural/Doze delivery, and lifecycle/failure observations. Original policy restoration and remaining broad release coverage are tracked there. The initial artifact evidence remains historical.
