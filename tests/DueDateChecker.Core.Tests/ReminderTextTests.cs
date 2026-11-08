using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Tests;

public sealed class ReminderTextTests
{
    private static Due Item(Money? amount = null, DueCategory category = DueCategory.Task, RepeatRule repeat = RepeatRule.Never) =>
        Due.Create("Rent", amount, new DateOnly(2026, 11, 8), DateTimeOffset.UnixEpoch, "SECRET NOTES", category, repeat);

    [Theory]
    [InlineData(7, "tomorrow")]
    [InlineData(8, "today")]
    [InlineData(6, "on 08 Nov 2026")]
    public void Text_uses_delivery_date_and_never_notes(int day, string when)
    {
        var text = ReminderText.Create(Item(), new(2026, 11, day));
        Assert.Equal($"Rent is due {when}", text.Title);
        Assert.Equal($"You have a due {when}", text.PublicBody);
        Assert.Equal("Task, no amount · Sun 08 Nov", text.Body);
        Assert.DoesNotContain("SECRET", text.ToString());
        Assert.DoesNotContain("Rent", text.PublicBody);
    }

    [Theory]
    [InlineData(RepeatRule.Weekly, "weekly")]
    [InlineData(RepeatRule.Monthly, "monthly")]
    [InlineData(RepeatRule.Yearly, "yearly")]
    public void Zero_is_an_explicit_amount_and_repeat_is_named(RepeatRule repeat, string label)
    {
        var text = ReminderText.Create(Item(Money.FromCentavos(0), DueCategory.Bill, repeat), new(2026, 11, 8));
        Assert.Equal($"₱0.00 · Sun 08 Nov · repeats {label}", text.Body);
    }

    [Fact]
    public void Date_boundary_uses_explicit_date_without_overflow()
    {
        var text = ReminderText.Create(Item() with { DueDate = DateOnly.MinValue }, DateOnly.MaxValue);
        Assert.Equal("Rent is due on 01 Jan 0001", text.Title);
    }

    [Fact]
    public void Fingerprint_detects_same_timestamp_edits_and_absent_versus_zero()
    {
        var due = Item();
        var version = ReminderText.Fingerprint(due);
        Assert.Equal(version, ReminderText.Fingerprint(due with { }));
        foreach (var edited in new[] { due with { Title = "Other" }, due with { Notes = "changed" },
            due with { Amount = Money.FromCentavos(0) }, due with { IsPaid = true },
            due with { DueDate = due.DueDate.AddDays(1) }, due with { DeletedAt = due.UpdatedAt } })
            Assert.NotEqual(version, ReminderText.Fingerprint(edited));
    }
}
