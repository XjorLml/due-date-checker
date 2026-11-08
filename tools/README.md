# Developer tools

Start from the repository root. Choose the tool for your task; the native runners have device/fixture preconditions and must not be used as a general health check.

| Task | Entry point | What it does |
|---|---|---|
| Check documentation | `python tools/check_docs.py` | Read-only local path/anchor checks; no network, application build or device access |
| Reuse/customize documentation for another app | [Starter kit](../docs/templates/maui-starter/README.md) | Copyable documentation, adoption workflow and the same portable checker |
| Prepare a draft release | [Release procedure](../docs/delivery/GITHUB-RELEASE.md) and [helper source](release/release.py) | Explicit package/signature validation and draft-release workflow; follow the procedure before invoking helpers |
| Verify Android behavior | [Native verification guide](verification/README.md) | Task-specific, guarded fixtures; read target/checkpoint/cleanup requirements first |
| Review the developer dashboard idea | [Proposal](../specs/developer-dashboard/spec.md) | Proposed only; no dashboard server or progress CLI exists yet |

## Documentation checks

Requires Python 3.10+ with only its standard library. From the root:

```powershell
python tools/check_docs.py
```

Expected result: PASS plus document/local-link counts. Exit 0 means supported checks passed, 1 means documentation errors, and 2 means invalid arguments or route configuration. Every error gives a file/line and next action. External URL counts and declared JavaScript routes are explicitly **not executed**; this is not a browser, acceptance or current external-policy check. No files are modified.

To inspect a subset or command options:

```powershell
python tools/check_docs.py docs/README.md docs/FEATURES.md
python tools/check_docs.py --help
```

If a target moved, update its inbound links and migration record. For a missing heading, correct its fragment or restore the heading; do not add aliases that redefine product contracts. Application routes must be inspected in their router before adding exact fragments to [docs-routes.json](docs-routes.json); an unknown HTML fragment fails instead of being assumed to work.

The implementation and its reusable tests live in the [copyable kit](../docs/templates/maui-starter/project/tools/README.md), so there is one checker source to maintain. The root script only chooses this repository. Run the meaningful checker tests with:

```powershell
python -m unittest discover -s docs/templates/maui-starter/project/tools/tests -v
```

Default coverage includes existing README/AGENTS, docs, specs, tools, prototypes and .vibe-wise Markdown; generated/artifact directories are excluded. Draft kit placeholders are allowed. Use `--ready` on a **customized destination kit**, not the source repository containing intentionally unfinished templates.

Limits: conventional inline/reference Markdown links, ATX/setext heading anchors, duplicate headings and quoted HTML ids/names are supported. Fenced code, comments and inline code are not treated as links. Raw HTML href attributes, external redirects, visual rendering, arbitrary embedded Markdown syntaxes and JavaScript behavior need separate review. A structural PASS does not establish requirements coverage or product acceptance.

## Maintenance and safety

Document a helper's inputs, prerequisites, effects, outputs, exits, cleanup and limits before normal use. Preserve selected-device/fixture guards and original settings. Keep signing data, personal financial data and raw device captures out of logs and version control. [TEST-PLAN](../docs/delivery/TEST-PLAN.md) owns expected native outcomes; [STATUS](../docs/delivery/STATUS.md) links observed evidence.
