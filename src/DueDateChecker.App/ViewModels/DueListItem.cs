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
    public string AmountText => Due.Amount.ToString();
    public string DateText => Due.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    public string ShortDateText => Due.DueDate.ToString("dd MMM", CultureInfo.InvariantCulture);
    public string MetaText => $"{ShortDateText} · {CategoryText}";
    public string StatusText => StatusLabel(Status);
    public string AccessibilityText => $"{Title}, {CategoryText}, {StatusText}, due {Due.DueDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}, {AmountText}. Opens due details.";

    public static string StatusLabel(DueStatus status) => status switch
    {
        DueStatus.Overdue => "Overdue",
        DueStatus.DueToday => "Due today",
        DueStatus.Upcoming => "Upcoming",
        _ => "Paid"
    };
}
