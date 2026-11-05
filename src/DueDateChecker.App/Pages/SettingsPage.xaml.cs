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
        PageLayout.Connect(this, PageContent, extraBottom: 24);
        Appearing += async (_, _) => { Refresh(); await _viewModel.RefreshReminderSummaryAsync(); };
    }

    public void Refresh() => _viewModel.LoadCommand.Execute(null);

    private async void OnRemindersTapped(object? sender, TappedEventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(nameof(ReminderSettingsPage)); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
