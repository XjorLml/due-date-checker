using System.Globalization;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>Row projection shared by Dues, Details, Dashboard and Calendar. Status is the F02
/// date status (Paid / Overdue / Due today / Upcoming) computed against the product calendar date.</summary>
public sealed record DueListItem(Due Due, DueStatus Status) : IDueRow
{
    /// <summary>Projects an active due for the given Manila date. A paid due is Paid regardless of date.</summary>
    public static DueListItem Create(Due due, DateOnly today)
        => new(due, DueStatusCalculator.Calculate(due, today) ?? (due.IsPaid ? DueStatus.Paid : DueStatus.Upcoming));

    public Guid Id => Due.Id;
    public string Title => Due.Title;
    public string CategoryText => Due.Category.ToString();
    public string DetailedCategoryText => PhilippineCategories.Get(Due.CategoryId).Name;
    public bool HasDetailedCategory => Due.CategoryId != PhilippineCategories.OtherId;
    /// <summary>Shown wherever a due has no amount (F07). Zero is never shown this way; it stays ₱0.00.</summary>
    public const string NoAmountText = "No amount";

    public string AmountText => Due.Amount?.ToString() ?? NoAmountText;
    /// <summary>UX14: Details shows an absent amount as quiet text, not in the large amount style.</summary>
    public bool HasAmount => Due.Amount is not null;
    public string DateText => Due.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    public string ShortDateText => Due.DueDate.ToString("dd MMM", CultureInfo.InvariantCulture);
    // F07 recurrence (RC07): the repeat is part of the spoken text; Never adds nothing. Since the
    // prototype alignment (2026-11-07) rows show it on its own "⟲ Every month" line, not in the meta line.
    public bool IsRepeating => Due.Repeat != RepeatRule.Never;
    public string MetaText => $"{ShortDateText} · {(HasDetailedCategory ? DetailedCategoryText : CategoryText)}";
    /// <summary>The row's repeat line (prototype "⟲ Every month"); empty for a due that does not repeat.</summary>
    public string RepeatLineText => Due.Repeat switch
    {
        RepeatRule.Weekly => "Every week",
        RepeatRule.Monthly => "Every month",
        RepeatRule.Yearly => "Every year",
        _ => ""
    };
    /// <summary>The row and Details icon for the due type (prototype type glyphs).</summary>
    public string TypeGlyph => HasDetailedCategory ? PhilippineCategories.Get(Due.CategoryId).Icon : Due.Category switch
    {
        DueCategory.Bill => "bill",
        DueCategory.Task => "task",
        DueCategory.Subscription => "subscription",
        _ => "other"
    };
    /// <summary>The Details "Repeats" value. Yearly names the anchor day, which can differ from this
    /// occurrence's date (29 Feb in a year that ends on the 28th).</summary>
    public string RepeatFactText => Due.Repeat switch
    {
        RepeatRule.Weekly => "Every week",
        RepeatRule.Monthly => $"Every month on day {Due.RepeatDay}",
        RepeatRule.Yearly => $"Every year on {Due.RepeatDay:00} {Due.DueDate.ToString("MMM", CultureInfo.InvariantCulture)}",
        _ => ""
    };
    public string StatusText => StatusLabel(Status);
    public string AccessibilityText => Spoken(Due.DueDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture));

    /// <summary>Spoken row text with the date in the caller's format; Dashboard keeps its shorter date.</summary>
    internal string Spoken(string dueDate)
    {
        var category = HasDetailedCategory ? $"{CategoryText}, {DetailedCategoryText}" : CategoryText;
        return IsRepeating
            ? $"{Title}, {category}, repeats {Due.Repeat.ToString().ToLowerInvariant()}, {StatusText}, due {dueDate}, {AmountText}. Opens due details."
            : $"{Title}, {category}, {StatusText}, due {dueDate}, {AmountText}. Opens due details.";
    }

    public static string StatusLabel(DueStatus status) => status switch
    {
        DueStatus.Overdue => "Overdue",
        DueStatus.DueToday => "Due today",
        DueStatus.Upcoming => "Upcoming",
        _ => "Paid"
    };
}
