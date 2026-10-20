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
        PageLayout.Connect(this, PageContent);
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
            else FocusFirstError();
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

    private void OnTitleUnfocused(object? sender, FocusEventArgs e)
    {
        if (_viewModel.CanEdit) _viewModel.ValidateTitle();
    }

    private void OnAmountUnfocused(object? sender, FocusEventArgs e)
    {
        if (_viewModel.CanEdit) _viewModel.ValidateAmount();
    }

    private void OnDateUnfocused(object? sender, FocusEventArgs e)
    {
        if (_viewModel.CanEdit) _viewModel.ValidateDate();
    }

    private void FocusFirstError()
    {
        if (_viewModel.HasTitleError) { TitleEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.TitleError); }
        else if (_viewModel.HasAmountError) { AmountEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.AmountError); }
        else if (_viewModel.HasDateError) { DueDatePicker.Focus(); SemanticScreenReader.Default.Announce(_viewModel.DateError); }
    }

    protected override bool OnBackButtonPressed() => _handlingAction || _viewModel.IsBusy || base.OnBackButtonPressed();
}
