using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public partial class DuesListViewModel(DatabaseDispatcher database, IDueRepository repository) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private IReadOnlyList<DueListItem> _dues = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private string _errorMessage = "";

    public bool CanAdd => IsReady && !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty => IsReady && !IsBusy && !HasError && Dues.Count == 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            await database.InitializeAsync();
            IsReady = true;
            var dues = await database.RunAsync(repository.ListActiveAsync);
            // Replace only after a successful read. A refresh failure retains the old list.
            Dues = dues.Select(due => new DueListItem(due)).ToArray();
        }
        catch (Exception)
        {
            ErrorMessage = IsReady
                ? "Couldn't refresh your dues. The list may be out of date. Try again."
                : "Couldn't open your saved dues. Check available storage or update the app, then try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ReportNavigationError() => ErrorMessage = "Couldn't open the due form. Try again.";
}
