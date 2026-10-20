# F04 preferences verification, 2026-10-18

Observer: agent-observed. User direction: "implement f04", following the discussed preferences slice. [PF01-PF07](../../../specs/f04-preferences/spec.md#acceptance-scenarios), [PRD](../../PRD.md#f04-preference-rules) and [SDD](../../SDD.md#f04-preference-contract) govern scope. Export and the broader future Settings concept remain separately deferred; signing/release/optional CI activation stay held.

## Change and environment

Added a strict versioned theme/week-start preference pair, thin MAUI Preferences adapter, startup/runtime UserAppTheme application, transient draft Settings page and existing Reminders access. Calendar derives headers/date alignment/caption from saved Monday/Sunday without resetting selected date/month. Settings is reachable without dues readiness; reminder taps defer while its draft is open. No Due/schema/migration/reminder scheduling/permission/dependency changes.

Windows, .NET SDK 10.0.400; connected M2101K6G Android 13/API 33. HEAD c0ab21c61040c11ec170561f1f60b9753b5c9cf3 plus preserved uncommitted feature/design/documentation work. A pre-F04 checkpoint records 167 existing docs/specs/src/tests/tools files. Phone original theme/font/connectivity and private saved-summary comparison hash were captured locally; no financial values are logged and no dues need to be created/changed for these checks.

## Automated and build

- **61 Core + 155 App = 216 tests passed**, 0 failed/skipped. Commands: `dotnet test tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj --no-restore`, then the equivalent Core command. Core source is unchanged. App cached NU1900 vulnerability-metadata access warning remains a network limitation, not a dependency security audit.
- Thirty new App cases cover missing/round-trip/unknown preferences, retained last-good values/read retry, write failure/no publication, invalid selections, draft Save/Back/resume semantics, and both calendar layouts across leap-year/DateOnly bounds plus unchanged selected month/date/agenda/saved due fields. All existing App regression cases passed.
- Initial Android Release build **0 warnings/errors**, **00:01:22.73**. Initial sandbox build failed writing generated metadata in the external MAUI/NuGet cache (XAAMP7019); reviewed SDK-cache access allowed the build to finish. No source workaround or dependency change was needed. Final calendar-overlap build also passed **0 warnings/errors**, **00:01:45.19**.
- Initial APK: `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`; SHA-256 **08FA72DCEB0E5B0386EA4CA6CA15240AD949FD636F21145C9A59B7B8D6938EDA**. Data-preserving `adb install -r` reported Success. This filename/build does not establish intended-key signing or release acceptance.
- Source review found that Calendar appearance could share an unfinished read before applying a newly saved week start. Final CalendarPage.RefreshAsync now calls RefreshPreferences before returning the shared task. The existing blocked-read/midnight test now also verifies Sunday layout during and after an unfinished read. The full final App suite passed **155** again; Core source/tests were unchanged, retaining the **61**-pass run above. This is a source-review boundary correction, not an observed phone failure.

## Native verification

The [verification runner](../../../tools/verification/f04_preferences.py) requires an explicitly selected serial and existing reviewed local original-state checkpoint. It never edits dues. UI XML stays in memory; only Settings screenshots are saved. Initial runner execution found Pillow unavailable before device interaction; screenshot canvas checks now use the Python standard library, with no added package dependency.

On the matching initial installed APK 08FA72...938EDA:

- Upgrade preserved the private saved-data summary hash and defaulted to System/Monday. Settings hid the three root tabs.
- Changing both selectors then pressing native Back discarded the unsaved choices. Reopening read System/Monday again.
- A Dark/Sunday draft survived Settings-to-Reminders Back and app Home/resume. Reminders stayed off. Save reported success and changed the app canvas to the dark token; the agent inspected the Settings-only screenshot and found readable/wrapping labels and reachable selectors/actions.
- While airplane mode was on with Wi-Fi/mobile data disabled, force-stop/relaunch preserved Dark/Sunday. The app canvas was dark after cold restart; no network was required.
- Calendar showed Sunday-first, then changing the saved preference to Monday updated its caption and moved the same actual date to the matching new column. Its viewed month and selected date were retained across the Settings round trip. The private saved-summary comparison still matched. Automated tests separately verify exact unchanged stored fields/count/agenda semantics; the phone summary hash is not a private-row field audit.

- With saved System, changing the phone's OS theme to Light then Dark changed the actual app canvas without restarting. Explicit Light then overrode the OS Dark setting. PNG canvas samples matched the light/dark tokens; these automated samples supplement the agent's visual inspection.
- Light/Dark at **100% and 200%** each passed native selector/Save, actual expected canvas, reachable Reminders and Back, without enabling reminders. The agent inspected both 200% Settings headers: labels/fields wrap and stay readable, with natural vertical scrolling for later content. This is targeted theme/text acceptance, not full spoken/focus/visual certification.

## Final artifact and restoration

Final APK SHA-256 **5B5153D14BC036F61580188B1414AF971FFC4D116A0F027CB0FA1408FB7B29E7**; build 0 warnings/errors. It differs from the initial native artifact only in CalendarPage's unconditional preference refresh before sharing a pending load. Settings, startup/theme/service and remaining Calendar projection sources are unchanged. Initial native results above retain their artifact attribution.

Final data-preserving replacement reported Success. The native runner recorded the installed final hash and repeated preference Save, Dark/Sunday offline restart with actual dark canvas, saved Sunday-to-Monday Calendar caption/date-position changes, retained viewed month/selected date, and matching private saved-summary baseline. These final checks ran at 100% text. The controlled unfinished-read boundary remains App-test/source evidence, not a native fault injected into the phone's private database. Cleanup/restoration is in progress.

## Limits and remaining acceptance

Unit tests use a fake preferences backend; actual Android theme/storage/navigation observations must be attributed separately. Full TalkBack/keyboard/switch, landscape/other devices, controlled native preferences-store faults, reminder-tap deferral while Settings is open and intended-key release acceptance remain separate unless explicitly observed below. Existing F01-F03/F07/F05 device results retain their recorded source/APK attribution. Android Preferences is a local convenience store, not backup/restore or guaranteed crash durability. No release is authorized.

## Source and documentation validation

The [source/test/assets/verification manifest](F04-PREFERENCES-2026-10-18.sha256) contains **104 files**, SHA-256 **6F258E83C41AAB4748EC11DB24CBBFD40B7BDB8A4708803F31781135AE2FCC3C**. Text normalizes CRLF to LF; font bytes remain binary. Build outputs, local device checkpoints/screenshots and signing secrets are excluded. All entries were rehashed against the final source; developer verification helpers do not ship in the APK.

All **167** pre-F04 docs/specs/src/tests/tools files remain. **23** preexisting changed files are confined to reviewed F04 contracts/startup/navigation/calendar/tests/roadmap and verification instructions; Core, migrations 001-003, existing edit/financial/reminder scheduling implementations and historical evidence retain checkpoint hashes. The reminder tap router change is the explicitly scoped Settings-draft guard. R01-R08 requirement and traceability rows are unchanged. Local links/anchors and Python helper syntax resolve; `git diff --check` passes with preexisting Git line-ending notices. Final link counts and restoration results are recorded after completion.
