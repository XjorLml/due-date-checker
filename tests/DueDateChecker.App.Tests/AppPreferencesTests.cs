using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;

namespace DueDateChecker.App.Tests;

public sealed class AppPreferencesTests
{
    [Fact]
    public void Missing_preferences_default_without_writing_and_publish_only_real_changes()
    {
        var storage = new MemoryPreferencesStorage();
        var service = new AppPreferencesService(storage);
        var events = 0;
        service.Changed += (_, _) => events++;
        Assert.True(service.Load());
        Assert.Equal(new AppPreferences(), service.Current);
        Assert.Equal(0, storage.Writes);
        Assert.Equal(0, events);
        Assert.True(service.Save(new(ThemePreference.Dark, WeekStartPreference.Sunday)));
        Assert.Equal(1, events);
        Assert.True(service.Load());
        Assert.Equal(1, events);
    }

    [Theory]
    [InlineData(ThemePreference.System, WeekStartPreference.Monday)]
    [InlineData(ThemePreference.System, WeekStartPreference.Sunday)]
    [InlineData(ThemePreference.Light, WeekStartPreference.Monday)]
    [InlineData(ThemePreference.Light, WeekStartPreference.Sunday)]
    [InlineData(ThemePreference.Dark, WeekStartPreference.Monday)]
    [InlineData(ThemePreference.Dark, WeekStartPreference.Sunday)]
    public void Saved_pair_round_trips_in_a_new_service(ThemePreference theme, WeekStartPreference week)
    {
        var storage = new MemoryPreferencesStorage();
        var value = new AppPreferences(theme, week);
        Assert.True(new AppPreferencesService(storage).Save(value));
        Assert.Equal(1, storage.Writes);
        var restarted = new AppPreferencesService(storage);
        Assert.True(restarted.Load());
        Assert.Equal(value, restarted.Current);
        Assert.Equal(1, storage.Writes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2|system|monday")]
    [InlineData("1|unknown|monday")]
    [InlineData("1|system|unknown")]
    [InlineData("1|system|monday|extra")]
    [InlineData("1|2|1")]
    public void Invalid_storage_is_reported_without_rewriting_and_explicit_Save_can_recover(string raw)
    {
        var storage = new MemoryPreferencesStorage { Value = raw };
        var service = new AppPreferencesService(storage);
        Assert.False(service.Load());
        Assert.NotEmpty(service.Error);
        Assert.Equal(new AppPreferences(), service.Current);
        Assert.Equal(raw, storage.Value);
        Assert.Equal(0, storage.Writes);
        Assert.True(service.Save(new()));
        Assert.True(service.Load());
        Assert.Empty(service.Error);
    }

    [Fact]
    public void Failed_read_keeps_last_good_preferences_and_retry_recovers_without_write()
    {
        var storage = new MemoryPreferencesStorage { Value = "1|dark|sunday" };
        var service = new AppPreferencesService(storage);
        Assert.True(service.Load());
        storage.FailRead = true;
        Assert.False(service.Load());
        Assert.Equal(new AppPreferences(ThemePreference.Dark, WeekStartPreference.Sunday), service.Current);
        Assert.Equal(0, storage.Writes);
        storage.FailRead = false;
        storage.Value = "1|light|monday";
        Assert.True(service.Load());
        Assert.Equal(ThemePreference.Light, service.Current.Theme);
        Assert.Empty(service.Error);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 2)]
    public void Invalid_enum_pair_never_reaches_storage(int theme, int week)
    {
        var storage = new MemoryPreferencesStorage();
        var service = new AppPreferencesService(storage);
        Assert.False(service.Save(new((ThemePreference)theme, (WeekStartPreference)week)));
        Assert.Equal(0, storage.Writes);
        Assert.Equal(new AppPreferences(), service.Current);
    }

    [Fact]
    public void Draft_does_not_apply_until_saved_and_survives_refresh_but_not_a_new_page()
    {
        var storage = new MemoryPreferencesStorage();
        var service = new AppPreferencesService(storage);
        var vm = new SettingsViewModel(service);
        vm.LoadCommand.Execute(null);
        vm.SelectedThemeIndex = 2;
        vm.SelectedWeekStartIndex = 1;
        vm.LoadCommand.Execute(null); // Resume or return from Reminders.
        Assert.Equal(2, vm.SelectedThemeIndex);
        Assert.Equal(1, vm.SelectedWeekStartIndex);
        Assert.Equal(new AppPreferences(), service.Current);
        Assert.Equal(0, storage.Writes);
        var backAndReopen = new SettingsViewModel(service);
        backAndReopen.LoadCommand.Execute(null);
        Assert.Equal(0, backAndReopen.SelectedThemeIndex);
        Assert.Equal(0, backAndReopen.SelectedWeekStartIndex);
        vm.SaveCommand.Execute(null);
        Assert.Equal("Preferences saved.", vm.Message);
        Assert.Equal(new AppPreferences(ThemePreference.Dark, WeekStartPreference.Sunday), service.Current);
        var reopened = new SettingsViewModel(new AppPreferencesService(storage));
        reopened.LoadCommand.Execute(null);
        Assert.Equal(2, reopened.SelectedThemeIndex);
        Assert.Equal(1, reopened.SelectedWeekStartIndex);
    }

    [Fact]
    public void Failed_Save_keeps_draft_stored_pair_and_applied_theme_until_successful_retry()
    {
        var storage = new MemoryPreferencesStorage { Value = "1|light|monday" };
        var service = new AppPreferencesService(storage);
        var vm = new SettingsViewModel(service);
        vm.LoadCommand.Execute(null);
        var changes = 0;
        service.Changed += (_, _) => changes++;
        vm.SelectedThemeIndex = 2;
        vm.SelectedWeekStartIndex = 1;
        storage.FailWrite = true;
        vm.SaveCommand.Execute(null);
        Assert.Contains("Couldn't save", vm.Message);
        Assert.Equal("1|light|monday", storage.Value);
        Assert.Equal(new AppPreferences(ThemePreference.Light), service.Current);
        Assert.Equal(2, vm.SelectedThemeIndex);
        Assert.Equal(1, vm.SelectedWeekStartIndex);
        Assert.Equal(0, changes);
        storage.FailWrite = false;
        vm.SaveCommand.Execute(null);
        Assert.Equal("Preferences saved.", vm.Message);
        Assert.Equal("1|dark|sunday", storage.Value);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void Invalid_picker_selection_reports_validation_and_read_retry_preserves_draft()
    {
        var storage = new MemoryPreferencesStorage { FailRead = true };
        var service = new AppPreferencesService(storage);
        var vm = new SettingsViewModel(service);
        vm.LoadCommand.Execute(null);
        Assert.Contains("Couldn't load", vm.Message);
        vm.SelectedThemeIndex = -1;
        vm.SaveCommand.Execute(null);
        Assert.Contains("Choose", vm.Message);
        vm.SelectedThemeIndex = 1;
        vm.SelectedWeekStartIndex = 9;
        vm.SaveCommand.Execute(null);
        Assert.Contains("Choose", vm.Message);
        Assert.Equal(0, storage.Writes);
        storage.FailRead = false;
        vm.SelectedWeekStartIndex = 1;
        vm.LoadCommand.Execute(null);
        Assert.Empty(vm.Message);
        Assert.Equal(1, vm.SelectedThemeIndex);
        Assert.Equal(1, vm.SelectedWeekStartIndex);
        vm.ReportNavigationError();
        Assert.Contains("Couldn't open Reminders", vm.Message);
        Assert.Equal(0, storage.Writes);
    }
}

internal sealed class MemoryPreferencesStorage : IAppPreferencesStorage
{
    public string? Value { get; set; }
    public bool FailRead { get; set; }
    public bool FailWrite { get; set; }
    public int Writes { get; private set; }
    public string? Read() => FailRead ? throw new IOException("Controlled read failure") : Value;
    public void Write(string value)
    {
        if (FailWrite) throw new IOException("Controlled write failure");
        Value = value;
        Writes++;
    }
}
