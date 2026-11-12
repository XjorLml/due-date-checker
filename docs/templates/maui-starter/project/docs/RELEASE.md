# Release and distribution procedure

**Template. Authority:** packaging, signing, distribution, and recovery procedure. [PRD](PRD.md) chooses channels/platforms; [TEST-PLAN](delivery/TEST-PLAN.md) defines acceptance gates; [STATUS](delivery/STATUS.md) records actual readiness. Nothing here authorizes publishing.

## Identity and channel

| Item | Selected value / owner |
|---|---|
| App/package/bundle identity | {{Identifier chosen for this app}} |
| Display version / build number | {{Versioning policy and platform constraints}} |
| Platform / audience / channel | {{StoreInternalSideloadAndAudience}} |
| Signing identity | {{Public certificate fingerprint or identifier; never private key material}} |
| Key/certificate custody | {{Owner, secure location reference, backup/recovery responsibility}} |
| Release decision owner | {{Owner and actual approval record when available}} |

## Packaging recipes

{{Add the exact project-specific publish command for each shipping target after checking current official guidance and SETUP versions. Record configuration, TFM, runtime/architecture, format, trimming/AOT mode, output path, and secure signing input mechanism.}}

| Platform, if selected | Packaging choices to resolve | Acceptance-specific checks |
|---|---|---|
| Android | APK versus AAB for the chosen channel; keystore/upload/store signing relationship | Intended signing certificate, manifest/permissions, install/update and launcher start |
| iOS | Device distribution method, bundle ID, certificate and provisioning profile | Signed device build, entitlements, permission flows and channel validation |
| Mac Catalyst | Intended distribution method, signing and notarization requirements | Signed install/launch, entitlement and desktop interaction checks |
| Windows | Packaged/unpackaged distribution and identity/certificate requirements | Target machine install/update, permissions and window/input behavior |

Android distribution requires signing; development/debug identity is not the intended release identity. Confirm the certificate on the actual artifact and distinguish an upload key from the installed signing identity for store-managed signing. Consult the [official Android publishing procedure](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0); verify other platform/channel requirements when selected. These rows do not claim those targets were built.

## Release sequence

1. Identify the source revision and any uncommitted changes; resolve required gates in TEST-PLAN and record remaining limitations.
2. Produce packages with SETUP's toolchain. Keep signing secrets out of command history, logs, source files, and ordinary build artifacts; document a supported secure input method for the chosen tooling.
3. Record package hashes, version/build number, public signing identity, and build environment. Verify the artifact itself rather than trusting a filename containing `Signed`.
4. Install and upgrade on required targets with controlled fixtures. Check migration/data preservation and signed Release runtime behavior, including binding/trimming/AOT paths as applicable.
5. Execute required native acceptance scenarios and record expected/actual outcomes with the exact artifact.
6. Record the release decision. Distribute only within the user's authorization, then record the channel/release identity and post-distribution smoke evidence.

## Backend deployment, if the app uses its own API

The API ships on its own schedule, while released apps stay installed. [API](API.md#compatibility-and-versioning) owns the compatibility rules; this section owns the deployment procedure.

| Item | Selected value / owner |
|---|---|
| Hosting and environments | {{HostEnvironmentsAndOwner}} |
| Deploy method | {{PipelineOrManualProcedure}} |
| Server database changes | {{MigrationBundleOrReviewedSqlScript}} |
| Production secrets | {{HostSecretStore; rotation owner}} |
| Health and monitoring | {{HealthEndpointAlertsLogRetention}} |
| Rollback | {{PreviousBuildAndDataCompatibility}} |

1. Deploy API changes that every supported app version can use **before** releasing an app that depends on them.
2. Remove or rename in two steps. First add the new shape while the old one keeps working. Then remove the old one in a later release, once no supported app or older API build uses it. This keeps a rollback from breaking newly written data.
3. Apply production database changes with a migration bundle or a reviewed SQL script; migrating at app startup needs a deliberate decision ([references](REFERENCES.md#remote-api-and-server)).
4. Verify the deployed API (health check and one real request) before releasing the app, and record the API build next to the app artifact in evidence.

## Recovery and support

{{Describe crash/support intake without personal-data leakage, release suspension/replacement, key loss/rotation, and data recovery limits.}}

A previous binary may be unable to read an upgraded schema. Choose a forward-fix, tested downgrade, or backup/restore procedure explicitly; do not promise rollback from an artifact archive alone. Preserve released migration scripts and artifact-specific evidence.
