# F05 implementation plan

1. Preserve the working tree, resolve date-only default/preset/quiet/privacy/late-delivery rules in PRD and record ADR 007 against its historical proposed identity.
2. Add pure settings/planner/occurrence contracts and schema 003 settings/journal tables without changing dues. Implement parameterized SQLite storage with atomic pending replacement, preserving delivered receipts.
3. Add a serialized coordinator using the existing DatabaseDispatcher, injected clock/platform/store/repository. Re-read current records at delivery, replace one alarm and clean obsolete notifications. Decorate successful repository writes to reconcile without reclassifying a committed CRUD write as failure.
4. Add Android channel/permission/status, inexact alarm, private receiver, JobScheduler worker, generic notification/test and validated tap transport. Reconcile startup/resume/system events; protect active drafts during taps.
5. Add Reminders page/view model, Dues entry, explicit opt-in, draft settings Save, retry/system settings and test actions. No permission prompt on launch.
6. Add meaningful planner/storage/coordinator/view-model tests; run Core/App sequentially and Android Release build. Perform available phone checks with task-only records, restore settings/reminder opt-in and clean artifacts/notifications. Update canonical docs/status/tasks/evidence and validate preservation/links/diff; distinguish unperformed device acceptance.


## Completion follow-up

7. Surface Android background/battery readiness and explicit app-settings access (RM08). Use API-31 expedited immediate jobs with same-ID normal fallback for quota rejection; retain inexact alarms/periodic recovery and no automatic exemption/exact-alarm request.
8. Use an isolated fresh API-36 AVD with workspace-owned data for natural alarm, reboot/update, Doze, clock/zone, revocation, cancellation/replay and recoverable database-failure checks. Inspect physical-phone app power settings; any temporary test policy change must be restored. Attribute all results by environment/artifact and preserve the initial F05 evidence unchanged.
9. Complete relevant automated/native regression, documentation and source manifests. Separate feature acceptance under tested conditions from outstanding broad assistive/OEM/device coverage and held intended-key release acceptance.
