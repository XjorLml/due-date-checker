# UX improvements implementation plan

Order: highest impact first, as requested on 2026-11-06. All options were decided the same day ([decision log](spec.md#decision-log)); the approaches below follow those choices. Findings: [UX review](../../docs/delivery/UX-REVIEW-2026-11-06.md). Outcomes and decisions: [spec](spec.md). Progress: [tasks](tasks.md). The [roadmap](../future-pages/plan.md#ux-review-priorities-2026-11-06) points here for the ordering of these items.

## Ranking method

Impact = reach × frequency × severity, each scored 1–3 for the PRD users (students and employees in the Philippines):

- **Reach:** 3 = every user; 2 = one persona or a large group (for example people with monthly bills); 1 = a smaller group (screen-reader users, 320 dp phones, landscape).
- **Frequency:** 3 = every use or every due; 2 = while adding or editing; 1 = rarely (changing phones, opening Details timestamps).
- **Severity:** 3 = the user can miss a payment or lose data; 2 = repeated friction or confusion; 1 = cosmetic or slight.

Ties are broken by smaller effort first. Effort: **S** = one screen or view model and its tests; **M** = several files, a new interaction or a PRD rule change; **L** = new data, schema or platform work with its own spec and ADR.

One judgement call is recorded rather than hidden: UX05 (export) scores 9 because losing a phone is rare, yet its harm cannot be undone. If irreversible loss should outrank frequency, move UX05 to the front; nothing else in the order changes.

| Rank | ID | Reach | Freq. | Sev. | Impact | Effort |
|---|---|---|---|---|---|---|
| 1 | UX01 Mark as paid on Details | 3 | 3 | 2 | 18 | S |
| 2 | UX02 Dues shows what is next | 3 | 3 | 2 | 18 | M |
| 3 | UX03 Recurring dues | 2 | 3 | 3 | 18 | L |
| 4 | UX04 Reminder reliability steps | 2 | 3 | 2 | 12 | S |
| 5 | UX05 Export and import | 3 | 1 | 3 | 9 | L |
| 6 | UX06 Confirmation messages, Undo after Delete | 3 | 3 | 1 | 9 | L |
| 7 | UX07 Editor date format | 3 | 2 | 1 | 6 | S |
| 8 | UX08 Decimal-only amount keyboard | 3 | 2 | 1 | 6 | S |
| 9 | UX09 Dues vs Dashboard roles (keep as is) | 3 | 2 | 1 | 6 | — |
| 10 | UX10 Counters as native buttons | 1 | 3 | 2 | 6 | S |
| 11 | UX11 Type before Amount | 2 | 2 | 1 | 4 | S |
| 12 | UX12 Manila timestamps on Details | 3 | 1 | 1 | 3 | S |
| 13 | UX13 Readable bar captions | 1 | 3 | 1 | 3 | M |
| 14 | UX14 Quieter "No amount" on Details | 2 | 1 | 1 | 2 | S |
| 15 | UX15 Calendar shrinks on narrow phones | 1 | 2 | 1 | 2 | S |
| 16 | UX16 Landscape headings | 1 | 1 | 1 | 1 | S |

## Delivery waves

Work proceeds in rank order. Each wave ends with both test suites, a Debug build, a Release compile, an emulator pass and updated docs; a wave's items can be reviewed together.

| Wave | Items | Why grouped |
|---|---|---|
| 1 — daily friction | UX01, UX02 | Highest impact, small to medium, no schema change. UX03 builds on UX01's paid action. |
| 2 — never miss a payment | UX03, UX04 | Recurrence needs a schema migration and its own spec; reminder guidance is small and lands with it because both protect against a missed payment. |
| 3 — protect the data | UX05 | Needs an ADR (privacy vs durability), a file format and a restore path. Export can ship before import. |
| 4 — polish batch | UX06–UX12 | Mostly small; UX06 is larger since Undo was chosen (guarded restore). UX07 and UX12 share one PRD update; UX09 needs no work (kept as is). |
| 5 — low impact | UX13–UX16 | Cosmetic or narrow-reach; do when convenient. |

## Item approach

**UX01 — Mark as paid on Details (S).** Add `TogglePaidCommand` to `DueDetailViewModel`: inside one `DatabaseDispatcher.RunAsync`, re-read the due, reject null or deleted (unavailable state), write `current with { IsPaid = !IsPaid, UpdatedAt = now }` through `IDueRepository.UpdateAsync` (the reminder-aware decorator then reconciles reminders), reload `Item`, announce the new status. Busy guard against double taps; storage failure shows a message and leaves the stored row unchanged. `DueDetailPage.xaml`: primary button above Edit due; Edit due becomes secondary. Tests first: toggle both ways keeps Id/CreatedAt; deleted-by-another-writer becomes unavailable without resurrection; failure leaves the row unchanged; repeated taps write once; reminder cancellation on paid (existing ReminderTests pattern). Docs: PRD F01, DESIGN item detail, F01 spec.

**UX02 — Dues shows what is next (M).** `DuesListViewModel` splits the loaded snapshot into "earlier paid" (paid and dated before today in Manila) and the rest; exposes `EarlierPaidCount`, `ShowEarlierPaid` and the visible list. `DuesListPage` adds a collapsible header row ("Show 12 earlier paid dues" / "Hide earlier paid dues"). The expand state resets on restart. Tests first: classification at the Manila date boundary; overdue unpaid never hidden; counts after toggle; empty and all-hidden states; refresh keeps the expand state. Docs: PRD R03 presentation, DESIGN Dues list, SDD list contract.

**UX03 — Recurring dues (L).** Start a dedicated `specs/f07-recurrence/` with its own scenarios. Likely shape: nullable `repeat_rule` column (schema 5, rebuild not needed for an added nullable column), domain `RepeatRule` value type with next-date calculation (month-end clamping), creation of the next occurrence in the same dispatcher operation as UX01's Mark as paid, reminders planned for the new row through the existing coordinator. Needs an ADR for the generation strategy. Tests: date arithmetic incl. 31st and 29 Feb, idempotence on repeated paid toggles, unpaid→paid→unpaid not creating duplicates, migration preservation.

**UX04 — Reminder reliability steps (S).** `IReminderPlatform` gains the device maker (Android `Build.Manufacturer`); `ReminderSettingsViewModel` adds a short maker-specific paragraph for Xiaomi/Redmi/POCO first (others later), shown only when reminders are on. Keep the existing status text and app-settings button; no deep links into private screens. Tests: text per maker and per status with a fake platform. Docs: F05 spec, DESIGN notification settings.

**UX05 — Export and import (L).** Start a dedicated F04 export spec and an ADR (user-chosen file vs Android Auto Backup). Likely shape: versioned JSON of every `dues` row (including tombstones and NULL amounts) plus reminder settings, written through the Android file picker (Storage Access Framework, no storage permission); import only into an empty database, validated before any write, inside one transaction. Tests: round trip preserves every field including NULL vs zero; corrupt or newer-version files rejected with no write; import refuses a non-empty database.

**UX06 — Confirmation messages with Undo after Delete (L; decided B).** A small app-level message host above the bottom bar (DESIGN toast: 3 s informational, 5 s with Undo; polite live region via `SemanticScreenReader.Announce`). Pages request a message after Save, Mark as paid/unpaid and Delete. Undo needs a new guarded repository operation, e.g. `RestoreAsync(id, deletedAt, now)` with `WHERE id = @id AND deleted_at = @deletedAt`, so it can only reverse the deletion just made (a later or repeated deletion is not undone, and a stale edit still cannot clear a tombstone); restore clears `deleted_at`, sets `updated_at` to the undo time, keeps every other field, and reconciles reminders through the reminder-aware repository. SDD §5.3 and the V1.3 soft-delete spec change with it. Tests: restore round trip keeps identity and fields; restore after the window or after another deletion does nothing; failure keeps the tombstone and reports it; reminders return for an unpaid restored due; no message on failure paths.

**UX07 — Editor date format (S).** `DatePicker Format="dd MMM yyyy"` with invariant month names; PRD rule updated. Emulator and phone check that the native picker still opens and the 200% layout holds.

**UX08 — Decimal-only keyboard (S).** An Android `EntryHandler` mapping for the amount entry only: `InputTypes.ClassNumber | InputTypes.NumberFlagDecimal` (no signed flag). Validation stays as the real guard. Phone check required (keyboards differ by maker).

**UX09 — Dues vs Dashboard roles (decided A: keep as is).** No work. Revisit only if UX02 in use shows the two pages still overlap.

**UX10 — Counters as native buttons (S).** Replace the three tappable `Border`s with `Button`s styled to match; keep `SelectFilter` and descriptions. Check TalkBack role when TalkBack testing resumes.

**UX11 — Type before Amount (S).** Move the Type block under the title in `DueEditPage.xaml`; adjust the "Title, amount and date first" caption. Update DESIGN and the Focus adoption spec's Essential-first order.

**UX12 — Manila timestamps (S).** `DueDetailViewModel.FormatTimestamp` converts to Asia/Manila (`TimeZoneInfo` already used by `ManilaCalendar`) and labels it; PRD F01 and DESIGN updated; tests with fixed instants around midnight UTC.

**UX13 — Readable bar captions (M).** In `DuewiseShellRenderer`, cap the caption text size for the bar only (Android `TextView` size in px rather than scaled sp, or a max scale); verify 100%/200% and 320 dp.

**UX14 — Quieter "No amount" (S).** Data trigger on Details: absent amount uses secondary text at body size.

**UX15 — Calendar shrinks to fit narrow phones (S; decided B).** In `CalendarPage`, drop `MinimumWidthRequest="322"` and the horizontal `ScrollView` once the columns cannot fit at 44 dp: cells take `(contentWidth − 6 × 2) / 7` when that is under 44 dp (about 40 dp at 320 dp), otherwise stay 44 × 48. Keep the 48 dp cell height. Check 320 dp at 100% and 200% text and that tapping each day still selects it; DESIGN records the exception.

**UX16 — Landscape headings (S).** Smaller heading style when width > height on root pages.

## Dependencies and order constraints

- UX03 depends on UX01 (the paid action is where the next occurrence is created).
- UX09 waits for UX02.
- UX07 and UX12 should share one PRD edit.
- UX05 import must never merge into existing data in its first version.
- Phone verification for every wave depends on restoring the phone's app from the laptop that holds its signing key, or on copying that key to this computer (see [STATUS](../../docs/delivery/STATUS.md)).

## Verification per item

1. Record the decision and update the governing doc text.
2. Write failing Core/App tests for the new behavior.
3. Implement; run Core then App tests; Debug build; Release compile.
4. Emulator: light, dark, 200% text; failure path where one exists.
5. Phone: same checks once possible; record evidence with date, artifact and observer.
6. Update STATUS, tasks and evidence; validate links.
