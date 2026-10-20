using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace DueDateChecker.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    { base.OnCreate(savedInstanceState); ReceiveReminder(Intent); }
    protected override void OnNewIntent(Intent? intent)
    { base.OnNewIntent(intent); ReceiveReminder(intent); }
    private static void ReceiveReminder(Intent? intent)
    {
        if (intent?.Action != Platforms.Android.AndroidReminderPlatform.TapAction) return;
        var id = intent.GetStringExtra("due_id"); var key = intent.GetStringExtra("occurrence_key");
        intent.RemoveExtra("due_id"); intent.RemoveExtra("occurrence_key");
        MainThread.BeginInvokeOnMainThread(() => MainApplication.ReminderServices?
            .GetRequiredService<Services.ReminderTapRouter>().Enqueue(id, key));
    }
}
