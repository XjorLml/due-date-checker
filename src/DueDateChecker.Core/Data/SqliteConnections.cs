using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Data;

internal static class SqliteConnections
{
    /// <summary>Opens (and creates if missing) the database file at <paramref name="databasePath"/>.</summary>
    public static SqliteConnection Open(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}
