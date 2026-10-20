# Main-page bottom navigation — 2026-10-17

Observer: agent. User request: “create a bottom navigation for the main pages.” Scope: [BN01–BN05](../../../specs/future-pages/spec.md#bottom-navigation-acceptance). Dues uses saved data; Dashboard/Calendar remain labeled samples. Production future features, release/signing and CI activation remain deferred.

## Environment and artifact

- Windows / PowerShell, .NET SDK 10.0.400; base `c0ab21c61040c11ec170561f1f60b9753b5c9cf3` plus existing working-tree design/mock/documentation changes. Unrelated work preserved.
- M2101K6G, Android 13 / API 33; physical display 1080×2400, density 440 with existing override 352. Data-preserving `adb install -r`; no uninstall or app data clear.
- Initial Android Release APK SHA-256: `5D6D8C5EAEC8EB2B122882CA7EB125B616D3697847BC2563501EBACF450D3F3F`. Final adaptive-height APK SHA-256: `206D1F3CB8DF4D5DA1E8477C07F9231125E402B25882BBB59EB4761359A6CDA4`. Both came from `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`; the final build replaced the local file and was installed with data preserved. Intended release signing identity remains unverified.
- The initial [65-file manifest](BOTTOM-NAVIGATION-2026-10-17.sha256) has SHA-256 `B851CA7BCD803921898FBDBD67F0160DD11D5C4EBDFE89AAF8BA5040D4E8F8D2`; the final [66-file manifest](BOTTOM-NAVIGATION-FINAL-2026-10-17.sha256) has SHA-256 `6A7DCCFD88C92F81A4C20DD1C01069C9566FA35A9E4E24D3D08CB9DD603B2113`. They cover nonignored tracked/untracked source/test text plus TTF fonts. Text CRLF is normalized to LF; binary TTF bytes are unchanged.

## Automated and build evidence

| Check | Agent-observed result |
|---|---|
| Core suite, `--no-restore` | 28 passed, 0 failed. |
| App suite, `--no-restore` | 54 passed, 0 failed. Cached NU1900 warning: package vulnerability metadata unavailable. No MAUI runtime is loaded by these tests. |
| Initial Android Release build, `--no-restore -v quiet` | Passed, 0 warnings/errors, 2m41.72s. Local verification only. |
| Final Android Release build after text-scale height adaptation | Passed, 0 warnings/errors, 1m11.21s. Core/App suites rerun sequentially: 28 + 54 passed; cached NU1900 remains. |

No new tests were added to mirror XAML. Meaningful navigation/state/editor verification occurs on the device below; existing domain/view-model regressions were executed sequentially.

## Native checks

Install/update and cold start succeeded; Dues was selected. The native bar shows all three icon/label destinations above the system gesture area. Early UI inspection appeared to show clipped labels: this phone's UIAutomator hierarchy is capped at y=2272, while Android reports the app frame ending at y=2365. Preview screenshots show the complete bar; the runner uses native tab descriptions/selection and physical taps rather than treating clipped hierarchy bounds as actual layout evidence. No app layout workaround was added based on that inspection output. Full TalkBack remains unverified.

The initial-artifact outcomes below apply to `5D6D8C…`. Earlier [mock-page evidence](PAGE-MOCKS-2026-10-17.md) describes the prior push/pop layout and does not accept this changed navigation.

Observed state/navigation checks passed: startup selected Dues; Dashboard → Completed (two samples) survived Calendar switches; Calendar → November 2 survived switches and reselection. Repeated Dashboard selection followed by system Back returned directly to Dues; Calendar root Back also selected Dues. The bar was hidden in Add, and Back restored Dues. An initial editor runner stopped before saving because it searched for “Save” rather than the existing “Save due” label; its settings restoration passed. The corrected runner resumed editor/matrix checks without repeating the passed state checks.

The corrected editor run saved one uniquely named zero-amount temporary due and returned to Dues with the bar visible. A brief ADB disconnect/reconnect interrupted the next form operation and prevented automatic settings restoration; the agent restored/verified the original settings after reconnection, restarted and recovered that same saved sample. No second sample was inserted. The resumed run verified the bar hidden in edit mode, Cancel without persisting an altered title, and confirmed deletion of the temporary verification due with the bar restored on Dues. Existing user dues were not changed. These runner/connection interruptions are not application failures or passing whole-run results; the individual native outcomes are attributed here.

The initial artifact's four Light/Dark × 100%/200% cases had reachable tabs/editor exits, but visual inspection at 200% exposed captions crowding their icons in the native default height. That is a UI defect, not a passing BN04 visual result. The final artifact adds an Android Shell appearance tracker with a 64-unit minimum that grows to 84 at 200%, preserving native selection/inset handling. A fresh 200% screenshot confirmed separation without shrinking the captions. Final artifact checks are recorded separately below; the preceding create/edit/delete observations are not claimed as repeated writes on the final artifact.

### Final artifact outcomes

On `206D1F…`, the agent observed:

- Fresh launch selected Dues. The final renderer retained Completed and November across Dashboard/Calendar switches; Today reset the calendar for the appearance checks.
- Light 1.0, Dark 1.0, Light 2.0 and Dark 2.0 all passed: Dashboard/Calendar selection and sample headers, Calendar root Back selecting Dues, Add with the bar hidden, and Cancel restoring the Dues tab/bar.
- Preview screenshots showed all three icons/captions above the gesture area, selected/unselected theme roles, and separated icons/labels at 200%. The 64-unit baseline and enlarged 200% bar were visually reviewed. Capture guards excluded notification overlays; screenshots contain sample preview UI only.
- The final runner completed with original font scale `0.81` and night mode `yes` restored and verified. Radio settings were not changed by this task.

Local ignored screenshots are under `artifacts/bottom-navigation/`. This final pass performs no new saved-due mutations; the single temporary due from the initial artifact was already cleaned up. No existing user due was changed.

## Remaining acceptance

Final follow-up verification: the pulled installed APK matches `206D1F…`; all 66 source/test hashes still match the final manifest. Font `0.81`, night `yes`, airplane mode disabled and Wi-Fi setting `1` were verified after the final runner. The remote temporary screenshot was removed. Local Markdown path/anchor validation and whitespace diff checks passed; no further application source changed after the final build.

Full TalkBack, keyboard/switch access, landscape and other widths/devices remain unverified. Native targeted results do not establish full accessibility compliance, production dashboard/calendar behavior, intended-key signing or release acceptance.
