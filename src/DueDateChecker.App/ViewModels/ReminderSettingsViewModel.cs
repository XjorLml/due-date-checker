using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public partial class ReminderSettingsViewModel(ReminderCoordinator coordinator, IReminderPlatform platform) : ObservableObject
{
    [ObservableProperty] private TimeSpan _reminderTime = TimeSpan.FromHours(9);
    [ObservableProperty] private TimeSpan _quietStart = TimeSpan.FromHours(22);
    [ObservableProperty] private TimeSpan _quietEnd = TimeSpan.FromHours(8);
    [ObservableProperty] private bool _dayBefore = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnabledText))]
    private bool _enabled;
    [ObservableProperty] private bool _hasLoaded;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    private bool _isBusy;
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _backgroundText = "";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _scheduleText = "";
    public bool CanEdit => !IsBusy && HasLoaded;
    public string EnabledText => Enabled ? "Reminders enabled in Duewise" : "Reminders are off";

    private bool Eligible() => !IsBusy && HasLoaded;
    private ReminderSettings Draft(bool enabled)
    {
        static int Minutes(TimeSpan value)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(value));
            return (int)value.TotalMinutes;
        }
        return new(enabled, DayBefore, Minutes(ReminderTime), Minutes(QuietStart), Minutes(QuietEnd));
    }
    private void UpdateStatus()
    {
        try { StatusText = platform.GetAccess().Description; }
        catch (Exception) { StatusText = "Android notification status is unavailable. Retry."; }
        try { BackgroundText = platform.GetBackgroundStatus().Description; }
        catch (Exception) { BackgroundText = "Android battery/background status is unavailable. Retry or open Android app settings."; }
        ScheduleText = coordinator.NextAt is { } next
            ? $"Next check: {TimeZoneInfo.ConvertTime(next, TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila")):dd MMM yyyy HH:mm} Asia/Manila. Android may deliver later."
            : "No future reminder is currently scheduled.";
    }
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var settings = await coordinator.ReadSettingsAsync();
            if (!HasLoaded)
            { ReminderTime = TimeSpan.FromMinutes(settings.MinuteOfDay); QuietStart = TimeSpan.FromMinutes(settings.QuietStart); QuietEnd = TimeSpan.FromMinutes(settings.QuietEnd); DayBefore = settings.DayBefore; }
            Enabled = settings.Enabled; HasLoaded = true; OnPropertyChanged(nameof(CanEdit));
            await coordinator.ReconcileAsync(); UpdateStatus(); Message = coordinator.LastError;
        }
        catch (Exception) { Message = "Couldn't load reminder settings. Your saved dues are unchanged. Retry."; }
        finally { IsBusy = false; }
    }
    private async Task SaveDraftAsync(bool enable, bool requestPermission)
    {
        if (!Eligible()) return;
        IsBusy = true; Message = "";
        try
        {
            var settings = Draft(enable);
            if (requestPermission)
            {
                await platform.RequestPermissionAsync();
                if (!platform.GetAccess().Allowed)
                { UpdateStatus(); Message = "Android notifications are blocked. Open Android notification settings to allow them."; return; }
            }
            var reconciled = await coordinator.SaveSettingsAsync(settings);
            Enabled = settings.Enabled; UpdateStatus();
            Message = reconciled ? "Reminder settings saved." : "Settings saved. " + coordinator.LastError;
        }
        catch (ArgumentOutOfRangeException) { Message = "Choose times within a single day."; }
        catch (Exception) { Message = "Couldn't save reminder settings. Your changes are still here. Retry."; }
        finally { IsBusy = false; }
    }
    [RelayCommand] private Task SaveAsync() => SaveDraftAsync(Enabled, false);
    [RelayCommand] private Task EnableAsync() => SaveDraftAsync(true, true);
    [RelayCommand]
    private async Task DisableAsync()
    {
        if (!Eligible()) return;
        IsBusy = true;
        try
        {
            var saved = await coordinator.ReadSettingsAsync();
            var ok = await coordinator.SaveSettingsAsync(saved with { Enabled = false });
            Enabled = false; UpdateStatus(); Message = ok ? "Reminders disabled." : "Disabled in Duewise. " + coordinator.LastError;
        }
        catch (Exception) { Message = "Couldn't disable reminders. Retry."; }
        finally { IsBusy = false; }
    }
    [RelayCommand]
    private void Test()
    {
        if (!Eligible()) return;
        try
        {
            if (!Enabled || !platform.GetAccess().Allowed) { Message = "Enable reminders and allow Android notifications first."; return; }
            platform.PostTest(); Message = "Test notification sent. Android may hide alerts if the device is muted.";
        }
        catch (Exception) { Message = "Couldn't send the test notification. Check Android settings and retry."; }
    }
    [RelayCommand]
    private void OpenSystemSettings()
    { try { platform.OpenSettings(); } catch (Exception) { Message = "Couldn't open Android notification settings."; } }
    [RelayCommand]
    private void OpenAppSettings()
    { try { platform.OpenAppSettings(); } catch (Exception) { Message = "Couldn't open Android app settings. Open Duewise in Android settings and review battery/background use."; } }
}
