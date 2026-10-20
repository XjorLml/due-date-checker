# Core-flow verification — 2026-10-17

Observer: agent. User direction: hold release and complete/verify create, list, edit, paid/unpaid, delete, and offline persistence first (R02–R07). This record supplements historical evidence; it is not release acceptance.

## Environment and artifact

| Field | Observed value |
|---|---|
| Host | Windows, PowerShell; .NET SDK 10.0.400. |
| Source | `c0ab21c61040c11ec170561f1f60b9753b5c9cf3`; clean working tree at inspection and automated execution. No application source or test changes made for this task. |
| Source comparison | `git diff --name-only 7c22b4b HEAD -- src tests` returned no changes. |
| Phone | M2101K6G; Android 13 / API 33. |
| Installed app | `com.lem.duedatechecker`; existing installed APK used without reinstalling or clearing data. |
| Installed APK SHA-256 | `DBFEDE09F850BE50AED27C6237E3AD5A4D988D326FF333807FC3D2E78F082400`, read from the installed `base.apk`; matches the [V1.3 artifact](V1.3-2026-10-17.md#environment-and-artifact). |
| Signing | Intended signing identity not verified; no signing configuration changed. |

## Automated checks

| Command | Agent-observed result |
|---|---|
| `dotnet test tests/DueDateChecker.Core.Tests/DueDateChecker.Core.Tests.csproj --no-restore` | 28 passed, 0 failed. |
| `dotnet test tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj --no-restore` | 42 passed, 0 failed. Cached NU1900 warning: NuGet vulnerability metadata unavailable. |

The initial concurrent test-project build encountered a shared Core output-file lock; running the App command after Core completed passed. This was a runner/build-output collision, not a failing application test. No application build or publication was performed.

## Native checks

Only task-created verification dues are used. UI hierarchy data is parsed in memory, with only verification outcomes retained. Existing user dues are not edited or deleted. Native checks run with airplane mode enabled and Wi-Fi explicitly disabled; original radio settings are restored in a `finally` block.

| Procedure | Agent-observed result |
|---|---|
| TP07 partial: new-form validation | New form has no Delete action; blank title and amount display field errors. |
| TP02/TP06 partial: offline create/list/restart | Two zero-amount dues created in reverse date order; title trimming, correct dates, unpaid text, and ascending ordering verified after process termination/relaunch. One due uses a past date and optional notes. |
| TP03/TP06 partial: offline edit/restart | Edited title, date, notes, and Paid state survived restart. With `1.235` confirmed in the amount control before Save, the list showed the rounded `₱1.24` after restart. The earlier edited date moved that due before the other verification due. |
| TP03/TP06 partial: payment transitions | Paid and subsequent Unpaid states both remained visible with the correct amount/date after Save and process restart in airplane mode. |
| TP03/TP06 partial: Cancel | Changed title, amount, date, notes, and payment state, then canceled. Reopening showed every original saved value and Paid state. No canceled change was retained. |
| TP03/TP06 partial: system Back | Changed title, amount, and date, then used Android Back. Reopening showed the saved title, amount, and date unchanged. |
| TP04/TP06 partial: confirmation/Cancel/dismissal | Confirmation named the stored title rather than an unsaved replacement. Both Cancel and Android Back dismissed confirmation while preserving unsaved editor text. Canceling the editor and restarting showed the saved due still active. |
| TP04/TP06 partial: confirmed deletion/restart | Confirmed deletion of both task-created verification dues, then terminated/relaunched the app. Both remained hidden from the list. Airplane mode remained enabled through all mutation/restart checks. |
| Cleanup | Task-created dues were soft-deleted through the app; existing user dues were not changed. App left on its list. Original airplane-mode-off and Wi-Fi-on settings restored and verified. |

Automation needed corrections to the restart helper, waiting for successful return to the list before terminating the process, and handling Android's autofill suggestion popup. One unverified text-injection attempt entered `1` instead of `1.235`; the stored amount correctly reflected that input. Repeating with the full input verified in the native control produced `₱1.24`. Radio settings were restored after interrupted runs, and verification resumed using the same temporary dues. No interrupted or mismatched-input attempt is treated as passing evidence.

## Limits

The basic functional checks in this record passed at their automated and native boundaries. No application source fix was required. The [current focus](../STATUS.md#current-focus) remains completing basic features before any release; optional CI activation is deferred.

Final signed-artifact acceptance, native failure injection, repeated taps during pending writes, full theme/large-text/TalkBack checks, and phone-private tombstone inspection remain separate work. Automated SQLite retention evidence does not establish phone-database inspection. Neither the test totals nor this functional pass authorize release or complete all TP01–TP07 gates.

## Documentation verification

Agent-observed: 111 local Markdown paths/heading anchors passed across eight changed/new documents; `git diff --check` passed. Requirement IDs and historical execution results were preserved, with follow-up results explicitly attributed. Application/test source remains unchanged.
