namespace DueDateChecker.App.Services;

public enum ThemePreference { System, Light, Dark }
public enum WeekStartPreference { Monday, Sunday }

/// <param name="PaydayFirst">UX33 (2026-11-16): the earlier of the user's two paydays, 1–31 (default 15).</param>
/// <param name="PaydaySecond">The later payday, 1–31 and after <paramref name="PaydayFirst"/> (default 30).</param>
public sealed record AppPreferences(ThemePreference Theme = ThemePreference.System,
    WeekStartPreference WeekStart = WeekStartPreference.Monday, int PaydayFirst = 15, int PaydaySecond = 30)
{
    /// <summary>Format 2 adds the paydays: <c>2|theme|week|first|second</c>. Format 1 still reads (PD04).</summary>
    public string Encode()
    {
        if (!ValidPaydays(PaydayFirst, PaydaySecond)) throw new ArgumentOutOfRangeException(nameof(PaydayFirst));
        return FormattableString.Invariant($"2|{Theme switch
        {
            ThemePreference.System => "system", ThemePreference.Light => "light", ThemePreference.Dark => "dark",
            _ => throw new ArgumentOutOfRangeException(nameof(Theme))
        }}|{WeekStart switch
        {
            WeekStartPreference.Monday => "monday", WeekStartPreference.Sunday => "sunday",
            _ => throw new ArgumentOutOfRangeException(nameof(WeekStart))
        }}|{PaydayFirst}|{PaydaySecond}");
    }

    /// <summary>Two different days of the month, stored earlier first.</summary>
    public static bool ValidPaydays(int first, int second) => first is >= 1 and <= 31 && second is >= 1 and <= 31 && first < second;

    public static AppPreferences Decode(string? value)
    {
        if (value is null) return new();
        var parts = value.Split('|');
        if (!(parts is ["1", _, _] || parts is ["2", _, _, _, _])) throw new FormatException("Unknown preferences format.");
        var theme = parts[1] switch
        {
            "system" => ThemePreference.System, "light" => ThemePreference.Light, "dark" => ThemePreference.Dark,
            _ => throw new FormatException("Unknown appearance preference.")
        };
        var week = parts[2] switch
        {
            "monday" => WeekStartPreference.Monday, "sunday" => WeekStartPreference.Sunday,
            _ => throw new FormatException("Unknown week-start preference.")
        };
        if (parts[0] == "1") return new(theme, week);
        if (!int.TryParse(parts[3], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var first) ||
            !int.TryParse(parts[4], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var second) ||
            !ValidPaydays(first, second))
            throw new FormatException("Unknown payday preference.");
        return new(theme, week, first, second);
    }
}

public interface IAppPreferencesStorage
{
    string? Read();
    void Write(string value);
}

// Foreground UI-thread service. One value prevents partial theme/week-start writes.
public sealed class AppPreferencesService(IAppPreferencesStorage storage)
{
    public AppPreferences Current { get; private set; } = new();
    public string Error { get; private set; } = "";
    public event EventHandler? Changed;

    public bool Load()
    {
        AppPreferences value;
        try { value = AppPreferences.Decode(storage.Read()); }
        catch (Exception)
        {
            Error = "Couldn't load preferences. Your last applied choices are still in use. Retry, or save your choices to replace these preferences.";
            return false;
        }
        Error = "";
        Publish(value);
        return true;
    }

    public bool Save(AppPreferences value)
    {
        try { storage.Write(value.Encode()); }
        catch (Exception)
        {
            Error = "Couldn't save preferences. Your changes have not been applied. Check available storage and try again.";
            return false;
        }
        Error = "";
        Publish(value);
        return true;
    }

    private void Publish(AppPreferences value)
    {
        if (value == Current) return;
        Current = value;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
