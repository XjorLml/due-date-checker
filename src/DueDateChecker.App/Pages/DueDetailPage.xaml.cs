using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class DueDetailPage : ContentPage, IQueryAttributable
{
    private readonly DueDetailViewModel _viewModel;
    private Task? _refreshTask;
    private bool _targetApplied;
    private bool _navigating;
    private readonly Services.IMessageHost _messages;

    public DueDetailPage(DueDetailViewModel viewModel, Services.IMessageHost messages)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _messages = messages;
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

    /// <summary>UX06: after an Undo, show the restored due again even if this page had marked it unavailable.</summary>
    public Task RefreshAfterRestoreAsync(Guid restoredId) => _viewModel.ShowsDue(restoredId) ? _viewModel.ReloadAsync() : RefreshAsync();

    private async void OnBackClicked(object? sender, EventArgs e) => await NavigateAsync("..");

    private async void OnTogglePaidClicked(object? sender, EventArgs e)
    {
        if (_navigating || !_viewModel.CanTogglePaid) return;
        await _viewModel.TogglePaidCommand.ExecuteAsync(null);
        // A failure stays inline and is announced; success shows the UX06 message, which announces itself.
        if (_viewModel.HasPaymentError) SemanticScreenReader.Default.Announce(_viewModel.PaymentError);
        else if (_viewModel.HasDetails) _messages.Show(_viewModel.PaymentAnnouncement);
    }

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
