using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX06 confirmation messages and Undo after Delete (specs/ux06-confirmations-undo CM01–CM08),
/// plus UX12 Manila timestamps on Details.</summary>
public sealed class ConfirmationUndoTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-undo-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _raw;
    private readonly IDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly Clock _clock = new() { Now = new DateTimeOffset(2026, 11, 6, 1, 0, 0, TimeSpan.Zero) };
    private readonly FakeMessages _messages = new();
    private readonly FakePlatform _platform = new();

    public ConfirmationUndoTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _raw = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
        // The production registration: writes go through the reminder-aware wrapper.
        _repository = new ReminderAwareDueRepository(_raw,
            new ReminderCoordinator(_database, _raw, new SqliteReminderStore(_path), _platform, _clock));
    }

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    private static Due Rent() => Due.Create("Rent", Money.FromCentavos(1500000), new DateOnly(2026, 11, 30), Created, "Unit 4B", DueCategory.Bill);

    private DeletionUndo NewUndo(IDueRepository? repository = null) => new(_database, repository ?? _repository, _messages, _clock);

    private async Task<DueEditViewModel> EditorAsync(Due due, IDueRepository? repository = null)
    {
        var editor = new DueEditViewModel(_database, repository ?? _repository, _clock);
        editor.PrepareForEdit(due.Id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    private async Task<DueDeletion> DeleteThroughEditorAsync(Due due)
    {
        var editor = await EditorAsync(due);
        await editor.DeleteCommand.ExecuteAsync(true);
        return editor.Deletion!;
    }

    // ---- CM01, CM03, CM07: editor messages ----

    [Fact]
    public async Task Save_messages_say_what_happened_and_failures_say_nothing()
    {
        var create = new DueEditViewModel(_database, _repository, _clock) { Title = "Rent", AmountText = "15000" };
        await create.SaveCommand.ExecuteAsync(null);
        Assert.Equal("Due saved.", create.CompletionMessage);

        var saved = Assert.Single(await _raw.ListActiveAsync());
        var edit = await EditorAsync(saved);
        edit.Notes = "Changed";
        await edit.SaveCommand.ExecuteAsync(null);
        Assert.Equal("Changes saved.", edit.CompletionMessage);
        Assert.Null(edit.Deletion);

        var failing = new DueEditViewModel(_database, new FailingRepository(_raw), _clock) { Title = "Water", AmountText = "850" };
        await failing.SaveCommand.ExecuteAsync(null);
        Assert.True(failing.HasError);
        Assert.Equal("", failing.CompletionMessage);
    }

    [Fact]
    public async Task Delete_records_the_exact_deletion_and_cancel_or_failure_records_none()
    {
        var due = Rent();
        await _raw.AddAsync(due);

        var cancelled = await EditorAsync(due);
        await cancelled.DeleteCommand.ExecuteAsync(false);
        Assert.Equal(("", (DueDeletion?)null), (cancelled.CompletionMessage, cancelled.Deletion));

        var failing = await EditorAsync(due, new FailingRepository(_raw));
        await failing.DeleteCommand.ExecuteAsync(true);
        Assert.Equal(("", (DueDeletion?)null), (failing.CompletionMessage, failing.Deletion));

        var editor = await EditorAsync(due);
        await editor.DeleteCommand.ExecuteAsync(true);
        Assert.Equal("Due deleted.", editor.CompletionMessage);
        Assert.Equal(new DueDeletion(due.Id, _clock.Now), editor.Deletion);
        Assert.Equal(_clock.Now, (await _raw.GetAsync(due.Id))!.DeletedAt);
    }

    // ---- CM03, CM04: Undo ----

    [Fact]
    public async Task Undo_restores_the_due_refreshes_and_plans_its_reminders()
    {
        var due = Rent();
        await _raw.AddAsync(due);
        var deletion = await DeleteThroughEditorAsync(due);
        var undo = NewUndo();
        Guid? restoredId = null;
        undo.Restored += (_, id) => restoredId = id;

        undo.Offer(deletion);
        Assert.Equal(("Due deleted.", true), (_messages.Last, _messages.LastHadUndo));
        var requests = _platform.Requests;
        _clock.Now = _clock.Now.AddSeconds(3);

        Assert.True(await _messages.TapUndoAsync());

        Assert.Equal(due with { UpdatedAt = _clock.Now }, await _raw.GetAsync(due.Id));
        Assert.Equal("Due restored.", _messages.Last);
        Assert.Equal(due.Id, restoredId);
        Assert.True(_platform.Requests > requests);
    }

    [Fact]
    public async Task Undo_works_once_and_only_for_the_latest_deletion()
    {
        var first = Rent();
        var second = Rent() with { Id = Guid.NewGuid(), Title = "Water" };
        await _raw.AddAsync(first);
        await _raw.AddAsync(second);
        var undo = NewUndo();

        var firstDeletion = await DeleteThroughEditorAsync(first);
        undo.Offer(firstDeletion);
        undo.Offer(await DeleteThroughEditorAsync(second)); // a newer Delete replaces the older offer

        Assert.False(await undo.UndoAsync(firstDeletion));
        Assert.Equal("Nothing to undo.", _messages.Last);
        Assert.NotNull((await _raw.GetAsync(first.Id))!.DeletedAt);

        Assert.True(await _messages.TapUndoAsync()); // the second, latest offer
        Assert.False(await _messages.TapUndoAsync()); // a second tap
        Assert.Equal("Nothing to undo.", _messages.Last);
        Assert.Null((await _raw.GetAsync(second.Id))!.DeletedAt);
    }

    [Fact]
    public async Task Undo_does_not_restore_a_due_whose_deletion_changed()
    {
        var due = Rent();
        await _raw.AddAsync(due);
        var deletion = await DeleteThroughEditorAsync(due);
        var undo = NewUndo();
        undo.Offer(deletion);
        // Restored and deleted again elsewhere: a different deletion now.
        await _raw.RestoreAsync(due.Id, deletion.DeletedAt, _clock.Now.AddSeconds(1));
        await _raw.SoftDeleteAsync(due.Id, _clock.Now.AddSeconds(2));
        var tombstone = await _raw.GetAsync(due.Id);

        Assert.False(await _messages.TapUndoAsync());

        Assert.Equal("Nothing to undo.", _messages.Last);
        Assert.Equal(tombstone, await _raw.GetAsync(due.Id));
    }

    [Fact]
    public async Task A_failed_restore_keeps_the_due_deleted_and_says_so()
    {
        var due = Rent();
        await _raw.AddAsync(due);
        var deletion = await DeleteThroughEditorAsync(due);
        var undo = NewUndo(new FailingRepository(_raw));
        var raised = false;
        undo.Restored += (_, _) => raised = true;
        undo.Offer(deletion);

        Assert.False(await _messages.TapUndoAsync());

        Assert.Equal("Couldn't undo. The due stays deleted.", _messages.Last);
        Assert.Equal(deletion.DeletedAt, (await _raw.GetAsync(due.Id))!.DeletedAt);
        Assert.False(raised);
    }

    [Fact]
    public async Task Details_that_showed_a_deleted_due_load_it_again_after_undo()
    {
        var due = Rent();
        await _raw.AddAsync(due);
        var details = new DueDetailViewModel(_database, _repository, _clock);
        details.PrepareForDetails(due.Id.ToString("D"));
        var deletion = await DeleteThroughEditorAsync(due);
        await details.LoadCommand.ExecuteAsync(null);
        Assert.True(details.IsUnavailable);

        await _raw.RestoreAsync(due.Id, deletion.DeletedAt, _clock.Now);
        Assert.True(details.ShowsDue(due.Id));
        await details.ReloadAsync();

        Assert.False(details.IsUnavailable);
        Assert.Equal(due.Id, details.Item!.Id);
    }

    // ---- UX12: Manila timestamps on Details ----

    [Fact]
    public async Task Details_show_timestamps_in_manila_time_across_midnight()
    {
        // 16:30 UTC on 05 Nov is 00:30 on 06 Nov in Asia/Manila.
        var due = Rent() with { CreatedAt = new DateTimeOffset(2026, 11, 5, 16, 30, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 11, 6, 15, 59, 0, TimeSpan.Zero) };
        await _raw.AddAsync(due);
        var details = new DueDetailViewModel(_database, _repository, _clock);
        details.PrepareForDetails(due.Id.ToString("D"));

        await details.LoadCommand.ExecuteAsync(null);

        Assert.Equal("06 Nov 2026, 00:30 Manila", details.CreatedText);
        Assert.Equal("06 Nov 2026, 23:59 Manila", details.UpdatedText);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }
        public override DateTimeOffset GetUtcNow() => Now;
    }

    /// <summary>Records messages; "tapping" Undo runs the last Undo action, as the real host does once.</summary>
    private sealed class FakeMessages : IMessageHost
    {
        private Func<Task<bool>>? _undo;
        public string Last { get; private set; } = "";
        public bool LastHadUndo { get; private set; }

        public void Show(string message) => (Last, LastHadUndo) = (message, false);

        public void ShowUndo(string message, Func<Task<bool>> undo) => (Last, LastHadUndo, _undo) = (message, true, undo);

        public Task<bool> TapUndoAsync() => _undo?.Invoke() ?? Task.FromResult(false);
    }

    private sealed class FailingRepository(IDueRepository inner) : IDueRepository
    {
        public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
        public Task<Due?> GetAsync(Guid id) => inner.GetAsync(id);
        public Task AddAsync(Due due) => throw new IOException("Controlled storage failure");
        public Task UpdateAsync(Due due) => throw new IOException("Controlled storage failure");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new IOException("Controlled storage failure");
        public Task<bool> RestoreAsync(Guid id, DateTimeOffset deletedAt, DateTimeOffset now) => throw new IOException("Controlled storage failure");
    }

    private sealed class FakePlatform : IReminderPlatform
    {
        public int Requests { get; private set; }
        public NotificationAccess GetAccess() => new(false, "Blocked");
        public BackgroundDeliveryStatus GetBackgroundStatus() => new(false, false, false);
        public Task RequestPermissionAsync() => Task.CompletedTask;
        public void OpenSettings() { }
        public void OpenAppSettings() { }
        public void Schedule(DateTimeOffset? next) { }
        public void RequestReconciliation() => Requests++;
        public void CleanNotifications(IReadOnlySet<string> validKeys, bool keepTestNotification) { }
        public void CancelDue(Guid id) { }
        public void Post(ReminderOccurrence occurrence, ReminderText text) { }
        public void PostTest() { }
    }
}
