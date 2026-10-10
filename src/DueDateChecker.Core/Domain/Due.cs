namespace DueDateChecker.Core.Domain;

/// <summary>
/// One thing the user has to pay by a date. Immutable: edits produce a new
/// instance via <c>with</c>, and the title rule is enforced on every path.
/// Timestamps come from the caller (a <see cref="TimeProvider"/> upstream),
/// never from the clock directly, so behaviour is testable.
/// </summary>
public sealed record Due(
    Guid Id,
    string Title,
    Money Amount,
    DateOnly DueDate,
    string? Notes,
    bool IsPaid,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt)
{
    private readonly string _title = ValidateTitle(Title);

    public string Title
    {
        get => _title;
        init => _title = ValidateTitle(value);
    }

    public static Due Create(string title, Money amount, DateOnly dueDate, DateTimeOffset now, string? notes = null)
        => new(Guid.NewGuid(), title, amount, dueDate, notes, IsPaid: false, CreatedAt: now, UpdatedAt: now, DeletedAt: null);

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        return title.Trim();
    }
}
