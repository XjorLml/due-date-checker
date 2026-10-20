using DueDateChecker.App.ViewModels;
using Microsoft.Maui.Dispatching;

namespace DueDateChecker.App.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly IDispatcherTimer _dateTimer;
    private Task? _refreshTask;
    private bool _navigating;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        _dateTimer = Dispatcher.CreateTimer();
        _dateTimer.Interval = TimeSpan.FromMinutes(1);
        _dateTimer.Tick += (_, _) => _viewModel.RefreshDate();
        Appearing += async (_, _) => { _dateTimer.Start(); await RefreshAsync(); };
        Disappearing += (_, _) => _dateTimer.Stop();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.Filter)) UpdateFilterStyles();
        };
        UpdateFilterStyles();
    }

    public Task RefreshAsync()
    {
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string filter }) _viewModel.SelectFilter(filter);
    }

    private void UpdateFilterStyles()
    {
        foreach (var button in new[] { AllFilter, PaidFilter, OverdueFilter, TodayFilter, UpcomingFilter })
        {
            var selected = Equals(button.CommandParameter, _viewModel.Filter);
            button.Style = (Style)Application.Current!.Resources[selected ? "BaseButton" : "SecondaryButton"];
            SemanticProperties.SetDescription(button, $"{button.Text} saved dues, {(selected ? "selected" : "not selected")} filter");
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e) => await NavigateAsync(nameof(DueEditPage));

    private async void OnDueSelected(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.OfType<DashboardItem>().FirstOrDefault();
        Results.SelectedItem = null;
        if (item is not null) await NavigateAsync($"{nameof(DueDetailPage)}?id={item.Id:D}");
    }

    private async Task NavigateAsync(string route)
    {
        if (_navigating || !_viewModel.CanAdd) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(route); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
