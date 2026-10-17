using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class EditDueTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 10, 16, 9, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"due-edit-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly TimeProvider _clock = new FixedClock(Created.AddDays(1));

    public EditDueTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
    }

    private DueEditViewModel NewEditor(IDueRepository? repository = null) => new(_database, repository ?? _repository, _clock);

    private async Task<Due> AddDueAsync()
    {
        var due = Due.Create("Rent", Money.FromDecimal(2450.50m), new DateOnly(2026, 11, 1), Created, "ref 123");
        await _repository.AddAsync(due);
        return due;
    }

    private async Task<DueEditViewModel> OpenAsync(Guid id, IDueRepository? repository = null)
    {
        var editor = NewEditor(repository);
        editor.PrepareForEdit(id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    [Fact]
    public async Task Load_populates_fields_without_writing_and_cancel_leaves_the_database_unchanged()
    {
        var due = await AddDueAsync();
        var editor = await OpenAsync(due.Id);
        Assert.True(editor.IsEditMode);
        Assert.Equal("Edit due", editor.PageTitle);
        Assert.True(editor.CanEdit);
        Assert.Equal(due.Title, editor.Title);
        Assert.Equal("2450.50", editor.AmountText);
        Assert.Equal(new DateTime(2026, 11, 1), editor.SelectedDate);
        Assert.Equal(due.Notes, editor.Notes);
        Assert.False(editor.IsPaid);
        editor.Title = "Unsaved change";
        editor.IsPaid = true;
        Assert.Equal(due, await _repository.GetAsync(due.Id)); // Leaving without Save has no mutation.
    }

    [Fact]
    public async Task Edit_round_trips_all_fields_keeps_identity_and_refreshes_order_and_paid_text()
    {
        var due = await AddDueAsync();
        await _repository.AddAsync(Due.Create("Other", Money.FromCentavos(1), new DateOnly(2026, 10, 30), Created));
        var editor = await OpenAsync(due.Id);
        editor.Title = "  Rent updated  ";
        editor.AmountText = "2500.005";
        editor.SelectedDate = new DateTime(2026, 10, 20);
        editor.Notes = "updated note";
        editor.IsPaid = true;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(due.Id, editor.SavedDueId);
        SqliteConnection.ClearAllPools();
        var reopened = new SqliteDueRepository(_path);
        var stored = await reopened.GetAsync(due.Id);
        Assert.NotNull(stored);
        Assert.Equal("Rent updated", stored.Title);
        Assert.Equal(250001, stored.Amount.Centavos);
        Assert.Equal(new DateOnly(2026, 10, 20), stored.DueDate);
        Assert.Equal("updated note", stored.Notes);
        Assert.True(stored.IsPaid);
        Assert.Equal(due.CreatedAt, stored.CreatedAt);
        Assert.Equal(_clock.GetUtcNow(), stored.UpdatedAt);
        Assert.Null(stored.DeletedAt);

        var list = new DuesListViewModel(new DatabaseDispatcher(new DatabaseInitializer(_path)), reopened);
        await list.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new[] { "Rent updated", "Other" }, list.Dues.Select(row => row.Title));
        Assert.Equal("Paid", list.Dues[0].StatusText);

        var unpaidEditor = await OpenAsync(due.Id);
        Assert.True(unpaidEditor.IsPaid);
        unpaidEditor.IsPaid = false;
        unpaidEditor.Notes = "  ";
        await unpaidEditor.SaveCommand.ExecuteAsync(null);
        var unpaid = await new SqliteDueRepository(_path).GetAsync(due.Id);
        Assert.False(unpaid!.IsPaid);
        Assert.Null(unpaid.Notes);
        Assert.Equal(due.CreatedAt, unpaid.CreatedAt);
        await list.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Unpaid", list.Dues[0].StatusText);
        Assert.Equal(2, list.Dues.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unknown_or_deleted_target_is_unavailable_and_cannot_be_saved(bool deleted)
    {
        var due = await AddDueAsync();
        if (deleted) await _repository.SoftDeleteAsync(due.Id, Created.AddHours(1));
        var editor = await OpenAsync(deleted ? due.Id : Guid.NewGuid());
        Assert.True(editor.IsUnavailable);
        Assert.False(editor.CanEdit);
        Assert.False(editor.CanSubmit);
        Assert.False(editor.LoadCommand.CanExecute(null));
        Assert.True(editor.HasError);
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.False(editor.HasSaved);
        var stored = await _repository.GetAsync(due.Id);
        Assert.Equal(deleted, stored!.DeletedAt.HasValue);
        Assert.Equal("Rent", stored.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Invalid_navigation_id_never_turns_into_create_mode(string? id)
    {
        var editor = NewEditor();
        editor.PrepareForEdit(id);
        await editor.LoadCommand.ExecuteAsync(null);
        editor.Title = "Do not insert";
        editor.AmountText = "1";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.IsEditMode);
        Assert.True(editor.IsUnavailable);
        Assert.False(editor.HasSaved);
        Assert.Empty(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Deletion_after_loading_is_rechecked_and_does_not_resurrect_the_row()
    {
        var due = await AddDueAsync();
        var editor = await OpenAsync(due.Id);
        editor.Title = "Changed";
        editor.IsPaid = true;
        var deletedAt = Created.AddHours(2);
        await _repository.SoftDeleteAsync(due.Id, deletedAt);
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.IsUnavailable);
        Assert.False(editor.HasSaved);
        var stored = await _repository.GetAsync(due.Id);
        Assert.Equal(deletedAt, stored!.DeletedAt);
        Assert.Equal("Rent", stored.Title);
        Assert.False(stored.IsPaid);
        Assert.Empty(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Load_failure_disables_save_and_retry_populates_the_form()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { FailReads = true };
        var editor = await OpenAsync(due.Id, fake);
        Assert.False(editor.CanSubmit);
        Assert.True(editor.HasError);
        Assert.True(editor.ShowLoadRetry);
        fake.FailReads = false;
        await editor.LoadCommand.ExecuteAsync(null);
        Assert.True(editor.CanEdit);
        Assert.False(editor.HasError);
        Assert.False(editor.ShowLoadRetry);
        Assert.Equal(due.Title, editor.Title);
    }

    [Fact]
    public async Task Failed_edit_keeps_input_and_retry_updates_once_without_inserting()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { FailUpdates = true };
        var editor = await OpenAsync(due.Id, fake);
        editor.Title = "Changed";
        editor.AmountText = "42";
        editor.IsPaid = true;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.False(editor.HasSaved);
        Assert.True(editor.CanEdit);
        Assert.True(editor.HasError);
        Assert.Equal("Changed", editor.Title);
        Assert.Equal("42", editor.AmountText);
        Assert.True(editor.IsPaid);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
        fake.FailUpdates = false;
        await editor.SaveCommand.ExecuteAsync(null);
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasSaved);
        Assert.Equal(1, fake.SuccessfulUpdates);
        Assert.Single(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Save_uses_fresh_immutable_fields_instead_of_the_loaded_copy()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository);
        var editor = await OpenAsync(due.Id, fake);
        // Simulate a refresh from another source before Save. The App must re-read.
        var changedCreatedAt = Created.AddHours(-1);
        using (var connection = new SqliteConnection($"Data Source={_path}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE dues SET created_at = @when WHERE id = @id";
            command.Parameters.AddWithValue("@when", changedCreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@id", due.Id.ToString());
            command.ExecuteNonQuery();
        }
        editor.Title = "Changed";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(changedCreatedAt, (await _repository.GetAsync(due.Id))!.CreatedAt);
        Assert.Equal(changedCreatedAt, fake.LastUpdate!.CreatedAt);
    }

    [Fact]
    public async Task Repeated_load_does_not_overwrite_unsaved_input()
    {
        var due = await AddDueAsync();
        var editor = await OpenAsync(due.Id);
        editor.Title = "Unsaved title";
        editor.IsPaid = true;
        await editor.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Unsaved title", editor.Title);
        Assert.True(editor.IsPaid);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Read_failure_before_update_keeps_edit_mode_and_input_for_retry()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository);
        var editor = await OpenAsync(due.Id, fake);
        editor.Title = "Changed";
        fake.FailReads = true;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.IsEditMode);
        Assert.True(editor.HasError);
        Assert.True(editor.CanEdit);
        Assert.False(editor.HasSaved);
        Assert.Equal("Changed", editor.Title);
        fake.FailReads = false;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(due.Id, editor.SavedDueId);
        Assert.Equal(1, fake.SuccessfulUpdates);
        Assert.Single(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Deletion_between_read_and_update_is_unavailable_instead_of_saved()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { DeleteBeforeUpdate = true };
        var editor = await OpenAsync(due.Id, fake);
        editor.Title = "Changed";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.IsUnavailable);
        Assert.False(editor.HasSaved);
        Assert.False(editor.CanSubmit);
        var stored = await _repository.GetAsync(due.Id);
        Assert.NotNull(stored!.DeletedAt);
        Assert.Equal("Rent", stored.Title);
    }

    [Fact]
    public async Task Repeated_save_during_an_update_writes_once()
    {
        var due = await AddDueAsync();
        var fake = new ControlledRepository(_repository) { BlockUpdates = true };
        var editor = await OpenAsync(due.Id, fake);
        editor.IsPaid = true;
        var save = editor.SaveCommand.ExecuteAsync(null);
        try
        {
            await fake.UpdateStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(editor.CanEdit);
            Assert.False(editor.CanSubmit);
            Assert.False(editor.LoadCommand.CanExecute(null));
            await editor.SaveCommand.ExecuteAsync(null);
        }
        finally
        {
            fake.ReleaseUpdate.TrySetResult();
            await save;
        }
        Assert.True(editor.HasSaved);
        Assert.Equal(1, fake.SuccessfulUpdates);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    [Fact]
    public async Task Invalid_edit_does_not_change_the_record()
    {
        var due = await AddDueAsync();
        var editor = await OpenAsync(due.Id);
        editor.AmountText = "-1";
        editor.Title = " ";
        editor.SelectedDate = null;
        editor.IsPaid = true;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasAmountError);
        Assert.True(editor.HasTitleError);
        Assert.True(editor.HasDateError);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ControlledRepository(IDueRepository inner) : IDueRepository
    {
        public bool FailReads { get; set; }
        public bool FailUpdates { get; set; }
        public bool DeleteBeforeUpdate { get; init; }
        public bool BlockUpdates { get; init; }
        public TaskCompletionSource UpdateStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseUpdate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Due? LastUpdate { get; private set; }
        public int SuccessfulUpdates { get; private set; }
        public Task<Due?> GetAsync(Guid id) => FailReads ? throw new IOException("Controlled read failure") : inner.GetAsync(id);
        public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
        public Task AddAsync(Due due) => throw new InvalidOperationException("An edit must not insert.");
        public async Task UpdateAsync(Due due)
        {
            if (FailUpdates) throw new IOException("Controlled write failure");
            LastUpdate = due;
            UpdateStarted.TrySetResult();
            if (BlockUpdates) await ReleaseUpdate.Task;
            if (DeleteBeforeUpdate) await inner.SoftDeleteAsync(due.Id, Created.AddHours(1));
            await inner.UpdateAsync(due);
            SuccessfulUpdates++;
        }
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => inner.SoftDeleteAsync(id, when);
    }
}
