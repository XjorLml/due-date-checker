using System.Text;
using System.Text.Json.Nodes;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>UX review decisions of 2026-11-16 in Core (specs/ux-improvements-3: OD01–OD05, AV02–AV05).</summary>
public sealed class UxImprovements3Tests : IDisposable
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 1, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"ux3-{Guid.NewGuid():N}.db");
    private readonly string _target = Path.Combine(Path.GetTempPath(), $"ux3-import-{Guid.NewGuid():N}.db");

    public void Dispose()
    {
        foreach (var path in new[] { _path, _target })
        {
            TestDatabase.ClearPool(path);
            File.Delete(path);
        }
    }

    private static Due Bill(DateOnly date, long? centavos = 245050, bool varies = false, RepeatRule repeat = RepeatRule.Monthly) =>
        Due.Create("Meralco", centavos is { } c ? Money.FromCentavos(c) : null, date, Now, null, DueCategory.Bill, repeat,
            "ph.utilities.electricity", amountVaries: varies);

    private static DateOnly Manila(DateTimeOffset at) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(at, Zone).DateTime);

    // ---- OD01–OD03: overdue reminders ----

    [Fact]
    public void Overdue_reminders_come_1_7_14_21_and_28_days_late_at_the_reminder_time()
    {
        var due = Bill(new DateOnly(2026, 11, 12));
        var plans = ReminderPlanner.Plan([due], new ReminderSettings(Enabled: true), Zone);

        Assert.Equal(new[] { 11, 12, 13, 19, 26 }.Select(d => new DateOnly(2026, 11, d))
                .Concat(new[] { new DateOnly(2026, 12, 3), new DateOnly(2026, 12, 10) }),
            plans.Select(item => Manila(item.At)));
        Assert.All(plans, item => Assert.Equal(9, TimeZoneInfo.ConvertTime(item.At, Zone).Hour));
        Assert.Equal(2, ReminderPlanner.Plan([due], new ReminderSettings(Enabled: true, Overdue: false), Zone).Count);
        Assert.Empty(ReminderPlanner.Plan([due with { IsPaid = true }], new ReminderSettings(Enabled: true), Zone));
        Assert.Equal(new DateOnly(2026, 10, 19), ReminderPlanner.EarliestRelevantDueDate(new DateOnly(2026, 11, 16)));
        Assert.Equal(DateOnly.MinValue, ReminderPlanner.EarliestRelevantDueDate(new DateOnly(1, 1, 5)));
    }

    [Fact]
    public void A_missed_overdue_reminder_is_delivered_later_that_day_only()
    {
        var settings = new ReminderSettings(Enabled: true, DayBefore: false);
        var dayLate = ReminderPlanner.Plan([Bill(new DateOnly(2026, 11, 15))], settings, Zone)
            .Single(item => Manila(item.At) == new DateOnly(2026, 11, 16));
        Assert.Equal(new DateOnly(2026, 11, 16), ReminderPlanner.LastDeliveryDate(dayLate, Zone));

        var noonManila = new DateTimeOffset(2026, 11, 16, 4, 0, 0, TimeSpan.Zero);
        Assert.Equal(noonManila, ReminderPlanner.DeliveryTime(dayLate, noonManila, settings, Zone));
        var nextMorning = new DateTimeOffset(2026, 11, 17, 1, 0, 0, TimeSpan.Zero);
        Assert.Null(ReminderPlanner.DeliveryTime(dayLate, nextMorning, settings, Zone));

        // Quiet hours from 08:00 to 10:00 move it, keeping the day.
        var quiet = settings with { QuietStart = 480, QuietEnd = 600 };
        var moved = ReminderPlanner.Plan([Bill(new DateOnly(2026, 11, 15))], quiet, Zone).First(item => Manila(item.At) == new DateOnly(2026, 11, 16));
        Assert.Equal(10, TimeZoneInfo.ConvertTime(moved.At, Zone).Hour);
    }

    [Fact]
    public void Overdue_reminder_text_says_how_late_and_keeps_the_lock_screen_generic()
    {
        var due = Bill(new DateOnly(2026, 11, 12));
        var oneDay = ReminderText.Create(due, new DateOnly(2026, 11, 13));
        Assert.Equal(("Meralco is 1 day late", ReminderKind.Overdue, "You have an overdue due"), (oneDay.Title, oneDay.Kind, oneDay.PublicBody));
        Assert.Equal("₱2,450.50 · Thu 12 Nov · repeats monthly", oneDay.Body);
        Assert.Equal("Meralco is 7 days late", ReminderText.Create(due, new DateOnly(2026, 11, 19)).Title);
        Assert.Equal(ReminderKind.DueToday, ReminderText.Create(due, due.DueDate).Kind);
        Assert.Equal(ReminderKind.ComingUp, ReminderText.Create(due, due.DueDate.AddDays(-1)).Kind);
    }

    [Fact]
    public async Task The_overdue_setting_is_stored_and_defaults_to_on()
    {
        new DatabaseInitializer(_path).Initialize();
        var store = new SqliteReminderStore(_path);
        Assert.True((await store.ReadSettingsAsync()).Overdue);
        await store.SaveSettingsAsync(new ReminderSettings(Enabled: true, Overdue: false));
        Assert.Equal(new ReminderSettings(Enabled: true, Overdue: false), await new SqliteReminderStore(_path).ReadSettingsAsync());
    }

    // ---- AV02–AV04: amounts that change each month ----

    [Fact]
    public void Only_a_repeating_due_may_wait_for_its_amount()
    {
        Assert.Null(Bill(new DateOnly(2026, 11, 20), centavos: null, varies: true).Amount);
        Assert.Equal(nameof(Due.Amount), Assert.Throws<ArgumentException>(() => Bill(new DateOnly(2026, 11, 20), centavos: null)).ParamName);
        Assert.Equal(nameof(Due.AmountVaries),
            Assert.Throws<ArgumentException>(() => Bill(new DateOnly(2026, 11, 20), varies: true, repeat: RepeatRule.Never)).ParamName);
        var due = Bill(new DateOnly(2026, 11, 20), varies: true);
        Assert.Throws<ArgumentException>(() => (due with { Repeat = RepeatRule.Never, RepeatDay = null }).Validate());
        Assert.Throws<ArgumentException>(() => (due with { AmountVaries = false, Amount = null }).Validate());
    }

    [Fact]
    public void The_next_occurrence_of_a_varying_bill_starts_without_an_amount()
    {
        var paid = Bill(new DateOnly(2026, 11, 20), varies: true) with { IsPaid = true };
        var next = Recurrence.NextOccurrence(paid, Now)!;
        Assert.Equal((new DateOnly(2026, 12, 20), (Money?)null, true), (next.DueDate, next.Amount, next.AmountVaries));

        var fixedBill = Bill(new DateOnly(2026, 11, 20)) with { IsPaid = true };
        Assert.Equal(fixedBill.Amount, Recurrence.NextOccurrence(fixedBill, Now)!.Amount);

        var text = ReminderText.Create(next, next.DueDate);
        Assert.StartsWith("Amount needed · ", text.Body);
        Assert.NotEqual(ReminderText.Fingerprint(fixedBill), ReminderText.Fingerprint(fixedBill with { AmountVaries = true }));
    }

    // ---- AV05 and OD05: migration 008 ----

    [Fact]
    public async Task Upgrade_to_schema_eight_keeps_rows_and_adds_the_two_switches()
    {
        CreateSchema(7);
        Execute("""
            INSERT INTO dues (id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id, repeat_day_2)
            VALUES ('00000000-0000-0000-0000-000000000001', 'Rent', 625000, '2026-11-30', NULL, 0, '2026-10-01T00:00:00.0000000+00:00', '2026-10-01T00:00:00.0000000+00:00', NULL, 1, 4, 15, NULL, 'ph.housing.rent', 30);
            UPDATE reminder_settings SET enabled = 1, minute_of_day = 600 WHERE id = 1;
            """);
        const string Old = "SELECT rowid, id, title, amount_centavos, due_date, is_paid, category, repeat_rule, repeat_day, repeat_day_2, category_id FROM dues";
        var before = Snapshot(Old);

        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(8L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot(Old));
        Assert.Equal(0L, Scalar("SELECT amount_varies FROM dues"));
        Assert.Equal("1|1|600|1", Snapshot("SELECT enabled, day_before, minute_of_day, overdue FROM reminder_settings"));
        Assert.False((await new SqliteDueRepository(_path).ListActiveAsync()).Single().AmountVaries);
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET amount_varies = 2"));
        Assert.Throws<SqliteException>(() => Execute("UPDATE reminder_settings SET overdue = 2"));
    }

    [Fact]
    public async Task A_varying_bill_round_trips_through_storage_and_export_format_four()
    {
        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_target).Initialize();
        var due = Bill(new DateOnly(2026, 11, 20), centavos: null, varies: true);
        await new SqliteDueRepository(_path).AddAsync(due);
        Assert.Equal(due, await new SqliteDueRepository(_path).GetAsync(due.Id));

        var file = DueArchive.Write(await new SqliteDueArchive(_path).ReadAllAsync(), Now);
        var json = JsonNode.Parse(file)!;
        Assert.Equal(4, (int)json["version"]!);
        Assert.True((bool)json["dues"]![0]!["amountVaries"]!);

        await new SqliteDueArchive(_target).ImportAsync(DueArchive.Read(file).Dues);
        Assert.Equal(due, await new SqliteDueRepository(_target).GetAsync(due.Id));

        json["dues"]![0]!["amountVaries"] = "yes";
        Assert.Equal("amountVaries", Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(json.ToJsonString()))).Field);
        json["dues"]![0]!["amountVaries"] = false; // a Bill without an amount that does not vary is invalid
        Assert.Equal("amountCentavos", Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(json.ToJsonString()))).Field);
        json["version"] = 3;
        json["dues"]![0]!.AsObject().Remove("amountVaries");
        Assert.Equal("amountCentavos", Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(json.ToJsonString()))).Field);
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _path }.ToString());
        connection.Open();
        return connection;
    }

    private void Execute(string sql)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private object? Scalar(string sql)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    private string Snapshot(string sql)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var rows = new List<string>();
        while (reader.Read())
            rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "NULL" : reader.GetValue(i))));
        return string.Join("\n", rows);
    }

    private void CreateSchema(int version)
    {
        var assembly = typeof(DatabaseInitializer).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.Contains(".Migrations.") && n.EndsWith(".sql"))
                     .Where(n => int.Parse(n.Split(".Migrations.")[1][..3]) <= version).Order())
        {
            using var reader = new StreamReader(assembly.GetManifestResourceStream(name)!);
            Execute(reader.ReadToEnd());
        }
        Execute($"PRAGMA user_version = {version};");
    }
}
