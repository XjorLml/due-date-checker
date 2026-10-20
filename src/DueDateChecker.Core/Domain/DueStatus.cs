namespace DueDateChecker.Core.Domain;

public enum DueStatus { Overdue, DueToday, Upcoming, Paid }

/// <summary>Derived dashboard status for an explicit product calendar date; never persisted.</summary>
public static class DueStatusCalculator
{
    public static DueStatus? Calculate(Due due, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(due);
        if (due.DeletedAt.HasValue) return null;
        if (due.IsPaid) return DueStatus.Paid;
        return due.DueDate < today ? DueStatus.Overdue
            : due.DueDate == today ? DueStatus.DueToday : DueStatus.Upcoming;
    }
}
