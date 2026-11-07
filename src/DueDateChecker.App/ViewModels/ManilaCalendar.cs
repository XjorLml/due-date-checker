namespace DueDateChecker.App.ViewModels;

/// <summary>The product calendar date: today in Asia/Manila for an injected clock. Shared by every
/// projection that derives a due status, so Dues, Details, Dashboard and Calendar agree on "today".</summary>
internal static class ManilaCalendar
{
    internal static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    public static DateOnly Today(TimeProvider clock)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Zone).DateTime);
}
