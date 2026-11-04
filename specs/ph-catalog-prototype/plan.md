# Philippine catalog prototype plan

Scope: [spec](spec.md). Evidence: [tasks](tasks.md). Deliverable: [Focus HTML](../../prototypes/design-exploration/index.html#a/dashboard).

## Modules

| Module | Responsibility |
|---|---|
| ph-catalog.js | Code-owned category/provider IDs, curated optional suggestions, normalized lookup, strict versioned browser-local custom catalog CRUD and last-good-state handling. |
| ph-ui.js | Searchable category bottom sheet, custom catalog modal/management, safe reference reassignment, optional biller suggestion rows and draft schedule fields. |
| app.js | Select Focus, integrate localized IDs/labels across views, preserve existing due-type amount rules and Save/Cancel, commit sample category/provider/schedule fields. |
| exploration-model.js | Extend existing date-only preview with quarterly/custom intervals and safe clamping/bounds. No occurrence generation. |
| index.html / styles.css | Native dialog shell, consistent semantic themes, recognizable icons, compact controls, responsive sheet and selected Focus presentation. |
| tests/ph-catalog.test.cjs | Meaningful inventory, identity, normalized-name, persistence/failure, suggestion, deletion and recurrence-boundary cases. |
| tests/ph-browser-flows.cjs / browser-check.cjs | Current selected-Focus browser acceptance and local-only Edge/CDP lifecycle. Earlier four-concept results remain historical. |

## Implementation decisions

Use the existing shared money/status model and fonts without modification. Add the taxonomy as a separate stable ID rather than overloading the four legacy amount-policy values. Retain fixture financial fields and render catalog names by ID, so renames need no rewrite of due amounts or titles. Keep provider IDs independent; save unknown manual names only after due fields validate. Do not add a package or network API.

Catalog writes occur before committing a new in-memory catalog or reassigning due references. Use native dialogs for keyboard focus containment and explicit confirmations; catalog changes are deliberate even when the surrounding due editor is later canceled. Only catalog/recent choices are persisted. Display the sample-versus-persistent boundary in My Categories & Billers.

New brief/user clarification supersedes the prior active four-way/refinement choice flow. Keep its code/reference evidence while removing alternatives from the normal Focus interface. Record this in the existing exploration spec rather than changing approved native DESIGN/PRD contracts.

## Verification

1. Run existing helper checks plus the new catalog tests.
2. Exercise picker search/grouping, custom creation/duplicates/icons/colors, reload, rename, filter/Details/agenda labels, safe deletion and a controlled write failure.
3. Verify manual/omitted provider, variable money, absent/zero, quarter/custom interval and multiple reminder-offset validation; retain draft and cancellation behavior.
4. Check selected navigation, preference drafts through catalog/Reminders, opt-in simulation, both themes and compact/desktop geometries; inspect screenshots.
5. Preserve earlier manifests, record a new source/dependency manifest, validate links/whitespace and verify the original prototype/native source are untouched. Do not run a MAUI build for this HTML-only artifact.
