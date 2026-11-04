# Interaction comparison implementation plan

Scope/scenarios are owned by [spec](spec.md); findings by [audit](../../prototypes/design-exploration/INTERACTION-AUDIT.md); results by [tasks](tasks.md).

1. Preserve existing tree and PH evidence manifest. Inspect canonical design scope, sample route/editor/action/nav code, catalog dialogs and browser harness.
2. Add the dedicated comparison section to `index.html`, semantic token-based rules in `interaction-lab.css`, and isolated `Lab` controller in `interaction-lab.js`. Reuse existing forms, navigation function, validation and sample writes.
3. Integrate small hooks in `app.js`: lab route, post-render presentation, dirty-exit policy, baseline capture and simulated busy wrapper. Keep ordinary Focus behavior intact.
4. Use native modal containment for policy C and preserve a single editor DOM. Let nested native dialogs sit above it. Reserve physical space for actions/nav/demo keyboard rather than overlaying inputs.
5. Extend the dependency-free Edge CDP suite with `interaction-browser-flows.cjs`. Cover all nine combinations at 320/360/390/430 in both palettes; root anchoring/active states, scroll geometry, target dimensions, dirty exits, draft retention, nested sheets/focus, Add/Edit and duplicate writes, keyboard reservation and independent preferences. Re-run existing PH flows and 11 pure helper checks.
6. Inspect representative screenshots, validate syntax/links/anchors/whitespace and source manifests, and update non-normative design/test routing. Do not run a MAUI build for HTML acceptance.

Dependencies: unchanged shared `mobile-review/model.js` and licensed fonts; installed Edge and Node only for verification. No new packages/runtime network requests. Browser execution uses an owned disposable profile and loopback server.

Known limits: VisualViewport resizing improves the preview but does not emulate all OS keyboard/Back behavior. A native modal makes outside controls inert; selectors inside the layer allow live comparisons without bypassing modal containment. Stored preference is not final-design adoption.

## Selected HTML adoption follow-up

IN10 applies the user's explicit A/B choice to the default Focus renderer, reusing the existing sticky action markup/header and semantic action CSS. Keep the shared root/pushed navigation policy, Save/Cancel validation and PH catalog behavior. Scope shared phone sizing/action styles independently from the comparison studio layout; returning from the lab must restore the selected normal layout even after previewing another option. Preserve the comparison evidence manifest, extend the existing browser flows for normal footer/root geometry and update current manifests/evidence. No native source or new dependency is needed.

## Essential-first Add/Edit follow-up

IN11–IN15 implement the approved Alternative A in the shared editor. Reorder essentials, split category/provider rendering, use native details/summary controls for type/schedules/optional details, and store open state separately using an editor-instance token. Reuse IDs, category management, validation, Save and the sticky action region. Add a pure UI-normalization/reminder-summary helper and a semantic-token CSS extension. Reveal all disclosure ancestors for errors and add required-state/error associations. Extend the existing dirty-baseline/confirmation controller to normal Add/Edit while retaining lab-only guards on other pages.

Preserve the pre-essential [selected-layout manifest](source-sticky-context.sha256). Extend existing browser helpers to reveal real disclosures before interacting; update old deliberate-discard fixtures for the newly approved normal confirmation. Add focused helper/browser checks for fold improvement, collapsed defaults, retained inputs/open state, global reminder summaries, invalid hidden controls, safe PHP paste, amount policies and originating-screen return. Keep native builds and timed-user acceptance separate.
