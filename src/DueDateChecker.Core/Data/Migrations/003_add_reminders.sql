-- No due rows or existing categories/timestamps are changed.
CREATE TABLE reminder_settings (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    enabled INTEGER NOT NULL DEFAULT 0 CHECK (enabled IN (0,1)),
    day_before INTEGER NOT NULL DEFAULT 1 CHECK (day_before IN (0,1)),
    minute_of_day INTEGER NOT NULL DEFAULT 540 CHECK (minute_of_day BETWEEN 0 AND 1439),
    quiet_start INTEGER NOT NULL DEFAULT 1320 CHECK (quiet_start BETWEEN 0 AND 1439),
    quiet_end INTEGER NOT NULL DEFAULT 480 CHECK (quiet_end BETWEEN 0 AND 1439)
);
INSERT INTO reminder_settings(id) VALUES(1);
CREATE TABLE reminder_occurrences (
    occurrence_key TEXT PRIMARY KEY,
    due_id TEXT NOT NULL,
    due_date TEXT NOT NULL,
    scheduled_at TEXT NOT NULL,
    delivered_at TEXT
);
