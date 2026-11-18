namespace DueDateChecker.Core.Domain;

// Persisted codes (dues.repeat_rule): never reorder or reuse values.
// TwiceMonthly (PRD TM, 2026-11-16) was appended as 4; it repeats on RepeatDay and RepeatDay2.
public enum RepeatRule { Never = 0, Weekly = 1, Monthly = 2, Yearly = 3, TwiceMonthly = 4 }
