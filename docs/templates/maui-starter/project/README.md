# {{ProjectName}}

**Template for a future project.** Replace this notice after customization. Describe the application in one sentence: {{PurposeAndUsers}}.

Start with the [documentation index](docs/README.md). Follow [SETUP](docs/SETUP.md) for prerequisites and verified commands, and [AGENTS](AGENTS.md) for the coding-agent workflow.

## First use

This copy contains documentation and a checker; it does not contain a generated application. From this repository's root, with Python 3.10+:

```powershell
python tools/check_docs.py
```

Expected: local checks pass with draft placeholders allowed. Customize PRD → SDD/SETUP (and API, if the app calls a server) → DESIGN → first feature, then run `python tools/check_docs.py --ready`. Readiness intentionally fails while any `{{...}}` field remains outside the reusable templates; remove inapplicable examples with a reason. No server? Delete `docs/API.md` and the links the checker then reports. [Tools help](tools/README.md) covers errors, exits and limits. Resolve these steps before replacing this template notice with actual project state.

## Current state

{{Short scope summary with a link to the PRD; avoid copying its requirements.}}

See [delivery status](docs/delivery/STATUS.md) for implementation, verification, and acceptance. No application has been built or verified by copying this template.

## Contributing and distribution

Use [development standards](docs/DEVELOPMENT.md) and the [test plan](docs/delivery/TEST-PLAN.md). Distribution procedure and signing requirements belong in [RELEASE](docs/RELEASE.md). License: {{LicenseOrDecisionNeeded}}. Asset licenses must be recorded separately where required.
