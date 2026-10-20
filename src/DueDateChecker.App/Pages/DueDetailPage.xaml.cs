using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class DueDetailPage : ContentPage, IQueryAttributable
{
    private readonly DueDetailViewModel _viewModel;
    private Task? _refreshTask;
    private bool _targetApplied;
    private bool _navigating;

    public DueDetailPage(DueDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        Appearing += async (_, _) => await RefreshAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (_targetApplied) return;
        _targetApplied = true;
        _viewModel.PrepareForDetails(query.TryGetValue("id", out var id) ? id?.ToString() : null);
    }

    public Task RefreshAsync()
    {
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (!_viewModel.CanEdit) return;
        await NavigateAsync($"{nameof(DueEditPage)}?id={_viewModel.Item!.Id:D}");
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await NavigateAsync("..");

    private async Task NavigateAsync(string route)
    {
        if (_navigating) return;
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
