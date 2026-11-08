# Implementation plan — V1.4

[Spec/scenarios](spec.md) own feature detail; [tasks](tasks.md) own execution tracking.

## Governing contracts

- [SDD verification strategy](../../docs/SDD.md#verification-strategy) and [project table](../../docs/SDD.md#4-architecture): both test projects target `net10.0` and need no MAUI runtime.
- [TEST-PLAN automated execution](../../docs/delivery/TEST-PLAN.md#automated-execution): CI reuses the same two `dotnet test` commands. The Android Release build stays local.

## Approach

One workflow file, `.github/workflows/ci.yml`, with one job:

```
push ──► GitHub ──► fresh ubuntu-latest VM
                     1. actions/checkout@v7       clone the pushed commit
                     2. actions/setup-dotnet@v6   install .NET SDK 10.0.x
                     3. dotnet test Core.Tests    restore + build + run; non-zero exit fails the job
                     4. dotnet test App.Tests     same, for the view-model harness
                  ──► pass/fail status on the commit
```

- Target the test `.csproj` files, not the solution. Building the solution would include the MAUI App, which needs the Android workload.
- `permissions: contents: read` limits the job's token to reading the repository.
- `10.0.x` follows the latest .NET 10 SDK patch. There is no `global.json`; pinning one would also change local builds and stays a separate decision.
- The actions use major-version tags of GitHub's own official actions. Pinning to commit SHAs is stricter but harder to read and update.

## Affected files

| File | Change |
|---|---|
| `.github/workflows/ci.yml` | New workflow. |
| [STATUS](../../docs/delivery/STATUS.md), [TEST-PLAN](../../docs/delivery/TEST-PLAN.md), [SDD](../../docs/SDD.md), [PRD](../../docs/PRD.md) | Replace "no CI workflow found" statements and link this spec. |

No application code, schema, migration, or package changes.

## Risks and limits

- The first run can only be observed after a push, which publishes the branch's commits.
- A floating SDK patch can change build warnings between runs. NU1900 (vulnerability-metadata) warnings do not fail the build.
- A green check proves only that the unit tests pass. It is not device or release acceptance.
