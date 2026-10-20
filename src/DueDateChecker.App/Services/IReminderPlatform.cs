using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

public sealed record NotificationAccess(bool Allowed, string Description);

public sealed record BackgroundDeliveryStatus(bool Restricted, bool BatterySaver, bool Optimized)
{
    public string Description => Restricted
        ? "Android restricts background activity for Duewise. Review background use in Android app settings."
        : BatterySaver
            ? "Android Battery Saver is on. Background reminders may be delayed."
            : Optimized
                ? "Battery optimization is active. Review Android app battery settings if reminders arrive late."
                : "Battery optimization is off for Duewise. Other device restrictions may still delay reminders.";
}

public interface IReminderPlatform
{
    NotificationAccess GetAccess();
    BackgroundDeliveryStatus GetBackgroundStatus();
    Task RequestPermissionAsync();
    void OpenSettings();
    void OpenAppSettings();
    void Schedule(DateTimeOffset? next);
    void RequestReconciliation();
    void CleanNotifications(IReadOnlySet<string> validKeys);
    void CancelDue(Guid id);
    void Post(ReminderOccurrence occurrence);
    void PostTest();
}
