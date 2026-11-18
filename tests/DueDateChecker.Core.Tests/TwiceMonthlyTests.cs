using System.Text;
using System.Text.Json.Nodes;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>Twice-a-month repeat (specs/f07-twice-monthly TM01–TM06, TM09; PRD TM).</summary>
public sealed class TwiceMonthlyTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 1, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"twice-{Guid.NewGuid():N}.db");
    private readonly string _target = Path.Combine(Path.GetTempPath(), $"twice-import-{Guid.NewGuid():N}.db");

    public void Dispose()
    {
        foreach (var path in new[] { _path, _target })
        {
            TestDatabase.ClearPool(path);
            File.Delete(path);
        }
    }

    private static Due Twice(DateOnly date, (int, int)? days = null, DueCategory category = DueCategory.Bill) =>
        Due.Create("Rent", Money.FromCentavos(625000), date, Now, "Half each payday", category, RepeatRule.TwiceMonthly,
            "ph.housing.rent", days);

    private static List<DateOnly> Chain(DateOnly start, int first, int second, int count)
    {
        var dates = new List<DateOnly> { start };
        for (var i = 0; i < count; i++) dates.Add(Recurrence.NextDate(dates[^1], RepeatRule.TwiceMonthly, first, second)!.Value);
        return dates;
    }

    private static DateOnly D(int year, int month, int day) => new(year, month, day);

    // ---- TM03–TM05: next dates ----

    [Fact]
    public void Fifteen_and_thirty_alternate_and_february_uses_its_last_day()
    {
        Assert.Equal(new[] { D(2026, 11, 15), D(2026, 11, 30), D(2026, 12, 15) }, Chain(D(2026, 11, 15), 15, 30, 2));
        Assert.Equal(new[] { D(2027, 1, 30), D(2027, 2, 15), D(2027, 2, 28), D(2027, 3, 15), D(2027, 3, 30) }, Chain(D(2027, 1, 30), 15, 30, 4));
        Assert.Equal(new[] { D(2026, 4, 15), D(2026, 4, 30), D(2026, 5, 15), D(2026, 5, 31) }, Chain(D(2026, 4, 15), 15, 31, 3));
    }

    [Fact]
    public void Two_days_landing_on_the_same_date_give_one_occurrence_that_month()
    {
        Assert.Equal(new[] { D(2027, 1, 30), D(2027, 2, 28), D(2027, 3, 29) }, Chain(D(2027, 1, 30), 29, 30, 2));
        Assert.Equal(new[] { D(2028, 1, 30), D(2028, 2, 29), D(2028, 3, 29) }, Chain(D(2028, 1, 30), 29, 30, 2));
    }

    [Theory]
    [InlineData("2026-11-20", "2026-11-30")]
    [InlineData("2026-12-01", "2026-12-15")]
    [InlineData("2026-12-31", "2027-01-15")]
    [InlineData("2026-11-15", "2026-11-30")]
    public void The_first_date_may_be_off_schedule_and_moves_to_the_next_picked_day(string current, string expected)
    {
        Assert.Equal(DateOnly.Parse(expected), Recurrence.NextDate(DateOnly.Parse(current), RepeatRule.TwiceMonthly, 15, 30));
    }

    [Fact]
    public void No_next_date_beyond_the_calendar_or_without_both_days()
    {
        Assert.Equal(D(9999, 12, 30), Recurrence.NextDate(D(9999, 12, 15), RepeatRule.TwiceMonthly, 15, 30));
        Assert.Null(Recurrence.NextDate(D(9999, 12, 30), RepeatRule.TwiceMonthly, 15, 30));
        Assert.Null(Recurrence.NextDate(D(2026, 11, 1), RepeatRule.TwiceMonthly, 15, null));
        Assert.Null(Recurrence.NextDate(D(2026, 11, 1), RepeatRule.TwiceMonthly, null, null));
    }

    // ---- TM02: domain ----

    [Fact]
    public void Create_stores_the_days_in_ascending_order_and_defaults_to_15_and_30()
    {
        var picked = Twice(D(2026, 11, 20), (30, 10));
        Assert.Equal((RepeatRule.TwiceMonthly, (int?)10, (int?)30), (picked.Repeat, picked.RepeatDay, picked.RepeatDay2));

        var defaulted = Twice(D(2026, 11, 20));
        Assert.Equal(((int?)15, (int?)30), (defaulted.RepeatDay, defaulted.RepeatDay2));

        var monthly = Due.Create("Rent", Money.FromCentavos(1), D(2026, 11, 20), Now, null, DueCategory.Bill, RepeatRule.Monthly, twiceMonthlyDays: (5, 6));
        Assert.Equal(((int?)20, (int?)null), (monthly.RepeatDay, monthly.RepeatDay2));
    }

    [Theory]
    [InlineData(15, 15)]
    [InlineData(0, 30)]
    [InlineData(15, 32)]
    public void Create_rejects_equal_or_out_of_range_days(int first, int second)
    {
        Assert.Throws<ArgumentException>(() => Twice(D(2026, 11, 20), (first, second)));
    }

    [Fact]
    public void Only_bill_and_subscription_repeat_twice_a_month()
    {
        Twice(D(2026, 11, 20), category: DueCategory.Subscription).Validate();
        Assert.Equal(nameof(Due.Repeat), Assert.Throws<ArgumentException>(() => Twice(D(2026, 11, 20), category: DueCategory.Task)).ParamName);
        Assert.Equal(nameof(Due.Repeat), Assert.Throws<ArgumentException>(() => Twice(D(2026, 11, 20), category: DueCategory.Other)).ParamName);
    }

    [Fact]
    public void Validate_checks_the_day_pair_on_every_path()
    {
        var due = Twice(D(2026, 11, 20));
        Assert.Equal(nameof(Due.RepeatDay2), Assert.Throws<ArgumentException>(() => (due with { RepeatDay2 = null }).Validate()).ParamName);
        Assert.Equal(nameof(Due.RepeatDay2), Assert.Throws<ArgumentException>(() => (due with { RepeatDay = 30, RepeatDay2 = 15 }).Validate()).ParamName);
        Assert.Equal(nameof(Due.RepeatDay), Assert.Throws<ArgumentException>(() => (due with { RepeatDay = null }).Validate()).ParamName);
        Assert.Equal(nameof(Due.RepeatDay2), Assert.Throws<ArgumentException>(() => (due with { Repeat = RepeatRule.Monthly }).Validate()).ParamName);
        Assert.Equal(nameof(Due.RepeatDay2), Assert.Throws<ArgumentException>(() => (due with { Repeat = RepeatRule.Never, RepeatDay = null }).Validate()).ParamName);
        (due with { Repeat = RepeatRule.Monthly, RepeatDay2 = null }).Validate();
    }

    [Fact]
    public void Switching_to_monthly_anchors_on_the_due_date_not_the_first_picked_day()
    {
        var due = Twice(D(2026, 11, 15), (4, 19));
        Assert.Equal(15, Recurrence.AnchorAfterEdit(due, RepeatRule.Monthly, due.DueDate));
        Assert.Equal(15, Recurrence.AnchorAfterEdit(due, RepeatRule.Yearly, due.DueDate));
        Assert.Null(Recurrence.AnchorAfterEdit(due, RepeatRule.TwiceMonthly, due.DueDate));
    }

    // ---- TM06: next occurrence ----

    [Fact]
    public void Next_occurrence_copies_both_days_and_the_category()
    {
        var paid = Twice(D(2026, 11, 15)) with { IsPaid = true };
        var later = Now.AddHours(2);

        var next = Recurrence.NextOnPayment(paid with { IsPaid = false }, paid, later)!;

        Assert.Equal(D(2026, 11, 30), next.DueDate);
        Assert.Equal((paid.Title, paid.Amount, paid.Notes, paid.Category, paid.CategoryId, paid.Repeat, paid.RepeatDay, paid.RepeatDay2),
            (next.Title, next.Amount, next.Notes, next.Category, next.CategoryId, next.Repeat, next.RepeatDay, next.RepeatDay2));
        Assert.False(next.IsPaid);
        Assert.Equal(paid.Id, next.RepeatSourceId);
        Assert.Equal(later, next.CreatedAt);
    }

    // ---- TM01: migration 007 ----

    [Fact]
    public void Fresh_database_accepts_code_four_and_the_second_day_column()
    {
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(8L, Scalar("PRAGMA user_version"));
        Assert.Equal("repeat_day_2|INTEGER|0|NULL",
            Snapshot("SELECT name, type, \"notnull\", dflt_value FROM pragma_table_info('dues') WHERE name = 'repeat_day_2'"));
        Assert.Equal("ix_dues_active_due_date\nix_dues_repeat_source",
            Snapshot("SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'dues' AND name LIKE 'ix_%' ORDER BY name"));
    }

    [Fact]
    public async Task Upgrade_from_schema_six_keeps_every_row_and_value()
    {
        CreateSchemaSix();
        Execute("""
            INSERT INTO dues (id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id)
            VALUES ('00000000-0000-0000-0000-000000000001', 'Rent', 1200000, '2026-11-30', 'Unit 4B', 0, '2026-10-01T00:00:00.0000000+00:00', '2026-10-01T00:00:00.0000000+00:00', NULL, 1, 2, 31, NULL, 'ph.housing.rent'),
                   ('00000000-0000-0000-0000-000000000002', 'Old rent', 1200000, '2026-10-31', NULL, 1, '2026-09-01T00:00:00.0000000+00:00', '2026-10-31T00:00:00.0000000+00:00', '2026-11-01T00:00:00.0000000+00:00', 1, 2, 31, NULL, 'ph.housing.rent'),
                   ('00000000-0000-0000-0000-000000000003', 'Renew ID', NULL, '2026-12-15', NULL, 0, '2026-10-02T00:00:00.0000000+00:00', '2026-10-02T00:00:00.0000000+00:00', NULL, 2, 0, NULL, '00000000-0000-0000-0000-000000000002', 'ph.other.general');
            """);
        const string Original = "SELECT rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id FROM dues ORDER BY rowid";
        var before = Snapshot(Original);

        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(8L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot(Original));
        Assert.Equal("NULL\nNULL\nNULL", Snapshot("SELECT repeat_day_2 FROM dues ORDER BY rowid"));
        var monthly = (await new SqliteDueRepository(_path).GetAsync(Guid.Parse("00000000-0000-0000-0000-000000000001")))!;
        Assert.Equal((RepeatRule.Monthly, (int?)31, (int?)null), (monthly.Repeat, monthly.RepeatDay, monthly.RepeatDay2));
        Assert.Equal(2L, Scalar("SELECT COUNT(*) FROM dues WHERE deleted_at IS NULL"));
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET repeat_rule = 5"));
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET repeat_day_2 = 32"));
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET category = 9"));
    }

    [Fact]
    public void A_failed_rebuild_leaves_schema_six_and_its_rows()
    {
        CreateSchemaSix();
        // A row the old table accepted only because CHECKs were off fails the copy into the new table,
        // after CREATE TABLE ran: the whole migration must roll back.
        Execute("""
            PRAGMA ignore_check_constraints = ON;
            INSERT INTO dues (id, title, amount_centavos, due_date, is_paid, created_at, updated_at, category)
            VALUES ('00000000-0000-0000-0000-000000000009', 'Broken', 1, '2026-11-30', 0, '2026-10-01T00:00:00.0000000+00:00', '2026-10-01T00:00:00.0000000+00:00', 9);
            PRAGMA ignore_check_constraints = OFF;
            """); // OFF again: the pooled connection is reused by the initializer.
        var before = Snapshot("SELECT rowid, * FROM dues");
        var schema = Snapshot("SELECT type, name, sql FROM sqlite_master ORDER BY name");

        Assert.Throws<SqliteException>(() => new DatabaseInitializer(_path).Initialize());

        Assert.Equal(6L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot("SELECT rowid, * FROM dues"));
        Assert.Equal(schema, Snapshot("SELECT type, name, sql FROM sqlite_master ORDER BY name"));
    }

    // ---- TM06: repository ----

    [Fact]
    public async Task Days_round_trip_and_paid_adds_one_next_occurrence()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Twice(D(2026, 11, 15), (15, 30));
        await repository.AddAsync(due);
        Assert.Equal(due, await new SqliteDueRepository(_path).GetAsync(due.Id));

        var moved = due with { RepeatDay = 10, RepeatDay2 = 25, UpdatedAt = Now.AddHours(1) };
        await repository.UpdateAsync(moved);
        Assert.Equal(moved, await repository.GetAsync(due.Id));

        var paid = moved with { IsPaid = true, UpdatedAt = Now.AddHours(2) };
        var next = Recurrence.NextOnPayment(moved, paid, Now.AddHours(2))!;
        Assert.True(await repository.UpdateWithNextAsync(paid, next));
        Assert.False(await repository.UpdateWithNextAsync(paid, Recurrence.NextOccurrence(paid, Now.AddHours(3))!));

        var stored = await repository.ListActiveAsync();
        Assert.Equal(2, stored.Count);
        var added = stored.Single(d => d.RepeatSourceId == due.Id);
        Assert.Equal((D(2026, 11, 25), (int?)10, (int?)25), (added.DueDate, added.RepeatDay, added.RepeatDay2));

        var monthly = moved with { Repeat = RepeatRule.Monthly, RepeatDay = 15, RepeatDay2 = null, IsPaid = true, UpdatedAt = Now.AddHours(4) };
        await repository.UpdateAsync(monthly);
        Assert.Null((await repository.GetAsync(due.Id))!.RepeatDay2);
    }

    // ---- TM09: export and import ----

    [Fact]
    public async Task Export_writes_both_days_and_import_restores_them()
    {
        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_target).Initialize();
        var due = Twice(D(2026, 11, 15), (4, 19));
        await new SqliteDueRepository(_path).AddAsync(due);

        var file = DueArchive.Write(await new SqliteDueArchive(_path).ReadAllAsync(), Now);
        var json = JsonNode.Parse(file)!;
        Assert.Equal(DueArchive.CurrentVersion, (int)json["version"]!);
        Assert.Equal(19, (int)json["dues"]![0]!["repeatDay2"]!);
        Assert.Equal(4, (int)json["dues"]![0]!["repeatDay"]!);

        await new SqliteDueArchive(_target).ImportAsync(DueArchive.Read(file).Dues);
        Assert.Equal(due, await new SqliteDueRepository(_target).GetAsync(due.Id));
    }

    [Fact]
    public void Version_two_files_stay_readable_without_a_second_day()
    {
        var text = """
            { "format": "duewise.dues", "version": 2, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [
              { "id": "00000000-0000-0000-0000-000000000001", "title": "Rent", "amountCentavos": 1500000, "currency": "PHP",
                "dueDate": "2026-11-30", "notes": null, "isPaid": false, "timezone": "Asia/Manila",
                "createdAt": "2026-11-01T02:00:00.0000000+00:00", "updatedAt": "2026-11-01T02:00:00.0000000+00:00", "deletedAt": null,
                "category": 1, "repeatRule": 2, "repeatDay": 30, "repeatSourceId": null, "categoryId": "ph.housing.rent" } ] }
            """;
        var record = Assert.Single(DueArchive.Read(Encoding.UTF8.GetBytes(text)).Dues);
        Assert.Null(record.RepeatDay2);

        // Version 2 never had the field, so a file that carries it is not a valid version 2 file.
        var extra = text.Replace("\"categoryId\": \"ph.housing.rent\"", "\"categoryId\": \"ph.housing.rent\", \"repeatDay2\": null");
        Assert.Equal("repeatDay2", Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(extra))).Field);
    }

    [Theory]
    [InlineData(4, "null", "repeatDay2")]   // twice a month needs a second day
    [InlineData(4, "4", "repeatDay2")]      // the days must differ
    [InlineData(4, "32", "repeatDay2")]
    [InlineData(2, "19", "repeatDay2")]     // only twice a month stores a second day
    [InlineData(5, "19", "repeatRule")]     // undefined repeat code
    public void Import_refuses_an_invalid_day_pair(int repeatRule, string repeatDay2, string field)
    {
        var text = $$"""
            { "format": "duewise.dues", "version": 3, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [
              { "id": "00000000-0000-0000-0000-000000000001", "title": "Rent", "amountCentavos": 625000, "currency": "PHP",
                "dueDate": "2026-11-19", "notes": null, "isPaid": false, "timezone": "Asia/Manila",
                "createdAt": "2026-11-01T02:00:00.0000000+00:00", "updatedAt": "2026-11-01T02:00:00.0000000+00:00", "deletedAt": null,
                "category": 1, "repeatRule": {{repeatRule}}, "repeatDay": 4, "repeatSourceId": null, "categoryId": "ph.housing.rent",
                "repeatDay2": {{repeatDay2}} } ] }
            """;
        var problem = Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(text)));
        Assert.Equal((DueArchiveProblem.InvalidDue, (int?)1, field), (problem.Problem, problem.DueNumber, problem.Field));
    }

    // ---- TM08 (Core part): reminder text and fingerprint ----

    [Fact]
    public void Reminder_text_says_twice_a_month_and_the_fingerprint_follows_the_second_day()
    {
        var due = Twice(D(2026, 11, 15), (15, 30));
        Assert.EndsWith(" · repeats twice a month", ReminderText.Create(due, D(2026, 11, 14)).Body);
        Assert.NotEqual(ReminderText.Fingerprint(due), ReminderText.Fingerprint(due with { RepeatDay2 = 31 }));

        // Dues without a second day keep the fingerprint they had before schema 7, so their
        // already-delivered notification actions stay valid.
        var monthly = due with { Repeat = RepeatRule.Monthly, RepeatDay = 15, RepeatDay2 = null };
        Assert.Equal(FingerprintBeforeSchemaSeven(monthly), ReminderText.Fingerprint(monthly));
        Assert.EndsWith(" · repeats monthly", ReminderText.Create(monthly, D(2026, 11, 14)).Body);
    }

    // The ReminderText.Fingerprint field sequence as it was before the second day existed.
    private static string FingerprintBeforeSchemaSeven(Due due)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(due.Id.ToString("N")); writer.Write(due.Title);
            writer.Write(due.Amount.HasValue); writer.Write(due.Amount?.Centavos ?? 0);
            writer.Write(due.DueDate.DayNumber); writer.Write(due.Notes is not null); writer.Write(due.Notes ?? "");
            writer.Write(due.IsPaid); writer.Write(due.CreatedAt.UtcTicks); writer.Write(due.UpdatedAt.UtcTicks);
            writer.Write(due.DeletedAt?.UtcTicks ?? 0); writer.Write((int)due.Category); writer.Write((int)due.Repeat);
            writer.Write(due.RepeatDay ?? 0); writer.Write(due.RepeatSourceId?.ToString("N") ?? "");
            writer.Write(due.CategoryId);
        }
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream.ToArray()));
    }

    private SqliteConnection Open(string? path = null)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path ?? _path }.ToString());
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

    // Schema 6 exactly as migrations 001–006 left it, before this slice's 007.
    private void CreateSchemaSix()
    {
        var assembly = typeof(DatabaseInitializer).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.Contains(".Migrations.") && n.EndsWith(".sql"))
                     .Where(n => int.Parse(n.Split(".Migrations.")[1][..3]) <= 6).Order())
        {
            using var reader = new StreamReader(assembly.GetManifestResourceStream(name)!);
            Execute(reader.ReadToEnd());
        }
        Execute("PRAGMA user_version = 6;");
    }
}
