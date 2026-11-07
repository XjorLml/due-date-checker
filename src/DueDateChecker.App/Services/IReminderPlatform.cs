using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

public sealed record NotificationAccess(bool Allowed, string Description);

/// <param name="Manufacturer">Android <c>Build.MANUFACTURER</c>; empty when unknown.</param>
/// <param name="Brand">Android <c>Build.BRAND</c>. Redmi and POCO phones report the manufacturer
/// Xiaomi with their own brand, so both are checked.</param>
public sealed record BackgroundDeliveryStatus(bool Restricted, bool BatterySaver, bool Optimized,
    string Manufacturer = "", string Brand = "")
{
    private static readonly string[] XiaomiFamily = ["xiaomi", "redmi", "poco"];

    /// <summary>
    /// UX04: extra steps for makers known to stop background work, or empty. Text only: deep links
    /// into maker settings screens change between MIUI and HyperOS versions (decision A, 2026-11-06).
    /// The labels below match MIUI/HyperOS app info as far as known; they still need a phone check.
    /// </summary>
    public string MakerSteps => IsMaker(XiaomiFamily)
        ? "Xiaomi, Redmi and POCO phones can stop reminders running in the background. Tap “Android app / battery settings”, "
          + "turn on Autostart, then open Battery saver and choose “No restrictions”. Names can differ slightly between phone software versions."
        : "";

    private bool IsMaker(string[] names) =>
        names.Contains(Manufacturer.Trim(), StringComparer.OrdinalIgnoreCase) || names.Contains(Brand.Trim(), StringComparer.OrdinalIgnoreCase);

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
    void CleanNotifications(IReadOnlySet<string> validKeys, bool keepTestNotification);
    void CancelDue(Guid id);
    void Post(ReminderOccurrence occurrence);
    void PostTest();
}
