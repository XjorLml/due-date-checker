namespace DueDateChecker.Core.Domain;

/// <summary>
/// Date arithmetic for recurring dues (ADR 009). Monthly and Yearly use an anchor day so a due on
/// the 31st returns to the 31st after a short month instead of drifting to the 28th for good.
/// </summary>
public static class Recurrence
{
    /// <summary>Twice a month starts on 15 and 30 unless the user picks other days (PRD TM).</summary>
    public static readonly (int First, int Second) DefaultTwiceMonthlyDays = (15, 30);

    /// <summary>
    /// The stored days for a repeat: the anchor for Monthly/Yearly, the two picked days in ascending
    /// order for TwiceMonthly (the default pair when none is given), otherwise none.
    /// </summary>
    public static (int? First, int? Second) DaysFor(RepeatRule repeat, DateOnly dueDate, (int, int)? twiceMonthlyDays) =>
        repeat == RepeatRule.TwiceMonthly
            ? Ordered(twiceMonthlyDays ?? DefaultTwiceMonthlyDays)
            : (AnchorFor(repeat, dueDate), null);

    private static (int? First, int? Second) Ordered((int A, int B) days) => (Math.Min(days.A, days.B), Math.Max(days.A, days.B));

    /// <summary>The anchor stored with a due: its day of month for Monthly/Yearly, otherwise none.</summary>
    public static int? AnchorFor(RepeatRule repeat, DateOnly dueDate) =>
        repeat is RepeatRule.Monthly or RepeatRule.Yearly ? dueDate.Day : null;

    /// <summary>
    /// The anchor after an edit (RC06): a new due date sets a new anchor, and an unchanged date keeps
    /// the stored one, so editing the 28 Feb occurrence of a "31st" due keeps it on the 31st. Only a
    /// Monthly/Yearly anchor is kept: a twice-a-month due's first day is not an anchor (PRD TM).
    /// </summary>
    public static int? AnchorAfterEdit(Due current, RepeatRule repeat, DateOnly dueDate) =>
        repeat is RepeatRule.Monthly or RepeatRule.Yearly && dueDate == current.DueDate
            && current.Repeat is RepeatRule.Monthly or RepeatRule.Yearly && current.RepeatDay is { } kept
            ? kept
            : AnchorFor(repeat, dueDate);

    /// <summary>
    /// The occurrence to add when <paramref name="current"/> is saved as <paramref name="saved"/>:
    /// only an unpaid → paid change of a repeating due creates one. Null otherwise.
    /// </summary>
    public static Due? NextOnPayment(Due current, Due saved, DateTimeOffset now) =>
        !current.IsPaid && saved.IsPaid ? NextOccurrence(saved, now) : null;

    /// <summary>The next due date, or null for Never or when it would pass 31 Dec 9999.</summary>
    /// <param name="secondDay">TwiceMonthly only: the later of the two days; <paramref name="anchorDay"/> is the earlier.</param>
    public static DateOnly? NextDate(DateOnly current, RepeatRule repeat, int? anchorDay, int? secondDay = null)
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
            case RepeatRule.TwiceMonthly:
                return NextTwiceMonthly(current, anchorDay, secondDay);
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
        if (NextDate(due.DueDate, due.Repeat, due.RepeatDay, due.RepeatDay2) is not { } date) return null;
        var next = new Due(Guid.NewGuid(), due.Title, due.Amount, date, due.Notes, IsPaid: false,
            CreatedAt: now, UpdatedAt: now, DeletedAt: null, due.Category, due.Repeat, due.RepeatDay, RepeatSourceId: due.Id,
            CategoryId: due.CategoryId, RepeatDay2: due.RepeatDay2);
        next.Validate();
        return next;
    }

    // TM03–TM05: the earliest of the two days strictly after the current date, so an off-schedule first
    // date moves to the next picked day. Both days clamp to the month's length; when they land on the
    // same date (29 and 30 in February) the month has one occurrence and the next is the following month.
    private static DateOnly? NextTwiceMonthly(DateOnly current, int? firstDay, int? secondDay)
    {
        if (firstDay is not { } a || secondDay is not { } b) return null;
        var (first, second) = (Math.Min(a, b), Math.Max(a, b));
        var candidates = new List<DateOnly> { OnAnchor(current.Year, current.Month, first), OnAnchor(current.Year, current.Month, second) };
        if (current is not { Year: 9999, Month: 12 })
        {
            candidates.Add(current.Month == 12 ? OnAnchor(current.Year + 1, 1, first) : OnAnchor(current.Year, current.Month + 1, first));
        }
        foreach (var candidate in candidates)
        {
            if (candidate > current) return candidate;
        }
        return null;
    }

    // A day that does not exist in the target month falls back to that month's last day.
    private static DateOnly OnAnchor(int year, int month, int day) => new(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));
}
