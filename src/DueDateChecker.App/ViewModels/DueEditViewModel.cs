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
        _baseline = Snapshot();
    }

    // UX22 (decided 2026-11-07 with the prototype alignment): the form as it opened, so leaving can
    // tell whether it would discard anything. Text compares trimmed, so whitespace alone is no change.
    private readonly record struct FormSnapshot(string Title, DueCategory? Category, string Amount, DateTime? Date, RepeatRule? Repeat, string Notes, bool IsPaid);
    private FormSnapshot _baseline;
    private FormSnapshot Snapshot() => new((Title ?? "").Trim(), SelectedCategory, (AmountText ?? "").Trim(), SelectedDate?.Date, SelectedRepeat, (Notes ?? "").Trim(), IsPaid);
    /// <summary>True when leaving now would discard typed input: the form differs from how it opened.
    /// False while loading, after Save or Delete, and for an unavailable due.</summary>
    public bool HasChanges => CanEdit && Snapshot() != _baseline;

    // Prototype editor rows (PA07).
    public string TypeRowText => $"Type: {(SelectedCategory is { } type && Enum.IsDefined(type) ? type.ToString() : "choose one")}";
    public string RepeatSummary => (SelectedRepeat ?? RepeatRule.Never) switch
    {
        RepeatRule.Weekly => "Every week",
        RepeatRule.Monthly => "Every month",
        RepeatRule.Yearly => "Every year",
        _ => "Doesn't repeat"
    };
    public string NotesSummary => string.IsNullOrWhiteSpace(Notes) ? "Optional" : "Notes added";
    public string AmountPreviewLine => AmountPreview switch
    {
        "" => "",
        DueListItem.NoAmountText => "Will save with no amount.",
        var preview => $"Will save as {preview}."
    };
    public string DateHintText => SelectedDate is { } date
        ? $"{date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)} · Manila. Past dates are allowed."
        : "Choose a date · Manila. Past dates are allowed.";

    public IReadOnlyList<DueCategory> Categories { get; } = Array.AsReadOnly(new[] { DueCategory.Bill, DueCategory.Task, DueCategory.Subscription, DueCategory.Other });
    public IReadOnlyList<RepeatRule> Repeats { get; } = Array.AsReadOnly(new[] { RepeatRule.Never, RepeatRule.Weekly, RepeatRule.Monthly, RepeatRule.Yearly });
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChooseRepeat))]
    [NotifyPropertyChangedFor(nameof(IsAmountRequired))]
    [NotifyPropertyChangedFor(nameof(AmountRequirementText))]
    [NotifyPropertyChangedFor(nameof(AmountPlaceholder))]
    [NotifyPropertyChangedFor(nameof(AmountDescription))]
    [NotifyPropertyChangedFor(nameof(AmountPreview))]
    [NotifyPropertyChangedFor(nameof(AmountPreviewDescription))]
    [NotifyPropertyChangedFor(nameof(AmountPreviewLine))]
    [NotifyPropertyChangedFor(nameof(TypeRowText))]
    [NotifyPropertyChangedFor(nameof(AmountFieldTag))]
    private DueCategory? _selectedCategory = DueCategory.Bill;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCategoryError))]
    private string _categoryError = "";
    public bool HasCategoryError => !string.IsNullOrEmpty(CategoryError);

    // F07 recurrence (RC06). Nullable like the category because a Picker can clear its selection;
    // null saves as Never.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepeatHelpText))]
    [NotifyPropertyChangedFor(nameof(RepeatSummary))]
    private RepeatRule? _selectedRepeat = RepeatRule.Never;
    public bool CanChooseRepeat => SelectedCategory is { } category && Enum.IsDefined(category) && Due.CanRepeat(category);
    public string RepeatHelpText => (SelectedRepeat ?? RepeatRule.Never) switch
    {
        RepeatRule.Weekly => "When you mark it paid, the next one is added 7 days later.",
        RepeatRule.Monthly => "When you mark it paid, the next one is added a month later. Shorter months use their last day.",
        RepeatRule.Yearly => "When you mark it paid, the next one is added a year later.",
        _ => "Doesn't repeat. Bill and Subscription can repeat weekly, monthly or yearly."
    };
    /// <summary>UX06: the confirmation the page shows after a successful Save or Delete; empty otherwise.</summary>
    public string CompletionMessage { get; private set; } = "";
    /// <summary>UX06: the deletion just saved, so the page can offer Undo for exactly that one.</summary>
    public DueDeletion? Deletion { get; private set; }

    [ObservableProperty] private string _title = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmountPreview))]
    [NotifyPropertyChangedFor(nameof(AmountPreviewDescription))]
    [NotifyPropertyChangedFor(nameof(AmountPreviewLine))]
    private string _amountText = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateHintText))]
    private DateTime? _selectedDate;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotesSummary))]
    private string _notes = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaymentStatusText))]
    private bool _isPaid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(IsAddMode))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(SaveLabel))]
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
    [NotifyPropertyChangedFor(nameof(SaveLabel))]
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
    [NotifyPropertyChangedFor(nameof(SaveLabel))]
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
    public bool IsAddMode => !IsEditMode;
    public string PaymentStatusText => IsPaid ? "Paid" : "Unpaid";
    public string CancelButtonText => IsUnavailable || HasCompleted ? "Go back" : "Cancel";
    public string BusyMessage => IsDeleting ? "Deleting your due" : IsLoaded ? "Saving your due" : "Loading your due";
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
    public bool HasAmountError => !string.IsNullOrEmpty(AmountError);
    public bool HasDateError => !string.IsNullOrEmpty(DateError);
    public string SaveButtonText => HasCompleted ? "Go back" : IsEditMode ? "Save changes" : "Save due";
    /// <summary>The footer button text in the prototype style ("✓  Save"); SaveButtonText stays its spoken name.</summary>
    public string SaveLabel => HasCompleted ? "Go back" : "✓  Save";
    /// <summary>The "required" / "optional" tag after the Amount label.</summary>
    public string AmountFieldTag => IsAmountRequired ? "required" : "optional";
    // F07 optional amounts: Bill/Subscription need an amount, Task/Other may leave it blank.
    // A missing or unsupported category is treated as required, the safer reading.
    private string? CategoryName => SelectedCategory is { } category && Enum.IsDefined(category) ? category.ToString() : null;
    public bool IsAmountRequired => CategoryName is null || Due.RequiresAmount(SelectedCategory!.Value);
    private bool IsAmountBlank => string.IsNullOrWhiteSpace(AmountText);
    public string AmountRequirementText => IsAmountRequired
        ? CategoryName is { } name ? $"Required for {name}." : "Required."
        : $"Optional for {CategoryName}. Blank means no amount; zero is a recorded amount.";
    // Short on purpose: a single-line Entry cannot wrap, and at 200% text a longer hint was cut off.
    public string AmountPlaceholder => IsAmountRequired ? "1250.50" : "Optional";
    public string AmountDescription => IsAmountRequired
        ? CategoryName is { } name ? $"Amount in Philippine pesos, required for {name}" : "Amount in Philippine pesos, required"
        : $"Amount in Philippine pesos, optional for {CategoryName}. Leave blank for no amount";
    public string AmountPreview => IsAmountBlank && !IsAmountRequired
        ? DueListItem.NoAmountText
        : TryGetAmount(out var amount) ? amount.ToString() : "";
    public string AmountPreviewDescription => AmountPreview switch
    {
        "" => "",
        DueListItem.NoAmountText => "Will be saved with no amount",
        var preview => $"Amount to save after rounding to centavos, {preview}"
    };
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
            AmountText = due.Amount is { } amount ? amount.ToDecimal().ToString("F2", CultureInfo.InvariantCulture) : "";
            SelectedDate = due.DueDate.ToDateTime(TimeOnly.MinValue);
            Notes = due.Notes ?? "";
            IsPaid = due.IsPaid;
            SelectedCategory = due.Category;
            SelectedRepeat = due.Repeat;
            IsLoaded = true;
            _baseline = Snapshot();
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
        ValidateCategory();
        if (HasTitleError || HasAmountError || HasDateError || HasCategoryError) return;

        IsBusy = true;
        CompletionMessage = "";
        try
        {
            var title = Title;
            var dueDate = DateOnly.FromDateTime(SelectedDate!.Value);
            var notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
            var isPaid = IsPaid;
            var category = SelectedCategory!.Value;
            // The picker is hidden for Task/Other and reset on a type change; this also covers a stale value.
            var repeat = Due.CanRepeat(category) ? SelectedRepeat ?? RepeatRule.Never : RepeatRule.Never;
            var now = _clock.GetUtcNow();
            Due? due;
            if (IsEditMode)
            {
                Due? added;
                (due, added) = await _database.RunAsync<(Due? Saved, Due? Added)>(async () =>
                {
                    // Read/check/write share the dispatcher gate so App operations cannot interleave.
                    var current = await _repository.GetAsync(_editingId!.Value);
                    if (current is null || current.DeletedAt.HasValue) return (null, null);
                    var edited = current with
                    {
                        Title = title, Amount = amount, DueDate = dueDate, Notes = notes,
                        IsPaid = isPaid, UpdatedAt = now, Category = category,
                        Repeat = repeat, RepeatDay = Recurrence.AnchorAfterEdit(current, repeat, dueDate)
                    };
                    // RC04: switching an unpaid repeating due to paid here adds its next one too.
                    if (Recurrence.NextOnPayment(current, edited, now) is { } next)
                    {
                        return (edited, await _repository.UpdateWithNextAsync(edited, next) ? next : null);
                    }
                    await _repository.UpdateAsync(edited);
                    return (edited, null);
                });
                if (due is null)
                {
                    MarkUnavailable();
                    return;
                }
                CompletionMessage = added is null
                    ? "Changes saved."
                    : $"Saved. Next one added for {added.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}.";
            }
            else
            {
                due = Due.Create(title, amount, dueDate, now, notes, category, repeat);
                await _database.RunAsync(() => _repository.AddAsync(due));
                CompletionMessage = "Due saved.";
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
        CompletionMessage = "";
        Deletion = null;
        try
        {
            var id = _editingId!.Value;
            var when = _clock.GetUtcNow();
            await _database.RunAsync(() => _repository.SoftDeleteAsync(id, when));
            // The stamp is what the repository stored, so an Undo can match exactly this deletion.
            Deletion = new DueDeletion(id, when);
            CompletionMessage = "Due deleted.";
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
    public void ValidateCategory() => CategoryError = SelectedCategory is { } category && Enum.IsDefined(category) ? "" : "Choose a category.";

    public void ValidateTitle() => TitleError = string.IsNullOrWhiteSpace(Title) ? "Enter a title." : "";

    public void ValidateAmount() => ValidateAmount(out _);

    public void ValidateDate() => DateError = SelectedDate.HasValue ? "" : "Choose a due date.";

    // A shown amount error is re-checked when the type changes, so a blank amount stops being an
    // error once the type allows no amount. No new error appears before blur or Save.
    partial void OnSelectedCategoryChanged(DueCategory? value)
    {
        if (HasAmountError) ValidateAmount();
        // RC06: Task and Other cannot repeat, so leaving Bill/Subscription clears the choice.
        if (!CanChooseRepeat) SelectedRepeat = RepeatRule.Never;
    }

    /// <summary>Blank is "no amount" (null) for Task/Other and a field error for Bill/Subscription.
    /// Typed text must still be a valid amount; it is never read as zero.</summary>
    private bool ValidateAmount(out Money? amount)
    {
        amount = null;
        if (IsAmountBlank)
        {
            AmountError = !IsAmountRequired ? ""
                : CategoryName is { } name ? $"Enter an amount for this {name}, for example 1250.50."
                : "Enter an amount, for example 1250.50.";
            return !IsAmountRequired;
        }

        var valid = TryGetAmount(out var parsed);
        if (valid) amount = parsed;
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
        ? "Your due was deleted. Tap Go back to return."
        : "Your due was saved. Tap Go back to return.";

    public void ReportDeleteConfirmationError() => ErrorMessage = "Couldn't show the delete confirmation. No due was deleted. Try again.";
}
