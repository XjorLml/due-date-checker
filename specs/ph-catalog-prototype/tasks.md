# Philippine catalog prototype evidence

User scope: **HTML prototype only**, explicitly confirmed on 2026-11-04. Implementation/scenarios are in [spec](spec.md); [plan](plan.md) maps modules. All results below are agent-observed unless stated otherwise.

## Completed implementation

- [x] Read the supplied brief, existing docs/design/contracts, prototype source and applicable helpers/tests; preserve the working tree and earlier evidence.
- [x] Select Focus in the normal UI, retain the centered Add/four destinations and suppress new navigation alternatives.
- [x] Add all ten localized groups, 53 built-in records including safe Other fallback, stable independent provider IDs and optional editable suggestions.
- [x] Build searchable grouped category modal with frequent/recent/My Categories, selected state and visible custom creation for Add/Edit and filtering.
- [x] Persist custom category/biller names, IDs, icon/color choices and recents under a separate browser catalog key; validate empty/normalized duplicates and retain data on read/write errors.
- [x] Add My Categories & Billers management, identity-preserving rename, confirmed category-to-Other reassignment and biller-name retention on deletion.
- [x] Use `en-PH`, PHP and Manila date context; preserve the old due-type amount policy separately from localized taxonomy.
- [x] Add quarterly/custom interval previews and multiple per-due reminder offsets with explicit simulation boundaries; retain manual amounts and Save/Cancel.
- [x] Resolve category/provider labels by ID on Dashboard, Calendar, Filters, Details and Editor.

## Verification — 2026-11-04

Environment: Windows workspace, Node v24.16.0 and installed Edge 156.0.4314.58 via headless CDP. Artifact: `prototypes/design-exploration/` plus unchanged shared model/fonts, identified by [initial source/dependency manifest](source-initial.sha256). That manifest preserves this pre-interaction-lab evidence; follow-up results belong to the [interaction lab tasks](../interaction-lab/tasks.md). Earlier exploration manifests retain historical evidence, not passing results for this revision.

- [x] Syntax checks passed for current app/catalog/UI modules.
- [x] `node --test prototypes/design-exploration/tests/model.test.cjs prototypes/design-exploration/tests/ph-catalog.test.cjs`: **11 tests passed**. Includes original money/date/contrast checks and localized inventory, independent IDs/suggestions, NFKC/case/whitespace duplicates, persistent rename, read/write failure retention, recent/frequent/search, provider reuse and quarterly/custom boundaries.
- [x] `node prototypes/design-exploration/tests/browser-check.cjs`: passed. Verified Focus/default entry, all four Add/Cancel roots, grouped/searchable picker, duplicate/empty custom creation, icon/color selection, catalog reload/rename, optional suggestions/manual provider, exact amount/custom interval/multiple offsets, due-edit Cancel and category/provider labels on matching surfaces.
- [x] Confirmed category deletion Cancel preserves its ID; confirmation reassigns its referenced sample due to Other while preserving amount/date/notes/paid state and record count. Custom biller deletion preserves the displayed name. Controlled storage write failure retains the editor/catalog; literal HTML-like category names render as text.
- [x] Verified absent Task amount and explicit zero, invalid custom interval/offset messages, saved Sunday start, Settings draft through Reminders/catalog and opt-in Save-only/Cancel/confirm.
- [x] Light/dark root/Reminders/editor and sheet geometry passed at 320/360/390/430/768/1600 pixels. Catalog page 200% browser-base-text geometry, unique IDs, local fonts and direct-file startup passed. Inspected desktop Focus, dark picker and light provider form screenshots under ignored `.qa/`.
- [x] Final review passed: 213 local links, 54 heading anchors, HTML/CSS assets, whitespace and 19 source/dependency hashes. Scoped Git comparison confirmed `prototypes/mobile-review/`, production source and native test projects are unchanged by this task. Original exploration/recommendation manifests remain preserved separately.

Two initial browser assertions assumed a newly created Today due would be in the three-row Dashboard priority excerpt and on any selected Calendar day. The fixture was adjusted to a visible earlier date and the test selected that exact day; no product ordering changed to satisfy the test. A Windows shell Unicode replacement damaged a separator/range/status glyph in edited strings; those strings were corrected and the completed browser run passed. These failed attempts are not passing evidence.

## Limits

- Native Android code, SQLite, permissions, signing and release state are unchanged. Native accessibility/keyboard/notification delivery acceptance is not established by browser tests.
- Custom catalogs persist locally; sample dues remain in memory and reset on reload. No real payment integrations, recurring series or OS reminders exist in this HTML.
- User acceptance of the localized experience remains open. No production adoption is inferred.
