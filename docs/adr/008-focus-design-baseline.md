# ADR 008: Focus design baseline and native Add action

Status: accepted for the authorized Focus adoption slice. Decision date: 2026-11-05.

## Context and provenance

Between 2026-11-04 and 2026-11-05 the user reviewed an HTML [design exploration](../../prototypes/design-exploration/index.html) with four directions, selected Minimal Fintech, then its Focus refinement with a centred Add action, then sticky bottom actions with context-aware navigation and the Essential-first editor. Each selection was recorded as HTML-only at the time (see the [DESIGN register](../DESIGN.md#design-explorations-non-normative)). On 2026-11-05 the user asked to implement the new design natively and chose the scope "look and navigation" from three offered options, declining the catalog, repeats, weekly Calendar, totals and multi-offset reminders for this slice. The [focus adoption spec](../../specs/focus-adoption/spec.md) owns the scenarios.

The historical project map does not reserve a proposed ADR 008; this number is new. [ADR 007](007-local-reminder-reconciliation.md) remains the only earlier record.

## Decision

1. DESIGN 2.0.0 replaces the 1.0.0 indigo/slate baseline with the Focus tokens: ink primary, lime accent, warm-grey canvas, 26/20/16 headings, 14 dp radius, 4-unit spacing, 48 dp actions and fields, 3 dp focus outline. Resource keys in Colors.xaml are kept so page markup is unchanged.
2. Navigation keeps .NET MAUI Shell. The bar has five slots in the prototype order Dashboard / Calendar / Add / Dues / Settings. Dues remains the start tab. Settings becomes a root tab. The centre slot is a disabled placeholder; the Android shell renderer wraps the native bar in a small FrameLayout that floats a 56 dp lime disc over that slot and hides it whenever Shell hides the bar.
3. The editor orders title, amount and date first, fixes Cancel/Save in a footer below the scrolling fields, and moves Delete to a header toolbar action.

## Rationale

- Shell owns routes, per-tab stacks, hidden-bar pages and the reminder tap router. Rebuilding navigation to float one button would rewrite every SDD §6 contract for a visual gain.
- Android's BottomNavigationView tints every item icon with one tint list, so a lime icon on a real fifth tab would render ink-coloured. A native overlay is the only way to get the prototype's untinted disc without a third-party control, and the project already had an Android renderer that owned the bar's height.
- A fifth real item reserves the centre width and provides the "Add" caption with Material's own metrics; disabling it natively plus cancelling its selection in `Shell.Navigating` keeps it from ever becoming a tab.
- The user's prototype selections were explicit and repeated; the rationale for Focus over Essential and Soft cards is in the prototype [system guide](../../prototypes/design-exploration/SYSTEM.md#live-recommendations-within-minimal-fintech) and [audit](../../prototypes/design-exploration/AUDIT.md).

## Alternatives considered

- **Custom root page with a hand-built bar.** Full control of the disc, but loses Shell routing, Back handling and the tap router; rejected for scope.
- **Fifth real tab styled as the disc.** Simplest, but the icon is tinted and sits inside the bar instead of raised; rejected for fidelity.
- **Right-aligned MAUI FAB inside each page.** Matches DESIGN 1.0.0's FAB, not the selected centred dock; rejected because the user selected the centred action.
- **Adopt the whole mockup (catalog, repeats, weekly Calendar).** Product scope changes needing PRD requirements and migrations; deferred by the user's scope choice.

## Consequences

- PRD F04's "Settings is reachable from Dues and hides the bottom tabs" and "no new bottom tab" wording is superseded by an explicit exception; Settings is still reachable when dues storage fails because it is a Shell root.
- The renderer assumes the bar's parent is a ViewGroup; if a MAUI update changes Shell's Android layout the disc silently disappears and the bar shows a disabled "Add" caption. Verify after upgrades.
- Emulator evidence is new to this project and does not replace phone acceptance. TalkBack review of the disc remains deferred by user direction.
- Indigo assets remain in git history only; `brand_mark.svg`, icon and splash now use ink.

## Links

[Spec](../../specs/focus-adoption/spec.md) · [Plan](../../specs/focus-adoption/plan.md) · [Tasks](../../specs/focus-adoption/tasks.md) · [DESIGN 2.0.0](../DESIGN.md) · [PRD exception](../PRD.md#exclusions-and-deferred-capabilities) · [SDD §6](../SDD.md#6-ui-and-navigation)
