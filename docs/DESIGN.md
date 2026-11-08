# Duewise — Product Design System

**Version:** 2.2.0 · **Status:** Normative · **Platforms:** Android (v1) · **Baseline:** 2026-11-05 (Focus adoption; supersedes 1.0.0 indigo baseline of 2026-10-16)

All dimensions are logical **dp** (Android) unless marked `px` (asset pixels), `sp` (scalable type), or `ms`. At 1× density, 1 dp corresponds to one reference pixel. Never hardcode physical pixels for layouts. Token paths in prose use `colors.light.text`; generated implementation constants may use camelCase.

## Changelog

| Version | Date | Change |
|---|---|---|
| 2.2.0 | 2026-11-07 | [Prototype alignment](#prototype-alignment-2026-11-07): every page takes the live prototype's look and layout (headers, rows, Dashboard sections, search on Dues, Calendar counts, Details, editor, Settings, Reminders); new prototype features stay deferred. |
| 2.1.1 | 2026-11-07 | Bottom bar completed to the selected Focus look. The 2.0.0 adoption had left out the floating rounded card, the lime pill behind the selected icon, Jakarta captions and the prototype's Dashboard and Dues glyphs; found when the user compared the build with the prototype. §6 and §7 bottom bar updated. No behavior change. |
| 2.1.0 | 2026-11-05 | Page content from the Focus mockup built on existing data ([focus-pages](../specs/focus-pages/spec.md)): ledger row, summary counters, filter chips, segmented control, icon/link buttons added to §7; as-built Dashboard, Calendar, Settings and Item detail sections in §8; Dashboard progress ring replaced by the Paid counter; Refresh/Retry only after an error. One product rule added: the PRD F02 upcoming total. |
| 2.0.0 | 2026-11-05 | Focus baseline adopted for look and navigation: palette, type scale, radius, spacing, five-slot bottom bar with centred Add action, sticky editor actions, Essential-first field order, Delete in the editor header; Settings becomes a root tab. No product-feature change. |
| 1.0.0 | 2026-10-16 | Original indigo/slate baseline. Superseded; retained in git history. |

## Scope and authority

This document owns UI/UX standards, brand, tokens, components, interaction, accessibility, and theme behavior. The [PRD](PRD.md) owns approved product scope and business behavior; the [SDD](SDD.md) owns the .NET MAUI implementation. Design specifications are not evidence that the current app implements or passes them. See [STATUS](delivery/STATUS.md) for delivery evidence.

The baseline below is the **Focus** system, the Minimal Fintech refinement from the HTML exploration, adopted on 2026-11-05 as the production visual and navigation system for the native Android app. The adoption changes look and navigation only; it adds no product features and replaces the 1.0.0 indigo baseline. The document still retains broader future concepts, and the previously listed ones remain outside approved scope: onboarding, categories, calendar/dashboard, timed/recurring items, reminders, notifications, widgets, export, sync, iOS, and settings concepts are authorized only where a scoped adaptation below (F01–F05, F07) says so. Their appearance here does not authorize implementation.

The subsequent 2026-10-17 request authorizes [dashboard/calendar mock previews](../specs/future-pages/spec.md#mock-acceptance-scenarios) only. They adapt §8 summary/filter and calendar/agenda layouts to native MAUI using read-only sample cards, a fixed sample date and Monday week start. Explicit preview labels and working navigation distinguish them from saved dues. Omit settings/FAB/mutation controls until those real actions ship; the mock does not approve future product rules or establish full accessibility acceptance.

The later [bottom-navigation request](../specs/future-pages/spec.md#bottom-navigation-acceptance) adopts a native Shell bar for the three implemented destinations: Dues, Dashboard and Calendar. Use 24-unit vector icons, visible native labels/selection semantics and semantic surface/primary/secondary colors. The bar has a 64-unit minimum, grows with system text to separate icons/captions, and retains Android's safe-area insets and native caption metrics. Three equal slots replace the future four-destination proposal below. Retain the sample headers, remove duplicate page switches/exits, and hide the bar in the editor. Verify light/dark, 200% text and Back/retained state at the device boundary; do not infer full accessibility acceptance from the native control. The 2026-11-05 Focus adoption supersedes that three-slot composition with the [five-slot bar with Add action](#bottom-bar-with-add-action) and makes Settings a root tab; the verification expectations still apply.

Product-facing exceptions for v1 are governed by the [PRD rules](PRD.md#business-rules-and-validation) and [workflows](PRD.md#user-workflows): PHP amounts required for Bill/Subscription and optional for Task/Other ([F07 optional amounts](#f07-optional-amount-adaptation), implemented 2026-11-06), no approved title-length cap, Asia/Manila calendar dates, date-sorted active dues including paid rows, payment changes on Save, Back/Cancel without a write, and confirmed soft deletion without Undo/restore. The broader optional-amount, 120-character title, completed-last, automatic-timezone/localization, immediate-toggle, unsaved-confirmation, and Undo descriptions below do not override those requirements.

Design adoption for the existing MAUI list/editor was authorized on 2026-10-17 and is tracked in the [adoption spec](../specs/design-adoption/spec.md), [implementation plan](../specs/design-adoption/plan.md), and [tasks](../specs/design-adoption/tasks.md). Implementation and observed verification remain distinct; see STATUS for actual results. The earlier [audit](delivery/DOCS-REFACTOR.md#unresolved-decisions) preserves the pre-adoption findings.

The [branding showcase image](Duely%20App%20Branding%20Showcase%20-%20chatgpt.png) is early AI-generated inspiration, not a specification. Its "Duely" name and other name candidates, accent palette, typography, and screens do not override this document: where they differ, the brand and tokens here apply. Its onboarding and reminder screens are outside v1 like the concepts above. Duewise is the chosen name (user decision, 2026-10-17).

Prototype review, 2026-10-18: the user-supplied Duewise overview is compared with code in the [prototype comparison](delivery/PROTOTYPE-REVIEW-2026-10-18.md). It introduces an explicit Statistics/Insights planning gap (F12), but its detailed screen/metric design is still to be specified. The attachment's Inter/SF Pro, bright blue palette, Sunday-first calendar and Home/Calendar/Add/Stats/Settings composition do not replace the adopted fonts/tokens, Monday-first samples or the native tabs current at that review (three then; the five-slot Focus bar since 2026-11-05). Resolve these differences per approved slice. Shared empty/error illustrations may be adapted later; an offline screen must not block working local CRUD. No account/profile feature is implied by its greeting/avatar. The [roadmap](../specs/future-pages/plan.md#priority-order-after-prototype-review) owns priority rather than the image's numbered order.

### Design explorations (non-normative)

HTML explorations under `prototypes/` are sample-data review artifacts. Browsing, selecting or exporting a direction in them does not change this document, approved product scope or native acceptance. Each exploration's spec owns its scope; its tasks file owns agent-observed browser evidence. The production system is the Focus baseline below, adopted natively on 2026-11-05; the rows record each exploration's state and what of it was adopted. "Focus" names both the selected refinement of Minimal Fintech and, after that selection, the whole selected HTML mockup.

| Date | Artifact | Adds | Status | Scope and evidence |
|---|---|---|---|---|
| 2026-11-04 | [Mobile HTML design study](../prototypes/mobile-review/index.html), [review](../prototypes/mobile-review/REVIEW.md) | Page-by-page layout alternatives, compact actions, separately labeled future onboarding | Proposal | [HP01–HP09](../specs/mobile-ui-prototype/spec.md#observable-acceptance-scenarios), [tasks](../specs/mobile-ui-prototype/tasks.md) |
| 2026-11-04 | [Four-concept exploration](../prototypes/design-exploration/index.html), [audit](../prototypes/design-exploration/AUDIT.md), [system guide](../prototypes/design-exploration/SYSTEM.md) | Minimal Fintech, Premium Dark, Friendly and Intuitive and Data-Driven directions; four-tab Dashboard-first navigation; sample totals, weekly Calendar, repeat and quick-paid proposals | Adopted 2026-11-05 as the production baseline for look and navigation (see [focus adoption spec](../specs/focus-adoption/spec.md)); catalog, repeats, weekly Calendar, totals and multi-offset reminders remain proposals | [HP10–HP18](../specs/mobile-ui-prototype/spec.md#four-concept-exploration-extension--2026-11-04), [tasks](../specs/mobile-ui-prototype/tasks.md#four-concept-exploration--2026-11-04) |
| 2026-11-04 | [Focus with Philippine catalog](../prototypes/design-exploration/index.html#a/dashboard) | Browser-local category/provider taxonomy with custom categories, optional providers and schedule controls | HTML only by explicit clarification; native F07 category and F05 reminder contracts unchanged | [PH01–PH09](../specs/ph-catalog-prototype/spec.md#observable-acceptance), [tasks](../specs/ph-catalog-prototype/tasks.md) |
| 2026-11-04 | [Interaction lab](../prototypes/design-exploration/index.html#interaction-lab), [audit](../prototypes/design-exploration/INTERACTION-AUDIT.md) | Three Cancel/Save placements and three navbar behaviors with dirty-draft, busy-state and native-dialog simulation | Comparison. The user selected **Sticky bottom actions + Context-aware navigation** for the HTML mockup; previewing or storing a preference adopts nothing; sticky actions and context-aware navigation adopted natively 2026-11-05 | [IN01–IN10](../specs/interaction-lab/spec.md#acceptance-scenarios), [selection evidence](../specs/interaction-lab/tasks.md#selected-html-adoption--2026-11-04) |
| 2026-11-04 | Essential-first Add/Edit, in the same mockup | Title, amount and date first; collapsed Repeat/Reminders; dirty-exit confirmation; PHP paste normalization | Approved Alternative A; field order adopted natively 2026-11-05; dirty-exit confirmation and PHP paste normalization remain HTML-only. The approximately 30-second entry target is unmeasured | [IN11–IN15](../specs/interaction-lab/spec.md#acceptance-scenarios), [evidence](../specs/interaction-lab/tasks.md#essential-first-alternative-a--2026-11-04) |
| 2026-11-06 | [UX decision lab](../prototypes/design-exploration/index.html#ux-decisions), in the same mockup ("UX decisions" tab) | Drawings of the native app as built next to each option for UX01–UX16, browser-local choices and notes, copy/export for the chat | Decision aid; choosing there records a browser preference only. Round 1 was decided from it on 2026-11-06 (see the [decision log](../specs/ux-improvements/spec.md#decision-log)). Drawings use the native light tokens, not the Focus HTML concept | [DL01–DL08](../specs/ux-improvements/spec.md#html-decision-lab), [evidence](../specs/ux-improvements/tasks.md#html-decision-lab--2026-11-06) |
| 2026-11-07 | Round 2 of the same [UX decision lab](../prototypes/design-exploration/index.html#ux-decisions) | UX17–UX27 drawn against the app as built after Waves 1–5, in light theme except the UX25 theme options; round 1 kept as a decided record | Decision aid; no option chosen | [DL09–DL13](../specs/ux-improvements-2/spec.md#html-decision-lab), [evidence](../specs/ux-improvements-2/tasks.md#html-decision-lab--2026-11-07) |
| 2026-11-07 | [Notification lab](../prototypes/design-exploration/index.html#notifications), in the same mockup ("Notifications" tab) | Each sample due's reminder the day before and on the day, on the shade and lock screen, in three privacy modes; a Mark as paid action with Undo | Decision aid; nothing chosen. Today's F05 generic text stays in force | [NL01–NL10](../specs/notification-lab/spec.md#acceptance-lab), [tasks](../specs/notification-lab/tasks.md) |

**Deviations from this baseline.** The exploration's [scope definitions](../prototypes/design-exploration/SYSTEM.md#calculation-and-scope-definitions) and [audit](../prototypes/design-exploration/AUDIT.md#scope-conflicts-resolved-for-this-artifact) list what differed from the 1.0.0 baseline and the PRD. At the 2026-11-05 adoption the following remained proposals (the [2026-11-07 prototype alignment](#prototype-alignment-2026-11-07) later adopted the lowercase in-app lockup and the remaining page layouts, but none of the features below): Dashboard-first entry (Dues remains the start tab), the accent serif, amount totals and charts, the weekly Calendar, repeat controls, multiple reminder offsets, quick-paid confirmation and the Philippine catalog. Each later adoption needs its own spec, PRD review for behavior changes and SDD review for navigation or data changes; the exploration itself does not authorize implementation. The explicit 2026-11-08 [Philippine preset adaptation](#philippine-preset-category-adaptation) supersedes the catalog deferral for presets only. Custom categories and billers/providers remain deferred. Other later approvals are owned by the PRD and their linked feature specs.

## Table of Contents

- [Changelog](#changelog)
- [Scope and authority](#scope-and-authority)

1. [Overview](#1-overview)
2. [Brand Identity](#2-brand-identity)
3. [Color System](#3-color-system)
4. [Typography](#4-typography)
5. [Layout and Spacing](#5-layout-and-spacing)
6. [Iconography and Illustration](#6-iconography-and-illustration)
7. [Components](#7-components)
8. [Screen Specifications](#8-screen-specifications)
9. [Interaction and Motion](#9-interaction-and-motion)
10. [Notifications and Status Logic](#10-notifications-and-status-logic)
11. [Accessibility](#11-accessibility)
12. [Design Tokens](#12-design-tokens)
13. [Implementation Notes](#13-implementation-notes)
14. [Quality Checklist](#quality-checklist)

### Native MAUI adoption for v1

F02 adaptation, authorized 2026-10-18: [production Dashboard](../specs/f02-dashboard/spec.md) replaces its fixture preview. Use global paid progress/counts, labeled title search, All/Overdue/Due today/Upcoming/Paid filters with selected semantics, chronological saved-data cards and distinct empty/no-match/loading/stale-error states. [PRD rules](PRD.md#f02-dashboard-rules) supersede the future Home section's Completed/completed-last grouping for F02. Keep Dues initial; the later F03 adaptation below replaces Calendar samples. Add is a working existing-form action; rows open Details. Details' explicit return caption is Back to list for either parent. Use CollectionView as the only scroll owner so large-text headers/actions remain reachable. No settings/profile/category/reminder affordance ships in this slice.

F01 adaptation, authorized 2026-10-18: [saved due details](../specs/f01-item-details/spec.md) uses read-only semantic cards for existing fields, explicit Paid/Unpaid and absent-notes text, and Created/Last updated timestamps labeled UTC (Asia/Manila labelled "Manila" since UX12, 2026-11-06). Retain the Asia/Manila DateOnly due date and PHP amount. Edit opens the existing editor; Back to dues returns to the list. Hide the bottom bar in Details as in the editor. Refresh on return/resume, with loading, retryable error and unavailable states. The future Item detail section's category/recurrence/reminder/Complete/Reopen/Undo actions remain outside F01. This scoped exception supersedes the future layout only for this slice; it does not approve those capabilities.

The existing list/editor map semantic colors to `Resources/Styles/Colors.xaml`, and type/component roles to `Styles.xaml`. Four unmodified, licensed static Plus Jakarta Sans weights are bundled and registered locally. Duewise display/icon/splash branding retains the existing Android package/data identity.

V1 keeps native Android controls/dialogs and a single-column form/card layout at every text scale. Margins follow §5; wide views center bounded content rather than adding the future two-pane navigation. List headings/actions scroll with the CollectionView to remain reachable at large text sizes. Amounts have a dedicated wrapping line. Paid/Unpaid retain literal text with a textual status symbol; future lifecycle statuses and category icons are not introduced. Native pressed feedback uses a brief opacity state, with disabled opacity and focus/error outlines; no custom motion dependency is added. Platform input metrics provide vertical field padding; the minimum outlined field height is 48 logical units. Validation on blur shares Save's rules and does not write or normalize input.

These are scoped native adaptations, not a claim of full compliance. Test theme changes, 200% text, TalkBack/keyboard focus, wrapping and native picker/action behavior using [DA01–DA10](../specs/design-adoption/spec.md#acceptance-scenarios). Remaining gaps and observed devices belong in delivery evidence. Future screens and the authorized mock milestone are mapped in the [roadmap](../specs/future-pages/plan.md).

## 1. Overview

The product vision/personas here include future capabilities. Approved v1 users and scope are defined in the PRD.

Duewise tracks bills, tasks, subscriptions, and deadlines with explicit due dates, recurring schedules, notifications, calendar visibility, and completion history. Default data entry is local-first; notification scheduling must function without network connectivity.

| Persona | Need | Default workflow |
|---|---|---|
| Busy student | Track assignments and exam dates | Add task → select date → choose reminder → complete |
| Household organizer | Track rent, utilities and renewals | Add bill/subscription → recurrence → reminders → review calendar |

**Principles:** (1) Show the next actionable date first. (2) One primary action per screen. (3) Status is readable without color. (4) Errors are recoverable without data loss. (5) Decoration never obscures information.

The [2026-11-06 UI/UX review](delivery/UX-REVIEW-2026-11-06.md) assessed the as-built app against these principles: (3) and (4) are met; (1) is met on Dashboard but not on the Dues start page as paid history grows (UX02). The user decided all sixteen items the same day; presentation decisions are below. A [second review on 2026-11-07](delivery/UX-REVIEW-2026-11-07.md), after all sixteen shipped, found principle (2) unmet on the Reminders page (two primary buttons, UX19) and recorded UX17–UX27 as options in [UX improvements, round 2](../specs/ux-improvements-2/spec.md). None is decided, and the as-built sections below stay in force until one is.

### UX review decisions (2026-11-06)

**Status: UX01 and UX02 shipped 2026-11-06 (now in [Item detail](#item-detail-as-built-2026-11-05) and [Dues list](#dues-list-as-built)); UX03 and UX04 shipped 2026-11-06 (see [Add / edit due item](#add--edit-due-item), Item detail and the [F05 reminder adaptation](#f05-reminder-adaptation)); UX05 shipped 2026-11-06 (see [Settings](#settings-as-built-2026-11-05)); UX06, UX07, UX08, UX10, UX11 and UX12 shipped 2026-11-06 in Wave 4 (see [Toasts/snackbars](#toastssnackbars), the dashboard, Item detail and Add / edit due item), UX09 needing no change; UX13–UX16 shipped 2026-11-07 in Wave 5 (bottom bar, Item detail, Calendar and root headings), so every UX decision has shipped.** Each entry replaces the matching as-built description in this document when its item ships under the [UX improvements plan](../specs/ux-improvements/plan.md); product rules for UX01–UX07 and UX12 are in the [PRD](PRD.md#ux-review-decisions-2026-11-06). Source: the [decision log](../specs/ux-improvements/spec.md#decision-log).

| ID | Presentation decision |
|---|---|
| UX01 | Item detail: "Mark as paid" / "Mark as unpaid" is the primary button; Edit due becomes secondary; Back to list stays a quiet action. |
| UX02 | Dues list: a muted full-width "Show N earlier paid dues" row with a chevron sits above the ledger rows; expanded, it reads "Hide earlier paid dues". |
| UX06 | Confirmation uses the [toast/snackbar](#toastssnackbars) component above the bottom bar (3,000 ms informational; 5,000 ms with an Undo button after Delete), announced through a polite live region. **Shipped 2026-11-06** as a Material snackbar; see the as-built note under [Toasts/snackbars](#toastssnackbars). |
| UX08 | Amount entry requests a decimal-number keyboard without the sign key. **Shipped 2026-11-06**; the stock emulator keyboard still draws its minus and comma keys but the field rejects them, phone keyboard unchecked. |
| UX10 | Summary counters become native buttons with the same appearance, so assistive technology announces the button role. **Shipped 2026-11-06** as a transparent native button over each unchanged counter card. |
| UX11 | Editor order becomes Title → Type → Amount (PHP) → Due date → Notes; the caption changes to match. This revises the Essential-first order adopted in the [Focus adoption](../specs/focus-adoption/spec.md). **Shipped 2026-11-06**; Repeat sits after Due date because its anchor follows the date. |
| UX13 | Bottom-bar captions stop scaling past a fixed maximum; icons and TalkBack names are unchanged. **Shipped 2026-11-07:** captions use the 12 sp caption size for active and inactive slots, follow the system text size up to 130% and shrink to no less than 85% when the longest caption would not fit its slot. The bar grows with the capped size only. |
| UX14 | Item detail shows "No amount" in `textSecondary` at body size instead of HeroAmount. **Shipped 2026-11-07.** |
| UX15 | **Exception to the 44 dp target:** when seven 44 dp columns plus their 2 dp gaps do not fit the content width (screens narrower than about 350 dp with the 14 dp compact margins), the day columns shrink to fit (about 40 dp each at 320 dp) instead of scrolling sideways. Cells stay above WCAG 2.2 AA's 24 px minimum; the 44 × 48 dp cell remains the rule wherever it fits. **Shipped 2026-11-07;** the month grid no longer scrolls sideways. |
| UX16 | Root pages use a compact heading when the window is wider than it is tall. **Shipped 2026-11-07:** the four root headings drop from 26 to 20 (the Heading2 size) while width exceeds height. |

UX03 (shipped) adds a Repeat picker to the editor, a "Repeats" fact row and a "Next one added for …" line on Item detail, and the repeat in row meta text. UX04 (shipped) adds the maker-specific steps inside the existing Reminders status card. UX05 (shipped) adds a "Your data" surface card to Settings with two secondary buttons and Android's own file dialogs. UX09 keeps both pages as they are.

| Voice | Do | Don’t |
|---|---|---|
| Calm | “Electric bill is due tomorrow.” | “PANIC! You’re running out of time!” |
| Direct | “Choose a due date.” | “Please proceed to configure the relevant temporal parameter.” |
| Supportive | “Marked complete.” | “Finally, you did it!” |
| Specific | “Couldn’t save. Try again.” | “Something went wrong.” |

### Prototype alignment (2026-11-07)

On 2026-11-07 the user asked to bring every page in line with the live HTML prototype (the default Focus view of the [design exploration](../prototypes/design-exploration/index.html#a/dashboard)). Scope, the six decisions behind it and acceptance scenarios PA01–PA10 are in the [prototype alignment spec](../specs/prototype-alignment/spec.md); product rule changes (search on Dues, the unsaved-changes prompt) are in the [PRD](PRD.md#prototype-alignment-decisions-2026-11-07). **Status: shipped 2026-11-07, emulator-observed ([evidence](delivery/evidence/PROTOTYPE-ALIGNMENT-2026-11-07.md)); phone check pending.** The older as-built sections below carry a note where this supersedes them.

| Area | Presentation decision |
|---|---|
| Root header | Brand mark plus lowercase "duewise" wordmark on the left, a bell icon button ("Reminders") on the right, on Dashboard, Calendar, Dues and Settings. Replaces the Dues-only Reminders chip and brand lockup. |
| Pushed header | Drawn by the page instead of Android's title bar: "‹ Back" (Details, Reminders), centred title, one trailing icon action (pencil on Details, bin on the editor in Edit mode). |
| Due row | Type icon in a tinted 40 dp circle, title, "dd MMM · Type", a "⟲ Every month" line for repeating dues; amount over the status shown as glyph and word in the status colour, without a fill. Details keeps the filled status pill. |
| Dashboard | Summary, next-date row with an arrow, three inline outlined counters, "Needs attention" and "A little further ahead" sections, global footnote; no search. |
| Dues | Search, status chips, Type filter, matching count and Reset filters above the list; UX02 fold while no filter is active. |
| Calendar | Count as "1 due" / "2 dues" under the day number (settles round 2 UX24); "Monday first · includes paid" with a Today link under the grid; agenda heading and footnote. |
| Details | Icon, title, amount, status pill, facts list (Due date, Due type, Repeats, Currency / timezone), primary "✓ Mark as paid", Notes card, Manila timestamps; edit from the header pencil. |
| Editor | "Required fields are marked."; required/optional labels; compact "Type: Bill · Change" row (still before Amount, UX11); "Will save as ₱…" preview; date hint with "Manila. Past dates are allowed."; collapsible Repeat and Notes rows; "Paid · applies on Save" check row; footer Cancel / ✓ Save; "Discard changes?" prompt (UX22). |
| Settings | Preference cards, Reminders row with bell, Your data card, privacy note, draft line, "✓ Save" fixed bottom right above the bar (UX25 keeps Save). |
| Reminders | "A gentle nudge." page: status card, "Your schedule" and "Quiet hours" cards, a collapsed "Notification readiness & privacy" section for Android status and tools, state-dependent footer (settles round 2 UX19). |

## 2. Brand Identity

**Wordmark:** `Duewise` in Plus Jakarta Sans Bold 700, title case; use `colors.light.text` on light and `colors.dark.text` on dark. Since 2026-11-07 the in-app root header uses the prototype's lowercase `duewise` beside the brand mark ([prototype alignment](#prototype-alignment-2026-11-07)); the launcher label, notifications and store name stay `Duewise`. Icon is a `colors.brand.ink` rounded square carrying a white calendar-check glyph and one small `colors.accent.coral` dot (`Resources/Images/brand_mark.svg`); maximum three principal shapes. No words or numbers inside the app icon.

| Rule | Specification |
|---|---|
| Master export | 1024×1024 px PNG, sRGB, opaque background |
| Symbol bounding box | 716×716 px centered, 154 px inset |
| Clear space | ≥128 px on all sides at 1024 px export; for wordmark lockup ≥0.5× symbol height |
| Minimum app icon | 40×40 px raster; below 40 px use flat silhouette without fine highlights |
| Wordmark minimum | 96 px wide in digital layouts |
| Corner treatment | Full-bleed square master; OS applies launcher masking |
| Forbidden | Stretching, rotating, adding drop-shadow outside canvas, outlining, recoloring the ink or lime, placing text, relying on coral alone for status |

**Why:** One recognizable silhouette stays identifiable in launcher masks, notification contexts, and small widgets.

The launcher keeps the same brand-ink background in both app appearances. Its full-bleed `Resources/AppIcon/appicon.svg` fill must match the project's `MauiIcon` background metadata; changing the in-app Light/Dark preference does not select different launcher artwork. The 2026-11-08 correction brought the previously overlooked purple SVG fill into this existing palette ([verification](delivery/evidence/ICON-PALETTE-2026-11-08.md)).

## 3. Color System

The semantic foreground tokens are deliberately darker in light mode and lighter in dark mode than decorative accents. Use theme tokens, never raw accent colors for text. Light and dark are two tunings of one green-on-paper palette: in light mode `primary` is the deep ink and the lime `accent` is reserved for the Add disc; in dark mode the lime becomes `primary` and the ink becomes its `onPrimary`.

| Token | Light hex | Dark hex | Usage |
|---|---|---|---|
| `colors.{mode}.background` | #F8F9F5 | #111710 | App canvas |
| `colors.{mode}.surface` | #FFFFFF | #1C251A | Cards, forms, bottom bar and editor footer |
| `colors.{mode}.surfaceRaised` | #FFFFFF | #273122 | Modal and floating surfaces |
| `colors.{mode}.surfaceMuted` | #EEF1E8 | #2A3524 | Subtle sections, selected card fill, unselected chips |
| `colors.{mode}.text` | #18201E | #F4F5F3 | Primary copy |
| `colors.{mode}.textSecondary` | #53605B | #B8C3BE | Supporting copy; live field outlines |
| `colors.{mode}.border` | #C9D0CC | #64716B | Decorative dividers and card edges only; field outlines use `textSecondary` |
| `colors.{mode}.primary` | #253C20 | #D9F27E | Primary actions, selected controls, active bar item |
| `colors.{mode}.onPrimary` | #FFFFFF | #203019 | Text on solid primary fill |
| `colors.{mode}.accent` | #D9F27E | #D9F27E | Add action disc fill |
| `colors.{mode}.onAccent` | #203019 | #203019 | Plus glyph on the Add disc |
| `colors.{mode}.hero` | #EAF1D8 | #26341E | Summary/hero panel fill |
| `colors.{mode}.heroText` | #21331C | #E8F3D3 | Copy on `hero` |
| `colors.{mode}.overdue` | #AE2638 | #FFB1BB | Overdue text and symbols; destructive actions; error outlines |
| `colors.{mode}.dueSoon` | #87430B | #FFD29C | Due-soon text and symbols |
| `colors.{mode}.completed` | #166534 | #A1E9B4 | Completed/paid text and symbols |
| `colors.{mode}.info` | #253C20 | #D9F27E | Informational labels |
| `colors.{mode}.focus` | #253C20 | #D9F27E | Keyboard focus ring; focused field outline |
| `colors.{mode}.disabled` | #53605B | #B8C3BE | Disabled content (not informational text) |
| `colors.{mode}.paidSurface` | #EDF8EF | #17352A | Tinted fill behind paid rows and badges |

| Brand / accent token | Hex | Usage |
|---|---|---|
| `colors.brand.ink` | #253C20 | Brand mark body; equals light `primary` |
| `colors.brand.lime` | #D9F27E | Brand highlight; equals `accent` and dark `primary` |
| `colors.accent.coral` | #E85D5D | Brand mark dot only; never a status or text color |

There is no brand gradient in 2.0.0. Neutrals are the semantic surface and border tokens above; no separate neutral ramp is defined.

**Contrast audit (WCAG 2.x ratios calculated from the hex values above with the relative-luminance formula; normal text needs ≥4.5:1, large text and non-text boundaries ≥3:1):**

| Foreground / background | Ratio | Threshold | Result |
|---|---:|---:|---|
| `light.text` / `light.background` | 15.71:1 | 4.5 | Pass |
| `light.textSecondary` / `light.background` | 6.22:1 | 4.5 | Pass |
| `light.text` / `light.surface` | 16.61:1 | 4.5 | Pass |
| `light.primary` / `light.surface` | 12.03:1 | 4.5 | Pass |
| `light.onPrimary` / `light.primary` | 12.03:1 | 4.5 | Pass |
| `light.onAccent` / `light.accent` | 11.31:1 | 4.5 | Pass |
| `light.overdue` / `light.surface` | 6.70:1 | 4.5 | Pass |
| `light.dueSoon` / `light.surface` | 7.40:1 | 4.5 | Pass |
| `light.completed` / `light.surface` | 7.13:1 | 4.5 | Pass |
| `light.info` / `light.surface` | 12.03:1 | 4.5 | Pass |
| `light.heroText` / `light.hero` | 11.62:1 | 4.5 | Pass |
| `light.completed` / `light.paidSurface` | 6.54:1 | 4.5 | Pass |
| `light.textSecondary` / `light.surface` (field outline, non-text) | 6.58:1 | 3.0 | Pass |
| `light.border` / `light.surface` (decorative only) | 1.57:1 | none | Not a control boundary |
| `dark.text` / `dark.background` | 16.64:1 | 4.5 | Pass |
| `dark.textSecondary` / `dark.background` | 10.04:1 | 4.5 | Pass |
| `dark.text` / `dark.surface` | 14.45:1 | 4.5 | Pass |
| `dark.primary` / `dark.surface` | 12.76:1 | 4.5 | Pass |
| `dark.onPrimary` / `dark.primary` | 11.31:1 | 4.5 | Pass |
| `dark.onAccent` / `dark.accent` | 11.31:1 | 4.5 | Pass |
| `dark.overdue` / `dark.surface` | 9.22:1 | 4.5 | Pass |
| `dark.dueSoon` / `dark.surface` | 11.25:1 | 4.5 | Pass |
| `dark.completed` / `dark.surface` | 11.16:1 | 4.5 | Pass |
| `dark.info` / `dark.surface` | 12.76:1 | 4.5 | Pass |
| `dark.heroText` / `dark.hero` | 11.42:1 | 4.5 | Pass |
| `dark.completed` / `dark.paidSurface` | 9.39:1 | 4.5 | Pass |
| `dark.textSecondary` / `dark.surface` (field outline, non-text) | 8.72:1 | 3.0 | Pass |
| `dark.border` / `dark.surface` (decorative only) | 3.10:1 | none | Not a control boundary |

If any combination is marked Fail, it is prohibited for that use. `border` is below 3:1 on light surfaces by design, so it never carries meaning on its own: field edges use `textSecondary`, and focus/error states use 3 dp `focus`/`overdue` outlines. Non-text controls and focus indicators require ≥3:1 against adjacent colors; the Add disc's `onAccent` glyph is tested against `accent`, and its surface-coloured ring separates the lime disc from the bar rather than relying on the disc's own edge.

## 4. Typography

**Font:** Plus Jakarta Sans is the only family. Four unmodified, licensed static weights (400/500/600/700) are bundled and registered as `JakartaRegular`, `JakartaMedium`, `JakartaSemibold` and `JakartaBold`; fallback is the Android system sans-serif. Do not synthesize missing weights. Sizes are scaled `sp`; the reference values below are at 100% system text.

| Token | Size sp | Weight | Line height dp | Letter spacing | Use |
|---|---:|---:|---:|---:|---|
| `typography.display` | 40 | 700 | 48 (provisional) | -0.3 | Reserved for a future hero amount; not used in v1 |
| `typography.h1` | 26 | 700 | 32 | -0.3 | Page heading |
| `typography.h2` | 20 | 700 | 26 | -0.2 | Section heading |
| `typography.h3` | 16 | 600 | 22 | 0 | Card title, field label |
| `typography.bodyL` | 16 | 400 | 24 | 0 | Body; same metrics as bodyM, kept as a role name |
| `typography.bodyM` | 16 | 400 | 24 | 0 | Body and field text |
| `typography.bodyS` | 14 | 400 | 20 | 0 | Supporting copy, errors |
| `typography.caption` | 12 | 500 | 16 | 0 | Captions, bar labels |
| `typography.button` | 16 | 600 | 24 | 0 | Button labels |

**Scaling:** honor system text scale up to 200% minimum; allow vertical reflow and multi-line labels. At 130% and above, keep a single column, permit button text to wrap to two lines, and let button height grow to fit content (minimum 48). Never truncate due date, amount, error, or primary action. Use baseline alignment rather than fixed text containers. Use `sp` and do not disable font scaling; the bottom bar grows with its captions. Exception (UX13, 2026-11-07): bottom-bar captions stop growing at 130%, because each slot has a fixed width and its icon and TalkBack name still identify it.

## 5. Layout and Spacing

Scroll bars: none. Pages and lists scroll without a visible scroll bar or scroll indicator (user request, 2026-11-07; implicit ScrollView and CollectionView styles). Long pages rely on content cut off at the bottom edge to show there is more.

| Token | dp |
|---|---:|
| `space-1` | 4 |
| `space-2` | 8 |
| `space-3` | 12 |
| `space-4` | 16 |
| `space-5` | 20 |
| `space-6` | 24 |
| `space-7` | 32 |

**Base grid:** 4-unit rhythm; 1 dp borders are the only exception. Horizontal content inset is 20 at phone widths, 14 below 360, 32 from 600 and 40 from 840 (`Pages/PageLayout.cs`). Readable content is capped at 640 from 600 dp and 720 from 840 dp and centred in the viewport. Respect the system top safe inset and bottom gesture inset; never assume a fixed status-bar height. Lists keep bottom clearance so the last card is not covered by the Add disc.

| Window width (dp) | Layout |
|---|---|
| 0–359 phone compact | Single column; 14 side insets |
| 360–599 phone | Single column; 20 side insets |
| 600–839 large phone / small tablet | Single column; 32 side insets, 640 max content |
| ≥840 tablet | Single column, centred; 40 side insets, 720 max content (two-pane home/calendar remains a future concept) |

| Radius token | dp | Use |
|---|---:|---|
| `radius-md` | 14 | Cards, fields, buttons, dialogs, sheets |
| `radius-pill` | 999 | Chips and status badges |
| `radius-disc` | 28 | Add action disc (56 dp circle) |

**Touch targets:** 44 dp minimum for actions in the bottom bar; 48 dp minimum for buttons, fields and calendar dates.

**Shadows:** one elevation level. `shadow-1` is Android elevation 6 dp (system-rendered, `#000000` base) and is used only by the Add disc; cards and the bar rely on surface color and 1 dp borders. `shadow-0` is flat. Never depend on shadow alone for hierarchy.

## 6. Iconography and Illustration

Bar and action glyphs are app-owned 24-unit outline SVGs under `Resources/Images` (`nav_dashboard`, `nav_calendar`, `nav_add`, `nav_dues`, `nav_settings`), Lucide-style with 2 dp stroke and round caps/joins (the plus glyph uses 2.2 for optical weight on the lime disc). Since 2026-11-07, Dashboard (two tiles, a tall tile and a short bar) and Dues (a sheet with three lines) use the Focus prototype's shapes. They are tinted by the bar: `onAccent` on the selected tab's lime pill, `textSecondary` inactive, `onAccent` on the disc. Standard size 24, compact 20, large 32; a glyph sits within a 44×44 minimum hit box. Optical adjustment ≤1 dp. Mirror directional arrows in RTL; never mirror calendar numerals or checkmarks.

**Brand mark:** a `colors.brand.ink` rounded square carrying a white calendar-check glyph and one `colors.accent.coral` dot (`Resources/Images/brand_mark.svg`). Launcher, splash and the monochrome notification icon derive from it.

**3D hero recipe:** 30° three-quarter view, 45° elevation; key light top-left at azimuth −45°, elevation 55°, softbox area 60% object width; fill light from front-right at 25% key intensity; ambient occlusion radius 18 px at 1024 px render; clay roughness 0.32, clearcoat 0.18, transmission 0.12 on glass details; bevel 12 px at 1024 px; contact shadow y=20 px blur=44 px opacity=0.16. Ink/lime body, coral ≤8% image area, green only for completed states. Export WebP 2× and 3× with transparent background.

3D is permitted on onboarding, first-run empty states, and achievement moments only; all navigation, list rows, calendar cells, controls, and dense dashboard surfaces use flat vectors. Provide static illustration fallback and avoid 3D render in low-power animation loops.

## 7. Components

**Universal state contract:** every interactive component supports default, pressed, disabled, focused, error (when applicable), and loading (when asynchronous). Pressed: native pressed state with a brief opacity change over `motion.duration-standard`; disabled: 40% foreground opacity with no action; focused: 3 dp `colors.{mode}.focus` outline (fields thicken their own outline in place; buttons get an outer ring); error: 3 dp `overdue` outline plus semantic overdue foreground and a descriptive message; loading: preserve width and label accessible name, show 20 dp spinner, block duplicate actions. Error/loading are inapplicable to purely decorative status indicators. Touch targets ≥44×44 for bottom-bar actions; buttons and fields ≥48×48.

### Buttons

| Property | Specification |
|---|---|
| Anatomy | Primary, secondary, ghost, destructive; leading icon optional, label required |
| Dimensions | Height 48, min width 96, padding 16 horizontal / 10 vertical, icon 20, gap 8, radius-md; full width in forms, or equal columns in the sticky footer |
| Colors | Primary: `primary/onPrimary`; secondary: `surface` with 1 dp `border` and `text`; ghost: transparent with `primary`; destructive: `overdue` on `surface`, 1 dp overdue outline. Focused: 3 dp `focus` outline |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | On press release invoke once; destructive always confirms if data cannot be restored; spinner replaces leading icon and retains text |

### Text inputs

| Property | Specification |
|---|---|
| Anatomy | Persistent label, field, optional prefix/suffix, helper/error line |
| Dimensions | Min height 48, padding 14 horizontal with platform vertical metrics, radius-md; outline 1 dp `textSecondary`, focused 3 dp `focus`, error 3 dp `overdue`; label above 8 gap; error below 4 gap |
| Colors | `surface` fill; outline `textSecondary` (not `border`, which fails 3:1); focused `focus`; error `overdue`; placeholder `textSecondary` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Trim leading/trailing spaces on save except notes; validate on blur and submit; do not clear entered text on error |

### Date/time pickers

| Property | Specification |
|---|---|
| Anatomy | Label, value, calendar/clock trigger, native picker or accessible custom dialog |
| Dimensions | Trigger 48 high in the shared field outline, icon 20, 14 padding; calendar day target 48 min |
| Colors | Selected date `primary` background and `onPrimary` text; invalid date `overdue` text |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Store absolute instant plus IANA timezone for timed dues; date-only uses local calendar date; confirm selection explicitly |

### Due-item card

| Property | Specification |
|---|---|
| Anatomy | Category icon, title, due date, status badge, optional amount, overflow action |
| Dimensions | Min height 96, padding 16, radius-md, gap 8, card margin-bottom 8 |
| Colors | `surface` with 1 dp `border`; selected `surfaceMuted` fill with 2 dp `focus` stroke; paid rows may use `paidSurface`; overdue left border 4 `overdue`; completed title strikethrough |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Tap opens detail; overflow exposes edit, complete, delete; amount never inferred |

### Category chips

| Property | Specification |
|---|---|
| Anatomy | Leading 16 icon, category label |
| Dimensions | Height 36, horizontal padding 12, gap 6, radius-pill |
| Colors | Unselected `surfaceMuted/text`; selected `primary/onPrimary` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Single select in form; multi select only for filters; selected announced |

### Status badges

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): rows show the status as glyph and word in the status colour without the pill (`StatusLine`); Details keeps the pill.

| Property | Specification |
|---|---|
| Anatomy | Status icon 14, short label |
| Dimensions | Min height 28, padding 8 horizontal/4 vertical, radius-pill |
| Anatomy | One pill per due: glyph plus the status word (`StatusBadge` control); used on the shared ledger row (Dues, Dashboard, Calendar) and on Details |
| Dimensions | Padding 10 × 4, radius-pill, glyph gap 6, medium weight word |
| Colors | Paid `completed` on `paidSurface` with ✓; Overdue `overdue` on `overdueSurface` (#FFF0F0 / #3B202B) with !; Due today `dueSoon` on `dueSoonSurface` (#FFF6E9 / #362B21) with ●; Upcoming `text` on `surfaceMuted` with ○ |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Never use hue alone; the word is always present and the pill's accessible description is "Status: word"; the glyph is hidden from assistive technology |

### Ledger row (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): a type icon in a tinted 40 dp circle leads the row, the repeat has its own "⟲ Every month" line, and the status has no fill (PA05).

| Property | Specification |
|---|---|
| Anatomy | `DueRow` control: title and "dd MMM · Category" on the left; amount and status badge on the right; 1 dp `border` hairline below. No card box. |
| Dimensions | Min height 70, vertical padding 14, column gap 12, right column max width 168; title 14 semibold, meta caption 12, amount 14 semibold wrapping per character |
| Colors | Text on the page canvas; selected row fills `surfaceMuted` |
| States | Universal state contract applies. |
| Behavior | Whole row opens Details; the row's accessible description is the projection's `AccessibilityText`. Shared by Dues, Dashboard and Calendar through `IDueRow`; supersedes the due-item card above for those lists. |

### Summary counters (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): counters are outlined cards with the count and word on one line, and they open Dues filtered to that status (PA03).

| Property | Specification |
|---|---|
| Anatomy | Three `CounterCard` surfaces in one row: coloured count (bold 16) stacked above the caption word so neither clips at 200% text; Overdue `overdue`, Today `dueSoon`, Paid `completed` as "paid/total" |
| Dimensions | Min height 44, radius 11, padding 5 × 8, gap 7 |
| Behavior | Tapping a counter selects that Dashboard status filter; the accessible description states the count and the filter it shows. The Paid counter carries the "x of y saved dues paid" progress description, replacing the ring on Dashboard. |

### Filter chips (as built, 2026-11-05)

| Property | Specification |
|---|---|
| Anatomy | `FilterChip` button: 13 medium text; selected `FilterChipSelected` is `primary`/`onPrimary` |
| Dimensions | Min height 44, padding 11 × 8, radius 9, gap 6; laid out in a three-column grid |
| Behavior | Single select; accessible description states "selected" or "not selected". |

### Segmented control (as built, 2026-11-05)

| Property | Specification |
|---|---|
| Anatomy | `SegmentedControl`: equal-width `SegmentButton`s inside a `surfaceMuted` track with 1 dp `border`; the selected segment is raised on `surface` in `primary` semibold |
| Dimensions | Track radius 10, padding 3, gap 3; segment radius 7, min height 44, padding 8, text 13 |
| Behavior | Single choice bound two-way to an index; each segment is a Button with a "selected" / "not selected" description. Used for Appearance and Start of week. |

### Icon and link buttons (as built, 2026-11-05)

| Property | Specification |
|---|---|
| Anatomy | `IconButton`: 44 × 44 transparent, glyph 22 in `textSecondary`, radius 12. `LinkButton`: transparent `primary` text 14 semibold, min height 44, no horizontal padding |
| Behavior | Icon buttons carry an explicit accessible description (Previous month, Next month). Link buttons name their target (next due title, Clear filters). |

### Swipeable list item

| Property | Specification |
|---|---|
| Anatomy | Card row plus revealed action rail |
| Dimensions | Row min height 80; actions 72 wide each; threshold 96 drag |
| Colors | Complete action green; delete action overdue; text uses accessible contrast |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Reveal actions at 96 drag; do not immediately delete; undo snackbar 5,000 ms |

### Bottom bar with Add action

| Property | Specification |
|---|---|
| Anatomy | A floating card holding Dashboard, Calendar, Add, Dues, Settings; icon and caption per slot. The selected slot's icon sits on a lime pill. The third slot is a disabled placeholder captioned "Add" that reserves space beneath the Add action disc and serves as the disc's label |
| Dimensions | Card height 64, grows with font scale (up to the 130% caption cap, UX13) to keep icons and captions apart; radius 22; 10 dp margin left, right and below, plus the system navigation inset below it, so the inset never pads the inside of the card; 4 dp inner side padding; elevation 6 with the soft `shadow` colour; icon 24, caption `typography.caption`; five equal slots; slot actions ≥44; selected pill 36 × 32, radius 11 |
| Colors | Card `surface` with a 1 dp `border` all round, on the page `background`; selected: pill `accent`, icon `onAccent` (in dark theme `primary` is lime too, so the icon must not use it), caption `primary` in Plus Jakarta Sans Bold; unselected: icon and caption `textSecondary`, caption Medium; the Add caption `text` in SemiBold. Only the pill and the bold `primary` caption mark the selected tab |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Preserve navigation state per root; Dues is the start tab; the bar shows on the four roots and is hidden on pushed Details, editor and Reminders pages; the placeholder slot never navigates; badge counts remain a future concept |

### App bar

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): pushed pages draw the prototype header (‹ Back, centred title, one icon action) instead of Android's title bar; root pages show the wordmark header with a bell (PA01, PA02).

| Property | Specification |
|---|---|
| Anatomy | Pushed pages only (editor, Details, Reminders): Back, title, optional trailing action such as Delete. Root pages (Dues, Dashboard, Calendar, Settings) show no app bar; the content heading is the page title and Dues carries the brand lockup at the top of its content |
| Dimensions | Height 56 plus safe top inset; horizontal padding 20; actions 44 hitbox |
| Colors | Background `background`, title `text` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Back respects navigation stack; never hide unsaved edits without confirmation |

### Calendar grid

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): the count reads "1 due" / "2 dues" under the number (round 2 UX24), and Today moves under the grid (PA06).

| Property | Specification |
|---|---|
| Anatomy | Month header, weekday labels, 7 columns, date cells, due indicators |
| Dimensions | Cell min 44×48, row gap 4, header 48, day labels caption |
| Colors | Today 2 dp primary outline; selected solid primary; dots use semantic plus count |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Month swipe and arrows; screen reader announces date and item count; start week from locale |

### Bottom sheets

| Property | Specification |
|---|---|
| Anatomy | Handle, heading, body, primary/secondary actions |
| Dimensions | Top radius-md, handle 36×4, padding 20, max height 90% viewport |
| Colors | `surfaceRaised` with 1 dp `border`; no shadow |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Drag dismiss only for non-destructive sheets; keyboard avoids focused input |

### Dialogs

| Property | Specification |
|---|---|
| Anatomy | Title, message, actions |
| Dimensions | Width min(available−48, 400), padding 20, radius-md, action height 48 |
| Colors | `surfaceRaised`, scrim #000000 at 48% |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Focus trapped; destructive button last; dismiss by Escape/back unless critical |

### Toasts/snackbars

| Property | Specification |
|---|---|
| Anatomy | Message, optional Undo button |
| Dimensions | Min height 48, padding 16, radius-md, bottom offset 16 above the bottom bar |
| Colors | Dark surface with light text in light mode; inverted in dark mode |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Toast 3,000 ms informational; undo snackbar 5,000 ms; screen reader live region polite |
| As built (UX06, 2026-11-06) | Material snackbar on the activity, so it survives page changes. It sits above the bottom bar and Add disc on root pages and at the bottom edge on pages without the bar, with an 8 dp side margin (Material default). TextLight fill, SurfaceLight text and PrimaryDark Undo in light mode, inverted in dark. Radius 14, Jakarta Medium without Material letter spacing, up to 4 lines. Texts: "Due saved.", "Changes saved.", "Saved. Next one added for …", "Marked paid.", "Marked unpaid.", "Due deleted." + Undo, "Due restored.", "Nothing to undo.", "Couldn't undo. The due stays deleted." No titles or amounts. |

### Empty states

| Property | Specification |
|---|---|
| Anatomy | Flat or static 3D illustration, headline, body, CTA |
| Dimensions | Illustration 192×192 max, vertical gaps 16, CTA 48 |
| Colors | Background and text; no excessive decoration |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Action opens create flow; explain missing content not user blame |

### Skeleton loaders

| Property | Specification |
|---|---|
| Anatomy | Shimmer placeholder for title, date, badge |
| Dimensions | Card 96 high; title 60% width×16, date 40%×12, badge 72×24 |
| Colors | `surfaceMuted` base; shimmer overlay 8% white/dark |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Show after 150 ms delay; no shimmer with reduced motion; announce Loading once |

### Progress ring

| Property | Specification |
|---|---|
| Anatomy | Track, fill, centered count and label |
| Dimensions | Diameter 112, stroke 10, center label H2 |
| Colors | Track border, progress `completed`, center text |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Clamp 0–100%; accessible label e.g. 6 of 8 completed |

### Add action disc

| Property | Specification |
|---|---|
| Anatomy | 56 dp circle, 24 dp plus glyph, 4 dp ring in the bar's surface color; no extended or labeled variant |
| Dimensions | 56×56 (`radius-disc`), centred over the third bar slot, raised 16 dp above the bar's top edge; `shadow-1` |
| Colors | `accent` fill, `onAccent` glyph, ring = bar `surface` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Accessible name "Add due"; opens the Add editor on the current root's navigation stack so Save or Cancel return to that root; hidden whenever the bar is hidden; ignores repeated taps while the editor is opening |

### Sticky form actions

| Property | Specification |
|---|---|
| Anatomy | Cancel (secondary) and Save (primary) side by side; Delete is a header toolbar action, not a footer button |
| Dimensions | 48 dp buttons in two equal columns, 12 gap, content inset per §5, 1 dp top divider; footer fixed below the scrolling fields |
| Colors | `surface` footer, `border` divider; buttons per Buttons |
| States | Universal state contract applies. Save is disabled until the form is submittable; both are disabled while busy |
| Behavior | Save commits; Cancel returns to the originating root without a write; Delete appears only for a loaded existing due and confirms before soft-deleting |

### Toggles and checkboxes

| Property | Specification |
|---|---|
| Anatomy | Label, optional helper, switch or square check |
| Dimensions | Switch 52×32 with 24 thumb; checkbox 24×24 inside 44 hit target |
| Colors | On primary; off border with muted surface; check onPrimary |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Switch changes immediately with rollback on failure; checkbox announces checked state |

## 8. Screen Specifications

These include future screen concepts. Apply the scope notice above before using their behavior rules for v1; they do not establish additional release acceptance gates.

### Onboarding 1 — Welcome

```text
Skip top-right → 3D calendar hero (280×280) → “Never miss a due date again.” (H1) → “Keep bills, tasks, and deadlines together.” (bodyM) → 4 progress dots (8 diameter, 8 gap) → Next 48 high.
```

**Behavior:** Skip opens Home; Next advances to screen 2.

### Onboarding 2 — Add everything

```text
Skip → 3D bill/task/subscription cards → “Bills, tasks, and deadlines in one place.” → “Add what matters in seconds.” → dots 2/4 → Next.
```

**Behavior:** Next advances to screen 3.

### Onboarding 3 — Smart reminders

```text
Skip → 3D bell/clock → “Get nudged at the right time.” → “Choose reminders that fit your schedule.” → dots 3/4 → Next.
```

**Behavior:** Next advances to screen 4.

### Onboarding 4 — Stay in control

```text
3D check/progress ring → “See what’s due at a glance.” → “Know what’s next and what’s done.” → dots 4/4 → Get Started.
```

**Behavior:** Get Started marks onboarding seen and opens Home; permission request only after user enables reminders.

### Home / dashboard

```text
App bar Duewise → summary ring + counts → segmented All / Upcoming / Overdue / Completed → chronological due cards → Add disc over the bottom bar.
```

**Behavior:** Sort overdue oldest first, then due today, then next due; completed last when All selected; empty state uses Add item CTA.

### Dues list (as built)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): search, status chips, a Type filter, the matching count and Reset filters sit above the list; the header bell replaces the Reminders chip (PA04).

```text
Brand lockup → "Your dues." heading + caption → Reminders chip → error card with Refresh/Retry (only after a failed load) → hairline → "Show N earlier paid dues ›" row (only when there are some) → date-sorted ledger rows → Add disc over the bottom bar.
```

**Behavior:** no app bar; the heading is the page title. Rows open Details. Status is the computed F02 state for the current Manila date. Since the [Focus page content](../specs/focus-pages/spec.md) slice (2026-11-05) the list uses the ledger row and Refresh/Retry appears only with an error. Since UX02 (2026-11-06) paid dues dated before today fold into a muted full-width row (`SurfaceMuted` fill, `primary` semibold text, radius 12, 48 dp minimum, chevron when folded); tapping it shows them in place in date order and the row reads "Hide earlier paid dues". When every active due is folded, a supporting line says so instead of the first-run card. Since UX03 (2026-11-06) a repeating due's meta line adds its repeat ("06 Nov · Bill · Monthly") and its row description adds "repeats monthly"; Dashboard and Calendar rows share this.

### Home / dashboard (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): no search or results list; "Needs attention" and "A little further ahead" lists follow the counters (PA03).

```text
"A clear plan for today." + date → "Upcoming · Month" caption → month total (HeroAmount) → "n unpaid · today to dd MMM" → hairline → "Next date · dd MMM" + next-due link → three summary counters → Find a due (search, category, status chips, Clear filters) → "Filter · n" heading → ledger rows → Add disc.
```
 Since UX10 (2026-11-06) each counter is announced as a button; its look is unchanged.
**Behavior:** the total and next due follow the [PRD F02 upcoming-total rule](PRD.md#f02-dashboard-rules); counters and chips select the same filter; the next-due link opens Details. There is no progress ring; the Paid counter carries the progress description. Refresh/Retry shows only inside the error card. Priority grouping from the mockup is not adopted (the single ordered F02 result list stands).

### Calendar (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): see PA06.

```text
"A date for everything." + caption → month name with ‹ › icon buttons and a Today chip on one line → week-start caption → weekday row → 7-column grid of 44 × 48 day cells (narrower cells on rows too narrow for seven, UX15; no sideways scroll) (number over count; selected `primary` fill, today 2 dp `primary` outline) → selected date heading with "n saved dues" caption → ledger rows → Add disc.
```

**Behavior:** F03 rules unchanged; weekly view, swipe and overflow counts remain future. Since UX15 (2026-11-07) the month always shows seven columns without scrolling: about 40 dp cells at 320 dp, 44 dp from 360 dp up.

### Settings (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): cards with a fixed bottom-right "✓ Save" and a privacy note with a shield (PA09).

```text
"Make it feel like you." + caption → Appearance card with System/Light/Dark segmented control → Start of week card with Monday/Sunday segmented control → Reminders row (title, saved state "On · HH:mm Manila · day before" or "Off", chevron) → "Your data" card (heading, what export and import do, not-encrypted caption, Export dues and Import dues secondary buttons, busy indicator, result line only when there is one) → local-data notice → Save caption → Save preferences → message → Refresh/Retry (only after a load error) → bottom bar.
```

**Behavior:** F04 rules unchanged; the Reminders row is navigation and its subtitle is read-only. UX05 (2026-11-06): both data actions are secondary so they do not compete with Save preferences, and they never touch the preference draft. Export opens Android's save dialog with the suggested name; Import opens Android's file picker, then a native alert "Import dues?" with the counts, Cancel and Import. Each result (counts, cancel, refusal with its reason) appears in the card and is announced. Import is refused before the picker while dues show.

### Item detail (as built, 2026-11-05)

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): the header pencil replaces Edit due, Back to list shows only for an unavailable due, and the page adds the type icon, a Currency / timezone fact and a Notes card (PA08).

```text
Shell title "Due details" + Back → caption → category eyebrow → title → amount (HeroAmount) → status badge → hairline → Due date row → Type row → Repeats row (repeating dues only) → Notes block → hairline → Created / Last updated captions → Retry (error only) → Mark as paid / Mark as unpaid (primary) → payment error (only after a failed save) → "Next one added for dd MMM yyyy." (supporting text, centered, only right after it happened) → Edit due (secondary) → Back to list (link).
```

**Behavior:** F01 rules plus UX01 (2026-11-06): the primary action saves the payment status at once and the badge updates; since UX06 the "Marked paid." or "Marked unpaid." confirmation appears as a snackbar at the bottom edge and announces itself. UX03 (2026-11-06): the Repeats row reads "Every week", "Every month on day N" or "Every year on DD Mon" (the anchor day, which can differ from this occurrence's date); paying a repeating due shows "Marked paid. Next one added for dd MMM yyyy." in the snackbar and as a line under the button, and its button hint says it adds the next one. Since UX12 the timestamps read "Created 06 Nov 2026, 17:27 Manila". After Delete from the editor, Details shows the unavailable card with the "Due deleted." snackbar; Undo reloads the due in place. Provider and other quick actions remain future.

### Add / edit due item

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): required/optional labels, a "Type: Bill · Change" row, collapsible Repeat and Notes, a Paid check row, the bin in the header and "Discard changes?" (PA07, UX22).

```text
App bar title + Delete (loaded existing due only) → What is due? (title) → Type (category picker) → Amount (PHP) → Due date (`dd MMM yyyy`) → Repeat (Bill/Subscription only) → Notes → Payment status (edit only) → sticky Cancel / Save footer.
```

**Behavior:** Essential-first order, revised by UX11 (2026-11-06): title, type, amount and date come first, so the amount requirement is already right when the user reaches Amount; Repeat, Notes and Payment status follow. The caption reads "Title, type, amount and date first. Everything else is optional." The date shows `dd MMM yyyy` with English month abbreviations (UX07), and the amount keyboard is requested without a minus key (UX08). Repeat (UX03, 2026-11-06) is a native Picker in the same outlined field as Type (Never, Weekly, Monthly, Yearly; default Never) with a caption explaining when the next one is added; it is hidden for Task/Other, and switching to those types resets it to Never. After Save the parent page shows "Due saved.", "Changes saved." or "Saved. Next one added for dd MMM yyyy."; after Delete it shows "Due deleted." with Undo (UX06). Required fields follow the [PRD rules](PRD.md#business-rules-and-validation) and the F07 adaptations below; prevent duplicate save; Cancel and Back return to the originating root without a write; Save commits and returns to that root; Delete confirms, then soft-deletes. The HTML mockup's dirty-exit confirmation and PHP paste normalization are not adopted natively.

### Calendar

```text
App bar month/year + month arrows → weekday row → 7-column month grid → selected-day agenda list → Add disc over the bottom bar.
```

**Behavior:** Tapping day updates agenda; week begins per locale; show overflow count +N if >3 items on a day.

### Item detail

```text
App bar back/edit/overflow → category/title/status → due date/time and timezone → recurrence → reminders → notes/amount if present → Complete or Reopen.
```

**Behavior:** Complete retains item in history; Reopen recalculates status; Delete requires confirmation and 5 s undo.

### Notification settings

> **Since 2026-11-07:** superseded in part by the [prototype alignment](#prototype-alignment-2026-11-07): the native Reminders page follows the prototype layout (PA10, round 2 UX19).

```text
App bar → reminders master switch → preset reminder chips → quiet hours start/end → notification permission state → Test notification.
```

**Behavior:** No OS permission prompt until first opt-in; quiet hours default 22:00–08:00 local.

### Settings

```text
App bar → appearance System/Light/Dark → start of week System → timezone Automatic → notifications → export data → about/privacy.
```

**Behavior:** Appearance updates instantly; export uses system share sheet (superseded by UX05: Android's save dialog, not the share sheet); deletion requires confirmation. Since 2026-11-05 Settings is a root tab reached from the bottom bar and shows the bar; Reminders pushed from it hides the bar. The F04 adaptation below governs the shipped content.

### F04 settings adaptation

The later "implement f04" request authorizes [preferences](../specs/f04-preferences/spec.md), governed by [PRD](PRD.md#f04-preference-rules). Use a scrollable Settings page with shared typography/surface cards (originally pushed from Dues with hidden tabs; since 2026-11-05 it is a root tab in the five-slot bar and shows the bar) and labeled native pickers for System/Light/Dark and Monday/Sunday. The defaults are System/Monday. Explicit **Save preferences** commits both choices and immediately applies appearance; this scoped adaptation replaces the future instant-selection rule. Back discards unsaved choices; returning from Reminders/resume retains the active draft. Literal save/error messages and Refresh/Retry remain reachable at 200% text. Opening existing Reminders is a working secondary action that hides the bar; Dues retains its own Reminders shortcut. Calendar headings/date alignment/caption follow the saved week start together. The fixed Manila timezone is explanatory text, not a dead selector. Export, About/privacy, accounts, currency/timezone selectors and data deletion remain outside this slice; bottom-bar composition is governed by §7. Record actual theme/text/focus/picker/navigation acceptance separately from implementation.

### Empty and error

```text
Centered icon/illustration → short heading → one explanatory sentence → primary retry/add CTA.
```

**Behavior:** Offline: local data remains accessible; sync error does not block adding; inline field errors retain input.

## 9. Interaction and Motion

| Token | Value | Use |
|---|---|---|
| `motion.duration-standard` | `160` | Control state feedback, ms |
| `motion.easing-standard` | `cubic-bezier(0.2, 0, 0, 1)` | Curve |

| Interaction | Motion | Haptic | Reduced motion |
|---|---|---|---|
| Push / pop screen | Native Shell transition; no custom tween | None | Native reduced-motion behavior |
| Press | Native pressed state; brief opacity change, 160 ms standard | None | Same static pressed state |
| Focus / error outline | Outline thickens in place, 160 ms standard | None | Instant |
| Add disc, bottom bar | Static; no entrance, pulse or ripple beyond the native control | None | Static |

V1 adds no custom motion library or dependency. When the system reduced-motion setting is on, transitions are removed or cut; nothing loops. Use platform haptics only when supported and user-enabled; no vibration for passive reminders inside the app. Never animate background indefinitely. The 1.0.0 push/pop/spring motion table is withdrawn.

## 10. Notifications and Status Logic

**N1 A / N2 A decided 2026-11-07; native implementation added 2026-11-08:** reminders name their due in the unlocked shade and provide a generic Android public version for the lock screen. Mark as paid and the 10-second Undo require unlocking. Confirmation text names the due and any next date; Undo retains an already-created next occurrence. Public versions contain no actions or financial details, and grouped summaries stay generic. Notes never appear. Wording and drawings: the [notification lab](../specs/notification-lab/spec.md#what-the-lab-proposes); rules: [PRD notification decisions](PRD.md#notification-decisions-2026-11-07). Android/maker lock-screen settings can affect presentation; actual-device acceptance is separate in [evidence](delivery/evidence/NOTIFICATIONS-2026-11-08.md).

The original broader design proposal below is not the current scheduler/status contract. F05 now resolves current date-only reminder policy in [PRD](PRD.md#f05-reminder-rules) and presentation in the [F05 adaptation](#f05-reminder-adaptation). Timed/recurring/per-due variants remain deferred. The word "canonical" below applies within this future concept only.

**Canonical status algorithm:** evaluate using device-local current time in the item’s stored IANA timezone. A date-only due item is due at the end of its calendar day (`23:59:59.999`); a timed item uses its exact local date/time resolved to a UTC instant. For DST gaps, move to next valid local instant; for overlaps, choose earlier offset. Completed takes precedence over all statuses. `dueToday` means due calendar date equals today; `dueSoon` means due date is within the next 3 local calendar days excluding today; `upcoming` means 4+ days away; `overdue` means due instant is strictly earlier than now. If date is earlier than today, overdue regardless of stored time. Recalculate on app foreground, local midnight, timezone change, edit, completion, and notification delivery.

| State | Token | Label | Card cue |
|---|---|---|---|
| Upcoming | `colors.{mode}.info` | Upcoming | Calendar icon |
| Due soon | `colors.{mode}.dueSoon` | Due soon | Clock icon |
| Due today | `colors.{mode}.dueSoon` | Due today | Clock icon + bold date |
| Overdue | `colors.{mode}.overdue` | Overdue | Alert icon + 4 dp leading rule |
| Completed | `colors.{mode}.completed` | Completed | Check icon + strikethrough title |

Implemented v1 states are Paid, Overdue, Due today and Upcoming, computed under the [PRD F02 rules](PRD.md#f02-dashboard-rules) and rendered by the §7 status badge on every surface since 2026-11-05. Due soon and Completed remain future labels.

**Default reminder schedule:** date-only items at 09:00 local 1 day before and 09:00 on due date; timed items 24 h and 1 h before exact instant. Do not schedule reminders in the past. If item created within a reminder window, schedule the next future reminder only. Recurring occurrences use original timezone and recurrence rule; schedule a rolling 30-day horizon, refresh daily and after device reboot where platform permits. Completed occurrences cancel remaining reminders for that occurrence, not future ones. Quiet hours 22:00–08:00: shift non-critical reminders to 08:00 next permitted day, never to after a completed occurrence. Respect OS permission and delivery limitations; never claim guaranteed delivery.

| Event | Title | Body |
|---|---|---|
| Day before | `Due tomorrow` | `{itemTitle} is due tomorrow.` |
| Due today | `Due today` | `{itemTitle} is due today.` |
| Timed 1 h | `Due in 1 hour` | `{itemTitle} is due at {localTime}.` |
| Overdue summary | `Overdue items` | `{count} items need your attention.` |
| Recurring renewal | `Renewal coming up` | `{itemTitle} renews on {localDate}.` |

Template variables are runtime data, not design placeholders. Localize plural rules and date/time with ICU. Notification app badge counts **overdue + due today incomplete occurrences**, capped visually at `99+`; update on foreground, completion, and OS background opportunity. Home tab badge shows overdue only, capped `99+`. Widget displays next 3 incomplete items; private mode hides titles and amounts.

## 11. Accessibility

Meet WCAG 2.2 AA: text contrast ≥4.5:1 (large ≥3:1); non-text state and focus cues ≥3:1, so field outlines use `textSecondary` and the focus ring is 3 dp in `focus`; focus visible, logical, and never clipped. Interactive target minimum 48×48 dp for buttons and fields and 44×44 dp for bottom-bar actions; spacing between adjacent smaller visual controls ≥8. The Add disc has the accessible name "Add due"; the disabled placeholder slot beneath it is not focusable content and is never announced as a destination. Support VoiceOver and TalkBack, keyboard, switch access, and 200% text.

| Element | Screen reader label pattern |
|---|---|
| Due card | `{title}, {category}, {status}, due {fullDate}, {amount if any}. Button, opens details.` |
| Category chip | `{category}, {selected or not selected}, filter` |
| Date cell | `{weekday}, {fullDate}, {count} due items, {selected if selected}` |
| Reminder toggle | `Reminders, on/off, switch` |
| Complete action | `Mark {title} complete, button` |
| Progress ring | `{done} of {total} completed, {percentage} percent` |
| Bottom bar item | `{tabName}, tab, {selected if selected}` (the native bar supplies position; the Add placeholder slot is excluded) |
| Snackbar | `Announcement: {message}. Undo available.` |
| Add disc | `Add due, button` |

**Focus order:** app bar → screen heading → primary content top-to-bottom → inline actions → sticky footer actions (editor) → Add disc → bottom bar; when modal opens, move focus to its heading, trap within, return to trigger on dismiss. Pair status color with icon and literal label. Do not rely on animation, haptic, or sound alone. RTL mirrors layout and directional navigation but not numerals, dates, clocks, checkmarks, or progress meaning; locale controls date format, first weekday, number grouping, currency and pluralization. Use IANA timezone IDs; never store date-only items as UTC midnight.

## 12. Design Tokens

This JSON is canonical. Keys are case-sensitive; consumers should generate platform constants from this object. Colors are opaque sRGB hex; shadow opacity is a unit interval; durations are milliseconds.

```json
{
  "colors": {
    "brand": {
      "ink": "#253C20",
      "lime": "#D9F27E"
    },
    "accent": {
      "coral": "#E85D5D"
    },
    "light": {
      "background": "#F8F9F5",
      "surface": "#FFFFFF",
      "surfaceRaised": "#FFFFFF",
      "surfaceMuted": "#EEF1E8",
      "text": "#18201E",
      "textSecondary": "#53605B",
      "border": "#C9D0CC",
      "primary": "#253C20",
      "onPrimary": "#FFFFFF",
      "accent": "#D9F27E",
      "onAccent": "#203019",
      "hero": "#EAF1D8",
      "heroText": "#21331C",
      "overdue": "#AE2638",
      "dueSoon": "#87430B",
      "completed": "#166534",
      "info": "#253C20",
      "focus": "#253C20",
      "disabled": "#53605B",
      "paidSurface": "#EDF8EF"
    },
    "dark": {
      "background": "#111710",
      "surface": "#1C251A",
      "surfaceRaised": "#273122",
      "surfaceMuted": "#2A3524",
      "text": "#F4F5F3",
      "textSecondary": "#B8C3BE",
      "border": "#64716B",
      "primary": "#D9F27E",
      "onPrimary": "#203019",
      "accent": "#D9F27E",
      "onAccent": "#203019",
      "hero": "#26341E",
      "heroText": "#E8F3D3",
      "overdue": "#FFB1BB",
      "dueSoon": "#FFD29C",
      "completed": "#A1E9B4",
      "info": "#D9F27E",
      "focus": "#D9F27E",
      "disabled": "#B8C3BE",
      "paidSurface": "#17352A"
    }
  },
  "typography": {
    "fontFamily": "Plus Jakarta Sans",
    "display": { "size": 40, "weight": 700, "lineHeight": 48, "letterSpacing": -0.3, "unit": "sp/dp", "reserved": true },
    "h1": { "size": 26, "weight": 700, "lineHeight": 32, "letterSpacing": -0.3, "unit": "sp/dp" },
    "h2": { "size": 20, "weight": 700, "lineHeight": 26, "letterSpacing": -0.2, "unit": "sp/dp" },
    "h3": { "size": 16, "weight": 600, "lineHeight": 22, "letterSpacing": 0, "unit": "sp/dp" },
    "bodyL": { "size": 16, "weight": 400, "lineHeight": 24, "letterSpacing": 0, "unit": "sp/dp" },
    "bodyM": { "size": 16, "weight": 400, "lineHeight": 24, "letterSpacing": 0, "unit": "sp/dp" },
    "bodyS": { "size": 14, "weight": 400, "lineHeight": 20, "letterSpacing": 0, "unit": "sp/dp" },
    "caption": { "size": 12, "weight": 500, "lineHeight": 16, "letterSpacing": 0, "unit": "sp/dp" },
    "button": { "size": 16, "weight": 600, "lineHeight": 24, "letterSpacing": 0, "unit": "sp/dp" }
  },
  "spacing": {
    "space-1": 4,
    "space-2": 8,
    "space-3": 12,
    "space-4": 16,
    "space-5": 20,
    "space-6": 24,
    "space-7": 32
  },
  "layout": {
    "contentInset": { "compact": 14, "phone": 20, "medium": 32, "expanded": 40 },
    "breakpoints": { "compactMax": 359, "phoneMax": 599, "mediumMax": 839 },
    "readableWidth": { "medium": 640, "expanded": 720 }
  },
  "radius": {
    "radius-md": 14,
    "radius-pill": 999,
    "radius-disc": 28
  },
  "elevation": {
    "shadow-0": 0,
    "shadow-1": 6,
    "unit": "dp"
  },
  "motion": {
    "duration-standard": 160,
    "easing-standard": "cubic-bezier(0.2, 0, 0, 1)"
  },
  "touchTargets": {
    "barAction": 44,
    "control": 48,
    "unit": "dp"
  },
  "components": {
    "button": { "height": 48, "radius": 14, "paddingX": 16, "paddingY": 10 },
    "field": { "height": 48, "radius": 14, "outline": 1, "focusOutline": 3, "errorOutline": 3 },
    "dueCard": { "minHeight": 96, "radius": 14 },
    "bottomBar": { "height": 64, "slots": 5, "addSlot": 2 },
    "addDisc": { "size": 56, "ring": 4, "raise": 16, "elevation": 6 }
  }
}
```

## 13. Implementation Notes

The native .NET MAUI Android app is the only consumer of these tokens; the [SDD](SDD.md) owns the architecture. XAML resource keys kept the names used since the indigo baseline so page markup did not change; only their values moved to Focus. The 1.0.0 Flutter/React Native handoff material is withdrawn.

| Token or role | Resource / file |
|---|---|
| `colors.light.*` / `colors.dark.*` | `Resources/Styles/Colors.xaml`, keys `<Name>Light` / `<Name>Dark`: `background` → `BackgroundLight/Dark`, `surface` → `SurfaceLight/Dark`, `surfaceRaised` → `SurfaceRaisedLight/Dark`, `surfaceMuted` → `SurfaceMutedLight/Dark`, `text` → `TextLight/Dark`, `textSecondary` → `TextSecondaryLight/Dark`, `border` → `BorderLight/Dark`, `primary` → `PrimaryLight/Dark`, `onPrimary` → `OnPrimaryLight/Dark`, `accent` → `AccentLight/Dark`, `onAccent` → `OnAccentLight/Dark`, `hero` → `HeroLight/Dark`, `heroText` → `HeroTextLight/Dark`, `overdue` → `DestructiveLight/Dark`, `dueSoon` → `DueSoonLight/Dark`, `completed` → `CompletedLight/Dark`, `info` → `InfoLight/Dark`, `focus` → `FocusLight/Dark`, `disabled` → `DisabledLight/Dark`, `paidSurface` → `PaidSurfaceLight/Dark` |
| Brand and platform colors | `Resources/Images/brand_mark.svg` (ink, white, coral). Android `Platforms/Android/Resources/values/colors.xml` and `values-night/colors.xml` mirror `primary` and `background` (plus accent/onAccent/surface for the bar renderer) so the system window and bar ring match the theme |
| Type roles | `Resources/Styles/Styles.xaml`: `Heading1` (h1), `Heading2` (h2), `Heading3` (h3), `BodyLabel` (bodyM), `SupportingLabel` (bodyS), `CaptionLabel` (caption), `FieldLabel` (semibold field label), `ErrorLabel` (bodyS in `overdue`); fonts registered as `JakartaRegular`, `JakartaMedium`, `JakartaSemibold`, `JakartaBold` |
| Components | `SurfaceCard` (card), `FieldBorder` (text-input outline, `textSecondary` stroke), `DueCard` (due-item card with Selected state), `BaseButton` (primary; implicit `Button` style), `SecondaryButton`, `DestructiveButton` |
| Bottom bar and Add disc | `AppShell.xaml` `TabBar` with five `Tab`s (`AppShell.AddSlot = 2`) plus `Platforms/Android/DuewiseShellRenderer.cs`, which disables slot 2, keeps the 64 dp bar growing with text, and floats the 56 dp disc with its ring and 6 dp elevation; `AppShell.OpenAddAsync` pushes the editor on the current root |
| Sticky form actions | `Pages/DueEditPage.xaml` outer `Grid` rows `*,Auto` (scrolling fields, footer) with footer rows `1,Auto` (divider, Cancel/Save); Delete `ToolbarItem` managed in `DueEditPage.xaml.cs`. Since 2026-11-06 the footer stays above the on-screen keyboard (the interaction lab's reserved-space choice): `Resize` soft-input mode while the editor is visible plus a bottom `SoftInput` safe-area edge; emulator-verified, phone pending |
| Insets and readable width | `Pages/PageLayout.cs` |

**Naming:** design tokens `colors.light.primary`, `typography.bodyM`, `space-4`, `radius-md`, `shadow-1`, `motion.duration-standard`; XAML resource keys are PascalCase with a `Light`/`Dark` suffix for themed colors; pages PascalCase (`CalendarPage`), assets snake_case (`nav_add.svg`).

Persist theme selection `system|light|dark`; `system` is default and follows OS changes without restart. Derive statuses from domain timestamps, not saved UI colors.

**Developer do:** use semantic theme colors; run contrast checks; test 200% type; respect safe areas; preserve local dates; handle denied notifications; confirm destructive actions; verify both themes and RTL. **Developer don't:** hardcode raw colors in components; set fixed text heights; use animated 3D in lists; auto-request permissions on launch; represent status by hue only; silently discard edits; promise exact OS notification delivery; outline fields with `border`.

## Quality Checklist

This checklist spans the full design vision. Deferred-feature rows are not mandatory v1 gates; unchecked items are not verification evidence. Current acceptance procedures and applicability are in the [test plan](delivery/TEST-PLAN.md).

- [ ] Uses only canonical token names and values; no arbitrary colors, radii, or spacing.
- [ ] All normal text pairs pass ≥4.5:1; large text and control boundaries pass ≥3:1.
- [ ] Every button and field is ≥48×48 dp; bottom-bar actions are ≥44×44 dp.
- [ ] Works in light/dark/system mode without loss of hierarchy.
- [ ] Supports 200% type, keyboard focus, VoiceOver/TalkBack, and RTL.
- [ ] Respects safe areas, the 4-unit spacing rhythm, 14 dp radius, and screen breakpoints.
- [ ] Focus indicator is 3 dp in `focus`; field outlines use `textSecondary` and meet 3:1.
- [ ] Bottom bar shows five equal slots with the 56 dp Add disc centred over the disabled placeholder; bar and disc hide together on pushed pages.
- [ ] All controls implement applicable default/pressed/disabled/focused/error/loading states.
- [ ] Status uses text + icon + semantic color; date-only and timezone rules are correct.
- [ ] Save, edit, recurrence, complete, reopen, delete, and offline flows work.
- [ ] Reminders obey permission, quiet hours, recurrence, and cancellation rules.
- [ ] Reduced-motion mode removes movement and indefinite animation.
- [ ] No decorative 3D is used in dense functional screens.
- [ ] No input is lost on validation, failure, back navigation, or permission denial.
- [ ] Widget protects private content and displays correct next-three items.
- [ ] All displayed dates, amounts, plurals, and notification copy are localized.

## F03 Calendar adaptation

Authorized 2026-10-18: [production Calendar](../specs/f03-calendar/spec.md) replaces the fixture preview. [PRD policy](PRD.md#f03-calendar-rules) deliberately keeps Monday first, numeric total counts including Paid, explicit Manila Today and session selection. This overrides future locale-start/Sunday-prototype suggestions for this slice; no stored preference is added. Use the adopted semantic resources and single vertical CollectionView owner, a persistent month/agenda header, horizontal overflow for seven minimum 48-unit date targets and wrapping agenda cards. Mark selected dates with the primary style and today with an outline plus explicit semantic descriptions. Agenda status text reuses F02 rules. All dates announce full date/count/selected/today; Add and rows are working actions. Details/editor hide tabs and return to the selected Calendar. Loading, global empty, selected-day empty and stale errors remain distinct. Full assistive-technology/other-width acceptance requires separate native evidence.

## F07 category adaptation

The authorized [category slice](../specs/f07-categories/spec.md) uses a labeled native Picker inside the existing outlined field, shared typography/themes, focus/error states, disabled busy/completed state and explicit validation announcement. A fixed selector replaces the prototype's horizontally crowded chips so all four names remain reachable at large text. Saved cards/Details show literal category text; row semantics include it. Dashboard's category Picker composes with search/status and Clear filters without changing the persistent header or global summary. No category icons, custom category management, recurrence controls or amount totals are introduced; optional amounts are the separate adaptation below. [PRD](PRD.md#f07-category-rules) overrides the future form's amount/timing rules; full assistive-technology/visual acceptance remains separately verified.

## Philippine preset category adaptation

Authorized 2026-11-08, **presets only**: [native scope/scenarios](../specs/ph-categories-native/spec.md), [PRD rules](PRD.md#philippine-preset-category-rules). The existing four-value selection is labelled **Type** and continues to control amount/repeat policy. Detailed **Category · optional** sits after Due date, with its current label and a compact Choose action; Other is the fallback. It does not push required fields later or alter the editor's sticky Cancel/Save.

Choose opens a full-screen modal with a wrapping title, accessible 48-unit Back/Cancel targets, labelled search, result count and a single virtualized grouped list. Use the existing Focus surface/outline/glyph treatment. Rows have a 56-unit minimum, expand for wrapped text, use decorative outline category glyphs, and mark the current selection with a check plus its spoken selected state. Search includes catalog/group names and common Filipino aliases. Search/type/theme changes retain editor values; selecting updates only the draft, and picker Cancel/Back keeps the preceding choice. Empty search results are explicit.

Shared saved rows show the detailed label/icon when selected; Other retains the preceding type presentation. Screen-reader text includes both type and detailed category. Details keeps Due type and adds Category as a separate fact. No custom-category management, biller/provider row, new filter policy or automatic financial rules are introduced by this slice. Verify both themes, compact width, 200% text, keyboard and native return behavior separately from actual-phone/TalkBack acceptance.

## F07 optional amount adaptation

The authorized [optional-amount slice](../specs/f07-optional-amounts/spec.md) keeps the existing outlined native amount Entry and persistent “Amount (PHP)” label. Add category-aware requirement text and semantics: Bill/Subscription announce a required amount; Task/Other announce an optional amount that may be left blank. A blank optional amount previews “No amount”; saving a blank required amount focuses the field and announces its error. Preserve numeric keyboard, invariant decimal hint, centavo preview, focus/error outlines, busy/completed disabled state and the existing screen-reader announcement pattern.

Implemented 2026-11-06: the requirement caption reads “Required for {Type}.” or “Optional for {Type}. Blank means no amount; zero is a recorded amount.”; the optional placeholder is the single word “Optional” because a one-line Entry cannot wrap and a longer hint was cut off at 200% text; the preview announces “Will be saved with no amount”.

Saved Dues/Dashboard cards, Details and Calendar agenda show the formatted amount when present and literal “No amount” when absent; row semantics include the literal. Since UX14 (2026-11-07) Details shows the literal in `textSecondary` at body size instead of HeroAmount, so it never reads like a peso figure. Do not infer zero, hide the amount line, add amount totals or alter status/count/layout behavior for absent amounts. Zero remains a visible `₱0.00` amount. Verify category switching, clearing an optional amount, required blank rejection, large text and both themes at the device boundary; full accessibility/other-width acceptance remains separate.

## F05 reminder adaptation

F05 is a separately authorized exception to the original deferred reminder concept. [PRD reminder rules](PRD.md#f05-reminder-rules) replace the conflicting historical/future schedule proposals for current date-only dues. The pushed Reminders page uses PageLayout, one vertical scroll owner, semantic cards/labels, full-width wrapping actions and outlined native TimePickers. Dues exposes a labeled Reminders entry; the pushed page hides tabs and uses native Back.

Show app opt-in separately from Android permission/channel state and a literal Android-reported battery/background status. Provide a working Android app / battery settings action for late-reminder recovery. Returning refreshes status and retains schedule drafts; no automatic power-policy change or exemption request. Use an explicit Save and enable action, a day-before switch, editable Manila time and quiet-hour labels, Save schedule, Disable, Refresh/Retry, Android settings and static Test notification. Do not show per-due presets, timed/recurring controls or general Settings destinations. Schedule controls are drafts; status refresh retains them. State/error text is literal, with busy/not-loaded actions disabled. Explain inexact delivery and force-stop/battery limits in the page. UX04 (2026-11-06): while reminders are on and the phone reports Xiaomi, Redmi or POCO, the status card adds one plain paragraph under the battery status: tap "Android app / battery settings", turn on Autostart, then set Battery saver to "No restrictions". It is body text in the existing card, not a new component or deep link. N1/N2 notifications use the app-owned monochrome calendar/check icon and the privacy/action presentation defined in section 10; Test remains generic. Test actions require opt-in and Android access. Verify themes, large text, native time dialogs, focus, permission flow and tap draft preservation at the device boundary; implementation alone does not establish full accessibility acceptance.
