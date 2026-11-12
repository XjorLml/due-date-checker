# {{ProjectName}} API contract

**Template, for apps that call a remote HTTP API (their own or a third party's). Authority:** the rules every endpoint follows: environments, compatibility, authentication, errors, retries, data encoding and offline behavior.
- The API's generated OpenAPI document owns endpoint shapes.
- [PRD](PRD.md) owns which workflows need the network.
- [SDD](SDD.md) owns how the app is built around this contract.
- Verified state belongs in [STATUS](delivery/STATUS.md).

No remote API? Delete this file, run `python tools/check_docs.py`, and remove each link it reports as missing.

Status: {{ProposedOrAsBuilt}}. Reviewed: {{DateAndSourceRevision}}.

## Environments

| Environment | Base URL | Data | Used by |
|---|---|---|---|
| Local | {{PC address; Android emulator address; device address}} | Synthetic only | Developers; see [SETUP](SETUP.md#local-api-development) |
| {{StagingOrNone}} | {{URL}} | {{Synthetic or masked data rules}} | {{Testers}} |
| Production | {{URL}} | Real user data | Released apps |

How a build selects its base URL: {{BuildConfigurationOrSetting}}. A Release build must point at production (or a deliberately chosen environment), never at a local address.

The app package contains no server secrets: anything shipped inside an app can be extracted. Third-party credentials stay on the server, which calls the provider on the app's behalf.

## Endpoint source of truth

The API generates its OpenAPI document; commit it at {{CommittedOpenApiPath}} and review its diff with every API change. A diff you did not expect is how a breaking change is caught before release. Do not copy endpoint lists into this file.

| Endpoint group | Requirement / scenario | Access | Contract tests |
|---|---|---|---|
| {{RouteGroup}} | {{R01 / FT01}} | {{SignedInUserOrPublic}} | {{TestPath}} |

## Compatibility and versioning

Released apps keep calling the API for months after a newer app ships, because users update late or never. Treat every released request and response shape as a promise.

- **Safe within a version:** new endpoints, new optional request fields, new response fields. A new enum value is safe only if released apps tolerate unknown values.
- **Breaking:** these need a new version or a migration plan:
  - removing or renaming a field
  - changing a field's type, meaning or unit
  - making an optional request field required
  - changing a status code or error type that apps act on
- **Client tolerance:** clients ignore unknown response fields and map unknown enum values to a safe fallback.
- **Version scheme:** {{UrlSegmentHeaderOrNone}}.
- **Oldest supported app version:** {{SupportPolicy}}. Unsupported apps learn they must update through: {{ResponseAndUiBehavior}}.
- **Deploy order:** deploy a backward-compatible API change before releasing the app that needs it. Remove old behavior only after the oldest supported app no longer uses it. See [RELEASE](RELEASE.md#backend-deployment-if-the-app-uses-its-own-api).

## Authentication and authorization

| Decision | Selected approach |
|---|---|
| Sign-in method | {{NoneOrSelectedMethod}} |
| Token type and lifetime | {{AccessAndRefreshTokenPolicy}} |
| Storage on the device | {{SecureStorageAndUnreadableValueBehavior}} |
| Expired session | {{RefreshOnceThenSignIn; drafts retained}} |
| Sign-out | {{ClearLocalTokens; server revocation if supported}} |

The server checks every request against the signed-in user. A record ID sent by the app is never proof of access.

Browser-based sign-in with an external provider needs a callback the app registers. The provider's client secret stays on the server.

## Errors

Error responses use Problem Details ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)) with a stable `type` or error code. The app acts on the code and status, never on message text.

| Category | Typical cause | App behavior |
|---|---|---|
| No connection or timeout | Offline, slow network, server down | {{KeepInputShowOfflineOrRetry; queue or not}} |
| 400 validation | Invalid input | {{ShowFieldErrorsKeepInput}} |
| 401 / 403 | Session expired / not allowed | {{RefreshOrSignIn / explain}} |
| 404 / 410 | Missing or deleted record | {{RefreshAndExplain}} |
| 409 / 412 | Changed by someone else | {{ConflictPolicyBelow}} |
| 429 | Too many requests | {{WaitThenRetry}} |
| 5xx | Server failure | {{KeepDataRetryLater; show a support code or not}} |

## Timeouts, retries, and duplicate prevention

- **Timeout:** total timeout per request is {{Seconds}}. Leaving a screen cancels its requests, and late responses for a closed or replaced screen are ignored.
- **When to retry:** only when repeating the request is safe. That means reads, or writes protected by {{ClientGeneratedIdOrIdempotencyKey}}. A create retried after a lost response must not create a second record.
- **Default retry handler:** the standard .NET resilience handler retries every HTTP method by default, including POST. Disable retries for unsafe methods, or protect every write as above ([references](REFERENCES.md#remote-api-and-server)).
- **Timeout after a write:** the outcome is unknown, not failed. The app finds out by {{ReReadByClientIdOrStatusEndpoint}} before saying anything to the user.

## Data encoding

| Value | Wire format | Rule |
|---|---|---|
| IDs | {{GuidOrServerInteger}} | {{WhoGeneratesThem}}; client-generated IDs make create retries safe |
| Instants | {{Iso8601UtcExample}} | Convert to local time only for display |
| Calendar dates | {{YyyyMmDdWithoutZone}} | A due date is not an instant; follow the PRD date policy |
| Money | {{DecimalStringOrMinorUnits; currency code}} | Never binary floating point |
| Lists | {{CursorOrPageSize; maximum page size}} | Stable order with a unique tie-breaker |

## Offline, caching, and conflicts

Selected level: {{OnlineOnlyReadCacheOrOfflineWrites}}.

| Level | Behavior |
|---|---|
| Online only | Shows an offline state and keeps drafts. |
| Read cache | Shows cached data marked as stale. |
| Offline writes | Queues changes durably, sends them in order, and defines what happens when the server rejects one. |

Conflict policy: {{LastWriteWinsVersionCheckOrMerge}}. A version or ETag check turns a silent overwrite into a 409 or 412 the app can explain.

The device reporting internet access does not prove the API is reachable. Treat the request's own failure as the signal.

## Security and privacy

- Use HTTPS everywhere, except the local development exceptions in [SETUP](SETUP.md#local-api-development), which must not ship in Release builds.
- Validate all input on the server. Client validation exists for usability only.
- Rate limits: {{Policy}}. Built-in limits are counted per server instance.
- Logs on both sides exclude tokens, passwords and personal or financial details. A correlation ID connects an app error to its server log entry.

## Decisions and verification

Record significant choices in an [ADR](adr/README.md): the sign-in provider, the version scheme, the offline level and the conflict policy. Procedures: [TEST-PLAN](delivery/TEST-PLAN.md#remote-api-procedures). Remaining risks: {{RiskMitigationOwner}}.
