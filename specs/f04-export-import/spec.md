# Export and import dues (UX05)

Status: authorized 2026-11-06 ("start wave 3") under the decided [PRD UX05 rule](../../docs/PRD.md#ux-review-decisions-2026-11-06); implemented 2026-11-06 with automated and emulator evidence ([evidence](../../docs/delivery/evidence/UX-WAVE3-2026-11-06.md)); phone check pending. Product decisions: option A of the [UX decision log](../ux-improvements/spec.md#decision-log) (a file the user chooses, import into an empty app only), the follow-up **JSON with a format version number**, and, decided 2026-11-06 at the start of this wave, **"empty" means no dues on screen**: hidden deleted dues do not block an import, and a file due with the same ID replaces the hidden one. Technical choices are recorded in [ADR 010](../../docs/adr/010-export-import-file.md). [Plan](plan.md), [tasks](tasks.md).

## Acceptance scenarios

| ID | Outcome |
|---|---|
| EX01 | Settings → "Export dues" opens Android's own save dialog with the suggested name `duewise-dues-YYYY-MM-DD.json` (Manila date). The saved file holds every stored due, including deleted ones, with all stored fields: no amount stays absent and zero stays 0. Success says how many dues (and how many deleted) were exported. Cancelling saves nothing and says so. With no stored dues, nothing is exported and the page says there is nothing to export. |
| EX02 | The file is UTF-8 JSON: `format` `"duewise.dues"`, `version` 1, `exportedAt`, and `dues`, one object per row with its fifteen stored columns in camelCase. Dates and timestamps are the stored strings, so a round trip is exact. |
| EX03 | Import reads and validates the whole file before writing anything. A file that is not JSON, has another `format`, lacks a field or has an unknown one, or a version below 1 is "not a Duewise export". A higher version asks the user to update the app. A file over 5 MB is refused. A due with a bad or repeated ID, blank title, negative amount, missing amount for Bill/Subscription, unknown type or repeat, wrong anchor day, or a malformed date or timestamp is refused with its position in the file. A file with no dues is refused. Nothing is written in any of these cases. |
| EX04 | Import runs only while the app shows no dues. Otherwise it is refused before the file picker opens, and again inside the write transaction. Hidden deleted dues stay; a file due whose ID matches one of them replaces it. |
| EX05 | Before writing, a confirmation shows how many dues the file holds, how many will show and when it was exported. Cancel writes nothing. |
| EX06 | All dues are written in one transaction; any failure writes none and reports that nothing was imported. Repeated taps while busy start nothing more. |
| EX07 | After an import the dues appear on Dues, Dashboard and Calendar. Reminder planning is refreshed; reminder settings and app preferences are not part of the file and stay as they are. Export → import into an empty app reproduces every row exactly. |
| EX08 | No storage permission, network or new dependency. The file goes only where the user saves it, and nothing is copied into app storage or logs. The page says the file is not encrypted. |

## Boundaries

- Dues only. Reminder settings and appearance preferences are left out: reminders need the user's explicit opt-in on each installation (F05), and both take seconds to set again.
- No merge, partial import, per-due choice, encryption, automatic or scheduled backup, cloud upload or share sheet. Android Auto Backup stays off.
- Import does not upgrade or downgrade files: version 1 only. A future format change needs a new version number and an ADR update.
- Release, signing and pushing to `main` remain held.
