-- The existing category integer is still the due type. Do not infer a detail category from old titles.
ALTER TABLE dues ADD COLUMN category_id TEXT NOT NULL DEFAULT 'ph.other.general';
