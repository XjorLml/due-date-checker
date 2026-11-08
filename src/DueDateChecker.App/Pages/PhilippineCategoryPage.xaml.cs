using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App.Pages;

public partial class PhilippineCategoryPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _selection = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _closing;
    public Task<string?> Selection => _selection.Task;
    public PhilippineCategoryPage(string selectedId)
    {
        InitializeComponent();
        BindingContext = new PhilippineCategoryPickerViewModel(selectedId);
        PageLayout.Connect(this, PickerContent, PickerFooter, scrollsInside: true);
    }
    private async void OnCategoryClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string id } && PhilippineCategories.IsKnown(id)) await CloseAsync(id);
    }
    private async void OnCancelClicked(object? sender, EventArgs e) => await CloseAsync(null);
    private void OnSearchPressed(object? sender, EventArgs e) => CategorySearch.Unfocus();
    private async Task CloseAsync(string? id)
    {
        if (_closing) return;
        _closing = true;
        try
        {
            CategorySearch.Unfocus();
            await Navigation.PopModalAsync();
            _selection.TrySetResult(id);
        }
        catch (Exception)
        {
            NavigationError.Text = "Couldn't return to your due. Try Cancel again.";
            NavigationError.IsVisible = true;
            _closing = false;
        }
    }
    protected override bool OnBackButtonPressed() { _ = CloseAsync(null); return true; }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Application.Current is { } app)
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(app,
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
    }
}
