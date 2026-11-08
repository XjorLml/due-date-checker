# Prototype alignment evidence — 2026-11-07

Scope: [prototype alignment spec](../../../specs/prototype-alignment/spec.md), scenarios PA01–PA10, after the user's request on 2026-11-07 to "update the documentation based on the current LIVE PROTOTYPE then implement all page changes" and their six scoping answers recorded in the spec. All evidence below is **agent-observed** on 2026-11-07 unless marked otherwise.

## Environment and artifact

- Windows 11, .NET SDK 10.0.403, MAUI 10, Android 35 emulator `duewise_api35_default` (1080 × 2400, 420 dpi, about 411 dp wide), signed with the laptop debug key now on this computer.
- Standalone Debug APK `Duewise-2026-11-07d.apk` (SHA-256 prefix `3F7C4C760AF42E0A`, MD5 `2f0874eb617744e67b257976fee35b73`), kept in the session scratchpad and copied to the phone's Downloads later the same day (checksum verified); installation by the user pending.

## Automated

| Check | Result |
|---|---|
| `dotnet test` Core | **162 passed** |
| `dotnet test` App | **279 passed**. New: DuesFilterTests (search and filters moved from Dashboard, the fold interplay, status links), PrototypeAlignmentTests (unsaved-changes check, editor rows, calendar words, row projection), new Dashboard section tests. Moved assertions in CategoryTests, OptionalAmountTests and RecurrenceTests. |
| Debug build with embedded assemblies | 0 warnings, 0 errors |
| Release compile, signing bypassed (`-p:AndroidKeyStore=false`) | 0 warnings, 0 errors |

## Emulator observations

| Scenario | Observed |
|---|---|
| PA01 | Wordmark header with bell on Dues, Dashboard, Calendar and Settings; the bell opened Reminders. |
| PA02 | Details and Reminders showed "‹ Back", a centred title, and on Details the pencil; header Back returned to the parent. The editor showed its title and, when editing, the outlined red bin. |
| PA03 | Dashboard: summary, next-date row with arrow, inline counters, "Needs attention" (Water bill, Rent, Internet), "A little further ahead" ("No later dues this month."), footnote. A row in the accessibility tree carried the full spoken text and opened Details. |
| PA04 | Dues: "Find by title", status chips, Type filter, "4 matching dues" with "Reset filters", the earlier paid row with no filter active. |
| PA05 | Rows with type icon, "dd MMM · Type", status as glyph and word without a fill. |
| PA06 | Calendar: "2 dues" and "1 due" under day numbers, selected day filled, "Monday first · includes paid" and "Today" under the grid. |
| PA07 | Edit and Add layouts as specified; "Change" opened the four-type sheet; Cancel with a typed change showed "Discard changes?" (Keep editing kept the text; Discard returned to Details with the title unchanged); Cancel without changes left at once; system Back with the title field unfocused showed the same prompt. |
| PA08 | Details: icon, title, amount, Overdue pill, facts with Currency / timezone, "✓ Mark as paid", Notes card, Manila timestamps. |
| PA09 | Settings cards and the fixed bottom-right "✓ Save". |
| PA10 | Reminders: status card, schedule and quiet-hours cards, the readiness section expandable with Android's status and tools, footer "Save only" / "Save & enable" while off. |
| Themes and sizes | Dark theme on Dashboard and Dues; 200% text on Dues (the count and Reset link collided and were given spacing afterwards); 320 dp on Calendar (seven columns, words fit) and Dashboard. |

Fixed during the pass: the Reminders status lines used body text (now supporting text); Dues and Calendar kept a blank band above the bar because their bottom padding sat outside the list (`PageLayout` `scrollsInside`); the Dues count and Reset link overlapped at 200% text.

## Limits

- **Back with a focused text field:** on the emulator, whose keyboard app is stopped for screenshots, system Back while the title field kept focus did not show the prompt and left the text in place. Android's input method consumed the presses. On a phone the first Back closes the keyboard and the next shows the prompt; this needs the phone check.
- **Launch time:** the first two launches after installing took about 28–32 seconds and Android reported "not responding" during startup; logcat showed the emulator still busy after the install. Later launches took 2.9–4.3 seconds. Watch the first launch on the phone.
- **Not observed:** the phone, TalkBack, the floating bar's bottom inset with a system navigation bar (this emulator image has none), Reminders while on (footer "Disable" / "Save schedule").
