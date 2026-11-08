# ADR 010: Export and import as a user-chosen, versioned JSON file

Status: accepted for the UX05 slice. Decision date: 2026-11-06.

## Context and provenance

Duewise keeps dues only in app-private SQLite with Android backup disabled (`allowBackup=false`), so uninstalling, clearing data or losing the phone loses every due. The 2026-11-06 [UX review](../delivery/UX-REVIEW-2026-11-06.md) ranked this the most severe data risk. The user decided the same day ([decision log](../../specs/ux-improvements/spec.md#decision-log)):

- UX05 option A: export to a file the user chooses, and import into an empty app only. Rejected: B, Android Auto Backup to the user's Google account.
- The follow-up: JSON with a format version number.
- At the start of the wave, "empty" means no dues on screen, and a file due replaces a hidden deleted one with the same ID.

The user authorized implementation with "start wave 3". The technical choices below were proposed by Claude while implementing; their rationale is stated here for the learner's review and has not been separately approved.

## Decision

1. **System file dialogs (Storage Access Framework).** Export uses `ACTION_CREATE_DOCUMENT` and import uses `ACTION_OPEN_DOCUMENT`. The user picks the location, which grants access to that one file. Bytes move through `ContentResolver` streams, with no copy in app storage. The dialogs answer through MAUI's Android `OnActivityResult` lifecycle event.
2. **Format `duewise.dues`, version 1.** UTF-8 JSON: `format`, `version`, `exportedAt` and `dues`. Each due carries the fifteen stored columns as stored, including currency, timezone and the exact timestamp strings. Deleted dues are included, an absent amount is `null` and zero is `0`.
3. **Strict reading, hand-written.** `Utf8JsonWriter`/`JsonDocument` instead of `JsonSerializer`. Reading checks `format` and `version` first: a higher version means "update the app" and a lower or missing one means "not an export". It then requires exactly the version-1 fields with the right types and no repeated names, and validates every due with the domain rules. Any problem refuses the whole file. Files are limited to 5 MB.
4. **One transaction, empty check inside it.** `SqliteDueArchive.ImportAsync` validates again, counts showing dues inside an IMMEDIATE transaction and upserts each row. The upsert may only replace a deleted row (`ON CONFLICT … DO UPDATE … WHERE dues.deleted_at IS NOT NULL`); a conflicting active row aborts the import.
5. **Dues only.** Reminder settings and app preferences are not in the file.
6. **Core owns the format, the App owns the words.** Core throws `DueArchiveException` with a problem code, due position and field name; `DataTransferViewModel` turns these into messages.

## Rationale

- SAF needs no storage permission and no new package; MAUI has no built-in save dialog, and CommunityToolkit.Maui's would add a dependency for one call. Reading the stream directly avoids MAUI FilePicker copying the chosen file into the app cache.
- Storing the columns as stored makes a round trip exact and keeps data the domain does not model (currency, timezone, historical timestamp offsets).
- The Android Release build trims code and disables reflection-based `JsonSerializer`; a writer and reader by hand need neither reflection nor source generation, and they spell out each rule.
- Refusing the whole file keeps the user's mental model simple: import either restores everything or changes nothing.
- Checking emptiness in the transaction closes the gap between choosing the file and confirming. The guarded upsert lets "export → delete everything → import" restore data, which the user chose.
- F05 requires explicit reminder opt-in on each installation, so restoring an enabled reminder setting from a file would bypass the permission flow.

## Alternatives considered

2026-11-08 format extension: [native Philippine preset categories](../../specs/ph-categories-native/spec.md) add `categoryId` to each exported due. The current writer is version 2; the reader retains strict version-1 compatibility with Other as the fallback. Version-2 records require a valid preset ID. The original version-1 decision above is retained as historical rationale; [SDD's archive contract](../SDD.md#ux05-export-and-import-contract) owns the current technical format.

- **Android Auto Backup** (the user's option B): automatic, but sends financial data to cloud backup and reverses `allowBackup=false`.
- **Share sheet:** one tap to Drive or chat apps; rejected because the decision was a file the user chooses, and sharing to a chat app is easy to do by mistake.
- **CSV:** opens in spreadsheets, but has no version field, loses null-versus-zero without conventions, and is easy to damage by editing; the user chose JSON.
- **Reflection or source-generated `JsonSerializer`:** less code, but reflection is off in trimmed builds, and generated contracts still need hand-written rules for unknown fields and repeated names.
- **Merge into a non-empty app:** needs conflict rules per due; excluded by the decision.

## Consequences

- The file is plain text: anyone who opens it can read the dues. The Settings card says so. Encryption would need a new decision.
- A future schema change must also decide whether the export format changes. Adding, removing or retyping a field needs version 2, and version-1 files must stay readable or be refused clearly.
- Import overwrites a hidden deleted row that shares an ID with the file. Rows only in the app stay as they are.
- Phone-specific providers (Drive, OEM file managers) may label JSON differently; the open dialog offers `application/json`, `text/plain` and `application/octet-stream`.

Links: [spec EX01–EX08](../../specs/f04-export-import/spec.md), [PRD UX05](../PRD.md#ux-review-decisions-2026-11-06), [SDD](../SDD.md#ux05-export-and-import-contract).
