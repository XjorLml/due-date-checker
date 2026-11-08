using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

public sealed class ReminderPaymentTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"notification-pay-{Guid.NewGuid():N}.db");
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    private readonly SqliteReminderStore _store;
    private readonly Clock _clock = new();
    private readonly Platform _platform = new();
    private readonly ReminderCoordinator _coordinator;
    private readonly ReminderPaymentService _payments;
    private readonly ReminderAwareDueRepository _writes;
    public ReminderPaymentTests()
    {
        _database = new(new DatabaseInitializer(_path)); _repository = new(_path); _store = new(_path);
        _coordinator = new(_database, _repository, _store, _platform, _clock);
        _writes = new(_repository, _coordinator); _payments = new(_database, _writes, _store, _clock);
    }
    private async Task<(Due Due, ReminderActionToken Token)> Delivered(bool repeats = false, bool noAmount = false)
    {
        var due = Due.Create("Sample due", noAmount ? null : Money.FromCentavos(12345), new(2026, 11, 8), _clock.Now,
            "PRIVATE", noAmount ? DueCategory.Task : DueCategory.Bill, repeats ? RepeatRule.Monthly : RepeatRule.Never);
        await _database.RunAsync(() => _writes.AddAsync(due));
        await _coordinator.SaveSettingsAsync(new(true));
        _clock.Now = _clock.Now.AddHours(1);
        Assert.True(await _coordinator.ReconcileAsync());
        var receipt = Assert.Single(await _store.ReadOccurrencesAsync(), x => x.DeliveredAt.HasValue);
        return (due, new(due.Id, receipt.Key, ReminderText.Fingerprint(due)));
    }
    private Task<ReminderActionResult> Pay(ReminderActionToken token) => _payments.ExecuteAsync(token, false, () => true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Pays_with_or_without_amount_and_uses_reminder_aware_write(bool noAmount)
    {
        var (due, token) = await Delivered(noAmount: noAmount);
        Guid? changed = null; _payments.Changed += (_, id) => changed = id;
        var result = await Pay(token);
        Assert.Equal(ReminderActionStatus.Saved, result.Status);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
        Assert.Equal(due.Id, changed); Assert.Equal(due.Id, _platform.Cancelled);
        Assert.True(_platform.Requests > 0); Assert.NotNull(result.Undo);
        await _coordinator.ReconcileAsync(); Assert.Empty(_platform.Visible);
        Assert.False(await _coordinator.CanOpenAsync(due.Id, token.Key));
        Assert.True(await _coordinator.CanOpenAsync(due.Id, token.Key, result.Undo.Version));
    }

    [Fact]
    public async Task Concurrent_duplicate_payments_create_exactly_one_next_and_undo_retains_it()
    {
        var (due, token) = await Delivered(repeats: true);
        var results = await Task.WhenAll(Pay(token), Pay(token));
        var result = Assert.Single(results, x => x.Status == ReminderActionStatus.Saved);
        Assert.Single(results, x => x.Status == ReminderActionStatus.Stale);
        Assert.Equal(new DateOnly(2026, 12, 8), result.Next!.DueDate);
        Assert.Equal(2, (await _repository.ListActiveAsync()).Count);
        _clock.Now = _clock.Now.AddSeconds(9);
        var undone = await _payments.ExecuteAsync(result.Undo!, true, () => true);
        Assert.Equal(ReminderActionStatus.Saved, undone.Status);
        Assert.False((await _repository.GetAsync(due.Id))!.IsPaid);
        Assert.Equal(2, (await _repository.ListActiveAsync()).Count);
        Assert.Equal(ReminderActionStatus.Stale, (await _payments.ExecuteAsync(result.Undo!, true, () => true)).Status);
        // Existing recurrence duplicate guard also survives a later Details payment.
        var current = (await _repository.GetAsync(due.Id))!;
        var paid = current with { IsPaid = true };
        Assert.False(await _writes.UpdateWithNextAsync(paid, Recurrence.NextOnPayment(current, paid, _clock.Now)!));
        await _coordinator.ReconcileAsync();
        Assert.Equal(2, (await _repository.ListActiveAsync()).Count);
        Assert.Equal(2, _coordinator.PendingCount);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("notes")]
    [InlineData("amount")]
    [InlineData("date")]
    [InlineData("paid")]
    [InlineData("deleted")]
    [InlineData("disabled")]
    [InlineData("settings")]
    [InlineData("overdue")]
    [InlineData("wrong-key")]
    [InlineData("wrong-version")]
    public async Task Rejects_obsolete_or_edited_reminders_without_writes(string change)
    {
        var (due, token) = await Delivered();
        var altered = change switch
        {
            "title" => due with { Title = "Edited" }, "notes" => due with { Notes = "Edited" },
            "amount" => due with { Amount = Money.FromCentavos(0) }, "date" => due with { DueDate = due.DueDate.AddDays(1) },
            "paid" => due with { IsPaid = true }, _ => due
        };
        if (altered != due) await _database.RunAsync(() => _writes.UpdateAsync(altered));
        if (change == "deleted") await _database.RunAsync(() => _writes.SoftDeleteAsync(due.Id, _clock.Now));
        if (change == "disabled") await _coordinator.SaveSettingsAsync(new(false));
        if (change == "settings") await _coordinator.SaveSettingsAsync(new(true, MinuteOfDay: 600));
        if (change == "overdue") _clock.Now = _clock.Now.AddDays(1);
        if (change == "wrong-key") token = token with { Key = token.Key + "invalid" };
        if (change == "wrong-version") token = token with { Version = new string('0', 64) };
        var before = await _repository.GetAsync(due.Id);
        Assert.Equal(ReminderActionStatus.Stale, (await Pay(token)).Status);
        Assert.Equal(before, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Undelivered_occurrence_and_forged_paid_confirmation_cannot_be_used()
    {
        var (due, token) = await Delivered();
        // Tomorrow creates a future day-of occurrence while the day-before receipt is delivered.
        var future = Due.Create("Future", null, due.DueDate.AddDays(1), _clock.Now);
        await _writes.AddAsync(future); await _coordinator.ReconcileAsync();
        var queued = (await _store.ReadOccurrencesAsync()).First(x => x.DueId == future.Id);
        Assert.Equal(ReminderActionStatus.Stale, (await Pay(new(future.Id, queued.Key, ReminderText.Fingerprint(future)))).Status);
        Assert.False(await _coordinator.CanOpenAsync(due.Id, token.Key, new string('0', 64)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Locked_before_or_after_reads_never_writes(bool relock)
    {
        var (due, token) = await Delivered(); var calls = 0;
        var result = await _payments.ExecuteAsync(token, false, () => relock && ++calls == 1);
        Assert.Equal(ReminderActionStatus.Locked, result.Status);
        Assert.False((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("edited")]
    [InlineData("deleted")]
    [InlineData("unpaid")]
    [InlineData("clock-back")]
    [InlineData("locked")]
    public async Task Undo_guards_all_record_and_time_changes(string change)
    {
        var (due, token) = await Delivered(); var result = await Pay(token);
        var paid = (await _repository.GetAsync(due.Id))!;
        if (change == "expired") _clock.Now = _clock.Now.AddSeconds(10);
        if (change == "clock-back") _clock.Now = _clock.Now.AddSeconds(-1);
        if (change == "edited") await _writes.UpdateAsync(paid with { Notes = "changed at same stamp" });
        if (change == "deleted") await _writes.SoftDeleteAsync(due.Id, _clock.Now);
        if (change == "unpaid") await _writes.UpdateAsync(paid with { IsPaid = false });
        var before = await _repository.GetAsync(due.Id);
        var undone = await _payments.ExecuteAsync(result.Undo!, true, () => change != "locked");
        Assert.Equal(change == "locked" ? ReminderActionStatus.Locked : ReminderActionStatus.Stale, undone.Status);
        Assert.Equal(before, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Undo_has_a_full_window_after_delayed_native_confirmation()
    {
        var (due, token) = await Delivered(); var paid = await Pay(token);
        _clock.Now = _clock.Now.AddSeconds(20); // Slow reconciliation before the card can be posted.
        var visibleToken = paid.Undo!.WithUndoWindow(_clock.Now);
        _clock.Now = _clock.Now.AddSeconds(9);
        Assert.Equal(ReminderActionStatus.Saved, (await _payments.ExecuteAsync(visibleToken, true, () => true)).Status);
        Assert.False((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    [Fact]
    public async Task Open_editor_cannot_overwrite_notification_payment_and_keeps_draft()
    {
        var (due, token) = await Delivered();
        var editor = new DueEditViewModel(_database, _writes, _clock);
        editor.PrepareForEdit(due.Id.ToString()); await editor.LoadCommand.ExecuteAsync(null);
        editor.Title = "Unsaved draft";
        await Pay(token); await editor.SaveCommand.ExecuteAsync(null);
        Assert.Null(editor.SavedDueId); Assert.Contains("changed while", editor.ErrorMessage);
        Assert.Equal("Unsaved draft", editor.Title); Assert.True(editor.HasChanges);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    public void Dispose() { TestDatabase.ClearPool(_path); File.Delete(_path); }
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = new(2026, 11, 8, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class Platform : IReminderPlatform
    {
        public Guid? Cancelled; public int Requests; public HashSet<string> Visible = [];
        public NotificationAccess GetAccess() => new(true, "Allowed");
        public BackgroundDeliveryStatus GetBackgroundStatus() => new(false, false, false);
        public Task RequestPermissionAsync() => Task.CompletedTask;
        public void OpenSettings() { } public void OpenAppSettings() { } public void Schedule(DateTimeOffset? next) { }
        public void RequestReconciliation() => Requests++;
        public void CancelDue(Guid id) { Cancelled = id; Visible.RemoveWhere(x => x.StartsWith(id.ToString("N"))); }
        public void CleanNotifications(IReadOnlySet<string> keys, bool keepTestNotification) => Visible.RemoveWhere(x => !keys.Contains(x));
        public void Post(ReminderOccurrence occurrence, ReminderText text) => Visible.Add(occurrence.Key);
        public void PostTest() { }
    }
}
