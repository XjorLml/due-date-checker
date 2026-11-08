# Native Philippine preset categories

Authorized 2026-11-08: “implement the detailed philippine categories”, clarified by the user as **preset Philippine categories only**. Uses the existing [HTML catalog](../../prototypes/design-exploration/ph-catalog.js) as the label/ID reference; that browser spec remains separate. [Plan](plan.md), [tasks](tasks.md).

## Scope and contracts

Extend F07 with an optional detailed category, independent of the existing Bill/Subscription/Task/Other due type. Copy the prototype's 53 stable `ph.*` IDs and labels across 10 groups. Add a searchable native picker, saved selection, row/Details labels, and preservation through recurrence and export/import. Existing dues migrate to `ph.other.general`; existing type, amount, status, reminders, Focus navigation, editor drafts and sticky actions remain intact. [PRD](../../docs/PRD.md), [SDD](../../docs/SDD.md), [DESIGN](../../docs/DESIGN.md) remain canonical.

Custom categories, category management, billers/providers, category-derived amounts/repeat rules, payment integrations and changes to the HTML prototype are excluded by the user's scope choice.

## Acceptance

| ID | Scenario | Observable result |
|---|---|---|
| PC01 | Catalog/search | All 53 IDs/names match the prototype; 10 groups; search finds names/groups and common Filipino aliases. Invalid IDs rejected; Other remains available. |
| PC02 | Upgrade schema 5→6 | Every existing row/timestamp/type/amount/paid/deleted/repeat field survives; new detailed category defaults to Other. Migration is transactional/idempotent; newer databases are preserved/rejected. |
| PC03 | Add/Edit/category picker | Core fields remain early; optional category opens a searchable grouped list with visible current selection. Tapping changes the editor draft, Cancel/Back retains the prior choice, other fields remain intact. Save/reopen persists; dirty exit includes category changes. |
| PC04 | Type independence | Choosing Electricity for Task does not require an amount; Subscription still requires it regardless of category. Type-based repeat eligibility stays unchanged. |
| PC05 | Saved projections | Dues/Dashboard/Calendar rows and Details identify the detailed category without losing type/status/date semantics; Other fallback keeps legacy row presentation. |
| PC06 | Payment/recurrence/reminders | Next occurrence retains the detailed category, including notification payment. Category edits invalidate old notification action snapshots. Undo/soft-delete preserve the field. |
| PC07 | Backup/import | New exports use format 2 including categoryId; format-1 exports remain readable and default to Other. Format-2 missing/invalid IDs fail without partial writes. Tombstones and NULL-versus-zero amounts survive. Older apps safely reject newer exports. |
| PC08 | Native verification | Build/tests and targeted emulator selection/search/save/edit/cancel/reopen, themes and compact/large-text layout recorded separately from actual phone/TalkBack acceptance. |

No data is inferred from a due title. The fixed due-type enum's persisted numeric codes remain unchanged.
