# Build a signed APK and draft GitHub release

The [Android draft release workflow](../../.github/workflows/release.yml) runs manually from GitHub's website. It tests the selected commit, signs the APK with your existing keystore and attaches `Duewise-v<version>.apk` plus its `.apk.sha256` to an unpublished release. It also retains the APK/checksum/notes as an Actions artifact for seven days. [Scope and scenarios](../../specs/github-release/spec.md).

## One-time setup

1. Complete the exposed signing-password cleanup/rotation and repository history cleanup before making a public release. Replacing the tracked example with placeholders does not erase old commits. Keep the actual keystore backed up privately; do not create another signing identity just to use this workflow.
2. Commit/push the workflow, `tools/release/` and its documentation. The workflow must exist on the repository's **default branch** for GitHub's Run workflow button. That does not require changing the app's release source to `main`: choose the desired branch, such as `develop`, when running it. Keep the workflow/helper versions on that branch too. No branch merge/push was performed by the agent.
3. In **Settings → Secrets and variables → Actions → New repository secret**, configure:

| Secret | Value |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | Base64 contents of your existing upload `.jks`/`.keystore`, not the file path. |
| `ANDROID_KEY_ALIAS` | Alias inside that keystore; your current setup uses `upload`. |
| `ANDROID_KEYSTORE_PASSWORD` | Keystore/store password. |
| `ANDROID_KEY_PASSWORD` | Private-key password; may differ from the store password. |

To copy the keystore's base64 directly to the clipboard in PowerShell, substitute the path to your existing key:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\path\to\your-existing-upload-key.jks')) | Set-Clipboard
```

Paste into `ANDROID_KEYSTORE_BASE64`, then clear your clipboard. Do not paste any secret into chat, source, a workflow file or release notes. Base64 is encoding, not encryption. If a secret exceeds GitHub's size limit, stop and review a supported encrypted-file approach instead of committing the key.

The workflow uses `GITHUB_TOKEN` for repository access; **no personal access token is needed**. Repository/organization policies must allow the release job's `contents: write` permission. The build job has only read access; signing secrets are scoped to signing steps, and temporary key/certificate files are cleaned even on failure. Passwords are passed to MSBuild using `env:` references, supported for this APK-only build. No local signing file is uploaded or read by hosted runners.

## Run from the website

1. Open **Actions → Android draft release → Run workflow**.
2. Select the branch/tag containing the source you want to build, usually `develop`.
3. Enter a three-part version without `v`, for example `1.0.0`, and an Android build number, initially `2` (the current local APK uses `1`). **Increase the number for every new distributed version**, including builds produced locally. The workflow validates range/format, not the build numbers installed on people's phones.
4. Run it. Tests must pass before signing. The build uses .NET 10, JDK 21, the MAUI Android workload and Android SDK/build-tools 36. It overrides display version and Android code for this build without editing the project file.
5. After both jobs succeed, open **Releases** and review the draft. Its notes identify the exact commit, signing certificate and APK checksum. Download/install the APK for remaining phone acceptance, using the signing identity compatible with the installed app.
6. Publish through GitHub only when review/device acceptance and secret cleanup are complete. The workflow has no automatic publication step. Avoid editing or publishing the draft while the workflow is running; GitHub's release/asset API calls are separate operations.

For `1.0.0`, the tag is `v1.0.0`. A new draft targets the exact selected commit rather than whatever `main` contains. A rerun of the same source may refresh its own draft assets. A published release, an existing tag pointing to another commit, or a draft without the matching source marker is refused. Use **Re-run jobs** to retry the same revision after correcting secrets or a transient failure. For a changed source revision, choose a new version/build number. Preserve the source marker in generated draft notes if you want automated retries.

No `.jks`, `.keystore`, signing props, intermediate unsigned APK or DLL is attached. The certificate fingerprint is public metadata and appears in the notes; it is not a password or private key.

## Verification and troubleshooting

Local checks:

```powershell
python -m unittest discover -s tools/release/tests -v
go run github.com/rhysd/actionlint/cmd/actionlint@v1.7.12 .github/workflows/release.yml .github/workflows/ci.yml
```

These validate helper policy and workflow syntax, not a hosted MAUI build. [Delivery evidence](evidence/GITHUB-RELEASE-2026-11-08.md) distinguishes local observations, the pending first GitHub run and phone acceptance.

- **No Run workflow button:** ensure the workflow is on the default branch and Actions is enabled. Choosing a branch alone does not install a workflow onto the default branch.
- **Signing failure:** confirm base64 represents the intended key file, alias exists, and both passwords are correct. There is no Debug-key fallback. Do not enable diagnostic logging with credentials present.
- **Version collision:** select a new version for changed code; this workflow will not replace a published release or move its tag.
- **Release API permission denied:** check repository/organization workflow-token policies. No PAT should be added merely to bypass those policies.
- **Runner/workload restore failure:** record the failing hosted run and environment. SDK/workload patches follow .NET 10 and may change independently of this repository.

## Official references

- [Manual workflow dispatch and default-branch requirement](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#workflow_dispatch).
- [Using secrets in GitHub Actions](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets).
- [.NET MAUI Android publishing and environment-backed signing passwords](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0).
- [Creating a draft release with GitHub CLI](https://cli.github.com/manual/gh_release_create) and [uploading assets](https://cli.github.com/manual/gh_release_upload).
