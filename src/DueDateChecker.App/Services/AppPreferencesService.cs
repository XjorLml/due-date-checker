namespace DueDateChecker.App.Services;

public enum ThemePreference { System, Light, Dark }
public enum WeekStartPreference { Monday, Sunday }

public sealed record AppPreferences(ThemePreference Theme = ThemePreference.System,
    WeekStartPreference WeekStart = WeekStartPreference.Monday)
{
    public string Encode() => $"1|{Theme switch
    {
        ThemePreference.System => "system", ThemePreference.Light => "light", ThemePreference.Dark => "dark",
        _ => throw new ArgumentOutOfRangeException(nameof(Theme))
    }}|{WeekStart switch
    {
        WeekStartPreference.Monday => "monday", WeekStartPreference.Sunday => "sunday",
        _ => throw new ArgumentOutOfRangeException(nameof(WeekStart))
    }}";

    public static AppPreferences Decode(string? value)
    {
        if (value is null) return new();
        var parts = value.Split('|');
        if (parts.Length != 3 || parts[0] != "1") throw new FormatException("Unknown preferences format.");
        return new(parts[1] switch
        {
            "system" => ThemePreference.System, "light" => ThemePreference.Light, "dark" => ThemePreference.Dark,
            _ => throw new FormatException("Unknown appearance preference.")
        }, parts[2] switch
        {
            "monday" => WeekStartPreference.Monday, "sunday" => WeekStartPreference.Sunday,
            _ => throw new FormatException("Unknown week-start preference.")
        });
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
