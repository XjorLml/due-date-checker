# Verification plan

**Template. Authority:** procedures, environments, expected outcomes, and acceptance gates. [PRD](../PRD.md) owns behavior; [STATUS](STATUS.md) owns results. Adapt these example procedures to actual requirements before treating them as gates.

## Environments and boundaries

| Boundary | What it establishes | What it cannot establish |
|---|---|---|
| Domain/unit tests | Exercised rules with controlled inputs/time | Native UI, startup/DI and device integration |
| Persistence integration | Real provider behavior, migrations and reopened data | Device filesystem/backup behavior without native tests |
| Presentation/service tests | State transitions, retry, concurrency, committed outcomes | XAML, handlers, Shell, permissions or OS lifecycle |
| Actual App target build | Compilation, generated bindings, resource packaging for that target | Runtime behavior or user acceptance |
| Signed Release device checks | Observed behavior on the recorded artifact/device | Other OS versions, devices, or unchecked scenarios |
| HTTP client tests, if an API is used | Request building, error mapping, timeout/retry decisions with a fake `HttpMessageHandler` | Real server behavior, TLS or device networking |
| API integration tests, if the app has its own API | Endpoints, authorization, validation and server data through `WebApplicationFactory` with an isolated test database | Hosting configuration, real networks or released apps |
| Contract compatibility, if an API is used | Reviewed OpenAPI diff; older client types read newer responses | Clients or versions that were not tested |
| Acceptance decision | Owner's decision against required gates | Untested functionality outside that decision |

With source-linked App tests, note that the App assembly and DI graph are not loaded. Keep compiler/package settings aligned and build the real target separately. Use isolated temporary databases, controllable clocks, and synthetic data; do not rely solely on mocks for persistence contracts.

## Scenario mapping

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary / environment |
|---|---|---|---|
| TP01 / {{R01}} | Complete the first approved workflow, then relaunch | {{Visible state and persistence outcome}} | {{Unit/integration + selected device}} |
| TP02 / {{Requirement}} | Invalid input, repeated taps, controlled operation failure, retry | {{No invalid/duplicate mutation; retained input; truthful recovery}} | {{Relevant tests + native flow}} |
| TP03 / {{Requirement}} | Refresh fails after successful load | {{Last good data remains visibly stale; retry recovers}} | {{Presentation + native}} |
| TP04 / {{DataRequirement}} | Upgrade a real old-schema fixture, reopen; fail migration part-way | {{Fields retained; atomic rollback/version behavior; no destructive reset}} | {{Provider integration + upgrade on device}} |
| TP05 / {{LifecycleRequirement}} | Navigate/Back, background/resume, terminate/restart, optional deep link | {{Contract-defined selection/draft/committed state}} | {{Native target}} |
| TP06 / {{DesignRequirement}} | Themes, maximum supported text scale, screen reader, keyboard, smallest width | {{Reachable/readable actions, correct labels/focus; no clipping}} | {{Each shipping target}} |
| TP07 / {{ReleaseRequirement}} | Verify signed package, install/update, start normally | {{Intended identity, data preservation, successful launch}} | {{Release artifact/device}} |

Remove nonapplicable rows explicitly and add domain-specific boundaries: date rollover/timezone/DST when relevant, amount precision/overflow, deterministic query ties, unsupported schema, permissions, offline transitions, and post-commit follow-up failure. Avoid asserting only that a method was called; observe state and persisted effects.

## Remote API procedures

Only when the app calls a remote API ([API](../API.md)); otherwise remove these rows. TP12 extends the TP07 Release check to network configuration.

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary / environment |
|---|---|---|---|
| TP08 / {{Requirement}} | Turn on airplane mode, or stop the API, during a load and during a save; reconnect and retry | {{Offline or error state shown; input kept; retry succeeds}} | {{HTTP client tests + native}} |
| TP09 / {{Requirement}} | Lose the response after a create is sent (timeout), then retry | {{Exactly one record exists; the app reports the real outcome}} | {{HTTP client + API integration}} |
| TP10 / {{Requirement}} | Expire or revoke the session, then act; sign in as a second user | {{Refresh or sign-in prompt; draft kept; no other user's data reachable}} | {{API integration + native}} |
| TP11 / {{Requirement}} | Run the previous released app, or its client types, against the new API | {{Previous app keeps working or shows the defined update message}} | {{Contract tests + device}} |
| TP12 / {{Requirement}} | Inspect the Release package and its network configuration | {{Production URL only; no cleartext exception, certificate bypass or secret}} | {{Release artifact}} |

`System.Text.Json` skips unknown properties by default, but rejects unknown enum names when enums travel as strings. Include a "newer server" response fixture in TP11.

## Execution and native matrix

Commands and versions: [SETUP](../SETUP.md). Native environment matrix: {{Platform, device/emulator, OS, architecture, screen size, selected artifact}}.

For each native run, record preconditions, exact artifact, expected/actual result, screenshots only when appropriate, and restoration of modified settings/fixtures. Background delivery needs actual OS scheduling observations; forced callbacks establish a narrower boundary.

## Gate definitions

| Gate | Required evidence | Decision owner |
|---|---|---|
| Feature implementation | Required scenarios implemented, reviewed against contracts | {{Owner}} |
| Automated/platform validation | Relevant tests and real target builds pass on recorded source | {{Owner}} |
| Native acceptance | Required device procedures pass for each shipping target/artifact | {{Owner}} |
| Release acceptance | Required gates closed, identity verified, limitations and decision recorded | {{Owner}} |

Optional checks must be labeled optional; do not let absent optional features block unrelated scope. Exceptions need a recorded decision, not an assumed pass.

## Documentation-only checks

Check local Markdown paths and heading anchors, authority/requirement traceability, template placeholder consistency, history preservation, and the final diff. Test copyable templates independently of their source repository. Record existing unrelated broken links separately from new defects. Application tests/builds are unnecessary when only documentation changes.
