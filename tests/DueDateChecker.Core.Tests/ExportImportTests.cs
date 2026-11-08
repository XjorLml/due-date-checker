using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>UX05 export and import (specs/f04-export-import EX01–EX07, ADR 010).</summary>
public sealed class ExportImportTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private readonly string _source = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}.db");
    private readonly string _target = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid():N}.db");

    // Every stored column, so snapshots compare whole rows.
    private const string AllColumns =
        "SELECT id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id FROM dues ORDER BY id";

    // Two valid dues: a monthly Bill and a Task without an amount.
    private const string ValidFile = """
        {
          "format": "duewise.dues",
          "version": 1,
          "exportedAt": "2026-11-06T01:00:00.0000000+00:00",
          "dues": [
            { "id": "00000000-0000-0000-0000-000000000001", "title": "Rent", "amountCentavos": 1500000, "currency": "PHP",
              "dueDate": "2026-11-30", "notes": "Unit 4B", "isPaid": false, "timezone": "Asia/Manila",
              "createdAt": "2026-11-01T02:00:00.0000000+00:00", "updatedAt": "2026-11-01T02:00:00.0000000+00:00", "deletedAt": null,
              "category": 1, "repeatRule": 2, "repeatDay": 30, "repeatSourceId": null },
            { "id": "00000000-0000-0000-0000-000000000002", "title": "Renew ID", "amountCentavos": null, "currency": "PHP",
              "dueDate": "2026-12-15", "notes": null, "isPaid": false, "timezone": "Asia/Manila",
              "createdAt": "2026-11-02T02:00:00.0000000+00:00", "updatedAt": "2026-11-02T02:00:00.0000000+00:00", "deletedAt": null,
              "category": 2, "repeatRule": 0, "repeatDay": null, "repeatSourceId": null }
          ]
        }
        """;

    private bool _created;

    // Created on first use: most file-format tests never touch a database.
    private string Source => Ready(_source);
    private string Target => Ready(_target);

    private string Ready(string path)
    {
        if (!_created)
        {
            new DatabaseInitializer(_source).Initialize();
            new DatabaseInitializer(_target).Initialize();
            _created = true;
        }
        return path;
    }

    public void Dispose()
    {
        if (!_created) return;
        foreach (var path in new[] { _source, _target })
        {
            TestDatabase.ClearPool(path);
            File.Delete(path);
        }
    }

    private void SeedSource()
    {
        Execute(Source, """
            INSERT INTO dues (id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id) VALUES
            ('10000000-0000-0000-0000-000000000001', 'Rent', 1500000, 'PHP', '2027-01-31', 'Unit 4B', 1, 'Asia/Manila', '2026-11-01T02:00:00.0000000+00:00', '2026-11-05T02:00:00.0000000+00:00', NULL, 1, 2, 31, NULL),
            ('10000000-0000-0000-0000-000000000002', 'Rent', 1500000, 'PHP', '2027-02-28', 'Unit 4B', 0, 'Asia/Manila', '2026-11-05T02:00:00.0000000+00:00', '2026-11-05T02:00:00.0000000+00:00', NULL, 1, 2, 31, '10000000-0000-0000-0000-000000000001'),
            ('10000000-0000-0000-0000-000000000003', 'Renew ID', NULL, 'PHP', '2026-12-15', NULL, 0, 'Asia/Manila', '2026-11-02T02:00:00.0000000+00:00', '2026-11-02T02:00:00.0000000+00:00', NULL, 2, 0, NULL, NULL),
            ('10000000-0000-0000-0000-000000000004', 'Streaming', 0, 'PHP', '2026-11-10', 'Free month', 1, 'Asia/Manila', '2026-10-01T12:00:00.0000000+08:00', '2026-10-02T12:00:00.0000000+08:00', NULL, 3, 0, NULL, NULL),
            ('10000000-0000-0000-0000-000000000005', 'Old gym "plan"', 99900, 'USD', '2026-10-15', 'gone, with ñ and ₱', 0, 'UTC', '2026-10-05T00:00:00.0000000+00:00', '2026-10-06T00:00:00.0000000+00:00', '2026-10-06T00:00:00.0000000+00:00', 0, 0, NULL, NULL);
            """);
    }

    // ---- EX01, EX02, EX07: export and round trip ----

    [Fact]
    public async Task Export_then_import_into_an_empty_app_reproduces_every_row()
    {
        SeedSource();
        var before = Snapshot(Source, AllColumns);

        var file = DueArchive.Write(await new SqliteDueArchive(Source).ReadAllAsync(), Now);
        var contents = DueArchive.Read(file);
        await new SqliteDueArchive(Target).ImportAsync(contents.Dues);

        Assert.Equal(Now, contents.ExportedAt);
        Assert.Equal((5, 4, 1), (contents.Dues.Count, contents.ActiveCount, contents.DeletedCount));
        Assert.Equal(before, Snapshot(Target, AllColumns));
        Assert.Equal(await new SqliteDueRepository(Source).ListActiveAsync(), await new SqliteDueRepository(Target).ListActiveAsync());
    }

    [Fact]
    public async Task The_file_has_the_documented_shape()
    {
        SeedSource();

        using var json = JsonDocument.Parse(DueArchive.Write(await new SqliteDueArchive(Source).ReadAllAsync(), Now));

        var root = json.RootElement;
        Assert.Equal(new[] { "format", "version", "exportedAt", "dues" }, root.EnumerateObject().Select(p => p.Name));
        Assert.Equal("duewise.dues", root.GetProperty("format").GetString());
        Assert.Equal(2, root.GetProperty("version").GetInt32());
        Assert.Equal("2026-11-06T01:00:00.0000000+00:00", root.GetProperty("exportedAt").GetString());
        var dues = root.GetProperty("dues").EnumerateArray().ToArray();
        Assert.Equal(5, dues.Length);
        Assert.Equal(new[] { "id", "title", "amountCentavos", "currency", "dueDate", "notes", "isPaid", "timezone", "createdAt",
            "updatedAt", "deletedAt", "category", "repeatRule", "repeatDay", "repeatSourceId", "categoryId" }, dues[0].EnumerateObject().Select(p => p.Name));
        var task = dues.Single(d => d.GetProperty("title").GetString() == "Renew ID");
        Assert.Equal(JsonValueKind.Null, task.GetProperty("amountCentavos").ValueKind); // no amount stays absent
        Assert.Equal(0, dues.Single(d => d.GetProperty("title").GetString() == "Streaming").GetProperty("amountCentavos").GetInt64());
        Assert.Equal("2026-10-06T00:00:00.0000000+00:00", dues.Single(d => d.GetProperty("currency").GetString() == "USD").GetProperty("deletedAt").GetString());
        Assert.Contains("gone, with ñ and ₱", Encoding.UTF8.GetString(DueArchive.Write(await new SqliteDueArchive(Source).ReadAllAsync(), Now)));
    }

    [Fact]
    public async Task Count_active_ignores_deleted_dues()
    {
        SeedSource();
        Assert.Equal(4, await new SqliteDueArchive(Source).CountActiveAsync());
        Assert.Equal(0, await new SqliteDueArchive(Target).CountActiveAsync());
    }

    // ---- EX03: refusals before any write ----

    public static TheoryData<string> NotExports => new()
    {
        "not json at all",
        "[]",
        "{}",
        """{ "format": "other.app", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
        """{ "format": "duewise.dues", "version": 0, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
        """{ "format": "duewise.dues", "version": "1", "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
        """{ "format": "duewise.dues", "version": 1.5, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
        """{ "format": "duewise.dues", "version": 1, "dues": [] }""",
        """{ "format": "duewise.dues", "version": 1, "exportedAt": "yesterday", "dues": [] }""",
        """{ "format": "duewise.dues", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": {} }""",
        """{ "format": "duewise.dues", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [], "extra": true }""",
        """{ "format": "duewise.dues", "format": "duewise.dues", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
        """{ "format": "duewise.dues", /* comment */ "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""",
    };

    [Theory]
    [MemberData(nameof(NotExports))]
    public void Files_that_are_not_duewise_exports_are_refused(string text)
    {
        var problem = Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(text)));
        Assert.Equal(DueArchiveProblem.NotAnExport, problem.Problem);
    }

    [Fact]
    public void Invalid_utf8_is_not_an_export()
    {
        var bytes = Encoding.UTF8.GetBytes(ValidFile.Replace("Renew ID", "Renew é"));
        var at = Array.IndexOf(bytes, (byte)0xC3);
        bytes[at] = 0xFF; // breaks the two-byte sequence

        Assert.Equal(DueArchiveProblem.NotAnExport, Assert.Throws<DueArchiveException>(() => DueArchive.Read(bytes)).Problem);
    }

    [Fact]
    public void A_newer_version_is_refused_before_its_contents_are_read()
    {
        var text = """{ "format": "duewise.dues", "version": 3, "somethingNew": [1, 2, 3] }""";
        Assert.Equal(DueArchiveProblem.NewerVersion, Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(text))).Problem);
    }

    [Fact]
    public void A_file_without_dues_or_over_the_size_limit_is_refused()
    {
        var empty = """{ "format": "duewise.dues", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""";
        Assert.Equal(DueArchiveProblem.NoDues, Assert.Throws<DueArchiveException>(() => DueArchive.Read(Encoding.UTF8.GetBytes(empty))).Problem);

        var huge = new byte[DueArchive.MaxBytes + 1];
        Assert.Equal(DueArchiveProblem.TooLarge, Assert.Throws<DueArchiveException>(() => DueArchive.Read(huge)).Problem);
    }

    [Fact]
    public void A_byte_order_mark_is_accepted()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(ValidFile)).ToArray();
        Assert.Equal(2, DueArchive.Read(bytes).Dues.Count);
    }

    public static TheoryData<string, Action<JsonObject>> InvalidSecondDue => new()
    {
        { "id", due => due["id"] = "not-a-guid" },
        { "id", due => due["id"] = Guid.Empty.ToString() },
        { "title", due => due["title"] = "   " },
        { "title", due => due["title"] = null },
        { "amountCentavos", due => due["amountCentavos"] = -1 },
        { "amountCentavos", due => due["amountCentavos"] = 1.5 },
        { "amountCentavos", due => { due["category"] = 1; due["amountCentavos"] = null; } }, // Bill needs an amount
        { "currency", due => due["currency"] = "" },
        { "dueDate", due => due["dueDate"] = "2026-13-01" },
        { "dueDate", due => due["dueDate"] = "15 Dec 2026" },
        { "isPaid", due => due["isPaid"] = "yes" },
        { "timezone", due => due["timezone"] = null },
        { "createdAt", due => due["createdAt"] = "yesterday" },
        { "updatedAt", due => due["updatedAt"] = 5 },
        { "deletedAt", due => due["deletedAt"] = "2026-11-02" },
        { "category", due => due["category"] = 7 },
        { "repeatRule", due => due["repeatRule"] = 9 },
        { "repeatRule", due => { due["repeatRule"] = 2; due["repeatDay"] = 15; } }, // a Task cannot repeat
        { "repeatDay", due => { due["category"] = 1; due["amountCentavos"] = 100; due["repeatRule"] = 2; due["repeatDay"] = null; } },
        { "repeatDay", due => due["repeatDay"] = 5 }, // Never carries no anchor
        { "repeatSourceId", due => due["repeatSourceId"] = "nope" },
        { "surprise", due => due["surprise"] = true }, // unknown field
        { "notes", due => due.Remove("notes") }, // missing field
    };

    [Theory]
    [MemberData(nameof(InvalidSecondDue))]
    public void An_invalid_due_is_refused_with_its_position_and_field(string field, Action<JsonObject> spoil)
    {
        var problem = Assert.Throws<DueArchiveException>(() => DueArchive.Read(Spoil(spoil)));

        Assert.Equal(DueArchiveProblem.InvalidDue, problem.Problem);
        Assert.Equal(2, problem.DueNumber);
        Assert.Equal(field, problem.Field);
    }

    [Fact]
    public void A_repeated_id_is_refused()
    {
        var problem = Assert.Throws<DueArchiveException>(() => DueArchive.Read(Spoil(due => due["id"] = "00000000-0000-0000-0000-000000000001")));
        Assert.Equal((DueArchiveProblem.DuplicateId, (int?)2), (problem.Problem, problem.DueNumber));
    }

    [Fact]
    public async Task Ids_are_stored_in_the_apps_own_form()
    {
        var upper = Spoil(due => { due["id"] = "ABCDEF00-0000-0000-0000-0000000000AB"; due["title"] = "  Renew ID  "; });

        await new SqliteDueArchive(Target).ImportAsync(DueArchive.Read(upper).Dues);

        var due = await new SqliteDueRepository(Target).GetAsync(Guid.Parse("abcdef00-0000-0000-0000-0000000000ab"));
        Assert.Equal("Renew ID", due!.Title);
        Assert.Equal("abcdef00-0000-0000-0000-0000000000ab", Snapshot(Target, "SELECT id FROM dues WHERE title = 'Renew ID'"));
    }

    // ---- EX04, EX06: the empty rule and the single transaction ----

    [Fact]
    public async Task Import_is_refused_while_any_due_shows_and_writes_nothing()
    {
        await new SqliteDueRepository(Target).AddAsync(Due.Create("Water", Money.FromCentavos(85000), new DateOnly(2026, 11, 20), Now));
        var before = Snapshot(Target, AllColumns);

        var problem = await Assert.ThrowsAsync<DueArchiveException>(() => new SqliteDueArchive(Target).ImportAsync(DueArchive.Read(Utf8(ValidFile)).Dues));

        Assert.Equal(DueArchiveProblem.AppNotEmpty, problem.Problem);
        Assert.Equal(before, Snapshot(Target, AllColumns));
    }

    [Fact]
    public async Task Hidden_deleted_dues_stay_and_a_matching_one_is_replaced_by_the_file()
    {
        Execute(Target, """
            INSERT INTO dues (id, title, amount_centavos, due_date, created_at, updated_at, deleted_at, category) VALUES
            ('00000000-0000-0000-0000-000000000001', 'Old rent', 100, '2026-01-01', '2026-01-01T00:00:00.0000000+00:00', '2026-01-02T00:00:00.0000000+00:00', '2026-01-02T00:00:00.0000000+00:00', 1),
            ('00000000-0000-0000-0000-0000000000ff', 'Kept tombstone', 200, '2026-02-01', '2026-02-01T00:00:00.0000000+00:00', '2026-02-02T00:00:00.0000000+00:00', '2026-02-02T00:00:00.0000000+00:00', 0);
            """);
        var kept = Snapshot(Target, AllColumns + " LIMIT -1 OFFSET 1");

        await new SqliteDueArchive(Target).ImportAsync(DueArchive.Read(Utf8(ValidFile)).Dues);

        Assert.Equal(3L, Scalar(Target, "SELECT COUNT(*) FROM dues"));
        Assert.Contains(kept, Snapshot(Target, AllColumns));
        var replaced = (await new SqliteDueRepository(Target).GetAsync(Guid.Parse("00000000-0000-0000-0000-000000000001")))!;
        Assert.Equal(("Rent", (DateTimeOffset?)null, RepeatRule.Monthly, (int?)30), (replaced.Title, replaced.DeletedAt, replaced.Repeat, replaced.RepeatDay));
        Assert.Equal(2, (await new SqliteDueRepository(Target).ListActiveAsync()).Count);
    }

    [Fact]
    public async Task A_failure_part_way_through_writes_nothing()
    {
        // The second due's insert fails after the first succeeded inside the transaction.
        Execute(Target, "CREATE TRIGGER fail_import BEFORE INSERT ON dues WHEN NEW.title = 'Renew ID' BEGIN SELECT RAISE(ABORT, 'controlled failure'); END;");

        await Assert.ThrowsAsync<SqliteException>(() => new SqliteDueArchive(Target).ImportAsync(DueArchive.Read(Utf8(ValidFile)).Dues));

        Assert.Equal(0L, Scalar(Target, "SELECT COUNT(*) FROM dues"));
    }

    [Fact]
    public async Task Import_checks_records_again_before_writing()
    {
        var valid = DueArchive.Read(Utf8(ValidFile)).Dues;
        var broken = new[] { valid[0], valid[1] with { Title = "" } };

        var problem = await Assert.ThrowsAsync<DueArchiveException>(() => new SqliteDueArchive(Target).ImportAsync(broken));

        Assert.Equal((DueArchiveProblem.InvalidDue, (int?)2, "title"), (problem.Problem, problem.DueNumber, problem.Field));
        Assert.Equal(0L, Scalar(Target, "SELECT COUNT(*) FROM dues"));
    }

    // ---- helpers ----

    private static byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text);

    private static byte[] Spoil(Action<JsonObject> spoil)
    {
        var file = JsonNode.Parse(ValidFile)!.AsObject();
        spoil(file["dues"]![1]!.AsObject());
        return Utf8(file.ToJsonString());
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString());
        connection.Open();
        return connection;
    }

    private static void Execute(string path, string sql)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static object? Scalar(string path, string sql)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    private static string Snapshot(string path, string sql)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var rows = new List<string>();
        while (reader.Read())
            rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "NULL" : reader.GetValue(i))));
        return string.Join("\n", rows);
    }
}
