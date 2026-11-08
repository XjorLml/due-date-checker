# GitHub draft release plan

Contracts and acceptance: [spec](spec.md), [SDD](../../docs/SDD.md#github-release-automation). Keep the existing CI workflow separate.

1. `.github/workflows/release.yml`: manual dispatch; immutable selected SHA; sequential Core/App tests; .NET 10/JDK 21/Android workload; signing through environment-backed MSBuild password references. Build on hosted Ubuntu with a read-only token. Decode the keystore under runner temp and delete it in an always-run cleanup.
2. `tools/release/release.py`: validate version/code, decode signing material without logging it, verify signed output metadata/certificate, prepare APK/checksum/notes and guard draft release updates. Test failure and collision behavior with temporary fixtures and fake command results, without credentials/network.
3. Verify with actionlint and Python standard-library tests. Exercise packaging validation against the existing local signed APK where possible; do not create or publish an external release to validate the workflow.
4. Update PRD delivery scope, SDD, TEST-PLAN, STATUS and docs index; add setup instructions for four repository secrets and manual Actions usage. Record the first hosted run separately when configured/authorized.

Only the release job gets `contents: write`; it receives public build artifacts and no signing secrets. Its checkout is fixed to the built SHA. Concurrent runs of the same version queue. Updates affect drafts only, and compare both tag commit and recorded draft source SHA before replacing assets. Existing published releases are never modified.
