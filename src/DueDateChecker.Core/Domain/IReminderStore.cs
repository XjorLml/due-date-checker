namespace DueDateChecker.Core.Domain;

public interface IReminderStore
{
    Task<ReminderSettings> ReadSettingsAsync();
    Task SaveSettingsAsync(ReminderSettings settings);
    Task<IReadOnlyList<ReminderOccurrence>> ReadOccurrencesAsync();
    Task ReplacePendingAsync(IReadOnlyList<ReminderOccurrence> pending);
    Task MarkDeliveredAsync(string key, DateTimeOffset when);
}
