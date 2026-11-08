using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

public enum ReminderActionStatus { Saved, Stale, Locked, Failed }
public sealed record ReminderActionToken(Guid DueId, string Key, string Version,
    DateTimeOffset? WindowStartedAt = null, DateTimeOffset? ExpiresAt = null)
{
    // The native adapter starts the visible window after reconciliation, immediately before posting.
    public ReminderActionToken WithUndoWindow(DateTimeOffset shownAt) => this with
    { WindowStartedAt = shownAt, ExpiresAt = shownAt.AddSeconds(10) };
}
public sealed record ReminderActionResult(ReminderActionStatus Status, Due? Due = null, Due? Next = null,
    ReminderActionToken? Undo = null);

/// <summary>N2: notification actions use the same serialized, guarded writes as Details.</summary>
public sealed class ReminderPaymentService(DatabaseDispatcher database, IDueRepository repository,
    IReminderStore store, TimeProvider clock)
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
    public event EventHandler<Guid>? Changed;

    public async Task<ReminderActionResult> ExecuteAsync(ReminderActionToken token, bool undo, Func<bool> isUnlocked)
    {
        try
        {
            if (!isUnlocked()) return new(ReminderActionStatus.Locked);
            var result = await database.RunAsync(async () =>
            {
                if (token.DueId == Guid.Empty || token.Key.Length > 256 ||
                    !token.Key.StartsWith(token.DueId.ToString("N") + "/", StringComparison.Ordinal) ||
                    token.Version.Length != 64) return new ReminderActionResult(ReminderActionStatus.Stale);
                var settings = await store.ReadSettingsAsync();
                var due = await repository.GetAsync(token.DueId);
                var receipt = (await store.ReadOccurrencesAsync()).FirstOrDefault(item =>
                    item.Key == token.Key && item.DueId == token.DueId && item.DeliveredAt.HasValue);
                if (!settings.Enabled || due is null || due.DeletedAt.HasValue || receipt is null ||
                    ReminderText.Fingerprint(due) != token.Version) return new(ReminderActionStatus.Stale);
                var now = clock.GetUtcNow();
                var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Zone).DateTime);
                if (undo)
                {
                    if (!due.IsPaid || token.WindowStartedAt is not { } startedAt || token.ExpiresAt is not { } expiry ||
                        expiry - startedAt != TimeSpan.FromSeconds(10) || now < startedAt || now >= expiry)
                        return new(ReminderActionStatus.Stale);
                }
                else if (due.IsPaid || due.DueDate < today || receipt.DueDate != due.DueDate ||
                    !ReminderPlanner.Plan([due], settings, Zone).Any(item => item.Key == token.Key))
                    return new(ReminderActionStatus.Stale);

                // This runs after all asynchronous reads, immediately before the serialized write.
                if (!isUnlocked()) return new(ReminderActionStatus.Locked);
                var saved = due with { IsPaid = !undo, UpdatedAt = now };
                Due? next = null;
                if (Recurrence.NextOnPayment(due, saved, now) is { } candidate)
                {
                    if (await repository.UpdateWithNextAsync(saved, candidate)) next = candidate;
                }
                else await repository.UpdateAsync(saved);
                var undoToken = undo ? null : new ReminderActionToken(saved.Id, token.Key,
                    ReminderText.Fingerprint(saved), now, now.AddSeconds(10));
                return new(ReminderActionStatus.Saved, saved, next, undoToken);
            });
            if (result.Status == ReminderActionStatus.Saved)
            {
                // UI refresh is best effort and must never turn a committed write into a failed action.
                try { Changed?.Invoke(this, token.DueId); } catch (Exception) { }
            }
            return result;
        }
        catch (Exception) { return new(ReminderActionStatus.Failed); }
    }
}
