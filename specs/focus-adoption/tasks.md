# Focus design adoption — task tracking

Authorization: user request on 2026-11-05 ("proceed on implementing the new design, use the emulator to test"), scope confirmed as look + navigation with an installed emulator. Scenarios: [spec](spec.md). Approach: [plan](plan.md). All results are agent-observed unless stated otherwise.

## Implementation

- [x] Inspect docs, Shell, pages, styles, renderer and tests; confirm no test depends on Shell or XAML.
- [x] Replace the palette, type scale, radius, button/field heights and focus width in Colors.xaml and Styles.xaml; mirror Android status-bar colours; recolour brand mark, icon and splash.
- [x] Five-slot Shell bar with Dues as start tab, Settings as a root, Add slot intercepted; `OpenAddAsync` on AppShell.
- [x] Android renderer: labelled items, disabled centre slot, `AddDock` wrapper floating the 56 dp lime disc 16 dp above the bar at 12 dp elevation; hidden with the bar.
- [x] Remove header Add/Settings buttons from Dues, Dashboard, Calendar; keep the Reminders shortcut; update empty-state copy; root pages reserve bottom clearance.
- [x] Editor: Essential-first order, fixed Cancel/Save footer sharing page margins, Delete as a header toolbar item bound to ShowDelete/CanDelete.
- [x] New icons `nav_settings.svg`, `nav_add.svg`; `PageLayout` footer and clearance parameters.

## Verification — 2026-11-05

Environment and full observations: [evidence record](../../docs/delivery/evidence/FOCUS-ADOPTION-2026-11-05.md).

- [x] `dotnet test` Core: **61 passed**. App: **158 passed** (219 total), run sequentially after the code changes (view models and Core are unchanged by this slice).
- [x] `dotnet build -c Debug -f net10.0-android`: **Build succeeded**, 0 errors.
- [x] `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false`: **0 warnings, 0 errors** (re-run after the renderer fixes). With the upload keystore enabled the build fails at jarsigner ("keystore password was incorrect" for the password in the ignored `local.signing.props`); this predates the slice and needs the user's correct password.
- [x] Emulator structural pass (Android 35 Google APIs ATD, Pixel 7 profile): FA03–FA10 observed through the accessibility tree: five slots with Dues selected at cold start and the Add slot disabled; 56 dp disc 16 dp above the 64 dp bar; editor from every root returns to that root; field order title/amount/date/type/notes; 48 dp Cancel/Save footer; header Delete only in Edit mode with the confirmation dialog; Settings root with the bar, Reminders pushed without it, Back from Settings to Dues; 200% text keeps footer, bar (84 dp) and disc reachable; create/edit/cancel round trip persisted a due.
- [x] Emulator visual pass (Android 35 AOSP default image, after enabling `debug.hwui.drawing_enabled`): seventeen screenshots in light and dark plus 200% text; sampled canvas, primary and accent pixels match DESIGN 2.0.0 §3; 14 dp radii, 26/20/16 type, 48 dp controls, Essential-first editor with fixed footer and header Delete confirmed visually (FA01, FA02, FA06, FA07, FA10).

Two defects found by the emulator pass were fixed before the results above (disc flush with the bar; bar winning taps over the disc by elevation). Earlier failed runs, the Fast Deployment startup abort of a bare `adb install`, and the black captures before enabling HWUI drawing are not passing evidence. The emulator image has no SystemUI, so the status-bar inset shows the app-bar layout's `colorPrimary`; on a device the opaque `colorPrimaryDark` status bar covers it, which the phone check below must confirm.

## Remaining acceptance

- [ ] Phone verification on the project's device (M2101K6G, API 33): status-bar colour and icon contrast in both themes, bar height with system text scaling, gesture-navigation insets under the footer and disc, Back behaviour from Settings, reminder tap while on Settings.
- [ ] TalkBack/keyboard/switch review of the disc ("Add due"), disabled centre slot and footer; deferred by user direction.
- [ ] Correct upload-keystore password, then intended-key signing and release gates, which remain held.
