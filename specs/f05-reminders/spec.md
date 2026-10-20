# F05 local reminders

Authorized by "implement next f05" on 2026-10-18. [PRD](../../docs/PRD.md#f05-reminder-rules) owns policy, [SDD](../../docs/SDD.md#f05-reminder-contract) owns contracts, and [DESIGN](../../docs/DESIGN.md#f05-reminder-adaptation) owns presentation. [Plan](plan.md)/[tasks](tasks.md) track delivery. Existing local CRUD/categories remain intact; release stays held.

## Acceptance scenarios

| ID | Observable outcome |
|---|---|
| RM01 | Upgrade schema 2 to 3 preserves dues/categories/timestamps/tombstones and starts reminders disabled. No launch permission prompt or implicit opt-in. Opening Reminders does not enable delivery. |
| RM02 | Enable explicitly requests Android notification permission when required. Denied/revoked/app-blocked/channel-blocked states are visible; local CRUD works. Disable cancels pending reminders and displayed app notifications. Settings persist offline/restart; draft changes require Save. |
| RM03 | One day-before plus due-day reminder is the default; due-day-only is selectable. Time defaults to 09:00 Asia/Manila; editable time and quiet hours default 22:00-08:00. Quiet start=end disables quiet hours. Shift out of quiet hours, skip shifts after the due date and never newly schedule elapsed times. DateOnly min/max, midnight and zone boundaries are safe. Recheck the actual clock after delayed reads and before every post so a pass crossing expiry or quiet-hour start cannot use stale eligibility. |
| RM04 | Reconcile saved settings/current unpaid active dues on launch/resume, successful writes, alarm/job, reboot/unlock, app replacement and clock/zone changes. At most ten ready notifications post per pass, with remaining ready work checked no earlier than one minute later. One replaceable inexact alarm points to the next eligible queued occurrence; durable occurrence keys prevent replay on reconciliation/restart. Paid/deleted/date-edited/settings-replaced occurrences are canceled. |
| RM05 | Alarm receiver delegates bounded work to Android JobScheduler. Delivery re-reads committed dues/settings and permissions, honors quiet hours and discards obsolete/expired work. OS delays/force-stop/battery restrictions are explained; no exact delivery or automatic catch-up for newly elapsed reminders is promised. Failed scheduling/posting is retryable without undoing a saved due. |
| RM06 | Generic notifications omit due title/amount/notes. Test notification is explicit and static. Taps carry only a validated ID/key into F01 Details on Dues, safely queue across cold launch, and do not discard an active editor draft. Missing/paid/deleted/stale targets do not open another record. No arbitrary public deep-link endpoint. |
| RM07 | Reminders page is reachable from Dues without a dead tab, hides bottom tabs and returns to Dues. Enable/disable/save/retry/system settings/test actions, selected presets, accessible labels, themes and 200% text work. Full assistive/native lifecycle checks are separately recorded. |
| RM08 | Show Android-reported background restriction, Battery Saver and battery-optimization state separately from notification permission. Provide an explicit Android app/battery settings action; returning refreshes status without saving a draft. Never change power settings or request an exemption automatically, and never imply that unrestricted status guarantees delivery. |

## Completion follow-up

The subsequent "complete f05" request authorizes closing the remaining feature-verification work. Use a fresh isolated API-36 emulator for reboot, Doze, revocation, clock/zone and controlled failures without changing the user's clock or private database. Investigate phone OEM restrictions directly and restore any temporary test settings. Add RM08 to make the observed power limitation actionable. Immediate Android jobs should use expedited scheduling where supported, with normal-job fallback when quota is unavailable; periodic recovery remains inexact. Keep intended-key release and broad cross-device/assistive acceptance distinct from feature implementation and observed lifecycle checks.

## Limits

No recurrence, timed dues, cloud push, exact-alarm permission, foreground service, per-due reminder editor or account is added. Policy applies to all active unpaid dues after explicit global opt-in. Journal receipts make normal retries idempotent; OS posting and SQLite receipt are not one atomic transaction, so a crash at that boundary is not an exactly-once guarantee. Full reboot/Doze/OEM/force-stop acceptance is separate from unit/build results.
