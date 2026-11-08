# Native Philippine category plan

Scope/scenarios: [spec](spec.md). Execution: [tasks](tasks.md).

1. Core catalog with immutable preset IDs/groups/names and search. Add optional defaulted `Due.CategoryId`; retain existing `Due.Category` enum as due type. Schema 006 adds the text column without rewriting old fields.
2. Update repository read/insert/update, recurrence copy and notification fingerprints. Extend raw archive records/import SQL. Export format 2 writes categoryId; format 1 reads with Other fallback and format 2 validates the new field strictly.
3. Native grouped searchable picker, optional compact category row after core editor fields, draft/load/save/dirty-exit handling, and detailed labels on shared rows and Details. Use one scrolling list; keep Focus root navigation and the editor's existing fixed actions.
4. Tests at catalog/domain, migration/SQLite, archive, editor/projection and notification-payment boundaries. Update schema-version expectations without changing historical fixture schemas.
5. Release build and task-owned emulator checks. Preserve the user's physical phone/data/signing configuration. Update canonical scope/contracts, existing deferred references and delivery evidence.

No custom-category table or biller persistence is introduced. The prototype remains unchanged. A selection must never alter the due type automatically.

Execution 2026-11-08: implementation, Release packaging and targeted API-36 emulator checks completed; both test projects compile, but Windows Application Control blocks their execution. The [evidence](../../docs/delivery/evidence/PH-CATEGORIES-2026-11-08.md) separates initial functional-APK checks from the final compact-heading refinement and records unexecuted assertions/phone acceptance. Full regression execution and intended-key device/TalkBack acceptance remain open.
