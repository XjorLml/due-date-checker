using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>Loads committed fields for one existing due; never writes or restores a row.</summary>
public partial class DueDetailViewModel(DatabaseDispatcher database, IDueRepository repository) : ObservableObject
{
    private Guid? _targetId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetails))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(NotesText))]
    [NotifyPropertyChangedFor(nameof(CreatedText))]
    [NotifyPropertyChangedFor(nameof(UpdatedText))]
    private DueListItem? _item;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(ShowRetry))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isUnavailable = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ShowRetry))]
    private string _errorMessage = "This due is no longer available. Return to your dues.";

    public bool HasDetails => Item is not null;
    public bool CanEdit => HasDetails && !IsBusy && !IsUnavailable;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool ShowRetry => HasError && !IsUnavailable;
    public string NotesText => Item?.Due.Notes is { } notes && !string.IsNullOrWhiteSpace(notes) ? notes : "No notes.";
    public string CreatedText => FormatTimestamp(Item?.Due.CreatedAt);
    public string UpdatedText => FormatTimestamp(Item?.Due.UpdatedAt);

    public void PrepareForDetails(string? id)
    {
        _targetId = Guid.TryParse(id, out var parsed) && parsed != Guid.Empty ? parsed : null;
        Item = null;
        IsUnavailable = !_targetId.HasValue;
        ErrorMessage = IsUnavailable ? "This due is no longer available. Return to your dues." : "";
        LoadCommand.NotifyCanExecuteChanged();
    }

    private bool CanLoad() => _targetId.HasValue && !IsBusy && !IsUnavailable;

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
                IsUnavailable = true;
                ErrorMessage = "This due is no longer available. Return to your dues.";
                return;
            }
            Item = new DueListItem(due);
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

    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";

    private static string FormatTimestamp(DateTimeOffset? value) => value?.ToUniversalTime()
        .ToString("dd MMM yyyy, HH:mm 'UTC'", CultureInfo.InvariantCulture) ?? "";
}
