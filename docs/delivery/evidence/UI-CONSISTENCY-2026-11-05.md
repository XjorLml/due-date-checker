# UI consistency emulator evidence — 2026-11-05

Scope: [UC01–UC06](../../../specs/ui-consistency/spec.md#acceptance-scenarios). Agent-observed on the AOSP Android 35 emulator described in the [Focus adoption record](FOCUS-ADOPTION-2026-11-05.md); not phone acceptance.

## Method

Debug build deployed with `dotnet build -t:Install`. `adb root` on the AOSP image, `settings put global auto_time 0`, then `date` moved to 03 Nov, 07 Nov and 05 Nov 2026 to create dues with those due dates through the app's own Add form; Phone load was then marked paid through Edit. Observations through `uiautomator dump` and pulled `screencap` images (kept outside the repository). The clock was restored to automatic afterwards.

## Observations

| Check | Observed | Scenario |
|---|---|---|
| Dues list | Pills read "! Overdue" (red text on pink tint) for Water bill and Phone load, "✓ Paid" (green on green tint) after marking Phone load paid, "● Due today" (amber on cream tint) for Rent, "○ Upcoming" (text on muted tint) for Internet; accessibility descriptions "Status: Overdue", "Status: Paid", "Status: Due today", "Status: Upcoming" | UC01, UC02, UC03 |
| Dashboard | Counts Overdue 2, Due today 1, Upcoming 1 for the same four dues before the paid change; rows use the same pill | UC02 |
| Details | "Status" label followed by the "! Overdue" pill for Water bill; "✓ Paid" for Phone load in dark mode | UC01, UC03 |
| Root pages | Dues, Dashboard, Calendar and Settings render with no toolbar; the tree has no title node above the content; Dues shows the brand mark and "Duewise" once at the top of its header | UC04 |
| Pushed pages | Add due, Due details and Reminders show the Shell title with the Back arrow and no second heading; Edit due keeps the header Delete action and the Cancel/Save footer | UC05 |
| Flows | Four creates, one edit with Save changes, Details and Back round trips all persisted | UC06 |

Light and dark captures were taken for Dues, Dashboard, Calendar, Settings, Details and the editor.

## Not established here

Phone status-bar appearance above hidden-bar roots, TalkBack reading of the pill description and page titles, 200% system text on the pills, and intended-key signing.
