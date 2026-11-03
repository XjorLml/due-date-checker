# F05 execution tracking

## Notification regression retest, 2026-11-04

- [x] Reproduce the user-reported Test-notification issue on the phone: post succeeds, but Reminders reload removes it with no future queue.
- [x] Preserve explicit test notifications during allowed/enabled empty-queue reconciliation; keep disabled/blocked cleanup.
- [x] Add three regression cases; Core 61 and App 158 pass (219 total). The focused regression failed before the fix.
- [x] Complete Android Release build (0 warnings/errors) and data-preserving install; the fixed test notification survives empty-queue reload and is exposed in the Android notification drawer. [Retest evidence](../../docs/delivery/evidence/F05-RETEST-2026-11-04.md).
- [x] Retest natural scheduled delivery using only a temporary due: requested 07:30, first observed at 07:33:39 in Android's inexact window; actual drawer card verified. Delete the fixture, verify the original private summary, retain opt-in/day-before and save the user's requested final 09:00/22:00-08:00 schedule. Unrelated phone settings were checked; current accessibility choices were left intact.
- [x] Record [artifact-specific evidence](../../docs/delivery/evidence/F05-RETEST-2026-11-04.md), update status/contracts and validate source hashes/links/diff while preserving concurrent unrelated work.

## Initial implementation milestone

- [x] Inspect authorities/current source and preserve a pre-F05 checkpoint.
- [x] Define RM01-RM07 and concrete reminder policy/architecture.
- [x] Implement pure planner/storage, serialized coordinator and write reconciliation.
- [x] Implement Android lifecycle/scheduling/permissions/notifications/taps and settings page.
- [x] Pass meaningful Core/App tests (61 + 113 = 174) and final Android Release build (0 warnings/errors). See [evidence](../../docs/delivery/evidence/F05-REMINDERS-2026-10-18.md).
- [x] Targeted native upgrade/settings/permission/offline forced-worker/cold-notification-tap/draft-deferral and four theme/text cases passed; task due/notifications cleaned, defaults/reminders-off and original phone/permission state restored. Natural alarm acceptance was open at this initial milestone; the follow-up below closes the observed-delivery gate.
- [x] Update canonical docs/ADR/roadmap/evidence; validate preservation, introduced local links/anchors (373 local links checked), unchanged R01-R08/migrations 001-002, matching 91-file source/test/assets manifest and working diff.

## Remaining acceptance

The initial natural-delivery and targeted lifecycle gates are closed by the separately attributed [completion evidence](../../docs/delivery/evidence/F05-COMPLETION-2026-10-18.md). Initial evidence remains unchanged.

- [ ] Broad TalkBack/spoken/switch, full cross-device/layout and complete visual acceptance. Targeted keyboard Tab/Enter, wider landscape and current Light/Dark 200% settings checks passed.
- [ ] Broader OEM/power-policy certification and controlled failure combinations beyond the observed cases. Natural delivery passed under temporary No restrictions on the phone and on the isolated API-36 emulator; default OEM restrictions can still defer delivery.
- [ ] Intended-key release acceptance. Release/signing and optional CI activation remain held.

## Completion follow-up (complete)

- [x] Inspect remaining gates and preserve a separate pre-completion checkpoint.
- [x] Add RM08 Android battery/background status and explicit app-settings access without automatic exemptions.
- [x] Add expedited immediate scheduling with same-ID normal fallback; fix clock snapshots after delayed reads and within batches. Final 61 Core + 125 App tests (186) and Android build pass (0 build warnings/errors).
- [x] Observe natural background/offline delivery on the phone under temporary No restrictions and on a fresh API-36 emulator; deep Doze delivery also observed without forced job/foreground refresh.
- [x] Real reboot/unlock rebuilt and delivered work without foreground launch; update, force-stop/reopen, Doze, permission/channel transitions, clock/zone, SQLite failure/retry and paid/delete/date cancellation passed on the isolated API-36 device.
- [x] Current settings UI passed phone Light/Dark 200%; keyboard Tab/Enter and wider landscape actions passed on API 36. Full spoken/switch/cross-device certification remains separate.
- [x] Clean task data and restore phone power/settings/permission/connectivity; isolated fixtures/defaults cleaned. Update artifact-specific evidence, source manifest, preservation/link checks and canonical state. Final-APK natural/update/force-stop/clock/expiry/revocation regression passed; release remains held.
