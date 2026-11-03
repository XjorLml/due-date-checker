# Project automation

**Future-project template.** Add scripts only when there is a repeatable task. Record setup in [SETUP](../docs/SETUP.md), expected verification in [TEST-PLAN](../docs/delivery/TEST-PLAN.md), and observed results in [STATUS](../docs/delivery/STATUS.md).

Useful initial utilities are Markdown path/anchor validation and repeatable build/test entry points. Add native UI helpers only when their maintenance cost is justified.

Each helper should document its required host/tools, input arguments, selected target/device, preconditions, output/artifact location, exit behavior, cleanup, and known limits. Select devices explicitly; isolate test data and make cleanup scoped and repeatable. Keep logs/screenshots free of personal data and signing material. Preserve original settings before modifying them and verify restoration afterwards.

Use repository-relative paths or documented parameters. Stop on failed prerequisites. Never use a passing script exit as acceptance of scenarios it did not exercise. Do not copy the reference app's hardcoded emulator names, serials, database paths, or fixture assumptions.
