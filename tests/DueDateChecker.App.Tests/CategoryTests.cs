using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;
using System.Collections.Specialized;

namespace DueDateChecker.App.Tests;

public sealed class CategoryTests : IDisposable
{
    private readonly string _path=Path.Combine(Path.GetTempPath(),$"app-categories-{Guid.NewGuid():N}.db");
    private readonly DatabaseDispatcher _database;
    private readonly SqliteDueRepository _repository;
    private readonly TimeProvider _clock=new FixedClock();
    private static readonly DateOnly Today=new(2026,10,18);
    public CategoryTests(){new DatabaseInitializer(_path).Initialize();_repository=new(_path);_database=new(new DatabaseInitializer(_path));}
    private DueEditViewModel Editor(IDueRepository? repository=null)=>new(_database,repository??_repository,_clock);
    private Due NewDue(string title,DueCategory category,bool paid=false)=>Due.Create(title,Money.FromCentavos(0),Today,_clock.GetUtcNow(),category:category) with {IsPaid=paid};

    [Theory]
    [InlineData(DueCategory.Bill)] [InlineData(DueCategory.Task)] [InlineData(DueCategory.Subscription)] [InlineData(DueCategory.Other)]
    public async Task Create_persists_each_category_and_every_saved_surface_exposes_it(DueCategory category)
    {
        var editor=Editor();Assert.Equal(DueCategory.Bill,editor.SelectedCategory);
        editor.Title="Category create";editor.AmountText="0";editor.SelectedCategory=category;
        await editor.SaveCommand.ExecuteAsync(null);
        var due=Assert.Single(await new SqliteDueRepository(_path).ListActiveAsync());Assert.Equal(category,due.Category);
        var details=new DueDetailViewModel(_database,_repository);details.PrepareForDetails(due.Id.ToString());await details.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),details.Item!.CategoryText);
        var list=new DuesListViewModel(_database,_repository);await list.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),Assert.Single(list.Dues).CategoryText);
        var dashboard=new DashboardViewModel(_database,_repository,_clock);await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),Assert.Single(dashboard.Items).CategoryText);
        Assert.Contains(category.ToString(),dashboard.Items[0].AccessibilityText);
        var calendar=new CalendarViewModel(_database,_repository,_clock,new AppPreferencesService(new MemoryPreferencesStorage()));await calendar.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),Assert.Single(calendar.Agenda).CategoryText);
        Assert.Equal(1,Assert.Single(calendar.Days,day=>day.Date==Today).Count);
    }

    [Fact]
    public async Task Edit_load_cancel_failure_retry_and_delete_preserve_commit_boundary_and_identity()
    {
        var due=NewDue("Legacy equivalent",DueCategory.Other);await _repository.AddAsync(due);
        var repository=new ControlledRepository(_repository);var editor=Editor(repository);editor.PrepareForEdit(due.Id.ToString());await editor.LoadCommand.ExecuteAsync(null);
        Assert.Equal(DueCategory.Other,editor.SelectedCategory);
        editor.SelectedCategory=DueCategory.Task;editor.ValidateCategory();Assert.Equal(due,await _repository.GetAsync(due.Id));
        repository.FailUpdates=true;await editor.SaveCommand.ExecuteAsync(null);
        Assert.True(editor.HasError);Assert.Equal(DueCategory.Task,editor.SelectedCategory);Assert.Equal(due,await _repository.GetAsync(due.Id));
        repository.FailUpdates=false;await editor.SaveCommand.ExecuteAsync(null);
        var saved=await _repository.GetAsync(due.Id);Assert.Equal(due with { Category=DueCategory.Task },saved);
        await editor.SaveCommand.ExecuteAsync(null);Assert.Equal(1,repository.SuccessfulUpdates);
        var remove=Editor();remove.PrepareForEdit(due.Id.ToString());await remove.LoadCommand.ExecuteAsync(null);
        remove.SelectedCategory=DueCategory.Subscription;await remove.DeleteCommand.ExecuteAsync(false);Assert.Equal(saved,await _repository.GetAsync(due.Id));
        await remove.DeleteCommand.ExecuteAsync(true);var tombstone=await _repository.GetAsync(due.Id);
        Assert.NotNull(tombstone!.DeletedAt);Assert.Equal(DueCategory.Task,tombstone.Category);
        var stale=Editor();stale.PrepareForEdit(due.Id.ToString());await stale.LoadCommand.ExecuteAsync(null);Assert.True(stale.IsUnavailable);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Missing_or_undefined_selection_is_field_error_and_never_writes(bool missing)
    {
        var editor=Editor();editor.Title="Valid";editor.AmountText="0";editor.SelectedCategory=missing?null:(DueCategory)99;
        editor.ValidateCategory();Assert.True(editor.HasCategoryError);
        await editor.SaveCommand.ExecuteAsync(null);Assert.False(editor.HasCompleted);Assert.Empty(await _repository.ListActiveAsync());
        editor.SelectedCategory=DueCategory.Task;editor.ValidateCategory();Assert.False(editor.HasCategoryError);
        editor.AmountText="";await editor.SaveCommand.ExecuteAsync(null);Assert.True(editor.HasAmountError);Assert.Empty(await _repository.ListActiveAsync());
    }

    [Theory]
    [InlineData("Bill", "Due today", 1)] [InlineData("Task", "Due today", 1)]
    [InlineData("Subscription", "Due today", 0)] [InlineData("Subscription", "Paid", 1)]
    [InlineData("Other", "All", 1)] [InlineData("All categories", "All", 4)]
    public async Task Category_query_and_status_intersect_without_changing_global_summary(string category,string status,int count)
    {
        foreach(var value in Enum.GetValues<DueCategory>())await _repository.AddAsync(NewDue("ALPHA "+value,value,value==DueCategory.Subscription));
        await _repository.AddAsync(NewDue("Beta bill",DueCategory.Bill));
        var deleted=NewDue("ALPHA deleted",DueCategory.Bill);await _repository.AddAsync(deleted);await _repository.SoftDeleteAsync(deleted.Id,_clock.GetUtcNow());
        var dashboard=new DashboardViewModel(_database,_repository,_clock);await dashboard.LoadCommand.ExecuteAsync(null);
        var source=dashboard.Items;var events=new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)source).CollectionChanged+=(_,e)=>events.Add(e.Action);
        dashboard.Query="  alpha  ";dashboard.CategoryFilter=category;dashboard.SelectFilter(status);
        Assert.Equal(count,dashboard.Items.Count);Assert.Equal(5,dashboard.TotalCount);Assert.Equal(1,dashboard.PaidCount);Assert.Equal(4,dashboard.TodayCount);
        if(category!="All categories")Assert.All(dashboard.Items,row=>Assert.Equal(category,row.CategoryText));
        Assert.True(dashboard.HasFilters);await dashboard.LoadCommand.ExecuteAsync(null);Assert.Equal(category,dashboard.CategoryFilter);
        Assert.Same(source,dashboard.Items);Assert.DoesNotContain(NotifyCollectionChangedAction.Reset,events);
        dashboard.ClearFiltersCommand.Execute(null);Assert.Equal("All categories",dashboard.CategoryFilter);Assert.Equal("All",dashboard.Filter);Assert.Equal("",dashboard.Query);Assert.Equal(5,dashboard.Items.Count);
        dashboard.CategoryFilter="Unknown";Assert.Equal("All categories",dashboard.CategoryFilter);
        var restarted=new DashboardViewModel(_database,_repository,_clock);Assert.Equal("All categories",restarted.CategoryFilter);
    }

    [Fact]
    public async Task Saved_category_edit_refreshes_filtered_dashboard_and_calendar_without_changing_date_counts()
    {
        var due=NewDue("Move category",DueCategory.Bill);await _repository.AddAsync(due);
        var dashboard=new DashboardViewModel(_database,_repository,_clock);await dashboard.LoadCommand.ExecuteAsync(null);dashboard.CategoryFilter="Bill";
        var calendar=new CalendarViewModel(_database,_repository,_clock,new AppPreferencesService(new MemoryPreferencesStorage()));await calendar.LoadCommand.ExecuteAsync(null);
        var edit=Editor();edit.PrepareForEdit(due.Id.ToString());await edit.LoadCommand.ExecuteAsync(null);edit.SelectedCategory=DueCategory.Task;
        await dashboard.LoadCommand.ExecuteAsync(null);Assert.Single(dashboard.Items); // Unsaved selection.
        await edit.SaveCommand.ExecuteAsync(null);await dashboard.LoadCommand.ExecuteAsync(null);await calendar.LoadCommand.ExecuteAsync(null);
        Assert.Empty(dashboard.Items);Assert.True(dashboard.IsNoMatches);Assert.Equal(1,dashboard.TotalCount);
        dashboard.CategoryFilter="Task";Assert.Equal(due.Id,Assert.Single(dashboard.Items).Id);
        Assert.Equal("Task",Assert.Single(calendar.Agenda).CategoryText);Assert.Equal(1,Assert.Single(calendar.Days,day=>day.Date==Today).Count);
        var reopened=new SqliteDueRepository(_path);Assert.Equal(DueCategory.Task,(await reopened.GetAsync(due.Id))!.Category);
    }

    private sealed class FixedClock:TimeProvider{public override DateTimeOffset GetUtcNow()=>new(2026,10,18,1,0,0,TimeSpan.Zero);}
    private sealed class ControlledRepository(IDueRepository inner):IDueRepository
    {
        public bool FailUpdates{get;set;} public int SuccessfulUpdates{get;private set;}
        public Task<IReadOnlyList<Due>> ListActiveAsync()=>inner.ListActiveAsync();public Task<Due?> GetAsync(Guid id)=>inner.GetAsync(id);
        public Task AddAsync(Due due)=>inner.AddAsync(due);public Task SoftDeleteAsync(Guid id,DateTimeOffset when)=>inner.SoftDeleteAsync(id,when);
        public async Task UpdateAsync(Due due){if(FailUpdates)throw new IOException("Controlled failure");await inner.UpdateAsync(due);SuccessfulUpdates++;}
    }
    public void Dispose(){SqliteConnection.ClearAllPools();File.Delete(_path);}
}
