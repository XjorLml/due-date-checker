# Native notification implementation — 2026-11-08

Authorized by the user's request to implement the notification plan. Implements the recorded N1 A / N2 A decisions; [PRD F05](../../docs/PRD.md#notification-decisions-2026-11-07) owns product behavior. The existing HTML lab stays unchanged.

1. Add deterministic reminder text and record fingerprints; cover privacy, absent versus zero amounts, date and recurrence text.
2. Add a serialized payment/Undo service using the existing dispatcher, delivered receipts and reminder-aware repository. Reject obsolete, edited, deleted, locked and duplicate actions. Recurrence uses the existing atomic update-with-next transaction. Undo lasts 10 seconds and retains the next occurrence.
3. Post private Android notifications with generic public versions, grouped reminders and immutable actions. Android 12+ requires authentication on the action; older Android uses a dedicated unlock activity. Recheck lock state immediately before writes. Keep notification taps and active editor drafts safe.
4. Verify domain/service behavior, build the Android package, and inspect available emulator behavior. Track phone/lock-screen acceptance separately.
5. Update technical/design contracts, test procedures and attributed delivery evidence.

No schema, native scheduling policy, OS notification delivery guarantee, signing identity or HTML changes.
