namespace DueDateChecker.Core.Domain;

/// <summary>
/// One thing the user has to pay by a date. Immutable: edits produce a new
/// instance via <c>with</c>, and the title rule is enforced on every path.
/// Timestamps come from the caller (a <see cref="TimeProvider"/> upstream),
/// never from the clock directly, so behaviour is testable.
/// </summary>
/// <remarks>
/// <see cref="Amount"/> is null when the due has no amount, which only Task and
/// Other allow (F07 optional amounts). Zero is a recorded amount, not "no amount".
/// That rule spans two properties, so it cannot sit in an init accessor: a
/// <c>with</c> expression sets properties one at a time and would see a
/// half-changed record. <see cref="Validate"/> checks the finished record instead;
/// <see cref="Create"/> and every repository write call it.
/// </remarks>
public sealed record Due(
    Guid Id,
    string Title,
    Money? Amount,
    DateOnly DueDate,
    string? Notes,
    bool IsPaid,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt,
    DueCategory Category = DueCategory.Other,
    RepeatRule Repeat = RepeatRule.Never,
    int? RepeatDay = null,
    Guid? RepeatSourceId = null)
{
    private readonly string _title = ValidateTitle(Title);
    private readonly DueCategory _category = ValidateCategory(Category);
    private readonly RepeatRule _repeat = ValidateRepeat(Repeat);

    public DueCategory Category
    {
        get => _category;
        init => _category = ValidateCategory(value);
    }

    /// <summary>F07 recurrence (ADR 009). Only Bill and Subscription may repeat; checked in <see cref="Validate"/>.</summary>
    public RepeatRule Repeat
    {
        get => _repeat;
        init => _repeat = ValidateRepeat(value);
    }

    public string Title
    {
        get => _title;
        init => _title = ValidateTitle(value);
    }

    public static Due Create(string title, Money? amount, DateOnly dueDate, DateTimeOffset now, string? notes = null,
        DueCategory category = DueCategory.Other, RepeatRule repeat = RepeatRule.Never)
    {
        var due = new Due(Guid.NewGuid(), title, amount, dueDate, notes, IsPaid: false, CreatedAt: now, UpdatedAt: now, DeletedAt: null,
            Category: category, Repeat: repeat, RepeatDay: Recurrence.AnchorFor(repeat, dueDate));
        due.Validate();
        return due;
    }

    /// <summary>Bill and Subscription must carry an amount; Task and Other may leave it out.</summary>
    public static bool RequiresAmount(DueCategory category) => category is DueCategory.Bill or DueCategory.Subscription;

    /// <summary>Only Bill and Subscription can repeat (user decision, 2026-11-06).</summary>
    public static bool CanRepeat(DueCategory category) => category is DueCategory.Bill or DueCategory.Subscription;

    /// <summary>Throws when the finished record breaks a rule that involves more than one property.</summary>
    public void Validate()
    {
        if (Amount is null && RequiresAmount(Category))
        {
            throw new ArgumentException($"A {Category} due requires an amount.", nameof(Amount));
        }

        if (Repeat != RepeatRule.Never && !CanRepeat(Category))
        {
            throw new ArgumentException($"A {Category} due cannot repeat.", nameof(Repeat));
        }

        var needsAnchor = Repeat is RepeatRule.Monthly or RepeatRule.Yearly;
        if (needsAnchor && RepeatDay is not (>= 1 and <= 31))
        {
            throw new ArgumentException("A monthly or yearly repeat needs a day of month from 1 to 31.", nameof(RepeatDay));
        }

        if (!needsAnchor && RepeatDay is not null)
        {
            throw new ArgumentException("Only a monthly or yearly repeat stores a day of month.", nameof(RepeatDay));
        }
    }

    private static RepeatRule ValidateRepeat(RepeatRule repeat) => Enum.IsDefined(repeat)
        ? repeat : throw new ArgumentOutOfRangeException(nameof(repeat), "Choose a supported repeat.");

    private static DueCategory ValidateCategory(DueCategory category) => Enum.IsDefined(category)
        ? category : throw new ArgumentOutOfRangeException(nameof(category), "Choose a supported category.");

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        return title.Trim();
    }
}
