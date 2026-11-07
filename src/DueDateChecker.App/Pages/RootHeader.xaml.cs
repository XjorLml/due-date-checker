namespace DueDateChecker.App.Pages;

public partial class RootHeader : ContentView
{
    private static bool _opening;

    public RootHeader() => InitializeComponent();

    /// <summary>Raised instead of navigating when a page needs its own guard (for example, an unopened database).</summary>
    public event EventHandler? RemindersClicked;

    private async void OnRemindersClicked(object? sender, EventArgs e)
    {
        if (RemindersClicked is not null) { RemindersClicked(this, EventArgs.Empty); return; }
        if (_opening) return;
        _opening = true;
        try { await Shell.Current.GoToAsync(nameof(ReminderSettingsPage)); }
        catch (Exception) { } // the page on screen stays usable; a second tap retries
        finally { _opening = false; }
    }
}
