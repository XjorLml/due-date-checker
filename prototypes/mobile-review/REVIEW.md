# Duewise mobile UI review

Design study dated **2026-11-04**. Open [the HTML prototype](index.html) directly in a modern browser; no installation, build or connection is required. Start at **Your dues**, use the screen explorer to review each page, and choose between its two live layout alternatives. Add notes and use **Export my choices** to download the decisions as Markdown.

The prototype follows the [DESIGN scope and native adaptations](../../docs/DESIGN.md#scope-and-authority), [PRD](../../docs/PRD.md) and [SDD navigation contracts](../../docs/SDD.md#6-ui-and-navigation). It is a local, sample-data design artifact. The production MAUI interface has not been changed. Selecting an alternative is a review preference, not approval to implement it. Implementation and verification of this artifact are tracked in the [prototype spec](../../specs/mobile-ui-prototype/spec.md), [plan](../../specs/mobile-ui-prototype/plan.md) and [tasks](../../specs/mobile-ui-prototype/tasks.md).

## Direction

Recommend a calm, information-first interface: Duewise indigo, locally bundled Plus Jakarta Sans, semantic light/dark colors, explicit PHP amounts, readable dates, restrained outlined cards and three labeled bottom destinations. The principal finance task is remembering obligations and recording payment state. Avoid presenting this as a banking app: there is no bank balance, money transfer, payment processing or spend analysis.

The recommended starting selections are cards, compact Dashboard, month with agenda, amount-first Details, flowing Add/Edit forms, visible Settings choices, progressive Reminders and minimal future onboarding. These are design judgments from the source review, not usability-study findings. Test them with intended users before adopting production changes.

## Page-by-page choices

| Page | A — recommended starting point | B — alternative | Why choose A / when to consider B |
|---|---|---|---|
| Dues | Calm cards | Ledger rows | Cards give title, amount, explicit date and payment state separate lines. Consider rows for frequent users with many dues; retain wrapping and avoid tiny type. |
| Dashboard | Compact paid progress | Large progress ring | Compact presentation leaves more space for search and actual dues. The ring emphasizes progress but takes more vertical space. |
| Calendar | Month + selected-day agenda | Agenda with collapsible month chooser | The full grid gives useful date context. Collapse it when a long selected-day agenda is the primary task. Both keep month selection and exact item counts. |
| Details | Amount first | Title first | Amount-first suits bills. Title-first may suit task-heavy use; an automatic category-dependent hierarchy would be a further design decision, not a behavior this prototype applies. |
| Add due | One flowing form | Grouped form | A short form needs little structure. Groups can help orientation if observation shows people lose their place. |
| Edit due | Same flowing form as Add | Grouped form | Preserve a familiar sequence. Whichever direction is chosen, use it for both Add and Edit; the review lets you compare them independently. |
| Settings | Visible segmented choices | Labeled dropdowns | Visible options make a small fixed choice set discoverable. Native dropdowns may work better at large text scales; validate actual MAUI semantics and wrapping. |
| Reminders | Progressive disclosure | Expanded controls | Put schedule and explicit enable/save first; disclose Android diagnostics when needed. Expanded controls are useful during troubleshooting. |
| Onboarding, future concept | Quiet icon introduction | Layered vector illustration | Four brief messages are easier to read with less decoration. The warmer option is a flat visual study, not the original 3D asset treatment. |

### 1. Dues — R02, R03, R05, R07

Inspected [DuesListPage.xaml](../../src/DueDateChecker.App/Pages/DuesListPage.xaml): Settings, Reminders, supporting copy, Add and Refresh precede the first saved card. Recommend moving the two occasional settings entries to accessible header actions, using a + icon for Add due and a quiet refresh icon. Header icon buttons have accessible names and desktop tooltips; validate whether visible text is preferable in native user testing.

Cards keep the amount on its own line and pair Paid/Unpaid with a symbol. Retain oldest date first, including paid rows; neither layout reorders payments to the end. Tapping a row opens Details. No swipe-only action is introduced. Empty, loading, read-error and stale-data previews are available through the review toolbar. An initial storage error offers Settings as well as Retry.

### 2. Dashboard — F02 and F07 categories

Inspected [DashboardPage.xaml](../../src/DueDateChecker.App/Pages/DashboardPage.xaml) and the [Dashboard tests](../../tests/DueDateChecker.App.Tests/DashboardTests.cs). The existing vertically arranged summary is information-rich, but its large header and controls delay results. Recommend a compact paid fraction, global unpaid date counts, labeled title search, wrapping status chips and a labeled single category selector.

The summary remains global even when results are filtered. Paid takes precedence; Upcoming excludes Today. All query/status/category conditions combine. A no-match message offers Clear filters and differs from no saved dues. Add works from this page. Do not call these counts monthly spending, cash flow or payment history; the approved data does not establish those measures.

### 3. Calendar — F03 and F04

Inspected [CalendarPage.xaml](../../src/DueDateChecker.App/Pages/CalendarPage.xaml), [CalendarViewModel](../../src/DueDateChecker.App/ViewModels/CalendarViewModel.cs) and [Calendar tests](../../tests/DueDateChecker.App.Tests/CalendarTests.cs). Keep a six-row month and a separate agenda. Put item counts on dates, explain that paid dues are included, and differentiate Today’s outline from the selected date’s fill.

The saved Monday/Sunday choice changes headers and offsets together. Month arrows select day one; Today restores **04 Nov 2026**, the fixed prototype clock. Selecting a due opens Details. Add still defaults to Today. The calendar has a bounded horizontal scroller on widths too small to fit seven 44-pixel day targets; normal phone widths fit directly. A native narrow-width or large-text adaptation needs device review rather than silently shrinking targets.

### 4. Details — F01

Inspected [DueDetailPage.xaml](../../src/DueDateChecker.App/Pages/DueDetailPage.xaml) and [Details tests](../../tests/DueDateChecker.App.Tests/DueDetailTests.cs). Separate the main obligation, date/payment facts, optional notes and lower-emphasis UTC timestamps. Keep one pencil icon action in the header for Edit; this screen does not mark paid or delete directly.

For finance use, a prominent amount helps retrieval. For tasks, preserve literal **No amount** and never substitute zero. Long titles and large amounts wrap. Back returns to the original Dues, Dashboard or Calendar context. Confirmed deletion through Edit leads to unavailable Details before returning to the parent, preserving the existing contract.

### 5. Add and Edit — R02, R04–R06, F07 optional amounts

Inspected [DueEditPage.xaml](../../src/DueDateChecker.App/Pages/DueEditPage.xaml), [editor tests](../../tests/DueDateChecker.App.Tests/EditDueTests.cs) and [Money tests](../../tests/DueDateChecker.Core.Tests/MoneyTests.cs). Recommend persistent labels, a decimal keyboard hint, category-aware amount requirements, and an exact **Will save as ₱…** preview. Bill/Subscription require an amount; Task/Other allow absence. Zero, centavo rounding and large supported values have separate handling.

Blur and Save validate without discarding input. Save moves focus to the first invalid field. The paid checkbox appears only for existing dues and commits only on Save. Keep a compact Save button and a separate trash icon in a persistent bottom action bar, with a named deletion confirmation. Cancel remains in the header and preserves the stored sample. The action bar is outside the form scroll owner so Save stays reachable; keyboard and 200% system-text behavior still need native verification. Recurrence, due time, unsaved-change prompts and Undo remain deferred.

### 6. Settings — F04

Inspected [SettingsPage.xaml](../../src/DueDateChecker.App/Pages/SettingsPage.xaml) and [preference tests](../../tests/DueDateChecker.App.Tests/AppPreferencesTests.cs). Compare visible choices against the current native picker pattern. Limit the page to System/Light/Dark, Monday/Sunday and the working Reminders entry. Keep the explanation and **Save preferences** action close to these controls.

Selection is a draft: neither theme nor week start applies until Save. Opening Reminders and returning retains that draft; leaving Settings discards it. Saved prototype preferences last only for this browser session, deliberately separate from the persistent review choices. The studio Light/Dark toolbar is an immediate visual inspection override. PHP and Asia/Manila are contextual information, not nonfunctional selectors. Exported review decisions are separate from the deferred product financial-data export feature.

### 7. Reminders — F05

Inspected [ReminderSettingsPage.xaml](../../src/DueDateChecker.App/Pages/ReminderSettingsPage.xaml) and [reminder tests](../../tests/DueDateChecker.App.Tests/ReminderTests.cs). Group the due-date schedule, optional day-before reminder, global time and quiet hours first. When off, make **Save and enable reminders** the principal action and provide Save without enabling. When enabled, show Save schedule and a secondary Disable.

Keep delivery readiness separate from opt-in. Place notification permission, battery/background information, Android settings, test and retry inside an expandable section in the recommended variant. The HTML explicitly simulates these platform interactions; it reads no Android state and schedules nothing. Notification previews remain generic. Do not promise delivery, introduce per-due settings or hide the quiet-hours boundary behavior.

### 8. Onboarding 1–4 — F06 future concept

All four screens from [DESIGN §8](../../docs/DESIGN.md#8-screen-specifications) can be stepped through: welcome, adding dues, reminders and calendar/progress. Next, Previous, Skip and Get started work in the sample. The introduction returns to Dues and stores no first-run flag. This is an exploratory adaptation; production onboarding remains deferred.

Recommend honest, brief copy rather than “never miss” claims. Describe current categories, local data and optional date-only reminders; ask for permission only at explicit opt-in. Neither illustration option implies account, synchronization, recurrence or a bank connection.

### 9. Shared empty/error states and navigation — F08

The toolbar can preview empty, loading, initial read-error and stale-data states on Dues, Dashboard, Calendar and Details. Details’ empty case is an unavailable target; Calendar’s empty case is an empty selected-day agenda. Retry returns to the normal fixture. These visual modes do not delete sample records. Form validation and confirmed deletion are interactive, rather than toolbar-only pictures.

Preserve Dues/Dashboard/Calendar tabs and their retained state. Hide tabs in pushed screens. Do not show a blocking offline screen: local dues are usable without internet. Empty-state actions must explain what changes next. A retry is for a failed read, not a generic connection warning. Production write failures, real asynchronous loading, startup failures and OS lifecycle behavior are outside this HTML simulation.

## Cross-page mobile checks

- Use DESIGN body/heading/caption roles within the phone, compact actions with a 44-pixel minimum touch target in this user-directed revision, visible focus, text status labels and wrapping content. The review studio itself uses denser desktop annotation typography.
- Keep amount presentation consistent and avoid using a dramatic colored amount as an urgency cue. Status text supplies urgency while the number remains readable.
- Test keyboard occlusion, Android Back, TalkBack order, focus after returning from dialogs and 200% system text on the real MAUI pages before adopting a layout. Browser automation does not establish those outcomes.
- The layout uses browser pixels as reference units; it is not a direct MAUI control implementation. Calendar count annotations are compact secondary cues with full date/count accessible labels. App icon, inline SVGs and onboarding artwork are handmade placeholders, not exported production branding or a pinned Lucide asset set.

The choice of persistent labels and appropriate input modes is consistent with Apple’s [Entering data](https://developer.apple.com/design/human-interface-guidelines/entering-data) and [Text fields](https://developer.apple.com/design/human-interface-guidelines/text-fields) guidance. Touch-target review is informed by W3C’s [Target Size (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html); the prototype targets the larger 44/48-pixel conventions in DESIGN where possible. These references support the interaction principles, not a claim that this study meets every accessibility criterion.

## Compact actions revision ? 2026-11-04

The user requested smaller buttons and icons to reduce repeated scrolling. Add now uses a persistent header **+**, Details uses a **pencil**, and Refresh uses a quiet **refresh** icon. Icon-only actions have accessible names and tooltips, with 44 ? 44-pixel touch targets. Main tabs retain their text labels.

Save is a content-width icon-and-label button in a compact bottom bar on Add/Edit, Settings and Reminders. It stays visible while content scrolls. Cancel remains in the header; Edit places a separate trash icon in the action bar. Reminder opt-in retains **Save & enable**, alongside **Save only**; disabling uses a named bell-off icon. Diagnostic actions use compact icon-and-label rows rather than stacked outlined buttons. The surrounding review/export controls are separate from the app layout.

This user-directed prototype revision takes precedence over the earlier study's 56-pixel full-width action recommendation. Production DESIGN action tokens remain unchanged. Confirmation, payment-on-Save, draft cancellation and reminder permission simulation retain the same behavior. The first due starts 180 pixels below the scroll area's top at a 390-pixel viewport in the observed browser check; Save remains inside the phone bounds at both scroll extremes. Native keyboard, TalkBack and large-system-text acceptance remain unperformed.

## Data, verification and limits

All displayed financial records are invented. Demo dues, payment state, deletion, reminders and app preferences are held in memory and reset on reload. Only design choices and reviewer notes are stored in browser local storage; if storage is unavailable, export still works and the interface reports the limit. No external fonts, analytics, services or packages are loaded. The bundled fonts retain their [OFL license](assets/PlusJakartaSans-OFL.txt).

Run `node --test prototypes/mobile-review/tests/model.test.cjs` for amount/date/filter checks. Run `node prototypes/mobile-review/tests/browser-check.cjs` for installed Edge/Chromium checks; set `EDGE_PATH` if necessary. The browser check starts and stops a loopback-only static server and a headless browser. It retains ignored disposable profiles and screenshots under `.qa/` for inspection. Recorded results belong in [tasks](../../specs/mobile-ui-prototype/tasks.md); test definitions are not passing evidence. No APK build or production test result is claimed for this artifact.
