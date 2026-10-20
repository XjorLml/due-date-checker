using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class ReminderSettingsPage : ContentPage
{
    private readonly ReminderSettingsViewModel _viewModel;
    private Task? _refresh;
    public ReminderSettingsPage(ReminderSettingsViewModel viewModel)
    {
        InitializeComponent(); BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        Appearing += async (_, _) => await RefreshAsync();
    }
    public Task RefreshAsync() => _refresh is { IsCompleted: false } ? _refresh : _refresh = _viewModel.LoadCommand.ExecuteAsync(null);
}
