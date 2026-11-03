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

## Product/contract review

PRD and SDD require no behavioral change: the HTML does not touch production data or routes. DESIGN receives a clearly non-normative review link; its tokens, future scope and native adaptations remain authoritative. No ADR is needed for an isolated, dependency-free sample artifact. Selected visual alternatives are inputs to a later production design-adoption task, not changes to approved product behavior.
