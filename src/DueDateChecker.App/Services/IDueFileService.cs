namespace DueDateChecker.App.Services;

/// <summary>
/// The system's own file dialogs for UX05 export/import. The user picks every location, so the app
/// needs no storage permission and never keeps a copy of the file.
/// </summary>
public interface IDueFileService
{
    /// <summary>Shows the save dialog and writes <paramref name="content"/>; false when the user cancels.
    /// Throws when the chosen file cannot be written.</summary>
    Task<bool> SaveAsync(string suggestedName, byte[] content);

    /// <summary>Shows the open dialog and reads the chosen file; null when the user cancels. Reads at most
    /// <paramref name="maxBytes"/> + 1 bytes, so a caller can tell a file is too large without loading it all.</summary>
    Task<byte[]?> OpenAsync(int maxBytes);
}
