using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public sealed record DashboardItem(Due Due, DueStatus Status) : IDueRow
{
    private DueListItem Row { get; } = new(Due, Status);
    public Guid Id => Due.Id;
    public string Title => Row.Title;
    public string CategoryText => Row.CategoryText;
    public string AmountText => Row.AmountText;
    public bool HasAmount => Row.HasAmount;
    public string DateText => Row.DateText;
    public string ShortDateText => Row.ShortDateText;
    public string MetaText => Row.MetaText;
    public bool IsRepeating => Row.IsRepeating;
    public string RepeatLineText => Row.RepeatLineText;
    public string TypeGlyph => Row.TypeGlyph;
    public string StatusText => Row.StatusText;
    public string AccessibilityText => Row.Spoken(DateText);
}
