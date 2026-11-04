# Duewise exploration system and handoff

This document owns the tokens/components of the separate [four-concept HTML exploration](index.html). It is a non-normative design proposal. [DESIGN.md](../../docs/DESIGN.md) remains authoritative for the existing application. The [audit](AUDIT.md) records findings, rationale, reference research and scope conflicts.

## Essential-first Add/Edit

Alternative A is explicitly approved for this HTML artifact. The default editor now begins with **What is due? / Amount (PHP) / Due date**, with required/optional labels and an independent compact **Type: Bill / Change** disclosure near Amount. Category remains optional through Other; its existing picker/custom records are retained. Repeat/Reminders are native collapsed summaries; biller/notes and custom offsets expand only when needed. Global Off or enabled offset/time status remains explicitly simulated. These controls do not generate recurring dues or schedule notifications.

Open sections belong to the current editor instance, independently of financial fields. Category rerenders and layout/theme changes retain their values and open states. New editors start at the top. Normal Add/Edit now asks before discarding changed drafts; clean Cancel remains immediate. Invalid hidden controls open their disclosure ancestors and receive focus, with associated errors. This changes only the HTML editor, not native Cancel rules. Existing explicit catalog Save still persists independently of due Cancel.

`essential-model.js` normalizes only well-formed PHP/grouped amounts before passing them to the unchanged exact-centavo parser; malformed grouping remains invalid. `essential-editor.css` uses existing semantic tokens. [IN11–IN15](../../specs/interaction-lab/spec.md#acceptance-scenarios) and [observed evidence](../../specs/interaction-lab/tasks.md#essential-first-alternative-a--2026-11-04) own this follow-up. Roughly 30-second entry is a human-test target, not an automated result.

## Selected HTML interaction

The user explicitly selected **Sticky bottom actions + Context-aware navigation** for the default [Focus mockup](index.html#a/dashboard). Add/Edit use a compact 48px outlined Cancel/primary Save row below the scrolling fields, with Delete in the Edit header. The centered Add/Focus navbar stays on Dashboard, Calendar, Dues and Settings; pushed tasks hide it until return. Shared semantic styling and viewport sizing apply outside the lab without its comparison chrome. Sample writes, validation and catalog rules remain; the later Essential-first approval extends dirty-editor protection to normal HTML. Busy simulation and other-page draft guards remain lab-only. [IN10 and observed evidence](../../specs/interaction-lab/tasks.md#selected-html-adoption--2026-11-04) track the earlier layout adoption, separately from production.

## Interaction comparison

Open the default [Focus prototype](index.html#a/dashboard), then **Compare actions & navigation**, or open [the interaction lab](index.html#interaction-lab) directly. This latest brief authorizes three action placements and three navbar behaviors; it retains Focus's visual style and does not reopen brand alternatives. [Audit and tradeoffs](INTERACTION-AUDIT.md), [scope](../../specs/interaction-lab/spec.md), [plan](../../specs/interaction-lab/plan.md) and [observed verification](../../specs/interaction-lab/tasks.md) describe this follow-up.

The comparison cards distinguish **Preview** from **Choose**. A actions/B navigation remain starting previews; stored reviewer preferences begin unchosen. Independent choices persist under `duewise.interaction-lab.v1`, export together as two separate decisions, and can be cleared. Browsing or storing another preference does not replace the explicitly selected A/B ordinary mockup. Compact selectors next to the phone and inside the modal allow switching without returning to the cards.

Actions A are adjacent outlined Cancel/Save; B stacks quiet Cancel above full-width Save; C keeps header Cancel and full-width bottom Save. Nav A retains tabs below pushed-page actions; B replaces them during focused tasks; C moves the one live form into a native modal above the parent, leaving tabs visible but inert behind the backdrop. All share `nav()`, the editor and sample Save logic. Native category/confirmation dialogs stack above the task. Dirty exits ask, Keep editing retains input, and an observable 450ms simulated Save interval prevents duplicate submissions. Disabled/loading/error/success previews are explicitly labeled.

`interaction-lab.css` reuses the existing semantic palette tokens. The action region stays outside the content scroll owner, with 48px actions and safe-area padding. VisualViewport sizing and a labeled keyboard-space reservation support review; they are not actual phone-keyboard acceptance. The lab preserves temporary sample-dues behavior and the PH catalog contracts below.

## Focus with Philippine categories

The preceding user brief selected **Focus navigation** and explicitly limited implementation to HTML. Open [Focus](index.html#a/dashboard); the default URL opens it too. The earlier four-concept/refinement-selection material below records historical exploration and is superseded for the ordinary flow. The [Philippine catalog spec](../../specs/ph-catalog-prototype/spec.md) owns its browser data and mutation rules; [tasks](../../specs/ph-catalog-prototype/tasks.md) preserve its initial verification.

- **Add or Edit → Category** opens a searchable bottom sheet with frequently used, recently selected, My Categories and collapsed predefined groups. All ten requested groups are represented. The visible **+ Create Custom Category** supports a name, optional icon/color, validation and persistent Save.
- **Provider / biller** is a separate optional field. Relevant suggestions can be selected or replaced with any manual name. These are text suggestions, not payment integrations. New manual billers are saved on due Save.
- **Settings → My Categories & Billers** provides persistent custom creation, rename and confirmed deletion. Category deletion moves referenced sample dues to Other; biller deletion keeps their biller name as text. No due is deleted. All labels and category filters resolve stable IDs.
- **Due type (amount policy)** preserves Bill/Task/Subscription/Other separately from the detailed category. It retains required-versus-optional amount handling, explicit zero and manually entered variable charges.
- **Schedule controls** support weekly/monthly/quarterly/annual/custom interval previews and multiple per-due offsets, while global time/quiet hours remain available. They create no recurring series and schedule no OS notification.

Use PHP, `en-PH` and Asia/Manila context throughout. Custom catalogs/recents persist under `duewise.ph-catalog.v1`; demo dues/preferences/reminders remain temporary. Catalog read/write failures are explicit and do not clear stored values. Reset sample data keeps the custom catalog. The original `mobile-review` files and Android app are unchanged.

## Opening and choosing

Open `prototypes/design-exploration/index.html` directly in a modern browser. No build, package install or network connection is needed. Keep the sibling `mobile-review` directory: the new artifact reuses its pure money/date/status helpers and licensed font files without editing them.

The shared typefaces retain their [SIL Open Font License](../mobile-review/assets/PlusJakartaSans-OFL.txt).

1. Start in **Compare all**. At desktop widths all four concepts are side by side; smaller screens reflow them into two or one column. Compare Dashboard, Calendar, Dues or Settings in signature, all-light or all-dark appearance. These overview cards are passive previews; choose **Explore A/B/C/D** to interact.
2. In **Explore one**, switch concepts and appearance while keeping the current sample data and view/draft state. Use the four bottom destinations or the screen selector. Plus adds a due, a row opens Details, the pencil edits and compact Save stays below the scroll area. Recurrence, quick status and weekly-calendar proposals are labeled as concepts.
3. **Choose this direction** opens a confirmation. Only that confirmation saves a preferred concept. Browsing does not. You can keep exploring, choose another, or clear the preference.
4. Add refinement notes separately for each concept. **Export my design brief** downloads the preference and all notes as Markdown. Share the resulting file or tell the collaborator the concept letter and refinements when ready for the next iteration. Export is not a financial-data export or production authorization.

No direction is endorsed or preselected. A starts the explorer alphabetically when no URL specifies another concept. Signature B starts dark, while A/C/D start light; both appearances exist for every concept. A link such as `index.html#b/calendar` opens a particular concept and root screen. The previous prototype remains reachable through the **Original prototype** link.

### Minimal Fintech refinement

On 2026-11-04 the user chose **A / Minimal Fintech** for refinement and authorized the centered floating Add treatment after reviewing the recommendation. Open [the refined dashboard](index.html#a/dashboard) directly. The personal browser comparison preference remains independently editable; B/C/D are preserved for reference.

A's bottom order is **Dashboard / Calendar / Add / Dues / Settings**. Add is an action, not a fifth destination: its lime circle is 56 pixels with a dark plus, visible Add caption, accessible name and subtle shadow, raised approximately 16 pixels above the surface. The bar reserves a center slot and extra content clearance; the last row stays clear of the circle. Header Add is removed for A, leaving Reminders. The floating action appears on the four roots, opens the same form and returns to its originating root after Save or Cancel. Details, editor and Reminders hide the bar; Save retains its separate compact footer. The theme palette, active destination cues and bottom safe-area inset are retained. Other concepts keep their header Add placement.

## Shared tokens

### Live recommendations within Minimal Fintech

The user requested further design recommendations **through the HTML**. In [A's explorer](index.html#a/dashboard), the “Three ways to make it feel right” panel offers live visual previews:

| Refinement | Recommendation / tradeoff |
|---|---|
| Essential | Retains the existing open amount summary, ledger and full-width bottom bar. Useful for the calmest presentation. It remains the initial preview for an unset refinement preference. |
| Focus — recommended | Compact amount/next-date summary, concise status chips, chronological overdue/today rows first, later dues below, and an inset floating dock. Recommended for finding the next action with less vertical scanning. |
| Soft cards | A tinted statement-style summary, outlined row cards and softer bar corners. Clearer grouping, with fewer visible rows at once. |

Every refinement keeps the lime centered Add, four destination tabs, global counts, integer money/date rules and the same fixture/drafts. Focus's priority grouping changes the Dashboard presentation only; Dues retains chronological order. No new lifecycle, bank balance or payment-history metric is introduced. Its total retains the same Today-through-month-end scope and explicit exclusion of missing amounts.

Click **Preview** to update the live phone. At mobile widths, swipe the recommendation cards horizontally; selecting one keeps its card in view and restores keyboard focus to its preview button. The selected refinement is a browser-local reviewer preference, validated when loaded and included in the Markdown design brief. It survives reload; demo financial changes do not. B/C/D remain available. The recommendation is a design judgment, not a measured user-study result or production adoption.

## Token reference

The executable source is [design-system.js](design-system.js). All concept colors use semantic names and are applied as `--c-*` custom properties. Shared component markup and logic are in [app.js](app.js); CSS varies presentation through `data-concept` and the active palette, without four copies of financial logic.

| Role | Shared value / contract |
|---|---|
| Body | Locally bundled Plus Jakarta Sans, 400/500/600/700; system sans fallback. |
| Main heading | 26 px reference, weight 700. |
| Section heading | 20 px reference, with compact 16 px subsection roles. |
| Body / small / caption | 16 / 14 / 12 px reference roles; overview cards use compact passive-preview type. |
| Amount | Concept-dependent 32–42 px reference; tabular numbers and wrapping. |
| Grid | Four-unit spacing rhythm; 4/8/12/16/20/24/32 values; mobile horizontal content inset generally 20 px and 14 px in compact frames. |
| Touch | 44 px minimum live app actions, 48 px fields/calendar dates, labeled native controls. |
| Radius | A 14, B 20, C 26, D 14 px default card radius, with appropriate component exceptions. |
| Elevation | Light studio framing, quiet card borders, one phone-frame shadow. B adds a subtle hero glow/inset highlight. No essential information is behind transparency. |
| Icons | Shared inline 24-unit outline glyphs with 1.8-unit stroke. Decorative SVGs are hidden from assistive technology; icon-only actions have names/tooltips. Placeholder icon set, not a production asset package. |
| Motion | 160 ms standard easing `cubic-bezier(.2,0,0,1)` for control state feedback; reduced-motion removes transitions. No looping background motion. |
| Focus / feedback | Visible 3 px focus outline; native dialog focus containment; inline validation and focus to first invalid field; polite toast/status messages. |

Browser CSS pixels are reference design units, not measured native dp/sp behavior. All interactive pages use a single scroll owner plus independent header/tab/action regions. App fields and actions grow with text; the small calendar labels also have full date/count accessible names. A bounded inner calendar scroller preserves date targets when seven columns no longer fit. The browser larger-text check cannot establish native TalkBack, OS keyboard or system-text acceptance.

## Palettes and structural identity

| Concept | Light background / primary / accent | Dark background / primary / accent | Structural difference |
|---|---|---|---|
| A Minimal | `#F8F9F5` / `#253C20` / `#D9F27E` | `#111710` / `#D9F27E` / `#D9F27E` | Open hero, large amount, separated counters, unboxed ledger. |
| B Premium | `#F4F2F9` / `#5B36A6` / `#CCB5FF` | `#121116` / `#CBB6FA` / `#CBB6FA` | Glowing dark hero, outlined layers, raised row cards. |
| C Friendly | `#FBF8F3` / `#1D6653` / `#FFD3B8` | `#13211E` / `#A8E4CA` / `#F4C5A5` | Peach hero, rounded groups, task guidance and paid progress before the list. |
| D Data | `#F3F6FA` / `#244E94` / `#B8E6DE` | `#101B2A` / `#B2CDF9` / `#AAE0D5` | More compact hero, unpaid due-date week bars, chronological timeline and category counts. |

Every palette also defines background/surface/raised/muted, primary/onPrimary, accent/onAccent, text/secondary, border, hero/heroText, and danger/warning/success pairs. Tests calculate normal-text contrast against intended flat surfaces for all four concepts in both modes. Decorative muted borders are not the sole indication of inputs; live input outlines use the secondary foreground. The Premium hero's decorative glow and the page screenshots still require visual inspection; token checks alone are not a complete contrast/accessibility audit.

The studio uses editorial Georgia for its italic headline and C uses it for one friendly hero line. This is a proposed accent treatment; all body, fields, numbers and controls retain Jakarta. The exploration's lowercase `duewise` lockup is a visual proposal only; it does not replace the adopted Duewise wordmark.

## Shared components and interactions

| Component | Behavior shared by all four concepts |
|---|---|
| Hero / status summary | Same records and calculations; a next-due row opens that exact record. Status cards open the matching filtered list. |
| Due row | Literal title/category/amount/date/status; full accessible description; row opens Details. Paid rows remain date-sorted. |
| Search / filter | Title substring + one status + category use AND, retaining state across concepts/pages. A separate repeat-preference view is available from Dashboard. |
| Calendar | Six-row month or selected-date week, Monday/Sunday from saved sample preferences; count and agenda membership use exact dates, including Paid. Today is fixed to 04 Nov 2026, Asia/Manila. |
| Details | Explicit amount or “No amount”, category, date, notes, repeat preference and UTC timestamps. Pencil opens the existing-style draft form. Reschedule focuses that form's date. Quick paid/unpaid is a separately confirmed prototype shortcut, never a transfer. |
| Editor / footer | Required title/date; amount required for Bill/Subscription and optional for Task/Other. Exact integer-centavo preview, zero distinct from absence, input retained on error. Category/repeat/payment changes commit only on Save. Header Cancel discards. Delete names the saved record and confirms; no Undo. |
| Repeat selector | None/week/month/year preference. Shows the next preview date and creates no new due. Monthly/yearly preview clamps the original day to the next period's last valid day; supported date boundaries stop safely. Full occurrence/series semantics remain undefined for production. |
| Settings | Appearance/week-start drafts commit together on Save. Reminders/Back retains the active draft; leaving Settings discards it. Studio theme toggles are independent inspection overrides for each concept. |
| Reminders | Off initially; due-date and optional day-before schedule plus global time/quiet hours. Save only keeps opt-in off; Save & enable confirms the simulation. Native permission, background state, alarms and delivery are not implemented. |
| States | Shared empty/no-match, loading, read error, stale snapshot and unavailable target; retry restores the fixture view. Read-state toolbar never deletes records. |
| Choice / notes / export | Browser-local design preference and per-concept notes only; explicit selection confirmation, clear/change and Markdown download. Failure to access local storage is reported with export still available. |

## Calculation and scope definitions

The shared base model remains [mobile-review/model.js](../mobile-review/model.js). [exploration-model.js](exploration-model.js) adds isolated sample aggregation/repeat helpers:

- **Upcoming · November**: sum known unpaid amounts whose due date is on/after sample Today and within November 2026. Today is included; overdue and paid are excluded. Count includes amount-less dues, while the caption explicitly excludes them from the sum. An explicit zero remains known zero.
- **Overdue**: active unpaid dates before Today, with separate count/known amount. **Paid progress**: all active saved records across dates. **Paid** means user-recorded status, not verified payment or a historical flow.
- **Week bars**: unpaid known amounts grouped by November due-date bands 1–7, 8–14, 15–21, 22–28, 29–30. They do not represent actual payment dates or spending history. Category bars are active-record counts, not money percentages.
- All money arithmetic uses `BigInt` centavos. Each entered amount retains the existing signed-64-bit per-record limit; aggregate display can exceed that limit without floating-point rounding or silent overflow. Long values wrap.
- Repeat preferences never generate rows, charge money or schedule notifications. Task completion remains the explicit existing **Paid** status in this study; a distinct Done lifecycle requires a later product decision.

These are transparent sample definitions for comparing layouts, not new PRD contracts. Main app routes, schema, production reminders, signing and release acceptance are unchanged. Palette variants, lowercase lockup, accent serif, four tabs, Dashboard-first entry, totals, weekly Calendar, repeat controls and quick-paid confirmations remain proposed deviations from current DESIGN/PRD and are identified in the [audit](AUDIT.md).

## Verification and implementation summary

New files live entirely under `prototypes/design-exploration/`; no framework or package dependency was added. The original prototype's model/fonts are reused by relative path and remain unchanged. The existing [prototype spec](../../specs/mobile-ui-prototype/spec.md#four-concept-exploration-extension--2026-11-04), [plan](../../specs/mobile-ui-prototype/plan.md) and [tasks](../../specs/mobile-ui-prototype/tasks.md) track this extension instead of duplicating the established feature.

Run the current selected-Focus checks from the repository root:

```text
node --test prototypes/design-exploration/tests/model.test.cjs prototypes/design-exploration/tests/ph-catalog.test.cjs prototypes/design-exploration/tests/essential-model.test.cjs
node prototypes/design-exploration/tests/browser-check.cjs
```

Use `node prototypes/design-exploration/tests/browser-check.cjs --essential` for the focused essential-editor suite while iterating; the full command runs PH catalog, interaction comparison and essential-editor flows together.

The second command requires installed Edge/Chromium, optionally selected through `EDGE_PATH`. It starts a loopback-only server and headless browser, records screenshots/downloads in ignored `.qa/`, then stops its owned processes. The Windows sandbox may require an approved external execution for browser rendering. Results and source hashes are recorded in feature tasks; a test definition is not a passing result.

Production adoption is a later step: review the user's exported choice and notes, refine that direction, reconcile outstanding scope decisions, then plan/apply MAUI design changes and native acceptance. The exploration deliberately does not pick or implement a winner.
