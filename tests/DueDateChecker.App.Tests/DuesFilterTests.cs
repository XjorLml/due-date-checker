using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using System.Collections.Specialized;

namespace DueDateChecker.App.Tests;

/// <summary>Search and filters on Dues (prototype alignment, PRD "UX09 (revised)", PA04). These rules and most of
/// these assertions moved here from the Dashboard tests unchanged; the new part is how they meet the UX02 fold.</summary>
public sealed class DuesFilterTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 10, 18);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dues-filter-{Guid.NewGuid():N}");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 18, 1, 0, 0, TimeSpan.Zero));

    public DuesFilterTests()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "dues.db");
        new DatabaseInitializer(path).Initialize();
        _repository = new SqliteDueRepository(path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(path));
    }

    private DuesListViewModel Dues(IDueRepository? repository = null) => new(_database, repository ?? new ReadOnlyRepository(_repository), _clock);
    private Due NewDue(string title, int offset = 0, bool paid = false) =>
        Due.Create(title, Money.FromCentavos(0), Today.AddDays(offset), _clock.GetUtcNow()) with { IsPaid = paid };

    // Five active dues (one paid three days ago, so it folds under UX02) and one deleted.
    private async Task SeedAsync()
    {
        foreach (var due in new[] { NewDue("Alpha upcoming", 1), NewDue("ALPHA today"), NewDue("Alpha overdue", -1), NewDue("Alpha paid", -3, true), NewDue("Beta", 2) })
            await _repository.AddAsync(due);
        var deleted = NewDue("Alpha deleted", -2);
        await _repository.AddAsync(deleted);
        await _repository.SoftDeleteAsync(deleted.Id, _clock.GetUtcNow());
    }

    [Theory]
    [InlineData("All", 4)]
    [InlineData("Overdue", 1)]
    [InlineData("Due today", 1)]
    [InlineData("Upcoming", 1)]
    [InlineData("Paid", 1)]
    public async Task Case_insensitive_trimmed_title_search_intersects_status(string filter, int count)
    {
        await SeedAsync();
        var vm = Dues();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.Query = "  aLpHa  ";
        vm.SelectFilter(filter);
        Assert.Equal(count, vm.Dues.Count);
        Assert.Equal(count, vm.MatchCount);
        Assert.Equal(count == 1 ? "1 matching due" : $"{count} matching dues", vm.MatchCountText);
        Assert.All(vm.Dues, row =>
        {
            Assert.Contains("alpha", row.Title.ToLowerInvariant());
            if (filter != "All") Assert.Equal(filter, row.StatusText);
        });
        vm.SelectFilter("Completed");
        Assert.Equal(filter, vm.Filter);
    }

    [Fact]
    public async Task Without_search_or_filter_earlier_paid_dues_fold_and_any_filter_shows_every_match()
    {
        await SeedAsync();
        var vm = Dues();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasFilters);
        Assert.Equal(1, vm.EarlierPaidCount);
        Assert.Equal(4, vm.Dues.Count);
        Assert.Equal(5, vm.MatchCount); // the folded one still matches
        Assert.DoesNotContain(vm.Dues, row => row.Title == "Alpha paid");

        vm.SelectFilter("Paid");
        Assert.Equal(0, vm.EarlierPaidCount);
        Assert.False(vm.HasEarlierPaid);
        Assert.Equal("Alpha paid", Assert.Single(vm.Dues).Title);

        vm.ShowStatus(null);
        vm.ToggleEarlierPaidCommand.Execute(null);
        Assert.Equal(5, vm.Dues.Count);
        Assert.Equal(new[] { "Alpha paid", "Alpha overdue", "ALPHA today", "Alpha upcoming", "Beta" }, vm.Dues.Select(x => x.Title));
    }

    [Fact]
    public async Task Live_search_keeps_one_read_only_binding_source_and_uses_incremental_events()
    {
        await SeedAsync();
        var vm = Dues();
        await vm.LoadCommand.ExecuteAsync(null);
        var boundSource = vm.Dues;
        var events = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)boundSource).CollectionChanged += (_, args) => events.Add(args.Action);
        foreach (var query in new[] { "a", "al", "alpha", "alpha upcoming", "absent", "" })
        {
            vm.Query = query;
            Assert.Same(boundSource, vm.Dues);
            Assert.Equal(vm.Dues.Count, vm.Dues.Select(x => x.Id).Distinct().Count());
        }
        Assert.Equal(4, vm.Dues.Count); // unfiltered again, so the earlier paid due folds
        Assert.NotEmpty(events);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, events);
        Assert.Throws<NotSupportedException>(() => ((IList<DueListItem>)boundSource).Clear());
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Same(boundSource, vm.Dues);
    }

    [Fact]
    public async Task Empty_database_and_no_match_are_distinct_and_reset_restores_the_list()
    {
        var vm = Dues();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.IsNoMatches);
        vm.Query = "absent";
        Assert.True(vm.IsEmpty);
        Assert.False(vm.IsNoMatches);
        await SeedAsync();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.IsNoMatches);
        Assert.False(vm.IsAllFolded);
        vm.Query = "Beta";
        vm.SelectFilter("Paid");
        Assert.True(vm.IsNoMatches);
        Assert.True(vm.HasFilters);
        vm.ClearFiltersCommand.Execute(null);
        Assert.Equal("All", vm.Filter);
        Assert.Equal("", vm.Query);
        Assert.Equal(DuesListViewModel.AllTypes, vm.TypeFilter);
        Assert.Equal(4, vm.Dues.Count);
        Assert.False(vm.IsNoMatches);
        Assert.False(vm.HasFilters);
        vm.Query = null!; // Android text clearing may send null.
        Assert.Equal(4, vm.Dues.Count);
    }

    [Fact]
    public async Task Dashboard_status_links_clear_search_and_type_then_select_the_status()
    {
        await SeedAsync();
        var vm = Dues();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.Query = "Beta";
        vm.TypeFilter = "Task";
        vm.ShowStatus("Overdue");
        Assert.Equal("", vm.Query);
        Assert.Equal(DuesListViewModel.AllTypes, vm.TypeFilter);
        Assert.Equal("Overdue", vm.Filter);
        Assert.Equal("Alpha overdue", Assert.Single(vm.Dues).Title);
        vm.ShowStatus(null); // "All dues"
        Assert.Equal("All", vm.Filter);
        Assert.False(vm.HasFilters);
    }

    [Fact]
    public async Task Search_and_filters_survive_reload_and_a_failed_refresh_but_not_a_new_session()
    {
        await SeedAsync();
        var reads = new ReadOnlyRepository(_repository);
        var vm = Dues(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        vm.Query = "Alpha";
        vm.SelectFilter("Upcoming");
        var original = Assert.Single(vm.Dues);
        await _repository.UpdateAsync(original.Due with { Title = "Alpha changed" });
        reads.FailReads = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(original, Assert.Single(vm.Dues));
        Assert.Contains("out of date", vm.ErrorMessage);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        Assert.Equal("Alpha changed", Assert.Single(vm.Dues).Title);
        Assert.Equal("Alpha", vm.Query);
        Assert.Equal("Upcoming", vm.Filter);

        var restarted = Dues();
        await restarted.LoadCommand.ExecuteAsync(null);
        Assert.Equal("All", restarted.Filter);
        Assert.Equal("", restarted.Query);
        Assert.Equal(4, restarted.Dues.Count);
    }

    [Fact]
    public async Task Busy_read_ignores_overlap_and_applies_query_selected_while_loading()
    {
        await SeedAsync();
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Dues(reads);
        var load = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.True(vm.IsBusy);
            Assert.False(vm.CanAdd);
            vm.Query = "alpha";
            vm.SelectFilter("Paid");
            await vm.LoadCommand.ExecuteAsync(null);
            Assert.Equal(1, reads.Reads);
        }
        finally { reads.ReleaseRead.TrySetResult(); await load; }
        Assert.Equal("Alpha paid", Assert.Single(vm.Dues).Title);
        Assert.True(vm.CanAdd);
    }

    public void Dispose()
    {
        TestDatabase.ClearPoolsUnder(_directory);
        Directory.Delete(_directory, recursive: true);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
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
        public Task<Due?> GetAsync(Guid id) => throw new InvalidOperationException("Dues reads the active snapshot only.");
        public Task AddAsync(Due due) => throw new InvalidOperationException("Dues cannot insert.");
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("Dues cannot update.");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new InvalidOperationException("Dues cannot delete.");
    }
}
