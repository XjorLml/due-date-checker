using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>Dashboard summary (PRD F02). Since the prototype alignment (2026-11-07) Dashboard has no search or
/// filters; those tests moved to <see cref="DuesFilterTests"/>. These cover the counts and the two short lists.</summary>
public sealed class DashboardTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 10, 18);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dashboard-{Guid.NewGuid():N}");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly MutableClock _clock = new(new DateTimeOffset(2026, 10, 18, 1, 0, 0, TimeSpan.Zero));

    public DashboardTests()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "dues.db");
        new DatabaseInitializer(path).Initialize();
        _repository = new SqliteDueRepository(path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(path));
    }

    private DashboardViewModel Dashboard(IDueRepository? repository = null) => new(_database, repository ?? new ReadOnlyRepository(_repository), _clock);
    private Due NewDue(string title, int offset = 0, bool paid = false) =>
        Due.Create(title, Money.FromCentavos(0), Today.AddDays(offset), _clock.Now) with { IsPaid = paid };

    private async Task SeedAsync()
    {
        foreach (var due in new[] { NewDue("Alpha upcoming", 1), NewDue("ALPHA today"), NewDue("Alpha overdue", -1), NewDue("Alpha paid", -3, true), NewDue("Beta", 2) })
            await _repository.AddAsync(due);
        var deleted = NewDue("Alpha deleted", -2);
        await _repository.AddAsync(deleted);
        await _repository.SoftDeleteAsync(deleted.Id, _clock.Now);
    }

    [Fact]
    public async Task Snapshot_uses_real_active_dues_and_counts_partition_without_writes()
    {
        await SeedAsync();
        var before = await _repository.ListActiveAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(5, vm.TotalCount);
        Assert.Equal(1, vm.PaidCount);
        Assert.Equal(1, vm.OverdueCount);
        Assert.Equal(1, vm.TodayCount);
        Assert.Equal(2, vm.UpcomingCount);
        Assert.Equal(4, vm.OpenCount);
        Assert.Equal(.2f, vm.Completion);
        Assert.Equal("20%", vm.PercentageText);
        Assert.Equal(vm.TotalCount, vm.PaidCount + vm.OverdueCount + vm.TodayCount + vm.UpcomingCount);
        Assert.Equal(before, await _repository.ListActiveAsync());
        Assert.True(vm.CanAdd);
        Assert.False(vm.HasError);
        Assert.True(vm.HasSummary);
    }

    // PA03: "Needs attention" holds unpaid dues dated today or earlier, oldest first; "A little further ahead"
    // holds unpaid dues after today up to month end. Paid and deleted dues appear in neither.
    [Fact]
    public async Task Sections_split_unpaid_dues_at_today_and_exclude_paid_and_deleted()
    {
        await SeedAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new[] { "Alpha overdue", "ALPHA today" }, vm.Attention.Select(x => x.Title));
        Assert.Equal(new[] { "Alpha upcoming", "Beta" }, vm.Later.Select(x => x.Title));
        Assert.True(vm.HasAttention);
        Assert.True(vm.HasAhead);
        Assert.False(vm.ShowNoAttention);
        Assert.False(vm.ShowNoAhead);
    }

    [Fact]
    public async Task Sections_show_at_most_three_and_further_ahead_stops_at_month_end()
    {
        for (var day = 1; day <= 5; day++) await _repository.AddAsync(NewDue($"Overdue {day}", -day));
        foreach (var offset in new[] { 1, 2, 3, 4 }) await _repository.AddAsync(NewDue($"Later {offset}", offset));
        await _repository.AddAsync(NewDue("Next month", 14)); // 01 Nov, after month end (31 Oct)
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new[] { "Overdue 5", "Overdue 4", "Overdue 3" }, vm.Attention.Select(x => x.Title));
        Assert.Equal(new[] { "Later 1", "Later 2", "Later 3" }, vm.Later.Select(x => x.Title));
        Assert.Equal(DashboardViewModel.SectionSize, vm.Attention.Count);
    }

    [Fact]
    public async Task Empty_sections_show_their_calm_lines_only_when_there_are_dues()
    {
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.HasSummary);
        Assert.False(vm.ShowNoAttention);
        Assert.False(vm.ShowNoAhead);
        Assert.False(vm.ShowNoNext);
        Assert.Equal(0, vm.Completion);
        Assert.Equal("0%", vm.PercentageText);

        await _repository.AddAsync(NewDue("Paid only", 0, paid: true));
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.HasSummary);
        Assert.True(vm.ShowNoAttention);
        Assert.True(vm.ShowNoAhead);
        Assert.True(vm.ShowNoNext);
    }

    [Fact]
    public async Task Manila_midnight_moves_a_due_between_sections_without_storage_access()
    {
        _clock.Now = new DateTimeOffset(2026, 10, 17, 15, 59, 59, TimeSpan.Zero);
        var due = NewDue("Midnight");
        await _repository.AddAsync(due);
        var reads = new ReadOnlyRepository(_repository);
        var vm = Dashboard(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(Today.AddDays(-1), vm.Today);
        Assert.Equal(1, vm.UpcomingCount);
        Assert.Equal(due.Id, Assert.Single(vm.Later).Id);
        Assert.Empty(vm.Attention);
        _clock.Now = _clock.Now.AddSeconds(1);
        vm.RefreshDate();
        Assert.Equal(Today, vm.Today);
        Assert.Equal(1, vm.TodayCount);
        Assert.Equal(0, vm.UpcomingCount);
        Assert.Equal(due.Id, Assert.Single(vm.Attention).Id);
        Assert.Empty(vm.Later);
        Assert.Equal(1, reads.Reads);
        _clock.Now = _clock.Now.AddDays(1);
        vm.RefreshDate();
        Assert.Equal(1, vm.OverdueCount);
        Assert.Equal(due.Id, Assert.Single(vm.Attention).Id);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Read_completion_uses_current_product_date_even_if_midnight_passes_during_read()
    {
        _clock.Now = new DateTimeOffset(2026, 10, 17, 15, 59, 59, TimeSpan.Zero);
        await _repository.AddAsync(NewDue("Read boundary"));
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Dashboard(reads);
        var load = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        _clock.Now = _clock.Now.AddSeconds(1);
        reads.ReleaseRead.TrySetResult();
        await load;
        Assert.Equal(Today, vm.Today);
        Assert.Equal(1, vm.TodayCount);
        Assert.Equal(0, vm.UpcomingCount);
    }

    [Fact]
    public async Task Return_after_real_editor_writes_refreshes_counts_and_sections_without_duplicates()
    {
        var dashboard = Dashboard();
        await dashboard.LoadCommand.ExecuteAsync(null);
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "New obligation", AmountText = "0", SelectedDate = Today.ToDateTime(TimeOnly.MinValue)
        };
        await editor.SaveCommand.ExecuteAsync(null);
        await dashboard.LoadCommand.ExecuteAsync(null);
        var id = Assert.Single(dashboard.Attention).Id;
        Assert.Equal(editor.SavedDueId, id);
        var edit = new DueEditViewModel(_database, _repository, _clock);
        edit.PrepareForEdit(id.ToString());
        await edit.LoadCommand.ExecuteAsync(null);
        edit.IsPaid = true;
        await dashboard.LoadCommand.ExecuteAsync(null); // Cancel/Back before Save has no mutation.
        Assert.Equal(1, dashboard.TodayCount);
        Assert.Single(dashboard.Attention);
        await edit.SaveCommand.ExecuteAsync(null);
        await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(1, dashboard.PaidCount);
        Assert.Equal(0, dashboard.TodayCount);
        Assert.Empty(dashboard.Attention);

        var remove = new DueEditViewModel(_database, _repository, _clock);
        remove.PrepareForEdit(id.ToString());
        await remove.LoadCommand.ExecuteAsync(null);
        await remove.DeleteCommand.ExecuteAsync(true);
        await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(0, dashboard.TotalCount);
        Assert.True(dashboard.IsEmpty);
        Assert.NotNull((await _repository.GetAsync(id))!.DeletedAt);
    }

    [Fact]
    public async Task Refresh_failure_keeps_last_successful_summary_with_stale_error_and_retry_replaces_it()
    {
        await SeedAsync();
        var reads = new ReadOnlyRepository(_repository) { FailReads = true };
        var vm = Dashboard(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.False(vm.HasLoaded);
        Assert.False(vm.IsEmpty);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        var original = vm.Later[0];
        Assert.Equal("Alpha upcoming", original.Title);
        await _repository.UpdateAsync(original.Due with { Title = "Alpha changed" });
        reads.FailReads = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(original, vm.Later[0]);
        Assert.Equal(5, vm.TotalCount);
        Assert.Contains("out of date", vm.ErrorMessage);
        Assert.False(vm.IsEmpty);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        Assert.Equal("Alpha changed", vm.Later[0].Title);
        Assert.Equal(5, vm.TotalCount);
    }

    [Fact]
    public async Task Initialization_failure_blocks_add_and_retry_preserves_path_then_recovers()
    {
        var path = Path.Combine(_directory, "blocked.db");
        Directory.CreateDirectory(path);
        var repository = new SqliteDueRepository(path);
        var vm = new DashboardViewModel(new DatabaseDispatcher(new DatabaseInitializer(path)), new ReadOnlyRepository(repository), _clock);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.CanAdd);
        Assert.False(vm.HasLoaded);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.HasError);
        Assert.True(Directory.Exists(path));
        Directory.Delete(path);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.CanAdd);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task Busy_read_ignores_overlap()
    {
        await SeedAsync();
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Dashboard(reads);
        var load = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.True(vm.IsBusy);
            Assert.False(vm.CanAdd);
            Assert.False(vm.LoadCommand.CanExecute(null));
            await vm.LoadCommand.ExecuteAsync(null);
            Assert.Equal(1, reads.Reads);
        }
        finally { reads.ReleaseRead.TrySetResult(); await load; }
        Assert.Equal(5, vm.TotalCount);
        Assert.True(vm.CanAdd);
    }

    [Fact]
    public async Task Navigation_error_is_reported_without_a_write()
    {
        await SeedAsync();
        var before = await _repository.ListActiveAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.ReportNavigationError();
        Assert.True(vm.HasError);
        Assert.True(vm.CanAdd);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        Assert.Equal(before, await _repository.ListActiveAsync());
    }

    public void Dispose()
    {
        TestDatabase.ClearPoolsUnder(_directory);
        Directory.Delete(_directory, recursive: true);
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class ReadOnlyRepository(IDueRepository inner) : IDueRepository
    {
        public bool FailReads { get; set; }
        public bool BlockReads { get; init; }
        public int Reads { get; private set; }
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<IReadOnlyList<Due>> ListActiveAsync()
        {
            Reads++;
            ReadStarted.TrySetResult();
            if (BlockReads) await ReleaseRead.Task;
            if (FailReads) throw new IOException("Controlled read failure");
            return await inner.ListActiveAsync();
        }
        public Task<Due?> GetAsync(Guid id) => throw new InvalidOperationException("Dashboard reads the active snapshot only.");
        public Task AddAsync(Due due) => throw new InvalidOperationException("Dashboard cannot insert.");
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("Dashboard cannot update.");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new InvalidOperationException("Dashboard cannot delete.");
    }
}
