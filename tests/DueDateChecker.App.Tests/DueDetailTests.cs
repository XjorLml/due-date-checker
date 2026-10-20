using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class DueDetailTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 10, 17, 16, 30, 0, TimeSpan.FromHours(8));
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"due-details-{Guid.NewGuid():N}");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;

    public DueDetailTests()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "dues.db");
        new DatabaseInitializer(path).Initialize();
        _repository = new SqliteDueRepository(path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(path));
    }

    private Task AddAsync(Due due) => _repository.AddAsync(due);
    private static Due NewDue(string? notes = "Line one\nLine two") =>
        Due.Create("Saved item", Money.FromCentavos(129900), new DateOnly(2026, 10, 19), Created, notes);

    private DueDetailViewModel Details(Guid id, IDueRepository? repository = null)
    {
        var vm = new DueDetailViewModel(_database, repository ?? new ReadOnlyRepository(_repository));
        vm.PrepareForDetails(id.ToString("D"));
        return vm;
    }

    [Fact]
    public async Task Read_and_reopen_show_only_the_selected_committed_record_without_mutation()
    {
        var due = NewDue() with { IsPaid = true };
        await AddAsync(due);
        await AddAsync(NewDue() with { Id = Guid.NewGuid(), Title = "Another item" });
        var vm = Details(due.Id);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal(due, vm.Item!.Due);
        Assert.True(vm.CanEdit);
        Assert.False(vm.IsUnavailable);
        Assert.False(vm.HasError);
        Assert.Equal("Paid", vm.Item.StatusText);
        Assert.Equal("₱1,299.00", vm.Item.AmountText);
        Assert.Equal("19 Oct 2026", vm.Item.DateText);
        Assert.Equal(due.Notes, vm.NotesText);
        Assert.Equal("17 Oct 2026, 08:30 UTC", vm.CreatedText);
        Assert.Equal(vm.CreatedText, vm.UpdatedText);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
        var reopened = Details(due.Id);
        await reopened.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due, reopened.Item!.Due);
        Assert.Equal(2, (await _repository.ListActiveAsync()).Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Missing_notes_have_an_explicit_read_only_empty_state(string? notes)
    {
        var due = NewDue(notes);
        await AddAsync(due);
        var vm = Details(due.Id);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal("No notes.", vm.NotesText);
        Assert.Equal(notes, (await _repository.GetAsync(due.Id))!.Notes);
        Assert.Equal("Unpaid", vm.Item!.StatusText);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-id")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Invalid_target_clears_old_fields_and_never_touches_storage(string? id)
    {
        var due = NewDue();
        await AddAsync(due);
        var reads = new ReadOnlyRepository(_repository);
        var vm = Details(due.Id, reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.HasDetails);
        vm.PrepareForDetails(id);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(1, reads.GetCalls);
        Assert.True(vm.IsUnavailable);
        Assert.False(vm.HasDetails);
        Assert.False(vm.CanEdit);
        Assert.False(vm.ShowRetry);
        Assert.False(vm.LoadCommand.CanExecute(null));
        Assert.Equal("", vm.CreatedText);
        Assert.Equal("", vm.UpdatedText);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unknown_and_deleted_targets_cannot_be_opened_or_restored(bool deleted)
    {
        var due = NewDue();
        if (deleted)
        {
            await AddAsync(due);
            await _repository.SoftDeleteAsync(due.Id, Created.AddDays(1));
        }
        var vm = Details(due.Id);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.IsUnavailable);
        Assert.True(vm.HasError);
        Assert.False(vm.CanEdit);
        Assert.Null(vm.Item);
        Assert.Empty(await _repository.ListActiveAsync());
        Assert.Equal(deleted, (await _repository.GetAsync(due.Id))?.DeletedAt.HasValue ?? false);
    }

    [Fact]
    public async Task Editor_save_and_cancel_refresh_committed_fields_then_delete_makes_details_unavailable()
    {
        var due = NewDue();
        await AddAsync(due);
        var details = Details(due.Id);
        await details.LoadCommand.ExecuteAsync(null);
        var editor = new DueEditViewModel(_database, _repository, new FixedClock(Created.AddDays(1)));
        editor.PrepareForEdit(due.Id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        editor.Title = "Canceled change";
        editor.IsPaid = true;
        // Cancel/Back makes no mutation: returning to details reads stored fields.
        await details.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due, details.Item!.Due);
        editor.Title = "Saved change";
        editor.AmountText = "99.50";
        editor.SelectedDate = new DateTime(2026, 11, 2);
        editor.Notes = "Updated notes";
        await editor.SaveCommand.ExecuteAsync(null);
        await details.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due.Id, details.Item!.Id);
        Assert.Equal(due.CreatedAt, details.Item.Due.CreatedAt);
        Assert.Equal("Saved change", details.Item.Title);
        Assert.Equal("Paid", details.Item.StatusText);
        Assert.Equal("₱99.50", details.Item.AmountText);
        Assert.Equal("02 Nov 2026", details.Item.DateText);
        Assert.Equal("Updated notes", details.NotesText);
        Assert.NotEqual(details.CreatedText, details.UpdatedText);

        var deleteEditor = new DueEditViewModel(_database, _repository, new FixedClock(Created.AddDays(2)));
        deleteEditor.PrepareForEdit(due.Id.ToString());
        await deleteEditor.LoadCommand.ExecuteAsync(null);
        await deleteEditor.DeleteCommand.ExecuteAsync(true);
        await details.LoadCommand.ExecuteAsync(null);
        Assert.True(details.IsUnavailable);
        Assert.False(details.HasDetails);
        Assert.False(details.CanEdit);
        Assert.NotNull((await _repository.GetAsync(due.Id))!.DeletedAt);
    }

    [Fact]
    public async Task Failed_refresh_hides_stale_fields_and_retry_reads_the_same_target()
    {
        var due = NewDue();
        await AddAsync(due);
        var reads = new ReadOnlyRepository(_repository) { FailReads = true };
        var vm = Details(due.Id, reads);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.True(vm.ShowRetry);
        Assert.False(vm.CanEdit);
        reads.FailReads = false;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due, vm.Item!.Due);
        reads.FailReads = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasDetails);
        Assert.False(vm.CanEdit);
        Assert.False(vm.IsBusy);
        Assert.False(vm.IsUnavailable);
        reads.FailReads = false;
        await _repository.UpdateAsync(due with { Title = "Fresh after failure" });
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due.Id, vm.Item!.Id);
        Assert.Equal("Fresh after failure", vm.Item.Title);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task Initialization_failure_keeps_database_path_retryable_without_resetting_saved_data()
    {
        var path = Path.Combine(_directory, "blocked.db");
        Directory.CreateDirectory(path); // SQLite cannot open a directory as a database file.
        var repository = new SqliteDueRepository(path);
        var vm = new DueDetailViewModel(new DatabaseDispatcher(new DatabaseInitializer(path)), new ReadOnlyRepository(repository));
        var due = NewDue();
        vm.PrepareForDetails(due.Id.ToString());
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.ShowRetry);
        Assert.False(vm.CanEdit);
        Assert.True(Directory.Exists(path));
        Directory.Delete(path);
        new DatabaseInitializer(path).Initialize();
        await repository.AddAsync(due);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(due, vm.Item!.Due);
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task Busy_reads_disable_edit_and_ignore_overlapping_refresh()
    {
        var due = NewDue();
        await AddAsync(due);
        var reads = new ReadOnlyRepository(_repository) { BlockReads = true };
        var vm = Details(due.Id, reads);
        var first = vm.LoadCommand.ExecuteAsync(null);
        await reads.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.True(vm.IsBusy);
            Assert.False(vm.CanEdit);
            Assert.False(vm.LoadCommand.CanExecute(null));
            await vm.LoadCommand.ExecuteAsync(null);
            Assert.Equal(1, reads.GetCalls);
        }
        finally
        {
            reads.ReleaseRead.TrySetResult();
            await first;
        }
        Assert.True(vm.CanEdit);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Navigation_failure_retains_loaded_identity_and_never_writes()
    {
        var due = NewDue();
        await AddAsync(due);
        var reads = new ReadOnlyRepository(_repository);
        var vm = Details(due.Id, reads);
        await vm.LoadCommand.ExecuteAsync(null);
        vm.ReportNavigationError();
        Assert.True(vm.HasError);
        Assert.True(vm.CanEdit);
        Assert.Equal(due, vm.Item!.Due);
        Assert.Equal(1, reads.GetCalls);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.HasError);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
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
        public int GetCalls { get; private set; }
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<Due?> GetAsync(Guid id)
        {
            GetCalls++;
            ReadStarted.TrySetResult();
            if (BlockReads) await ReleaseRead.Task;
            if (FailReads) throw new IOException("Controlled read failure");
            return await inner.GetAsync(id);
        }
        public Task<IReadOnlyList<Due>> ListActiveAsync() => throw new InvalidOperationException("Details must use the selected ID.");
        public Task AddAsync(Due due) => throw new InvalidOperationException("Read-only details cannot insert.");
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("Read-only details cannot update.");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new InvalidOperationException("Read-only details cannot delete.");
    }
}
