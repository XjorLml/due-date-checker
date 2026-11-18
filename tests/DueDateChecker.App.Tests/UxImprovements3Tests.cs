using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX review decisions of 2026-11-16 in the app (specs/ux-improvements-3: AL, OD, PD and AV scenarios).</summary>
public sealed class UxImprovements3Tests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-ux3-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly SqliteReminderStore _store;
    private readonly DatabaseDispatcher _database;
    private readonly Clock _clock = new() { Now = new(2026, 11, 16, 0, 0, 0, TimeSpan.Zero) }; // 08:00 Manila
    private readonly Platform _platform = new();
    private readonly ReminderCoordinator _coordinator;

    public UxImprovements3Tests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _store = new SqliteReminderStore(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
        _coordinator = new ReminderCoordinator(_database, _repository, _store, _platform, _clock);
    }

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    private static Due Bill(DateOnly date, long? centavos = 85000, bool varies = false) =>
        Due.Create("Water bill", centavos is { } c ? Money.FromCentavos(c) : null, date, Created, null, DueCategory.Bill,
            RepeatRule.Monthly, "ph.utilities.water", amountVaries: varies);

    // ---- OD01–OD04: overdue reminders through the coordinator and the payment action ----

    [Fact]
    public async Task An_overdue_reminder_is_delivered_on_its_day_and_can_mark_the_due_paid()
    {
        var due = Bill(new DateOnly(2026, 11, 15)); // due yesterday
        await _repository.AddAsync(due);
        Assert.True(await _coordinator.SaveSettingsAsync(new ReminderSettings(Enabled: true)));
        Assert.Equal(new DateTimeOffset(2026, 11, 16, 1, 0, 0, TimeSpan.Zero), _platform.Next); // 09:00 Manila, 1 day late

        _clock.Now = _clock.Now.AddHours(1);
        Assert.True(await _coordinator.ReconcileAsync());
        var (item, text) = Assert.Single(_platform.Posted);
        Assert.Equal(("Water bill is 1 day late", ReminderKind.Overdue), (text.Title, text.Kind));

        var payments = new ReminderPaymentService(_database, _repository, _store, _clock);
        var result = await payments.ExecuteAsync(new ReminderActionToken(due.Id, item.Key, ReminderText.Fingerprint(due)), false, () => true);
        Assert.Equal(ReminderActionStatus.Saved, result.Status);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
        Assert.Equal(new DateOnly(2026, 12, 15), result.Next!.DueDate);
    }

    [Fact]
    public async Task Turning_overdue_off_removes_pending_overdue_reminders_and_an_old_one_expires()
    {
        await _repository.AddAsync(Bill(new DateOnly(2026, 11, 10)));
        Assert.True(await _coordinator.SaveSettingsAsync(new ReminderSettings(Enabled: true)));
        Assert.Equal(4, _coordinator.PendingCount); // 7, 14, 21 and 28 days late; 1 day late has passed
        Assert.True(await _coordinator.SaveSettingsAsync(new ReminderSettings(Enabled: true, Overdue: false)));
        Assert.Equal(0, _coordinator.PendingCount);

        // A due 29 days late has no reminder left to plan.
        await _repository.AddAsync(Bill(new DateOnly(2026, 10, 18)));
        Assert.True(await _coordinator.SaveSettingsAsync(new ReminderSettings(Enabled: true)));
        Assert.Equal(4, _coordinator.PendingCount);
    }

    // ---- AL02–AL04 and OD05: Reminders page ----

    [Fact]
    public async Task Reminders_page_shows_alert_styles_says_the_channel_change_once_and_saves_overdue()
    {
        _platform.ChannelNotice = true;
        var vm = new ReminderSettingsViewModel(_coordinator, _platform);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(("Banner · Chime", "Sound, no banner · Chime"), (vm.DueTodayStyle, vm.ComingUpStyle));
        Assert.Contains("two Android channels, Due today and Coming up", vm.Message);
        Assert.True(vm.Overdue);

        await vm.LoadCommand.ExecuteAsync(null);
        Assert.DoesNotContain("two Android channels", vm.Message);

        vm.OpenDueTodaySoundCommand.Execute(null);
        vm.OpenComingUpSoundCommand.Execute(null);
        Assert.Equal(new[] { ReminderChannel.DueToday, ReminderChannel.ComingUp }, _platform.Opened);

        _platform.Styles[ReminderChannel.DueToday] = "Silent";
        vm.RefreshStatus();
        Assert.Equal("Silent", vm.DueTodayStyle);

        vm.Overdue = false;
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.False((await _store.ReadSettingsAsync()).Overdue);
    }

    // ---- PD01–PD04: paydays ----

    [Theory]
    [InlineData("1|dark|sunday", 15, 30)]
    [InlineData("2|light|monday|10|25", 10, 25)]
    public void Preferences_read_both_formats(string stored, int first, int second)
    {
        var value = AppPreferences.Decode(stored);
        Assert.Equal((first, second), (value.PaydayFirst, value.PaydaySecond));
    }

    [Theory]
    [InlineData("2|light|monday|25|10")]
    [InlineData("2|light|monday|15|15")]
    [InlineData("2|light|monday|0|30")]
    [InlineData("2|light|monday|15|32")]
    [InlineData("2|light|monday|15")]
    [InlineData("2|light|monday|+1|30")]
    public void Preferences_refuse_invalid_paydays(string stored)
    {
        Assert.Throws<FormatException>(() => AppPreferences.Decode(stored));
    }

    [Fact]
    public void Settings_saves_paydays_in_order_and_refuses_equal_days()
    {
        var storage = new MemoryPreferencesStorage();
        var service = new AppPreferencesService(storage);
        var vm = new SettingsViewModel(service);
        vm.LoadCommand.Execute(null);
        Assert.Equal(((int?)15, (int?)30), (vm.FirstPayday, vm.SecondPayday));

        vm.SecondPayday = 15;
        vm.SaveCommand.Execute(null);
        Assert.Equal("Choose two different paydays.", vm.Message);
        Assert.Null(storage.Value);

        vm.FirstPayday = 25; vm.SecondPayday = 10;
        vm.SaveCommand.Execute(null);
        Assert.Equal("2|system|monday|10|25", storage.Value);
        Assert.Equal((10, 25), (service.Current.PaydayFirst, service.Current.PaydaySecond));
    }

    [Fact]
    public async Task Dashboard_totals_before_the_next_payday_and_follows_a_payday_change()
    {
        _clock.Now = new DateTimeOffset(2026, 11, 16, 1, 0, 0, TimeSpan.Zero); // Mon 16 Nov, Manila
        foreach (var (title, day, centavos) in new[] { ("Water", 12, 85000L), ("Rent", 16, 625000L), ("Meralco", 20, 245050L), ("SSS", 30, 57000L) })
            await _repository.AddAsync(Due.Create(title, Money.FromCentavos(centavos), new DateOnly(2026, 11, day), Created, null, DueCategory.Bill));
        var preferences = new AppPreferencesService(new MemoryPreferencesStorage());
        var vm = new DashboardViewModel(_database, _repository, _clock, preferences);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal(new DateOnly(2026, 11, 30), vm.NextPayday);
        Assert.Equal("Before payday · 30 Nov", vm.PaydayLabel);
        Assert.Equal("₱9,550.50", vm.PaydayTotalText); // water (overdue), rent and Meralco; SSS is paid from the 30th
        Assert.Equal("3 unpaid · ₱850.00 overdue", vm.PaydayMetaText);

        Assert.True(preferences.Save(new AppPreferences(PaydayFirst: 10, PaydaySecond: 25)));
        Assert.Equal("Before payday · 25 Nov", vm.PaydayLabel);
        Assert.Equal("₱9,550.50", vm.PaydayTotalText);
        Assert.True(preferences.Save(new AppPreferences(PaydayFirst: 5, PaydaySecond: 18)));
        Assert.Equal("Before payday · 18 Nov", vm.PaydayLabel);
        Assert.Equal("₱7,100.00", vm.PaydayTotalText);
    }

    // ---- AV01–AV04: amounts that change each month ----

    [Fact]
    public async Task Editor_lets_a_repeating_bill_wait_for_its_amount()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "Meralco", SelectedDate = new DateTime(2026, 11, 20),
        };
        Assert.False(editor.CanVaryAmount); // Never repeats
        editor.SelectedRepeat = RepeatRule.Monthly;
        Assert.True(editor.CanVaryAmount);
        Assert.True(editor.IsAmountRequired);
        editor.AmountVaries = true;
        Assert.False(editor.IsAmountRequired);
        Assert.Equal("Optional: fill it in when the bill arrives.", editor.AmountRequirementText);
        Assert.Equal("Will save as “Amount needed” until you add it.", editor.AmountPreviewLine);

        editor.SelectedRepeat = RepeatRule.Never;
        Assert.False(editor.AmountVaries);
        Assert.True(editor.IsAmountRequired);
        editor.SelectedRepeat = RepeatRule.Monthly;
        editor.AmountVaries = true;

        await editor.SaveCommand.ExecuteAsync(null);
        var saved = Assert.Single(await _repository.ListActiveAsync());
        Assert.Equal(((Money?)null, true, RepeatRule.Monthly), (saved.Amount, saved.AmountVaries, saved.Repeat));
        Assert.Equal("Amount needed", DueListItem.Create(saved, new DateOnly(2026, 11, 16)).AmountText);

        var reopened = new DueEditViewModel(_database, _repository, _clock);
        reopened.PrepareForEdit(saved.Id.ToString());
        await reopened.LoadCommand.ExecuteAsync(null);
        Assert.True(reopened.AmountVaries);
        Assert.False(reopened.HasChanges);
        reopened.SelectedCategory = DueCategory.Task;
        Assert.False(reopened.AmountVaries);
    }

    [Fact]
    public async Task Paying_a_varying_bill_on_details_adds_the_next_one_without_an_amount()
    {
        var due = Bill(new DateOnly(2026, 11, 20), centavos: 245050, varies: true);
        await _repository.AddAsync(due);
        var details = new DueDetailViewModel(_database, _repository, _clock);
        details.PrepareForDetails(due.Id.ToString("D"));
        await details.LoadCommand.ExecuteAsync(null);

        await details.TogglePaidCommand.ExecuteAsync(null);

        var next = (await _repository.ListActiveAsync()).Single(row => row.Id != due.Id);
        Assert.Equal((new DateOnly(2026, 12, 20), (Money?)null, true), (next.DueDate, next.Amount, next.AmountVaries));
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Platform : IReminderPlatform
    {
        public bool ChannelNotice;
        public List<(ReminderOccurrence Item, ReminderText Text)> Posted { get; } = [];
        public List<ReminderChannel> Opened { get; } = [];
        public Dictionary<ReminderChannel, string> Styles { get; } = new()
        {
            [ReminderChannel.DueToday] = "Banner · Chime",
            [ReminderChannel.ComingUp] = "Sound, no banner · Chime",
        };
        public DateTimeOffset? Next;
        public NotificationAccess GetAccess() => new(true, "Allowed");
        public BackgroundDeliveryStatus GetBackgroundStatus() => new(false, false, false);
        public Task RequestPermissionAsync() => Task.CompletedTask;
        public void OpenSettings() { }
        public void OpenAppSettings() { }
        public void Schedule(DateTimeOffset? next) => Next = next;
        public void RequestReconciliation() { }
        public void CleanNotifications(IReadOnlySet<string> validKeys, bool keepTestNotification) { }
        public void CancelDue(Guid id) { }
        public void Post(ReminderOccurrence occurrence, ReminderText text) => Posted.Add((occurrence, text));
        public void PostTest() { }
        public string GetAlertStyle(ReminderChannel channel) => Styles[channel];
        public void OpenChannelSettings(ReminderChannel channel) => Opened.Add(channel);
        public bool TakeChannelNotice() { var value = ChannelNotice; ChannelNotice = false; return value; }
    }
}
