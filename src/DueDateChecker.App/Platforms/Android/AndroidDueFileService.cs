using global::Android.App;
using global::Android.Content;
using DueDateChecker.App.Services;
using AndroidUri = global::Android.Net.Uri;

namespace DueDateChecker.App.Platforms.Android;

/// <summary>
/// UX05 file dialogs through Android's Storage Access Framework: ACTION_CREATE_DOCUMENT to save and
/// ACTION_OPEN_DOCUMENT to open. The user picks the location in the system dialog, which grants access to
/// that one file, so no storage permission or extra package is needed. Bytes go straight between the
/// chosen file and memory through ContentResolver; no copy is kept in app storage.
/// </summary>
/// <remarks>
/// The dialog's answer arrives in the activity's OnActivityResult, wired up in MauiProgram. Only one
/// dialog can be open at a time; if the process is killed while it is open, the answer is lost and the
/// user simply tries again.
/// </remarks>
public sealed class AndroidDueFileService : IDueFileService
{
    private const int SaveRequest = 5301;
    private const int OpenRequest = 5302;
    private static TaskCompletionSource<AndroidUri?>? _pending;
    private static int _pendingRequest;

    private static Context Context => global::Android.App.Application.Context;

    public async Task<bool> SaveAsync(string suggestedName, byte[] content)
    {
        var intent = new Intent(Intent.ActionCreateDocument)
            .AddCategory(Intent.CategoryOpenable)
            .SetType("application/json")
            .PutExtra(Intent.ExtraTitle, suggestedName);
        var uri = await ShowAsync(intent, SaveRequest);
        if (uri is null) return false;

        // The dialog always creates a new, empty document, so plain "w" (write) mode is enough.
        await using var output = Context.ContentResolver!.OpenOutputStream(uri, "w")
            ?? throw new IOException("The chosen file could not be opened for writing.");
        await output.WriteAsync(content);
        await output.FlushAsync();
        return true;
    }

    public async Task<byte[]?> OpenAsync(int maxBytes)
    {
        // Some file apps label .json files as text or as generic binary, so all three are offered.
        var intent = new Intent(Intent.ActionOpenDocument)
            .AddCategory(Intent.CategoryOpenable)
            .SetType("*/*")
            .PutExtra(Intent.ExtraMimeTypes, new[] { "application/json", "text/plain", "application/octet-stream" });
        var uri = await ShowAsync(intent, OpenRequest);
        if (uri is null) return null;

        await using var input = Context.ContentResolver!.OpenInputStream(uri)
            ?? throw new IOException("The chosen file could not be opened for reading.");
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        // Stop one byte past the limit: enough to know the file is too large without reading all of it.
        while (buffer.Length <= maxBytes)
        {
            var read = await input.ReadAsync(chunk.AsMemory(0, (int)Math.Min(chunk.Length, maxBytes + 1 - buffer.Length)));
            if (read == 0) break;
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    /// <summary>Receives every activity result; ignores those this class did not ask for.</summary>
    public static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (_pending is not { } pending || requestCode != _pendingRequest) return;
        _pending = null;
        pending.TrySetResult(resultCode == Result.Ok ? data?.Data : null);
    }

    private static Task<AndroidUri?> ShowAsync(Intent intent, int request)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("There is no screen to show the file dialog on.");
        _pending?.TrySetResult(null); // an earlier dialog that never answered counts as cancelled
        var pending = new TaskCompletionSource<AndroidUri?>(TaskCreationOptions.RunContinuationsAsynchronously);
        (_pending, _pendingRequest) = (pending, request);
        activity.StartActivityForResult(intent, request);
        return pending.Task;
    }
}
