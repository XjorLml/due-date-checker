using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX01: "Mark as paid" / "Mark as unpaid" on Details (specs/ux-improvements, PRD UX review decisions).</summary>
public sealed class DetailPaymentTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    // 01:00 UTC is 09:00 in Asia/Manila on 06 Nov 2026.
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"detail-payment-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;

    public DetailPaymentTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
    }

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        File.Delete(_path);
    }

    private static Due NewDue(bool paid = false) =>
        Due.Create("Internet", Money.FromCentavos(169900), new DateOnly(2026, 11, 7), Created, "ref 42", DueCategory.Bill) with { IsPaid = paid };

    private async Task<DueDetailViewModel> Loaded(Due due, IDueRepository? repository = null)
    {
        var vm = new DueDetailViewModel(_database, repository ?? _repository, new FixedClock(Now));
        vm.PrepareForDetails(due.Id.ToString("D"));
        await vm.LoadCommand.ExecuteAsync(null);
        return vm;
    }

    [Fact]
    public async Task Mark_as_paid_then_unpaid_persists_with_the_same_identity()
    {
        var due = NewDue();
        await _repository.AddAsync(due);
        var vm = await Loaded(due);
        Assert.Equal("Mark as paid", vm.PaidActionText);
        Assert.True(vm.CanTogglePaid);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        var paid = await new SqliteDueRepository(_path).GetAsync(due.Id);
        Assert.Equal(due with { IsPaid = true, UpdatedAt = Now }, paid);
        Assert.Equal("Paid", vm.Item!.StatusText);
        Assert.Equal("Mark as unpaid", vm.PaidActionText);
        Assert.Equal("Marked paid.", vm.PaymentAnnouncement);
        Assert.False(vm.HasPaymentError);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(due with { UpdatedAt = Now }, await _repository.GetAsync(due.Id));
        Assert.Equal("Upcoming", vm.Item!.StatusText);
        Assert.Equal("Mark as paid", vm.PaidActionText);
        Assert.Equal("Marked unpaid.", vm.PaymentAnnouncement);
    }

    [Fact]
    public async Task Repeated_taps_while_saving_write_once()
    {
        var due = NewDue();
        await _repository.AddAsync(due);
        var controlled = new ControlledRepository(_repository) { Gate = new TaskCompletionSource() };
        var vm = await Loaded(due, controlled);

        var first = vm.TogglePaidCommand.ExecuteAsync(null);
        Assert.True(vm.IsSaving);
        Assert.False(vm.CanTogglePaid);
        Assert.False(vm.CanEdit);
        var second = vm.TogglePaidCommand.ExecuteAsync(null);
        controlled.Gate.SetResult();
        await Task.WhenAll(first, second);

        Assert.Equal(1, controlled.Updates);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
        Assert.False(vm.IsSaving);
    }

    [Fact]
    public async Task Storage_failure_keeps_the_row_and_details_and_allows_a_retry()
    {
        var due = NewDue();
        await _repository.AddAsync(due);
        var controlled = new ControlledRepository(_repository) { FailUpdates = true };
        var vm = await Loaded(due, controlled);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.True(vm.HasPaymentError);
        Assert.Equal("Couldn't update the payment status. Nothing was changed. Try again.", vm.PaymentError);
        Assert.False(vm.ShowRetry); // "Retry loading" is for load failures only
        Assert.Equal(due, await _repository.GetAsync(due.Id));
        Assert.Equal("Upcoming", vm.Item!.StatusText);

        controlled.FailUpdates = false;
        await vm.TogglePaidCommand.ExecuteAsync(null);
        Assert.False(vm.HasPaymentError);
        Assert.True((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    [Fact]
    public async Task A_due_deleted_elsewhere_becomes_unavailable_and_stays_deleted()
    {
        var due = NewDue();
        await _repository.AddAsync(due);
        var vm = await Loaded(due);
        await _repository.SoftDeleteAsync(due.Id, Now.AddMinutes(-5));
        var tombstone = await _repository.GetAsync(due.Id);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.True(vm.IsUnavailable);
        Assert.Null(vm.Item);
        Assert.False(vm.CanTogglePaid);
        Assert.Equal(tombstone, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Already_changed_elsewhere_refreshes_without_another_write()
    {
        var due = NewDue();
        await _repository.AddAsync(due);
        var controlled = new ControlledRepository(_repository);
        var vm = await Loaded(due, controlled);
        var paidElsewhere = due with { IsPaid = true, UpdatedAt = Now.AddMinutes(-1) };
        await _repository.UpdateAsync(paidElsewhere);

        await vm.TogglePaidCommand.ExecuteAsync(null); // the user asked for "paid", which is already true

        Assert.Equal(0, controlled.Updates);
        Assert.Equal(paidElsewhere, await _repository.GetAsync(due.Id));
        Assert.Equal("Paid", vm.Item!.StatusText);
        Assert.Equal("Mark as unpaid", vm.PaidActionText);
    }

    [Fact]
    public async Task Nothing_loaded_means_no_payment_action_and_no_storage_access()
    {
        var controlled = new ControlledRepository(_repository);
        var vm = new DueDetailViewModel(_database, controlled, new FixedClock(Now));
        vm.PrepareForDetails("not-an-id");

        Assert.False(vm.CanTogglePaid);
        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(0, controlled.Reads);
        Assert.Equal(0, controlled.Updates);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ControlledRepository(IDueRepository inner) : IDueRepository
    {
        public TaskCompletionSource? Gate { get; set; }
        public bool FailUpdates { get; set; }
        public int Reads { get; private set; }
        public int Updates { get; private set; }

        public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
        public Task<Due?> GetAsync(Guid id) { Reads++; return inner.GetAsync(id); }
        public Task AddAsync(Due due) => inner.AddAsync(due);
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => inner.SoftDeleteAsync(id, when);

        public async Task UpdateAsync(Due due)
        {
            if (Gate is not null) await Gate.Task;
            if (FailUpdates) throw new IOException("Controlled storage failure");
            Updates++;
            await inner.UpdateAsync(due);
        }
    }
}
