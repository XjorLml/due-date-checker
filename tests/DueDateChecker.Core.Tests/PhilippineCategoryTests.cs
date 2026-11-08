using System.Text.Json.Nodes;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

public sealed class PhilippineCategoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"ph-categories-{Guid.NewGuid():N}.db");
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 1, 0, 0, TimeSpan.Zero);
    private static Due Item(string categoryId = "ph.utilities.electricity", DueCategory type = DueCategory.Bill) =>
        Due.Create("Sample", Money.FromCentavos(0), new(2026, 11, 30), Now, "original note", type, RepeatRule.Never, categoryId);
    private SqliteConnection Open() { var c = new SqliteConnection($"Data Source={_path}"); c.Open(); return c; }
    private void Execute(string sql)
    {
        using var c = Open(); using var command = c.CreateCommand(); command.CommandText = sql;
        using var reader = command.ExecuteReader(); while (reader.NextResult()) { }
    }
    private object Scalar(string sql) { using var c = Open(); using var command = c.CreateCommand(); command.CommandText = sql; return command.ExecuteScalar()!; }
    private string Snapshot()
    {
        using var c = Open(); using var command = c.CreateCommand();
        command.CommandText = "SELECT rowid,id,title,amount_centavos,currency,due_date,notes,is_paid,timezone,created_at,updated_at,deleted_at,category,repeat_rule,repeat_day,repeat_source_id FROM dues ORDER BY rowid";
        using var reader = command.ExecuteReader(); var rows = new List<string>();
        while (reader.Read()) rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "<NULL>" : reader.GetValue(i).ToString())));
        return string.Join("\n", rows);
    }
    private void SchemaFive()
    {
        var assembly = typeof(DatabaseInitializer).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.Contains(".Migrations.") && n.EndsWith(".sql"))
                     .Where(n => int.Parse(n.Split(".Migrations.")[1][..3]) <= 5).Order())
        {
            using var reader = new StreamReader(assembly.GetManifestResourceStream(name)!);
            Execute(reader.ReadToEnd());
        }
        Execute("PRAGMA user_version=5;");
    }

    [Fact]
    public void Catalog_has_53_stable_unique_ids_in_10_groups_and_all_are_valid()
    {
        Assert.Equal(53, PhilippineCategories.All.Count);
        Assert.Equal(53, PhilippineCategories.All.Select(x => x.Id).Distinct().Count());
        Assert.Equal(10, PhilippineCategories.All.Select(x => x.GroupId).Distinct().Count());
        foreach (var entry in PhilippineCategories.All)
        {
            Assert.StartsWith("ph."+entry.GroupId+".", entry.Id);
            Assert.Equal(entry.Id, Item(entry.Id).CategoryId);
            Assert.False(string.IsNullOrWhiteSpace(entry.Name));
        }
        Assert.Equal("Real Property Tax (Amilyar)", PhilippineCategories.Get("ph.housing.amilyar").Name);
        Assert.Equal("Pag-IBIG MP2 Savings", PhilippineCategories.Get("ph.government.mp2").Name);
    }

    [Theory]
    [InlineData("KURYENTE", "ph.utilities.electricity")]
    [InlineData("tubig", "ph.utilities.water")]
    [InlineData("  SSS  ", "ph.government.sss")]
    [InlineData("pagibig mp2", "ph.government.mp2")]
    [InlineData("renta", "ph.housing.rent")]
    public void Search_matches_names_groups_and_local_aliases(string query, string expected) =>
        Assert.Contains(PhilippineCategories.Search(query), x => x.Id == expected);

    [Fact]
    public void Invalid_ids_are_rejected_and_category_never_changes_type_policy()
    {
        foreach (var invalid in new[] { "", "other", "ph.unknown.item", "PH.UTILITIES.WATER", "ph.utilities.water " })
        {
            Assert.Throws<ArgumentException>(() => Item(invalid));
            Assert.Throws<ArgumentException>(() => Item() with { CategoryId = invalid });
        }
        var task = Due.Create("Task", null, new(2026, 11, 8), Now, category: DueCategory.Task, categoryId: "ph.credit.card");
        Assert.Equal(DueCategory.Task, task.Category); Assert.Null(task.Amount);
        Assert.Throws<ArgumentException>(() => Due.Create("Bill", null, new(2026, 11, 8), Now, category: DueCategory.Bill,
            categoryId: "ph.family.personal"));
    }

    [Fact]
    public async Task Upgrade_preserves_schema_five_fields_tombstones_and_defaults_without_guessing()
    {
        SchemaFive();
        Execute("""
            INSERT INTO dues(id,title,amount_centavos,due_date,notes,is_paid,created_at,updated_at,deleted_at,category,repeat_rule,repeat_day)
            VALUES('00000000-0000-0000-0000-000000000001','Electricity',0,'2026-11-30','keep',1,'2026-11-01T00:00:00.0000000+00:00','2026-11-02T00:00:00.0000000+00:00',NULL,1,2,30),
                  ('00000000-0000-0000-0000-000000000002','Task',NULL,'2026-11-08',NULL,0,'2026-11-01T00:00:00.0000000+00:00','2026-11-02T00:00:00.0000000+00:00','2026-11-02T00:00:00.0000000+00:00',2,0,NULL);
            """);
        var before = Snapshot();
        new DatabaseInitializer(_path).Initialize(); new DatabaseInitializer(_path).Initialize();
        Assert.Equal(6L, Scalar("PRAGMA user_version")); Assert.Equal(before, Snapshot());
        var records = await new SqliteDueArchive(_path).ReadAllAsync();
        Assert.Equal(2, records.Count); Assert.All(records, item => Assert.Equal(PhilippineCategories.OtherId, item.CategoryId));
        Assert.Null(records[1].AmountCentavos); Assert.True(records[1].IsDeleted);
        Assert.Equal(PhilippineCategories.OtherId, (await new SqliteDueRepository(_path).GetAsync(Guid.Parse(records[0].Id)))!.CategoryId);
    }

    [Fact]
    public void Failed_migration_preserves_version_and_newer_database_is_never_reset()
    {
        SchemaFive(); Execute("ALTER TABLE dues ADD COLUMN category_id TEXT NOT NULL DEFAULT 'ph.other.general'");
        Assert.Throws<SqliteException>(() => new DatabaseInitializer(_path).Initialize());
        Assert.Equal(5L, Scalar("PRAGMA user_version"));
        Execute("PRAGMA user_version=7");
        Assert.Throws<InvalidOperationException>(() => new DatabaseInitializer(_path).Initialize());
        Assert.Equal(7L, Scalar("PRAGMA user_version"));
    }

    [Fact]
    public async Task Category_roundtrips_through_edits_deletion_restore_and_recurrence()
    {
        new DatabaseInitializer(_path).Initialize(); var repository = new SqliteDueRepository(_path);
        var due = Item() with { Repeat = RepeatRule.Monthly, RepeatDay = 30 };
        await repository.AddAsync(due); Assert.Equal(due, await repository.GetAsync(due.Id));
        var edited = due with { CategoryId = "ph.government.sss", IsPaid = true, UpdatedAt = Now.AddMinutes(1) };
        var next = Recurrence.NextOnPayment(due, edited, edited.UpdatedAt)!;
        Assert.Equal(edited.CategoryId, next.CategoryId);
        Assert.True(await repository.UpdateWithNextAsync(edited, next));
        Assert.Equal(edited.CategoryId, (await repository.GetAsync(next.Id))!.CategoryId);
        await repository.SoftDeleteAsync(due.Id, Now.AddMinutes(2));
        Assert.Equal(edited.CategoryId, (await repository.GetAsync(due.Id))!.CategoryId);
        Assert.True(await repository.RestoreAsync(due.Id, Now.AddMinutes(2), Now.AddMinutes(3)));
        Assert.Equal(edited.CategoryId, (await repository.GetAsync(due.Id))!.CategoryId);
    }

    [Fact]
    public async Task Format_two_preserves_categories_and_format_one_defaults_to_other()
    {
        new DatabaseInitializer(_path).Initialize(); var repository = new SqliteDueRepository(_path); await repository.AddAsync(Item());
        var archive = new SqliteDueArchive(_path); var records = await archive.ReadAllAsync();
        var bytes = DueArchive.Write(records, Now); var root = JsonNode.Parse(bytes)!.AsObject();
        Assert.Equal(2, (int)root["version"]!); Assert.Equal(records, DueArchive.Read(bytes).Dues);
        root["version"] = 1;
        foreach (var record in root["dues"]!.AsArray()) record!.AsObject().Remove("categoryId");
        Assert.Equal(PhilippineCategories.OtherId, Assert.Single(DueArchive.Read(System.Text.Encoding.UTF8.GetBytes(root.ToJsonString())).Dues).CategoryId);
        await repository.SoftDeleteAsync(Guid.Parse(records.Single().Id), Now);
        await archive.ImportAsync(records);
        Assert.Equal("ph.utilities.electricity", (await repository.GetAsync(Guid.Parse(records.Single().Id)))!.CategoryId);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("null")]
    [InlineData("ph.unknown.item")]
    public async Task Format_two_rejects_missing_or_invalid_ids(string change)
    {
        new DatabaseInitializer(_path).Initialize(); await new SqliteDueRepository(_path).AddAsync(Item());
        var root = JsonNode.Parse(DueArchive.Write(await new SqliteDueArchive(_path).ReadAllAsync(), Now))!;
        var record = root["dues"]![0]!.AsObject();
        if (change == "missing") record.Remove("categoryId"); else record["categoryId"] = change == "null" ? null : change;
        var error = Assert.Throws<DueArchiveException>(() => DueArchive.Read(System.Text.Encoding.UTF8.GetBytes(root.ToJsonString())));
        Assert.Equal("categoryId", error.Field);
    }

    [Fact]
    public void Category_edit_at_same_timestamp_changes_notification_fingerprint()
    {
        var due = Item();
        Assert.NotEqual(ReminderText.Fingerprint(due), ReminderText.Fingerprint(due with { CategoryId = "ph.utilities.water" }));
    }

    public void Dispose() { using var c = new SqliteConnection($"Data Source={_path}"); SqliteConnection.ClearPool(c); File.Delete(_path); }
}
