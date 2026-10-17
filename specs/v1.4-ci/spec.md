# V1.4 — Continuous integration (optional)

Status: Workflow implemented and its commands verified locally on 2026-10-17; the first GitHub run is pending a push. Task tracking is in [tasks](tasks.md).  
Governing requirement: [PRD R08](../../docs/PRD.md#approved-requirements), an optional delivery requirement that does not block mandatory v1 acceptance.  
Verification gate: [optional CI gate](../../docs/delivery/TEST-PLAN.md#acceptance-gates). Implementation approach: [plan](plan.md).

## Scope

On every push to any branch, a GitHub Actions workflow runs the Core and App test projects on a GitHub-hosted Linux runner and reports pass/fail on the pushed commit.

Decisions confirmed by the user on 2026-10-17:

- Run both test projects. R08 names the Core tests; the App harness is included because it compiles the real view models and dispatcher without MAUI or Android.
- Use a Linux runner (`ubuntu-latest`). The repository is public, so hosted-runner minutes are free.
- Keep phone acceptance manual ([TP01–TP07](../../docs/delivery/TEST-PLAN.md#device-procedures)). GitHub's runners cannot reach a USB-attached phone.

Excluded: building, signing, or publishing the Android APK in CI; repository secrets; self-hosted runners; automated on-device tests; branch protection; deployment.

## Acceptance scenarios

| ID | Scenario | Expected observable result | Verification |
|---|---|---|---|
| CI01 | Push a commit whose tests pass. | The workflow starts automatically, both test steps succeed, and the commit shows a passing check. | Observed GitHub run. |
| CI02 | Push a commit with a failing test. | The test step exits non-zero, the job fails, and the commit shows a failing check. | Optional demonstration on a throwaway branch; follows from `dotnet test` exit-code behavior. |
| CI03 | Inspect the workflow's access. | It uses a read-only repository token and no secrets; it never needs `local.signing.props` or the keystore. | Workflow file review. |
| CI04 | Run on Linux. | Tests pass with case-sensitive paths and the IANA `Asia/Manila` zone. | Observed GitHub run. |

## Unresolved questions

- Whether to add an APK build on tag or manual trigger (suggested in the historical [PLAN](../../docs/PLAN.md#gaps-found-in-the-original-plan-what-to-keep-in-mind)). It needs its own decision, and signing in CI would need a secrets design.
- Whether to require the check before merging to `main` (a GitHub branch-protection setting, which is the user's choice).
