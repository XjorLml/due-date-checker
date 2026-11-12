# {{ProjectName}} software design

**Template. Authority:** architecture and technical contracts. [PRD](PRD.md) owns behavior. [SETUP](SETUP.md) owns chosen tool versions; [DEVELOPMENT](DEVELOPMENT.md) owns coding practices. Status: {{ProposedOrAsBuilt}}. Reviewed: {{DateAndSourceRevision}}.

## Architecture and dependencies

{{Describe App -> Core, folder responsibilities, and any justified optional projects. Distinguish current source from proposed changes. Add a small dependency diagram if useful.}}

| Component | Responsibility | Allowed dependencies | Source |
|---|---|---|---|
| Domain | {{Rules and contracts}} | {{Platform-independent types}} | {{Path}} |
| Data, if needed | {{Provider and migration implementation}} | {{Domain, selected provider}} | {{Path}} |
| Presentation | {{View-model state and orchestration}} | {{Domain/app contracts}} | {{Path}} |
| MAUI/native adapters | {{Controls, navigation, platform capabilities}} | {{Presentation/Core and target APIs}} | {{Path}} |

## Platform capability matrix

List only selected or explicitly deferred platforms. Keep capability differences visible rather than reporting one platform's success as universal support. Exact SDK/native-tool versions belong in SETUP.

| Platform | TFM / minimum OS | Needed capabilities and implementation | Unavailable behavior | Build/device evidence |
|---|---|---|---|---|
| {{SelectedPlatform}} | {{TFMAndMinimum}} | {{StorageNotificationsFilesEtc}} | {{FallbackOrUnsupportedState}} | {{STATUS link; initially pending}} |

{{Record registration boundaries, manifests/entitlements, permission rationale, desktop input/window requirements, and which native adapters must exist before enabling another TFM.}}

## Data and persistence contracts

| Contract / requirement | Definition |
|---|---|
| Identity and mutation | {{Create/update/delete semantics; identity stability; idempotency/conflict policy}} |
| Dates, instants, money | {{Storage representations and conversion boundaries implementing PRD}} |
| Queries | {{Filtering, explicit order/tie-breakers, scale limits}} |
| Storage and ownership | {{Private path, provider, connections, serialization and UI dispatch}} |
| Schema evolution | {{Version marker, ordered migrations, atomicity, old-version upgrade path}} |
| Failure/downgrade | {{Preserve data on failure; unsupported newer-schema behavior; recovery}} |
| Retention and backup | {{Implement the approved retention/export/encryption/backup policy}} |

Mark storage sections not applicable if there is no persistence. With a remote API, say whether each row describes the server database, the device's local copy, or both. For SQL, specify parameterization, constraints, transaction boundaries, migration immutability, and how tests exercise existing files. Keep migration definitions with source; document their guarantees here rather than copying SQL.

## Remote API, if used

[API](API.md) owns the client–server rules, and the committed OpenAPI document owns endpoint shapes. Record here only how the app is built around them. Delete this section with API.md if there is no remote API.

| Contract | Definition |
|---|---|
| Client boundary | {{TypedClientInterfaceInCore; HttpClient registration and handlers at the composition root}} |
| Source of truth | {{ServerOnly, DeviceCacheOfServerData or OfflineQueue; which side wins}} |
| Background sync | {{NoneOrOsSpecificContract; lifecycle owner}} |

## Navigation and screen state

| Route / screen | Input contract | Lifetime / retained state | Return, resume, deep link behavior |
|---|---|---|---|
| {{Route}} | {{Validated ID or typed parameters}} | {{DraftSessionDurable}} | {{Refresh, Back, malformed/stale target}} |

{{Describe route registration, query parsing, duplicate-navigation guards, and handling of successful persistence followed by navigation failure. Define whether multi-window use is supported.}}

## DI, startup, async work, and lifecycle

| Service / screen | Lifetime | Owner and concurrency/disposal contract |
|---|---|---|
| {{Service}} | {{SingletonTransientExplicitScope}} | {{Why; subscriptions/tasks/cancellation ownership}} |

{{Describe resources/theme initialization before pages, awaited initialization and retry, UI thread updates, shared-load behavior, timer stop/start, event cleanup, and process-restart recovery. A transient registration does not promise a fresh page if Shell retains it.}}

{{Define cancellation and stale-response rules. A cancellation after commit must not falsely report that nothing was saved. Background tasks need OS-specific contracts, not just page timers.}}

## Errors, diagnostics, and security

{{Map error categories to safe user messages and retry behavior. Define sanitized diagnostic event IDs/fields, allowed sinks and retention. Document permission use, secret storage, redaction, database protection, and optional network/authentication boundaries.}}

## Traceability and decisions

| Requirement / feature scenario | Technical component | Verification boundary |
|---|---|---|
| {{R01 / FT01}} | {{Source paths}} | {{Test/procedure ID in TEST-PLAN}} |

Significant rationale: [ADR index](adr/README.md). Conflicts: {{Link to docs index conflict row}}. Remaining technical risks: {{Risk, mitigation, owner}}. Verified state belongs in [STATUS](delivery/STATUS.md).
