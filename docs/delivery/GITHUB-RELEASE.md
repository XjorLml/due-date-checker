# Automatic Android releases on GitHub

Since 2026-11-18 every merge to `main` that changes the app is released automatically. The [Android release workflow](../../.github/workflows/release.yml) runs these steps:

1. Choose the next version.
2. Run the Core, App and release-tool tests.
3. Build the APK and sign it with your existing keystore.
4. Verify the signature, package and version.
5. **Publish** a GitHub release with `Duewise-v<version>.apk`, its `.apk.sha256` and a list of the changes since the previous release.

It also keeps the APK, checksum and notes as an Actions artifact for seven days. [Scope and scenarios](../../specs/github-release/spec.md#automatic-release-2026-11-18), [decision](../adr/013-automatic-release.md).

## How a change ships

1. Work on `develop` (or a feature branch). Every push and pull request runs [CI](../../.github/workflows/ci.yml): Core and App tests, release-helper tests and the documentation link check.
2. Open a pull request into `main`. CI also builds the Android app unsigned, so a change that would break the release build is caught before merging.
3. Merge. The release workflow starts on `main`.
4. When both jobs are green, the new release is under **Releases**, marked *Latest*.

**Versions:**
- Each release is the next patch after the highest existing `vX.Y.Z` tag or release (drafts count too). The first one is `v1.0.0`.
- For a minor or major bump, open **Actions → Android release → Run workflow** on `main`, enter the new version (for example `1.1.0`, no `v`), and run it. It must be newer than every existing version.
- You never type the Android build number. It is computed from the version as major×1,000,000 + minor×1,000 + patch (1.0.0 → 1000000, 1.0.1 → 1000001). It always increases, so phones accept each release as an update.

**Not every merge releases:**
- **Documentation-only merges:** changes only under `docs/`, `specs/`, `prototypes/`, `.vibe-wise/`, `tools/developer-dashboard/` or Markdown files do not start a release.
- **Reruns:** re-running the workflow for a commit that is already released does nothing. Its tag already points at that commit, so the same version is reused and found published.

## Before the first automatic release

Because merging now publishes immediately, the signing material must not be exposed. Agent-observed checks on 2026-11-18 found nothing exposed. They read the password from the ignored `local.signing.props` without printing it, and covered every local branch, reflog and leftover object, plus GitHub's `main`, `develop`, `test_branch` and pull requests 1–3:

- No commit contains the current keystore password.
- No keystore file (`.jks`/`.keystore`) has ever been committed.
- The old `local.signing.props.example` survives only in a stale local `main` branch, with `CHANGE_ME` placeholders.

Rotating the password is therefore **optional**, not a prerequisite. These checks cannot see GitHub's internal caches or copies cloned before the history cleanup. Rotate anyway if you have reused the password elsewhere or have ever copied the keystore file to a shared place. Rotating keeps the same certificate, so updates still install:

1. Back up the keystore.
2. Run `keytool -storepasswd -keystore <path>`. Also run `-keypasswd -alias upload` only for a JKS-type keystore; for PKCS12 the store password covers the key.
3. Check that `keytool -list -v` shows the same SHA-256 certificate fingerprint as before.
4. Update the ignored `local.signing.props` and the three secrets `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_PASSWORD` and `ANDROID_KEYSTORE_BASE64`. The keystore file's bytes change, so re-encode it.

Keep the keystore backed up privately and never commit it.

## One-time setup

1. The workflow must be on `main` (the default branch). Merging `develop` into `main` puts it there, and that merge is itself the first release if it changes the app.
2. In **Settings → Secrets and variables → Actions → New repository secret**, configure:

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

Paste into `ANDROID_KEYSTORE_BASE64`, then clear your clipboard. Do not paste any secret into chat, source, a workflow file or release notes. Base64 is encoding, not encryption.

3. Recommended: in **Settings → Branches**, protect `main`: require a pull request and the CI checks (`test`, `Release tool and documentation checks`, `Android build (unsigned)`) before merging. Every merge ships, so this is the only gate before users.

The workflow uses `GITHUB_TOKEN`; **no personal access token is needed**. Repository/organization policies must allow the release job's `contents: write` permission. The build job has only read access. Signing secrets are scoped to the signing steps, and temporary key and certificate files are removed even on failure. Passwords reach MSBuild through `env:` references. No `.jks`, `.keystore`, signing props, unsigned APK or DLL is attached to a release. The certificate fingerprint in the notes is public metadata, not a password or key.

## After a release

Install the published APK on your phone and run the relevant [device checks](TEST-PLAN.md#device-procedures). Publication no longer waits for them. If a release turns out broken, fix it on `develop` and merge again; the next patch replaces it as *Latest*. You can also edit the broken release on GitHub and mark it as a pre-release. The workflow never modifies a published release or moves its tag.

## Verification and troubleshooting

Local checks:

```powershell
python -m unittest discover -s tools/release/tests -v
go run github.com/rhysd/actionlint/cmd/actionlint@v1.7.12 .github/workflows/release.yml .github/workflows/ci.yml
```

These validate the helper's rules and the workflow syntax, not a hosted MAUI build. [Delivery evidence](evidence/GITHUB-RELEASE-2026-11-08.md) records the earlier local observations.

- **No release after a merge:** check that the merge changed more than documentation, that the workflow file is on `main`, and that Actions is enabled.
- **"Requested version must be newer":** a manual version must exceed every existing tag and release, drafts included.
- **"Belongs to another/unknown source":** a tag or release with that version already exists for another commit. Delete a stray draft or tag, or run manually with a newer version. The workflow will not replace it.
- **Signing failure:** confirm the base64 represents the intended key file, the alias exists, and both passwords are correct. There is no Debug-key fallback. Do not enable diagnostic logging while credentials are present.
- **Release API permission denied:** check the repository/organization workflow-token policies. Do not add a PAT just to bypass them.
- **Runner/workload restore failure:** record the failing hosted run and environment. SDK and workload patches follow .NET 10 and may change independently of this repository.

## Official references

- [Workflow triggers, path filters and manual dispatch](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows).
- [Using secrets in GitHub Actions](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets).
- [Protected branches and required status checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches).
- [.NET MAUI Android publishing and environment-backed signing passwords](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0).
- [Creating a release with GitHub CLI](https://cli.github.com/manual/gh_release_create) and [uploading assets](https://cli.github.com/manual/gh_release_upload).
