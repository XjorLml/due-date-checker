# Payment history (amount paid per due)

Status: **proposed 2026-11-19; previewed in the prototype; native implementation not authorized.** The user asked to "plan on tracking of paid dues or history of amounts paid in a particular due… for the amount textfield user can input the minimum amount to pay then clicking mark as paid prompt a new modal to input how much they paid… review the app and provide recommendation… create a prd then update the prototype so i can check". The [PRD section](../../docs/PRD.md#payment-history-proposed-2026-11-19) records the proposal; existing rules stay in force until the user approves it. [Plan](plan.md), [tasks](tasks.md).

## Review of the current app

- **Paid is a flag only.** `Due.IsPaid` is the only payment data. There is no paid amount and no paid date, and Details says the action "records a status, not a payment". The only trace of when a due was paid is `UpdatedAt`, which any later edit overwrites.
- **Credit cards fit badly.** The amount field has to hold either the minimum due or the full statement balance, not both. Totals then show one of them as if it were what you will pay.
- **Repeats keep no history.** Each occurrence is its own row linked to the previous one (`RepeatSourceId`), but nothing reads that chain. You cannot see what you paid last month.
- **The PRD rules it out today:** R05 (paid/unpaid only), F01 ("writes only the payment status"), UX01 (one-tap payment), F02 ("not … payment history"), N2 (notification Mark as paid without opening the app), and UX35 (payment records, deferred).
- **Notification Mark as paid cannot ask a question.** It saves without opening Duewise, so it needs a default amount.

## Decision log

| Decided by | Decision |
|---|---|
| User, 2026-11-19 | **One payment per due.** Mark as paid records a single payment and closes the due; the amount can be edited later. Several payments per due were declined. |
| User, 2026-11-19 | **Optional full balance with quick chips.** A due may store an optional full (statement) balance; the payment dialog offers "Minimum", "Full balance" and "Other amount". |
| User, 2026-11-19 | **The dialog asks only for the amount.** The paid date is the day it is marked paid. A date field and reference number were declined. |
| Proposed by Claude, not separately approved | The payment is stored on the due. The dialog is prefilled with the amount due. A due with no amount and no full balance skips the dialog. Blank means "amount not recorded". The full balance must be at least the amount due, is offered for Bill and Subscription only, and is not carried to the next occurrence. Mark as unpaid clears the payment. Notification Mark as paid records the amount due. Paid rows show the amount paid. History lives on Details only, with a summary of count, total and average. Payment tags: Full balance, Minimum, Above minimum, Less than due. |

## Recommendations

1. **Store the payment on the due.** Because each due has at most one payment, a paid amount and paid date on the due are enough. A repeating bill's history is its chain of paid occurrences. No payments table is needed, and Delete, Undo and export keep working on whole dues.
2. **Prefill the dialog and offer one-tap choices.** Most payments are exactly the minimum or the full balance. Any other amount is typed by hand.
3. **Keep the full balance per statement.** It changes every month, so the next occurrence starts without one. The minimum (the amount) is still copied, as today.
4. **Make the record honest.** Leaving the amount blank records "amount not recorded" rather than inventing one, and the copy says no payment is made from the app.
5. **Leave statistics out.** Spending charts, budgets, a global history page and receipts stay out of scope, as the PRD already excludes them.

## Acceptance scenarios

| ID | Outcome |
|---|---|
| PY01 | Mark as paid on Details or in the editor (unpaid → paid) asks "How much did you pay?". The amount field is prefilled with the due's amount, or with its full balance when there is no amount. Cancel changes nothing. |
| PY02 | The dialog shows chips: "Minimum ₱X" when a full balance exists ("Amount due ₱X" otherwise), "Full balance ₱Y" when set, and "Other amount". A chip fills the field; typing deselects chips that no longer match. |
| PY03 | A due with no amount and no full balance (for example a Task) is marked paid directly, with no dialog, and its payment reads "amount not recorded". |
| PY04 | The amount paid follows the existing PHP amount rules (zero or more, centavo rounding, ₱ and thousands separators accepted). An invalid entry is refused with an error and focus, and nothing is saved. Blank saves "amount not recorded". |
| PY05 | Bill and Subscription dues have an optional **Full balance**, in a collapsed editor section after the date so title, amount and date stay first. It must be at least the amount due ("Full balance can't be less than the amount due."). When set, Details labels the amount "Minimum due · Full balance ₱Y". It is not copied to the next occurrence. |
| PY06 | A paid due shows "Paid ₱X on dd MMM yyyy" (or "Paid · amount not recorded") with **Edit amount**, which reopens the dialog prefilled with the recorded amount. Saving updates only the amount; Cancel keeps it. |
| PY07 | A repeating due's Details shows **Payment history**: each paid occurrence of the series, newest first. Each row shows the due date, amount paid, the minimum and full balance at the time, the paid date, and a tag (Full balance, Minimum, Above minimum, Less than due). Above them is a summary: "N payments · ₱total paid · average ₱X", counting recorded amounts only. A non-repeating due shows only its paid line. |
| PY08 | Marking a repeating due paid records the payment and adds the next occurrence once, as UX03 does today. The confirmation reads "Paid ₱X. Next one added for …". |
| PY09 | On Dues, Dashboard and Calendar rows, a paid due shows the amount paid, and its spoken text says "paid ₱X". Unpaid dues are unchanged. |
| PY10 | Mark as unpaid (after confirming) clears the amount and date; a next occurrence already added stays. In the native app, Undo restores the previous values. |
| PY11 | Native only: notification Mark as paid (N2) records the due's amount as paid, and the confirmation reads "₱X recorded · edit in Duewise". |
| PY12 | Native only: a migration adds the paid amount, paid date and full balance to existing dues with no values. Earlier paid dues show "amount not recorded". Export and import carry the new fields in a new format version; older files import without them. |

## Prototype preview

The [design exploration](../../prototypes/design-exploration/index.html) previews PY01–PY10:

- **Credit card sample:** a "BPI Credit Card" (minimum ₱1,500.00, full balance ₱12,480.00, due 03 Dec). Its earlier months (July to November) hold five recorded payments, kept in a separate history list so they don't crowd the Dues list.
- **Try it:** Dues → BPI Credit Card → Mark paid.
- **Sample data resets on reload.**

PY11 and PY12 are native-only and not previewed. The preview's paid date is the sample Today (04 Nov 2026).

## Out of scope

Several payments per due; a paid-date or reference field; spending statistics, charts and budgets; a global payment history page; receipts or attachments; payment connections. Release, signing and pushing to `main` remain held for native work until approval.
