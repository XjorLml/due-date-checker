# Focus interaction comparison — HTML only

User authorization: supplied Navigation Consistency and Action Button UX Redesign brief, 2026-11-04. Status/evidence: [tasks](tasks.md). Approach: [plan](plan.md). Audit/recommendations: [interaction audit](../../prototypes/design-exploration/INTERACTION-AUDIT.md).

## Scope and authority

Compare Cancel/Save placement and navigation behavior inside the existing HTML artifact. Retain Focus's four destinations, centered Add, tokens, localized catalog and existing sample business logic. [PRD R02–R07](../../docs/PRD.md#approved-requirements) and [SDD navigation](../../docs/SDD.md#6-ui-and-navigation) remain native authorities. [DESIGN scope](../../docs/DESIGN.md#scope-and-authority) explicitly distinguishes proposals from production. The [PH catalog spec](../ph-catalog-prototype/spec.md) still owns sample category/provider contracts.

The comparison brief authorized new **behavior** alternatives despite the previous request to stop proposing visual navbar alternatives. Its three action layouts and three navbar policies are independent. Neither preview defaults nor choosing a browser preference approves permanent implementation. The subsequent explicit user request, “update the html mock desingn with Sticky bottom actions and Context-aware navigation,” approves A actions/B navigation for the default Focus HTML mockup. No native feature/schema, notification, payment, account, dependency or hosting change is included.

## Acceptance scenarios

| ID | Observable result | Boundary |
|---|---|---|
| IN01 | Dedicated lab has side-by-side action previews and three explained navigation options. Preview and Choose are distinct; no choice is preselected. | Browser UI and local storage. |
| IN02 | A: adjacent outlined Cancel/primary Save; B: text Cancel above full-width Save; C: header Cancel/bottom full-width Save. Same long Add/Edit form, validation and mutations. | Scroll, widths and Save/Cancel. |
| IN03 | All policies use the shared Focus nav on Dashboard/Calendar/Dues/Settings, with identical height/position/icons/labels and accurate selection. | Geometry across roots, modes and widths. |
| IN04 | A also keeps tabs below pushed-page actions; B hides them on focused pages; C uses a native modal above the parent/tabbar. Tabs behind modal cannot be activated. | DOM/native dialog, hit testing and focus. |
| IN05 | Layout switches, category sheets and Keep editing retain every draft field. Dirty Cancel, tab exit and Escape ask before discarding. Settings child pages preserve their parent draft. | Browser flows. |
| IN06 | Action targets are at least 48px; nav targets at least 44px. Footer remains outside the content scroll region, with safe-area padding and viewport sizing. Keyboard reservation demo is clearly simulated. | Geometry; real devices remain separate. |
| IN07 | Disabled, loading, error and success previews are labeled. Actual Save validates, shows busy state, blocks repeated submissions and writes once. Preview error retains draft; success preview never claims a write. | Browser event and record-count assertions. |
| IN08 | Two choices persist independently under `duewise.interaction-lab.v1`, export separately, can be cleared, and do not change preview defaults or the ordinary Focus entry. Storage failure is visible. | Browser persistence/export. |
| IN09 | Existing PH catalog, amounts, custom intervals, reminders, sample Save/Cancel and direct-file startup continue to work. Original HTML and native source remain unchanged. | Existing helper/browser suite and scoped Git diff. |
| IN10 | Default Focus opens without entering the lab. Add/Edit show adjacent 48px outlined Cancel/primary Save below the scroll owner; header Cancel is removed and Edit Delete stays in the header. The shared centered-Add navbar stays on all four roots and hides on pushed pages. Returning from another lab combination restores this selected layout without rewriting reviewer preferences. | Default-route geometry, Add/Edit Save/Cancel, both themes, responsive browser regression. |
| IN11 | Essential-first Add/Edit presents What is due?, Amount (PHP), Due date before optional metadata. The date is substantially earlier than the audited 662px label offset. Required/optional labels and Amount's semantic requirement track the independent Bill/Subscription/Task/Other policy. | Rendered order/fold and amount-policy browser checks. |
| IN12 | Type uses a compact Change disclosure; Category remains optional with Other fallback. Repeat/Reminders are collapsed summaries; biller/notes and custom offsets are expandable. Draft values and disclosure state survive category renders, theme/layout changes and validation. | Browser retention, catalog and schedule regression. |
| IN13 | Reminders show global Off or enabled offset/time summary and explicit Simulation status. Existing frequency/custom offsets remain preferences, without recurring-record generation or OS scheduling. | Summary/helper and browser assertions. |
| IN14 | Normal Add/Edit dirty exits offer Keep editing/Discard changes; unchanged forms leave immediately. Errors inside closed disclosures open their ancestors, focus the invalid field and reference associated error text. Add returns to its originating root; Edit returns through Details to that root. | Cancel/route/Save/focus browser flows. |
| IN15 | Well-formed PHP prefixes and grouped pasted amounts normalize at the UI boundary; malformed/ambiguous grouping is rejected by the unchanged exact-centavo parser. Blank versus zero and rounding are preserved. | Pure helper and browser Save/error tests. |

## State contracts

Only explicit Choose controls persist `action` and `navigation` values (`a`, `b`, `c`, or null). Stored financial data is not added. Preview defaults remain A actions / B navigation; the normal Focus mockup always uses the explicitly approved A/B combination, regardless of lab preview or stored reviewer choices. Existing sample dues stay in memory. The later explicit approval of Alternative A extends dirty-exit protection to normal HTML Add/Edit. Busy simulation and Settings/reminder-draft guards remain lab-only; native PRD/SDD behavior is unchanged.

The layer moves the **single live** header/body/action DOM into a native modal; it does not clone the editor, navbar or Save logic. The backdrop retains a read-only parent rendering. Nested category/editor/confirmation dialogs keep native top-layer ordering. Layer-local preview selectors permit switching while the rest of the document is inert.

## Acceptance still required

User selection of A/B for the HTML mockup is confirmed by the explicit follow-up request. Production adoption remains separate. Actual iOS/Android keyboard, safe area, system Back, touch and screen-reader behavior require device review; headless layout tests and simulated keyboard reservation do not establish them.

## Essential-first approval and boundary

The user explicitly approved Alternative A and supplied the implementation prompt after a read-only audit of the actual current Add Due page. The audited default had 1,607px of content in a 626px scroll region at 390×844; Due date's label began about 662px below the body top. This is baseline evidence, not a measurement of the revised page. IN11–IN15 authorize the above HTML refinements, reusing existing category/provider identities and schedule contracts. They do not approve recurring series, real notification delivery, per-due quiet-hour overrides, backend persistence, title suggestions or a native app change.

Disclosure visibility is temporary editor UI state, separate from financial fields and dirty comparison. The existing explicit custom-catalog Save still persists independently of due Cancel. PHP paste normalization never uses floating-point arithmetic, never blindly strips malformed commas, and feeds the unchanged shared amount validator. Approximately 30 seconds is a timed-user-review target; automated browser duration cannot establish it.
