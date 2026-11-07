namespace DueDateChecker.App.Services;

/// <summary>
/// UX06 confirmation messages above the bottom bar (DESIGN toasts/snackbars). A new message replaces the
/// one on screen. Screen readers hear each message from the message itself, so callers must not also
/// announce it.
/// </summary>
public interface IMessageHost
{
    /// <summary>A short confirmation shown for about 3 seconds.</summary>
    void Show(string message);

    /// <summary>A message with an Undo action, shown for 5 seconds (longer if the user set a longer
    /// accessibility timeout). <paramref name="undo"/> runs at most once, when the user taps Undo.</summary>
    void ShowUndo(string message, Func<Task<bool>> undo);
}
