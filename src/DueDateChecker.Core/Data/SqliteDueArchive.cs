using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Data;

/// <summary>Storage side of UX05 export/import. The App runs these calls inside its database gate.</summary>
public interface IDueArchiveStore
{
    /// <summary>Every stored row, deleted ones included, in insertion order.</summary>
    Task<IReadOnlyList<DueRecord>> ReadAllAsync();

    /// <summary>How many dues currently show (not deleted).</summary>
    Task<int> CountActiveAsync();

    /// <summary>
    /// Writes <paramref name="dues"/> in one transaction, only while no due shows. A record whose id
    /// matches a hidden deleted row replaces it (user decision, 2026-11-06). Throws
    /// <see cref="DueArchiveException"/> (nothing written) for invalid records or a non-empty app.
    /// </summary>
    Task ImportAsync(IReadOnlyList<DueRecord> dues);
}

public sealed class SqliteDueArchive(string databasePath) : IDueArchiveStore
{
    private const string Columns =
        "id, title, amount_centavos, currency, due_date, notes, is_paid, timezone, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id, category_id";

    private const string Values =
        "@id, @title, @amount, @currency, @due_date, @notes, @is_paid, @timezone, @created_at, @updated_at, @deleted_at, @category, @repeat_rule, @repeat_day, @repeat_source_id, @category_id";

    // The WHERE lets a conflicting id replace only a deleted row; an active one leaves 0 rows changed.
    private const string UpsertSql =
        $"""
        INSERT INTO dues ({Columns}) VALUES ({Values})
        ON CONFLICT (id) DO UPDATE SET
            title = excluded.title, amount_centavos = excluded.amount_centavos, currency = excluded.currency,
            due_date = excluded.due_date, notes = excluded.notes, is_paid = excluded.is_paid, timezone = excluded.timezone,
            created_at = excluded.created_at, updated_at = excluded.updated_at, deleted_at = excluded.deleted_at,
            category = excluded.category, repeat_rule = excluded.repeat_rule, repeat_day = excluded.repeat_day,
            repeat_source_id = excluded.repeat_source_id, category_id = excluded.category_id
        WHERE dues.deleted_at IS NOT NULL
        """;

    public async Task<IReadOnlyList<DueRecord>> ReadAllAsync()
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {Columns} FROM dues ORDER BY rowid";
        using var reader = await command.ExecuteReaderAsync();
        var records = new List<DueRecord>();
        while (await reader.ReadAsync())
        {
            records.Add(new DueRecord(
                Id: reader.GetString(0),
                Title: reader.GetString(1),
                AmountCentavos: reader.IsDBNull(2) ? null : reader.GetInt64(2),
                Currency: reader.GetString(3),
                DueDate: reader.GetString(4),
                Notes: reader.IsDBNull(5) ? null : reader.GetString(5),
                IsPaid: reader.GetInt64(6) != 0,
                Timezone: reader.GetString(7),
                CreatedAt: reader.GetString(8),
                UpdatedAt: reader.GetString(9),
                DeletedAt: reader.IsDBNull(10) ? null : reader.GetString(10),
                Category: checked((int)reader.GetInt64(11)),
                RepeatRule: checked((int)reader.GetInt64(12)),
                RepeatDay: reader.IsDBNull(13) ? null : checked((int)reader.GetInt64(13)),
                RepeatSourceId: reader.IsDBNull(14) ? null : reader.GetString(14),
                CategoryId: reader.GetString(15)));
        }
        return records;
    }

    public async Task<int> CountActiveAsync()
    {
        using var connection = SqliteConnections.Open(databasePath);
        return await CountActiveAsync(connection, transaction: null);
    }

    public async Task ImportAsync(IReadOnlyList<DueRecord> dues)
    {
        var records = DueArchive.Validate(dues);
        using var connection = SqliteConnections.Open(databasePath);
        // Microsoft.Data.Sqlite begins IMMEDIATE transactions, so the check below and the writes see the same data.
        using var transaction = connection.BeginTransaction();
        if (await CountActiveAsync(connection, transaction) > 0) throw new DueArchiveException(DueArchiveProblem.AppNotEmpty);

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = UpsertSql;
        foreach (var name in Values.Split(", "))
        {
            insert.Parameters.Add(new SqliteParameter(name, null));
        }

        foreach (var due in records)
        {
            Set(insert, "@id", due.Id);
            Set(insert, "@title", due.Title);
            Set(insert, "@amount", due.AmountCentavos);
            Set(insert, "@currency", due.Currency);
            Set(insert, "@due_date", due.DueDate);
            Set(insert, "@notes", due.Notes);
            Set(insert, "@is_paid", due.IsPaid ? 1 : 0);
            Set(insert, "@timezone", due.Timezone);
            Set(insert, "@created_at", due.CreatedAt);
            Set(insert, "@updated_at", due.UpdatedAt);
            Set(insert, "@deleted_at", due.DeletedAt);
            Set(insert, "@category", due.Category);
            Set(insert, "@category_id", due.CategoryId);
            Set(insert, "@repeat_rule", due.RepeatRule);
            Set(insert, "@repeat_day", due.RepeatDay);
            Set(insert, "@repeat_source_id", due.RepeatSourceId);
            // Disposing the transaction without Commit rolls back every row written so far.
            if (await insert.ExecuteNonQueryAsync() != 1) throw new DueArchiveException(DueArchiveProblem.AppNotEmpty);
        }

        await transaction.CommitAsync();
    }

    private static async Task<int> CountActiveAsync(SqliteConnection connection, SqliteTransaction? transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM dues WHERE deleted_at IS NULL";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static void Set(SqliteCommand command, string name, object? value) => command.Parameters[name].Value = value ?? DBNull.Value;
}
