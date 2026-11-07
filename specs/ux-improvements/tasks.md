# UX improvements task tracking

See the [spec](spec.md) for outcomes and the [decision log](spec.md#decision-log), and the [plan](plan.md) for order and approach. Status 2026-11-06: all sixteen items decided; Wave 1 (UX01, UX02) implemented with automated and emulator evidence, phone pending; the rest not implemented.

## Planning

- [x] Record the review findings with evidence and limits ([UX review](../../docs/delivery/UX-REVIEW-2026-11-06.md)).
- [x] Rank findings by impact and write outcomes, options and the plan (2026-11-06).
- [x] Link the review and plan from the docs index, STATUS, PRD, DESIGN and the roadmap.

## HTML decision lab — 2026-11-06

- [x] Add the **UX decisions** tab to the design exploration with drawings of the native app as built next to every option ([DL01–DL08](spec.md#html-decision-lab)).
- [x] Node data tests: `node --test tests/decision-lab.test.cjs` → 3 passed (DL01–DL03); the other prototype unit tests still pass (model 5, essential-model 3, ph-catalog 6). Agent-observed, Node 22.12.0.
- [x] Browser checks: `node tests/browser-check.cjs` (full suite) passed, including the new decisions flow (DL04–DL08) and the existing PH catalog, interaction-lab and essential-first flows; no runtime errors. Agent-observed in headless Edge, Windows 11.
- [x] Visual review of the generated screenshots (ignored `.qa/decision-UX01…UX16.png`, `.qa/decision-mobile-UX02.png`): two-phone cards were clipped and the page intro pushed decisions down; fixed by spanning two-phone cards, a full-width "Today" strip, safe centring and hiding the intro in this view. The list becomes a swipeable strip on narrow screens.
- [x] Source at the time of these checks (SHA-256 prefixes): `decision-lab.js` 247FD289AB34A51E, `decision-lab.css` B4CDA5DB80BFC1FA, `index.html` 9245D0B3A031CEF5, `app.js` F5F0AB2E14FD8099.
- [x] The user reviewed the options in the lab and pasted the export (2026-11-06); recorded in the [decision log](spec.md#decision-log).

Limits: drawings are illustrations in light theme only, not captures of the running app; the lab is browser-only evidence and changes no production behavior.

## Decisions recorded — 2026-11-06

- [x] Decision log with the two non-recommended choices called out (UX06 B Undo, UX15 B shrink cells).
- [x] PRD [UX review decisions](../../docs/PRD.md#ux-review-decisions-2026-11-06) (UX01–UX07, UX12) and the deferred-capabilities note updated; replaced rules stay in force until each item ships.
- [x] DESIGN [UX review decisions](../../docs/DESIGN.md#ux-review-decisions-2026-11-06) (UX01, UX02, UX06, UX08, UX10, UX11, UX13–UX16), including the UX15 target-size exception.
- [x] Plan updated for the choices: UX06 effort M → L (guarded restore for Undo), UX09 no work, UX15 shrink approach.

## Wave 1 — daily friction

- [x] UX01 decided: A, Mark as paid on Details.
- [x] UX01 failing tests (6), implementation (`DueDetailViewModel.TogglePaidCommand`, Details page), suites, builds, emulator pass; PRD F01/workflow, SDD, F01 spec and DESIGN item detail (as built) updated.
- [x] UX02 decided: A, collapse earlier paid dues.
- [x] UX02 failing tests (7), implementation (`DuesListViewModel` fold, Dues page row and all-folded line), suites, builds, emulator pass; PRD R03, SDD list contract and DESIGN Dues list (as built) updated.
- [x] Wave 1 evidence ([UX-WAVE1-2026-11-06](../../docs/delivery/evidence/UX-WAVE1-2026-11-06.md)) and STATUS update.
- [ ] Wave 1 phone check (needs the phone's app restored from the laptop or the laptop's debug key here).

## Wave 2 — never miss a payment

- [x] UX03 decided: A, the next occurrence appears on Mark as paid; Never/Weekly/Monthly/Yearly; month-end falls back to the last day.
- [x] UX03 follow-up decided 2026-11-06: only Bill and Subscription repeat.
- [x] UX03 dedicated [`specs/f07-recurrence/`](../f07-recurrence/spec.md) spec, plan, tasks and [ADR 009](../../docs/adr/009-recurring-dues.md); SDD schema 5 and recurrence contract.
- [x] UX03 implementation and verification per its [own tasks](../f07-recurrence/tasks.md) (2026-11-06, emulator; phone pending).
- [x] UX04 decided: A, maker-specific steps.
- [x] UX04 tests (9, written before the code but not run red first), implementation (`BackgroundDeliveryStatus` maker fields, Reminders view model and page), suites, builds, emulator pass (no steps on the Google emulator, as expected).
- [x] Wave 2 evidence ([UX-WAVE2-2026-11-06](../../docs/delivery/evidence/UX-WAVE2-2026-11-06.md)) and STATUS update.
- [ ] Wave 2 phone check, including the Xiaomi steps against the phone's own settings labels.

## Wave 3 — protect the data

- [x] UX05 decided: A, export to a user-chosen file, import into an empty app.
- [x] UX05 follow-up decided 2026-11-06: JSON with a format version number.
- [x] UX05 dedicated [export/import spec](../f04-export-import/spec.md), [ADR 010](../../docs/adr/010-export-import-file.md), SDD contract (2026-11-06). Follow-up decided at the start of the wave: "empty" means no dues on screen; a file due replaces a hidden deleted one.
- [x] UX05 export and import implemented and verified per their [own tasks](../f04-export-import/tasks.md) (2026-11-06, emulator; [evidence](../../docs/delivery/evidence/UX-WAVE3-2026-11-06.md)).
- [ ] Wave 3 phone check, including the phone's own file apps.

## Wave 4 — polish batch

- [x] UX06 decided: B, confirmation messages with a 5-second Undo after Delete.
- [x] UX06 [spec](../ux06-confirmations-undo/spec.md), guarded restore contract (SDD, V1.3 spec note), message host, tests, emulator pass (2026-11-06).
- [x] UX07 decided: A, `dd MMM yyyy` in the editor.
- [x] UX07 implementation with invariant month names; PRD date rule updated (2026-11-06).
- [x] UX08 decided: A, decimal-only keyboard.
- [x] UX08 implementation (input type without the signed flag, verified on the emulator); phone keyboard check still required.
- [x] UX09 decided: A, keep as is. No work.
- [x] UX10 decided: A, native buttons with the same look.
- [x] UX10 implementation (transparent native button over each counter card; emulator tree shows Buttons).
- [x] UX11 decided: A, Title → Type → Amount → Date.
- [x] UX11 implementation and Focus adoption spec note (Repeat placed after Due date).
- [x] UX12 decided: A, Manila time on Details.
- [x] UX12 implementation; PRD F01 and F01 spec updated.
- [x] Wave 4 evidence ([UX-WAVE4-2026-11-06](../../docs/delivery/evidence/UX-WAVE4-2026-11-06.md)) and STATUS update.
- [ ] Wave 4 phone and TalkBack check.

## Wave 5 — low impact

- [x] UX13 decided: A, cap caption size in the bar.
- [x] UX13 implementation (captions 12 sp up to 130%, shrink to fit the slot, 2026-11-07).
- [x] UX14 decided: A, quieter "No amount".
- [x] UX14 implementation (Details "No amount" in secondary body text).
- [x] UX15 decided: B, shrink cells to fit (DESIGN exception recorded).
- [x] UX15 implementation (cells about 40 dp at 320 dp, no sideways scroll).
- [x] UX16 decided: A, compact headings in landscape.
- [x] UX16 implementation (root headings 20 in landscape).
- [x] Wave 5 evidence ([UX-WAVE5-2026-11-07](../../docs/delivery/evidence/UX-WAVE5-2026-11-07.md)) and STATUS update.

## Remaining acceptance

- [ ] Phone verification of every implemented item (needs the phone's app restored from the laptop or that laptop's debug key on this computer).
- [ ] TalkBack review of changed controls when the user resumes accessibility checks.

Release/signing and pushing to `main` remain held.
