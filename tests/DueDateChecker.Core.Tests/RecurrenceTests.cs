using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>F07 recurring dues (specs/f07-recurrence RC01–RC05, RC08; ADR 009).</summary>
public sealed class RecurrenceTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"recurrence-{Guid.NewGuid():N}.db");

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    private static Due Bill(DateOnly date, RepeatRule repeat, DueCategory category = DueCategory.Bill, long centavos = 120000) =>
        Due.Create("Rent", Money.FromCentavos(centavos), date, Now, "Unit 4B", category, repeat);

    // ---- RC03: next dates ----

    [Theory]
    [InlineData("2026-11-05", "2026-11-12")]
    [InlineData("2026-12-29", "2027-01-05")]
    public void Weekly_adds_seven_days(string current, string expected)
    {
        Assert.Equal(DateOnly.Parse(expected), Recurrence.NextDate(DateOnly.Parse(current), RepeatRule.Weekly, null));
    }

    [Fact]
    public void Monthly_returns_to_the_anchor_after_a_short_month()
    {
        var dates = new List<DateOnly>();
        var date = new DateOnly(2027, 1, 31);
        for (var i = 0; i < 4; i++) { date = Recurrence.NextDate(date, RepeatRule.Monthly, 31)!.Value; dates.Add(date); }

        Assert.Equal(new[] { new DateOnly(2027, 2, 28), new DateOnly(2027, 3, 31), new DateOnly(2027, 4, 30), new DateOnly(2027, 5, 31) }, dates);
        Assert.Equal(new DateOnly(2028, 2, 29), Recurrence.NextDate(new DateOnly(2028, 1, 30), RepeatRule.Monthly, 30));
        Assert.Equal(new DateOnly(2027, 1, 15), Recurrence.NextDate(new DateOnly(2026, 12, 15), RepeatRule.Monthly, 15));
    }

    [Fact]
    public void Yearly_keeps_the_month_and_handles_29_February()
    {
        var date = new DateOnly(2028, 2, 29);
        var dates = new List<DateOnly>();
        for (var i = 0; i < 4; i++) { date = Recurrence.NextDate(date, RepeatRule.Yearly, 29)!.Value; dates.Add(date); }

        Assert.Equal(new[] { new DateOnly(2029, 2, 28), new DateOnly(2030, 2, 28), new DateOnly(2031, 2, 28), new DateOnly(2032, 2, 29) }, dates);
    }

    [Fact]
    public void No_next_date_beyond_the_calendar_or_without_a_repeat()
    {
        Assert.Null(Recurrence.NextDate(new DateOnly(9999, 12, 25), RepeatRule.Weekly, null));
        Assert.Equal(new DateOnly(9999, 12, 27), Recurrence.NextDate(new DateOnly(9999, 12, 20), RepeatRule.Weekly, null));
        Assert.Null(Recurrence.NextDate(new DateOnly(9999, 12, 15), RepeatRule.Monthly, 15));
        Assert.Null(Recurrence.NextDate(new DateOnly(9999, 3, 1), RepeatRule.Yearly, 1));
        Assert.Null(Recurrence.NextDate(new DateOnly(2026, 11, 5), RepeatRule.Never, null));
    }

    // ---- RC02: domain rules ----

    [Theory]
    [InlineData(DueCategory.Bill, RepeatRule.Monthly, 31)]
    [InlineData(DueCategory.Subscription, RepeatRule.Yearly, 31)]
    [InlineData(DueCategory.Bill, RepeatRule.Weekly, null)]
    [InlineData(DueCategory.Task, RepeatRule.Never, null)]
    public void Create_sets_the_anchor_only_for_monthly_and_yearly(DueCategory category, RepeatRule repeat, int? anchor)
    {
        var due = Due.Create("Item", Money.FromCentavos(100), new DateOnly(2026, 12, 31), Now, null, category, repeat);

        Assert.Equal(repeat, due.Repeat);
        Assert.Equal(anchor, due.RepeatDay);
        Assert.Null(due.RepeatSourceId);
    }

    [Theory]
    [InlineData(DueCategory.Task)]
    [InlineData(DueCategory.Other)]
    public void Task_and_other_cannot_repeat(DueCategory category)
    {
        Assert.Throws<ArgumentException>(() => Due.Create("Errand", null, new DateOnly(2026, 11, 20), Now, null, category, RepeatRule.Monthly));
        var bill = Bill(new DateOnly(2026, 11, 20), RepeatRule.Monthly);
        Assert.Throws<ArgumentException>((bill with { Category = category }).Validate);
    }

    [Fact]
    public void Anchor_must_match_the_repeat()
    {
        var bill = Bill(new DateOnly(2026, 11, 20), RepeatRule.Monthly);
        Assert.Throws<ArgumentException>((bill with { RepeatDay = null }).Validate);
        Assert.Throws<ArgumentException>((bill with { RepeatDay = 0 }).Validate);
        Assert.Throws<ArgumentException>((bill with { RepeatDay = 32 }).Validate);
        Assert.Throws<ArgumentException>((bill with { Repeat = RepeatRule.Weekly }).Validate); // weekly carries no anchor
        (bill with { Repeat = RepeatRule.Never, RepeatDay = null }).Validate();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Undefined_repeat_codes_are_rejected(int code)
    {
        var value = (RepeatRule)code;
        Assert.Throws<ArgumentOutOfRangeException>(() => Due.Create("Rent", Money.FromCentavos(1), new DateOnly(2026, 11, 20), Now, null, DueCategory.Bill, value));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bill(new DateOnly(2026, 11, 20), RepeatRule.Never) with { Repeat = value });
    }

    [Fact]
    public void Edit_keeps_the_stored_anchor_only_while_the_date_is_unchanged()
    {
        // The 28 Feb occurrence of a due on the 31st.
        var current = Bill(new DateOnly(2027, 1, 31), RepeatRule.Monthly) with { DueDate = new DateOnly(2027, 2, 28) };

        Assert.Equal(31, Recurrence.AnchorAfterEdit(current, RepeatRule.Monthly, new DateOnly(2027, 2, 28)));
        Assert.Equal(31, Recurrence.AnchorAfterEdit(current, RepeatRule.Yearly, new DateOnly(2027, 2, 28)));
        Assert.Equal(15, Recurrence.AnchorAfterEdit(current, RepeatRule.Monthly, new DateOnly(2027, 2, 15)));
        Assert.Null(Recurrence.AnchorAfterEdit(current, RepeatRule.Weekly, new DateOnly(2027, 2, 28)));
        Assert.Null(Recurrence.AnchorAfterEdit(current, RepeatRule.Never, new DateOnly(2027, 2, 28)));
        var weekly = current with { Repeat = RepeatRule.Weekly, RepeatDay = null };
        Assert.Equal(28, Recurrence.AnchorAfterEdit(weekly, RepeatRule.Monthly, new DateOnly(2027, 2, 28)));
    }

    [Fact]
    public void Only_an_unpaid_to_paid_save_of_a_repeating_due_creates_the_next_one()
    {
        var unpaid = Bill(new DateOnly(2026, 11, 20), RepeatRule.Weekly);
        var paid = unpaid with { IsPaid = true };

        Assert.Equal(new DateOnly(2026, 11, 27), Recurrence.NextOnPayment(unpaid, paid, Now)!.DueDate);
        Assert.Null(Recurrence.NextOnPayment(paid, paid, Now));
        Assert.Null(Recurrence.NextOnPayment(paid, unpaid, Now));
        Assert.Null(Recurrence.NextOnPayment(unpaid, unpaid with { Notes = "edited" }, Now));
        var never = unpaid with { Repeat = RepeatRule.Never };
        Assert.Null(Recurrence.NextOnPayment(never, never with { IsPaid = true }, Now));
    }

    [Fact]
    public void Next_occurrence_copies_the_due_and_links_its_source()
    {
        var paid = Bill(new DateOnly(2027, 1, 31), RepeatRule.Monthly) with { IsPaid = true };
        var later = Now.AddHours(3);

        var next = Recurrence.NextOccurrence(paid, later)!;

        Assert.NotEqual(paid.Id, next.Id);
        Assert.Equal(new DateOnly(2027, 2, 28), next.DueDate);
        Assert.Equal((paid.Title, paid.Amount, paid.Notes, paid.Category, paid.Repeat, paid.RepeatDay), (next.Title, next.Amount, next.Notes, next.Category, next.Repeat, next.RepeatDay));
        Assert.False(next.IsPaid);
        Assert.Equal(later, next.CreatedAt);
        Assert.Equal(later, next.UpdatedAt);
        Assert.Null(next.DeletedAt);
        Assert.Equal(paid.Id, next.RepeatSourceId);
        Assert.Null(Recurrence.NextOccurrence(Bill(new DateOnly(2027, 1, 31), RepeatRule.Never) with { IsPaid = true }, later));
    }

    // ---- RC01: migration 005 ----

    [Fact]
    public void Fresh_database_reaches_schema_five_with_the_recurrence_columns()
    {
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(5L, Scalar("PRAGMA user_version"));
        Assert.Equal("repeat_rule|INTEGER|1|0\nrepeat_day|INTEGER|0|NULL\nrepeat_source_id|TEXT|0|NULL",
            Snapshot("SELECT name, type, \"notnull\", dflt_value FROM pragma_table_info('dues') WHERE cid >= 12 ORDER BY cid"));
        Assert.Equal(1L, Scalar("SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'ix_dues_repeat_source'"));
    }

    [Fact]
    public void Upgrade_from_schema_four_keeps_rows_and_reads_them_as_never()
    {
        CreateSchema(4);
        Execute("""
            INSERT INTO dues (id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at, category)
            VALUES ('00000000-0000-0000-0000-000000000001', 'Active bill', 12345, '2026-11-20', 'ref', 0, '2026-10-01T00:00:00.0000000+00:00', '2026-10-01T00:00:00.0000000+00:00', NULL, 1),
                   ('00000000-0000-0000-0000-000000000002', 'Deleted task', NULL, '2026-10-15', NULL, 1, '2026-10-02T00:00:00.0000000+00:00', '2026-10-03T00:00:00.0000000+00:00', '2026-10-03T00:00:00.0000000+00:00', 2);
            """);
        const string Original = "SELECT rowid, id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category FROM dues ORDER BY rowid";
        var before = Snapshot(Original);

        new DatabaseInitializer(_path).Initialize();
        new DatabaseInitializer(_path).Initialize();

        Assert.Equal(5L, Scalar("PRAGMA user_version"));
        Assert.Equal(before, Snapshot(Original));
        Assert.Equal("0|NULL|NULL\n0|NULL|NULL", Snapshot("SELECT repeat_rule, repeat_day, repeat_source_id FROM dues ORDER BY rowid"));
        var due = new SqliteDueRepository(_path).GetAsync(Guid.Parse("00000000-0000-0000-0000-000000000001")).Result!;
        Assert.Equal(RepeatRule.Never, due.Repeat);
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET repeat_rule = 9"));
        Assert.Throws<SqliteException>(() => Execute("UPDATE dues SET repeat_day = 32"));
    }

    [Fact]
    public void Failure_after_the_columns_were_added_rolls_back_to_schema_four()
    {
        CreateSchema(4);
        // The index name is global: an index of that name elsewhere makes the last statement fail
        // after all three ALTER TABLE statements succeeded. This is also the regression test for the
        // migration runner: the first ALTER has a CHECK, after which ExecuteNonQuery dropped the
        // CREATE INDEX error and the migration committed as schema 5 without its index.
        Execute("CREATE TABLE blocker (x INTEGER); CREATE INDEX ix_dues_repeat_source ON blocker (x);");
        var columns = Snapshot("SELECT name FROM pragma_table_info('dues')");

        Assert.Throws<SqliteException>(() => new DatabaseInitializer(_path).Initialize());

        Assert.Equal(4L, Scalar("PRAGMA user_version"));
        Assert.Equal(columns, Snapshot("SELECT name FROM pragma_table_info('dues')"));
        Assert.Equal("blocker", Snapshot("SELECT tbl_name FROM sqlite_master WHERE name = 'ix_dues_repeat_source'"));
    }

    // ---- RC04, RC05, RC08: repository ----

    [Fact]
    public async Task Repeat_fields_round_trip()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Bill(new DateOnly(2026, 11, 30), RepeatRule.Monthly);
        await repository.AddAsync(due);

        Assert.Equal(due, await new SqliteDueRepository(_path).GetAsync(due.Id));
        var weekly = due with { Repeat = RepeatRule.Weekly, RepeatDay = null, UpdatedAt = Now.AddHours(1) };
        await repository.UpdateAsync(weekly);
        Assert.Equal(weekly, Assert.Single(await new SqliteDueRepository(_path).ListActiveAsync()));
    }

    [Fact]
    public async Task Paid_and_next_are_saved_together_and_only_once_per_source()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Bill(new DateOnly(2026, 11, 30), RepeatRule.Monthly);
        await repository.AddAsync(due);
        var paid = due with { IsPaid = true, UpdatedAt = Now.AddHours(1) };
        var next = Recurrence.NextOccurrence(paid, Now.AddHours(1))!;

        Assert.True(await repository.UpdateWithNextAsync(paid, next));

        var reopened = new SqliteDueRepository(_path);
        Assert.Equal(paid, await reopened.GetAsync(due.Id));
        Assert.Equal(next, await reopened.GetAsync(next.Id));
        Assert.Equal(new DateOnly(2026, 12, 30), next.DueDate);

        // Unpaid, then paid again: the guard refuses a second occurrence from the same source.
        await repository.UpdateAsync(paid with { IsPaid = false, UpdatedAt = Now.AddHours(2) });
        var again = Recurrence.NextOccurrence(paid, Now.AddHours(3))!;
        Assert.False(await repository.UpdateWithNextAsync(paid with { UpdatedAt = Now.AddHours(3) }, again));
        Assert.Null(await reopened.GetAsync(again.Id));
        Assert.True((await reopened.GetAsync(due.Id))!.IsPaid);

        // Even after the generated occurrence is deleted.
        await repository.SoftDeleteAsync(next.Id, Now.AddHours(4));
        var third = Recurrence.NextOccurrence(paid, Now.AddHours(5))!;
        Assert.False(await repository.UpdateWithNextAsync(paid with { UpdatedAt = Now.AddHours(5) }, third));
        Assert.Null(await reopened.GetAsync(third.Id));
        Assert.Equal(2, Convert.ToInt32(Scalar("SELECT COUNT(*) FROM dues")));
    }

    [Fact]
    public async Task A_failed_insert_also_undoes_the_paid_update()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Bill(new DateOnly(2026, 11, 30), RepeatRule.Monthly);
        var other = Bill(new DateOnly(2026, 12, 1), RepeatRule.Never) with { Title = "Other" };
        await repository.AddAsync(due);
        await repository.AddAsync(other);
        var paid = due with { IsPaid = true, UpdatedAt = Now.AddHours(1) };
        var clash = Recurrence.NextOccurrence(paid, Now.AddHours(1))! with { Id = other.Id }; // primary key clash

        await Assert.ThrowsAnyAsync<SqliteException>(() => repository.UpdateWithNextAsync(paid, clash));

        Assert.Equal(due, await new SqliteDueRepository(_path).GetAsync(due.Id));
        Assert.Equal(other, await repository.GetAsync(other.Id));
    }

    [Fact]
    public async Task A_deleted_due_is_not_marked_paid_and_gets_no_next()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Bill(new DateOnly(2026, 11, 30), RepeatRule.Monthly);
        await repository.AddAsync(due);
        await repository.SoftDeleteAsync(due.Id, Now.AddMinutes(5));
        var tombstone = await repository.GetAsync(due.Id);
        var paid = due with { IsPaid = true, UpdatedAt = Now.AddHours(1) };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateWithNextAsync(paid, Recurrence.NextOccurrence(paid, Now.AddHours(1))!));

        Assert.Equal(tombstone, await repository.GetAsync(due.Id));
        Assert.Equal(1L, Scalar("SELECT COUNT(*) FROM dues"));
    }

    [Fact]
    public async Task Update_never_rewrites_the_source_link()
    {
        new DatabaseInitializer(_path).Initialize();
        var repository = new SqliteDueRepository(_path);
        var due = Bill(new DateOnly(2026, 11, 30), RepeatRule.Monthly);
        await repository.AddAsync(due);
        var paid = due with { IsPaid = true, UpdatedAt = Now.AddHours(1) };
        var next = Recurrence.NextOccurrence(paid, Now.AddHours(1))!;
        await repository.UpdateWithNextAsync(paid, next);

        await repository.UpdateAsync(next with { RepeatSourceId = Guid.NewGuid(), Title = "Rent (Dec)", UpdatedAt = Now.AddHours(2) });

        var stored = await repository.GetAsync(next.Id);
        Assert.Equal(due.Id, stored!.RepeatSourceId);
        Assert.Equal("Rent (Dec)", stored.Title);
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

    private void CreateSchema(int version)
    {
        using var connection = Open();
        var names = new[] { "001_create_dues.sql", "002_add_category.sql", "003_add_reminders.sql", "004_optional_amounts.sql" }.Take(version);
        foreach (var name in names)
        {
            using var reader = new StreamReader(typeof(DatabaseInitializer).Assembly
                .GetManifestResourceStream("DueDateChecker.Core.Data.Migrations." + name)!);
            using var command = connection.CreateCommand();
            command.CommandText = reader.ReadToEnd();
            command.ExecuteNonQuery();
        }
        using var bump = connection.CreateCommand();
        bump.CommandText = $"PRAGMA user_version = {version}";
        bump.ExecuteNonQuery();
    }
}
