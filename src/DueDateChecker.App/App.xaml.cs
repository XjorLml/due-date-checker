using Microsoft.Extensions.DependencyInjection;

namespace DueDateChecker.App;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		_services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Pages resolve StaticResource values while being constructed. App resources
		// must exist before DI constructs Shell and its injected list page.
		var shell = _services.GetRequiredService<AppShell>();
		var window = new Window(shell);
		window.Resumed += async (_, _) => await shell.RefreshDuesAsync();
		return window;
	}
}
