using System.Globalization;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Data;

public sealed class SqliteReminderStore(string databasePath) : IReminderStore
{
    private static string Stamp(DateTimeOffset at) => at.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    public async Task<ReminderSettings> ReadSettingsAsync()
    {
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT enabled,day_before,minute_of_day,quiet_start,quiet_end FROM reminder_settings WHERE id=1";
        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) throw new InvalidOperationException("Reminder settings unavailable.");
        var settings = new ReminderSettings(reader.GetInt64(0) != 0, reader.GetInt64(1) != 0, reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));
        settings.Validate(); return settings;
    }
    public async Task SaveSettingsAsync(ReminderSettings settings)
    {
        settings.Validate();
        using var connection = SqliteConnections.Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE reminder_settings SET enabled=@enabled,day_before=@before,minute_of_day=@minute,quiet_start=@start,quiet_end=@end WHERE id=1";
        command.Parameters.AddWithValue("@enabled", settings.Enabled ? 1 : 0); command.Parameters.AddWithValue("@before", settings.DayBefore ? 1 : 0);
        command.Parameters.AddWithValue("@minute", settings.MinuteOfDay); command.Parameters.AddWithValue("@start", settings.QuietStart); command.Parameters.AddWithValue("@end", settings.QuietEnd);
        if (await command.ExecuteNonQueryAsync() != 1) throw new InvalidOperationException("Reminder settings unavailable.");
    }
    public async Task<IReadOnlyList<ReminderOccurrence>> ReadOccurrencesAsync()
    {
        using var connection = SqliteConnections.Open(databasePath); using var command = connection.CreateCommand();
        command.CommandText = "SELECT occurrence_key,due_id,due_date,scheduled_at,delivered_at FROM reminder_occurrences";
        using var reader = await command.ExecuteReaderAsync(); var rows = new List<ReminderOccurrence>();
        while (await reader.ReadAsync()) rows.Add(new(reader.GetString(0), Guid.Parse(reader.GetString(1)),
            DateOnly.ParseExact(reader.GetString(2), "yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset.ParseExact(reader.GetString(3), "O", CultureInfo.InvariantCulture),
            reader.IsDBNull(4) ? null : DateTimeOffset.ParseExact(reader.GetString(4), "O", CultureInfo.InvariantCulture)));
        return rows;
    }
    public async Task ReplacePendingAsync(IReadOnlyList<ReminderOccurrence> pending)
    {
        using var connection = SqliteConnections.Open(databasePath); using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        { command.Transaction = transaction; command.CommandText = "DELETE FROM reminder_occurrences WHERE delivered_at IS NULL"; await command.ExecuteNonQueryAsync(); }
        foreach (var item in pending)
        {
            using var command = connection.CreateCommand(); command.Transaction = transaction;
            command.CommandText = "INSERT INTO reminder_occurrences(occurrence_key,due_id,due_date,scheduled_at) VALUES(@key,@id,@date,@at) ON CONFLICT(occurrence_key) DO NOTHING";
            command.Parameters.AddWithValue("@key", item.Key); command.Parameters.AddWithValue("@id", item.DueId.ToString());
            command.Parameters.AddWithValue("@date", item.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); command.Parameters.AddWithValue("@at", Stamp(item.At));
            await command.ExecuteNonQueryAsync();
        }
        transaction.Commit();
    }
    public async Task MarkDeliveredAsync(string key, DateTimeOffset when)
    {
        using var connection = SqliteConnections.Open(databasePath); using var command = connection.CreateCommand();
        command.CommandText = "UPDATE reminder_occurrences SET delivered_at=@when WHERE occurrence_key=@key AND delivered_at IS NULL";
        command.Parameters.AddWithValue("@when", Stamp(when)); command.Parameters.AddWithValue("@key", key); await command.ExecuteNonQueryAsync();
    }
}
