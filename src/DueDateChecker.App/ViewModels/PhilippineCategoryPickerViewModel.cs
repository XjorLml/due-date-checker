using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.ViewModels;

public sealed record PhilippineCategoryOption(PhilippineCategory Category, bool IsSelected)
{
    public string Id => Category.Id;
    public string Icon => Category.Icon;
    public string Label => Category.Name + (IsSelected ? "  ✓" : "");
    public string AccessibilityText => $"{Category.Name}, {Category.GroupName}{(IsSelected ? ", selected" : "")}";
}

public sealed class PhilippineCategoryGroup(string name, IEnumerable<PhilippineCategoryOption> items)
    : List<PhilippineCategoryOption>(items)
{
    public string Name { get; } = name;
}

/// <summary>Local searchable presets only. Selecting/cancelling never writes a due.</summary>
public partial class PhilippineCategoryPickerViewModel : ObservableObject
{
    private readonly string _selectedId;
    public PhilippineCategoryPickerViewModel(string selectedId)
    {
        _selectedId = PhilippineCategories.Get(selectedId).Id;
        Refresh();
    }
    public ObservableCollection<PhilippineCategoryGroup> Groups { get; } = [];
    [ObservableProperty] private string _query = "";
    [ObservableProperty] private string _resultText = "";
    partial void OnQueryChanged(string value) => Refresh();
    private void Refresh()
    {
        var results = PhilippineCategories.Search(Query);
        Groups.Clear();
        foreach (var group in results.GroupBy(item => item.GroupName))
            Groups.Add(new(group.Key, group.Select(item => new PhilippineCategoryOption(item, item.Id == _selectedId))));
        ResultText = results.Count == 1 ? "1 category" : $"{results.Count} categories";
    }
}
