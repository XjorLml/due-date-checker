# Mobile prototype implementation plan

Scope/scenarios: [spec](spec.md). Execution evidence: [tasks](tasks.md). Deliverable: [prototype](../../prototypes/mobile-review/index.html) and [page review](../../prototypes/mobile-review/REVIEW.md).

## Modules and dependencies

| Module | Purpose |
|---|---|
| prototypes/mobile-review/index.html | Review studio, screen navigation, live phone region and choices/notes/export controls. |
| styles.css and assets/ | Responsive layout, scoped DESIGN light/dark tokens, bundled Plus Jakarta Sans/OFL and reference mobile type/control sizes. |
| model.js | Pure sample-data helpers: exact decimal-to-centavo conversion, status/filter/global summary and six-week grids. |
| app.js | In-memory interactive screens, draft/navigation state, simulated platform actions and persistent reviewer choices. |
| tests/ | Node helper tests and dependency-free Edge/Chromium CDP interaction/geometry checks. |
| REVIEW.md | Source-backed page observations, alternatives, recommendations, proposed/deferred distinctions and external interaction guidance. |

No package installation, runtime network, external app integration or hosted publication dependency. Use the existing repository checkout for this local project artifact. Copy fonts from the existing licensed resources with the OFL intact; use lightweight inline vector placeholders.

## Compact actions follow-up

Apply HP09 within the existing HTML artifact: header plus/pencil icons, quiet refresh icon, and a flex-sibling action bar outside the page scroll owner. Associate external Save buttons with their HTML forms so native validation, submitter intent and existing Save handlers still work. Put Delete/Disable in named icon buttons while retaining confirmation and explicit opt-in labels. Replace stacked diagnostic buttons with icon-and-label rows. Preserve existing layout choices and sample data rules. Verify Save visibility before/after scrolling, real icon/Save clicks, invalid-field focus, both themes and the existing width/interaction checks; update the review and source manifest after validation.

## Sequence and verification

1. Preserve the initial unrelated STATUS/preferences/TalkBack work. Read the docs index, DESIGN scope/tokens/screens, PRD, relevant SDD/test plan, current page source and applicable tests.
2. Implement current-screen samples and separately labeled onboarding concepts. Review production rules rather than inheriting conflicting future DESIGN behavior.
3. Add meaningful helper checks for large money values, rounding, absent-versus-zero, Paid precedence, combined filters and calendar boundaries.
4. Exercise live layouts at desktop and mobile widths and perform sample Save/Cancel/payment/delete, draft-return and simulated opt-in flows. Inspect light/dark screenshots. Resolve observed overflow/runtime failures before recording passes.
5. Validate local links, document final evidence and add index/design references. Do not run MAUI builds for this standalone browser artifact.

## Four-concept exploration extension

Follow [HP10–HP16](spec.md#four-concept-exploration-extension--2026-11-04) in the separate `prototypes/design-exploration/` directory. The audit was written before implementation. Reuse the original pure model and bundled fonts unchanged; one added helper supplies exact sample aggregates and bounded recurrence/week previews. One renderer/state store drives four palettes and distinct Dashboard/row structures. Gallery previews are inert with explicit Explore actions; interactive pages share drafts, filters and dates across concept switches. Selection/notes are stored separately from temporary sample records, with no default winner.

New modules are index.html (studio/selection shell), design-system.js (concept metadata/tokens), exploration-model.js (sample extensions), app.js (shared rendering/interactions), styles.css (tokens and structural variants), AUDIT.md, SYSTEM.md and focused helper/browser checks. Browser QA covers four concepts, two appearances, comparison screens, 320–1600-pixel viewports, date/repeat/amount contracts, draft/save/confirmation flows and preference/export/reload. Measure 200% browser-base text reflow and reduced motion separately from native acceptance. Inspect generated screenshots before recording visual evidence. The original prototype's hashes and Git diff must remain unchanged.

DESIGN receives a non-normative exploration link. PRD and SDD remain unchanged because new routes/totals/recurrence/shortcuts are expressly sample-only. Selection is a user review preference, not production authorization. Do not stage, commit or deploy this extension unless requested.

## Product/contract review

HP18 extends the existing concept metadata and shared renderer with three Minimal Fintech refinements, rather than adding another application. Live inert previews reuse Dashboard/Nav components; the active preview writes only a validated refinement ID to the existing reviewer preference record. Focus uses the same summary and date predicates, placing chronological overdue/today rows before later dues; root Dues sorting is unchanged. CSS supplies inset/full-width/soft navigation and row grouping. Preserve in-progress forms, filters and sample records on switching. Verify all three at desktop/phone widths and both modes, relative position of Focus's first actionable row, draft/query retention, selection focus, reload/export and existing flows. Preserve the pre-recommendation source manifest before refreshing hashes.

HP17 adds one action between Calendar and Dues for concept A only, keeping the four destination routes unchanged. The center control uses the existing Add route and return behavior; CSS supplies the circular 56-pixel treatment, rise, safe-area spacing and content clearance. Remove A's header duplicate and verify geometry/hit testing, tab semantics, pushed-page hiding and Save/Cancel return from each root. Preserve earlier exploration hashes before refreshing the source manifest.

PRD and SDD require no behavioral change: the HTML does not touch production data or routes. DESIGN receives a clearly non-normative review link; its tokens, future scope and native adaptations remain authoritative. No ADR is needed for an isolated, dependency-free sample artifact. Selected visual alternatives are inputs to a later production design-adoption task, not changes to approved product behavior.
