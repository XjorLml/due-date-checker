# Official reference checks

**Template reference baseline:** checked 2026-10-21 for this kit; the remote API rows were checked 2026-11-09. Recheck at adoption and toolchain changes; the next project's versions are selected in [SETUP](SETUP.md). These sources support the technical guidance, not evidence that a generated application works.

## Toolchain and platforms

| Source | Verified guidance / adoption implication |
|---|---|
| [MAUI support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/maui) | On this review date the policy lists MAUI 10 as supported, latest patch 10.0.110, ending support 2027-05-11. This is a dated observation, not a permanent version recommendation; MAUI support differs from the .NET runtime lifecycle. |
| [Supported platforms](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0) | MAUI supports Android, iOS, Mac Catalyst and Windows. iOS development requires a Mac host. Choose product platforms and check version-specific OS/native-tool requirements. |
| [MAUI release compatibility](https://github.com/dotnet/maui/wiki/Release-Versions) | Use the selected release's compatibility details for SDK, workload and native tool choices. Do not infer compatibility from a matching major version alone. |
| [global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) | Selects the .NET SDK, independently of project target frameworks; exact version and roll-forward policy are configurable. |
| [Workload sets](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-sets) | A workload-set version selects a coordinated workload group and can be recorded as `sdk.workloadVersion` in global.json. Keep it compatible with the selected SDK. |
| [NuGet dependency locking](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies) | A lock file records the resolved dependency graph; locked restore can reject a changed graph in CI. Generate and maintain lock files deliberately. |

The supported-platform documentation can contain multiple version-specific sections. Select the intended documentation version and follow its release links before recording OS baselines; this kit deliberately does not copy a universal minimum OS number.

## Implementation and distribution

| Source | Verified guidance / adoption implication |
|---|---|
| [Dependency injection](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection?view=net-maui-10.0) | Built-in DI supports constructor injection. Document actual service lifetimes and page retention in SDD. |
| [App lifecycle](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0) | Window events expose lifecycle transitions. Resume and stopping require explicit resource/state handling. |
| [Compiled bindings](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings?view=net-maui-10.0) | `x:DataType` enables compile-time binding checks; full trimming and NativeAOT require compiled bindings. |
| [Shell navigation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-10.0) | `QueryPropertyAttribute` is not safe for full trimming/NativeAOT; use `IQueryAttributable`. String query values received through that interface are not automatically URL decoded. |
| [Microsoft.Data.Sqlite async limitations](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async) | ADO.NET async methods execute synchronously for this provider; awaiting them alone does not move work off the UI thread. |
| [Android publishing from CLI](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0) | Distribution needs the appropriate signing setup and channel-specific packaging; verify the produced package and signing identity. |

## Remote API and server

Use these sources only when the app calls a remote API ([API](API.md)). Each row was read on 2026-11-09 by an agent web fetch; the guidance is not evidence that any project works.

App side (.NET MAUI):

| Source | Verified guidance / adoption implication |
|---|---|
| [Local web services](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-10.0) | The Android emulator reaches the PC's loopback at `10.0.2.2`; the iOS simulator uses the Mac's network. The ASP.NET Core development certificate is not trusted by the emulator or simulator. For plain HTTP the page offers `UsesCleartextTraffic` inside `#if DEBUG` or a network security config file. An app in a Mac's simulator cannot reach `localhost` on a Windows PC. The page also mentions .NET Aspire integration for .NET 10 as an option. |
| [Secure storage](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?view=net-maui-10.0) | Designed for small amounts of text. On Android, Auto Backup can restore values that "cannot be decrypted": exclude the MAUI secure preferences file from backup (or disable backup) and handle a failed read by clearing stored values. iOS Keychain entries can survive an uninstall. |
| [Networking (Connectivity)](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/networking?view=net-maui-10.0) | `NetworkAccess.Internet` can be reported when full web access isn't available; it does not prove the API is reachable. Android needs `ACCESS_NETWORK_STATE`. |
| [Web authenticator](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication?view=net-maui-10.0) | Starts browser-based sign-in and listens for a registered callback URL. Recommends a web backend as the middle layer instead of storing secrets in the app. At the check date the page said it wasn't working on Windows. |
| [HTTP resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience) | Package `Microsoft.Extensions.Http.Resilience`; `AddStandardResilienceHandler()` adds a rate limiter, total timeout (30 s), retry (3 attempts with backoff), circuit breaker and attempt timeout (10 s). It retries **all** HTTP methods by default; `Retry.DisableForUnsafeHttpMethods()` stops retries of POST, PUT, PATCH, DELETE and CONNECT. |

Server side (ASP.NET Core):

| Source | Verified guidance / adoption implication |
|---|---|
| [OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0) | Package `Microsoft.AspNetCore.OpenApi`; `AddOpenApi()` and `MapOpenApi()` serve `/openapi/v1.json` by default, OpenAPI 3.1 in .NET 10. Build-time generation uses `Microsoft.Extensions.ApiDescription.Server` and runs the app's startup code. The endpoint has no authorization by default. |
| [Integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) | Package `Microsoft.AspNetCore.Mvc.Testing`; `WebApplicationFactory<TEntryPoint>` runs the API in memory on a test server. Service replacements in `ConfigureWebHost` run after `Program.cs`. The factory defaults to the Development environment. |
| [Errors in APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0) | `AddProblemDetails()` with `UseExceptionHandler()` and `UseStatusCodePages()` produces Problem Details responses. The page links both RFC 7807 and its replacement RFC 9457. |
| [Applying EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) | For production, prefer a migration bundle for automation or an idempotent SQL script when review is required; "carefully consider" migrating at app startup. |
| [App secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0) | Secret Manager (`dotnet user-secrets`) doesn't encrypt and is for development only. Production secrets come from a controlled store such as Azure Key Vault; secrets are not deployed with the app. |
| [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) | `AddRateLimiter()`/`UseRateLimiter()`; counters are kept in memory per app instance. Set `RejectionStatusCode` explicitly (the page's examples use 429). |

## Revalidation log

| Date | Topic / source | Chosen version or decision | Verified by / affected docs |
|---|---|---|---|
| {{Date}} | {{Official source URL}} | {{Decision or unresolved incompatibility}} | {{Observer; SETUP/SDD/RELEASE links}} |

Store requirements, native SDK policies, and support dates change. Cite the current official platform guidance when adding a shipping target or publishing channel. No store-policy compliance is established by this reference list.
