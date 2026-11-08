using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

// Reconciliation is an after-commit side effect. It never turns a successful
// SQLite write into a failed Save/Delete or waits on the dispatcher's held gate.
public sealed class ReminderAwareDueRepository(IDueRepository inner, ReminderCoordinator coordinator) : IDueRepository
{
    public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
    public Task<Due?> GetAsync(Guid id) => inner.GetAsync(id);
    public async Task AddAsync(Due due) { await inner.AddAsync(due); coordinator.AfterWrite(due.Id); }
    public async Task UpdateAsync(Due due) { await inner.UpdateAsync(due); coordinator.AfterWrite(due.Id); }
    public async Task SoftDeleteAsync(Guid id, DateTimeOffset when) { await inner.SoftDeleteAsync(id, when); coordinator.AfterWrite(id); }

    // Must forward: the interface default refuses. A restored unpaid due needs its reminders planned again.
    public async Task<bool> RestoreAsync(Guid id, DateTimeOffset deletedAt, DateTimeOffset now)
    {
        var restored = await inner.RestoreAsync(id, deletedAt, now);
        if (restored) coordinator.AfterWrite(id);
        return restored;
    }

    // Must forward: the interface default would run this wrapper's Update then Add as two writes.
    // One AfterWrite is enough; its reconciliation re-plans every due, the new occurrence included.
    public async Task<bool> UpdateWithNextAsync(Due updated, Due next)
    {
        var inserted = await inner.UpdateWithNextAsync(updated, next);
        coordinator.AfterWrite(updated.Id);
        return inserted;
    }
}
