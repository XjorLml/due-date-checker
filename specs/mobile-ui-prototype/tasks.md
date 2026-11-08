# Mobile prototype tasks and evidence

## Implementation

- [x] Inspect current design, scope, page source and relevant tests; preserve unrelated working-tree changes.
- [x] Build a local HTML review studio with nine review entries, including four future onboarding steps.
- [x] Add two live layout alternatives per entry, recommendations, notes and Markdown export.
- [x] Implement in-memory sample create/edit/paid/confirmed-delete, title/status/category filtering, calendar, preference drafts and simulated reminders.
- [x] Bundle existing licensed fonts and provide local, buildless startup.
- [x] Add page-by-page source-backed recommendations and explicit production/deferred boundaries.

## Verification — 2026-11-04

Environment: Windows workspace, Node v24.16.0, Microsoft Edge **156.0.4314.58** in headless mode through the Chrome DevTools Protocol. Initial artifact: `prototypes/mobile-review/` working-tree files, with [initial source hashes](source-initial.sha256). Results below are agent-observed unless explicitly marked pending. No production app acceptance is changed.

- [x] `node --check` for app.js and model.js passed before browser review.
- [x] `node --test prototypes/mobile-review/tests/model.test.cjs`: **4 tests passed**, covering exact money/rounding/overflow, optional/zero handling, Paid/deleted summaries, combined filtering and six-week calendar date/week-start/leap/bounds cases.
- [x] Final Edge browser checks passed: all nine screens with both layout alternatives at desktop 1440 and mobile 390/360/320 pixels, without document/app horizontal overflow. The calendar uses its inner scroller where necessary.
- [x] Browser interactions passed: Dashboard search/no-match/clear with global counts and query retention, calendar agenda membership/selection retention and Add's Today default, sample create/exact rounding, literal HTML-like title handling, payment draft Cancel/Save, Settings draft through Reminders, saved Sunday week start, simulated reminder opt-in Cancel/confirm, deletion Cancel/confirm/unavailable, read-state previews, all four onboarding steps, choice/note reload persistence, Markdown download content, and direct-file startup with the bundled font loaded. Browser accessibility-tree navigation presence was checked; this is not a screen-reader pass.
- [x] Desktop light and mobile dark screenshots inspected, including Calendar, Add and Reminders. Screenshots are local ignored files under `prototypes/mobile-review/.qa/`; they are not native acceptance evidence.
- [x] Checked 121 relative Markdown paths, HTML/CSS assets and the source hash manifest; all paths resolve. Final scoped documentation diff and whitespace check passed. Existing STATUS/preferences/TalkBack work and concurrent reminder-source/test changes were preserved.

Initial sandboxed browser attempts timed out enabling the DevTools Runtime domain. Running the same local headless check outside the sandbox resolved the renderer startup failure; the completed run passed. The helper owns and stops its loopback server/browser; it does not start a public service or install packages.

An extended browser check initially assumed that Add always returned to Dues. It failed after starting from Calendar, which correctly retained its selected day. The check was corrected to verify Calendar's Today default and retained selection, then explicitly start the separate Dues create flow. The final run passed; no product behavior was changed to satisfy that mistaken expectation.

## Compact actions follow-up — 2026-11-04

User requested smaller buttons and icons to avoid repeatedly scrolling through large actions. HP09 is implemented in the same prototype; production app files and global DESIGN tokens are unchanged by this follow-up.

- [x] Add uses a named + icon in each main-page header; Details has a pencil Edit action; Refresh is a quiet icon.
- [x] Replace stacked form actions with a persistent compact bottom bar. Content-width Save retains its label; Delete/Disable use separately named icons. Reminder opt-in and Save only retain explicit text.
- [x] Compact regular buttons to 44-pixel minimum height; retain readable field/body sizes, tab labels and calendar targets. Show diagnostic tools as compact icon-and-label rows.
- [x] Syntax check and final Edge browser run passed across all nine screens, both alternatives and 1440/390/360/320-pixel viewports. Named icon checks and Save visibility/44-pixel minimum at both scroll extremes passed. At 390 pixels, the first Dues card begins 180 pixels into the scroll area.
- [x] Real header Add, footer Save and invalid-field focus passed, along with existing payment/Cancel/delete, preferences/reminders, export/reload and direct-file checks.
- [x] Inspected updated desktop light, mobile light Dues and mobile dark editor screenshots. `mobile-compact-dues.png` and the refreshed standard captures are under ignored `.qa/`.
- [x] Updated page review, spec and plan; preserved initial hashes and recorded [revised source hashes](source.sha256). Relative links/assets, hashes and whitespace checked after the changes.

