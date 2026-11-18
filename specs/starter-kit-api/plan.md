# Starter kit API add-on: plan

Scope: [KA01–KA05](spec.md#scope-and-acceptance). [Tracking](tasks.md).

## Approach

1. **One owner per rule.** Add `project/docs/API.md` as the single home for client–server rules. Endpoint shapes stay in the API's generated OpenAPI document, which the project commits and reviews as a diff. Other templates link to API.md and add only what they own: setup values, test procedures, release steps, UI states.
2. **Optional, not default.** Every API addition is labeled "if the app uses an API", so local-only projects keep the original path. Deleting API.md is the opt-out. The checker then lists the links to remove.
3. **Compatibility first.** Released apps keep calling the API long after a new version ships, so compatibility rules, deploy order and an old-client test procedure are part of the baseline, not later hardening.
4. **Checked claims only.** Behavior that differs from common assumptions is stated with its source. Examples: the standard resilience handler retries every HTTP method by default; `NetworkAccess.Internet` does not prove the server is reachable; Android Auto Backup breaks restored `SecureStorage` values.
5. **Readiness fix.** Skip placeholder enforcement only for reusable templates, identified by name (`TEMPLATE.md`) or folder (`feature-template`). Copies get new names, so they are still enforced. Links in templates are still checked.

## Affected files

| Path (under `docs/templates/maui-starter/`) | Change | Scenario |
|---|---|---|
| `project/docs/API.md` | New template | KA01, KA03 |
| `project/docs/{PRD,SDD,SETUP,DESIGN,DEVELOPMENT,RELEASE,REFERENCES,README}.md`, `project/docs/delivery/TEST-PLAN.md`, `project/AGENTS.md`, `project/README.md` | Optional API rows/sections and links | KA02, KA05 |
| `STRUCTURE.md`, `README.md` | Client–server layout, sharing tradeoffs, adoption steps | KA02 |
| `project/tools/check_docs.py`, `project/tools/tests/test_check_docs.py`, `project/tools/README.md` | Template-aware readiness and its tests | KA04 |

Repository navigation: [feature catalog](../../docs/FEATURES.md) and the templates section of the [documentation index](../../docs/README.md). Historical records (kit REVIEW, November 8 audit) stay unchanged.

## Verification

Run the checker tests, the normal check on an isolated copy, `--ready` on that copy (expect failures only in customizable files), the copy with API.md deleted (expect only links to API.md), the full repository check, and `git diff --check`. No .NET build or device run: this is a documentation-only change ([AGENTS](../../AGENTS.md#documentation-governance)).
