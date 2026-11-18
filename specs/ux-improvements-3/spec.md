# UX improvements from the 2026-11-16 review

Status: **decided 2026-11-16 by the user's "implement 1-5"** (the five recommendations, in order, from the [UX review 2026-11-16](../../docs/delivery/UX-REVIEW-2026-11-16.md#ringtone-and-alert-recommendations-ux29ux31)); implementation tracked in [tasks](tasks.md), approach in [plan](plan.md). Governing rules: [PRD UX review decisions (2026-11-16)](../../docs/PRD.md#ux-review-decisions-2026-11-16).

## Decision log

| # | Review item | Decision | Details proposed by Claude while implementing (not separately approved) |
|---|---|---|---|
| 1 | UX28 | Correct the Reminders privacy text to match N1. | Exact wording below (PR01). |
| 2 | UX29 | Visible "Sound & alert style" card; reminders split into two Android channels; Test works before enabling. | Channel names "Due today" (High: banner and sound) and "Coming up" (Default); the old channel is deleted once with a one-time message; no bundled custom sound. |
| 3 | UX30 | Overdue nudges. | 1, 7, 14, 21 and 28 days after the due date at the reminder time, then stop; an "Also when overdue" switch, on by default; sent on the Due today channel. |
| 4 | UX33 | Payday setting and a "before payday" Dashboard figure. | Two payday days in Settings (default 15 and 30, same pickers and month-end fallback as Twice a month); the Dashboard headline becomes "Before payday · {next payday}" and includes overdue dues (settles UX18); the "A little further ahead" list keeps month end. |
| 5 | UX32 | "Amount changes each month" for repeating bills. | Only on a repeating Bill or Subscription; the next occurrence starts with no amount, shown as "Amount needed"; such a Bill may be saved without an amount. |

## Acceptance scenarios

| ID | Outcome |
|---|---|
| PR01 | Reminders → "Notification readiness & privacy" reads: "Unlocked, a reminder shows the due's title, amount and date. The lock screen only says you have a due. Notes are never shown. Android may delay reminders to save battery, and Force stop can pause them until you reopen Duewise." |
| AL01 | Reminders are posted on two channels: the due date and overdue nudges on "Due today" (High importance, so a banner and the channel's sound), the day-before reminder on "Coming up" (Default). The test notification, payment feedback and the group summary use "Due today"; feedback and summary never alert again. |
| AL02 | On first use of the new build the old "Due reminders" channel is deleted. The Reminders page then says once: "Reminders now use two Android channels, Due today and Coming up. If you changed the reminder sound before, set it again under Sound & alert style." |
| AL03 | The Reminders page shows a "Sound & alert style" card outside the collapsed section, with one row per channel showing its state ("Banner · {sound name}", "Sound, no banner · {sound name}", "Silent" or "Off"). Tapping a row opens Android's settings for that channel. The state refreshes on return. |
| AL04 | "Play a test reminder" works whether or not reminders are enabled: it asks for Android notification permission when needed and posts on "Due today". Blocked access explains how to allow it. |
| AL05 | Notification access counts as allowed when Android permission, app notifications and the Due today channel are allowed; a blocked Coming up channel is reported in the status text without blocking reminders. |
| OD01 | With reminders enabled and "Also when overdue" on, each unpaid active due gets a reminder at the reminder time 1, 7, 14, 21 and 28 days after its due date, moved out of quiet hours like other reminders; nothing after day 28. |
| OD02 | An overdue reminder reads "{Title} is 1 day late" / "… is 7 days late", with the amount (or "Amount needed" / "{Type}, no amount") and the due date; the lock screen version says "You have an overdue due". |
| OD03 | A missed overdue reminder is still delivered later on its own day, never on a later day. Paying, deleting or editing the due removes its pending nudges as for other reminders. |
| OD04 | "Mark as paid" and a tap on an overdue reminder work while that reminder is current, with the same guards as N2. |
| OD05 | "Also when overdue" is stored with the reminder settings (schema 8, default on) and turning it off removes pending overdue reminders. |
| PD01 | Settings shows "Paydays" with two day pickers (1–31, default 15 and 30), saved with the other preferences; equal days are refused with "Choose two different paydays." |
| PD02 | The next payday is the earliest of the two days strictly after today (Manila), with the month-end fallback of Twice a month: on 16 Nov with 15/30 it is 30 Nov; on 30 Nov it is 15 Dec; 30 → 28 Feb. |
| PD03 | The Dashboard headline reads "Before payday · 30 Nov" and totals unpaid dues with an amount dated before the next payday, overdue ones included. The line below reads "{N} unpaid", then "· ₱X overdue" when overdue dues carry amounts, then "· {M} without an amount, excluded" when any. |
| PD04 | Preferences saved by older builds ("1|…") load with paydays 15 and 30; new saves use format "2|theme|week|first|second". Changing paydays updates the Dashboard without restarting. |
| AV01 | The editor shows "Amount changes each month" for a Bill or Subscription that repeats. While on, Amount is optional ("Optional: fill it in when the bill arrives."). Choosing Never or Task/Other turns it off. |
| AV02 | The domain allows a Bill/Subscription without an amount only with the flag on and a repeat; the flag requires a repeat. Undefined combinations are rejected on create, `with`, read and import. |
| AV03 | Marking a flagged due paid creates the next occurrence with no amount and the flag kept; other repeating dues still copy the amount. |
| AV04 | A flagged due without an amount shows "Amount needed" on rows, Dashboard and Details, and its reminder reads "Amount needed"; totals exclude it like other dues without an amount. |
| AV05 | Schema 8 adds `amount_varies` (0/1, default 0) to dues; existing rows keep every value. Export format 4 adds a required `amountVaries` boolean; formats 1–3 import with false; older apps refuse format 4 as newer. |

## Boundaries

- "Remind me later" (UX31), a bundled Duewise sound, full-screen or exact alarms, per-due reminder times, and reminders on paydays are not included.
- Phone, TalkBack and intended-key acceptance remain separate. Release, signing and pushing to `main` remain held.
