# UX improvements plan, round 2

Order: highest impact first, as in round 1. Findings: [UX review 2026-11-07](../../docs/delivery/UX-REVIEW-2026-11-07.md). Options: [spec](spec.md). Progress: [tasks](tasks.md). None of the items is decided yet, so each approach below describes the **recommended** option. If the user picks another option, the matching approach is rewritten before work starts.

## Ranking

The scoring uses the [round 1 method](../ux-improvements/plan.md#ranking-method): reach × frequency × severity, each 1–3. Ties go to smaller effort first, then ID order. The review stated totals only; the breakdowns below were written on 2026-11-07 while planning and add up to the same totals.

| Rank | ID | Reach | Freq. | Sev. | Impact | Effort | Why these scores |
|---|---|---|---|---|---|---|---|
| 1 | UX17 Reminder state on Dues | 2 | 2 | 3 | 12 | S | Everyone who never opens Reminders. It hurts only when they would not open the app before a date, but then a payment can be missed. |
| 2 | UX18 Overdue amount on Dashboard | 2 | 3 | 2 | 12 | S | People with overdue bills that have amounts, on every Dashboard visit; the figure misleads but the dues still show. |
| 3 | UX19 Reminders page by state | 3 | 1 | 3 | 9 | M | Every user should set reminders up, usually once; a setup that never finishes means no reminders. |
| 4 | UX20 More dues per screen | 3 | 3 | 1 | 9 | M | Every user, every visit; extra scrolling only. |
| 5 | UX21 Last export and a monthly suggestion | 3 | 1 | 3 | 9 | M | Losing a phone is rare, but without a copy the loss cannot be undone (as with UX05). |
| 6 | UX22 Ask before discarding typed input | 3 | 1 | 2 | 6 | M | Anyone adding or editing; an accidental Back is rare, and retyping is friction, not stored-data loss. |
| 7 | UX23 How late an overdue due is | 2 | 2 | 1 | 4 | S | People with overdue dues; extra information, not a blocker. |
| 8 | UX24 Calendar counts that do not look like dates | 2 | 2 | 1 | 4 | S | Calendar users; a moment of confusion. |
| 9 | UX25 Settings apply on tap | 2 | 1 | 2 | 4 | M | People who change Appearance or Start of week, rarely; tapping "Dark" and seeing nothing is confusing. |
| 10 | UX26 Plainer, fewer captions | 3 | 1 | 1 | 3 | S | Read on first use, then skipped; the space they take is counted in UX20. |
| 11 | UX27 Delete only inside the editor | 2 | 1 | 1 | 2 | — | People deleting a due, rarely. The recommendation keeps it as is. |

## Delivery waves

The waves continue the round 1 numbering. Each wave ends like the earlier ones: both test suites, Debug build, Release compile, emulator pass (light, dark, 200% text, 320 dp), updated docs and, now that this computer can update the phone, a phone check.

| Wave | Items | Why grouped |
|---|---|---|
| 6 — never miss a payment | UX17, UX18, UX19 | All three are about reminders and the amounts still owed. UX17's chip leads to the page UX19 rebuilds. |
| 7 — more dues, plainer words | UX20, UX23, UX26 | All three touch the same rows, headers and captions; doing them together reworks each row once. |
| 8 — protect data and typing | UX21, UX22, UX25 | Settings and editor behavior; UX22 and UX25 both change PRD Back/Save rules, so they share one PRD edit. |
| 9 — small | UX24 (UX27 needs no work if A is kept) | Low impact; do when convenient. |

## Item approach (recommended options)

**UX17 — Reminder state on Dues (S).** The Dues chip binds to a reminder summary. `SettingsViewModel.ReminderSummary` already produces "On · 09:00 Manila · day before" / "Off"; move that formatting into a shared helper that both pages use. Off: "Reminders off" with a bell-off glyph and an amber outline. On: "Reminders on · 09:00". The chip refreshes on appearing and after returning from Reminders. TalkBack reads "Reminders off. Opens reminder settings" (or "on"). Tests: summary text for off, on and on with day before; the list view model refreshes the summary after the reminder settings change. Docs: DESIGN Dues list (as built).

**UX18 — Overdue amount on Dashboard (S).** `DashboardViewModel` adds `OverdueTotalText` and `HasOverdueAmount`: the sum of known amounts of unpaid dues dated before today in Manila, with their count. Absent amounts do not count, and the line hides when no overdue due has an amount. The line sits under the upcoming meta line with the "!" marker and the overdue colours; tapping it selects the Overdue filter (`SelectFilter`). The upcoming total is unchanged. Tests: overdue sum at the Manila date boundary, absent amounts excluded, paid and deleted excluded, the line hidden with no overdue amounts, the filter selected on tap. Docs: PRD F02 (adds the overdue total), DESIGN dashboard (as built).

**UX19 — Reminders page by state (M).** In `ReminderSettingsPage.xaml`, wrap the content in two state groups bound to `Enabled`. Off group: status card with a one-line schedule summary, primary "Turn on reminders" (the existing Save and enable command), then a "Change schedule" section with the existing fields. On group: status card with the next reminder, the schedule fields, primary "Save schedule" enabled only when the draft differs from the stored schedule (new `HasScheduleChanges`), then secondary "Test notification" and "Turn off reminders". Both states then show an "If reminders arrive late" section with the background status, maker steps and the two Android settings buttons. "Refresh / Retry" shows only after a failure. Rewrite the status texts in plain words (see the [caption rewrites](#ux26-caption-rewrites)). The commands do not change. Tests: `HasScheduleChanges` for each field, the visibility flags per state, the texts. Docs: DESIGN F05 reminder adaptation.

**UX20 — More dues per screen (M).** Dues header: the brand row carries the reminder chip on its right, and the caption is removed (UX26). Shared ledger row (`DuesListPage`, Dashboard and Calendar results): the right column holds only the amount, and the badge moves to the start of the date line, so each row has two lines in total. At 100% text the date line stays on one line and shortens its end ("Monthly") if needed; at 200% it may wrap. Measure rows per screen on the emulator before and after, at 100% and 200% text, and record the numbers. Check that the date line wraps cleanly at 200% and 320 dp. Docs: DESIGN ledger row and Dues list (as built).

**UX21 — Last export and a monthly suggestion (M).** On a successful export, store the export time as a preference (Android Preferences, alongside the existing appearance key); failure does not store it. Settings → Your data shows "Last exported 07 Nov 2026" or "Not exported yet". The Dashboard suggestion shows when there is at least one active due, the newest `updated_at` is after the last export (or there has been none), the last export is more than 30 days old (or missing), and the suggestion was not dismissed in the last 30 days. Its "Export" opens Settings at Your data, and its close button stores the dismissal time. Tests: each condition at the 30-day boundary with fixed clocks, a failed export not counting. Docs: PRD UX05 (suggestion rule), SDD preference keys, DESIGN Settings and dashboard.

**UX22 — Ask before discarding (M).** `DueEditViewModel` gains `HasChanges`: the fields differ from the loaded due, or from a blank new due. The page intercepts the hardware back (`OnBackButtonPressed`), the app-bar back (`Shell.BackButtonBehavior`) and Cancel; when there are changes it shows a dialog, "Discard changes?", with "Keep editing" and "Discard". Save is unaffected. The deferred reminder-tap rule still applies: a tap waits until the editor is left. Tests: `HasChanges` for each field, including whitespace-only edits and the amount text; no prompt without changes. Docs: PRD workflow "Back/Cancel" row, DESIGN dialogs.

**UX23 — How late (S).** `DueListItem` gains `DaysLateText`: "1 day late" or "N days late" for Overdue only, counted in Manila calendar days. The row date line and Details show it next to the badge, and TalkBack reads it after the status. Tests: 1 day, many days, the Manila midnight boundary, nothing for other statuses. Docs: DESIGN status badges and ledger row; PRD F02 status text note.

**UX24 — Calendar count badge (S).** In `CalendarPage` `FitDayCells`/cell building, render the count as a small rounded badge (accent fill, `OnAccent` text) placed top-right in the cell's grid, instead of the text line under the number. The spoken label does not change. Check at 320 dp, at 200% text and on the selected day. Docs: DESIGN calendar grid.

**UX25 — Settings apply on tap (M).** `SettingsViewModel`: selecting an appearance or week start saves through the existing preference store and applies at once. On failure it reverts to the stored value and shows the error. Remove "Save preferences" and the draft text. Check how this interacts with returning from Reminders (no draft left to keep). Tests: each choice persists, failure reverts, appearance applies immediately. Docs: PRD F04 rules, DESIGN Settings (as built).

**UX26 — Plainer captions (S).** Apply the rewrites below; state "Amounts in PHP · Dates and times in Asia/Manila" once in Settings, near the existing "Saved on this device" line. Remove "Back to list" from Details. Update tests that assert the old texts. Docs: DESIGN voice notes and page anatomy; PRD F01 ("Edit and Back to dues").

**UX27 — Delete (recommended A: keep as is).** No work. Revisit if Delete on Details is asked for.

### UX26 caption rewrites

Proposed wording for option A. Each line can be adjusted when the user decides.

| Where | Today | Proposed |
|---|---|---|
| Dues caption | Date order · all active saved dues · amounts in PHP | Removed |
| Details caption | Saved information · Amounts in PHP · Dates use Asia/Manila | Removed |
| Details actions | Edit due, Back to list | Edit due (the app-bar arrow returns to the list) |
| Dashboard caption | Counts cover all saved dues. The total covers known unpaid amounts from today to month end. | Total: unpaid amounts from today to 30 Nov. |
| Calendar caption | Find your day. See what's on it. Dates use Asia/Manila. | Find your day and see what's on it. |
| Settings caption | A few preferences. A clearer routine. Saved on this device. | Saved on this device. Amounts in PHP; dates and times in Asia/Manila. |
| Editor amount | Bill and Subscription need an amount. Task and Other can leave it blank. · Required for Bill. · Use a decimal point, without commas or a peso symbol. Zero is allowed. · preview | Required for Bill. · preview (the format rule appears only as the error for an invalid amount) |
| Reminders status | Battery optimization is active… · Newly elapsed times are skipped; delivery is not guaranteed. | Android may delay reminders to save battery. · A reminder time that has already passed is skipped, not sent late. |

## Dependencies and order constraints

- UX26's Dashboard caption depends on the UX18 choice, and its Dues caption on UX20.
- UX23's row text depends on UX20's row layout; build them in the same wave.
- UX19 and UX25 both touch Save rules; if UX25 is A, Reminders still keeps its Save (rescheduling must be explicit).
- UX21's suggestion appears on Dashboard; check it against UX20/UX26 so the page does not gain clutter back.
- UX22 must not block the reminder-tap deferral or the storage-failure paths of the editor.

## Verification per item

As in [round 1](../ux-improvements/plan.md#verification-per-item): record the decision and update the governing text; write failing tests; implement; run Core then App tests, the Debug build and the Release compile. Then run the emulator checks (light, dark, 200% text, 320 dp, failure paths), then the phone check, and record evidence with date, artifact and observer. Finally update STATUS, tasks and evidence and validate links.
