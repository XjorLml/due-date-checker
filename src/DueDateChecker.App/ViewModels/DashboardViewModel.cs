using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

/// <summary>Dashboard summary (PRD F02). Since the prototype alignment (decided 2026-11-07, PRD
/// "UX09 (revised)") it is a summary only: total, next date, counters and two short lists. Search
/// and filters live on Dues; the counters open Dues with that status selected.</summary>
public partial class DashboardViewModel : ObservableObject
{
    /// <summary>Rows shown in each summary list ("Needs attention", "A little further ahead").</summary>
    public const int SectionSize = 3;
    private static readonly TimeZoneInfo ProductZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    private readonly DatabaseDispatcher _database;
    private readonly IDueRepository _repository;
    private readonly TimeProvider _clock;
    private readonly AppPreferencesService? _preferences;
    private IReadOnlyList<Due> _dues = [];
    private IReadOnlyList<DashboardItem> _allItems = [];

    public DashboardViewModel(DatabaseDispatcher database, IDueRepository repository, TimeProvider clock,
        AppPreferencesService? preferences = null)
    {
        _database = database;
        _repository = repository;
        _clock = clock;
        _preferences = preferences;
        Today = GetToday(clock);
        // PD04: new paydays apply without a restart. Preferences are saved on the UI thread.
        if (preferences is not null) preferences.Changed += (_, _) => { if (HasLoaded) RebuildSnapshot(Today); };
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    private bool _isReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(HasSummary))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(HasSummary))]
    private bool _hasLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private string _errorMessage = "";

    /// <summary>Unpaid dues dated today or earlier, oldest first (prototype "Needs attention").</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAttention))]
    private IReadOnlyList<DashboardItem> _attention = [];

    /// <summary>Unpaid dues after today up to month end, earliest first (prototype "A little further ahead").</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAhead))]
    private IReadOnlyList<DashboardItem> _later = [];

    public DateOnly Today { get; private set; }
    public string DateText => $"{Today.ToString("dddd, dd MMM yyyy", CultureInfo.InvariantCulture)} · Asia/Manila";
    public bool CanAdd => IsReady && !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty => HasLoaded && TotalCount == 0 && !IsBusy && !HasError;
    /// <summary>The summary and lists show once data has loaded and there is at least one due.</summary>
    public bool HasSummary => HasLoaded && TotalCount > 0;
    public bool HasAttention => Attention.Count > 0;
    public bool HasAhead => Later.Count > 0;
    /// <summary>The calm lines shown in place of an empty list.</summary>
    public bool ShowNoAttention => HasSummary && !HasAttention;
    public bool ShowNoAhead => HasSummary && !HasAhead;
    public bool ShowNoNext => HasSummary && !HasNext;
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

    // "A little further ahead" still runs to the end of today's Manila month.
    private IReadOnlyList<DashboardItem> _ahead = [];
    public DateOnly MonthEnd => new(Today.Year, Today.Month, DateTime.DaysInMonth(Today.Year, Today.Month));

    // UX33 (2026-11-16, PD02/PD03): the headline totals unpaid dues dated before the next payday,
    // overdue ones included (which also settles UX18). Paydays follow the Twice a month day rules.
    private IReadOnlyList<DashboardItem> _beforePayday = [];
    private (int First, int Second) Paydays => _preferences is { } p ? (p.Current.PaydayFirst, p.Current.PaydaySecond) : (15, 30);
    /// <summary>The earliest payday strictly after today; null only at the end of the calendar.</summary>
    public DateOnly? NextPayday => Recurrence.NextDate(Today, RepeatRule.TwiceMonthly, Paydays.First, Paydays.Second);
    private string NextPaydayText => NextPayday is { } payday ? payday.ToString("dd MMM", CultureInfo.InvariantCulture) : "the end of the calendar";
    public string PaydayLabel => $"Before payday · {NextPaydayText}";
    // A due without an amount (F07, UX32) adds nothing and is counted as excluded instead.
    // Summed in decimal: each amount fits in a long, but two of the largest allowed do not.
    private static decimal Sum(IEnumerable<DashboardItem> items) => items.Sum(item => (decimal)(item.Due.Amount?.Centavos ?? 0)) / 100m;
    public string PaydayTotalText => Money.Format(Sum(_beforePayday));
    private decimal OverdueAmount => Sum(_beforePayday.Where(item => item.Status == DueStatus.Overdue));
    private int BeforePaydayWithoutAmount => _beforePayday.Count(item => item.Due.Amount is null);
    public string PaydayMetaText => $"{_beforePayday.Count} unpaid"
        + (OverdueAmount > 0 ? $" · {Money.Format(OverdueAmount)} overdue" : "")
        + (BeforePaydayWithoutAmount > 0 ? $" · {BeforePaydayWithoutAmount} without an amount, excluded" : "");
    public string PaydayDescription => $"Due before payday on {NextPaydayText}: {PaydayTotalText}, {PaydayMetaText}";
    public DashboardItem? NextDue => _allItems.FirstOrDefault(item => item.Status != DueStatus.Paid && item.Due.DueDate >= Today);
    public bool HasNext => NextDue is not null;
    public string NextTitle => NextDue?.Title ?? "";
    public string NextDateText => NextDue is { } next ? $"Next date · {next.ShortDateText}" : "No unpaid dues coming up.";
    public string NextDescription => NextDue is { } next ? $"Next date {next.DateText}, {next.Title}. Opens due details." : "";
    public string PaidCounterText => $"{PaidCount}/{TotalCount}";

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
        _ahead = _allItems.Where(item => item.Status != DueStatus.Paid && item.Due.DueDate >= today && item.Due.DueDate <= MonthEnd).ToArray();
        var payday = NextPayday;
        _beforePayday = _allItems.Where(item => item.Status != DueStatus.Paid && (payday is null || item.Due.DueDate < payday)).ToArray();
        Attention = _allItems.Where(item => item.Status != DueStatus.Paid && item.Due.DueDate <= today).Take(SectionSize).ToArray();
        Later = _ahead.Where(item => item.Due.DueDate > today).Take(SectionSize).ToArray();
        foreach (var name in new[] { nameof(Today), nameof(DateText), nameof(TotalCount), nameof(PaidCount),
            nameof(TodayCount), nameof(OverdueCount), nameof(UpcomingCount), nameof(OpenCount), nameof(Completion),
            nameof(PercentageText), nameof(ProgressText), nameof(ProgressDescription), nameof(IsEmpty), nameof(HasSummary),
            nameof(NextPayday), nameof(PaydayLabel), nameof(PaydayTotalText), nameof(PaydayMetaText), nameof(PaydayDescription),
            nameof(NextDue), nameof(HasNext), nameof(NextTitle), nameof(NextDateText), nameof(NextDescription), nameof(PaidCounterText),
            nameof(ShowNoAttention), nameof(ShowNoAhead), nameof(ShowNoNext) })
            OnPropertyChanged(name);
    }

    private int Count(DueStatus status) => _allItems.Count(item => item.Status == status);
    private static DateOnly GetToday(TimeProvider time) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(time.GetUtcNow(), ProductZone).DateTime);
    public void ReportNavigationError() => ErrorMessage = "Couldn't open the requested page. Try again.";
}
