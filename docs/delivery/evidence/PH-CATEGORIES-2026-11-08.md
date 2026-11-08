# Native Philippine presets — 2026-11-08

Agent-observed implementation/build/emulator evidence for [PC01–PC08](../../../specs/ph-categories-native/spec.md#acceptance). User authorization: detailed Philippine categories, clarified as **preset categories only**. This extends F07 while retaining the four due types and their amount/repeat policies. No HTML, custom-category/biller feature, physical-phone data or signing configuration was changed. Existing uncommitted GitHub-release work is preserved.

## Source and artifact

Working-tree implementation on baseline `7adbe52`; this record is not a commit, release or production acceptance. Core adds the 53-preset catalog, independent `Due.CategoryId`, migration 006, repository/archive persistence, recurrence copy and notification fingerprint coverage. App adds the searchable modal picker, optional editor row, dirty snapshot and shared saved labels/icons. Canonical behavior/contracts are linked from the [feature spec](../../../specs/ph-categories-native/spec.md); test definitions live in both projects' `PhilippineCategoryTests.cs` and the existing regression suites.

Final local artifact: `artifacts/ph-categories-check/Duewise-ph-categories-release.apk` (ignored).

- SHA-256: `1c7e34e0cba9ba99e07796c498b88a4ba4e7b6ada8c50c60e58d1fc18bb224b0`.
- Package `com.lem.duedatechecker`, version 1.0/code 1, minimum API 26, target API 36.
- Release configuration, signed for local verification with the default Debug certificate, **not an upload-key production release**. `apksigner verify` passed v2/v3, one signer, certificate SHA-256 `3ae0b536ba5aae747e1815ff2f7ac843c32aaee75500e70f2320df7f5563a1c1`.
- Final packaging succeeded with **0 warnings / 0 errors** in 1m40.58s. Command: `dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release -f net10.0-android -t:PackageForAndroid -p:AndroidPackageFormats=apk -p:AndroidKeyStore=false --nologo -v:minimal`.
- The production-key `artifacts/release` APK was not overwritten. Nothing was installed on the phone or published.

## Automated checks and limits

Catalog parity checked successfully with Node/Python against `prototypes/design-exploration/ph-catalog.js`: all **53 IDs, names, group IDs/names and icon keys** match; 10 groups. The native catalog also supplies Filipino search aliases.

Both .NET test projects compile. Core reports the existing xUnit1031 warning in `RecurrenceTests`; App Release compilation reports NU1900 because vulnerability metadata could not be fetched from NuGet. Test execution/discovery attempts encountered Windows Application Control assembly-load rejection (`0x800711C7`), including the Core/App test assemblies and Core dependency. No policy was disabled or bypassed. The Release logs report no available/matching tests; that is **not a passing suite**. Current category assertions and the full regression suites must run successfully in a permitted environment. The earlier 170 Core + 304 App pass belongs to the notification baseline and does not verify this revision.

Definitions cover valid/invalid catalog IDs, Filipino aliases, type independence, schema-5 upgrade with all old fields/rowid, migration failure/newer-version preservation, CRUD/delete/restore/recurrence, format-1/2 archive compatibility/refusal, editor draft/dirty behavior, projections and category-only stale notification actions. These are coverage definitions, not execution evidence.

## Native observations

Task-owned `DuewiseNotificationsCheck`, explicitly selected `emulator-5580`, Android API 36 Google APIs x86_64. Reused the previously cleaned notification-check profile; verified no unrelated dues before migration seeding. Java SQLite inspection is emulator-only. Fixtures use the `NPH ` title prefix and contain no personal data. [Helper](../../../tools/verification/ph_categories.py), [procedure](../TEST-PLAN.md#native-philippine-preset-verification).

| Check | Observed result / attribution |
|---|---|
| Actual schema 5→6 upgrade | Passed on the initial functional APK: active zero-amount monthly Bill plus deleted NULL-amount Task preserved IDs/title/amount/date/notes/paid/timestamps/tombstone/type/repeat rule/day; both new category IDs became Other. The initial run compared these 12 prior fields, not rowid/currency/timezone/repeat-source. The reusable helper and unit fixture now compare all prior columns/rowid; those broader assertions are not claimed as executed. |
| Search / selection / type | Native picker showed 53 presets. `KURYENTE` found Electricity; selecting it kept Bill. Category-only Cancel prompted Discard changes; Keep editing retained the selection. Searching SSS then picker Cancel retained Electricity. Initial functional APK. |
| Save / restart | Native title/42.50/selected Electricity saved as type Bill, 4250 centavos and `ph.utilities.electricity`; process restart retained the selection. Initial functional APK; final APK installed in-place and retained fixtures. |
| Real export | Android create-document flow wrote format 2 with the selected ID and Other for legacy dues. All three synthetic rows included; explicit zero, absent amount and tombstone preserved. APK before the final redundant-heading removal; archive/domain/editor logic identical to final artifact. |
| Real v1/v2 import | On the same controlled database with active task dues soft-deleted, Android open-document/confirmation imported a format-1 fixture (original field set, Other for every row). Repeating with the exported format-2 file restored Electricity, explicit zero, absent amount and hidden tombstone. No direct insert substituted for import. Same functional APK as export. |
| Compact picker | Final artifact at 320dp width / 200% text in light and dark with keyboard: Electricity result and fixed Cancel reachable, heading wraps, local search retained through live theme switching. Removed the redundant search heading after an earlier check showed it crowded out results. SearchBar retains an accessible description. Task-only screenshots saved under ignored `artifacts/ph-categories-check/final-picker-*`. |
| All four amount policies | Final artifact, actual editor with Electricity selected: Bill and Subscription rejected blank amounts with type-specific errors; Task and Other each saved SQL NULL while retaining their own numeric type and the Electricity category. `ph_categories.py policies` completed successfully. |
| Edit / payment / recurrence | Final artifact: edited the categorized sample to Monthly, saved back to Details showing Electricity, then tapped Mark as paid. Database inspection confirmed the source paid and exactly one next occurrence with Electricity and 4250 centavos. This is native Details payment, not a notification action test. |
| Shared saved surfaces | Final artifact: Dues exposes Bill and Electricity in row semantics, Details shows the separate category fact, Dashboard shows Electricity on an eligible unpaid Task, and Calendar's agenda shows the categorized sample. An initial Dashboard assertion incorrectly expected the now-paid sample; corrected to an eligible unpaid row under the existing Dashboard rule, without changing product code. |

The final UI refinement changes only the search heading; earlier functional observations are attributed separately above. A system font/size configuration change recreated the activity during setup; this does not establish draft recovery across process/activity recreation. Live theme-only retention was observed. Actual TalkBack order/announcements and OS-bar contrast during live theme switching remain separate accessibility checks. Local documentation validation resolved 776 relative file targets in changed/untracked Markdown; Python helper syntax and `git diff --check` passed (existing LF/CRLF notices only).

## Remaining acceptance

Documentation follow-up, 2026-11-08: rechecked canonical documents/spec tracking against schema 006 and archive version 2. Clarified the PRD/DESIGN historical catalog deferrals so they do not contradict the authorized preset-only extension; custom categories and billers remain deferred. Corrected the PC scenario-range label and migration-list punctuation. All 697 local file targets in the ten reviewed Markdown documents resolved; `git diff --check` passed with the existing line-ending notices. Documentation-only follow-up: no builds or application tests rerun.

Cleanup observed: only `NPH ` fixtures were present; task dues and reminder occurrences removed, database empty, font 1.0/display-size override removed/night mode restored to auto, no task notifications, and both task-created Downloads backups removed. The AVD name was checked again before shutting down only this emulator. Ignored local logs/checkpoints/screenshots/test APK remain available; no phone operation occurred.

Run the Core/App regressions in an environment that permits the assemblies. Verify the final intended-key APK on the user's phone, full TalkBack traversal, additional 360–430dp/landscape layouts, interruption/recreation behavior and category-only stale notification/payment/Undo paths. Unit-defined migration failure/idempotence/all-column preservation and malformed-version-2 rejection remain unexecuted here. No timed usability claim is made. Emulator results and a successful Release build do not accept OEM behavior or a production release.
