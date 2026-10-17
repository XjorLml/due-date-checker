using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class DeleteDueTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 10, 16, 9, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"due-delete-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly MutableClock _clock = new(Created.AddDays(1));

    public DeleteDueTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
    }

    private async Task<Due> AddDueAsync()
    {
        var due = Due.Create("Rent", Money.FromDecimal(2500m), new DateOnly(2026, 11, 1), Created, "reference") with { IsPaid = true };
        await _repository.AddAsync(due);
        return due;
    }

    private async Task<DueEditViewModel> OpenAsync(Guid id, IDueRepository? repository = null)
    {
        var editor = new DueEditViewModel(_database, repository ?? _repository, _clock);
        editor.PrepareForEdit(id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    [Fact]
    public async Task Confirmed_delete_survives_reopen_retains_fields_and_refreshes_the_list()
    {
        var due = await AddDueAsync();
        var list = new DuesListViewModel(_database, _repository);
        await list.LoadCommand.ExecuteAsync(null);
        Assert.Single(list.Dues);
        var editor = await OpenAsync(due.Id);
        Assert.True(editor.ShowDelete);
        Assert.True(editor.CanDelete);
        // Delete does not validate or save unfinished form edits.
        editor.Title = " ";
        editor.AmountText = "invalid";
        editor.SelectedDate = null;
        editor.Notes = "Unsaved note";
        editor.IsPaid = false;
        await editor.DeleteCommand.ExecuteAsync(true);

        Assert.True(editor.HasDeleted);
        Assert.True(editor.HasCompleted);
        Assert.False(editor.HasSaved);
        Assert.False(editor.CanEdit);
        Assert.False(editor.ShowDelete);
        Assert.False(editor.CanDelete);
        Assert.True(editor.CanSubmit); // Navigation retry is still available.
        await list.LoadCommand.ExecuteAsync(null);
        Assert.Empty(list.Dues);
        Assert.True(list.IsEmpty);

        SqliteConnection.ClearAllPools();
        var reopened = new SqliteDueRepository(_path);
        var stored = await reopened.GetAsync(due.Id);
        Assert.Equal(due with { DeletedAt = _clock.GetUtcNow(), UpdatedAt = _clock.GetUtcNow() }, stored);
        Assert.Empty(await reopened.ListActiveAsync());
        editor.ReportNavigationError();
        Assert.Contains("deleted", editor.ErrorMessage);
        await editor.SaveCommand.ExecuteAsync(null);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.Equal(stored, await reopened.GetAsync(due.Id));
    }

    [Fact]
    public async Task Cancel_confirmation_leaves_record_and_unsaved_form_unchanged()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository);
        var editor = await OpenAsync(due.Id, fake);
        editor.Title = "Unsaved title";
        editor.Notes = "Unsaved note";
        Assert.Contains(due.Title, editor.DeleteConfirmationMessage);
        Assert.DoesNotContain(editor.Title, editor.DeleteConfirmationMessage);
        await editor.DeleteCommand.ExecuteAsync(false);
        Assert.False(editor.HasDeleted);
        Assert.False(editor.IsBusy);
        Assert.True(editor.CanEdit);
        Assert.True(editor.CanDelete);
        Assert.Equal(0, fake.DeleteCalls);
        Assert.Equal("Unsaved title", editor.Title);
        Assert.Equal("Unsaved note", editor.Notes);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task New_form_cannot_delete()
    {
        var fake = new ControlledRepository(_repository);
        var editor = new DueEditViewModel(_database, fake, _clock);
        Assert.False(editor.ShowDelete);
        Assert.False(editor.CanDelete);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.Equal(0, fake.DeleteCalls);
        Assert.False(editor.HasCompleted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unknown_or_deleted_targets_cannot_delete(bool deleted)
    {
        var due = await AddDueAsync();
        if (deleted) await _repository.SoftDeleteAsync(due.Id, Created.AddHours(1));
        var fake = new ControlledRepository(_repository);
        var editor = await OpenAsync(deleted ? due.Id : Guid.NewGuid(), fake);
        Assert.False(editor.ShowDelete);
        Assert.False(editor.CanDelete);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.Equal(0, fake.DeleteCalls);
        Assert.False(editor.HasDeleted);
    }

    [Fact]
    public async Task Failed_delete_preserves_input_and_allows_retry()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { FailDeletes = true };
        var editor = await OpenAsync(due.Id, fake);
        editor.Title = "Unsaved title";
        editor.Notes = "Unsaved note";
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.False(editor.HasDeleted);
        Assert.False(editor.IsBusy);
        Assert.False(editor.IsDeleting);
        Assert.True(editor.HasError);
        Assert.True(editor.CanDelete);
        Assert.Equal("Unsaved title", editor.Title);
        Assert.Equal("Unsaved note", editor.Notes);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
        fake.FailDeletes = false;
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.True(editor.HasDeleted);
        Assert.False(editor.HasError);
        Assert.Equal(2, fake.DeleteCalls);
        Assert.Equal(1, fake.SuccessfulDeletes);
    }

    [Fact]
    public async Task Repeated_delete_during_write_blocks_save_and_deletes_once()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { BlockDeletes = true };
        var editor = await OpenAsync(due.Id, fake);
        var deletion = editor.DeleteCommand.ExecuteAsync(true);
        try
        {
            await fake.DeleteStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(editor.IsBusy);
            Assert.True(editor.IsDeleting);
            Assert.False(editor.CanEdit);
            Assert.False(editor.CanSubmit);
            Assert.False(editor.CanDelete);
            await editor.DeleteCommand.ExecuteAsync(true);
            await editor.SaveCommand.ExecuteAsync(null);
        }
        finally
        {
            fake.ReleaseDelete.TrySetResult();
            await deletion;
        }
        Assert.True(editor.HasDeleted);
        Assert.Equal(1, fake.DeleteCalls);
        Assert.Equal(1, fake.SuccessfulDeletes);
        var stored = await _repository.GetAsync(due.Id);
        _clock.Now = _clock.Now.AddDays(1);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.Equal(1, fake.DeleteCalls);
        Assert.Equal(stored, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Deletion_by_another_writer_after_loading_keeps_the_original_tombstone()
    {
        var due = await AddDueAsync();
        var editor = await OpenAsync(due.Id);
        var firstDeletion = Created.AddHours(1);
        await _repository.SoftDeleteAsync(due.Id, firstDeletion);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.True(editor.HasDeleted);
        Assert.Equal(firstDeletion, (await _repository.GetAsync(due.Id))!.DeletedAt);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class ControlledRepository(IDueRepository inner) : IDueRepository
    {
        public bool FailDeletes { get; set; }
        public bool BlockDeletes { get; init; }
        public int DeleteCalls { get; private set; }
        public int SuccessfulDeletes { get; private set; }
        public TaskCompletionSource DeleteStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseDelete { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<Due?> GetAsync(Guid id) => inner.GetAsync(id);
        public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
        public Task AddAsync(Due due) => throw new InvalidOperationException("Delete must not insert.");
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("Delete must not save form edits.");
        public async Task SoftDeleteAsync(Guid id, DateTimeOffset when)
        {
            DeleteCalls++;
            if (FailDeletes) throw new IOException("Controlled delete failure");
            DeleteStarted.TrySetResult();
            if (BlockDeletes) await ReleaseDelete.Task;
            await inner.SoftDeleteAsync(id, when);
            SuccessfulDeletes++;
        }
    }
}
