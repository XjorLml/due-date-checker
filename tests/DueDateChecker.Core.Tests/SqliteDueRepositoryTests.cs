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
    public async Task Initialize_rejects_newer_schema_without_changing_existing_data()
    {
        var due = Due.Create("Rent", Money.FromCentavos(100), new DateOnly(2026, 10, 17), Now);
        await _repo.AddAsync(due);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version = 99";
            command.ExecuteNonQuery();
        }

        Assert.Throws<InvalidOperationException>(() => new DatabaseInitializer(_dbPath).Initialize());
        Assert.Equal(99, ReadUserVersion());
        Assert.Equal(due, await _repo.GetAsync(due.Id));
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

    [Fact]
    public async Task Repeated_soft_delete_keeps_the_first_timestamps_after_reopening()
    {
        var due = Due.Create("Rent", Money.FromCentavos(100), new DateOnly(2026, 10, 17), Now);
        await _repo.AddAsync(due);
        var firstDeletion = Now.AddHours(1);
        await _repo.SoftDeleteAsync(due.Id, firstDeletion);
        await _repo.SoftDeleteAsync(due.Id, Now.AddHours(2));
        var reopened = new SqliteDueRepository(_dbPath);
        Assert.Equal(due with { DeletedAt = firstDeletion, UpdatedAt = firstDeletion }, await reopened.GetAsync(due.Id));
        Assert.Empty(await reopened.ListActiveAsync());
    }

    [Fact]
    public async Task Deleting_an_unknown_id_is_a_no_op_for_other_rows()
    {
        var due = Due.Create("Rent", Money.FromCentavos(100), new DateOnly(2026, 10, 17), Now);
        await _repo.AddAsync(due);
        await _repo.SoftDeleteAsync(Guid.NewGuid(), Now.AddHours(1));
        Assert.Equal(due, Assert.Single(await _repo.ListActiveAsync()));
    }

    [Fact]
    public async Task Update_rejects_a_deleted_due_and_preserves_the_tombstone()
    {
        var due = Due.Create("Rent", Money.FromCentavos(100), new DateOnly(2026, 10, 17), Now);
        await _repo.AddAsync(due);
        var deletedAt = Now.AddHours(1);
        await _repo.SoftDeleteAsync(due.Id, deletedAt);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repo.UpdateAsync(due with { Title = "Stale edit", UpdatedAt = Now.AddHours(2) }));
        var stored = await _repo.GetAsync(due.Id);
        Assert.Equal(deletedAt, stored!.DeletedAt);
        Assert.Equal(deletedAt, stored.UpdatedAt);
        Assert.Equal("Rent", stored.Title);
    }

    [Fact]
    public async Task Update_of_an_unknown_id_throws()
    {
        var due = Due.Create("Unknown", Money.FromCentavos(0), new DateOnly(2026, 10, 17), Now);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repo.UpdateAsync(due));
    }

    [Fact]
    public async Task Update_preserves_created_at_and_cannot_soft_delete_an_active_record()
    {
        var due = Due.Create("Rent", Money.FromCentavos(0), new DateOnly(2026, 10, 17), Now);
        await _repo.AddAsync(due);
        await _repo.UpdateAsync(due with { Title = "Edited", CreatedAt = Now.AddDays(-1), DeletedAt = Now, UpdatedAt = Now.AddDays(1) });
        var stored = await _repo.GetAsync(due.Id);
        Assert.Equal(due.CreatedAt, stored!.CreatedAt);
        Assert.Null(stored.DeletedAt);
        Assert.Equal("Edited", stored.Title);
        Assert.Single(await _repo.ListActiveAsync());
    }

    private object Scalar(string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar()!;
    }
}
