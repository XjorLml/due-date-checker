# GitHub draft release tasks

See [spec/scenarios](spec.md) and [plan](plan.md).

- [x] Inspect current CI, Android project and signing template; preserve unrelated work.
- [x] Record authorized scope, acceptance and implementation plan.
- [x] Implement manual signed-build/draft-release workflow and safe helpers. Existing push CI is unchanged.
- [x] Agent-observed local helper tests and actionlint passed; real SDK metadata/signature verification accepted the existing locally built signed APK. Counts and limits in [evidence](../../docs/delivery/evidence/GITHUB-RELEASE-2026-11-08.md).
- [x] Update documentation contracts, setup guide and attributed delivery evidence.
- [ ] User: configure GitHub Secrets, place workflow on default branch and push selected source.
- [ ] Observe first hosted signed build and draft release (GR01–GR06); inspect downloaded assets.
- [ ] Actual-phone acceptance of the CI-built intended-key APK. A green hosted run is not device acceptance.
