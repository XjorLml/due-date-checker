-- PRD TM twice-a-month repeat (2026-11-16). Adds repeat code 4 and the second day.
-- Migration 005 limited repeat_rule with CHECK (repeat_rule IN (0, 1, 2, 3)), and SQLite cannot
-- change a CHECK with ALTER TABLE, so this follows SQLite's documented rebuild like migration 004:
-- create the new shape, copy, drop, rename, re-index. DatabaseInitializer runs this script and the
-- user_version bump in one transaction, so a failure leaves schema 6 and every row untouched.
-- Column order, types, defaults and CHECKs match schema 6; repeat_day_2 is appended. No value is rewritten.
CREATE TABLE dues_v7 (
    id               TEXT PRIMARY KEY,
    title            TEXT NOT NULL,
    amount_centavos  INTEGER,                                -- NULL = no amount (Task/Other only, app-enforced)
    currency         TEXT NOT NULL DEFAULT 'PHP',
    due_date         TEXT NOT NULL,
    notes            TEXT,
    is_paid          INTEGER NOT NULL DEFAULT 0,
    timezone         TEXT NOT NULL DEFAULT 'Asia/Manila',
    created_at       TEXT NOT NULL,
    updated_at       TEXT NOT NULL,
    deleted_at       TEXT,
    category         INTEGER NOT NULL DEFAULT 0 CHECK (category IN (0, 1, 2, 3)),
    repeat_rule      INTEGER NOT NULL DEFAULT 0 CHECK (repeat_rule IN (0, 1, 2, 3, 4)),
    repeat_day       INTEGER CHECK (repeat_day BETWEEN 1 AND 31),
    repeat_source_id TEXT,
    category_id      TEXT NOT NULL DEFAULT 'ph.other.general',
    repeat_day_2     INTEGER CHECK (repeat_day_2 BETWEEN 1 AND 31)  -- TwiceMonthly's later day; NULL otherwise (app-enforced)
);

-- Copy rowid too, so even SQLite's hidden row order is unchanged. Tombstones are copied like any row.
INSERT INTO dues_v7 (rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id)
SELECT rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id
FROM dues;

-- Dropping the old table also drops both of its indexes, so they are recreated below.
DROP TABLE dues;
ALTER TABLE dues_v7 RENAME TO dues;

CREATE INDEX ix_dues_active_due_date ON dues (due_date) WHERE deleted_at IS NULL;
CREATE INDEX ix_dues_repeat_source ON dues (repeat_source_id) WHERE repeat_source_id IS NOT NULL;
