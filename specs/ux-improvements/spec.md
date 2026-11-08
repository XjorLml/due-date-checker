# UX improvements from the 2026-11-06 review

Status: **all sixteen items decided 2026-11-06 and implemented 2026-11-06/07 in five waves (UX09 needed no change); emulator evidence per wave, phone and TalkBack checks pending** (see [tasks](tasks.md)). The user asked on 2026-11-06 to "update the docs with these finding then create a plan to implement starting with the highest impact to the lowest", reviewed the options in the [HTML decision lab](#html-decision-lab) and pasted its export the same day. Findings and evidence are in the [UX review](../../docs/delivery/UX-REVIEW-2026-11-06.md); the order and approach are in the [plan](plan.md); progress is in [tasks](tasks.md).

Decided product behavior is recorded in the [PRD UX decision rules](../../docs/PRD.md#ux-review-decisions-2026-11-06) and decided presentation in [DESIGN](../../docs/DESIGN.md#ux-review-decisions-2026-11-06); both are marked not implemented until each item ships, and the rules they replace stay in force until then. SDD contracts change with the implementation of each item. Large items (UX03 recurrence, UX05 export) get their own spec folders when started.

## Decision log

Source: the decision-lab export pasted by the user on 2026-11-06 (no notes were added). Reasons were not stated beyond the options' listed pros and cons.

| ID | Choice | Matches recommendation | Consequence recorded |
|---|---|---|---|
| UX01 | A — Mark as paid on Details | Yes | PRD F01 Details actions |
| UX02 | A — Collapse earlier paid dues | Yes | PRD R03 presentation |
| UX03 | A — Next one appears when you mark paid (Never/Weekly/Monthly/Yearly; 31st → last day of shorter months) | Yes | PRD recurrence exclusion lifted for this rule set. Follow-up 2026-11-06: **only Bill and Subscription can repeat** |
| UX04 | A — Steps for the phone's maker | Yes | F05/DESIGN text only |
| UX05 | A — Export to a file you choose, import into an empty app | Yes | PRD export exclusion lifted for this rule set. Follow-up 2026-11-06: **JSON with a format version number** |
| UX06 | **B — Message with Undo for delete** | No (A recommended) | PRD "no Undo in v1" replaced for this case: a 5-second Undo after Delete. Adds a guarded restore contract (SDD) and its tests; effort raised to L |
| UX07 | A — `06 Nov 2026` in the editor | Yes | PRD editor date rule |
| UX08 | A — Digits and decimal point only | Yes | None (phone check) |
| UX09 | A — Keep as is | Yes | No work. Reversed 2026-11-07 by the [prototype alignment](../prototype-alignment/spec.md): search and filters moved to Dues |
| UX10 | A — Native buttons, same look | Yes | DESIGN counters |
| UX11 | A — Title → Type → Amount → Date | Yes | DESIGN / Focus editor order |
| UX12 | A — Manila time on Details | Yes | PRD F01 timestamp labels |
| UX13 | A — Cap label size in the bar | Yes | DESIGN bottom bar |
| UX14 | A — Quieter body text | Yes | DESIGN optional-amount adaptation |
| UX15 | **B — Shrink cells to fit** | No (A recommended) | DESIGN exception: when seven 44 dp columns do not fit (screens narrower than about 350 dp; the option text said 340), the columns shrink, about 40 dp wide at 320 dp (under DESIGN's 44 dp, above WCAG 2.2 AA's 24 px minimum) |
| UX16 | A — Smaller headings in landscape | Yes | DESIGN layout |

## Options considered

The table below is the option set the user chose from; each row states the outcome of the recommended option. Where the decision log differs (UX06, UX15), the decision log wins. "Decision" lists the options; the first is the recommendation.

| ID | Proposed outcome | Decision needed (recommended first) | Rule to update when decided |
|---|---|---|---|
| UX01 | Details shows a primary "Mark as paid" button for an unpaid due and "Mark as unpaid" for a paid one. One tap saves the change through the existing guarded update (same ID, CreatedAt, tombstone guard, reminder reconciliation), refreshes the status badge and announces the change. Edit due stays available. | **A. Details button only** (one tap from Details, two from the list; reuses the stored-copy update). B. Also a swipe action on rows (DESIGN's swipeable item; more gesture and TalkBack work). C. A checkbox on every row (fastest, but easy to tap by mistake and crowds the row). | PRD F01 Details actions; DESIGN item detail (as built); F01 spec |
| UX02 | Opening Dues shows what is next first. Paid dues dated before today collapse into one "Show N earlier paid dues" row at the top; tapping it expands them in place. Everything else keeps ascending date order, and unpaid overdue dues stay visible. | **A. Collapse earlier paid dues** (keeps R03's date order, smallest change). B. Group into Overdue / Upcoming / Paid sections (clearer, but a new list structure). C. Scroll to today on open (no hiding, but a jump that is easy to miss and hard for screen readers). | PRD R03 wording; DESIGN Dues list (as built) |
| UX03 | A Bill or Subscription can repeat monthly (also weekly and yearly). Marking one paid creates the next occurrence with the same title, amount, type and notes on the next date, so recurring bills never need re-entry. | Repeat choices (**monthly, weekly, yearly**); when the next one appears (**on Mark as paid**, or ahead of time); month-end rule (**31st → last day of shorter months**); whether Task/Other can repeat. Detailed in its own F07 recurrence spec. | PRD exclusions and a new F07 recurrence rule set; SDD schema 5; DESIGN repeat picker |
| UX04 | When reminders are switched on, the Reminders page adds plain steps for the phone's maker when it is known to stop background work (first Xiaomi/Redmi/POCO: Autostart on, Battery saver "No restrictions"), next to the existing Android status text and app-settings button. | **A. Maker-specific text plus the existing settings button** (robust; no private settings screens). B. Deep-link into maker settings screens (one tap closer, but these screens change between MIUI/HyperOS versions and can crash). | F05 spec; DESIGN notification settings |
| UX05 | Settings offers "Export dues" that saves a file the user chooses (all active and deleted dues, readable later) and, in a second step, "Import" that restores it into an empty app. Nothing leaves the phone unless the user moves the file. | **A. User-chosen export file, then import** (no cloud, keeps today's privacy choice). B. Turn on Android Auto Backup to the user's Google account (automatic, but sends financial data to cloud backup, reversing the deliberate `allowBackup=false`). Format (**JSON with a version number**, or CSV for spreadsheets). Needs an ADR. | PRD exclusions (export) and F04; SDD data-loss limits; ADR |
| UX06 | After Save, Mark as paid/unpaid and Delete, a short message ("Saved", "Marked paid", "Deleted") appears above the bottom bar and is announced to screen readers. No Undo. | **A. Short message, no Undo** (PRD keeps no Undo in v1). B. Message with Undo for delete (needs a PRD change and restore rules). | DESIGN toasts; PRD stays as is for option A |
| UX07 | The editor date reads like the rest of the app (`06 Nov 2026`). | **A. `dd MMM yyyy`** in the date field. B. Keep `yyyy-MM-dd`. | PRD business rule for the editor date format |
| UX08 | The amount keyboard offers digits and the decimal point only. | None expected: use Android decimal-number input without the sign flag. Must be checked on the phone's keyboard. | None |
| UX09 | Dues and Dashboard have distinct jobs. | Decide after UX02 ships: **A. Keep Dues as the full list and leave search on Dashboard** (smallest). B. Add search to Dues. C. Start the app on Dashboard and rename Dues "All dues". | PRD F02/R03 page roles if B or C |
| UX10 | Dashboard counters are native buttons, so screen readers announce them as buttons. | None expected: replace the tappable borders with styled Buttons that keep the same look. | DESIGN summary counters |
| UX11 | Type is chosen right after the title, so the amount hint is already correct when the user reaches Amount. | **A. Title → Type → Amount → Date** (Type as the existing picker). B. Keep the current order. | DESIGN and Focus adoption editor order |
| UX12 | Details shows Created and Last updated in Manila time (`06 Nov 2026, 11:11 Manila`). | **A. Manila time with the zone named.** B. Keep UTC. | PRD F01 ("explicit UTC labels"); DESIGN item detail open finding |
| UX13 | Bar captions stay readable at 200% text and on 320 dp screens. | **A. Cap caption text scaling for the bar only** (icons still identify each tab; TalkBack reads the full name). B. Two-line captions. C. Hide unselected captions. | DESIGN bottom bar |
| UX14 | "No amount" on Details is visibly different from a peso amount. | **A. Secondary text colour at body size.** B. Keep HeroAmount. | DESIGN F07 optional-amount adaptation |
| UX15 | Calendar on 320 dp screens. | **A. Keep 44 dp cells and sideways scroll** (target size wins). B. Shrink cells to fit 7 columns below 340 dp (breaks the 44 dp target). | None for A |
| UX16 | Landscape pages show more rows. | **A. Smaller headings in landscape only.** B. Leave as is (landscape is rare for this app). | DESIGN layout |

## Shared acceptance rules

Every item keeps: offline operation, restart persistence, R02–R07 behavior not named above, the startup and storage-failure guards, light/dark and 200% text, and existing tests. Each item adds failing tests first, then emulator checks, then a phone check once the phone holds a build this computer can update (see [STATUS](../../docs/delivery/STATUS.md)).

## HTML decision lab

Added 2026-11-06 at the user's request ("show it on the index.html so i can decide properly when im visually seing this"): a **UX decisions** tab in the [design exploration](../../prototypes/design-exploration/index.html#ux-decisions) (`decision-lab.js`, `decision-lab.css`, small hooks in `index.html` and `app.js`). It is a non-normative decision aid like the [interaction lab](../interaction-lab/spec.md); choosing there records a browser-local preference, not a decision. Since 2026-11-07 the tab opens on [round 2](../ux-improvements-2/spec.md#html-decision-lab) (UX17–UX27); this round stays available as a record through the round switch, with its drawings unchanged and each decided option marked "Decided".

| ID | Scenario | Expected result |
|---|---|---|
| DL01 | Open the lab data. | Sixteen decisions in the plan's rank order with the plan's reach/frequency/severity; impact never rises down the list. |
| DL02 | Inspect any decision. | A "Today" drawing of the native app plus one drawing per option; exactly one option is recommended; every drawing is decorative (`aria-hidden`) and has a text description. |
| DL03 | Export with nothing chosen. | Markdown lists all sixteen as "Not chosen" and states that choices are preferences until confirmed. |
| DL04 | Open `#ux-decisions` or the header tab. | The lab replaces the compare/explore views and the tab is pressed. |
| DL05 | Step through every decision at 1440 px. | Today plus all options render; the page never scrolls sideways. |
| DL06 | Choose, change, add a note, reload, toggle off. | The list shows the chosen key, the status line announces it, choices and notes survive reload, the export table carries both, and choosing the same option again removes it. |
| DL07 | Phone width (390 px). | The decision list becomes a swipeable strip; no sideways page scroll. |
| DL08 | Leave the lab. | Compare and explore views and their hashes still work. |

Drawings use the native light tokens from `Colors.xaml` and the layouts seen in the 2026-11-06 emulator screenshots; they are illustrations, not pixel captures, and show light theme only.

## Out of scope

Accounts, sync, budgets, statistics, onboarding and widgets stay on the [roadmap](../future-pages/plan.md). This plan does not authorize release, signing changes or pushing to `main`.
