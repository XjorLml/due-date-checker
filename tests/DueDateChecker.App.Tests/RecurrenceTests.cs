using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>UX03 / F07 recurring dues in the app (specs/f07-recurrence RC04–RC08, ADR 009).</summary>
public sealed class RecurrenceTests : IDisposable
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);
    // 01:00 UTC is 09:00 in Asia/Manila on 06 Nov 2026.
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 1, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 6);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-recurrence-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly TimeProvider _clock = new FixedClock(Now);

    public RecurrenceTests()
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

    private static Due Rent(DateOnly date, RepeatRule repeat = RepeatRule.Monthly, DueCategory category = DueCategory.Bill) =>
        Due.Create("Rent", Money.FromCentavos(1500000), date, Created, "Unit 4B", category, repeat);

    private async Task<DueDetailViewModel> DetailsAsync(Due due, IDueRepository? repository = null)
    {
        var vm = new DueDetailViewModel(_database, repository ?? _repository, _clock);
        vm.PrepareForDetails(due.Id.ToString("D"));
        await vm.LoadCommand.ExecuteAsync(null);
        return vm;
    }

    private async Task<DueEditViewModel> EditorAsync(Due due)
    {
        var editor = new DueEditViewModel(_database, _repository, _clock);
        editor.PrepareForEdit(due.Id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    private Task<IReadOnlyList<Due>> ActiveAsync() => new SqliteDueRepository(_path).ListActiveAsync();

    // ---- RC04: Details ----

    [Fact]
    public async Task Marking_a_monthly_bill_paid_on_details_adds_the_next_one_and_says_so()
    {
        var due = Rent(new DateOnly(2027, 1, 31));
        await _repository.AddAsync(due);
        var vm = await DetailsAsync(due);
        Assert.Contains("adds the next one", vm.PaidActionHint);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        var rows = await ActiveAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(due with { IsPaid = true, UpdatedAt = Now }, rows.Single(row => row.Id == due.Id));
        var next = rows.Single(row => row.Id != due.Id);
        Assert.Equal(new DateOnly(2027, 2, 28), next.DueDate);
        Assert.Equal((due.Title, due.Amount, due.Notes, due.Category, RepeatRule.Monthly, (int?)31, (Guid?)due.Id),
            (next.Title, next.Amount, next.Notes, next.Category, next.Repeat, next.RepeatDay, next.RepeatSourceId));
        Assert.False(next.IsPaid);
        Assert.Equal(Now, next.CreatedAt);
        Assert.Equal("Marked paid. Next one added for 28 Feb 2027.", vm.PaymentAnnouncement);
        Assert.Equal("Next one added for 28 Feb 2027.", vm.NextOccurrenceText);
        Assert.True(vm.HasNextOccurrenceText);
        Assert.Equal("Saves this due as unpaid now", vm.PaidActionHint);
    }

    // ---- RC05: one next occurrence per source ----

    [Fact]
    public async Task Paid_unpaid_paid_adds_only_one_next_occurrence_even_after_it_was_deleted()
    {
        var due = Rent(new DateOnly(2026, 11, 30), RepeatRule.Weekly);
        await _repository.AddAsync(due);
        var vm = await DetailsAsync(due);

        await vm.TogglePaidCommand.ExecuteAsync(null);
        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(2, (await ActiveAsync()).Count); // unpaid keeps the next occurrence
        Assert.Equal("Marked unpaid.", vm.PaymentAnnouncement);
        Assert.False(vm.HasNextOccurrenceText);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(2, (await ActiveAsync()).Count);
        Assert.Equal("Marked paid.", vm.PaymentAnnouncement);
        Assert.False(vm.HasNextOccurrenceText);

        var next = (await ActiveAsync()).Single(row => row.Id != due.Id);
        Assert.Equal(new DateOnly(2026, 12, 7), next.DueDate);
        await _repository.SoftDeleteAsync(next.Id, Now);
        await vm.TogglePaidCommand.ExecuteAsync(null);
        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Single(await ActiveAsync());
    }

    [Theory]
    [InlineData(DueCategory.Bill)]
    [InlineData(DueCategory.Task)]
    public async Task A_due_that_does_not_repeat_adds_nothing_when_paid(DueCategory category)
    {
        var due = Rent(new DateOnly(2026, 11, 30), RepeatRule.Never, category);
        await _repository.AddAsync(due);
        var vm = await DetailsAsync(due);
        Assert.Equal("Saves this due as paid now; it records a status, not a payment", vm.PaidActionHint);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(due with { IsPaid = true, UpdatedAt = Now }, Assert.Single(await ActiveAsync()));
        Assert.Equal("Marked paid.", vm.PaymentAnnouncement);
    }

    // ---- RC08: failures ----

    [Fact]
    public async Task A_failed_paid_and_next_write_changes_nothing_and_is_reported()
    {
        var due = Rent(new DateOnly(2026, 11, 30));
        await _repository.AddAsync(due);
        var failing = new FailingNextRepository(_repository);
        var vm = await DetailsAsync(due, failing);

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.Equal(1, failing.Attempts);
        Assert.Equal("Couldn't update the payment status. Nothing was changed. Try again.", vm.PaymentError);
        Assert.Equal(due, Assert.Single(await ActiveAsync()));
        Assert.Equal("Upcoming", vm.Item!.StatusText);
        Assert.False(vm.HasNextOccurrenceText);
    }

    [Fact]
    public async Task A_repeating_due_deleted_elsewhere_becomes_unavailable_without_a_next_one()
    {
        var due = Rent(new DateOnly(2026, 11, 30));
        await _repository.AddAsync(due);
        var vm = await DetailsAsync(due);
        await _repository.SoftDeleteAsync(due.Id, Now.AddMinutes(-5));

        await vm.TogglePaidCommand.ExecuteAsync(null);

        Assert.True(vm.IsUnavailable);
        Assert.Empty(await ActiveAsync());
        Assert.False((await _repository.GetAsync(due.Id))!.IsPaid);
    }

    // ---- RC06: editor ----

    [Fact]
    public void Repeat_is_offered_only_for_bill_and_subscription_and_resets_to_never()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock);
        Assert.Equal(new[] { RepeatRule.Never, RepeatRule.Weekly, RepeatRule.Monthly, RepeatRule.Yearly }, editor.Repeats);
        Assert.Equal(RepeatRule.Never, editor.SelectedRepeat);
        Assert.True(editor.CanChooseRepeat); // Bill is the default type

        editor.SelectedRepeat = RepeatRule.Monthly;
        editor.SelectedCategory = DueCategory.Subscription;
        Assert.True(editor.CanChooseRepeat);
        Assert.Equal(RepeatRule.Monthly, editor.SelectedRepeat);

        editor.SelectedCategory = DueCategory.Task;
        Assert.False(editor.CanChooseRepeat);
        Assert.Equal(RepeatRule.Never, editor.SelectedRepeat);

        editor.SelectedCategory = DueCategory.Bill;
        Assert.Equal(RepeatRule.Never, editor.SelectedRepeat); // a reset is not undone
        editor.SelectedCategory = null;
        Assert.False(editor.CanChooseRepeat);
    }

    [Fact]
    public async Task Creating_a_monthly_bill_stores_the_repeat_and_its_anchor_day()
    {
        var editor = new DueEditViewModel(_database, _repository, _clock)
        {
            Title = "Rent", AmountText = "15000", SelectedDate = new DateTime(2027, 1, 31), SelectedRepeat = RepeatRule.Monthly
        };

        await editor.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(await ActiveAsync());
        Assert.Equal((RepeatRule.Monthly, (int?)31, (Guid?)null), (saved.Repeat, saved.RepeatDay, saved.RepeatSourceId));
        Assert.Equal("Due saved.", editor.CompletionMessage);
    }

    [Fact]
    public async Task Editing_keeps_the_anchor_and_source_unless_the_date_changes()
    {
        var source = Rent(new DateOnly(2027, 1, 31)) with { IsPaid = true };
        var occurrence = Recurrence.NextOccurrence(source, Created)!; // 28 Feb 2027, anchor 31
        await _repository.AddAsync(source);
        await _repository.AddAsync(occurrence);

        var editor = await EditorAsync(occurrence);
        Assert.Equal(RepeatRule.Monthly, editor.SelectedRepeat);
        editor.Title = "Rent, unit 4B";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(occurrence with { Title = "Rent, unit 4B", UpdatedAt = Now }, await _repository.GetAsync(occurrence.Id));

        editor = await EditorAsync(occurrence);
        editor.SelectedDate = new DateTime(2027, 2, 15);
        await editor.SaveCommand.ExecuteAsync(null);
        var moved = (await _repository.GetAsync(occurrence.Id))!;
        Assert.Equal((new DateOnly(2027, 2, 15), (int?)15, (Guid?)source.Id), (moved.DueDate, moved.RepeatDay, moved.RepeatSourceId));

        editor = await EditorAsync(occurrence);
        editor.SelectedRepeat = RepeatRule.Weekly;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Null((await _repository.GetAsync(occurrence.Id))!.RepeatDay);

        editor = await EditorAsync(occurrence);
        editor.SelectedRepeat = RepeatRule.Yearly; // no stored anchor to keep, so the date's day is used
        await editor.SaveCommand.ExecuteAsync(null);
        var yearly = (await _repository.GetAsync(occurrence.Id))!;
        Assert.Equal((RepeatRule.Yearly, (int?)15), (yearly.Repeat, yearly.RepeatDay));
    }

    [Fact]
    public async Task Changing_a_repeating_bill_to_a_task_saves_it_as_not_repeating()
    {
        var due = Rent(new DateOnly(2026, 11, 30));
        await _repository.AddAsync(due);
        var editor = await EditorAsync(due);

        editor.SelectedCategory = DueCategory.Task;
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasSaved);
        Assert.Equal(due with { Category = DueCategory.Task, Repeat = RepeatRule.Never, RepeatDay = null, UpdatedAt = Now },
            await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task Marking_paid_in_the_editor_adds_the_next_one_once()
    {
        var due = Rent(new DateOnly(2026, 11, 30), RepeatRule.Monthly, DueCategory.Subscription);
        await _repository.AddAsync(due);

        var editor = await EditorAsync(due);
        editor.IsPaid = true;
        await editor.SaveCommand.ExecuteAsync(null);

        var rows = await ActiveAsync();
        Assert.Equal(2, rows.Count);
        var next = rows.Single(row => row.Id != due.Id);
        Assert.Equal((new DateOnly(2026, 12, 30), (Guid?)due.Id, false), (next.DueDate, next.RepeatSourceId, next.IsPaid));
        Assert.Equal("Saved. Next one added for 30 Dec 2026.", editor.CompletionMessage);

        editor = await EditorAsync(due); // already paid: saving again is not a new payment
        editor.Notes = "Changed";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal("Changes saved.", editor.CompletionMessage);

        editor = await EditorAsync(due);
        editor.IsPaid = false;
        await editor.SaveCommand.ExecuteAsync(null);
        editor = await EditorAsync(due);
        editor.IsPaid = true;
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(2, (await ActiveAsync()).Count);
        Assert.Equal("Changes saved.", editor.CompletionMessage);
    }

    // ---- RC07: rows and Details ----

    [Fact]
    public void Rows_and_details_show_the_repeat()
    {
        var monthly = Rent(new DateOnly(2026, 11, 7));
        var item = DueListItem.Create(monthly, Today);
        var dashboard = new DashboardItem(monthly, DueStatus.Upcoming);

        Assert.Equal("07 Nov · Bill", item.MetaText); // prototype alignment: the repeat has its own row line
        Assert.Equal("Every month", item.RepeatLineText);
        Assert.Equal($"Rent, Bill, repeats monthly, Upcoming, due 07 November 2026, {monthly.Amount}. Opens due details.", item.AccessibilityText);
        Assert.True(item.IsRepeating);
        Assert.Equal("Every month on day 7", item.RepeatFactText);
        Assert.Equal(item.MetaText, dashboard.MetaText);
        Assert.Equal($"Rent, Bill, repeats monthly, Upcoming, due 07 Nov 2026, {monthly.Amount}. Opens due details.", dashboard.AccessibilityText);

        Assert.Equal("Every week", DueListItem.Create(Rent(new DateOnly(2026, 11, 7), RepeatRule.Weekly), Today).RepeatFactText);
        var leap = Recurrence.NextOccurrence(Rent(new DateOnly(2028, 2, 29), RepeatRule.Yearly) with { IsPaid = true }, Created)!;
        Assert.Equal("Every year on 29 Feb", DueListItem.Create(leap, Today).RepeatFactText);

        var once = DueListItem.Create(Rent(new DateOnly(2026, 11, 7), RepeatRule.Never), Today);
        Assert.Equal("07 Nov · Bill", once.MetaText);
        Assert.False(once.IsRepeating);
        Assert.Equal("", once.RepeatFactText);
        Assert.StartsWith("Rent, Bill, Upcoming, due 07 November 2026", once.AccessibilityText);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Fails the combined write the way a storage error would, before anything is stored.</summary>
    private sealed class FailingNextRepository(IDueRepository inner) : IDueRepository
    {
        public int Attempts { get; private set; }
        public Task<IReadOnlyList<Due>> ListActiveAsync() => inner.ListActiveAsync();
        public Task<Due?> GetAsync(Guid id) => inner.GetAsync(id);
        public Task AddAsync(Due due) => inner.AddAsync(due);
        public Task UpdateAsync(Due due) => throw new InvalidOperationException("A repeating due must be paid through UpdateWithNextAsync.");
        public Task SoftDeleteAsync(Guid id, DateTimeOffset when) => inner.SoftDeleteAsync(id, when);
        public Task<bool> UpdateWithNextAsync(Due updated, Due next)
        {
            Attempts++;
            throw new IOException("Controlled storage failure");
        }
    }
}
