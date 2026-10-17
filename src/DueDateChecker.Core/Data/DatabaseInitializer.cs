using System.Globalization;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Data;

/// <summary>
/// Brings a database file up to the current schema. Migrations are the embedded
/// <c>Data/Migrations/NNN_*.sql</c> scripts; the applied version is SQLite's own
/// <c>PRAGMA user_version</c> integer in the file header. Each script runs in a
/// transaction together with the version bump, so a crash mid-migration leaves
/// the file at the previous version, never half-applied.
/// </summary>
public sealed class DatabaseInitializer(string databasePath)
{
    public void Initialize()
    {
        using var connection = SqliteConnections.Open(databasePath);
        var currentVersion = ReadUserVersion(connection);
        var migrations = LoadMigrations().ToArray();
        var supportedVersion = migrations.Length == 0 ? 0 : migrations.Max(m => m.Version);
        if (currentVersion < 0 || currentVersion > supportedVersion)
        {
            throw new InvalidOperationException("The database schema is not supported by this version of the app.");
        }

        foreach (var (version, sql) in migrations.Where(m => m.Version > currentVersion))
        {
            using var transaction = connection.BeginTransaction();
            Execute(connection, transaction, sql);
            // PRAGMA takes no parameters; version is an int we parsed ourselves.
            Execute(connection, transaction, $"PRAGMA user_version = {version}");
            transaction.Commit();
        }
    }

    private static int ReadUserVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version";
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>Embedded scripts ordered by their numeric prefix (001_, 002_, ...).</summary>
    private static IEnumerable<(int Version, string Sql)> LoadMigrations()
    {
        var assembly = typeof(DatabaseInitializer).Assembly;

        return assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(name => (Version: ParseVersion(name), Sql: ReadResource(assembly, name)))
            .OrderBy(m => m.Version)
            .ToList();
    }

    private static int ParseVersion(string resourceName)
    {
        // Resource names look like "DueDateChecker.Core.Data.Migrations.001_create_dues.sql".
        var fileName = resourceName[(resourceName.LastIndexOf(".Migrations.", StringComparison.Ordinal) + ".Migrations.".Length)..];
        var digits = new string(fileName.TakeWhile(char.IsDigit).ToArray());
        return int.Parse(digits, CultureInfo.InvariantCulture);
    }

    private static string ReadResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Missing embedded migration '{name}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
