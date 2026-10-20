using global::Android.App;
using global::Android.App.Job;
using global::Android.Content;
using DueDateChecker.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DueDateChecker.App.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced, Intent.ActionTimeChanged, Intent.ActionTimezoneChanged, Intent.ActionUserUnlocked })]
public sealed class ReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        // Return immediately; the OS owns the job lifetime and retry policy.
        try { AndroidReminderPlatform.Enqueue(); } catch (Exception) { }
    }
}

[Service(Permission = "android.permission.BIND_JOB_SERVICE", Exported = true)]
public sealed class ReminderJobService : JobService
{
    private readonly Dictionary<int, JobParameters> _running = [];
    public override bool OnStartJob(JobParameters? parameters)
    {
        if (parameters is null) return false;
        _running[parameters.JobId] = parameters;
        _ = RunAsync(parameters); return true;
    }
    private async Task RunAsync(JobParameters parameters)
    {
        var ok = false;
        try
        {
            var coordinator = MainApplication.ReminderServices?.GetRequiredService<ReminderCoordinator>();
            if (coordinator is not null) ok = await coordinator.ReconcileAsync();
        }
        catch (Exception) { }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_running.TryGetValue(parameters.JobId, out var current) && ReferenceEquals(current, parameters))
            { _running.Remove(parameters.JobId); JobFinished(parameters, !ok); }
        });
    }
    public override bool OnStopJob(JobParameters? parameters)
    {
        if (parameters is not null) _running.Remove(parameters.JobId);
        return true; // Already committed SQLite operations are not rolled back.
    }
}
