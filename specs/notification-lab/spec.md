# Notification lab

Status: **HTML proposal added 2026-11-07; N1 A and N2 A decided the same day ([decision log](#decision-log)); not implemented, so the app still sends the generic text.** The user asked on 2026-11-07: "what are the expected notification samples specifically for each dues and also a way to mark a due as paid via the push notifications. show it via the index html". The lab is the **Notifications** tab of the [design exploration](../../prototypes/design-exploration/index.html#notifications) (`notification-lab.js`, `notification-lab.css`, small hooks in `index.html` and `app.js`). Like the [UX decision lab](../ux-improvements/spec.md#html-decision-lab), it is a non-normative decision aid: choosing there records a browser-local preference, not a decision. Progress: [tasks](tasks.md).

## What the app does today

Duewise schedules **local** notifications on the phone; there is no server, so these are not push notifications, though they look the same. Per the [PRD F05 rules](../../docs/PRD.md#f05-reminder-rules), every reminder reads "Duewise reminder · Open Duewise to review an unpaid due date." with private visibility (Android hides it on the lock screen as "Contents hidden"). Tapping it opens that due's Details. There is no action button. Reminders come at the reminder time on the due date, and the day before if that setting is on; quiet hours move them, and nothing is sent once the date has passed.

## What the lab proposes

For six sample dues (Rent, Internet, Renew passport, Spotify, Water bill, Tuition) and a "two on one day" case, the lab draws each reminder the day before and on the day, on the notification shade and on the lock screen, in three privacy modes:

| Mode | Shade (phone unlocked) | Lock screen | Button |
|---|---|---|---|
| Today's app | "Duewise reminder · Open Duewise to review an unpaid due date." | "Duewise · Contents hidden" | None |
| Details when unlocked (recommended) | "Rent is due today · ₱12,000.00 · Thu 05 Nov · repeats monthly"; a Task reads "Task, no amount · Sat 07 Nov" | "Duewise · You have a due today" (or "You have 2 dues today") | Mark as paid, shade only |
| Details everywhere | as the shade | as the shade | Mark as paid, after the PIN or fingerprint |

After **Mark as paid** the notification reads "Rent marked paid · Next one added for 05 Dec 2026." (or "Saved in Duewise." for a due that does not repeat), with Undo for 10 seconds. The save would be the one Details already makes: a repeating due adds its next occurrence and its other reminders are cancelled. A due already paid, edited or deleted since the reminder is not touched; the notification then asks to open Duewise. Two dues at the same time are grouped under "Duewise · 2 dues today", each with its own button.

## Decisions offered

| ID | Decision | Options (recommended first) | Rule affected |
|---|---|---|---|
| N1 | What a reminder says | **A. Details when unlocked**, hidden on the lock screen. B. Keep today's generic text. C. Details everywhere. | PRD F05 "Notification text is generic and contains no due title, amount or notes." (A and C change it) |
| N2 | Paying from a reminder | **A. Mark as paid button** (unlock needed; Undo for 10 seconds). B. Button opens Details with Mark as paid ready. C. No button (today). | PRD F05 reminder tap opens Details (A and B add an action); SDD reminder platform (a broadcast receiver for the action) |

N1 B with N2 A is flagged in the lab: a button on a reminder that does not name its due could pay the wrong one.

## Decision log

Source: the lab export pasted by the user on 2026-11-07, with no notes.

| ID | Choice | Matches recommendation | Consequence recorded |
|---|---|---|---|
| N1 | A — Details when unlocked | Yes | PRD [N1](../../docs/PRD.md#notification-decisions-2026-11-07) replaces the F05 generic-text rule when it ships |
| N2 | A — Mark as paid button | Yes | PRD [N2](../../docs/PRD.md#notification-decisions-2026-11-07) adds the action; SDD reminder contract changes with the implementation |

## Implementation plan (not started)

1. **Text, tested first.** A pure `ReminderText` (title, text, public text) built from the due and the occurrence. It covers the day before and the day itself, no amount, repeats, and two or more dues together. It never includes notes.
2. **Posting.** `ReminderCoordinator` already holds each due when it posts; it passes the text to `IReminderPlatform.Post`. Android builds the private notification with a public version, a group with a summary for two or more, and a **Mark as paid** action. The action is an explicit, immutable `PendingIntent` to a new non-exported receiver, with `setAuthenticationRequired(true)` on Android 12 and later.
3. **Paying.** A testable `ReminderPaymentService` behind the receiver (`goAsync`). It runs the Details save through `DatabaseDispatcher` and the reminder-aware repository: it checks the due is active, unpaid and on the occurrence date, then calls `UpdateWithNextAsync`. A stale due writes nothing and gets the "Open Duewise to check this due." text.
4. **Confirm and Undo.** The notification is replaced with "… marked paid", plus the next date when one was added, and an Undo action with a 10-second timeout. Undo is a guarded unpaid write: only if the due is still paid with the same update stamp.
5. **Tests and checks.** Text rules; payment success, stale and deleted cases, one next occurrence, the Undo guard and reminder reconciliation, using fakes. Then the emulator, then the phone (Android 13 MIUI: action, unlock prompt, grouping, lock-screen public version).
6. **Documents.** SDD reminder platform contract, DESIGN notification spec, TEST-PLAN, STATUS and evidence.

## Acceptance (lab)

| ID | Scenario | Expected result |
|---|---|---|
| NL01 | Today's mode, any due. | The generic title and text, no due title or amount, no button. |
| NL02 | Details when unlocked. | Shade names the due, amount (or "no amount"), date and repeat; the lock screen says only "You have a due today"; Details everywhere shows the details on the lock screen with a button that needs unlocking. |
| NL03 | Mark as paid, then Undo. | The card becomes "… marked paid" with the next date for a repeating due, the due list shows Paid, Undo restores the reminder. |
| NL04 | Lab data and export. | Two decisions, one recommended option each; export lists N1 and N2 as "Not chosen" until chosen. |
| NL05 | Open `#notifications` or the header tab. | The lab replaces the other views and the tab is pressed. |
| NL06 | Each due and the two-on-one-day case, each mode, shade and lock screen. | Cards render; the page never scrolls sideways at 1440 px; the phone shows the day the reminder arrives. |
| NL07 | Mark as paid on the shade. | Confirmed in place with Undo; Undo restores. |
| NL08 | Mark as paid on a locked phone (Details everywhere); private and generic modes. | A simulated unlock prompt first; no button in the private lock-screen and generic modes. |
| NL09 | Choose, add a note, reload, export. | Choices and notes persist; choosing N1 sets the preview mode; the N1 B + N2 A pair warns; export carries both. |
| NL10 | 390 px; leave the lab. | No sideways page scroll; the compare and decision views still work. |

The phone drawings are a light Android 13-style illustration, not captures of the app or of a specific phone maker's skin.

## Out of scope

Snooze, "remind me later", per-due reminder times and reminders after the due date (overdue nudges) are not proposed here. Implementing either decision needs a PRD and SDD update, tests and a phone check, and stays held until the user decides.
