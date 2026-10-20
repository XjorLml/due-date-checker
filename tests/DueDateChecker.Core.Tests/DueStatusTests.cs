using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Tests;

public sealed class DueStatusTests
{
    private static readonly DateOnly Today = new(2026, 10, 18);
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(-1, false, DueStatus.Overdue)]
    [InlineData(0, false, DueStatus.DueToday)]
    [InlineData(1, false, DueStatus.Upcoming)]
    [InlineData(-1, true, DueStatus.Paid)]
    [InlineData(0, true, DueStatus.Paid)]
    [InlineData(1, true, DueStatus.Paid)]
    public void Date_groups_are_exclusive_and_paid_takes_precedence(int offset, bool paid, DueStatus expected)
    {
        var due = Due.Create("Boundary", Money.FromCentavos(0), Today.AddDays(offset), Now) with { IsPaid = paid };
        Assert.Equal(expected, DueStatusCalculator.Calculate(due, Today));
        Assert.Equal(paid, due.IsPaid); // Classification never mutates stored payment state.
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deleted_dues_have_no_dashboard_status(bool paid)
    {
        var due = Due.Create("Deleted", Money.FromCentavos(0), Today, Now) with { IsPaid = paid, DeletedAt = Now };
        Assert.Null(DueStatusCalculator.Calculate(due, Today));
    }

    [Fact]
    public void DateOnly_extremes_do_not_require_date_arithmetic()
    {
        var due = Due.Create("Extremes", Money.FromCentavos(0), DateOnly.MinValue, Now);
        Assert.Equal(DueStatus.DueToday, DueStatusCalculator.Calculate(due, DateOnly.MinValue));
        Assert.Equal(DueStatus.Overdue, DueStatusCalculator.Calculate(due, DateOnly.MaxValue));
        due = due with { DueDate = DateOnly.MaxValue };
        Assert.Equal(DueStatus.Upcoming, DueStatusCalculator.Calculate(due, DateOnly.MinValue));
        Assert.Equal(DueStatus.DueToday, DueStatusCalculator.Calculate(due, DateOnly.MaxValue));
    }
}
