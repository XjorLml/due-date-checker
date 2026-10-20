# Duewise design adoption

Authorized by the user on 2026-10-17: implement design adoption for existing pages, then plan future pages. Release remains on hold. State and evidence: [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).

## Scope and governing contracts

Apply [DESIGN scope](../../docs/DESIGN.md#scope-and-authority), [brand](../../docs/DESIGN.md#2-brand-identity), [colors](../../docs/DESIGN.md#3-color-system), [typography](../../docs/DESIGN.md#4-typography), [layout](../../docs/DESIGN.md#5-layout-and-spacing), [components](../../docs/DESIGN.md#7-components), and [accessibility](../../docs/DESIGN.md#11-accessibility) to the existing dues list and editor. Preserve [PRD R01–R07 and business rules](../../docs/PRD.md#approved-requirements) and [SDD navigation](../../docs/SDD.md#6-ui-and-navigation).

Adopt Duewise as the launcher/display name, a calendar/checkmark vector identity, bundled licensed Plus Jakarta Sans, semantic theme resources, responsive spacing, readable cards, outlined fields, and primary/secondary/destructive controls. Keep package identity, SQLite path/schema, IDs, list ordering, PHP validation, Manila dates, payment-on-Save, Cancel/Back, and confirmation without Undo.

Existing behavior: local CRUD, retry/busy/error states and persistence. Proposed changes: presentation plus field validation on blur (same rules as Save); never write from blur. New pages, statuses, categories, reminders, navigation tabs and data contracts are excluded. Future-page planning is separate from this visual-adoption slice.

## Acceptance scenarios

| ID | Observable outcome | Verification boundary |
|---|---|---|
| DA01 | Launcher, shell and splash use Duewise identity; existing installation/data identity retained. | Source/build, then phone update/launcher. |
| DA02 | Light and dark views use the DESIGN palette for canvas, surface, copy, borders, actions and errors; no template purple. | Resource review and native theme inspection. |
| DA03 | Regular, medium, semibold and bold use bundled Plus Jakarta Sans with an included license; no font download at runtime. | Asset/provenance review and APK inspection. |
| DA04 | At compact/phone/tablet widths, content has documented margins and a bounded readable width. At 200% text, amount/date/errors/actions wrap and remain reachable by scrolling. | Native large-text inspection; untested widths remain explicit. |
| DA05 | Cards show title, explicit date, PHP amount and Paid/Unpaid text with a status symbol, retaining date order and edit navigation. | App regression plus native list/edit checks. |
| DA06 | Inputs retain labels/hints; focus and validation errors have visible outlines. Blur validates without persisting or trimming user input. Save validates again. | App validation test and native input/focus inspection. |
| DA07 | Actions are at least 56 dp tall, wrap text, distinguish primary/secondary/destructive roles, and expose pressed/focused/disabled states. Busy indicators retain action labels and guards. | XAML/build plus native inspection. |
| DA08 | Empty, loading, startup/read/write error and retry states retain their existing meaning in both themes. | Existing App tests plus available native scenarios. |
| DA09 | Create/edit/paid/unpaid/delete, cancellation and restart persistence still work offline. | Core/App regression and native CRUD smoke; preserve prior evidence separately. |
| DA10 | Labels, reading order and error focus support TalkBack; no action or payment state depends on color alone. | Native accessibility review; visual inspection alone cannot close TalkBack acceptance. |

## MAUI adaptations and limits

Use native controls and dialogs, not a new component framework. Forms and cards remain single-column at all text scales to avoid clipping; large-screen content is centered rather than introducing future two-pane navigation. Native focus/press feedback implements state changes without custom motion. Only current Paid/Unpaid state is styled; overdue/completed lifecycle rules remain deferred. Icon/splash assets are native vectors; the in-app mark reuses the same identity. System text scaling and system appearance remain authoritative; no settings screen is added.

Do not claim complete WCAG or full DESIGN compliance from a build or a resource audit. Record device model, artifact, themes, text scale, exact actions, observer and unperformed scenarios in delivery evidence.
