# Reusable .NET MAUI starter kit

**Documentation templates and a local checker for future projects.** The original source review is dated 2026-10-21; the [November 8 usability audit](../../delivery/DOCS-AUDIT-2026-11-08.md) records the later toolkit changes, and the [API add-on spec](../../../specs/starter-kit-api/spec.md) records the 2026-11-09 client–server additions. This kit supplies a proposed source layout, not a generated or build-tested application.

## Start here

1. Copy the **contents** of [project/](project/README.md) into a new, empty repository. For an existing project, merge deliberately and keep its established instructions, requirements and evidence.
2. From that destination's root, run the command below. It needs Python 3.10+ and no packages; `py -3` may be available on Windows if `python` is not.
3. Follow the customization order below, or use the [reusable prompt](PROMPT.md). Start with a documentation-only scope if product/toolchain decisions are unresolved.
4. After customization, run the readiness check. A successful check proves local documentation structure, not an application build or approval.

```powershell
python tools/check_docs.py
python tools/check_docs.py --ready
```

The first command should report PASS for the untouched copy, with placeholders explicitly allowed. The second intentionally fails until all `{{...}}` fields and inapplicable examples outside the reusable templates have been resolved. The ADR, evidence and feature templates keep their placeholders for future copies; their links are still checked. Errors show the file, line and next action; [checker help and recovery](project/tools/README.md) explain exits and limits. Commands do not generate an application, fetch external sites, change files or touch devices.

## Customize a new project

| Step | Decide or fill | Ready to continue when |
|---|---|---|
| 1. Product | [PRD](project/docs/PRD.md): users, first workflows, platforms, data/privacy, locale and distribution | Approved scope and unresolved owners are explicit; no copied reference-app rules |
| 2. Technical shape | [STRUCTURE](STRUCTURE.md) and [SDD](project/docs/SDD.md) | A small layout and actual platform boundaries fit the approved workflows |
| 3. Server, if any | [API](project/docs/API.md) and the [client–server layout](STRUCTURE.md#client-server-baseline) | Environments, compatibility, sign-in, error, retry and offline choices are explicit, or API.md is deleted with its links |
| 4. Development environment | [SETUP](project/docs/SETUP.md) | Compatible versions/hosts are selected from revalidated official guidance; command placeholders are resolved |
| 5. Interface and work | [DESIGN](project/docs/DESIGN.md) and [feature template](project/specs/feature-template/spec.md) | First feature has observable scenarios, plan/tasks and explicit verification boundaries |
| 6. Evidence and delivery | [STATUS](project/docs/delivery/STATUS.md), [TEST-PLAN](project/docs/delivery/TEST-PLAN.md), [RELEASE](project/docs/RELEASE.md) | Unrun checks remain pending and actual results identify their source/artifact and observer |

Generate solution/project/workflow files only within the chosen implementation scope, after toolchain decisions. Rename feature-template deliberately, update inbound links and rerun the checker. Keep the [historical repository review](REVIEW.md) as background for pattern selection; its old findings are not a fresh audit of a new project.

Use [PROMPT](PROMPT.md) to ask a coding assistant to perform these steps in the next repository. It requests missing product decisions while continuing independent setup work. Do not transfer Duewise's identifiers, signing identity, branding, test counts or release approvals.

## Contents and ownership

| Material | Purpose | Copy into a new repository? |
|---|---|---|
| [REVIEW](REVIEW.md) | Dated Duewise findings and evidence limits | No; reference when selecting patterns |
| [STRUCTURE](STRUCTURE.md) | Source/folder responsibilities and growth options | Optional planning reference |
| [PROMPT](PROMPT.md) | Reusable customization request | Use as a prompt |
| [Project README](project/README.md), [AGENTS](project/AGENTS.md) | Entry point and agent workflow | Yes, customize |
| [Documentation index](project/docs/README.md) | Authority map and task routes | Yes |
| [PRD](project/docs/PRD.md), [SDD](project/docs/SDD.md), [DESIGN](project/docs/DESIGN.md) | Behavior, technical contracts, and UI standards | Yes |
| [API](project/docs/API.md) | Client–server rules for apps that call a server | Yes if the app calls an API; otherwise delete it and the links the checker reports |
| [SETUP](project/docs/SETUP.md), [DEVELOPMENT](project/docs/DEVELOPMENT.md), [RELEASE](project/docs/RELEASE.md) | Reproducible setup, coding practices, and distribution procedures | Yes, select applicable practices |
| [REFERENCES](project/docs/REFERENCES.md) | Official guidance checked for this kit | Yes; revalidate when adopting/upgrading |
| [Test plan](project/docs/delivery/TEST-PLAN.md), [STATUS](project/docs/delivery/STATUS.md), [evidence template](project/docs/delivery/evidence/TEMPLATE.md) | Verification procedures, current state, and observed results | Yes; start with no results |
| [ADR index](project/docs/adr/README.md), [ADR template](project/docs/adr/TEMPLATE.md) | Significant decision rationale | Yes; start with no decisions |
| [Feature spec](project/specs/feature-template/spec.md), [plan](project/specs/feature-template/plan.md), [tasks](project/specs/feature-template/tasks.md) | Observable scenarios and bounded implementation | Yes; retain the example or duplicate it per feature |
| [Tools guide and checker](project/tools/README.md), [checker source](project/tools/check_docs.py) | Read-only local documentation checks, reusable tests and safe automation guidance | Yes; no external packages required |

## Adoption checklist

- [ ] Product decisions and exclusions are explicit; unresolved choices have owners.
- [ ] Only intended platforms are targeted; required build hosts and test devices are available.
- [ ] SDK, workload set, platform tools, and dependencies are recorded and reproducible.
- [ ] Documentation links resolve independently of the Duewise repository.
- [ ] If the app calls an API: API.md decisions are explicit, the OpenAPI document is committed, and local addresses are recorded in SETUP.
- [ ] The normal checker passes; `--ready` passes after placeholder/example cleanup.
- [ ] Placeholder/example scenario IDs have been adapted; none are marked accepted.
- [ ] A first vertical slice has meaningful automated coverage and an actual platform build.
- [ ] Required native checks and signing/distribution acceptance are tracked separately.

The suggested baseline is two production projects and two test projects. A one-screen app may start smaller; the responsibility boundaries still apply. Persistence, a server API, reminders, authentication, sync, analytics, and a particular currency are product choices, not starter-kit requirements.
