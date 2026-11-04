# Interaction comparison implementation and evidence

Date: 2026-11-04. Scope: **HTML prototype only**. [Scenarios](spec.md), [plan](plan.md), [audit/recommendations](../../prototypes/design-exploration/INTERACTION-AUDIT.md).

## Implementation

- [x] Inspect current source/docs/tests; preserve unrelated work and prior PH manifest as [initial evidence](../ph-catalog-prototype/source-initial.sha256).
- [x] Document actual source findings, tradeoffs and recommendation without inventing a root navbar defect or claiming user approval.
- [x] Add independent 3×3 action/navigation comparisons, long live Add/Edit, light/dark, explicit Choose controls, persistence/clear/export.
- [x] Keep Focus nav component, shared validation/sample writes, localized catalog and ordinary prototype entry.
- [x] Add modal layering, draft retention/dirty guard, busy duplicate guard, feedback previews, target/safe-area/viewport sizing and labeled keyboard reservation.

## Verification

Environment: Windows workspace, Node v24.16.0, installed Edge 156.0.4314.58 through the existing disposable local CDP harness. The original comparison artifact and unchanged shared dependencies are identified by the [comparison source manifest](source-comparison.sha256). The subsequent selected sticky/context-aware artifact is preserved in [source-sticky-context.sha256](source-sticky-context.sha256). Latest follow-up hashes are in [source.sha256](source.sha256); PH and mobile-exploration current manifests mirror it. Results are agent-observed; no native-device acceptance is inferred.

- [x] Syntax checks passed for app/controller/browser flow. `node --test prototypes/design-exploration/tests/model.test.cjs prototypes/design-exploration/tests/ph-catalog.test.cjs`: **11 passed**.
- [x] `node prototypes/design-exploration/tests/browser-check.cjs`: passed both the existing PH regression and new IN01–IN09 flows, without runtime exceptions. All nine combinations at 320/360/390/430 in light/dark retained root navbar position/height/active state, 44px nav and 48px action targets, unique IDs, long-form scrolling, fixed action geometry and no horizontal overflow. B/C Save spans the footer; persistent-nav Save clears the raised Add target.
- [x] Browser flows verified dirty Cancel/Escape/tab exit, Keep editing, draft/layout retention, nested category-dialog Tab containment, error/loading reset inside the layer, Add/Edit Save/return and exactly one insertion for duplicate submit events. Disabled/loading/error/success previews did not write. Nested Settings draft survived management/Keep editing and saved correctly.
- [x] Keyboard reservation passed all nine combinations at 390×720, plus 320×568 and 390×640 compact frames with a remaining scroll area. Choices persisted independently without changing previews; exported Markdown named both decisions separately. Controlled storage failure retained current choices and displayed guidance. Return to ordinary Focus and the existing direct-file startup checks passed.
- [x] Inspected ignored `.qa/lab-comparison-desktop.png`, `lab-sticky-context-mobile.png`, `lab-primary-keyboard.png` and `lab-layered-keyboard.png`. Visual review corrected the stacked Save width and increased clearance above the raised Add button. Added compact live selectors to reduce repeated scrolling and compact comparison chrome for shorter frames.
- [x] Local links/anchors/assets, whitespace, 23 source hashes and scoped original/native Git preservation checks passed. No MAUI build was run for this HTML task.

An intermediate preference assertion expected preview B after the nested-Settings fixture had explicitly selected A. The fixture now restores B before checking that choosing a preference does not change the preview. The completed run above passed; the earlier failed assertion is not passing evidence.

## Selected HTML adoption — 2026-11-04

The explicit follow-up request selects **Sticky bottom actions + Context-aware navigation** for the HTML mockup. This supersedes the earlier unchosen state for the normal prototype; browser reviewer preferences remain separate.

