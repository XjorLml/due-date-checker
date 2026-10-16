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
}
