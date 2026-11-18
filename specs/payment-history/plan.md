# Payment history plan

Scenarios: [PY01–PY12](spec.md#acceptance-scenarios).

## Prototype (done 2026-11-19)

| Area | Change |
|---|---|
| `exploration-model.js` | Pure helpers: `paymentKind`, `validFullBalance`, `seriesHistory`, `historySummary`, `payOccurrence` (paid patch plus a next occurrence once, full balance not carried), `unpay`. The seed adds the credit card and `seriesId`/`paidAmount`/`paidOn` on every due; `seedHistory()` holds the earlier paid months. |
| `ph-catalog.js` | Seed mapping for the card (`ph.credit.card`, BPI). |
| `index.html` | `#payment-dialog`, built on the catalog editor sheet: context line, chips, amount field with error, and Cancel / Mark paid. |
| `app.js` | Mark paid opens the dialog and Mark unpaid clears the payment. Details gets the paid line with Edit amount, Payment history and the "Minimum due" note. Paid rows show the amount paid. The editor gets a collapsed Full balance section and an Amount paid field shown while Paid is ticked. |
| `essential-editor.css` | Styles for the paid line, history rows, dialog and chips, using existing tokens. |
| Tests | Two new `model.test.cjs` cases; the payment journey in `essential-browser-flows.cjs`; six seed-size assertions updated for the added card. |

## Native (proposed; awaits approval)

1. **Domain:** `Due` gains `PaidAmount` (`Money?`), `PaidOn` (`DateOnly?`) and `FullBalance` (`Money?`) as trailing optional members. `Validate()` adds three rules:
   - The full balance is at least the amount, when both are present.
   - The paid amount and date are only set while paid.
   - The full balance is only allowed on Bill and Subscription.
2. **Next occurrence:** `Recurrence.NextOccurrence` clears the paid fields and the full balance, and copies the amount as today.
3. **Storage:** migration `009` adds `paid_amount_centavos`, `paid_on` and `full_balance_centavos` (all nullable). `SqliteDueRepository` maps the three columns, and `ListSeriesAsync(id)` returns a series with a recursive CTE over `repeat_source_id`, in both directions.
4. **Export:** `DueArchive` format 5 carries the three fields; versions 1–4 import with them empty.
5. **Reminders:** `ReminderText.Fingerprint` appends the new fields only when they are set, so delivered notifications stay valid. `ReminderPaymentService` records the amount due as the paid amount and the Manila date as the paid date; Undo restores the values from before.
6. **App:**
   - **Details:** a payment sheet before `TogglePaid`'s write, plus the paid line, Edit amount and Payment history.
   - **Editor:** a Full balance section and an Amount paid field.
   - **Rows:** `DueListItem` amount text for paid dues.
   - **Notification:** the confirmation text.
7. **Docs:** PRD rule changes (R05, F01, UX01, F02, N2, UX35), SDD contract, DESIGN, TEST-PLAN and an ADR for storing the payment on the due.
8. **Tests:** Core (validation, migration, series query, export 5, fingerprint) and App (sheet, prefill, chips, edit, unpay, notification default, rows).
