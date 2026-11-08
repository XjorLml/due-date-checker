namespace DueDateChecker.App;

using Microsoft.Extensions.DependencyInjection;

public partial class AppShell : Shell
{
	/// <summary>Zero-based bottom-bar slot of the Add action; the Android renderer reads this.</summary>
	public const int AddSlot = 2;

	private readonly Pages.DuesListPage _duesPage;
	private readonly Services.DatabaseDispatcher _database;
	private bool _openingAdd;

	public AppShell(Pages.DuesListPage duesPage, IServiceProvider services)
	{
		InitializeComponent();
		_duesPage = duesPage;
		_database = services.GetRequiredService<Services.DatabaseDispatcher>();
		DuesContent.Content = duesPage;
		DashboardContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<Pages.DashboardPage>());
		CalendarContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<Pages.CalendarPage>());
		SettingsContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<Pages.SettingsPage>());
		// Never shown: the Add slot is intercepted below and disabled natively. A page is still
		// required so Shell can build the fifth bottom-bar item that reserves the centre slot.
		AddContent.ContentTemplate = new DataTemplate(() => new ContentPage { Title = "Add" });
		MainTabs.CurrentItem = DuesTab;
		Routing.RegisterRoute(nameof(Pages.ReminderSettingsPage), typeof(Pages.ReminderSettingsPage));
		Routing.RegisterRoute(nameof(Pages.DueEditPage), typeof(Pages.DueEditPage));
		Routing.RegisterRoute(nameof(Pages.DueDetailPage), typeof(Pages.DueDetailPage));
		Navigating += OnNavigating;
        var taps = services.GetRequiredService<Services.ReminderTapRouter>();
        void OnQueued() => MainThread.BeginInvokeOnMainThread(async () => await taps.TryOpenAsync(this));
        Loaded += async (_, _) => { taps.Queued -= OnQueued; taps.Queued += OnQueued; await taps.TryOpenAsync(this); };
        Unloaded += (_, _) => taps.Queued -= OnQueued;
        Navigated += async (_, _) => await taps.TryOpenAsync(this);
        // UX06: a restored due must appear on the page already on screen; other pages reload when shown.
        var undo = services.GetRequiredService<Services.DeletionUndo>();
        void OnRestored(object? sender, Guid id) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await (CurrentPage is Pages.DueDetailPage details ? details.RefreshAfterRestoreAsync(id) : RefreshCurrentPageAsync()); }
            catch (Exception) { } // each page reports its own load errors; a refresh failure must not crash the app
        });
        Loaded += (_, _) => { undo.Restored -= OnRestored; undo.Restored += OnRestored; };
        Unloaded += (_, _) => undo.Restored -= OnRestored;
        var payments = services.GetRequiredService<Services.ReminderPaymentService>();
        void OnPayment(object? sender, Guid id) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await RefreshCurrentPageAsync(); } catch (Exception) { }
        });
        Loaded += (_, _) => { payments.Changed -= OnPayment; payments.Changed += OnPayment; };
        Unloaded += (_, _) => payments.Changed -= OnPayment;
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

	/// <summary>Prototype alignment (PRD "UX09 (revised)"): Dashboard's counters and "All dues" open the
	/// Dues tab with that status selected (null clears search and filters).</summary>
	public void ShowDues(string? status)
	{
		_duesPage.ShowStatus(status);
		MainTabs.CurrentItem = DuesTab;
	}

	/// <summary>Opens the editor on the current root's stack so Save/Cancel return to that root.</summary>
	public async Task OpenAddAsync()
	{
		if (_openingAdd || CurrentPage is Pages.DueEditPage) return;
		_openingAdd = true;
		try
		{
			// SDD startup contract: Add stays unavailable until the database has opened. The old
			// in-page Add button checked CanAdd; the disc lives in the bar, so it asks directly.
			// This also retries a failed startup, and opens the editor only if that succeeds.
			if (!await _database.TryInitializeAsync())
			{
				await DisplayAlertAsync("Couldn't open your saved dues",
					"Check available storage or update the app, then try again.", "OK");
				return;
			}
			await GoToAsync(nameof(Pages.DueEditPage));
		}
		catch (Exception)
		{
			await DisplayAlertAsync("Couldn't open Add due", "Try again.", "OK");
		}
		finally
		{
			_openingAdd = false;
		}
	}

	private void OnNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		// Programmatic or stray selection of the Add slot becomes an Add action instead of a tab change.
		var location = e.Target?.Location?.OriginalString;
		if (location is null || !location.Contains("//main/add", StringComparison.Ordinal)) return;
		if (e.CanCancel) e.Cancel();
		_ = OpenAddAsync();
	}

	private static Task RefreshSettings(Pages.SettingsPage settings)
	{
		settings.Refresh();
		return Task.CompletedTask;
	}

	protected override bool OnBackButtonPressed()
	{
		// Root tabs are peers, not pushed pages. Dashboard/Calendar/Settings Back returns to saved dues.
		if (CurrentPage is Pages.DashboardPage or Pages.CalendarPage or Pages.SettingsPage)
		{
			MainTabs.CurrentItem = DuesTab;
			return true;
		}
		return base.OnBackButtonPressed();
	}
}
