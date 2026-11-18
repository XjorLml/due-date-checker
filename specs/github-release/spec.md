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

## Automatic release (2026-11-18)

Decided by the user on 2026-11-18 ("make ci/cd with automatic release"), choosing **every merge to main** and **publish immediately** from the options offered. This supersedes the draft-only scope above: GR05's "stays draft" and GR06's same-commit draft refresh now apply only to a leftover draft, which is completed and published. Claude proposed these details while implementing; they were not separately approved: the derived Android code, skipping documentation-only merges, the manual minor/major override, the change list in the notes, and the PR checks. [ADR 013](../../docs/adr/013-automatic-release.md) records them.

| ID | Scenario | Observable outcome |
|---|---|---|
| GR08 | A merge (push) to `main` that changes app, test, tool or workflow files | The release workflow runs for that commit. Documentation-only changes (`docs/`, `specs/`, `prototypes/`, `.vibe-wise/`, developer dashboard, Markdown) do not start it. Runs never overlap. |
| GR09 | Choosing the version | The next patch after the highest `vX.Y.Z` among tags and releases (drafts included), or 1.0.0 when none exists. A manual run may name a newer version for a minor/major bump; an older or equal one is refused before signing. A non-`main` manual run does nothing. |
| GR10 | Android build number | Derived as major×1,000,000 + minor×1,000 + patch, so every later version has a larger code and 1.0.0 (1,000,000) is above the codes already used (1, 2). Minor and patch above 999 are refused. |
| GR11 | Tests and checks pass | Core, App and release-tool tests, the signed build and APK verification pass, then a **published** release (marked latest) is created at the built commit with the APK, its SHA-256 and notes listing the changes since the previous release. Any failure publishes nothing. |
| GR12 | Rerun or collision | A rerun for a commit already published reuses its version and writes nothing. A same-commit leftover draft is completed. A tag or release of that version from another commit fails closed. |
| GR13 | Pull requests | CI runs the Core and App tests, release-helper tests and the documentation link check on every push and pull request, and an unsigned Android Release build on pull requests into `main`. |
