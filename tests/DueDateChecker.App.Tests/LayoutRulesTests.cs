using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX Wave 5 sizing rules (UX13 bar captions, UX15 calendar cells, UX16 headings) and UX14's flag.</summary>
public sealed class LayoutRulesTests
{
    // ---- UX13: captions follow the system text size up to a cap, and shrink a little only to fit ----

    [Theory]
    [InlineData(1.0, 80, 59, 1.0)]    // normal text, roomy slot
    [InlineData(1.15, 80, 59, 1.15)]  // follows the system up to the cap
    [InlineData(2.0, 80, 59, 1.3)]    // 200% text stops at the cap...
    [InlineData(2.0, 70, 59, 70.0 / 59)] // ...or earlier when the longest caption would not fit
    [InlineData(1.0, 56, 59, 56.0 / 59)] // a 320 dp phone: shrink slightly so "Dashboard" fits
    [InlineData(1.0, 40, 59, 0.85)]   // never below the floor; past that, truncation is accepted
    [InlineData(0.85, 80, 59, 0.85)]  // a smaller system size is respected
    public void Caption_scale_follows_the_system_within_the_cap_and_the_slot(double fontScale, double slot, double longest, double expected)
    {
        Assert.Equal(expected, LayoutRules.CaptionScale(fontScale, slot, longest), precision: 6);
    }

    [Fact]
    public void Caption_scale_without_a_measurement_uses_the_capped_system_size()
    {
        Assert.Equal(1.3, LayoutRules.CaptionScale(2.0, 0, 0));
        Assert.Equal(1.0, LayoutRules.CaptionScale(1.0, 80, 0));
    }

    // ---- UX15: day cells shrink only when seven 44 dp columns cannot fit ----

    [Theory]
    [InlineData(292, 40)]  // 320 dp phone with 14 dp margins
    [InlineData(320, 44)]  // 360 dp phone with 20 dp margins: exactly fits
    [InlineData(400, 44)]  // wider stays at the 44 dp rule
    [InlineData(306, 42)]
    [InlineData(-1, 44)]   // not measured yet
    [InlineData(100, 24)]  // never below WCAG's 24 px minimum target
    public void Day_cells_take_the_44_dp_rule_unless_the_row_cannot_fit(double gridWidth, double expected)
    {
        Assert.Equal(expected, LayoutRules.DayCellMinimumWidth(gridWidth));
    }

    // ---- UX16: compact root headings when wider than tall ----

    [Theory]
    [InlineData(411, 914, 26)]  // portrait
    [InlineData(914, 411, 20)]  // landscape
    [InlineData(600, 600, 26)]  // square counts as portrait
    [InlineData(-1, -1, 26)]
    public void Root_headings_are_compact_only_in_landscape(double width, double height, double expected)
    {
        Assert.Equal(expected, LayoutRules.RootHeadingSize(width, height));
    }

    // ---- UX14: Details can tell an absent amount from a peso amount ----

    [Fact]
    public void A_row_says_whether_it_has_an_amount()
    {
        var now = new DateTimeOffset(2026, 11, 7, 1, 0, 0, TimeSpan.Zero);
        var task = Due.Create("Renew ID", null, new DateOnly(2026, 11, 20), now, category: DueCategory.Task);
        var zero = Due.Create("Free month", Money.FromCentavos(0), new DateOnly(2026, 11, 20), now, category: DueCategory.Subscription);

        Assert.False(DueListItem.Create(task, new DateOnly(2026, 11, 7)).HasAmount);
        Assert.True(DueListItem.Create(zero, new DateOnly(2026, 11, 7)).HasAmount); // zero is a recorded amount
    }
}
