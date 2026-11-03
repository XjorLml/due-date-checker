# F04 implementation plan

1. Preserve the dirty tree and inspect authorities, startup, navigation, calendar and tests. Define PF01-PF07 and update the governing contracts.
2. Add platform-independent preference values/codec/service and a thin MAUI Preferences adapter. Store a single versioned pair; publish only successfully read/saved values, preserving last good state on failure.
3. Apply saved UserAppTheme after application resources initialize and before Shell construction. Default System maps to Unspecified; native theme resources continue responding normally.
4. Add transient Settings page/view model with draft selectors, Save, Retry and guarded Reminders navigation. Keep existing tabs/direct reminder entry and allow Settings without a working dues database.
5. Inject saved week start into Calendar; derive matching weekday headings/offset/caption, refreshing on appearance without resetting its selected date/month or agenda.
6. Run meaningful service/view-model/calendar boundary tests plus the existing Core/App suites and Android Release build. Perform available data-preserving native preference/theme/navigation/offline/restart/large-text checks; restore original preferences and device state.
7. Update canonical status/roadmap/evidence, distinguish automated/native/unperformed acceptance and validate source manifest, local links, requirement identities and unrelated-work preservation. Release stays held.

## Closeout follow-up

The 2026-11-03 "finish f04" request resumes steps 6-7. Reconcile the existing native cleanup checkpoint before repeating any state-changing procedure; compare the source manifest and local/installed artifact identities. Preserve the original checkpoint and current device choices, particularly after time has passed. Record the independent restoration audit separately from the recovered historical cleanup result in [closeout evidence](../../docs/delivery/evidence/F04-CLOSEOUT-2026-11-03.md). A new build/test cycle is required only if implementation changes or a new verification concern requires it.
