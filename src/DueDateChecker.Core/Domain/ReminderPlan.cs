namespace DueDateChecker.Core.Domain;

public sealed record ReminderSettings(bool Enabled = false, bool DayBefore = true,
    int MinuteOfDay = 540, int QuietStart = 1320, int QuietEnd = 480)
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
    public static IReadOnlyList<ReminderOccurrence> Plan(IEnumerable<Due> dues, ReminderSettings settings, TimeZoneInfo zone)
    {
        settings.Validate();
        if (!settings.Enabled) return [];
        var result = new List<ReminderOccurrence>();
        foreach (var due in dues.Where(due => !due.IsPaid && due.DeletedAt is null))
        foreach (var offset in settings.DayBefore ? new[] { -1, 0 } : new[] { 0 })
        {
            if (offset < 0 && due.DueDate == DateOnly.MinValue) continue;
            var date = due.DueDate.AddDays(offset);
            var local = date.ToDateTime(TimeOnly.MinValue).AddMinutes(settings.MinuteOfDay);
            var shifted = OutsideQuiet(local, settings);
            if (shifted is null || DateOnly.FromDateTime(shifted.Value) > due.DueDate) continue;
            var at = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(shifted.Value, zone));
            // Offsets remain distinct even if quiet hours merge their delivery times.
            var key = FormattableString.Invariant($"{due.Id:N}/{due.DueDate:yyyy-MM-dd}/{offset}/{at.ToUnixTimeMilliseconds()}");
            result.Add(new(key, due.Id, due.DueDate, at));
        }
        return result.OrderBy(item => item.At).ThenBy(item => item.Key, StringComparer.Ordinal).ToArray();
    }

    public static DateTimeOffset? DeliveryTime(ReminderOccurrence item, DateTimeOffset now, ReminderSettings settings, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTime(now, zone).DateTime;
        if (DateOnly.FromDateTime(local) > item.DueDate) return null;
        if (item.At > now) return item.At;
        var shifted = OutsideQuiet(local, settings);
        return shifted is null || DateOnly.FromDateTime(shifted.Value) > item.DueDate ? null
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
