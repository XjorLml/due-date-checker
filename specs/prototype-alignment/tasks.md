# Live prototype alignment tasks

See the [spec](spec.md) and [plan](plan.md). Evidence: [PROTOTYPE-ALIGNMENT-2026-11-07](../../docs/delivery/evidence/PROTOTYPE-ALIGNMENT-2026-11-07.md).

## Decisions and documents — 2026-11-07

- [x] Capture the live prototype's ten screens (Dashboard, Dues, Calendar, Week, Details, Edit, Add, Settings, Reminders, Categories) in headless Edge and compare them with the app (agent-observed; screenshots kept in the session scratchpad, not the repository).
- [x] Six scoping answers from the user recorded in the [spec](spec.md).
- [x] PRD prototype alignment decisions (UX09 reversed, UX22 decided), DESIGN 2.2.0 decision notes, round 2 decision log (UX19, UX22, UX24, UX25), round 1 note on UX09.
- [x] DESIGN supersession notes on the older as-built sections, SDD contract, TEST-PLAN section, docs index, STATUS and evidence.

## Implementation — 2026-11-07

- [x] Shared: `Glyph` (prototype path icons as themed vectors), `RootHeader`, `PushedHeader`, `StatusLine`, `OverlayButton` style; `DueRow` with type icon, repeat line and plain status; `IDueRow` gains `HasAmount`, `IsRepeating`, `RepeatLineText`, `TypeGlyph`; the meta line drops the repeat.
- [x] Dues: search, status chips, Type filter, matching count and Reset moved from Dashboard into `DuesListViewModel`; the fold applies only with no filter; `ShowStatus` and `AppShell.ShowDues`.
- [x] Dashboard: summary-only view model with `Attention` and `Later` (three each) and calm empty lines; counters and "All dues" open Dues.
- [x] Calendar: "1 due" / "2 dues" (UX24), drawn cells with an overlay button, Today link under the grid, agenda footnote.
- [x] Details: header pencil, type icon, Currency / timezone, "✓ Mark as paid", Notes card; Back to list only when unavailable.
- [x] Editor: prototype layout, "Type: Bill · Change" action sheet, collapsible Repeat and Notes, Paid check row, bin in the header, `HasChanges` and "Discard changes?" on Cancel and system Back (UX22).
- [x] Settings: cards, privacy note, fixed bottom-right "✓ Save" (UX25).
- [x] Reminders: prototype layout, readiness section (opens itself when reminders are on and maker steps exist), state footer (UX19).
- [x] List pages scroll to the bar (`PageLayout` `scrollsInside`).

## Verification

- [x] Core 162 and App 279 tests passed; Debug build and Release compile 0 warnings (agent-observed).
- [x] Emulator pass: every page in light, Dashboard and Dues in dark, Dues at 200% text, Calendar and Dashboard at 320 dp, the discard prompt, the type sheet, header Back and the bell (agent-observed; see the evidence limits).
- [x] Copy `Duewise-2026-11-07d.apk` to the phone's Downloads (2026-11-07, checksum verified; the never-installed `…07c` removed).
- [ ] The user installs it from the file manager.
- [ ] Phone check per the [test plan](../../docs/delivery/TEST-PLAN.md#prototype-alignment-verification-pa01pa10), including Back with the keyboard open and the bar's bottom inset.
- [ ] TalkBack review of the new headers, overlay buttons and Dashboard rows (deferred by user direction, as for earlier work).
- [ ] The decision lab still shows round 2 items UX19, UX22, UX24 and UX25 as open; mark them decided there.
