namespace DueDateChecker.App.ViewModels;

/// <summary>What the shared ledger row (<c>Pages/DueRow</c>) needs from a projected due. Both
/// <see cref="DueListItem"/> and <see cref="DashboardItem"/> implement it so Dues, Dashboard and
/// Calendar can share one template without merging the two records.</summary>
public interface IDueRow
{
    Guid Id { get; }
    string Title { get; }
    string CategoryText { get; }
    string AmountText { get; }
    string DateText { get; }
    string ShortDateText { get; }
    string MetaText { get; }
    string StatusText { get; }
    string AccessibilityText { get; }
}
