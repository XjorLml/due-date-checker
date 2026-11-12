# Reusable new-project prompt

Copy the text below into the next repository after making this kit available there. Replace inputs you know; unresolved decisions must remain explicit.

```text
Set up a practical .NET MAUI project using the documentation starter kit at:
{{StarterKitPath}}

Project name: {{ProjectName}}
Product purpose and users: {{PurposeAndUsers}}
First release workflows: {{InitialWorkflows}}
Target platforms: {{PlatformsOrUndecided}}
Available development/build hosts: {{Hosts}}
Storage/offline/network needs: {{DataNeedsOrUndecided}}
Server: {{NoServerOwnAspNetCoreApiOrThirdPartyApi}}
Languages, currency, date/timezone policy: {{LocalePolicyOrUndecided}}
Distribution channel: {{DistributionOrUndecided}}
Requested scope: {{DocumentationOnlyOrScaffoldAndFirstSlice}}

Follow the destination repository's AGENTS.md. Check git status and preserve
existing changes. Read the kit guide, structure, and copyable templates. If this
is an existing application, inspect its source, tests, and docs before adapting
the proposal; preserve established specifications instead of duplicating them.

Create/customize the documentation index, PRD, SDD, DESIGN, SETUP, DEVELOPMENT,
RELEASE, delivery status/test plan/evidence template, ADR index/template, feature
spec/plan/tasks, root README, and AGENTS.md. With a server, also customize API.md
and follow the structure's client-server baseline; without one, delete API.md
and the links that the checker then reports. Keep relative links valid. Record
unknowns and ask concise questions for decisions that block implementation;
continue work that does not depend on those answers.

Use a small architecture first. Separate platform-independent rules from MAUI
and native adapters. Explain any extra project/layer using a concrete need.
Declare supported platforms explicitly. Recheck current official MAUI support,
SDK/workload compatibility, native tools, and publishing guidance; cite sources
and record exact chosen versions. Do not reuse the reference app's identifiers,
business rules, branding, credentials, historical evidence, or approval status.

Define observable scenarios for one vertical slice, including failure/retry,
state retention, persistence if relevant, and required native behavior. Make
implementation, automated verification, platform builds, device verification,
and acceptance independently visible. Do not mark unrun checks as passed.

If documentation-only scope was selected, validate links, consistency, and the
diff. Use the copied toolkit's `python tools/check_docs.py`; after customization,
also run `python tools/check_docs.py --ready`. Resolve placeholders honestly or
remove inapplicable examples; a structural pass is not product approval. Do not
generate an application merely to validate the documentation toolkit.
If scaffolding/implementation was selected, create the selected solution
structure, pin the toolchain, implement the authorized slice, and run meaningful
checks on available hosts. Record unavailable platform checks as pending.
Prepare distribution instructions without publishing unless authorized.

Finish with created files, architecture choices and tradeoffs, validation
actually performed, remaining decisions/acceptance, and next development steps.
```
