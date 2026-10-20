using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private bool _navigating;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        Appearing += (_, _) => Refresh();
    }

    public void Refresh() => _viewModel.LoadCommand.Execute(null);

    private async void OnRemindersClicked(object? sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(nameof(ReminderSettingsPage)); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
