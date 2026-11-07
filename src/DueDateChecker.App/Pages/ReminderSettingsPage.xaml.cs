using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class ReminderSettingsPage : ContentPage
{
    private readonly ReminderSettingsViewModel _viewModel;
    private Task? _refresh;
    private bool _readinessShownOnce;

    public ReminderSettingsPage(ReminderSettingsViewModel viewModel)
    {
        InitializeComponent(); BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent, ActionBar);
        Appearing += async (_, _) => { await RefreshAsync(); ShowStepsWhenOn(); };
    }

    public Task RefreshAsync() => _refresh is { IsCompleted: false } ? _refresh : _refresh = _viewModel.LoadCommand.ExecuteAsync(null);

    private void OnReadinessToggled(object? sender, EventArgs e) => SetReadiness(!ReadinessBody.IsVisible);

    // UX04 keeps the maker steps visible while reminders are on: open the section once if there are steps to follow.
    private void ShowStepsWhenOn()
    {
        if (_readinessShownOnce || !_viewModel.Enabled || !_viewModel.HasMakerSteps) return;
        _readinessShownOnce = true;
        SetReadiness(true);
    }

    private void SetReadiness(bool open)
    {
        ReadinessBody.IsVisible = open;
        ReadinessGlyph.Kind = open ? "minus" : "plus";
    }
}
