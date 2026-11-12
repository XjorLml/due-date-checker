# Starter kit API add-on: tasks

Authorized 2026-11-09. [Scope](spec.md); [plan](plan.md).

- [x] Inspect the kit, its checker/tests and the working tree; record the baseline and readiness defect.
- [x] Check current official guidance for the platform-dependent claims (KA05).
- [x] Add the API contract template (KA01).
- [x] Route existing templates and the kit guide to it (KA02, KA03).
- [x] Make `--ready` skip reusable templates and add tests (KA04).
- [x] Validate: checker tests, isolated copy (normal, `--ready`, API.md deleted), full repository check, diff.
- [x] Update the feature catalog, documentation index and learning notes; record evidence below.

## Evidence

**Environment:** 2026-11-09, agent-observed on Windows (Git Bash/PowerShell).
- Python is not on this PC's PATH, so the checker ran under the Python 3.13.12 bundled with pgAdmin 4. No packages were installed.
- Source baseline: `develop` at `2cc92c1`, with a clean working tree before this task.

**Checker results:**

| Check | Observed result |
|---|---|
| Readiness defect reproduced | Before the fix, an isolated kit copy failed `--ready` with 276 errors; 81 came from the ADR, evidence and feature templates. The new test failed first, with 4 errors instead of 2. |
| Checker tests after the fix | 10 of 10 passed, in the kit and again inside an isolated copy |
| Isolated copy, normal check | PASS: 20 Markdown files, 116 local links, 0 errors |
| Isolated copy, `--ready` | Exit 1 as intended: 283 placeholders in customizable files; 81 kept in reusable templates, none flagged |
| Isolated copy with `docs/API.md` deleted | Exit 1 with exactly 10 errors, each a link to API.md that the no-API path must remove (KA03) |
| Full repository check | PASS: 178 Markdown files, 2,055 local links, 0 errors (baseline before the change: 174 files, 2,011 links, 0 errors) |
| `git diff --check`; new-file whitespace | Clean |
| Reference-app identity in the copyable tree | No new mentions. The only match is the existing PRD warning not to inherit Manila rules. |

**References (KA05):** two web-fetch subagents read the 11 official pages cited in [REFERENCES](../../docs/templates/maui-starter/project/docs/REFERENCES.md#remote-api-and-server) on 2026-11-09. Corrections adopted from that check:
- The standard resilience handler retries every HTTP method by default.
- EF Core advises careful consideration, rather than prohibition, for migrating at startup.
- The rate limiter's default rejection status was not confirmed, so the template says to set it explicitly.
- The errors page links both RFC 7807 and RFC 9457.

**Cleanup:** the test runs created `__pycache__` files under the kit's `tools/`. They were deleted after validation.

**Not done:** no .NET build, ASP.NET Core project, emulator, device or CI run, as the spec's limits state. External links were not crawled beyond the pages read.
