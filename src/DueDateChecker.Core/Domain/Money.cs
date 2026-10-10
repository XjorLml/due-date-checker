using System.Globalization;

namespace DueDateChecker.Core.Domain;

/// <summary>
/// An amount of Philippine pesos stored as whole centavos, so arithmetic never
/// meets binary floating point. Only the two factories can create a value, which
/// is where the rounding rule and the non-negative rule live.
/// </summary>
public readonly record struct Money
{
    public long Centavos { get; }

    private Money(long centavos) => Centavos = centavos;

    public static Money FromCentavos(long centavos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(centavos);
        return new Money(centavos);
    }

    public static Money FromDecimal(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        var centavos = decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return new Money((long)centavos);
    }

    public decimal ToDecimal() => Centavos / 100m;

    // Fixed format on purpose: Android culture data varies by device, and the
    // app is PHP-only (ADR 002).
    public override string ToString() => "₱" + ToDecimal().ToString("N2", CultureInfo.InvariantCulture);
}
