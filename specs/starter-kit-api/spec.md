# Starter kit: client–server API add-on

Authorized: the user's toolkit review request and follow-up, 2026-11-09: future projects will use a C# stack (a .NET MAUI app plus its own ASP.NET Core API). The add-on covers the API gaps the review found and fixes the readiness check. It is documentation and developer tooling only: no Duewise product requirement, source or native behavior changes. [Plan](plan.md); [tasks](tasks.md).

Authorities: [AGENTS](../../AGENTS.md), [documentation ownership](../../docs/README.md), the [starter kit guide](../../docs/templates/maui-starter/README.md) and the earlier [toolkit scope DTK01–DTK07](../docs-toolkit/spec.md#scope-and-acceptance), which this extends without changing.

## Baseline (inspected 2026-11-09)

- **Kit model:** the kit models an offline app that stores its data on the phone in SQLite. Remote APIs appear in only two places: one sentence in the SDD template ("optional network/authentication boundaries") and one optional row in DEVELOPMENT.
- **Missing templates:** the kit had no template for environments, compatibility with released app versions, authentication, error mapping, retries, data encoding or offline behavior.
- **Setup, tests and release:** SETUP had no emulator networking guidance. TEST-PLAN had no network-failure procedures. RELEASE covered app packaging only.
- **Checker:** `check_docs.py --ready` reported every placeholder, including those in the reusable ADR, evidence and feature templates that the kit tells adopters to keep. In an isolated copy, 81 of 276 readiness errors came from those templates, so readiness could not pass without deleting them.

## Scope and acceptance

| ID | Observable outcome | Verification |
|---|---|---|
| KA01 | A copyable [API contract template](../../docs/templates/maui-starter/project/docs/API.md) owns client–server rules: environments, endpoint source of truth (committed OpenAPI document), compatibility and versioning, authentication, errors, timeouts/retries/duplicate prevention, data encoding, offline/conflicts and security. | Template review against checked official references |
| KA02 | Existing templates route to API.md instead of copying its rules: PRD scope row, SDD section, STRUCTURE client–server layout and contract-sharing tradeoffs, SETUP local API development, TEST-PLAN boundaries and procedures TP08–TP12, RELEASE backend deployment, DESIGN remote-data states, DEVELOPMENT practice, REFERENCES rows, documentation index, AGENTS and kit guide. | Link check and ownership review |
| KA03 | A project without a remote API can delete API.md. The instructions say how, and the checker reports every link that must then be removed. | Isolated copy with API.md deleted |
| KA04 | `--ready` keeps enforcing placeholders in customized files and copies, but skips placeholders in reusable templates (files named `TEMPLATE.md`, folders named `feature-template`). Their links are still checked and the skipped count is reported. | New checker tests; isolated copy |
| KA05 | Technical claims that depend on current platform behavior cite official pages checked on 2026-11-09. The guidance is labeled as a proposal: no app, API or device was built or run for it. | REFERENCES review |

## Limits

- **No code is generated or built.** No solution, API, CI workflow or package is added.
- **Not proven in a project:** retry, authentication, offline and deployment choices remain per-project decisions. The add-on is not evidence that they work in a project.
- **Duewise is unchanged:** its requirements, contracts, status and release gates do not change.
- **References are dated:** the external pages were read once on 2026-11-09 and must be rechecked when a project adopts them.
