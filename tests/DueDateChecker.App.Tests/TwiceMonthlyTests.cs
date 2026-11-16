using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>Twice-a-month repeat in the app (specs/f07-twice-monthly TM06–TM08, TM10; PRD TM).</summary>
public sealed class TwiceMonthlyTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    // 01:00 UTC is 09:00 in Asia/Manila on 16 Nov 2026.
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 1, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 16);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-twice-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly TimeProvider _clock = new FixedClock(Now);

    public TwiceMonthlyTests()
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

    private static Due Rent(DateOnly date, (int, int)? days = null) =>
        Due.Create("Rent", Money.FromCentavos(625000), date, Created, "Half each payday", DueCategory.Bill,
            RepeatRule.TwiceMonthly, "ph.housing.rent", days ?? (15, 30));

    private Task<IReadOnlyList<Due>> ActiveAsync() => new SqliteDueRepository(_path).ListActiveAsync();

    private async Task<DueEditViewModel> EditorAsync(Due due)
    {
        var editor = new DueEditViewModel(_database, _repository, _clock);
        editor.PrepareForEdit(due.Id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    // ---- TM07: editor ----

    [Fact]
    public void Twice_a_month_is_offered_with_15_and_30_and_labelled_in_the_picker()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock);
        Assert.Equal(new[] { "Never", "Weekly", "Monthly", "Twice a month", "Yearly" }, editor.RepeatLabels);
        Assert.Equal(editor.Repeats.Count, editor.RepeatLabels.Count);
        Assert.Equal(0, editor.SelectedRepeatIndex);
        Assert.False(editor.IsTwiceMonthly);

        editor.SelectedRepeatIndex = 3;
        Assert.Equal(RepeatRule.TwiceMonthly, editor.SelectedRepeat);
        Assert.True(editor.IsTwiceMonthly);
        Assert.Equal(((int?)15, (int?)30), (editor.FirstRepeatDay, editor.SecondRepeatDay));
        Assert.Equal(Enumerable.Range(1, 31), editor.RepeatDayOptions);
        Assert.Equal("Twice a month · 15 & 30", editor.RepeatSummary);
        Assert.Contains("next of your two days", editor.RepeatHelpText);

        editor.SelectedRepeat = RepeatRule.Yearly;
        Assert.Equal(4, editor.SelectedRepeatIndex);
        editor.SelectedRepeatIndex = -1;
        Assert.Null(editor.SelectedRepeat);
    }

    [Fact]
    public void Leaving_bill_and_subscription_resets_twice_a_month_to_never()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock) { SelectedRepeat = RepeatRule.TwiceMonthly };
        editor.SelectedCategory = DueCategory.Subscription;
        Assert.True(editor.IsTwiceMonthly);

        editor.SelectedCategory = DueCategory.Task;
        Assert.Equal(RepeatRule.Never, editor.SelectedRepeat);
        Assert.False(editor.IsTwiceMonthly);
    }

    [Fact]
    public async Task Creating_stores_the_picked_days_in_order()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "Rent", AmountText = "6250", SelectedDate = new DateTime(2026, 11, 20),
            SelectedRepeat = RepeatRule.TwiceMonthly, FirstRepeatDay = 25, SecondRepeatDay = 10,
        };

        await editor.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(await ActiveAsync());
        Assert.Equal((RepeatRule.TwiceMonthly, (int?)10, (int?)25, new DateOnly(2026, 11, 20)), (saved.Repeat, saved.RepeatDay, saved.RepeatDay2, saved.DueDate));
        Assert.Equal("Due saved.", editor.CompletionMessage);
    }

    [Fact]
    public async Task Equal_or_missing_days_are_refused_with_a_reason_and_nothing_is_saved()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "Rent", AmountText = "6250", SelectedDate = new DateTime(2026, 11, 20),
            SelectedRepeat = RepeatRule.TwiceMonthly, FirstRepeatDay = 15, SecondRepeatDay = 15,
        };

        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal("Choose two different days for Twice a month.", editor.RepeatDaysError);
        Assert.True(editor.HasRepeatDaysError);
        Assert.Empty(await ActiveAsync());

        editor.SecondRepeatDay = null;
        Assert.Equal("Choose both days for Twice a month.", editor.RepeatDaysError);
        editor.SecondRepeatDay = 30;
        Assert.False(editor.HasRepeatDaysError); // fixing the days clears the shown error

        editor.SecondRepeatDay = 15;
        Assert.False(editor.HasRepeatDaysError); // no new error before Save
        editor.SelectedRepeat = RepeatRule.Monthly;
        await editor.SaveCommand.ExecuteAsync(null);
        var saved = Assert.Single(await ActiveAsync());
        Assert.Equal((RepeatRule.Monthly, (int?)20, (int?)null), (saved.Repeat, saved.RepeatDay, saved.RepeatDay2)); // hidden days are ignored
    }

    [Fact]
    public async Task Editing_loads_the_days_counts_day_changes_and_switches_to_monthly()
    {
        var due = Rent(new DateOnly(2026, 11, 15), (4, 19));
        await _repository.AddAsync(due);

        var editor = await EditorAsync(due);
        Assert.Equal((RepeatRule.TwiceMonthly, (int?)4, (int?)19), (editor.SelectedRepeat, editor.FirstRepeatDay, editor.SecondRepeatDay));
        Assert.False(editor.HasChanges);
        editor.SecondRepeatDay = 20;
        Assert.True(editor.HasChanges); // UX22 asks before discarding a day-only change
        editor.SecondRepeatDay = 19;
        Assert.False(editor.HasChanges);

        editor.SecondRepeatDay = 28;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(due with { RepeatDay2 = 28, UpdatedAt = Now }, await _repository.GetAsync(due.Id));

        var monthly = await EditorAsync(due);
        monthly.SelectedRepeat = RepeatRule.Monthly;
        await monthly.SaveCommand.ExecuteAsync(null);
        var stored = (await _repository.GetAsync(due.Id))!;
        Assert.Equal((RepeatRule.Monthly, (int?)15, (int?)null), (stored.Repeat, stored.RepeatDay, stored.RepeatDay2));

        var back = await EditorAsync(stored);
        Assert.Equal(((int?)15, (int?)30), (back.FirstRepeatDay, back.SecondRepeatDay)); // a Monthly due offers the default days
    }

    // ---- TM06: paying adds the next occurrence once ----

    [Fact]
    public async Task Marking_paid_on_details_adds_the_next_cutoff_once()
    {
        var due = Rent(new DateOnly(2026, 11, 15));
        await _repository.AddAsync(due);
        var vm = new DueDetailViewModel(_database, _repository, _clock);
        vm.PrepareForDetails(due.Id.ToString("D"));
        await vm.LoadCommand.ExecuteAsync(null);

        await vm.TogglePaidCommand.ExecuteAsync(null);
        Assert.Equal("Marked paid. Next one added for 30 Nov 2026.", vm.PaymentAnnouncement);
        await vm.TogglePaidCommand.ExecuteAsync(null); // unpaid
        await vm.TogglePaidCommand.ExecuteAsync(null); // paid again: nothing more

        var rows = await ActiveAsync();
        Assert.Equal(2, rows.Count);
        var next = rows.Single(row => row.Id != due.Id);
        Assert.Equal((new DateOnly(2026, 11, 30), RepeatRule.TwiceMonthly, (int?)15, (int?)30, (Guid?)due.Id, "ph.housing.rent"),
            (next.DueDate, next.Repeat, next.RepeatDay, next.RepeatDay2, next.RepeatSourceId, next.CategoryId));
    }

    [Fact]
    public async Task Marking_paid_in_the_editor_moves_an_off_schedule_date_to_the_next_picked_day()
    {
        var due = Rent(new DateOnly(2026, 11, 20));
        await _repository.AddAsync(due);
        var editor = await EditorAsync(due);
        editor.IsPaid = true;

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Saved. Next one added for 30 Nov 2026.", editor.CompletionMessage);
        Assert.Contains(await ActiveAsync(), row => row.RepeatSourceId == due.Id && row.DueDate == new DateOnly(2026, 11, 30));
    }

    // ---- TM08: rows and Details ----

    [Fact]
    public void Rows_and_details_name_twice_a_month_and_both_days()
    {
        var due = Rent(new DateOnly(2026, 11, 30), (4, 19));
        var item = DueListItem.Create(due, Today);
        var dashboard = new DashboardItem(due, DueStatus.Upcoming);

        Assert.True(item.IsRepeating);
        Assert.Equal("Twice a month", item.RepeatLineText);
        Assert.Equal("Twice a month on days 4 and 19", item.RepeatFactText);
        Assert.Contains("repeats twice a month, Upcoming, due 30 November 2026", item.AccessibilityText);
        Assert.Equal(item.RepeatLineText, dashboard.RepeatLineText);
        Assert.Contains("repeats twice a month", dashboard.AccessibilityText);
    }

    // ---- TM10: no conversion ----

    [Fact]
    public async Task Existing_monthly_pairs_stay_two_monthly_dues()
    {
        var fifteenth = Due.Create("Loan", Money.FromCentavos(100000), new DateOnly(2026, 11, 15), Created, null, DueCategory.Bill, RepeatRule.Monthly);
        var thirtieth = Due.Create("Loan", Money.FromCentavos(100000), new DateOnly(2026, 11, 30), Created, null, DueCategory.Bill, RepeatRule.Monthly);
        await _repository.AddAsync(fifteenth);
        await _repository.AddAsync(thirtieth);

        var rows = await ActiveAsync();
        Assert.Equal(new[] { (RepeatRule.Monthly, (int?)15, (int?)null), (RepeatRule.Monthly, (int?)30, (int?)null) },
            rows.Select(row => (row.Repeat, row.RepeatDay, row.RepeatDay2)));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
