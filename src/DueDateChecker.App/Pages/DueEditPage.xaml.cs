using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class DueEditPage : ContentPage, IQueryAttributable
{
    private readonly DueEditViewModel _viewModel;
    private bool _handlingAction;
    private bool _editTargetApplied;

    public DueEditPage(DueEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        Appearing += async (_, _) => await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Query delivery is synchronous; Appearing observes the async load command.
        // Ignore repeat delivery so returning/resuming cannot overwrite unsaved input.
        if (!_editTargetApplied && query.TryGetValue("id", out var id))
        {
            _editTargetApplied = true;
            _viewModel.PrepareForEdit(id?.ToString());
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_handlingAction || _viewModel.IsBusy) return;
        _handlingAction = true;
        try
        {
            await _viewModel.SaveCommand.ExecuteAsync(null);
            if (_viewModel.HasCompleted) await ReturnToListAsync();
        }
        finally
        {
            _handlingAction = false;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        if (_handlingAction || _viewModel.IsBusy) return;
        _handlingAction = true;
        try
        {
            await ReturnToListAsync();
        }
        finally
        {
            _handlingAction = false;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_handlingAction || !_viewModel.CanDelete) return;
        _handlingAction = true;
        try
        {
            bool confirmed;
            try
            {
                confirmed = await DisplayAlertAsync("Delete due?", _viewModel.DeleteConfirmationMessage, "Delete", "Cancel");
            }
            catch (Exception)
            {
                _viewModel.ReportDeleteConfirmationError();
                return;
            }

            await _viewModel.DeleteCommand.ExecuteAsync(confirmed);
            if (_viewModel.HasDeleted) await ReturnToListAsync();
        }
        finally
        {
            _handlingAction = false;
        }
    }

    private async Task ReturnToListAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception)
        {
            if (_viewModel.HasCompleted) _viewModel.ReportNavigationError();
            else await DisplayAlertAsync("Couldn't return", "Try the back button to return to your dues.", "OK");
        }
    }

    protected override bool OnBackButtonPressed() => _handlingAction || _viewModel.IsBusy || base.OnBackButtonPressed();
}
