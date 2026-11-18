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
    /// <summary>UX30: also remind 1, 7, 14, 21 and 28 days after an unpaid due date.</summary>
    [ObservableProperty] private bool _overdue = true;
    /// <summary>UX29: the current Android alert style of each reminder channel.</summary>
    [ObservableProperty] private string _dueTodayStyle = "";
    [ObservableProperty] private string _comingUpStyle = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnabledText))]
    [NotifyPropertyChangedFor(nameof(IsOff))]
    [NotifyPropertyChangedFor(nameof(StatusTitle))]
    private bool _enabled;
    [ObservableProperty] private bool _hasLoaded;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    private bool _isBusy;
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _backgroundText = "";
    // UX04: shown only while reminders are on, below the general background status.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMakerSteps))]
    private string _makerStepsText = "";
    public bool HasMakerSteps => !string.IsNullOrEmpty(MakerStepsText);
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _scheduleText = "";
    public bool CanEdit => !IsBusy && HasLoaded;
    public string EnabledText => Enabled ? "Reminders enabled in Duewise" : "Reminders are off";
    // Prototype Reminders page (UX19, decided 2026-11-07): the status card title and the footer state.
    public string StatusTitle => Enabled ? "Reminders on" : "Reminders off";
    public bool IsOff => !Enabled;

    private bool Eligible() => !IsBusy && HasLoaded;
    private ReminderSettings Draft(bool enabled)
    {
        static int Minutes(TimeSpan value)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(value));
            return (int)value.TotalMinutes;
        }
        return new(enabled, DayBefore, Minutes(ReminderTime), Minutes(QuietStart), Minutes(QuietEnd), Overdue);
    }
    private void UpdateStatus()
    {
        try { StatusText = platform.GetAccess().Description; }
        catch (Exception) { StatusText = "Android notification status is unavailable. Retry."; }
        try
        {
            var background = platform.GetBackgroundStatus();
            BackgroundText = background.Description;
            MakerStepsText = Enabled ? background.MakerSteps : "";
        }
        catch (Exception)
        {
            BackgroundText = "Android battery/background status is unavailable. Retry or open Android app settings.";
            MakerStepsText = "";
        }
        try
        {
            DueTodayStyle = platform.GetAlertStyle(ReminderChannel.DueToday);
            ComingUpStyle = platform.GetAlertStyle(ReminderChannel.ComingUp);
        }
        catch (Exception) { DueTodayStyle = ComingUpStyle = ""; }
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
            { ReminderTime = TimeSpan.FromMinutes(settings.MinuteOfDay); QuietStart = TimeSpan.FromMinutes(settings.QuietStart); QuietEnd = TimeSpan.FromMinutes(settings.QuietEnd); DayBefore = settings.DayBefore; Overdue = settings.Overdue; }
            Enabled = settings.Enabled; HasLoaded = true; OnPropertyChanged(nameof(CanEdit));
            await coordinator.ReconcileAsync(); UpdateStatus(); Message = coordinator.LastError;
            // AL02: said once, after the old single reminder channel was replaced.
            if (TakeChannelNotice())
                Message = "Reminders now use two Android channels, Due today and Coming up. If you changed the reminder sound before, set it again under Sound & alert style.";
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
    private bool TakeChannelNotice()
    {
        try { return platform.TakeChannelNotice(); }
        catch (Exception) { return false; }
    }
    /// <summary>AL04: a test works before reminders are enabled, so the sound can be heard before deciding.</summary>
    [RelayCommand]
    private async Task TestAsync()
    {
        if (!Eligible()) return;
        try
        {
            if (!platform.GetAccess().Allowed)
            {
                await platform.RequestPermissionAsync();
                if (!platform.GetAccess().Allowed)
                { UpdateStatus(); Message = "Android notifications are blocked. Open Android notification settings to allow them."; return; }
            }
            platform.PostTest(); UpdateStatus();
            Message = "Test reminder sent. It uses the Due today sound. Android may hide alerts while the phone is muted or in Do Not Disturb.";
        }
        catch (Exception) { Message = "Couldn't send the test reminder. Check Android settings and retry."; }
    }
    [RelayCommand]
    private void OpenDueTodaySound() => OpenChannel(ReminderChannel.DueToday, "Due today");
    [RelayCommand]
    private void OpenComingUpSound() => OpenChannel(ReminderChannel.ComingUp, "Coming up");
    private void OpenChannel(ReminderChannel channel, string name)
    {
        try { platform.OpenChannelSettings(channel); }
        catch (Exception) { Message = $"Couldn't open Android settings for {name}. Open Duewise's notification settings in Android instead."; }
    }
    /// <summary>Refreshes Android-owned state after the user returns from Android settings.</summary>
    public void RefreshStatus() { if (HasLoaded) UpdateStatus(); }
    [RelayCommand]
    private void OpenSystemSettings()
    { try { platform.OpenSettings(); } catch (Exception) { Message = "Couldn't open Android notification settings."; } }
    [RelayCommand]
    private void OpenAppSettings()
    { try { platform.OpenAppSettings(); } catch (Exception) { Message = "Couldn't open Android app settings. Open Duewise in Android settings and review battery/background use."; } }
}
