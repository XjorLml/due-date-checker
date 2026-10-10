using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

// Each test gets its own temp database file so tests can't see each other's rows.
public sealed class SqliteDueRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 9, 0, 0, TimeSpan.FromHours(8));

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"dues-test-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repo;

    public SqliteDueRepositoryTests()
    {
        new DatabaseInitializer(_dbPath).Initialize();
        _repo = new SqliteDueRepository(_dbPath);
    }

    public void Dispose()
    {
        // Pooled connections keep the file open; clear them before deleting.
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Fact]
    public void Initialize_sets_user_version_and_creates_the_dues_table()
    {
        Assert.Equal(1, ReadUserVersion());
        Assert.Equal(1L, Scalar("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'dues'"));
    }

    [Fact]
    public void Initialize_twice_is_a_no_op()
    {
        new DatabaseInitializer(_dbPath).Initialize();

        Assert.Equal(1, ReadUserVersion());
    }

    [Fact]
    public async Task Add_then_list_round_trips_every_field()
    {
        var due = Due.Create("Meralco", Money.FromDecimal(2450m), new DateOnly(2026, 10, 15), Now, notes: "ref 123");

        await _repo.AddAsync(due);
        var listed = await _repo.ListActiveAsync();

        var stored = Assert.Single(listed);
        Assert.Equal(due, stored);
    }

    [Fact]
    public async Task List_orders_by_due_date_ascending()
    {
        var later = Due.Create("Later", Money.FromCentavos(0), new DateOnly(2026, 11, 1), Now);
        var sooner = Due.Create("Sooner", Money.FromCentavos(0), new DateOnly(2026, 10, 12), Now);
        await _repo.AddAsync(later);
        await _repo.AddAsync(sooner);

        var listed = await _repo.ListActiveAsync();

        Assert.Equal(new[] { sooner.Id, later.Id }, listed.Select(d => d.Id));
    }

    [Fact]
    public async Task Get_returns_null_for_unknown_id()
    {
        Assert.Null(await _repo.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_persists_changed_fields()
    {
        var due = Due.Create("Rent", Money.FromDecimal(8000m), new DateOnly(2026, 10, 30), Now);
        await _repo.AddAsync(due);
        var later = Now.AddHours(1);

        var edited = due with { Title = "Rent (Oct)", Amount = Money.FromDecimal(8500m), IsPaid = true, UpdatedAt = later };
        await _repo.UpdateAsync(edited);

        var stored = await _repo.GetAsync(due.Id);
        Assert.Equal(edited, stored);
    }

    [Fact]
    public async Task SoftDelete_hides_from_list_but_keeps_the_row()
    {
        var due = Due.Create("Globe", Money.FromDecimal(999m), new DateOnly(2026, 10, 20), Now);
        await _repo.AddAsync(due);
        var when = Now.AddDays(1);

        await _repo.SoftDeleteAsync(due.Id, when);

        Assert.Empty(await _repo.ListActiveAsync());
        var stored = await _repo.GetAsync(due.Id);
        Assert.NotNull(stored);
        Assert.Equal(when, stored.DeletedAt);
        Assert.Equal(when, stored.UpdatedAt);
    }

    private int ReadUserVersion() => (int)(long)Scalar("PRAGMA user_version");

    private object Scalar(string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar()!;
    }
}
