# Focus adoption emulator evidence — 2026-11-05

Scope: [FA01–FA10](../../../specs/focus-adoption/spec.md#acceptance-scenarios). All observations below are **agent-observed on an Android emulator**, not phone acceptance. Procedure: [TEST-PLAN](../TEST-PLAN.md#focus-adoption-verification).

## Environment

- Windows 11 Pro, .NET SDK 10.0.403, Android workload 36.1.118, JDK 21.0.8, Visual Studio Android SDK (platforms 35/36, build-tools 36.0.0).
- Emulator 37.2.13 and platform-tools 37.0.1 under `%LOCALAPPDATA%\Android\EmulatorSdk`, deliberately outside the auto-detected `Android\Sdk` path (a partial SDK there broke `aapt2` discovery). Java needed `-Djavax.net.ssl.trustStoreType=Windows-ROOT` to reach the repository; the 1.6 GB Google APIs image download was aborted by the network layer, so two smaller images were fetched with BITS and unpacked by hand (hand-written AVD `config.ini`, no package manifest):
  - `duewise_api35`: Android 35 **Google APIs ATD** (x86_64-35_r01), used for the structural pass. ATD images strip the rendering pipeline and cannot produce screenshots.
  - `duewise_api35_default`: Android 35 **AOSP default** (x86_64-35_r02), used for the visual pass. Both AVDs use the Pixel 7 profile (1080×2400 at 420 dpi, density 2.625), 2 GB RAM, WHPX acceleration.
- Rendering quirk: both images boot with `debug.hwui.drawing_enabled=0`, which makes every app draw nothing even though layout and touch work; every GPU backend produced byte-identical black captures until `adb shell setprop debug.hwui.drawing_enabled 1` was issued. The AOSP image also runs without SystemUI, so no status bar exists and the status-bar inset shows the app-bar layout's `colorPrimary` background (ink in light, lime in dark). On a device the opaque status bar (`colorPrimaryDark`, the canvas colour) covers that region, as the earlier design-adoption phone evidence recorded; this is listed for the phone check rather than changed.
- Artifact: Debug `net10.0-android` build of the working tree after the Focus changes, deployed with `dotnet build -t:Install` (Fast Deployment; a bare `adb install` of the Debug APK aborts at startup with "No assemblies found", expected for Debug builds).
- Inputs: `adb shell input tap/text/keyevent`, `cmd uimode night`, `settings put system font_scale`. Observations: `uiautomator dump` accessibility tree (text, content description, bounds, enabled, selected) and `screencap` pulled from the device. Screenshots are kept outside the repository. 1 dp = 2.625 px.

## Structural results (accessibility tree, ATD image)

| Step | Observed | Scenario |
|---|---|---|
| Cold start | Dues selected; bar slots Dashboard, Calendar, Add, Dues, Settings at y 2232–2400 (64 dp); Add slot `enabled=false`; disc "Add due" at [466,2190]–[613,2337] (56 dp, top 16 dp above the bar) | FA03, FA04 |
| Tap disc | Editor opens; no bar, no disc; texts in order What is due? → Amount (PHP) → Due date → Type → Notes; footer Cancel [53,2243]–[525,2369] and Save due [556,2243]–[1028,2369] (48 dp); no Delete in Add mode | FA04, FA06, FA07 |
| Type title and amount, Save | Returns to Dues; card "Focus check due, Bill, Unpaid, due 05 November 2026, ₱123.45" | FA04, FA09 |
| Card → Details → Edit due | Details has no bar; Edit mode shows header **Delete** [911,146]–[1080,272], Save changes, Cancel; dialog "Delete due?" opens and Cancel keeps Edit mode; editor Cancel returns to Details; Back returns to Dues with the card | FA07, FA09 |
| Dashboard tab → disc → Cancel | Editor opens from Dashboard and Cancel returns to Dashboard (selected=Dashboard) | FA03, FA04 |
| Calendar tab | Month grid and controls present, bar and disc present | FA03 |
| Settings tab | Settings root shows the bar (selected=Settings); scrolled Reminders button opens Reminders with no bar; Back returns to Settings; Back again selects Dues | FA05 |
| 200% text (`font_scale 2.0`) | Bar grows to y 2179–2400 (84 dp) with the disc at [466,2137]–[613,2284]; editor footer at y 2204–2369 remains fully on screen; restored to 1.0 | FA10 |
| Header buttons | Dues shows Reminders only (no Settings, no Add); Dashboard and Calendar have no Add button; empty-state copy reads "Tap Add in the bottom bar to create your first due" | FA08 |

Two defects were found and fixed during this pass before the results above: the disc was laid out flush with the bar's top instead of 16 dp above it (padding was applied to the disc as well as the bar; fixed with a bar top margin), and taps inside the bar's bounds reached the bar instead of the disc because Android dispatches touches to the highest-elevation child and Material's bar has 8 dp (fixed by raising the disc to 12 dp). The failed attempts are not passing evidence.

## Visual results (screenshots, AOSP default image)

Seventeen screenshots: Dues, Dashboard, Calendar, Settings, Add editor, Details and Edit editor in light and dark, plus Dues and the Add editor at 200% text in light. Sampled pixel values match DESIGN 2.0.0 §3: canvas `#F8F9F5` / `#111710`, disc `#D9F27E`, primary `#253C20` light.

| Check | Observed | Scenario |
|---|---|---|
| Palette, light | Warm-grey canvas, white cards with `border`-coloured 1 dp strokes, ink headings and primary Save button, lime disc with white ring, ink selected tab label, secondary-grey captions and unselected labels, muted status chip | FA01 |
| Palette, dark | Near-black canvas, dark-green surfaces, lime Save button and selected tab label, lime disc with dark ring, light text, outlined secondary buttons | FA01 |
| Shape and type | 14 dp card/field/button corners; 26 dp page headings, 20 dp card amounts, 16 dp semibold card titles; 48 dp buttons and fields; 20 dp page inset | FA02 |
| Editor | Title, amount, date, type, notes order; `textSecondary` field outlines; fixed footer with outlined Cancel and filled Save above a 1 dp divider; Edit mode shows the header Delete text action | FA06, FA07 |
| 200% text | Headings and captions wrap; footer buttons grow to two lines and remain fully visible; bar grows and the disc stays centred | FA10 |
| Dashboard / Calendar / Settings | Progress ring uses the muted track and primary arc; calendar day buttons use primary/secondary styles; Settings shows the bar with the Settings label selected | FA01, FA03, FA05 |

Not visible on this image: status-bar icons and the opaque status-bar colour (no SystemUI), ripple/pressed states (not captured).

## Not established here

Phone acceptance on M2101K6G, status-bar appearance with SystemUI present, TalkBack/keyboard/switch review, real keyboard overlap of the footer, gesture-navigation insets, and intended-key signing.
