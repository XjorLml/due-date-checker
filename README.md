# Due Date Checker

An offline-first Android app built with .NET MAUI and SQLite for tracking dues in Philippine pesos.

## Project status

The approved v1 scope is Android local create/list/edit/paid/soft-delete, without reminders, login, or synchronization. Duewise is the chosen product name (user decision, 2026-10-17); existing projects and the app title still use Due Date Checker.

Create/list/edit/paid and delete confirmation code are committed through `7c22b4b` (source inspected 2026-10-17). See [delivery status](docs/delivery/STATUS.md) for dated automated results, attributed phone checks, and outstanding signing/V1.3 acceptance. Source presence does not establish release acceptance.

## Build and test

Use the .NET 10 SDK and the Android MAUI workload, with the Android SDK/JDK configured for Android builds. Run from the repository root:

```powershell
dotnet test tests/DueDateChecker.Core.Tests/DueDateChecker.Core.Tests.csproj
dotnet test tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release
```

The test projects run without an Android device. The App test harness exercises platform-independent view-model/database behavior; native screens, navigation, and Release runtime behavior still require phone testing.

APK output: `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`. Configure the intended keystore through the ignored `local.signing.props` file using the checked-in example before release acceptance. Never commit keys or passwords.

## Documentation

Start with the [documentation index](docs/README.md).

- [Product requirements](docs/PRD.md): approved behavior, stable R01-R08 IDs, and exclusions.
- [Software design](docs/SDD.md): architecture, data contracts, and implementation boundaries.
- [UI/UX design](docs/DESIGN.md): visual standards and clearly scoped future concepts.
- [Delivery status](docs/delivery/STATUS.md) and [test plan](docs/delivery/TEST-PLAN.md): evidence, remaining acceptance, and procedures.
- [V1.3 soft-delete](specs/v1.3-soft-delete/spec.md) and [V1.4 CI](specs/v1.4-ci/spec.md) specifications: scenarios, technical plans, and tasks.
- [Agent workflow](AGENTS.md) and [ADR index](docs/adr/README.md): development and documentation governance.
- [Refactor audit and migration map](docs/delivery/DOCS-REFACTOR.md): preservation, findings, and validation.
- [Historical plan](docs/PLAN.md), [V1.1](docs/phases/V1.1.md), [V1.2](docs/phases/V1.2.md), [V1.3](docs/phases/V1.3.md), and [learning progress](.vibe-wise/progress.md): retained provenance and evidence.
