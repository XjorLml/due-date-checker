namespace DueDateChecker.App.Pages;

/// <summary>Shared ledger row for Dues, Dashboard and Calendar. Place it inside an item template whose
/// item implements <see cref="ViewModels.IDueRow"/>; it inherits that item as its BindingContext.</summary>
public partial class DueRow : ContentView
{
    public DueRow() => InitializeComponent();
}
