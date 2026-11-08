using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.App.Tests;

/// <summary>F07 optional amounts in the editor and on saved-data pages (specs/f07-optional-amounts OA02-OA05).</summary>
public sealed class OptionalAmountTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 11, 6);
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-optional-amounts-{Guid.NewGuid():N}.db");
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    // 01:00 UTC is 09:00 in Asia/Manila on the same date.
    private readonly TimeProvider _clock = new FixedClock(new DateTimeOffset(2026, 11, 6, 1, 0, 0, TimeSpan.Zero));

    public OptionalAmountTests()
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

    private DueEditViewModel NewEditor() => new(_database, _repository, _clock);

    private async Task<DueEditViewModel> LoadEditor(Guid id)
    {
        var editor = NewEditor();
        editor.PrepareForEdit(id.ToString());
        await editor.LoadCommand.ExecuteAsync(null);
        return editor;
    }

    private Due NewDue(string title, DueCategory category, long? centavos, int dayOffset = 0, bool paid = false) =>
        Due.Create(title, centavos is { } c ? Money.FromCentavos(c) : null, Today.AddDays(dayOffset), _clock.GetUtcNow(), category: category)
            with { IsPaid = paid };

    // ---- Editor requirement text (OA03) ----

    [Theory]
    [InlineData(DueCategory.Bill, true, "Required for Bill.")]
    [InlineData(DueCategory.Subscription, true, "Required for Subscription.")]
    [InlineData(DueCategory.Task, false, "Optional for Task. Blank means no amount; zero is a recorded amount.")]
    [InlineData(DueCategory.Other, false, "Optional for Other. Blank means no amount; zero is a recorded amount.")]
    public void Requirement_text_placeholder_and_description_follow_the_category(DueCategory category, bool required, string text)
    {
        var editor = NewEditor();
        editor.SelectedCategory = category;

        Assert.Equal(required, editor.IsAmountRequired);
        Assert.Equal(text, editor.AmountRequirementText);
        Assert.Equal(required ? "1250.50" : "Optional", editor.AmountPlaceholder);
        Assert.Equal(required
            ? $"Amount in Philippine pesos, required for {category}"
            : $"Amount in Philippine pesos, optional for {category}. Leave blank for no amount", editor.AmountDescription);
    }

    [Fact]
    public void A_new_form_starts_at_bill_so_an_amount_is_required()
    {
        var editor = NewEditor();

        Assert.Equal(DueCategory.Bill, editor.SelectedCategory);
        Assert.True(editor.IsAmountRequired);
        Assert.Equal("", editor.AmountPreview);
    }

    // ---- Editor save rules (OA02, OA03) ----

    [Theory]
    [InlineData(DueCategory.Task, "")]
    [InlineData(DueCategory.Task, "   ")]
    [InlineData(DueCategory.Other, "")]
    public async Task Blank_optional_amount_saves_no_amount(DueCategory category, string text)
    {
        var editor = NewEditor();
        editor.Title = "Renew passport";
        editor.SelectedCategory = category;
        editor.AmountText = text;

        Assert.Equal("No amount", editor.AmountPreview);
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasSaved);
        Assert.False(editor.HasAmountError);
        var saved = await new SqliteDueRepository(_path).GetAsync(editor.SavedDueId!.Value);
        Assert.Null(saved!.Amount);
        Assert.Equal(category, saved.Category);
    }

    [Fact]
    public async Task Zero_on_an_optional_category_is_saved_as_zero_not_as_no_amount()
    {
        var editor = NewEditor();
        editor.Title = "Free check-up";
        editor.SelectedCategory = DueCategory.Task;
        editor.AmountText = "0";

        Assert.Equal("₱0.00", editor.AmountPreview);
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(Money.FromCentavos(0), (await _repository.GetAsync(editor.SavedDueId!.Value))!.Amount);
    }

    [Theory]
    [InlineData(DueCategory.Bill)]
    [InlineData(DueCategory.Subscription)]
    public async Task Blank_required_amount_is_a_field_error_and_writes_nothing(DueCategory category)
    {
        var editor = NewEditor();
        editor.Title = "Electricity";
        editor.SelectedCategory = category;
        editor.AmountText = "  ";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasAmountError);
        Assert.Equal($"Enter an amount for this {category}, for example 1250.50.", editor.AmountError);
        Assert.False(editor.HasSaved);
        Assert.Empty(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Changing_category_re_evaluates_a_shown_amount_error()
    {
        var editor = NewEditor();
        editor.Title = "Changes its mind";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasAmountError);

        // Blank stops being an error once the type allows no amount.
        editor.SelectedCategory = DueCategory.Task;
        Assert.False(editor.HasAmountError);
        Assert.Equal("No amount", editor.AmountPreview);

        // Switching back does not raise an error before the user leaves the field or saves.
        editor.SelectedCategory = DueCategory.Subscription;
        Assert.False(editor.HasAmountError);
        Assert.Equal("", editor.AmountPreview);
        editor.ValidateAmount();
        Assert.Equal("Enter an amount for this Subscription, for example 1250.50.", editor.AmountError);

        // Malformed text stays an error under every category.
        editor.AmountText = "1,000";
        editor.ValidateAmount();
        editor.SelectedCategory = DueCategory.Other;
        Assert.True(editor.HasAmountError);
        Assert.Empty(await _repository.ListActiveAsync());
    }

    // ---- Editing existing dues (OA03, OA05) ----

    [Fact]
    public async Task Existing_task_amount_can_be_cleared_and_keeps_its_identity()
    {
        var due = NewDue("Pick up documents", DueCategory.Task, 15_000);
        await _repository.AddAsync(due);

        var editor = await LoadEditor(due.Id);
        Assert.Equal("150.00", editor.AmountText);
        editor.AmountText = "";
        await editor.SaveCommand.ExecuteAsync(null);

        var saved = await new SqliteDueRepository(_path).GetAsync(due.Id);
        Assert.Equal(due with { Amount = null }, saved);
        Assert.Equal(due.CreatedAt, saved!.CreatedAt);
    }

    [Fact]
    public async Task Existing_bill_amount_cannot_be_cleared()
    {
        var due = NewDue("Water", DueCategory.Bill, 50_000);
        await _repository.AddAsync(due);

        var editor = await LoadEditor(due.Id);
        editor.AmountText = "";
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasAmountError);
        Assert.False(editor.HasSaved);
        Assert.Equal(due, await _repository.GetAsync(due.Id));
    }

    [Fact]
    public async Task A_due_without_an_amount_loads_blank_and_needs_one_before_becoming_a_bill()
    {
        var due = NewDue("Call the bank", DueCategory.Task, null);
        await _repository.AddAsync(due);

        var editor = await LoadEditor(due.Id);
        Assert.Equal("", editor.AmountText);
        Assert.Equal("No amount", editor.AmountPreview);

        editor.SelectedCategory = DueCategory.Bill;
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasAmountError);
        Assert.Equal(due, await _repository.GetAsync(due.Id));

        editor.AmountText = "320";
        await editor.SaveCommand.ExecuteAsync(null);
        var saved = await _repository.GetAsync(due.Id);
        Assert.Equal(DueCategory.Bill, saved!.Category);
        Assert.Equal(Money.FromCentavos(32_000), saved.Amount);
    }

    // ---- Saved-data pages (OA04) ----

    [Fact]
    public async Task Every_saved_page_shows_No_amount_and_zero_stays_a_peso_amount()
    {
        var missing = NewDue("Renew passport", DueCategory.Task, null);
        var zero = NewDue("Free trial", DueCategory.Subscription, 0, dayOffset: 1);
        await _repository.AddAsync(missing);
        await _repository.AddAsync(zero);

        var list = new DuesListViewModel(_database, _repository, _clock);
        await list.LoadCommand.ExecuteAsync(null);
        var row = Assert.Single(list.Dues, item => item.Id == missing.Id);
        Assert.Equal("No amount", row.AmountText);
        Assert.Contains("No amount", row.AccessibilityText);
        Assert.Equal("₱0.00", Assert.Single(list.Dues, item => item.Id == zero.Id).AmountText);

        var dashboard = new DashboardViewModel(_database, _repository, _clock);
        await dashboard.LoadCommand.ExecuteAsync(null);
        var card = Assert.Single(dashboard.Attention.Concat(dashboard.Later), item => item.Id == missing.Id);
        Assert.Equal("No amount", card.AmountText);
        Assert.Contains("No amount", card.AccessibilityText);

        var details = new DueDetailViewModel(_database, _repository, _clock);
        details.PrepareForDetails(missing.Id.ToString());
        await details.LoadCommand.ExecuteAsync(null);
        Assert.Equal("No amount", details.Item!.AmountText);

        var calendar = new CalendarViewModel(_database, _repository, _clock, new AppPreferencesService(new MemoryPreferencesStorage()));
        await calendar.LoadCommand.ExecuteAsync(null);
        var agenda = Assert.Single(calendar.Agenda);
        Assert.Equal("No amount", agenda.AmountText);
        Assert.Contains("No amount", agenda.AccessibilityText);
    }

    [Fact]
    public async Task Missing_amounts_do_not_change_status_counts_or_order()
    {
        await _repository.AddAsync(NewDue("Overdue task", DueCategory.Task, null, dayOffset: -1));
        await _repository.AddAsync(NewDue("Today task", DueCategory.Other, null));
        await _repository.AddAsync(NewDue("Upcoming bill", DueCategory.Bill, 10_000, dayOffset: 3));
        await _repository.AddAsync(NewDue("Paid task", DueCategory.Task, null, dayOffset: 2, paid: true));

        var dashboard = new DashboardViewModel(_database, _repository, _clock);
        await dashboard.LoadCommand.ExecuteAsync(null);

        Assert.Equal(4, dashboard.TotalCount);
        Assert.Equal(1, dashboard.OverdueCount);
        Assert.Equal(1, dashboard.TodayCount);
        Assert.Equal(1, dashboard.UpcomingCount);
        Assert.Equal(1, dashboard.PaidCount);
        // Order and the status filter live on Dues since the prototype alignment.
        var list = new DuesListViewModel(_database, _repository, _clock);
        await list.LoadCommand.ExecuteAsync(null);
        Assert.Equal(new[] { "Overdue task", "Today task", "Paid task", "Upcoming bill" }, list.Dues.Select(item => item.Title));
        list.SelectFilter("Overdue");
        Assert.Equal("Overdue task", Assert.Single(list.Dues).Title);
    }

    [Fact]
    public async Task Upcoming_total_leaves_out_and_counts_dues_without_an_amount()
    {
        await _repository.AddAsync(NewDue("Rent", DueCategory.Bill, 250_000));
        await _repository.AddAsync(NewDue("Renew passport", DueCategory.Task, null, dayOffset: 4));
        await _repository.AddAsync(NewDue("Zero task", DueCategory.Task, 0, dayOffset: 5));
        await _repository.AddAsync(NewDue("Paid errand", DueCategory.Other, null, dayOffset: 2, paid: true));

        var dashboard = new DashboardViewModel(_database, _repository, _clock);
        await dashboard.LoadCommand.ExecuteAsync(null);

        Assert.Equal("₱2,500.00", dashboard.UpcomingTotalText);
        Assert.Equal("3 unpaid · today to 30 Nov · 1 without an amount, excluded", dashboard.UpcomingMetaText);
        Assert.Equal("Upcoming total ₱2,500.00, 3 unpaid · today to 30 Nov · 1 without an amount, excluded", dashboard.UpcomingDescription);
    }

    [Fact]
    public async Task Upcoming_total_beyond_the_largest_single_amount_does_not_throw()
    {
        // Each amount is valid on its own (the editor accepts up to long.MaxValue centavos),
        // but their sum does not fit in a long. The total is a display figure, so it is summed
        // in decimal and formatted with the same fixed peso format.
        await _repository.AddAsync(NewDue("Huge one", DueCategory.Bill, long.MaxValue));
        await _repository.AddAsync(NewDue("Huge two", DueCategory.Bill, long.MaxValue, dayOffset: 1));

        var dashboard = new DashboardViewModel(_database, _repository, _clock);
        await dashboard.LoadCommand.ExecuteAsync(null);

        Assert.Equal("₱184,467,440,737,095,516.14", dashboard.UpcomingTotalText);
        Assert.StartsWith("Upcoming total ₱184,467,440,737,095,516.14", dashboard.UpcomingDescription);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
