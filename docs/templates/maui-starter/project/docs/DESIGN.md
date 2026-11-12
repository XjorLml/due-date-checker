# {{ProjectName}} UI and UX standards

**Template. Authority:** visual and interaction standards. [PRD](PRD.md) owns feature scope. [SDD](SDD.md) owns technical implementation. Status: {{DraftOrAdopted}}. Review date: {{Date}}.

## Scope notice

Applies to: {{Approved screens/platforms}}. Proposed concepts: {{Separate links or none}}. A mock, screenshot, or design proposal is not authorization to build a feature or evidence of accessibility compliance.

## Visual system

| Role | Value / semantic resource | Rationale and verification |
|---|---|---|
| Brand identity | {{Name, app icon, splash, assets}} | {{Ownership/licenses and readability}} |
| Color | {{SurfaceTextPrimaryError roles in light/dark}} | {{Contrast evidence and non-color cues}} |
| Typography | {{Bundled/system fonts; heading/body/caption roles}} | {{Licenses, fallback, text scaling}} |
| Layout | {{Spacing, content widths, touch targets}} | {{Smallest supported view and resizing}} |
| Motion | {{Focus/pressed/loading feedback}} | {{Reduced-motion handling where supported}} |

Centralize semantic resources. Document exceptions instead of allowing each page to invent colors, spacing, and status symbols. Use logical layout units and flexible dimensions; test the chosen sizes on actual targets.

## Screen and component states

| Component / workflow | States to design | Observable behavior |
|---|---|---|
| {{List/search}} | Initial loading, ready, empty, no match, initial error, stale data | {{What is visible and retryable in each state}} |
| {{Editor}} | Pristine, draft, validation error, saving, failure, committed | {{Input preservation, focus, enabled actions, return}} |
| {{Native capability}} | Available, not granted, denied, unsupported | {{Explanation and appropriate next action}} |
| {{Remote data, if an API is used}} | Loading, offline, slow, session expired, server error, saved but not yet sent (offline writes only) | {{What stays visible and editable; how the outcome is reported; see API errors}} |

Link business rules to the PRD instead of redefining them. Preserve list identity/focus where possible during refresh/filtering. Ensure the keyboard does not hide required actions. Avoid nested scroll ownership that makes controls unreachable.

## Accessibility, responsive layout, and localization

- Provide meaningful text labels, focus order, status announcements, and text equivalents for color/icon states. Test native dialogs and custom controls with the target screen reader.
- Specify the supported font-scaling range and test its maximum. Include light/dark/system theme changes, portrait/landscape, safe areas, keyboard, and the narrowest supported size.
- For desktop targets, define keyboard traversal/shortcuts, pointer behavior, resize limits, and multi-window scope.
- Put localizable UI and accessibility text in resources when localization is selected. Handle plurals and text expansion; use typed values for logic rather than translated display labels.
- Separate display culture from storage encoding and the PRD's timezone/currency policy. Include RTL layout and locale-sensitive input only when supported, with explicit verification.

## Verification and exceptions

Procedures: [TEST-PLAN](delivery/TEST-PLAN.md). Observed results: [STATUS](delivery/STATUS.md). Record {{KnownGap, affected platform/component, owner, next check}}. Native controls and compiled XAML alone do not establish compliance.
