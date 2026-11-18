namespace DueDateChecker.Core.Domain;

/// <param name="Overdue">UX30 (2026-11-16): also remind 1, 7, 14, 21 and 28 days after an unpaid due date.</param>
public sealed record ReminderSettings(bool Enabled = false, bool DayBefore = true,
    int MinuteOfDay = 540, int QuietStart = 1320, int QuietEnd = 480, bool Overdue = true)
{
    public void Validate()
    {
        foreach (var value in new[] { MinuteOfDay, QuietStart, QuietEnd })
            if (value is < 0 or > 1439) throw new ArgumentOutOfRangeException(nameof(value));
    }
}

public sealed record ReminderOccurrence(string Key, Guid DueId, DateOnly DueDate, DateTimeOffset At, DateTimeOffset? DeliveredAt = null);

public static class ReminderPlanner
{
    /// <summary>UX30: days after the due date that get an overdue reminder; none after the last.</summary>
    public static readonly IReadOnlyList<int> OverdueDays = [1, 7, 14, 21, 28];

    /// <summary>The oldest due date that can still have a pending reminder on <paramref name="today"/>.</summary>
    public static DateOnly EarliestRelevantDueDate(DateOnly today) =>
        today.DayNumber - OverdueDays[^1] >= DateOnly.MinValue.DayNumber ? today.AddDays(-OverdueDays[^1]) : DateOnly.MinValue;

    public static IReadOnlyList<ReminderOccurrence> Plan(IEnumerable<Due> dues, ReminderSettings settings, TimeZoneInfo zone)
    {
        settings.Validate();
        if (!settings.Enabled) return [];
        var offsets = (settings.DayBefore ? new[] { -1, 0 } : new[] { 0 }).Concat(settings.Overdue ? OverdueDays : []).ToArray();
        var result = new List<ReminderOccurrence>();
        foreach (var due in dues.Where(due => !due.IsPaid && due.DeletedAt is null))
        foreach (var offset in offsets)
        {
            if (offset < 0 && due.DueDate == DateOnly.MinValue) continue;
            if (offset > 0 && due.DueDate.DayNumber + offset > DateOnly.MaxValue.DayNumber) continue;
            var date = due.DueDate.AddDays(offset);
            var local = date.ToDateTime(TimeOnly.MinValue).AddMinutes(settings.MinuteOfDay);
            var shifted = OutsideQuiet(local, settings);
            // Before-and-on reminders never move past the due date; an overdue one keeps its own day.
            if (shifted is null || offset <= 0 && DateOnly.FromDateTime(shifted.Value) > due.DueDate) continue;
            var at = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(shifted.Value, zone));
            // Offsets remain distinct even if quiet hours merge their delivery times.
            var key = FormattableString.Invariant($"{due.Id:N}/{due.DueDate:yyyy-MM-dd}/{offset}/{at.ToUnixTimeMilliseconds()}");
            result.Add(new(key, due.Id, due.DueDate, at));
        }
        return result.OrderBy(item => item.At).ThenBy(item => item.Key, StringComparer.Ordinal).ToArray();
    }

    /// <summary>
    /// The last Manila date a reminder may still be delivered: its due date for the day before and the
    /// day itself, and its own planned day for an overdue reminder (OD03), which is always later.
    /// </summary>
    public static DateOnly LastDeliveryDate(ReminderOccurrence item, TimeZoneInfo zone)
    {
        var planned = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(item.At, zone).DateTime);
        return planned > item.DueDate ? planned : item.DueDate;
    }

    public static DateTimeOffset? DeliveryTime(ReminderOccurrence item, DateTimeOffset now, ReminderSettings settings, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTime(now, zone).DateTime;
        var last = LastDeliveryDate(item, zone);
        if (DateOnly.FromDateTime(local) > last) return null;
        if (item.At > now) return item.At;
        var shifted = OutsideQuiet(local, settings);
        return shifted is null || DateOnly.FromDateTime(shifted.Value) > last ? null
            : new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(shifted.Value, zone));
    }

    private static DateTime? OutsideQuiet(DateTime local, ReminderSettings settings)
    {
        var minute = local.Hour * 60 + local.Minute;
        var start = settings.QuietStart; var end = settings.QuietEnd;
        if (start == end || !(start < end ? minute >= start && minute < end : minute >= start || minute < end)) return local;
        if (start > end && minute >= start)
        {
            if (DateOnly.FromDateTime(local) == DateOnly.MaxValue) return null;
            local = local.AddDays(1);
        }
        return local.Date.AddMinutes(end);
    }
}
