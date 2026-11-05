# Focus page content: pages and components aligned with the selected mockup

Authorized by the user on 2026-11-05 ("implement ui update for page and components") after the page-by-page comparison of the native app with the [Focus mockup](../../prototypes/design-exploration/index.html#a/dashboard). Scope: presentation of existing data and existing features. Release remains held. State and evidence: [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).

## Scope and governing contracts

[DESIGN 2.0.0](../../docs/DESIGN.md) owns tokens, components and screen specs; the [focus adoption spec](../focus-adoption/spec.md) adopted its palette, bar and editor and the [UI consistency spec](../ui-consistency/spec.md) its badge and single title. This spec adopts the mockup's page content where it can be built from data the app already stores. The [PRD F02 rules](../../docs/PRD.md#f02-dashboard-rules) own the Dashboard summary, including the new upcoming-total rule added with this slice; [F03](../../docs/PRD.md#f03-calendar-rules) and [F04](../../docs/PRD.md#f04-preference-rules) are unchanged in behaviour. [SDD §6](../../docs/SDD.md#6-ui-and-navigation) owns the shared row and segmented controls.

In scope:

- Dashboard: the Focus summary (upcoming known-amount total for the rest of the Manila month, unpaid count, next due), three status counters that act as quick filters, pill filter chips, the shared ledger row, and removal of the progress ring in favour of the Paid counter.
- Dues, Dashboard and Calendar: one shared ledger row (title, date · category, amount, status badge) replacing the boxed due card.
- Calendar: month heading with icon arrows and a Today action on one line, 44×48 day cells with count captions, agenda as a section.
- Settings: segmented System/Light/Dark and Monday/Sunday controls, a Reminders row that shows the saved reminder state, and a local-data notice.
- Details: title, large amount and badge above a facts list with dividers; timestamps as a closing caption.
- Refresh/Retry controls shown only after a failed load (review finding P2).

Out of scope and still deferred: Dashboard priority grouping ("Needs attention" / "A little further ahead"), which conflicts with the F02 single ordered result list; search and chips on Dues; the weekly Calendar; repeat, provider and catalog facts on Details; Manila-local timestamps on Details (separate finding); Reminders page copy; any schema, repository, reminder, sorting or status-rule change.

## Acceptance scenarios

| ID | Observable outcome | Verification boundary |
|---|---|---|
| FP01 | Dashboard shows "Upcoming · {month}" with the total of unpaid amounts dated from today to the end of the current Manila month, "{n} unpaid · today to {dd MMM}", and a "Next date" row naming the earliest unpaid due on or after today that opens its Details. With no such due the summary says so. | App tests on the view model; emulator with seeded dates. |
| FP02 | Three counters (Overdue, Today, Paid x/total) sit under the summary; tapping one selects that status filter and the list and heading follow. The progress ring is gone; the Paid counter carries the "x of y saved dues paid" description. | Emulator tree and taps; existing Dashboard tests. |
| FP03 | Dashboard, Dues and Calendar rows are divider-separated ledger rows at least 70 dp tall: title and "dd MMM · Category" on the left, amount and status badge on the right; a tap still opens Details. | Emulator tree and screenshots. |
| FP04 | Calendar shows the month name with ‹ › icon buttons and Today on one line; day cells are at least 44×48 with the count beneath the number; selected is primary fill, today is a primary outline; the agenda is a section headed by the selected date with the count caption. | Emulator; existing Calendar tests unchanged. |
| FP05 | Settings uses segmented controls for appearance and week start inside Appearance and Start of week groups, a Reminders row reading "On · HH:mm Manila" or "Off" from saved reminder settings, and a local-data notice; Save still applies both drafts together. | Emulator; existing preference tests unchanged. |
| FP06 | Details shows the title, the amount in the large numeral style, the badge, then Category, Due date and Notes as divided fact rows; Created and Last updated are a closing caption; Edit and Back keep their behaviour. | Emulator; existing Details tests unchanged. |
| FP07 | Refresh/Retry on Dashboard, Dues, Calendar and Settings is visible only while an error is shown; a successful load hides it. | Emulator error-free pass; code review of bindings. |
| FP08 | All of the above hold in light and dark and at 200% system text without clipped controls. | Emulator screenshots. |

## Evidence limits

An emulator run is agent-observed evidence on a virtual device, not phone acceptance. TalkBack, phone and intended-key signing gates remain separate and are tracked in [STATUS](../../docs/delivery/STATUS.md).
