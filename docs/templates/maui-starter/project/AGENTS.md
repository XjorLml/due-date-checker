# Agent workflow template

This is a future-project template. Activate it by customizing it at the destination repository root; its placeholders do not establish approved product requirements.

Start with [docs/README.md](docs/README.md), then read only the documents relevant to the task. Inspect applicable source and tests before making implementation claims.

## Authorities

- [PRD](docs/PRD.md): approved behavior, stable requirement IDs, exclusions, acceptance outcomes.
- [SDD](docs/SDD.md): technical contracts and dependency boundaries.
- [API](docs/API.md), if the app calls a server: client–server rules; the committed OpenAPI document owns endpoint shapes.
- [DESIGN](docs/DESIGN.md): UI standards and explicitly scoped concepts.
- [SETUP](docs/SETUP.md): chosen toolchain and reproducible commands.
- [DEVELOPMENT](docs/DEVELOPMENT.md): implementation and review practices.
- [STATUS](docs/delivery/STATUS.md): delivery state and attributed evidence.
- [TEST-PLAN](docs/delivery/TEST-PLAN.md): procedures and acceptance gates.
- [RELEASE](docs/RELEASE.md): packaging, signing, and distribution procedure.
- [Feature template](specs/feature-template/spec.md): feature scenarios, plan, and task structure.
- [ADR index](docs/adr/README.md): recorded decision rationale.

## Workflow

1. Check the working tree and preserve unrelated modifications and untracked work.
2. Identify governing requirement/scenario IDs. Record contradictions between requirements, documentation, and code instead of silently changing scope.
3. For an independently implementable feature, create or update `specs/<feature>/spec.md`, `plan.md`, and `tasks.md`. Reuse established specs. Small fixes may reference existing IDs directly.
4. Define observable acceptance outcomes, affected modules, dependencies, and verification before implementing.
5. Work within the user's authorization. Documentation reviews are part of the task, not automatic additional permission gates. Preserve explicitly agreed learner checkpoints, if any: {{LearningWorkflowOrNone}}.
6. Follow DEVELOPMENT and verify at the relevant boundary. A test definition is not a passing result; a build is not native or product acceptance.
7. Update affected authorities and evidence. Report changes, observed checks, limitations, and remaining acceptance.

## Documentation and evidence rules

- Keep each contract authoritative in one place. Link to it from plans and tests.
- Review PRD for behavior changes, SDD for contract changes, and DESIGN for UI-system changes.
- Released apps keep calling the API. Follow API's compatibility rules, review the OpenAPI diff with every API change, and record a decision before any breaking change.
- Future concepts do not authorize features. Keep proposals and accepted behavior distinct.
- Record actual dates, source/artifact identity, environments, observer attribution, and expected/actual outcomes. Never transfer another project's passes or approvals.
- Use ADRs for significant choices with supported rationale. Do not invent decision dates, identities, or approval.
- Preserve historical records; mark supersession and update links when moving documents.
- For documentation-only work, validate links, anchors, traceability, preservation, and the diff. Do not run application builds merely to validate Markdown.
- Keep credentials, signing keys, personal data, and sensitive logs out of version control. Use isolated test fixtures and record cleanup where device settings or data are changed.
