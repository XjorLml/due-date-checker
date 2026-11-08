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
            power.IsPowerSaveMode, !power.IsIgnoringBatteryOptimizations(Context.PackageName!),
            Build.Manufacturer ?? "", Build.Brand ?? "");
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
    private const string Group = "duewise-dues";
    private const string SummaryTag = "duewise-summary";
    public void CleanNotifications(IReadOnlySet<string> validKeys, bool keepTestNotification)
    {
        if (validKeys.Count == 0 && !keepTestNotification) { Manager.CancelAll(); return; }
        foreach (var notification in Manager.GetActiveNotifications() ?? [])
            if (!(keepTestNotification && notification.Tag == "duewise-test") && !validKeys.Contains(notification.Tag ?? "") &&
                !(keepTestNotification && notification.Notification?.Extras?.GetLong("feedback_until", 0) > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
                Manager.Cancel(notification.Tag, notification.Id);
        UpdateGroup();
    }
    public void CancelDue(Guid id)
    {
        foreach (var notification in Manager.GetActiveNotifications() ?? [])
            if (notification.Notification?.Extras?.GetString("due_id") == id.ToString()) Manager.Cancel(notification.Tag, notification.Id);
        UpdateGroup();
    }
    private static PendingIntent TapIntent(ReminderOccurrence? item, string? version = null)
    {
        var intent = new Intent(Context, typeof(MainActivity)).AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        if (item is not null)
        {
            intent.SetAction(TapAction).SetData(global::Android.Net.Uri.Parse("duewise://reminder/" + item.DueId.ToString("N") + "?key=" + Uri.EscapeDataString(item.Key) + "&version=" + version));
            intent.PutExtra("due_id", item.DueId.ToString()).PutExtra("occurrence_key", item.Key);
            if (version is not null) intent.PutExtra("confirmation_version", version);
        }
        return PendingIntent.GetActivity(Context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
    private static Notification PublicVersion(string text) => new Notification.Builder(Context, ChannelId)
        .SetSmallIcon(Resource.Drawable.notification_due).SetContentTitle("Duewise").SetContentText(text)
        .SetVisibility(NotificationVisibility.Public).Build();

    private static Notification.Action Action(ReminderActionToken token, bool undo)
    {
        var modern = OperatingSystem.IsAndroidVersionAtLeast(31);
        var intent = new Intent(Context, modern ? typeof(ReminderActionReceiver) : typeof(ReminderUnlockActivity))
            .SetAction(undo ? ReminderActionHandler.UndoAction : ReminderActionHandler.PayAction)
            .SetData(global::Android.Net.Uri.Parse($"duewise://action/{(undo ? "undo" : "pay")}/{token.DueId:N}/{Uri.EscapeDataString(token.Key)}/{token.Version}"))
            .PutExtra("due_id", token.DueId.ToString()).PutExtra("occurrence_key", token.Key).PutExtra("version", token.Version);
        if (token.WindowStartedAt is { } startedAt && token.ExpiresAt is { } expiry)
            intent.PutExtra("undo_started_at", startedAt.ToUnixTimeMilliseconds()).PutExtra("expires_at", expiry.ToUnixTimeMilliseconds())
                .PutExtra("elapsed_expiry", SystemClock.ElapsedRealtime() + Math.Max(0, expiry.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        var pending = modern
            ? PendingIntent.GetBroadcast(Context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!
            : PendingIntent.GetActivity(Context, 0, intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ExcludeFromRecents), PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
        var builder = new Notification.Action.Builder((global::Android.Graphics.Drawables.Icon?)null, undo ? "Undo" : "Mark as paid", pending);
        if (modern) builder.SetAuthenticationRequired(true);
        return builder.Build();
    }

    public void Post(ReminderOccurrence item, ReminderText text)
    {
        if (!GetAccess().Allowed) throw new InvalidOperationException("Android notifications blocked.");
        // The day-of card replaces yesterday's day-before card; retrying the same key stays idempotent.
        foreach (var previous in Manager.GetActiveNotifications() ?? [])
            if (previous.Tag != item.Key && previous.Notification?.Extras?.GetString("due_id") == item.DueId.ToString())
                Manager.Cancel(previous.Tag, previous.Id);
        var extras = new Bundle(); extras.PutString("due_id", item.DueId.ToString());
        extras.PutString("kind", "reminder"); extras.PutString("public_text", text.PublicBody);
        var notification = new Notification.Builder(Context, ChannelId)
            .SetSmallIcon(Resource.Drawable.notification_due).SetContentTitle(text.Title)
            .SetContentText(text.Body).SetStyle(new Notification.BigTextStyle().BigText(text.Body))
            .SetVisibility(NotificationVisibility.Private).SetPublicVersion(PublicVersion(text.PublicBody))
            .SetGroup(Group).SetGroupAlertBehavior(NotificationGroupAlertBehavior.Children)
            .AddAction(Action(new(item.DueId, item.Key, text.Version), false))
            .SetAutoCancel(true).SetOnlyAlertOnce(true).SetContentIntent(TapIntent(item)).SetExtras(extras).Build();
        Manager.Notify(item.Key, 0, notification);
        UpdateGroup();
    }

    private static void UpdateGroup()
    {
        var reminders = (Manager.GetActiveNotifications() ?? []).Where(n => n.Notification?.Extras?.GetString("kind") == "reminder").ToArray();
        var count = reminders.Select(n => n.Notification!.Extras!.GetString("due_id")).Distinct().Count();
        if (count < 2) { Manager.Cancel(SummaryTag, 0); return; }
        var texts = reminders.Select(n => n.Notification!.Extras!.GetString("public_text")).Distinct().ToArray();
        var text = texts.Length == 1 && texts[0] == "You have a due today" ? $"You have {count} dues today"
            : texts.Length == 1 && texts[0] == "You have a due tomorrow" ? $"You have {count} dues tomorrow" : $"You have {count} due reminders";
        Manager.Notify(SummaryTag, 0, new Notification.Builder(Context, ChannelId)
            .SetSmallIcon(Resource.Drawable.notification_due).SetContentTitle("Duewise").SetContentText(text)
            .SetVisibility(NotificationVisibility.Private).SetPublicVersion(PublicVersion(text)).SetOnlyAlertOnce(true)
            .SetGroup(Group).SetGroupSummary(true).SetGroupAlertBehavior(NotificationGroupAlertBehavior.Children)
            .SetAutoCancel(true).SetContentIntent(TapIntent(null)).Build());
    }

    public void PostResult(ReminderActionToken original, ReminderActionResult result, bool undo)
    {
        EnsureChannel();
        if (!GetAccess().Allowed || result.Status == ReminderActionStatus.Locked) return;
        // A duplicate tap must not replace a still-valid Undo with stale feedback.
        if (result.Status != ReminderActionStatus.Saved && (Manager.GetActiveNotifications() ?? []).Any(n =>
            n.Tag == original.Key && n.Notification?.Extras?.GetString("kind") == "feedback")) return;
        if (result.Undo is { } undoToken)
            result = result with { Undo = undoToken.WithUndoWindow(DateTimeOffset.UtcNow) };
        var due = result.Due;
        var title = due is null ? "Duewise" : $"{due.Title} marked {(undo ? "unpaid" : "paid")}";
        var body = result.Status == ReminderActionStatus.Saved ? result.Next is { } next
            ? $"Next one added for {next.DueDate.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)}."
            : undo ? "Payment undone. Any next occurrence remains in Duewise." : "Saved in Duewise."
            : result.Status == ReminderActionStatus.Failed ? "Couldn't save. Open Duewise to check this due." : "Open Duewise to check this due.";
        var expires = result.Undo?.ExpiresAt ?? DateTimeOffset.UtcNow.AddSeconds(10);
        var extras = new Bundle(); extras.PutString("due_id", original.DueId.ToString()); extras.PutString("kind", "feedback");
        extras.PutLong("feedback_until", expires.ToUnixTimeMilliseconds());
        var item = due is null ? null : new ReminderOccurrence(original.Key, due.Id, due.DueDate, DateTimeOffset.UtcNow);
        var builder = new Notification.Builder(Context, ChannelId).SetSmallIcon(Resource.Drawable.notification_due)
            .SetContentTitle(title).SetContentText(body).SetStyle(new Notification.BigTextStyle().BigText(body))
            .SetVisibility(NotificationVisibility.Private).SetPublicVersion(PublicVersion("Open Duewise to review your dues."))
            .SetOnlyAlertOnce(true).SetAutoCancel(true).SetExtras(extras)
            .SetTimeoutAfter(Math.Max(1, expires.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
            .SetContentIntent(TapIntent(item, due is null ? null : ReminderText.Fingerprint(due)));
        if (result.Undo is { } token) builder.AddAction(Action(token, true));
        Manager.Notify(original.Key, 0, builder.Build()); UpdateGroup();
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
