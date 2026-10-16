# Agent workflow

Start with [docs/README.md](docs/README.md), then load only documentation relevant to the task. Inspect applicable source and tests before making implementation claims.

## Authorities

- [PRD](docs/PRD.md): approved scope, requirements R01–R08, business rules, acceptance outcomes.
- [SDD](docs/SDD.md): architecture and technical contracts.
- [DESIGN](docs/DESIGN.md): UI/UX standards; read its scope notice before applying future concepts to v1.
- [STATUS](docs/delivery/STATUS.md): implementation state and attributed verification evidence.
- [TEST-PLAN](docs/delivery/TEST-PLAN.md): verification procedures and acceptance gates.
- [Feature specs](specs/v1.3-soft-delete/spec.md): scenarios, technical plan, and task tracking for independently implementable changes.
- [ADR index](docs/adr/README.md): significant architecture decisions and rationale.

PLAN.md, phase records, the archived SDD, and `.vibe-wise` project/progress notes retain historical evidence. They do not override canonical requirements or current delivery status. Consult the [learning profile](.vibe-wise/profile.md) when mentoring; distinguish explained concepts from demonstrated understanding.

## Development workflow

1. Check the working tree and preserve unrelated changes, including untracked work.
2. Identify governing requirement IDs and relevant technical/design contracts. Record contradictions rather than silently changing scope to match code.
3. For independently implementable feature changes, create or update `specs/<feature>/spec.md`, `plan.md`, and `tasks.md`. Use the existing soft-delete directory as the structure example; do not duplicate an established spec. Small fixes can reference governing IDs directly.
4. Define observable acceptance scenarios, affected modules, dependencies, and meaningful verification. Distinguish existing behavior from proposed changes and deferred work.
5. Implement within the authorized scope. Preserve learner review/explanation checkpoints when applicable; existing user authorization governs whether work may proceed.
6. Validate at the appropriate boundary using the test plan. A test definition is not a passing result, and a build is not device acceptance.
7. Update affected canonical documents and delivery evidence. Report what changed, validation performed, remaining acceptance, and limitations.

## Documentation governance

- Keep each requirement and technical contract authoritative in one location; link instead of copying. Preserve stable IDs and historical evidence.
- Product behavior changes require PRD review/update; contract changes require SDD review/update; UI system changes require DESIGN review/update. These reviews are part of the authorized task, not automatic additional permission gates.
- Record significant architecture decisions in ADRs when rationale is available. Do not invent rationale, approval, dates, or ADR identities from source comments.
- Deferred design concepts do not authorize new features. Record unresolved decisions in the relevant spec or existing conflict record and continue unaffected work.
- Track implementation, automated verification, device verification, and acceptance separately. Record date, environment/artifact, and whether evidence was agent-observed, user-reported, or historical.
- Update local links and section references when moving content. For documentation-only changes, validate links, traceability, preservation, and the diff; do not run builds or application tests merely to validate Markdown.
- Keep signing keys/passwords out of version control. Do not log user financial details. Preserve the existing scope and security constraints in the PRD/SDD.
