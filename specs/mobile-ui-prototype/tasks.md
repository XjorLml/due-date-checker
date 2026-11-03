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

## Remaining acceptance

- [ ] User selects preferred layouts and reviews recommendations.
- [ ] If later implemented in MAUI: native keyboard, TalkBack, Android Back, 200% system text, theme, safe-area and permission/power/lifecycle checks for the changed pages. These are outside this HTML task.

Selecting or exporting prototype choices is not production implementation authorization or release acceptance.
