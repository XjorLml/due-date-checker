# GitHub draft release tasks

See [spec/scenarios](spec.md) and [plan](plan.md).

- [x] Inspect current CI, Android project and signing template; preserve unrelated work.
- [x] Record authorized scope, acceptance and implementation plan.
- [x] Implement manual signed-build/draft-release workflow and safe helpers. Existing push CI is unchanged.
- [x] Agent-observed local helper tests and actionlint passed; real SDK metadata/signature verification accepted the existing locally built signed APK. Counts and limits in [evidence](../../docs/delivery/evidence/GITHUB-RELEASE-2026-11-08.md).
- [x] Update documentation contracts, setup guide and attributed delivery evidence.
- [x] 2026-11-18: automatic release on merge to `main` with immediate publication (GR08–GR13); helper rewritten (`version`, `publish`, derived code, change list); CI gains pull-request checks and an Android build. 28 release-helper tests pass locally; both workflows parse. actionlint was not run here: downloading the linter was not permitted in this session.
- [x] Signing exposure checked 2026-11-18 (agent-observed, values never printed): the current password is in no commit on any local or GitHub ref; no keystore file was ever committed. Rotation is optional.
- [x] User: configured GitHub Secrets and ran the workflow on `main` (2026-11-18).
- [x] First hosted signed build and draft release observed through GitHub's public API: run `44342499904` on `c3a2019` succeeded in both jobs. Run `44340221811` failed at the draft step; the log needs sign-in. The draft assets were seen by the user (GR01–GR06 hosted). Asset download and inspection by the agent were not done.
- [ ] First automatic (GR08–GR13) run after merging these workflow changes to `main`.
- [ ] Actual-phone acceptance of the CI-built intended-key APK. A green hosted run is not device acceptance.
