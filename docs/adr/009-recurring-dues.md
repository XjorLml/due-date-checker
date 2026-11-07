# ADR 009: Recurring dues as linked occurrences

Status: accepted for the UX03 recurrence slice. Decision date: 2026-11-06.

## Context and provenance

The user decided on 2026-11-06 ([decision log](../../specs/ux-improvements/spec.md#decision-log)) that a due can repeat Weekly, Monthly or Yearly, that only Bill and Subscription repeat, that the next occurrence appears when the current one is marked paid (option A, chosen over "always visible ahead"), and that a missing day falls back to the month's last day. The user then authorized implementation with "start wave 2". The storage and consistency choices below were proposed by Claude while implementing; their rationale is stated here for the learner's review and has not been separately approved. The historical project map reserves no ADR 009.

## Decision

1. **Rule on each row, not a series table.** `dues` gains `repeat_rule`, `repeat_day` and `repeat_source_id` (migration 005, additive `ALTER TABLE`). Every occurrence is an ordinary due.
2. **Anchor day.** Monthly and Yearly store the intended day of month in `repeat_day`; each next date is computed from that anchor, clamped to the month's length.
3. **Link and guard.** A generated occurrence stores its source due's ID in `repeat_source_id`. The insert runs as `INSERT … SELECT … WHERE NOT EXISTS (SELECT 1 FROM dues WHERE repeat_source_id = @source)`, so one source never yields two occurrences, including after paid → unpaid → paid or after the generated one was deleted. A partial index on `repeat_source_id` serves that check.
4. **One transaction.** `IDueRepository.UpdateWithNextAsync(updated, next)` saves the paid row and inserts the next one in one SQLite transaction. The interface provides a default (update, then add) so test doubles keep compiling; `SqliteDueRepository` overrides it with the transaction and `ReminderAwareDueRepository` forwards it and requests one reminder reconciliation, which re-plans every due including the new one.
5. **Domain owns the rule.** `Due.Validate()` rejects a repeat on Task/Other and a missing or out-of-range anchor; `Recurrence` computes next dates and builds the next occurrence.

## Rationale

- Option A means only one unpaid occurrence of a series exists at a time, so a separate series table adds a second source of truth without a user-visible benefit; ordinary rows keep Details, reminders, filters and delete unchanged.
- Computing from the last date alone drifts permanently after a short month (31 → 28 → 28); the anchor keeps the user's day.
- A double tap, a stale screen or undoing a mistaken "paid" must not create duplicate bills; the database guard holds even if the app's own checks are bypassed.
- A crash between saving "paid" and inserting the next occurrence would silently end the series; the transaction makes both or neither.
- The default interface member avoids editing every test double while keeping production atomic. Plain C# (C# 8+) and no new dependency.

## Alternatives considered

- **Series table plus generated instances.** Supports "always visible ahead" and series edits; rejected because the user chose option A and it doubles the editing rules.
- **Compute from the previous date without an anchor.** Simpler; rejected for drift after short months.
- **App-side duplicate check only.** Rejected: not atomic with the insert and bypassable by another write path.
- **Two separate repository calls.** Rejected: a failure between them ends the series without telling the user.

## Consequences

- Schema version 5; export (UX05) must include the new columns.
- Editing an occurrence affects later ones only through the paid occurrence being copied.
- Removing the guard or the anchor later needs a new migration and an ADR.

Links: [spec RC01–RC08](../../specs/f07-recurrence/spec.md), [PRD UX03](../PRD.md#ux-review-decisions-2026-11-06), [SDD](../SDD.md#f07-recurrence-contract).
