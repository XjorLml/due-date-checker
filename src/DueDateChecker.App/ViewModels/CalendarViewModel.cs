using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public sealed record CalendarDay(DateOnly? Date, int Count, bool IsSelected, bool IsToday)
{
    public string Indicator => Count == 0 ? "" : Count.ToString(CultureInfo.InvariantCulture);
    public string AccessibilityText => Date is { } date
        ? $"{date.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture)}, {Count} saved dues{(IsSelected ? ", selected" : "")}{(IsToday ? ", today" : "")}"
        : "";
}

public partial class CalendarViewModel : ObservableObject
{
    private static readonly TimeZoneInfo ProductZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    private readonly DatabaseDispatcher _database;
    private readonly IDueRepository _repository;
    private readonly TimeProvider _clock;
    private readonly AppPreferencesService _preferences;
    private readonly ObservableCollection<DashboardItem> _agenda = [];
    private IReadOnlyList<Due> _dues = [];

    public CalendarViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider clock, AppPreferencesService preferences)
    {
        _database = database;
        _repository = repository;
        _clock = clock;
        _preferences = preferences;
        WeekStart = preferences.Current.WeekStart;
        Today = GetToday();
        SelectedDate = Today;
        Month = new(Today.Year, Today.Month, 1);
        Agenda = new ReadOnlyObservableCollection<DashboardItem>(_agenda);
        RebuildCalendar();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNavigate))]
    private bool _isReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNavigate))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsDayEmpty))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsDayEmpty))]
    private bool _hasLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsDayEmpty))]
    private string _errorMessage = "";

    public DateOnly Today { get; private set; }
    public WeekStartPreference WeekStart { get; private set; }
    public IReadOnlyList<string> Weekdays => WeekStart == WeekStartPreference.Sunday
        ? ["Su", "M", "Tu", "W", "Th", "F", "Sa"] : ["M", "Tu", "W", "Th", "F", "Sa", "Su"];
    public string WeekStartDescription => $"{WeekStart} first · Numbers count all saved dues, including Paid";
    public DateOnly Month { get; private set; }
    public DateOnly SelectedDate { get; private set; }
    public IReadOnlyList<CalendarDay> Days { get; private set; } = [];
    public ReadOnlyObservableCollection<DashboardItem> Agenda { get; }
    public string MonthText => Month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
    public string SelectedDateText => SelectedDate.ToString("dddd, dd MMM yyyy", CultureInfo.InvariantCulture);
    public string TodayDescription => $"Return to today, {Today.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}, Asia/Manila";
    public string AgendaSummary => $"{Agenda.Count} saved {(Agenda.Count == 1 ? "due" : "dues")}";
    public bool CanGoPrevious => Month > DateOnly.MinValue;
    public bool CanGoNext => Month < new DateOnly(9999, 12, 1);
    public bool CanNavigate => IsReady && !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty => HasLoaded && _dues.Count == 0 && !IsBusy && !HasError;
    public bool IsDayEmpty => HasLoaded && _dues.Count > 0 && Agenda.Count == 0 && !IsBusy && !HasError;

    public void MoveMonth(int direction)
    {
        if (direction is not (-1 or 1) || direction < 0 && !CanGoPrevious || direction > 0 && !CanGoNext) return;
        Month = Month.AddMonths(direction);
        SelectedDate = Month;
        RebuildCalendar();
    }

    public void SelectDate(DateOnly date)
    {
        if (date.Year != Month.Year || date.Month != Month.Month || date == SelectedDate) return;
        SelectedDate = date;
        RebuildCalendar();
    }

    public void ResetToday()
    {
        Today = GetToday();
        SelectedDate = Today;
        Month = new(Today.Year, Today.Month, 1);
        RebuildCalendar();
    }

    public void RefreshDate()
    {
        var today = GetToday();
        if (today == Today) return;
        Today = today; // Preserve the user's viewed month/day at midnight.
        RebuildCalendar();
    }

    public void RefreshPreferences()
    {
        if (WeekStart == _preferences.Current.WeekStart) return;
        WeekStart = _preferences.Current.WeekStart;
        RebuildCalendar(); // Keep the viewed month/date and saved-data snapshot.
    }

    private bool CanLoad() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (!CanLoad()) return;
        RefreshPreferences();
        IsBusy = true;
        ErrorMessage = "";
        try
        {
            await _database.InitializeAsync();
            IsReady = true;
            var dues = await _database.RunAsync(_repository.ListActiveAsync);
            _dues = Array.AsReadOnly(dues.Where(due => due.DeletedAt is null)
                .OrderBy(due => due.DueDate).ThenBy(due => due.CreatedAt).ToArray());
            Today = GetToday();
            HasLoaded = true;
            RebuildCalendar();
        }
        catch (Exception)
        {
            RefreshDate();
            ErrorMessage = HasLoaded
                ? "Couldn't refresh your calendar. Saved information may be out of date. Try again."
                : "Couldn't load your saved dues. Check available storage and try again.";
        }
        finally { IsBusy = false; }
    }

    private void RebuildCalendar()
    {
        var counts = _dues.GroupBy(due => due.DueDate).ToDictionary(group => group.Key, group => group.Count());
        var offset = ((int)Month.DayOfWeek + (WeekStart == WeekStartPreference.Monday ? 6 : 0)) % 7;
        var length = DateTime.DaysInMonth(Month.Year, Month.Month);
        Days = Array.AsReadOnly(Enumerable.Range(0, 42).Select(index =>
        {
            var day = index - offset + 1;
            if (day < 1 || day > length) return new CalendarDay(null, 0, false, false);
            var date = new DateOnly(Month.Year, Month.Month, day);
            return new CalendarDay(date, counts.GetValueOrDefault(date), date == SelectedDate, date == Today);
        }).ToArray());
        // Retain the CollectionView/header instance when selecting an empty day.
        var matching = _dues.Where(due => due.DueDate == SelectedDate)
            .Select(due => new DashboardItem(due, DueStatusCalculator.Calculate(due, Today)!.Value)).ToArray();
        for (var index = 0; index < matching.Length; index++)
        {
            if (index >= _agenda.Count) _agenda.Add(matching[index]);
            else if (_agenda[index] != matching[index]) _agenda[index] = matching[index];
        }
        while (_agenda.Count > matching.Length) _agenda.RemoveAt(_agenda.Count - 1);
        foreach (var property in new[] { nameof(Today), nameof(TodayDescription), nameof(Month), nameof(MonthText),
            nameof(SelectedDate), nameof(SelectedDateText), nameof(Days), nameof(AgendaSummary),
            nameof(IsEmpty), nameof(IsDayEmpty), nameof(CanGoPrevious), nameof(CanGoNext),
            nameof(WeekStart), nameof(Weekdays), nameof(WeekStartDescription) })
            OnPropertyChanged(property);
    }

    private DateOnly GetToday() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), ProductZone).DateTime);
    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";
}
