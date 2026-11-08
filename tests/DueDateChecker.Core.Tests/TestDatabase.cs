using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

/// <summary>Closes pooled SQLite connections for one test's own database file only.</summary>
/// <remarks>
/// <c>SqliteConnection.ClearAllPools()</c> is process-wide, and xUnit runs test classes in
/// parallel. Clearing every pool could close a connection another class was still using
/// (seen 2026-11-06 as an ObjectDisposedException in the App tests), so each class clears only
/// the pool for its own file. Pools are keyed by connection string, so the string here must
/// match the one <c>SqliteConnections.Open</c> builds.
/// </remarks>
internal static class TestDatabase
{
    public static void ClearPool(string databasePath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString());
        SqliteConnection.ClearPool(connection);
    }
}
