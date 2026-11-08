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
        PageLayout.Connect(this, PageContent, heading: Heading, scrollsInside: true);
        _dateTimer = Dispatcher.CreateTimer();
        _dateTimer.Interval = TimeSpan.FromMinutes(1);
        _dateTimer.Tick += (_, _) => _viewModel.RefreshDate();
        Appearing += async (_, _) => { _dateTimer.Start(); await RefreshAsync(); };
        Disappearing += (_, _) => _dateTimer.Stop();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CalendarViewModel.Days)) RenderMonth();
        };
        MonthGrid.SizeChanged += (_, _) => FitDayCells();
        RenderMonth();
    }

    private double _dayCellMinimumWidth = LayoutRules.DayCellWidth;

    // UX15: recompute the cell minimum only when the row width changes it, so resizing settles in one pass.
    private void FitDayCells()
    {
        var width = LayoutRules.DayCellMinimumWidth(MonthGrid.Width);
        if (width == _dayCellMinimumWidth) return;
        _dayCellMinimumWidth = width;
        foreach (var cell in MonthGrid.Children.OfType<Grid>()) cell.MinimumWidthRequest = width;
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
            MonthGrid.Add(DayCell(day, date), index % 7, index / 7 + 1);
        }
    }

    // Prototype day cell (DESIGN calendar grid, PA06): the number with "1 due" / "2 dues" beneath in
    // smaller type; selected is a primary fill, today a 2 dp primary outline. The drawing is decorative
    // and a transparent native Button on top carries the tap and the full spoken date.
    private Grid DayCell(CalendarDay day, DateOnly date)
    {
        var selected = day.IsSelected;
        var frame = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            StrokeThickness = day.IsToday && !selected ? 2 : 0,
            InputTransparent = true,
            Padding = new Thickness(0, 6),
        };
        AutomationProperties.SetExcludedWithChildren(frame, true);
        if (selected) frame.SetAppThemeColor(BackgroundColorProperty, Theme.Color("PrimaryLight"), Theme.Color("PrimaryDark"));
        else frame.BackgroundColor = Colors.Transparent;
        frame.SetAppTheme<Brush>(Border.StrokeProperty, new SolidColorBrush(Theme.Color("PrimaryLight")), new SolidColorBrush(Theme.Color("PrimaryDark")));

        var number = new Label { Text = date.Day.ToString(System.Globalization.CultureInfo.InvariantCulture), FontFamily = selected ? "JakartaBold" : "JakartaMedium", FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };
        var count = new Label { Text = day.Indicator, FontFamily = "JakartaMedium", FontSize = 10, HorizontalTextAlignment = TextAlignment.Center, IsVisible = day.Count > 0, LineBreakMode = LineBreakMode.NoWrap };
        var (light, dark) = selected ? ("OnPrimaryLight", "OnPrimaryDark") : ("TextLight", "TextDark");
        number.SetAppThemeColor(Label.TextColorProperty, Theme.Color(light), Theme.Color(dark));
        var (countLight, countDark) = selected ? ("OnPrimaryLight", "OnPrimaryDark") : ("TextSecondaryLight", "TextSecondaryDark");
        count.SetAppThemeColor(Label.TextColorProperty, Theme.Color(countLight), Theme.Color(countDark));
        frame.Content = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { number, count } };

        var button = new Button { Style = (Style)Application.Current!.Resources["OverlayButton"], MinimumWidthRequest = 0, AutomationId = $"CalendarDay-{date:yyyy-MM-dd}" };
        SemanticProperties.SetDescription(button, day.AccessibilityText);
        button.Clicked += (_, _) => _viewModel.SelectDate(date);
        return new Grid { MinimumWidthRequest = _dayCellMinimumWidth, MinimumHeightRequest = 52, Children = { frame, button } };
    }

    private async void OnRemindersClicked(object? sender, EventArgs e) => await NavigateAsync(nameof(ReminderSettingsPage));

    private void OnPreviousClicked(object? sender, EventArgs e) => _viewModel.MoveMonth(-1);
    private void OnNextClicked(object? sender, EventArgs e) => _viewModel.MoveMonth(1);
    private void OnTodayClicked(object? sender, EventArgs e) => _viewModel.ResetToday();

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
