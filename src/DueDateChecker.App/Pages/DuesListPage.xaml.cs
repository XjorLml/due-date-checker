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
        PageLayout.Connect(this, PageContent, heading: Heading, scrollsInside: true);
        Appearing += async (_, _) => await RefreshAsync();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(DuesListViewModel.Filter)) UpdateFilterStyles();
        };
        UpdateFilterStyles();
    }

    // The page observes the load task, sharing it with a simultaneous resume event.
    public Task RefreshAsync()
    {
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    /// <summary>Dashboard's counters and "All dues" arrive here (AppShell.ShowDues).</summary>
    public void ShowStatus(string? status) => _viewModel.ShowStatus(status);

    private void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string filter }) _viewModel.SelectFilter(filter);
    }

    private void UpdateFilterStyles()
    {
        foreach (var button in new[] { AllFilter, OverdueFilter, TodayFilter, UpcomingFilter, PaidFilter })
        {
            var selected = Equals(button.CommandParameter, _viewModel.Filter);
            button.Style = (Style)Application.Current!.Resources[selected ? "FilterChipSelected" : "FilterChip"];
            SemanticProperties.SetDescription(button, $"{button.Text} saved dues, {(selected ? "selected" : "not selected")} filter");
        }
    }

    private async void OnRemindersClicked(object? sender, EventArgs e)
        => await OpenPageAsync(nameof(ReminderSettingsPage));

    private async void OnDueSelected(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.OfType<DueListItem>().FirstOrDefault();
        DuesCollection.SelectedItem = null;
        if (item is not null) await OpenPageAsync($"{nameof(DueDetailPage)}?id={item.Id:D}");
    }

    private async Task OpenPageAsync(string route)
    {
        if (_navigating || !_viewModel.CanAdd) return;
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
