namespace DueDateChecker.App.Services;

// Quota rejection is a normal outcome for expedited jobs. Fall back using the
// same Android job ID so that one reconciliation request cannot fan out.
public static class ReminderJobDispatch
{
    public static void Enqueue(bool supportsExpedited, Func<bool, bool> trySchedule)
    {
        if (supportsExpedited && trySchedule(true)) return;
        if (!trySchedule(false)) throw new InvalidOperationException("Reminder job unavailable.");
    }
}
