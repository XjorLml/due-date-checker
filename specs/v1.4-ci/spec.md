# V1.4 — Continuous integration (optional)

Status: push-test workflow implemented; local evidence dates to 2026-10-17, and passing GitHub runs were recorded 2026-11-06 in [tasks](tasks.md). Earlier default-branch placement observations remain historical. The separate [GitHub draft release extension](../github-release/spec.md) was authorized 2026-11-08; it does not replace this workflow. \
Governing requirement: [PRD R08](../../docs/PRD.md#approved-requirements), an optional delivery requirement that does not block mandatory v1 acceptance.  
Verification gate: [optional CI gate](../../docs/delivery/TEST-PLAN.md#acceptance-gates). Implementation approach: [plan](plan.md).

## Scope

On every push to any branch, a GitHub Actions workflow runs the Core and App test projects on a GitHub-hosted Linux runner and reports pass/fail on the pushed commit.

Decisions confirmed by the user on 2026-10-17:

- Run both test projects. R08 names the Core tests; the App harness is included because it compiles the real view models and dispatcher without MAUI or Android.
- Use a Linux runner (`ubuntu-latest`). The repository is public, so hosted-runner minutes are free.
- Keep phone acceptance manual ([TP01–TP07](../../docs/delivery/TEST-PLAN.md#device-procedures)). GitHub's runners cannot reach a USB-attached phone.

Excluded from this push-test workflow: Android building/signing/publication, repository secrets, self-hosted runners, automated on-device tests, branch protection and deployment. Manually building/signing a draft release is now separately authorized in the [release spec](../github-release/spec.md).

## Acceptance scenarios

| ID | Scenario | Expected observable result | Verification |
|---|---|---|---|
| CI01 | Push a commit whose tests pass. | The workflow starts automatically, both test steps succeed, and the commit shows a passing check. | Observed GitHub run. |
| CI02 | Push a commit with a failing test. | The test step exits non-zero, the job fails, and the commit shows a failing check. | Optional demonstration on a throwaway branch; follows from `dotnet test` exit-code behavior. |
| CI03 | Inspect the workflow's access. | It uses a read-only repository token and no secrets; it never needs `local.signing.props` or the keystore. | Workflow file review. |
| CI04 | Run on Linux. | Tests pass with case-sensitive paths and the IANA `Asia/Manila` zone. | Observed GitHub run. |

## Unresolved questions

- Manual signed APK/draft automation was decided 2026-11-08; its secrets design and verification are owned by the [release spec](../github-release/spec.md). Automatic tag-triggered builds/publication remain outside that manual workflow.
- Whether to require the check before merging to `main` (a GitHub branch-protection setting, which is the user's choice).
