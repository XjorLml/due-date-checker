-- F07 recurring dues (ADR 009). Additive: existing rows read as Never and are otherwise unchanged.
-- repeat_rule: 0 Never, 1 Weekly, 2 Monthly, 3 Yearly. repeat_day: the anchor day of month for
-- Monthly/Yearly. repeat_source_id: the due an occurrence was generated from.
-- The category/repeat and anchor rules are app-enforced in Due.Validate(), like the amount rule.
ALTER TABLE dues ADD COLUMN repeat_rule INTEGER NOT NULL DEFAULT 0 CHECK (repeat_rule IN (0, 1, 2, 3));
ALTER TABLE dues ADD COLUMN repeat_day INTEGER CHECK (repeat_day BETWEEN 1 AND 31);
ALTER TABLE dues ADD COLUMN repeat_source_id TEXT;

-- Serves the "one occurrence per source" check in SqliteDueRepository.UpdateWithNextAsync.
-- Partial: only generated occurrences have a source, so most rows stay out of the index.
CREATE INDEX ix_dues_repeat_source ON dues (repeat_source_id) WHERE repeat_source_id IS NOT NULL;
