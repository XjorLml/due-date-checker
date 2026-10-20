using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class ReminderTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-reminders-{Guid.NewGuid():N}.db");
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    private readonly SqliteReminderStore _store;
    private readonly Platform _platform = new();
    private readonly Clock _clock = new();
    private readonly ReminderCoordinator _coordinator;
    private readonly ReminderAwareDueRepository _writes;
    private static ReminderSettings Enabled => new(true);
    public ReminderTests()
    {
        new DatabaseInitializer(_path).Initialize(); _database = new(new DatabaseInitializer(_path));
        _repository = new(_path); _store = new(_path);
        _coordinator = Coordinator(); _writes = new(_repository, _coordinator);
    }
    private ReminderCoordinator Coordinator() => new(_database, _repository, _store, _platform, _clock);
    private Due Item(int days = 1) => Due.Create("Private due title", Money.FromCentavos(12345), new DateOnly(2026, 10, 18).AddDays(days), _clock.Now);
    private async Task<Due> Queue(int days = 1)
    {
        var due = Item(days); await _writes.AddAsync(due); Assert.True(await _coordinator.SaveSettingsAsync(Enabled)); return due;
    }

    [Fact]
    public async Task Off_does_not_request_permission_or_queue_and_enable_schedules_two_occurrences()
    {
        await _writes.AddAsync(Item()); Assert.True(await _coordinator.ReconcileAsync());
        Assert.Empty(await _store.ReadOccurrencesAsync()); Assert.Null(_platform.Next); Assert.Equal(0, _platform.PermissionRequests);
        Assert.True(await _coordinator.SaveSettingsAsync(Enabled)); Assert.Equal(2, _coordinator.PendingCount);
        Assert.Equal(new DateTimeOffset(2026, 10, 18, 1, 0, 0, TimeSpan.Zero), _platform.Next);
        Assert.Empty(_platform.Posted); Assert.Equal(0, _platform.PermissionRequests);
    }

    [Fact]
    public async Task Delivered_receipts_survive_restart_repeated_reconcile_and_paid_unpaid_toggle()
    {
        var due = await Queue(); var key = (await _store.ReadOccurrencesAsync()).OrderBy(x => x.At).First().Key;
        Assert.False(await _coordinator.CanOpenAsync(due.Id, key)); // A queued key is not a notification receipt.
        _clock.Now = _clock.Now.AddHours(1);
        Assert.True(await _coordinator.ReconcileAsync()); Assert.Single(_platform.Posted);
        Assert.True(await _coordinator.CanOpenAsync(due.Id, key));
        Assert.True(await Coordinator().ReconcileAsync()); Assert.Single(_platform.Posted);
        await _writes.UpdateAsync(due with { IsPaid = true }); await _coordinator.ReconcileAsync();
        Assert.False(await _coordinator.CanOpenAsync(due.Id, key)); Assert.Empty(_platform.Visible);
        await _writes.UpdateAsync(due); await _coordinator.ReconcileAsync(); Assert.Single(_platform.Posted);
        Assert.Equal(1, _coordinator.PendingCount);
        Assert.False(await _coordinator.CanOpenAsync(Guid.NewGuid(), key));
        Assert.False(await _coordinator.CanOpenAsync(due.Id, key + "tampered"));
    }

    [Fact]
    public async Task New_elapsed_times_skip_but_previously_queued_can_deliver_late_until_due_date()
    {
        var queued = await Queue();
        _clock.Now = _clock.Now.AddHours(3);
        var fresh = Item(); await _writes.AddAsync(fresh); await _coordinator.ReconcileAsync();
        Assert.Equal(queued.Id, Assert.Single(_platform.Posted).DueId);
        Assert.DoesNotContain(await _store.ReadOccurrencesAsync(), x => x.DueId == fresh.Id && x.At < _clock.Now);
        _clock.Now = new(2026, 10, 20, 0, 0, 0, TimeSpan.Zero);
        await _coordinator.ReconcileAsync(); Assert.Single(_platform.Posted); Assert.Null(_platform.Next);
        Assert.False(await _coordinator.CanOpenAsync(queued.Id, _platform.Posted[0].Key));
    }

    [Fact]
    public async Task Late_queued_reminders_wait_out_quiet_hours_without_delivering_after_due_date()
    {
        await Queue(); _clock.Now = new(2026, 10, 18, 15, 0, 0, TimeSpan.Zero); // 23:00 Manila.
        await _coordinator.ReconcileAsync(); Assert.Empty(_platform.Posted);
        Assert.Equal(new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero), _platform.Next);
        _clock.Now = _platform.Next!.Value; await _coordinator.ReconcileAsync(); Assert.Single(_platform.Posted);
        _clock.Now = new(2026, 10, 19, 15, 0, 0, TimeSpan.Zero);
        await _coordinator.ReconcileAsync(); Assert.Single(_platform.Posted); Assert.Null(_platform.Next);
    }

    [Fact]
    public async Task Date_edit_payment_delete_and_disable_replace_pending_and_remove_visible_notifications()
    {
        var due = await Queue(); var old = (await _store.ReadOccurrencesAsync()).Select(x => x.Key).ToHashSet();
        due = due with { DueDate = due.DueDate.AddDays(2) }; await _writes.UpdateAsync(due); await _coordinator.ReconcileAsync();
        Assert.DoesNotContain(await _store.ReadOccurrencesAsync(), x => old.Contains(x.Key));
        await _writes.UpdateAsync(due with { IsPaid = true }); await _coordinator.ReconcileAsync(); Assert.Empty(await _store.ReadOccurrencesAsync());
        await _writes.UpdateAsync(due); await _coordinator.ReconcileAsync(); Assert.Equal(2, _coordinator.PendingCount);
        await _writes.SoftDeleteAsync(due.Id, _clock.Now); await _coordinator.ReconcileAsync(); Assert.Empty(await _store.ReadOccurrencesAsync());
        await _writes.AddAsync(Item()); await _coordinator.ReconcileAsync();
        _clock.Now = _clock.Now.AddHours(1); await _coordinator.ReconcileAsync(); Assert.Single(_platform.Visible);
        var receipt = Assert.Single(_platform.Posted);
        await _coordinator.SaveSettingsAsync(Enabled with { Enabled = false });
        Assert.Null(_platform.Next); Assert.Empty(_platform.Visible); Assert.Equal(0, _coordinator.PendingCount);
        Assert.False(await _coordinator.CanOpenAsync(receipt.DueId, receipt.Key));
        Assert.Single(await _store.ReadOccurrencesAsync()); // Keep delivered journal, clear pending.
    }

    [Fact]
    public async Task Revoked_channel_clears_queue_and_reallow_only_schedules_future_times()
    {
        await Queue(); _platform.Allowed = false; await _coordinator.ReconcileAsync();
        Assert.Empty(await _store.ReadOccurrencesAsync()); Assert.Null(_platform.Next);
        Assert.True((await _store.ReadSettingsAsync()).Enabled);
        _clock.Now = _clock.Now.AddHours(2); _platform.Allowed = true; await _coordinator.ReconcileAsync();
        Assert.Empty(_platform.Posted); Assert.Equal(1, _coordinator.PendingCount);
    }

    [Fact]
    public async Task Failed_scheduler_or_post_is_retryable_and_never_rolls_back_settings_or_dues()
    {
        _platform.FailSchedule = true; var due = Item(); await _writes.AddAsync(due);
        Assert.False(await _coordinator.SaveSettingsAsync(Enabled)); Assert.True((await _store.ReadSettingsAsync()).Enabled);
        Assert.Equal(due, await _repository.GetAsync(due.Id)); Assert.NotEmpty(_coordinator.LastError);
        _platform.FailSchedule = false; Assert.True(await _coordinator.ReconcileAsync());
        _clock.Now = _clock.Now.AddHours(1); _platform.FailPost = true;
        Assert.False(await _coordinator.ReconcileAsync()); Assert.Empty(_platform.Posted);
        Assert.All(await _store.ReadOccurrencesAsync(), x => Assert.Null(x.DeliveredAt));
        Assert.Equal(_clock.Now.AddMinutes(5), _platform.Next);
        _platform.FailPost = false; Assert.True(await _coordinator.ReconcileAsync()); Assert.Single(_platform.Posted);
        Assert.Equal("", _coordinator.LastError);
    }

    [Fact]
    public async Task Write_side_effect_failure_is_success_and_failed_write_has_no_side_effect()
    {
        _platform.FailEnqueue = true; var due = Item();
        await _database.RunAsync(() => _writes.AddAsync(due)); // Also proves no dispatcher re-entry deadlock.
        Assert.Equal(due, await _repository.GetAsync(due.Id)); Assert.Contains("saved", _coordinator.LastError);
        _platform.FailEnqueue = false; var before = _platform.Requests;
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _writes.UpdateAsync(Item()));
        Assert.Equal(before, _platform.Requests);
    }

    [Fact]
    public async Task Large_due_batch_posts_ten_per_pass_and_schedules_remaining_without_past_alarm_spin()
    {
        for (var i = 0; i < 12; i++) await _writes.AddAsync(Item());
        await _coordinator.SaveSettingsAsync(Enabled); _clock.Now = _clock.Now.AddHours(1);
        await _coordinator.ReconcileAsync(); Assert.Equal(10, _platform.Posted.Count);
        Assert.Equal(_clock.Now.AddMinutes(1), _platform.Next);
        _clock.Now = _platform.Next!.Value; await _coordinator.ReconcileAsync(); Assert.Equal(12, _platform.Posted.Count);
        Assert.Equal(12, _platform.Posted.Select(x => x.Key).Distinct().Count());
        Assert.True(_platform.Next > _clock.Now);
    }

    [Fact]
    public async Task Settings_page_load_save_and_enable_have_explicit_permission_and_draft_boundaries()
    {
        var vm = new ReminderSettingsViewModel(_coordinator, _platform); await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.Enabled); Assert.Equal(0, _platform.PermissionRequests);
        vm.DayBefore = false; vm.ReminderTime = TimeSpan.FromHours(10);
        Assert.Equal(new(), await _store.ReadSettingsAsync());
        await vm.LoadCommand.ExecuteAsync(null); Assert.False(vm.DayBefore); // Refresh retains draft.
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.Equal(new(false, false, 600), await _store.ReadSettingsAsync()); Assert.Equal(0, _platform.PermissionRequests);
        _platform.Allowed = false; _platform.GrantOnRequest = false;
        await vm.EnableCommand.ExecuteAsync(null); Assert.False(vm.Enabled); Assert.Equal(1, _platform.PermissionRequests);
        Assert.False((await _store.ReadSettingsAsync()).Enabled);
        _platform.GrantOnRequest = true; await vm.EnableCommand.ExecuteAsync(null);
        Assert.True(vm.Enabled); Assert.True((await _store.ReadSettingsAsync()).Enabled);
        vm.ReminderTime = TimeSpan.FromHours(11); await vm.DisableCommand.ExecuteAsync(null);
        Assert.Equal(TimeSpan.FromHours(11), vm.ReminderTime); Assert.Equal(600, (await _store.ReadSettingsAsync()).MinuteOfDay);
        Assert.False(vm.Enabled); Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Settings_invalid_time_and_test_notification_do_not_mutate_dues_or_enable_implicitly()
    {
        var vm = new ReminderSettingsViewModel(_coordinator, _platform);
        await vm.EnableCommand.ExecuteAsync(null); Assert.Equal(0, _platform.PermissionRequests); // Not loaded.
        await vm.LoadCommand.ExecuteAsync(null); vm.TestCommand.Execute(null); Assert.Equal(0, _platform.Tests);
        vm.ReminderTime = TimeSpan.FromDays(1); await vm.EnableCommand.ExecuteAsync(null);
        Assert.Equal(0, _platform.PermissionRequests); Assert.Equal(new(), await _store.ReadSettingsAsync());
        vm.ReminderTime = TimeSpan.FromHours(9); await vm.EnableCommand.ExecuteAsync(null);
        vm.TestCommand.Execute(null); Assert.Equal(1, _platform.Tests); Assert.Empty(await _repository.ListActiveAsync());
        _platform.Allowed = false; vm.TestCommand.Execute(null); Assert.Equal(1, _platform.Tests);
    }

    [Fact]
    public async Task Saved_settings_are_reported_as_saved_even_if_platform_status_query_fails()
    {
        var vm = new ReminderSettingsViewModel(_coordinator, _platform); await vm.LoadCommand.ExecuteAsync(null);
        _platform.FailAccess = true; vm.DayBefore = false; await vm.SaveCommand.ExecuteAsync(null);
        Assert.False((await _store.ReadSettingsAsync()).DayBefore);
        Assert.Contains("saved", vm.Message, StringComparison.OrdinalIgnoreCase); Assert.Contains("unavailable", vm.StatusText);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Clock_change_during_repository_read_cannot_deliver_an_expired_queued_due()
    {
        var due = await Queue(0);
        _clock.Now = _clock.Now.AddHours(2);
        var delayed = new ClockChangingRepository(_repository, () => _clock.Now = _clock.Now.AddDays(1));
        var coordinator = new ReminderCoordinator(_database, delayed, _store, _platform, _clock);
        Assert.True(await coordinator.ReconcileAsync());
        Assert.Empty(_platform.Posted);
        Assert.DoesNotContain(await _store.ReadOccurrencesAsync(), x => x.DueId == due.Id);
        Assert.Null(_platform.Next);
    }

    [Fact]
    public async Task Batch_crossing_quiet_start_rechecks_clock_before_each_notification()
    {
        await Queue(); await _writes.AddAsync(Item()); await _coordinator.ReconcileAsync();
        _clock.Now = _clock.Now.AddHours(2);
        _platform.AfterPost = () => _clock.Now = new(2026, 10, 18, 14, 0, 0, TimeSpan.Zero);
        Assert.True(await _coordinator.ReconcileAsync());
        Assert.Single(_platform.Posted);
        Assert.Equal(new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero), _platform.Next);
    }

    [Theory]
    [InlineData(true, false, true, "restricts background activity")]
    [InlineData(false, true, true, "Battery Saver is on")]
    [InlineData(false, false, true, "optimization is active")]
    [InlineData(false, false, false, "Other device restrictions may still delay")]
    public async Task Background_status_and_settings_action_do_not_enable_or_save_drafts(bool restricted, bool saver, bool optimized, string expected)
    {
        _platform.Background = new(restricted, saver, optimized);
        var vm = new ReminderSettingsViewModel(_coordinator, _platform); await vm.LoadCommand.ExecuteAsync(null);
        Assert.Contains(expected, vm.BackgroundText); Assert.Equal("Allowed", vm.StatusText);
        vm.DayBefore = false; vm.OpenAppSettingsCommand.Execute(null);
        Assert.Equal(1, _platform.AppSettingsOpened); Assert.Equal(0, _platform.PermissionRequests);
        Assert.Equal(new(), await _store.ReadSettingsAsync());
        _platform.Background = new(false, false, false); await vm.LoadCommand.ExecuteAsync(null);
        Assert.Contains("optimization is off", vm.BackgroundText); Assert.False(vm.DayBefore); Assert.False(vm.Enabled);
    }

    [Fact]
    public async Task Unavailable_background_status_does_not_misreport_saved_settings_or_lose_draft()
    {
        var vm = new ReminderSettingsViewModel(_coordinator, _platform); await vm.LoadCommand.ExecuteAsync(null);
        vm.DayBefore = false; _platform.FailBackground = true; await vm.SaveCommand.ExecuteAsync(null);
        Assert.False((await _store.ReadSettingsAsync()).DayBefore); Assert.Contains("saved", vm.Message);
        Assert.Contains("unavailable", vm.BackgroundText); Assert.False(vm.IsBusy);
        _platform.FailAppSettings = true; vm.OpenAppSettingsCommand.Execute(null);
        Assert.Contains("Couldn't open Android app settings", vm.Message);
    }

    [Theory]
    [InlineData(true, true, true, "True")]
    [InlineData(true, false, true, "True,False")]
    [InlineData(false, false, true, "False")]
    [InlineData(true, false, false, "True,False")]
    [InlineData(false, false, false, "False")]
    public void Immediate_dispatch_uses_supported_expedited_work_and_falls_back_on_quota_rejection(bool supported, bool expeditedAccepted, bool normalAccepted, string expected)
    {
        var attempts = new List<bool>();
        void Run() => ReminderJobDispatch.Enqueue(supported, expedited => { attempts.Add(expedited); return expedited ? expeditedAccepted : normalAccepted; });
        if ((supported && expeditedAccepted) || normalAccepted) Run();
        else Assert.Throws<InvalidOperationException>(Run);
        Assert.Equal(expected, string.Join(",", attempts));
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class ClockChangingRepository(IDueRepository inner, Action change) : IDueRepository
    {
        public async Task<IReadOnlyList<Due>> ListActiveAsync() { var rows = await inner.ListActiveAsync(); change(); return rows; }
        public Task<Due?> GetAsync(Guid id) => inner.GetAsync(id);
        public Task AddAsync(Due due) => inner.AddAsync(due);
        public Task UpdateAsync(Due due) => inner.UpdateAsync(due);
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => inner.SoftDeleteAsync(id, when);
    }
    private sealed class Platform : IReminderPlatform
    {
        public bool Allowed = true, GrantOnRequest = true, FailSchedule, FailPost, FailEnqueue, FailAccess, FailBackground, FailAppSettings;
        public int AppSettingsOpened;
        public BackgroundDeliveryStatus Background = new(false, false, true);
        public Action? AfterPost;
        public int PermissionRequests, Requests, Tests;
        public DateTimeOffset? Next;
        public List<ReminderOccurrence> Posted { get; } = [];
        public HashSet<string> Visible { get; } = [];
        public NotificationAccess GetAccess() => FailAccess ? throw new IOException("Controlled access failure") : new(Allowed, Allowed ? "Allowed" : "Blocked");
        public BackgroundDeliveryStatus GetBackgroundStatus() => FailBackground ? throw new IOException("Controlled background status failure") : Background;
        public Task RequestPermissionAsync() { PermissionRequests++; Allowed = GrantOnRequest; return Task.CompletedTask; }
        public void OpenSettings() { }
        public void OpenAppSettings() { if (FailAppSettings) throw new IOException("Controlled settings failure"); AppSettingsOpened++; }
        public void Schedule(DateTimeOffset? at) { if (FailSchedule) throw new IOException("Controlled schedule failure"); Next = at; }
        public void RequestReconciliation() { Requests++; if (FailEnqueue) throw new IOException("Controlled enqueue failure"); }
        public void CleanNotifications(IReadOnlySet<string> validKeys) => Visible.RemoveWhere(key => !validKeys.Contains(key));
        public void CancelDue(Guid id) => Visible.RemoveWhere(key => key.StartsWith(id.ToString("N") + "/", StringComparison.Ordinal));
        public void Post(ReminderOccurrence item) { if (FailPost) throw new IOException("Controlled post failure"); Posted.Add(item); Visible.Add(item.Key); AfterPost?.Invoke(); }
        public void PostTest() => Tests++;
    }
    public void Dispose() { SqliteConnection.ClearAllPools(); File.Delete(_path); }
}
