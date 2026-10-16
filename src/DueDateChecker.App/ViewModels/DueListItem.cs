using System.Globalization;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public sealed record DueListItem(Due Due)
{
    public Guid Id => Due.Id;
    public string Title => Due.Title;
    public string AmountText => Due.Amount.ToString();
    public string DateText => Due.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    public string StatusText => Due.IsPaid ? "Paid" : "Unpaid";
}
