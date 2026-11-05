# Focus design adoption

Authorized by the user on 2026-11-05: implement the selected Focus direction (Minimal Fintech refinement) from the [HTML design exploration](../../prototypes/design-exploration/index.html#a/dashboard) in the native MAUI app, scope **look and navigation**, tested on an Android emulator. Release remains held. State and evidence: [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).

## Scope and governing contracts

[DESIGN 2.0.0](../../docs/DESIGN.md) owns the adopted tokens, components and screen specs; this spec refines how the existing pages adopt them. The [PRD navigation exception](../../docs/PRD.md#exclusions-and-deferred-capabilities) authorizes the five-slot bottom bar with a centred Add action and Settings as a root tab; all other PRD rules (R01–R08, F01–F07, F05) are unchanged. [SDD §6](../../docs/SDD.md#6-ui-and-navigation) owns the Shell, renderer and editor-footer contracts.

In scope: palette, type scale, radius, spacing and focus-outline tokens on all seven pages; a five-slot bottom bar (Dashboard / Calendar / Add / Dues / Settings) with Dues as the start tab; a 56 dp lime Add disc floating over the centre slot; Settings as a root tab; the editor's Essential-first field order; a fixed Cancel/Save footer with Delete moved to the header; removal of the header Add/Settings buttons that the bar now replaces.

Out of scope and still deferred (recorded in the [DESIGN register](../../docs/DESIGN.md#design-explorations-non-normative)): Dashboard-first entry, the Philippine category/provider catalog, repeat controls, weekly Calendar, amount totals and charts, multiple reminder offsets, quick-paid confirmation, dirty-exit confirmation and PHP paste normalization. No schema, repository, reminder or status-logic change is made.

## Acceptance scenarios

| ID | Observable outcome | Verification boundary |
|---|---|---|
| FA01 | Light and dark views use the Focus palette (ink primary, lime accent, warm-grey canvas) for canvas, surfaces, copy, outlines, status text and the Android status bar; no indigo remains. | Resource review, emulator screenshots in both themes. |
| FA02 | Headings are 26/20/16, cards/fields/buttons use 14 dp radius, buttons and fields are at least 48 dp tall, page inset is 20 dp (14 dp under 360 dp). | Styles review, emulator inspection at 100% and 200% text. |
| FA03 | The bottom bar shows Dashboard, Calendar, Add, Dues, Settings in that order on every root page. Dues is selected at cold start. Tapping a destination switches tabs and retains each tab's state; the Add slot never becomes the selected tab. | Emulator navigation; Shell route `//main/dues/list` unchanged. |
| FA04 | A 56 dp lime disc with a plus glyph is centred over the Add slot and raised above the bar. Its accessible name is "Add due". Tapping it opens the editor on the current root's stack; Save and Cancel return to that root. The disc and bar are hidden on the editor, Details and Reminders and reappear on return. | Emulator tap/return from each of the four roots, light/dark, 200% text. |
| FA05 | Settings is a root tab that shows the bar. Back from Settings selects Dues. Reminders pushed from Settings hides the bar and returns to Settings. Reminder taps still defer while Settings is the current page. | Emulator navigation; code review of ReminderTapRouter (unchanged). |
| FA06 | The editor orders fields What is due?, Amount (PHP), Due date, Type, Notes, then Payment status in edit mode. Validation rules, blur validation and first-error focus are unchanged. | Emulator Add/Edit; existing App tests. |
| FA07 | Cancel and Save sit side by side in a fixed footer at least 48 dp tall, visible at both scroll extremes and at 200% text. Delete appears only as a header action for a loaded existing due and still opens the named confirmation. | Emulator Add/Edit scroll and delete flow. |
| FA08 | Dues, Dashboard and Calendar no longer show header Add buttons; Dues no longer shows a Settings button. The Reminders shortcut remains on Dues. Empty-state copy points to the bar's Add action. | Markup review, emulator empty state. |
| FA09 | Create, edit, pay/unpay, delete, cancel, restart persistence, Dashboard search/filters, Calendar selection and reminder settings behave as before. | Core and App suites pass; emulator CRUD smoke. |
| FA10 | Focus outlines are 3 dp. Native text scaling to 200% keeps the footer, bar and disc reachable without clipping. | Emulator 200% text. TalkBack and other assistive checks remain deferred by user direction. |

## Evidence limits

An emulator run is agent-observed evidence on a virtual device, not phone acceptance. The project's phone acceptance, intended-key signing and TalkBack gates remain separate and are tracked in [STATUS](../../docs/delivery/STATUS.md). Browser evidence from the prototype does not transfer to the native app.
