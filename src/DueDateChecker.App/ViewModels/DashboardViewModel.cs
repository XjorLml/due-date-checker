using System.Globalization;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private static readonly TimeZoneInfo ProductZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    private readonly DatabaseDispatcher _database;
    private readonly IDueRepository _repository;
    private readonly TimeProvider _clock;
    private readonly ObservableCollection<DashboardItem> _visibleItems = [];
    private IReadOnlyList<Due> _dues = [];
    private IReadOnlyList<DashboardItem> _allItems = [];
    private string _filter = "All";
    public string Filter => _filter;
    public IReadOnlyList<string> CategoryFilters { get; } = Array.AsReadOnly(new[] { "All categories", "Bill", "Task", "Subscription", "Other" });
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFilters))]
    private string _categoryFilter = "All categories";

    partial void OnCategoryFilterChanged(string value)
    {
        if (!CategoryFilters.Contains(value)) { CategoryFilter = "All categories"; return; }
        ApplyFilters();
    }


    // Keep the adapter/header stable while typing; replacing ItemsSource destroys
    // Android's focused header Entry after its first character.
    public ReadOnlyObservableCollection<DashboardItem> Items { get; }

    public DashboardViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider clock)
    {
        _database = database;
        _repository = repository;
        _clock = clock;
        Items = new ReadOnlyObservableCollection<DashboardItem>(_visibleItems);
        Today = GetToday(clock);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFilters))]
    private string _query = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    private bool _isReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    private bool _hasLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsNoMatches))]
    private string _errorMessage = "";

    public DateOnly Today { get; private set; }
    public string DateText => $"{Today.ToString("dddd, dd MMM yyyy", CultureInfo.InvariantCulture)} · Asia/Manila";
    public bool CanAdd => IsReady && !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasFilters => !string.IsNullOrWhiteSpace(Query) || Filter != "All" || CategoryFilter != "All categories";
    public bool IsEmpty => HasLoaded && TotalCount == 0 && !IsBusy && !HasError;
    public bool IsNoMatches => HasLoaded && TotalCount > 0 && Items.Count == 0 && !IsBusy && !HasError;
    public int TotalCount => _allItems.Count;
    public int PaidCount => Count(DueStatus.Paid);
    public int TodayCount => Count(DueStatus.DueToday);
    public int OverdueCount => Count(DueStatus.Overdue);
    public int UpcomingCount => Count(DueStatus.Upcoming);
    public int OpenCount => TotalCount - PaidCount;
    public float Completion => TotalCount == 0 ? 0 : (float)PaidCount / TotalCount;
    public string PercentageText => Math.Round(Completion * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
    public string ProgressText => $"{PaidCount} of {TotalCount} saved dues paid";
    public string ProgressDescription => $"{ProgressText}, {PercentageText}";
    public string ResultsHeading => $"{Filter} · {Items.Count}";

    partial void OnQueryChanged(string value) => ApplyFilters();

    public void SelectFilter(string filter)
    {
        if (filter is not ("All" or "Overdue" or "Due today" or "Upcoming" or "Paid")) return;
        if (!SetProperty(ref _filter, filter, nameof(Filter))) return;
        OnPropertyChanged(nameof(HasFilters));
        ApplyFilters();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        Query = "";
        CategoryFilter = "All categories";
        SelectFilter("All");
    }

    private bool CanLoad() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (!CanLoad()) return;
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            await _database.InitializeAsync();
            IsReady = true;
            var dues = await _database.RunAsync(_repository.ListActiveAsync);
            _dues = dues.ToArray();
            HasLoaded = true;
            RebuildSnapshot(GetToday(_clock));
        }
        catch (Exception)
        {
            RefreshDate();
            ErrorMessage = HasLoaded
                ? "Couldn't refresh your dashboard. Saved information may be out of date. Try again."
                : "Couldn't load your saved dues. Check available storage and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void RefreshDate()
    {
        var today = GetToday(_clock);
        if (today != Today) RebuildSnapshot(today);
    }

    private void RebuildSnapshot(DateOnly today)
    {
        Today = today;
        _allItems = Array.AsReadOnly(_dues
            .Select(due => (Due: due, Status: DueStatusCalculator.Calculate(due, today)))
            .Where(item => item.Status.HasValue)
            .OrderBy(item => item.Due.DueDate).ThenBy(item => item.Due.CreatedAt)
            .Select(item => new DashboardItem(item.Due, item.Status!.Value)).ToArray());
        foreach (var name in new[] { nameof(Today), nameof(DateText), nameof(TotalCount), nameof(PaidCount),
            nameof(TodayCount), nameof(OverdueCount), nameof(UpcomingCount), nameof(OpenCount), nameof(Completion),
            nameof(PercentageText), nameof(ProgressText), nameof(ProgressDescription), nameof(IsEmpty) })
            OnPropertyChanged(name);
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = (Query ?? "").Trim();
        var matching = _allItems.Where(item =>
            (Filter == "All" || item.StatusText == Filter) &&
            (CategoryFilter == "All categories" || item.CategoryText == CategoryFilter) &&
            (query.Length == 0 || item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))).ToArray();
        for (var index = 0; index < matching.Length; index++)
        {
            var next = matching[index];
            var existing = -1;
            for (var candidate = index; candidate < _visibleItems.Count; candidate++)
                if (_visibleItems[candidate].Id == next.Id) { existing = candidate; break; }
            if (existing < 0) _visibleItems.Insert(index, next);
            else
            {
                if (existing != index) _visibleItems.Move(existing, index);
                if (_visibleItems[index] != next) _visibleItems[index] = next;
            }
        }
        while (_visibleItems.Count > matching.Length) _visibleItems.RemoveAt(_visibleItems.Count - 1);
        OnPropertyChanged(nameof(ResultsHeading));
        OnPropertyChanged(nameof(IsNoMatches));
    }

    private int Count(DueStatus status) => _allItems.Count(item => item.Status == status);
    private static DateOnly GetToday(TimeProvider time) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(time.GetUtcNow(), ProductZone).DateTime);
    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";
}
