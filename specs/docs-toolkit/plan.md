# Documentation/toolkit implementation plan

Scope: [DTK01–DTK07](spec.md#scope-and-acceptance). Authorized 2026-11-08; no additional approval gate. [Tracking](tasks.md).

## Inspected baseline

The existing reusable toolkit is [the MAUI documentation kit](../../docs/templates/maui-starter/README.md), with copyable templates under project/. It is not an application generator or runnable dashboard. [The dashboard](../developer-dashboard/spec.md) remains a separate unapproved implementation proposal. Executable release and native verification helpers exist under tools and retain their own preconditions.

The root README duplicates build/test commands and a dated feature list. docs/README mixes authority with a long feature catalog. F02/F04/F07 and roadmap summaries contain older scope/navigation wording beside later approvals. The prior kit review and refactor audit are dated historical inspections; do not rewrite their conclusions as current facts.

## Changes

1. Audit local links across repository documentation, source references and template isolation. Record prioritized findings in one new dated audit, linked from existing migration history.
2. Keep document ownership in docs/README; move feature navigation verbatim to docs/FEATURES, correcting stale descriptions through existing PRD/spec references. Add a concise local SETUP and tools entry point; link test commands to TEST-PLAN and release procedures to GITHUB-RELEASE.
3. Add scoped supersession notes/corrections where current summaries obscure later approved behavior. Preserve dated evidence, requirement/scenario IDs, ADR rationale and all existing STATUS edits.
4. Improve starter-kit entry guide and copyable onboarding. Add one reusable Python-standard-library documentation checker inside the copyable tools directory, its meaningful tests and a thin root launcher; keep implementation in one location. Do not add dependencies or generate a pretend MAUI application.
5. Validate links/anchors, template-only copy, placeholder readiness, checker error cases, ID preservation and diff. Update this task record and audit with actual results.

## Verification

The checker validates local paths/anchors; it does not fetch external sites or execute JavaScript routes. Known runtime fragments use a checked-in route manifest grounded in inspected routers and are counted separately. Template placeholders are allowed for drafting, but an explicit readiness mode fails until customized. This is structural readiness, not product approval or a native test pass.

Use temporary directories for tests, never overwrite a destination repository to demonstrate copying. Run only checker/tool tests; Markdown edits do not require .NET builds, Android tests or device access. Record any unsupported Markdown constructs or unresolved decision/evidence gaps in the audit.
