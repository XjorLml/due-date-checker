# ADR 012: Two reminder channels, overdue reminders by date, and a waiting amount

Status: accepted for the UX review decisions of 2026-11-16. Decision date: 2026-11-16.

## Context and provenance

The [2026-11-16 UX review](../delivery/UX-REVIEW-2026-11-16.md) found three problems:
- Reminders used one Default-importance channel that sounded like any app.
- Reminders stopped at the due date.
- Bills whose amount changes copied a stale amount.

The user asked to "implement 1-5", the review's ordered recommendations ([PRD](../PRD.md#ux-review-decisions-2026-11-16)). The technical choices below were proposed by Claude while implementing. Their rationale is recorded for the learner's review and was not separately approved. [ADR 007](007-local-reminder-reconciliation.md) (reconciliation) and [ADR 009](009-recurring-dues.md) (linked occurrences) stay in force.

## Decision

1. **Two new channel IDs and deleting the old one.** Android fixes a channel's sound and importance when it is created, and afterwards only the user can change them. To get a High-importance "Due today" channel, the app therefore creates `duewise_due_today_v1` (High) and `duewise_coming_up_v1` (Default) and deletes `duewise_reminders_v1` once. A preference flag shows a one-time explanation.
2. **The app shows and links, Android owns the sound.** The Reminders page reads each channel's importance and sound title and opens that channel's system page. There is no in-app ringtone picker and no bundled sound.
3. **Overdue reminders are ordinary occurrences with positive offsets.** They use the same planner, key format and delivery receipts, with offsets +1, +7, +14, +21 and +28. Each occurrence's last delivery date is the later of its due date and its own Manila date. That one rule replaces "never after the due date" for both planning and the open and payment guards.
4. **The overdue setting lives with the other reminder settings in SQLite** (`reminder_settings.overdue`, migration 008, default on).
5. **`amount_varies` is a column on the due row** (migration 008). The domain allows a NULL amount on a Bill/Subscription only with the flag and a repeat. The next occurrence starts with a NULL amount. Export format 4 carries the flag.
6. **Paydays are an app preference** (format `2|theme|week|first|second`), not a SQLite setting. They only shape a display figure, and they reuse the Twice a month day rules.

## Rationale

- New channel IDs are the only way to change a channel's default importance; keeping the old ID would leave everyone on a no-banner channel.
- Linking to Android's page keeps the user's choice of ringtone, vibration and Do Not Disturb behavior authoritative and avoids recreating channels behind their back.
- Reusing the occurrence model means reconciliation, receipts, stale-action guards and the ten-per-pass limit apply to overdue reminders without a second scheduler.
- A stored flag on the due follows the series, because each next occurrence copies it; this matches how repeat days are kept.
- Paydays affect no stored due, so they belong with the other display preferences and need no migration.

## Alternatives considered

- **Keep one channel and raise its importance.** Not possible after creation; rejected.
- **A daily overdue reminder until paid.** Too noisy for a forgotten task; rejected for 1 day, then weekly for four weeks.
- **A separate "Overdue" channel.** More settings for little gain; overdue reminders share "Due today".
- **A NULL amount for every repeating bill.** It would lose the right amount for rent and subscriptions; rejected for an explicit switch.
- **Paydays in SQLite.** It would need a migration and export change for a display-only setting; rejected.

## Consequences

- A user who had changed the old channel's sound must set it again; the app says so once.
- Schema version 8 and export format 4. Older builds refuse both as newer.
- Each unpaid due can now have up to seven planned reminders; the delivery limit per pass is unchanged.
- The planner's and guards' "due date has not passed" checks now read `LastDeliveryDate`.

Links: [spec](../../specs/ux-improvements-3/spec.md), [SDD](../SDD.md#ux-review-2026-11-16-contract), [evidence](../delivery/evidence/UX-WAVE6-2026-11-16.md).
