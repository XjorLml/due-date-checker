# Focus page content — task tracking

Authorization: user request on 2026-11-05 ("implement ui update for page and components"). Scenarios: [spec](spec.md). Approach: [plan](plan.md). Results are agent-observed unless stated otherwise.

## Implementation

- [x] PRD F02 rule for the upcoming total; F04 note that Settings shows the saved reminder state read-only.
- [x] `IDueRow` interface, `DueRow` ledger control, `ShortDateText` and `MetaText` on the projections.
- [x] Dashboard view model members (`MonthLabel`, `UpcomingTotalText`, `UpcomingMetaText`, `NextDue`, `HasNext`, `NextTitle`, `NextDateText`, `PaidCounterText`) and page: summary, counters wired to `SelectFilter`, chips, ring removed, Refresh inside the error card only.
- [x] Calendar controls (‹ › icon buttons, Today chip), `DayButton` cells, agenda section line, row template.
- [x] `SegmentedControl`, Settings page groups, Reminders row with `ReminderSummary`, local-data notice, `HasLoadError` for Refresh.
- [x] Details facts layout (category eyebrow, title, amount, badge, divided rows, timestamp captions).
- [x] Dues row template, caption, Reminders chip, Refresh inside the error card only.
- [x] Styles: `HeroAmount`, `RowTitle`, `RowAmount`, `FactValue`, `Divider`, `NoticeCard`, `SettingsRow`, `SegmentHost`, `CounterCard`, `FilterChip(+Selected)`, `IconButton`, `LinkButton`, `SegmentButton(+Selected)`, `DayButton(+Selected)`.
- [x] App tests: `DashboardSummaryTests` (upcoming total, next due, zero case, `IDueRow` contract).

## Verification — 2026-11-05

- [x] `dotnet test` App: **161 passed** (158 existing + 3 new). Core untouched.
- [x] `dotnet build -c Debug -f net10.0-android`: **Build succeeded**, 0 warnings, 0 errors.
- [x] `dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false`: **0 warnings, 0 errors** (keystore password still wrong in `local.signing.props`, so signing stays bypassed).
- [x] Emulator pass (AOSP Android 35, seeded four states, light/dark/200%): FP01–FP08 observed; one 200% counter clipping defect fixed and re-verified; a transient bottom-bar ghost ruled out by retakes. Record: [FOCUS-PAGES-2026-11-05](../../docs/delivery/evidence/FOCUS-PAGES-2026-11-05.md).

## Remaining acceptance

- [ ] Phone verification, TalkBack review (including the counter cards, which are tappable Borders rather than native buttons) and intended-key signing remain held (see [STATUS](../../docs/delivery/STATUS.md)).
