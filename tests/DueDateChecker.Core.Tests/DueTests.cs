using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Tests;

public class DueTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 9, 0, 0, TimeSpan.FromHours(8));

    [Fact]
    public void Create_sets_defaults_from_the_supplied_clock()
    {
        var due = Due.Create("Meralco", Money.FromDecimal(2450m), new DateOnly(2026, 10, 15), Now);

        Assert.NotEqual(Guid.Empty, due.Id);
        Assert.Equal("Meralco", due.Title);
        Assert.Equal(Money.FromCentavos(245000), due.Amount);
        Assert.Equal(new DateOnly(2026, 10, 15), due.DueDate);
        Assert.Null(due.Notes);
        Assert.False(due.IsPaid);
        Assert.Equal(Now, due.CreatedAt);
        Assert.Equal(Now, due.UpdatedAt);
        Assert.Null(due.DeletedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string title)
    {
        Assert.Throws<ArgumentException>(() => Due.Create(title, Money.FromCentavos(0), new DateOnly(2026, 10, 15), Now));
    }

    [Fact]
    public void Create_trims_the_title()
    {
        var due = Due.Create("  Rent ", Money.FromCentavos(0), new DateOnly(2026, 10, 15), Now);

        Assert.Equal("Rent", due.Title);
    }

    [Fact]
    public void With_expression_still_validates_title()
    {
        var due = Due.Create("Rent", Money.FromCentavos(0), new DateOnly(2026, 10, 15), Now);

        Assert.Throws<ArgumentException>(() => due with { Title = " " });
    }

    [Fact]
    public void Two_dues_created_separately_get_different_ids()
    {
        var a = Due.Create("A", Money.FromCentavos(0), new DateOnly(2026, 10, 15), Now);
        var b = Due.Create("B", Money.FromCentavos(0), new DateOnly(2026, 10, 15), Now);

        Assert.NotEqual(a.Id, b.Id);
    }
}
