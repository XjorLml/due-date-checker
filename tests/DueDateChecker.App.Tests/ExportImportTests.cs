using System.Text;
using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX05 export and import on Settings (specs/f04-export-import EX01–EX08).</summary>
public sealed class ExportImportTests : IDisposable
{
    // 17:00 UTC on 05 Nov is 01:00 on 06 Nov in Asia/Manila: the file name must use the Manila date.
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 17, 0, 0, TimeSpan.Zero);
    private const string Blocked = "Import works only when Duewise shows no dues. Nothing was imported.";
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-import-{Guid.NewGuid():N}.db");
    private readonly string _otherPath = Path.Combine(Path.GetTempPath(), $"app-export-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly FakeFiles _files = new();
    private readonly FakePlatform _platform = new();

    public ExportImportTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
    }

    public void Dispose()
    {
        foreach (var path in new[] { _path, _otherPath })
        {
            TestDatabase.ClearPool(path);
            File.Delete(path);
        }
    }

    private DataTransferViewModel NewViewModel(IDueArchiveStore? archive = null) =>
        new(_database, archive ?? new SqliteDueArchive(_path), _files, new FixedClock(Now),
            new ReminderCoordinator(_database, _repository, new SqliteReminderStore(_path), _platform, new FixedClock(Now)));

    private static Due Bill(string title, int day) => Due.Create(title, Money.FromCentavos(150000), new DateOnly(2026, 11, day), Now, null, DueCategory.Bill);

    // A file made by the real exporter from another database: two showing dues and one deleted.
    private async Task<byte[]> ExportFileAsync()
    {
        new DatabaseInitializer(_otherPath).Initialize();
        var other = new SqliteDueRepository(_otherPath);
        await other.AddAsync(Bill("Rent", 30));
        await other.AddAsync(Due.Create("Renew ID", null, new DateOnly(2026, 12, 15), Now, category: DueCategory.Task));
        var gone = Bill("Old gym", 10);
        await other.AddAsync(gone);
        await other.SoftDeleteAsync(gone.Id, Now);
        return DueArchive.Write(await new SqliteDueArchive(_otherPath).ReadAllAsync(), new DateTimeOffset(2026, 11, 4, 20, 0, 0, TimeSpan.Zero));
    }

    // ---- EX01: export ----

    [Fact]
    public async Task Export_saves_every_due_under_a_dated_name_and_reports_the_counts()
    {
        await _repository.AddAsync(Bill("Rent", 30));
        await _repository.AddAsync(Bill("Water", 20));
        var gone = Bill("Old gym", 10);
        await _repository.AddAsync(gone);
        await _repository.SoftDeleteAsync(gone.Id, Now);
        var vm = NewViewModel();

        await vm.ExportCommand.ExecuteAsync(null);

        Assert.Equal("duewise-dues-2026-11-06.json", _files.SavedName);
        var contents = DueArchive.Read(_files.Saved!);
        Assert.Equal((3, 1), (contents.Dues.Count, contents.DeletedCount));
        Assert.Equal(Now, contents.ExportedAt);
        Assert.Equal("Exported 3 dues, including 1 deleted, to the file you chose.", vm.Message);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Export_with_nothing_stored_opens_no_dialog_and_a_cancelled_save_says_so()
    {
        var vm = NewViewModel();
        await vm.ExportCommand.ExecuteAsync(null);
        Assert.Equal(0, _files.SaveCalls);
        Assert.Equal("There are no dues to export yet.", vm.Message);

        await _repository.AddAsync(Bill("Rent", 30));
        _files.SaveResult = false;
        await vm.ExportCommand.ExecuteAsync(null);
        Assert.Equal(1, _files.SaveCalls);
        Assert.Equal("Export cancelled. No file was saved.", vm.Message);

        _files.SaveResult = true;
        await vm.ExportCommand.ExecuteAsync(null);
        Assert.Equal("Exported 1 due to the file you chose.", vm.Message);
    }

    [Fact]
    public async Task A_failed_save_is_reported_and_changes_nothing()
    {
        await _repository.AddAsync(Bill("Rent", 30));
        _files.FailSave = true;
        var vm = NewViewModel();

        await vm.ExportCommand.ExecuteAsync(null);

        Assert.Equal("Couldn't save the export. Nothing in Duewise changed. If an empty file was created, delete it and try again.", vm.Message);
        Assert.Single(await _repository.ListActiveAsync());
        Assert.False(vm.IsBusy);
    }

    // ---- EX04, EX05, EX07: import ----

    [Fact]
    public async Task Import_confirms_then_writes_every_due_and_refreshes_reminders()
    {
        _files.OpenResult = await ExportFileAsync();
        var vm = NewViewModel();

        await vm.ChooseImportFileCommand.ExecuteAsync(null);

        Assert.True(vm.HasPendingImport);
        Assert.Equal("Import 3 dues from a file exported on 05 Nov 2026? 2 will show in your list; 1 deleted due stays hidden.", vm.ImportConfirmationMessage);
        Assert.Empty(await _repository.ListActiveAsync()); // nothing written before confirming
        var requests = _platform.Requests;

        await vm.ImportCommand.ExecuteAsync(true);

        Assert.False(vm.HasPendingImport);
        Assert.Equal("Imported 3 dues. 2 now show in your list.", vm.Message);
        Assert.Equal(new[] { "Renew ID", "Rent" }, (await _repository.ListActiveAsync()).Select(due => due.Title).Order(StringComparer.Ordinal));
        Assert.Equal(await new SqliteDueArchive(_otherPath).ReadAllAsync(), await new SqliteDueArchive(_path).ReadAllAsync());
        Assert.True(_platform.Requests > requests);
    }

    [Fact]
    public async Task Import_is_refused_before_the_file_dialog_while_dues_show()
    {
        await _repository.AddAsync(Bill("Rent", 30));
        var vm = NewViewModel();

        await vm.ChooseImportFileCommand.ExecuteAsync(null);

        Assert.Equal(0, _files.OpenCalls);
        Assert.Equal(Blocked, vm.Message);
        Assert.False(vm.HasPendingImport);
    }

    [Fact]
    public async Task Hidden_deleted_dues_do_not_block_an_import()
    {
        var gone = Bill("Deleted here", 3);
        await _repository.AddAsync(gone);
        await _repository.SoftDeleteAsync(gone.Id, Now);
        _files.OpenResult = await ExportFileAsync();
        var vm = NewViewModel();

        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        await vm.ImportCommand.ExecuteAsync(true);

        Assert.Equal(2, (await _repository.ListActiveAsync()).Count);
        Assert.NotNull(await _repository.GetAsync(gone.Id)); // the hidden one stays
    }

    [Fact]
    public async Task Cancelling_the_dialog_or_the_confirmation_writes_nothing()
    {
        var vm = NewViewModel();
        _files.OpenResult = null;
        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        Assert.Equal("Import cancelled. Nothing was imported.", vm.Message);
        Assert.False(vm.HasPendingImport);

        _files.OpenResult = await ExportFileAsync();
        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        await vm.ImportCommand.ExecuteAsync(false);

        Assert.Equal("Import cancelled. Nothing was imported.", vm.Message);
        Assert.False(vm.HasPendingImport);
        Assert.Empty(await new SqliteDueArchive(_path).ReadAllAsync());
        await vm.ImportCommand.ExecuteAsync(true); // nothing pending any more
        Assert.Empty(await new SqliteDueArchive(_path).ReadAllAsync());
    }

    public static TheoryData<string, string> RefusedFiles => new()
    {
        { "not json", "This file isn't a Duewise export. Nothing was imported." },
        { """{ "format": "duewise.dues", "version": 2 }""", "This file comes from a newer version of Duewise. Update the app, then import it. Nothing was imported." },
        { """{ "format": "duewise.dues", "version": 1, "exportedAt": "2026-11-06T01:00:00.0000000+00:00", "dues": [] }""", "This file has no dues in it. Nothing was imported." },
        { "DUE2:dueDate", "Due 2 in this file has an invalid due date. Nothing was imported." },
        { "DUE2:amountCentavos", "Due 2 in this file has an invalid amount. Nothing was imported." },
        { "DUE2:surprise", "Due 2 in this file has an unexpected field. Nothing was imported." },
        { "DUE2:id", "Due 2 in this file repeats another due's ID. Nothing was imported." },
    };

    [Theory]
    [MemberData(nameof(RefusedFiles))]
    public async Task Each_refused_file_says_why_and_writes_nothing(string file, string message)
    {
        _files.OpenResult = file.StartsWith("DUE2:") ? Spoil(await ExportFileAsync(), file[5..]) : Encoding.UTF8.GetBytes(file);
        var vm = NewViewModel();

        await vm.ChooseImportFileCommand.ExecuteAsync(null);

        Assert.Equal(message, vm.Message);
        Assert.False(vm.HasPendingImport);
        Assert.Empty(await new SqliteDueArchive(_path).ReadAllAsync());
    }

    [Fact]
    public async Task An_oversize_file_is_refused()
    {
        _files.OpenResult = new byte[DueArchive.MaxBytes + 1];
        var vm = NewViewModel();

        await vm.ChooseImportFileCommand.ExecuteAsync(null);

        Assert.Equal(DueArchive.MaxBytes, _files.LastMaxBytes);
        Assert.Equal("This file is larger than 5 MB, too large for a Duewise export. Nothing was imported.", vm.Message);
    }

    [Fact]
    public async Task An_unreadable_file_or_a_failed_write_changes_nothing()
    {
        var vm = NewViewModel(new FailingArchive(new SqliteDueArchive(_path)));
        _files.FailOpen = true;
        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        Assert.Equal("Couldn't read that file. Nothing was imported. Try again.", vm.Message);

        _files.FailOpen = false;
        _files.OpenResult = await ExportFileAsync();
        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        await vm.ImportCommand.ExecuteAsync(true);

        Assert.Equal("Couldn't import. Nothing was imported. Check available storage and try again.", vm.Message);
        Assert.Empty(await new SqliteDueArchive(_path).ReadAllAsync());
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task A_due_added_before_confirming_blocks_the_import()
    {
        _files.OpenResult = await ExportFileAsync();
        var vm = NewViewModel();
        await vm.ChooseImportFileCommand.ExecuteAsync(null);
        var added = Bill("Added meanwhile", 12);
        await _repository.AddAsync(added);

        await vm.ImportCommand.ExecuteAsync(true);

        Assert.Equal(Blocked, vm.Message);
        Assert.Equal(added, Assert.Single(await _repository.ListActiveAsync()));
    }

    [Fact]
    public async Task Repeated_taps_while_a_dialog_is_open_start_nothing_more()
    {
        await _repository.AddAsync(Bill("Rent", 30));
        _files.Gate = new TaskCompletionSource();
        var vm = NewViewModel();

        var first = vm.ExportCommand.ExecuteAsync(null);
        Assert.True(vm.IsBusy);
        Assert.False(vm.CanAct);
        var second = vm.ExportCommand.ExecuteAsync(null);
        var import = vm.ChooseImportFileCommand.ExecuteAsync(null);
        _files.Gate.SetResult();
        await Task.WhenAll(first, second, import);

        Assert.Equal(1, _files.SaveCalls);
        Assert.Equal(0, _files.OpenCalls);
    }

    private static byte[] Spoil(byte[] file, string field)
    {
        var json = System.Text.Json.Nodes.JsonNode.Parse(file)!.AsObject();
        var dues = json["dues"]!.AsArray();
        var second = dues[1]!.AsObject();
        switch (field)
        {
            case "dueDate": second["dueDate"] = "2026-02-30"; break;
            case "amountCentavos": second["amountCentavos"] = -5; break;
            case "surprise": second["surprise"] = 1; break;
            case "id": second["id"] = dues[0]!["id"]!.GetValue<string>(); break;
        }
        return Encoding.UTF8.GetBytes(json.ToJsonString());
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeFiles : IDueFileService
    {
        public TaskCompletionSource? Gate { get; set; }
        public bool SaveResult { get; set; } = true;
        public bool FailSave { get; set; }
        public bool FailOpen { get; set; }
        public byte[]? OpenResult { get; set; }
        public string? SavedName { get; private set; }
        public byte[]? Saved { get; private set; }
        public int SaveCalls { get; private set; }
        public int OpenCalls { get; private set; }
        public int LastMaxBytes { get; private set; }

        public async Task<bool> SaveAsync(string suggestedName, byte[] content)
        {
            SaveCalls++;
            if (Gate is not null) await Gate.Task;
            if (FailSave) throw new IOException("Controlled save failure");
            if (!SaveResult) return false;
            (SavedName, Saved) = (suggestedName, content);
            return true;
        }

        public async Task<byte[]?> OpenAsync(int maxBytes)
        {
            OpenCalls++;
            LastMaxBytes = maxBytes;
            if (Gate is not null) await Gate.Task;
            if (FailOpen) throw new IOException("Controlled read failure");
            return OpenResult;
        }
    }

    private sealed class FailingArchive(IDueArchiveStore inner) : IDueArchiveStore
    {
        public Task<IReadOnlyList<DueRecord>> ReadAllAsync() => inner.ReadAllAsync();
        public Task<int> CountActiveAsync() => inner.CountActiveAsync();
        public Task ImportAsync(IReadOnlyList<DueRecord> dues) => throw new IOException("Controlled storage failure");
    }

    private sealed class FakePlatform : IReminderPlatform
    {
        public int Requests { get; private set; }
        public NotificationAccess GetAccess() => new(false, "Blocked");
        public BackgroundDeliveryStatus GetBackgroundStatus() => new(false, false, false);
        public Task RequestPermissionAsync() => Task.CompletedTask;
        public void OpenSettings() { }
        public void OpenAppSettings() { }
        public void Schedule(DateTimeOffset? next) { }
        public void RequestReconciliation() => Requests++;
        public void CleanNotifications(IReadOnlySet<string> validKeys, bool keepTestNotification) { }
        public void CancelDue(Guid id) { }
        public void Post(ReminderOccurrence occurrence) { }
        public void PostTest() { }
    }
}
