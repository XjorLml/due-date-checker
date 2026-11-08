# F07 optional amounts implementation plan

1. Preserve the working tree and record OA01-OA06 plus the approved category scope, NULL-versus-zero rule and "No amount" display. Reuse the existing editor, projections, repository and migration pipeline.
2. Change `Due.Amount` to nullable `Money?`; keep `Money` non-negative and unchanged. Add `Due.Validate()` for the Bill/Subscription requirement, call it from `Due.Create` and repository writes, and map NULL in `SqliteDueRepository`.
3. Append immutable migration `004_optional_amounts.sql`; rebuild `dues` to make `amount_centavos` nullable while preserving every column, category constraint, tombstone and the active-date index. Do not edit migrations 001-003.
4. Add category-aware amount requirement text/placeholder/semantics and validation to `DueEditViewModel`/`DueEditPage`; allow clearing Task/Other amounts and reject blank Bill/Subscription amounts. Update list, Dashboard, Details and Calendar projections to show "No amount" and include it in semantics.
5. Verify schema-3 upgrade and idempotency, NULL/zero round trips, domain and repository rejection, editor required/optional/category-switch/clear/failure paths, projections and unchanged status/count/reminder behavior. Run Core and App tests sequentially, then a Release build.
6. Perform targeted native checks with uniquely tagged Task/Other and Bill records only; preserve data, clean up, restore settings, and record artifact identity. Broader accessibility/other-device/intended-key checks remain separate.
7. Update PRD, SDD, DESIGN, F07/roadmap links, README, tasks, STATUS and evidence. Validate links, preservation of older evidence, unrelated changes and the diff.
