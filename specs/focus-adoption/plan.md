# Implementation plan — Focus design adoption

Scope and scenarios: [spec](spec.md). Tracking: [tasks](tasks.md).

## Affected modules

| Area | Files | Change |
|---|---|---|
| Tokens | `src/DueDateChecker.App/Resources/Styles/Colors.xaml`, `Styles.xaml` | Focus palette under the existing resource keys plus new Accent/OnAccent/Hero/HeroText pairs; 26/20/16 headings; 14 dp radius; 48 dp buttons and fields; field outline uses `TextSecondary`; 3 dp focus outline. |
| Platform colours | `Platforms/Android/Resources/values/colors.xml`, `values-night/colors.xml` | Status-bar and primary mirrors; `duewiseAccent`, `duewiseOnAccent`, `duewiseSurface` for the native disc. |
| Brand assets | `Resources/Images/brand_mark.svg`, `DueDateChecker.App.csproj` (icon and splash colour), new `nav_settings.svg`, `nav_add.svg` | Ink background replaces indigo; two new 24-unit outline glyphs. |
| Shell | `AppShell.xaml`, `AppShell.xaml.cs` | Five tabs in prototype order; `MainTabs.CurrentItem = DuesTab` before first display; Settings becomes a tab root; `OpenAddAsync` pushes the editor on the current root; `Navigating` cancels any selection of the Add slot; Back from Settings selects Dues; the SettingsPage route registration is removed because it is now a root. |
| Android renderer | `Platforms/Android/DuewiseShellRenderer.cs` | Keeps the large-text bar height; forces labelled items; disables menu slot 2; wraps the `BottomNavigationView` in an `AddDock` FrameLayout that adds 16 dp of clearance and lays the 56 dp disc over the centre slot; the dock measures to zero whenever Shell hides the bar. |
| Layout helper | `Pages/PageLayout.cs` | 14/20/32/40 dp insets; optional footer sharing the margins; extra bottom clearance for root pages. |
| Pages | `DuesListPage`, `DashboardPage`, `CalendarPage`, `SettingsPage` (+ code-behind) | Header Add/Settings buttons removed, empty-state copy updated, Settings shows the bar, root pages reserve clearance for the disc. |
| Editor | `Pages/DueEditPage.xaml`, `.xaml.cs` | Two-row Grid: scrolling Essential-first fields, then a surface footer with Cancel and Save; Delete becomes a `ToolbarItem` added only while `ShowDelete` is true and enabled by `CanDelete`. All view-model bindings, handlers and first-error focus are unchanged. |

Unchanged: view models, Core domain, repositories, migrations, reminder coordinator/platform, test projects.

## Dependencies and choices

- **Why Shell is kept.** Routes, per-tab stacks, hidden-bar pages and the reminder tap router all depend on Shell. Replacing it with a hand-built root page would touch every navigation contract in [SDD §6](../../docs/SDD.md#6-ui-and-navigation) for a purely visual gain.
- **Why the disc is native.** Shell cannot float a cross-platform view over its bar. Android's `BottomNavigationView` tints every item icon, so a lime icon on a fifth tab would come out ink-coloured. The existing Android renderer already owned the bar's height, so it is the smallest place to add a wrapper view. The wrapper is a plain `FrameLayout` with `GradientDrawable` and `ImageView`; no package is added.
- **Why a fifth placeholder tab.** Material's bar divides its width equally across items, so a real fifth item reserves the centre slot and supplies the "Add" caption for free. It is disabled natively so it can never be selected, and `Shell.Navigating` cancels any programmatic selection as a second guard.
- **Why Delete moves to the header.** The prototype's selected layout keeps the footer to two actions; a header action is the native equivalent and remains hidden for a new due or an unavailable target.
- **Emulator.** The Visual Studio SDK under `Program Files (x86)` has no emulator and is not writable without elevation. The emulator, platform-tools and an Android 35 x86_64 Google APIs image live under `%LOCALAPPDATA%\Android\EmulatorSdk`, deliberately not the auto-detected `Android\Sdk` path, because a partial SDK there makes the Android workload stop finding `aapt2`. Java needed `-Djavax.net.ssl.trustStoreType=Windows-ROOT` to download through this machine's TLS-inspecting layer.

## Verification sequence and outcomes

1. `dotnet test` Core then App: all tests pass (nothing in the suites depends on XAML or Shell).
2. `dotnet build -c Debug -f net10.0-android`: succeeds with no errors.
3. Emulator (Pixel-class AVD, API 35): FA01–FA10 in light and dark, 100% and 200% text, with screenshots saved outside the repository. Record environment, APK identity and exact checks in [tasks](tasks.md) and STATUS as emulator-observed.
4. `dotnet build -c Release -f net10.0-android`: succeeds; not device acceptance.
5. Documentation: DESIGN 2.0.0, PRD exception, SDD §6, STATUS, TEST-PLAN, README, ADR 008; link and anchor validation.

## Risks and limits

- The dock assumes Shell's layout places the bar inside a `ViewGroup`; if a future MAUI version changes that layout, `AddDock.Wrap` returns null and the bar simply shows the disabled "Add" caption without a disc. Verify after MAUI upgrades.
- A disabled Material item draws its caption with the bar's default text colour; if a MAUI update adds a disabled state colour the caption could dim.
- Emulator results do not establish phone acceptance, TalkBack, real keyboard overlap or gesture-navigation insets.

## Contract and source navigation

- [DESIGN 2.0.0](../../docs/DESIGN.md) sections 3–7, 9, 11–13.
- [PRD exception](../../docs/PRD.md#exclusions-and-deferred-capabilities) for the five-slot bar and Settings tab.
- [SDD §6](../../docs/SDD.md#6-ui-and-navigation) and the design resource mapping.
- [ADR 008](../../docs/adr/008-focus-design-baseline.md) records the baseline change.
