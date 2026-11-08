# GitHub draft release automation

Authorized 2026-11-08 by “create a github workflow for the release”. Extends delivery automation under [R01/R08](../../docs/PRD.md#approved-requirements); the existing [V1.4 CI](../v1.4-ci/spec.md) remains test-only on push. [Plan](plan.md), [tasks](tasks.md), [setup guide](../../docs/delivery/GITHUB-RELEASE.md).

## Scope

A manual GitHub Actions workflow accepts a three-part version and an explicit increasing Android version code. It tests the selected source revision, builds a signed Release APK using the existing upload keystore supplied through GitHub Secrets, verifies package/version/signature and creates or updates a **draft** GitHub release with APK and SHA-256 assets. It does not publish, alter device data, generate replacement keys, rewrite history or change native product behavior. Play distribution and automated device acceptance remain excluded.

## Acceptance

| ID | Scenario | Observable outcome |
|---|---|---|
| GR01 | Run manually with valid version/build code | Selected commit is built; APK version overrides match the input. Invalid/path/shell-injection inputs fail before signing. |
| GR02 | Core, App or release-tool tests fail | Build/release stops. Existing push CI remains unchanged. |
| GR03 | Missing/invalid signing secrets | Fail clearly without exposing values; no Debug-key fallback. Temporary keystore is cleaned on success/failure. |
| GR04 | Verify produced package | Exactly one signed APK; verified signature matches supplied keystore certificate; package ID/version/code correct and non-debuggable; SHA-256 generated. Only explicit public deliverables uploaded. |
| GR05 | Create a new release | Tag targets the built commit; release stays draft with APK/checksum and traceable notes. Build token read-only; only release job has contents write. |
| GR06 | Retry or version collision | Same-commit draft may be refreshed; published release, conflicting tag or different-commit draft is refused. API/network failures fail closed. |
| GR07 | Local validation / hosted execution | Record local helper tests/lint separately from a real signed GitHub run, release assets and actual-phone acceptance. |

Defaults: version `1.0.0`, Android code `2` (current source uses code 1); maintainers must increase the code for each distributed version. Workflow dispatch requires the workflow on the default branch. Setup/secret creation, push/merge and first hosted run remain user actions.
