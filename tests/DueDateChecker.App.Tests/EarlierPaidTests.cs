using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX02: Dues folds paid dues dated before today so what is next comes first.</summary>
public sealed class EarlierPaidTests : IDisposable
{
    // 01:00 UTC is 09:00 in Asia/Manila on 06 Nov 2026.
    private static readonly DateTimeOffset Morning = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"earlier-paid-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private int _created;

    public EarlierPaidTests()
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

    private async Task<Due> Add(string title, int day, bool paid)
    {
        // Distinct creation times keep the equal-date order deterministic.
        var due = Due.Create(title, Money.FromCentavos(1000), new DateOnly(2026, 11, day), Morning.AddMinutes(_created++), category: DueCategory.Bill) with { IsPaid = paid };
        await _repository.AddAsync(due);
        return due;
    }

    private async Task<DuesListViewModel> Loaded(DateTimeOffset now)
    {
        var vm = new DuesListViewModel(_database, _repository, new FixedClock(now));
        await vm.LoadCommand.ExecuteAsync(null);
        return vm;
    }

    private static string[] Titles(DuesListViewModel vm) => vm.Dues.Select(d => d.Title).ToArray();

    [Fact]
    public async Task Only_paid_dues_dated_before_today_are_folded()
    {
        await Add("Overdue unpaid", 1, paid: false);
        await Add("Paid yesterday", 5, paid: true);
        await Add("Paid today", 6, paid: true);
        await Add("Upcoming unpaid", 7, paid: false);
        await Add("Paid ahead", 10, paid: true);

        var vm = await Loaded(Morning);

        Assert.Equal(new[] { "Overdue unpaid", "Paid today", "Upcoming unpaid", "Paid ahead" }, Titles(vm));
        Assert.True(vm.HasEarlierPaid);
        Assert.Equal(1, vm.EarlierPaidCount);
        Assert.Equal("Show 1 earlier paid due", vm.EarlierPaidText);
        Assert.Equal("Show 1 earlier paid due  ›", vm.EarlierPaidLabel);
        Assert.StartsWith("Show 1 paid due dated before today.", vm.EarlierPaidDescription);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.IsAllFolded);
    }

    [Theory]
    [InlineData(15, 30, true)]   // 23:30 in Manila on 05 Nov: a due dated 05 Nov is today, not earlier
    [InlineData(16, 30, false)]  // 00:30 in Manila on 06 Nov: a due dated 05 Nov is now earlier
    public async Task The_fold_uses_the_Manila_date(int utcHour, int utcMinute, bool visible)
    {
        await Add("Paid on the 5th", 5, paid: true);
        await Add("Unpaid on the 9th", 9, paid: false);

        var vm = await Loaded(new DateTimeOffset(2026, 11, 5, utcHour, utcMinute, 0, TimeSpan.Zero));

        Assert.Equal(visible, Titles(vm).Contains("Paid on the 5th"));
        Assert.Equal(visible ? 0 : 1, vm.EarlierPaidCount);
        Assert.Equal(!visible, vm.HasEarlierPaid);
    }

    [Fact]
    public async Task Expanding_shows_them_in_place_in_date_order_and_collapsing_hides_them()
    {
        await Add("Paid on the 1st", 1, paid: true);
        await Add("Paid on the 3rd", 3, paid: true);
        await Add("Overdue unpaid", 4, paid: false);
        await Add("Paid on the 5th", 5, paid: true);
        await Add("Upcoming unpaid", 8, paid: false);
        var vm = await Loaded(Morning);
        Assert.Equal("Show 3 earlier paid dues", vm.EarlierPaidText);
        Assert.Equal(new[] { "Overdue unpaid", "Upcoming unpaid" }, Titles(vm));

        vm.ToggleEarlierPaidCommand.Execute(null);

        Assert.True(vm.ShowEarlierPaid);
        Assert.Equal("Hide earlier paid dues", vm.EarlierPaidText);
        Assert.Equal(new[] { "Paid on the 1st", "Paid on the 3rd", "Overdue unpaid", "Paid on the 5th", "Upcoming unpaid" }, Titles(vm));

        vm.ToggleEarlierPaidCommand.Execute(null);
        Assert.Equal(new[] { "Overdue unpaid", "Upcoming unpaid" }, Titles(vm));
    }

    [Fact]
    public async Task Expanded_state_survives_a_refresh_and_resets_on_restart()
    {
        await Add("Paid on the 3rd", 3, paid: true);
        await Add("Upcoming unpaid", 8, paid: false);
        var vm = await Loaded(Morning);
        vm.ToggleEarlierPaidCommand.Execute(null);
        await Add("Paid on the 4th", 4, paid: true);

        await vm.LoadCommand.ExecuteAsync(null);

        Assert.True(vm.ShowEarlierPaid);
        Assert.Equal(new[] { "Paid on the 3rd", "Paid on the 4th", "Upcoming unpaid" }, Titles(vm));
        Assert.Equal(2, vm.EarlierPaidCount);

        var restarted = await Loaded(Morning);
        Assert.False(restarted.ShowEarlierPaid);
        Assert.Equal(new[] { "Upcoming unpaid" }, Titles(restarted));
    }

    [Fact]
    public async Task Everything_folded_is_not_the_empty_state()
    {
        await Add("Paid on the 3rd", 3, paid: true);
        var vm = await Loaded(Morning);

        Assert.Empty(vm.Dues);
        Assert.True(vm.IsAllFolded);
        Assert.False(vm.IsEmpty);

        var empty = new DuesListViewModel(new DatabaseDispatcher(new DatabaseInitializer(_path + ".empty")), new SqliteDueRepository(_path + ".empty"), new FixedClock(Morning));
        await empty.LoadCommand.ExecuteAsync(null);
        Assert.True(empty.IsEmpty);
        Assert.False(empty.IsAllFolded);
        Assert.False(empty.HasEarlierPaid);
        TestDatabase.ClearPool(_path + ".empty");
        File.Delete(_path + ".empty");
    }

    [Fact]
    public async Task Without_earlier_paid_dues_there_is_no_fold_row()
    {
        await Add("Upcoming unpaid", 8, paid: false);
        var vm = await Loaded(Morning);

        Assert.False(vm.HasEarlierPaid);
        Assert.Equal(new[] { "Upcoming unpaid" }, Titles(vm));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
