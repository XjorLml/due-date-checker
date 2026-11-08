using Microsoft.Extensions.DependencyInjection;

namespace DueDateChecker.App;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		_services = services;
		var preferences = services.GetRequiredService<Services.AppPreferencesService>();
		preferences.Load(); // Invalid/unavailable preferences never block local CRUD.
		ApplyTheme();
		preferences.Changed += (_, _) => ApplyTheme();

		void ApplyTheme() => UserAppTheme = preferences.Current.Theme switch
		{
			Services.ThemePreference.Light => AppTheme.Light,
			Services.ThemePreference.Dark => AppTheme.Dark,
			_ => AppTheme.Unspecified
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Pages resolve StaticResource values while being constructed. App resources
		// must exist before DI constructs Shell and its injected list page.
		var shell = _services.GetRequiredService<AppShell>();
		var window = new Window(shell);
		var reminders = _services.GetRequiredService<Services.ReminderCoordinator>();
        var taps = _services.GetRequiredService<Services.ReminderTapRouter>();
        window.Created += async (_, _) => { await reminders.ReconcileAsync(); await taps.TryOpenAsync(shell); };
        window.Resumed += async (_, _) => { await reminders.ReconcileAsync(); await shell.RefreshCurrentPageAsync(); await taps.TryOpenAsync(shell); };
		return window;
	}
}
