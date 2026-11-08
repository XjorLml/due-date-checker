# Mobile HTML design-review prototype

User request, 2026-11-04: create an HTML prototype from DESIGN.md, review page by page, recommend revisions suited to a mobile finance app, and add choices to select. This authorizes a local review artifact and documentation; it does not authorize implementing the selected directions in MAUI.

## Scope and authority

Use [DESIGN](../../docs/DESIGN.md#scope-and-authority), the latest [PRD](../../docs/PRD.md) rules for R02–R07 and F01/F02/F03/F04/F05/F07, and [SDD navigation](../../docs/SDD.md#6-ui-and-navigation). Review current page source and applicable tests before describing existing behavior. The [review](../../prototypes/mobile-review/REVIEW.md) owns recommendations; production contracts remain in the canonical documents. The [future roadmap](../future-pages/plan.md) retains ownership of deferred capabilities.

Deliver a buildless local HTML/CSS/JavaScript prototype with bundled licensed fonts, sample records, all current screens, and separately labeled four-step future onboarding. Provide per-page live layout choices, notes and a review-choice export. Demonstrate sample CRUD, search/filter, calendar, preference drafts, simulated reminders and shared read states. Preserve optional-versus-zero amount handling, payment-on-Save, date sorting, explicit PHP/Manila context and confirmed deletion without Undo.

No production app code, database, schema, signing, permissions, release state or real notification changes. No account, payment processing, bank connection, financial aggregation, recurring/timed items, cloud services or product financial-data export is introduced into production. The later four-concept extension isolates proposed totals and repeat controls in sample state. Export contains reviewer decisions only. App state is temporary; only reviewer choices/notes persist in browser storage.

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

## Four-concept exploration extension — 2026-11-04

The supplied “Duewise — UI/UX Redesign and Interactive Design Exploration” brief authorizes four separate interactive alternatives and a user-owned selection workflow. Preserve `prototypes/mobile-review/` and production implementation. Build the extension in [prototypes/design-exploration](../../prototypes/design-exploration/index.html), reusing the original pure sample model and licensed fonts without editing those files. The pre-implementation [audit](../../prototypes/design-exploration/AUDIT.md) records existing weaknesses, official reference research and scope conflicts; [SYSTEM](../../prototypes/design-exploration/SYSTEM.md) owns only the new prototype's tokens/components and interaction definitions.

All four alternatives use identical fixtures and one interaction/aggregation implementation. Different dashboard structure, density, radius, palette and voice distinguish Minimal Fintech, Premium Dark, Friendly and Intuitive, and Data-Driven Dashboard. Default browsing starts with the comparison gallery, not a chosen winner. Main navigation in this sandbox is Dashboard/Calendar/Dues/Settings. Recurrence preferences, month/week viewing, amount aggregates and confirmed quick-paid actions are explicitly prototype proposals. PRD/SDD production scope and DESIGN's adopted palette/navigation remain unchanged. The original payment-on-Save editor flow remains available, as do Cancel, confirmed deletion, PHP, Manila dates and absent-versus-zero handling.

| ID | Extension outcome | Verification boundary |
|---|---|---|
| HP10 | The audit precedes implementation and links inspected files, official references and current/proposed distinctions. Original prototype files remain unchanged. | Audit, repository diff and source hashes. |
| HP11 | Four visibly distinct concepts can be compared together on desktop and browsed responsively on mobile; all four main screens can be compared with signature/light/dark appearances. | Browser navigation, geometry and screenshots. |
| HP12 | Concept switches retain the same data, query, selected calendar date and editor/preference drafts. Add/Edit/Cancel/delete, exact amount preview and explicit statuses work in every concept through shared components. | Browser flows and shared-helper tests. |
| HP13 | Month/week views agree on exact dates/counts/agenda, Today and week-start preferences. Repeat choices show a bounded next-date preview without generating a series. | Date-boundary helper and browser tests. |
| HP14 | Amount summaries explicitly scope unpaid due dates from Today to month end; absent amounts are counted/excluded, zeros remain zero, overdue is separate and charts never imply payment history. | Integer-centavo aggregation tests, labels and browser consistency. |
| HP15 | No concept is initially preferred. Explicit user confirmation saves a choice; change/clear/notes/reload/export work and are distinguished from preview selection. | Browser storage and downloaded brief. |
| HP16 | Compact named controls, persistent form actions, both themes, visible focus, state previews, reduced motion and desktop/320–430-pixel reflow are checked. Native accessibility acceptance remains separate. | Browser checks and evidence in tasks. |
| HP17 | User-selected Minimal Fintech has Dashboard / Calendar / floating Add / Dues / Settings in the bottom bar. The 56-pixel lime action sits centrally above the bar, has an Add label/name and no selected-tab state; its header duplicate is removed. It remains clear of content, hides in pushed pages and preserves origin on Save/Cancel. | Browser geometry, pointer hit testing, mobile screenshot and navigation flow. |
| HP18 | Minimal Fintech offers live Essential, Focus and Soft cards recommendations in HTML. All preserve the centered Add and shared sample/draft state. Focus moves overdue/today rows earlier; its compact total keeps the original amount scope. Preview choice persists, supports both modes/mobile widths, remains visible in the mobile carousel and is included in the exported brief. | Browser layout, interaction, focus, reload/export and source review. |

Minimal Fintech refinement, 2026-11-04: the user chose A and requested a centered floating Add action, then authorized the HTML update after reviewing the recommendation. Apply HP17 to A in the explorer and comparison preview; preserve B/C/D as comparison alternatives. This is a prototype refinement, not production adoption.

The subsequent request to improve and recommend alternatives via HTML authorizes HP18: Essential retains A's existing presentation; Focus is the agent's recommendation for quicker scanning with overdue/today priority and an inset dock; Soft cards offers tinted grouping. Retain Essential as the initial refinement until the user previews another. Recommendation and previewing are design exploration, not authorization to change the production app.

## Selected Focus and localized catalog follow-up

The later supplied Philippine-category brief and explicit **HTML prototype only** clarification authorize the [localized catalog feature](../ph-catalog-prototype/spec.md). Focus is now the selected default in the normal interface; the earlier no-winner/alternative-picker behavior is historical evidence for the exploration milestone. No new navigation alternatives are generated. PH01–PH09 own the new catalog/picker/persistence/schedule scenarios and current evidence, without altering native PRD/SDD scope.

## Acceptance limits

This prototype is not an APK or device acceptance artifact. Native screen-reader, keyboard, 200% system-text, Android permission, power, delivery and lifecycle acceptance remain with the original feature gates. Browser reference geometry and accessibility-tree inspection cannot close those gates. UI recommendations remain proposals, including header-action placement, segmented Settings choices and progressive Reminders.
