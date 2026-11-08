# Official reference checks

**Template reference baseline:** checked 2026-10-21 for this kit. Recheck at adoption and toolchain changes; the next project's versions are selected in [SETUP](SETUP.md). These sources support the technical guidance, not evidence that a generated application works.

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

## Revalidation log

| Date | Topic / source | Chosen version or decision | Verified by / affected docs |
|---|---|---|---|
| {{Date}} | {{Official source URL}} | {{Decision or unresolved incompatibility}} | {{Observer; SETUP/SDD/RELEASE links}} |

Store requirements, native SDK policies, and support dates change. Cite the current official platform guidance when adding a shipping target or publishing channel. No store-policy compliance is established by this reference list.
