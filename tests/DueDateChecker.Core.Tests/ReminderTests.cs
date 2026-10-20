using System.Globalization;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

public sealed class ReminderTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"reminders-{Guid.NewGuid():N}.db");
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
    private static Due Item(DateOnly? date = null) => Due.Create("Test due", Money.FromCentavos(0), date ?? new(2026, 10, 20), Now);
    private static ReminderSettings Enabled => new(Enabled: true);

    [Fact]
    public void Default_off_and_opt_in_two_dates_use_Manila_nine_am()
    {
        var due = Item();
        Assert.Empty(ReminderPlanner.Plan([due], new(), Zone));
        var plans = ReminderPlanner.Plan([due], Enabled, Zone);
        Assert.Equal(new[] { new DateTimeOffset(2026, 10, 19, 1, 0, 0, TimeSpan.Zero), new(2026, 10, 20, 1, 0, 0, TimeSpan.Zero) }, plans.Select(x => x.At));
        Assert.All(plans, x => Assert.Equal(due.Id, x.DueId));
        Assert.Single(ReminderPlanner.Plan([due], Enabled with { DayBefore = false }, Zone));
        Assert.Empty(ReminderPlanner.Plan([due with { IsPaid = true }, due with { DeletedAt = Now }], Enabled, Zone));
    }

    [Theory]
    [InlineData(1380, 1320, 480, 1, 8)] // Overnight: prior-day reminder moves to due-day morning; due-day one expires.
    [InlineData(420, 1320, 480, 2, 8)]
    [InlineData(540, 480, 600, 2, 10)] // Same-day quiet hours.
    [InlineData(540, 540, 540, 2, 9)] // Equal endpoints turn quiet hours off.
    [InlineData(480, 1320, 480, 2, 8)] // Quiet end is exclusive.
    public void Quiet_hours_shift_and_expiration_are_explicit(int minute, int start, int end, int count, int hour)
    {
        var plans = ReminderPlanner.Plan([Item()], new(true, true, minute, start, end), Zone);
        Assert.Equal(count, plans.Count);
        Assert.All(plans, x => Assert.Equal(hour, TimeZoneInfo.ConvertTime(x.At, Zone).Hour));
        Assert.All(plans, x => Assert.True(DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(x.At, Zone).DateTime) <= x.DueDate));
    }

    [Fact]
    public void Date_bounds_and_late_delivery_never_overflow_or_cross_due_date()
    {
        Assert.Single(ReminderPlanner.Plan([Item(DateOnly.MinValue)], Enabled, Zone));
        var settings = Enabled with { MinuteOfDay = 1380 };
        Assert.Single(ReminderPlanner.Plan([Item(DateOnly.MaxValue)], settings, Zone));
        var due = Item(new(2026, 10, 18));
        var occurrence = ReminderPlanner.Plan([due], Enabled, Zone).Last();
        Assert.Equal(new DateTimeOffset(2026, 10, 18, 1, 0, 0, TimeSpan.Zero), ReminderPlanner.DeliveryTime(occurrence, Now, Enabled, Zone));
        var late = new DateTimeOffset(2026, 10, 18, 14, 0, 0, TimeSpan.Zero); // 22:00 Manila.
        Assert.Null(ReminderPlanner.DeliveryTime(occurrence, late, Enabled, Zone));
        Assert.Null(ReminderPlanner.DeliveryTime(occurrence, Now.AddDays(1), Enabled, Zone));
        Assert.Equal(Now.AddHours(2), ReminderPlanner.DeliveryTime(occurrence, Now.AddHours(2), Enabled, Zone));
    }

    [Fact]
    public void Keys_are_culture_invariant_and_change_with_date_or_schedule()
    {
        var due = Item(); var original = CultureInfo.CurrentCulture;
        try
        {
            var keys = ReminderPlanner.Plan([due], Enabled, Zone).Select(x => x.Key).ToArray();
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            Assert.Equal(keys, ReminderPlanner.Plan([due], Enabled, Zone).Select(x => x.Key));
            Assert.DoesNotContain(ReminderPlanner.Plan([due], Enabled with { MinuteOfDay = 600 }, Zone), x => keys.Contains(x.Key));
            Assert.DoesNotContain(ReminderPlanner.Plan([due with { DueDate = due.DueDate.AddDays(1) }], Enabled, Zone), x => keys.Contains(x.Key));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Theory]
    [InlineData(-1, 1320, 480)] [InlineData(1440, 1320, 480)] [InlineData(540, -1, 480)] [InlineData(540, 1320, 1440)]
    public async Task Invalid_settings_never_replace_saved_values(int minute, int start, int end)
    {
        new DatabaseInitializer(_path).Initialize(); var store = new SqliteReminderStore(_path);
        var invalid = new ReminderSettings(true, true, minute, start, end);
        Assert.Throws<ArgumentOutOfRangeException>(() => ReminderPlanner.Plan([Item()], invalid, Zone));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.SaveSettingsAsync(invalid));
        Assert.Equal(new(), await store.ReadSettingsAsync());
    }

    [Fact]
    public async Task Upgrade_from_schema_two_preserves_dues_and_starts_disabled_idempotently()
    {
        using (var connection = new SqliteConnection($"Data Source={_path}"))
        {
            connection.Open();
            foreach (var name in new[] { "001_create_dues.sql", "002_add_category.sql" })
            {
                using var reader = new StreamReader(typeof(DatabaseInitializer).Assembly.GetManifestResourceStream("DueDateChecker.Core.Data.Migrations." + name)!);
                using var command = connection.CreateCommand(); command.CommandText = reader.ReadToEnd(); command.ExecuteNonQuery();
            }
            using var version = connection.CreateCommand(); version.CommandText = "PRAGMA user_version=2"; version.ExecuteNonQuery();
        }
        var repository = new SqliteDueRepository(_path);
        var due = Item() with { IsPaid = true, Category = DueCategory.Subscription, DeletedAt = Now, Notes = "Preserved" };
        await repository.AddAsync(due);
        new DatabaseInitializer(_path).Initialize(); new DatabaseInitializer(_path).Initialize();
        Assert.Equal(due, await repository.GetAsync(due.Id));
        var store = new SqliteReminderStore(_path); Assert.Equal(new(), await store.ReadSettingsAsync()); Assert.Empty(await store.ReadOccurrencesAsync());
        using var check = new SqliteConnection($"Data Source={_path}"); check.Open(); using var commandCheck = check.CreateCommand();
        commandCheck.CommandText = "PRAGMA user_version"; Assert.Equal(3L, commandCheck.ExecuteScalar());
    }

    [Fact]
    public async Task Replacing_pending_is_atomic_and_retains_delivery_receipts_across_reopen()
    {
        new DatabaseInitializer(_path).Initialize(); var store = new SqliteReminderStore(_path);
        var plans = ReminderPlanner.Plan([Item()], Enabled, Zone);
        await store.SaveSettingsAsync(Enabled); await store.ReplacePendingAsync(plans);
        await store.MarkDeliveredAsync(plans[0].Key, Now); await store.MarkDeliveredAsync(plans[0].Key, Now.AddHours(1));
        await store.ReplacePendingAsync(plans);
        Assert.Equal(Now, (await store.ReadOccurrencesAsync()).Single(x => x.Key == plans[0].Key).DeliveredAt);
        // A failed parameter bind must roll back the delete of the existing pending row.
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ReplacePendingAsync([plans[1] with { Key = null! }]));
        Assert.Equal(2, (await store.ReadOccurrencesAsync()).Count);
        await store.ReplacePendingAsync([]);
        var reopened = new SqliteReminderStore(_path);
        Assert.Equal(Enabled, await reopened.ReadSettingsAsync());
        Assert.Equal(Now, Assert.Single(await reopened.ReadOccurrencesAsync()).DeliveredAt);
    }

    public void Dispose() { SqliteConnection.ClearAllPools(); File.Delete(_path); }
}
