namespace DueDateChecker.Core.Domain;

/// <summary>
/// What the domain needs from storage. The app and the tests talk to this
/// interface; the SQLite implementation lives in Data/.
/// </summary>
public interface IDueRepository
{
    /// <summary>Dues not soft-deleted, soonest due date first.</summary>
    Task<IReadOnlyList<Due>> ListActiveAsync();

    /// <summary>Any due by id, including soft-deleted ones; null if unknown.</summary>
    Task<Due?> GetAsync(Guid id);

    Task AddAsync(Due due);

    /// <summary>
    /// Updates editable fields and UpdatedAt on an active due, preserving CreatedAt and DeletedAt.
    /// Throws if the id is unknown or soft-deleted.
    /// </summary>
    Task UpdateAsync(Due due);

    /// <summary>Marks the due deleted at <paramref name="when"/>; the row is kept for later sync.</summary>
    Task SoftDeleteAsync(Guid id, DateTimeOffset when);

    /// <summary>
    /// Saves <paramref name="updated"/> (an active due just marked paid) and inserts <paramref name="next"/>,
    /// the occurrence generated from it (ADR 009). Returns false when an occurrence from the same source
    /// already exists; then only the update is saved. Throws like <see cref="UpdateAsync"/> for an unknown
    /// or deleted due. Storage implementations must make this one transaction. This default (update, then
    /// add, without the duplicate guard) only exists so wrappers and test doubles keep compiling.
    /// </summary>
    async Task<bool> UpdateWithNextAsync(Due updated, Due next)
    {
        await UpdateAsync(updated);
        await AddAsync(next);
        return true;
    }

    /// <summary>
    /// UX06 Undo: clears the deletion of <paramref name="id"/> only if it was deleted at exactly
    /// <paramref name="deletedAt"/>, and sets UpdatedAt to <paramref name="now"/>; every other field stays.
    /// Returns false (nothing changed) for an active, unknown or differently stamped row, so an Undo can
    /// reverse only the deletion it was offered for. Storage implementations must provide this; the
    /// default only lets wrappers and test doubles that never restore keep compiling.
    /// </summary>
    Task<bool> RestoreAsync(Guid id, DateTimeOffset deletedAt, DateTimeOffset now) =>
        throw new NotSupportedException("This repository cannot restore deleted dues.");
}
