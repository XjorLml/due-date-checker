# Focus page content — emulator evidence (2026-11-05)

Scope: [specs/focus-pages](../../../specs/focus-pages/spec.md) FP01–FP08. All observations below are agent-observed on 2026-11-05 unless stated otherwise. This is emulator evidence on a virtual device, not phone acceptance; TalkBack, phone and intended-key signing gates remain held in [STATUS](../STATUS.md).

## Environment

- Windows 11, .NET 10.0.403, MAUI `net10.0-android`, Debug build installed with `-t:Install` (Fast Deployment) on `emulator-5554`.
- AVD `duewise_api35_default` (AOSP Android 35 x86_64 default image, Pixel 7 profile, `-gpu host`), `debug.hwui.drawing_enabled=1`, `auto_time 0`, device clock set to 05 Nov 2026 10:00. The image has no SystemUI, so the status-bar band is the app-bar colour.
- Seeded data from the UI consistency pass, retained on the device: Water bill (03 Nov, ₱850, Overdue), Phone load (03 Nov, ₱299, Paid), Rent (05 Nov, ₱12,000, Due today), Internet (07 Nov, ₱1,699, Upcoming).
- Scripts: scratchpad `visual-pass3.ps1` (seed check, tree dumps, screenshots), `shot.ps1`; two targeted retakes afterwards.

## Automated and build results

| Check | Result |
|---|---|
| `dotnet test tests/DueDateChecker.App.Tests` | 161 passed (158 existing + 3 in `DashboardSummaryTests`); one draft assertion corrected during the run (default category is Other) |
| `dotnet build -c Debug -f net10.0-android` | Build succeeded, 0 warnings, 0 errors |
| `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false` | 0 warnings, 0 errors, before and after the counter layout fix |

## Structural observations (uiautomator tree)

- **FP03 rows.** Dues exposes four `ViewGroup` rows with descriptions such as "Water bill, Bill, Overdue, due 03 November 2026, ₱850.00. Opens due details." Each row spans about 229 px (87 dp) tall at 100% text. No "Refresh" node on Dues, Dashboard, Calendar or Settings after an error-free load (FP07).
- **FP01 summary.** Dashboard shows "Upcoming · November", "2 unpaid · today to 30 Nov" and "Next date · 05 Nov"; the next-due link button reads "Rent" and tapping it opened Details titled "Due details" with the "Status: Due today" badge and Due date / Type / Notes rows. The total ₱13,699.00 (12,000 + 1,699) is visible in the screenshot; the hero label carries the accessible description instead of plain text, so it was verified visually.
- **FP02 counters.** Descriptions "1 overdue. Shows overdue dues", "1 due today. Shows dues due today", "1 of 4 saved dues paid, 25%. Shows paid dues". Tapping the Overdue counter selected the Overdue chip and changed the heading to "Overdue · 1" (screenshot 32). "Payment progress" no longer exists in the tree.
- **FP04 Calendar.** "November 2026" heading with Buttons "‹" (Previous month), "›" (Next month) and "Today" (Return to today, 05 Nov 2026, Asia/Manila) on one line; day cells such as "3⏎2" (Tuesday, November 3, 2026, 2 saved dues) measure 134 × 126 px, about 51 × 48 dp; the selected cell reads "selected, today"; agenda caption "1 saved due".
- **FP05 Settings.** Segment Buttons "System, selected" / "Light, not selected" / "Dark, not selected" and "Monday, selected" / "Sunday, not selected"; Reminders row "Reminders, Off. Opens due-date reminders"; tapping it opened the Reminders page. At 200% text the same segments remain present and selected.
- **FP06 Details.** Category eyebrow, title, HeroAmount, badge, divided Due date / Type / Notes rows and Created / Last updated captions (screenshot 33).

## Visual observations (screenshots in the scratchpad `shots` folder)

| File | Observation |
|---|---|
| 30-dues-light / 40-dues-retake-a | Ledger rows with hairlines, amounts right-aligned, four badge tints; Reminders chip; no card boxes |
| 31-dashboard-light / 41-dashboard-light-v2 | Summary, next-due link in primary, three counters, chips in a three-column grid, "All · 4" heading |
| 32-dashboard-overdue-filter | Overdue chip selected, heading "Overdue · 1" after the counter tap |
| 33-details-from-next-light / 33-details-dark | Details layout in both themes |
| 34-calendar-light / 34-calendar-dark | Controls row, transparent day cells, selected fill and today outline, agenda section |
| 35-settings-light / 35-settings-dark | Segments, Reminders row, notice, Save; dark raises the selected segment on the surface colour with lime text |
| 31-dashboard-dark | Lime selected chip, counters in the dark status colours |
| 36-dues-200, 37-dashboard-200, 38-settings-200, 42-dashboard-200-v2 | 200% text: rows, summary, segments and counters reachable; the first 200% Dashboard capture clipped the "Overdue" counter word, fixed by stacking the count above the word (42 shows the fix) |

## Defects found and handled

1. **Counter word clipped at 200% text** (37-dashboard-200). The count and word were side by side; the word was cut at the card edge. Fixed by stacking them vertically; verified in 42-dashboard-200-v2 and at 100% in 41-dashboard-light-v2.
2. **Faint ghost of the bottom-bar icons** near the top of 30-dues-light and 31-dashboard-light. A relaunch and immediate retake (40-dues-retake-a/b) and the later Dashboard retakes show no ghost at any offset, and the Calendar and Settings captures from the same run were clean, so this is a transient emulator layer artifact under `-gpu host`, not app markup. It is noted for the phone check.

## Not evidence

The first `uiautomator dump` after switching to dark mode returned "null root node"; the dark screenshots were taken after the app relaunched and are valid, but no dark-mode tree assertions were recorded. Browser evidence from the HTML mockup does not transfer to the native app.