Environment is the same Windows/Node/Edge setup above. These are agent-observed browser results for the revised HTML artifact. Native keyboard, system text and TalkBack acceptance remain outside this task.

## Four-concept exploration — 2026-11-04

The user's supplied redesign brief authorizes a separate comparison/selection experience. [HP10–HP16](spec.md#four-concept-exploration-extension--2026-11-04) govern this extension; the earlier implementation and evidence above remain attributed to the original artifact. New deliverable: [design exploration](../../prototypes/design-exploration/index.html), [audit](../../prototypes/design-exploration/AUDIT.md) and [system/handoff guide](../../prototypes/design-exploration/SYSTEM.md).

- [x] Read the supplied brief, docs index, current scope/design contracts, existing prototype code/checks and applicable native page source/tests. Write the audit before implementing the new prototype; consult official reference-product sources and link them in the audit.
- [x] Implement four distinct directions in a separate directory: Minimal Fintech, Premium Dark, Friendly and Intuitive, and Data-Driven Dashboard. Share fixtures, money/status/date helpers, components and interaction state.
- [x] Build side-by-side root-screen comparison with signature/light/dark choices, individual exploration, four live destinations, compact named actions, Details/editor and simulated reminder settings.
- [x] Add isolated amount aggregates, month/week Calendar, repeat preferences/next-date preview and a confirmed quick-paid proposal, with explicit sample-only definitions. Keep existing-style editor Save/Cancel and confirmed deletion semantics.
- [x] Implement personal selection with no default preference, confirmation, change/clear, per-concept notes and Markdown export. Only choices/notes persist; demo records/preferences/reminders reset on reload.
- [x] Update existing spec/plan rather than create a duplicate feature spec. Add non-normative docs index/DESIGN links; PRD/SDD and production implementation remain unchanged by this task.

### Observed extension verification

Environment: Windows workspace, Node v24.16.0, installed Edge 156.0.4314.58 in headless mode through CDP. Artifact: `prototypes/design-exploration/` and unchanged shared model/fonts, identified by [initial extension source/dependency hashes](exploration-source-initial.sha256). All results here are agent-observed on 2026-11-04.

- [x] Syntax checks passed for app.js, design-system.js and exploration-model.js.
- [x] `node --test prototypes/design-exploration/tests/model.test.cjs`: **5 tests passed**. Covered upcoming/overdue/paid/absent/zero scope, exact and large integer sums, chart/count reconciliation, recurrence month-end/leap/date bounds without generation, week-start/bounds and all four concept/mode foreground/background token combinations (88 flat normal-text contrast comparisons, each at least 4.5:1).
- [x] `node prototypes/design-exploration/tests/browser-check.cjs`: passed. Comparison screens checked at 320/360/390/430/768/1280/1600 pixels; live roots/Reminders/Add checked across four concepts, both modes, at 320/360/390/430/768/1600 pixels. No document/live-content horizontal overflow; calendar retains a bounded internal scroller where needed.
- [x] Browser flow coverage passed: identical totals; shared title/status filters and retained query; month/week count/agenda/date retention; rescheduling through editor with draft retained across concept change; invalid-footer Save focus; optional blank/zero/exact-rounding distinctions; repeat preview without extra rows; editor Cancel; confirmed quick-paid Cancel/confirm; named deletion Cancel/confirm/unavailable; Settings draft through Reminders and saved appearance/week start; reminder opt-in Cancel/confirm; empty/loading/error/stale/normal previews; literal HTML-like user text; named icons and unique DOM IDs.
- [x] Explicit selection Cancel leaves no preference; confirmation, per-concept notes, reload persistence, Markdown download content and Clear passed. Direct-file startup and shared bundled font loading passed. Browsing did not select a winner.
- [x] At a 390-pixel viewport, 200% browser base text reflow passed across the four concepts and main/Reminders screens. Reduced-motion transition removal passed. These are browser checks, not Android system-text or TalkBack acceptance.
- [x] Desktop four-way comparison, desktop explorer and mobile signatures for A/B/C/D visually inspected. Screenshots are ignored local `.qa/` artifacts, not committed application/device evidence.
- [x] Verify original `prototypes/mobile-review/` remains identical to HEAD, local documentation/asset references and heading anchors resolve, source hashes match, and scoped diff/whitespace checks pass.

One initial harness attempt looked for Add on the pushed Reminders page. The test was corrected to enter a main destination first; the application did not gain an unintended Add action on Reminders. Completed runs passed after that test correction. Browser rendering used approved execution outside the sandbox, as required by this environment's renderer restriction; each run stops its owned headless browser and loopback server.

## Minimal Fintech floating Add — 2026-11-04

