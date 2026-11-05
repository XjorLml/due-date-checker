using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>PRD F02 upcoming-total rule and the Focus summary members (specs/focus-pages FP01, FP02).</summary>
public sealed class DashboardSummaryTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 11, 5);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dashboard-summary-{Guid.NewGuid():N}");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    // 01:00 UTC is 09:00 in Asia/Manila on the same date.
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 11, 5, 1, 0, 0, TimeSpan.Zero));

    public DashboardSummaryTests()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "dues.db");
        new DatabaseInitializer(path).Initialize();
        _repository = new SqliteDueRepository(path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(path));
    }

    private Due NewDue(string title, int offset, long centavos, bool paid = false) =>
        Due.Create(title, Money.FromCentavos(centavos), Today.AddDays(offset), _clock.GetUtcNow()) with { IsPaid = paid };

    [Fact]
    public async Task Upcoming_total_sums_unpaid_known_amounts_from_today_to_month_end_and_names_the_next_due()
    {
        foreach (var due in new[]
        {
            NewDue("Water bill", -2, 100_000),        // overdue: outside the window
            NewDue("Rent", 0, 250_000),               // due today: inside
            NewDue("Internet", 5, 50_000),            // 10 Nov: inside
            NewDue("Phone load", 0, 99_900, true),    // paid today: excluded
            NewDue("Insurance", 26, 70_000)           // 1 Dec: next month, outside
        })
            await _repository.AddAsync(due);

        var vm = new DashboardViewModel(_database, _repository, _clock);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("Upcoming · November", vm.MonthLabel);
        Assert.Equal("₱3,000.00", vm.UpcomingTotalText);
        Assert.Equal("2 unpaid · today to 30 Nov", vm.UpcomingMetaText);
        Assert.True(vm.HasNext);
        Assert.Equal("Rent", vm.NextTitle);
        Assert.Equal("Next date · 05 Nov", vm.NextDateText);
        Assert.Equal("1/5", vm.PaidCounterText);
        Assert.Equal(5, vm.TotalCount);
    }

    [Fact]
    public async Task No_unpaid_due_ahead_gives_a_zero_total_and_no_next_due()
    {
        await _repository.AddAsync(NewDue("Water bill", -2, 100_000));
        await _repository.AddAsync(NewDue("Phone load", 3, 20_000, true));

        var vm = new DashboardViewModel(_database, _repository, _clock);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("₱0.00", vm.UpcomingTotalText);
        Assert.Equal("0 unpaid · today to 30 Nov", vm.UpcomingMetaText);
        Assert.False(vm.HasNext);
        Assert.Equal("", vm.NextTitle);
        Assert.Equal("No unpaid dues coming up.", vm.NextDateText);
    }

    [Fact]
    public void Projections_expose_the_shared_row_contract()
    {
        var due = NewDue("Internet", 5, 50_000);
        IDueRow list = DueListItem.Create(due, Today);
        IDueRow dashboard = new DashboardItem(due, DueStatus.Upcoming);
        Assert.Equal("10 Nov", list.ShortDateText);
        Assert.Equal("10 Nov · Other", list.MetaText);
        Assert.Equal(list.MetaText, dashboard.MetaText);
        Assert.Equal("Upcoming", dashboard.StatusText);
    }

    public void Dispose()
    {
        try { Directory.Delete(_directory, true); } catch (IOException) { }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
