using DueDateChecker.App.ViewModels;
using Microsoft.Maui.Dispatching;

namespace DueDateChecker.App.Pages;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;
    private readonly IDispatcherTimer _dateTimer;
    private Task? _refreshTask;
    private bool _navigating;

    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        PageLayout.Connect(this, PageContent);
        _dateTimer = Dispatcher.CreateTimer();
        _dateTimer.Interval = TimeSpan.FromMinutes(1);
        _dateTimer.Tick += (_, _) => _viewModel.RefreshDate();
        Appearing += async (_, _) => { _dateTimer.Start(); await RefreshAsync(); };
        Disappearing += (_, _) => _dateTimer.Stop();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CalendarViewModel.Days)) RenderMonth();
        };
        RenderMonth();
    }

    public Task RefreshAsync()
    {
        _viewModel.RefreshPreferences();
        if (_refreshTask is { IsCompleted: false }) return _refreshTask;
        return _refreshTask = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void RenderMonth()
    {
        MonthGrid.Children.Clear();
        MonthGrid.RowDefinitions.Clear();
        for (var row = 0; row < 7; row++) MonthGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var weekdays = _viewModel.Weekdays;
        for (var column = 0; column < weekdays.Count; column++)
            MonthGrid.Add(new Label { Text = weekdays[column], Style = (Style)Application.Current!.Resources["CaptionLabel"], HorizontalTextAlignment = TextAlignment.Center }, column, 0);
        var days = _viewModel.Days;
        for (var index = 0; index < days.Count; index++)
        {
            var day = days[index];
            if (day.Date is not { } date) continue;
            var button = new Button
            {
                Text = $"{date.Day}\n{day.Indicator}", FontSize = 14,
                Style = (Style)Application.Current!.Resources[day.IsSelected ? "BaseButton" : "SecondaryButton"],
                Padding = new Thickness(0, 8), MinimumWidthRequest = 48, MinimumHeightRequest = 64,
                CornerRadius = 12, AutomationId = $"CalendarDay-{date:yyyy-MM-dd}"
            };
            if (day.IsToday && !day.IsSelected)
            {
                button.BorderWidth = 2;
                button.SetAppThemeColor(Button.BorderColorProperty, (Color)Application.Current.Resources["PrimaryLight"], (Color)Application.Current.Resources["PrimaryDark"]);
            }
            SemanticProperties.SetDescription(button, day.AccessibilityText);
            button.Clicked += (_, _) => _viewModel.SelectDate(date);
            MonthGrid.Add(button, index % 7, index / 7 + 1);
        }
    }

    private void OnPreviousClicked(object? sender, EventArgs e) => _viewModel.MoveMonth(-1);
    private void OnNextClicked(object? sender, EventArgs e) => _viewModel.MoveMonth(1);
    private void OnTodayClicked(object? sender, EventArgs e) => _viewModel.ResetToday();

    private async void OnAddClicked(object? sender, EventArgs e) => await NavigateAsync(nameof(DueEditPage));

    private async void OnDueSelected(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.OfType<DashboardItem>().FirstOrDefault();
        Results.SelectedItem = null;
        if (item is not null) await NavigateAsync($"{nameof(DueDetailPage)}?id={item.Id:D}");
    }

    private async Task NavigateAsync(string route)
    {
        if (_navigating || !_viewModel.CanNavigate) return;
        _navigating = true;
        try { await Shell.Current.GoToAsync(route); }
        catch (Exception) { _viewModel.ReportNavigationError(); }
        finally { _navigating = false; }
    }
}
