using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>The Dues start page.
/// <para>UX02 (decided 2026-11-06): paid dues dated before today's Manila date fold into one
/// "Show N earlier paid dues" row so what is next comes first; everything else keeps the R03
/// ascending date order.</para>
/// <para>Prototype alignment (decided 2026-11-07, PRD "UX09 (revised)"): search and the status and
/// type filters live here with the former Dashboard rules. The fold applies only while none is active,
/// so a search or filter always shows every match.</para></summary>
public partial class DuesListViewModel : ObservableObject
{
    public const string AllTypes = "All types";
    private readonly DatabaseDispatcher _database;
    private readonly IDueRepository _repository;
    private readonly TimeProvider _clock;
    private readonly ObservableCollection<DueListItem> _visible = [];
    // Last successful snapshot of every active due; the visible list is derived from it.
    private IReadOnlyList<DueListItem> _all = [];
    private DateOnly _today;
    private string _filter = "All";

    public DuesListViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider? clock = null)
    {
        _database = database;
        _repository = repository;
        _clock = clock ?? TimeProvider.System;
        // One bound collection updated in place: replacing ItemsSource would destroy the focused
        // search Entry in the list header after its first character (Android).
        Dues = new ReadOnlyObservableCollection<DueListItem>(_visible);
    }

    public ReadOnlyObservableCollection<DueListItem> Dues { get; }
    public IReadOnlyList<string> TypeFilters { get; } = Array.AsReadOnly(new[] { AllTypes, "Bill", "Task", "Subscription", "Other" });
    public string Filter => _filter;

    [ObservableProperty]
    private string _query = "";

    [ObservableProperty]
    private string _typeFilter = AllTypes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsAllFolded))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    private bool _isReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsAllFolded))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsAllFolded))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    private string _errorMessage = "";

    // Session-only: a restart starts folded again.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EarlierPaidText))]
    [NotifyPropertyChangedFor(nameof(EarlierPaidDescription))]
    [NotifyPropertyChangedFor(nameof(EarlierPaidLabel))]
    private bool _showEarlierPaid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEarlierPaid))]
    [NotifyPropertyChangedFor(nameof(EarlierPaidText))]
    [NotifyPropertyChangedFor(nameof(EarlierPaidDescription))]
    [NotifyPropertyChangedFor(nameof(EarlierPaidLabel))]
    private int _earlierPaidCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MatchCountText))]
    private int _matchCount;

    public bool CanAdd => IsReady && !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasFilters => !string.IsNullOrWhiteSpace(Query) || Filter != "All" || TypeFilter != AllTypes;
    /// <summary>No active dues at all (the first-run empty state).</summary>
    public bool IsEmpty => IsReady && !IsBusy && !HasError && _all.Count == 0;
    /// <summary>Active dues exist, no search or filter is active, and all of them are folded earlier paid dues.</summary>
    public bool IsAllFolded => IsReady && !IsBusy && !HasError && _all.Count > 0 && !HasFilters && _visible.Count == 0;
    /// <summary>Active dues exist but none matches the search and filters.</summary>
    public bool IsNoMatches => IsReady && !IsBusy && !HasError && _all.Count > 0 && HasFilters && MatchCount == 0;
    public string MatchCountText => MatchCount == 1 ? "1 matching due" : $"{MatchCount} matching dues";
    public bool HasEarlierPaid => EarlierPaidCount > 0;
    public string EarlierPaidText => ShowEarlierPaid ? "Hide earlier paid dues"
        : EarlierPaidCount == 1 ? "Show 1 earlier paid due" : $"Show {EarlierPaidCount} earlier paid dues";
    /// <summary>Button text: a chevron invites expanding; the expanded state reads as plain text.</summary>
    public string EarlierPaidLabel => ShowEarlierPaid ? EarlierPaidText : $"{EarlierPaidText}  ›";
    private string PaidDuesPhrase => EarlierPaidCount == 1 ? "1 paid due" : $"{EarlierPaidCount} paid dues";
    public string EarlierPaidDescription => ShowEarlierPaid
        ? $"Hide {PaidDuesPhrase} dated before today"
        : $"Show {PaidDuesPhrase} dated before today. They are folded so what is next comes first";

    partial void OnQueryChanged(string value) => Apply();

    partial void OnTypeFilterChanged(string value)
    {
        if (!TypeFilters.Contains(value)) { TypeFilter = AllTypes; return; }
        Apply();
    }

    /// <summary>Selects one status filter: All, Overdue, Due today, Upcoming or Paid. Anything else is ignored.</summary>
    public void SelectFilter(string filter)
    {
        if (filter is not ("All" or "Overdue" or "Due today" or "Upcoming" or "Paid")) return;
        if (!SetProperty(ref _filter, filter, nameof(Filter))) return;
        Apply();
    }

    /// <summary>Dashboard's counters and "All dues" land here: clears the search and type, then selects the status.</summary>
    public void ShowStatus(string? status)
    {
        Query = "";
        TypeFilter = AllTypes;
        SelectFilter(status ?? "All");
    }

    [RelayCommand]
    private void ClearFilters() => ShowStatus(null);

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            await _database.InitializeAsync();
            IsReady = true;
            var dues = await _database.RunAsync(_repository.ListActiveAsync);
            // Replace only after a successful read. A refresh failure retains the old list.
            _today = ManilaCalendar.Today(_clock);
            _all = dues.Select(due => DueListItem.Create(due, _today))
                .OrderBy(item => item.Due.DueDate).ThenBy(item => item.Due.CreatedAt).ToArray();
            Apply();
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

    [RelayCommand]
    private void ToggleEarlierPaid()
    {
        ShowEarlierPaid = !ShowEarlierPaid;
        Apply();
    }

    private bool IsEarlierPaid(DueListItem item) => item.Due.IsPaid && item.Due.DueDate < _today;

    // Filtering a date-ordered snapshot keeps the order, so expanded rows appear in place.
    private void Apply()
    {
        var query = (Query ?? "").Trim();
        var matching = _all.Where(item =>
            (Filter == "All" || item.StatusText == Filter) &&
            (TypeFilter == AllTypes || item.CategoryText == TypeFilter) &&
            (query.Length == 0 || item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))).ToArray();
        var fold = !HasFilters;
        EarlierPaidCount = fold ? matching.Count(IsEarlierPaid) : 0;
        var visible = fold && !ShowEarlierPaid ? matching.Where(item => !IsEarlierPaid(item)).ToArray() : matching;
        Sync(visible);
        MatchCount = matching.Length;
        OnPropertyChanged(nameof(HasFilters));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsAllFolded));
        OnPropertyChanged(nameof(IsNoMatches));
    }

    // Move, insert and remove in place, so the bound collection never resets.
    private void Sync(IReadOnlyList<DueListItem> target)
    {
        for (var index = 0; index < target.Count; index++)
        {
            var next = target[index];
            var existing = -1;
            for (var candidate = index; candidate < _visible.Count; candidate++)
                if (_visible[candidate].Id == next.Id) { existing = candidate; break; }
            if (existing < 0) _visible.Insert(index, next);
            else
            {
                if (existing != index) _visible.Move(existing, index);
                if (_visible[index] != next) _visible[index] = next;
            }
        }
        while (_visible.Count > target.Count) _visible.RemoveAt(_visible.Count - 1);
    }

    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";
}
