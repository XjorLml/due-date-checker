using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Tests;

/// <summary>View-model rules added by the prototype alignment (specs/prototype-alignment, decided 2026-11-07):
/// the editor's unsaved-changes check (UX22), the editor's prototype rows, calendar count words (UX24) and the
/// row projection (type icon, repeat line). Moved search and filter rules are in <see cref="DuesFilterTests"/>.</summary>
public sealed class PrototypeAlignmentTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 17, 1, 0, 0, TimeSpan.Zero); // 17 Oct, 09:00 Manila
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"prototype-{Guid.NewGuid():N}.db");
    private readonly SqliteDueRepository _repository;
    private readonly DatabaseDispatcher _database;
    private readonly TimeProvider _clock = new FixedClock(Now);

    public PrototypeAlignmentTests()
    {
        new DatabaseInitializer(_path).Initialize();
        _repository = new SqliteDueRepository(_path);
        _database = new DatabaseDispatcher(new DatabaseInitializer(_path));
    }

    private DueEditViewModel NewEditor() => new(_database, _repository, _clock);

    // ---- UX22: unsaved changes ----

    [Fact]
    public void New_editor_has_changes_only_when_a_field_differs_from_how_it_opened()
    {
        var editor = NewEditor();
        Assert.False(editor.HasChanges);
        editor.Title = "   ";
        Assert.False(editor.HasChanges); // whitespace alone is no change
        editor.Title = "Rent";
        Assert.True(editor.HasChanges);
        editor.Title = "";
        Assert.False(editor.HasChanges);

        editor.SelectedCategory = DueCategory.Task;
        Assert.True(editor.HasChanges);
        editor.SelectedCategory = DueCategory.Bill;
        Assert.False(editor.HasChanges);

        editor.AmountText = "12";
        Assert.True(editor.HasChanges);
        editor.AmountText = "";
        editor.SelectedDate = editor.SelectedDate!.Value.AddDays(1);
        Assert.True(editor.HasChanges);
        editor.SelectedDate = editor.SelectedDate!.Value.AddDays(-1);
        editor.Notes = "ref";
        Assert.True(editor.HasChanges);
        editor.Notes = "";
        editor.SelectedRepeat = RepeatRule.Monthly;
        Assert.True(editor.HasChanges);
        editor.SelectedRepeat = RepeatRule.Never;
        Assert.False(editor.HasChanges);
    }

    [Fact]
    public async Task Edit_mode_compares_with_the_loaded_due_and_never_asks_after_save_or_when_unavailable()
    {
        var due = Due.Create("Rent", Money.FromDecimal(2450.50m), new DateOnly(2026, 11, 1), Now, "ref 123");
        await _repository.AddAsync(due);
        var editor = NewEditor();
        editor.PrepareForEdit(due.Id.ToString());
        Assert.False(editor.HasChanges); // still loading
        await editor.LoadCommand.ExecuteAsync(null);
        Assert.False(editor.HasChanges);
        editor.IsPaid = true;
        Assert.True(editor.HasChanges);
        editor.IsPaid = false;
        Assert.False(editor.HasChanges);
        editor.Title = "Rent (October)";
        Assert.True(editor.HasChanges);
        await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasCompleted);
        Assert.False(editor.HasChanges); // saved: nothing to discard

        var missing = NewEditor();
        missing.PrepareForEdit(Guid.NewGuid().ToString());
        await missing.LoadCommand.ExecuteAsync(null);
        Assert.True(missing.IsUnavailable);
        missing.Title = "Typed anyway";
        Assert.False(missing.HasChanges);
    }

    // ---- PA07: editor rows ----

    [Fact]
    public void Editor_rows_read_like_the_prototype()
    {
        var editor = NewEditor();
        Assert.True(editor.IsAddMode);
        Assert.Equal("Type: Bill", editor.TypeRowText);
        Assert.Equal("required", editor.AmountFieldTag);
        Assert.Equal("", editor.AmountPreviewLine);
        editor.AmountText = "1250.5";
        Assert.Equal($"Will save as {Money.FromDecimal(1250.50m)}.", editor.AmountPreviewLine);
        Assert.Equal("17 Oct 2026 · Manila. Past dates are allowed.", editor.DateHintText);
        Assert.Equal("Doesn't repeat", editor.RepeatSummary);
        editor.SelectedRepeat = RepeatRule.Yearly;
        Assert.Equal("Every year", editor.RepeatSummary);
        Assert.Equal("Optional", editor.NotesSummary);
        editor.Notes = "Account 42";
        Assert.Equal("Notes added", editor.NotesSummary);
        Assert.Equal("✓  Save", editor.SaveLabel);

        editor.SelectedCategory = DueCategory.Task;
        editor.AmountText = "";
        Assert.Equal("Type: Task", editor.TypeRowText);
        Assert.Equal("optional", editor.AmountFieldTag);
        Assert.Equal("Will save with no amount.", editor.AmountPreviewLine);
        editor.SelectedCategory = null;
        Assert.Equal("Type: choose one", editor.TypeRowText);
    }

    // ---- UX24 and PA06: calendar ----

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "1 due")]
    [InlineData(2, "2 dues")]
    [InlineData(12, "12 dues")]
    public void Calendar_counts_read_as_words(int count, string expected)
    {
        var day = new CalendarDay(new DateOnly(2026, 11, 3), count, IsSelected: false, IsToday: false);
        Assert.Equal(expected, day.Indicator);
        Assert.Contains($"{count} saved dues", day.AccessibilityText); // the spoken label is unchanged
    }

    // ---- PA05: rows ----

    [Theory]
    [InlineData(DueCategory.Bill, "bill")]
    [InlineData(DueCategory.Task, "task")]
    [InlineData(DueCategory.Subscription, "subscription")]
    [InlineData(DueCategory.Other, "other")]
    public void Rows_carry_the_type_glyph_and_a_separate_repeat_line(DueCategory type, string glyph)
    {
        var due = Due.Create("Row", type == DueCategory.Task ? null : Money.FromCentavos(100), new DateOnly(2026, 11, 3), Now,
            category: type, repeat: Due.CanRepeat(type) ? RepeatRule.Weekly : RepeatRule.Never);
        var item = DueListItem.Create(due, new DateOnly(2026, 10, 17));
        var dashboard = new DashboardItem(due, item.Status);
        Assert.Equal(glyph, item.TypeGlyph);
        Assert.Equal($"03 Nov · {type}", item.MetaText);
        Assert.Equal(Due.CanRepeat(type) ? "Every week" : "", item.RepeatLineText);
        Assert.Equal(item.TypeGlyph, dashboard.TypeGlyph);
        Assert.Equal(item.RepeatLineText, dashboard.RepeatLineText);
        Assert.Equal(item.IsRepeating, dashboard.IsRepeating);
        Assert.Equal(item.HasAmount, dashboard.HasAmount);
    }

    public void Dispose()
    {
        TestDatabase.ClearPool(_path);
        if (File.Exists(_path)) File.Delete(_path);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
