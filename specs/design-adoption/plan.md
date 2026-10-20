# Implementation plan — design adoption

[Spec](spec.md) defines scope and DA01–DA10; [tasks](tasks.md) tracks execution.

## Modules and sequence

1. Replace template colors/styles with semantic light/dark resources and reusable typography, card, field, error and button styles. Native theme bindings update existing screens without a restart.
2. Bundle static Plus Jakarta Sans weights and upstream license/provenance. Register aliases in MauiProgram. Replace the launcher/splash SVG identity and display name, retaining the ApplicationId and storage location.
   Resolve AppShell in App.CreateWindow, after App.InitializeComponent has loaded resources. Constructor-injecting Shell eagerly constructs the list before shared StaticResource keys exist; a passing build alone does not catch this startup order.
3. Restyle DuesListPage with a scrollable header, responsive card stack, wrapped amounts/dates, explicit Paid/Unpaid badge, and existing loading/error/empty/retry states. Keep CollectionView as the scrolling owner and its existing selection handler.
4. Restyle DueEditPage as a single-column labeled form. Use focus/error borders, unchanged native date selection, amount preview and payment-on-Save. Add blur validation using the same view-model rules and focus the first invalid control after submit.
5. Apply shared responsive margins/max-width through a small page-layout helper; avoid display-scale heuristics or a new framework.
6. Run the existing Core and App suites sequentially (shared Core output), add one meaningful blur/no-write regression, and build Android Release per [TEST-PLAN](../../docs/delivery/TEST-PLAN.md#automated-execution). Check native themes/large text and basic workflows when a phone is available. Building locally is verification; release remains on hold.
7. Record source/artifact identity, observed results and remaining acceptance. Review PRD/SDD/DESIGN and update their links without duplicating their contracts.


## Dependencies and risks

No production package or database migration is required. Fonts are pinned to an upstream source commit and included locally with their license. Long titles, maximum PHP amounts, keyboard/autofill behavior, source-generated XAML styles, scroll-header binding, native themes, and Release trimming require boundary checks. Android update installation must preserve data; signing mismatch must not be worked around with uninstall/data clear.

## Review checkpoint

Review semantic resources first, then list/editor bindings and error/focus behavior. Styles encapsulate presentation; existing repository/view-model boundaries continue to own data behavior. The user has authorized implementation; review/explanation is not a new permission gate or evidence of learner understanding.
