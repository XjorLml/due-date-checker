# UX improvements 3 implementation plan

Approach for the [scenarios](spec.md#acceptance-scenarios). Contracts: [SDD](../../docs/SDD.md#ux-review-2026-11-16-contract); technical choices: [ADR 012](../../docs/adr/012-reminder-channels-and-overdue.md).

| Item | Core | App | Storage |
|---|---|---|---|
| UX28 (PR01) | — | Reminders page caption | — |
| UX29 (AL01–AL05) | `ReminderText.Kind` | `AndroidReminderPlatform` two channels and legacy deletion; `IReminderPlatform` default members; `ReminderSettingsViewModel` styles, channel commands, async test, notice; Reminders page card; resume refresh | Preference flag for the notice |
| UX30 (OD01–OD05) | `ReminderSettings.Overdue`, `ReminderPlanner.OverdueDays`, `LastDeliveryDate`, `EarliestRelevantDueDate`, overdue text | Coordinator plan window and open guard; payment guard; overdue switch | `reminder_settings.overdue` (migration 008) |
| UX32 (AV01–AV05) | `Due.AmountVaries`, validation, `Recurrence.NextOccurrence`, `ReminderText.AmountNeeded`, fingerprint | Editor switch and amount rule; `DueListItem.AmountText` | `dues.amount_varies` (migration 008), export format 4 |
| UX33 (PD01–PD04) | Reuses `Recurrence.NextDate` (TwiceMonthly) | `AppPreferences` format 2, Settings paydays, Dashboard payday figure | App preference only |

Order: Core and storage, then App, then update the existing tests whose contract changed, then new tests, then the Android build, emulator checks and docs.
