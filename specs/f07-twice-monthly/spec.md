# F07 twice-a-month repeat (TM)

Status: decided and authorized 2026-11-16 under the [PRD twice-a-month rule](../../docs/PRD.md#twice-a-month-repeat-2026-11-16); **implemented 2026-11-16** with automated, Android build and emulator evidence ([evidence](../../docs/delivery/evidence/TWICE-MONTHLY-2026-11-16.md)); phone check pending. [Plan](plan.md), [tasks](tasks.md), [ADR 011](../../docs/adr/011-twice-monthly-repeat.md).

Governing requirements: [PRD TM](../../docs/PRD.md#twice-a-month-repeat-2026-11-16), which extends [UX03](../../docs/PRD.md#ux-review-decisions-2026-11-06) and its scenarios [RC01–RC08](../f07-recurrence/spec.md#acceptance-scenarios). Technical contracts: [SDD twice-a-month contract](../../docs/SDD.md#twice-a-month-repeat-contract), building on the [F07 recurrence contract](../../docs/SDD.md#f07-recurrence-contract) and [ADR 009](../../docs/adr/009-recurring-dues.md).

## Why

Many employees in the Philippines are paid on two cutoffs a month, usually the 15th and the 30th, and pay some bills on each cutoff. Today a due can repeat only Weekly, Monthly or Yearly, so a twice-a-month bill needs two separate Monthly dues.

## Provenance

User-decided on 2026-11-16 in the chat, from the three options offered:

- The repeat is called **"Twice a month"**.
- **The user picks the two days.** The default is 15 and 30.
- The scope is **decided and authorized** for implementation.

Claude proposed the remaining rules below while writing this spec. They follow the existing UX03 rules wherever one applies. They are recorded here for the user's review and were not approved separately: Bill/Subscription only, the month-end fallback and collision rule, the first date being the date entered, carrying both days forward, no conversion of existing dues, and the exclusions.

## Acceptance scenarios

| ID | Outcome |
|---|---|
| TM01 | Upgrading the schema adds repeat code 4 (Twice a month) and a nullable second-day column (1–31). Existing rows keep their repeat, anchor day, source link, amounts and timestamps unchanged and have no second day. Initialization is idempotent, a failed upgrade leaves the previous schema, and newer schemas stay rejected. |
| TM02 | The domain accepts Twice a month only for Bill and Subscription. It needs two different days, each 1–31, stored as first day < second day. Monthly and Yearly still carry exactly one day; Never and Weekly carry none. Undefined repeat codes and invalid day pairs are rejected on create, construction, `with`, read and import. |
| TM03 | Next dates for days 15 and 30: 15 Nov 2026 → 30 Nov 2026 → 15 Dec 2026; 30 Jan 2027 → 15 Feb 2027 → 28 Feb 2027 → 15 Mar 2027 → 30 Mar 2027. A day the month does not have falls back to the month's last day (days 15 and 31: 15 Apr → 30 Apr → 15 May → 31 May). The day returns in later months: the fallback does not drift. |
| TM04 | If both days land on the same date in a short month, that month has one occurrence. With days 29 and 30, 30 Jan 2027 → 28 Feb 2027 → 29 Mar 2027. In 2028 a leap year: 30 Jan 2028 → 29 Feb 2028 → 29 Mar 2028. No next date exists beyond 31 Dec 9999. |
| TM05 | The date the user enters is the first occurrence, even if it is not one of the two days. Each next date is the earliest of the two days strictly after the current due date. With days 15 and 30: 20 Nov → 30 Nov; 01 Dec → 15 Dec; 31 Dec → 15 Jan. |
| TM06 | Marking a twice-a-month due paid, from Details, the editor or a reminder's Mark as paid ([N2](../../docs/PRD.md#notification-decisions-2026-11-07)), saves it as paid and inserts the next occurrence in one transaction. The next occurrence has a new ID and the same title, amount (or no amount), type, detailed category, notes, repeat and both days. It is unpaid, created now and linked to its source. Details announces "Marked paid. Next one added for dd MMM yyyy." Only one next occurrence is ever created per source, exactly as RC05; if the transaction fails, nothing is written, as RC08. |
| TM07 | In the editor, Repeat offers Never, Weekly, Monthly, Twice a month and Yearly, for Bill and Subscription only. Choosing Twice a month shows "First day" and "Second day" pickers (1–31), set to 15 and 30 the first time. Two equal days cannot be saved, and the editor says why. Switching to Task or Other resets Repeat to Never and clears the days. Switching between Monthly and Twice a month is allowed. Saving keeps identity and the other fields. Changing only the days counts as an unsaved change for "Discard changes?" ([UX22](../../docs/PRD.md#prototype-alignment-decisions-2026-11-07)). |
| TM08 | Rows on Dues, Dashboard and Calendar show the repeat line "Twice a month"; their accessible text says "repeats twice a month". Details shows "Repeats: Twice a month on days 15 and 30". With the phone unlocked, a reminder reads "… · repeats twice a month" ([N1](../../docs/PRD.md#notification-decisions-2026-11-07)). New occurrences count in totals, the Calendar and reminder planning like any due. |
| TM09 | Export writes both days; import reads them back. The export format version increases. Earlier exports stay readable: their dues have no second day. An older app refuses a newer file without changing data, per [UX05](../../docs/PRD.md#ux-review-decisions-2026-11-06). Import rejects a file whose day pair breaks TM02, writing nothing. |
| TM10 | Existing dues are not converted or merged: two Monthly dues on the 15th and 30th stay two Monthly dues until the user edits them. |

## Prototype preview

On 2026-11-16 the user asked to see the rule in the prototype. The [design exploration](../../prototypes/design-exploration/index.html) now offers **Twice a month** in the editor's Repeat picker, with "First day" and "Second day" pickers (default 15 and 30). It previews the next two dates using the TM03–TM05 rules, refuses equal days, and shows "Twice a month" on rows and "Twice a month on days 4 and 19" in Details. The sample Apartment rent now repeats this way. The preview is non-normative: like the prototype's other repeats it generates no next due when marked paid, and it does not limit the option to Bill and Subscription (its hint says the app does). The [PH catalog prototype](../ph-catalog-prototype/spec.md#localization-and-schedule-controls) records the extra schedule choice.

## Boundaries

- Each occurrence is an ordinary due, as in UX03: payment, Details, edit, delete, reminders and filters work as before. Editing the days changes only that occurrence. Because the next occurrence copies the one that was paid, the change carries forward.
- Option A is unchanged. Only the current occurrence exists, so the Calendar does not show the other cutoff ahead of time.
- Each occurrence carries the same amount. The user enters the amount paid per cutoff.
- Not included: every 14 days (bi-weekly), three or more days a month, different amounts for each cutoff, series editing, skipping, end dates and reminders per series.
- Release, signing and pushing to `main` remain held.
