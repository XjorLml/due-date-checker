using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Services;

/// <summary>The deletion an Undo may reverse: which due, and the exact stamp its deletion was saved with.</summary>
public sealed record DueDeletion(Guid Id, DateTimeOffset DeletedAt);

/// <summary>
/// UX06 Undo after Delete (specs/ux06-confirmations-undo). Offers one Undo at a time; only the latest
/// offer can be undone, once, and the repository restores only a row still deleted at that exact stamp.
/// A singleton: the editor that deleted the due is gone by the time the user taps Undo.
/// </summary>
public sealed class DeletionUndo(DatabaseDispatcher database, IDueRepository repository, IMessageHost messages, TimeProvider clock)
{
    private const string NothingToUndo = "Nothing to undo.";
    private readonly object _gate = new();
    private DueDeletion? _offer;

    /// <summary>Raised with the restored due's id so the page on screen can reload. May fire off the UI thread.</summary>
    public event EventHandler<Guid>? Restored;

    public void Offer(DueDeletion deletion)
    {
        lock (_gate) _offer = deletion;
        messages.ShowUndo("Due deleted.", () => UndoAsync(deletion));
    }

    public async Task<bool> UndoAsync(DueDeletion deletion)
    {
        lock (_gate)
        {
            // A second tap, or an older offer that a newer Delete replaced.
            if (_offer != deletion)
            {
                messages.Show(NothingToUndo);
                return false;
            }
            _offer = null;
        }

        bool restored;
        try
        {
            restored = await database.RunAsync(() => repository.RestoreAsync(deletion.Id, deletion.DeletedAt, clock.GetUtcNow()));
        }
        catch (Exception)
        {
            messages.Show("Couldn't undo. The due stays deleted.");
            return false;
        }

        if (!restored)
        {
            // Restored or deleted again elsewhere since: that is no longer this deletion.
            messages.Show(NothingToUndo);
            return false;
        }

        messages.Show("Due restored.");
        Restored?.Invoke(this, deletion.Id);
        return true;
    }
}
