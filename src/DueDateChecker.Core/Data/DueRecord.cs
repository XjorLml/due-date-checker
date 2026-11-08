namespace DueDateChecker.Core.Data;

/// <summary>
/// One <c>dues</c> row as SQLite holds it (UX05 export/import).
/// Unlike <see cref="Domain.Due"/> it keeps currency, timezone and the exact timestamp strings, so an
/// export followed by an import reproduces the row. <see cref="DueArchive.Validate"/> checks it against
/// the domain rules before it may be written.
/// </summary>
public sealed record DueRecord(
    string Id,
    string Title,
    long? AmountCentavos,
    string Currency,
    string DueDate,
    string? Notes,
    bool IsPaid,
    string Timezone,
    string CreatedAt,
    string UpdatedAt,
    string? DeletedAt,
    int Category,
    int RepeatRule,
    int? RepeatDay,
    string? RepeatSourceId,
    string CategoryId = Domain.PhilippineCategories.OtherId)
{
    public bool IsDeleted => DeletedAt is not null;
}