The user selected A / Minimal Fintech for refinement, reviewed the proposed central floating action, and requested the HTML update. HP17 applies only to concept A; the original prototype and B/C/D references remain preserved.

- [x] Place Add between Calendar and Dues in a five-slot bottom bar containing four destination tabs. Use a 56-pixel lime circle/dark plus, visible Add label, accessible name, subtle shadow and approximately 16-pixel rise. Remove A's header Add duplicate and retain Reminders.
- [x] Reserve bottom content clearance and safe-area spacing, retain active destination semantics, and hide the bar on pushed screens. Reuse the existing Add/form/navigation logic.
- [x] Syntax and final Edge browser run passed. Added checks for exact 56 × 56 geometry, horizontal centering, 12–18-pixel observed rise, pointer hit testing, no Add selected-tab state, header removal and last-row clearance at the end of the scroll. Actual Add/Save/Cancel returns passed from Dashboard, Calendar, Dues and Settings.
- [x] Existing four-concept responsive/light-dark, 200% browser-text, form/payment/delete, calendar, reminders, preference/export/reload and direct-file checks passed. Same Windows/Node/Edge environment as the extension evidence above.
- [x] Visually inspected `prototypes/design-exploration/.qa/minimal-floating-add-mobile.png` and the updated comparison preview. Updated spec, plan and system guide; preserved initial extension hashes and refreshed [floating-Add source/dependency hashes](exploration-source-floating-add.sha256). Checked local links, hashes and preservation of the original prototype.

Open [A's dashboard](../../prototypes/design-exploration/index.html#a/dashboard) to see the refinement. This is HTML prototype verification, not native acceptance or production design adoption.

## Minimal Fintech recommendations in HTML — 2026-11-04

User requested further improvements and alternative recommendations through the HTML. HP18 adds Essential, Focus and Soft cards within the selected A direction. Focus is labeled as the agent's recommendation; Essential remains the unset preference default, and all three remain available without changing the production app.

- [x] Add three visual preview cards with rationale/tradeoffs, live phone switching, mobile horizontal comparison and a selected-card cue. Keep the central Add action across all refinements.
- [x] Implement Focus's compact scoped total/next date, concise status chips, chronological overdue/today attention list, later dues and inset dock. Soft cards adds a tinted amount card and grouped outlined dues. Preserve shared money/status logic and root Dues ordering.
- [x] Preserve active editor drafts and title filters across refinement switches. Validate/persist the refinement ID independently of the preferred global concept; include it in the downloaded design brief. Restore keyboard focus to the active preview action and keep its card in view on mobile.
- [x] Syntax check passed. Final `node prototypes/design-exploration/tests/browser-check.cjs` passed in the Windows/Node/Edge environment above: all three refinements at 320/390/1600 pixels, both modes and all four root screens; no document/live app horizontal overflow. Verified Focus brings its first actionable row earlier than Essential and starts with the overdue sample.
- [x] Draft/query retention, active preview focus, refinement reload persistence and brief export passed. Existing center-Add geometry/Save/Cancel, four-concept responsiveness, payment/deletion, Calendar/Reminders, note/preference reload, larger browser text/reduced motion and direct-file checks passed.
- [x] Inspected `.qa/minimal-focus.png` and `.qa/minimal-soft.png`, including the mobile comparison strip and live phone. These are local ignored screenshots. The previous floating-Add source manifest is preserved; [recommendation source hashes](exploration-source-recommendations.sha256) identify this revision.
- [x] Updated spec/plan/system guidance and verified local links/anchors, source hashes, whitespace and unchanged original prototype.

The expanded rapid-navigation test left the URL stale while the in-memory concept and stored notes were correct, consistent with browser history-update throttling. The disposable headless test browser now disables IPC flooding protection for this local automated matrix, and reload assertions wait for a genuinely new ready document. The completed run passed; production/browser application code does not disable this browser protection. Native acceptance remains separate.

## Remaining acceptance

- [x] The user selected Minimal Fintech and its Focus refinement (HP17–HP18) and, in the follow-up specs, sticky actions with context-aware navigation and the Essential-first editor. These are HTML selections; see the [DESIGN register](../../docs/DESIGN.md#design-explorations-non-normative). Both prototype directories are committed (e368ac2, 53d00e8).
- [ ] Review of the original mobile-review alternatives (HP01–HP09) and any production adoption decision for the selected direction.
- [ ] If later implemented in MAUI: native keyboard, TalkBack, Android Back, 200% system text, theme, safe-area and permission/power/lifecycle checks for the changed pages. These are outside this HTML task.

Selecting or exporting prototype choices is not production implementation authorization or release acceptance.
