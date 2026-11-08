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

## Bottom bar completed — 2026-11-07

On 2026-11-07 the user compared the build with the selected Focus prototype ("this part is not being implemented on the current build. would you recheck"). The recheck confirmed that the adoption above had left out four parts of the prototype's bar. DESIGN's 2.0.0 bar row described a flat bar, so the gap went unrecorded. In scope of the 2026-11-05 "look and navigation" authorization:

- [x] Floating card: rounded 22, 1 dp border, 10 dp margin left, right and below on the page canvas, 4 dp inner padding, soft elevation. The system navigation inset moves from inside the card to the margin below it (`AddDock`, a window-insets listener replacing Material's).
- [x] Lime pill (36 × 32, radius 11) behind the selected icon, which is drawn in `onAccent`; the caption is `primary` Bold. Material's active indicator is used, so the pill follows selection.
- [x] Captions in Plus Jakarta Sans (Bold selected, Medium unselected, SemiBold `text` for the Add label), loaded from the packaged font assets; applied before the UX13 caption fit, so the fit measures the drawn face.
- [x] Dashboard and Dues glyphs redrawn in the prototype's shapes.
- [x] Automated, agent-observed: Core **162 passed**, App **261 passed**; Debug build (standalone APK) 0 warnings; Release compile with signing bypassed (`-p:AndroidKeyStore=false`) **0 warnings, 0 errors**. The renderer is Android-only and has no unit tests; these runs show nothing else regressed.
- [x] Emulator, agent-observed (Android 35 AOSP image, 411 dp): Dues and Dashboard selected; dark theme (icon readable on the pill); 200% text and 320 dp (captions fit inside the card, clear of the corners); Add disc opens the editor with bar and disc hidden; Cancel brings the bar back. Two early captures were taken before the first frame settled (one black, one without the pill); they are not evidence.
- [ ] Phone check, including the bottom inset with the phone's own navigation mode. The emulator image has no system navigation bar, so the inset path is unobserved.

## Launcher palette correction — 2026-11-08

Small fix under R01 / FA01 and DESIGN §2/§3, requested after the user observed a purple launcher despite the adopted green theme. Source inspection found `Resources/AppIcon/appicon.svg` still filled `#4338CA`; project icon/splash metadata and the in-app mark already used the prescribed ink `#253C20`.

- [x] Change only the launcher background SVG fill to the existing brand ink; retain the white calendar-check glyph and coral dot.
- [x] Agent-observed: Release APK rebuilt with 0 warnings/errors. Visually inspected before/after generated icons; all 15 packaged legacy, round and adaptive backgrounds sampled the expected ink across mdpi–xxxhdpi. Source foreground/splash unchanged. [Evidence](../../docs/delivery/evidence/ICON-PALETTE-2026-11-08.md).
- [ ] Confirm the updated launcher on the user's phone after installing an update signed with their existing key. No phone installation is part of this fix.

Unit tests are not needed for this asset-only recolor; build/resource inspection verifies the affected boundary. Other feature work and historical test evidence remain untouched.

## Remaining acceptance

- [ ] Phone verification on the project's device (M2101K6G, API 33): status-bar colour and icon contrast in both themes, bar height with system text scaling, gesture-navigation insets under the footer and disc, Back behaviour from Settings, reminder tap while on Settings. Keyboard overlap: the editor footer was hidden by the keyboard; fixed and emulator-verified on 2026-11-06 ([record](../../docs/delivery/evidence/EMULATOR-HARDENING-2026-11-06.md#defects-found-and-fixed)), phone check pending (Android 13 may resize differently).
- [ ] TalkBack/keyboard/switch review of the disc ("Add due"), disabled centre slot and footer; deferred by user direction.
- [ ] Correct upload-keystore password, then intended-key signing and release gates, which remain held.
