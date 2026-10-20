using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;

namespace DueDateChecker.App.ViewModels;

public partial class SettingsViewModel(AppPreferencesService preferences) : ObservableObject
{
    private bool _hasDraft;
    public IReadOnlyList<string> Themes { get; } = Array.AsReadOnly(new[] { "System", "Light", "Dark" });
    public IReadOnlyList<string> WeekStarts { get; } = Array.AsReadOnly(new[] { "Monday", "Sunday" });
    [ObservableProperty] private int _selectedThemeIndex;
    [ObservableProperty] private int _selectedWeekStartIndex;
    [ObservableProperty] private string _message = "";

    [RelayCommand]
    private void Load()
    {
        var loaded = preferences.Load();
        if (!_hasDraft)
        {
            SelectedThemeIndex = (int)preferences.Current.Theme;
            SelectedWeekStartIndex = (int)preferences.Current.WeekStart;
            _hasDraft = true;
        }
        Message = loaded ? "" : preferences.Error;
    }

    [RelayCommand]
    private void Save()
    {
        if (SelectedThemeIndex < 0 || SelectedThemeIndex >= Themes.Count ||
            SelectedWeekStartIndex < 0 || SelectedWeekStartIndex >= WeekStarts.Count)
        {
            Message = "Choose an appearance and a week start before saving.";
            return;
        }
        var value = new AppPreferences((ThemePreference)SelectedThemeIndex, (WeekStartPreference)SelectedWeekStartIndex);
        Message = preferences.Save(value) ? "Preferences saved." : preferences.Error;
    }

    public void ReportNavigationError() => Message = "Couldn't open Reminders. Try again.";
}
