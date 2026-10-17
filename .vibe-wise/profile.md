# Learner Profile

> Learning preferences remain applicable. The original product description and seven-day goal below are historical; approved scope is now owned by the [PRD](../docs/PRD.md), with current delivery evidence in [STATUS](../docs/delivery/STATUS.md). This annotation does not change the learner profile or infer demonstrated understanding.

Learning mode: active
Onboarding: complete

## Project
Situation: New
Building: Offline-first Android app (.NET MAUI) that tracks dues, budgets and savings and reminds the user before a due date via local scheduled notifications. Users: students and employees in the Philippines (PHP, Asia/Manila). 7-day window from 2026-10-06; day-7 goal is S0–S3 local-only as a sideloaded Release APK.
Codebase familiarity: Not applicable (new project)
Learning scope: Whole system

## Experience
Overall programming: Intermediate (self-described junior-level desktop and fullstack)
Stack familiarity: C#: some (wants to deepen async/await, DI, records, nullable refs, LINQ, testing). .NET MAUI / Android: none (never built or deployed a MAUI app). SQLite, Supabase, GitHub Actions: Not specified.

## Goals
Primary: (1) deepen C#; (2) spec-driven, test-driven, agent-assisted workflow where they review and can explain every diff; (3) senior habits: scoping, tradeoffs, ADRs, threat modeling.
Capability goal: grow toward a senior engineering role.

## Preferences
Checkpoint frequency: Normal
Question style: Mixed (requested 2026-10-06): one decision per question, kept simple; show a recommended option with pros and cons for each option; learner picks and gives a short reason
Implementation style: AI writes code
Mentoring style requested: explain under-the-hood mechanics (OS, runtime, DB, network), prefer simple/boring/proven solutions, no abstraction/pattern/dependency without justification, mark uncertain claims, tag "verify current docs" for MAUI/Android/Play/Supabase facts, ASCII diagrams for data flow, one slice per response. When writing or reviewing SQL, point out a better-performing alternative whenever one exists (requested 2026-10-06). In progress notes, relate concepts to the OOP pillars (encapsulation, abstraction, inheritance, polymorphism) when they apply (requested 2026-10-06). Per-slice output compressed to four sections (goal, under the hood, spec + failing tests, review checklist) during the 7-day window (proposed, accepted with the plan).

## Strong Concepts
No demonstrated understanding recorded yet in conversation. The learner's planning document states senior-shaped choices with rationale (vertical slices, alarms as a disposable cache, injected TimeProvider, integer-centavo Money, single notification channel); treat as self-reported until reasoned through at a checkpoint.

## Developing Concepts
- Android alarm lifecycle (force-stop, app update, doze) — introduced 2026-10-06, not yet applied.
- Android exact-alarm policy and Play closed-testing rule — introduced 2026-10-06.

## Revisit
None recorded yet.
