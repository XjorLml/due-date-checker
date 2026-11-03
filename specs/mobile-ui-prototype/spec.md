# Mobile HTML design-review prototype

User request, 2026-11-04: create an HTML prototype from DESIGN.md, review page by page, recommend revisions suited to a mobile finance app, and add choices to select. This authorizes a local review artifact and documentation; it does not authorize implementing the selected directions in MAUI.

## Scope and authority

Use [DESIGN](../../docs/DESIGN.md#scope-and-authority), the latest [PRD](../../docs/PRD.md) rules for R02–R07 and F01/F02/F03/F04/F05/F07, and [SDD navigation](../../docs/SDD.md#6-ui-and-navigation). Review current page source and applicable tests before describing existing behavior. The [review](../../prototypes/mobile-review/REVIEW.md) owns recommendations; production contracts remain in the canonical documents. The [future roadmap](../future-pages/plan.md) retains ownership of deferred capabilities.

Deliver a buildless local HTML/CSS/JavaScript prototype with bundled licensed fonts, sample records, all current screens, and separately labeled four-step future onboarding. Provide per-page live layout choices, notes and a review-choice export. Demonstrate sample CRUD, search/filter, calendar, preference drafts, simulated reminders and shared read states. Preserve optional-versus-zero amount handling, payment-on-Save, date sorting, explicit PHP/Manila context and confirmed deletion without Undo.

No production app code, database, schema, signing, permissions, release state or real notification changes. No account, payment processing, bank connection, financial aggregation, recurring/timed items, cloud services or product export is introduced. Export contains reviewer decisions only. App state is temporary; only reviewer choices/notes persist in browser storage.

## Observable acceptance scenarios

Follow-up, 2026-11-04: the user requested smaller buttons and icon actions to reduce scrolling. In this prototype, replace repeated full-width Add/Edit/Refresh actions with named 44-pixel icon targets and provide a persistent compact action bar for Save and related actions. Keep explicit labels for commits and reminder opt-in; payment/deletion/draft semantics remain unchanged. This is a user-directed prototype exception to DESIGN's default 56-unit full-width actions, not a production token change.

| ID | Outcome | Boundary |
|---|---|---|
| HP01 | Open index.html directly; fonts and scripts load locally with no dependencies. | Local browser and relative-asset check. |
| HP02 | Review Dues, Dashboard, Calendar, Details, Add, Edit, Settings, Reminders and all four future onboarding steps. | Browser navigation and source/review traceability. |
| HP03 | Each screen has two live layout choices, a recommendation, guardrails and notes; browser reload retains review choices; Markdown export contains all decisions. | Browser state and downloaded content. |
| HP04 | Sample title/date/amount validation retains input; centavo preview, zero/absence, Save/Cancel, paid draft and named delete confirmation behave consistently. | Pure-helper and browser interaction checks. |
| HP05 | Dashboard combines title/status/category while counts stay global; calendar dates/counts/agenda agree; week start changes after Save. | Helper tests and browser interactions. |
| HP06 | Settings drafts survive a Reminders visit; reminder opt-in uses a clearly simulated confirmation and never requests OS permission. | Browser interactions and source boundary review. |
| HP07 | Light/dark and 320/360/390-pixel mobile plus desktop layouts avoid page/app horizontal overflow, with an inner calendar scroller when necessary. Empty/loading/read-error/stale modes are reviewable. | Headless-browser geometry, screenshots and visual inspection. |
| HP08 | Recommendations cite inspected source and distinguish current behavior, proposed presentation and deferred product features. Canonical scope and existing unrelated work remain intact. | Documentation links, traceability and diff. |
| HP09 | Add, Edit and Refresh use named compact icons; Save stays reachable at both scroll extremes on Add/Edit/Settings/Reminders. Compact actions remain at least 44 pixels high, and Dues shows more content before scrolling. | Mobile browser geometry, real button interaction and visual inspection. |

## Acceptance limits

This prototype is not an APK or device acceptance artifact. Native screen-reader, keyboard, 200% system-text, Android permission, power, delivery and lifecycle acceptance remain with the original feature gates. Browser reference geometry and accessibility-tree inspection cannot close those gates. UI recommendations remain proposals, including header-action placement, segmented Settings choices and progressive Reminders.
