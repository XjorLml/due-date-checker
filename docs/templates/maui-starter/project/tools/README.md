# Project automation

**Future-project template.** Add scripts only when there is a repeatable task. Record setup in [SETUP](../docs/SETUP.md), expected verification in [TEST-PLAN](../docs/delivery/TEST-PLAN.md), and observed results in [STATUS](../docs/delivery/STATUS.md).

## Check documentation

Requires Python 3.10+ and no pip packages. From the copied repository root:

```powershell
python tools/check_docs.py
python tools/check_docs.py --ready
```

The normal check allows unfinished template placeholders. Use `--ready` after customization; it fails on remaining `{{...}}` fields, including examples. Reusable templates are the exception: files named `TEMPLATE.md` and anything in a `feature-template` folder keep their placeholders, and the summary counts them. Their links are still checked, and copies you make from them (new names or folders) are enforced. Resolve decisions honestly or remove inapplicable examples with a reason. A structurally ready document can still have pending product approval, builds or device checks.

PASS means supported local paths and heading/HTML anchors resolve. Exit 0 = pass, 1 = documentation errors, 2 = invalid arguments/configuration. Failures include file/line and corrective action. Use `python tools/check_docs.py --help` for options; paths are relative to `--root`, which defaults to this copied project independently of the working directory. Example subset: `python tools/check_docs.py docs/README.md`.

The checker does not modify files, use the network, generate/build an app or access a device. External links and application routes are reported as not executed. Conventional inline/reference links, percent-encoded/angle-bracket paths, ATX/setext headings, duplicate headings and quoted HTML id/name anchors are supported. Fenced examples, comments and inline code are excluded. Raw HTML hrefs, arbitrary embedded Markdown, visual rendering and runtime behavior require separate review.

| Report | Action |
|---|---|
| Missing target or outside root | Correct the relative path, restore its target or copy the missing documentation. A copied project must not depend on the source repository. |
| Missing anchor | Match the actual heading/id and update inbound links after renaming. |
| Undefined reference | Add the matching link definition or use an inline link. |
| Unresolved placeholder | Customize the field or remove an inapplicable example; the original kit is expected to fail readiness. |
| No Markdown files / missing root | Pass the correct `--root PATH` or existing documentation paths. |
| Unsupported fragment/scheme | Use portable relative links or perform and document a manual check for unsupported targets. |

If the project later has JavaScript routes, add `tools/docs-routes.json` mapping relative HTML paths to exact inspected fragment lists. These declarations are not browser verification; unrelated missing fragments still fail. The default needs no manifest.

Run the reusable failure/recovery tests with:

```powershell
python -m unittest discover -s tools/tests -v
```

## Add automation when needed

Add repeatable build/test entry points only after actual projects and verified commands exist. Add native UI helpers only when their maintenance cost is justified.

Each helper should document its required host/tools, input arguments, selected target/device, preconditions, output/artifact location, exit behavior, cleanup, and known limits. Select devices explicitly; isolate test data and make cleanup scoped and repeatable. Keep logs/screenshots free of personal data and signing material. Preserve original settings before modifying them and verify restoration afterwards.

Use repository-relative paths or documented parameters. Stop on failed prerequisites. Never use a passing script exit as acceptance of scenarios it did not exercise. Do not copy the reference app's hardcoded emulator names, serials, database paths, or fixture assumptions.
