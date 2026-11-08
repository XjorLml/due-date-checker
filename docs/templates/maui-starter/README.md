# Reusable .NET MAUI starter kit

**Templates for future projects.** Reviewed on 2026-10-21 against Duewise; this kit does not change its approved requirements or implementation. It provides documentation and a proposed source layout, not a generated or build-tested application.

## Start here

1. Read the [repository review](REVIEW.md) for patterns, limitations, and prioritized improvements.
2. Choose the small-app layout in [STRUCTURE](STRUCTURE.md). Add projects only for the listed reasons.
3. Copy the **contents** of [project](project/README.md) into a new repository, preserving relative paths. Merge deliberately when destination files exist; do not overwrite existing instructions or evidence.
4. Replace `{{...}}` placeholders. Decide the product scope, platforms, identifier, storage, language, timezone, and distribution channel before choosing dependencies. Do not transfer Duewise's requirements, test counts, release approvals, or signing identity.
5. Fill [PRD](project/docs/PRD.md), then [SDD](project/docs/SDD.md), [SETUP](project/docs/SETUP.md), and [DESIGN](project/docs/DESIGN.md). Each template states its authority and provides writing guidance. Remove inapplicable sections with a short reason.
6. Generate the application with the selected supported MAUI toolchain and map it to the proposed layout. Create actual SDK/configuration files only after resolving the version placeholders. The kit intentionally contains no runnable workflow or project file with pretend versions.
7. Start the first feature from [feature-template](project/specs/feature-template/spec.md), update its three files together, and keep all evidence pending until observed. Validate links again after renaming it.

Use [PROMPT](PROMPT.md) to ask a coding assistant to perform these steps in the next repository. It requests missing product decisions while continuing independent setup work.

## Contents and ownership

| Material | Purpose | Copy into a new repository? |
|---|---|---|
| [REVIEW](REVIEW.md) | Dated Duewise findings and evidence limits | No; reference when selecting patterns |
| [STRUCTURE](STRUCTURE.md) | Source/folder responsibilities and growth options | Optional planning reference |
| [PROMPT](PROMPT.md) | Reusable customization request | Use as a prompt |
| [Project README](project/README.md), [AGENTS](project/AGENTS.md) | Entry point and agent workflow | Yes, customize |
| [Documentation index](project/docs/README.md) | Authority map and task routes | Yes |
| [PRD](project/docs/PRD.md), [SDD](project/docs/SDD.md), [DESIGN](project/docs/DESIGN.md) | Behavior, technical contracts, and UI standards | Yes |
| [SETUP](project/docs/SETUP.md), [DEVELOPMENT](project/docs/DEVELOPMENT.md), [RELEASE](project/docs/RELEASE.md) | Reproducible setup, coding practices, and distribution procedures | Yes, select applicable practices |
| [REFERENCES](project/docs/REFERENCES.md) | Official guidance checked for this kit | Yes; revalidate when adopting/upgrading |
| [Test plan](project/docs/delivery/TEST-PLAN.md), [STATUS](project/docs/delivery/STATUS.md), [evidence template](project/docs/delivery/evidence/TEMPLATE.md) | Verification procedures, current state, and observed results | Yes; start with no results |
| [ADR index](project/docs/adr/README.md), [ADR template](project/docs/adr/TEMPLATE.md) | Significant decision rationale | Yes; start with no decisions |
| [Feature spec](project/specs/feature-template/spec.md), [plan](project/specs/feature-template/plan.md), [tasks](project/specs/feature-template/tasks.md) | Observable scenarios and bounded implementation | Yes; retain the example or duplicate it per feature |
| [Tools guide](project/tools/README.md) | Automation responsibilities and safe fixture handling | Yes, add actual scripts when needed |

## Adoption checklist

- [ ] Product decisions and exclusions are explicit; unresolved choices have owners.
- [ ] Only intended platforms are targeted; required build hosts and test devices are available.
- [ ] SDK, workload set, platform tools, and dependencies are recorded and reproducible.
- [ ] Documentation links resolve independently of the Duewise repository.
- [ ] Placeholder/example scenario IDs have been adapted; none are marked accepted.
- [ ] A first vertical slice has meaningful automated coverage and an actual platform build.
- [ ] Required native checks and signing/distribution acceptance are tracked separately.

The minimum structure is two production projects and two test projects, following the repository's practical split. A one-screen app may start smaller; the responsibility boundaries still apply. Persistence, reminders, authentication, sync, analytics, and a particular currency are product choices, not starter-kit requirements.
