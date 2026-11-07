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
        var list=new DuesListViewModel(_database,_repository,_clock);await list.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),Assert.Single(list.Dues).CategoryText);
        var dashboard=new DashboardViewModel(_database,_repository,_clock);await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(category.ToString(),Assert.Single(dashboard.Attention).CategoryText); // dated today: "Needs attention"
        Assert.Contains(category.ToString(),dashboard.Attention[0].AccessibilityText);
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
        editor.SelectedCategory=DueCategory.Bill;editor.ValidateCategory();Assert.False(editor.HasCategoryError);
        // F07 optional amounts: only Bill/Subscription still reject a blank amount (Task/Other save "No amount").
        editor.AmountText="";await editor.SaveCommand.ExecuteAsync(null);Assert.True(editor.HasAmountError);Assert.Empty(await _repository.ListActiveAsync());
    }

    [Theory]
    [InlineData("Bill", "Due today", 1)] [InlineData("Task", "Due today", 1)]
    [InlineData("Subscription", "Due today", 0)] [InlineData("Subscription", "Paid", 1)]
    [InlineData("Other", "All", 1)] [InlineData("All types", "All", 4)]
    public async Task Category_query_and_status_intersect_without_changing_global_summary(string category,string status,int count)
    {
        // Prototype alignment (PRD "UX09 (revised)"): the type filter lives on Dues with the former Dashboard rules.
        foreach(var value in Enum.GetValues<DueCategory>())await _repository.AddAsync(NewDue("ALPHA "+value,value,value==DueCategory.Subscription));
        await _repository.AddAsync(NewDue("Beta bill",DueCategory.Bill));
        var deleted=NewDue("ALPHA deleted",DueCategory.Bill);await _repository.AddAsync(deleted);await _repository.SoftDeleteAsync(deleted.Id,_clock.GetUtcNow());
        var list=new DuesListViewModel(_database,_repository,_clock);await list.LoadCommand.ExecuteAsync(null);
        var source=list.Dues;var events=new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)source).CollectionChanged+=(_,e)=>events.Add(e.Action);
        list.Query="  alpha  ";list.TypeFilter=category;list.SelectFilter(status);
        Assert.Equal(count,list.Dues.Count);Assert.Equal(count,list.MatchCount);
        if(category!="All types")Assert.All(list.Dues,row=>Assert.Equal(category,row.CategoryText));
        Assert.True(list.HasFilters);await list.LoadCommand.ExecuteAsync(null);Assert.Equal(category,list.TypeFilter);
        Assert.Same(source,list.Dues);Assert.DoesNotContain(NotifyCollectionChangedAction.Reset,events);
        var dashboard=new DashboardViewModel(_database,_repository,_clock);await dashboard.LoadCommand.ExecuteAsync(null);
        Assert.Equal(5,dashboard.TotalCount);Assert.Equal(1,dashboard.PaidCount);Assert.Equal(4,dashboard.TodayCount); // global summary unaffected
        list.ClearFiltersCommand.Execute(null);Assert.Equal("All types",list.TypeFilter);Assert.Equal("All",list.Filter);Assert.Equal("",list.Query);Assert.Equal(5,list.Dues.Count);
        list.TypeFilter="Unknown";Assert.Equal("All types",list.TypeFilter);
        var restarted=new DuesListViewModel(_database,_repository,_clock);Assert.Equal("All types",restarted.TypeFilter);
    }

    [Fact]
    public async Task Saved_category_edit_refreshes_filtered_dues_and_calendar_without_changing_date_counts()
    {
        var due=NewDue("Move category",DueCategory.Bill);await _repository.AddAsync(due);
        var list=new DuesListViewModel(_database,_repository,_clock);await list.LoadCommand.ExecuteAsync(null);list.TypeFilter="Bill";
        var calendar=new CalendarViewModel(_database,_repository,_clock,new AppPreferencesService(new MemoryPreferencesStorage()));await calendar.LoadCommand.ExecuteAsync(null);
        var edit=Editor();edit.PrepareForEdit(due.Id.ToString());await edit.LoadCommand.ExecuteAsync(null);edit.SelectedCategory=DueCategory.Task;
        await list.LoadCommand.ExecuteAsync(null);Assert.Single(list.Dues); // Unsaved selection.
        await edit.SaveCommand.ExecuteAsync(null);await list.LoadCommand.ExecuteAsync(null);await calendar.LoadCommand.ExecuteAsync(null);
        Assert.Empty(list.Dues);Assert.True(list.IsNoMatches);Assert.False(list.IsEmpty);
        list.TypeFilter="Task";Assert.Equal(due.Id,Assert.Single(list.Dues).Id);
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
    public void Dispose(){TestDatabase.ClearPool(_path);File.Delete(_path);}
}
