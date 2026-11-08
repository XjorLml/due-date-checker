# GitHub draft release automation — 2026-11-08

User authorized “create a github workflow for the release”. Governing scope: R01/R08 delivery extension in [PRD](../../PRD.md#approved-requirements), [spec](../../../specs/github-release/spec.md), [plan](../../../specs/github-release/plan.md), [tasks](../../../specs/github-release/tasks.md). Source baseline before edits: `7adbe52`; working tree was clean.

## Implemented

- Manual `.github/workflows/release.yml`, separate from unchanged push CI. Selected SHA, version and Android code; Core/App/tool tests before signing; .NET 10/JDK 21/MAUI Android/SDK 36.
- Four GitHub Secrets for the existing upload key. Private runner-temp decode, environment-backed password arguments, no Debug-key fallback, cleanup on failure/success, explicit public artifact allowlist.
- Read-only build token and separate contents-write draft job without signing secrets. Signature/certificate/identity/version/non-debuggable checks, APK/checksum/source notes, immutable selected SHA target, collision guards and same-source draft retry.
- Setup guide and canonical contracts updated. `.gitignore` now permits release-tool Python source under the otherwise ignored Release directory name, while retaining Python cache exclusions.

## Agent-observed local checks

Windows, Python 3.14, Go 1.26.4. No hosted run was started.

- `python -m unittest discover -s tools/release/tests -v`: **22 tests passed**. Cases include invalid/injected input, signing-secret handling/failure redaction, existing-key protection and idempotent temp-only cleanup, wrong identity/version/debug/certificate, exact signed output selection, APK/checksum/notes packaging, API failures, annotated tags, published/draft/tag collisions, source/checksum tampering and same-source draft policy. GitHub/signing tool responses in these tests are fakes; fixtures are not actual credentials.
- `go run github.com/rhysd/actionlint/cmd/actionlint@v1.7.12 .github/workflows/release.yml .github/workflows/ci.yml`: passed. Current official action versions and signing `env:` support checked against primary documentation.
- Git Bash `bash -n` accepted both multiline workflow scripts; syntax only, neither script executed.
- The actual local `artifacts/release/com.lem.duedatechecker-Signed.apk` passed the helper's metadata/certificate verification using real SDK `apksigner` and `aapt`: package `com.lem.duedatechecker`, version `1.0`, code `1`, non-debuggable, expected public v2 upload-key certificate fingerprint. This is the pre-existing locally built APK, **not** the new workflow's version `1.0.0`/code `2`, and not a Linux build.
- Application tests/builds were not rerun merely for workflow/docs changes. Their earlier attributed evidence is preserved; the new workflow will run Core/App tests itself.

## Remaining hosted/device acceptance

User must configure repository secrets, put the workflow on the default branch, push the selected source and run it. First hosted MAUI restore/build/signing, actual GitHub API/draft/assets behavior, hosted secret cleanup and intended-key phone acceptance remain pending. Version-code progression remains the maintainer's responsibility. API writes are not atomic; do not publish/edit a draft mid-run.

No commit, push, merge, GitHub secret creation, workflow dispatch, tag, release, key change, history rewrite or phone operation was performed. Existing exposed-secret/history cleanup remains separate and required before public publication.
