using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public sealed record DashboardItem(Due Due, DueStatus Status)
{
    private DueListItem Row { get; } = new(Due);
    public Guid Id => Due.Id;
    public string Title => Row.Title;
    public string CategoryText => Row.CategoryText;
    public string AmountText => Row.AmountText;
    public string DateText => Row.DateText;
    public string StatusText => Status switch
    {
        DueStatus.Overdue => "Overdue",
        DueStatus.DueToday => "Due today",
        DueStatus.Upcoming => "Upcoming",
        _ => "Paid"
    };
    public string AccessibilityText => $"{Title}, {CategoryText}, {StatusText}, due {DateText}, {AmountText}. Opens due details.";
}
