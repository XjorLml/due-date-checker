# Local setup and toolchain

**Template. Authority:** selected versions, build hosts, configuration, and developer commands. Fill placeholders before running examples. [SDD](SDD.md) owns supported platform capabilities; [RELEASE](RELEASE.md) owns distribution.

## Reproducibility record

| Item | Selected value | Source / checked date |
|---|---|---|
| .NET SDK | {{ExactSdkVersion}} | {{Official compatibility source / Date}} |
| MAUI workload set | {{ExactWorkloadSetVersion}} | {{Matched SDK feature band / Date}} |
| MAUI Controls | {{ResolvedVersionAndHowSelected}} | {{Restore evidence}} |
| Target frameworks | {{SelectedTFMs}} | {{SDD platform matrix}} |
| IDE/extensions | {{VersionsOrCLIOnly}} | {{Compatibility source}} |
| Android tools, if targeted | {{JDK, SDK platform, build-tools, emulator image}} | {{Release compatibility source}} |
| Apple tools, if targeted | {{macOS, Xcode, SDK, signing/provisioning setup}} | {{Release compatibility source}} |
| Windows tools, if targeted | {{Windows SDK, Windows App SDK/build requirements}} | {{Release compatibility source}} |
| Package sources / dependencies | {{Feed config; version and update policy}} | {{Project files or central package file}} |
| API host, if the app has its own API | {{ASP.NET Core version; hosting model and server OS}} | {{Official source / Date}} |
| Server database, if used | {{Provider and version; EF Core version if used}} | {{Official source / Date}} |

