using DueDateChecker.App.Pages;

namespace DueDateChecker.App.Services;

public sealed class ReminderTapRouter(ReminderCoordinator coordinator)
{
    private (Guid Id, string Key)? _pending;
    private bool _opening;
    public event Action? Queued;
    public void Enqueue(string? idText, string? key)
    {
        if (!Guid.TryParse(idText, out var id) || id == Guid.Empty || key is null || key.Length > 256 || !key.StartsWith(id.ToString("N") + "/", StringComparison.Ordinal)) return;
        _pending = (id, key); Queued?.Invoke();
    }
    public async Task TryOpenAsync(AppShell shell)
    {
        if (_opening || _pending is not { } pending || shell.CurrentPage is DueEditPage or SettingsPage) return;
        _opening = true;
        try
        {
            if (!await coordinator.CanOpenAsync(pending.Id, pending.Key)) { if (_pending == pending) _pending = null; return; }
            if (shell.CurrentPage is DueEditPage or SettingsPage) return;
            await shell.GoToAsync("//main/dues/list");
            await shell.GoToAsync($"{nameof(DueDetailPage)}?id={pending.Id:D}");
            if (_pending == pending) _pending = null;
        }
        catch (Exception) { /* Keep the tap for the next navigation/resume retry. */ }
        finally { _opening = false; }
    }
}
