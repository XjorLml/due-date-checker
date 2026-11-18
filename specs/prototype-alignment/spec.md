# Live prototype alignment (look and layout)

Status: **decided and implemented 2026-11-07; automated and emulator evidence ([evidence](../../docs/delivery/evidence/PROTOTYPE-ALIGNMENT-2026-11-07.md)); phone check pending** (see [tasks](tasks.md)). The user asked on 2026-11-07 to "update the documentation based on the current LIVE PROTOTYPE then implement all page changes". The live prototype is the default Focus view of the [design exploration](../../prototypes/design-exploration/index.html#a/dashboard) (refinement "Focus", Essential-first editor, sticky actions). Before any change, the user answered six scoping questions; each answer took the recommended option.

| Question | Decision (2026-11-07) |
|---|---|
| Scope | **Look and layout of existing features first.** New features shown in the prototype are deferred (see [Out of scope](#out-of-scope)). |
| Conflicts with later decisions | **Keep the later decisions.** Where the prototype predates a decision the user made and shipped (UX01–UX16), the app keeps that decision and takes only the prototype's look. |
| Round 2 (UX17–UX27) | **The prototype settles UX19, UX22 and UX25**; the other round 2 items stay open in the [decision lab](../ux-improvements-2/spec.md). |
| Header | **Prototype header, lowercase wordmark**: "duewise" plus a bell button on the four root pages. The app's launcher name stays "Duewise". |
| Search | **Follow the prototype**: search and filters move from Dashboard to Dues, and Dashboard becomes a summary. This reverses UX09 "keep as is". |
| Calendar counts | **Use "1 due" / "2 dues"** under the day number. This settles round 2 item UX24. |

Governing documents: [PRD](../../docs/PRD.md) (UX09 and UX22 rule changes, recorded under [Prototype alignment decisions](../../docs/PRD.md#prototype-alignment-decisions-2026-11-07)), [DESIGN](../../docs/DESIGN.md) (component and page specifications, version 2.2.0) and the [SDD](../../docs/SDD.md) (view-model contracts that move from Dashboard to Dues). The prototype's own record is [SYSTEM.md](../../prototypes/design-exploration/SYSTEM.md); it stays a non-normative source.

## Kept decisions

The prototype predates these choices; the app keeps them and draws them in the prototype's style.

| Topic | Prototype (2026-11-04) | App keeps |
|---|---|---|
| Dates | `Nov 02, 2026`, `Nov 02` | `02 Nov 2026`, `02 Nov` (UX07, R03) |
| Details timestamps | UTC | Asia/Manila, labelled "Manila" (UX12) |
| Delete | Confirm, no Undo | Confirm, then 5-second Undo (UX06) |
| Editor order | Title, Amount, Date; Type in a disclosure after Amount | Title, Type, Amount, Date (UX11); Type drawn as the prototype's compact "Type: Bill · Change" row |
| Repeat | A preference marked "Concept" | Real recurrence for Bill and Subscription (UX03), without a "Concept" tag |
| Earlier paid dues | Always listed | Folded into one row (UX02) while no search or filter is active |
| Details payment | "Mark paid" behind a confirmation | Immediate Mark as paid / unpaid with a message (UX01, UX06) |
| Settings | No data card | "Your data" export and import card (UX05) |
| Reminders | Simulation | Real Android reminders with permission, background and maker information (F05, UX04) |

## Out of scope

**Subsequent authorization, 2026-11-08:** the [native Philippine preset slice](../ph-categories-native/spec.md) implements detailed preset categories only. Its PC scenarios and evidence own that extension. The following exclusions describe the November 7 alignment decision; they do not defer the later preset-only approval. Custom category/biller management and the other unapproved concepts remain separate.

Deferred to a later round, each needing its own spec, PRD review and, for data, a migration: detailed categories with their own icons (Internet, Electricity…), billers/providers, the My Categories & Billers manager, per-due reminder offsets, the Calendar Week view, the Details Reschedule button, the Dashboard "Concept" extras of other refinements, and quick-paid confirmation. The prototype's sample-data bar ("Sample · 04 Nov 2026") is studio chrome, not app UI. Round 2 items UX17, UX18, UX20, UX21, UX23, UX26 and UX27 stay open.

## Page targets and acceptance

Light and dark themes, 100% and 200% text and 320 dp phones apply to every scenario, as do offline operation, restart persistence and the storage-failure guards. "Root pages" are Dashboard, Calendar, Dues and Settings; "pushed pages" are Details, the editor and Reminders.

| ID | Scenario | Expected result |
|---|---|---|
| PA01 | Open any root page. | A header row shows the lowercase "duewise" wordmark (brand mark plus text) and a bell button named "Reminders" that opens Reminders. Dues no longer shows its separate Reminders chip. |
| PA02 | Open a pushed page. | Android's title bar is replaced by the prototype header: "‹ Back" on the left (Details, Reminders), the page title centred, and one icon action on the right: a pencil on Details, a bin on the editor in Edit mode. Back, the header button and system Back behave as before (the editor asks first, see PA07). |
| PA03 | Open Dashboard with dues. | "A clear plan for today." and the date; the upcoming label, total and meta line (count, range and "N without an amount, excluded"); a "Next date · 05 Nov  Rent  →" row that opens that due; three inline counters (Overdue, Today, Paid n/total) that open Dues filtered to that status; "Needs attention" (unpaid dues dated today or earlier, first three, with "All dues ›") and "A little further ahead" (unpaid dues after today up to month end, first three); the footnote "Counts are global. Amounts cover known unpaid dues from today to month end." No search or filter controls. Each section has a calm empty line when it has no rows. |
| PA04 | Open Dues; search, filter, reset. | "Your dues." with "Date order · all active saved dues"; "Find by title" search; status chips All / Overdue / Due today / Upcoming / Paid; a Type filter (All types, Bill, Task, Subscription, Other); "N matching dues" with "Reset filters". Search and filters follow the former Dashboard rules (trimmed, case-insensitive title substring; one status; one type; AND). With no search or filter, earlier paid dues fold as UX02 describes; with any active, every match shows. Counters on Dashboard arrive here with that status selected. |
| PA05 | Look at any due row (Dues, Dashboard, Calendar). | A type icon in a tinted circle, the title, "05 Nov · Bill", a "⟲ Every month" line for repeating dues, and on the right the amount (or quiet "No amount") above the status as glyph and word in the status colour, without a filled pill. Tapping opens Details; the spoken text is unchanged. |
| PA06 | Open Calendar; pick days. | Month name with ‹ › buttons; weekday initials; each day cell shows the number and, when it has dues, "1 due" / "2 dues" beneath; the selected day is filled; a row "Monday first · includes paid" with a "Today" link; the selected date as a section heading with its rows or "Nothing due. A little breathing room."; the footnote "Add defaults to Today, independently of this selected date." No Week view. |
| PA07 | Add or edit a due, then leave with and without changes. | "Required fields are marked."; labels carry "required" / "optional"; Title, then a "Type: Bill · Change" row, Amount with "Required for Bill." and "Will save as ₱1,699.00.", Due date with "02 Nov 2026 · Manila. Past dates are allowed."; Repeat (Bill and Subscription) and Notes as collapsible rows showing their value; in Edit, a "Paid · applies on Save" check row; in Add, "New dues start unpaid."; footer Cancel / ✓ Save. With changes, Cancel, the header and system Back ask "Discard changes?" (Keep editing / Discard); without changes they leave at once (UX22). |
| PA08 | Open Details. | Type icon, title, amount (or quiet "No amount"), status pill; facts Due date, Due type, Repeats (repeating dues), Currency / timezone "PHP / Asia/Manila"; the next-occurrence line where it applies; a primary "✓ Mark as paid" (or "Mark as unpaid"); a Notes card; Created and Updated in Manila time. Edit is the header pencil; there is no "Edit due" or "Back to list" button. |
| PA09 | Open Settings; change and save. | "Make it feel like you." with "A few preferences. A clearer routine."; Appearance and Start of week cards; a Reminders row with a bell and its saved state; the Your data card; a note "Saved on this device. No account, bank connection or currency switch."; the line "Save applies both preferences. Leaving this page discards a draft; a Reminders visit keeps it."; a "✓ Save" button fixed at the bottom right above the bar (UX25: Save stays). |
| PA10 | Open Reminders while off, turn on, change, turn off. | "A gentle nudge." / "Only when you ask for one."; a status card ("Reminders off" or "Reminders on" with "All active, unpaid dues" and, when on, the next check time); "Your schedule" card (day-before check, time); "Quiet hours" card (From, Until, explanation); a collapsed "Notification readiness & privacy" section holding Android's notification and battery status, maker steps, Test notification, the two Android settings buttons and Refresh. Footer: off → "Save only" and "Save & enable"; on → "Disable" and "Save schedule" (UX19). Commands and rules are unchanged. |

## Shared acceptance rules

Every existing behavior not named above stays: R01–R08, F01–F07, the UX01–UX16 decisions as kept above, reminders reconciliation and the editor's storage-failure paths. Tests that assert moved behavior move with it (Dashboard search and filters to Dues). Each page is checked on the emulator in light and dark, at 200% text and at 320 dp, then on the phone.

Preset-category follow-up, 2026-11-08: [native Philippine categories](../ph-categories-native/spec.md) now separately authorizes the fixed catalog and category icons/picker. Custom categories and billers are still deferred. Earlier alignment evidence does not verify this extension.
