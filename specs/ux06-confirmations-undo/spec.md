# Confirmation messages and Undo after Delete (UX06)

Status: authorized 2026-11-06 ("start wave 4") under the decided [PRD UX06 rule](../../docs/PRD.md#ux-review-decisions-2026-11-06) (option **B** of the [UX decision log](../ux-improvements/spec.md#decision-log): messages, with a 5-second Undo after Delete); implemented 2026-11-06 with automated and emulator evidence ([evidence](../../docs/delivery/evidence/UX-WAVE4-2026-11-06.md)); phone and TalkBack check pending. It changes the [soft-delete](../v1.3-soft-delete/spec.md) rule "no Undo in v1" for this one case only. [Plan](plan.md), [tasks](tasks.md).

## Acceptance scenarios

| ID | Outcome |
|---|---|
| CM01 | After a successful Save in the editor, a message appears above the bottom bar for about 3 seconds: "Due saved." for a new due, "Changes saved." for an edit, or "Saved. Next one added for dd MMM yyyy." when saving a repeating due as paid added its next occurrence. |
| CM02 | After Mark as paid or Mark as unpaid on Details, the same kind of message reads "Marked paid.", "Marked unpaid." or "Marked paid. Next one added for dd MMM yyyy.". |
| CM03 | After a confirmed Delete, "Due deleted." appears with an Undo action for 5 seconds. Android may keep it longer when the user has set a longer accessibility timeout. |
| CM04 | Undo restores that due with the same ID, fields and creation time; only the deletion is cleared and Last updated becomes the time of the Undo. Reminders are planned again for it, the page on screen refreshes (Details shows it again; lists include it), and "Due restored." appears. |
| CM05 | Undo reverses only the deletion it was offered for, at most once: a second tap, an older offer replaced by a newer Delete, or a due whose deletion stamp has changed is not restored and nothing changes ("Nothing to undo."). An edit still can never revive a deleted due. |
| CM06 | If restoring fails, the due stays deleted and "Couldn't undo. The due stays deleted." appears. |
| CM07 | Failed or cancelled Save, payment and Delete actions show no confirmation; their existing inline errors stay. |
| CM08 | Messages name no title or amount. Screen readers hear each message once, from the message itself; the old separate announcements for these actions are removed. |

## Boundaries

- No recycle bin, history or later restore. Undo exists only while its message shows.
- Other actions (export, import, reminder settings, preferences) keep their in-page messages.
- Release, signing and pushing to `main` remain held.
