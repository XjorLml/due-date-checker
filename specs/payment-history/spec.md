# Credit card payment history

Status: **proposed 2026-11-19; previewed in the prototype; native implementation not authorized.** The user first asked to "plan on tracking of paid dues or history of amounts paid in a particular due… for example is the credit card dues… user can input the minimum amount to pay then clicking mark as paid prompt a new modal to input how much they paid". After checking the first preview, they narrowed it: "revert back the design and make it when user clicked mark as paid a credit card a modal will appear with the minimum amount as default value and save the history especially when due is credit card and has been set as repeat. this a history of paid amount for that specific credit card due". The [PRD section](../../docs/PRD.md#payment-history-proposed-2026-11-19) records the proposal; existing rules stay in force until the user approves it. [Plan](plan.md), [tasks](tasks.md).

## Review of the current app

- **Paid is a flag only.** `Due.IsPaid` is the only payment data. There is no paid amount and no paid date, and Details says the action "records a status, not a payment".
- **Credit cards fit badly.** The amount holds the minimum due, but people often pay more, and the app cannot say what was actually paid.
- **Repeats keep no history.** Each occurrence links to the previous one (`RepeatSourceId`), but nothing reads that chain, so last month's payment cannot be seen.
- **Credit cards are already identifiable.** Both the prototype and the native app have the Philippine category `ph.credit.card` ("Credit Card Bills"), so the feature can be limited to it without a new field.
- **The PRD rules it out today:** R05 (paid/unpaid only), F01 ("writes only the payment status"), UX01 (one-tap payment), F02 ("not … payment history"), N2 (notification Mark as paid without opening the app), and UX35 (payment records, deferred).

## Decision log

| Decided by | Decision |
|---|---|
| User, 2026-11-19 | **One payment per due.** Mark as paid records a single payment and closes the due. |
| User, 2026-11-19 | **The dialog asks only for the amount.** The paid date is the day it is marked paid. |
| User, 2026-11-19 (revision) | **Credit cards only, minimum as the default.** Only a credit card due asks "How much did you pay?", prefilled with its minimum (the amount). The history is that card's paid amounts, kept especially for a repeating card. **Every other due and screen keeps the original design.** This replaces the first preview's full balance field, Minimum/Full chips, Edit amount, editor Amount paid field and paid-amount rows, which applied to all dues. |
| Proposed by Claude, not separately approved | A credit card is a due in the `ph.credit.card` category. The amount is labelled "Minimum due" on its Details. Blank records "amount not recorded". The history appears for repeating cards only, newest first, with a count, total and average and a Minimum / Above minimum / Below minimum tag. Ticking Paid in the editor records the minimum. Mark as unpaid clears the payment. Paying a repeating card adds next month's due once, with the same minimum. Notification Mark as paid records the minimum. |

## Acceptance scenarios

| ID | Outcome |
|---|---|
| PY01 | Mark as paid on a credit card's Details opens "How much did you pay?", showing the card, its minimum and due date. The amount field is prefilled with the minimum and selected. Cancel changes nothing. |
| PY02 | The amount follows the existing PHP amount rules (zero or more, centavo rounding, ₱ and thousands separators accepted). An invalid entry is refused with an error and focus, and nothing is saved. Blank saves "amount not recorded". |
| PY03 | Every due that is not a credit card keeps today's Mark paid confirmation, with no dialog and no history. Rows, totals and the editor are unchanged for all dues. |
| PY04 | A paid credit card shows "Paid ₱X on dd MMM yyyy" (or "Paid · amount not recorded") on Details, and its amount is labelled "Minimum due". |
| PY05 | A repeating credit card's Details shows **Payment history**: each paid month of that card, newest first, with the due date, amount paid, the minimum at the time, the paid date and a tag (Minimum, Above minimum, Below minimum). A summary reads "N payments · ₱total paid · average ₱X", averaging recorded amounts only. Another card's payments never appear. |
| PY06 | Paying a repeating credit card adds next month's due once, with the same minimum, and confirms "Paid ₱X. Next one added for …". |
| PY07 | Mark as unpaid on a credit card (after confirming) clears its amount and date; next month's due already added stays. In the native app, Undo restores the previous values. |
| PY08 | Ticking Paid for a credit card in the editor records the minimum as the amount paid; unticking clears it. |
| PY09 | Native only: notification Mark as paid on a credit card records the minimum, and the confirmation reads "₱X recorded". |
| PY10 | Native only: a migration adds the paid amount and paid date to existing dues with no values; earlier paid cards show "amount not recorded". Export and import carry both fields in a new format version; older files import without them. |

## Prototype preview

The [design exploration](../../prototypes/design-exploration/index.html) previews PY01–PY08:

- **Credit card sample:** a monthly "BPI Credit Card" (Credit Card Bills, minimum ₱1,500.00, due 03 Dec). Its earlier months (July to November) hold five recorded payments, kept in a separate history list so they don't crowd the Dues list.
- **Try it:** Dues → BPI Credit Card → Mark paid. Try another due, such as Water, to see the unchanged confirmation.
- **Sample data resets on reload.** The preview's paid date is the sample Today (04 Nov 2026).

PY09 and PY10 are native-only and not previewed.

## Out of scope

Payment amounts for dues other than credit cards; a full (statement) balance; several payments per due; a paid-date or reference field; editing a recorded amount (unpay and pay again instead); spending statistics, charts and budgets; a global payment history page; receipts or attachments; payment connections.
