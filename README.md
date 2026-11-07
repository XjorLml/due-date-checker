# Due Date Checker

An offline-first Android app built with .NET MAUI and SQLite for tracking dues in Philippine pesos.

## Project status

Duewise (the product name; projects keep the Due Date Checker name) is an offline Android app for dues in Philippine pesos and Asia/Manila time. As of 2026-11-07, version 1.0 covers:
- create, list, edit, mark paid and soft-delete with a 5-second Undo;
- the Dashboard, Calendar and Details pages;
- due types with optional amounts for tasks;
- repeating bills and subscriptions;
- optional local reminders;
- export and import to a file the user chooses;
- the Focus design, aligned with the live HTML prototype.

There is no login, sync or cloud backup. A signed v1.0 release APK has been built but not published. See [delivery status](docs/delivery/STATUS.md) for dated automated results, emulator and phone evidence, and open acceptance. Source presence does not establish release acceptance.

## Build and test

Use the .NET 10 SDK and the Android MAUI workload, with the Android SDK/JDK configured for Android builds. Run from the repository root:

```powershell
dotnet test tests/DueDateChecker.Core.Tests/DueDateChecker.Core.Tests.csproj
dotnet test tests/DueDateChecker.App.Tests/DueDateChecker.App.Tests.csproj
dotnet build src/DueDateChecker.App/DueDateChecker.App.csproj -c Release
```

The test projects run without an Android device. The App test harness exercises platform-independent view-model/database behavior; native screens, navigation, and Release runtime behavior still require phone testing.

Release APK: add `-f net10.0-android -p:AndroidPackageFormat=apk` to the Release build. The output is `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`.

Release builds are signed with the upload key configured in the ignored `local.signing.props`; copy the checked-in `local.signing.props.example` to create it. To check that Release compiles without the key, add `-p:AndroidKeyStore=false`. Release artifacts for GitHub go in the ignored `artifacts/release/` folder. Never commit keys or passwords.

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
