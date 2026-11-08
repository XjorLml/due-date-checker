using global::Android.App;
using global::Android.Content;
using global::Android.OS;
using DueDateChecker.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DueDateChecker.App.Platforms.Android;

internal static class ReminderActionHandler
{
    public const string PayAction = "com.lem.duedatechecker.PAY_REMINDER";
    public const string UndoAction = "com.lem.duedatechecker.UNDO_REMINDER";
    public static bool IsUnlocked(Context context)
    {
        var keyguard = (KeyguardManager)context.GetSystemService(Context.KeyguardService)!;
        return !keyguard.IsDeviceLocked && !keyguard.IsKeyguardLocked;
    }

    public static async Task RunAsync(Context context, Intent? intent)
    {
        if (intent?.Action is not (PayAction or UndoAction) ||
            !Guid.TryParse(intent.GetStringExtra("due_id"), out var id)) return;
        var key = intent.GetStringExtra("occurrence_key"); var version = intent.GetStringExtra("version");
        if (key is null || version is null) return;
        var undo = intent.Action == UndoAction;
        var services = MainApplication.ReminderServices;
        if (services is null) return;
        var platform = (AndroidReminderPlatform)services.GetRequiredService<IReminderPlatform>();
        if (!platform.GetAccess().Allowed) return;
        var token = new ReminderActionToken(id, key, version);
        if (undo)
        {
            var elapsedExpiry = intent.GetLongExtra("elapsed_expiry", 0);
            if (elapsedExpiry <= SystemClock.ElapsedRealtime() || elapsedExpiry - SystemClock.ElapsedRealtime() > 10_000) return;
            try
            {
                token = token with { WindowStartedAt = DateTimeOffset.FromUnixTimeMilliseconds(intent.GetLongExtra("undo_started_at", 0)),
                    ExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(intent.GetLongExtra("expires_at", 0)) };
            }
            catch (ArgumentOutOfRangeException) { return; }
        }
        var result = await services.GetRequiredService<ReminderPaymentService>().ExecuteAsync(token, undo,
            () => IsUnlocked(context) && (!undo || SystemClock.ElapsedRealtime() < intent.GetLongExtra("elapsed_expiry", 0)));
        // Reconcile before posting feedback: the paid due's ordinary reminders are removed first.
        await services.GetRequiredService<ReminderCoordinator>().ReconcileAsync();
        platform.PostResult(token, result, undo);
    }
}

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderActionReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null) return;
        var pending = GoAsync();
        _ = RunAsync(context.ApplicationContext!, intent, pending);
    }
    private static async Task RunAsync(Context context, Intent intent, PendingResult? pending)
    {
        try { await ReminderActionHandler.RunAsync(context, intent); }
        catch (Exception) { /* Never log due content or crash the receiver. A later refresh recovers scheduling. */ }
        finally { pending?.Finish(); }
    }
}

/// <summary>Android 8–11: the action directly launches this activity, so it can request system unlock.
/// No receiver-to-activity trampoline and no main-app navigation are involved.</summary>
[Activity(Exported = false, ExcludeFromRecents = true, NoHistory = true, Theme = "@android:style/Theme.Material.Light.NoActionBar")]
public sealed class ReminderUnlockActivity : Activity
{
    private bool _started;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window?.AddFlags(global::Android.Views.WindowManagerFlags.ShowWhenLocked);
        var message = new global::Android.Widget.TextView(this) { Text = "Unlock to update this due in Duewise." };
        message.SetPadding(24, 48, 24, 24); SetContentView(message);
        if (ReminderActionHandler.IsUnlocked(this)) { _ = ExecuteAsync(); return; }
        var keyguard = (KeyguardManager)GetSystemService(KeyguardService)!;
        keyguard.RequestDismissKeyguard(this, new UnlockCallback(this));
    }
    private async Task ExecuteAsync()
    {
        if (_started) return; _started = true;
        try { await ReminderActionHandler.RunAsync(this, Intent); }
        catch (Exception) { }
        finally { Finish(); }
    }
    private sealed class UnlockCallback(ReminderUnlockActivity activity) : KeyguardManager.KeyguardDismissCallback
    {
        public override void OnDismissSucceeded() => _ = activity.ExecuteAsync();
        public override void OnDismissCancelled() => activity.Finish();
        public override void OnDismissError() => activity.Finish();
    }
}
