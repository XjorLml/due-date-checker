# Form actions and navigation audit

2026-11-04, recorded before implementing the new comparison lab. Inspected the current [app renderer/navigation](app.js), [styles](styles.css), [HTML shell](index.html), [catalog dialogs](ph-ui.js), [browser checks](tests/ph-browser-flows.cjs), [DESIGN scope](../../docs/DESIGN.md#scope-and-authority), [PRD workflows](../../docs/PRD.md#existing-dues-workflows) and the current [HTML catalog spec](../../specs/ph-catalog-prototype/spec.md).

The subsequent explicit user request selects **A sticky actions + B context-aware navigation** for the normal HTML mockup. [IN10](../../specs/interaction-lab/spec.md#acceptance-scenarios) and [follow-up evidence](../../specs/interaction-lab/tasks.md#selected-html-adoption--2026-11-04) track adoption. Findings below describe the earlier inspected baseline; production/native behavior remains separate.

## Findings

The later approved Essential-first Alternative A implements [IN11–IN15](../../specs/interaction-lab/spec.md#acceptance-scenarios): collapsed editor summaries, earlier essentials and dirty-exit protection in normal HTML Add/Edit. [Current evidence](../../specs/interaction-lab/tasks.md#essential-first-alternative-a--2026-11-04) supersedes the earlier baseline findings below for those areas, while preserving this audit's history.

| Area | Source-backed finding | Proposed comparison |
|---|---|---|
| Cancel / Save | Due Cancel is a header action while Save sits in the flex-sibling bottom action region. Delete is a separate footer icon. Their hierarchy is clear, but Cancel and Save are far apart. | Compare adjacent sticky actions, full-width Save with secondary Cancel, and header Cancel with bottom Save. |
| Form reach | Save is already outside the content scroll owner, so scrolling through long fields does not move it. Current footer usually has only one compact Save. | Keep all three Save treatments reachable; demonstrate the space cost and thumb reach of each. |
| Main navigation | `renderPhone` uses one shared `nav()` and shows it on Dashboard, Calendar, Dues and Settings. Source inspection does not establish a current root-screen disappearance defect. | Measure consistent navbar position/size and active states on all four roots, including Settings' extra action region. |
| Pushed pages | Current code intentionally hides tabs in Add/Edit, Details, Reminders and catalog management. | Explain this policy and compare it with persistent tabs and a native-dialog layer above the parent tabs. |
| Unsaved input | Main prototype Back/Cancel discards form fields without a prompt, matching the existing native baseline. | In the explicitly requested comparison environment, ask before discarding a dirty due/preference/reminder draft. Do not silently change native rules. |
| Safe areas | Bottom actions and tabs include safe-area padding, but the phone's minimum-height/mobile viewport rules are not evidence of keyboard safety. | Add a keyboard reservation demo plus VisualViewport-aware sizing; verify geometry and preserve native-device acceptance as a separate gate. |
| Modal stacking | Existing category/custom dialogs use native `dialog`; their modal backdrop properly blocks the underlying app. | Keep tabs below overlays. A layered form should use the same native modal/focus behavior, not elevate tabs above it. |
| State feedback | Field errors, catalog-write errors and toasts exist. The in-memory due Save is synchronous, so it does not demonstrate an observable loading interval. | Add labeled disabled/loading/error/success previews and a short simulated commit interval with duplicate-submit guards in the lab. |
| Layout integrity | Navbar/footer are siblings of the one scrolling content region. Their position is determined by the phone frame, not the form's document height. | Retain that structure; isolate form-layer rendering and re-use the same route/Save logic. |

## Action alternatives

- **A — Sticky bottom actions:** outlined Cancel beside primary Save. Both stay within thumb reach while scrolling. Recommended starting preview; the pair uses horizontal space.
- **B — Primary Save / secondary Cancel:** full-width Save with a quiet Cancel above. Strong hierarchy and easy primary action, at the cost of a taller action region.
- **C — Header Cancel / bottom Save:** Cancel stays top-left, Save gets the bottom width. The smallest footer, with greater reach to cancel. Every dirty exit still asks first.

## Navigation alternatives

- **A — Persistent:** one Focus navbar on the roots and pushed screens, with form actions in a separate region above it. Leaving a dirty draft asks first. Advantage: destinations remain available; tradeoff: more footer space and competing actions.
- **B — Context-aware:** roots retain the same bar; Add/Edit and other focused pushed screens replace it with contextual actions. Advantage: less clutter and more form space; tradeoff: return through Save/Cancel/Back.
- **C — Layered:** roots retain the same bar; focused pushed pages appear in a modal layer above the parent. Tabs stay visible behind the dimmed layer and cannot be used until it closes. Advantage: clear task context; tradeoff: less vertical room and careful keyboard/modal handling.

Sticky actions + context-aware navigation is the agent's recommendation for everyday forms. This is a design judgment, not a user-study result or an approved combination. Preview defaults and personally chosen preferences must be visibly separate. Each decision has its own Choose action and stored preference; neither should force the other.

## Scope and verification limits

The new lab belongs inside the HTML design-exploration artifact. The selected Focus visual style, Philippine catalog, current sample data rules and ordinary prototype route remain. Native PRD/SDD/MAUI behavior is unchanged. All alternatives are preview-only until explicitly chosen; even a chosen browser preference does not authorize production adoption.

Browser geometry, reduced-height viewport and simulated keyboard checks cannot establish actual iOS/Android keyboard, Back or screen-reader acceptance. Document exactly what was observed. Use 44–48-pixel minimum live controls, wrapping labels, focus indicators, native modal containment and one actual form instance so drafts and validation do not diverge between alternatives.
