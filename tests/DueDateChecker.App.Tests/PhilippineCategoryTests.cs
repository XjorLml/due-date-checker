using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

public sealed class PhilippineCategoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"app-ph-{Guid.NewGuid():N}.db");
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    private readonly Clock _clock = new();
    public PhilippineCategoryTests()
    {
        _database = new(new DatabaseInitializer(_path)); _repository = new(_path);
    }
    private DueEditViewModel Editor() => new(_database, _repository, _clock);

    [Fact]
    public void Picker_search_keeps_selection_and_exposes_all_grouped_presets()
    {
        var picker = new PhilippineCategoryPickerViewModel("ph.utilities.electricity");
        Assert.Equal(10, picker.Groups.Count); Assert.Equal(53, picker.Groups.Sum(group => group.Count));
        Assert.Equal("ph.utilities.electricity", Assert.Single(picker.Groups.SelectMany(x => x), x => x.IsSelected).Id);
        picker.Query = "KURYENTE";
        var option = Assert.Single(Assert.Single(picker.Groups));
        Assert.True(option.IsSelected); Assert.Contains("selected", option.AccessibilityText); Assert.Contains("✓", option.Label);
        picker.Query = "not-a-category"; Assert.Empty(picker.Groups); Assert.Equal("0 categories", picker.ResultText);
        picker.Query = ""; Assert.Equal(53, picker.Groups.Sum(x => x.Count));
        Assert.Single(picker.Groups.SelectMany(x => x), x => x.IsSelected);
    }

    [Theory]
    [InlineData(DueCategory.Task, true)]
    [InlineData(DueCategory.Other, true)]
    [InlineData(DueCategory.Bill, false)]
    [InlineData(DueCategory.Subscription, false)]
    public async Task Detailed_category_does_not_change_amount_or_repeat_policy(DueCategory type, bool optional)
    {
        var editor = Editor(); editor.Title = "Amount policy"; editor.SelectedCategory = type;
        editor.SelectedCategoryId = "ph.utilities.electricity"; editor.AmountText = "";
        Assert.Equal(type, editor.SelectedCategory); Assert.Equal(!optional, editor.IsAmountRequired);
        Assert.Equal(Due.CanRepeat(type), editor.CanChooseRepeat);
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal(optional, editor.HasSaved); Assert.Equal(!optional, editor.HasAmountError);
        if (optional) Assert.Equal("ph.utilities.electricity", Assert.Single(await _repository.ListActiveAsync()).CategoryId);
    }

    [Fact]
    public async Task Save_load_edit_and_dirty_draft_preserve_separate_type_and_category()
    {
        var editor = Editor(); Assert.False(editor.HasChanges);
        editor.SelectedCategoryId = "ph.government.sss"; Assert.True(editor.HasChanges);
        editor.SelectedCategoryId = PhilippineCategories.OtherId; Assert.False(editor.HasChanges);
        editor.Title = "Sample SSS"; editor.AmountText = "0"; editor.SelectedCategoryId = "ph.government.sss";
        await editor.SaveCommand.ExecuteAsync(null); Assert.True(editor.HasSaved);
        var saved = Assert.Single(await _repository.ListActiveAsync());
        var edit = Editor(); edit.PrepareForEdit(saved.Id.ToString()); await edit.LoadCommand.ExecuteAsync(null);
        Assert.Equal("SSS Contributions", edit.DetailedCategoryText); Assert.False(edit.HasChanges);
        edit.Title = "Unsaved title"; edit.AmountText = "42.50"; edit.Notes = "Unsaved note";
        edit.SelectedCategoryId = "ph.utilities.water";
        // Returning from a picker causes Appearing/Load; it must not overwrite the editor draft.
        await edit.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Unsaved title", edit.Title); Assert.Equal("42.50", edit.AmountText); Assert.Equal("Unsaved note", edit.Notes);
        Assert.Equal("ph.utilities.water", edit.SelectedCategoryId);
        Assert.Equal(saved, await _repository.GetAsync(saved.Id)); // no write before Save / Cancel boundary
        await edit.SaveCommand.ExecuteAsync(null);
        var changed = (await _repository.GetAsync(saved.Id))!;
        Assert.Equal(saved.Id, changed.Id); Assert.Equal(saved.CreatedAt, changed.CreatedAt);
        Assert.Equal(DueCategory.Bill, changed.Category); Assert.Equal("ph.utilities.water", changed.CategoryId);
    }

    [Fact]
    public async Task Invalid_category_is_a_field_error_and_does_not_write()
    {
        var editor = Editor(); editor.Title = "Invalid category"; editor.AmountText = "0";
        editor.SelectedCategoryId = "ph.invalid.category";
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasDetailedCategoryError); Assert.False(editor.HasSaved);
        Assert.Equal("Invalid category", editor.Title);
        await _database.InitializeAsync(); Assert.Empty(await _repository.ListActiveAsync());
    }

    [Fact]
    public async Task Shared_saved_surfaces_show_detail_while_type_filters_keep_their_meaning()
    {
        await _database.InitializeAsync();
        var due = Due.Create("Water sample", Money.FromCentavos(0), new(2026, 11, 8), _clock.GetUtcNow(),
            category: DueCategory.Bill, categoryId: "ph.utilities.water");
        await _repository.AddAsync(due);
        var list = new DuesListViewModel(_database, _repository, _clock); await list.LoadCommand.ExecuteAsync(null);
        var row = Assert.Single(list.Dues); Assert.Equal("Bill", row.CategoryText);
        Assert.Equal("Water", row.DetailedCategoryText); Assert.Contains("Water", row.MetaText);
        Assert.Contains("Bill, Water", row.AccessibilityText); Assert.Equal("water", row.TypeGlyph);
        var details = new DueDetailViewModel(_database, _repository, _clock);
        details.PrepareForDetails(due.Id.ToString()); await details.LoadCommand.ExecuteAsync(null);
        Assert.Equal("Water", details.Item!.DetailedCategoryText);
        var dashboard = new DashboardViewModel(_database, _repository, _clock); await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Contains("Water", Assert.Single(dashboard.Attention).MetaText);
        var calendar = new CalendarViewModel(_database, _repository, _clock, new AppPreferencesService(new MemoryPreferencesStorage()));
        await calendar.LoadCommand.ExecuteAsync(null); Assert.Contains("Water", Assert.Single(calendar.Agenda).MetaText);
    }

    public void Dispose() { TestDatabase.ClearPool(_path); File.Delete(_path); }
    private sealed class Clock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 11, 8, 1, 0, 0, TimeSpan.Zero);
    }
}
