-- v1 schema. currency and timezone are fixed for now (ADR 002) but stored per row
-- so a future multi-currency or sync feature needs no table rewrite.
CREATE TABLE dues (
    id              TEXT PRIMARY KEY,                       -- Guid, 36-char text
    title           TEXT NOT NULL,
    amount_centavos INTEGER NOT NULL,
    currency        TEXT NOT NULL DEFAULT 'PHP',
    due_date        TEXT NOT NULL,                          -- 'yyyy-MM-dd' sorts as text
    notes           TEXT,
    is_paid         INTEGER NOT NULL DEFAULT 0,
    timezone        TEXT NOT NULL DEFAULT 'Asia/Manila',
    created_at      TEXT NOT NULL,                          -- ISO 8601 round-trip ("O") with offset
    updated_at      TEXT NOT NULL,
    deleted_at      TEXT                                    -- NULL = active (soft delete)
);

-- The list screen's query: WHERE deleted_at IS NULL ORDER BY due_date.
-- A partial index covers exactly those rows, so the query walks the index
-- in order instead of scanning and sorting the table.
CREATE INDEX ix_dues_active_due_date ON dues (due_date) WHERE deleted_at IS NULL;
