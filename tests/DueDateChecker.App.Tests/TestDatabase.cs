using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

/// <summary>Closes pooled SQLite connections for one test's own database files only.</summary>
/// <remarks>
/// <c>SqliteConnection.ClearAllPools()</c> is process-wide, and xUnit runs test classes in
/// parallel. Clearing every pool could close a connection another class was still using
/// (seen 2026-11-06 as an ObjectDisposedException inside DeleteDueTests), so each class clears
/// only the pools for its own files. Pools are keyed by connection string, so the string here
/// must match the one <c>SqliteConnections.Open</c> builds.
/// </remarks>
internal static class TestDatabase
{
    public static void ClearPool(string databasePath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString());
        SqliteConnection.ClearPool(connection);
    }

    /// <summary>For tests that keep their databases in a private folder, including nested ones.</summary>
    public static void ClearPoolsUnder(string directory)
    {
        if (!Directory.Exists(directory)) return;
        foreach (var file in Directory.EnumerateFiles(directory, "*.db", SearchOption.AllDirectories))
        {
            ClearPool(file);
        }
    }
}
