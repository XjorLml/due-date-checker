-- Existing untyped rows, including tombstones, remain otherwise unchanged.
ALTER TABLE dues ADD COLUMN category INTEGER NOT NULL DEFAULT 0
    CHECK (category IN (0, 1, 2, 3));
