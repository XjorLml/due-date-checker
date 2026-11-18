# Work with this checkout

Authority: local prerequisites, first-use routes and setup recovery. This is the existing Duewise repository; for a new app use the [starter kit](templates/maui-starter/README.md). [SDD](SDD.md#3-source-boundaries) owns architecture, [TEST-PLAN](delivery/TEST-PLAN.md#automated-execution) owns test/build procedures, and [STATUS](delivery/STATUS.md) owns observed results.

## Choose the task

| Task | Prerequisites | First action / expected result |
|---|---|---|
| Read or edit docs | A text editor; Python 3.10+ for the local checker | Run `python tools/check_docs.py` from the repository root. PASS means supported local links/anchors resolve; external URLs and runtime routes are reported separately. |
| Open the existing HTML prototype | Modern browser; no package installation | Open [design exploration](../prototypes/design-exploration/index.html#a/dashboard). It is an interactive sample-data artifact, not the native app. |
| Run platform-independent tests | Compatible .NET 10 SDK and restored project dependencies | Follow [automated execution](delivery/TEST-PLAN.md#automated-execution). Tests target individual projects; they do not require an Android device. |
| Build Android | .NET 10, MAUI Android workload and compatible Android SDK/JDK | Follow the build procedure in TEST-PLAN. The [App project](../src/DueDateChecker.App/DueDateChecker.App.csproj) declares net10.0-android and minimum API 26. A successful build is not device acceptance. |
| Package/sign for a draft release | Existing signing identity and approved secure configuration | Follow [GITHUB-RELEASE](delivery/GITHUB-RELEASE.md). Verify the actual package/certificate; do not trust a Signed.apk filename. |

No global.json currently pins an SDK in this checkout. Workload-supplied MAUI versions and machine tools must be recorded when producing evidence; do not assume two machines resolve identically. New-project version selection belongs in the kit's [SETUP template](templates/maui-starter/project/docs/SETUP.md), with official compatibility review at adoption. This page does not claim a fresh restore or supported-toolchain audit.

## First checkout

1. Read [AGENTS](../AGENTS.md), inspect `git status --short`, and preserve unrelated work.
2. Use [STATUS current focus](delivery/STATUS.md#current-focus) for evidence and blockers. Historical passes do not verify changed source.
3. Choose one route above. Documentation and prototype review do not require restoring or building MAUI.
4. For .NET work, inspect `dotnet --info` and `dotnet workload list`, then use the test plan's project-specific commands. Run sequentially where outputs are shared and inspect each exit code.
5. Record actual results with source/artifact identity, environment and observer; leave unavailable native checks pending.

## Local signing and artifacts

The App conditionally imports an ignored `local.signing.props`. The older `local.signing.props.example` is absent from this checkout; old instructions to copy it cannot be followed. Obtain the existing authorized local signing configuration through its owner, or use the documented hosted secret mechanism. Do not copy credentials from history or generate a replacement identity just to get a build through.

The previous compile-only override `-p:AndroidKeyStore=false` bypasses the intended keystore configuration; it is not an upload-key or in-place-update verification. For actual packaging and distribution use the release procedure and its acceptance gates. Local build outputs live beneath the App's bin directory; controlled distribution artifacts live in ignored artifacts/release. Neither directory is a source of automatic release acceptance.

## Troubleshooting

| What happened | Next step |
|---|---|
| `python` is unavailable | Select a Python 3.10+ interpreter; on Windows `py -3` may be available instead. The checker needs no pip packages. |
| Checker reports a file/line | Follow [checker recovery instructions](../tools/README.md#documentation-checks), correct the path/anchor or restore its target, then rerun. |
| SDK/workload or target is unavailable | Compare installed tools with the declared target and the evidence-producing environment. Select a compatible authorized host; do not blindly update all tools. |
| Test assembly execution is blocked by application-control policy | Record the exact failure as blocked and follow the environment owner's approved process or use an authorized compatible host. Do not disable the policy or report compilation as a test pass. |
| APK update is rejected | Check the installed and candidate signing identities and current installation evidence in STATUS. Preserve user data; do not uninstall/clear data to simplify testing. |
| Native helper refuses a target/checkpoint | Read its [verification guide](../tools/verification/README.md). Fixture and device guards are prerequisites; do not bypass them. |

For features and change boundaries use the [feature catalog](FEATURES.md); for exact native verification use TEST-PLAN. Publishing is a separate authorized action.
