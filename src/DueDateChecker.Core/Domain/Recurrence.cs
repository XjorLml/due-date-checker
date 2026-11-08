namespace DueDateChecker.Core.Domain;

/// <summary>
/// Date arithmetic for recurring dues (ADR 009). Monthly and Yearly use an anchor day so a due on
/// the 31st returns to the 31st after a short month instead of drifting to the 28th for good.
/// </summary>
public static class Recurrence
{
    /// <summary>The anchor stored with a due: its day of month for Monthly/Yearly, otherwise none.</summary>
    public static int? AnchorFor(RepeatRule repeat, DateOnly dueDate) =>
        repeat is RepeatRule.Monthly or RepeatRule.Yearly ? dueDate.Day : null;

    /// <summary>
    /// The anchor after an edit (RC06): a new due date sets a new anchor, and an unchanged date keeps
    /// the stored one, so editing the 28 Feb occurrence of a "31st" due keeps it on the 31st.
    /// </summary>
    public static int? AnchorAfterEdit(Due current, RepeatRule repeat, DateOnly dueDate) =>
        repeat is RepeatRule.Monthly or RepeatRule.Yearly && dueDate == current.DueDate && current.RepeatDay is { } kept
            ? kept
            : AnchorFor(repeat, dueDate);

    /// <summary>
    /// The occurrence to add when <paramref name="current"/> is saved as <paramref name="saved"/>:
    /// only an unpaid → paid change of a repeating due creates one. Null otherwise.
    /// </summary>
    public static Due? NextOnPayment(Due current, Due saved, DateTimeOffset now) =>
        !current.IsPaid && saved.IsPaid ? NextOccurrence(saved, now) : null;

    /// <summary>The next due date, or null for Never or when it would pass 31 Dec 9999.</summary>
    public static DateOnly? NextDate(DateOnly current, RepeatRule repeat, int? anchorDay)
    {
        switch (repeat)
        {
            case RepeatRule.Weekly:
                return current.DayNumber + 7 <= DateOnly.MaxValue.DayNumber ? current.AddDays(7) : null;
            case RepeatRule.Monthly:
                if (current is { Year: 9999, Month: 12 }) return null;
                var year = current.Month == 12 ? current.Year + 1 : current.Year;
                var month = current.Month == 12 ? 1 : current.Month + 1;
                return OnAnchor(year, month, anchorDay ?? current.Day);
            case RepeatRule.Yearly:
                return current.Year == 9999 ? null : OnAnchor(current.Year + 1, current.Month, anchorDay ?? current.Day);
            default:
                return null;
        }
    }

    /// <summary>
    /// The occurrence after <paramref name="due"/>: same title, amount, notes, type and repeat, unpaid,
    /// created at <paramref name="now"/> and linked to its source. Null when there is no next date.
    /// </summary>
    public static Due? NextOccurrence(Due due, DateTimeOffset now)
    {
        if (NextDate(due.DueDate, due.Repeat, due.RepeatDay) is not { } date) return null;
        var next = new Due(Guid.NewGuid(), due.Title, due.Amount, date, due.Notes, IsPaid: false,
            CreatedAt: now, UpdatedAt: now, DeletedAt: null, due.Category, due.Repeat, due.RepeatDay, RepeatSourceId: due.Id,
            CategoryId: due.CategoryId);
        next.Validate();
        return next;
    }

    // A day that does not exist in the target month falls back to that month's last day.
    private static DateOnly OnAnchor(int year, int month, int day) => new(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));
}
