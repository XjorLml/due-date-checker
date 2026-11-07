using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Pages;

public partial class DueEditPage : ContentPage, IQueryAttributable
{
    private readonly DueEditViewModel _viewModel;
    private readonly IMessageHost _messages;
    private readonly DeletionUndo _undo;
    private bool _handlingAction;
    private bool _editTargetApplied;

    public DueEditPage(DueEditViewModel viewModel, IMessageHost messages, DeletionUndo undo)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _messages = messages;
        _undo = undo;
        PageLayout.Connect(this, PageContent, ActionBar);
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
            if (_viewModel.HasCompleted)
            {
                await ReturnToParentAsync();
                // UX06: shown after returning, so it sits above the bottom bar the parent page brings back.
                // The message itself is announced; nothing else is.
                if (_viewModel.HasSaved && _viewModel.CompletionMessage is { Length: > 0 } message) _messages.Show(message);
            }
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
            if (await ConfirmLeaveAsync()) await ReturnToParentAsync();
        }
        finally
        {
            _handlingAction = false;
        }
    }

    /// <summary>UX22 (decided 2026-11-07): with changes, ask before discarding; Keep editing is the safe choice.
    /// Without changes, leave at once.</summary>
    private async Task<bool> ConfirmLeaveAsync()
    {
        if (!_viewModel.HasChanges) return true;
        try
        {
            return await DisplayAlertAsync("Discard changes?", "What you typed here will not be saved.", "Discard", "Keep editing");
        }
        catch (Exception)
        {
            return false; // could not ask: keep the input
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
            if (_viewModel.HasDeleted)
            {
                await ReturnToParentAsync();
                if (_viewModel.Deletion is { } deletion) _undo.Offer(deletion); // "Due deleted." with Undo
            }
        }
        finally
        {
            _handlingAction = false;
        }
    }

    // Prototype "Type: Bill · Change": the four types in a native sheet; Cancel keeps the current type.
    private async void OnChangeTypeClicked(object? sender, EventArgs e)
    {
        if (_handlingAction || !_viewModel.CanEdit) return;
        string? choice;
        try { choice = await DisplayActionSheetAsync("Due type", "Cancel", null, "Bill", "Subscription", "Task", "Other"); }
        catch (Exception) { return; }
        if (Enum.TryParse<DueCategory>(choice, out var type) && _viewModel.CanEdit)
        {
            _viewModel.SelectedCategory = type;
            _viewModel.ValidateCategory();
        }
    }

    private void OnRepeatToggled(object? sender, EventArgs e) => Toggle(RepeatBody, RepeatGlyph);
    private void OnNotesToggled(object? sender, EventArgs e) => Toggle(NotesBody, NotesGlyph);

    // A collapsible editor row (prototype disclosure): the summary row stays; the field shows below it.
    private static void Toggle(VisualElement body, Glyph glyph)
    {
        body.IsVisible = !body.IsVisible;
        glyph.Kind = body.IsVisible ? "minus" : "plus";
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

    private void FocusFirstError()
    {
        if (_viewModel.HasTitleError) { TitleEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.TitleError); }
        else if (_viewModel.HasCategoryError) { ChangeTypeButton.Focus(); SemanticScreenReader.Default.Announce(_viewModel.CategoryError); }
        else if (_viewModel.HasAmountError) { AmountEntry.Focus(); SemanticScreenReader.Default.Announce(_viewModel.AmountError); }
        else if (_viewModel.HasDateError) { DueDatePicker.Focus(); SemanticScreenReader.Default.Announce(_viewModel.DateError); }
    }

    // System Back: blocked while busy; with changes it asks first (UX22), otherwise Shell pops the page.
    protected override bool OnBackButtonPressed()
    {
        if (_handlingAction || _viewModel.IsBusy) return true;
        if (!_viewModel.HasChanges) return base.OnBackButtonPressed();
        _handlingAction = true;
        Dispatcher.Dispatch(async () =>
        {
            try { if (await ConfirmLeaveAsync()) await ReturnToParentAsync(); }
            finally { _handlingAction = false; }
        });
        return true;
    }

    // Android pans the whole window by default, which leaves the fixed Cancel/Save row under the
    // on-screen keyboard. While the editor is visible the window resizes instead, so the row sits
    // above the keyboard. Root pages keep panning, so the tab bar does not ride on the keyboard.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetSoftInputAdjust(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
    }

    protected override void OnDisappearing()
    {
        SetSoftInputAdjust(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Pan);
        base.OnDisappearing();
    }

    private static void SetSoftInputAdjust(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust mode)
    {
        if (Application.Current is not { } app) return;
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(app, mode);
    }
}
