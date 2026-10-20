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
}
