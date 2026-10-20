using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>Creates dues or loads and edits an active due without changing its identity.</summary>
public partial class DueEditViewModel : ObservableObject
{
    private readonly DatabaseDispatcher _database;
    private readonly IDueRepository _repository;
    private readonly TimeProvider _clock;
    private Guid? _editingId;
    private string _loadedTitle = "";

    public DueEditViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider clock)
    {
        _database = database;
        _repository = repository;
        _clock = clock;
        SelectedDate = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila")).Date;
    }

    [ObservableProperty] private string _title = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmountPreview))]
    private string _amountText = "";
    [ObservableProperty] private DateTime? _selectedDate;
    [ObservableProperty] private string _notes = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaymentStatusText))]
    private bool _isPaid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(ShowLoadRetry))]
    [NotifyPropertyChangedFor(nameof(ShowDelete))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isEditMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(ShowLoadRetry))]
    [NotifyPropertyChangedFor(nameof(BusyMessage))]
    [NotifyPropertyChangedFor(nameof(ShowDelete))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isLoaded = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(ShowLoadRetry))]
    [NotifyPropertyChangedFor(nameof(CancelButtonText))]
    [NotifyPropertyChangedFor(nameof(ShowDelete))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isUnavailable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(ShowLoadRetry))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(HasSaved))]
    [NotifyPropertyChangedFor(nameof(HasCompleted))]
    [NotifyPropertyChangedFor(nameof(CancelButtonText))]
    [NotifyPropertyChangedFor(nameof(ShowDelete))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Guid? _savedDueId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCompleted))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(CancelButtonText))]
    [NotifyPropertyChangedFor(nameof(ShowDelete))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _hasDeleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BusyMessage))]
    private bool _isDeleting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTitleError))]
    private string _titleError = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAmountError))]
    private string _amountError = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateError))]
    private string _dateError = "";

    public bool HasSaved => SavedDueId.HasValue;
    public bool HasCompleted => HasSaved || HasDeleted;
    public bool IsIdle => !IsBusy;
    public bool CanEdit => !IsBusy && !HasCompleted && IsLoaded && !IsUnavailable;
    public bool CanSubmit => !IsBusy && (HasCompleted || CanEdit);
    public bool ShowDelete => IsEditMode && IsLoaded && !IsUnavailable && !HasCompleted;
    public bool CanDelete => ShowDelete && !IsBusy;
    public bool ShowLoadRetry => IsEditMode && !IsLoaded && !IsUnavailable && !IsBusy;
    public string PageTitle => IsEditMode ? "Edit due" : "Add due";
    public string PaymentStatusText => IsPaid ? "Paid" : "Unpaid";
    public string CancelButtonText => IsUnavailable || HasCompleted ? "Back to dues" : "Cancel";
    public string BusyMessage => IsDeleting ? "Deleting your due" : IsLoaded ? "Saving your due" : "Loading your due";
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
    public bool HasAmountError => !string.IsNullOrEmpty(AmountError);
    public bool HasDateError => !string.IsNullOrEmpty(DateError);
    public string SaveButtonText => HasCompleted ? "Back to dues" : IsEditMode ? "Save changes" : "Save due";
    public string AmountPreview => TryGetAmount(out var amount) ? amount.ToString() : "";
    public string DeleteConfirmationMessage => $"Delete “{_loadedTitle}”? It will be removed from your dues list. You cannot undo this in the app.";

    // Shell supplies only an id. Malformed edit navigation must never become an insert.
    public void PrepareForEdit(string? id)
    {
        IsEditMode = true;
        IsLoaded = false;
        _editingId = Guid.TryParse(id, out var parsed) && parsed != Guid.Empty ? parsed : null;
        IsUnavailable = !_editingId.HasValue;
        ErrorMessage = IsUnavailable ? "This due is no longer available. Return to your dues." : "";
    }

    private bool CanLoad() => IsEditMode && !IsLoaded && !IsUnavailable && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (!CanLoad()) return;
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            var due = await _database.RunAsync(() => _repository.GetAsync(_editingId!.Value));
            if (due is null || due.DeletedAt.HasValue)
            {
                MarkUnavailable();
                return;
            }

            Title = due.Title;
            _loadedTitle = due.Title;
            AmountText = due.Amount.ToDecimal().ToString("F2", CultureInfo.InvariantCulture);
            SelectedDate = due.DueDate.ToDateTime(TimeOnly.MinValue);
            Notes = due.Notes ?? "";
            IsPaid = due.IsPaid;
            IsLoaded = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Couldn't load this due. Check available storage and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task SaveAsync()
    {
        // Also guard direct ExecuteAsync calls and navigation retries after a successful save.
        if (!CanEdit) return;
        ErrorMessage = "";
        ValidateTitle();
        ValidateAmount(out var amount);
        ValidateDate();
        if (HasTitleError || HasAmountError || HasDateError) return;

        IsBusy = true;
        try
        {
            var title = Title;
            var dueDate = DateOnly.FromDateTime(SelectedDate!.Value);
            var notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
            var isPaid = IsPaid;
            var now = _clock.GetUtcNow();
            Due? due;
            if (IsEditMode)
            {
                due = await _database.RunAsync(async () =>
                {
                    // Read/check/write share the dispatcher gate so App operations cannot interleave.
                    var current = await _repository.GetAsync(_editingId!.Value);
                    if (current is null || current.DeletedAt.HasValue) return null;
                    var edited = current with
                    {
                        Title = title, Amount = amount, DueDate = dueDate, Notes = notes,
                        IsPaid = isPaid, UpdatedAt = now
                    };
                    await _repository.UpdateAsync(edited);
                    return edited;
                });
                if (due is null)
                {
                    MarkUnavailable();
                    return;
                }
            }
            else
            {
                due = Due.Create(title, amount, dueDate, now, notes);
                await _database.RunAsync(() => _repository.AddAsync(due));
            }
            SavedDueId = due.Id;
        }
        catch (KeyNotFoundException) when (IsEditMode)
        {
            // A deletion by another writer after the read is rejected by the repository too.
            MarkUnavailable();
        }
        catch (Exception)
        {
            ErrorMessage = "Couldn't save this due. Your entries are still here. Check available storage and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(bool confirmed)
    {
        // Cancel/dismissal never reaches storage. Also guard direct command calls and retries.
        if (!confirmed || !CanDelete) return;
        IsDeleting = true;
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            var id = _editingId!.Value;
            var when = _clock.GetUtcNow();
            await _database.RunAsync(() => _repository.SoftDeleteAsync(id, when));
            HasDeleted = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Couldn't delete this due. Your entries are still here. Check available storage and try again.";
        }
        finally
        {
            IsBusy = false;
            IsDeleting = false;
        }
    }

    private void MarkUnavailable()
    {
        IsUnavailable = true;
        ErrorMessage = "This due is no longer available. Return to your dues.";
    }

    // Blur and Save share field rules; validation never normalizes input or touches storage.
    public void ValidateTitle() => TitleError = string.IsNullOrWhiteSpace(Title) ? "Enter a title." : "";

    public void ValidateAmount() => ValidateAmount(out _);

    public void ValidateDate() => DateError = SelectedDate.HasValue ? "" : "Choose a due date.";

    private bool ValidateAmount(out Money amount)
    {
        var valid = TryGetAmount(out amount);
        AmountError = valid ? "" : "Enter a valid PHP amount, for example 1250.50, within the supported range.";
        return valid;
    }

    private bool TryGetAmount(out Money amount)
    {
        amount = default;
        var text = AmountText?.Trim() ?? "";
        if (!Regex.IsMatch(text, @"\A[0-9]+(?:\.[0-9]+)?\z", RegexOptions.NonBacktracking)
            || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            return false;

        try
        {
            amount = Money.FromDecimal(value);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    public void ReportNavigationError() => ErrorMessage = HasDeleted
        ? "Your due was deleted. Tap Back to dues to return to the list."
        : "Your due was saved. Tap Back to dues to return to the list.";

    public void ReportDeleteConfirmationError() => ErrorMessage = "Couldn't show the delete confirmation. No due was deleted. Try again.";
}
