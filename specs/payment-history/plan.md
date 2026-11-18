# Credit card payment history plan

Scenarios: [PY01–PY10](spec.md#acceptance-scenarios).

## Prototype (revised 2026-11-19)

The first preview's changes to all dues were reverted to the design before the feature (commit `2cbdb9b`), then the credit card version was added:

| Area | Change |
|---|---|
| `exploration-model.js` | Pure helpers: `isCreditCard` (category `ph.credit.card`), `paymentKind` (against the minimum), `seriesHistory`, `historySummary`, `payOccurrence` (paid patch plus next month once), `unpay`. The seed adds the monthly BPI credit card; `seedHistory()` holds its five earlier paid months. |
| `ph-catalog.js` | Seed mapping for the card (`ph.credit.card`, BPI). |
| `index.html` | `#payment-dialog`, built on the catalog editor sheet: context line, amount field with hint and error, and Cancel / Mark paid. |
| `app.js` | Mark paid on a credit card opens the dialog; other dues keep the confirmation. A credit card's Details gets "Minimum due", the paid line and, when repeating, Payment history. Ticking Paid in the editor records the minimum; unpaying clears the payment. |
| `essential-editor.css` | Styles for the paid line, history rows and dialog, using existing tokens. |
| Tests | Two `model.test.cjs` cases and the credit card journey in `essential-browser-flows.cjs`. Six seed-size assertions count the added card. |

## Native (proposed; awaits approval)

1. **Domain:** `Due` gains `PaidAmount` (`Money?`) and `PaidOn` (`DateOnly?`) as trailing optional members. `Validate()` allows them only while paid and only for `CategoryId == "ph.credit.card"`.
2. **Next occurrence:** `Recurrence.NextOccurrence` clears both fields and copies the amount as today.
3. **Storage:** migration `009` adds nullable `paid_amount_centavos` and `paid_on`. `SqliteDueRepository` maps them, and `ListSeriesAsync(id)` returns a series with a recursive CTE over `repeat_source_id`, in both directions.
4. **Export:** `DueArchive` format 5 carries both fields; versions 1–4 import with them empty.
5. **Reminders:** `ReminderText.Fingerprint` appends the fields only when they are set, so delivered notifications stay valid. `ReminderPaymentService` records the minimum and the Manila date for a credit card; Undo restores the values from before.
6. **App:** on Details, a payment sheet before `TogglePaid`'s write for credit cards, the "Minimum due" label, the paid line and Payment history. The editor records the minimum when Paid is ticked. Plus the notification confirmation text.
7. **Docs:** PRD rule changes (R05, F01, UX01, F02, N2, UX35), SDD contract, DESIGN, TEST-PLAN and an ADR for storing the payment on the due.
8. **Tests:** Core (validation, migration, series query, export 5, fingerprint) and App (sheet, default, other dues unchanged, unpay, notification default).
