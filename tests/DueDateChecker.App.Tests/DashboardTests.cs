using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;
using System.Collections.Specialized;

namespace DueDateChecker.App.Tests;

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
    public async Task Snapshot_uses_real_active_dues_in_date_order_and_counts_partition_without_writes()
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
        Assert.Equal(before.Select(x => x.Id), vm.Items.Select(x => x.Id));
        Assert.Equal(before, await _repository.ListActiveAsync());
        Assert.True(vm.CanAdd);
        Assert.False(vm.HasError);
    }

    [Theory]
    [InlineData("All", 4)]
    [InlineData("Overdue", 1)]
    [InlineData("Due today", 1)]
    [InlineData("Upcoming", 1)]
    [InlineData("Paid", 1)]
    public async Task Case_insensitive_trimmed_title_search_intersects_status_without_changing_global_counts(string filter, int count)
    {
        await SeedAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.Query = "  aLpHa  ";
        vm.SelectFilter(filter);
        Assert.Equal(count, vm.Items.Count);
        Assert.All(vm.Items, row =>
        {
            Assert.Contains("alpha", row.Title.ToLowerInvariant());
            if (filter != "All") Assert.Equal(filter, row.StatusText);
        });
        Assert.Equal(5, vm.TotalCount);
        Assert.Equal(1, vm.TodayCount);
        Assert.Equal(2, vm.UpcomingCount);
        Assert.Equal(.2f, vm.Completion);
        Assert.Equal($"{filter} · {count}", vm.ResultsHeading);
        vm.SelectFilter("Completed");
        Assert.Equal(filter, vm.Filter);
    }

    [Fact]
    public async Task Live_search_keeps_one_read_only_binding_source_and_uses_incremental_events()
    {
        await SeedAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        var boundSource = vm.Items;
        var events = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)boundSource).CollectionChanged += (_, args) => events.Add(args.Action);
        foreach (var query in new[] { "a", "al", "alpha", "alpha upcoming", "absent", "" })
        {
            vm.Query = query;
            Assert.Same(boundSource, vm.Items);
            Assert.Equal(vm.Items.Count, vm.Items.Select(x => x.Id).Distinct().Count());
        }
        Assert.Equal(5, vm.Items.Count);
        Assert.NotEmpty(events);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, events);
        Assert.Throws<NotSupportedException>(() => ((IList<DashboardItem>)boundSource).Clear());
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Same(boundSource, vm.Items);
    }

    [Fact]
    public async Task Empty_database_and_no_match_are_distinct_and_clear_restores_all_results()
    {
        var vm = Dashboard();
        Assert.False(vm.IsEmpty);
        Assert.False(vm.HasLoaded);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.IsNoMatches);
        Assert.Equal(0, vm.Completion);
        Assert.Equal("0%", vm.PercentageText);
        vm.Query = "absent";
        Assert.True(vm.IsEmpty);
        await SeedAsync();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.IsNoMatches);
        vm.Query = "Beta";
        vm.SelectFilter("Paid");
        Assert.True(vm.IsNoMatches);
        Assert.True(vm.HasFilters);
        vm.ClearFiltersCommand.Execute(null);
        Assert.Equal("All", vm.Filter);
        Assert.Equal("", vm.Query);
        Assert.Equal(5, vm.Items.Count);
        Assert.False(vm.IsNoMatches);
        Assert.False(vm.HasFilters);
        vm.Query = null!; // Android text clearing may send null.
        Assert.Equal(5, vm.Items.Count);
    }

    [Fact]
    public async Task Manila_midnight_refreshes_status_and_active_filter_without_storage_access()
    {
        _clock.Now = new DateTimeOffset(2026, 10, 17, 15, 59, 59, TimeSpan.Zero);
        var due = NewDue("Midnight");
        await _repository.AddAsync(due);
        var reads = new ReadOnlyRepository(_repository);
        var vm = Dashboard(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(Today.AddDays(-1), vm.Today);
        Assert.Equal(1, vm.UpcomingCount);
        vm.Query = "midnight";
        vm.SelectFilter("Upcoming");
        _clock.Now = _clock.Now.AddSeconds(1);
        vm.RefreshDate();
        Assert.Equal(Today, vm.Today);
        Assert.Equal(1, vm.TodayCount);
        Assert.Equal(0, vm.UpcomingCount);
        Assert.Empty(vm.Items);
        Assert.True(vm.IsNoMatches);
        Assert.Equal(1, reads.Reads);
        vm.SelectFilter("Due today");
        Assert.Equal(due.Id, Assert.Single(vm.Items).Id);
        _clock.Now = _clock.Now.AddDays(1);
        vm.RefreshDate();
        Assert.Equal(1, vm.OverdueCount);
        Assert.Empty(vm.Items);
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
    public async Task Return_after_real_editor_writes_refreshes_counts_filter_and_identity_without_duplicates()
    {
        var dashboard = Dashboard();
        await dashboard.LoadCommand.ExecuteAsync(null);
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "New obligation", AmountText = "0", SelectedDate = Today.ToDateTime(TimeOnly.MinValue)
        };
        await editor.SaveCommand.ExecuteAsync(null);
        await dashboard.LoadCommand.ExecuteAsync(null);
        var id = Assert.Single(dashboard.Items).Id;
        Assert.Equal(editor.SavedDueId, id);
        dashboard.Query = "obligation";
        dashboard.SelectFilter("Due today");
        var edit = new DueEditViewModel(_database, _repository, _clock);
        edit.PrepareForEdit(id.ToString());
        await edit.LoadCommand.ExecuteAsync(null);
        edit.IsPaid = true;
        await dashboard.LoadCommand.ExecuteAsync(null); // Cancel/Back before Save has no mutation.
        Assert.Equal(1, dashboard.TodayCount);
        Assert.Single(dashboard.Items);
        await edit.SaveCommand.ExecuteAsync(null);
        await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(1, dashboard.PaidCount);
        Assert.Equal(0, dashboard.TodayCount);
        Assert.Empty(dashboard.Items);
        Assert.Equal("obligation", dashboard.Query);
        Assert.Equal("Due today", dashboard.Filter);
        dashboard.SelectFilter("Paid");
        Assert.Equal(id, Assert.Single(dashboard.Items).Id);

        var remove = new DueEditViewModel(_database, _repository, _clock);
        remove.PrepareForEdit(id.ToString());
        await remove.LoadCommand.ExecuteAsync(null);
        await remove.DeleteCommand.ExecuteAsync(true);
        await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(0, dashboard.TotalCount);
        Assert.Empty(dashboard.Items);
        Assert.True(dashboard.IsEmpty);
        Assert.NotNull((await _repository.GetAsync(id))!.DeletedAt);
    }

    [Fact]
    public async Task Refresh_failure_keeps_last_successful_rows_with_stale_error_and_retry_replaces_them()
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
        vm.Query = "Alpha";
        vm.SelectFilter("Upcoming");
        var original = Assert.Single(vm.Items);
        await _repository.UpdateAsync(original.Due with { Title = "Alpha changed" });
        reads.FailReads = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(original, Assert.Single(vm.Items));
        Assert.Equal(5, vm.TotalCount);
        Assert.Contains("out of date", vm.ErrorMessage);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.IsNoMatches);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        Assert.Equal("Alpha changed", Assert.Single(vm.Items).Title);
        Assert.Equal("Alpha", vm.Query);
        Assert.Equal("Upcoming", vm.Filter);
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
    public async Task Busy_read_ignores_overlap_and_applies_query_selected_while_loading()
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
            vm.Query = "alpha";
            vm.SelectFilter("Paid");
            await vm.LoadCommand.ExecuteAsync(null);
            Assert.Equal(1, reads.Reads);
        }
        finally { reads.ReleaseRead.TrySetResult(); await load; }
        Assert.Equal("Alpha paid", Assert.Single(vm.Items).Title);
        Assert.True(vm.CanAdd);
    }

    [Fact]
    public async Task Fresh_session_resets_only_query_filter_and_navigation_error_does_not_write()
    {
        await SeedAsync();
        var vm = Dashboard();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.Query = "Beta";
        vm.SelectFilter("Paid");
        vm.ReportNavigationError();
        Assert.True(vm.HasError);
        Assert.True(vm.CanAdd);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Beta", vm.Query);
        Assert.Equal("Paid", vm.Filter);
        var restarted = Dashboard();
        await restarted.LoadCommand.ExecuteAsync(null);
        Assert.Equal("All", restarted.Filter);
        Assert.Equal("", restarted.Query);
        Assert.Equal(5, restarted.Items.Count);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
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
