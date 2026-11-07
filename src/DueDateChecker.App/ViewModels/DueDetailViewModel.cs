using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>Loads committed fields for one existing due. Its only write is the UX01 payment
/// toggle, which re-reads the row inside the database gate and never restores a deleted row.</summary>
public partial class DueDetailViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider? clock = null) : ObservableObject
{
    private const string UnavailableMessage = "This due is no longer available. Return to your dues.";
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private Guid? _targetId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetails))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanTogglePaid))]
    [NotifyPropertyChangedFor(nameof(PaidActionText))]
    [NotifyPropertyChangedFor(nameof(PaidActionHint))]
    [NotifyPropertyChangedFor(nameof(NotesText))]
    [NotifyPropertyChangedFor(nameof(CreatedText))]
    [NotifyPropertyChangedFor(nameof(UpdatedText))]
    [NotifyCanExecuteChangedFor(nameof(TogglePaidCommand))]
    private DueListItem? _item;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanTogglePaid))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePaidCommand))]
    private bool _isBusy;

    // Separate from IsBusy so a payment save never shows the "Loading due details" state.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanTogglePaid))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePaidCommand))]
    private bool _isSaving;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanTogglePaid))]
    [NotifyPropertyChangedFor(nameof(ShowRetry))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePaidCommand))]
    private bool _isUnavailable = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ShowRetry))]
    private string _errorMessage = UnavailableMessage;

    // Payment failures are reported apart from ErrorMessage, whose Retry button reloads.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPaymentError))]
    private string _paymentError = "";

    // F07 recurrence (RC04): shown under the payment action so sighted users see what was added.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNextOccurrenceText))]
    private string _nextOccurrenceText = "";

    /// <summary>Text the page announces to screen readers after a successful toggle.</summary>
    public string PaymentAnnouncement { get; private set; } = "";

    public bool HasDetails => Item is not null;
    public bool CanEdit => HasDetails && !IsBusy && !IsSaving && !IsUnavailable;
    public bool CanTogglePaid => CanEdit;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasPaymentError => !string.IsNullOrEmpty(PaymentError);
    public bool HasNextOccurrenceText => !string.IsNullOrEmpty(NextOccurrenceText);
    public bool ShowRetry => HasError && !IsUnavailable;
    public string PaidActionText => Item?.Due.IsPaid == true ? "Mark as unpaid" : "Mark as paid";
    public string PaidActionHint => Item?.Due.IsPaid == true
        ? "Saves this due as unpaid now"
        : Item?.IsRepeating == true
            ? "Saves this due as paid now and adds the next one; it records a status, not a payment"
            : "Saves this due as paid now; it records a status, not a payment";
    public string NotesText => Item?.Due.Notes is { } notes && !string.IsNullOrWhiteSpace(notes) ? notes : "No notes.";
    public string CreatedText => FormatTimestamp(Item?.Due.CreatedAt);
    public string UpdatedText => FormatTimestamp(Item?.Due.UpdatedAt);

    public void PrepareForDetails(string? id)
    {
        _targetId = Guid.TryParse(id, out var parsed) && parsed != Guid.Empty ? parsed : null;
        Item = null;
        IsUnavailable = !_targetId.HasValue;
        ErrorMessage = IsUnavailable ? UnavailableMessage : "";
        PaymentError = "";
        NextOccurrenceText = "";
        LoadCommand.NotifyCanExecuteChanged();
    }

    private bool CanLoad() => _targetId.HasValue && !IsBusy && !IsSaving && !IsUnavailable;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (!CanLoad()) return;
        IsBusy = true;
        Item = null;
        ErrorMessage = "";
        try
        {
            var due = await database.RunAsync(() => repository.GetAsync(_targetId!.Value));
            if (due is null || due.DeletedAt.HasValue)
            {
                MarkUnavailable();
                return;
            }
            Item = DueListItem.Create(due, ManilaCalendar.Today(_clock));
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

    /// <summary>UX01: one tap saves the opposite of the shown payment status.</summary>
    [RelayCommand(CanExecute = nameof(CanTogglePaid))]
    private async Task TogglePaidAsync()
    {
        // Also guards direct ExecuteAsync calls and taps that arrive while a save is running.
        if (!CanTogglePaid) return;
        var id = Item!.Id;
        var markPaid = !Item.Due.IsPaid; // the intent is what the user saw, not a blind flip
        IsSaving = true;
        PaymentError = "";
        NextOccurrenceText = "";
        try
        {
            var (saved, added) = await database.RunAsync<(Due? Saved, Due? Added)>(async () =>
            {
                // Read/check/write share the dispatcher gate, like the editor's Save.
                var current = await repository.GetAsync(id);
                if (current is null || current.DeletedAt.HasValue) return (null, null);
                if (current.IsPaid == markPaid) return (current, null); // already done elsewhere: no write
                var now = _clock.GetUtcNow();
                var edited = current with { IsPaid = markPaid, UpdatedAt = now };
                // RC04/RC05: paying a repeating due also adds its next occurrence in the same
                // transaction; the repository skips it when this due already produced one.
                if (Recurrence.NextOnPayment(current, edited, now) is { } next)
                {
                    return (edited, await repository.UpdateWithNextAsync(edited, next) ? next : null);
                }
                await repository.UpdateAsync(edited);
                return (edited, null);
            });
            if (saved is null)
            {
                Item = null;
                MarkUnavailable();
                return;
            }
            Item = DueListItem.Create(saved, ManilaCalendar.Today(_clock));
            NextOccurrenceText = added is null ? "" : $"Next one added for {added.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}.";
            PaymentAnnouncement = !saved.IsPaid ? "Marked unpaid."
                : added is null ? "Marked paid."
                : $"Marked paid. {NextOccurrenceText}";
        }
        catch (KeyNotFoundException)
        {
            // A deletion that lands between the read and the update is refused by the repository.
            Item = null;
            MarkUnavailable();
        }
        catch (Exception)
        {
            PaymentError = "Couldn't update the payment status. Nothing was changed. Try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void MarkUnavailable()
    {
        IsUnavailable = true;
        ErrorMessage = UnavailableMessage;
    }

    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";

    /// <summary>Whether this page is about <paramref name="id"/>, even while it shows the due as unavailable.</summary>
    public bool ShowsDue(Guid id) => _targetId == id;

    /// <summary>UX06: after an Undo restored this due, load it again even though it was shown as unavailable.</summary>
    public Task ReloadAsync()
    {
        PrepareForDetails(_targetId?.ToString("D"));
        return LoadCommand.ExecuteAsync(null);
    }

    // UX12 (decided 2026-11-06): stored UTC instants are shown in Asia/Manila and say so.
    private static string FormatTimestamp(DateTimeOffset? value) => value is { } instant
        ? TimeZoneInfo.ConvertTime(instant, ManilaCalendar.Zone).ToString("dd MMM yyyy, HH:mm 'Manila'", CultureInfo.InvariantCulture)
        : "";
}
