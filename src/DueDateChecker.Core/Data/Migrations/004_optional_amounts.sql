-- F07 optional amounts: amount_centavos becomes nullable (NULL = "No amount").
-- SQLite cannot drop NOT NULL with ALTER TABLE, so this follows SQLite's
-- documented rebuild: create the new shape, copy, drop, rename, re-index.
-- DatabaseInitializer runs this script and the user_version bump in one
-- transaction, so a failure leaves schema 3 and every row untouched.
-- Column order, types, defaults and the category CHECK match schemas 1-3;
-- only the NOT NULL on amount_centavos is gone. No value is rewritten.
CREATE TABLE dues_v4 (
    id              TEXT PRIMARY KEY,
    title           TEXT NOT NULL,
    amount_centavos INTEGER,                                -- NULL = no amount (Task/Other only, app-enforced)
    currency        TEXT NOT NULL DEFAULT 'PHP',
    due_date        TEXT NOT NULL,
    notes           TEXT,
    is_paid         INTEGER NOT NULL DEFAULT 0,
    timezone        TEXT NOT NULL DEFAULT 'Asia/Manila',
    created_at      TEXT NOT NULL,
    updated_at      TEXT NOT NULL,
    deleted_at      TEXT,
    category        INTEGER NOT NULL DEFAULT 0 CHECK (category IN (0, 1, 2, 3))
);

-- Copy rowid too, so even SQLite's hidden row order is unchanged. Tombstones are copied like any row.
INSERT INTO dues_v4 (rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category)
SELECT rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category
FROM dues;

-- Dropping the old table also drops ix_dues_active_due_date, so it is recreated below.
DROP TABLE dues;
ALTER TABLE dues_v4 RENAME TO dues;

CREATE INDEX ix_dues_active_due_date ON dues (due_date) WHERE deleted_at IS NULL;
