using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class DuesListPage : ContentPage
{
    private readonly DuesListViewModel _viewModel;
    private Task? _refreshTask;
    private bool _navigating;

    public DuesListPage(DuesListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        Appearing += async (_, _) => await RefreshAsync();
    }

    // The page observes the load task, sharing it with a simultaneous resume event.
    public Task RefreshAsync()
    {
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnRemindersClicked(object? sender, EventArgs e)
        => await OpenPageAsync(nameof(ReminderSettingsPage));

    private async void OnSettingsClicked(object? sender, EventArgs e)
        => await OpenPageAsync(nameof(SettingsPage), requiresDues: false);

    private async void OnAddClicked(object? sender, EventArgs e)
        => await OpenPageAsync(nameof(DueEditPage));

    private async void OnDueSelected(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.OfType<DueListItem>().FirstOrDefault();
        DuesCollection.SelectedItem = null;
        if (item is not null) await OpenPageAsync($"{nameof(DueDetailPage)}?id={item.Id:D}");
    }

    private async Task OpenPageAsync(string route, bool requiresDues = true)
    {
        if (_navigating || requiresDues && !_viewModel.CanAdd) return;
        _navigating = true;
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception)
        {
            _viewModel.ReportNavigationError();
        }
        finally
        {
            _navigating = false;
        }
    }
}
