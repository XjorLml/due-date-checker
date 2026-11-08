using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly DataTransferViewModel _data;
    private bool _navigating;

    public SettingsPage(SettingsViewModel viewModel, DataTransferViewModel data)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        DataCard.BindingContext = _data = data;
        PageLayout.Connect(this, PageContent, SaveBar, heading: Heading);
        Appearing += async (_, _) => { Refresh(); await _viewModel.RefreshReminderSummaryAsync(); };
    }

    public void Refresh() => _viewModel.LoadCommand.Execute(null);

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        if (!_data.CanAct) return;
        await _data.ExportCommand.ExecuteAsync(null);
        AnnounceDataMessage();
    }

    // UX05: pick and check the file first, then ask, then write. Cancel or a failed dialog writes nothing.
    private async void OnImportClicked(object? sender, EventArgs e)
    {
        if (!_data.CanAct) return;
        await _data.ChooseImportFileCommand.ExecuteAsync(null);
        if (_data.HasPendingImport)
        {
            bool confirmed;
            try
            {
                confirmed = await DisplayAlertAsync("Import dues?", _data.ImportConfirmationMessage, "Import", "Cancel");
            }
            catch (Exception)
            {
                confirmed = false;
            }
            await _data.ImportCommand.ExecuteAsync(confirmed);
        }
        AnnounceDataMessage();
    }

    // The result appears in the card silently, so screen-reader users hear it.
    private void AnnounceDataMessage()
    {
        if (!string.IsNullOrEmpty(_data.Message)) SemanticScreenReader.Default.Announce(_data.Message);
    }

    private void OnRemindersClicked(object? sender, EventArgs e) => OnRemindersTapped(sender, null);

    private async void OnRemindersTapped(object? sender, TappedEventArgs? e)
    {
        if (_navigating) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(nameof(ReminderSettingsPage)); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
