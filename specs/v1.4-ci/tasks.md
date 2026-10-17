# V1.4 task tracking

See [spec scenarios](spec.md#acceptance-scenarios) and the [plan](plan.md).

## Tasks

- [x] Record decisions (user, 2026-10-17): Core + App tests, Linux runner, manual phone checks.
- [x] Add `.github/workflows/ci.yml` per the plan (2026-10-17; committed locally, not yet pushed).
- [x] Run the workflow's two `dotnet test` commands locally: 28 Core + 42 App passed on 2026-10-17 (agent-observed; Windows, .NET SDK 10.0.401). This shows the commands work, not that CI passed on Linux.
- [x] Update STATUS, TEST-PLAN, SDD, and PRD traceability to link this spec.
- [ ] Push and observe a passing GitHub run (CI01, CI04). Requires the user's go-ahead to push.
- [ ] Optional: demonstrate CI02 on a throwaway branch.

## Completion criteria

R08 is met when a push produces a passing run of the test projects (CI01). Record the run URL, commit, date, and observer in [STATUS](../../docs/delivery/STATUS.md). Local runs show that the commands work, not that CI passed.
