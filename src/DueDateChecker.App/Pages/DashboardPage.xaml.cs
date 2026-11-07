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
        PageLayout.Connect(this, PageContent, extraBottom: 24, heading: Heading);
        _dateTimer = Dispatcher.CreateTimer();
        _dateTimer.Interval = TimeSpan.FromMinutes(1);
        _dateTimer.Tick += (_, _) => _viewModel.RefreshDate();
        Appearing += async (_, _) => { _dateTimer.Start(); await RefreshAsync(); };
        Disappearing += (_, _) => _dateTimer.Stop();
    }

    public Task RefreshAsync()
    {
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    // The counters are the prototype's status links: they open Dues with that status selected.
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string filter } && Shell.Current is AppShell shell) shell.ShowDues(filter);
    }

    private void OnAllDuesClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is AppShell shell) shell.ShowDues(null);
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (_viewModel.NextDue is { } next) await NavigateAsync($"{nameof(DueDetailPage)}?id={next.Id:D}");
    }

    private async void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: DashboardItem item }) await NavigateAsync($"{nameof(DueDetailPage)}?id={item.Id:D}");
    }

    private async void OnRemindersClicked(object? sender, EventArgs e) => await NavigateAsync(nameof(ReminderSettingsPage));

    private async Task NavigateAsync(string route)
    {
        if (_navigating || !_viewModel.CanAdd) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(route); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
