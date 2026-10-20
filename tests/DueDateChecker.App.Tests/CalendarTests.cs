using System.Collections.Specialized;
using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class CalendarTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 10, 18);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"calendar-{Guid.NewGuid():N}");
    private readonly SqliteDueRepository _repository;
    private readonly AppPreferencesService _preferences = new(new MemoryPreferencesStorage());
    private readonly DatabaseDispatcher _database;
    private readonly MutableClock _clock = new(new DateTimeOffset(2026, 10, 18, 1, 0, 0, TimeSpan.Zero));

    public CalendarTests()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "dues.db");
        new DatabaseInitializer(path).Initialize();
        _repository = new SqliteDueRepository(path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(path));
    }

    private CalendarViewModel Calendar(IDueRepository? repository = null) => new(_database, repository ?? new ReadOnlyRepository(_repository), _clock, _preferences);
    private Due NewDue(string title, int offset = 0, bool paid = false, int createdOffset = 0) =>
        Due.Create(title, Money.FromCentavos(0), Today.AddDays(offset), _clock.Now.AddSeconds(createdOffset)) with { IsPaid = paid };

    [Theory]
    [InlineData(WeekStartPreference.Monday, 2026, 10)]
    [InlineData(WeekStartPreference.Sunday, 2026, 10)]
    [InlineData(WeekStartPreference.Monday, 2028, 2)]
    [InlineData(WeekStartPreference.Sunday, 2028, 2)]
    [InlineData(WeekStartPreference.Monday, 1, 1)]
    [InlineData(WeekStartPreference.Sunday, 1, 1)]
    [InlineData(WeekStartPreference.Monday, 9999, 12)]
    [InlineData(WeekStartPreference.Sunday, 9999, 12)]
    public void Saved_week_start_aligns_headers_and_every_real_date_at_leap_and_date_bounds(WeekStartPreference start, int year, int month)
    {
        _preferences.Save(new(WeekStart: start));
        _clock.Now = new DateTimeOffset(year, month, 1, 12, 0, 0, TimeSpan.Zero);
        var vm = Calendar();
        Assert.Equal(42, vm.Days.Count);
        Assert.Equal(start == WeekStartPreference.Monday ? "M" : "Su", vm.Weekdays[0]);
        Assert.StartsWith(start.ToString()+" first", vm.WeekStartDescription);
        var dates = vm.Days.Where(day => day.Date.HasValue).ToArray();
        Assert.Equal(DateTime.DaysInMonth(year, month), dates.Length);
        for (var i = 0; i < vm.Days.Count; i++)
            if (vm.Days[i].Date is { } date)
                Assert.Equal(((int)date.DayOfWeek+(start == WeekStartPreference.Monday ? 6 : 0))%7, i%7);
        Assert.Equal(Enumerable.Range(1, dates.Length), dates.Select(day => day.Date!.Value.Day));
    }

    [Fact]
    public async Task Changing_week_start_preserves_view_selection_agenda_counts_and_saved_fields()
    {
        var due = NewDue("Unchanged calendar", paid: true);
        await _repository.AddAsync(due);
        var vm = Calendar();
        await vm.LoadCommand.ExecuteAsync(null);
        var agenda = vm.Agenda.ToArray();
        var selected = vm.SelectedDate;
        var before = await _repository.GetAsync(due.Id);
        _preferences.Save(new(WeekStart: WeekStartPreference.Sunday));
        vm.RefreshPreferences();
        Assert.Equal(selected, vm.SelectedDate);
        Assert.Equal(new DateOnly(2026, 10, 1), vm.Month);
        Assert.Equal(agenda, vm.Agenda);
        Assert.Equal(1, vm.Days.Sum(day => day.Count));
        Assert.Equal(before, await _repository.GetAsync(due.Id));
        vm.MoveMonth(1);
        vm.SelectDate(new(2026, 11, 12));
        _preferences.Save(new(WeekStart: WeekStartPreference.Monday));
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new DateOnly(2026, 11, 12), vm.SelectedDate);
        Assert.Equal(new DateOnly(2026, 11, 1), vm.Month);
        Assert.Equal("M", vm.Weekdays[0]);
        Assert.Empty(vm.Agenda);
    }

    [Fact]
    public async Task Saved_counts_match_exact_agenda_in_creation_order_include_paid_and_exclude_tombstones_without_writes()
    {
        var later = NewDue("Later", createdOffset: 2);
        var earlier = NewDue("Paid earlier", paid: true);
        var next = NewDue("Next day", 1);
        var deleted = NewDue("Deleted");
        foreach (var due in new[] { later, earlier, next, deleted }) await _repository.AddAsync(due);
        await _repository.SoftDeleteAsync(deleted.Id, _clock.Now);
        var before = await _repository.ListActiveAsync();
        // Deliberately return a tombstone as well: the projection must exclude it.
        var reads = new ReadOnlyRepository(_repository) { Extra = deleted with { DeletedAt = _clock.Now } };
        var vm = Calendar(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new[] { earlier.Id, later.Id }, vm.Agenda.Select(row => row.Id));
        Assert.Equal(new[] { "Paid", "Due today" }, vm.Agenda.Select(row => row.StatusText));
        Assert.Equal(2, Assert.Single(vm.Days, day => day.Date == Today).Count);
        Assert.Equal(3, vm.Days.Sum(day => day.Count));
        vm.SelectDate(Today.AddDays(1));
        Assert.Equal(next.Id, Assert.Single(vm.Agenda).Id);
        Assert.Equal("Upcoming", vm.Agenda[0].StatusText);
        Assert.Contains("1 saved dues", Assert.Single(vm.Days, day => day.IsSelected).AccessibilityText);
        Assert.Equal(before, await _repository.ListActiveAsync());
        Assert.Equal(1, reads.Reads);
    }

    [Fact]
    public async Task Monday_grid_aligns_real_today_selection_and_unselectable_padding()
    {
        var vm = Calendar();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(42, vm.Days.Count);
        Assert.All(vm.Days.Take(3), day => Assert.Null(day.Date));
        Assert.Equal(new DateOnly(2026, 10, 1), vm.Days[3].Date);
        Assert.Equal(31, vm.Days.Count(day => day.Date.HasValue));
        var selected = Assert.Single(vm.Days, day => day.IsSelected);
        Assert.Equal(Today, selected.Date);
        Assert.True(selected.IsToday);
        Assert.Contains("today", selected.AccessibilityText);
        Assert.Contains("selected", selected.AccessibilityText);
        vm.SelectDate(new(2026, 11, 1));
        vm.MoveMonth(0);
        vm.MoveMonth(2);
        Assert.Equal(Today, vm.SelectedDate);
        Assert.Equal("Sunday, 18 Oct 2026", vm.SelectedDateText);
    }

    [Fact]
    public void Month_arrows_select_first_cross_year_and_Today_reads_current_Manila_clock()
    {
        var vm = Calendar();
        for (var i = 0; i < 3; i++) vm.MoveMonth(1);
        Assert.Equal(new DateOnly(2027, 1, 1), vm.Month);
        Assert.Equal(vm.Month, vm.SelectedDate);
        vm.MoveMonth(-1);
        Assert.Equal(new DateOnly(2026, 12, 1), vm.Month);
        _clock.Now = new DateTimeOffset(2026, 10, 18, 16, 0, 0, TimeSpan.Zero);
        vm.ResetToday();
        Assert.Equal(Today.AddDays(1), vm.SelectedDate);
        Assert.Equal(new DateOnly(2026, 10, 1), vm.Month);
        Assert.Contains("19 Oct 2026", vm.TodayDescription);
    }

    [Fact]
    public void Leap_and_non_leap_February_have_correct_days()
    {
        var vm = Calendar();
        for (var i = 0; i < 16; i++) vm.MoveMonth(1);
        Assert.Equal(new DateOnly(2028, 2, 1), vm.Month);
        Assert.Equal(29, vm.Days.Count(day => day.Date.HasValue));
        for (var i = 0; i < 12; i++) vm.MoveMonth(-1);
        Assert.Equal(28, vm.Days.Count(day => day.Date.HasValue));
    }

    [Fact]
    public void Month_navigation_stops_at_DateOnly_bounds_without_overflow()
    {
        _clock.Now = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var vm = Calendar();
        Assert.False(vm.CanGoPrevious);
        vm.MoveMonth(-1);
        Assert.Equal(DateOnly.MinValue, vm.Month);
        Assert.Equal(31, vm.Days.Count(day => day.Date.HasValue));
        _clock.Now = new DateTimeOffset(9999, 12, 31, 12, 0, 0, TimeSpan.Zero);
        vm.ResetToday();
        Assert.False(vm.CanGoNext);
        vm.MoveMonth(1);
        Assert.Equal(new DateOnly(9999, 12, 1), vm.Month);
        Assert.Equal(31, vm.Days.Count(day => day.Date.HasValue));
        Assert.Equal(new DateOnly(9999, 12, 31), vm.SelectedDate);
    }

    [Fact]
    public async Task No_saved_dues_and_empty_selected_day_are_distinct_without_false_loading_empty()
    {
        var vm = Calendar();
        Assert.False(vm.IsEmpty);
        Assert.False(vm.IsDayEmpty);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.IsDayEmpty);
        await _repository.AddAsync(NewDue("Tomorrow", 1));
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.IsDayEmpty);
        Assert.Equal("0 saved dues", vm.AgendaSummary);
        vm.SelectDate(Today.AddDays(1));
        Assert.False(vm.IsDayEmpty);
        Assert.Equal("1 saved due", vm.AgendaSummary);
    }

    [Fact]
    public async Task Midnight_reclassifies_agenda_and_today_marker_without_moving_user_selection_or_reading_storage()
    {
        _clock.Now = new DateTimeOffset(2026, 10, 17, 15, 59, 59, TimeSpan.Zero);
        await _repository.AddAsync(NewDue("Boundary"));
        await _repository.AddAsync(NewDue("Paid boundary", paid: true));
        var reads = new ReadOnlyRepository(_repository);
        var vm = Calendar(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        vm.SelectDate(Today);
        Assert.Equal("Upcoming", vm.Agenda[0].StatusText);
        _clock.Now = _clock.Now.AddSeconds(1);
        vm.RefreshDate();
        Assert.Equal(Today, vm.SelectedDate);
        Assert.Equal("Due today", vm.Agenda[0].StatusText);
        Assert.Equal("Paid", vm.Agenda[1].StatusText);
        Assert.True(Assert.Single(vm.Days, day => day.Date == Today).IsToday);
        _clock.Now = _clock.Now.AddDays(1);
        vm.RefreshDate();
        Assert.Equal("Overdue", vm.Agenda[0].StatusText);
        Assert.Equal(Today, vm.SelectedDate);
        vm.MoveMonth(1);
        var selected = vm.SelectedDate;
        _clock.Now = _clock.Now.AddDays(1);
        vm.RefreshDate();
        Assert.Equal(selected, vm.SelectedDate);
        Assert.DoesNotContain(vm.Days, day => day.IsToday);
        Assert.Equal(1, reads.Reads);
    }

    [Fact]
    public async Task Read_completion_uses_new_date_and_latest_selection_even_if_midnight_passes_during_read()
    {
        _clock.Now = new DateTimeOffset(2026, 10, 17, 15, 59, 59, TimeSpan.Zero);
        await _repository.AddAsync(NewDue("Read boundary"));
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Calendar(reads);
        var load = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            vm.SelectDate(Today);
            _preferences.Save(new(WeekStart: WeekStartPreference.Sunday));
            vm.RefreshPreferences(); // Appearance can share an unfinished data load.
            Assert.Equal("Su", vm.Weekdays[0]);
            _clock.Now = _clock.Now.AddSeconds(1);
        }
        finally { reads.ReleaseRead.TrySetResult(); await load; }
        Assert.Equal(Today, vm.Today);
        Assert.Equal(Today, vm.SelectedDate);
        Assert.Equal("Due today", Assert.Single(vm.Agenda).StatusText);
        Assert.Equal("Su", vm.Weekdays[0]);
    }

    [Fact]
    public async Task Editor_create_cancel_paid_date_change_and_delete_refresh_same_calendar_selection()
    {
        var vm = Calendar();
        await vm.LoadCommand.ExecuteAsync(null);
        var editor = new DueEditViewModel(_database, _repository, _clock) { Title = "Calendar create", AmountText = "0" };
        Assert.Equal(Today, DateOnly.FromDateTime(editor.SelectedDate!.Value));
        await editor.SaveCommand.ExecuteAsync(null);
        await vm.LoadCommand.ExecuteAsync(null);
        var id = Assert.Single(vm.Agenda).Id;
        Assert.Equal(editor.SavedDueId, id);
        var edit = new DueEditViewModel(_database, _repository, _clock);
        edit.PrepareForEdit(id.ToString());
        await edit.LoadCommand.ExecuteAsync(null);
        edit.IsPaid = true;
        await vm.LoadCommand.ExecuteAsync(null); // Unsaved edit/Cancel has no effect.
        Assert.Equal("Due today", Assert.Single(vm.Agenda).StatusText);
        await edit.SaveCommand.ExecuteAsync(null);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Paid", Assert.Single(vm.Agenda).StatusText);
        Assert.Equal(1, Assert.Single(vm.Days, day => day.Date == Today).Count);
        var move = new DueEditViewModel(_database, _repository, _clock);
        move.PrepareForEdit(id.ToString());
        await move.LoadCommand.ExecuteAsync(null);
        move.SelectedDate = Today.AddDays(1).ToDateTime(TimeOnly.MinValue);
        await move.SaveCommand.ExecuteAsync(null);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(Today, vm.SelectedDate);
        Assert.Empty(vm.Agenda);
        Assert.Equal(0, Assert.Single(vm.Days, day => day.Date == Today).Count);
        vm.SelectDate(Today.AddDays(1));
        Assert.Equal(id, Assert.Single(vm.Agenda).Id);
        var remove = new DueEditViewModel(_database, _repository, _clock);
        remove.PrepareForEdit(id.ToString());
        await remove.LoadCommand.ExecuteAsync(null);
        await remove.DeleteCommand.ExecuteAsync(true);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(Today.AddDays(1), vm.SelectedDate);
        Assert.True(vm.IsEmpty);
        Assert.Equal(0, vm.Days.Sum(day => day.Count));
        Assert.NotNull((await _repository.GetAsync(id))!.DeletedAt);
    }

    [Fact]
    public async Task Refresh_failure_preserves_counts_rows_and_selection_with_stale_warning_then_retry_replaces_snapshot()
    {
        await _repository.AddAsync(NewDue("Saved"));
        var reads = new ReadOnlyRepository(_repository) { FailReads = true };
        var vm = Calendar(reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.False(vm.HasLoaded);
        Assert.False(vm.IsEmpty);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        var original = Assert.Single(vm.Agenda);
        await _repository.UpdateAsync(original.Due with { Title = "Changed" });
        reads.FailReads = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(original, Assert.Single(vm.Agenda));
        Assert.Contains("out of date", vm.ErrorMessage);
        Assert.Equal(1, Assert.Single(vm.Days, day => day.Date == Today).Count);
        Assert.False(vm.IsDayEmpty);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Changed", Assert.Single(vm.Agenda).Title);
        Assert.Equal(Today, vm.SelectedDate);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task Initialization_failure_preserves_path_blocks_navigation_and_recovers_on_retry()
    {
        var path = Path.Combine(_directory, "blocked.db");
        Directory.CreateDirectory(path);
        var vm = new CalendarViewModel(new DatabaseDispatcher(new DatabaseInitializer(path)), new SqliteDueRepository(path), _clock, _preferences);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.CanNavigate);
        Assert.False(vm.HasLoaded);
        Assert.False(vm.IsEmpty);
        Assert.True(vm.HasError);
        Assert.True(Directory.Exists(path));
        Directory.Delete(path);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.CanNavigate);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task Busy_load_blocks_overlapping_reads_and_navigation_and_keeps_selected_month()
    {
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Calendar(reads);
        var load = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.True(vm.IsBusy);
            Assert.False(vm.CanNavigate);
            Assert.False(vm.IsEmpty);
            Assert.False(vm.LoadCommand.CanExecute(null));
            vm.MoveMonth(1);
            await vm.LoadCommand.ExecuteAsync(null);
            Assert.Equal(1, reads.Reads);
        }
        finally { reads.ReleaseRead.TrySetResult(); await load; }
        Assert.Equal(new DateOnly(2026, 11, 1), vm.SelectedDate);
        Assert.True(vm.CanNavigate);
    }

    [Fact]
    public async Task Selection_retains_one_read_only_agenda_source_without_Reset_events_or_duplicate_rows()
    {
        await _repository.AddAsync(NewDue("One"));
        await _repository.AddAsync(NewDue("Two", 1));
        var vm = Calendar();
        var source = vm.Agenda;
        var events = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)source).CollectionChanged += (_, args) => events.Add(args.Action);
        await vm.LoadCommand.ExecuteAsync(null);
        vm.SelectDate(Today.AddDays(1));
        vm.SelectDate(Today.AddDays(2));
        vm.ResetToday();
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Same(source, vm.Agenda);
        Assert.Single(source);
        Assert.NotEmpty(events);
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, events);
        Assert.Throws<NotSupportedException>(() => ((IList<DashboardItem>)source).Clear());
    }

    [Fact]
    public async Task Reopened_database_retains_paid_and_delete_while_new_session_resets_only_calendar_selection()
    {
        var paid = NewDue("Reopen paid", 1, true);
        var removed = NewDue("Reopen deleted", 1);
        await _repository.AddAsync(paid);
        await _repository.AddAsync(removed);
        await _repository.SoftDeleteAsync(removed.Id, _clock.Now);
        var vm = Calendar();
        await vm.LoadCommand.ExecuteAsync(null);
        vm.MoveMonth(1);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new DateOnly(2026, 11, 1), vm.SelectedDate);
        vm.ReportNavigationError();
        Assert.True(vm.HasError);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        var path = Path.Combine(_directory, "dues.db");
        var reopened = new CalendarViewModel(new DatabaseDispatcher(new DatabaseInitializer(path)), new ReadOnlyRepository(new SqliteDueRepository(path)), _clock, _preferences);
        await reopened.LoadCommand.ExecuteAsync(null);
        Assert.Equal(Today, reopened.SelectedDate);
        reopened.SelectDate(Today.AddDays(1));
        Assert.Equal(paid.Id, Assert.Single(reopened.Agenda).Id);
        Assert.Equal("Paid", reopened.Agenda[0].StatusText);
        Assert.Equal(1, Assert.Single(reopened.Days, day => day.Date == Today.AddDays(1)).Count);
        Assert.NotNull((await _repository.GetAsync(removed.Id))!.DeletedAt);
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
        public Due? Extra { get; init; }
        public int Reads { get; private set; }
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<IReadOnlyList<Due>> ListActiveAsync()
        {
            Reads++;
            ReadStarted.TrySetResult();
            if (BlockReads) await ReleaseRead.Task;
            if (FailReads) throw new IOException("Controlled read failure");
            var dues = await inner.ListActiveAsync();
            return Extra is { } extra ? dues.Append(extra).ToArray() : dues;
        }
        public Task<Due?> GetAsync(Guid id) => throw new InvalidOperationException("Calendar reads the active snapshot only.");
        public Task AddAsync(Due due) => throw new InvalidOperationException("Calendar cannot insert.");
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("Calendar cannot update.");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new InvalidOperationException("Calendar cannot delete.");
    }
}
