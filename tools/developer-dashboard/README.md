# Developer dashboard

A local, read-mostly view of project progress beside the live HTML prototype. It reads the PRD, the spec folders and a few committed record files, then shows deterministic counts. It never calls an AI model, never runs tests on its own and is not part of the Android app. Scope, scenarios DD01–DD14 and evidence: [spec](../../specs/developer-dashboard/spec.md), [plan](../../specs/developer-dashboard/plan.md), [tasks](../../specs/developer-dashboard/tasks.md).

Requires Node.js 22 or later; no packages to install.

## Use it

```powershell
node tools/developer-dashboard/cli.mjs serve            # http://127.0.0.1:4317
node tools/developer-dashboard/cli.mjs status           # 15-line summary for you or a coding agent
node tools/developer-dashboard/cli.mjs feature f03-calendar
node tools/developer-dashboard/cli.mjs check            # validate config, mappings and records
node tools/developer-dashboard/cli.mjs help
```

The page has:
- **Overview cards:** verified gates, checked tasks, status counts, blockers, focus (read from STATUS) and the latest tests.
- **Feature list:** searchable and filterable.
- **Inspector:** acceptance criteria with their verification records, open tasks and a visual review.
- **Live prototype panel:** can be resized.
- **Activity cards:** AI sessions, Git, tests and data problems.

Editing a spec, task list or record updates the page within a second. Editing a prototype file reloads only the preview. If the prototype has an unsaved sample draft or an open dialog, the page shows a banner and waits for you to choose Reload.

## Where the numbers come from

| Shown | Source | Rule |
|---|---|---|
| Features | `docs/PRD.md` "Approved requirements" rows and every `specs/<folder>/spec.md` | Derived; nothing is typed twice |
| Gates (acceptance criteria) | Requirement rows; first-column IDs in spec tables (`DD01`, `PC03`, …) | References such as `R02` and `TP01` are skipped |
| Tasks checked | `- [x]` and `- [ ]` lines in each `tasks.md` | Workflow progress only, never verification |
| Phase, scope, prototype screen | [`records/mappings.json`](records/mappings.json) | Hand-kept; `check` validates every screen against the prototype's real IDs |
| Gate results | `records/verifications.jsonl` | Added only with `verify`, which needs existing evidence and an observer |
| Visual reviews, blockers | `records/reviews.jsonl`, `records/blockers.jsonl` | Committed, so they follow you between computers |
| Sessions, test runs | `.developer-dashboard/*.jsonl` | Machine-local and git-ignored |

**Status rules**, in order of precedence:
1. **Blocked:** an active blocker record.
2. **Verified:** every gate has a current passing record.
3. **Manual seed:** a status set in `mappings.json` with a source document. Only R01–R07 use this, seeded from STATUS "Release slices" on 2026-11-10.
4. **From tasks:** no task checked → **Backlog**; some tasks open → **In Progress**; all tasks checked → **In Review**.

**Percentages** count gates with a current passing record, divided by all gates in the approved scope. Optional, proposed, prototype, tooling and roadmap features are reported separately, not averaged in.

