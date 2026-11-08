using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>F07 optional amounts (specs/f07-optional-amounts OA01, OA02, OA05; reminder part of OA04).</summary>
public sealed class OptionalAmountTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Date = new(2026, 11, 20);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"optional-amounts-{Guid.NewGuid():N}.db");

    // The schema-3 columns, named so later migrations that add columns do not change the snapshot.
    private const string OriginalRows =
        "SELECT rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category FROM dues ORDER BY rowid";

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    // ---- Domain (OA02) ----

    [Theory]
    [InlineData(DueCategory.Bill, true)]
    [InlineData(DueCategory.Subscription, true)]
    [InlineData(DueCategory.Task, false)]
    [InlineData(DueCategory.Other, false)]
    public void Only_bill_and_subscription_require_an_amount(DueCategory category, bool required)
    {
        Assert.Equal(required, Due.RequiresAmount(category));
    }

    [Theory]
    [InlineData(DueCategory.Task)]
    [InlineData(DueCategory.Other)]
    public void Task_and_other_can_be_created_without_an_amount(DueCategory category)
    {
        var due = Due.Create("Renew passport", null, Date, Now, category: category);

        Assert.Null(due.Amount);
        due.Validate();
    }

    [Theory]
    [InlineData(DueCategory.Bill)]
    [InlineData(DueCategory.Subscription)]
    public void Bill_and_subscription_reject_a_missing_amount(DueCategory category)
    {
        Assert.Throws<ArgumentException>(() => Due.Create("Electricity", null, Date, Now, category: category));
    }

    [Theory]
    [InlineData(DueCategory.Bill)]
    [InlineData(DueCategory.Subscription)]
    [InlineData(DueCategory.Task)]
    [InlineData(DueCategory.Other)]
    public void Zero_is_a_recorded_amount_for_every_category(DueCategory category)
    {
        var due = Due.Create("Free tier", Money.FromCentavos(0), Date, Now, category: category);

        Assert.Equal(Money.FromCentavos(0), due.Amount);
        Assert.NotNull(due.Amount);
    }

    [Fact]
    public void A_copied_record_is_checked_by_Validate()
    {
        // A with-expression sets properties one at a time, so the cross-field rule
        // cannot live in an init accessor; Validate() checks the finished record.
        var task = Due.Create("Call the bank", null, Date, Now, category: DueCategory.Task);
        var bill = task with { Category = DueCategory.Bill };

        Assert.Throws<ArgumentException>(bill.Validate);
        (bill with { Amount = Money.FromCentavos(1) }).Validate();
    }

    // ---- Migration 004 (OA01) ----

    [Fact]
    public void Fresh_database_reaches_the_current_schema_with_a_nullable_amount()
    {
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(6L, Scalar("PRAGMA user_version"));
        Assert.Equal(0L, Scalar("SELECT \"notnull\" FROM pragma_table_info('dues') WHERE name = 'amount_centavos'"));
    }

    [Fact]
    public void Upgrade_from_schema_three_preserves_every_row_index_and_constraint()
    {
        CreateSchemaThree();
        Execute("""
            INSERT INTO dues (id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category)
            VALUES ('a1', 'Active bill', 12345, 'PHP', '2026-11-20', 'ref 1', 0, 'Asia/Manila', '2026-10-01T12:00:00.0000000+08:00', '2026-10-02T12:00:00.0000000+08:00', NULL, 1),
                   ('a2', 'Paid zero task', 0, 'PHP', '2026-11-01', NULL, 1, 'Asia/Manila', '2026-10-03T00:00:00.0000000+00:00', '2026-10-04T00:00:00.0000000+00:00', NULL, 2),
                   ('a3', 'Deleted subscription', 99900, 'PHP', '2026-10-15', 'gone', 0, 'Asia/Manila', '2026-10-05T00:00:00.0000000+00:00', '2026-10-06T00:00:00.0000000+00:00', '2026-10-06T00:00:00.0000000+00:00', 3),
                   ('a4', 'Unusual stored values', 500, 'USD', '2026-12-31', NULL, 0, 'UTC', '2026-10-07T00:00:00.0000000+00:00', '2026-10-07T00:00:00.0000000+00:00', NULL, 0);
            UPDATE reminder_settings SET enabled = 1, minute_of_day = 600 WHERE id = 1;
            """);
        var rowsBefore = Snapshot(OriginalRows);
        var indexBefore = Scalar("SELECT sql FROM sqlite_master WHERE name = 'ix_dues_active_due_date'");
        var columnsBefore = Snapshot("SELECT cid, name, type, dflt_value, pk FROM pragma_table_info('dues') WHERE cid < 12");
        var notNullBefore = Snapshot("SELECT name, \"notnull\" FROM pragma_table_info('dues') WHERE cid < 12 AND name <> 'amount_centavos'");
        var planBefore = ListQueryPlan();
        var remindersBefore = Snapshot("SELECT * FROM reminder_settings");

        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(6L, Scalar("PRAGMA user_version")); // 004, then 005 (recurrence columns, checked in RecurrenceTests)
        Assert.Equal(rowsBefore, Snapshot(OriginalRows));
        Assert.Equal(indexBefore, Scalar("SELECT sql FROM sqlite_master WHERE name = 'ix_dues_active_due_date'"));
        Assert.Equal(columnsBefore, Snapshot("SELECT cid, name, type, dflt_value, pk FROM pragma_table_info('dues') WHERE cid < 12"));
        Assert.Equal(notNullBefore, Snapshot("SELECT name, \"notnull\" FROM pragma_table_info('dues') WHERE cid < 12 AND name <> 'amount_centavos'"));
        Assert.Equal(0L, Scalar("SELECT \"notnull\" FROM pragma_table_info('dues') WHERE name = 'amount_centavos'"));
        Assert.Equal(planBefore, ListQueryPlan());
        Assert.Equal(remindersBefore, Snapshot("SELECT * FROM reminder_settings"));
        Assert.Equal(0L, Scalar("SELECT COUNT(*) FROM sqlite_master WHERE name = 'dues_v4'"));
        // The category CHECK from migration 002 survives the rebuild.
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET category = 9 WHERE id = 'a1'"));
    }

    [Fact]
    public void Failed_upgrade_leaves_schema_three_and_its_rows_untouched()
    {
        CreateSchemaThree();
        Execute("""
            INSERT INTO dues (id, title, amount_centavos, due_date, created_at, updated_at, category)
            VALUES ('kept', 'Kept', 100, '2026-11-20', 'original', 'original', 1);
            CREATE TABLE dues_v4 (blocker INTEGER);
            """);
        var before = Snapshot("SELECT rowid, * FROM dues ORDER BY rowid");

        Assert.Throws<SqliteException>(() => new DatabaseInitializer(_path).Initialize());

        Assert.Equal(3L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot("SELECT rowid, * FROM dues ORDER BY rowid"));
        Assert.Equal(1L, Scalar("SELECT \"notnull\" FROM pragma_table_info('dues') WHERE name = 'amount_centavos'"));
    }

    [Fact]
    public void Failure_after_earlier_statements_rolls_back_the_whole_script()
    {
        // Mid-script failure (STATUS known gap, closed 2026-11-06): CREATE TABLE dues_v4 succeeds,
        // then the copy hits a row that breaks the category CHECK (planted with checks disabled).
        // SQLite DDL is transactional, so the created table must disappear with the rollback.
        CreateSchemaThree();
        Execute("""
            PRAGMA ignore_check_constraints = ON;
            INSERT INTO dues (id, title, amount_centavos, due_date, created_at, updated_at, category)
            VALUES ('good', 'Good row', 100, '2026-11-20', 'original', 'original', 1),
                   ('corrupt', 'Corrupt row', 200, '2026-11-21', 'original', 'original', 9);
            PRAGMA ignore_check_constraints = OFF;
            """);
        // PRAGMAs belong to the connection, and pooled connections are reused, so the switch is
        // turned off again before the initializer borrows the same connection.
        var before = Snapshot("SELECT rowid, * FROM dues ORDER BY rowid");
        var schemaBefore = Snapshot("SELECT type, name, sql FROM sqlite_master ORDER BY name");

        Assert.Throws<SqliteException>(() => new DatabaseInitializer(_path).Initialize());

        Assert.Equal(3L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot("SELECT rowid, * FROM dues ORDER BY rowid"));
        Assert.Equal(schemaBefore, Snapshot("SELECT type, name, sql FROM sqlite_master ORDER BY name"));
        Assert.Equal(0L, Scalar("SELECT COUNT(*) FROM sqlite_master WHERE name = 'dues_v4'"));
    }

    // ---- Repository (OA02, OA05) ----

    [Theory]
    [InlineData(DueCategory.Task)]
    [InlineData(DueCategory.Other)]
    public async Task Missing_amount_and_zero_stay_distinct_after_reopening(DueCategory category)
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var missing = Due.Create("No amount", null, Date, Now, category: category);
        var zero = Due.Create("Zero amount", Money.FromCentavos(0), Date, Now, category: category);
        await repository.AddAsync(missing);
        await repository.AddAsync(zero);

        var reopened = new SqliteDueRepository(_path);

        Assert.Equal(missing, await reopened.GetAsync(missing.Id));
        Assert.Null((await reopened.GetAsync(missing.Id))!.Amount);
        Assert.Equal(Money.FromCentavos(0), (await reopened.GetAsync(zero.Id))!.Amount);
        Assert.Equal(DBNull.Value, Scalar($"SELECT amount_centavos FROM dues WHERE id = '{missing.Id}'"));
        Assert.Equal(0L, Scalar($"SELECT amount_centavos FROM dues WHERE id = '{zero.Id}'"));
    }

    [Fact]
    public async Task Clearing_and_restoring_an_amount_keeps_identity_and_soft_delete_keeps_it_absent()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Due.Create("Pick up documents", Money.FromDecimal(150m), Date, Now, category: DueCategory.Task);
        await repository.AddAsync(due);

        var cleared = due with { Amount = null, UpdatedAt = Now.AddHours(1) };
        await repository.UpdateAsync(cleared);
        Assert.Equal(cleared, await new SqliteDueRepository(_path).GetAsync(due.Id));

        var restored = cleared with { Amount = Money.FromCentavos(2500), UpdatedAt = Now.AddHours(2) };
        await repository.UpdateAsync(restored);
        var stored = await new SqliteDueRepository(_path).GetAsync(due.Id);
        Assert.Equal(restored, stored);
        Assert.Equal(due.CreatedAt, stored!.CreatedAt);

        await repository.UpdateAsync(restored with { Amount = null, UpdatedAt = Now.AddHours(3) });
        await repository.SoftDeleteAsync(due.Id, Now.AddHours(4));
        var tombstone = await new SqliteDueRepository(_path).GetAsync(due.Id);
        Assert.NotNull(tombstone!.DeletedAt);
        Assert.Null(tombstone.Amount);
    }

    [Theory]
    [InlineData(DueCategory.Bill)]
    [InlineData(DueCategory.Subscription)]
    public async Task Repository_refuses_to_write_a_required_category_without_an_amount(DueCategory category)
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var task = Due.Create("Was a task", null, Date, Now, category: DueCategory.Task);

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddAsync(task with { Category = category }));
        Assert.Empty(await repository.ListActiveAsync());

        await repository.AddAsync(task);
        await Assert.ThrowsAsync<ArgumentException>(() => repository.UpdateAsync(task with { Category = category, UpdatedAt = Now.AddHours(1) }));
        Assert.Equal(task, await repository.GetAsync(task.Id));
    }

    // ---- Reminders ignore the amount (OA04) ----

    [Fact]
    public void Reminder_planning_is_the_same_with_or_without_an_amount()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        var settings = new ReminderSettings(Enabled: true);
        var withAmount = Due.Create("Same due", Money.FromCentavos(1000), Date, Now, category: DueCategory.Task);
        var without = withAmount with { Amount = null };

        Assert.Equal(ReminderPlanner.Plan([withAmount], settings, zone), ReminderPlanner.Plan([without], settings, zone));
    }

    // ---- Helpers ----

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

    // The repository's list query; the rebuilt table must still be read through the partial index.
    private string ListQueryPlan() => Snapshot(
        "EXPLAIN QUERY PLAN SELECT id FROM dues WHERE deleted_at IS NULL ORDER BY due_date, created_at");

    private void CreateSchemaThree()
    {
        using var connection = Open();
        foreach (var name in new[] { "001_create_dues.sql", "002_add_category.sql", "003_add_reminders.sql" })
        {
            using var reader = new StreamReader(typeof(DatabaseInitializer).Assembly
                .GetManifestResourceStream("DueDateChecker.Core.Data.Migrations." + name)!);
            using var command = connection.CreateCommand();
            command.CommandText = reader.ReadToEnd();
            command.ExecuteNonQuery();
        }
        using var version = connection.CreateCommand();
        version.CommandText = "PRAGMA user_version = 3";
        version.ExecuteNonQuery();
    }
}
