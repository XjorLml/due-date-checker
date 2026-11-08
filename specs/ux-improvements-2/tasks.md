# UX improvements round 2 task tracking

See the [spec](spec.md) for the options and the [plan](plan.md) for order and approach. Status 2026-11-07: findings recorded and planned, decision lab updated. UX19, UX22, UX24 and UX25 were decided and implemented through the prototype alignment; the other seven are open.

## Planning — 2026-11-07

- [x] Record the review findings with evidence and limits ([UX review 2026-11-07](../../docs/delivery/UX-REVIEW-2026-11-07.md)).
- [x] Write options with a recommendation, the rule each would touch, scores, waves and approaches ([spec](spec.md), [plan](plan.md)).
- [x] Link the review and this spec from the docs index, STATUS, PRD, DESIGN and the roadmap.

## HTML decision lab — 2026-11-07

- [x] Add round 2 (UX17–UX27) to the **UX decisions** tab with "Today" drawings of the app as built after Waves 1–5; keep round 1 as a decided record ([DL09–DL13](spec.md#html-decision-lab)). Round 1 drawings and its browser key are unchanged; the drawing helpers gained optional badge text, several hint lines and an app-bar action without changing round 1 output.
- [x] Node data tests: `node --test tests/decision-lab.test.cjs` → 7 passed (both rounds' ranking and scores against the plans, drawings, the round 1 decided marks, per-round export). The other prototype unit tests still pass (model 5, essential-model 3, ph-catalog 6). Agent-observed 2026-11-07, Node 22.12.0.
- [x] Browser checks: `node tests/browser-check.cjs` (full suite) passed, including the decisions flow for both rounds (every decision at 1440 px, round 2 also at 390 px, round switch, per-round storage and export) and the existing PH catalog, interaction-lab and essential-first flows; no runtime errors. Agent-observed 2026-11-07 in headless Edge, Windows 11.
- [x] Visual review of the generated screenshots (ignored `.qa/decision-UX17…UX27.png`, `.qa/decision-mobile-UX19.png`). Fixed during review: compact rows in UX20 wrapped their badge onto a third line and later truncated it; the badge now leads the date line at full width. The UX24 "Today" text was reworded, and the editor's Delete is drawn as plain toolbar text, as in the app.
- [x] Source at the time of these checks (SHA-256 prefixes): `decision-lab.js` B8181879B0E4A1E8, `decision-lab.css` B977CF9C8AD062A8, `index.html` 9A1FFEDF03314D9C, `tests/decision-lab.test.cjs` ECA7FEB22AE8F1AC, `tests/decision-browser-flows.cjs` 528F325983B7EB96.
- [ ] The user chooses in the lab and pastes the export; record it in the [decision log](spec.md#decision-log).

Limits: drawings are illustrations, not captures of the running app; the lab is browser-only evidence and changes no production behavior.

## Wave 6 — never miss a payment

- [ ] UX17 decided.
- [ ] UX18 decided.
- [x] UX19 decided 2026-11-07 and implemented the same day under the [prototype alignment](../prototype-alignment/spec.md) (see the [decision log](spec.md#decision-log)).
- [ ] Implementation, tests, emulator and phone checks, evidence and STATUS update.

## Wave 7 — more dues, plainer words

- [ ] UX20 decided.
- [ ] UX23 decided.
- [ ] UX26 decided.
- [ ] Implementation, tests, emulator and phone checks, evidence and STATUS update.

## Wave 8 — protect data and typing

- [ ] UX21 decided.
- [x] UX22 decided 2026-11-07 and implemented the same day under the [prototype alignment](../prototype-alignment/spec.md) (see the [decision log](spec.md#decision-log)).
- [x] UX25 decided 2026-11-07 and implemented the same day under the [prototype alignment](../prototype-alignment/spec.md) (see the [decision log](spec.md#decision-log)).
- [ ] Implementation, tests, emulator and phone checks, evidence and STATUS update.

## Wave 9 — small

- [x] UX24 decided 2026-11-07 and implemented the same day under the [prototype alignment](../prototype-alignment/spec.md) (see the [decision log](spec.md#decision-log)).
- [ ] UX27 decided.
- [ ] Implementation (if any), tests, emulator and phone checks, evidence and STATUS update.

Release/signing changes and pushing to `main` remain held.
