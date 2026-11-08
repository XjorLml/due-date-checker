# F07 categories execution - 2026-10-18

Observer: agent. User request: "implement f07", following the next-feature discussion identifying categories/types. [CT01-CT06](../../../specs/f07-categories/spec.md#acceptance-scenarios) refine this independent category slice, not the entire F07 umbrella. [PRD](../../PRD.md#f07-category-rules) owns policy. Recurrence, timed dues and optional amounts remain separate; release/signing and optional CI stay held.

## Environment and artifact

Windows/PowerShell, .NET SDK 10.0.400, base `c0ab21c61040c11ec170561f1f60b9753b5c9cf3` plus preserved earlier working-tree work and F07 categories. No commit/push. [75-file source/test manifest](F07-CATEGORIES-2026-10-18.sha256), SHA-256 `368289781C3DB76885FF12BB9ACDEFDDB8C8B64D3E9A233E8EAEFE0C7DBBE022`; text CRLF normalized to LF, TTF binary, no signing keys/build outputs. APK `src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk`, SHA-256 `136FFBBF8F5551F1A27AF8DAA90F8C566416FBE07774103C9F14749CF9E2F0B2`. M2101K6G / Android 13 API 33, connected unlocked; data-preserving installation passed. Intended release signing identity remains unverified. The pulled installed APK hash matches this build.

## Automated and build results

- Core: 47 passed, 0 failed, including ten new category/migration cases. The two existing schema-version expectations now correctly require version 2; migration 001 and initializer code are unchanged.
- App: 101 passed, 0 failed, including fourteen new category/editor/projection/filter cases. Cached NU1900: NuGet vulnerability metadata unavailable. The harness does not load XAML/Android controls.
- Android Release build: passed, 0 warnings/errors, 1m49.15s. Approved SDK/NuGet cache access used for the local build; no package/build-signing configuration change.
- Exact legacy-field preservation includes active, Paid and tombstoned records and their original timestamps/currency/timezone. Tests also exercise invalid domain/SQL values, corrupt reads, controlled migration conflict without resetting version/data, category round trips and tombstone protection, draft/missing-selection/amount rules, Save failure/retry, no duplicate completed Save, all saved projections and title AND status AND category with stable binding/global summaries.

## Native results

Upgrade, agent-observed: in airplane mode, compare old/new Dashboard summary membership in memory around `adb install -r`. All active total/Paid/today/overdue/upcoming membership stayed unchanged. Selecting Other matches the entire active population after migration, and the native dialog exposes Bill/Task/Subscription/Other. No existing due was edited/deleted; no user titles/amounts/count values/screenshots were recorded. Original font/theme/radio settings restored and checked. This does not inspect the phone-private database; exact stored-field/tombstone preservation is established by automated SQLite tests.

Core/category interactions, agent-observed on the same APK in airplane mode:

- New form defaults to Bill; native picker selection and zero-amount Save succeed. Details shows Bill, Cancel after choosing Task keeps Bill, and a saved Task change refreshes Details and the Dues row.
- Dashboard title query AND Task category yields the task; adding Paid yields zero results. Multi-character typing/correcting a query while empty works. Global total/Paid/date partitions stay unchanged by category/search/status filtering. Tab switches retain the filter.
- Saving Subscription plus Paid through Dashboard Details/editor refreshes Details, removes the task from the old Task filter and includes it under Subscription/Paid. Clear filters resets category along with query/status. Calendar and Dues rows show Subscription.
- Force-stop/relaunch in airplane mode preserves Subscription/Paid and resets only the ephemeral category filter. No user due was edited/deleted. One uniquely named zero-amount task due is tracked for cleanup. Original phone settings restored and checked after this run.

Light/Dark at 100%/200% passed category dialogs, saved Subscription labels, edit Other then Cancel without changing saved data, retained Dashboard parent/filter, category/status no-match, multi-character typing with empty results, and Clear filters. Cases start fresh app sessions around OS setting changes. Original font/theme/radio settings restored and checked after the matrix. These are targeted interactions, not full visual/accessibility acceptance. Cleanup passed: the single task due was confirmed deleted through Details/editor. Restart in airplane mode plus a prefix-only Dashboard query returned zero active matches; the stored-category tombstone is intentionally retained by the existing soft-delete contract (physical phone row not inspected). No existing user due was edited/deleted. Original font 0.81, dark theme, airplane off, Wi-Fi on and mobile data on were restored and checked; the app was reopened with those settings. No release/signing setup/CI activation occurred.

## Remaining acceptance

Full TalkBack/keyboard/switch, other widths/devices/landscape, complete visual acceptance, controlled native migration/read/write/navigation failures, physical phone tombstone inspection and intended-key release acceptance remain open. F07 recurrence/timing/optional amounts are unimplemented independent extensions, not accepted by these category results. No release or CI activation.

## Documentation and preservation

Canonical PRD/SDD/DESIGN, test plan, current status, roadmap and dedicated category spec/plan/tasks were updated. Verification compares against the pre-F07 working-tree checkpoint: all prior files retained; only declared category modules and two schema-version assertions changed among existing source/tests; migration 001 and all unrelated source/tests remained unchanged. R01-R08 are preserved. Changed-document local paths/anchors, new-file whitespace/headings, task diff and all 75 normalized source/test hashes were validated. Historical artifact/evidence records are preserved; new category results do not retrospectively accept prior artifacts.
