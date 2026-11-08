# UI consistency: one status badge, one title per page

Authorized by the user on 2026-11-05 ("start the status-badge and duplicate title fixes") after the [gap assessment](../../docs/delivery/evidence/FOCUS-ADOPTION-2026-11-05.md) and the stashed 2026-10-27 design review (findings P3, P4 and S1). Scope: presentation of existing data. Release remains held. State and evidence: [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).

## Scope and governing contracts

[PRD existing dues workflows](../../docs/PRD.md#existing-dues-workflows) now state that Dues rows and Details show the same computed status as Dashboard and Calendar; the [F02 rules](../../docs/PRD.md#f02-dashboard-rules) own that classification. [DESIGN 2.0.0 §7](../../docs/DESIGN.md#status-badges) owns the badge; [§7 App bar](../../docs/DESIGN.md#app-bar) owns where page titles live. [SDD §6](../../docs/SDD.md#6-ui-and-navigation) owns the shared projection and the navigation-bar rule.

In scope: a shared status badge control used on Dues cards, Dashboard and Calendar rows and Details; the Dues and Details projections computing the F02 date status against the Manila calendar date; root pages (Dues, Dashboard, Calendar, Settings) hiding the Shell navigation bar so their page heading is the only title, with the brand lockup moved into the Dues header; pushed pages (editor, Details, Reminders) keeping the Shell title and dropping their duplicate in-page heading.

Out of scope: any change to sorting, filtering, counts, reminders, data or the status rules themselves; the "Due soon" state in DESIGN §10, which has no product rule; the always-visible Refresh/Retry buttons (P2), Calendar controls, Dashboard filter chips and Reminders layout, which remain open findings.

## Acceptance scenarios

| ID | Observable outcome | Verification boundary |
|---|---|---|
| UC01 | Every due row on Dues, Dashboard and Calendar, and the Details page, shows one pill reading exactly Paid, Overdue, Due today or Upcoming, with a tinted surface and a glyph; the word is always present and the pill's accessible description is "Status: word". | Markup review; emulator tree and screenshots. |
| UC02 | A Dues row's status matches what Dashboard shows for the same due on the same Manila date: paid dues are Paid regardless of date; unpaid dues are Overdue, Due today or Upcoming by due date. | App tests (Dues, Details, Dashboard, Calendar projections); emulator with the device date moved. |
| UC03 | Paid status still appears as text on Dues (R05 text indication) and Details; no colour-only state. | Markup review. |
| UC04 | Dues, Dashboard, Calendar and Settings show no Shell navigation bar; each has exactly one heading, and Dues shows the brand mark and name once at the top of its content. | Emulator tree (no toolbar title node) and screenshots. |
| UC05 | Add/Edit, Details and Reminders show the Shell title with the Back arrow and no second in-page heading; the editor keeps its supporting line, Delete header action and fixed footer. | Emulator tree and screenshots. |
| UC06 | Existing App tests pass with the projections carrying the new status; create, edit, pay and navigate behave as before. | App suite; emulator smoke. |

## Evidence limits

Emulator observations are agent-observed, not phone acceptance. TalkBack announcement of the badge description and the loss of an in-page level-1 heading on pushed pages need the deferred assistive review.
