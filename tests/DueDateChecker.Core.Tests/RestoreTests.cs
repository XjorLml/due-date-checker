using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.Core.Tests;

/// <summary>UX06 Undo after Delete: the guarded restore (specs/ux06-confirmations-undo CM04, CM05).</summary>
public sealed class RestoreTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Deleted = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Undone = Deleted.AddSeconds(3);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;

    public RestoreTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
    }

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    private async Task<Due> AddDeletedAsync()
    {
        var due = Due.Create("Rent", Money.FromCentavos(1500000), new DateOnly(2026, 11, 30), Created, "Unit 4B", DueCategory.Bill, RepeatRule.Monthly);
        await _repository.AddAsync(due);
        await _repository.SoftDeleteAsync(due.Id, Deleted);
        return due;
    }

    [Fact]
    public async Task Restore_clears_only_that_deletion_and_keeps_every_field()
    {
        var due = await AddDeletedAsync();

        Assert.True(await _repository.RestoreAsync(due.Id, Deleted, Undone));

        Assert.Equal(due with { UpdatedAt = Undone }, await new SqliteDueRepository(_path).GetAsync(due.Id));
        Assert.Equal(due.Id, Assert.Single(await _repository.ListActiveAsync()).Id);
    }

    [Fact]
    public async Task Restore_does_nothing_twice_or_for_another_deletion()
    {
        var due = await AddDeletedAsync();
        Assert.True(await _repository.RestoreAsync(due.Id, Deleted, Undone));

        Assert.False(await _repository.RestoreAsync(due.Id, Deleted, Undone.AddSeconds(1))); // already active

        var later = Undone.AddMinutes(1);
        await _repository.SoftDeleteAsync(due.Id, later);
        var tombstone = await _repository.GetAsync(due.Id);
        Assert.False(await _repository.RestoreAsync(due.Id, Deleted, later.AddSeconds(1))); // the first stamp no longer matches
        Assert.Equal(tombstone, await _repository.GetAsync(due.Id));
        Assert.False(await _repository.RestoreAsync(Guid.NewGuid(), Deleted, Undone));
    }

    [Fact]
    public async Task An_edit_still_cannot_revive_a_deleted_due()
    {
        var due = await AddDeletedAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(due with { Title = "Edited", DeletedAt = null, UpdatedAt = Undone }));

        Assert.Equal(Deleted, (await _repository.GetAsync(due.Id))!.DeletedAt);
    }
}
