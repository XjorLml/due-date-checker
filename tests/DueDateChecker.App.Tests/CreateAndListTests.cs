using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

public sealed class CreateAndListTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"due-app-{Guid.NewGuid():N}");
    private readonly string _path;
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    // Manila has already crossed midnight while UTC is still on October 16.
    private readonly TimeProvider _clock = new FixedClock(new DateTimeOffset(2026, 10, 16, 18, 0, 0, TimeSpan.Zero));

    public CreateAndListTests()
    {
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "dues.db");
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
        _repository = new SqliteDueRepository(_path);
    }

    private DueEditViewModel NewEditor(IDueRepository? repository = null) => new(_database, repository ?? _repository, _clock);

    [Fact]
    public void New_form_uses_Manila_date_and_requires_an_amount()
    {
        var editor = NewEditor();
        Assert.Equal(new DateTime(2026, 10, 17), editor.SelectedDate);
        Assert.Equal("", editor.AmountText);
        Assert.False(editor.HasSaved);
    }

    [Fact]
    public async Task Create_then_restart_lists_persisted_dues_in_date_order()
    {
        var later = NewEditor();
        later.Title = "  Rent  ";
        later.AmountText = "2450.005";
        later.SelectedDate = new DateTime(2026, 11, 1);
        later.Notes = "   ";
        Assert.Equal("₱2,450.01", later.AmountPreview);
        await later.SaveCommand.ExecuteAsync(null);

        var sooner = NewEditor();
        sooner.Title = "Electricity";
        sooner.AmountText = "0";
        sooner.SelectedDate = new DateTime(2026, 10, 10); // Past dates are valid.
        sooner.Notes = "ref 123";
        await sooner.SaveCommand.ExecuteAsync(null);

        SqliteConnection.ClearAllPools();
        var reopened = new SqliteDueRepository(_path);
        var restarted = new DuesListViewModel(new DatabaseDispatcher(new DatabaseInitializer(_path)), reopened);
        await restarted.LoadCommand.ExecuteAsync(null);

        Assert.True(restarted.IsReady);
        Assert.True(restarted.CanAdd);
        Assert.Equal(new[] { "Electricity", "Rent" }, restarted.Dues.Select(d => d.Title));
        var stored = await reopened.GetAsync(later.SavedDueId!.Value);
        Assert.NotNull(stored);
        Assert.Equal(245001, stored.Amount.Centavos);
        Assert.Null(stored.Notes);
        Assert.False(stored.IsPaid);
        Assert.Equal(_clock.GetUtcNow(), stored.CreatedAt);
        Assert.Equal(stored.CreatedAt, stored.UpdatedAt);
        Assert.Null(stored.DeletedAt);
        Assert.Equal("ref 123", (await reopened.GetAsync(sooner.SavedDueId!.Value))!.Notes);

        using var connection = new SqliteConnection($"Data Source={_path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dues WHERE currency = 'PHP' AND timezone = 'Asia/Manila'";
        Assert.Equal(2L, command.ExecuteScalar());
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("92233720368547758.07", long.MaxValue)]
    public async Task Zero_and_maximum_centavo_amount_are_accepted(string text, long expected)
    {
        var editor = NewEditor();
        editor.Title = "Boundary amount";
        editor.AmountText = text;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasSaved);
        Assert.Equal(expected, (await _repository.GetAsync(editor.SavedDueId!.Value))!.Amount.Centavos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("1,000")]
    [InlineData("₱10")]
    [InlineData("1e3")]
    [InlineData("abc")]
    [InlineData("92233720368547758.08")]
    [InlineData("79228162514264337593543950335")]
    public async Task Invalid_or_overflowing_amount_does_not_write(string text)
    {
        var editor = NewEditor();
        editor.Title = "Rent";
        editor.AmountText = text;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasAmountError);
        Assert.False(editor.HasSaved);
        Assert.False(File.Exists(_path));
    }

    [Fact]
    public async Task Missing_title_and_date_are_reported_without_writing()
    {
        var editor = NewEditor();
        editor.Title = "   ";
        editor.AmountText = "10";
        editor.SelectedDate = null;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasTitleError);
        Assert.True(editor.HasDateError);
        Assert.False(File.Exists(_path));
    }

    [Fact]
    public async Task Blur_validation_retains_input_without_storage_access_and_save_revalidates()
    {
        var editor = NewEditor();
        editor.Title = "   ";
        editor.AmountText = "1,000";
        editor.SelectedDate = null;
        editor.Notes = "  keep this note  ";
        editor.ValidateTitle();
        editor.ValidateAmount();
        editor.ValidateDate();
        Assert.True(editor.HasTitleError);
        Assert.True(editor.HasAmountError);
        Assert.True(editor.HasDateError);
        Assert.Equal("   ", editor.Title);
        Assert.Equal("1,000", editor.AmountText);
        Assert.Equal("  keep this note  ", editor.Notes);
        Assert.False(File.Exists(_path));

        editor.Title = "  Valid title  ";
        editor.AmountText = "1.235";
        editor.SelectedDate = new DateTime(2026, 10, 17);
        editor.ValidateTitle();
        editor.ValidateAmount();
        editor.ValidateDate();
        Assert.False(editor.HasTitleError);
        Assert.False(editor.HasAmountError);
        Assert.False(editor.HasDateError);
        Assert.Equal("  Valid title  ", editor.Title);
        Assert.Equal("1.235", editor.AmountText);
        Assert.False(File.Exists(_path));

        // Validating a field does not bypass the validation performed by Save.
        editor.AmountText = "-1";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasAmountError);
        Assert.False(editor.HasSaved);
        Assert.False(File.Exists(_path));
        editor.AmountText = "1.235";
        await editor.SaveCommand.ExecuteAsync(null);
        var saved = await _repository.GetAsync(editor.SavedDueId!.Value);
        Assert.NotNull(saved);
        Assert.Equal("Valid title", saved.Title);
        Assert.Equal(124, saved.Amount.Centavos);
        Assert.Equal("  keep this note  ", saved.Notes);
    }

    [Fact]
    public async Task Save_failure_preserves_input_and_retry_writes_once()
    {
        var fake = new ControlledRepository { Fail = true };
        var editor = NewEditor(fake);
        editor.Title = "Rent";
        editor.AmountText = "10";
        editor.Notes = "Keep this note";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.False(editor.HasSaved);
        Assert.True(editor.HasError);
        Assert.False(editor.IsBusy);
        Assert.Equal("Rent", editor.Title);
        Assert.Equal("10", editor.AmountText);
        Assert.Equal("Keep this note", editor.Notes);

        fake.Fail = false;
        await editor.SaveCommand.ExecuteAsync(null);
        await editor.SaveCommand.ExecuteAsync(null); // Navigation retry must not insert again.
        Assert.True(editor.HasSaved);
        Assert.False(editor.HasError);
        Assert.Equal(1, fake.SuccessfulWrites);
    }

    [Fact]
    public async Task Repeated_save_during_pending_write_does_not_insert_twice()
    {
        var fake = new ControlledRepository { BlockWrites = true };
        var editor = NewEditor(fake);
        editor.Title = "Rent";
        editor.AmountText = "10";
        var first = editor.SaveCommand.ExecuteAsync(null);
        await fake.WriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(editor.IsBusy);
        Assert.False(editor.SaveCommand.CanExecute(null));
        await editor.SaveCommand.ExecuteAsync(null);
        fake.ReleaseWrite.SetResult();
        await first;
        Assert.Equal(1, fake.SuccessfulWrites);
    }

    [Fact]
    public async Task Failed_startup_disables_add_and_retry_initializes_database()
    {
        var nestedDirectory = Path.Combine(_directory, "not-created-yet");
        var path = Path.Combine(nestedDirectory, "dues.db");
        var vm = new DuesListViewModel(new DatabaseDispatcher(new DatabaseInitializer(path)), new SqliteDueRepository(path));
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.False(vm.IsReady);
        Assert.False(vm.CanAdd);
        Assert.True(vm.HasError);

        Directory.CreateDirectory(nestedDirectory);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.True(vm.CanAdd);
        Assert.False(vm.HasError);
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task Failed_refresh_preserves_rows_and_successful_reload_does_not_duplicate()
    {
        var fake = new ControlledRepository();
        var vm = new DuesListViewModel(_database, fake);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Single(vm.Dues);
        fake.Fail = true;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Single(vm.Dues);
        Assert.True(vm.HasError);
        Assert.False(vm.IsBusy);
        fake.Fail = false;
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Single(vm.Dues);
        Assert.False(vm.HasError);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_directory, recursive: true);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ControlledRepository : IDueRepository
    {
        public bool Fail { get; set; }
        public bool BlockWrites { get; init; }
        public int SuccessfulWrites { get; private set; }
        public TaskCompletionSource WriteStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseWrite { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IReadOnlyList<Due>> ListActiveAsync()
        {
            if (Fail) throw new IOException("Controlled storage failure");
            IReadOnlyList<Due> result = [Due.Create("Rent", Money.FromCentavos(0), new DateOnly(2026, 10, 17), DateTimeOffset.UnixEpoch)];
            return Task.FromResult(result);
        }

        public async Task AddAsync(Due due)
        {
            if (Fail) throw new IOException("Controlled storage failure");
            WriteStarted.TrySetResult();
            if (BlockWrites) await ReleaseWrite.Task;
            SuccessfulWrites++;
        }

        public Task<Due?> GetAsync(Guid id) => throw new NotSupportedException();
        public Task UpdateAsync(Due due) => throw new NotSupportedException();
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => throw new NotSupportedException();
    }
}
