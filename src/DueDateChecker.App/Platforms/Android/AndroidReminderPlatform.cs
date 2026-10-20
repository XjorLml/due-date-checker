using global::Android.App;
using global::Android.App.Job;
using global::Android.Content;
using global::Android.OS;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Platforms.Android;

public sealed class NotificationPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(33) ? [(global::Android.Manifest.Permission.PostNotifications, true)] : [];
}

public sealed class AndroidReminderPlatform : IReminderPlatform
{
    public const string ChannelId = "duewise_reminders_v1";
    public const int ImmediateJob = 5205;
    public const int RecoveryJob = 5206;
    public const string TapAction = "com.lem.duedatechecker.OPEN_REMINDER";
    private static Context Context => global::Android.App.Application.Context;
    private static NotificationManager Manager => (NotificationManager)Context.GetSystemService(Context.NotificationService)!;
    private static JobScheduler Jobs => (JobScheduler)Context.GetSystemService(Context.JobSchedulerService)!;
    private static void EnsureChannel() => Manager.CreateNotificationChannel(new NotificationChannel(ChannelId, "Due reminders", NotificationImportance.Default)
    { Description = "Local reminders for unpaid due dates" });

    public NotificationAccess GetAccess()
    {
        EnsureChannel();
        if (OperatingSystem.IsAndroidVersionAtLeast(33) && Context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != global::Android.Content.PM.Permission.Granted)
            return new(false, "Android notification permission is not allowed.");
        if (!Manager.AreNotificationsEnabled()) return new(false, "Notifications for Duewise are blocked in Android settings.");
        if (Manager.GetNotificationChannel(ChannelId)?.Importance == NotificationImportance.None)
            return new(false, "The Due reminders channel is blocked in Android settings.");
        return new(true, "Android notification permission and Due reminders channel are allowed.");
    }
    public async Task RequestPermissionAsync()
    { EnsureChannel(); await Permissions.RequestAsync<NotificationPermission>(); }
    public BackgroundDeliveryStatus GetBackgroundStatus()
    {
        var activity = (ActivityManager)Context.GetSystemService(Context.ActivityService)!;
        var power = (PowerManager)Context.GetSystemService(Context.PowerService)!;
        return new(OperatingSystem.IsAndroidVersionAtLeast(28) && activity.IsBackgroundRestricted,
            power.IsPowerSaveMode, !power.IsIgnoringBatteryOptimizations(Context.PackageName!));
    }
    public void OpenAppSettings() => Context.StartActivity(
        new Intent(global::Android.Provider.Settings.ActionApplicationDetailsSettings,
            global::Android.Net.Uri.Parse("package:" + Context.PackageName)).AddFlags(ActivityFlags.NewTask));
    public void OpenSettings()
    {
        var intent = new Intent(global::Android.Provider.Settings.ActionChannelNotificationSettings)
            .PutExtra(global::Android.Provider.Settings.ExtraAppPackage, Context.PackageName)
            .PutExtra(global::Android.Provider.Settings.ExtraChannelId, ChannelId).AddFlags(ActivityFlags.NewTask);
        Context.StartActivity(intent);
    }
    private static PendingIntent AlarmIntent() => PendingIntent.GetBroadcast(Context, 5205,
        new Intent(Context, typeof(ReminderReceiver)).SetAction("com.lem.duedatechecker.REMINDER_ALARM"),
        PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    public void Schedule(DateTimeOffset? next)
    {
        var alarm = (AlarmManager)Context.GetSystemService(Context.AlarmService)!;
        alarm.Cancel(AlarmIntent());
        if (next is null) { Jobs.Cancel(RecoveryJob); return; }
        alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, next.Value.ToUnixTimeMilliseconds(), AlarmIntent());
        if (!Jobs.AllPendingJobs.Any(job => job.Id == RecoveryJob))
        {
            using var builder = new JobInfo.Builder(RecoveryJob, new ComponentName(Context, Java.Lang.Class.FromType(typeof(ReminderJobService))));
            builder.SetPeriodic(15 * 60 * 1000L);
            builder.SetPersisted(true);
            var job = builder.Build();
            if (Jobs.Schedule(job!) != JobScheduler.ResultSuccess) throw new InvalidOperationException("Reminder recovery job unavailable.");
        }
    }
    public void RequestReconciliation() => Enqueue();
    public static void Enqueue() => ReminderJobDispatch.Enqueue(OperatingSystem.IsAndroidVersionAtLeast(31), expedited =>
    {
        using var builder = new JobInfo.Builder(ImmediateJob, new ComponentName(Context, Java.Lang.Class.FromType(typeof(ReminderJobService))));
        if (expedited && OperatingSystem.IsAndroidVersionAtLeast(31)) builder.SetExpedited(true);
        else builder.SetOverrideDeadline(0);
        var job = builder.Build();
        return Jobs.Schedule(job!) == JobScheduler.ResultSuccess;
    });
    public void CleanNotifications(IReadOnlySet<string> validKeys)
    {
        if (validKeys.Count == 0) { Manager.CancelAll(); return; }
        foreach (var notification in Manager.GetActiveNotifications() ?? [])
            if (notification.Tag != "duewise-test" && !validKeys.Contains(notification.Tag ?? "")) Manager.Cancel(notification.Tag, notification.Id);
    }
    public void CancelDue(Guid id)
    {
        foreach (var notification in Manager.GetActiveNotifications() ?? [])
            if (notification.Notification?.Extras?.GetString("due_id") == id.ToString()) Manager.Cancel(notification.Tag, notification.Id);
    }
    private static PendingIntent TapIntent(ReminderOccurrence? item)
    {
        var intent = new Intent(Context, typeof(MainActivity)).AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        if (item is not null)
        {
            intent.SetAction(TapAction).SetData(global::Android.Net.Uri.Parse("duewise://reminder/" + item.DueId.ToString("N") + "?key=" + Uri.EscapeDataString(item.Key)));
            intent.PutExtra("due_id", item.DueId.ToString()).PutExtra("occurrence_key", item.Key);
        }
        return PendingIntent.GetActivity(Context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
    public void Post(ReminderOccurrence item)
    {
        if (!GetAccess().Allowed) throw new InvalidOperationException("Android notifications blocked.");
        var extras = new Bundle(); extras.PutString("due_id", item.DueId.ToString());
        var notification = new Notification.Builder(Context, ChannelId)
            .SetSmallIcon(Resource.Drawable.notification_due).SetContentTitle("Duewise reminder")
            .SetContentText("Open Duewise to review an unpaid due date.").SetVisibility(NotificationVisibility.Private)
            .SetAutoCancel(true).SetOnlyAlertOnce(true).SetContentIntent(TapIntent(item)).SetExtras(extras).Build();
        Manager.Notify(item.Key, 0, notification);
    }
    public void PostTest()
    {
        if (!GetAccess().Allowed) throw new InvalidOperationException("Android notifications blocked.");
        Manager.Notify("duewise-test", 0, new Notification.Builder(Context, ChannelId)
            .SetSmallIcon(Resource.Drawable.notification_due).SetContentTitle("Duewise test")
            .SetContentText("Reminders can show notifications on this device.").SetAutoCancel(true)
            .SetVisibility(NotificationVisibility.Private).SetContentIntent(TapIntent(null)).Build());
    }
}