**Stale results:**
- A verification record carries the commit it was made at. If files it covers (`--files`, or the feature's `relatedFiles`) changed since then, that gate shows **Stale**.
- A visual review stores a hash of the prototype files and turns stale when they change.

## Record progress

Update the canonical document first (spec tasks, STATUS or an evidence file), then record a pointer to it:

```powershell
node tools/developer-dashboard/cli.mjs verify --feature f03-calendar --gate CA01 --result pass `
  --observer agent --evidence docs/delivery/evidence/FILE.md#section --env "Pixel 7 emulator, API 34" `
  --files src/DueDateChecker.App/ViewModels/CalendarViewModel.cs
node tools/developer-dashboard/cli.mjs block --feature R01 --reason "Waiting for the intended signing key"
node tools/developer-dashboard/cli.mjs unblock --feature R01
node tools/developer-dashboard/cli.mjs test --list
node tools/developer-dashboard/cli.mjs test dashboard   # runs one configured suite and records the result
```

- **Results:** `--result` is `pass`, `fail`, `blocked` or `not-run`.
- **Observer:** `--observer` is `agent`, `user` or `historical`.
- **Commit:** a historical record gets no current commit unless you pass `--commit`.
- **Rejected input** exits with code 2: an unknown gate, a missing evidence file or a bad date.
- **Append-only:** records are never edited in place. A later record wins, and a torn last line is reported in **Data problems** rather than deleted.

## AI sessions

The session panel never claims an agent is running. A recent signal reads "last signal 3 min ago, not confirmed running", and after 30 minutes without a signal it reads "stale". The threshold is in the config.

- **Claude Code:** the project's [`.claude/settings.json`](../../.claude/settings.json) runs `cli.mjs hook claude-code` on SessionStart, Stop and SessionEnd. The hook stores only the session ID, the event and the changed file paths. It never stores prompts, replies or transcript paths, prints nothing, and always exits 0. If hooks don't fire in a session that was already open when this file was added, open `/hooks` once or restart.
- **Codex (not verified):** Codex CLI documents a `notify` program that it runs after each turn, passing a JSON argument. To try it, add this to `%USERPROFILE%\.codex\config.toml` (use your own absolute path):

  ```toml
  notify = ["node", "D:/LEM/Projects/due-date-checker/tools/developer-dashboard/cli.mjs", "hook", "codex"]
  ```

  The CLI accepts that argument and keeps only its event type. Whether the VS Code Codex extension honors `notify` has not been checked.
- **Manual checkpoints:** use these for task, blocker and verification milestones, not for every edit:

  ```powershell
  node tools/developer-dashboard/cli.mjs session checkpoint --agent codex --task "F03 week view" --done "View model" --pending "Page"
  ```

## Prototype link

The prototype loads [`dashboard-bridge.js`](../../prototypes/design-exploration/dashboard-bridge.js), which does nothing unless the page is inside the dashboard iframe with `?embed=dashboard`.

**Messages:** they use the `duewise-devdash` channel, version 1.
- **Origin:** both sides accept only same-origin messages from the expected window.
- **Handshake:** the dashboard answers the bridge's `ready` with a random nonce, and every later message must carry it.
- **Message types:** dashboard → prototype `handshake`, `get-state`, `navigate`; prototype → dashboard `ready`, `state`, `navigation-result`.
- **Allowlisted targets:** views `explore`, `lab`, `decide`, `notify` and `compare`; the screens in `lib/config.mjs`; the `category-picker` overlay.

**Navigation:**
- The bridge calls the prototype's own `navigate()`, `loadHash()` and `phOpenPicker()`, so the unsaved-draft guard can still ask. If you choose "Keep editing", the dashboard reports *cancelled*.
- Details and Edit open the selected sample due, or the first active one. If none is left, the target reports *unavailable*.
- When you move around inside the prototype, the dashboard selects the matching feature only when exactly one feature maps to that screen. Shared screens show a "relates to" list instead of a guess.

## Security

- **Loopback only:** binds to 127.0.0.1 and rejects requests whose `Host` header is not loopback (DNS-rebinding protection).
- **Allowlisted files:**
  - the dashboard's own three files
  - the prototype folders, minus tests and dot-folders
  - Markdown under `docs/` and `specs/`, served as plain text
- **Never served:** `.git`, signing files, records and source.
- **One write:** a visual review. It needs the same origin, a per-process token embedded in the page, JSON input and schema validation.
- **Content policy:** the page allows scripts from its own origin only, with no inline scripts or styles.

## Test

```powershell
node --test tools/developer-dashboard/tests/*.test.mjs        # unit, CLI and HTTP tests (temporary fixture repos)
node tools/developer-dashboard/tests/browser-check.mjs        # Edge/Chrome headless end-to-end; EDGE_PATH overrides
```

The browser check uses the real prototype and specs but writes reviews and sessions to temporary copies of the records. Screenshots go to a temporary folder printed at the end.

## Limits

- **Coverage:** only scenario IDs that appear in spec tables become gates. A spec without such a table shows 0 gates and can never be Verified.
- **No per-boundary results:** gates are not yet split by verification boundary (automated, emulator, device, acceptance). A record's environment text says where it ran.
- **No frozen baselines:** scope baselines are not frozen or versioned yet. Adding a scenario to a spec changes the denominator immediately.
- **Full screen:** the browser check cannot confirm full screen in a headless browser; check that button by hand.
- **Records** are pointers to evidence, not the evidence itself. STATUS, specs and evidence files remain the authority.