- [x] Apply adjacent Cancel/Save to default Focus Add/Edit without entering the lab; keep Edit Delete in the header and retain the shared root/pushed-page nav behavior.
- [x] Reuse sticky markup, tokens and viewport sizing without adding comparison controls/feedback to the ordinary app flow. Preserve sample financial/catalog behavior and unrelated work.
- [x] IN10 browser checks passed in the Windows/Node/Edge environment above. The normal route has adjacent Cancel/Save targets at least 48px high, no duplicated header Cancel, a header Delete in Edit, hidden task tabs, fixed footer position through scrolling and stable root navbar anchoring. The existing PH Add/Edit Save/Cancel/catalog regression passed in both themes at 320/360/390/430/768/1600px. The comparison suite passed and verified exiting C actions/C navigation restores the ordinary A/B layout.
- [x] Inspected `.qa/focus-sticky-actions-selected.png`: compact outlined Cancel and primary Save remain below the long scrolling form. Syntax check and all **11 helper tests passed**. Validated 261 local links/66 heading anchors, 20 HTML/CSS assets, current 23-file manifests and whitespace. Scoped Git comparison confirmed original `mobile-review`, native source and native test projects unchanged. The comparison-era hashes remain in `source-comparison.sha256`.

## Remaining acceptance

A/B is selected for the HTML mockup only. Production implementation and real mobile keyboard/safe area, system Back, touch and assistive-technology review remain separate. Browser success/error previews are demonstrations, and sample dues reset on reload.

## Essential-first Alternative A — 2026-11-04

Authorization: explicit approval of Alternative A and the supplied implementation-ready prompt. This follows the read-only, agent-observed audit of the actual default form, not an assumption that the lab behavior was already present in normal Add/Edit. Scope/scenarios: IN11–IN15 in [spec](spec.md).

- [x] Reorder the essentials and shorten introductory copy; expose type through Change, keep optional category with stable IDs, and collapse schedules/biller/notes/custom offsets.
- [x] Preserve form/disclosure state across category/layout/theme changes; retain existing catalog, amount, recurrence and reminder behavior, and accurate Off/Simulation summaries.
- [x] Extend dirty-exit confirmation to normal Add/Edit; keep immediate clean exits and origin return. Reveal/focus invalid controls with associated errors.
- [x] Add safe PHP paste normalization through the existing exact-centavo parser, with malformed grouping rejected.
- [x] Syntax checks passed. `node --test prototypes/design-exploration/tests/model.test.cjs prototypes/design-exploration/tests/ph-catalog.test.cjs prototypes/design-exploration/tests/essential-model.test.cjs`: **14 tests passed**. New cases cover valid PHP/grouped paste with exact rounding, rejected malformed/ambiguous formats and global reminder summaries.
- [x] Focused `browser-check.cjs --essential` and final full `browser-check.cjs` passed without runtime exceptions. The full run includes PH catalog and all nine action/navigation combinations plus IN11–IN15 default Add/Edit flows. It verified collapsed defaults, earlier essentials, dynamic amount requirements, category/disclosure/theme retention, clean/dirty Cancel and route exits, hidden interval/offset/biller error reveal/focus, controlled catalog write failure retention, PHP Save rounding, absent/zero/required amounts, Add return to all four roots and Edit return through Details.
- [x] At 390×844, observed default content fell from the audited 1,607px to **779px**, within the same 626px content viewport. The revised date input begins about **345px** from the body top; the baseline date label began about 662px down. Core fields fit before scrolling at the tested 360×800, 390×844 and 430×932 frames. The 320×568 frame retained scrolling and 48px sticky actions without horizontal overflow. Both themes passed.
- [x] Inspected `.qa/essential-light-mobile.png` and `.qa/essential-dark-mobile.png`. Verified 272 local links, 74 heading anchors, 22 HTML/CSS assets, source whitespace and current **27-file** source/dependency manifests. The prior selected-layout manifest is preserved separately. Scoped Git comparison confirmed original `mobile-review`, native source and native test projects unchanged; no MAUI build was run.

New-editor checks exposed a real inherited scroll-position issue: returning to a new Add/Edit could restore the previous form's scroll position. Each new editor now starts at its essentials, while Keep editing retains the active draft. Early focused assertions also needed correction for native closed-disclosure visibility, page-relative geometry after screenshot scrolling, summary-toggle interaction and startup readiness. The final runs above passed; intermediate failed attempts are not passing evidence.

Approximately 30-second creation remains an unmeasured user-test target. No real phone keyboard/screen-reader or native acceptance is inferred.
