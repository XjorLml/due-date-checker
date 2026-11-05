using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Pages;

public partial class DueEditPage : ContentPage, IQueryAttributable
{
    private readonly DueEditViewModel _viewModel;
    private readonly ToolbarItem _deleteItem;
    private bool _handlingAction;
    private bool _editTargetApplied;

    public DueEditPage(DueEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent, ActionBar);
        // Delete sits in the header (Focus selection); the toolbar item exists only while the
        // loaded due can be deleted, so a new due or an unavailable target never shows it.
        _deleteItem = new ToolbarItem { Text = "Delete", Order = ToolbarItemOrder.Primary, Priority = 0, AutomationId = "EditorDelete" };
        _deleteItem.Clicked += OnDeleteClicked;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(DueEditViewModel.ShowDelete) or nameof(DueEditViewModel.CanDelete)) UpdateDeleteItem();
        };
        UpdateDeleteItem();
        Appearing += async (_, _) => await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void UpdateDeleteItem()
    {
        var present = ToolbarItems.Contains(_deleteItem);
        if (_viewModel.ShowDelete && !present) ToolbarItems.Add(_deleteItem);
        else if (!_viewModel.ShowDelete && present) ToolbarItems.Remove(_deleteItem);
        _deleteItem.IsEnabled = _viewModel.CanDelete;
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
            if (_viewModel.HasCompleted) await ReturnToParentAsync();
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
            await ReturnToParentAsync();
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
            if (_viewModel.HasDeleted) await ReturnToParentAsync();
        }
        finally
        {
            _handlingAction = false;
        }
    }

    private async Task ReturnToParentAsync()
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

    private void OnCategoryUnfocused(object? sender, FocusEventArgs e)
    {
        if (_viewModel.CanEdit) _viewModel.ValidateCategory();
    }

    private void FocusFirstError()
    {
        if (_viewModel.HasTitleError) { TitleEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.TitleError); }
        else if (_viewModel.HasCategoryError) { CategoryPicker.Focus(); SemanticScreenReader.Default.Announce(_viewModel.CategoryError); }
        else if (_viewModel.HasAmountError) { AmountEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.AmountError); }
        else if (_viewModel.HasDateError) { DueDatePicker.Focus(); SemanticScreenReader.Default.Announce(_viewModel.DateError); }
    }

    protected override bool OnBackButtonPressed() => _handlingAction || _viewModel.IsBusy || base.OnBackButtonPressed();
}
