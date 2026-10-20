# Design-adoption execution — 2026-10-17

Observer: agent, during the user-authorized existing-page design adoption and future-page planning task. [Spec/DA01–DA10](../../../specs/design-adoption/spec.md) defines scope. Release remains on hold. This record does not replace earlier V1.3/core-flow evidence or establish final intended-key acceptance.

## Environment and source

- Windows / PowerShell; .NET SDK 10.0.400. Source base: `c0ab21c61040c11ec170561f1f60b9753b5c9cf3` with uncommitted application/test/design changes. Earlier modified/untracked documentation was preserved.
- Phone: M2101K6G, Android 13 / API 33. Package `com.lem.duedatechecker`; local updates used `adb install -r` with no uninstall or data clear. No existing user due was edited/deleted.
- Display/icon/splash/shell adopt Duewise; project/namespace, package identity, version, SQLite path and schema remain unchanged. Four unmodified static Plus Jakarta Sans weights and upstream license are bundled locally; provenance is in the app's Raw/Fonts README.
- The [53-file source/test manifest](DESIGN-ADOPTION-2026-10-17.sha256) identifies this working tree. SHA-256 is computed over each file's bytes, normalizing CRLF to LF only for `.cs`, `.csproj`, `.xaml`, `.xml`, `.svg`, `.sql`, `.txt` and `.md`; binary files are unchanged. Manifest SHA-256: `06CCAC2CA241EA3BFCACBEE5417CBD1FF8CB09FC81F67751B4B7AB54D31290AC`.

## Automated and build evidence

| Check | Agent-observed result |
|---|---|
| New blur regression before implementation | Failed to compile because ValidateTitle/ValidateAmount/ValidateDate did not yet exist. This is the recorded red step, not a passing test. |
| App suite after validation implementation | 43 passed, 0 failed. Includes no-write/no-normalization blur validation and Save revalidation/rounding/persistence. Cached NU1900 warning: NuGet vulnerability metadata unavailable. |
| Core suite | 28 passed, 0 failed. |
| Final Android Release build, `--no-restore -v quiet` | Passed with 0 warnings and 0 errors; 1m47.10s. A local verification build, not a release. |
| APK contents | Exactly four Plus Jakarta Sans TTF files and one PlusJakartaSans-OFL.txt license present. |
| Merged manifest | Duewise display label; unchanged package; minSdk 26, targetSdk 36, allowBackup=false. |

## Artifacts and native checks

| Artifact | SHA-256 / scope |
|---|---|
| Preliminary styled build | `302A153CD288ABC625C62228E48D6433017B2CBE674A56E9A7881E39DBA41DAA`; compiled but failed device startup because the list was constructed before application resources loaded. Never treated as native acceptance. |
| Startup-corrected build | `A2270BE60F3FD8265D1F2968A6FB83B658331126FA4F058CBAD0A3521A1A94B1`; Shell resolution moved to CreateWindow after resource initialization. Both themes at 100%/200% and native required-title error passed. Offline creation/list/restart and retained maximum PHP amount passed with a task-created verification due. |
| Final native-chrome build | `40A09B29672A721D910B35A6F5A99956633F37D9BAB56E6E60C3528D2DDB24E1`; adds transparent native input underlines inside shared field borders and light status-bar canvas color. This is the final installed verification artifact. |

An early runner expected the editor to survive a system font-scale activity recreation. Settings were restored after that interruption; the corrected runner reopens the saved due after changing font scale. This does not establish unsaved-form recovery, which is outside the PRD promise. The same task-created due is reused for subsequent checks.

### Final artifact device outcomes

On the final `40A09B…` artifact, the agent observed:

- Light and dark new forms at font scales 1.0 and 2.0; fields and scrolling Save/Cancel controls remained reachable. Native underlines were absent inside field borders; the light status bar used the canvas color with readable dark system glyphs.
- Blank submit displayed the title error. TalkBack speech/focus-order acceptance was not performed.
- The task-created maximum-amount due from the preceding artifact remained listed after update/restart. The editor exposed its saved amount intact; its peso preview wrapped at 200% rather than being ellipsized. Creation is attributed to `A2270B…` above, not to this final run.
- An offline Paid edit persisted after restart at 200% text. Delete Cancel returned to the editor; subsequent confirmed deletion hid the verification due after restart.
- Existing user dues were not changed. The app was left on its list, with the task-created due soft-deleted.
- Original font scale `0.81`, night mode `yes`, airplane mode `0`, and Wi-Fi setting `1` were restored and verified in the runner's finally block.

Local ignored screenshots under `artifacts/design-adoption/` contain only blank forms or the task-created editor. They supplement these observations, not comprehensive accessibility acceptance.

## Verification limits

Full TalkBack/keyboard/switch-access testing, tablet/compact-width devices, native controlled-failure injection, repeated taps during writes, inspection of phone-private tombstones, and intended-key signing/release acceptance remain open. Screenshot inspection and reachable controls do not prove full WCAG compliance. Earlier V1.3/core-flow passes do not automatically accept this changed UI. Only completed checks are attributed above; source review and a build are not device acceptance.

## Documentation and snapshot verification

Agent-observed: 310 local Markdown path/heading-anchor checks passed across 21 changed/new documents, and `git diff --check` passed. All 53 normalized source/test manifest hashes matched after the final device checks. Pre-existing documentation work and historical evidence were preserved; no signing keys/passwords or existing user financial details were added to these records.
