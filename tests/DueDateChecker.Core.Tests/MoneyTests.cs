using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(2450.005, 245001)]   // exact half rounds away from zero
    [InlineData(2450.004, 245000)]
    [InlineData(0.015, 2)]
    [InlineData(0, 0)]
    public void FromDecimal_rounds_half_away_from_zero(decimal amount, long expectedCentavos)
    {
        var money = Money.FromDecimal(amount);

        Assert.Equal(expectedCentavos, money.Centavos);
    }

    [Fact]
    public void FromDecimal_rejects_negative_amounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.FromDecimal(-0.01m));
    }

    [Fact]
    public void FromCentavos_rejects_negative_amounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.FromCentavos(-1));
    }

    [Fact]
    public void ToString_uses_fixed_peso_format()
    {
        var money = Money.FromCentavos(245000);

        Assert.Equal("₱2,450.00", money.ToString());
    }

    [Fact]
    public void ToDecimal_round_trips()
    {
        Assert.Equal(2450.00m, Money.FromCentavos(245000).ToDecimal());
    }

    [Fact]
    public void Equality_is_by_value()
    {
        Assert.Equal(Money.FromDecimal(10m), Money.FromCentavos(1000));
        Assert.NotEqual(Money.FromCentavos(1000), Money.FromCentavos(1001));
    }
}
