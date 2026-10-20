# Dashboard/calendar mock execution — 2026-10-17

Observer: agent. User request: “create a mock pages for calendar dashboards.” Scope and expected outcomes are [MP01–MP05](../../../specs/future-pages/spec.md#mock-acceptance-scenarios); production F02/F03 remain deferred. Release and optional CI remain on hold.

## Environment and source

- Windows / PowerShell, .NET SDK 10.0.400. Base `c0ab21c61040c11ec170561f1f60b9753b5c9cf3` plus existing uncommitted design/core-flow documentation and design-adoption source changes; unrelated work preserved.
- Android phone M2101K6G, Android 13 / API 33. Package `com.lem.duedatechecker`. Local verification updates use `adb install -r`; no uninstall, app data clear or saved-due mutation.
- Native preview pages, shared read-only cards/progress graphic and pure fixture/state classes; routes are separate from saved dues. Sample date October 17, 2026; fixed Monday-first calendar; eight immutable samples.
- The [63-file source/test manifest](PAGE-MOCKS-2026-10-17.sha256) hashes tracked/untracked nonignored `.cs`, `.csproj`, `.xaml`, `.xml`, `.svg`, `.sql`, `.txt`, `.md` and `.ttf` files under src/tests. Text CRLF is normalized to LF; TTF bytes are unchanged. Manifest SHA-256: `07D83E35DA6CD7494163E84ED36B753FF94CB5E64CDE3E658912D99C6D461180`.

## Automated checks

| Check | Agent-observed result |
|---|---|
| App suite | 54 passed, 0 failed, including 11 preview cases. Final run has only cached NU1900 warning: NuGet vulnerability metadata unavailable. No test analyzer warning remains. |
| Core suite | 28 passed, 0 failed. |
| Preview coverage | Summary/filter agreement and notifications; Monday alignment/date counts/agenda/empty state; year/leap boundaries; DateOnly limits; fixture immutability. These tests do not load MAUI pages. |
| Initial Android Release build | Passed, 0 warnings/errors, 1m48.67s. |
| Final Android Release build | Passed, 0 warnings/errors, 1m42.94s. Includes the percentage label bound to the completion fraction rather than a duplicated literal. No application source changed after this build began. |

## Native verification

Initial APK SHA-256: `AC2920F3A1B76DC5F65CB58676347237AB3EE62E5054995FE5AA36BEE6CDFA3A`. Data-preserving install and explicit activity start succeeded; process remained running. The phone was locked, so attempting to locate the preview entry could not exercise the app. This is an environment limitation, not a passing mock-flow or launcher check. The user was asked to unlock it; no credentials were requested or entered.

Theme/font/radio settings at inspection: font scale 0.81, night mode yes, airplane mode 0, Wi-Fi setting 1. No verification setting changes have been made at this point.

Final APK SHA-256: `9ABD05C281EE91B5781B935251E7D482F7CC6189F4E77FF9D30DE36DC290F6A5`, from `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`. A second data-preserving update and explicit activity start succeeded; the app process was present. The signing identity has not been verified against an intended release key.

### Unlocked-phone follow-up

The user subsequently confirmed the phone was unlocked. Agent-observed checks resumed on the same M2101K6G / Android 13 device. Pulling the installed base APK produced the final `9ABD05…` hash above. The 63-file source/test manifest remained unchanged; no application source fix, rebuild or new test-suite run was needed for this follow-up.

Observed native behavior at 100% text in the light theme:

- The visible sample label, quarter progress ring, 2/8 completed, six open, two today and one overdue matched the fixture.
- All/Upcoming/Overdue/Completed showed 8/5/1/2 results, with the expected sample cards, including the task with no amount.
- October's Monday-first grid/dots and selected October 17 matched the two-item agenda. Selecting October 15 showed Water bill; October 18 showed the empty agenda.
- Next/Previous changed October → November → October. Today restored October 17 and its two samples.
- System Back returned Calendar → Dashboard with the Completed filter retained → Your dues. Explicit exits from both previews returned to Your dues.

Early runner attempts were interrupted to correct UTF-8 decoding and scrolling that overshot a sample title. Settings were restored before rerunning; those interrupted attempts are not passing runs and required no app changes.

The theme/text matrix subsequently passed for both pages in Light 1.0, Dark 1.0, Light 2.0 and Dark 2.0: summary/filter sample content, calendar date/agenda content, vertical scrolling and explicit exits remained reachable. Native screenshots were visually inspected for the progress ring, wrapping, seven-column date layout, dots, selection and theme contrast. Device geometry: 1080×2400; physical density 440 with existing override 352 (not changed during verification). This is one portrait phone, not compact/tablet or full accessibility acceptance.

The matrix runner stopped at the offline setup because Android rejected the protected AIRPLANE_MODE broadcast; font/theme and global radio settings were checked back at their originals. A separate runner uses `cmd connectivity airplane-mode` and `svc wifi` instead. A screenshot obscured by an incoming system notification was excluded from retained evidence; a clean replacement is captured separately. These were verification-harness issues, not application failures.

The separate offline runner passed after explicitly checking airplane mode enabled and Wi-Fi disabled: force-stop/fresh launch → Dashboard, Upcoming filter (five samples), Calendar, October 17 agenda (two samples, including Internet bill). Its finally block restored and verified the original airplane/Wi-Fi settings.

The final navigation runner passed repeated taps on Explore page previews and Calendar: one Back returned to the expected preceding page in each case, with no extra preview frame. The Calendar page's Dashboard control returned to the existing Overdue filter (one sample). The obscured Light 2.0 screenshot was replaced with a clean, visually inspected capture; capture guards check for notification overlays before and after acquisition.

Final settings were verified at font scale `0.81`, night mode `yes`, airplane mode `disabled` (global setting `0`) and Wi-Fi setting `1`. The density override was unchanged. The app was left on the Dashboard preview for review. No saved user due was created, edited, paid or deleted. Local ignored screenshots under `artifacts/page-mocks/` show sample UI; the remote temporary screenshot was removed. These targeted checks do not establish complete accessibility compliance.

## Remaining acceptance

The native flows/themes/text/offline/repeated-navigation outcomes above are observed passes. Full TalkBack, keyboard/switch access, landscape and additional screen sizes remain separate acceptance. No intended-key signing or release acceptance is established. No saved-due create/edit/payment/delete operation was performed in this follow-up. Production F02/F03 still require their own approved product rules and real-data implementation.

## Documentation verification

Agent-observed: local Markdown paths/heading anchors and whitespace diff checks passed. Source/test manifest regenerated after the final build and remained identical. Canonical PRD/SDD/DESIGN/STATUS and the existing future-page spec/plan/tasks distinguish these mock pages from production future features and preserve earlier historical evidence.
