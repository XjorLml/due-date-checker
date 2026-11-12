# Development standards

**Future-project baseline.** Adopt and tailor these proposed practices; implementation and verification must be established in the new project. [SDD](SDD.md) owns actual project contracts. Each exception should have a reason and an owner.

## Essential practices

| Area | Standard and purpose | Review / verification |
|---|---|---|
| Scope and architecture | Link work to stable requirement/scenario IDs; keep platform-independent rules outside native UI code | Review changed source against PRD/SDD; record contradictions |
| MVVM | Use observable state and awaited commands; validate domain invariants independently of UI; keep code-behind for presentation adapters | Tests cover invalid input, state transitions and persistence effects |
| Binding | Use correct `x:DataType` on pages and data templates; review binding/compiler warnings | Build actual App targets and test Release runtime behavior |
| DI | Prefer constructor dependencies; register at the composition root; document lifetime/retention and disposal | Check singleton dependencies, cached pages, and per-window/draft state |
| Async | Return/await Task; reserve async void for required event adapters; handle failures at those boundaries; avoid blocking waits | Controlled failure, repeated tap, overlap, and late-result tests |
| Ownership | Give each operation an owner; define cancellation, stale-result rejection and completion-after-navigation behavior | Cancel/leave/resume scenarios; canceled committed writes retain truthful outcomes |
| UI threading | Perform bound state/collection changes on the UI thread; move blocking database/CPU work behind an explicit boundary | Responsiveness checks with realistic data; thread assertions where valuable |
| Lifecycle | Define refresh on appearance/resume, shared in-flight loads, draft retention, timers and event cleanup | Repeated navigation, background/foreground, process termination/restart |
| Navigation | Pass validated IDs/typed values; define Back/deep-link/stale-target behavior and prevent duplicate navigation | Native routing tests; distinguish commit success from return-navigation failure |
| Storage, if used | Parameterize SQL; encode dates/instants/currency deliberately; use atomic migrations and explicit ordering | Reopen-file tests, upgrade/failure tests, equal-sort-key cases |
| Remote calls, if used | Typed clients registered with `IHttpClientFactory` at the composition root; cancellation tokens from the screen's lifetime; retries only where repeating is safe; Problem Details mapped to [API](API.md) error categories | Fake-handler client tests, API integration tests and TEST-PLAN TP08–TP12 |
| Error recovery | Separate loading/empty/stale/error states; preserve useful data and drafts; allow appropriate retry | Controlled read/write/startup/navigation failures |
| Diagnostics | Log sanitized event IDs, categories, duration and recovery outcome; keep sensitive fields out | Review each logged field; test failure paths without leaking fixtures or secrets |
| Security | Use only required permissions; implement the chosen backup/retention policy; protect credentials and signing material | Inspect manifests/entitlements and packaged configuration |
| UI and locale | Follow DESIGN tokens, semantic labels, scaling and culture rules; use typed filter/status keys | Native accessibility/layout checks and localized/pseudo-localized cases |
| Verification | Combine plain .NET tests, real target builds, and scenario-based native checks | Evidence includes revision/artifact, environment, date, attribution, result |
| Documentation | Update the owning contract and link it from specs; preserve attributed history | Link/anchor checks and final diff review |

Compiled bindings improve compile-time checking; full trimming/NativeAOT require them. Review applicability for the chosen MAUI version. [Microsoft compiled-binding guidance](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings?view=net-maui-10.0).

Shell query handling through `IQueryAttributable` avoids the trimming limitation documented for `QueryPropertyAttribute`; validate input and account for its string-decoding behavior. [Microsoft Shell guidance](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-10.0).

For Microsoft.Data.Sqlite, Task-returning ADO.NET methods execute synchronously. Choose a deliberate background/serialization boundary and avoid assuming `await` moves work off the UI thread. WAL and connection policy are workload-specific choices, not substitutes for UI dispatch. [Microsoft SQLite async guidance](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async).

Window lifecycle events are the cross-platform integration points; stopped apps may not resume. Persist committed work when it happens and define background-task/timer cleanup. [Microsoft lifecycle guidance](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0).

## CI baseline

1. On proposed merges and pushes to maintained branches, run the relevant plain .NET tests. For a solo project, choose the simplest equivalent trigger policy explicitly.
2. Build every shipping App target on an appropriate host, using SETUP's pinned toolchain. Keep fast tests separate from more expensive platform jobs.
3. Preserve useful sanitized test/build artifacts. Check nullable, compiler, binding and trimming warnings; document justified exceptions rather than hiding all warnings.
4. Run link/anchor checks for documentation changes. Run native smoke checks appropriate to the release risk; a green pure-test job does not accept native behavior.
5. Separate packaging/distribution jobs from ordinary contributions. Use minimal permissions and protected signing material; untrusted contributions must not receive signing secrets.

Record an actual successful remote run before claiming CI works. Choose runner/action versions at setup time and maintain them; this kit supplies no floating-version workflow to copy blindly.

## Optional additions and triggers

| Addition | Add when | Tradeoff |
|---|---|---|
| Presentation/Infrastructure projects | Source linking or dependency boundaries become costly | More projects, clearer compiled boundaries |
| Structured telemetry/crash reporting | There is a defined support need and privacy policy | Useful diagnosis; data retention, user expectations and operations costs |
| API/authentication/sync | Approved workflows require them | Network, expiry, authorization and conflict contracts become mandatory; fill in [API](API.md) |
| Automated native UI suite | Repeated regressions justify maintenance | Device infrastructure and selectors need ownership |
| Background notifications | Product explicitly requires them | OS-specific permissions, timing limits and lifecycle tests |
| Performance/paging/caching | Measured data scale or latency exceeds an approved target | Complexity and cache invalidation; measure first |

For a small local app, a concrete service may be enough. Introduce interfaces for meaningful substitution, platform seams, or tests; avoid creating an interface for every class. Keep human-readable status labels independent from enum/ID logic so localization cannot change behavior.

## Change completion

{{Record the team's review policy and required checks.}} A completed change has its authorized behavior implemented, appropriate observed verification, updated contracts/evidence, and explicit remaining acceptance. Documentation-only changes require documentation checks, not application test runs.
