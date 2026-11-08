using System.Globalization;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Data;

/// <summary>
/// Raw-SQL implementation of <see cref="IDueRepository"/>. Every value travels
/// as an @parameter, never concatenated into the SQL text. One short-lived
/// connection per call: Microsoft.Data.Sqlite pools them, and SQLite itself is
/// synchronous underneath the async surface.
/// </summary>
public sealed class SqliteDueRepository(string databasePath) : IDueRepository
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimestampFormat = "O";

    private const string SelectColumns =
        "SELECT id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id FROM dues";

    private const string InsertColumns =
        "id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at, category, repeat_rule, repeat_day, repeat_source_id";

    private const string InsertValues =
        "@id, @title, @amount, @due_date, @notes, @is_paid, @created_at, @updated_at, @deleted_at, @category, @repeat_rule, @repeat_day, @repeat_source_id";

    // repeat_source_id is deliberately absent: where an occurrence came from never changes.
    private const string UpdateSql =
        """
        UPDATE dues
        SET title = @title, amount_centavos = @amount, due_date = @due_date, notes = @notes,
            is_paid = @is_paid, updated_at = @updated_at, category = @category,
            repeat_rule = @repeat_rule, repeat_day = @repeat_day
        WHERE id = @id AND deleted_at IS NULL
        """;

    public async Task<IReadOnlyList<Due>> ListActiveAsync()
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE deleted_at IS NULL ORDER BY due_date, created_at";

        using var reader = await command.ExecuteReaderAsync();
        var dues = new List<Due>();
        while (await reader.ReadAsync())
        {
            dues.Add(Map(reader));
        }

        return dues;
    }

    public async Task<Due?> GetAsync(Guid id)
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task AddAsync(Due due)
    {
        // The table does not enforce the category/amount/repeat rules (they are app-enforced, like the
        // other value rules), so a copied record is checked before any SQL runs.
        due.Validate();
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO dues ({InsertColumns}) VALUES ({InsertValues})";
        AddInsertParameters(command, due);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Due due)
    {
        due.Validate();
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = UpdateSql;
        AddEditableParameters(command, due);

        var rows = await command.ExecuteNonQueryAsync();
        if (rows == 0)
        {
            throw new KeyNotFoundException($"Due {due.Id} does not exist or has been deleted.");
        }
    }

    /// <summary>
    /// ADR 009: the paid update and the next occurrence share one transaction, so a failure writes
    /// neither. The insert is skipped when any row (active or deleted) already came from the same
    /// source, which makes paid → unpaid → paid harmless.
    /// </summary>
    public async Task<bool> UpdateWithNextAsync(Due updated, Due next)
    {
        updated.Validate();
        next.Validate();
        if (next.RepeatSourceId != updated.Id)
        {
            throw new ArgumentException("The next occurrence must be linked to the due it follows.", nameof(next));
        }

        using var connection = SqliteConnections.Open(databasePath);
        using var transaction = connection.BeginTransaction();

        using (var update = connection.CreateCommand())
        {
            update.Transaction = transaction;
            update.CommandText = UpdateSql;
            AddEditableParameters(update, updated);
            if (await update.ExecuteNonQueryAsync() == 0)
            {
                // Disposing the transaction without Commit rolls it back.
                throw new KeyNotFoundException($"Due {updated.Id} does not exist or has been deleted.");
            }
        }

        int inserted;
        using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText =
                $"""
                INSERT INTO dues ({InsertColumns})
                SELECT {InsertValues}
                WHERE NOT EXISTS (SELECT 1 FROM dues WHERE repeat_source_id = @repeat_source_id)
                """;
            AddInsertParameters(insert, next);
            inserted = await insert.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return inserted == 1;
    }

    public async Task SoftDeleteAsync(Guid id, DateTimeOffset when)
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dues SET deleted_at = @when, updated_at = @when WHERE id = @id AND deleted_at IS NULL";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@when", when.ToString(TimestampFormat, CultureInfo.InvariantCulture));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> RestoreAsync(Guid id, DateTimeOffset deletedAt, DateTimeOffset now)
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        // The stamp is compared as stored text: SoftDeleteAsync wrote it with the same "O" format.
        command.CommandText = "UPDATE dues SET deleted_at = NULL, updated_at = @now WHERE id = @id AND deleted_at = @deleted_at";
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@deleted_at", deletedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@now", now.ToString(TimestampFormat, CultureInfo.InvariantCulture));

        return await command.ExecuteNonQueryAsync() == 1;
    }

    private static void AddInsertParameters(SqliteCommand command, Due due)
    {
        AddEditableParameters(command, due);
        command.Parameters.AddWithValue("@created_at", due.CreatedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@deleted_at", due.DeletedAt is { } deleted
            ? deleted.ToString(TimestampFormat, CultureInfo.InvariantCulture)
            : DBNull.Value);
        command.Parameters.AddWithValue("@repeat_source_id", due.RepeatSourceId is { } source ? source.ToString() : DBNull.Value);
    }

    private static void AddEditableParameters(SqliteCommand command, Due due)
    {
        command.Parameters.AddWithValue("@id", due.Id.ToString());
        command.Parameters.AddWithValue("@title", due.Title);
        // No amount is SQL NULL, never 0: zero is a recorded amount.
        command.Parameters.AddWithValue("@amount", due.Amount is { } amount ? (object)amount.Centavos : DBNull.Value);
        command.Parameters.AddWithValue("@due_date", due.DueDate.ToString(DateFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@notes", (object?)due.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_paid", due.IsPaid ? 1 : 0);
        command.Parameters.AddWithValue("@category", (int)due.Category);
        command.Parameters.AddWithValue("@updated_at", due.UpdatedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@repeat_rule", (int)due.Repeat);
        command.Parameters.AddWithValue("@repeat_day", due.RepeatDay is { } day ? day : DBNull.Value);
    }

    // Column order matches SelectColumns.
    private static Due Map(SqliteDataReader reader) => new(
        Id: Guid.Parse(reader.GetString(0)),
        Title: reader.GetString(1),
        Amount: reader.IsDBNull(2) ? null : Money.FromCentavos(reader.GetInt64(2)),
        DueDate: DateOnly.ParseExact(reader.GetString(3), DateFormat, CultureInfo.InvariantCulture),
        Notes: reader.IsDBNull(4) ? null : reader.GetString(4),
        IsPaid: reader.GetInt64(5) != 0,
        CreatedAt: ParseTimestamp(reader.GetString(6)),
        UpdatedAt: ParseTimestamp(reader.GetString(7)),
        DeletedAt: reader.IsDBNull(8) ? null : ParseTimestamp(reader.GetString(8)),
        Category: (DueCategory)checked((int)reader.GetInt64(9)),
        Repeat: (RepeatRule)checked((int)reader.GetInt64(10)),
        RepeatDay: reader.IsDBNull(11) ? null : checked((int)reader.GetInt64(11)),
        RepeatSourceId: reader.IsDBNull(12) ? null : Guid.Parse(reader.GetString(12)));

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.ParseExact(value, TimestampFormat, CultureInfo.InvariantCulture);
}
