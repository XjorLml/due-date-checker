# Focus navigation and Philippine catalog — HTML only

Authorized by the user's supplied brief and explicit **“HTML prototype only”** clarification on 2026-11-04. Extend [design-exploration](../../prototypes/design-exploration/index.html#a/dashboard); preserve the original prototype and all MAUI/Core/database code. Earlier exploration scope/evidence remains in [mobile-ui-prototype](../mobile-ui-prototype/spec.md). This spec owns the new browser catalog, not production category or scheduler contracts.

## Scope and contracts

Use the selected Focus navigation, its existing centered Add and four destination tabs. Stop presenting navigation/refinement alternatives in the normal flow; older concepts are historical references. Default entry opens Focus. The [PRD](../../docs/PRD.md) and [SDD](../../docs/SDD.md) still govern native functionality; [DESIGN](../../docs/DESIGN.md#scope-and-authority) remains the adopted production design. Current presentation and behavioral deviations remain confined to HTML.

Implement all ten requested groups and their 52 named specialized leaves, plus **Other** as a safe fallback: **53 built-in categories**. Other's visible Create Custom Category action replaces the idea of a literal built-in “Custom Category” record. Keep both Mortgage / Housing Loan and Housing Loans as requested, with distinct stable IDs. Provider suggestions are the user-supplied examples, never verified payment integrations.

Separate three concepts:

- **Due type** retains Bill/Task/Subscription/Other and its existing amount policy. It remains a field labeled “Due type (amount policy)”; this preserves existing Task/Other optional amounts and explicit zero.
- **Category** is a stable `ph.<group>.<leaf>` or `custom.category.<uuid>` ID with a display name, icon and color. It controls grouping/filter labels, not amount inference or status/date order.
- **Provider/biller** is an optional independent `provider.<slug>` or `custom.provider.<uuid>` ID. Suggestions follow the chosen category, but choosing a different category does not silently replace an already entered biller. Manual names are saved/reused on successful due Save. No payment connection exists.

The supplied fixture is enriched by explicit ID mapping only: rent → Housing/Rent, internet → Internet/PLDT, music → Streaming/Spotify, electricity → Electricity/Meralco, water → Water/Maynilad, task → Other. Title, amount, due date, notes, paid state and ID stay intact. These are fictitious sample associations, not guesses about real user records.

## Browser persistence and safe deletion

`duewise.ph-catalog.v1` contains a version-1 JSON record with custom categories, custom providers and up to six recent category IDs. Reuse browser local storage already used for review preferences, in a separate namespace. Built-ins remain code-owned; sample financial records remain in memory. Custom catalog entries persist across reload and sample reset; sample dues reset. Do not store financial titles/amounts/notes in the catalog key.

Read validation rejects malformed/unsupported data without rewriting it, retaining the last good/default state and an actionable error. A single `setItem` writes the proposed catalog before publishing new in-memory state; failure keeps the previous catalog and active form. This is browser preference storage, not SQLite, cross-tab transactional storage or a native durability guarantee.

Names trim/collapse whitespace and compare case-insensitively after NFKC normalization. Empty and duplicate category names, including built-in names, are rejected. Renaming preserves identity. Custom icons/colors are optional curated choices; names render as literal text.

Deleting a custom category requires confirmation with its sample reference count. After a successful catalog write, all references move to Other; no due is deleted and no amount/date/notes/payment fields change. An active draft also falls back to Other, and a deleted filter resets to All. Cancel or failed write changes nothing. Deleting a custom biller similarly retains its latest name as manual text on referenced dues. Built-in categories/providers cannot be edited or deleted. Custom billers can be created manually, renamed and deleted from My Billers; normalized duplicates reuse the same ID on due Save.

## Localization and schedule controls

Use PHP amounts, `en-PH` date/number presentation and Asia/Manila calendar context. Category/provider suggestions do not imply that all obligations share a schedule or have fixed monthly amounts. Amounts remain manually editable with the existing exact centavo validation.

Support None, weekly, monthly, quarterly, annually and a custom positive interval of 1–365 days/weeks/months/years. Preview the next date with month-end/leap handling and supported-date bounds; do not generate a series. Per-due reminder drafts allow multiple offsets: due date, 1/3/7 days before and additional comma-separated whole days from 0–365, deduplicated/sorted on Save. Global time/quiet-hour controls remain. These are HTML simulations, without OS permission, alarms or delivery. Series mutation and real per-due scheduling remain separate native product work.

## Observable acceptance

| ID | Outcome | Boundary |
|---|---|---|
| PH01 | Focus is selected by default, no new navigation alternatives appear, centered Add and active tab labels remain functional, and Save/Cancel return to the originating root. | Browser/default entry and navigation. |
| PH02 | All ten groups/53 stable built-ins are searchable; the bottom sheet shows frequent/recent choices, My Categories, collapsed groups, icons, selected state and visible custom creation. | Catalog inventory and browser picker. |
| PH03 | Add/Edit keep category and optional biller separate; relevant suggestions are editable, unknown names are allowed, and normalized saved providers have stable identities. | Pure catalog and browser workflows. |
| PH04 | Custom category create/rename/delete validates names and presentation, persists across reload, retains identity and protects existing dues on cancel/confirm/write failure. | Catalog storage tests and browser reference checks. |
| PH05 | Custom provider create/rename/delete persists; deleting a referenced provider retains its name without deleting a due. | Catalog tests and browser manager. |
| PH06 | Rename/selection/deletion appear consistently on Dashboard, matching-day Calendar, Filters, Details and Editor without changing amounts/status/date ordering. | Matching fixture and browser surface checks. |
| PH07 | Manual amounts, optional/zero distinction, recurrence/custom preview, multiple reminder offsets and validation round-trip; Cancel discards drafts. | Helpers and browser Save/Cancel. |
| PH08 | Light/dark picker/manager/editor fit 320–430-pixel phones and desktop; named controls, native modal focus, readable wrapping and persistent Save remain. | Browser geometry/screenshots; native acceptance separate. |
| PH09 | Original HTML and production source remain unchanged; source hashes, links, limits and observed evidence are recorded. | Scoped diff/documentation review. |

No Android implementation, schema migration, app deployment, release/signing action or real reminder-delivery claim is authorized. Browser storage may be unavailable or cleared; failures are explicit. Cross-tab sync, catalog import/export, real payment integrations and native acceptance are outside this HTML task.

Native follow-up, 2026-11-08: the user separately authorized [preset Philippine categories on Android](../ph-categories-native/spec.md), without custom categories or billers. The original HTML-only scope and browser evidence above remain historical and do not establish native acceptance.
