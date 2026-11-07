using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DueDateChecker.App.Services;
using DueDateChecker.Core.Data;

namespace DueDateChecker.App.ViewModels;

/// <summary>
/// UX05 "Your data" on Settings: export every due to a file the user chooses, and import such a file
/// while no dues show (specs/f04-export-import). Import is two steps so the page can confirm in between:
/// <see cref="ChooseImportFileCommand"/> reads and validates, <see cref="ImportCommand"/> writes.
/// </summary>
public partial class DataTransferViewModel(DatabaseDispatcher database, IDueArchiveStore archive, IDueFileService files,
    TimeProvider clock, ReminderCoordinator? reminders = null) : ObservableObject
{
    private const string NothingImported = "Nothing was imported.";
    private const string BlockedMessage = "Import works only when Duewise shows no dues. " + NothingImported;
    private const string CancelledMessage = "Import cancelled. " + NothingImported;
    private DueArchiveContents? _pending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAct))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    private string _message = "";

    [ObservableProperty] private bool _hasPendingImport;
    [ObservableProperty] private string _importConfirmationMessage = "";

    public bool CanAct => !IsBusy;
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    /// <summary>Suggested file name, dated in Asia/Manila like every other date in the app.</summary>
    public string ExportFileName => $"duewise-dues-{ManilaCalendar.Today(clock):yyyy-MM-dd}.json";

    [RelayCommand]
    private async Task ExportAsync()
    {
        // Also guards direct ExecuteAsync calls while a dialog is open.
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var dues = await database.RunAsync(archive.ReadAllAsync);
            if (dues.Count == 0)
            {
                Message = "There are no dues to export yet.";
                return;
            }
            var file = DueArchive.Write(dues, clock.GetUtcNow());
            if (!await files.SaveAsync(ExportFileName, file))
            {
                Message = "Export cancelled. No file was saved.";
                return;
            }
            var deleted = dues.Count(due => due.IsDeleted);
            Message = deleted == 0
                ? $"Exported {Count(dues.Count)} to the file you chose."
                : $"Exported {Count(dues.Count)}, including {deleted} deleted, to the file you chose.";
        }
        catch (Exception)
        {
            Message = "Couldn't save the export. Nothing in Duewise changed. If an empty file was created, delete it and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ChooseImportFileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearPending();
        try
        {
            // Checked before the dialog so the user is not asked to pick a file that cannot be used.
            if (await database.RunAsync(archive.CountActiveAsync) > 0)
            {
                Message = BlockedMessage;
                return;
            }
            byte[]? file;
            try
            {
                file = await files.OpenAsync(DueArchive.MaxBytes);
            }
            catch (Exception)
            {
                Message = "Couldn't read that file. " + NothingImported + " Try again.";
                return;
            }
            if (file is null)
            {
                Message = CancelledMessage;
                return;
            }

            var contents = DueArchive.Read(file);
            _pending = contents;
            var exported = TimeZoneInfo.ConvertTime(contents.ExportedAt, ManilaCalendar.Zone)
                .ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            var hidden = contents.DeletedCount switch
            {
                0 => "",
                1 => "; 1 deleted due stays hidden",
                var n => $"; {n} deleted dues stay hidden",
            };
            ImportConfirmationMessage = $"Import {Count(contents.Dues.Count)} from a file exported on {exported}? "
                + $"{contents.ActiveCount} will show in your list{hidden}.";
            HasPendingImport = true;
            Message = "";
        }
        catch (DueArchiveException problem)
        {
            Message = Explain(problem);
        }
        catch (Exception)
        {
            Message = "Couldn't check whether Duewise has dues. " + NothingImported + " Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Writes the chosen file after the page's confirmation; false (Cancel) writes nothing.</summary>
    [RelayCommand]
    private async Task ImportAsync(bool confirmed)
    {
        if (IsBusy || _pending is not { } contents) return;
        ClearPending();
        if (!confirmed)
        {
            Message = CancelledMessage;
            return;
        }

        IsBusy = true;
        try
        {
            // The store checks again inside its transaction: a due added after choosing the file blocks it.
            await database.RunAsync(() => archive.ImportAsync(contents.Dues));
            reminders?.AfterImport();
            Message = $"Imported {Count(contents.Dues.Count)}. {contents.ActiveCount} now show in your list.";
        }
        catch (DueArchiveException problem)
        {
            Message = Explain(problem);
        }
        catch (Exception)
        {
            Message = "Couldn't import. " + NothingImported + " Check available storage and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearPending()
    {
        _pending = null;
        HasPendingImport = false;
        ImportConfirmationMessage = "";
    }

    private static string Count(int dues) => dues == 1 ? "1 due" : $"{dues} dues";

    private static string Explain(DueArchiveException problem) => problem.Problem switch
    {
        DueArchiveProblem.NewerVersion => "This file comes from a newer version of Duewise. Update the app, then import it. " + NothingImported,
        DueArchiveProblem.TooLarge => $"This file is larger than {DueArchive.MaxBytes / (1024 * 1024)} MB, too large for a Duewise export. " + NothingImported,
        DueArchiveProblem.NoDues => "This file has no dues in it. " + NothingImported,
        DueArchiveProblem.DuplicateId => $"Due {problem.DueNumber} in this file repeats another due's ID. " + NothingImported,
        DueArchiveProblem.InvalidDue => InvalidDue(problem),
        DueArchiveProblem.AppNotEmpty => BlockedMessage,
        _ => "This file isn't a Duewise export. " + NothingImported,
    };

    private static string InvalidDue(DueArchiveException problem)
    {
        var what = problem.Field switch
        {
            "id" => "an invalid ID",
            "title" => "an invalid title",
            "amountCentavos" => "an invalid amount",
            "currency" => "an invalid currency",
            "dueDate" => "an invalid due date",
            "notes" => "invalid notes",
            "isPaid" => "an invalid paid status",
            "timezone" => "an invalid time zone",
            "createdAt" => "an invalid creation time",
            "updatedAt" => "an invalid last-updated time",
            "deletedAt" => "an invalid deletion time",
            "category" => "an invalid type",
            "repeatRule" => "an invalid repeat",
            "repeatDay" => "an invalid repeat day",
            "repeatSourceId" => "an invalid repeat link",
            null => "invalid content",
            _ => "an unexpected field",
        };
        return $"Due {problem.DueNumber} in this file has {what}. " + NothingImported;
    }
}