Recheck [MAUI support and release compatibility](REFERENCES.md#toolchain-and-platforms) at adoption and upgrades. MAUI has its own support lifecycle. Exact native-tool requirements depend on the selected release. iOS development requires a Mac build host; plan a macOS runner for Apple builds and Windows for Windows builds. Select an Android-capable host for Android. A Linux test runner for plain .NET code does not establish any MAUI target build.

## SDK and workload selection

Create a root `global.json` using real versions after verification. Example shape only:

```json
{
  "sdk": {
    "version": "{{ExactSdkVersion}}",
    "rollForward": "disable",
    "allowPrerelease": false,
    "workloadVersion": "{{ExactWorkloadSetVersion}}"
  }
}
```

The SDK version must be a full version, not `10.0.x`. Exact selection favors reproducibility and requires deliberate servicing updates. Record an alternative roll-forward policy if the team chooses one. SDK selection and workload-set selection are separate controls. See [global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) and [workload sets](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-sets).

## First checkout

1. Install the selected SDK and the native tools required by the chosen targets. Use the same versions in CI.
2. Inspect `dotnet --info`, `dotnet workload list`, and `dotnet workload --version`. Record resolved values when diagnosing differences.
3. From the repository root, restore only the applicable workloads/projects for this host. Multi-target projects must condition TFMs appropriately or use a host-specific target selection documented here.
4. Restore dependencies and run the plain .NET test projects. Then build each applicable App target explicitly.
5. Launch on a selected simulator/device and record setup smoke results separately from release acceptance.

PowerShell command examples; replace every placeholder first and check each exit code:

```powershell
dotnet workload restore src/{{ProjectName}}.App/{{ProjectName}}.App.csproj
dotnet restore tests/{{ProjectName}}.Core.Tests/{{ProjectName}}.Core.Tests.csproj
dotnet test tests/{{ProjectName}}.Core.Tests/{{ProjectName}}.Core.Tests.csproj -c Release
dotnet test tests/{{ProjectName}}.App.Tests/{{ProjectName}}.App.Tests.csproj -c Release
dotnet build src/{{ProjectName}}.App/{{ProjectName}}.App.csproj -c Release -f {{SelectedTFM}}
```

These commands have not been executed for a new project. Adjust App.Tests if presentation logic is extracted. The build command does not publish a distribution package. Test commands target individual projects so a pure test job need not restore unsupported MAUI targets.

## Configuration and dependencies

- Check in `.editorconfig` and deliberate nullable/analyzer settings. Use `Directory.Build.props` only for settings actually shared by projects; keep target-specific settings conditional.
- Use explicit package versions. A workload-supplied `MauiVersion` still needs its resolved value recorded. Consider `Directory.Packages.props` when shared versions become repetitive; central management is optional.
- For repeatable application restores, choose and document a NuGet lock-file policy, commit the appropriate lock files, and use locked mode in CI only after generating them. Review dependency/servicing updates with relevant tests and platform builds. See [NuGet dependency locking](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies).
- Document non-secret environment variable names and example values. Inject secrets from a local ignored file or CI secret store; never embed service credentials in a distributed client.
- Keep local signing files, keystores/certificates, build output, and sensitive device captures ignored. Confirm examples contain placeholders rather than developer-specific paths.

## Local API development

Only when the app calls its own API ([API](API.md)); otherwise delete this section. Record the values once verified, and keep secrets out of the repository.

| Item | Selected value |
|---|---|
| API project and local URLs | {{ProjectPath; ports from launch settings}} |
| Address from the Android emulator | {{EmulatorUrl, such as http://10.0.2.2:port}}; the emulator's own `localhost` is the emulator, not the PC |
| Address from a physical device | {{PcNetworkAddressWithFirewallRule or a development tunnel}} |
| Local secrets | {{UserSecretsKeyNames}}; never in `appsettings*.json` or the app |
| Local database | {{Provider; reset and seed command with synthetic data}} |

- **Plain HTTP:** Android blocks it by default. For local work, either allow cleartext only for the development address, or keep HTTPS and accept the development certificate only in Debug builds. Keep both exceptions out of Release builds (for example, conditional on the Debug configuration), and confirm with TEST-PLAN TP12 ([official options](REFERENCES.md#remote-api-and-server)).
- **iOS simulator:** it uses the Mac's network, so an API running on a Windows PC is not `localhost` there.
- **Development secrets:** `dotnet user-secrets` is unencrypted and for development only. Production secrets come from the host's secret store.

PowerShell examples; replace placeholders and record the verified commands:

```powershell
dotnet user-secrets set "{{SecretKey}}" "{{LocalValue}}" --project src/{{ProjectName}}.Api/{{ProjectName}}.Api.csproj
dotnet run --project src/{{ProjectName}}.Api/{{ProjectName}}.Api.csproj --launch-profile {{Profile}}
dotnet test tests/{{ProjectName}}.Api.Tests/{{ProjectName}}.Api.Tests.csproj -c Release
```

Run the API and the app as separate processes. The API tests need no emulator.

## Troubleshooting and maintenance

| Situation | Recovery |
|---|---|
| Build command still contains a placeholder | Complete the reproducibility record and replace the example before executing it. A documentation-only copy has no App project to build yet. |
| Documentation path/anchor error | Run `python tools/check_docs.py` from the repository root; use its file/line to correct the link or restore the target. Rerun after renaming a feature. |
| Readiness check reports placeholders | Decide each value or remove the inapplicable example with a reason. Keep unknown product decisions explicit; do not invent approvals or passes to clear the check. |
| Required SDK/workload/native host unavailable | Compare the selected versions with installed tools, then use an authorized compatible host or record the check blocked. Avoid reinstalling/updating everything without reviewing compatibility. |
| A test/build could not execute | Record the concrete failure and next step in STATUS. A successful compile, restored package or another machine's historical pass does not replace that execution. |

Project-specific failures, artifact locations and servicing owner: {{KnownFailuresOutputLocationsAndOwner}}. The [tools guide](../tools/README.md) owns checker commands, exit meanings and parser limits.

After a toolchain change, record exact versions and relevant test/build/native results in [STATUS](delivery/STATUS.md). Do not inherit earlier artifact acceptance automatically.
