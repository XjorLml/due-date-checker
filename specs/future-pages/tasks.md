# Future-page planning and execution tracking

[Spec](spec.md) owns planning/mock history and [plan](plan.md) owns priorities/dependencies. F01, F02 and F03 have dedicated specs/tasks; other production features remain deferred. Earlier mock/BN milestones below describe their original artifacts.

## Planning delivered

- [x] Distinguish current list/editor from proposed future pages (FP01).
- [x] Map every DESIGN screen, including four onboarding steps and shared empty/error states (FP02).
- [x] Propose dependency order and modules without registering routes or adding migrations/services (FP03).
- [x] Record conflicts and product decisions for each relevant slice (FP04).
- [x] Define per-slice spec, requirement, architecture and verification entry checks (FP05).
- [x] Include later auth/sync, budget, savings and widget/platform discovery without inventing missing designs (FP06).
- [x] Compare the 2026-10-18 supplied prototype with inspected source/tests, map all numbered areas and visual/navigation differences, and prioritize important features (FP07). [Comparison](../../docs/delivery/PROTOTYPE-REVIEW-2026-10-18.md); [priority order](plan.md#priority-order-after-prototype-review).

## Authorized mock milestone

Delivered before bottom navigation. Its original push/pop controls and native checks are historical; the following milestone replaces navigation while retaining the sample content boundary.

- [x] Define sample-only scope, MP01–MP05 and affected modules; preserve existing work.
- [x] Implement shared fixture, dashboard filters/summary and calendar month/selection state.
- [x] Implement native pages, read-only cards, responsive layout and guarded navigation.
- [x] Verify meaningful state behavior and existing Core/App regressions; build Android: 28 Core + 54 App passed, final Release build 0 warnings/errors.
- [x] Exercise phone preview flows/themes/text scaling: after user unlock, summary/filters, calendar/agenda/empty state/month/Today, Back/exits/repeated navigation, offline reopening and both themes at 100%/200% passed. [Evidence and remaining acceptance](../../docs/delivery/evidence/PAGE-MOCKS-2026-10-17.md).
- [x] Update canonical scope/contracts/status and validate documentation links/diff.

## Authorized bottom navigation milestone

- [x] Inspect current source/contracts and preserve prior work; define BN01–BN05 and navigation policy.
- [x] Add native Shell bottom tabs, theme colors and vector icons; retain preview page state.
- [x] Remove obsolete stacked-preview controls/routes and hide the bar in the editor.
- [x] Run existing regression suites and Android Release build: 28 Core + 54 App passed; build 0 warnings/errors.
- [x] Verify native tab/state/Back/editor flows, both themes and large text. Initial native 200% crowding corrected with adaptive bar height; final renderer/state/Back/editor-hiding checks and four theme/text cases passed. Temporary Save/Cancel/delete cleanup is separately attributed to the initial artifact. [Execution evidence](../../docs/delivery/evidence/BOTTOM-NAVIGATION-2026-10-17.md).
- [x] Update canonical navigation/scope/status and validate documentation links/diff.

Full TalkBack, keyboard/switch access, landscape and additional widths/devices remain native acceptance work; targeted phone results do not establish complete accessibility compliance. These original mock results predate production F02/F03; release remains held.

## Production implementation and remaining slices

Use the [P0–P4 priority order](plan.md#priority-order-after-prototype-review); slice numbering is a stable identifier, not execution order. P0 is existing-core hardening, with recorded passes and remaining gates in STATUS. Unchecked boxes remain implementation work, regardless of completed planning or sample UI. Checked implementation must link actual verification and remaining limits.

- [x] F01: implemented authorized item details; 97 tests, clean Android build and targeted native/offline/theme/text checks passed. [F01 tasks](../f01-item-details/tasks.md) retain remaining full accessibility/native hardening/release acceptance.
- [x] F02: implemented saved-data Dashboard/status/search; 116 tests, clean final Android build and artifact-specific native/offline/theme/text checks. [F02 tasks](../f02-dashboard/tasks.md) retain remaining accessibility/hardening/release gates.
- [x] F03: implemented saved-data Calendar/agenda, fixed Monday start, Manila Today and retained parent navigation; 124 tests, clean build, targeted native/offline/cross-tab/theme/text checks and cleanup. [F03 tasks](../f03-calendar/tasks.md) retain remaining acceptance.
- [x] F04 preference implementation: System/Light/Dark and Monday/Sunday, explicit Save, startup persistence and existing Reminders access; 216 tests and clean Android build. [Dedicated tasks](../f04-preferences/tasks.md) track native verification/remaining acceptance. Export and remaining general Settings concepts stay deferred.
- [x] F05: authorized date-only reminders complete; **186 tests**, clean final Android build, natural offline/screen-off delivery and targeted Doze/reboot/lifecycle/failure/tap/cancellation/layout checks with cleanup. [Dedicated tasks](../f05-reminders/tasks.md) retain broad assistive/OEM/intended-key release acceptance separately.
- [ ] F06: implement onboarding for shipped capabilities.
- [ ] F07 umbrella: [categories](../f07-categories/tasks.md) are implemented with schema 2; recurrence, timing and optional-amount changes remain separate unimplemented extensions.
- [ ] F08: extend shared states/navigation as each destination ships.
- [ ] F09: discover/specify account/auth/sync behavior and page design.
- [ ] F10: discover/specify budget and savings behavior and page designs.
- [ ] F11: discover/specify widgets and other-platform support.
- [ ] F12: specify statistics/insights metrics and detailed design, then implement period totals/category charts with required data and accessible text equivalents.

Do not check off an implementation task based on this plan. Record observed results in STATUS only after executing its approved slice and relevant checks.

## F03 production Calendar

The later "implement f03" request authorizes the saved-data Calendar replacement. [Dedicated tasks](../f03-calendar/tasks.md) own implementation, verification and remaining acceptance; [PRD](../../docs/PRD.md#f03-calendar-rules) owns the resolved calendar policy. Original mock/BN entries above remain historical.

## F07 categories delivered independently

- [x] [Category slice](../f07-categories/spec.md): migration/editor/labels/filter implemented, 148 tests and clean build, targeted native upgrade/offline/theme/text checks and task cleanup passed. [Tasks and remaining acceptance](../f07-categories/tasks.md) retain open gates. The F07 umbrella above remains unchecked because recurrence/timing/optional-amount extensions are unimplemented.

## F05 reminders complete for authorized scope

[Dedicated spec/plan/tasks](../f05-reminders/spec.md) own the completed authorized date-only reminder slice. [Completion evidence](../../docs/delivery/evidence/F05-COMPLETION-2026-10-18.md) records 186 tests, clean final build, natural offline/screen-off delivery and targeted lifecycle/failure/cancellation/layout checks with explicit artifact attribution. The initial OEM deferral remains documented: temporary No restrictions allowed phone delivery, and the original policy was restored. Broader assistive/OEM/intended-key release checks remain separate. F04 preferences is now separately authorized under its [dedicated spec](../f04-preferences/spec.md); export needs its own format/privacy boundary. Release stays held.
