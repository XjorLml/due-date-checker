using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

public sealed class ReminderCoordinator(DatabaseDispatcher database, IDueRepository repository,
    IReminderStore store, IReminderPlatform platform, TimeProvider clock)
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    public string LastError { get; private set; } = "";
    public int PendingCount { get; private set; }
    public DateTimeOffset? NextAt { get; private set; }
    public Task<ReminderSettings> ReadSettingsAsync() => database.RunAsync(store.ReadSettingsAsync);

    public async Task<bool> SaveSettingsAsync(ReminderSettings settings)
    {
        settings.Validate();
        await database.RunAsync(() => store.SaveSettingsAsync(settings));
        return await ReconcileAsync(); // A later scheduling failure does not undo settings.
    }

    public void AfterWrite(Guid dueId)
    {
        try { platform.CancelDue(dueId); platform.RequestReconciliation(); }
        catch (Exception) { LastError = "Your due is saved, but reminders need refreshing. Open Reminders and retry."; }
    }

    /// <summary>UX05: imported dues have no notifications yet, so only a fresh plan is requested.</summary>
    public void AfterImport()
    {
        try { platform.RequestReconciliation(); }
        catch (Exception) { LastError = "Your dues were imported, but reminders need refreshing. Open Reminders and retry."; }
    }

    public async Task<bool> ReconcileAsync()
    {
        try
        {
            await database.RunAsync(async () =>
            {
                var settings = await store.ReadSettingsAsync();
                var previous = (await store.ReadOccurrencesAsync()).ToDictionary(item => item.Key);
                var dues = await repository.ListActiveAsync();
                // Reads or process suspension can span a clock/date boundary.
                var now = clock.GetUtcNow();
                var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Zone).DateTime);
                var plans = ReminderPlanner.Plan(dues.Where(due => due.DueDate >= today), settings, Zone).ToArray();
                var allowed = settings.Enabled && platform.GetAccess().Allowed;
                platform.CleanNotifications(allowed ? plans.Select(item => item.Key).ToHashSet() : new HashSet<string>(),
                    keepTestNotification: allowed);
                if (!allowed)
                {
                    await store.ReplacePendingAsync([]);
                    platform.Schedule(null); PendingCount = 0; NextAt = null; return;
                }
                var pending = plans.Where(item =>
                    (!previous.TryGetValue(item.Key, out var old) || old.DeliveredAt is null) &&
                    (item.At > now || previous.ContainsKey(item.Key)) &&
                    ReminderPlanner.DeliveryTime(item, now, settings, Zone).HasValue).ToArray();
                await store.ReplacePendingAsync(pending);
                // Gate spans the last eligibility read, posting and receipt. App writes
                // cannot interleave a paid/delete transition inside this operation.
                foreach (var item in pending.Where(item => ReminderPlanner.DeliveryTime(item, now, settings, Zone) <= now).Take(10))
                {
                    var deliveryNow = clock.GetUtcNow();
                    var deliveryAt = ReminderPlanner.DeliveryTime(item, deliveryNow, settings, Zone);
                    if (deliveryAt is null || deliveryAt > deliveryNow) continue;
                    var due = dues.First(due => due.Id == item.DueId);
                    platform.Post(item, ReminderText.Create(due,
                        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(deliveryNow, Zone).DateTime)));
                    await store.MarkDeliveredAsync(item.Key, deliveryNow);
                }
                var remaining = (await store.ReadOccurrencesAsync()).Where(item => item.DeliveredAt is null).ToArray();
                var scheduleNow = clock.GetUtcNow();
                var eligible = remaining.Where(item => ReminderPlanner.DeliveryTime(item, scheduleNow, settings, Zone).HasValue).ToArray();
                if (eligible.Length != remaining.Length) await store.ReplacePendingAsync(eligible);
                remaining = eligible;
                PendingCount = remaining.Length;
                NextAt = remaining.Select(item => ReminderPlanner.DeliveryTime(item, scheduleNow, settings, Zone))
                    .Where(at => at.HasValue).OrderBy(at => at).FirstOrDefault();
                if (NextAt <= scheduleNow) NextAt = scheduleNow.AddMinutes(1);
                platform.Schedule(NextAt);
            });
            LastError = ""; return true;
        }
        catch (Exception)
        {
            LastError = "Reminders couldn't refresh. Saved dues are safe. Check storage and Android notification settings, then retry.";
            try { platform.Schedule(clock.GetUtcNow().AddMinutes(5)); } catch (Exception) { }
            return false;
        }
    }

    public Task<bool> CanOpenAsync(Guid id, string key, string? confirmationVersion = null) => database.RunAsync(async () =>
    {
        var settings = await store.ReadSettingsAsync();
        if (!settings.Enabled) return false;
        var due = await repository.GetAsync(id);
        if (due is null || due.DeletedAt is not null) return false;
        if (confirmationVersion is not null)
            return ReminderText.Fingerprint(due) == confirmationVersion &&
                (await store.ReadOccurrencesAsync()).Any(item => item.Key == key && item.DueId == id && item.DeliveredAt.HasValue);
        if (due.IsPaid) return false;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Zone).DateTime);
        return due.DueDate >= today && (await store.ReadOccurrencesAsync()).Any(item => item.Key == key && item.DueId == id && item.DeliveredAt.HasValue)
            && ReminderPlanner.Plan([due], settings, Zone).Any(item => item.Key == key);
    });
}
