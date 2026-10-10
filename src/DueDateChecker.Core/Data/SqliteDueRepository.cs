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
        "SELECT id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at FROM dues";

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
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO dues (id, title, amount_centavos, due_date, notes, is_paid, created_at, updated_at, deleted_at)
            VALUES (@id, @title, @amount, @due_date, @notes, @is_paid, @created_at, @updated_at, @deleted_at)
            """;
        AddEditableParameters(command, due);
        command.Parameters.AddWithValue("@created_at", due.CreatedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Due due)
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE dues
            SET title = @title, amount_centavos = @amount, due_date = @due_date, notes = @notes,
                is_paid = @is_paid, updated_at = @updated_at, deleted_at = @deleted_at
            WHERE id = @id
            """;
        AddEditableParameters(command, due);

        var rows = await command.ExecuteNonQueryAsync();
        if (rows == 0)
        {
            throw new KeyNotFoundException($"Due {due.Id} does not exist.");
        }
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

    private static void AddEditableParameters(SqliteCommand command, Due due)
    {
        command.Parameters.AddWithValue("@id", due.Id.ToString());
        command.Parameters.AddWithValue("@title", due.Title);
        command.Parameters.AddWithValue("@amount", due.Amount.Centavos);
        command.Parameters.AddWithValue("@due_date", due.DueDate.ToString(DateFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@notes", (object?)due.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_paid", due.IsPaid ? 1 : 0);
        command.Parameters.AddWithValue("@updated_at", due.UpdatedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@deleted_at", due.DeletedAt is { } deleted
            ? deleted.ToString(TimestampFormat, CultureInfo.InvariantCulture)
            : DBNull.Value);
    }

    // Column order matches SelectColumns.
    private static Due Map(SqliteDataReader reader) => new(
        Id: Guid.Parse(reader.GetString(0)),
        Title: reader.GetString(1),
        Amount: Money.FromCentavos(reader.GetInt64(2)),
        DueDate: DateOnly.ParseExact(reader.GetString(3), DateFormat, CultureInfo.InvariantCulture),
        Notes: reader.IsDBNull(4) ? null : reader.GetString(4),
        IsPaid: reader.GetInt64(5) != 0,
        CreatedAt: ParseTimestamp(reader.GetString(6)),
        UpdatedAt: ParseTimestamp(reader.GetString(7)),
        DeletedAt: reader.IsDBNull(8) ? null : ParseTimestamp(reader.GetString(8)));

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.ParseExact(value, TimestampFormat, CultureInfo.InvariantCulture);
}
