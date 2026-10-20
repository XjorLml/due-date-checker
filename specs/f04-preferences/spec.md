# F04 settings and preferences

Authorized by "implement f04" on 2026-10-18 following the proposed preferences slice. [PRD](../../docs/PRD.md#f04-preference-rules) owns behavior; [SDD](../../docs/SDD.md#f04-preference-contract) owns storage/startup/navigation; [DESIGN](../../docs/DESIGN.md#f04-settings-adaptation) owns presentation. See [plan](plan.md) and [tasks](tasks.md).

## Scope

Add working System/Light/Dark appearance and Monday/Sunday week-start preferences with explicit Save, offline/restart persistence and access to the existing Reminders page. Default to System and Monday. Preserve dues, current calendar month/selection and reminder settings. Export, accounts, currency/timezone controls, data deletion, About/privacy content and new bottom tabs remain separate work. No schema/dependency/signing/release change.

## Acceptance scenarios

| ID | Observable outcome | Verification |
|---|---|---|
| PF01 | Missing preferences use System/Monday without a storage write. Saved values load before constructing Shell. Preferences contain no due data. | App tests; native upgrade/restart. |
| PF02 | Native labeled selectors expose only System/Light/Dark and Monday/Sunday. Changes are drafts until Save; Back discards unsaved changes. Save applies the theme immediately, stores both choices together and reports success. Returning from Reminders/resume preserves an existing draft. | App tests; native Save/Back/resume. |
| PF03 | System follows OS theme changes without restart; explicit Light/Dark overrides OS. Both choices survive process restart/offline use. | Service persistence tests; Android theme/restart checks. |
| PF04 | Calendar headers, first-date offset and caption agree with the saved week start on next appearance/refresh. Changing it preserves month/selected date, counts/agenda/order and Manila date rules. Six-week padding, leap years and DateOnly bounds remain safe for either start. | Calendar tests; native retained selection. |
| PF05 | Failed reads retain last good/default state and never overwrite stored values; retry remains available. Failed Save keeps the draft and applied preferences unchanged with a visible error. Unknown/corrupt values are reported rather than silently reset; explicit Save can replace only these UI preferences. | Controlled storage/invalid-value App tests. |
| PF06 | Settings opens from Dues even when dues storage cannot initialize; pushed Settings/Reminders hide tabs and Back returns through the same stack. Reminders access does not opt in or change its schedule. Existing direct Reminders entry remains. No dead settings rows. | Source/navigation review; Android entry/Back/reminder-off checks. |
| PF07 | Shared responsive tokens/native pickers, literal labels, save/error status and scrollable actions work in both themes and at 200% text. Record full TalkBack/keyboard/wider-device acceptance separately. | App validation tests; targeted native theme/text/layout checks. |

## State and limits

Preferences are a single versioned value in the existing platform preferences facility; financial and reminder data remain in SQLite. App-local storage/uninstall/backup limits still apply. Saving a preference is not a backup/export feature. Explicit Save refines the future DESIGN's instant-selection concept: only committed choices affect the app. Product and design authorities are updated with this scoped decision.
