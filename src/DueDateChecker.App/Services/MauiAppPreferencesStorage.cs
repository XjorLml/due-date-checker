namespace DueDateChecker.App.Services;

public sealed class MauiAppPreferencesStorage : IAppPreferencesStorage
{
    private const string Key = "duewise.preferences.v1";
    public string? Read() => Preferences.Default.ContainsKey(Key) ? Preferences.Default.Get(Key, "") : null;
    public void Write(string value) => Preferences.Default.Set(Key, value);
}
