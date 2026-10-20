namespace DueDateChecker.App;

using Microsoft.Extensions.DependencyInjection;

public partial class AppShell : Shell
{
	private readonly Pages.DuesListPage _duesPage;

	public AppShell(Pages.DuesListPage duesPage, IServiceProvider services)
	{
		InitializeComponent();
		_duesPage = duesPage;
		DuesContent.Content = duesPage;
		DashboardContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<Pages.DashboardPage>());
		CalendarContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<Pages.CalendarPage>());
		Routing.RegisterRoute(nameof(Pages.ReminderSettingsPage), typeof(Pages.ReminderSettingsPage));
		Routing.RegisterRoute(nameof(Pages.SettingsPage), typeof(Pages.SettingsPage));
		Routing.RegisterRoute(nameof(Pages.DueEditPage), typeof(Pages.DueEditPage));
		Routing.RegisterRoute(nameof(Pages.DueDetailPage), typeof(Pages.DueDetailPage));
        var taps = services.GetRequiredService<Services.ReminderTapRouter>();
        void OnQueued() => MainThread.BeginInvokeOnMainThread(async () => await taps.TryOpenAsync(this));
        Loaded += async (_, _) => { taps.Queued -= OnQueued; taps.Queued += OnQueued; await taps.TryOpenAsync(this); };
        Unloaded += (_, _) => taps.Queued -= OnQueued;
        Navigated += async (_, _) => await taps.TryOpenAsync(this);
	}

	public Task RefreshCurrentPageAsync() => CurrentPage switch
	{
		Pages.DueDetailPage details => details.RefreshAsync(),
		Pages.SettingsPage settings => RefreshSettings(settings),
        Pages.ReminderSettingsPage reminders => reminders.RefreshAsync(),
		Pages.DashboardPage dashboard => dashboard.RefreshAsync(),
		Pages.CalendarPage calendar => calendar.RefreshAsync(),
		_ when CurrentPage == _duesPage => _duesPage.RefreshAsync(),
		_ => Task.CompletedTask
	};

	private static Task RefreshSettings(Pages.SettingsPage settings)
	{
		settings.Refresh();
		return Task.CompletedTask;
	}

	protected override bool OnBackButtonPressed()
	{
		// Root tabs are peers, not pushed pages. Calendar/Dashboard Back returns to saved dues.
		if (CurrentPage is Pages.DashboardPage or Pages.CalendarPage)
		{
			MainTabs.CurrentItem = DuesTab;
			return true;
		}
		return base.OnBackButtonPressed();
	}
}
