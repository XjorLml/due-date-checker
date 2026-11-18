-- UX review decisions (2026-11-16). Additive: existing rows keep every value.
-- UX32: amount_varies marks a repeating Bill/Subscription whose amount changes each month; its next
-- occurrence starts without an amount. The pairing with repeat_rule and the amount rule is app-enforced.
ALTER TABLE dues ADD COLUMN amount_varies INTEGER NOT NULL DEFAULT 0 CHECK (amount_varies IN (0, 1));
-- UX30: overdue reminders (1, 7, 14, 21 and 28 days late), on by default.
ALTER TABLE reminder_settings ADD COLUMN overdue INTEGER NOT NULL DEFAULT 1 CHECK (overdue IN (0, 1));
