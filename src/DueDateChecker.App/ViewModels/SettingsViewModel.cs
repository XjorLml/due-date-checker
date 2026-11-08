using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;

namespace DueDateChecker.App.ViewModels;

public partial class SettingsViewModel(AppPreferencesService preferences, ReminderCoordinator? reminders = null) : ObservableObject
{
    private bool _hasDraft;
    public IReadOnlyList<string> Themes { get; } = Array.AsReadOnly(new[] { "System", "Light", "Dark" });
    public IReadOnlyList<string> WeekStarts { get; } = Array.AsReadOnly(new[] { "Monday", "Sunday" });
    [ObservableProperty] private int _selectedThemeIndex;
    [ObservableProperty] private int _selectedWeekStartIndex;
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _hasLoadError;
    /// <summary>Read-only summary of the saved F05 reminder settings for the Reminders row (PRD F04).</summary>
    [ObservableProperty] private string _reminderSummary = "Open due-date reminders";

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
        HasLoadError = !loaded;
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

    /// <summary>Reads the saved reminder state for display only. A failure leaves the generic label,
    /// because the row is navigation into Reminders, which reports its own errors.</summary>
    public async Task RefreshReminderSummaryAsync()
    {
        if (reminders is null) return;
        try
        {
            var settings = await reminders.ReadSettingsAsync();
            ReminderSummary = settings.Enabled
                ? $"On · {TimeSpan.FromMinutes(settings.MinuteOfDay):hh\\:mm} Manila{(settings.DayBefore ? " · day before" : "")}"
                : "Off";
        }
        catch (Exception)
        {
            ReminderSummary = "Open due-date reminders";
        }
    }

    public void ReportNavigationError() => Message = "Couldn't open Reminders. Try again.";
}
