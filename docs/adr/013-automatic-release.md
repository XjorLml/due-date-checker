# ADR 013: Automatic release on every merge to main

Status: accepted. Decision date: 2026-11-18. Supersedes the "draft only, publish by hand" part of the 2026-11-08 [release workflow](../../specs/github-release/spec.md).

## Context and provenance

On 2026-11-18 the user asked to "update git workflow for release. make ci/cd with automatic release". Asked about the trigger and publication, they chose **every merge to main** and **publish immediately**. The details below were proposed by Claude while implementing. Their rationale is recorded for the learner's review and was not separately approved.

## Decision

1. **Trigger:** a push to `main` (a merge) runs the release workflow. Documentation-only changes do not. A manual run, also from `main` only, may name a newer version for a minor or major bump. Runs are serialized.
2. **Version:** the next patch after the highest `vX.Y.Z` among git tags and GitHub releases, drafts included. A tag already on the commit is reused, so reruns are idempotent.
3. **Android build number:** derived from the version as major×1,000,000 + minor×1,000 + patch instead of typed in.
4. **Publication:** after the Core, App and release-tool tests, the signed build and APK verification, the release is created as published and marked latest. Its notes list the commit subjects since the previous release.
5. **Gate before main:** CI runs the tests, release-helper tests and documentation link check on pull requests, plus an unsigned Android build for pull requests into `main`.

## Rationale

- Choosing the version from what already exists removes the two manual inputs that could collide or go backwards.
- A derived build number can never decrease, and 1.0.0 (1,000,000) sits above the codes 1 and 2 already installed, so Android accepts updates.
- Skipping documentation-only merges avoids shipping identical APKs under new version numbers.
- Since every merge ships, the Android build has to pass before merging, not after.
- Signing, verification and secret handling are unchanged from the reviewed 2026-11-08 design.

## Alternatives considered

- **Tag-triggered or release-PR (release-please) flows.** These keep a human step or add a bot; the user chose every merge.
- **Draft or pre-release publication.** Safer, but the user chose immediate publication.
- **`github.run_number` as the build number.** It is unrelated to the version and resets if the workflow is renamed; rejected.

## Consequences

- Anything merged to `main` that changes the app reaches users within one workflow run. Phone acceptance moves after publication.
- The repository is public, so the signing secrets must stay out of it. Checks on 2026-11-18 found the current password in no commit and no keystore ever committed ([release guide](../delivery/GITHUB-RELEASE.md#before-the-first-automatic-release)). The workflow cannot enforce this for future commits.
- The patch number grows with every app-changing merge; at 999 it rolls over to the next minor version.

Links: [spec GR08–GR13](../../specs/github-release/spec.md#automatic-release-2026-11-18), [SDD](../SDD.md#github-release-automation), [workflow](../../.github/workflows/release.yml).
