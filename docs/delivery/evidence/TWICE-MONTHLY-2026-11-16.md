# Twice-a-month repeat (PRD TM), 2026-11-16

Scope: [PRD TM](../../PRD.md#twice-a-month-repeat-2026-11-16) and [TM01–TM10](../../../specs/f07-twice-monthly/spec.md#acceptance-scenarios). The user decided and authorized the scope on 2026-11-16 and asked to "implement it" the same day. Technical choices: [ADR 011](../../adr/011-twice-monthly-repeat.md); contract: [SDD](../../SDD.md#twice-a-month-repeat-contract). All results below are agent-observed unless marked otherwise.

## Environment

Base `2cc92c1` plus the uncommitted working tree, which already held the user's unrelated changes. Those were left untouched.

- **Tests:** Windows Application Control blocked the freshly built test assemblies (`0x800711C7`, "An Application Control policy has blocked this file"), as in the [PH categories evidence](PH-CATEGORIES-2026-11-08.md). No policy was changed. The suites were run instead on a copy of `src/` and `tests/` in WSL (Linux) with .NET SDK 11.0.100, the net10.0 test hosts rolled forward to runtime 11.0.0 (`DOTNET_ROLL_FORWARD=Major`). That is not the Windows/.NET 10 environment of earlier evidence.
- **Android builds:** Windows `dotnet.exe` defaulted to SDK 11.0.100, which has no workloads (`NETSDK1147 … wasm-tools-net10`). Each build used SDK 10.0.400 with its MAUI workload (10.0.20/10.0.100) through a temporary `global.json` (`"rollForward": "disable"`) that was deleted after every build. No SDK or workload was installed.
- **Emulator:** AVD `DuewiseNotificationsCheck` (Android, x86_64), started for this pass and shut down afterwards.

## Automated and build results

| Check | Result |
|---|---|
| Baseline before changes (Linux copy) | Core 184/185, App 313/313. The one failure is `MoneyTests.FromDecimal_rounds_half_away_from_zero(0.015 → 2)`: on runtime 11 the `[InlineData(0.015)]` double reaches the test as 0.01499…, which rounds to 1. It is pre-existing, environmental and unrelated; it was not changed. |
| Core after changes | **211/212**; the only failure is the same baseline MoneyTests case. 27 new tests in `TwiceMonthlyTests`. |
| App after changes | **322/322**. 9 new tests in `TwiceMonthlyTests`. |
| `dotnet build -c Debug -f net10.0-android` (SDK 10.0.400) | Succeeded, 0 warnings, 0 errors (XAML compiled) |
| Same, `-p:EmbedAssembliesIntoApk=true`, for the emulator | Succeeded, 0 warnings, 0 errors. APK SHA-256 after the layout fix: `3b7ac57c8a5b7e38d090c4f1eddcf03074577237cc5500bf4643db85115b91e4` |
| `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false` (SDK 10.0.400) | Succeeded, 0 warnings, 0 errors (trimmed Release compile, signing bypassed; no APK was signed or distributed) |

Test order: the Core and App implementations were written before their tests, so there is no failing-first run for this slice. The tests then found one defect (below).

Existing tests updated for the intended contract change: schema-version assertions 6 → 7 (the "newer schema" probe 7 → 8); the "undefined repeat code" probe 4 → 5; export version 2 → 3 and its field list gaining `repeatDay2`; the "newer file" probes 3 → 4; the PH format test, which turns a current export into format 1, now also drops `repeatDay2`; the editor's `Repeats` list gains TwiceMonthly.

## Defects found and fixed

1. **Switching to Monthly kept the wrong day (found by the new App test).** `Recurrence.AnchorAfterEdit` kept any stored `RepeatDay` while the date was unchanged. A twice-a-month due on 15 Nov with days 4 and 19 would have become "Monthly on day 4". It now keeps only a Monthly/Yearly anchor, so the switch anchors on the 15th. A Core test covers it.
2. **The Repeat row squeezed its label at 200% text (found on the emulator).** The longer summary "Twice a month · 15 & 30" sat in an `Auto` column beside a `*` label column, which pushed "Repeat" into one letter per line in dark mode at 200% text. The row is now `Auto,*,Auto`, with the summary end-aligned and wrapping. Rechecked on the emulator.

## Emulator checks

The app's database on the emulator was at schema 6 with no dues. The installed app was a non-debuggable Release build. The new Debug build installed over it as an update, so the signing keys matched and the app data was kept. A first install without embedded assemblies aborted at startup (`No assemblies found … Fast Deployment`) before opening the database; the embedded-assemblies rebuild fixed that. The app is not changed by this.

- **TM01 upgrade:** two uniquely tagged temporary rows were added at schema 6 through `run-as … sqlite3`: a Monthly Bill on the 30th, and a deleted Task with no amount. Launching the app ran migration 007: `user_version` 7, both rows identical in all seventeen columns including `rowid` (compared by `diff`), `repeat_day_2` NULL, and `ix_dues_active_due_date` and `ix_dues_repeat_source` present. Dues listed the Monthly row as "Every month".
- **TM07 editor:** the Repeat picker listed Never, Weekly, Monthly, Twice a month and Yearly. Twice a month showed First day 15 and Second day 30, the summary "Twice a month · 15 & 30", and the new help line. With both days set to 15, Save showed "Choose two different days for Twice a month." in the error colour, opened the second-day picker (the same focus behavior as the date field) and saved nothing. With 15 and 30, it saved "TMQA twice" (₱6,250.00, 16 Nov 2026) as `repeat_rule` 4, `repeat_day` 15, `repeat_day_2` 30.
- **TM08 display:** the Dues row showed "Twice a month", with the accessible text "TMQA twice, Bill, repeats twice a month, …". Details showed "Repeats: Twice a month on days 15 and 30".
- **TM05/TM06 payment:** "Mark as paid" on Details showed "Next one added for 30 Nov 2026." and stored one unpaid row on 2026-11-30 with both days and the source link. Mark as unpaid and then paid again added nothing: still two rows, and no "Next one added" line.
- **Text size and theme:** at 200% text in dark mode the editor's pickers, captions and help text fit. After the layout fix the header row also fits.
- **Cleanup:** text size and theme were restored to 1.0 and automatic. Only the tagged test rows and their reminder-journal rows were removed, leaving 0 dues as found. The database stays at schema 7, and the emulator now has this Debug build installed instead of the earlier Release build.

Not checked on the emulator: a reminder notification's "repeats twice a month" text (covered by Core `ReminderText` tests), an export file on the device (covered by Core archive tests), light mode at 200%, and the editor's Mark-as-paid path (covered by App tests).

## Remaining acceptance

Phone checks with the intended signed build (TP procedure: [twice-a-month verification](../TEST-PLAN.md#twice-a-month-repeat-verification-tm01tm10)), TalkBack reading of the day pickers, and a run of the suites on the Windows/.NET 10 environment once Application Control allows it, or in CI. Release, signing and pushing to `main` remain held.
