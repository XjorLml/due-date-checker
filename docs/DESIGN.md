# Duewise — Product Design System

**Version:** 1.0.0 · **Status:** Normative · **Platforms:** iOS / Android · **Baseline:** 2026-10-16

All dimensions are logical **dp** (Android) / **pt** (iOS) unless marked `px` (asset pixels), `sp` (scalable type), or `ms`. At 1× density, 1 dp/pt corresponds to one reference pixel. Never hardcode physical pixels for layouts. Token paths in prose use `colors.light.text`; generated implementation constants may use camelCase.

## Scope and authority

This document owns UI/UX standards, brand, tokens, components, interaction, accessibility, and theme behavior. The [PRD](PRD.md) owns approved product scope and business behavior; the [SDD](SDD.md) owns the .NET MAUI implementation. Design specifications are not evidence that the current app implements or passes them. See [STATUS](delivery/STATUS.md) for delivery evidence.

The original design baseline below intentionally retains broader future concepts. For approved v1, apply visual/accessibility standards to the local dues list and editor. Onboarding, categories, calendar/dashboard, timed/recurring items, reminders, notifications, widgets, export, sync, iOS, and settings concepts are outside approved v1 scope. Their appearance here does not authorize implementation.

Product-facing exceptions for v1 are governed by the [PRD rules](PRD.md#business-rules-and-validation) and [workflows](PRD.md#user-workflows): required PHP amounts, no approved title-length cap, Asia/Manila calendar dates, date-sorted active dues including paid rows, payment changes on Save, Back/Cancel without a write, and confirmed soft deletion without Undo/restore. The future optional-amount, 120-character title, completed-last, automatic-timezone/localization, immediate-toggle, unsaved-confirmation, and Undo descriptions below do not override those requirements.

The design brand/fonts/token system are not fully adopted by the existing MAUI screens. Preserve the standard without claiming compliance or changing code during documentation work. Unresolved product/design adoption decisions are recorded in the [audit](delivery/DOCS-REFACTOR.md#unresolved-decisions).

The [branding showcase image](Duely%20App%20Branding%20Showcase%20-%20chatgpt.png) is early AI-generated inspiration, not a specification. Its "Duely" name and other name candidates, accent palette, typography, and screens do not override this document: where they differ, the brand and tokens here apply. Its onboarding and reminder screens are outside v1 like the concepts above. Duewise is the chosen name (user decision, 2026-10-17).

## Table of Contents

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

## 1. Overview

The product vision/personas here include future capabilities. Approved v1 users and scope are defined in the PRD.

Duewise tracks bills, tasks, subscriptions, and deadlines with explicit due dates, recurring schedules, notifications, calendar visibility, and completion history. Default data entry is local-first; notification scheduling must function without network connectivity.

| Persona | Need | Default workflow |
|---|---|---|
| Busy student | Track assignments and exam dates | Add task → select date → choose reminder → complete |
| Household organizer | Track rent, utilities and renewals | Add bill/subscription → recurrence → reminders → review calendar |

**Principles:** (1) Show the next actionable date first. (2) One primary action per screen. (3) Status is readable without color. (4) Errors are recoverable without data loss. (5) Decoration never obscures information.

| Voice | Do | Don’t |
|---|---|---|
| Calm | “Electric bill is due tomorrow.” | “PANIC! You’re running out of time!” |
| Direct | “Choose a due date.” | “Please proceed to configure the relevant temporal parameter.” |
| Supportive | “Marked complete.” | “Finally, you did it!” |
| Specific | “Couldn’t save. Try again.” | “Something went wrong.” |

## 2. Brand Identity

**Wordmark:** `Duewise` in Plus Jakarta Sans Bold 700, title case; use `colors.light.text` on light and `colors.dark.text` on dark. Icon is a single rounded calendar silhouette with a centered checkmark and one small coral date marker; maximum three principal shapes. No words or numbers inside the app icon.

| Rule | Specification |
|---|---|
| Master export | 1024×1024 px PNG, sRGB, opaque background |
| Symbol bounding box | 716×716 px centered, 154 px inset |
| Clear space | ≥128 px on all sides at 1024 px export; for wordmark lockup ≥0.5× symbol height |
| Minimum app icon | 40×40 px raster; below 40 px use flat silhouette without fine highlights |
| Wordmark minimum | 96 px wide in digital layouts |
| Corner treatment | Full-bleed square master; OS applies launcher masking |
| Forbidden | Stretching, rotating, adding drop-shadow outside canvas, outlining, changing gradient direction, placing text, relying on coral alone for status |

**Why:** One recognizable silhouette stays identifiable in launcher masks, notification contexts, and small widgets.

## 3. Color System

The semantic foreground tokens are deliberately darker in light mode and lighter in dark mode than decorative accents. Use theme tokens, never raw accent colors for text.

| Token | Light hex | Dark hex | Usage |
|---|---|---|---|
| `colors.{mode}.background` | #F8FAFC | #0B1220 | App canvas |
| `colors.{mode}.surface` | #FFFFFF | #111C30 | Cards and forms |
| `colors.{mode}.surfaceRaised` | #FFFFFF | #1E293B | Modal and floating surfaces |
| `colors.{mode}.surfaceMuted` | #F1F5F9 | #25334A | Subtle sections |
| `colors.{mode}.text` | #0F172A | #F8FAFC | Primary copy |
| `colors.{mode}.textSecondary` | #475569 | #CBD5E1 | Supporting copy |
| `colors.{mode}.border` | #CBD5E1 | #64748B | Dividers and field outlines |
| `colors.{mode}.primary` | #4338CA | #A5B4FC | Primary actions and selected controls |
| `colors.{mode}.onPrimary` | #FFFFFF | #0F172A | Text on solid primary fill |
| `colors.{mode}.overdue` | #B42332 | #FDA4AF | Overdue text and symbols |
| `colors.{mode}.dueSoon` | #92400E | #FCD34D | Due-soon text and symbols |
| `colors.{mode}.completed` | #166534 | #86EFAC | Completed text and symbols |
| `colors.{mode}.info` | #1D4ED8 | #93C5FD | Informational labels |
| `colors.{mode}.focus` | #2563EB | #93C5FD | Keyboard focus ring |
| `colors.{mode}.disabled` | #64748B | #94A3B8 | Disabled content (not informational text) |

| Brand / accent token | Hex | Usage |
|---|---|---|
| `colors.brand.indigo` | #4338CA | Decorative gradient stop |
| `colors.brand.blue` | #2563EB | Decorative gradient stop |
| `colors.brand.gradientStart` | #4338CA | Decorative gradient stop |
| `colors.brand.gradientMiddle` | #4F46E5 | Decorative gradient stop |
| `colors.brand.gradientEnd` | #2563EB | Decorative gradient stop |
| `colors.accent.coral` | #E85D5D | Illustration accent; semantic text uses theme variant |
| `colors.accent.amber` | #B45309 | Illustration accent; semantic text uses theme variant |
| `colors.accent.green` | #15803D | Illustration accent; semantic text uses theme variant |
| `colors.accent.info` | #1D4ED8 | Illustration accent; semantic text uses theme variant |

**Primary gradient:** 135° from `colors.brand.gradientStart` at 0%, `colors.brand.gradientMiddle` at 52%, to `colors.brand.gradientEnd` at 100%. Use only for brand hero, onboarding CTA and progress accents; functional solid primary buttons use `colors.{mode}.primary`.

| Neutral token | Hex |
|---|---|
| `colors.slate.100` | #F8FAFC |
| `colors.slate.200` | #F1F5F9 |
| `colors.slate.300` | #E2E8F0 |
| `colors.slate.400` | #CBD5E1 |
| `colors.slate.500` | #94A3B8 |
| `colors.slate.600` | #64748B |
| `colors.slate.700` | #475569 |
| `colors.slate.800` | #334155 |
| `colors.slate.900` | #1E293B |
| `colors.slate.950` | #0F172A |

**Contrast audit (WCAG ratio calculated from hex values; normal text needs ≥4.5:1, large text ≥3:1):**

| Foreground / background | Ratio | Normal AA |
|---|---:|---|
| `light.text` / `light.background` | 17.06:1 | Pass |
| `light.textSecondary` / `light.background` | 7.24:1 | Pass |
| `light.text` / `light.surface` | 17.85:1 | Pass |
| `light.primary` / `light.surface` | 7.90:1 | Pass |
| `light.overdue` / `light.surface` | 6.51:1 | Pass |
| `light.dueSoon` / `light.surface` | 7.09:1 | Pass |
| `light.completed` / `light.surface` | 7.13:1 | Pass |
| `light.info` / `light.surface` | 6.70:1 | Pass |
| `light.onPrimary` / `light.primary` | 7.90:1 | Pass |
| `dark.text` / `dark.background` | 17.89:1 | Pass |
| `dark.textSecondary` / `dark.background` | 12.61:1 | Pass |
| `dark.text` / `dark.surface` | 16.28:1 | Pass |
| `dark.primary` / `dark.surface` | 8.55:1 | Pass |
| `dark.overdue` / `dark.surface` | 9.01:1 | Pass |
| `dark.dueSoon` / `dark.surface` | 11.82:1 | Pass |
| `dark.completed` / `dark.surface` | 12.13:1 | Pass |
| `dark.info` / `dark.surface` | 9.45:1 | Pass |
| `dark.onPrimary` / `dark.primary` | 8.96:1 | Pass |

If any combination is marked Fail, it is prohibited for normal-size text. Non-text controls and focus indicators require ≥3:1 against adjacent colors. Gradient text backgrounds require testing the *least* contrasting stop.

## 4. Typography

**Font:** Plus Jakarta Sans (bundled variable or static files, licensed with app). Fallback iOS: SF Pro Text → system sans; Android: Roboto → sans-serif. Do not synthesize missing font weights. Type metrics are identical across platforms; use scaled font units (`sp` Android, scaled `pt` iOS).

| Token | iOS size pt | Android size sp | Weight | Line height pt/dp | Tracking pt/dp |
|---|---:|---:|---:|---:|---:|
| `typography.display` | 36 | 36 | 700 | 42 | -0.7 |
| `typography.h1` | 30 | 30 | 700 | 38 | -0.5 |
| `typography.h2` | 24 | 24 | 700 | 32 | -0.35 |
| `typography.h3` | 20 | 20 | 600 | 28 | -0.2 |
| `typography.bodyL` | 18 | 18 | 400 | 28 | 0 |
| `typography.bodyM` | 16 | 16 | 400 | 24 | 0 |
| `typography.bodyS` | 14 | 14 | 400 | 20 | 0.1 |
| `typography.caption` | 12 | 12 | 500 | 16 | 0.2 |
| `typography.button` | 16 | 16 | 600 | 24 | 0 |
| `typography.overline` | 11 | 11 | 700 | 16 | 1 |

**Scaling:** honor system text scale up to 200% minimum; allow vertical reflow and multi-line labels. At 130% and above, replace two-column forms with one column, permit button text to wrap to two lines, and increase button height to fit content (minimum 56). Never truncate due date, amount, error, or primary action. Use baseline alignment rather than fixed text containers. On iOS use `UIFontMetrics`; on Android use `sp` and do not disable font scaling.

## 5. Layout and Spacing

| Token | dp/pt |
|---|---:|
| `space-1` | 8 |
| `space-2` | 16 |
| `space-3` | 24 |
| `space-4` | 32 |
| `space-5` | 40 |
| `space-6` | 48 |
| `space-7` | 56 |
| `space-8` | 64 |
| `space-9` | 72 |
| `space-10` | 80 |
| `space-11` | 88 |
| `space-12` | 96 |

**Base grid:** 8 pt; exceptions: 4 pt optical alignment, 1–2 pt borders. Screen horizontal margins: phone 24, large phone 24, tablet 40. Content max width on tablet: 720; center within viewport. Respect system top safe inset and bottom gesture inset; never assume fixed status-bar height.

| Window width (dp/pt) | Layout |
|---|---|
| 0–359 phone compact | Single column; 16 side margins |
| 360–599 phone | Single column; 24 side margins |
| 600–839 large phone / small tablet | Single column; 32 side margins, 640 max content |
| ≥840 tablet | Two-pane home/calendar; 40 side margins, 720 max per content pane |

| Radius token | dp/pt |
|---|---:|
| `radius-xs` | 4 |
| `radius-sm` | 8 |
| `radius-md` | 12 |
| `radius-lg` | 16 |
| `radius-xl` | 20 |
| `radius-2xl` | 24 |
| `radius-pill` | 999 |

**Shadows:** color `#0F172A` light / `#000000` dark; shadow tokens are `(x, y, blur, spread, opacity)` in dp/pt except opacity. Android approximate with elevation 1/3/6 for levels 1/2/3 plus surface tonal separation; never depend on shadow alone for hierarchy.

| Token | X | Y | Blur | Spread | Opacity |
|---|---:|---:|---:|---:|---:|
| `shadow-0` | 0 | 0 | 0 | 0 | 0.00 |
| `shadow-1` | 0 | 2 | 8 | 0 | 0.08 |
| `shadow-2` | 0 | 6 | 20 | -2 | 0.12 |
| `shadow-3` | 0 | 12 | 32 | -4 | 0.16 |
## 6. Iconography and Illustration

Use **Lucide** icons with a pinned package version, 2 dp stroke, round caps/joins. Standard size 24, compact 20, large 32; glyph sits within a 44×44 minimum hit box. Optical adjustment ≤1 dp. Mirror directional arrows in RTL; never mirror calendar numerals or checkmarks.

**3D hero recipe:** 30° three-quarter view, 45° elevation; key light top-left at azimuth −45°, elevation 55°, softbox area 60% object width; fill light from front-right at 25% key intensity; ambient occlusion radius 18 px at 1024 px render; clay roughness 0.32, clearcoat 0.18, transmission 0.12 on glass details; bevel 12 px at 1024 px; contact shadow y=20 px blur=44 px opacity=0.16. Indigo/blue body, coral ≤8% image area, green only for completed states. Export WebP 2× and 3× with transparent background.

3D is permitted on onboarding, first-run empty states, and achievement moments only; all navigation, list rows, calendar cells, controls, and dense dashboard surfaces use flat vectors. Provide static illustration fallback and avoid 3D render in low-power animation loops.

## 7. Components

**Universal state contract:** every interactive component supports default, pressed, disabled, focused, error (when applicable), and loading (when asynchronous). Pressed: overlay `#0F172A` at 8% light / `#FFFFFF` at 10% dark for 80 ms; disabled: 40% foreground opacity with no action; focused: 2 dp `colors.{mode}.focus` outer ring with 2 dp gap; error: semantic overdue foreground plus descriptive message; loading: preserve width and label accessible name, show 20 dp spinner, block duplicate actions. Error/loading are inapplicable to purely decorative status indicators. Touch targets ≥44×44; controls in Android target ≥48×48 where possible.

### Buttons

| Property | Specification |
|---|---|
| Anatomy | Primary, secondary, ghost, destructive; leading icon optional, label required |
| Dimensions | Height 56, min width 96, horizontal padding 20, icon 20, gap 8, radius-lg; full width in forms |
| Colors | Primary: `primary/onPrimary`; secondary: `surface` with `border` and `text`; ghost: transparent with `primary`; destructive: `overdue` on `surface`, 1 dp overdue outline |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | On press release invoke once; destructive always confirms if data cannot be restored; spinner replaces leading icon and retains text |

### Text inputs

| Property | Specification |
|---|---|
| Anatomy | Persistent label, field, optional prefix/suffix, helper/error line |
| Dimensions | Min height 56, padding 16 horizontal/12 vertical, radius-md, border 1 (focus 2); label above 8 gap; error below 4 gap |
| Colors | Surface fill, border; focus ring; error border `overdue`; placeholder `textSecondary` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Trim leading/trailing spaces on save except notes; validate on blur and submit; do not clear entered text on error |

### Date/time pickers

| Property | Specification |
|---|---|
| Anatomy | Label, value, calendar/clock trigger, native picker or accessible custom dialog |
| Dimensions | Trigger 56 high, icon 20, 16 padding; calendar day target 44 min |
| Colors | Selected date `primary` background and `onPrimary` text; invalid date `overdue` text |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Store absolute instant plus IANA timezone for timed dues; date-only uses local calendar date; confirm selection explicitly |

### Due-item card

| Property | Specification |
|---|---|
| Anatomy | Category icon, title, due date, status badge, optional amount, overflow action |
| Dimensions | Min height 104, padding 16, radius-lg, gap 8, card margin-bottom 8 |
| Colors | Surface with border; overdue left border 4 `overdue`; completed title strikethrough |
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

| Property | Specification |
|---|---|
| Anatomy | Status icon 14, short label |
| Dimensions | Min height 28, padding 8 horizontal/4 vertical, radius-pill |
| Colors | Text semantic status; tinted surface at 10% semantic color with tested contrast |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Never use hue alone; label includes status word |

### Swipeable list item

| Property | Specification |
|---|---|
| Anatomy | Card row plus revealed action rail |
| Dimensions | Row min height 80; actions 72 wide each; threshold 96 drag |
| Colors | Complete action green; delete action overdue; text uses accessible contrast |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Reveal actions at 96 drag; do not immediately delete; undo snackbar 5,000 ms |

### Bottom tab bar

| Property | Specification |
|---|---|
| Anatomy | Home, Calendar, Inbox, Settings; icon and label |
| Dimensions | Height 64 plus safe bottom inset, icon 24, label caption, 4 equal slots |
| Colors | Surface, top border; active `primary`, inactive `textSecondary` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Preserve navigation state per tab; badge counts actionable overdue items only |

### App bar

| Property | Specification |
|---|---|
| Anatomy | Back or title, optional trailing actions |
| Dimensions | Height 56 plus safe top inset; horizontal padding 24; actions 44 hitbox |
| Colors | Background `background`, title `text` |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Back respects navigation stack; never hide unsaved edits without confirmation |

### Calendar grid

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
| Dimensions | Top radius-2xl, handle 36×4, padding 24, max height 90% viewport |
| Colors | SurfaceRaised, shadow-3 |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Drag dismiss only for non-destructive sheets; keyboard avoids focused input |

### Dialogs

| Property | Specification |
|---|---|
| Anatomy | Title, message, actions |
| Dimensions | Width min(available−48, 400), padding 24, radius-xl, action height 48 |
| Colors | SurfaceRaised, scrim #000000 at 48% |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Focus trapped; destructive button last; dismiss by Escape/back unless critical |

### Toasts/snackbars

| Property | Specification |
|---|---|
| Anatomy | Message, optional Undo button |
| Dimensions | Min height 48, padding 16, radius-md, bottom offset 16 above tab bar |
| Colors | Dark surface with light text in light mode; inverted in dark mode |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Toast 3,000 ms informational; undo snackbar 5,000 ms; screen reader live region polite |

### Empty states

| Property | Specification |
|---|---|
| Anatomy | Flat or static 3D illustration, headline, body, CTA |
| Dimensions | Illustration 192×192 max, vertical gaps 16, CTA 56 |
| Colors | Background and text; no excessive decoration |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Action opens create flow; explain missing content not user blame |

### Skeleton loaders

| Property | Specification |
|---|---|
| Anatomy | Shimmer placeholder for title, date, badge |
| Dimensions | Card 104 high; title 60% width×16, date 40%×12, badge 72×24 |
| Colors | SurfaceMuted base; shimmer overlay 8% white/dark |
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

### Floating action button

| Property | Specification |
|---|---|
| Anatomy | Plus icon, optional short label on extended variant |
| Dimensions | 56×56 circular, icon 24; extended height 56 padding 20 |
| Colors | Primary background, onPrimary glyph, shadow-2 |
| States | Universal state contract applies; for non-input components error shows adjacent inline error, and loading only if action awaits completion. |
| Behavior | Opens Add item; position right 24 above tab bar 16; hide when keyboard shown |

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
Skip top-right → 3D calendar hero (280×280) → “Never miss a due date again.” (H1) → “Keep bills, tasks, and deadlines together.” (bodyM) → 4 progress dots (8 diameter, 8 gap) → Next 56 high.
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
App bar Duewise + settings → summary ring + counts → segmented All / Upcoming / Overdue / Completed → chronological due cards → FAB.
```

**Behavior:** Sort overdue oldest first, then due today, then next due; completed last when All selected; empty state uses Add item CTA.

### Add / edit due item

```text
App bar title + close → title input → category chips → due date → optional due time → recurrence → reminder selector → optional amount for bill/subscription → notes → sticky Save.
```

**Behavior:** Required: title 1–120 chars, category, date; date-only allowed; prevent duplicate save; unsaved changes confirmation.

### Calendar

```text
App bar month/year + month arrows → weekday row → 7-column month grid → selected-day agenda list → FAB.
```

**Behavior:** Tapping day updates agenda; week begins per locale; show overflow count +N if >3 items on a day.

### Item detail

```text
App bar back/edit/overflow → category/title/status → due date/time and timezone → recurrence → reminders → notes/amount if present → Complete or Reopen.
```

**Behavior:** Complete retains item in history; Reopen recalculates status; Delete requires confirmation and 5 s undo.

### Notification settings

```text
App bar → reminders master switch → preset reminder chips → quiet hours start/end → notification permission state → Test notification.
```

**Behavior:** No OS permission prompt until first opt-in; quiet hours default 22:00–08:00 local.

### Settings

```text
App bar → appearance System/Light/Dark → start of week System → timezone Automatic → notifications → export data → about/privacy.
```

**Behavior:** Appearance updates instantly; export uses system share sheet; deletion requires confirmation.

### Empty and error

```text
Centered icon/illustration → short heading → one explanatory sentence → primary retry/add CTA.
```

**Behavior:** Offline: local data remains accessible; sync error does not block adding; inline field errors retain input.

## 9. Interaction and Motion

| Token | Value | Use |
|---|---|---|
| `motion.duration-instant` | `80` | Duration |
| `motion.duration-fast` | `120` | Duration |
| `motion.duration-standard` | `200` | Duration |
| `motion.duration-enter` | `280` | Duration |
| `motion.duration-exit` | `180` | Duration |
| `motion.duration-emphasis` | `360` | Duration |
| `motion.easing-standard` | `cubic-bezier(0.2, 0, 0, 1)` | Curve |
| `motion.easing-enter` | `cubic-bezier(0, 0, 0, 1)` | Curve |
| `motion.easing-exit` | `cubic-bezier(0.3, 0, 1, 1)` | Curve |
| `motion.easing-spring` | `cubic-bezier(0.2, 1.2, 0.3, 1)` | Curve |

| Interaction | Motion | Haptic | Reduced motion |
|---|---|---|---|
| Push screen | Translate X 16→0 and fade 0→1, 280 ms enter | None | Instant cut or 80 ms fade |
| Pop screen | Translate X 0→16 and fade 1→0, 180 ms exit | None | Instant cut |
| Check complete | Check scale 0.8→1.08→1, 360 ms spring | Light success | Replace icon without scale |
| Swipe reveal | Track finger; settle 200 ms standard | Selection at threshold | Snap without tween |
| Overdue pulse | One border-opacity cycle 0.6→1 over 360 ms on screen entry; never loop | None | Static overdue border |
| Sheet enter | Y 24→0 and fade 0→1, 280 ms enter | None | 80 ms fade |
| Press | 80 ms overlay | Light on primary CTA only | Same static pressed overlay |

Use platform haptics only when supported and user-enabled; no vibration for passive reminders inside the app. Never animate background indefinitely.

## 10. Notifications and Status Logic

Deferred design proposal, not an implemented or approved v1 scheduler/status contract. The word "canonical" below applies within this future concept only. Earlier reminder proposals differ; reconcile them before approving a reminder feature.

**Canonical status algorithm:** evaluate using device-local current time in the item’s stored IANA timezone. A date-only due item is due at the end of its calendar day (`23:59:59.999`); a timed item uses its exact local date/time resolved to a UTC instant. For DST gaps, move to next valid local instant; for overlaps, choose earlier offset. Completed takes precedence over all statuses. `dueToday` means due calendar date equals today; `dueSoon` means due date is within the next 3 local calendar days excluding today; `upcoming` means 4+ days away; `overdue` means due instant is strictly earlier than now. If date is earlier than today, overdue regardless of stored time. Recalculate on app foreground, local midnight, timezone change, edit, completion, and notification delivery.

| State | Token | Label | Card cue |
|---|---|---|---|
| Upcoming | `colors.{mode}.info` | Upcoming | Calendar icon |
| Due soon | `colors.{mode}.dueSoon` | Due soon | Clock icon |
| Due today | `colors.{mode}.dueSoon` | Due today | Clock icon + bold date |
| Overdue | `colors.{mode}.overdue` | Overdue | Alert icon + 4 dp leading rule |
| Completed | `colors.{mode}.completed` | Completed | Check icon + strikethrough title |

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

Meet WCAG 2.2 AA: text contrast ≥4.5:1 (large ≥3:1); non-text state and focus cues ≥3:1; focus visible, logical, and never clipped. Interactive target minimum 44×44 pt iOS and 48×48 dp Android; spacing between adjacent smaller visual controls ≥8. Support VoiceOver and TalkBack, keyboard, switch access, and 200% text.

| Element | Screen reader label pattern |
|---|---|
| Due card | `{title}, {category}, {status}, due {fullDate}, {amount if any}. Button, opens details.` |
| Category chip | `{category}, {selected or not selected}, filter` |
| Date cell | `{weekday}, {fullDate}, {count} due items, {selected if selected}` |
| Reminder toggle | `Reminders, on/off, switch` |
| Complete action | `Mark {title} complete, button` |
| Progress ring | `{done} of {total} completed, {percentage} percent` |
| Bottom tab | `{tabName}, tab {index} of 4, {selected if selected}` |
| Snackbar | `Announcement: {message}. Undo available.` |
| FAB | `Add due item, button` |

**Focus order:** app bar → screen heading → primary content top-to-bottom → inline actions → floating action → tab bar; when modal opens, move focus to its heading, trap within, return to trigger on dismiss. Pair status color with icon and literal label. Do not rely on animation, haptic, or sound alone. RTL mirrors layout and directional navigation but not numerals, dates, clocks, checkmarks, or progress meaning; locale controls date format, first weekday, number grouping, currency and pluralization. Use IANA timezone IDs; never store date-only items as UTC midnight.

## 12. Design Tokens

This JSON is canonical. Keys are case-sensitive; consumers should generate platform constants from this object. Colors are opaque sRGB hex; shadow opacity is a unit interval; durations are milliseconds.

```json
{
  "colors": {
    "brand": {
      "indigo": "#4338CA",
      "blue": "#2563EB",
      "gradientStart": "#4338CA",
      "gradientMiddle": "#4F46E5",
      "gradientEnd": "#2563EB"
    },
    "accent": {
      "coral": "#E85D5D",
      "amber": "#B45309",
      "green": "#15803D",
      "info": "#1D4ED8"
    },
    "slate": {
      "100": "#F8FAFC",
      "200": "#F1F5F9",
      "300": "#E2E8F0",
      "400": "#CBD5E1",
      "500": "#94A3B8",
      "600": "#64748B",
      "700": "#475569",
      "800": "#334155",
      "900": "#1E293B",
      "950": "#0F172A"
    },
    "light": {
      "background": "#F8FAFC",
      "surface": "#FFFFFF",
      "surfaceRaised": "#FFFFFF",
      "surfaceMuted": "#F1F5F9",
      "text": "#0F172A",
      "textSecondary": "#475569",
      "border": "#CBD5E1",
      "primary": "#4338CA",
      "onPrimary": "#FFFFFF",
      "overdue": "#B42332",
      "dueSoon": "#92400E",
      "completed": "#166534",
      "info": "#1D4ED8",
      "focus": "#2563EB",
      "disabled": "#64748B"
    },
    "dark": {
      "background": "#0B1220",
      "surface": "#111C30",
      "surfaceRaised": "#1E293B",
      "surfaceMuted": "#25334A",
      "text": "#F8FAFC",
      "textSecondary": "#CBD5E1",
      "border": "#64748B",
      "primary": "#A5B4FC",
      "onPrimary": "#0F172A",
      "overdue": "#FDA4AF",
      "dueSoon": "#FCD34D",
      "completed": "#86EFAC",
      "info": "#93C5FD",
      "focus": "#93C5FD",
      "disabled": "#94A3B8"
    }
  },
  "typography": {
    "display": {
      "size": 36,
      "weight": 700,
      "lineHeight": 42,
      "letterSpacing": -0.7,
      "unit": "dp/sp"
    },
    "h1": {
      "size": 30,
      "weight": 700,
      "lineHeight": 38,
      "letterSpacing": -0.5,
      "unit": "dp/sp"
    },
    "h2": {
      "size": 24,
      "weight": 700,
      "lineHeight": 32,
      "letterSpacing": -0.35,
      "unit": "dp/sp"
    },
    "h3": {
      "size": 20,
      "weight": 600,
      "lineHeight": 28,
      "letterSpacing": -0.2,
      "unit": "dp/sp"
    },
    "bodyL": {
      "size": 18,
      "weight": 400,
      "lineHeight": 28,
      "letterSpacing": 0,
      "unit": "dp/sp"
    },
    "bodyM": {
      "size": 16,
      "weight": 400,
      "lineHeight": 24,
      "letterSpacing": 0,
      "unit": "dp/sp"
    },
    "bodyS": {
      "size": 14,
      "weight": 400,
      "lineHeight": 20,
      "letterSpacing": 0.1,
      "unit": "dp/sp"
    },
    "caption": {
      "size": 12,
      "weight": 500,
      "lineHeight": 16,
      "letterSpacing": 0.2,
      "unit": "dp/sp"
    },
    "button": {
      "size": 16,
      "weight": 600,
      "lineHeight": 24,
      "letterSpacing": 0,
      "unit": "dp/sp"
    },
    "overline": {
      "size": 11,
      "weight": 700,
      "lineHeight": 16,
      "letterSpacing": 1,
      "unit": "dp/sp"
    }
  },
  "spacing": {
    "space-1": 8,
    "space-2": 16,
    "space-3": 24,
    "space-4": 32,
    "space-5": 40,
    "space-6": 48,
    "space-7": 56,
    "space-8": 64,
    "space-9": 72,
    "space-10": 80,
    "space-11": 88,
    "space-12": 96
  },
  "radius": {
    "radius-xs": 4,
    "radius-sm": 8,
    "radius-md": 12,
    "radius-lg": 16,
    "radius-xl": 20,
    "radius-2xl": 24,
    "radius-pill": 999
  },
  "shadows": {
    "shadow-0": {
      "x": 0,
      "y": 0,
      "blur": 0,
      "spread": 0,
      "opacity": 0
    },
    "shadow-1": {
      "x": 0,
      "y": 2,
      "blur": 8,
      "spread": 0,
      "opacity": 0.08
    },
    "shadow-2": {
      "x": 0,
      "y": 6,
      "blur": 20,
      "spread": -2,
      "opacity": 0.12
    },
    "shadow-3": {
      "x": 0,
      "y": 12,
      "blur": 32,
      "spread": -4,
      "opacity": 0.16
    }
  },
  "motion": {
    "duration-instant": 80,
    "duration-fast": 120,
    "duration-standard": 200,
    "duration-enter": 280,
    "duration-exit": 180,
    "duration-emphasis": 360,
    "easing-standard": "cubic-bezier(0.2, 0, 0, 1)",
    "easing-enter": "cubic-bezier(0, 0, 0, 1)",
    "easing-exit": "cubic-bezier(0.3, 0, 1, 1)",
    "easing-spring": "cubic-bezier(0.2, 1.2, 0.3, 1)"
  }
}
```

## 13. Implementation Notes

The Dart/TypeScript structure and Flutter/React Native mappings below are illustrative design handoff material, not the repository architecture or a framework migration decision. Use the SDD for the actual .NET MAUI structure.

**Naming:** design tokens `colors.light.primary`, `typography.bodyM`, `space-3`, `radius-lg`, `shadow-2`, `motion.duration-standard`; Dart constants convert hyphens to camelCase, while TypeScript can preserve hyphenated keys. Components PascalCase (`DueItemCard`), screens PascalCase (`CalendarScreen`), hooks/functions camelCase (`useDueStatus`, `computeDueStatus`), assets kebab-case (`hero-reminders.webp`).

```text
src/
  design-system/
    tokens/design-tokens.json
    theme/light.ts
    theme/dark.ts
    typography/index.ts
    components/{Button,TextField,DueItemCard,StatusBadge,CalendarGrid}/
    icons/index.ts
    illustrations/
  features/{home,items,calendar,reminders,settings,onboarding}/
  domain/{due-status,recurrence,timezone,notifications}/
  accessibility/labels.ts
  localization/
```

**Flutter mapping:** `ThemeData`/`ColorScheme`, `TextTheme`, `ThemeExtension` for custom tokens, `MediaQuery.textScaler`, `SafeArea`, `Semantics`, `Material` + platform adaptive pickers. **React Native mapping:** typed token object, `useColorScheme`, `StyleSheet`, `useWindowDimensions`, `accessibilityRole`/`accessibilityLabel`, native date/time picker and OS notification APIs. Persist theme selection `system|light|dark`; `system` is default and follows OS changes without restart. Derive statuses from domain timestamps, not saved UI colors.

**Developer do:** use semantic theme colors; run contrast checks; test 200% type; respect safe areas; preserve local dates; handle denied notifications; provide Undo for deletion; verify both themes and RTL. **Developer don’t:** hardcode raw colors in components; set fixed text heights; use animated 3D in lists; auto-request permissions on launch; represent status by hue only; silently discard edits; promise exact OS notification delivery.

## Quality Checklist

This checklist spans the full design vision. Deferred-feature rows are not mandatory v1 gates; unchecked items are not verification evidence. Current acceptance procedures and applicability are in the [test plan](delivery/TEST-PLAN.md).

- [ ] Uses only canonical token names and values; no arbitrary colors, radii, or spacing.
- [ ] All normal text pairs pass ≥4.5:1; large text and control boundaries pass ≥3:1.
- [ ] Every tap target is ≥44×44 pt iOS / ≥48×48 dp Android.
- [ ] Works in light/dark/system mode without loss of hierarchy.
- [ ] Supports 200% type, keyboard focus, VoiceOver/TalkBack, and RTL.
- [ ] Respects safe areas, 8-pt spacing, and screen breakpoints.
- [ ] All controls implement applicable default/pressed/disabled/focused/error/loading states.
- [ ] Status uses text + icon + semantic color; date-only and timezone rules are correct.
- [ ] Save, edit, recurrence, complete, reopen, delete/undo, and offline flows work.
- [ ] Reminders obey permission, quiet hours, recurrence, and cancellation rules.
- [ ] Reduced-motion mode removes movement and indefinite animation.
- [ ] No decorative 3D is used in dense functional screens.
- [ ] No input is lost on validation, failure, back navigation, or permission denial.
- [ ] Widget protects private content and displays correct next-three items.
- [ ] All displayed dates, amounts, plurals, and notification copy are localized.
